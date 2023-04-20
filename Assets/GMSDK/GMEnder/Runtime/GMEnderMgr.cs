using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using System.Threading;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using Ender;
using Ender.LitJson;

namespace Ender
{
    public class GMEnderMgr : Ender.MonoSingleton<GMEnderMgr>
    {
        private int callIndex;
        private static int connectionCount;
        private long initialSendTime;
        private static Semaphore startupSemaphore;

        private static List<int> syncCallIDQueue;

        //TODO:视情况而定是否需要加锁
        private static Dictionary<int, string> dicSyncCallResult;
        private static Dictionary<int, Semaphore> dicSyncSemaphore;

        public delegate void enderRecvMsgCallback(string message);

        public delegate void enderConnectionsCallback(int count);

        public delegate void enderCallbackObject(string json);

        private enderCallbackObject msgCallback;

        //超时
        private static System.Timers.Timer Timer;
        private static bool IsTimerStarted;
        private static Dictionary<int, long> dicSyncCallAddTime;

        //Universal
        private int pointerIndex;
        private Dictionary<string, GMEnderCFuncParam> dicPointer = new Dictionary<string, GMEnderCFuncParam>();

        private EnderSettingsModel model;
        private const string settingsFileName = "EnderSettings.json";
        private static string settingsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), @"GMEnderSettings/");

        [DllImport("GMEnderEngine")]
        private static extern void CreateCronetEngine();

        [DllImport("GMEnderEngine")]
        private static extern void InitClientNet(string ip_addr, int port);

        [DllImport("GMEnderEngine")]
        private static extern void TcpClientSend(int msgType, string message);

        [DllImport("GMEnderEngine")]
        private static extern void DisconnectAndDestroyClient();

        [DllImport("GMEnderEngine")]
        private static extern void setEnderMessageCallback(enderRecvMsgCallback callback);

        [DllImport("GMEnderEngine")]
        private static extern void setEnderConnectionsCallback(enderConnectionsCallback callback);

        private void Awake()
        {
            this.Init();
        }

        [MonoPInvokeCallback(typeof(enderRecvMsgCallback))]
        public static void HandleMsgFromNative(string message)
        {
            HandleMsg(message);
        }

        private static void HandleMsg(string message)
        {
            //注意线程切换
            //Debug.Log("接收到消息：" + message);
            try
            {
                GMEnderMgr.instance.handleReceiveMsgFromNative(message);
            }
            catch (Exception e)
            {
                Debug.LogError("[Ender] HandleMsgFromNative exception:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(enderConnectionsCallback))]
        public static void HandleEnderConnections(int count)
        {
            HandleConnect(count);
        }

        private static void HandleConnect(int count)
        {
            //注意线程切换
            Debug.Log("[Ender] 当前连接数：" + count + ", time:" + DateTime.Now.Ticks);
            connectionCount = count;
            if (connectionCount == 1)
            {
                if (startupSemaphore != null)
                {
                    Debug.Log("[Ender] HandleEnderConnections 信号量 release:" + DateTime.Now.Ticks);
                    startupSemaphore.Release();
                    startupSemaphore = null;
                }
            }
        }

        public void Init()
        {
            //GMEnderReflection.InvokeMethod("MainSceneController", null, "private_testFunction", new Type[] { typeof(int), typeof(string)}, false, new object[] { 1, "222" });
            Loom.Initialize();
            startupSemaphore = new Semaphore(0, 1);
            syncCallIDQueue = new List<int>();
            dicSyncCallResult = new Dictionary<int, string>();
            dicSyncSemaphore = new Dictionary<int, Semaphore>();
            Timer = new System.Timers.Timer();
            IsTimerStarted = false;
            dicSyncCallAddTime = new Dictionary<int, long>();

            callIndex = 1;
            pointerIndex = 1;
            initialSendTime = 0;
            connectionCount = 0;

            if (model == null)
            {
                string fullPath = Path.Combine(settingsFolderPath, settingsFileName);
                if (System.IO.File.Exists(fullPath))
                {
                    string configSettingJson = File.ReadAllText(fullPath);
                    model = JsonMapper.ToObject<EnderSettingsModel>(configSettingJson);
                }
            }

            if (isEnderOn())
            {
                Debug.Log("[Ender] init:" + DateTime.Now.Ticks);
                if (startupSemaphore != null)
                {
                    Debug.Log("[Ender] 信号量 init");
                }
                else
                {
                    Debug.Log("[Ender] 信号量 uninit");
                }

                StartTimer();
                string type = isLocalMode() ? "Local" : "Anywhere";
                Debug.Log("current ender type: " + type);
                if (isLocalMode())
                {
                    setEnderMessageCallback(HandleMsgFromNative);
                    setEnderConnectionsCallback(HandleEnderConnections);
                    string ip = getIP();
                    if (ip.Length > 0)
                    {
                        CreateCronetEngine();
                        InitClientNet(ip, 9898);
                    }
                }
                else
                {
                    EnderRemoteManager.instance.CheckCloudDeviceIsAvailable(model.deviceSerial, model.deviceDeadLine, (available) =>
                    {
                        if (available)
                        {
                            if (model.remoteVerified)
                            {
                                EnderRemoteManager.instance.SetEnderRemoteCallback(new EnderRemoteCallback());
                                EnderRemoteManager.instance.Init(model.uid, model.channelId);
                            }
                            else
                            {
                                Debug.LogError("没有进行验证操作");
                            }

                        }
                        else
                        {
                            ClearRemoteFlag();
                            Debug.LogError("云设备已释放，请重新部署");
                        }
                    }, newDeadLine =>
                    {
                        if (model != null)
                        {
                            Loom.QueueOnMainThread(() =>
                            {
                                model.deviceDeadLine = newDeadLine;
                                Save();
                            });
                        }
                    });
                }

                AppDomain.CurrentDomain.DomainUnload += (obj, e) =>
                {
                    Debug.Log("[Ender] OnDomainUnload");
                    ClearEnderResources();
                };
            }

            return;
        }

        private string getIP()
        {
            if (model != null && model.verified)
            {
                return model.ip;
            }

            return "";
        }

        public bool isIOSPlatform()
        {
            if (model != null)
            {
                return model.isiOSPlatform;
            }

            return false;
        }

        public bool isAndroidPlatform()
        {
            if (model != null)
            {
                return !model.isiOSPlatform;
            }

            return false;
        }

        public bool isEnderOn()
        {
            //return false;
            if (model != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isLocalMode()
        {
            return model.enderType == EnderRemoteConstants.EnderType.EnderLocal;
        }

        private class EnderRemoteCallback : IEnderRemoteCallback
        {
            public void HandleEnderRemoteMsgFromNative(string message)
            {
                HandleMsg(message);
            }

            public void HandleEnderRemoteAlohaMsgFromNative(string message)
            {
                //do nothing
            }

            public void HandleConnectionChange(int count)
            {
                HandleConnect(count);
            }
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);

            if (!Directory.Exists(settingsFolderPath))
            {
                Directory.CreateDirectory(settingsFolderPath);
            }

            string fullPath = Path.Combine(settingsFolderPath,
                settingsFileName);

            string json = JsonMapper.ToJson(model);
            File.WriteAllText(fullPath, json);
        }

        public void ClearRemoteFlag()
        {
            if (model == null)
            {
                return;
            }

            instance.model.ClearRemoteFlag();
            Save();
        }

        public void ClearEnderResources()
        {
            if (isEnderOn())
            {
                StopTimer();
                setEnderConnectionsCallback(null);
                setEnderMessageCallback(null);
                DisconnectAndDestroyClient();
                EnderRemoteManager.instance.Release();
            }
        }

        public void OnDestroy()
        {
            ClearEnderResources();
            Debug.Log("[Ender] Manager destroy success");
        }

        ~GMEnderMgr()
        {
            ClearEnderResources();
            Debug.Log("[Ender] Manager dealloc success");
        }

        public object callEnder(GMEnderCFunction funcRevoke)
        {
            if (isAndroidPlatform())
            {
                return null;
            }
            JsonData packet = funcRevoke.packJsonData();
            string retJson = callEnderSendMessage(packet, (int) funcRevoke.returnType > 0, GMCallEnderType.Universal);
            //返回的是json
            if (retJson == null)
            {
                return null;
            }

            JsonData ret = JsonMapper.ToObject(retJson);
            int code = GMEnderHelper.GetInt(ret, "code");
            string failMsg = GMEnderHelper.GetString(ret, "failMsg");

            if (code != 0)
            {
                Debug.LogError("[Ender] {code:" + code + "],failMsg:" + failMsg + "}");
            }

            object value = GMEnderHelper.GetObject(ret, "value");
            return value;
        }

        public string callEnder(JsonData jsonData, bool hasRetValue)
        {
            return callEnderSendMessage(jsonData, hasRetValue, GMCallEnderType.UNBridge);
        }

        public string callEnder(JsonData jsonData, bool hasRetValue, GMCallEnderType callType)
        {
            return callEnderSendMessage(jsonData, hasRetValue, callType);
        }

        private string callEnderSendMessage(JsonData jsonData, bool hasRetValue, GMCallEnderType callType)
        {
            if (jsonData == null)
            {
                return null;
            }

            if (isEnderOn())
            {
                int callID = callIndex;
                jsonData["CallEnderID"] = callIndex++;
                jsonData["CallEnderType"] = (int) callType;
                jsonData["HasRetValue"] = hasRetValue;
                bool sendSuccess = sendMessage(jsonData.ToJson());
                //是否会出现return过快的情况？
                if (hasRetValue && sendSuccess)
                {
                    //Debug.Log("send sync, id:" + (callID));
                    syncCallIDQueue.Add(callID);
                    Semaphore sema = new Semaphore(0, 1);
                    dicSyncSemaphore.Add(callID, sema);
                    dicSyncCallAddTime.Add(callID, DateTime.Now.Ticks);
                    sema.WaitOne();
                    //Debug.Log("send sync, after sema, id:" + (callID));
                    string result = null;
                    dicSyncCallResult.TryGetValue(callID, out result);
                    dicSyncCallResult.Remove(callID);
                    return result;
                }
            }
            //返回json

            return null;
        }

        private bool sendMessage(string message)
        {
            if (isEnderOn())
            {
                if (connectionCount == 0 && callIndex == 2)
                {
                    Debug.Log("[Ender] 判断信号量");
                    if (startupSemaphore != null)
                    {
                        initialSendTime = DateTime.Now.Ticks;
                        Debug.Log("[Ender] 等待信号量:" + initialSendTime);
                        startupSemaphore.WaitOne();
                    }

                    Debug.Log("[Ender] 信号量释放:" + DateTime.Now.Ticks);
                    startupSemaphore = null;
                    Thread.Sleep(20);
                }

                if (connectionCount == 0)
                {
                    Debug.LogError("[Ender] 当前Ender未连接成功，发送失败: " + message + ", callIndex:" + callIndex);
                    return false;
                }

                if (isLocalMode())
                {
                    TcpClientSend(0, message);
                }
                else
                {
                    EnderRemoteManager.instance.SendMessage(0, message);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public void handleReceiveMsgFromNative(string message)
        {
            JsonData packet = JsonMapper.ToObject(message);
            //解析出message中的CallEnderID
            if (syncCallIDQueue.Count > 0)
            {
                if (packet.ContainsKey("CallEnderID"))
                {
                    int callID = GMEnderHelper.GetInt(packet, "CallEnderID");
                    for (int i = syncCallIDQueue.Count - 1; i >= 0; i--)
                    {
                        int cQueueID = syncCallIDQueue[i];
                        if (callID == cQueueID)
                        {
                            dicSyncCallResult.Add(callID, message);
                            syncCallIDQueue.RemoveAt(i);
                            //Debug.Log("receive sync, id:" + callID);

                            Semaphore sema = null;
                            dicSyncSemaphore.TryGetValue(callID, out sema);
                            if (sema != null)
                            {
                                dicSyncSemaphore.Remove(callID);
                                sema.Release();
                            }

                            //Debug.Log("receive sync, after sema, id:" + callID);
                            return;
                        }
                    }

                    //这里出错了
                    Debug.Log("[Ender] cannot match any callenderid:" + callID);
                    return;
                }
            }

            if (packet.ContainsKey("methodPointerID"))
            {
                string pointerID = GMEnderHelper.GetString(packet, "methodPointerID");
                if (dicPointer.ContainsKey(pointerID))
                {
                    GMEnderCFuncParam param = dicPointer[pointerID];
                    List<object> arrParams = new List<object>();

                    if (packet.ContainsKey("methodPointerParams"))
                    {
                        int i = 0;
                        JsonData ptrParams = packet["methodPointerParams"];
                        foreach (object obj in ptrParams)
                        {
                            //Debug.Log("obj:" + obj + ", type:" + param.listPointerParams[i]);
                            switch (param.listPointerParams[i])
                            {
                                case GMEnderValueType.type_bool:
                                    arrParams.Add(bool.Parse(obj.ToString()));
                                    break;
                                case GMEnderValueType.type_float:
                                    arrParams.Add(float.Parse(obj.ToString()));
                                    break;
                                case GMEnderValueType.type_int:
                                    arrParams.Add(int.Parse(obj.ToString()));
                                    break;
                                case GMEnderValueType.type_string:
                                    arrParams.Add(obj == null ? null : obj.ToString());
                                    break;
                                case GMEnderValueType.type_uint:
                                    arrParams.Add(uint.Parse(obj.ToString()));
                                    break;
                                case GMEnderValueType.type_longlong:
                                    arrParams.Add(long.Parse(obj.ToString()));
                                    break;
                                case GMEnderValueType.type_double:
                                    arrParams.Add(double.Parse(obj.ToString()));
                                    break;
                                default:
                                    Debug.Log("类型出错了!" + param.listPointerParams[i]);
                                    break;
                            }

                            i++;
                        }
                    }

                    if (param.methodInfo != null)
                    {
                        //线程切换
                        Ender.Loom.QueueOnMainThread(() =>
                        {
                            try
                            {
                                param.methodInfo.Invoke(param.value, arrParams.ToArray());
                            }
                            catch (Exception e)
                            {
                                Debug.LogError("[Ender] handleReceiveMsg invoke name: " +
                                               GMEnderHelper.GetString(packet, "target") + ", error: " + e.Message);
                            }
                            //Debug.Log("调用完成:" + pointerID);
                        });
                    }
                }
                else
                {
                    Debug.Log("[Ender] handleReceiveMsg invoke name: " + GMEnderHelper.GetString(packet, "target") +
                              ", dicPointer not existed:" + pointerID);
                }

                return;
            }

            if (packet.ContainsKey("CallBackId"))
            {
                Ender.Loom.QueueOnMainThread(() => { EnderCallBackUtils.InvokeCallBack(message); });
                return;
            }

            if (msgCallback != null)
            {
                msgCallback(message);
            }
        }

        public string registerUniversalPointer(GMEnderCFuncParam pointer)
        {
            int pointerID = pointerIndex++;
            string strPointerID = pointerID.ToString();
            dicPointer.Add(strPointerID, pointer);
            return strPointerID;
        }

        /// <summary>
        /// 启动超时定时器
        /// </summary>
        public static void StartTimer()
        {
            if (!IsTimerStarted)
            {
                Timer.Interval = 1000;
                Timer.Enabled = true;
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();
                IsTimerStarted = true;
            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (startupSemaphore != null && GMEnderMgr.instance.initialSendTime > 0)
                {
                    Debug.Log("[Ender] Tiktok信号量:" + DateTime.Now.Ticks);
                    long diff = (DateTime.Now.Ticks - GMEnderMgr.instance.initialSendTime) / 10000;
                    long time = 2000;
                    if (diff > time)
                    {
                        Debug.Log("[Ender] Tiktok信号量 release:" + DateTime.Now.Ticks);
                        startupSemaphore.Release();
                        startupSemaphore = null;
                    }
                }

                for (int i = syncCallIDQueue.Count - 1; i >= 0; i--)
                {
                    int callID = syncCallIDQueue[i];
                    long addTime = 0;
                    dicSyncCallAddTime.TryGetValue(callID, out addTime);
                    if (addTime > 0)
                    {
                        //ns need translate to ms
                        long diff = (DateTime.Now.Ticks - addTime) / 10000;
                        long time = 2000;
                        if (diff > time)
                        {
                            dicSyncCallAddTime.Remove(callID);
                            Semaphore sema = null;
                            dicSyncSemaphore.TryGetValue(callID, out sema);
                            syncCallIDQueue.RemoveAt(i);
                            if (sema != null)
                            {
                                dicSyncSemaphore.Remove(callID);
                                sema.Release();
                                Debug.LogError("[Ender] Timeout sync call:" + callID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[Ender] Timer exception:" + ex.Message);
            }
        }

        /// <summary>
        /// 停止超时定时器
        /// </summary>
        public static void StopTimer()
        {
            if (!Timer.Enabled && !IsTimerStarted)
            {
                return;
            }

            Timer.Enabled = false;
            Timer.Stop();
            IsTimerStarted = false;
        }

        public void setEnderMessageCallbackObject(enderCallbackObject callbackObject)
        {
            msgCallback = callbackObject;
        }
    }
}
#endif

//private delegate void delegate_testFuntion(long b, double c, bool d, string e);
//[AOT.MonoPInvokeCallback(typeof(delegate_testFuntion))]
//private static void private_testFunction(long b, double c, bool d, string e)
//{
//    Debug.Log("收到了！！" + ", " + b + ", " + c + ", " + d + ", " + e);
//}

//Action<long, double, bool, string> tmpMethod = private_testFunction;
//object obj = GMEnderMgr.instance.callEnder(new GMEnderCFunction(
//    GMEnderValueType.type_int,
//    "testC",
//    new List<GMEnderCFuncParam>() {
//                new GMEnderCFuncParam(GMEnderValueType.type_int, 11),
//                new GMEnderCFuncParam(GMEnderValueType.type_longlong, 22),
//                new GMEnderCFuncParam(GMEnderValueType.type_double, 33.33),
//                new GMEnderCFuncParam(GMEnderValueType.type_bool, true),
//                new GMEnderCFuncParam(GMEnderValueType.type_string, "555"),
//                new GMEnderCFuncParam(GMEnderValueType.type_pointer, null, tmpMethod.Method, new List<GMEnderValueType>(){
//                    GMEnderValueType.type_longlong,
//                    GMEnderValueType.type_double,
//                    GMEnderValueType.type_bool,
//                    GMEnderValueType.type_string
//                }),
//    }));
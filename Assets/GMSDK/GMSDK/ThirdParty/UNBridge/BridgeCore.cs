using System;
using UNBridgeLib.LitJson;
using System.Collections.Generic;

namespace UNBridgeLib
{
    /// <summary>
    /// UBridge的内部实现
    /// </summary>
    internal static class BridgeCore
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        private const long TIMEOUT_TIME = 10000;
        public const int TYPE_CALL = 0;
        public const int TYPE_LISTEN = 1;
        public const int TYPE_UNLISTEN = 2;
        public const int TYPE_EVENT = 3;

        public const int SOURCE_UNITY = 1;
        public const int SOURCE_NATIVE = 2;
        public const int SOURCE_WEBGL = 3;

        /// <summary>
        /// 成功
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        /// 接口未支持
        /// </summary>
        public const int ERROR_UNSUPPORT = 100;

        /// <summary>
        /// 接口不支持异步调用
        /// </summary>
        public const int ERROR_NOT_SUPPORT_SYNC = 101;

        /// <summary>
        /// 缺少参数
        /// </summary>
        public const int ERROR_LACK_PARAM = 102;

        /// <summary>
        /// 接口不支持同步调用
        /// </summary>
        public const int ERROR_NOT_SUPPORT_ASYN = 103;

        /// <summary>
        /// 以callback_id为key维护回调。
        /// </summary>
        private static readonly Dictionary<long, BridgeCallBack> CallbackMap = new Dictionary<long, BridgeCallBack>();
        /// <summary>
        /// 以接口名为key，维护回调
        /// </summary>
        private static readonly Dictionary<string, BridgeCallBack> EventMap = new Dictionary<string, BridgeCallBack>();

        /// <summary>
        ///  Unity侧以接口的形式实现的API,比委托优先
        /// </summary>
        private static readonly Dictionary<string, IBridgeAPI> ApiMap = new Dictionary<string, IBridgeAPI>();

        /// <summary>
        /// Unity侧以委托的形式实现的API
        /// </summary>
        private static readonly Dictionary<string, BridgeAPI> BridgeDelegateMap = new Dictionary<string, BridgeAPI>();


        /// <summary>
        /// 本地注册的事件列表
        /// </summary>
        private static readonly List<string> LocalEventList = new List<string>();
        /// <summary>
        /// Native端注册的事件列表
        /// </summary>
        private static readonly List<string> RemoteEventList = new List<string>();

        /// <summary>
        /// 超时定时器
        /// </summary>
        private static readonly System.Timers.Timer Timer = new System.Timers.Timer();

        /// <summary>
        /// 定时器是否已经启动，避免重复启动；
        /// </summary>
        private static bool IsTimerStarted;

        /// <summary>
        /// 超时时间，默认10秒
        /// </summary>
        private static long MaxTimeoutTime = TIMEOUT_TIME;
        
        private static object _lock = new object();

        /// <summary>
        /// 启动超时定时器
        /// </summary>
        public static void StartTimer()
        {
            if (!IsTimerStarted) {
                Timer.Interval = 1000;
                Timer.Enabled = true;
                Timer.Elapsed += Timer_Elapsed;
                Timer.Start();
                IsTimerStarted = true;
                LogUtils.D("Start Timer");
            }
        
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                lock (_lock)
                {
                    //LogUtils.D("CheckTimeout tick");
                    var keyList = new List<long>(CallbackMap.Keys);
                    foreach (var key in keyList)
                    {
                        BridgeCallBack callBack = CallbackMap[key];
                        if (callBack != null)
                        {
                            //ns need translate to ms
                            long diff = (DateTime.Now.Ticks - callBack.AddTime) / 10000;
                            long time = (callBack.TimeoutTime > 0) ? callBack.TimeoutTime : MaxTimeoutTime;
                            if (diff > time)
                            {
                                CallbackMap.Remove(key);
                                LogUtils.D("Remove timeout callback:" , key.ToString());
                                Loom.QueueOnMainThread(() =>
                                {
                                    if (callBack.OnTimeout != null)
                                    {
                                        callBack.OnTimeout();
                                    }
                                    else
                                    {
                                        LogUtils.D("Callback onTimeout is null, type: " , callBack.GetType().ToString());
                                    }
                                });
                            }

                        }
                    }
                }
            }
            catch (KeyNotFoundException ex)
            {
                LogUtils.D("Callback has been released.");
            }
            catch (Exception ex)
            {
                LogUtils.D(ex.ToString());
            }
        }

        /// <summary>
        /// 停止超时定时器
        /// </summary>
        public static void StopTimer()
        {
            Timer.Enabled = false;
            Timer.Stop();
            IsTimerStarted = false;
        }

        /// <summary>
        /// 重新设置Call的超时时间
        /// </summary>
        /// <param name="time">单位ms</param>
        public static void setCallBackTimeout(long time)
        {
            if (time <= 0)
            {
                LogUtils.E("timeout must > 0");
                return;
            }
            MaxTimeoutTime = time;
        }

        /// <summary>
        /// Unity注册接口形式的API接口
        /// </summary>
        /// <param name="name">接口名</param>
        /// <param name="api">接口实现</param>
        public static void RegisterAPI(string name, IBridgeAPI api)
        {
            if (string.IsNullOrEmpty(name) || api == null)
            {
                LogUtils.W("RegisterAPI:param can't be null.");
                return;
            }
            if (!ApiMap.ContainsKey(name))
            {
                ApiMap.Add(name, api);
            }
        }

        /// <summary>
        /// Unity注册委托形式的API
        /// </summary>
        /// <param name="name">接口名</param>
        /// <param name="api">接口实现</param>
        public static void RegisterAPI(string name, BridgeAPI api)
        {
            if (string.IsNullOrEmpty(name) || api == null)
            {
                LogUtils.W("RegisterAPI:param can't be null.");
                return;
            }
            if (!BridgeDelegateMap.ContainsKey(name))
            {
                BridgeDelegateMap.Add(name, api);
            }
        }


        /// <summary>
        /// Unity注册事件
        /// </summary>
        /// <param name="target"></param>
        public static void RegisterEvent(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("RegisterEvent:target can't be null.");
                return;
            }
            if (!LocalEventList.Contains(target))
            {
                LocalEventList.Add(target);
            }

        }

        /// <summary>
        ///  Unity发送事件消息
        /// </summary>
        /// <param name="target">目标事件</param>
        /// <param name="data">数据</param>
        public static bool SendEvent(string target, JsonData data)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("RegisterAPI:param null.");
                return false;
            }
            if (LocalEventList.Contains(target) && RemoteEventList.Contains(target))
            {
                return true;
            }
            LogUtils.W("LoalEventMap & RemoteEventMap is emtpy.");
            return false;
        }

        ///<summary>
        ///异步调用native的方法
        ///</summary>
        ///<param name="target">目标接口</param>
        ///<param name="param">参数数据</param>
        ///<param name="callBack">回调</param>
        ///
        public static long Call(string target, JsonData param, BridgeCallBack callBack)
        {
           return Call(target, param, callBack, 0);
        }


        ///<summary>
        ///异步调用native的方法
        ///</summary>
        ///<param name="target">目标接口</param>
        ///<param name="param">参数数据</param>
        ///<param name="callBack">回调</param>
        ///
        public static long Call(string target, JsonData param, BridgeCallBack callBack,long timeout)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("Call:target can't be null.");
                return 0;
            }
            callBack.TimeoutTime = timeout;
            long callbackId = MsgUtils.GetCallBackId();
            lock (_lock)
            {
                while (CallbackMap.ContainsKey(callbackId))
                {
                    callbackId += 1;
                }
                callBack.AddTime = DateTime.Now.Ticks;
                CallbackMap.Add(callbackId, callBack);
            }
            return callbackId;
        }

        /// <summary>
        /// MOCK异步调用native的方法
        /// </summary>
        /// <param name="target">目标接口</param>
        /// <param name="param">参数</param>
        /// <param name="mock">回包的MOCK数据</param>
        /// <param name="callBack">回调</param>
        public static void CallMock(string target, JsonData param, JsonData mock, BridgeCallBack callBack)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("CallMock:target can't be null.");
                return;
            }
            if (callBack != null && callBack.OnSuccess != null)
            {
                callBack.OnSuccess(mock);
            }
        }

        ///<summary>
        ///监听事件
        ///</summary>
        ///<param name="target">目标接口</param>
        ///<param name="overOld">新的监听是否覆盖旧的</param>
        ///<param name="callBack">回调</param> 
        ///
        public static void Listen(string target,bool overOld, BridgeCallBack callBack)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("Listen:target can't be null.");
                return;
            }
            if (overOld)
            {
                if (EventMap.ContainsKey(target))
                {
                    EventMap.Remove(target);
                }
                EventMap.Add(target, callBack);
            }
            else {
                if (!EventMap.ContainsKey(target))
                {
                    EventMap.Add(target, callBack);
                }
            }
          
        }

        /// <summary>
        /// MOCK监听事件
        /// </summary>
        /// <param name="target">目标事件</param>
        /// <param name="mock">回包的MOCK数据</param>
        /// <param name="callBack">回调</param>
        public static void ListenMock(string target, JsonData mock, BridgeCallBack callBack)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("ListenMock:target can't be null.");
                return;
            }
            if (callBack != null && callBack.OnSuccess != null)
            {
                callBack.OnSuccess(mock);
            }
        }

        ///<summary>
        ///关闭监听事件
        ///</summary>
        ///
        ///<param name="target">目标接口</param>
        ///
        public static void UnListen(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                LogUtils.W("UnListen:target can't be null.");
                return;
            }
            if (EventMap.ContainsKey(target))
            {
                EventMap.Remove(target);
            }
        }


        /// <summary>
        /// 处理Native调用过来的消息
        /// </summary>
        /// <param name="json">消息数据，JSON格式</param>
        public static void HandleMsgFromNative(string json)
        {
            LogUtils.D("HandleMsgFromNative:" , json);
            BridgeMsg msg = MsgUtils.ParseMsg(json);

            switch (msg.Source)
            {
                case SOURCE_NATIVE:
                case SOURCE_WEBGL:
                    HandleNativeCall(msg);
                    break;
                case SOURCE_UNITY:
                    HandleNativeCallBack(msg);
                    break;
                default:
                    break;
            }
        }

        public static void InvokeMockEvent(string target, JsonData data)
        {
            if (EventMap.ContainsKey(target))
            {
                BridgeCallBack callBack = EventMap[target];
                if (callBack != null && callBack.OnSuccess != null)
                {
                    callBack.OnSuccess(data);
                }
            }
        }


        /// <summary>
        /// 处理Native的调用消息。
        /// </summary>
        /// <param name="msg"></param>
        private static void HandleNativeCall(BridgeMsg msg)
        {
            LogUtils.D("HandleNativeCall");
            string target = msg.Target;
            if (!string.IsNullOrEmpty(target))
            {
                switch (msg.Type)
                {
                    case BridgeCore.TYPE_EVENT:
                        //处理Native过来的event
                        if (EventMap.ContainsKey(msg.Target))
                        {
                            BridgeCallBack callBack = EventMap[msg.Target];
                            if (callBack != null && callBack.OnSuccess != null)
                            {
                                callBack.OnSuccess(msg.Data);
                            }
                        }
                        break;
                    case BridgeCore.TYPE_CALL:
                        //处理Native过来的Call
                        UNBridgeContext context = new UNBridgeContext(msg);
                        IBridgeAPI bridgeAPI = null;
                        if (ApiMap.ContainsKey(target))
                        {
                            bridgeAPI = ApiMap[target];
                        }
                        BridgeAPI bridgeAPIDelegate = null;
                        if (BridgeDelegateMap.ContainsKey(target))
                        {
                            bridgeAPIDelegate = BridgeDelegateMap[target];
                        }
                        if (bridgeAPI != null)
                        {
                            LogUtils.D("OnCall:" , target);
                            bridgeAPI.OnCall(context, msg.Param);
                        }
                        else if (bridgeAPIDelegate != null && bridgeAPIDelegate.OnCallBridgeAPI != null)
                        {
                            LogUtils.D("OnCallBridgeAPI:" , target);
                            bridgeAPIDelegate.OnCallBridgeAPI(context, msg.Param);
                        }
                        else
                        {
                            LogUtils.W("There is No Api Named[" + target + "],Are you had registered?");
                            context.CallBackFailed(BridgeCore.ERROR_UNSUPPORT, "There is No Api Named[" + target + "],Are you had registered?");
                        }

                        break;
                    case BridgeCore.TYPE_LISTEN:
                        //添加事件
                        if (!RemoteEventList.Contains(target))
                        {
                            RemoteEventList.Add(target);
                        }
                        break;
                    case BridgeCore.TYPE_UNLISTEN:
                        //移除事件
                        if (RemoteEventList.Contains(target))
                        {
                            RemoteEventList.Remove(target);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 处理Unity调用Native回来的回包。
        /// </summary>
        /// <param name="msg"></param>
        private static void HandleNativeCallBack(BridgeMsg msg)
        {
            LogUtils.D("HandleNativeCallBack");
            if (msg.Code != BridgeCore.SUCCESS)
            {
                //失败情况
                bool foundCallback = false;
                BridgeCallBack callBack = null;
                lock (CallbackMap)
                {
                    if (CallbackMap.TryGetValue(msg.CallbackId, out callBack))
                    {
                        foundCallback = true;
                        CallbackMap.Remove(msg.CallbackId);
                    }
                }

                if (foundCallback)
                {
                    if (callBack != null && callBack.OnFailed != null)
                    {
                        try
                        {
                            callBack.OnFailed(msg.Code, msg.FailMsg);
                        }
                        catch (Exception ex)
                        {
                            LogUtils.E(ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                    }
                }
                
                return;
            }

            switch (msg.Type)
            {
                case BridgeCore.TYPE_CALL:
                    {
                        bool foundCallback = false;
                        BridgeCallBack callBack = null;
                        lock (CallbackMap)
                        {
                            if (CallbackMap.TryGetValue(msg.CallbackId, out callBack))
                            {
                                foundCallback = true;
                                CallbackMap.Remove(msg.CallbackId);
                            }
                        }

                        if (foundCallback)
                        {
                            if (callBack != null && callBack.OnSuccess != null)
                            {
                                try
                                {
                                    callBack.OnSuccess(msg.Data);
                                }
                                catch (Exception ex)
                                {
                                    LogUtils.E(ex.Message + Environment.NewLine + ex.StackTrace);
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;

            }
        }
    }
}
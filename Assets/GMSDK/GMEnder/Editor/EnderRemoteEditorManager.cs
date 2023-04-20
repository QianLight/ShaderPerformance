using System;
using System.Runtime.InteropServices;
using System.Timers;
using AOT;
using Timer = System.Timers.Timer;
using UnityEngine;
#if UNITY_EDITOR
using Ender.LitJson;
using GMSDK;

namespace Ender
{
    public class EnderRemoteEditorManager : ServiceSingleton<EnderRemoteEditorManager>
    {
        public const int OS_VERSION_NINE = 9;
        private const string UID_FORMAT = "ender_{0}_unity_{1}";
        private static IEnderRemoteCallback enderRemoteCallback;
        private static int connectCount;
        
        public static string uid = "";
        public static string channelId = "";
        public static string deviceSerial = "";
        public static long deviceDeadLine;
        public static int androidOsVersion;
        public static string dynamicCode;
        public static string packageUrl = "";
        public static string packageBuildTime = "";
        public static string appId = "";

        private static Timer _timer;
        
        public Action<int> onInstallResult;

        private delegate void enderRecvMsgCallback(string message);

        private delegate void enderRecvAlohaMsgCallback(string message);

        private delegate void enderConnectionsCallback(int count);

        private delegate void enderSelfStateCallback(int state);

        private delegate void enderOnErrorCallback(int error);

        [DllImport("GMEnderRemote")]
        private static extern void InitChannel(string roomid, string uid, string token);

        [DllImport("GMEnderRemote")]
        private static extern void ReleaseChannel();

        [DllImport("GMEnderRemote")]
        private static extern void ReNewToken(string token);

        [DllImport("GMEnderRemote")]
        private static extern void SendMsg(int msgType, string msg);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteMessageCallback(enderRecvMsgCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteAlohaMessageCallback(enderRecvAlohaMsgCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteConnectionsCallback(enderConnectionsCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteSelfStateCallback(enderSelfStateCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteOnErrorCallback(enderOnErrorCallback callback);

        [MonoPInvokeCallback(typeof(enderRecvMsgCallback))]
        public static void HandleMsgFromNative(string message)
        {
            Debug.Log("Ender Remote HandleMsgFromNative: " + message);
            if (enderRemoteCallback != null)
            {
                enderRemoteCallback.HandleEnderRemoteMsgFromNative(message);
            }
        }

        [MonoPInvokeCallback(typeof(enderRecvAlohaMsgCallback))]
        public static void HandleAlohaMsgFromNative(string message)
        {
            Debug.Log("Ender Remote HandleAlohaMsgFromNative: " + message);
            if (enderRemoteCallback != null)
            {
                enderRemoteCallback.HandleEnderRemoteAlohaMsgFromNative(message);
            }
        }

        [MonoPInvokeCallback(typeof(enderConnectionsCallback))]
        public static void HandleConnectionChange(int count)
        {
            Debug.Log("Ender Remote HandleConnectionChange: " + count);
            connectCount = count;
        }

        [MonoPInvokeCallback(typeof(enderSelfStateCallback))]
        public static void HandleSelfStateChange(int state)
        {
            Debug.Log("Ender Remote HandleSelfStateChange: " + state);
            if (state == EnderRemoteConstants.SelfState.Retry)
            {
                Debug.LogError("当前连接已断开，请检查网络状态，重试中...");
            }
        }

        [MonoPInvokeCallback(typeof(enderOnErrorCallback))]
        public static void HandleError(int error)
        {
            Debug.Log("Ender Remote HandleError: " + error);
            if (error == -1000) // token invalid
            {
                EnderHttpRequestUtils.GetRtcToken(channelId, uid, (token) =>
                {
                    if (String.IsNullOrEmpty(token))
                    {
                        Debug.LogError("renew token, get token failed");
                        return;
                    }

                    ReNewToken(token);
                });
            }
            else
            {
                //todo handle other error
            }
        }

        private EnderRemoteEditorManager()
        {
        }

        ~EnderRemoteEditorManager()
        {
            Release();
            Debug.Log("EnderRemoteEditorManager Release");
        }

        public void Init(string appid, string sdkVersion, EnderRemoteConstants.EnderPlatform platform,
            Action<int> onResult)
        {
            QueryEnderRemoteInstallPackage(appid, sdkVersion, platform, onResult, false);
        }

        /**
         * step 1：获取对应的安装包
         */
        public void QueryEnderRemoteInstallPackage(string appid, string sdkVersion,
            EnderRemoteConstants.EnderPlatform platform, Action<int> onResult, bool reInstall)
        {
            EnderHttpRequestUtils.QueryEnderRemoteInstallPackage(appid, sdkVersion, platform, (response) =>
            {
                if (String.IsNullOrEmpty(response))
                {
                    Debug.LogError("Init step1: install package url is empty 1");
                    onResult(EnderRemoteConstants.DeployErrorStatus.UnavailableInstallPackage);
                    return;
                }

                JsonData result = JsonMapper.ToObject(response);
                string url = result["packageUrl"].ToString();
                if (String.IsNullOrEmpty(url))
                {
                    Debug.LogError("Init step1: install package url is empty 2");
                    onResult(EnderRemoteConstants.DeployErrorStatus.UnavailableInstallPackage);
                }
                else
                {
                    Debug.Log("Init step1: install package url: " + url);
                    packageUrl = url;
                    packageBuildTime = result["buildTime"].ToString();
                    onResult(EnderRemoteConstants.DeployStatus.GetInstallPackage);
                    OccupyEnderRemoteDevice(platform, onResult, reInstall);
                }
            });
        }

        /**
         *  step 2：占用一台云真机设备
         */
        public void OccupyEnderRemoteDevice(EnderRemoteConstants.EnderPlatform platform, Action<int> onResult,
            bool reInstall)
        {
            if (reInstall)
            {
                onResult(EnderRemoteConstants.DeployStatus.OccupyDevice);
                InstallPackageToCloudDevice(onResult);
                return;
            }

            //首先从白名单列表查找设备，白名单设备会提前安装好应用
            EnderHttpRequestUtils.QueryWhiteDevice(appId, packageUrl, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.Log("OccupyEnderRemoteDevice");
                    //白名单设备列表都不可用，从设备池寻找
                    EnderHttpRequestUtils.OccupyEnderRemoteDevice(platform, (response) =>
                    {
                        if (String.IsNullOrEmpty(response))
                        {
                            Debug.LogError("Init step2: device serial is empty");
                            onResult(EnderRemoteConstants.DeployErrorStatus.OccupyDeviceFailed);
                        }
                        else
                        {
                            JsonData ocResult = JsonMapper.ToObject(response);
                            string serial = ocResult["serial"].ToString();
                            deviceDeadLine = long.Parse(ocResult["deadline"].ToString()) * 1000;
                            androidOsVersion = int.Parse(ocResult["version"].ToString());
                            Debug.Log("Init step2: device serial: " + serial + " ， deadline: " + deviceDeadLine + " , osVersion: " + androidOsVersion);
                            deviceSerial = serial;
                            onResult(EnderRemoteConstants.DeployStatus.OccupyDevice);
                            InitRemoteNetTool(appId);
                            InstallPackageToCloudDevice(onResult);
                            EnderHttpRequestUtils.AddDeviceWhiteList(appId, serial,
                                (success) => { Debug.Log("add cloud device to white list, result: " + success); });
                        }
                    });
                }
                else
                {
                    //命中白名单
                    JsonData resultJson = JsonMapper.ToObject(result);
                    string serial = resultJson["serial"].ToString();
                    deviceDeadLine = long.Parse(resultJson["deadline"].ToString()) * 1000;
                    androidOsVersion = int.Parse(resultJson["version"].ToString());
                    Debug.Log("Init step2 from whitelist: device serial: " + serial + " ， deadline: " + deviceDeadLine + " , osVersion: " + androidOsVersion);
                    deviceSerial = serial;
                    onResult(EnderRemoteConstants.DeployStatus.OccupyDevice);
                    InitRemoteNetTool(appId);
                    EnderHttpRequestUtils.QueryRemotePackageIsInstalled(appId, serial, packageUrl, (installed) =>
                    {
                        if (installed)
                        {
                            Debug.Log("QueryRemotePackageIsInstalled, installed");
                            EnderHttpRequestUtils.LaunchApp(appId, deviceSerial, isOk =>
                            {
                                Debug.Log(isOk ? "launch app success" : "launch app failed, please manually open");
                                onInstallResult(EnderRemoteConstants.DeployErrorStatus.NoError);
                            });
                        }
                        else
                        {
                            Debug.Log("QueryRemotePackageIsInstalled, not install");
                            InstallPackageToCloudDevice(onResult);
                        }
                    });
                }
            });
        }


        /**
         *  step 3：安装包到云真机
         */
        public void InstallPackageToCloudDevice(Action<int> onResult)
        {
            onInstallResult = onResult;
            Debug.Log("Init step3: install package，" + "package url: " + packageUrl);
            EnderHttpRequestUtils.InstallPackage(packageUrl, deviceSerial, success =>
            {
                if (success)
                {
                    TimerElapsed();
                }
                else
                {
                    onInstallResult(EnderRemoteConstants.DeployErrorStatus.InstallFailed);
                }
            });
        }

        public void ReInstallToOtherDeviceIfNeed(EnderRemoteConstants.EnderPlatform platform, Action<int> onResult)
        {
            EnderHttpRequestUtils.IsRemoteDeviceInstallFailed(deviceSerial, isFailed =>
            {
                if (isFailed)
                {
                    EnderHttpRequestUtils.AddDeviceBlackList(deviceSerial, success =>
                    {
                        EnderHttpRequestUtils.ReleaseEnderRemoteDevice(deviceSerial);
                        OccupyEnderRemoteDevice(platform, onResult, false); //查找新设备并部署
                    });
                }
                else
                {
                    OccupyEnderRemoteDevice(platform, onResult, true); //在该设备重新安装
                }
            });
        }

        private void TimerElapsed()
        {
            if (_timer == null)
            {
                _timer = new Timer {Enabled = true, Interval = 3000};
            }

            _timer.Stop();
            _timer.AutoReset = true;
            _timer.Elapsed += TimerHandler;
            _timer.Start();
        }

        private void TimerHandler(object source, ElapsedEventArgs e)
        {
            if (String.IsNullOrEmpty(packageUrl) || String.IsNullOrEmpty(deviceSerial))
            {
                Debug.Log("package url or device serial is null");
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }

                return;
            }

            EnderHttpRequestUtils.QueryInstallStatus(packageUrl, deviceSerial, isDone =>
            {
                if (isDone)
                {
                    Debug.Log("install success");
                    if (_timer != null)
                    {
                        _timer.Stop();
                        _timer = null;
                    }

                    EnderHttpRequestUtils.LaunchApp(appId, deviceSerial, isOk =>
                    {
                        Debug.Log(isOk ? "launch app success" : "launch app failed, please manually open");
                        onInstallResult(EnderRemoteConstants.DeployErrorStatus.NoError);
                    });
                }
                else
                {
                    onInstallResult(EnderRemoteConstants.DeployErrorStatus.InstallFailed);
                    Debug.Log("install failed");
                    if (_timer != null)
                    {
                        _timer.Stop();
                    }
                }
            });
        }

        public void ReInstallNewPackage(string appid, string sdkVersion, EnderRemoteConstants.EnderPlatform platform,
            Action<int> onResult)
        {
            InitRemoteNetTool(appid);
            QueryEnderRemoteInstallPackage(appid, sdkVersion, platform, onResult, true);
        }

        public void AddTimeManual(string serial, Action<long> onResult)
        {
            EnderHttpRequestUtils.AddDeviceTime(serial, success =>
            {
                if (success)
                {
                    EnderHttpRequestUtils.QueryEnderRemoteDeviceInfo(serial, response =>
                    {
                        JsonData data = JsonMapper.ToObject(response);
                        long newDeadLine = long.Parse(data["occupation_deadline"].ToString()) * 1000;
                        Debug.Log("AddTimeManual, device new deadline: " + newDeadLine);
                        deviceDeadLine = newDeadLine;
                        onResult(newDeadLine);
                    });
                }
            });
        }

        public void RestoreStatus(EnderSettingsModel model)
        {
            Debug.Log("restore status when reload, current deploy status: " + model.currentDeployStatus);
            uid = model.uid;
            channelId = model.channelId;
            packageUrl = model.packageUrl;
            packageBuildTime = model.packageBuildTime;
            deviceSerial = model.deviceSerial;
            appId = model.appId;
            dynamicCode = model.dynamicCode;
            androidOsVersion = model.androidOsVersion;
            deviceDeadLine = model.deviceDeadLine;
            if (model.currentDeployStatus >= EnderRemoteConstants.DeployStatus.InstallPackage && !model.remoteVerified)
            {
                InitRemoteNetTool(appId);
                if (model.currentDeployStatus == EnderRemoteConstants.DeployStatus.InstallPackage)
                {
                    TimerElapsed();
                }
            }
        }

        public void InitRemoteNetTool(string appid)
        {
            ReleaseRemoteNetTool();
            if (String.IsNullOrEmpty(uid))
            {
                uid = String.Format(UID_FORMAT, appid, GetTimeStamp());
            }

            InitChannelId();
            if (String.IsNullOrEmpty(channelId))
            {
                Debug.Log("channel id is empty");
                return;
            }

            EnderHttpRequestUtils.GetRtcToken(channelId, uid, (token) =>
            {
                if (String.IsNullOrEmpty(token))
                {
                    Debug.LogError("ender remote: get token failed");
                    return;
                }

                Debug.Log("remote net tool token: " + token);
                Debug.Log("uid: " + uid + " , channelId: " + channelId);

                InitChannel(channelId, uid, token);
                SetEnderRemoteMessageCallback(HandleMsgFromNative);
                SetEnderRemoteAlohaMessageCallback(HandleAlohaMsgFromNative);
                SetEnderRemoteConnectionsCallback(HandleConnectionChange);
                SetEnderRemoteSelfStateCallback(HandleSelfStateChange);
                SetEnderRemoteOnErrorCallback(HandleError);
            });
        }

        private void InitChannelId()
        {
            channelId = androidOsVersion > OS_VERSION_NINE ? dynamicCode : deviceSerial;
        }

        // 获取当前时间戳 ms
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var ret = Convert.ToInt64(ts.TotalSeconds * 1000);
            return ret;
        }

        public static string GetDate(long timestamp)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return startTime.AddTicks(timestamp * 10000).ToString("MM-dd HH:mm:ss");
        }

        public void Release()
        {
            packageUrl = "";
            deviceSerial = "";
            dynamicCode = "";
            androidOsVersion = 0;
            channelId = "";
            uid = "";
            if (_timer != null)
            {
                _timer.Stop();
            }

            ReleaseRemoteNetTool();
        }

        public void ReleaseRemoteNetTool()
        {
            connectCount = 0;
            ReleaseChannel();
            SetEnderRemoteMessageCallback(null);
            SetEnderRemoteAlohaMessageCallback(null);
            SetEnderRemoteConnectionsCallback(null);
            SetEnderRemoteSelfStateCallback(null);
            SetEnderRemoteOnErrorCallback(null);
        }

        public void SetEnderRemoteCallback(IEnderRemoteCallback remoteCallback)
        {
            Debug.Log("SetEnderRemoteCallback");
            enderRemoteCallback = remoteCallback;
        }

        public bool IsConnected()
        {
            return connectCount > 0;
        }

        public void SendMessage(int msgType, string message)
        {
            SendMsg(msgType, message);
        }
    }
}
#endif
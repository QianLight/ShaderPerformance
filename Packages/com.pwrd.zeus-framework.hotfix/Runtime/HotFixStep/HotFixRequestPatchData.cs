/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Networking;

namespace Zeus.Framework.Hotfix
{
    public class HotFixRequestPatchData : BaseHotFixStep
    {
        public HotFixRequestPatchData(HotfixService executer) : base(executer, HotfixStep.RequestPatchData, 0)
        { }
        public override void Run()
        {
            if(null == _hotFixExecuter.ResVersion)
            {
                OnError(HotfixError.NoResVersionData, "no ResVersionData found, and create failed");
                return;
            }
            if (!_hotFixExecuter.LocalConfig.openHotfix ||
                (!string.IsNullOrEmpty(_hotFixExecuter.CustomTargetVersion) && _hotFixExecuter.CustomTargetVersion.Equals(_hotFixExecuter.ResVersion.ResVersion)))
            {
                Finish();
            }
            else
            {
                OnProcess(0, 1);
                _hotFixExecuter.HFControlDataList.Clear();
                SendRequest(_hotFixExecuter.ResVersion.ResVersion);
            }
        }

        private void SendRequest(string version)
        {
            if (_hotFixExecuter.ControlDataUrls == null || _hotFixExecuter.ControlDataUrls.Length == 0)
            {
                OnError(HotfixError.InitError, "ControlDataUrls is null or ControlDataUrls.Length == 0");
            }
            else
            {
                HotfixInfoRequest hotfixRequest = new HotfixInfoRequest(_hotFixExecuter.ControlDataUrls, _hotFixExecuter.LocalConfig,
                    version, this.HotFixRequestCallback, _hotFixExecuter.UrlRefreshParam);
                hotfixRequest.Start();
            }
        }

        private void HotFixRequestCallback(HFControlData hFControlData, DateTime serverData, HotfixError error, string errorInfo)
        {
            if (hFControlData != null && error == HotfixError.Null)
            {
                _hotFixExecuter.ServerDate = serverData;
                string targetVersion;
                if (IsValidHFControlData(hFControlData, out targetVersion))
                {
                    _hotFixExecuter.HFControlDataList.Add(hFControlData);
                    SendRequest(targetVersion);
                }
                else
                {
                    OnProcess(1, 1);
                    NextStep();
                }
            }
            else
            {
                if (_hotFixExecuter.LocalConfig.ignoreInit404Error && error == HotfixError.HttpStatusCode404Error)
                {
                    Finish();
                }
                else
                {
                    OnError(error, errorInfo);
                }
            }
        }

        private bool IsValidHFControlData(HFControlData controlData, out string targetVersion)
        {
            if (controlData.PatchConfigData != null &&
                    controlData.PatchConfigData.Type != HotFixType.None &&
                    !string.IsNullOrEmpty(controlData.PatchConfigData.TargetVersion))
            {
                targetVersion = controlData.PatchConfigData.TargetVersion;
                return true;
            }
            if (controlData.AppRecommendPatchConfigData != null &&
                controlData.AppRecommendPatchConfigData.Type != HotFixType.None &&
                !string.IsNullOrEmpty(controlData.AppRecommendPatchConfigData.TargetVersion))
            {
                targetVersion = controlData.AppRecommendPatchConfigData.TargetVersion;
                return true;
            }
            if (controlData.TestingData != null &&
                controlData.TestingData.PatchConfigData != null &&
                controlData.TestingData.PatchConfigData.Type != HotFixType.None &&
                !string.IsNullOrEmpty(controlData.TestingData.PatchConfigData.TargetVersion))
            {
                bool testDevice = false;
                switch (_hotFixExecuter.LocalConfig.testMode)
                {
                    case HotfixLocalConfig.HotfixTestMode.Default:
                        {
#if UNITY_IOS
                            HotfixLogger.Log("Device.advertisingIdentifier" + UnityEngine.iOS.Device.advertisingIdentifier);
                            //ios设备根据广告码来确定测试设备
                            testDevice = controlData.TestingData.IDFAList != null && controlData.TestingData.IDFAList.Contains(UnityEngine.iOS.Device.advertisingIdentifier);
#else
                            //iOS以外的平台设备根据热更目录内的标记文件来确定测试设备
                            string testMarkFile = Zeus.Core.FileSystem.OuterPackage.GetRealPath("kramtset.kramtset");//testmark倒序,内容abcd123456
                            if (File.Exists(testMarkFile))
                            {
                                testDevice = MD5Util.GetMD5FromFile(testMarkFile).Equals("da3177cbd9f064004b6a0d59a3a484bb");
                            }
#endif
                        }
                        break;
                    case HotfixLocalConfig.HotfixTestMode.Testing:
                        {
                            testDevice = true;
                        }
                        break;
                    case HotfixLocalConfig.HotfixTestMode.OnlyNormal:
                        {
                            testDevice = false;
                        }
                        break;
                }
                if (testDevice)
                {
                    targetVersion = controlData.TestingData.PatchConfigData.TargetVersion;
                    return true;
                }
            }
            targetVersion = null;
            return false;
        }

        private bool IsAppStoreUpdate(HFControlData controlData)
        {
            if (controlData.PatchConfigData != null &&
                    controlData.PatchConfigData.Type != HotFixType.None &&
                    !string.IsNullOrEmpty(controlData.PatchConfigData.TargetVersion))
            {
                if (controlData.PatchConfigData.Type == HotFixType.AppStore)
                    return true;
            }
            if (controlData.AppRecommendPatchConfigData != null &&
                controlData.AppRecommendPatchConfigData.Type != HotFixType.None &&
                !string.IsNullOrEmpty(controlData.AppRecommendPatchConfigData.TargetVersion))
            {
                if (controlData.AppRecommendPatchConfigData.Type == HotFixType.AppStoreRecommend)
                    return true;
            }
            if (controlData.TestingData != null &&
                controlData.TestingData.PatchConfigData != null &&
                controlData.TestingData.PatchConfigData.Type != HotFixType.None &&
                !string.IsNullOrEmpty(controlData.TestingData.PatchConfigData.TargetVersion))
            {
                bool testDevice = false;
                switch (_hotFixExecuter.LocalConfig.testMode)
                {
                    case HotfixLocalConfig.HotfixTestMode.Default:
                        {
#if UNITY_IOS
                            HotfixLogger.Log("Device.advertisingIdentifier" + UnityEngine.iOS.Device.advertisingIdentifier);
                            //ios设备根据广告码来确定测试设备
                            testDevice = controlData.TestingData.IDFAList != null && controlData.TestingData.IDFAList.Contains(UnityEngine.iOS.Device.advertisingIdentifier);
#else
                            //iOS以外的平台设备根据热更目录内的标记文件来确定测试设备
                            string testMarkFile = Zeus.Core.FileSystem.OuterPackage.GetRealPath("kramtset.kramtset");//testmark倒序,内容abcd123456
                            if (File.Exists(testMarkFile))
                            {
                                testDevice = MD5Util.GetMD5FromFile(testMarkFile).Equals("da3177cbd9f064004b6a0d59a3a484bb");
                            }
#endif
                        }
                        break;
                    case HotfixLocalConfig.HotfixTestMode.Testing:
                        {
                            testDevice = true;
                        }
                        break;
                    case HotfixLocalConfig.HotfixTestMode.OnlyNormal:
                        {
                            testDevice = false;
                        }
                        break;
                }
                if (testDevice)
                {
                    if (controlData.TestingData.PatchConfigData.Type == HotFixType.AppStore || controlData.TestingData.PatchConfigData.Type == HotFixType.AppStoreRecommend)
                        return true;
                }
            }
            return false;
        }
    }

    public class HotfixInfoRequest
    {
        Action<HFControlData, DateTime, HotfixError, string> OnFinish;

        HotfixLocalConfig LocalConfig;
        string ResVersion;
        HFControlData HFControlData;
        string[] _serverUrls;
        string refreshStr;

        int urlIndex = 0;
        int resultCode = -1;
        string resultJson = string.Empty;
        DateTime? serverDate = null;

        public HotfixInfoRequest(string[] urls, HotfixLocalConfig LocalConfig, string ResVersion,
            Action<HFControlData, DateTime, HotfixError, string> OnFinish, string refreshStr = null)
        {
            this.refreshStr = refreshStr;
            this._serverUrls = urls;
            this.LocalConfig = LocalConfig;
            this.ResVersion = ResVersion;
            if (OnFinish == null)
            {
                HotfixLogger.LogError("callback is null");
            }
            this.OnFinish = OnFinish;
        }

        public void Start()
        {
            urlIndex = 0;
            resultCode = -1;
            resultJson = string.Empty;
            serverDate = null;

            SendRequest();
        }

        private void SendRequest()
        {
            if (_serverUrls == null || _serverUrls.Length == 0)
            {
                if (OnFinish != null)
                {
                    OnFinish(HFControlData,
                        serverDate == null ? DateTime.UtcNow : serverDate.Value,
                        HotfixError.InitError,
                        "LocalConfig.serverUrls is null or .LocalConfig.serverUrls.Count == 0");
                }
            }
            else
            {
                int index = Mathf.Min(urlIndex, _serverUrls.Length - 1);
                string url;
                if (!string.IsNullOrEmpty(refreshStr))
                {
                    url = Path.Combine(_serverUrls[index], LocalConfig.ChannelId, ResVersion, ResVersion + ".json" + refreshStr);
                }
                else
                {
                    //url加一个每10分钟会变化一次的时间参数，是为了使cdn在参数变化之后从源站拉取更新数据
                    string timeParam = string.Format("?Time={0}{1}{2}{3}{4}", DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                        DateTime.UtcNow.Hour, DateTime.UtcNow.Minute / LocalConfig.urlRefreshParam);
                    url = Path.Combine(_serverUrls[index], LocalConfig.ChannelId, ResVersion, ResVersion + ".json" + timeParam);
                }

                HotfixLogger.Log("Request Remote Version,url:", url);
                //发送请求
                Zeus.Framework.Http.HttpManager.Instance().Get(url, 5, RespCallback);
            }
        }

        private void RespCallback(UnityWebRequest _request)
        {
            bool suc = false;
            resultCode = (int)_request.responseCode;
            if (!_request.isHttpError && !_request.isNetworkError)
            {
                if (resultCode == 200)
                {
                    suc = true;
                }
            }

            if (suc)
            {
                resultJson = DownloadHandlerBuffer.GetContent(_request);
                Dictionary<string, string> dic = _request.GetResponseHeaders();
                string date;
                int age = 0;
                string tmp;
                if (dic.TryGetValue("Age", out tmp))
                {
                    int.TryParse(tmp, out age);
                }
                if (dic.TryGetValue("Date", out date))
                {
                    DateTime dt;
                    if (DateTime.TryParse(date, out dt))
                    {
                        serverDate = dt.ToUniversalTime().AddSeconds(age);
                    }
                }
                HandleResponse(resultCode, resultJson, serverDate);
            }
            else
            {
                if (resultCode == 404)
                {
                    SwitchUrl(HotfixError.HttpStatusCode404Error, "HttpStatusCode404Error, Can not get remote HFControlData json.");
                }
                else
                {
                    SwitchUrl(HotfixError.NetError, "Can not get remote HFControlData json.");
                }
            }

            _request.Dispose();
        }

        private void HandleResponse(int resultCode, string resultJson, DateTime? serverDate)
        {
            HotfixLogger.Log("Remote Version Response,resultCode:", resultCode.ToString(), ",resultJson:", resultJson);
#if UNITY_EDITOR
            UnityEngine.Debug.Log("Remote Version Response,resultCode:" + resultCode.ToString() + ",resultJson:" + resultJson);
#endif
            bool exception = false;
            try
            {
                HFControlData = JsonUtility.FromJson<HFControlData>(resultJson);
            }
            catch (System.Exception e)
            {
                exception = true;
                SwitchUrl(HotfixError.Exception, e.ToString());
            }
            if (!exception)
            {
                if (HFControlData != null)
                {
                    if (OnFinish != null)
                    {
                        OnFinish(HFControlData, serverDate == null ? DateTime.UtcNow : serverDate.Value, HotfixError.Null, string.Empty);
                    }
                }
                else
                {
                    SwitchUrl(HotfixError.InitError, "HFControlData is null,Json:" + resultJson);
                }
            }
        }

        private void SwitchUrl(HotfixError error, string errorInfo)
        {
            urlIndex++;
            if (urlIndex >= _serverUrls.Length)
            {
                if (OnFinish != null)
                {
                    OnFinish(HFControlData, serverDate == null ? DateTime.UtcNow : serverDate.Value, error, errorInfo);
                }
            }
            else
            {
                SendRequest();
            }
        }
    }
}
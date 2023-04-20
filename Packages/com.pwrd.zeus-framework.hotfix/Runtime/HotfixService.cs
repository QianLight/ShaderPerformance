/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeus.Core;
using System;
using Zeus.Framework.Http.UnityHttpDownloader;

namespace Zeus.Framework.Hotfix
{
    public abstract class HotfixService
    {

        public static readonly string HotFixTempFolder = "HotFixTemp";
        public static readonly string HotFixTempUnzipFolder = "HotFixTemp/UnzipTemp";
        public static readonly string LocalVerDataKey = "ZeusHotfixResVersionDataKey";
        public static readonly string LocalVerDataFile = "zeus-hf-data.json";


        /// <summary>
        /// 安装包内置版本，除非换包，否则不会变化
        /// </summary>
        HotfixLocalConfig _localConfig;

        /// <summary>
        /// 热更下来的资源版本，初始为安装包内置版本，以后随热更资源变化
        /// </summary>
        ResVersionData _resVersion;

        /// <summary>
        /// 热更控制数据
        /// </summary>
        List<HFControlData> _hFControlData;
        /// <summary>
        /// 热更数据
        /// </summary>
        List<HFPatchConfigData> _hFPatchDatas;
        /// <summary>
        /// 达到开启时间的热更数据
        /// </summary>
        List<HFPatchConfigData> _reachOpentimeHFPatchDatas;
        /// <summary>
        /// 未达到开启时间的热更数据
        /// </summary>
        List<HFPatchConfigData> _unReachOpentimeHFPatchDatas;
        /// <summary>
        /// 需要进行预下载的热更数据
        /// </summary>
        List<HFPatchConfigData> _preDownloadHFPatchDataList;
        /// <summary>
        /// 商店更新热更数据
        /// </summary>
        HFPatchConfigData _appStoreHFPatchData;
        /// <summary>
        /// 商店推荐更新更新热更数据
        /// </summary>
        HFPatchConfigData _appStoreRecommendHFPatchData;
        /// <summary>
        /// 服务器时间
        /// </summary>
        DateTime _serverDate;
        float _syncTime;

        bool _registedUpdate = false;
        WaitForTrigger _finishTrigger;
        BaseHotFixStep _curStepObject;
        HotfixStep _curStepType;

        private long _downloadSpeed;
        private long _downloadTime;

        private string _customTargetVersion;
        private string[] _customUrls;
        private string[] _customIndependentControlDataUrls;

        public HotfixLocalConfig LocalConfig
        {
            get
            {
                if (_localConfig == null)
                {
                    _localConfig = HotfixLocalConfigHelper.LoadLocalConfig();
                }
                return _localConfig;
            }
        }

        /// <summary>
        /// 有可能为null，比如在没有找到文件，并且创建失败的情况下
        /// </summary>
        public ResVersionData ResVersion
        {
            get
            {
                if (_resVersion == null)
                {
                    _resVersion = ResVersionData.Load();
                }
                return _resVersion;
            }
        }
        public List<HFControlData> HFControlDataList
        {
            get
            {
                if (_hFControlData == null)
                {
                    _hFControlData = new List<HFControlData>();
                }
                return _hFControlData;
            }
        }
        public List<HFPatchConfigData> HFPatchDataList
        {
            get
            {
                if (_hFPatchDatas == null)
                {
                    _hFPatchDatas = new List<HFPatchConfigData>();
                }
                return _hFPatchDatas;
            }
        }
        public List<HFPatchConfigData> ReachOpenTimeHFPatchDataList
        {
            get
            {
                if (_reachOpentimeHFPatchDatas == null)
                {
                    _reachOpentimeHFPatchDatas = new List<HFPatchConfigData>();
                }
                return _reachOpentimeHFPatchDatas;
            }
        }
        public List<HFPatchConfigData> UnReachOpenTimeHFPatchDataList
        {
            get
            {
                if (_unReachOpentimeHFPatchDatas == null)
                {
                    _unReachOpentimeHFPatchDatas = new List<HFPatchConfigData>();
                }
                return _unReachOpentimeHFPatchDatas;
            }
        }
        public List<HFPatchConfigData> PreDownloadHFPatchDataList
        {
            get
            {
                if (_preDownloadHFPatchDataList == null)
                {
                    _preDownloadHFPatchDataList = new List<HFPatchConfigData>();
                }
                return _preDownloadHFPatchDataList;
            }
        }
        public HFPatchConfigData AppStoreHFPatchData
        {
            get
            {
                return _appStoreHFPatchData;
            }
            set
            {
                _appStoreHFPatchData = value;
            }
        }
        public HFPatchConfigData AppStoreRecommendHFPatchData
        {
            get
            {
                return _appStoreRecommendHFPatchData;
            }
            set
            {
                _appStoreRecommendHFPatchData = value;
            }
        }
        public DateTime ServerDate
        {
            get
            {
                return _serverDate.AddSeconds(Time.realtimeSinceStartup - _syncTime);
            }
            set
            {
                _serverDate = value;
                _syncTime = Time.realtimeSinceStartup;
            }
        }
        /// <summary>
        /// 当前本地资源版本号
        /// </summary>
        public string CurVersion
        {
            get
            {
                if (ResVersion != null)
                {
                    return ResVersion.ResVersion;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 将要下载的资源版本号
        /// </summary>
        public string TargetVersion
        {
            get
            {
                if (AppStoreHFPatchData != null)
                {
                    return AppStoreHFPatchData.TargetVersion;
                }
                if (AppStoreRecommendHFPatchData != null)
                {
                    return AppStoreRecommendHFPatchData.TargetVersion;
                }
                if (ReachOpenTimeHFPatchDataList != null && ReachOpenTimeHFPatchDataList.Count > 0)
                {
                    return ReachOpenTimeHFPatchDataList[ReachOpenTimeHFPatchDataList.Count - 1].TargetVersion;
                }
                if (UnReachOpenTimeHFPatchDataList != null && UnReachOpenTimeHFPatchDataList.Count > 0)
                {
                    return UnReachOpenTimeHFPatchDataList[0].TargetVersion;
                }
                return string.Empty;
            }
        }
        /// <summary>
        /// 平均下载速度
        /// </summary>
        public long AvgDownloadSpeed { get { return _downloadSpeed; } set { _downloadSpeed = value; } }
        /// <summary>
        /// 下载用时
        /// </summary>
        public long DownloadTime { get { return _downloadTime; } set { _downloadTime = value; } }

        /// <summary>
        /// 外部指定url，如果有值的话，会在热更时替换掉包内配置文件的serverUrls
        /// </summary>
        public string[] CustomUrls { get { return _customUrls; } }

        /// <summary>
        /// 外部指定url，如果有值的话，会在热更时替换掉包内配置文件的controlDataUrls
        /// </summary>
        public string[] CustomIndependentControlDataUrls { get { return _customIndependentControlDataUrls; } }

        public string[] ControlDataUrls
        {
            get
            {
                if (LocalConfig.independentControlDataUrl)
                {
                    if (CustomIndependentControlDataUrls != null && CustomIndependentControlDataUrls.Length > 0)
                    {
                        return CustomIndependentControlDataUrls;
                    }
                    if (LocalConfig.controlDataUrls != null && LocalConfig.controlDataUrls.Count > 0)
                    {
                        return LocalConfig.controlDataUrls.ToArray();
                    }
                }
                else
                {
                    if (CustomUrls != null && CustomUrls.Length > 0)
                    {
                        return CustomUrls;
                    }
                    if (LocalConfig.serverUrls != null)
                    {
                        return LocalConfig.serverUrls.ToArray();
                    }
                }
                return new string[0];
            }
        }

        public string[] PatchDownloadUrls
        {
            get
            {
                if (CustomUrls != null && CustomUrls.Length > 0)
                {
                    return CustomUrls;
                }
                if (LocalConfig != null && LocalConfig.serverUrls != null)
                {
                    return LocalConfig.serverUrls.ToArray();
                }
                return new string[0];
            }
        }


        /// <summary>
        /// 外部指定的目标版本号，如果有值且与本地值一致，将跳过热更
        /// </summary>
        public string CustomTargetVersion { get { return _customTargetVersion; } }

        /// <summary>
        /// 测试版就只用时间刷新，使用时间就是Time，使用版本号就是version，如果测试版指定了目标版本，也要用时间刷新。如果正式版没指定版本号，用时间刷新参数，指定了参数用参数刷新
        /// </summary>
        public string UrlRefreshParam
        {
            get
            {
                return GetUrlRefreshParam(LocalConfig, CustomTargetVersion);
            }
        }

        public HotfixService()
        {
            InitHasLaunchedGame();
            ResVersionData.InitHotfixLocalConfigSettingValue(LocalConfig);
        }

        public CustomYieldInstruction Execute()
        {
            _finishTrigger = new WaitForTrigger();
            HotfixLogger.Log("HotfixService execute");

            StartHotFix();

            return _finishTrigger;
        }

        private void StartHotFix()
        {
            if (IsPreDownloading())
            {
                StopPreDownLoad();
            }
            if (!_registedUpdate)
            {
                _registedUpdate = true;
                Zeus.Core.ZeusCore.Instance.RegisterUpdate(Update);
            }
            bool openHotfix = true;
#if UNITY_EDITOR
            openHotfix = PlayerPrefs.GetInt("OpenHotfixEditor", 0) == 1;
#endif
            if (openHotfix)
            {
                _curStepType = HotfixStep.None;
                _curStepObject = null;
                NextStep();
            }
            else
            {
                Finish();
            }
        }

        private void ReStartHotFix()
        {
            bool openHotfix = true;
#if UNITY_EDITOR
            openHotfix = PlayerPrefs.GetInt("OpenHotfixEditor", 0) == 1;
#endif
            if (openHotfix)
            {
                _curStepType = HotfixStep.None;
                _curStepObject = null;
                NextStep();
            }
            else
            {
                Finish();
            }
        }

        internal void NextStep()
        {
            _curStepType++;
            switch (_curStepType)
            {
                case HotfixStep.CheckAndFinishLastHotfix:
                    EnterStep(new HotFixCheckAndFinishLastHotfix(this));
                    break;
                case HotfixStep.RequestPatchData:
                    EnterStep(new HotFixRequestPatchData(this));
                    break;
                case HotfixStep.Check:
                    EnterStep(new HotFixCheck(this));
                    break;
                case HotfixStep.Download:
                    EnterStep(new HotFixDownload(this));
                    break;
                case HotfixStep.Unzip:
                    EnterStep(new HotFixUnzip(this));
                    break;
                case HotfixStep.Report:
                    EnterStep(new HotFixReport(this));
                    break;
                case HotfixStep.Finish:
                    Finish();
                    break;
            }
        }

        private void EnterStep(BaseHotFixStep step)
        {
            _curStepObject = step;
            _curStepObject.Run();
        }

        public bool StartPreDownload()
        {
            return StartPreDownload(PreDownloadHFPatchDataList, LocalConfig, PatchDownloadUrls);
        }

        private static bool StartPreDownload(List<HFPatchConfigData> list, HotfixLocalConfig config, string[] urls)
        {
            if (list.Count > 0)
            {
                list.RemoveAll(t => { return t == null || (t.Type != HotFixType.Force && t.Type != HotFixType.Recommend); });
            }
            if (list.Count <= 0)
            {
                return false;
            }
            if (HotFixPreDownload.Instance.IsDownloading())
            {
                HotFixPreDownload.Instance.StopDownload();
            }
            HotFixPreDownload.Instance.Init(list, config, urls);
            return HotFixPreDownload.Instance.Run();
        }

        private static void AddPerDownloadData(List<HFPatchConfigData> list)
        {
            if (list.Count > 0)
            {
                list.RemoveAll(t => { return t == null || (t.Type != HotFixType.Force && t.Type != HotFixType.Recommend); });
            }
            if (list.Count > 0)
            {
                HotFixPreDownload.Instance.Add(list);
            }
        }

        public void StopPreDownLoad()
        {
            HotFixPreDownload.Instance.StopDownload();
        }

        public bool IsPreDownloading()
        {
            return HotFixPreDownload.Instance.IsDownloading();
        }

        public bool IsPreDownloadPause()
        {
            return HotFixPreDownload.Instance.IsPause();
        }

        public void PausePreDownload()
        {
            HotFixPreDownload.Instance.PauseDownload();
        }

        public void ResumePreDownload()
        {
            HotFixPreDownload.Instance.ResumeDownload();
        }

        public List<string> GetPredDownloadingVersions()
        {
            return HotFixPreDownload.Instance.GetDownloadingVersions();
        }

        private void Update()
        {
            if (_curStepObject != null)
            {
                _curStepObject.Update();
            }
        }

        internal void Finish()
        {
            _curStepType = HotfixStep.None;
            _curStepObject = null;
            _finishTrigger.Trigger();
            _registedUpdate = false;
            Zeus.Core.ZeusCore.Instance.UnRegisterUpdate(Update);
            MarkHasLaunchedGame();
            OnFinish();
        }

        public void Error(HotfixException exception)
        {
            ZeusCore.Instance.StartCoroutine(TipError(exception));
        }

        private IEnumerator TipError(HotfixException exception)
        {
            HotfixLogger.LogError(exception);

            switch (exception.errorType)
            {
                case HotfixError.NetError:
                case HotfixError.InitError:
                case HotfixError.ServerError:
                case HotfixError.HardDiskFull:
                case HotfixError.CheckFail:
                case HotfixError.UnzipError:
                case HotfixError.Exception:
                    {
                        WaitForTrigger trigger = new WaitForTrigger();
                        OnErrorConfirm(exception.errorType, trigger, exception.param);
                        yield return trigger;
                        ReStartHotFix();
                    }
                    break;
                case HotfixError.DownloadError:
                case HotfixError.HttpStatusCode404Error:
                    {
                        if (ReachOpenTimeHFPatchDataList != null && ReachOpenTimeHFPatchDataList.Exists(t => t.Type == HotFixType.Force))
                        {
                            WaitForTrigger trigger = new WaitForTrigger();
                            OnErrorConfirm(exception.errorType, trigger, exception.param);
                            yield return trigger;
                            ReStartHotFix();
                        }
                        else
                        {
                            WaitForYesOrNo yesno = new WaitForYesOrNo();
                            OnErrorChoice(exception.errorType, yesno, exception.param);
                            yield return yesno;
                            if (yesno.IsYes)
                            {
                                ReStartHotFix();
                            }
                            else
                            {
                                Finish();
                            }
                        }
                    }
                    break;
                default:
                    {
                        throw new Exception("unhandle error type:" + exception.errorType);
                    }
                    break;
            }
        }

        public virtual void OnProcess(HotfixStep step, long current, long target)
        {

        }

        public virtual void OnChoice(HotfixTips tips, WaitForYesOrNo yesOrNo, object param)
        {
            yesOrNo.YesOrNo(true);
        }

        public virtual void OnConfirm(HotfixTips tips, WaitForTrigger trigger, object param)
        {
            trigger.Trigger();
        }

        public virtual void OnErrorConfirm(HotfixError error, WaitForTrigger trigger, object param)
        {
            trigger.Trigger();
        }

        public virtual void OnErrorChoice(HotfixError error, WaitForYesOrNo yesOrNo, object param)
        {
            yesOrNo.YesOrNo(true);
        }

        public virtual void OnFinish()
        {

        }
        /// <summary>
        /// 查询热更信息，
        /// （1）正常的更新：根据返回的信息正确处理就可以了
        /// （2）预下载更新：如果热更的配置文件内开启了自动预下载，则会自动进行预下载并返回无更新；如果没有开启自动预下载，则返回更新内容，
        ///                  调用方检测到返回的信息为预下载时，可以查询是否在进行预下载，来选择是否重新走热更流程，因为设置中未开启自动预下载时，
        ///                  热更会做UI层提示给玩家，最终会由玩家决定本次是否启动预下载。注：如果当前预下载内容已经开始下载了，则返回无更新。
        /// </summary>
        /// <param name="success">成功回调,参数1：热更类型  参数2：热更包大小  参数3：是否为预下载</param>
        /// <param name="fail">失败回调</param>
        /// <param name="serverUrls">自定义的查询地址，会覆盖包内配置文件的serverUrls</param>
        /// <param name="customTargetVersion">自定义查询目标版本，如果有值且与本地版本一致则无更新</param>
        /// <param name="controlDataUrls">自定义的查询地址，会覆盖包内配置文件的controlDataUrls</param>
        public static void QueryHotfixInfo(Action<HotFixType, double, bool> success, Action<HotfixError> fail, string serverUrls = null, string customTargetVersion = null, string controlDataUrls = null)
        {
            if (success == null)
            {
                return;
            }
            string[] _serverUrls = string.IsNullOrEmpty(serverUrls) ? null : serverUrls.Split(';');
            HotfixLocalConfig LocalConfig = HotfixLocalConfigHelper.LoadLocalConfig();
            if (_serverUrls == null || _serverUrls.Length == 0)
            {
                _serverUrls = LocalConfig.serverUrls.ToArray();
            }
            string[] _controlDataUrls = _serverUrls;
            if (LocalConfig.independentControlDataUrl)
            {
                _controlDataUrls = string.IsNullOrEmpty(controlDataUrls) ? LocalConfig.controlDataUrls.ToArray() : controlDataUrls.Split(';');
            }
            ResVersionData ResVersion = ResVersionData.Load();
            if (null == ResVersion)
            {
                fail?.Invoke(HotfixError.NoResVersionData);
                return;
            }
            if (!string.IsNullOrEmpty(customTargetVersion) && customTargetVersion.Equals(ResVersion.ResVersion))
            {
                success(HotFixType.None, 0, false);
                return;
            }

            string urlRefreshParam = GetUrlRefreshParam(LocalConfig, customTargetVersion);

            HotfixInfoRequest request = new HotfixInfoRequest(_controlDataUrls, LocalConfig, ResVersion.ResVersion, (controllData, serverDate, error, errorInfo) =>
            {
                if (controllData == null)
                {
                    //如果设置了忽略404错误，这里按无更新处理
                    if (LocalConfig.ignoreInit404Error && error == HotfixError.HttpStatusCode404Error)
                    {
                        success(HotFixType.None, 0, false);
                        return;
                    }
                    //HotfixError.NetError
                    //HotfixError.InitError
                    //HotfixError.HttpStatusCode404Error
                    //HotfixError.Exception
                    HotfixLogger.LogError(new HotfixException(errorInfo, error));
                    if (fail != null)
                    {
                        fail(error);
                    }
                }
                else if (controllData.PatchConfigData == null)
                {
                    success(HotFixType.None, 0, false);
                }
                else
                {
                    if ((!string.IsNullOrEmpty(controllData.PatchConfigData.OpenTime) &&
                        serverDate < TimeUtil.ParseTimeStr(controllData.PatchConfigData.OpenTime)))
                    {
                        if (controllData.PatchConfigData.Type != HotFixType.AppStore)
                        {
                            //如果当前预下载内容已经开始下载了，则返回无更新
                            if (HotFixPreDownload.Instance.GetDownloadingVersions().Contains(controllData.PatchConfigData.TargetVersion))
                            {
                                success(HotFixType.None, 0, false);
                            }
                            else
                            {
                                if (LocalConfig.autoPreDownload)
                                {
                                    //还未到开启时间，自动开启预下载，直接返回无更新内容
                                    if (HotFixPreDownload.Instance.IsDownloading())
                                    {
                                        if (!HotFixPreDownload.Instance.IsPause())
                                        {
                                            HotFixPreDownload.Instance.PauseDownload();
                                        }
                                        AddPerDownloadData(new List<HFPatchConfigData>() { controllData.PatchConfigData });
                                        HotFixPreDownload.Instance.ResumeDownload();
                                    }
                                    else
                                    {
                                        StartPreDownload(new List<HFPatchConfigData>() { controllData.PatchConfigData }, LocalConfig, _serverUrls);
                                    }
                                    success(HotFixType.None, 0, false);
                                }
                                else
                                {
                                    success(controllData.PatchConfigData.Type, controllData.PatchConfigData.GetPatchSize(), true);
                                }
                            }
                        }
                        else
                        {
                            success(HotFixType.None, 0, false);
                        }
                    }
                    else
                    {
                        success(controllData.PatchConfigData.Type, controllData.PatchConfigData.GetPatchSize(), false);
                    }
                }
            }, urlRefreshParam);
            request.Start();
        }

        /// <summary>
        /// 测试版就只用时间刷新，使用时间就是Time，使用版本号就是version，如果测试版指定了目标版本，也要用时间刷新。如果正式版没指定版本号，用时间刷新参数，指定了参数用参数刷新
        /// </summary>
        private static string GetUrlRefreshParam(HotfixLocalConfig localConfig, string customTargetVersion = null)
        {
            if (localConfig == null)
            {
                localConfig = HotfixLocalConfigHelper.LoadLocalConfig();
            }
            bool useTimeRefreshParam = true;
            switch (localConfig.testMode)
            {
                case HotfixLocalConfig.HotfixTestMode.Default:
#if UNITY_ANDROID
                    string testMarkFile = Zeus.Core.FileSystem.OuterPackage.GetRealPath("kramtset.kramtset");//testmark倒序,内容abcd123456
                    if (!System.IO.File.Exists(testMarkFile) || !MD5Util.GetMD5FromFile(testMarkFile).Equals("da3177cbd9f064004b6a0d59a3a484bb"))
                    {
                        if (!string.IsNullOrEmpty(customTargetVersion))
                        {
                            useTimeRefreshParam = false;
                        }
                    }
#endif
                    break;
                case HotfixLocalConfig.HotfixTestMode.OnlyNormal:
                    if (!string.IsNullOrEmpty(customTargetVersion))
                    {
                        useTimeRefreshParam = false;
                    }
                    break;
            }
            if (useTimeRefreshParam)
            {
                //每隔一定时长会变化一次，urlRefreshParam 可由 Editor 内热更配置面板设置
                return string.Format("?Time={0}{1}{2}{3}{4}", DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                    DateTime.UtcNow.Hour, DateTime.UtcNow.Minute / localConfig.urlRefreshParam);
            }
            else
            {
                return string.Format("?version={0}{1}", localConfig.testMode.ToString(), customTargetVersion);
            }
        }


        /// <summary>
        /// 设置指定url，会在热更时替换掉包内配置文件的url，如果有多个参数，使用 ";" 分割
        /// </summary>
        public void SetCustomUrls(string urls)
        {
            if (!string.IsNullOrEmpty(urls))
            {
                _customUrls = urls.Split(';');
                HotfixLocalConfig config = LocalConfig != null ? LocalConfig : HotfixLocalConfigHelper.LoadLocalConfig();
                if (config != null && config.serverUrls != null && config.serverUrls.Count > 0)
                {
                    Uri uri = new Uri(config.serverUrls[0]);
                    string absolutePath = uri.AbsolutePath.Substring(1);
                    for (int i = 0; i < _customUrls.Length; i++)
                    {
                        _customUrls[i] = UriUtil.CombineUri(_customUrls[i], absolutePath);
                    }
                }
                else
                {
                    Debug.LogError("[HotfixService] CustomUrls Fail,can't load local config.");
                }
            }
            else
            {
                Debug.LogError("[HotfixService] CustomUrls Fail,urls is empty or null.");
            }
        }

        public void SetCustomIndependentControlDataUrls(string urls)
        {
            if (!string.IsNullOrEmpty(urls))
            {
                _customIndependentControlDataUrls = urls.Split(';');
                HotfixLocalConfig config = LocalConfig != null ? LocalConfig : HotfixLocalConfigHelper.LoadLocalConfig();
                if (config != null && config.independentControlDataUrl && config.controlDataUrls != null && config.controlDataUrls.Count > 0)
                {
                    Uri uri = new Uri(config.controlDataUrls[0]);
                    string absolutePath = uri.AbsolutePath.Substring(1);
                    for (int i = 0; i < _customIndependentControlDataUrls.Length; i++)
                    {
                        _customIndependentControlDataUrls[i] = UriUtil.CombineUri(_customIndependentControlDataUrls[i], absolutePath);
                    }
                }
                else
                {
                    Debug.LogError("[HotfixService] SetCustomIndependentControlDataUrls Fail,can't load local config.");
                }
            }
            else
            {
                Debug.LogError("[HotfixService] SetCustomIndependentControlDataUrls Fail,urls is empty or null.");
            }
        }

        /// <summary>
        /// 设置自定义的Channel，不会写入文件，程序结束后即失效，如有需要下次启动需重新设置
        /// </summary>
        /// <param name="channel"></param>
        public void SetCustomChannel(string channel)
        {
            HotfixLocalConfig.OverrideChannelId(channel);
        }

        /// <summary>
        /// 设置指定的目标版本号，如果有值且与本地值一致，将跳过热更
        /// </summary>
        public void SetCustomTargetVersion(string target)
        {
            _customTargetVersion = target;
        }


        #region callback
        private Action<string> _onCheckAndFinishLastHotfixStart;
        private Action<string, int> _onCheckAndFinishLastHotfixSuc;
        private Action<string, string, int> _onCheckAndFinishLastHotfixFail;
        private Action<HotFixType, string> _onInit;
        private Action<string[]> _onDownloadStart;
        private Action<string[], int> _onDownloadSuc;
        private Action<string[], double, string, int> _onDownloadFail;
        private Action<string> _onUnzipStart;
        private Action<string, int> _onUnzipSuc;
        private Action<string, string, string, int> _onUnzipFail;

        /// <summary>
        /// 检查到上次未完成的解压，参数：待解压完成的资源的热更版本号
        /// </summary>
        /// <param name="onCheckAndFinishLastHotfixStart"></param>
        public void RegistOnCheckAndFinishLastHotfixStart(Action<string> onCheckAndFinishLastHotfixStart)
        {
            _onCheckAndFinishLastHotfixStart += onCheckAndFinishLastHotfixStart;
        }
        public void UnregistOnCheckAndFinishLastHotfixStart(Action<string> onCheckAndFinishLastHotfixStart)
        {
            _onCheckAndFinishLastHotfixStart -= onCheckAndFinishLastHotfixStart;
        }
        public void InvokeOnCheckAndFinishLastHotfixStart(string version)
        {
            try
            {
                if (_onCheckAndFinishLastHotfixStart != null)
                    _onCheckAndFinishLastHotfixStart(version);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }

        /// <summary>
        /// 继续上次未完成的解压，解压成功: 版本号 耗时
        /// </summary>
        /// <param name="onCheckAndFinishLastHotfixSuc"></param>
        public void RegistOnCheckAndFinishLastHotfixSuc(Action<string, int> onCheckAndFinishLastHotfixSuc)
        {
            _onCheckAndFinishLastHotfixSuc += onCheckAndFinishLastHotfixSuc;
        }
        public void UnregistOnCheckAndFinishLastHotfixSuc(Action<string, int> onCheckAndFinishLastHotfixSuc)
        {
            _onCheckAndFinishLastHotfixSuc -= onCheckAndFinishLastHotfixSuc;
        }
        public void InvokeOnCheckAndFinishLastHotfixSuc(string version, int time)
        {
            try
            {
                if (_onCheckAndFinishLastHotfixSuc != null)
                    _onCheckAndFinishLastHotfixSuc(version, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }

        /// <summary>
        /// 继续上次未完成的解压，解压失败: 版本号 失败原因 耗时
        /// </summary>
        /// <param name="onCheckAndFinishLastHotfixFail"></param>
        public void RegistOnCheckAndFinishLastHotfixFail(Action<string, string, int> onCheckAndFinishLastHotfixFail)
        {
            _onCheckAndFinishLastHotfixFail += onCheckAndFinishLastHotfixFail;
        }
        public void UnregistOnCheckAndFinishLastHotfixFail(Action<string, string, int> onCheckAndFinishLastHotfixFail)
        {
            _onCheckAndFinishLastHotfixFail -= onCheckAndFinishLastHotfixFail;
        }
        public void InvokeOnCheckAndFinishLastHotfixFail(string version, string error, int time)
        {
            try
            {
                if (_onCheckAndFinishLastHotfixFail != null)
                    _onCheckAndFinishLastHotfixFail(version, error, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }

        /// <summary>
        /// 获取到热更信息：热更类型  热更版本号 
        /// </summary>
        /// <param name="onInit"></param>
        public void RegistOnInit(Action<HotFixType, string> onInit)
        {
            _onInit += onInit;
        }
        public void UnregistOnInit(Action<HotFixType, string> onInit)
        {
            _onInit -= onInit;
        }
        public void InvokeOnInit(HotFixType type, string version)
        {
            try
            {
                if (_onInit != null)
                    _onInit(type, version);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 开始下载：资源包url
        /// </summary>
        /// <param name="onDownloadStart"></param>
        public void RegistOnDownloadStart(Action<string[]> onDownloadStart)
        {
            _onDownloadStart += onDownloadStart;
        }
        public void UnregistOnDownloadStart(Action<string[]> onDownloadStart)
        {
            _onDownloadStart -= onDownloadStart;
        }
        public void InvokeOnDownloadStart(string[] url)
        {
            try
            {
                if (_onDownloadStart != null)
                    _onDownloadStart(url);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 下载成功：资源包url 耗时(ms)
        /// </summary>
        /// <param name="onDownloadSuc"></param>
        public void RegistOnDownloadSuc(Action<string[], int> onDownloadSuc)
        {
            _onDownloadSuc += onDownloadSuc;
        }
        public void UnregistOnDownloadSuc(Action<string[], int> onDownloadSuc)
        {
            _onDownloadSuc -= onDownloadSuc;
        }
        public void InvokeOnDownloadSuc(string[] url, int time)
        {
            try
            {
                if (_onDownloadSuc != null)
                    _onDownloadSuc(url, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 下载失败：资源包url 资源包大小 失败原因 耗时(ms)
        /// </summary>
        /// <param name="onDownloadFail"></param>
        public void RegistOnDownloadFail(Action<string[], double, string, int> onDownloadFail)
        {
            _onDownloadFail += onDownloadFail;
        }
        public void UnregistOnDownloadFail(Action<string[], double, string, int> onDownloadFail)
        {
            _onDownloadFail -= onDownloadFail;
        }
        public void InvokeOnDownloadFail(string[] url, double size, string error, int time)
        {
            try
            {
                if (_onDownloadFail != null)
                    _onDownloadFail(url, size, error, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 开始解压：资源版本号
        /// </summary>
        /// <param name="onUnzipStart"></param>
        public void RegistOnUnzipStart(Action<string> onUnzipStart)
        {
            _onUnzipStart += onUnzipStart;
        }
        public void UnregistOnUnzipStart(Action<string> onUnzipStart)
        {
            _onUnzipStart -= onUnzipStart;
        }
        public void InvokeOnUnzipStart(string resVersion)
        {
            try
            {
                if (_onUnzipStart != null)
                    _onUnzipStart(resVersion);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 解压成功：资源版本号 耗时(ms)
        /// </summary>
        /// <param name="onUnzipSuc"></param>
        public void RegistOnUnzipSuc(Action<string, int> onUnzipSuc)
        {
            _onUnzipSuc += onUnzipSuc;
        }
        public void UnregistOnUnzipSuc(Action<string, int> onUnzipSuc)
        {
            _onUnzipSuc -= onUnzipSuc;
        }
        public void InvokeOnUnzipSuc(string resVersion, int time)
        {
            try
            {
                if (_onUnzipSuc != null)
                    _onUnzipSuc(resVersion, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }


        /// <summary>
        /// 解压失败：资源版本号 资源名 错误原因 耗时(ms)
        /// </summary>
        /// <param name="onUnzipFail"></param>
        public void RegistOnUnzipFail(Action<string, string, string, int> onUnzipFail)
        {
            _onUnzipFail += onUnzipFail;
        }
        public void UnregistOnUnzipFail(Action<string, string, string, int> onUnzipFail)
        {
            _onUnzipFail -= onUnzipFail;
        }
        public void InvokeOnUnzipFail(string resVersion, string name, string error, int time)
        {
            try
            {
                if (_onUnzipFail != null)
                    _onUnzipFail(resVersion, name, error, time);
            }
            catch (Exception e)
            {
                HotfixLogger.LogError(e);
            }
        }
        #endregion


        #region 首包Patch相关
        public static bool hasLaunchedGame;//是否登陆过游戏
        private const string ZEUS_HOTFIX_HAS_LAUNCHED_GAME_KEY = "ZEUS_HOTFIX_HAS_LAUNCHED_GAME_KEY";

        private static void InitHasLaunchedGame()
        {
            hasLaunchedGame = PlayerPrefs.GetInt(ZEUS_HOTFIX_HAS_LAUNCHED_GAME_KEY, 0) == 1;
        }

        public static void MarkHasLaunchedGame()
        {
            hasLaunchedGame = true;
            PlayerPrefs.SetInt(ZEUS_HOTFIX_HAS_LAUNCHED_GAME_KEY, 1);
        }

        public static void ResetHasLaunchedGame()
        {
            hasLaunchedGame = false;
            PlayerPrefs.SetInt(ZEUS_HOTFIX_HAS_LAUNCHED_GAME_KEY, 0);
        }

        #endregion
    }
}
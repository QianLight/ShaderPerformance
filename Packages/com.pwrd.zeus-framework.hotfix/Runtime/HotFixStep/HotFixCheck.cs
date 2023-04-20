/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using Zeus.Core;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zeus.Framework.Http.UnityHttpDownloader;


namespace Zeus.Framework.Hotfix
{
    public class HotFixCheck : BaseHotFixStep
    {
        public HotFixCheck(HotfixService executer) : base(executer, HotfixStep.Check)
        { }

        public override void Run()
        {
            OnProcess(0, 1);
            bool testDevice = false;
            _hotFixExecuter.HFPatchDataList.Clear();
            _hotFixExecuter.AppStoreHFPatchData = null;
            _hotFixExecuter.AppStoreRecommendHFPatchData = null;
            switch (_hotFixExecuter.LocalConfig.testMode)
            {
                case HotfixLocalConfig.HotfixTestMode.Default:
                    {
#if UNITY_IOS
                        if (_hotFixExecuter.HFControlDataList.Count > 0)
                        {
                            HFControlData data = _hotFixExecuter.HFControlDataList[_hotFixExecuter.HFControlDataList.Count - 1];
                            HotfixLogger.Log("Device.advertisingIdentifier" + UnityEngine.iOS.Device.advertisingIdentifier);
                            //ios设备根据广告码来确定测试设备
                            testDevice = data.TestingData != null && data.TestingData.IDFAList != null && data.TestingData.IDFAList.Contains(UnityEngine.iOS.Device.advertisingIdentifier);
                        }
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
            for (int i = 0; i < _hotFixExecuter.HFControlDataList.Count; i++)
            {
                HFControlData controlData = _hotFixExecuter.HFControlDataList[i];

                //如果当前设备是测试设备，取用最后一个版本中的测试数据
                if (testDevice && i == _hotFixExecuter.HFControlDataList.Count - 1)
                {
                    if (controlData.TestingData != null && controlData.TestingData.PatchConfigData != null)
                    {
                        if (controlData.TestingData.PatchConfigData.Type == HotFixType.AppStore)
                        {
                            if (IsPatchDataReachOpenTime(controlData.TestingData.PatchConfigData))
                            {
                                string appStoreUrl = controlData.TestingData.PatchConfigData.GetAppStoreUrl();
                                if (!string.IsNullOrEmpty(appStoreUrl) || !_hotFixExecuter.LocalConfig.ignoreEmptyAppStoreUrl)
                                {
                                    _hotFixExecuter.AppStoreHFPatchData = controlData.TestingData.PatchConfigData;
                                    continue;
                                }
                            }
                        }
                        else if (controlData.TestingData.PatchConfigData.Type == HotFixType.Force || controlData.TestingData.PatchConfigData.Type == HotFixType.Recommend)
                        {
                            if (controlData.TestingData.PatchConfigData.SourceVersion.Equals(_hotFixExecuter.CurVersion))
                            {
                                _hotFixExecuter.HFPatchDataList.Add(controlData.TestingData.PatchConfigData);
                                continue;
                            }
                        }
                        else if (controlData.TestingData.PatchConfigData.Type == HotFixType.AppStoreRecommend)
                        {
                            if (IsPatchDataReachOpenTime(controlData.TestingData.PatchConfigData))
                            {
                                string appStoreUrl = controlData.TestingData.PatchConfigData.GetAppStoreUrl();
                                if (!string.IsNullOrEmpty(appStoreUrl) || !_hotFixExecuter.LocalConfig.ignoreEmptyAppStoreUrl)
                                {
                                    //如果商店推荐更新前边有商店更新，则此商店推荐更新按商店更新处理
                                    if (_hotFixExecuter.AppStoreHFPatchData != null)
                                    {
                                        controlData.TestingData.PatchConfigData.Type = HotFixType.AppStore;
                                        _hotFixExecuter.AppStoreHFPatchData = controlData.TestingData.PatchConfigData;
                                    }
                                    else
                                    {
                                        _hotFixExecuter.AppStoreRecommendHFPatchData = controlData.TestingData.PatchConfigData;
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (controlData.PatchConfigData != null)
                {
                    if (controlData.PatchConfigData.Type == HotFixType.AppStore)
                    {
                        if (IsPatchDataReachOpenTime(controlData.PatchConfigData))
                        {
                            string appStoreUrl = controlData.PatchConfigData.GetAppStoreUrl();
                            if (!string.IsNullOrEmpty(appStoreUrl) || !_hotFixExecuter.LocalConfig.ignoreEmptyAppStoreUrl)
                            {
                                _hotFixExecuter.AppStoreHFPatchData = controlData.PatchConfigData;
                            }
                        }
                    }
                    else if (controlData.PatchConfigData.Type == HotFixType.Force || controlData.PatchConfigData.Type == HotFixType.Recommend)
                    {
                        _hotFixExecuter.HFPatchDataList.Add(controlData.PatchConfigData);
                    }
                }
                if (controlData.AppRecommendPatchConfigData != null && controlData.AppRecommendPatchConfigData.Type == HotFixType.AppStoreRecommend)
                {
                    if (IsPatchDataReachOpenTime(controlData.AppRecommendPatchConfigData))
                    {
                        string appStoreUrl = controlData.AppRecommendPatchConfigData.GetAppStoreUrl();
                        if (!string.IsNullOrEmpty(appStoreUrl) || !_hotFixExecuter.LocalConfig.ignoreEmptyAppStoreUrl)
                        {
                            //如果商店推荐更新前边有商店更新，则此商店推荐更新按商店更新处理
                            if (_hotFixExecuter.AppStoreHFPatchData != null)
                            {
                                controlData.TestingData.PatchConfigData.Type = HotFixType.AppStore;
                                _hotFixExecuter.AppStoreHFPatchData = controlData.AppRecommendPatchConfigData;
                            }
                            else
                            {
                                _hotFixExecuter.AppStoreRecommendHFPatchData = controlData.AppRecommendPatchConfigData;
                            }
                        }
                    }
                }
            }

            if (_hotFixExecuter.AppStoreHFPatchData != null)
            {
                _hotFixExecuter.InvokeOnInit(_hotFixExecuter.AppStoreHFPatchData.Type, _hotFixExecuter.AppStoreHFPatchData.TargetVersion);
                ZeusCore.Instance.StartCoroutine(HandleAppStoreHotfix(_hotFixExecuter.AppStoreHFPatchData));
                return;
            }

            if (_hotFixExecuter.AppStoreRecommendHFPatchData != null)
            {
                _hotFixExecuter.InvokeOnInit(_hotFixExecuter.AppStoreRecommendHFPatchData.Type, _hotFixExecuter.AppStoreRecommendHFPatchData.TargetVersion);
                ZeusCore.Instance.StartCoroutine(HandleAppStoreRecommandHotfix(_hotFixExecuter.AppStoreRecommendHFPatchData));
                return;
            }

            ZeusCore.Instance.StartCoroutine(HandleResHotfix());

        }

        private bool IsPatchDataReachOpenTime(HFPatchConfigData data)
        {
            if (data != null && !string.IsNullOrEmpty(data.OpenTime) && _hotFixExecuter.ServerDate < TimeUtil.ParseTimeStr(data.OpenTime))
            {
                return false;
            }
            return true;
        }

        private IEnumerator HandleAppStoreHotfix(HFPatchConfigData data)
        {
            string appStoreUrl = data.GetAppStoreUrl();
            WaitForTrigger trigger = new WaitForTrigger();
            while (true)
            {
                OnConfirm(HotfixTips.AppStoreDownload, trigger);
                yield return trigger;
                trigger = new WaitForTrigger();
                Application.OpenURL(appStoreUrl);
            }
        }

        private IEnumerator HandleAppStoreRecommandHotfix(HFPatchConfigData data)
        {
            string appStoreUrl = data.GetAppStoreUrl();
            WaitForYesOrNo yesOrNo = new WaitForYesOrNo();
            OnChoice(HotfixTips.AppStoreRecommandDownload, yesOrNo);
            yield return yesOrNo;
            while (yesOrNo.IsYes)
            {
                Application.OpenURL(appStoreUrl);
                yesOrNo = new WaitForYesOrNo();
                OnChoice(HotfixTips.AppStoreRecommandDownload, yesOrNo);
                yield return yesOrNo;
            }
            _hotFixExecuter.AppStoreRecommendHFPatchData = null;
            yield return HandleResHotfix();
        }

        private IEnumerator HandleResHotfix()
        {
            if (_hotFixExecuter.HFPatchDataList.Count == 0)
            {
                Finish();
            }
            else
            {
                //如果一个预下载更新后边有已达生效时间的更新，其按照生效的更新处理
                _hotFixExecuter.ReachOpenTimeHFPatchDataList.Clear();
                _hotFixExecuter.UnReachOpenTimeHFPatchDataList.Clear();
                for (int i = 0; i < _hotFixExecuter.HFPatchDataList.Count; i++)
                {
                    if (_hotFixExecuter.HFControlDataList[i] == null) continue;
                    _hotFixExecuter.InvokeOnInit(_hotFixExecuter.HFPatchDataList[i].Type, _hotFixExecuter.HFPatchDataList[i].TargetVersion);
                    if (IsPatchDataReachOpenTime(_hotFixExecuter.HFPatchDataList[i]))
                    {
                        if (_hotFixExecuter.UnReachOpenTimeHFPatchDataList.Count > 0)
                        {
                            _hotFixExecuter.ReachOpenTimeHFPatchDataList.AddRange(_hotFixExecuter.UnReachOpenTimeHFPatchDataList);
                            _hotFixExecuter.UnReachOpenTimeHFPatchDataList.Clear();
                        }
                        _hotFixExecuter.ReachOpenTimeHFPatchDataList.Add(_hotFixExecuter.HFPatchDataList[i]);
                    }
                    else
                    {
                        _hotFixExecuter.UnReachOpenTimeHFPatchDataList.Add(_hotFixExecuter.HFPatchDataList[i]);
                    }
                }

                bool hasForceHotfix = false;
                bool hasRecommendHotfix = false;
                long needDownloadSizeAll = 0;
                for (int i = 0; i < _hotFixExecuter.ReachOpenTimeHFPatchDataList.Count; i++)
                {
                    if (_hotFixExecuter.ReachOpenTimeHFPatchDataList[i].Type == HotFixType.Force)
                    {
                        hasForceHotfix = true;
                    }
                    else if (_hotFixExecuter.ReachOpenTimeHFPatchDataList[i].Type == HotFixType.Recommend)
                    {
                        hasRecommendHotfix = true;
                    }
                    string patchPath = _hotFixExecuter.ReachOpenTimeHFPatchDataList[i].GetPatchSavePath();
                    long patchSize = _hotFixExecuter.ReachOpenTimeHFPatchDataList[i].GetPatchSize();
                    if (!File.Exists(patchPath) || new FileInfo(patchPath).Length != patchSize)
                    {
                        needDownloadSizeAll += HttpDownloader.CalcRealDownloadSize(patchPath, patchSize);
                    }
                }
                //只要存在强制更新，推荐更新一律按强制更新处理
                if (hasForceHotfix)
                {
                    if (needDownloadSizeAll <= 0)
                    {
                        OnProcess(1, 1);
                        NextStep();
                    }
                    else
                    {
                        WaitForTrigger trigger = new WaitForTrigger();
                        OnConfirm(HotfixTips.ForceDownload, trigger, needDownloadSizeAll);
                        yield return trigger;
                        while (needDownloadSizeAll >= DiskUtils.CheckAvailableSpaceBytes())
                        {
                            trigger = new WaitForTrigger();
                            OnConfirm(HotfixTips.HardDiskFull, trigger, needDownloadSizeAll);
                            yield return trigger;
                        }
                        OnProcess(1, 1);
                        NextStep();
                    }
                }
                else
                {
                    if (hasRecommendHotfix)
                    {
                        if (needDownloadSizeAll <= 0)
                        {
                            OnProcess(1, 1);
                            NextStep();
                        }
                        else
                        {
                            WaitForYesOrNo yesOrNo = new WaitForYesOrNo();
                            OnChoice(HotfixTips.RecommendDownload, yesOrNo, needDownloadSizeAll);
                            yield return yesOrNo;
                            if (yesOrNo.IsYes)
                            {
                                while (yesOrNo.IsYes && needDownloadSizeAll >= DiskUtils.CheckAvailableSpaceBytes())
                                {
                                    yesOrNo = new WaitForYesOrNo();
                                    OnChoice(HotfixTips.HardDiskFull, yesOrNo, needDownloadSizeAll);
                                    yield return yesOrNo;
                                }
                                if (yesOrNo.IsYes)
                                {
                                    OnProcess(1, 1);
                                    NextStep();
                                }
                                else
                                {
                                    //空间不足，跳过更新
                                    Finish();
                                }
                            }
                            else
                            {
                                _hotFixExecuter.PreDownloadHFPatchDataList.Clear();
                                for (int i = 0; i < _hotFixExecuter.ReachOpenTimeHFPatchDataList.Count; i++)
                                {
                                    _hotFixExecuter.PreDownloadHFPatchDataList.Add(_hotFixExecuter.ReachOpenTimeHFPatchDataList[i]);
                                }
                                //预下载全部到达生效时间资源
                                _hotFixExecuter.StartPreDownload();
                                Finish();
                            }
                        }
                    }
                    else
                    {
                        //预下载第一份未达到开启时间的热更资源
                        if (_hotFixExecuter.UnReachOpenTimeHFPatchDataList.Count > 0)
                        {
                            _hotFixExecuter.PreDownloadHFPatchDataList.Clear();
                            _hotFixExecuter.PreDownloadHFPatchDataList.Add(_hotFixExecuter.UnReachOpenTimeHFPatchDataList[0]);
                            if (_hotFixExecuter.LocalConfig.autoPreDownload)
                            {
                                _hotFixExecuter.StartPreDownload();
                                Finish();
                            }
                            else
                            {
                                yield return TipPreDownload();
                            }
                        }
                        else
                        {
                            Finish();
                        }
                    }
                }
            }
        }

        private IEnumerator TipPreDownload()
        {
            long needDownloadSize = 0;

            for (int i = 0; i < _hotFixExecuter.PreDownloadHFPatchDataList.Count; i++)
            {
                string patchPath = _hotFixExecuter.PreDownloadHFPatchDataList[i].GetPatchSavePath();
                long patchSize = _hotFixExecuter.PreDownloadHFPatchDataList[i].GetPatchSize();
                //因为文件名包含MD5，并且在下载完后会校验MD5，所以此处不需要校验MD5，只需要检测文件是否存在以及文件大小即可
                if (!File.Exists(patchPath) || new FileInfo(patchPath).Length != patchSize)
                {
                    needDownloadSize = HttpDownloader.CalcRealDownloadSize(patchPath, patchSize);
                }
            }
            if (needDownloadSize == 0)
            {
                //结束更新
                Finish();
            }
            else
            {
                WaitForYesOrNo yesOrNo = new WaitForYesOrNo();
                OnChoice(HotfixTips.PreDownload, yesOrNo, needDownloadSize);
                yield return yesOrNo;
                if (yesOrNo.IsYes)
                {
                    while (yesOrNo.IsYes && needDownloadSize >= DiskUtils.CheckAvailableSpaceBytes())
                    {
                        yesOrNo = new WaitForYesOrNo();
                        OnChoice(HotfixTips.HardDiskFull, yesOrNo, needDownloadSize);
                        yield return yesOrNo;
                    }
                }
                if (yesOrNo.IsYes)
                {
                    _hotFixExecuter.StartPreDownload();
                }
                //结束更新
                Finish();
            }
        }
    }
}
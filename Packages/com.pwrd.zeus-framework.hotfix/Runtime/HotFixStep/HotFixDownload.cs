/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Zeus.Framework.Http.UnityHttpDownloader;
using Zeus.Core;
using UnityEngine;

namespace Zeus.Framework.Hotfix
{
    public class HotFixDownload : BaseHotFixStep
    {
        bool _suc = false;
        bool _lastDownload = false;//是否上次已经下载好文件了
        HttpDownloader downloader;
        List<HFPatchConfigData> _downloadList;
        int index = 0;
        long targetSize = 0;
        long finishSize = 0;
        long downloadTime = 0;
        public HotFixDownload(HotfixService executer) : base(executer, HotfixStep.Download, 0)//下载内部会重试，外部不需要重试
        {
            ZeusCore.Instance.RegisterOnApplicationQuit(OnQuit);
#if UNITY_ANDROID && !UNITY_EDITOR
            HttpDownloader.GenAndroidJavaClass();
#endif
        }

        public override void Run()
        {
            _suc = false;
            _lastDownload = false;
            downloader = null;
            targetSize = 0;
            finishSize = 0;
            downloadTime = 0;
            index = 0;
            _downloadList = new List<HFPatchConfigData>();
            if (_hotFixExecuter.ReachOpenTimeHFPatchDataList.Count > 0)
            {
                for (int i = 0; i < _hotFixExecuter.ReachOpenTimeHFPatchDataList.Count; i++)
                {
                    string savePath = _hotFixExecuter.ReachOpenTimeHFPatchDataList[i].GetPatchSavePath();
                    long patchSize = _hotFixExecuter.ReachOpenTimeHFPatchDataList[i].GetPatchSize();
                    if (!File.Exists(savePath) || new FileInfo(savePath).Length != patchSize)
                    {
                        _downloadList.Add(_hotFixExecuter.ReachOpenTimeHFPatchDataList[i]);
                        targetSize += HttpDownloader.CalcRealDownloadSize(savePath, patchSize);
                        index = 0;
                    }
                }
            }
            if (_downloadList.Count == 0)
            {
                _suc = true;
                _lastDownload = true;
                OnDownloadComplete();
            }
            else
            {
                StartDownload();
            }
        }

        public void UpdateNetworkStatusObseverUrls(string[] urls)
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string observerAddress = "";
                foreach (string url in urls)
                {
                    Uri uri = new Uri(url);
                    if (string.IsNullOrEmpty(observerAddress))
                    {
                        observerAddress = uri.Host;
                    }
                    else
                    {
                        observerAddress += ";" + uri.Host;
                    }
                }
                HttpDownloader.SetNetworkStatusObseverUrls(observerAddress);
            }
        }

        private void StartDownload()
        {
            string savePath = _downloadList[index].GetPatchSavePath();
            long patchSize = _downloadList[index].GetPatchSize();
            string[] urls = _downloadList[index].GetUrls(_hotFixExecuter.PatchDownloadUrls);
            if (downloader != null)
            {
                downloader.Dispose();
                downloader = null;
            }
            UpdateNetworkStatusObseverUrls(urls);
            _hotFixExecuter.InvokeOnDownloadStart(urls);
            Zeus.Core.FileUtil.DeleteFile(savePath);
            //删除其他版本未完成下载而残留的临时文件
            string path = PathUtil.FormatPathSeparator(savePath.Substring(0, savePath.Length - Path.GetExtension(savePath).Length));
            string directoryPath = Path.GetDirectoryName(savePath);
            if (Directory.Exists(directoryPath))
            {
                string[] files = Directory.GetFiles(directoryPath);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(HttpDownloader.TempMarkStr) && !files[i].Contains(path))
                    {
                        File.Delete(files[i]);
                    }
                }
            }
            DownloadArg arg = new DownloadArg(DownloadType.WholeFile);
            arg.sourceUrls = urls;
            arg.destPath = savePath;
            arg.isMultiThread = true;
            arg.targetSize = patchSize;
            arg.checkAlgorithm = new CheckAlgorithm(CheckAlgorithmType.Md5, _downloadList[index].GetPatchMd5());
            arg.callback = AddMainThreadComplete;
            downloader = new HttpDownloader(arg);
            downloader.SetAllowDownloadInBackground(true);
            downloader.SetAllowCarrierDataNetworkDownload(true);
            downloader.StartDownLoad();
        }

        public override void Update()
        {
            if (downloader != null && !downloader.IsAbort)
            {
                _hotFixExecuter.AvgDownloadSpeed = downloader.AvgDownloadSpeed;
                _hotFixExecuter.DownloadTime = downloadTime + downloader.DownloadTime;
                if (downloader.RealNeedDownloadSize > 0)
                {
                    OnProcess(finishSize + downloader.TotalReceived, targetSize);
                }
            }
        }

        private void AddMainThreadComplete(bool suc)
        {
            _suc = suc;
            ZeusCore.Instance.AddMainThreadTask(OnDownloadComplete);
        }

        private void OnDownloadComplete()
        {
            ZeusCore.Instance.UnRegisterOnApplicationQuit(OnQuit);
            if (downloader != null)
            {
                _hotFixExecuter.AvgDownloadSpeed = downloader.AvgDownloadSpeed;
                downloadTime += downloader.DownloadTime;
                finishSize += downloader.TotalReceived;
            }
            if (_suc)
            {
                int oriIndex = index;
                index++;
                if (index < _downloadList.Count)
                {
                    StartDownload();
                }
                else
                {
                    ZeusCore.Instance.UnRegisterOnApplicationQuit(OnQuit);
                    if (!_lastDownload && _downloadList.Count > 0 && downloader != null)
                    {
                        _hotFixExecuter.InvokeOnDownloadSuc(_downloadList[oriIndex].GetUrls(_hotFixExecuter.PatchDownloadUrls), (int)downloader.DownloadTime);
                    }
                    downloader = null;
                    NextStep();
                }
            }
            else
            {
                ErrorType errorType = downloader.TopPriorityError;
                _hotFixExecuter.InvokeOnDownloadFail(_downloadList[index].GetUrls(_hotFixExecuter.PatchDownloadUrls),
                    _downloadList[index].GetPatchSize(), errorType.ToString(), (int)downloader.DownloadTime);
                HotfixError hotfixError = HotfixError.DownloadError;
                object param = null;
                switch (errorType)
                {
                    case ErrorType.NetError:
                        hotfixError = HotfixError.NetError;
                        break;
                    case ErrorType.HardDiskFull:
                        hotfixError = HotfixError.HardDiskFull;
                        param = targetSize - finishSize;
                        break;
                    case ErrorType.CheckFail:
                        hotfixError = HotfixError.CheckFail;
                        break;
                }
                downloader = null;
                if (hotfixError == HotfixError.HardDiskFull)
                {
                    ZeusCore.Instance.StartCoroutine(OnHardiskFull(param));
                }
                else
                {
                    OnError(hotfixError, "[HotfixService] Download Patch Fail.", param);
                }
            }
        }


        private IEnumerator OnHardiskFull(object needHardiskSpace)
        {
            WaitForTrigger trigger = new WaitForTrigger();
            long needHardiskSpaceLong = (long)needHardiskSpace;
            OnConfirm(HotfixTips.HardDiskFull, trigger, needHardiskSpace);
            yield return trigger;
            while (needHardiskSpaceLong >= DiskUtils.CheckAvailableSpaceBytes())
            {
                trigger = new WaitForTrigger();
                OnConfirm(HotfixTips.HardDiskFull, trigger, needHardiskSpace);
                yield return trigger;
            }
            Run();
        }

        private void OnQuit()
        {
            ZeusCore.Instance.UnRegisterOnApplicationQuit(OnQuit);
            if (downloader != null) downloader.Abort();
        }

        ~HotFixDownload()
        {
            ZeusCore.Instance.UnRegisterOnApplicationQuit(OnQuit);
#if !UNITY_EDITOR && UNITY_ANDROID
            try
            {
                HttpDownloader.DisposeAndroidJavaClass();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
#endif
        }
    }
}
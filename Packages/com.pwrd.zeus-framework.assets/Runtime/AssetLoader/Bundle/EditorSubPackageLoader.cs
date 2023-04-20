using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zeus.Core;

namespace Zeus.Framework.Asset
{
    public class EditorSubPackageLoader : ISubPackageLoader
    {
        private DownloadService _downloadService;
        private string level;

        public EditorSubPackageLoader(string level)
        {
            if (SubPackageBundleInfoContainer.IsExistSubpackageInfo(level))
            {
                _downloadService = new DownloadService();
                _downloadService.SetSubpackageLevel(level);
            }
            else
            {
                Debug.LogWarning("EditorSubPackageLoader not found subpackageInfo");
            }
        }

        public void DownloadSubpackage(int maxDownloadingTask,
           Action<double, double, double, SubpackageState, SubpackageError> downloadingProgressCallback,
           double limitSpeed,
           bool isBackgroundDownloading,
           string[] tags,
           bool isDownloadAll)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.DownloadSubpackage(maxDownloadingTask, downloadingProgressCallback, limitSpeed, isBackgroundDownloading, tags, isDownloadAll);
        }


        public void PauseDownloading()
        {
            if(_downloadService != null)
            {
                _downloadService.Pause();
            }
        }

        public void SetLimitSpeed(double limitSpeed)
        {
            if (_downloadService != null)
            {
                _downloadService.SetLimitSpeed(limitSpeed);
            }
        }

        public void SetAllowDownloadInBackground(bool isAllowd)
        {

        }

        public void SetCarrierDataNetworkDownloading(bool isAllowed)
        {

        }

        public bool GetCarrierDataNetworkDownloadingAllowed()
        {
            return true;
        }

        public bool IsBackgroundDownloadAllowed()
        {
            return false;
        }

        public void SetCdnUrl(string urlsStr) { }

        public void SetCdnUrl(string[] urls) { }

        public void SetSucNotificationStr(string str) { }

        public void SetFailNotificationStr(string str) { }

        public void SetKeepAliveNotificationStr(string str) { }

        public void SetShowBackgroundDownloadProgress(bool show, string downloadingNotificationStr = null, string carrierDataNetworkNotificationStr = null) { }

        public Dictionary<string, double> GetTag2SizeDic()
        {
            if(_downloadService != null)
            {
                return _downloadService.GetTag2SizeDic();
            }
            else
            {
                return null;
            }
        }

        public bool IsSubpackageReady(string tag)
        {
            if(_downloadService != null)
            {
                return _downloadService.IsSubpackageReady(tag);
            }
            else
            {
                return true;
            }
        }

        public bool IsHardDiskEnough() { return true; }

        public double CalcUnCompleteChunkSizeForTag(string tag)
        {
            if(_downloadService != null)
            {
                return _downloadService.CalcUnCompleteChunkSizeForTag(tag);
            }
            else
            {
                return 0;
            }
        }

        public double GetSizeToDownloadOfTag(string tag) 
        {
            if(_downloadService != null) 
            {
                return _downloadService.GetSizeToDownloadOfTag(tag);
            }
            else
            {
                return 0;
            }
        }

        public double GetTagSize(string tag) 
        { 
            if(_downloadService != null)
            {
                return _downloadService.GetTagSize(tag);
            }
            else
            {
                return 0;
            }
        }

        public void GetSubPackageSize(out double totalSize, out double completeSize) 
        {
            if (_downloadService != null)
            {
                _downloadService.GetSubPackageSize(out totalSize, out completeSize);
            }
            else
            {
                totalSize = 0;
                completeSize = 0;
            }
        }

        public void SetAssetLoadExceptionObserver(Action<AssetLoadErrorType, string> observer) { }

        public void SetAssetRemoteLoadObserver(Action<string, string, int, float, string> observer) { }

        public void SetTagStatusObserver(Action<string, bool> observer) { }
        //检查远端（CDN）资源是否就绪，用于初步检查二包数据是否上传
        public void CheckRemoteFileStatus(Action<ChunkListStatus> remote)
        {
            remote(ChunkListStatus.Ready);
        }

        public void AddPercentNotification(int percent, string notificationStr) { }

        public void ClearPercentNotification() { }

        public void SetIsAutoRetryDownloading(bool value)
        {
            if (_downloadService != null)
            {
                _downloadService.SetIsAutoRetryDownloading(value);
            }
        }

        public bool GetIsAutoRetryDownloading()
        {
            if (_downloadService != null)
            {
                return _downloadService.GetIsAutoRetryDownloading();
            }
            return false;
        }
    }
}


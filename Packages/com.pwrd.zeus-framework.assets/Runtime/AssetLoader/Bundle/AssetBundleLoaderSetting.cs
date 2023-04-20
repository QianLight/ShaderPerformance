/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    enum SubpackageMode
    {
        CreateOtherTagForUnrecordedAsset,
        AddUnrecordedAssetToFirstpackage
    }

    [System.Serializable]
    class AssetBundleLoaderSetting
    {
        public const string FirstPackageTag = "FirstPackage";
        public const string OthersTag = "Others";

        public DefaultBundleCollectorSetting defaultBundleCollectorSetting;
        public bool useSubPackage;
        public bool useBundleScatter;
        public bool fullAsyncCallback;
        public List<string> remoteURL;
        public SubpackageMode mode;

        //打包设置
        public int AssetBundleChunkSizeInMB;
        public int TargetiOSAssetSizeInMB;//用于首包扩充的资源大小
        public int TargetAndroidAssetSizeInMB;
        public List<string> ScenesInBuild;

        //是否开启首包扩充
        public bool isFillFirstPackageAndroid;
        public bool isFillFirstPackageiOS;
        //是否开启运营商网络下载
        public bool isCarrierDataNetworkDownloadAllowed;
        //是否支持后台下载
        public bool isSupportBackgroundDownload;
        //是否允许后台下载
        public bool isBackgroundDownloadAllowed;
        public string resourcesRootFolder;
        public bool isRemoveResourcesRootFolder = false;
        public bool enableAssetLevel = false;
        public bool enableAssetLevel_GenerateAll = false;
        //资源异常log输出等级
        public AssetAbsenceLogLevel assetAbsenceLogLevel = AssetAbsenceLogLevel.EditorDevelopmentDevice;
        //分包网络问题下载失败时是否自动重试
        public bool isAutoRetryDownloading = false;

        public AssetBundleLoaderSetting()
        {
            resourcesRootFolder = "Resources/";
            isCarrierDataNetworkDownloadAllowed = false;
            isFillFirstPackageAndroid = false;
            isFillFirstPackageiOS = false;
            isSupportBackgroundDownload = true;
            isBackgroundDownloadAllowed = true;
            defaultBundleCollectorSetting = new DefaultBundleCollectorSetting();
            useSubPackage = false;
            useBundleScatter = true;
            fullAsyncCallback = false;
            remoteURL = new List<string>();
            mode = SubpackageMode.CreateOtherTagForUnrecordedAsset;

            AssetBundleChunkSizeInMB = 2;
            TargetiOSAssetSizeInMB = 3700;
#if UNITY_EDITOR
            ScenesInBuild = new List<string>();
            foreach (UnityEditor.EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
            {
                ScenesInBuild.Add(scene.path);
            }
#else
            ScenesInBuild = null;
#endif
            enableAssetLevel = false;
        }

        public void RemoveAllInvalidURL()
        {
            remoteURL.RemoveAll((string str) => { return string.IsNullOrEmpty(str); });
            ScenesInBuild.RemoveAll((string str) => { return string.IsNullOrEmpty(str); });
        }

        #region 流量下载相关
        const string CarrierDataNetworkDownloadingStoreKey = "Zeus_Asset_AllowCarrierDataNetworkDownloading";
        const string BackgroundDownloadingStoreKey = "Zeus_Asset_AllowBackgroundDownloading";
        const string AutoRetryDownloadingStoreKey = "Zeus_Asset_AutoRetryDownloading";
        const int AssetBundleLoaderAllowInt = 1;
        const int AssetBundleLoaderDontAllowInt = 0;


        public void LoadPlayerPrefs()
        {
            if (Application.isPlaying)
            {
                if (PlayerPrefs.HasKey(CarrierDataNetworkDownloadingStoreKey))
                {
                    isCarrierDataNetworkDownloadAllowed = PlayerPrefs.GetInt(CarrierDataNetworkDownloadingStoreKey) == AssetBundleLoaderAllowInt ? true : false;
                }
                if (isSupportBackgroundDownload)
                {
                    if (PlayerPrefs.HasKey(BackgroundDownloadingStoreKey))
                    {
                        isBackgroundDownloadAllowed = PlayerPrefs.GetInt(BackgroundDownloadingStoreKey) == AssetBundleLoaderAllowInt ? true : false;
                    }
                }
                else
                {
                    isBackgroundDownloadAllowed = false;
                }
                if (PlayerPrefs.HasKey(AutoRetryDownloadingStoreKey))
                {
                    isAutoRetryDownloading = PlayerPrefs.GetInt(AutoRetryDownloadingStoreKey) == AssetBundleLoaderAllowInt ? true : false;
                }
            }
        }

        public static void SetCarrierDataNetworkDownloading(bool isAllowed)
        {
            int allow = isAllowed ? AssetBundleLoaderAllowInt : AssetBundleLoaderDontAllowInt;
            PlayerPrefs.SetInt(CarrierDataNetworkDownloadingStoreKey, allow);
            if (AssetManager.AssetSetting != null)
            {
                AssetManager.AssetSetting.bundleLoaderSetting.isCarrierDataNetworkDownloadAllowed = isAllowed;
            }
            PlayerPrefs.Save();
        }

        internal static bool GetCarrierDataNetworkDownloadingAllowed()
        {
            return PlayerPrefs.GetInt(CarrierDataNetworkDownloadingStoreKey, AssetBundleLoaderDontAllowInt) == 1 ? true : false;
        }

        public static void SetAllowDownloadInBackground(bool isAllowed)
        {
            int allow = isAllowed ? AssetBundleLoaderAllowInt : AssetBundleLoaderDontAllowInt;
            PlayerPrefs.SetInt(BackgroundDownloadingStoreKey, allow);
            if (AssetManager.AssetSetting != null && AssetManager.AssetSetting.bundleLoaderSetting.isSupportBackgroundDownload)
            {
                AssetManager.AssetSetting.bundleLoaderSetting.isBackgroundDownloadAllowed = isAllowed;
            }
            PlayerPrefs.Save();
        }

        internal static bool IsBackgroundDownloadAllowed()
        {
            if (AssetManager.AssetSetting != null && !AssetManager.AssetSetting.bundleLoaderSetting.isSupportBackgroundDownload)
            {
                return false;
            }
            return PlayerPrefs.GetInt(BackgroundDownloadingStoreKey, AssetBundleLoaderDontAllowInt) == 1 ? true : false;
        }

        public static void SetAutoRetryDownloading(bool value)
        {
            int retry = value ? AssetBundleLoaderAllowInt : AssetBundleLoaderDontAllowInt;
            PlayerPrefs.SetInt(AutoRetryDownloadingStoreKey, retry);
            if (AssetManager.AssetSetting != null)
            {
                AssetManager.AssetSetting.bundleLoaderSetting.isAutoRetryDownloading = value;
            }
            PlayerPrefs.Save();
        }

        internal static bool IsAutoRetryDownloading()
        {
            return PlayerPrefs.GetInt(AutoRetryDownloadingStoreKey, AssetBundleLoaderDontAllowInt) == 1 ? true : false;
        }
        #endregion
    }
}
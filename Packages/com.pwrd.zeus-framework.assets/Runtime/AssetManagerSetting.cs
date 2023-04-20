/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using UnityEngine;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Asset
{
    public enum AssetLoaderType
    {
        AssetBundle,
        Resources,
        AssetDatabase
    }

    public enum AssetAbsenceLogLevel
    {
        EditorDevice,             //设备上也输出log
        EditorDevelopmentDevice,  //developmentment打包及editor平台输出，release build不输出
        EditorOnly,               //只在editor上输出
    }

    [System.Serializable]
    class AssetManagerSetting
    {
        public AssetLoaderType assetLoaderType;
        public bool removeFileExtension;

        [SerializeField]
        public ResourceLoaderSetting resourceLoaderSetting;
        [SerializeField]
        public AssetBundleLoaderSetting bundleLoaderSetting;
        [SerializeField]
        public AssetDataBaseLoaderSetting assetDatabaseSetting;

        public AssetManagerSetting()
        {
            assetLoaderType = AssetLoaderType.AssetBundle;
            resourceLoaderSetting = new ResourceLoaderSetting();
            bundleLoaderSetting = new AssetBundleLoaderSetting();
            assetDatabaseSetting = new AssetDataBaseLoaderSetting();
        }

        public static AssetManagerSetting LoadSetting()
        {
            string relativeSettingPath = VFileSystem.GetZeusSettingPath("ZeusAssetManagerSetting.json");
            AssetManagerSetting setting;
            if (!VFileSystem.Exists(relativeSettingPath))
            {
#if UNITY_EDITOR
                if (!PackageUtility.CopySettingFile(PackageInfo.PackageName))
                {
#endif
                    setting = new AssetManagerSetting();
                    SaveSetting(setting);
                    return setting;
#if UNITY_EDITOR
                }
#endif
            }
            string content = VFileSystem.ReadAllText(relativeSettingPath, System.Text.Encoding.UTF8);
            setting = UnityEngine.JsonUtility.FromJson<AssetManagerSetting>(content);
            //重载"是否开启流量下载"设置
            setting.bundleLoaderSetting.LoadPlayerPrefs();
            return setting;
        }

        public static void SaveSetting(AssetManagerSetting setting)
        {
            if (setting != null)
            {
                string settingFullPath = VFileSystem.GetRealZeusSettingPath("ZeusAssetManagerSetting.json");
                setting.bundleLoaderSetting.RemoveAllInvalidURL();
                string settingContent = UnityEngine.JsonUtility.ToJson(setting, true);
                if (!Directory.Exists(Path.GetDirectoryName(settingFullPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(settingFullPath));
                }
                System.IO.File.WriteAllText(settingFullPath, settingContent);
            }
            else
            {
                Debug.LogError("_setting is null");
            }
        }
    }
}

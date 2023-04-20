/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Text;
using UnityEngine;
using Zeus.Core.FileSystem;
using System.Collections.Generic;

namespace Zeus.Framework.Hotfix
{
    public static class HotfixLocalConfigHelper
    {
        public static string HotfixLocalConfigFile = VFileSystem.GetBuildinSettingPath("HotfixLocalConfig.json");

        public static HotfixLocalConfig LoadLocalConfig()
        {
            HotfixLocalConfig config;
            if (InnerPackage.ExistsFile(HotfixLocalConfigFile))
            {
                string content = VFileSystem.ReadAllText(HotfixLocalConfigFile, Encoding.UTF8);
                config = JsonUtility.FromJson<HotfixLocalConfig>(content);
            }
            else
            {
                config = new HotfixLocalConfig();
            }
            return config;
        }

        public static void SaveLocalConfig(HotfixLocalConfig config)
        {
            if (config != null)
            {
                string _configPath = VFileSystem.GetAssetsFolderRealPath(HotfixLocalConfigFile);
                string content = JsonUtility.ToJson(config, true);
                string directory = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(_configPath, content, Encoding.UTF8);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            else
            {
                Debug.LogError("_localConfig is null");
            }
        }
    }
}
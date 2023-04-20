/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    [System.Serializable]
    public class SubpackageSetting
    {
        public static readonly string SettingPath = "ZeusSetting/EditorSetting/Asset/Subpackage/SubPackageSetting.json";
        private static string _settingPath = Path.Combine(Application.dataPath, SettingPath);

        public int currentTag;

        public string assetListOutputPath;
        public const string DEFAULT_OUTPUT_PATH = "AssetListLog";
        public string SubpackageUrlListOutputPath;
        public List<string> UrlList;

        public SubpackageSetting()
        {
            assetListOutputPath = DEFAULT_OUTPUT_PATH;
            if (!Directory.Exists(assetListOutputPath))
            {
                Directory.CreateDirectory(assetListOutputPath);
            }
            SubpackageUrlListOutputPath = string.Empty;
            UrlList = new List<string>();
            
            string directory = Path.GetDirectoryName(_settingPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(_settingPath, JsonUtility.ToJson(this, true));
            }
        }

        public void RemoveAllInvalidURL()
        {
            UrlList.RemoveAll((string str) => { return string.IsNullOrEmpty(str); });
        }

        public void Save()
        {
            RemoveAllInvalidURL();
            string settingContent = UnityEngine.JsonUtility.ToJson(this, true);
            string directory = Path.GetDirectoryName(_settingPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(_settingPath, settingContent);
        }

        public static SubpackageSetting LoadSetting()
        {
            if (File.Exists(_settingPath))
            {
                string settingContent = File.ReadAllText(_settingPath);
                return UnityEngine.JsonUtility.FromJson<SubpackageSetting>(settingContent);
            }
            else
            {
                return new SubpackageSetting();
            }
        }
    }
}

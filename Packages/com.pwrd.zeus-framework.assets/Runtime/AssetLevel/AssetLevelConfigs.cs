using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Asset
{
    [Serializable]
    public class AssetLevelMapperConfig
    {
        public bool IsDefault;
        public string LevelName;
        public string ClassName;
        public string AssemblyName;
        [NonSerialized]
        public int AssemblyIndex;
        [NonSerialized]
        public int ClassNameIndex;
    }

    [Serializable]
    public class AssetLevelConfigs
    {
        public const string DefaultMapperLevelName = "Default";
        public List<AssetLevelMapperConfig> Levels = new List<AssetLevelMapperConfig>();
        private static AssetLevelMapperConfig m_defaultConfigItem;
        private const string ConfigFileName = "AssetLevelConfig.json";

#if UNITY_EDITOR
        public void Save()
        {
            var json = JsonUtility.ToJson(this, true);
            File.WriteAllText(VFileSystem.GetRealZeusSettingPath(ConfigFileName), json);
            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        public static AssetLevelMapperConfig DefaultMapperConfig
        {
            get
            {
                if (null == m_defaultConfigItem)
                {
                    m_defaultConfigItem = new AssetLevelMapperConfig
                    {
                        LevelName = DefaultMapperLevelName,
                        AssemblyName = "Zeus.Asset",
                        ClassName = "Zeus.Framework.Asset.AssetLevelMapperDefault",
                        IsDefault = true,
                    };
                }
                return m_defaultConfigItem;
            }
        }
        public static AssetLevelConfigs Load()
        {
            string content = VFileSystem.ReadAllText(VFileSystem.GetZeusSettingPath(ConfigFileName), System.Text.Encoding.UTF8);
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("AssetLevelConfigs.json missing!");
                return new AssetLevelConfigs();
            }
            else
            {
                AssetLevelConfigs config = JsonUtility.FromJson<AssetLevelConfigs>(content);
                if (config == null)
                {
                    Debug.LogError("AssetLevelConfigs.json parse error!");
                    return new AssetLevelConfigs();
                }
                else
                {
                    return config;
                }
            }
        }
    }
}
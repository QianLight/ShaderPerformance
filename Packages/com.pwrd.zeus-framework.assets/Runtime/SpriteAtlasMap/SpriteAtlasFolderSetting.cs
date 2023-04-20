/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Collections.Generic;

#if !RESOURCE_PROJECT
using Zeus.Core.FileSystem;
#endif

namespace Zeus.Framework.Asset
{
    [System.Serializable]
    class SpriteAtlasFolderSetting
    {

#if RESOURCE_PROJECT
        static string settingFilePath = "Assets/SpriteAtlasFolderSetting.json";
#else
        static string settingFileName = "SpriteAtlasFolderSetting.json";
#endif
        public string atlasFolderPath_editor = string.Empty;
        public string atlasFolderPath = string.Empty;

        static SpriteAtlasFolderSetting _setting;
        public static SpriteAtlasFolderSetting Setting
        {
            get
            {
                if (_setting == null)
                    _setting = LoadSetting();
                return _setting;
            }
        }

        public static string AtlasFolderPath_Editor
        {
            get
            {
                return Setting.atlasFolderPath_editor;
            }
        }

        public static string AtlasFolderPath
        {
            get
            {
                return Setting.atlasFolderPath;
            }
        }

        static SpriteAtlasFolderSetting LoadSetting()
        {
#if RESOURCE_PROJECT
            if(!File.Exists(settingFilePath))
                return new SpriteAtlasFolderSetting();

            string content = File.ReadAllText(settingFilePath, System.Text.Encoding.UTF8);
            var afp = UnityEngine.JsonUtility.FromJson<SpriteAtlasFolderSetting>(content);
            return afp;
#else
            string relativeSettingPath = VFileSystem.GetZeusSettingPath(settingFileName);
            if (!VFileSystem.Exists(relativeSettingPath))
            {
                return new SpriteAtlasFolderSetting();
            }

            string content = VFileSystem.ReadAllText(relativeSettingPath, System.Text.Encoding.UTF8);
            var afp = UnityEngine.JsonUtility.FromJson<SpriteAtlasFolderSetting>(content);
            return afp;
#endif
        }

        public static void Save(string atlasFolderPath_Editor, string atlasFolderPath)
        {
            Setting.atlasFolderPath_editor = atlasFolderPath_Editor;
            Setting.atlasFolderPath = atlasFolderPath;
            SaveSetting(Setting);
        }

        private static void SaveSetting(SpriteAtlasFolderSetting afp)
        {
#if RESOURCE_PROJECT
             if (!Directory.Exists(Path.GetDirectoryName(settingFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingFilePath));
            }

            string afpContent = UnityEngine.JsonUtility.ToJson(afp);
            UnityEngine.Debug.Log("Update file in " + settingFilePath);
            System.IO.File.WriteAllText(settingFilePath, afpContent);       
#else
            string storePath = VFileSystem.GetRealZeusSettingPath(settingFileName);
            string afpContent = UnityEngine.JsonUtility.ToJson(afp, true);

            if (!Directory.Exists(Path.GetDirectoryName(storePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(storePath));
            }

            UnityEngine.Debug.Log("Update file in " + storePath);

            System.IO.File.WriteAllText(storePath, afpContent);
#endif
        }
    }
}

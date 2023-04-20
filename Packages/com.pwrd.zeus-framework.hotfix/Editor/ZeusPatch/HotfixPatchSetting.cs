/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Hotfix
{
    public class HotfixPatchSetting
    {
        public static string LuaPatchSettingFile = "/ZeusSetting/EditorSetting/Hotfix/PatchSetting.json";

        public BuildTarget buildTarget;
        public bool useLuaEncryptor;
        public List<IncludeFolderItem> includeFolder;
        public bool bLuaPatch;
        public List<string> m_beforeGenerate;

        public HotfixPatchSetting()
        {
            buildTarget = BuildTarget.Android;
            useLuaEncryptor = false;
            includeFolder = new List<IncludeFolderItem>();
            m_beforeGenerate = new List<string>();
        }

        public void RemoveInvailidString()
        {
            includeFolder.RemoveAll(folderItem => null == folderItem || string.IsNullOrEmpty(folderItem.AbsolatePath));
            m_beforeGenerate.RemoveAll(s => string.IsNullOrEmpty(s));
        }

        public static HotfixPatchSetting LoadSetting()
        {
            HotfixPatchSetting setting;
            if(VFileSystem.ExistsFile(LuaPatchSettingFile))
            {
                string content = VFileSystem.ReadAllText(LuaPatchSettingFile, System.Text.Encoding.UTF8);
                setting = JsonUtility.FromJson<HotfixPatchSetting>(content);
            }
            else
            {
                setting = new HotfixPatchSetting();
            }
            return setting;

        }

        public static void SaveSetting(HotfixPatchSetting setting)
        {
            if(setting != null)
            {
                setting.RemoveInvailidString();
                string content = JsonUtility.ToJson(setting);
                string directory = Path.GetDirectoryName(VFileSystem.GetRealPath(LuaPatchSettingFile));
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(VFileSystem.GetRealPath(LuaPatchSettingFile), content);
                UnityEditor.AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("_luaPathSetting is null");
            }
        }
    }
}
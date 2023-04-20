/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEngine;
using Zeus.Core;
using Zeus.Core.FileSystem;

namespace Zeus
{
    public enum ZeusObbBuild
    {
        None,
        Unity,
        Zeus,
        AAB,
    }

    internal class InApkSetting
    {
        private const string relativeSettingPath = "InApkSetting.json";

        public ZeusObbBuild obbMode;

        public InApkSetting()
        {
            obbMode = ZeusObbBuild.None;
        }

        public static InApkSetting LoadSetting()
        {
            try
            {
                string content = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(relativeSettingPath)).text;
                return JsonUtility.FromJson<InApkSetting>(content);
            }
            catch(Exception e)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Get InApkSetting failed.");
#else
                Debug.LogException(e);
                Debug.LogError("Get InApkSetting failed.");
#endif
                return new InApkSetting();
            }
        }

#if UNITY_EDITOR
        public static void SaveSetting(InApkSetting setting)
        {
            string path =VFileSystem.GetAssetsFolderRealPath(VFileSystem.GetInApkSettingPath(relativeSettingPath));
            FileUtil.EnsureFolder(path);
            string content = JsonUtility.ToJson(setting, true);
            File.WriteAllText(path, content);
        }
#endif
    }
}
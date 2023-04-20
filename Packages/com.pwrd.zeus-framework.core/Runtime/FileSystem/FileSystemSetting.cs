/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

namespace Zeus.Core.FileSystem
{
    [System.Serializable]
    public class FileSystemSettingConfig
    {
        /// <summary>
        /// 覆盖安装清理OuterPackage文件夹时，需要选择性保留内部文件的文件夹
        /// </summary>
        public List<string> RetainFileDirectoryList = new List<string>()
        {
            "Bundles",
        };
        /// <summary>
        /// 需要从包内拷贝到包外的文件列表
        /// </summary>
        public List<string> InnerToOuterFileList = new List<string>();
        public List<string> InnerToOuterJsonList = new List<string>();
        public bool isCombineFile = true;
    }

    public static class FileSystemSetting
    {
        public static string FileSystemSettingPath = VFileSystem.GetBuildinSettingPath("FileSystemSettingConfig.json");
        public static FileSystemSettingConfig LoadLocalConfig()
        {
            FileSystemSettingConfig config;
            if (VFileSystem.ExistsFile(FileSystemSettingPath))
            {
                string content = VFileSystem.ReadAllText(FileSystemSettingPath, Encoding.UTF8);
                config = JsonUtility.FromJson<FileSystemSettingConfig>(content);
            }
            else
            {
                config = new FileSystemSettingConfig();
            }
            return config;
        }

        public static void SaveLocalConfig(FileSystemSettingConfig config)
        {
            if (config != null)
            {
                config.RetainFileDirectoryList.RemoveAll(t => string.IsNullOrEmpty(t));
                string content = JsonUtility.ToJson(config, true);
                string directory = Path.GetDirectoryName(VFileSystem.GetRealPath(FileSystemSettingPath));
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(VFileSystem.GetRealPath(FileSystemSettingPath), content, Encoding.UTF8);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
            else
            {
                Debug.LogError("_RedundantFileConfig is null");
            }
        }
    }

}
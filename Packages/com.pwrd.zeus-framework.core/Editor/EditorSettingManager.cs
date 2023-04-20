/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using Zeus.Framework;
using UnityEditor;
using System;
using Zeus.Build;
using UnityEngine;
using System.IO;
using Zeus.Core.FileSystem;

namespace Zeus.Core
{
    public class EditorSettingManager : IModifyPlayerSettings
	{
        private readonly string[] ExcludeSettingFolder = { "EditorSetting", "Resources" };
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            // 统计所有涉及的文件夹
            //            Dictionary<string, bool> dictionaryIncludeFold = new Dictionary<string, bool>();
            //Dictionary<string, bool> dictionaryExcludePath = new Dictionary<string, bool>();

            //HashSet<string> dictionaryIncludePathSet = new HashSet<string>();
            //HashSet<string> dictionaryExcludePathSet = new HashSet<string>();
            //foreach (string directory in dictionaryIncludePathSet)
            //{
            //    string sourceFullPath = Path.Combine(Application.dataPath, directory);
            //    GameBuildProcessor.AddIncludeBuildPath(sourceFullPath, directory);
            //}
            /*
            foreach (KeyValuePair<Type, string> pairs in SettingManager.m_dictionaryType2Path)
            {
                if (SettingManager.IsBuildWith(pairs.Key))
                {
                    string parentDirectory = PathUtil.GetPathParentDirectory(pairs.Value);
                    if (!string.IsNullOrEmpty(parentDirectory))
                    {
                        dictionaryIncludeFold[parentDirectory] = true;
                    }
                }
                else
                {
                    dictionaryExcludePath[pairs.Value] = true;
                }
            }*/

            string settingFilePath = VFileSystem.GetZeusSettingPath("");
            string settingFullPath = Path.Combine(Application.dataPath, settingFilePath);
            GameBuildProcessor.AddIncludeBuildPath(settingFullPath, settingFilePath);

            foreach (string folder in ExcludeSettingFolder)
            {
                string fullDirectory = Path.GetFullPath(VFileSystem.GetAssetsFolderRealPath(VFileSystem.GetZeusSettingPath(folder)));
                if (Directory.Exists(fullDirectory))
                {
                    foreach (string file in Directory.GetFiles(fullDirectory, "*", SearchOption.AllDirectories))
                    {
                        GameBuildProcessor.AddExcludeBuildPath(PathUtil.FormatPathSeparator(file));
                    }
                }
            }

            //foreach (string path in dictionaryExcludePathSet)
            //{
            //    GameBuildProcessor.AddExcludeBuildPath(Path.Combine(Application.dataPath, path));
            //}
        }
    }
}

/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

namespace Zeus.Framework.Asset
{
    public class OSSUploadUtil: IUploadUtil
    {
        private string OSSUtilPath
        {
            get
            {
                return PackageInfo.PackageFullPath + "/Editor/Utilities/OSSUtil/";
            }
        }

        OSSSetting _setting;

        public OSSUploadUtil(OSSSetting setting)
        {
            _setting = setting;
        }

        public OSSUploadUtil()
        {
            string settingPath = Application.dataPath + "/" + CdnSettingWindow.CdnSettingPath;
            if (File.Exists(settingPath))
            {
                string settingContent = File.ReadAllText(settingPath);
                _setting = UnityEngine.JsonUtility.FromJson<CdnSetting>(settingContent).ossSetting;
            }
            else
            {
                throw new Exception("Missing CdnSetting file.");
            }
        }

        private void CreateOSSConfig(string luaexe)
        {
            string args = "config --config-file " + OSSUtilPath + "config " + "-e " + _setting.endPoint + " -i " + _setting.accessKey + " -k " + _setting.accessKeySecret + " --output-dir " + OSSUtilPath + "Log" + " -L CH";
            try
            {
                ToolsUtility.ExecuteCommand(luaexe, args);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string GetUploadArgs(string srcPath)
        {
            //批量上传时，若指定--update（可缩写为-u）选项，只有当目标文件不存在，或源文件的最后修改时间晚于目标文件时，ossutil才会执行上传操作。
            if (Directory.Exists(srcPath))
            {
                return "cp -r " + srcPath + " oss://" + _setting.bucketName + "/" + _setting.targetFolder + " -u --config-file " + OSSUtilPath + "config";
            }
            else if(File.Exists(srcPath))
            {
                return "cp " + srcPath + " oss://" + _setting.bucketName + "/" + _setting.targetFolder + " -u --config-file " + OSSUtilPath + "config";
            }
            else
            {
                throw new Exception(string.Format("Missing of path \"{0}\", please check it in \"Zues->Setting->Asset->Subpackage->OSS设置\"", srcPath));
            }
        }

        public void UploadBundle()
        {
            UploadBundle(_setting.sourceBundlesFolder);
        }

        public void UploadBundle(string sourceFolder)
        {
            string luaexe = OSSUtilPath;
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                luaexe += "ossutil64.exe";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                luaexe += "./ossutilmac64";
                ToolsUtility.ExecuteCommand("chmod", "777" + luaexe);
            }
            else
            {
                throw new Exception("Wrong platform to execute ossutil.");
            }
            try
            {
                CreateOSSConfig(luaexe);
                ToolsUtility.ExecuteCommand(luaexe, GetUploadArgs(sourceFolder));
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            Debug.Log("OSS upload successfully.");
        }

        public void OnGUI()
        {
            _setting.bucketName = EditorGUILayout.TextField("Bucket Name", _setting.bucketName).Trim();
            _setting.accessKey = EditorGUILayout.TextField("Access Key", _setting.accessKey).Trim();
            _setting.accessKeySecret = EditorGUILayout.TextField("Access Key Secret", _setting.accessKeySecret).Trim();
            _setting.endPoint = EditorGUILayout.TextField("Endpoint", _setting.endPoint).Trim();
            _setting.targetFolder = EditorGUILayout.TextField("Target Folder", _setting.targetFolder).Trim();
            GUILayout.BeginHorizontal();
            _setting.sourceBundlesFolder = EditorGUILayout.TextField("Source Bundle Folder", _setting.sourceBundlesFolder).Trim();
            if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(55)))
            {
                string temp = EditorUtility.OpenFolderPanel("Output Folder", _setting.sourceBundlesFolder, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    _setting.sourceBundlesFolder = temp;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public enum UploadUtil
    {
        All,
        OSS
    }

    public class CdnSettingWindow : EditorWindow
    {
        public static string CdnSettingPath = "ZeusSetting/EditorSetting/Asset/Subpackage/CdnSetting.json";

        private Type _currentUploadUtilType;
        private Dictionary<UploadUtil, IUploadUtil> uploadUtilDic;

        private CdnSetting _setting;
        private string _settingPath;

        private void OnEnable()
        {
            uploadUtilDic = new Dictionary<UploadUtil, IUploadUtil>();
            _settingPath = Application.dataPath + "/" + CdnSettingPath;
            LoadSetting();
        }

        private void OnGUI()
        {
            _setting.util = (UploadUtil)EditorGUILayout.EnumPopup("UploadUtil", _setting.util);
            if(!_setting.util.Equals(UploadUtil.All))
            {
                IUploadUtil uploadUtil;
                if (uploadUtilDic.ContainsKey(_setting.util))
                {
                    uploadUtil = uploadUtilDic[_setting.util];
                }
                else
                {
                    _currentUploadUtilType = Assembly.GetExecutingAssembly().GetType("Zeus.Framework.Asset." + _setting.util.ToString() + "UploadUtil");
                    uploadUtil = Activator.CreateInstance(_currentUploadUtilType, _setting.ossSetting) as IUploadUtil;
                    uploadUtilDic.Add(_setting.util, uploadUtil);
                }
                uploadUtil.OnGUI();
                if (GUILayout.Button("SaveSetting"))
                {
                    SaveSetting();
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button("Upload"))
                {
                    if (uploadUtil != null)
                    {
                        uploadUtil.UploadBundle();
                    }
                }
            }
            else
            {
                if (GUILayout.Button("SaveSetting"))
                {
                    SaveSetting();
                }
                EditorGUILayout.Separator();
                if (GUILayout.Button("Upload"))
                {
                    foreach (string util in Enum.GetNames(typeof(UploadUtil)))
                    {
                        if (util.Equals(UploadUtil.All.ToString()))
                            continue;
                        Type currentUploadUtilType = Assembly.GetExecutingAssembly().GetType("Zeus.Framework.Asset." + util.ToString() + "UploadUtil");
                        IUploadUtil uploadUtil = Activator.CreateInstance(currentUploadUtilType) as IUploadUtil;
                        uploadUtil.UploadBundle();
                    }
                }
            }
        }

        private void LoadSetting()
        {
            if (File.Exists(_settingPath))
            {
                string settingContent = File.ReadAllText(_settingPath);
                _setting = UnityEngine.JsonUtility.FromJson<CdnSetting>(settingContent);
            }
            else
            {
                _setting = new CdnSetting();
            }
        }

        private void SaveSetting()
        {
            if (_setting != null)
            {
                string settingContent = UnityEngine.JsonUtility.ToJson(_setting, true);
                string directory = Path.GetDirectoryName(_settingPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(_settingPath, settingContent);
            }
            else
            {
                Debug.LogError("_setting is null");
            }
        }
    }
}

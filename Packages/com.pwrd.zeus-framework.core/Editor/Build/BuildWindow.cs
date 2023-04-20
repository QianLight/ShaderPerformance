/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Zeus.Framework;
using Zeus.Core;

namespace Zeus.Build
{
    public class BuildWindow : EditorWindow
    {
        private readonly string PropertiesExtension = ".properties";
        private PlatformSetting _platformSetting;
        private Properties _properties;
        private List<string> _proBuildSettingPath;
        private string[] _proBuildSettingFileName;
        private int _proBuildSettingIndex;
        private DateTime _lastBuildEndTime;

        [MenuItem("Zeus/BuildWindow &b", false, 0)]
        public static void Open()
        {
            BuildWindow window = GetWindow<BuildWindow>("BuildWindow", true);
            window.autoRepaintOnSceneChange = true;
            window.Show();
        }

        public void OnEnable()
        {
            BuildScript.EnsureBuildConfigs();
            InitBuildSettingFile();
        }

        Vector2 vec = Vector2.zero;
        private void OnGUI()
        {
            vec = GUILayout.BeginScrollView(vec);
            OnGUIFromProperties();
            OnGUIFromJson();
            OnGUIAABProperities();

            //上次打包结束之后等待10s 防止GUI按钮连续触发bug
            if (DateTime.Now -_lastBuildEndTime < TimeSpan.FromSeconds(10))
            {
                GUI.enabled = false;
                GUILayout.Button("请等待");
                GUI.enabled = true;
            }
            else if (GUILayout.Button("Build"))
            {
                string target = null;
                if (_properties.TryGetString(GlobalBuild.CmdArgsKey.PLATFORM, ref target))
                {
                    var currentTarget = EditorUserBuildSettings.activeBuildTarget.ToString().Replace("Standalone", "");
                    if (!string.Equals(target, currentTarget, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!EditorUtility.DisplayDialog("即将执行打包配置" + _proBuildSettingFileName[_proBuildSettingIndex],
                            "检测到当前目标平台为" + EditorUserBuildSettings.activeBuildTarget + ", 若继续执行将会切换到" + target + "平台!",
                            "确认执行", "取消"))
                        {
                            return;
                        }
                    }
                }

                Type log = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
                var clearMethod = log.GetMethod("Clear");
                clearMethod.Invoke(null, null);
                Build();
                _lastBuildEndTime = DateTime.Now;
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 更新AAB相关的属性显示，屏蔽不合理的设置
        /// </summary>
        private void OnGUIAABProperities()
        {
            bool exportAAB = false;
            bool exportAndroidProject = false;
            //如果没有选择导出Android工程,禁止勾选aab
            if(!_properties.TryGetBool(GlobalBuild.CmdArgsKey.IS_BUILD_ANDROID_PROJECT, ref exportAndroidProject) || !exportAndroidProject)
            {
                if(_properties.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref exportAAB) && exportAAB)
                {
                    EditorUtility.DisplayDialog("Warning", "只支持导出配置好aab的Android工程", "ok");
                    //导出apk也不应该勾
                    _properties.Update(GlobalBuild.CmdArgsKey.EXPORT_AAB, false.ToString());
                }
            }
        }

        private bool _lastUseJson;
        private void OnGUIFromJson()
        {
            _lastUseJson = _platformSetting.enable;
            _platformSetting.enable = EditorGUILayout.ToggleLeft("开启额外配置", _platformSetting.enable);
            if (_lastUseJson != _platformSetting.enable)
            {
                SaveJsonSetting();
            }
            if (_platformSetting.enable)
            {
                EditorGUILayout.BeginVertical();
                _platformSetting.luaEncrypt = (LuaEncrypt)EditorGUILayout.EnumPopup("LuaEncrypt", _platformSetting.luaEncrypt);
                _platformSetting.isDevelopment = EditorGUILayout.Toggle("Development", _platformSetting.isDevelopment);
                if (_platformSetting.isDevelopment)
                {
                    _platformSetting.isConnectProfiler = EditorGUILayout.Toggle("Connect Profiler", _platformSetting.isConnectProfiler);
                    _platformSetting.isDeepProfiling = EditorGUILayout.Toggle("Deep Profiling", _platformSetting.isDeepProfiling);
                }

                GUILayout.BeginHorizontal();
                _platformSetting.OutputPath = EditorGUILayout.TextField("OutputPath", _platformSetting.OutputPath);
                if (GUILayout.Button("…", EditorStyles.toolbar, GUILayout.Width(30)))
                {
                    string temp = EditorUtility.OpenFolderPanel("Output Folder", _platformSetting.OutputPath, string.Empty);
                    if (!string.IsNullOrEmpty(temp))
                    {
                        _platformSetting.OutputPath = temp;
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.Separator();
                for (int i = 0; i < _platformSetting.keyValueList.Count; i++)
                {
                    KeyValueSetting element = _platformSetting.keyValueList[i];
                    if (element == null) continue;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Key:", GUILayout.MaxWidth(30));
                    element.Key = EditorGUILayout.TextField(element.Key, GUILayout.MinWidth(160));
                    EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(40));
                    element.Value = EditorGUILayout.TextField(element.Value, GUILayout.MinWidth(160));
                    element.Type = (ValueType)EditorGUILayout.EnumPopup(element.Type);
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _platformSetting.keyValueList[i] = null;
                    }
                    EditorGUILayout.Separator();
                    GUILayout.EndHorizontal();
                }
                EditorGUILayout.Separator();

                EditorGUILayout.Separator();
                if (GUILayout.Button("Add Key Value"))
                {
                    KeyValueSetting element = new KeyValueSetting();
                    _platformSetting.keyValueList.Add(element);
                }

                if (GUILayout.Button("SaveJson"))
                {
                    SaveJsonSetting();
                }

                EditorGUILayout.Separator();
                EditorGUILayout.EndVertical();
            }
        }

        private bool _showPro = true;
        private void OnGUIFromProperties()
        {
            EditorGUIUtility.labelWidth = 200;
            _showPro = EditorGUILayout.Foldout(_showPro, "Properties");
            if (_showPro)
            {
                EditorGUILayout.BeginVertical();
                int currentIndex = EditorGUILayout.Popup("PropertiesFile", _proBuildSettingIndex, _proBuildSettingFileName);
                if (_proBuildSettingIndex != currentIndex)
                {
                    _proBuildSettingIndex = currentIndex;
                    LoadPropertiesSetting();
                }
                foreach (var element in _properties.Keys)
                {
                    bool b = false;
                    string formatKey = FormatKey(element);
                    if (_properties.TryGetBool(element, ref b))
                    {
                        bool temp = EditorGUILayout.Toggle(formatKey, b);
                        _properties.Update(element, temp.ToString());
                    }
                    else
                    {
                        string str = "";
                        if (_properties.TryGetString(element, ref str))
                        {
                            string temp = EditorGUILayout.TextField(formatKey, str);
                            _properties.Update(element, temp);
                        }
                    }
                }
                if (GUILayout.Button("SaveProperties"))
                {
                    SaveProSetting();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUIUtility.labelWidth = 0;
        }

        private string FormatKey(string key)
        {
            string format = string.Empty;
            string[] strs = key.Split('_');
            for (int i = 0; i < strs.Length; ++i)
            {
                if (strs[i].Length > 1)
                {
                    format += strs[i][0] + strs[i].Substring(1).ToLower() + " ";
                }
                else
                {
                    format += strs[i][0] + " ";
                }
            }
            return format.Substring(0, format.Length - 1);
        }

        private void Build()
        {
            CommandLineArgs.Clear();

            foreach (string key in _properties.Keys)
            {
                string value = string.Empty;
                if (_properties.TryGetString(key, ref value))
                {
                    CommandLineArgs.Add(key, value);
                }
            }

            if (_platformSetting.enable)
            {
                CommandLineArgs.Add(GlobalBuild.CmdArgsKey.IS_DEVELOPMENT_BUILD, _platformSetting.isDevelopment.ToString().ToLower());
                CommandLineArgs.Add(GlobalBuild.CmdArgsKey.LUA_ENCRYPT_BUILD, _platformSetting.luaEncrypt.ToString());
                if (!string.IsNullOrEmpty(_platformSetting.OutputPath))
                {
                    CommandLineArgs.Add(GlobalBuild.CmdArgsKey.OUTPUT_PATH, _platformSetting.OutputPath);
                }
                
                for (int i = 0; i < _platformSetting.keyValueList.Count; i++)
                {
                    KeyValueSetting element = _platformSetting.keyValueList[i];
                    if (element == null) continue;
                    CommandLineArgs.Add(element.Key, element.Value);
                }
            }

            DateTime beforeBuild = DateTime.Now;
            Zeus.Build.BuildScript.BuildPlayerInGraphicMode();
            TimeSpan buildTimeSpan = DateTime.Now - beforeBuild;
            Debug.Log("Build spend : " + buildTimeSpan.ToString());
        }

        private void InitBuildSettingFile()
        {
            GetProSettingFileName();
            LoadJsonSetting();
            LoadPropertiesSetting();
        }

        private void GetProSettingFileName()
        {
            _proBuildSettingPath = new List<string>();
            _proBuildSettingIndex = 0;
            string path = GlobalBuild.BuildConst.ZEUS_BUILD_PATH_CONFIG;
            string[] subFilePath = Directory.GetFiles(path);
            List<string> fileNameList = new List<string>();
            foreach (string str in subFilePath)
            {
                string temp = str.Replace((path), "").Substring(1);
                if (temp.Contains(PropertiesExtension) && temp.Split('.').Length < 3) // is properties file
                {
                    _proBuildSettingPath.Add(str);
                    fileNameList.Add(temp.Replace(PropertiesExtension, ""));
                }
            }
            _proBuildSettingFileName = fileNameList.ToArray();
        }

        private void LoadJsonSetting()
        {
            string path = Application.dataPath;
            if (!File.Exists(path + "/BuildSetting.json"))
            {
                Debug.LogWarning("Can't find BuildSetting file.");
                _platformSetting = new PlatformSetting();
            }
            else
            {
                string settingContent = File.ReadAllText(path + "/BuildSetting.json");
                _platformSetting = UnityEngine.JsonUtility.FromJson<PlatformSetting>(settingContent);
            }
        }

        private void LoadPropertiesSetting()
        {
            if (_proBuildSettingPath.Count > _proBuildSettingIndex)
            {
                if (_properties == null)
                {
                    _properties = new Properties(_proBuildSettingPath[_proBuildSettingIndex]);
                }
                else
                {
                    _properties.Load(_proBuildSettingPath[_proBuildSettingIndex]);
                }
            }
            else
            {
                Debug.LogWarning("_proBuildSettingIndex is out of bound!");
            }
        }

        private void SaveJsonSetting()
        {
            if (_platformSetting != null)
            {
                _platformSetting.RemoveAllInvalidSetting();
                string settingContent = UnityEngine.JsonUtility.ToJson(_platformSetting, true);
                File.WriteAllText(Application.dataPath + "/BuildSetting.json", settingContent);
            }
            else
            {
                Debug.LogError("_JsonBuildWindowSetting is null");
            }
        }

        private void SaveProSetting()
        {
            if (_properties != null)
            {
                if (_proBuildSettingPath.Count > _proBuildSettingIndex)
                {
                    _properties.Save(_proBuildSettingPath[_proBuildSettingIndex]);
                }
                else
                {
                    Debug.LogWarning("_proBuildSettingindex is out of bound, save in default path");
                    _properties.Save();
                }
            }
            else
            {
                Debug.LogError("_properties is null");
            }
        }

        public enum ValueType
        {
            String,
            Integer,
            Boolean,
        }

        [Serializable]
        public class KeyValueSetting
        {
            public string Key;
            public string Value;
            public ValueType Type = ValueType.String;
        }

        [Serializable]
        class PlatformSetting
        {
            public bool enable = false;
            public bool isDevelopment = false;
            public bool isConnectProfiler = false;
            public bool isDeepProfiling = false;
            public LuaEncrypt luaEncrypt = LuaEncrypt.None;
            [SerializeField]
            private string _outputPath = "";
            public string OutputPath
            {
                get { return _outputPath; }
                set { _outputPath = PathUtil.FormatPathSeparator(value); }
            }

            public List<KeyValueSetting> keyValueList = new List<KeyValueSetting>();

            public void RemoveAllInvalidSetting()
            {
                keyValueList.RemoveAll((KeyValueSetting s) => { return s == null; });
            }
        }
    }
}

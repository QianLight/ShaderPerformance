/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using Debug = UnityEngine.Debug;

namespace Zeus.Build.AAB
{
    internal class AABBuildWindow : EditorWindow
    {
        private readonly string PropertiesExtension = ".properties";
        Vector2 vec = Vector2.zero;
        private int m_proBuildSettingIndex;
        private string[] m_proBuildSettingFileName;
        private Properties m_properties;
        private List<string> m_proBuildSettingPath = new List<string>();
        private string m_aabExportPath = "";
        private string m_apksExportPath = "";

        public static AABBuildWindow Open()
        {
            var window = GetWindow<AABBuildWindow>("AAB测试窗口");
            window.Init();
            return window;
        }

        private void Init()
        {
            InitAABBuildSetting();
            GetProSettingFileName();
            LoadPropertiesSetting();
        }

        private void InitAABBuildSetting()
        {
            if (string.IsNullOrEmpty(m_aabExportPath))
            {
                m_aabExportPath = Path.Combine(Application.dataPath, "..");
            }
            if (string.IsNullOrEmpty(m_apksExportPath))
            {
                m_apksExportPath = Path.Combine(Application.dataPath, "..");
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            vec = GUILayout.BeginScrollView(vec);
            GUILayout.BeginHorizontal();
            int currentIndex = EditorGUILayout.Popup("AAB配置:", m_proBuildSettingIndex, m_proBuildSettingFileName);
            if (m_proBuildSettingIndex != currentIndex)
            {
                m_proBuildSettingIndex = currentIndex;
                LoadPropertiesSetting();
            }
            if (GUILayout.Button("编辑配置"))
            {
                BuildWindow.Open();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AAB导出位置:", m_aabExportPath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                if (string.IsNullOrEmpty(m_aabExportPath))
                {
                    m_aabExportPath = EditorUtility.OpenFolderPanel("选择AAB导出位置", Application.dataPath, "");
                }
                else
                {
                    m_aabExportPath = EditorUtility.OpenFolderPanel("选择AAB导出位置", m_aabExportPath, "");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("APKS导出位置:", m_apksExportPath);
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                if (string.IsNullOrEmpty(m_apksExportPath))
                {
                    m_apksExportPath = EditorUtility.OpenFolderPanel("选择AAB导出位置", Application.dataPath, "");
                }
                else
                {
                    m_apksExportPath = EditorUtility.OpenFolderPanel("选择AAB导出位置", m_apksExportPath, "");
                }
            }

            GUILayout.EndHorizontal();
            if (GUILayout.Button("一键导出UniversalAPK"))
            {
                BuildAABThenExportAPKSThenExportAPK(false);
            }
            if (GUILayout.Button("跳过AAB导出，导出UniversalAPK"))
            {
                ExportAPKSThenExportAPK();
            }
            if (GUILayout.Button("一键安装AAB到设备"))
            {
                BuildAABThenExportAPKSThenInstallToDevice();
            }
            if (GUILayout.Button("跳过AAB导出，安装AAB到设备"))
            {
                ExportAPKSThenInstallToDevice();
            }

            GUILayout.EndScrollView();
        }

        private void BuildAABThenExportAPKSThenExportAPK(bool batchMode)
        {
            var isBatchMode = ToolsUtility.IsBatchMode || batchMode;
            string aabPathWithoutExtension;
            string apksPathWithoutExtension;
            InitPaths(out aabPathWithoutExtension, out apksPathWithoutExtension);
            if (!isBatchMode)
            {
                EditorUtility.DisplayProgressBar("Running", "Running build aab", 0);
            }
            if (!BuildAAB(m_aabExportPath))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
            if (!isBatchMode)
            {
                EditorUtility.DisplayProgressBar("Running", "Running export apks", 0.3f);
            }
            var aabPath = aabPathWithoutExtension + ".aab";
            var apksPath = apksPathWithoutExtension + ".apks"; ;
            ExportAPKSThenExportAPK(aabPath, apksPath, () =>
            {
                if (!isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("Running", "Running export universal apk", 0.6f);
                }
            });
            if (!isBatchMode)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Finish", "build successed", "ok");
            }
            Debug.LogFormat("BuildAABThenExportAPKSThenExportAPK finished");
        }

        private void ExportAPKSThenExportAPK(string aabPath = null, string apksPath = null, System.Action onExportUniversalAPKSFinished = null)
        {
            if (string.IsNullOrEmpty(aabPath))
            {
                string aabPathWithoutExtension;
                string apksPathWithoutExtension;
                InitPaths(out aabPathWithoutExtension, out apksPathWithoutExtension);
                aabPath = aabPathWithoutExtension + ".aab";
                apksPath = apksPathWithoutExtension + ".apks"; ;
            }
            AABUtility.ExportUniversalAPKS(apksPath, aabPath);
            onExportUniversalAPKSFinished?.Invoke();
            AABUtility.ExportUniversalAPK(apksPath);
        }

        private void BuildAABThenExportAPKSThenInstallToDevice()
        {
            try
            {
                string aabPathWithoutExtension;
                string apksPathWithoutExtension;
                InitPaths(out aabPathWithoutExtension, out apksPathWithoutExtension);
                EditorUtility.DisplayProgressBar("Running", "BuildAABThenExportAPKSThenInstallToDevice Begin", 0);
                if (!BuildAAB(m_aabExportPath))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                EditorUtility.DisplayProgressBar("Running", "BuildAAB finished, start export apks", 0.3f);
                var aabPath = aabPathWithoutExtension + ".aab";
                var apksPath = apksPathWithoutExtension + ".apks"; ;
                ExportAPKSThenInstallToDevice(aabPath, apksPath, () =>
                {
                    EditorUtility.DisplayProgressBar("Running", "export apks finished start export universal apk", 0.6f);
                });
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Finish", "build successed", "ok");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                //交给外部去处理
                throw e;
            }
        }

        private void ExportAPKSThenInstallToDevice(string aabPath = null, string apksPath = null, System.Action onExportAPKSFinished = null)
        {
            if (string.IsNullOrEmpty(aabPath))
            {
                string aabPathWithoutExtension;
                string apksPathWithoutExtension;
                InitPaths(out aabPathWithoutExtension, out apksPathWithoutExtension);
                aabPath = aabPathWithoutExtension + ".aab";
                apksPath = apksPathWithoutExtension + ".apks"; ;
            }
            AABUtility.ExportAPKS(apksPath, aabPath);
            onExportAPKSFinished?.Invoke();
            AABUtility.InstallToDevice(apksPath, aabPath);
        }

        private void InitPaths(out string aabPathWithoutExtension, out string apksPathWithoutExtension)
        {
            var packageName = "AndroidProject";
            packageName = m_properties.TryGetString(GlobalBuild.CmdArgsKey.PACKAGE_NAME, packageName);
            aabPathWithoutExtension = Path.Combine(m_aabExportPath, packageName);
            apksPathWithoutExtension = Path.Combine(m_apksExportPath, packageName);
        }

        private bool BuildAAB(string aabPath)
        {
            CommandLineArgs.Initialize(m_proBuildSettingFileName[m_proBuildSettingIndex]);
            CommandLineArgs.Add(GlobalBuild.CmdArgsKey.OUTPUT_PATH, aabPath);
            return BuildScript.BuildPlayerInGraphicMode();
        }


        private void GetProSettingFileName()
        {
            m_proBuildSettingIndex = 0;
            string path = GlobalBuild.BuildConst.ZEUS_BUILD_PATH_CONFIG;
            string[] subFilePath = Directory.GetFiles(path);
            List<string> fileNameList = new List<string>();
            foreach (string filePath in subFilePath)
            {
                var extension = Path.GetExtension(filePath);
                if (extension == (PropertiesExtension))
                {
                    var properties = new Properties(filePath);
                    var exportAAB = false;
                    if (properties.TryGetBool(GlobalBuild.CmdArgsKey.EXPORT_AAB, ref exportAAB) && exportAAB)
                    {
                        m_proBuildSettingPath.Add(filePath);
                        fileNameList.Add(Path.GetFileNameWithoutExtension(filePath));
                    }
                }
            }
            m_proBuildSettingFileName = fileNameList.ToArray();
        }

        private void LoadPropertiesSetting()
        {
            if (m_proBuildSettingPath.Count > m_proBuildSettingIndex)
            {
                if (m_properties == null)
                {
                    m_properties = new Properties(m_proBuildSettingPath[m_proBuildSettingIndex]);
                }
                else
                {
                    m_properties.Load(m_proBuildSettingPath[m_proBuildSettingIndex]);
                }
            }
            else
            {
                Debug.LogWarning("_proBuildSettingIndex is out of bound!");
                if (m_proBuildSettingPath.Count > 0)
                {
                    m_proBuildSettingIndex = 0;
                    LoadPropertiesSetting();
                }
            }
        }
    }
}

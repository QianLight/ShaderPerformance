using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Devops.Core
{

    public class DevopsCoreConfigWindow : EditorWindow
    {
        static DevopsCoreConfig devopsCoreConfig;
        static DevopsCoreConfigWindow window;
        [MenuItem("Devops/DevopsCoreConfig")]
        static void ShowWindow()
        {
            window = (DevopsCoreConfigWindow)EditorWindow.GetWindow(typeof(DevopsCoreConfigWindow));
            window.titleContent = new GUIContent("DevopsCoreConfig");
            window.maxSize = new Vector2(400, 300);
            window.minSize = new Vector2(400, 300);
            window.Show();
            window.Init();
        }

        string DevopsIpPort = string.Empty;
        string VersionId = string.Empty;

        void Init()
        {
            CreateConfig.CreateCoreConfig();
            devopsCoreConfig = AssetDatabase.LoadAssetAtPath<DevopsCoreConfig>(DevopsCoreDefine.DevopsCoreConfigPath);
            DevopsIpPort = devopsCoreConfig.DevopsIpPort;
            VersionId = devopsCoreConfig.VersionId;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("DevopsIpAndPort");
            DevopsIpPort = GUILayout.TextField(DevopsIpPort);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("VersionId");
            VersionId = GUILayout.TextField(VersionId);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("保存"))
            {
                Save();
            }

            GUILayout.EndVertical();
        }

        void Save()
        {
            devopsCoreConfig.DevopsIpPort = DevopsIpPort;
            devopsCoreConfig.VersionId = VersionId;
            EditorUtility.SetDirty(devopsCoreConfig);
            AssetDatabase.SaveAssets();
            EditorDevopsInfoSettings.SetDataDirty();
            window.Close();
        }
    }

}
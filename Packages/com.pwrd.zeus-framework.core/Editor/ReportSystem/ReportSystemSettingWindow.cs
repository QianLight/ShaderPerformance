/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using UnityEditor;
using UnityEngine;

namespace Zeus.Core.ReportSystem
{
    internal class ReportSystemSettingWindow : EditorWindow
    {
        private ZeusReportConf _reportConf;

        [MenuItem("Zeus/Core/ReportSystem")]
        private static void Open()
        {
            var window = GetWindow<ReportSystemSettingWindow>();
            window.Show();
        }

        private void OnEnable()
        {
            titleContent.text = "Report System Settings";
            _reportConf = ReportUtil.LoadConf();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            _reportConf.enable = EditorGUILayout.Toggle("Enable", _reportConf.enable);
            _reportConf.encryptData = EditorGUILayout.Toggle("Encrypt Data", _reportConf.encryptData);
            _reportConf.uploadInterval = EditorGUILayout.IntField("Upload Interval (sec)", _reportConf.uploadInterval);
            _reportConf.uploadSize = EditorGUILayout.IntField("Upload Size", _reportConf.uploadSize);
            _reportConf.endPoint = EditorGUILayout.TextField("Endpoint", _reportConf.endPoint);
            _reportConf.projectName = EditorGUILayout.TextField("Project Name", _reportConf.projectName);
            _reportConf.channelName = EditorGUILayout.TextField("Channel Name", _reportConf.channelName);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Banned stores");

            var stores = _reportConf.bannedStores;
            for (int i = 0; i < stores.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                stores[i] = EditorGUILayout.TextArea(stores[i]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    stores.RemoveAt(i--);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add a new banned store"))
            {
                stores.Add("");
            }

            EditorGUILayout.Space();


            if (GUILayout.Button("Save"))
            {
                ReportUtil.SaveConf(_reportConf);
            }
        }
    }

}

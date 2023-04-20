/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using UnityEditor;

namespace Zeus.Framework.Hotfix
{
    public class HotfixLocalConfigSettingWindow : EditorWindow
    {
        ResVersionData _resVersionData;
        HotfixLocalConfig _localConfig;
        Vector2 vec = Vector2.zero;

        private void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            _localConfig = HotfixLocalConfigHelper.LoadLocalConfig();
            _resVersionData = ResVersionData.Load();
        }

        [MenuItem("Zeus/Hotfix/Setting", false, 1)]
        public static void Open()
        {
            HotfixLocalConfigSettingWindow localConfigWindow = GetWindow<HotfixLocalConfigSettingWindow>("LocalConfig");
            localConfigWindow.Show();
            localConfigWindow.Focus();
        }

        private void OnGUI()
        {
            vec = GUILayout.BeginScrollView(vec);
            EditorGUILayout.LabelField("ServerUrl");
            for (int i = 0; i < _localConfig.serverUrls.Count; i++)
            {
                GUILayout.BeginHorizontal();
                _localConfig.serverUrls[i] = EditorGUILayout.TextField("Url " + i + ":", _localConfig.serverUrls[i]);
                if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    _localConfig.serverUrls.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("new url"))
            {
                _localConfig.serverUrls.Add(string.Empty);
            }

            if (GUILayout.Button("clear url"))
            {
                _localConfig.serverUrls.Clear();
            }
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("独立的控制文件地址");
            _localConfig.independentControlDataUrl = EditorGUILayout.Toggle(_localConfig.independentControlDataUrl);
            EditorGUILayout.EndHorizontal();
            if (_localConfig.independentControlDataUrl)
            {
                for (int i = 0; i < _localConfig.controlDataUrls.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    _localConfig.controlDataUrls[i] = EditorGUILayout.TextField("Url " + i + ":", _localConfig.controlDataUrls[i]);
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        _localConfig.controlDataUrls.RemoveAt(i);
                        i--;
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("new url"))
                {
                    _localConfig.controlDataUrls.Add(string.Empty);
                }

                if (GUILayout.Button("clear url"))
                {
                    _localConfig.controlDataUrls.Clear();
                }
            }
            EditorGUILayout.Space();

            _localConfig.ChannelId = EditorGUILayout.TextField("ChannelId", _localConfig.ChannelId);
            _localConfig.Version = EditorGUILayout.TextField("Version", _localConfig.Version);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("OpenHotfix");
            _localConfig.openHotfix = EditorGUILayout.Toggle(_localConfig.openHotfix);
            EditorGUILayout.EndHorizontal();

            if (_localConfig.openHotfix)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("OpenHotfix(Editor)");
                bool isOpen = PlayerPrefs.GetInt("OpenHotfixEditor", 0) == 1;
                bool newIsOpen = EditorGUILayout.Toggle(isOpen);
                if (!isOpen.Equals(newIsOpen))
                {
                    PlayerPrefs.SetInt("OpenHotfixEditor", newIsOpen ? 1 : 0);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AutoPreDownload");
            _localConfig.autoPreDownload = EditorGUILayout.Toggle(_localConfig.autoPreDownload);
            EditorGUILayout.EndHorizontal();

            if (_localConfig.autoPreDownload)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("AllowPreDownloadOnCarrierDataNetwork");
                _localConfig.allowPreDownloadOnCarrierDataNetwork = EditorGUILayout.Toggle(_localConfig.allowPreDownloadOnCarrierDataNetwork);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("PreDownloadSpeedLimit(kb/s)");
                _localConfig.preDownloadSpeedLimit = EditorGUILayout.IntSlider(_localConfig.preDownloadSpeedLimit, 1, 10240);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UrlRefreshParam(minutes)");
            _localConfig.urlRefreshParam = EditorGUILayout.IntPopup(_localConfig.urlRefreshParam,
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" }, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Test Mode");
            _localConfig.testMode = (HotfixLocalConfig.HotfixTestMode)EditorGUILayout.EnumPopup(_localConfig.testMode);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当此版本的安装包未上传到后台，进入游戏时是否跳过热更");
            _localConfig.ignoreInit404Error = EditorGUILayout.Toggle(_localConfig.ignoreInit404Error);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当商店更新地址为空时是否跳过商店更新");
            _localConfig.ignoreEmptyAppStoreUrl = EditorGUILayout.Toggle(_localConfig.ignoreEmptyAppStoreUrl);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("使用实体文件存储包外版本（文件存储或PlayerPrefs存储）");
            _localConfig.saveOuterPackageVersionInFile = EditorGUILayout.Toggle(_localConfig.saveOuterPackageVersionInFile);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("加密包外版本信息");
            _localConfig.encodeOuterPackageVersion = EditorGUILayout.Toggle(_localConfig.encodeOuterPackageVersion);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前包外版本号(本设置仅用于编辑器模式)");
            string newResVersion = EditorGUILayout.TextField(_resVersionData.ResVersion);
            if (!newResVersion.Equals(_resVersionData.ResVersion))
            {
                _resVersionData.ResVersion = newResVersion;
            }
            if (GUILayout.Button("重置"))
            {
                _resVersionData.ResVersion = _localConfig.Version;
                GUI.FocusControl(null);
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("重置包外版本后需点击下方保存按钮才能生效", MessageType.Warning);

            EditorGUILayout.Space();
            if (GUILayout.Button("Save"))
            {
                int newVerInt;
                if (!int.TryParse(_localConfig.ver, out newVerInt) || newVerInt <= 0)
                {
                    EditorUtility.DisplayDialog("Warning", "Version number must be a positive integer!", "ok");
                }
                else if(string.IsNullOrEmpty(_resVersionData.ResVersion))
                {
                    EditorUtility.DisplayDialog("Warning", "包外版本不能为空!请重新设置包外版本。", "ok");
                }
                else
                {
                    HotfixLocalConfigHelper.SaveLocalConfig(_localConfig);
                    ResVersionData.InitHotfixLocalConfigSettingValue(_localConfig);
                    _resVersionData.Save();
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Hotfix Background Website"))
            {
                Application.OpenURL("http://10.5.32.129:9002/login");
            }

            GUILayout.EndScrollView();
        }
    }
}

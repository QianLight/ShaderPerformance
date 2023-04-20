/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Asset
{
    class ResourceSettingWindow : EditorWindow
    {
#if UNITY_EDITOR_WIN
        private static readonly string BuildBatName = "BuildSoftLink.bat";
        private static readonly string ClearBatName = "ClearSoftLink.bat";
        private static readonly string Command_CreateLinkPrefix = "mklink /D";
        private static readonly string Command_CreateLink = "mklink /D  \"{0}\" \"{1}\"";
        private static readonly string Command_RemoveLink = "rmdir /s/q {0}";
#elif UNITY_EDITOR_OSX
        private static readonly string BuildBatName_OSX = "BuildSoftLink.sh";
        private static readonly string ClearBatName_OSX = "ClearSoftLink.sh";
        private static readonly string Command_CreateLinkPrefix_OSX = "ln -s";
        private static readonly string Command_CreateLink_OSX = "ln -s {0} {1}";
        private static readonly string Command_RemoveLink_OSX = "rm ";
#endif

        AssetManagerSetting _setting;
        static string ResProjectStoreKey;
        static string MainProjectStoreKey;
        ResourcesEditorSetting _resourcesEditorSetting = new ResourcesEditorSetting();
        string[] _assetTags;
        int _tagIndex = 0;
        public void OnEnable()
        {
            Init();
        }

        private void Init()
        {
            LoadSetting();
            LoadResourcesEditorSetting();
            _assetTags = TagAsset.LoadTagAsset().TagListWithoutChild.ToArray();
        }

        private void LoadSetting()
        {
            _setting = AssetManagerSetting.LoadSetting();
        }

        private void LoadResourcesEditorSetting()
        {
            ResProjectStoreKey = Application.dataPath + "/ResProjectStorePath";
            MainProjectStoreKey = Application.dataPath + "/MainProjectStorePath";

            _resourcesEditorSetting.ResProjectPath = PlayerPrefs.GetString(ResProjectStoreKey, "");
            _resourcesEditorSetting.MainProjectPath = PlayerPrefs.GetString(MainProjectStoreKey, "");
        }

        [MenuItem("Zeus/Asset/AssetSetting", false, 1)]
        public static void Open()
        {
            ResourceSettingWindow window = GetWindow<ResourceSettingWindow>("AssetSetting", true);
            window.Show();
        }

        bool _showScenes = false;
        Vector2 vec = Vector2.zero;
        private void OnGUI()
        {
            EditorGUILayout.LabelField("选择加载资源的方式");
            _setting.assetLoaderType = (AssetLoaderType)EditorGUILayout.EnumPopup(_setting.assetLoaderType);
            EditorGUILayout.Space();
            if(_setting.assetLoaderType != AssetLoaderType.AssetDatabase)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("自动去除资源路径扩展名", GUILayout.Width(150));
                _setting.removeFileExtension = EditorGUILayout.Toggle(_setting.removeFileExtension);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("参数：");
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("资源异常Log输出等级:");
                _setting.bundleLoaderSetting.assetAbsenceLogLevel = (AssetAbsenceLogLevel)EditorGUILayout.EnumPopup(_setting.bundleLoaderSetting.assetAbsenceLogLevel);
                EditorGUILayout.Space();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("选择AssetBundle资源目录");
                EditorGUILayout.BeginHorizontal();
                _setting.bundleLoaderSetting.resourcesRootFolder = EditorGUILayout.TextField("Bundle Resources Folder", _setting.bundleLoaderSetting.resourcesRootFolder).Replace("\\", "/");
                if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    string temp = EditorUtility.OpenFolderPanel("Bundle Resources Folder", Application.dataPath, "Resources");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        string formatTemp = temp.Replace("\\", "/");
                        string relativeDir = formatTemp.Replace(Application.dataPath.Replace("\\", "/") + "/", "");
                        if (relativeDir == formatTemp)
                        {
                            EditorUtility.DisplayDialog("Error", $"Please choose a path under assets path \"{Application.dataPath}\"", "Ok");
                        }
                        else
                        {
                            _setting.bundleLoaderSetting.resourcesRootFolder = relativeDir;
                        }
                    }
                }
                if(_setting.bundleLoaderSetting.resourcesRootFolder.EndsWith("/"))
                {
                    //去掉末尾的分隔符
                    int count = _setting.bundleLoaderSetting.resourcesRootFolder.Length - 1;
                    _setting.bundleLoaderSetting.resourcesRootFolder = _setting.bundleLoaderSetting.resourcesRootFolder.Substring(0, count);
                }
                EditorGUILayout.LabelField(" ", GUILayout.Width(3));
                _setting.bundleLoaderSetting.isRemoveResourcesRootFolder = EditorGUILayout.Toggle(_setting.bundleLoaderSetting.isRemoveResourcesRootFolder, GUILayout.Width(15));
                EditorGUILayout.LabelField(new GUIContent("打包时是否移除此目录", "打包过程中将当前所选择的资源目录移除，以防当其名称为Resources时内部资源被打入包内，打包结束后还原回来"), GUILayout.Width(125));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("MaxGCNumPerFrame:", GUILayout.Width(300));
                _setting.bundleLoaderSetting.defaultBundleCollectorSetting.maxGcNumPerFrame = EditorGUILayout.IntField(_setting.bundleLoaderSetting.defaultBundleCollectorSetting.maxGcNumPerFrame);
                EditorGUILayout.EndHorizontal();
                _setting.bundleLoaderSetting.fullAsyncCallback = EditorGUILayout.Toggle("强制异步回调：", _setting.bundleLoaderSetting.fullAsyncCallback);
                _setting.bundleLoaderSetting.useBundleScatter = EditorGUILayout.Toggle("启用Bundle分散：", _setting.bundleLoaderSetting.useBundleScatter);
                EditorGUILayout.BeginHorizontal();
                _setting.bundleLoaderSetting.enableAssetLevel = EditorGUILayout.Toggle("启用资源分级：", _setting.bundleLoaderSetting.enableAssetLevel);
                if (_setting.bundleLoaderSetting.enableAssetLevel && _setting.bundleLoaderSetting.useSubPackage)
                {
                    _setting.bundleLoaderSetting.enableAssetLevel_GenerateAll = EditorGUILayout.Toggle("二包资源支持混合等级：", _setting.bundleLoaderSetting.enableAssetLevel_GenerateAll);
                }
                EditorGUILayout.EndHorizontal();

                _setting.bundleLoaderSetting.useSubPackage = EditorGUILayout.Toggle("启用分包功能:", _setting.bundleLoaderSetting.useSubPackage);
                
                if (_setting.bundleLoaderSetting.useSubPackage)
                {
                    _setting.bundleLoaderSetting.isCarrierDataNetworkDownloadAllowed = EditorGUILayout.Toggle("启用流量下载", _setting.bundleLoaderSetting.isCarrierDataNetworkDownloadAllowed);
                    _setting.bundleLoaderSetting.isSupportBackgroundDownload = EditorGUILayout.Toggle(new GUIContent("支持后台下载", "是否支持后台下载，false会在打包的时候剔除保活模块，主要指的是构建阶段"), _setting.bundleLoaderSetting.isSupportBackgroundDownload);
                    if (_setting.bundleLoaderSetting.isSupportBackgroundDownload)
                    {
                        _setting.bundleLoaderSetting.isBackgroundDownloadAllowed = EditorGUILayout.Toggle(new GUIContent("启用后台下载", "是否允许后台下载，主要指运行时，需要在构建阶段开启'支持后台下载'才能生效，否则无意义"), _setting.bundleLoaderSetting.isBackgroundDownloadAllowed);
                    }
                    _setting.bundleLoaderSetting.mode = (SubpackageMode)EditorGUILayout.EnumPopup("分包模式", _setting.bundleLoaderSetting.mode);
                    EditorGUILayout.LabelField("Subpackage Server URL:");
                    for (int i = 0; i < _setting.bundleLoaderSetting.remoteURL.Count; i++)
                    {
                        string url = _setting.bundleLoaderSetting.remoteURL[i];
                        if (url == null) continue;
                        EditorGUILayout.BeginHorizontal();
                        _setting.bundleLoaderSetting.remoteURL[i] = EditorGUILayout.TextField("URL", _setting.bundleLoaderSetting.remoteURL[i]);
                        if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            _setting.bundleLoaderSetting.remoteURL[i] = null;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.Separator();
                    if (GUILayout.Button("Add URL"))
                    {
                        string url = "";
                        _setting.bundleLoaderSetting.remoteURL.Add(url);
                    }
                    EditorGUILayout.BeginHorizontal();
                    _setting.bundleLoaderSetting.AssetBundleChunkSizeInMB = EditorGUILayout.IntField("AssetBundleChunk大小", _setting.bundleLoaderSetting.AssetBundleChunkSizeInMB);
                    EditorGUILayout.LabelField("MB");
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    _setting.bundleLoaderSetting.isFillFirstPackageAndroid = EditorGUILayout.Toggle("开启Android首包扩充", _setting.bundleLoaderSetting.isFillFirstPackageAndroid);
                    if (_setting.bundleLoaderSetting.isFillFirstPackageAndroid)
                    {
                        EditorGUILayout.LabelField("扩充至",GUILayout.Width(42));
                        _setting.bundleLoaderSetting.TargetAndroidAssetSizeInMB = EditorGUILayout.IntField(_setting.bundleLoaderSetting.TargetAndroidAssetSizeInMB,GUILayout.Width(70));
                        EditorGUILayout.LabelField("MB");
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    _setting.bundleLoaderSetting.isFillFirstPackageiOS = EditorGUILayout.Toggle("开启iOS首包扩充", _setting.bundleLoaderSetting.isFillFirstPackageiOS);
                    if (_setting.bundleLoaderSetting.isFillFirstPackageiOS)
                    {
                        EditorGUILayout.LabelField("扩充至",GUILayout.Width(42));
                        _setting.bundleLoaderSetting.TargetiOSAssetSizeInMB = EditorGUILayout.IntField(_setting.bundleLoaderSetting.TargetiOSAssetSizeInMB,GUILayout.Width(70));
                        EditorGUILayout.LabelField("MB");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Separator();
                _showScenes = EditorGUILayout.Foldout(_showScenes, "打入包内的场景设置");
                if (_showScenes)
                {
                    vec = GUILayout.BeginScrollView(vec);
                    for (int i = 0; i < _setting.bundleLoaderSetting.ScenesInBuild.Count; ++i)
                    {
                        string scenePath = _setting.bundleLoaderSetting.ScenesInBuild[i];
                        if (scenePath == null)
                        {
                            continue;
                        }
                        EditorGUILayout.BeginHorizontal();
                        _setting.bundleLoaderSetting.ScenesInBuild[i] = EditorGUILayout.TextField(scenePath, GUILayout.Height(20));
                        SceneAsset sceneAsset = EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<SceneAsset>(_setting.bundleLoaderSetting.ScenesInBuild[i]), typeof(SceneAsset), true, GUILayout.Width(200)) as SceneAsset;
                        if (sceneAsset != null)
                        {
                            _setting.bundleLoaderSetting.ScenesInBuild[i] = AssetDatabase.GetAssetPath(sceneAsset);
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            _setting.bundleLoaderSetting.ScenesInBuild[i] = null;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                    if (GUILayout.Button("Add Scene"))
                    {
                        _setting.bundleLoaderSetting.ScenesInBuild.Add(string.Empty);
                    }
                }
            }
            else if (_setting.assetLoaderType == AssetLoaderType.Resources)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("MaxLoadingNum:", GUILayout.Width(300));
                _setting.resourceLoaderSetting.MaxLoadingNum = EditorGUILayout.IntField(_setting.resourceLoaderSetting.MaxLoadingNum);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("添加场景资源路径前缀:", GUILayout.Width(130));
                _setting.resourceLoaderSetting.AddScenePathPrefix = EditorGUILayout.Toggle(_setting.resourceLoaderSetting.AddScenePathPrefix);
                _setting.resourceLoaderSetting.ScenePathPrefix = EditorGUILayout.TextField(_setting.resourceLoaderSetting.ScenePathPrefix);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("添加资源路径前缀:", GUILayout.Width(130));
                _setting.assetDatabaseSetting.AddAssetPathPrefix = EditorGUILayout.Toggle(_setting.assetDatabaseSetting.AddAssetPathPrefix);
                _setting.assetDatabaseSetting.AssetPathPrefix = EditorGUILayout.TextField(_setting.assetDatabaseSetting.AssetPathPrefix);
                EditorGUILayout.EndHorizontal();
                _setting.assetDatabaseSetting.enableAssetLevel = EditorGUILayout.Toggle("启用资源分级:", _setting.assetDatabaseSetting.enableAssetLevel);
            }

            if (GUILayout.Button("SaveSetting"))
            {
                AssetManagerSetting.SaveSetting(_setting);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择ResProject和MainProject对应的目录，点击Build SoftLink生成软链接资源");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ResProject TargetPath:", GUILayout.Width(200));
            _resourcesEditorSetting.ResProjectPath = EditorGUILayout.TextField(_resourcesEditorSetting.ResProjectPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string resProjectTargetPath = EditorUtility.OpenFolderPanel("Load The ResProject TargetPath", Application.dataPath, "ResProject");
                if (!string.IsNullOrEmpty(resProjectTargetPath))
                {
                    PlayerPrefs.SetString(ResProjectStoreKey, resProjectTargetPath);
                    _resourcesEditorSetting.ResProjectPath = resProjectTargetPath;
                    _resourcesEditorSetting.ResProjectPath = EditorGUILayout.TextField(_resourcesEditorSetting.ResProjectPath);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MainProject TargetPath:", GUILayout.Width(200));
            _resourcesEditorSetting.MainProjectPath = EditorGUILayout.TextField(_resourcesEditorSetting.MainProjectPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string mainProjectTargetPath = EditorUtility.OpenFolderPanel("Load The MainProject TargetPath", Application.dataPath, "MainProject");
                if (!string.IsNullOrEmpty(mainProjectTargetPath))
                {
                    PlayerPrefs.SetString(MainProjectStoreKey, mainProjectTargetPath);
                    _resourcesEditorSetting.MainProjectPath = mainProjectTargetPath;
                    _resourcesEditorSetting.MainProjectPath = EditorGUILayout.TextField(_resourcesEditorSetting.MainProjectPath);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Build SoftLink"))
            {
#if UNITY_EDITOR_WIN
                Link_win();
#elif UNITY_EDITOR_OSX
		        Link_macos();	
#endif
            }
        }

#if UNITY_EDITOR_WIN
        private void MakeClearBat_win()
        {
            StringBuilder stringBuilder = new StringBuilder();
            string newMainPath = _resourcesEditorSetting.MainProjectPath.Replace("/", "\\");
            stringBuilder.AppendLine(string.Format(Command_RemoveLink, newMainPath));
            MakeBatByAdmin(ClearBatName, stringBuilder.ToString());
        }

        private void Link_win()
        {
            if (!Directory.Exists(_resourcesEditorSetting.ResProjectPath))
            {
                Debug.LogError(string.Format("{0} does not exist", _resourcesEditorSetting.ResProjectPath));
                return;
            }

            if (Directory.Exists(_resourcesEditorSetting.MainProjectPath))
            {
                if (!EditorUtility.DisplayDialog("警告", string.Format("{0} 已存在，软连需要删除该文件夹，是否继续", _resourcesEditorSetting.MainProjectPath), "好，继续软连", "我想先处理一下，取消软连"))
                    return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            string newMainPath = _resourcesEditorSetting.MainProjectPath.Replace("/", "\\");
            string newResPath = _resourcesEditorSetting.ResProjectPath.Replace("/", "\\");

            stringBuilder.AppendLine(string.Format("if exist \"{0}\" ( rmdir /s/q \"{0}\")", newMainPath));
            stringBuilder.AppendLine(string.Format(Command_CreateLink, newMainPath, newResPath));

            MakeBatByAdmin(BuildBatName, stringBuilder.ToString());
            MakeClearBat_win();
            EditorUtil.RunBat(BuildBatName, "", Application.persistentDataPath);
            ForceCompile();
        }

        private void MakeBatByAdmin(string batFile, string customCommands)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("@echo off");
            stringBuilder.AppendLine(">nul 2>&1 \"%SYSTEMROOT%\\system32\\cacls.exe\" \"%SYSTEMROOT%\\system32\\config\\system\"");
            stringBuilder.AppendLine("if '%errorlevel%' NEQ '0' (");
            stringBuilder.AppendLine("goto UACPrompt");
            stringBuilder.AppendLine(") else ( goto gotAdmin )");
            stringBuilder.AppendLine(":UACPrompt");
            stringBuilder.AppendLine("echo Set UAC = CreateObject^(\"Shell.Application\"^) > \"%temp%\\getadmin.vbs\"");
            stringBuilder.AppendLine("echo UAC.ShellExecute \"%~s0\", \"\", \"\", \"runas\", 1 >> \"%temp%\\getadmin.vbs\"");
            stringBuilder.AppendLine("\"%temp%\\getadmin.vbs\"");
            stringBuilder.AppendLine("exit /B");
            stringBuilder.AppendLine(":gotAdmin");
            stringBuilder.AppendLine("if exist \"%temp%\\getadmin.vbs\" ( del \"%temp%\\getadmin.vbs\")");
            stringBuilder.AppendLine(":customCMD");

            stringBuilder.AppendLine(customCommands);
            stringBuilder.AppendLine("pause");

            string batPath = Application.persistentDataPath + "/" + batFile;
            if (File.Exists(batPath))
            {
                Debug.Log("delete " + batPath);
                File.Delete(batPath);
            }
            File.WriteAllText(batPath, stringBuilder.ToString(), System.Text.Encoding.UTF8);
        }
#endif

#if UNITY_EDITOR_OSX
        private void Link_macos()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("echo \"Start ============BuildSoftLink.sh\"");
            if (Directory.Exists(_resourcesEditorSetting.ResProjectPath))
            {
                    string resPath = _resourcesEditorSetting.ResProjectPath;
                    string destPath = _resourcesEditorSetting.MainProjectPath;
                    stringBuilder.AppendLine(string.Format(Command_CreateLink_OSX, resPath, destPath));
            }
            RunBatByAdmin(BuildBatName_OSX, stringBuilder.ToString());
            ForceCompile();
        }

        private void RunBatByAdmin(string batFile, string customCommands)
        {
            File.WriteAllText(batFile, customCommands, System.Text.Encoding.UTF8);
            System.Diagnostics.Process.Start("/bin/sh", batFile);
        }
#endif
        private void ForceCompile()
        {
            //筛选出本类的相对路径，进行Project强制更新进行编译
            DirectoryInfo dataPathDir = new DirectoryInfo(Application.dataPath);
            System.Uri dataPathUri = new System.Uri(Application.dataPath);
            foreach (FileInfo file in dataPathDir.GetFiles(this.GetType().Name + ".cs", SearchOption.AllDirectories))
            {
                System.Uri relativeUri = dataPathUri.MakeRelativeUri(new System.Uri(file.FullName));
                string relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
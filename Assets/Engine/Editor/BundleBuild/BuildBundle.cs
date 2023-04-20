using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CFEngine.Editor
{
    [ExecuteInEditMode]
    public class BuildBundle : EditorWindow
    {
        protected BuildBundleConfigTool bbcTool;
        [MenuItem ("Tools/Build/BuidBundleConfig")]
        static void Init ()
        {
            EditorWindow.GetWindow (typeof (BuildBundle), true, "BuidBundleConfig");
        }
        public static void BuildAllAssetBundlesWithList (string path = "", bool quiet = true)
        {
            BuildBundleConfig.instance.BuildBundle ();
        }
        public void OnEnable ()
        {
            bbcTool = BuildBundleConfigTool.CreateInstance<BuildBundleConfigTool> ();
            bbcTool.OnInit ();
        }
        void OnGUI ()
        {
            if (bbcTool != null)
            {
                bbcTool.rect = this.position;
                bbcTool.DrawGUI (ref bbcTool.rect);
            }
        }
        void Update ()
        {
            if (bbcTool != null)
            {
                bbcTool.Update ();
            }
        }
    }

    public class BuildBundleConfigTool : BaseConfigTool<BuildBundleConfig>
    {

        public enum OpBundleConfigType
        {
            None,
            OpSave,
            OpSort,
            OpPreBuild,
            OpBuild,
            OpGenManifest,
            OpRePatch
        }
        private Vector2 configScroll = Vector2.zero;
        private float configHeight;
        private Vector2 buildResultScroll = Vector2.zero;
        private float buildResultHeight;
        private Vector2 prebuildScroll = Vector2.zero;
        private float prebuildHeight;
        private Vector2 outpackageScroll = Vector2.zero;

        private Vector2 bytesResultScroll = Vector2.zero;

        private OpBundleConfigType opBundleType = OpBundleConfigType.None;
        private BuildType buildType = BuildType.TestBulld;
        private bool singleBundleBuild = false;
        private int testIndex = -1;
        public Rect rect;
        private List<PreBuildPreProcess> preProcess = new List<PreBuildPreProcess>();
        private string preBuildName = "";
        private bool preBuildFolder = true;
        private bool buildFolder = true;
        private bool outPackageFolder = true;
        public override void OnInit ()
        {
            base.OnInit ();
            config = BuildBundleConfig.instance;
            var types = EngineUtility.GetAssemblyType(typeof(PreBuildPreProcess));
            foreach (var t in types)
            {
                var process = Activator.CreateInstance(t) as PreBuildPreProcess;
                if (process != null)
                {
                    preProcess.Add(process);
                }
            }
            preProcess.Sort((x, y) => x.Priority - y.Priority);
        }

        public override void Update ()
        {
            switch (opBundleType)
            {
                case OpBundleConfigType.OpSave:
                    config.Save ();
                    break;
                case OpBundleConfigType.OpSort:
                    config.configs.Sort ((x, y) => x.dir.CompareTo (y.dir));
                    break;
                case OpBundleConfigType.OpPreBuild:
                    {
                        if (EditorUtility.DisplayDialog("PreBuildBundle", "Is Build? ", "OK", "Cancel"))
                        {
                            BuildBundleConfig.instance.BuildBundle(preBuildName, -1, BuildType.PreBuild, false, singleBundleBuild);
                            preBuildName = "";
                        }
                    }
                    break;
                case OpBundleConfigType.OpBuild:
                    {
                        if (EditorUtility.DisplayDialog ("BuildBundle", "Is Build? ", "OK", "Cancel"))
                        {
                            BuildBundleConfig.instance.BuildBundle ("",testIndex, buildType, false, singleBundleBuild);
                            testIndex = -1;
                        }
                    }
                    break;
                case OpBundleConfigType.OpGenManifest:
                    BuildBundleConfig.instance.GenManifestData();
                    break;
                case OpBundleConfigType.OpRePatch:
                    BuildBundleConfig.instance.RePatch();
                    break;
                    
            }
            opBundleType = OpBundleConfigType.None;
        }

        private void BundleConfigGUI ()
        {
            int bundleCount = BuildBundleConfig.instance.context.count;
            int bytesCount = PreBuildPreProcess.count;
            TwoOpInfo showHideAll = new TwoOpInfo();
            TwoOpInfo enableDisableAll = new TwoOpInfo();
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Save", GUILayout.MaxWidth (100)))
            {
                opBundleType = OpBundleConfigType.OpSave;
            }
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (100)))
            {
                config.configs.Add (new BundlePath ());
            }
            if (GUILayout.Button ("Sort", GUILayout.MaxWidth (100)))
            {
                opBundleType = OpBundleConfigType.OpSort;
            }
            showHideAll.OnButtonGUI ("ShowAll", "HideAll", 100);
            enableDisableAll.OnButtonGUI ("EnableAll", "DisabelAll", 100);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType",buildType, GUILayout.MaxWidth(300));
            singleBundleBuild = EditorGUILayout.Toggle("SingleABBuild", singleBundleBuild, GUILayout.MaxWidth(300));
            if (GUILayout.Button("Build", GUILayout.MaxWidth(100)))
            {
                opBundleType = OpBundleConfigType.OpBuild;
            }
            if (GUILayout.Button("GenManifest", GUILayout.MaxWidth(100)))
            {
                opBundleType = OpBundleConfigType.OpGenManifest;
            }
            if (GUILayout.Button("RePatch", GUILayout.MaxWidth(100)))
            {
                opBundleType = OpBundleConfigType.OpRePatch;
            }
            EditorGUILayout.EndHorizontal ();
            preBuildFolder = EditorGUILayout.Foldout(preBuildFolder, "PreBuild");
            if(preBuildFolder)
            {
                prebuildHeight = (preProcess.Count > 0) ? preProcess.Count * 22 : rect.height - 50;
                prebuildScroll = EditorGUILayout.BeginScrollView(prebuildScroll, GUILayout.MinHeight(prebuildHeight));
                for (int i = 0; i < preProcess.Count; ++i)
                {
                    var preBuild = preProcess[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(preBuild.GetType().Name, GUILayout.MaxWidth(200));
                    if (GUILayout.Button("Build", GUILayout.MaxWidth(80)))
                    {
                        preBuildName = preBuild.GetType().Name;
                        opBundleType = OpBundleConfigType.OpPreBuild;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }



            EditorGUILayout.Space();
            buildFolder = EditorGUILayout.Foldout(buildFolder, "BuildAB");
            if (buildFolder)
            {
                configHeight = (bundleCount > 0 || bytesCount > 0) ? 350 : rect.height - 50;
                configScroll = EditorGUILayout.BeginScrollView(configScroll, GUILayout.MinHeight(configHeight));
                int removeIndex = -1;
                for (int i = 0; i < config.configs.Count; ++i)
                {
                    var bp = config.configs[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("{0}.", i.ToString()), GUILayout.MaxWidth(20));
                    bp.enable = EditorGUILayout.Toggle("", bp.enable, GUILayout.MaxWidth(40));
                    if (enableDisableAll.NeedProcess())
                    {
                        enableDisableAll.Process(ref bp.enable);
                    }
                    ToolsUtility.FolderSelect(ref bp.dir);
                    EditorGUILayout.LabelField("Package", GUILayout.MaxWidth(60));
                    bp.assetBundleName = EditorGUILayout.TextField("", bp.assetBundleName, GUILayout.MaxWidth(160));
                    EditorGUILayout.LabelField(string.Format("Rules:{0}", bp.rules.Count.ToString()), GUILayout.MaxWidth(100));

                    string folderPath = bp.GetHash();
                    bool isFolder = config.folder.IsFolder(folderPath);
                    if (GUILayout.Button(isFolder ? "Hide" : "Show", GUILayout.MaxWidth(80)) ||
                        showHideAll.NeedProcess())
                    {
                        if (showHideAll.Process(ref isFolder))
                        {
                            isFolder = !isFolder;
                        }
                        config.folder.SetFolder(folderPath, isFolder);
                    }
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                    {
                        bp.rules.Add(new BundleRule());
                    }
                    if (GUILayout.Button("Build", GUILayout.MaxWidth(80)))
                    {
                        testIndex = i;
                        opBundleType = OpBundleConfigType.OpBuild;
                    }
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (isFolder)
                    {
                        EditorGUILayout.Space();
                        int ruleRemoveIndex = -1;
                        EditorGUI.indentLevel++;

                        for (int j = 0; j < bp.rules.Count; ++j)
                        {
                            var rule = bp.rules[j];
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(string.Format("R.{0}", j.ToString()), GUILayout.MaxWidth(40));
                            rule.path = EditorGUILayout.TextField("", rule.path, GUILayout.MaxWidth(300));
                            int index = 0;
                            EditorGUI.BeginChangeCheck();
                            index = EditorGUILayout.Popup("", index, AssetsConfig.extList, GUILayout.MaxWidth(160));
                            if (EditorGUI.EndChangeCheck())
                            {
                                rule.path = AssetsConfig.extList[index];
                            }
                            rule.op = (BundleType)EditorGUILayout.EnumPopup("", rule.op, GUILayout.MaxWidth(160));
                            rule.buildPlatform = (uint)(BuildPlatform)EditorGUILayout.EnumFlagsField((BuildPlatform)rule.buildPlatform, GUILayout.MaxWidth(160));
                            if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                            {
                                ruleRemoveIndex = j;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        if (ruleRemoveIndex >= 0)
                        {
                            bp.rules.RemoveAt(ruleRemoveIndex);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
                if (removeIndex >= 0)
                {
                    config.configs.RemoveAt(removeIndex);
                }
            }

            outPackageFolder = EditorGUILayout.Foldout(outPackageFolder, "AndroidOutPackageDir");
            if (outPackageFolder)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                {
                    config.outPackagePath.Add("");
                }
                EditorGUILayout.EndHorizontal();
                outpackageScroll = EditorGUILayout.BeginScrollView(outpackageScroll);
                int removeIndex = -1;
                for (int i = 0; i < config.outPackagePath.Count; ++i)
                {
                    var dir = config.outPackagePath[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    ToolsUtility.FolderSelect(ref dir);                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        config.outPackagePath[i] = dir;
                    }
                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                    {
                        removeIndex = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
                if (removeIndex >= 0)
                {
                    config.outPackagePath.RemoveAt(removeIndex);
                }
            }
        }

        private void BytesDataGUI()
        {
            int bytesCount = PreBuildPreProcess.count;
            if (bytesCount > 0)
            {
                TwoOpInfo showHideAll = new TwoOpInfo();
                var preProcess = BuildBundleConfig.instance.buildProcess;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(string.Format("----------------------BytesList({0})----------------------",
                    bytesCount.ToString()));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear", GUILayout.MaxWidth(100)))
                {
                    PreBuildPreProcess.count = 0;

                    for (int i = 0; i < preProcess.Count; ++i)
                    {
                        var process = preProcess[i];
                        process.Clear();
                    }
                }
                showHideAll.OnButtonGUI("ShowAll", "HideAll", 100);

                EditorGUILayout.EndHorizontal();
                bytesResultScroll = EditorGUILayout.BeginScrollView(bytesResultScroll, GUILayout.MinHeight(preProcess.Count * 22));
                for (int i = 0; i < preProcess.Count; ++i)
                {
                    var process = preProcess[i];
                    EditorGUILayout.BeginHorizontal();
                    process.folder = EditorGUILayout.Foldout(process.folder,
                        string.Format("Dir:{0}({1})", process.Name,
                            process.files.Count.ToString()));
                    EditorGUILayout.EndHorizontal();
                    if (showHideAll.NeedProcess())
                    {
                        showHideAll.Process(ref process.folder);
                    }
                    if (process.folder)
                    {
                        EditorGUI.indentLevel++;
                        for (int j = 0; j < process.files.Count; ++j)
                        {
                            var file = process.files[j];
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(file);
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void BundleDataGUI ()
        {
            int bundleCount = BuildBundleConfig.instance.context.count;
            if (bundleCount > 0)
            {
                bool showHide = false;
                bool isShow = false;
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (string.Format ("----------------------BundleList({0})----------------------",
                    bundleCount.ToString ()));
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Clear", GUILayout.MaxWidth (100)))
                {
                    BuildBundleConfig.instance.context.count = 0;

                    for (int i = 0; i < config.configs.Count; ++i)
                    {
                        var bp = config.configs[i];
                        bp.bundleList.Clear ();
                    }

                }
                if (GUILayout.Button ("ShowAll", GUILayout.MaxWidth (100))) { showHide = true; isShow = true; }
                if (GUILayout.Button ("HideAll", GUILayout.MaxWidth (100))) { showHide = true; isShow = false; }

                EditorGUILayout.EndHorizontal ();
                buildResultScroll = EditorGUILayout.BeginScrollView (buildResultScroll, GUILayout.MinHeight (rect.height / 2 - 80));

                for (int i = 0; i < config.configs.Count; ++i)
                {
                    var bp = config.configs[i];
                    EditorGUILayout.BeginHorizontal ();
                    bp.bundleFolder = EditorGUILayout.Foldout (bp.bundleFolder,
                        string.Format ("Name:{0}({1})", bp.dir, bp.bundleList.Count.ToString ()));
                    EditorGUILayout.EndHorizontal ();
                    if (showHide)
                    {
                        bp.bundleFolder = isShow;
                    }
                    if (bp.bundleFolder)
                    {
                        EditorGUI.indentLevel++;
                        for (int j = 0; j < bp.bundleList.Count; ++j)
                        {
                            var bundle = bp.bundleList[j];
                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.LabelField (string.Format ("Path({1}):{0}", bundle.assetBundleName, bundle.assetNames.Length.ToString ()));
                            EditorGUILayout.EndHorizontal ();
                            EditorGUI.indentLevel++;
                            for (int k = 0; k < bundle.assetNames.Length; ++k)
                            {
                                var name = bundle.assetNames[k];
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.LabelField (string.Format ("Asset:{0}", name));
                                EditorGUILayout.EndHorizontal ();
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.EndScrollView ();
            }
        }
        public override void DrawGUI (ref Rect rect)
        {
            BundleConfigGUI ();
            BytesDataGUI ();
            BundleDataGUI ();
        }
    }
}
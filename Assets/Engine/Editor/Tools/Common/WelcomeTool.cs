using CFEngine.SRP;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.Editor
{
    public class WelcomeTool : CommonToolTemplate
    {
        enum EOpType
        {
            None,
            RunTest,
        }
        private string search = "";
        private EOpType opType = EOpType.None;
        private Vector2 resScrollVector;
        private bool showResInfo = false;
        private Vector2 scrollVector2;
        private bool showStyle = false;
        public override void OnInit ()
        {
            base.OnInit ();
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        public override void DrawGUI (ref Rect rect)
        {
            if (!EngineContext.IsRunning)
            {
                DrawPipelineSetting();

                if (GUILayout.Button("ReleaseMemory"))
                {
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    Resources.UnloadUnusedAssets();
                }
                if (GUILayout.Button("TestEngine"))
                {
                    opType = EOpType.RunTest;
                }


                if (GUILayout.Button("ShowStyle"))
                {
                    showStyle = !showStyle;
                }
                if (showStyle)
                {
                    GUILayout.BeginHorizontal("HelpBox");
                    GUILayout.Space(30);
                    search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(rect.x / 3));
                    GUILayout.Label("", "SearchCancelButtonEmpty");
                    GUILayout.EndHorizontal();
                    scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
                    foreach (GUIStyle style in GUI.skin.customStyles)
                    {
                        if (style.name.ToLower().Contains(search.ToLower()))
                        {
                            DrawStyleItem(style);
                        }
                    }
                    GUILayout.EndScrollView();
                }

            }
            var resInfo = LoadMgr.singleton.GetResInfo();
            var objPool = LoadMgr.singleton.objPool;
            if (EditorCommon.BeginFolderGroup(string.Format("ResInfo({0}) ObjInPool:{1}", resInfo.Count.ToString(),
                objPool != null ? objPool.childCount.ToString() : "0"),
                ref showResInfo, rect.width - 10, 600))
            {                
                var it = resInfo.GetEnumerator();
                EditorCommon.BeginScroll(ref resScrollVector, resInfo.Count);
                while (it.MoveNext())
                {
                    var value = it.Current.Value;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(value.asset as UnityEngine.Object, typeof(UnityEngine.Object), false);
                    EditorGUILayout.LabelField(string.Format("path:{0} refCount:{1}", value.location, value.refCount.ToString()));
                    EditorGUILayout.EndHorizontal();
                }

                EditorCommon.EndScroll();
                EditorCommon.EndFolderGroup();
            }
        }

        private static void DrawPipelineSetting()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
            var newRP = (OPRenderPipeline)EditorGUILayout.EnumPopup("RenderPipeline", RenderPipelineManager.renderPipeline);
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                if (newRP == OPRenderPipeline.LegacySRP)
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<SparkRenderPipelineAsset>(
                        string.Format("{0}Config/SRPAsset.asset", LoadMgr.singleton.EngineResPath));
                    SymbolsManager.Set("PIPELINE_URP", false);
                }
                else if (newRP == OPRenderPipeline.Builtin)
                {
                    GraphicsSettings.renderPipelineAsset = null;
                    SymbolsManager.Set("PIPELINE_URP", false);
                }
                else
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Engine/Runtime/Shading/URPAsset.asset");
                    SymbolsManager.Set("PIPELINE_URP", true);
                }
            }
        }

        void DrawStyleItem (GUIStyle style)
        {
            GUILayout.BeginHorizontal ("box");
            GUILayout.Space (40);
            EditorGUILayout.SelectableLabel (style.name);
            GUILayout.FlexibleSpace ();
            EditorGUILayout.SelectableLabel (style.name, style);
            GUILayout.Space (40);
            EditorGUILayout.SelectableLabel ("", style, GUILayout.Height (40), GUILayout.Width (40));
            GUILayout.Space (50);
            if (GUILayout.Button ("Copy"))
            {
                TextEditor textEditor = new TextEditor ();
                textEditor.text = style.name;
                textEditor.OnFocus ();
                textEditor.Copy ();
            }
            GUILayout.EndHorizontal ();
            GUILayout.Space (10);
        }

        private void RunTest ()
        {
            EngineTest.singleton.Init ();
            SceneChunkLoadTest.testSceneNames.Enqueue ("OP_enieslobby_entrance");
            // string sceneListPath = string.Format ("{0}/SceneList.asset",
            //     AssetsConfig.instance.EngineResPath);
            // SceneList sceneListConfig = AssetDatabase.LoadAssetAtPath<SceneList> (sceneListPath);
            // if (sceneListConfig != null)
            // {
            //     for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
            //     {
            //         var scene = sceneListConfig.sceneList[i];
            //         if (scene != null && !scene.notBuild && scene.sceneAsset != null)
            //         {
            //             SceneChunkLoadTest.testSceneNames.Enqueue (scene.sceneAsset.name);
            //         }
            //     }
            // }

        }
        public override void Update ()
        {
            switch (opType)
            {
                case EOpType.RunTest:
                    RunTest ();
                    break;
            }
            opType = EOpType.None;
        }
    }
}
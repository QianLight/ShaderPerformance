using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class OutlineConfigTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpSaveConfig,
        }
        private OutlineConfigData editorOutlineConfig;
        private MiscConfig miscConfig;
        private Transform outlineRoot;
        private OpType opType = OpType.OpNone;
        private Vector2 prefabsScroll = Vector2.zero;
        public override void OnInit ()
        {
            base.OnInit ();
            string path = string.Format ("{0}/EditorOutlineConfig.asset", AssetsConfig.instance.EngineResPath);
            editorOutlineConfig = AssetDatabase.LoadAssetAtPath<OutlineConfigData> (path);
            if (editorOutlineConfig == null)
            {
                editorOutlineConfig = ScriptableObject.CreateInstance<OutlineConfigData> ();
                editorOutlineConfig = CommonAssets.CreateAsset<OutlineConfigData> (path, ".asset", editorOutlineConfig);
            }
            path = string.Format ("{0}Config/MiscConfig.asset", LoadMgr.singleton.EngineResPath);
            miscConfig = AssetDatabase.LoadAssetAtPath<MiscConfig> (path);
            if (miscConfig == null)
            {
                miscConfig = ScriptableObject.CreateInstance<MiscConfig> ();
                miscConfig = CommonAssets.CreateAsset<MiscConfig> (path, ".asset", miscConfig);
                WorldSystem.miscConfig = miscConfig;
            }
            GameObject rolesGo = GameObject.Find ("_Outline");
            if (rolesGo == null)
            {
                rolesGo = new GameObject ("_Outline");
            }
            outlineRoot = rolesGo.transform;
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        public override void DrawGizmos ()
        {
            if (miscConfig != null)
            {

                EngineContext context = EngineContext.instance;
                if (context != null)
                {
                    var color = Gizmos.color;
                    var camera = context.CameraTransCache;
                    var forward = camera.forward;
                    var pos0 = camera.position + forward * miscConfig.defaultDist.minDist;
                    var pos1 = camera.position + forward * miscConfig.defaultDist.maxDist;
                    var far = camera.position + forward * 100;
                    Gizmos.DrawLine (camera.position, far);
                    Gizmos.DrawSphere (pos0, 0.1f);
                    Gizmos.DrawSphere (pos1, 0.1f);
                    Gizmos.color = color;
                }

            }
        }

        public override void DrawGUI (ref Rect rect)
        {
            if (editorOutlineConfig != null)
            {
                PrefabListGUI ("0.PrefabList");
            }
        }

        private void PutAtPos (Transform t, float dist, float offsetY)
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                var camera = context.CameraTransCache;
                var forward = camera.forward;
                var pos = camera.position + forward * dist;
                pos.y += offsetY;
                t.position = pos;
            }
        }

        private float GetDistFade ()
        {
            EngineContext context = EngineContext.instance;
            if (editorOutlineConfig.testGo != null && context != null)
            {
                var config = editorOutlineConfig.testGo;
                var t = config.go.transform;
                if (t != null)
                {
                    var pos = t.position;
                    var cameraPos = context.CameraTransCache.position;
                    return miscConfig.CalcDistFadeScale(ref pos,ref cameraPos);
                }
            }
            return 1;
        }

        private void PrefabListGUI (string info)
        {
            editorOutlineConfig.configFolder = EditorGUILayout.Foldout (editorOutlineConfig.configFolder, info);
            if (!editorOutlineConfig.configFolder)
                return;
            EditorCommon.BeginGroup ("Prefabs");
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (80)))
            {
                editorOutlineConfig.configs.Add (new PrefabOutlineConfig ());
            }
            if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
            {
                opType = OpType.OpSaveConfig;
            }
            EditorGUILayout.EndHorizontal ();
            float height = 0;
            for (int i = 0; i < editorOutlineConfig.configs.Count; ++i)
            {
                var config = editorOutlineConfig.configs[i];
                height += 21;
                if (config.edit)
                {
                    height += 21 * 3 + config.partOutlineConfig.Count * 21;
                }
            }
            EditorCommon.BeginScroll (ref prefabsScroll, editorOutlineConfig.configs.Count, 10, height);
            int removeIndex = -1;
            for (int i = 0; i < editorOutlineConfig.configs.Count; ++i)
            {
                var config = editorOutlineConfig.configs[i];
                EditorGUILayout.BeginHorizontal ();
                config.prefab = EditorGUILayout.ObjectField (config.prefab, typeof (GameObject), false, GUILayout.MaxWidth (300)) as GameObject;
                if (GUILayout.Button (config.edit? "UnEdit": "Edit", GUILayout.MaxWidth (80)))
                {
                    config.edit = !config.edit;
                    if (config.edit && config.prefab != null)
                    {
                        config.go = GameObject.Find (config.prefab.name);
                        if (config.go == null)
                        {
                            config.go = PrefabUtility.InstantiatePrefab (config.prefab) as GameObject;
                            config.go.name = config.prefab.name;
                        }
                        config.go.transform.parent = outlineRoot;
                        var renders = EditorCommon.GetRenderers (config.go);
                        for (int j = 0; j < renders.Count; ++j)
                        {
                            var render = renders[j];
                            Material mat = render.sharedMaterial;
                            if (mat != null)
                            {
                                var partConfig = config.partOutlineConfig.Find ((x) => x.partName == render.name);
                                if (partConfig == null)
                                {
                                    partConfig = new PartOutlineConfig ()
                                    {
                                    partName = render.name
                                    };
                                    config.partOutlineConfig.Add (partConfig);
                                }
                                partConfig.mat = mat;
                            }
                        }

                    }
                }
                if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal ();

                if (config.edit)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.ObjectField (config.go, typeof (GameObject), true, GUILayout.MaxWidth (300));

                    if (config.go != null)
                    {
                        if (GUILayout.Button ("TestDisFade", GUILayout.MaxWidth (120)))
                        {
                            editorOutlineConfig.testGo = config;
                            editorOutlineConfig.currentScale = GetDistFade ();
                        }
                        if (GUILayout.Button ("UnTestDisFade", GUILayout.MaxWidth (120)))
                        {
                            if (config == editorOutlineConfig.testGo)
                                editorOutlineConfig.testGo = null;
                        }
                    }
                    EditorGUILayout.EndHorizontal ();

                    EditorGUI.BeginChangeCheck ();

                    var outline = config.outline;
                    EditorGUILayout.BeginHorizontal ();
                    outline.outlineColor = EditorGUILayout.ColorField ("Color", outline.outlineColor, GUILayout.MaxWidth (300));
                    outline.outlineWidth = EditorGUILayout.Slider ("Width", outline.outlineWidth, 0, 1, GUILayout.MaxWidth (300));
                    EditorGUILayout.EndHorizontal ();

                    // EditorGUILayout.BeginHorizontal ();
                    // outline.outlineMinWidth = EditorGUILayout.Slider ("MinWidth", outline.outlineMinWidth, 0, 1, GUILayout.MaxWidth (300));
                    // outline.outlineMaxWidth = EditorGUILayout.Slider ("MixWidth", outline.outlineMaxWidth, 1, 2, GUILayout.MaxWidth (300));
                    // EditorGUILayout.EndHorizontal ();
                    if (EditorGUI.EndChangeCheck ())
                    {
                        for (int j = 0; j < config.partOutlineConfig.Count; ++j)
                        {
                            var partConfig = config.partOutlineConfig[j];
                            if (!partConfig.isOverride && partConfig.mat != null)
                            {
                                partConfig.mat.SetVector ("_ColorOutline",
                                    new Vector4 (outline.outlineColor.r, outline.outlineColor.g, outline.outlineColor.b, outline.outlineWidth));
                                // partConfig.mat.SetVector ("_ParamOutline",
                                //     new Vector4 (outline.outlineMinWidth, outline.outlineMaxWidth, 1, 0));
                            }
                        }
                    }

                    for (int j = 0; j < config.partOutlineConfig.Count; ++j)
                    {
                        var partConfig = config.partOutlineConfig[j];
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.LabelField (partConfig.partName);
                        partConfig.isOverride = EditorGUILayout.Toggle ("IsOverride", partConfig.isOverride);
                        EditorGUILayout.EndHorizontal ();
                    }
                }
            }
            if (removeIndex >= 0)
            {
                editorOutlineConfig.configs.RemoveAt (removeIndex);
            }
            EditorCommon.EndScroll ();
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.defaultDist.minDist = EditorGUILayout.Slider ("Dist0", miscConfig.defaultDist.minDist, 0.1f, 2);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.defaultDist.minScale = EditorGUILayout.Slider ("Scale0", miscConfig.defaultDist.minScale, 0.1f, 2);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.defaultDist.maxDist = EditorGUILayout.Slider ("Dist1", miscConfig.defaultDist.maxDist, 2, 20);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.defaultDist.maxScale = EditorGUILayout.Slider ("Scale1", miscConfig.defaultDist.maxScale, 0.1f, 2);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            miscConfig.defaultDist.disapperDist = EditorGUILayout.Slider ("DisapperDist", miscConfig.defaultDist.disapperDist, miscConfig.defaultDist.maxScale, miscConfig.defaultDist.maxScale + 20);
            EditorGUILayout.EndHorizontal ();

            if (EditorGUI.EndChangeCheck ())
            {
                editorOutlineConfig.currentScale = GetDistFade();
                var context = EngineContext.instance;
                if (context != null)
                    EnvHelp.DirtyEnv(context, EnvSettingType.SceneMisc);
            }

            if (editorOutlineConfig.testGo != null)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.ObjectField (editorOutlineConfig.testGo.go, typeof (GameObject), true, GUILayout.MaxWidth (300));
                var trans = editorOutlineConfig.testGo.go.transform;
                if (GUILayout.Button ("P0", GUILayout.MaxWidth (80)))
                {
                    PutAtPos (trans, miscConfig.defaultDist.minDist, editorOutlineConfig.offsetY);
                }
                if (GUILayout.Button ("P1", GUILayout.MaxWidth (80)))
                {
                    PutAtPos (trans, miscConfig.defaultDist.maxDist, editorOutlineConfig.offsetY);
                }
                if (GUILayout.Button ("UnTestDisFade", GUILayout.MaxWidth (120)))
                {
                    editorOutlineConfig.testGo = null;
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUI.BeginChangeCheck ();
                editorOutlineConfig.offsetY = EditorGUILayout.Slider ("OffsetY", editorOutlineConfig.offsetY, -2, 2, GUILayout.MaxWidth (400));
                if (EditorGUI.EndChangeCheck ())
                {
                    PutAtPos (trans, miscConfig.defaultDist.minDist, editorOutlineConfig.offsetY);
                }

                EditorGUI.BeginChangeCheck ();
                editorOutlineConfig.dist = EditorGUILayout.Slider ("Dist", editorOutlineConfig.dist, 0, miscConfig.defaultDist.disapperDist + 1, GUILayout.MaxWidth (400));
                if (EditorGUI.EndChangeCheck ())
                {
                    PutAtPos (trans, editorOutlineConfig.dist, editorOutlineConfig.offsetY);
                    editorOutlineConfig.currentScale = GetDistFade ();
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.FloatField ("Scale", editorOutlineConfig.currentScale);
                EditorGUILayout.EndHorizontal ();
            }
            EditorCommon.EndGroup ();
        }

        public override void Update ()
        {

            switch (opType)
            {
                case OpType.OpSaveConfig:
                    SaveConfig ();
                    break;

            }
            opType = OpType.OpNone;
        }

        private void SaveConfig ()
        {
            if (editorOutlineConfig != null)
            {
                CommonAssets.SaveAsset (editorOutlineConfig);
            }
            if (miscConfig != null)
            {
                CommonAssets.SaveAsset (miscConfig);
            }
        }
    }
}
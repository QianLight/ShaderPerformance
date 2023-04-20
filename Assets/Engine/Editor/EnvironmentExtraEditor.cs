using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [CustomEditor (typeof (EnvironmentExtra))]
    public partial class EnvironmentExtraEditor : BaseEditor<EnvironmentExtra>
    {
        #region globalEffect
        private SerializedProperty envProfile;
        private SerializedProperty uiCamera;
        private SerializedProperty forceUpdate;
        #endregion

        //testObj
        // private SerializedProperty pointLight;
        // private SerializedProperty interactiveParam;
        private SerializedProperty debugEnvArea;
        //fast run
        #region fast run
        private SerializedProperty loadGameAtHere;
        private SerializedProperty bytesPrefabProgress;
        private SerializedProperty testBytesExport;
        private SerializedProperty sceneID;
        private SerializedProperty replaceStartScene;

        private SerializedProperty gotoScene;
        private SerializedProperty useStaticBatch;
        private SerializedProperty debugHeadData;
        private SerializedProperty debugChunkIndex;
        #endregion
        #region freeCamera
        private SerializedProperty forceUpdateFreeCamera;
        private SerializedProperty forceIgnore;
        private SerializedProperty holdRightMouseCapture;
        private SerializedProperty lookSpeed;
        private SerializedProperty moveSpeed;
        private SerializedProperty sprintSpeed;
        #endregion

        #region uiBlurDebugMat

        private SerializedProperty uiBlurRTDebugSize;
        #endregion
        #region debug
        private SerializedProperty drawFrustum;
        private SerializedProperty disableSceneObject;
        private SerializedProperty disableTerrainObject;
        private SerializedProperty drawDynamicWall;
        private SerializedProperty drawLodGrid;
        private SerializedProperty drawCameraPos;
        private SerializedProperty drawTerrainGrid;
        private SerializedProperty drawCacheChunk;
        private SerializedProperty previewLightProbe;
        private SerializedProperty drawGlobalShadowObject;

        private SerializedProperty drawCrossChunkPoint;
        private SerializedProperty drawLookAtPoint;
        private SerializedProperty previewDummyCamera;
        private SerializedProperty previewDrawCall;
        private SerializedProperty lodLevel;
        private SerializedProperty drawType;

        private SerializedProperty quadLevel;
        private SerializedProperty quadIndex;
        private SerializedProperty matLod;

        private SerializedProperty statistics;

        private SerializedProperty isPostProcessDebug;

        private SerializedProperty debugMode;
        private SerializedProperty ppDebugMode;
        private SerializedProperty debugDisplayType;
        private SerializedParameter splitAngle;
        private SerializedProperty splitPos;
        #endregion
        private EnvironmentExtra ee;
        private float splitLeft = -1;
        private float splitRight = 1;

        private static ReflectFun onInspectorExternal;
        public void OnEnable ()
        {
            #region globalEffect
            envProfile = FindProperty (x => x.envProfile);
            uiCamera = FindProperty (x => x.uiCamera);
            forceUpdate = FindProperty (x => x.forceUpdate);
            #endregion
            #region toggle
            // interactiveParam = FindProperty (x => x.interactiveParam);
            #endregion
            #region fastrun
            loadGameAtHere = FindProperty (x => x.loadGameAtHere);
            bytesPrefabProgress = FindProperty (x => x.bytesPrefabProgress);
            testBytesExport = FindProperty(x => x.testBytesExport);
            sceneID = FindProperty (x => x.sceneID);
            replaceStartScene = FindProperty (x => x.replaceStartScene);
            gotoScene = FindProperty (x => x.gotoScene);
            useStaticBatch = FindProperty (x => x.useStaticBatch);
            debugHeadData = FindProperty (x => x.debugHeadData);
            debugChunkIndex = FindProperty (x => x.debugChunkIndex);
            #endregion
            #region freecamera
            forceUpdateFreeCamera = FindProperty (x => x.forceUpdateFreeCamera);
            forceIgnore = FindProperty (x => x.forceIgnore);
            holdRightMouseCapture = FindProperty (x => x.holdRightMouseCapture);
            lookSpeed = FindProperty (x => x.lookSpeed);
            moveSpeed = FindProperty (x => x.moveSpeed);
            sprintSpeed = FindProperty (x => x.sprintSpeed);
            #endregion
            #region ui
            uiBlurRTDebugSize = FindProperty (x => x.uiBlurRTDebugSize);
            #endregion
            #region debug
            drawFrustum = FindProperty (x => x.drawFrustum);
            disableSceneObject = FindProperty (x => x.disableSceneObject);
            disableTerrainObject = FindProperty (x => x.disableTerrainObject);
            drawDynamicWall = FindProperty (x => x.drawDynamicWall);
            drawLodGrid = FindProperty (x => x.drawLodGrid);
            drawCameraPos = FindProperty (x => x.drawCameraPos);
            drawTerrainGrid = FindProperty (x => x.drawTerrainGrid);
            drawCacheChunk = FindProperty (x => x.drawCacheChunk);
            previewLightProbe = FindProperty (x => x.preiewLightProbe);
            drawGlobalShadowObject = FindProperty(x => x.drawGlobalShadowObject);
            drawCrossChunkPoint = FindProperty (x => x.drawCrossChunkPoint);
            drawLookAtPoint = FindProperty(x => x.drawLookAtPoint);
            previewDummyCamera = FindProperty (x => x.previewDummyCamera);
            previewDrawCall = FindProperty (x => x.previewDrawCall);
            lodLevel = FindProperty (x => x.lodLevel);

            drawType = FindProperty (x => x.drawType);
            quadLevel = FindProperty (x => x.quadLevel);
            quadIndex = FindProperty (x => x.quadIndex);
            matLod = FindProperty (x => x.matLod);

            statistics = FindProperty (x => x.statistics);

            debugEnvArea = FindProperty (x => x.debugEnvArea);

            isPostProcessDebug = FindProperty (x => x.isPostProcessDebug);
            debugMode = FindProperty (x => x.debugMode);
            ppDebugMode = FindProperty (x => x.ppDebugMode);
            debugDisplayType = FindProperty (x => x.debugContext.debugDisplayType);
            splitAngle = FindParameter (x => x.debugContext.splitAngle);
            splitPos = FindProperty (x => x.debugContext.splitPos);
            #endregion
            AssetsConfig.RefreshShaderDebugNames (false, true);
            AssetsConfig.RefreshShaderDebugNames (true, true);

            if (onInspectorExternal == null)
            {
                onInspectorExternal = EditorCommon.GetInternalFunction (typeof (EnvironmentExtraEditor), "OnInspectorExternal", false, false, true, false);
            }
        }

        private void CalcSplitLeftRight ()
        {
            float k = Mathf.Tan (Mathf.Deg2Rad * (90 - splitAngle.value.floatValue));
            float b = 1 + k;
            splitLeft = -b / k;
            splitRight = -splitLeft;

        }
        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            ee = target as EnvironmentExtra;
            if (ee != null)
            {

                EngineContext context = EngineContext.instance;
                if (EngineContext.IsRunning)
                {

                }
                else
                {
                    if (EditorCommon.BeginFolderGroup ("GlobalEffect", ref ee.globalEffectFolder))
                    {
                        EditorGUILayout.PropertyField (envProfile);
                        if (GUILayout.Button ("CreateOrLoad", GUILayout.MaxWidth (160)))
                        {
                            envProfile.objectReferenceValue = EnvProfile.CreateOrLoad (ee.gameObject);
                        }
                        EditorGUILayout.PropertyField (uiCamera);
                        EditorGUILayout.PropertyField (forceUpdate);
                        EditorCommon.EndGroup ();
                    }
                    if (EditorCommon.BeginFolderGroup ("Test", ref ee.testFolder))
                    {
                        EditorGUILayout.BeginHorizontal ();
                        if (GUILayout.Button ("RefreshMat"))
                        {
                            ee.InitMatObject ();
                            ee.UpdateInstanceObject ();
                        }
                        if (GUILayout.Button ("ResetMat"))
                        {
                            ee.ClearMatObject ();
                        }
                        EditorGUILayout.EndHorizontal ();
                        EditorCommon.EndGroup();
                    }
                    if (EditorCommon.BeginFolderGroup("FastRun", ref ee.fastRunFolder))
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.PropertyField(loadGameAtHere);
                        if (EditorGUI.EndChangeCheck())
                        {
                            ee.LoadGameAtHere(loadGameAtHere.boolValue);
                        }
                        EditorGUILayout.PropertyField(bytesPrefabProgress);
                        EditorGUILayout.PropertyField(testBytesExport);
                        EditorGUILayout.PropertyField(gotoScene);
                        EditorGUILayout.PropertyField (sceneID);
                        EditorGUILayout.PropertyField (replaceStartScene);
                        EditorGUILayout.PropertyField (useStaticBatch);
                        EditorGUILayout.PropertyField (debugHeadData);
                        EditorGUILayout.PropertyField (debugChunkIndex);
                        EditorGUI.BeginChangeCheck();
                        EnvironmentExtra.frameRate.Value = EditorGUILayout.IntSlider("FrameRate", EnvironmentExtra.frameRate.Value, 30, 60);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Application.targetFrameRate = EnvironmentExtra.frameRate.Value;
                        }
                        EditorCommon.EndFolderGroup ();
                    }
                }
                
                if (EditorCommon.BeginFolderGroup ("FreeCamera", ref ee.freeCameraFolder))
                {
                    EditorGUILayout.PropertyField (forceUpdateFreeCamera);
                    EditorGUILayout.PropertyField (forceIgnore);
                    EditorGUILayout.PropertyField (holdRightMouseCapture);
                    EditorGUILayout.PropertyField (lookSpeed);
                    EditorGUILayout.PropertyField (moveSpeed);
                    EditorGUILayout.PropertyField (sprintSpeed);
                    EditorCommon.EndFolderGroup ();
                }
                if (EditorCommon.BeginFolderGroup ("UI", ref ee.uiFolder))
                {
                    if (GUILayout.Button ("TestUIBlur"))
                    {
                        //ee.uiBlurRT = RenderingManager.instance.GetRT (ERTType.EUIBlurRT);
                        RenderingManager.instance.CaptureUIBGImage (EngineContext.instance);
                    }
                    //UISceneSystem.editorDebug = EditorGUILayout.Toggle("UISceneDebug",UISceneSystem.editorDebug);
                    // if (ee.uiBlur)
                    // {
                    //     EditorGUILayout.PropertyField (uiBlurRTDebugSize);
                    //     float size = 256 * uiBlurRTDebugSize.floatValue;
                    //     GUILayout.Space (size + 20);
                    //     Rect r = GUILayoutUtility.GetLastRect ();
                    //     r.y += 10;
                    //     r.width = size;
                    //     r.height = size;
                    //     EditorGUI.DrawPreviewTexture (r, ee.uiBlurRT);

                    // }

                    EditorCommon.EndFolderGroup ();
                }

                if (EditorCommon.BeginFolderGroup ("Debug", ref ee.debugFolder))
                {
                    //scene info
                    EditorGUILayout.LabelField (string.Format ("Scene Name:{0}", EngineContext.sceneName));
                    if (context != null)
                    {
                        if (EngineContext.IsRunning)
                        {
                            EditorGUILayout.LabelField (string.Format ("Dynamic Scene Name:{0}", context.dynamicSceneName));
                            EditorGUILayout.IntField ("AreaMask", (int) context.areaMask);
                        }

                        context.layerMask = (uint) EditorGUILayout.MaskField ("RenderLayer", (int) context.layerMask, DefaultGameObjectLayer.layerMaskName);
                    }

                    EditorGUILayout.PropertyField (drawFrustum);
                    EditorGUILayout.PropertyField (debugEnvArea);
                    EditorGUI.BeginChangeCheck ();
                    ee.drawTerrainHeight = EditorGUILayout.Toggle ("DrawTerrainHeight", ee.drawTerrainHeight);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        ee.EnableDrawTerrainHeight();
                    }
                    if (EngineContext.IsRunning)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(disableSceneObject);
                        if (EditorGUI.EndChangeCheck())
                        {
                            context.renderflag.SetFlag(EngineContext.RFlag_DisableSceneObject, disableSceneObject.boolValue);
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(disableTerrainObject);
                        if (EditorGUI.EndChangeCheck())
                        {
                            context.renderflag.SetFlag(EngineContext.RFlag_DisableTerrainObject, disableTerrainObject.boolValue);
                        }

                        EditorGUILayout.PropertyField(drawCameraPos);
                        EditorGUILayout.PropertyField(drawDynamicWall);
                        EditorGUILayout.PropertyField(drawCrossChunkPoint);
                        EditorGUILayout.PropertyField(drawLookAtPoint);
                        EngineContext.notLoadSceneObject = EditorGUILayout.Toggle("NotLoadSceneObject", EngineContext.notLoadSceneObject);

                        //SFXMgr.useEmptySfx = EditorGUILayout.Toggle("UseEmptySfx", SFXMgr.useEmptySfx);
                        //EditorGUILayout.LabelField(string.Format("Player State:{0}", context.colliderState));

                        //rendering
                        EditorGUILayout.PropertyField(previewLightProbe);
                        EditorGUILayout.PropertyField(previewDummyCamera);


                        EditorGUILayout.PropertyField(drawLodGrid);
                        EditorGUILayout.PropertyField(lodLevel);
                        EditorGUILayout.PropertyField(matLod);
                        context.MatLodDebug = matLod.intValue;
                        EditorGUILayout.PropertyField(drawTerrainGrid);
                        EditorGUILayout.PropertyField(previewDrawCall);
                        if (previewDrawCall.boolValue)
                        {
                            EditorCommon.BeginGroup("DrawCalls");
                            PreviewDrawCall(context);
                            EditorCommon.EndGroup();
                        }

                        EditorGUILayout.PropertyField(drawCacheChunk);
                        EditorGUILayout.PropertyField(drawType);
                        int level = quadLevel.intValue;
                        EditorGUILayout.PropertyField(quadLevel);

                        if (quadLevel.intValue == (int)QuadTreeLevel.Level3)
                        {
                            int index = quadIndex.intValue;

                            EditorGUI.BeginChangeCheck();
                            int newindex = EditorGUILayout.IntSlider("cull block", quadIndex.intValue, -1, 15);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "cull block");
                                if (index != newindex)
                                {
                                    quadIndex.intValue = newindex;
                                }
                            }

                        }
                        EditorGUILayout.PropertyField(drawGlobalShadowObject);
                        SceneObjectDebug.drawGlobalShadowObject = drawGlobalShadowObject.boolValue;

                        GUILayout.BeginHorizontal();
                        bool isLodEnable = context.saveflag.HasFlag(EngineContext.SFlag_DistCull);
                        isLodEnable = EditorGUILayout.Toggle("DistCull", isLodEnable);
                        context.saveflag.SetFlag(EngineContext.SFlag_DistCull, isLodEnable);
                        GUILayout.EndHorizontal();
                        EditorGUI.BeginChangeCheck();
                        ToolsUtility.LodSizeGUI("Near", ref context.lodNearSize);
                        ToolsUtility.LodSizeGUI("Far", ref context.lodFarSize);
                        if (EditorGUI.EndChangeCheck())
                        {
                            context.lodConfigDirty = true;
                        }

                        SceneChunkLoadSystem.meshName = EditorGUILayout.TextField("Mesh", SceneChunkLoadSystem.meshName);

                        LoadMgr.debugResName = EditorGUILayout.TextField("DebugResName", LoadMgr.debugResName);
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("BeginRecordRes",GUILayout.MaxWidth(120)))
                        {
                            LoadMgr.singleton.BegineRecord();
                        }
                        if (GUILayout.Button("EndRecordRes", GUILayout.MaxWidth(120)))
                        {
                            LoadMgr.singleton.EndRecord();
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {

                        EditorGUILayout.PropertyField(drawGlobalShadowObject);
                        if (GUILayout.Button("RefreshTerrainHeight"))
                        {
                            EnvironmentExtra.FillTerrainVertex();
                        }

                        if (GUILayout.Button("ResetEnv"))
                        {
                            if (AnimEnv.currentAE != null)
                            {
                                AnimEnv.currentAE.UnInit();
                            }
                        }
                        if (GUILayout.Button("ReloadEngineRes"))
                        {
                            WorldSystem.InitEngineRes();
                        }
                        if (GUILayout.Button("ExportEnvConfig"))
                        {
                            EnvProfile.ExportEnvConfig();
                        }

                        EditorGUILayout.PropertyField(previewDummyCamera);
                    }
                    if (GUILayout.Button("CaptureRT", GUILayout.MaxWidth(100)))
                    {
                        RenderingManager.instance.SetCapture(256, 256, Color.blue,
                            () =>
                            {
                                context.renderflag.SetFlag(EngineContext.RFlag_NotRenderSky, true);
                            },
                            () =>
                            {
                                context.renderflag.SetFlag(EngineContext.RFlag_NotRenderSky, false);
                            },
                            (ref NativeArray<byte> data) =>
                            {
                                if (data.Length > 0)
                                {
                                    var now = System.DateTime.Now;
                                    string path = string.Format("Assets/../Dump/Capture_{0}-{1}-{2}_{3}-{4}-{5}.png",
                                        now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                                    EngineUtility.SavePNG(ref data, 256, 256, path);
                                    path = path.Replace("/", "\\");
                                    System.Diagnostics.Process.Start("Explorer", "/select," + path);
                                }              
                            });
                    }
                    if (onInspectorExternal != null)
                    {
                        onInspectorExternal.Call (this, null);
                    }
                    EditorGUI.BeginChangeCheck ();
                    EditorGUILayout.PropertyField (statistics);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        EngineProfiler.SetStatisticsType (statistics.intValue);
                    }
                    // EditorGUILayout.PropertyField (drawTerrainHeight);

                    EditorCommon.EndFolderGroup ();
                }
                if (EditorCommon.BeginFolderGroup ("Shader", ref ee.debugShaderFolder))
                {
                    EditorGUI.BeginChangeCheck ();
                    EditorGUILayout.PropertyField (isPostProcessDebug);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        Undo.RecordObject (target, "isPostProcessDebug");
                        ee.RefreshDebug (isPostProcessDebug.boolValue);
                        debugDisplayType.intValue = 0;
                        splitAngle.value.floatValue = 0;
                        splitPos.floatValue = 0;

                        GlobalContex.globalDebugMode = isPostProcessDebug.boolValue ?
                            ppDebugMode.intValue :
                            debugMode.intValue;
                    }
                    string[] debugNames = null;
                    bool refreshDebug = false;
                    if (GUILayout.Button ("Refresh"))
                    {
                        refreshDebug = true;
                    }
                    SerializedProperty debugModeSp = null;
                    if (isPostProcessDebug.boolValue)
                    {
                        debugNames = AssetsConfig.shaderPPDebugContext.debugNames;
                        ee.debugContext.shaderID = EnvironmentExtra.ppDebugShaderIDS;
                        debugModeSp = ppDebugMode;
                    }
                    else
                    {

                        debugNames = AssetsConfig.shaderDebugContext.debugNames;
                        ee.debugContext.shaderID = EnvironmentExtra.debugShaderIDS;
                        debugModeSp = debugMode;
                    }

                    if (debugNames != null)
                    {
                        EditorGUI.BeginChangeCheck ();
                        int debugValue = EditorGUILayout.Popup ("DebugMode", debugModeSp.intValue, debugNames);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            Undo.RecordObject (target, "GlobalDebugMode");
                            GlobalContex.globalDebugMode = debugValue;
                            ee.debugContext.modeModify = true;
                            if (isPostProcessDebug.boolValue)
                            {
                                RenderContext.currentDebugMode = debugValue;
                            }
                            ee.UpdateDebugMode ();
                        }
                        EditorGUI.BeginChangeCheck ();
                        EditorGUILayout.PropertyField (debugDisplayType);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            Undo.RecordObject (target, "DebugDisplayType");
                            ee.debugContext.typeModify = true;
                        }
                        if (debugDisplayType.intValue == (int) DebugDisplayType.Split)
                        {
                            EditorGUI.BeginChangeCheck ();
                            PropertyField (splitAngle);
                            if (EditorGUI.EndChangeCheck ())
                            {
                                Undo.RecordObject (target, "SplitAngle");
                                ee.debugContext.angleModify = true;
                            }
                            EditorGUI.BeginChangeCheck ();
                            splitPos.floatValue = EditorGUILayout.Slider ("SplitPos", splitPos.floatValue, splitLeft, splitRight);
                            if (EditorGUI.EndChangeCheck ())
                            {
                                Undo.RecordObject (target, "SplitPos");
                                ee.debugContext.posModify = true;
                            }
                        }

                    }
                    EditorCommon.EndFolderGroup ();

                    if (refreshDebug)
                    {
                        AssetsConfig.RefreshShaderDebugNames (isPostProcessDebug.boolValue, true);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties ();
        }

        void PreviewDrawCall (EngineContext context)
        {
            for (int i = 0; i < context.drawCalls.Count; ++i)
            {
                var dc = context.drawCalls[i];
                if (dc != null)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.ObjectField ("", dc, typeof (SceneObjectData), true);
                    EditorGUILayout.EndHorizontal ();
                }

            }
        }
    }
}
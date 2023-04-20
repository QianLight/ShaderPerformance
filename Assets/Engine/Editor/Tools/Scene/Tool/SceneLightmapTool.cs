using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;
using UnityEngine.SceneManagement;

namespace CFEngine.Editor
{
    enum OpLightmapType
    {
        OpNone,
        OpClearBake,
        OpBake,
        OpCancelBake,
        OpTestPrepare,
        OpTestBake,
        OpBakeAll,
        OpTryBind,
        OpCombineLightmapMesh,
        OpTestExportLightProbes,
        OpLoadTerrainLightmap,
        OpSetReflectionProbe,
        OpUseCombineLightmap,
        OpUseDefaultLightmap,
        OpConvertLightmap2Combine,
        OpRecoverOrginLightmap,
        OpFormatLightmap

    }
    public enum LightmapBakingType
    {
        Single,
        Total
    }
    public enum LightmapCofigType
    {
        Center,
        Around,
        Terrain,
        UnBake
    }

    public partial class SceneEditTool : CommonToolTemplate
    {
        public class LightmapBakeVolumn
        {
            public string name;
            public bool bake = true;
            public LightmapVolumn volumn = null;
            public List<ILightmapObject> bakeObjects = new List<ILightmapObject>();
            public int[] aroundVolumn = new int[9];
            public LightmapBakeVolumn()
            {
                for (int i = 0; i < aroundVolumn.Length; ++i)
                {
                    aroundVolumn[i] = -1;
                }
            }
        }

        class LightmapBakeContext
        {
            public List<LightmapBakeVolumn> lightmapVolumns = new List<LightmapBakeVolumn>();
            public List<MeshRenderer> unBakeObjects = new List<MeshRenderer>();
            public LightmapBakeVolumn currentBakeVolumn = null;
            public int currentBakeIndex = -1;
            public LightmapBakingType bakeType = LightmapBakingType.Total;
            public bool isBaking = false;
            public bool realBake = false;
            public int bakeFinishIndex = -1;
            public EditorCommon.EnumTransform fun;
            public Action lightmapBakeEnd;
            public LightmapVolumnData lvd;

            public Transform[] chunkVolumns;
            public Transform globalVolumn = null;
            public List<Transform> transTmp = new List<Transform>();
            public string sceneName;

            //public List<RenderBatch> lightProbeEditBatch = new List<RenderBatch> ();
        }
        
        public enum NewBakedState
        {
            defaultState,
            singleSideState,
            doubleSideState
        }

        

        private Vector2Int bakeIndex = new Vector2Int(-1, -1);
        private Vector2 volumnScroll = Vector2.zero;
        private LightmapBakeContext lightmapBakeContext;

        private OpLightmapType lightmapOp = OpLightmapType.OpNone;
        private Vector2 lightmapConfigScroll = Vector2.zero;
        private LightmapConfigData lightmapConfigData;
        int[] LightmapSizeValues = { 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        string[] LightmapSizeStr = { "32", "64", "128", "256", "512", "1024", "2048", "4096" };
        //bool displayBakeryTool;
        private bool doubleSidedGI = false;
        private bool isUseDoubleBakePipeline = false;
        private NewBakedState curBakedState = NewBakedState.singleSideState;
        private bool isUseCombineLightmap = true;

        private bool isBlurShadowmask = false;
        private int blurIterationCount = 0;
        private float blurOffset = 0f;
        
        //private LightprobeAreaEdit lpae;

        // private double lastCbUpdate;
        // private double dirtyTime = 0;
        private void PostInitLightmap()
        {
            lightmapBakeContext = new LightmapBakeContext();
            lightmapBakeContext.lightmapBakeEnd = LightMapBakeEnd;
            string configPath = string.Format("{0}/{1}_LightmapConfig.asset", sceneContext.configDir, sceneContext.name);
            lightmapConfigData = EditorCommon.LoadAsset<LightmapConfigData>(configPath);
            UnityEngine.Rendering.Universal.Skybox.EditorOverrideMaterial = null;
            if (!File.Exists(configPath))
            {
                EditorCommon.SaveAsset(configPath, lightmapConfigData);
            }

        }
        private void UnInitLightmap() { }
        #region 0.prepare

        private void SetRenderBakeState(Renderer render, float lightmapScale, SerializedProperty sp,
            LightmapCofigType configIndex)
        {
            // LightmapParameters lp = AssetsConfig.instance.LightmapParam[(int) configIndex];
            // sp.objectReferenceValue = lp;
            // sp.serializedObject.ApplyModifiedProperties ();
            CommonAssets.SetSerializeValue(render, "m_ScaleInLightmap", lightmapScale);
            if (doubleSidedGI && render.sharedMaterial != null && !render.sharedMaterial.doubleSidedGI)
            {
                render.sharedMaterial.doubleSidedGI = doubleSidedGI;
                allDoubleSidedGI.Add(render.sharedMaterial);
            }

            if (GameObjectUtility.GetStaticEditorFlags(render.gameObject) == 0)
                GameObjectUtility.SetStaticEditorFlags(render.gameObject,
                    StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);
        }

        private void PrepareBakeObjectExt(Renderer render, ILightmapObject lmo, LightmapCofigType configIndex)
        {
            if (configIndex == LightmapCofigType.UnBake)
            {
                //GameObjectUtility.SetStaticEditorFlags(render.gameObject, (StaticEditorFlags)0);
            }
            else
            {
                SerializedProperty sp = CommonAssets.GetSerializeProperty(render, "m_LightmapParameters");
                float lightmapScale = configIndex == LightmapCofigType.Around ? 0 : lmo.LightmapScale;
                SetRenderBakeState(render, lightmapScale, sp, configIndex);
                lmo.BeginBake();
            }
        }
        private Transform AddVolumn(Transform trans, string name, int chunkIndex)
        {
            var volumn = trans.Find(name);
            if (volumn == null)
            {
                volumn = new GameObject(name).transform;
            }
            volumn.parent = trans;
            LightmapVolumn lightmapVolumn = volumn.GetComponent<LightmapVolumn>();
            if (lightmapVolumn == null)
            {
                lightmapVolumn = volumn.gameObject.AddComponent<LightmapVolumn>();
            }
            lightmapVolumn.chunkIndex = chunkIndex;
            return lightmapVolumn.transform;
        }

        private void MoveObject2ChunkVolumn(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
            if (volumn != null) { }
            else
            {
                if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    context.transTmp.Add(trans);
                }
                else
                {
                    EditorCommon.EnumChildObject(trans, param, context.fun);
                }
            }
        }
        private void CreateChunkLightmapVolumn()
        {
            lightmapBakeContext.transTmp.Clear();

            //init
            int chunkCount = widthCount * heightCount;
            if (lightmapBakeContext.chunkVolumns == null ||
                chunkCount != lightmapBakeContext.chunkVolumns.Length)
            {
                lightmapBakeContext.chunkVolumns = new Transform[chunkCount];
            }

            var trans = TerrainSystem.system.GetMeshTerrain();
            var staticTrans = StaticObjectSystem.system.resData.workspace;
            if (trans != null && staticTrans != null)
            {
                //global
                lightmapBakeContext.globalVolumn = AddVolumn(staticTrans, "GlobalVolum", -1);
                //check volumn
                var monos = EditorCommon.GetScripts<TerrainObject>(trans.gameObject);
                for (int i = 0; i < monos.Count; ++i)
                {
                    var obj = monos[i] as TerrainObject;
                    lightmapBakeContext.chunkVolumns[obj.chunkID] = AddVolumn(staticTrans, obj.name, obj.chunkID);
                }

                //readjust chunk objects
                for (int i = 0; i < staticTrans.childCount; ++i)
                {
                    var child = staticTrans.GetChild(i);
                    var volumn = child.GetComponent<LightmapVolumn>();
                    if (volumn != null)
                    {
                        for (int j = 0; j < volumn.transform.childCount; ++j)
                        {
                            var cc = volumn.transform.GetChild(j);
                            if (cc.name.StartsWith("Chunk_") || cc.name.StartsWith("GlobalVolumn"))
                            {
                                lightmapBakeContext.transTmp.Add(cc);
                            }
                        }
                    }
                }

                for (int i = 0; i < lightmapBakeContext.transTmp.Count; ++i)
                {
                    var t = lightmapBakeContext.transTmp[i];
                    t.parent = staticTrans;
                }
                lightmapBakeContext.transTmp.Clear();

                lightmapBakeContext.fun = MoveObject2ChunkVolumn;
                EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);
                for (int i = 0; i < lightmapBakeContext.transTmp.Count; ++i)
                {
                    var t = lightmapBakeContext.transTmp[i];
                    var chunkIndex = FindChunkIndex(t.position);
                    if (chunkIndex >= 0 && chunkIndex < lightmapBakeContext.chunkVolumns.Length)
                    {
                        var volumn = lightmapBakeContext.chunkVolumns[chunkIndex];
                        t.parent = volumn;
                    }
                    else
                    {
                        t.parent = lightmapBakeContext.globalVolumn;
                    }
                }
                lightmapBakeContext.transTmp.Clear();
            }

        }
        private void AddObj2BakeVolumn(LightmapBakeContext context, Transform trans, LightmapVolumn volumn)
        {
            var monos = EditorCommon.GetScripts<MeshRenderObject>(trans.gameObject);
            if (monos.Count > 0)
            {
                LightmapBakeVolumn bakeVolumn = new LightmapBakeVolumn();
                bakeVolumn.volumn = volumn;
                if (!volumn.name.StartsWith("Chunk_") && !volumn.name.StartsWith("CustomVolumn_"))
                {
                    volumn.name = string.Format("Volumn_{0}", context.lightmapVolumns.Count);
                }
                bakeVolumn.name = volumn.name;
                volumn.volumnName = bakeVolumn.name;
                for (int i = 0; i < monos.Count; ++i)
                {
                    var obj = monos[i] as MeshRenderObject;
                    if (obj.gameObject.activeInHierarchy)
                        bakeVolumn.bakeObjects.Add(obj);
                }
                context.lightmapVolumns.Add(bakeVolumn);
            }
        }
        private void PrepareVolumn(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
            if (volumn != null)
            {
                AddObj2BakeVolumn(context, trans, volumn);
            }
            else
            {
                if (!EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    EditorCommon.EnumChildObject(trans, param, context.fun);
                }
            }
        }

        private void PrepareTerrainVolumn()
        {
            var trans = TerrainSystem.system.GetMeshTerrain();
            if (trans != null)
            {

                var monos = EditorCommon.GetScripts<TerrainObject>(trans.gameObject);
                if (monos.Count > 0)
                {

                    for (int i = 0; i < monos.Count; ++i)
                    {
                        var obj = monos[i] as TerrainObject;

                        LightmapVolumn volumn = obj.GetComponent<LightmapVolumn>();
                        if (volumn == null)
                        {
                            volumn = obj.gameObject.AddComponent<LightmapVolumn>();
                        }
                        LightmapBakeVolumn bakeVolumn = new LightmapBakeVolumn();
                        bakeVolumn.volumn = volumn;
                        bakeVolumn.name = "Terrain_" + obj.name;
                        volumn.volumnName = bakeVolumn.name;
                        bakeVolumn.bakeObjects.Add(obj);
                        lightmapBakeContext.lightmapVolumns.Add(bakeVolumn);
                    }
                }
            }
        }

        private void PrepareUnBakeObject(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            MeshRenderer mr = trans.GetComponent<MeshRenderer>();
            if (mr)
            {
                context.unBakeObjects.Add(mr);
            }
            EditorCommon.EnumChildObject(trans, param, context.fun);
        }

        private void PrepareVolumn()
        {
            CreateChunkLightmapVolumn();
            lightmapBakeContext.lightmapVolumns.Clear();
            lightmapBakeContext.fun = PrepareVolumn;
            EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);

            PrepareTerrainVolumn();

            lightmapBakeContext.fun = PrepareUnBakeObject;
            EditorCommon.EnumPath(EditorSceneObjectType.Prefab, lightmapBakeContext.fun, lightmapBakeContext);
            EditorCommon.EnumPath(EditorSceneObjectType.Instance, lightmapBakeContext.fun, lightmapBakeContext);
        }

        private void PrepareBakeLightMap()
        {
            PrepareVolumn();

            for (int i = 0; i < lightmapBakeContext.unBakeObjects.Count; ++i)
            {
                var render = lightmapBakeContext.unBakeObjects[i];
                PrepareBakeObjectExt(render, null, LightmapCofigType.UnBake);
            }
        }
        #endregion

        #region 1.bake

        private void PrepareBakeObject(LightmapBakeVolumn volumn)
        {
            for (int i = 0; i < lightmapBakeContext.lightmapVolumns.Count; ++i)
            {
                var v = lightmapBakeContext.lightmapVolumns[i];
                var bakeType = LightmapCofigType.Around;
                if (v == volumn)
                {
                    if (v.name.StartsWith("Terrain_"))
                    {
                        bakeType = LightmapCofigType.Terrain;
                    }
                    else
                    {
                        bakeType = LightmapCofigType.Center;
                    }
                }
                for (int j = 0; j < v.bakeObjects.Count; ++j)
                {
                    var bakeObject = v.bakeObjects[j];
                    var render = bakeObject.GetRenderer();
                    if (render != null)
                    {
                        PrepareBakeObjectExt(render, bakeObject, bakeType);
                    }
                }
            }
        }

        private List<Material> allDoubleSidedGI = new List<Material>();

        private void ClearDoubleSidedGI()
        {
            for (int i = 0; i < allDoubleSidedGI.Count; i++)
            {
                if (allDoubleSidedGI[i] != null)
                    allDoubleSidedGI[i].doubleSidedGI = false;
            }

            allDoubleSidedGI.Clear();
        }
        
        private void BakeVolumn(int index, bool realBake)
        {
            StoreStaticEditorFlags();

            allDoubleSidedGI.Clear();

            SetDoubleSideState();
            
            lightmapBakeContext.isBaking = true;
            AmbientModify.autoRefreshSystemValue = false;
            SkyboxModify.autoRefreshSystemValue = false;
            if (index < lightmapBakeContext.lightmapVolumns.Count)
            {
                var volumn = lightmapBakeContext.lightmapVolumns[index];
                lightmapBakeContext.currentBakeVolumn = volumn;

                PrepareBakeObject(volumn);
                if (realBake)
                {
                    RealBakeLightMap();
                }
            }
        }

        private void SetDoubleSideState()
        {
            switch (curBakedState)
            {
                case NewBakedState.defaultState:
                case NewBakedState.singleSideState:
                    doubleSidedGI = false;
                    break;
                case NewBakedState.doubleSideState:
                    doubleSidedGI = true;
                    break;
            }
        }

        private static Dictionary<GameObject, StaticEditorFlags> allGameObjectsFlagsDic = new Dictionary<GameObject, StaticEditorFlags>();
        public static void StoreStaticEditorFlags()
        {
            allGameObjectsFlagsDic.Clear();
            GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>(true);
            for (int i = 0; i < allObjs.Length; i++)
            {
                GameObject obj = allObjs[i];
                StaticEditorFlags sef = GameObjectUtility.GetStaticEditorFlags(obj);

                allGameObjectsFlagsDic.Add(obj, sef);
            }
        }

        public static void RecoverStaticEditorFlags()
        {
            foreach (KeyValuePair<GameObject, StaticEditorFlags> itm in allGameObjectsFlagsDic)
            {
                if (itm.Key == null) continue;
                GameObjectUtility.SetStaticEditorFlags(itm.Key, itm.Value);
            }
        }


        private void BakeFinish()
        {
            Lightmapping.bakeCompleted -= BakeFinish;

            UnityEngine.Debug.Log("BakeFinish");
        }

        private void BakeRecover()
        {
            ClearDoubleSidedGI();
            if (curBakedState == NewBakedState.defaultState || curBakedState == NewBakedState.doubleSideState)
            {
                RecoverLightmapRes();
            }
            
            RecoverStaticEditorFlags();
            UnityEngine.Debug.Log("BakeRecover");
        }

        private int FindNextBake()
        {
            int index = ++lightmapBakeContext.currentBakeIndex;
            while (index < lightmapBakeContext.lightmapVolumns.Count)
            {
                var volumn = lightmapBakeContext.lightmapVolumns[index];
                if (volumn.bake)
                {

                    return index;
                }
                index = ++lightmapBakeContext.currentBakeIndex;
            }
            return -1;
        }

        private bool BakeNext()
        {
            SceneTool.DoRepaint();
            int index = FindNextBake();
            if (index >= 0)
            {
                SetBakedPipelineType(index);
                BakeVolumn(index, true);
                return true;
            }
            return false;
        }

        private void BakeAllVolumns()
        {
            lightmapBakeContext.currentBakeIndex = -1;
            int index = FindNextBake();
            if (index >= 0)
            {
                lightmapBakeContext.realBake = true;
                lightmapBakeContext.bakeType = LightmapBakingType.Total;
                SetBakedPipelineType(index);
                BakeVolumn(index, true);
            }
        }

        private void BakeCurrentVolumn()
        {
            if (lightmapBakeContext.currentBakeIndex >= 0)
            {
                BakeVolumn(lightmapBakeContext.currentBakeIndex, lightmapBakeContext.realBake);
            }
        }

        private void SetBakedPipelineType(int volumeIndex)
        {
            curBakedState = NewBakedState.defaultState;
            if (volumeIndex < 0 || volumeIndex >= lightmapBakeContext.lightmapVolumns.Count)
            {
                return;
            }

            if (lightmapConfigData == null || lightmapConfigData.configs == null)
            {
                return;
            }

            LightmapBakeVolumn lightmapBakeVolumn = lightmapBakeContext.lightmapVolumns[volumeIndex];
            
            for (int i = 0; i < lightmapConfigData.configs.Count; ++i)
            {
                var config = lightmapConfigData.configs[i];
                if (config.name != lightmapBakeVolumn.volumn.configName)
                {
                    continue;
                }
                
                isUseDoubleBakePipeline = config.useDoubleBakePipeline;
                curBakedState = config.useDoubleBakePipeline ? NewBakedState.singleSideState : NewBakedState.defaultState;
                return;
            }
        }

        private void RealBakeLightMap()
        {
            SetLightmapConfig(lightmapBakeContext.currentBakeVolumn.volumn.configName);
            EditorUtility.DisplayProgressBar("bakeing...",
                string.Format("bake volumn {0} {1}/{2}",
                    lightmapBakeContext.currentBakeVolumn.name,
                    lightmapBakeContext.currentBakeIndex,
                    lightmapBakeContext.lightmapVolumns.Count),
                (float)lightmapBakeContext.currentBakeIndex / lightmapBakeContext.lightmapVolumns.Count);
            Lightmapping.bakeCompleted += lightmapBakeContext.lightmapBakeEnd;
            Lightmapping.bakeCompleted += BakeFinish;
            Lightmapping.Clear();
            Lightmapping.BakeAsync();
        }

        private void LightMapBakeEnd()
        {
            Lightmapping.bakeCompleted -= lightmapBakeContext.lightmapBakeEnd;
            lightmapBakeContext.bakeFinishIndex = lightmapBakeContext.currentBakeIndex;
            if (lightmapBakeContext.bakeFinishIndex < lightmapBakeContext.lightmapVolumns.Count)
            {
                var volumn = lightmapBakeContext.lightmapVolumns[lightmapBakeContext.bakeFinishIndex];
                DebugLog.AddEngineLog2("{0} bake finish", volumn.name);
            }

        }
        #endregion

        #region 2.finish bake
        private void FinishBakeObjct(LightmapBakeVolumn bakeVolumn)
        {
            ExportLightmapVolumnRes(bakeVolumn.volumn, bakeVolumn.bakeObjects);
            CleanBakeObjct();
            UnBindLightmapData();
            BakeRecover();
        }

        private void ExportLightmapVolumnRes(LightmapVolumn volumn, List<ILightmapObject> bakeObjects)
        {
            Dictionary<int, List<ILightmapObject>> lightmapIndex = new Dictionary<int, List<ILightmapObject>>();

            List<LigthmapRenderData> objLists = new List<LigthmapRenderData>();
            
            bool isFirstExport = (curBakedState == NewBakedState.defaultState || curBakedState == NewBakedState.singleSideState);
            bool isFinalExport = (curBakedState == NewBakedState.defaultState || curBakedState == NewBakedState.doubleSideState);
            
            for (int i = 0; i < bakeObjects.Count; ++i)
            {
                var bakeObject = bakeObjects[i];
                if (bakeObject == null) continue;

                var render = bakeObject.GetRenderer();
                if (render != null && render.lightmapIndex >= 0)
                {
                    bakeObject.SetLightmapData(render.lightmapIndex, render.lightmapScaleOffset);
                    List<ILightmapObject> objects = null;
                    if (!lightmapIndex.TryGetValue(render.lightmapIndex, out objects))
                    {
                        objects = new List<ILightmapObject>();
                        lightmapIndex.Add(render.lightmapIndex, objects);
                    }
                    objects.Add(bakeObject);

                    LigthmapRenderData itmData = new LigthmapRenderData();
                    itmData.render = render;
                    itmData.lightmapIndex = render.lightmapIndex;
                    itmData.lightmapScaleOffset = render.lightmapScaleOffset;
                    itmData.realtimeLightmapIndex = render.realtimeLightmapIndex;
                    itmData.realtimeLightmapScaleOffset = render.realtimeLightmapScaleOffset;
                    objLists.Add(itmData);
                }

            }

            volumn.renders = objLists.ToArray();

            //copy lightmaps
            var lightmaps = LightmapSettings.lightmaps;
            if (volumn != null && isFirstExport)
            {
                volumn.res = new LigthmapRes[lightmaps.Length];
            }
            string folder = SceneAssets.CreateFolder(sceneContext.configDir, "SceneLightmapBackup");
            folder = SceneAssets.CreateFolder(folder, volumn.volumnName);
            if (isFirstExport)
            {
                Debug.Log("Export LightProbes");
                ExportLightProbes(volumn, folder);
            }
            
            string sceneName = SceneManager.GetActiveScene().name;
            string targetBakeLightmap = String.Empty;
            string targetBakeShadowMask = String.Empty;
            string targetBakeDir = String.Empty;
            // string targetColorCombineShadowmas = String.Empty;
            Texture2D colorMap = null;
            Texture2D shadowMask = null;
            Texture2D colorCombineShadowmask = null;
            for (int i = 0; i < lightmaps.Length; ++i)
            {
                var lightmapData = lightmaps[i];
                List<ILightmapObject> objects = null;
                if (!lightmapIndex.TryGetValue(i, out objects))
                {
                    continue;
                }
                
                if (lightmapData.lightmapColor == null)
                {
                    continue;
                }

                if (volumn.res[i] == null)
                {
                    volumn.res[i] = new LigthmapRes();
                }
                
                if (isFirstExport && lightmapData.shadowMask != null)
                {
                    targetBakeShadowMask = string.Format("{0}/{1}_Lightmap_{2}-{3}.png", folder, sceneName, volumn.volumnName, i.ToString());
                    string shadowMaskPath = AssetDatabase.GetAssetPath(lightmapData.shadowMask);
                    AssetDatabase.CopyAsset(shadowMaskPath, targetBakeShadowMask);
                    shadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(targetBakeShadowMask);
                    if (isBlurShadowmask)
                    {
                        LightmapCombineManager.Instance.BlurTex(shadowMask,blurIterationCount,blurOffset);
                    }
                    volumn.res[i].shadowMask = shadowMask;
                }
                
                if (lightmapData.lightmapDir != null)
                {
                    targetBakeDir = string.Format("{0}/{1}_LightmapDir_{2}-{3}.exr", folder, sceneName, volumn.volumnName, i.ToString());
                    string dirPath = AssetDatabase.GetAssetPath(lightmapData.lightmapDir);
                    AssetDatabase.CopyAsset(dirPath, targetBakeDir);
                }

                if (isFinalExport)
                {
                    targetBakeLightmap = string.Format("{0}/{1}_Lightmap_{2}-{3}.exr", folder, sceneName, volumn.volumnName, i.ToString());
                    string colorPath = AssetDatabase.GetAssetPath(lightmapData.lightmapColor);
                    AssetDatabase.CopyAsset(colorPath, targetBakeLightmap);
                    colorMap = AssetDatabase.LoadAssetAtPath<Texture2D>(targetBakeLightmap);
                    volumn.res[i].color = colorMap;
                    
                    // targetColorCombineShadowmas = string.Format("{0}/{1}_Lightmap_Combine_{2}-{3}.tga", folder, sceneName, volumn.volumnName, i.ToString());
                    // colorCombineShadowmask = LightmapCombineManager.Instance.GenerateSignleCombineLightmap(targetColorCombineShadowmas, colorMap, volumn.res[i].shadowMask);
                    // volumn.res[i].colorCombineShadowMask = colorCombineShadowmask;
                    // volumn.res[i].color = null;
                    // volumn.res[i].shadowMask = null;
                    
                    AssetDatabase.SaveAssets();
                    
                    for (int j = 0; j < objects.Count; ++j)
                    {
                        var lmo = objects[j];
                        lmo.SetLightmapRes(volumn.res[i].color, volumn.res[i].shadowMask, null, null);
                    }
                }
            }
        }
        private void CleanBakeObjct()
        {
            for (int i = 0; i < lightmapBakeContext.lightmapVolumns.Count; ++i)
            {
                var v = lightmapBakeContext.lightmapVolumns[i];
                for (int j = 0; j < v.bakeObjects.Count; ++j)
                {
                    var bakeObject = v.bakeObjects[j];
                    bakeObject.EndBake();
                }
            }
        }

        //private void TryBindLightmapRes(Transform trans, object param)
        //{
        //    var context = param as LightmapBakeContext;
        //    LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
        //    if (volumn != null)
        //    {
        //        var monos = EditorCommon.GetScripts<MeshRenderObject>(trans.gameObject);
        //        if (monos.Count > 0)
        //        {
        //            var bakeObjects = new List<ILightmapObject>();
        //            for (int i = 0; i < monos.Count; ++i)
        //            {
        //                bakeObjects.Add(monos[i] as ILightmapObject);
        //            }
        //            ExportLightmapVolumnRes(volumn, bakeObjects);
        //        }
        //    }
        //    else
        //    {
        //        if (!EditorCommon.IsPrefabOrFbx(trans.gameObject))
        //        {
        //            EditorCommon.EnumChildObject(trans, param, context.fun);
        //        }
        //    }
        //}

        //private void BindLightmapRes()
        //{
        //    if (LightmapSettings.lightmaps != null)
        //    {
        //        lightmapBakeContext.fun = TryBindLightmapRes;
        //        EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);
        //        if (GlobalContex.ee != null)
        //        {
        //            GlobalContex.ee.UpdateMatObject();
        //        }
        //    }
        //}
        //private void TryBindLightmapRes ()
        //{
        //    lightmapBakeContext.bindBakeLightmap = false;
        //    BindLightmapRes ();
        //}

        //private void TryBindBakeLightmap ()
        //{
        //    lightmapBakeContext.bindBakeLightmap = true;
        //    BindLightmapRes ();
        //}

        #endregion

        #region lightmap bake helper
        //private static void CopyLightmapParameters (int index)
        //{
        //    if (AssetsConfig.instance.LightmapParam != null)
        //    {
        //        MethodInfo mi = typeof (LightmapEditorSettings).GetMethod ("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
        //        if (mi != null)
        //        {

        //            LightmapSettings lightmapSettings = mi.Invoke (null, null) as LightmapSettings;
        //            if (lightmapSettings != null)
        //            {
        //                SerializedObject so = new SerializedObject (lightmapSettings);
        //                SerializedProperty sp = so.FindProperty ("m_LightmapEditorSettings.m_LightmapParameters");
        //                if (sp != null)
        //                {
        //                    LightmapParameters src = sp.objectReferenceValue as LightmapParameters;
        //                    if (src != null)
        //                    {
        //                        LightmapParameters lp = AssetsConfig.instance.LightmapParam[index];
        //                        lp.limitLightmapCount = src.limitLightmapCount;
        //                        lp.antiAliasingSamples = src.antiAliasingSamples;
        //                        lp.directLightQuality = src.directLightQuality;
        //                        lp.blurRadius = src.blurRadius;
        //                        lp.AOAntiAliasingSamples = src.AOAntiAliasingSamples;
        //                        lp.AOQuality = src.AOQuality;
        //                        lp.maxLightmapCount = src.maxLightmapCount;
        //                        lp.isTransparent = src.isTransparent;
        //                        lp.stitchEdges = src.stitchEdges;
        //                        lp.modellingTolerance = src.modellingTolerance;
        //                        lp.backFaceTolerance = src.backFaceTolerance;
        //                        lp.irradianceQuality = src.irradianceQuality;
        //                        lp.irradianceBudget = src.irradianceBudget;
        //                        lp.clusterResolution = src.clusterResolution;
        //                        lp.resolution = src.resolution;
        //                        lp.systemTag = src.systemTag;
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}
        private void UnBindLightmapData()
        {

            if (sceneLocalConfig.clearLightmapAfterBake)
            {
                // UnityEngine.Object lightmapSetting = EditorCommon.CallInternalFunction (typeof (LightmapEditorSettings), "GetLightmapSettings", true, true, false, null, null) as UnityEngine.Object;
                // if (lightmapSetting != null)
                // {
                //     SerializedProperty sp = CommonAssets.GetSerializeProperty (lightmapSetting, "m_LightingDataAsset");
                //     if (sp != null)
                //     {
                //         sp.objectReferenceValue = null;
                //         sp.serializedObject.ApplyModifiedProperties ();
                //     }
                // }

                Lightmapping.ClearLightingDataAsset();
                Lightmapping.Clear();
            }

        }
        private void SetLightmapConfig(string name)
        {
            if (lightmapConfigData != null)
            {
                for (int i = 0; i < lightmapConfigData.configs.Count; ++i)
                {
                    var config = lightmapConfigData.configs[i];
                    if (config.name == name)
                    {
                        SetLightmapConfig(config);
                        return;
                    }
                }
            }
        }
        private void SetLightmapConfig(LightmapConfig config)
        {
            if (config.ambientType == UnityEngine.Rendering.AmbientMode.Flat)
            {
                RenderSettings.ambientLight = config.ambientLight;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            }
            else if (config.ambientType == UnityEngine.Rendering.AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = config.ambientSkyColor;
                RenderSettings.ambientEquatorColor = config.ambientEquatorColor;
                RenderSettings.ambientGroundColor = config.ambientGroundColor;
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            }
            else if (config.ambientType == UnityEngine.Rendering.AmbientMode.Skybox)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientIntensity = config.skyIntensity;
                UnityEngine.Rendering.Universal.Skybox.EditorOverrideMaterial = config.skyMaterial;
                RenderSettings.skybox = config.skyMaterial;
            }

            LightingSettings ls = Lightmapping.lightingSettings;

            if (config.enableShadowMask)
            {
                ls.mixedBakeMode = MixedLightingMode.Shadowmask;
            }
            else
            {
                ls.mixedBakeMode = MixedLightingMode.IndirectOnly;
            }


            //ls. = config.mis;
            ls.directSampleCount = config.directSampleCount;
            ls.indirectSampleCount = config.indirectSampleCount;
            ls.environmentSampleCount = config.environmentSampleCount;
            ls.maxBounces = config.bounces;
            ls.lightmapResolution = config.bakeResolution;
            ls.lightmapPadding = config.padding;
            ls.lightmapMaxSize = config.maxAtlasSize;
            ls.ao = config.enableAmbientOcclusion;
            ls.aoMaxDistance = config.aoMaxDistance;
            ls.aoExponentIndirect = config.aoExponentIndirect;
            ls.aoExponentDirect = config.aoExponentDirect;
            ls.albedoBoost = config.bounceBoost;
            ls.indirectScale = config.indirectOutputScale;

            //Blur Params
            isBlurShadowmask = config.blurShadowmask;
            blurOffset = config.blurOffset;
            blurIterationCount = config.blurIterationCount;
        }


        private void ClearBake(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
            {
                var monos = EditorCommon.GetScripts<MeshRenderObject>(trans.gameObject);
                for (int i = 0; i < monos.Count; ++i)
                {
                    var mro = monos[i] as MeshRenderObject;
                    mro.ClearLightmap();
                }
            }
            else
            {
                EditorCommon.EnumChildObject(trans, param, context.fun);
            }
        }
        private void ClearTerrainBake(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            TerrainObject to = trans.GetComponent<TerrainObject>();
            if (to != null)
            {
                to.ClearLightmap();
            }
        }
        private void ClearBake()
        {
            lightmapBakeContext.fun = ClearBake;
            EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);

            lightmapBakeContext.fun = ClearTerrainBake;
            EditorCommon.EnumPath(EditorSceneObjectType.MeshTerrain, lightmapBakeContext.fun, lightmapBakeContext);
        }

        private void CancelBake()
        {
            CleanBakeObjct();
            EditorUtility.ClearProgressBar();
            Lightmapping.Cancel();
            lightmapBakeContext.isBaking = false;
            AmbientModify.autoRefreshSystemValue = true;
            SkyboxModify.autoRefreshSystemValue = true;
            UnityEngine.Rendering.Universal.Skybox.EditorOverrideMaterial = null;

            BakeRecover();
        }

        private void UseCombineLightmap()
        {
            if (isUseCombineLightmap)
            {
                return;
            }
            
            LightmapCombineManager.Instance.SetUseCombineLightmap(true);
            RecoverLightmapRes();
            
            Shader.SetGlobalFloat("_UseCombineLightmap", 1f);
            Shader.EnableKeyword("_USE_COMBINE_LIGHTMAP");
            
            isUseCombineLightmap = true;
            
            Debug.Log("UseCombineLightmap");
        }

        private void UseDefaultLightmap()
        {
            if (!isUseCombineLightmap)
            {
                return;
            }

            LightmapCombineManager.Instance.SetUseCombineLightmap(false);
            RecoverLightmapRes();
            
            Shader.SetGlobalFloat("_UseCombineLightmap", 0f);
            Shader.DisableKeyword("_USE_COMBINE_LIGHTMAP");
            
            isUseCombineLightmap = false;
            Debug.Log("UseDefaultLightmap" +
                      "");
        }

        /// <summary>
        /// 光照贴图转换成混合贴图
        /// </summary>
        private void ConvertLightmap()
        {
            LightmapVolumn[] lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
            LightmapCombineManager.Instance.ConvertLightmap(lightmapVolumns);
        }
        
        /// <summary>
        /// 混合贴图转换成原来的光照贴图
        /// </summary>
        private void RecoverOrginLightmap()
        {
            LightmapVolumn[] lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
            LightmapCombineManager.Instance.RecoverLightmap(lightmapVolumns);
        }
        
        private void FormatLightmap()
        {
            LightmapVolumn[] lightmapVolumns = FindObjectsOfType<LightmapVolumn>(true);
            string currentSceneName = SceneManager.GetActiveScene().name;
            LightmapCombineManager.Instance.FormatLightmap(lightmapVolumns, currentSceneName);
        }

        List<UnityEngine.LightmapData> allLightMaps = new List<UnityEngine.LightmapData>();
        private void RecoverLightmapRes(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
            if (volumn != null)
            {
                var res = volumn.res;
                if (res == null) return;

                int nStartLightmapAdd = allLightMaps.Count;

                for (int i = 0; i < res.Length; ++i)
                {
                    var lightmap = res[i];
                    if (lightmap == null || lightmap.color == null) continue;

                    lightmap.combine = lightmap.color;
                    // string lightmapName = lightmap.combine.name;
                    // string src = string.Format ("{0}/Scene/{1}/{2}.exr",
                    //     AssetsConfig.instance.ResourcePath,
                    //     sceneContext.name,
                    //     lightmapName);
                    // string des = string.Format ("{0}/SceneLightmapBackup/{1}/{2}.exr",
                    //     sceneContext.configDir,
                    //     volumn.volumnName,
                    //     lightmapName);
                    // if (!File.Exists (des) && File.Exists (src))
                    // {
                    //     AssetDatabase.MoveAsset (src, des);
                    // }

                    // string targetLightmapPath = string.Format ("{0}/Scene/{1}/Lightmap_{2}-{3}.exr",
                    //     AssetsConfig.instance.ResourcePath, sceneContext.name,
                    //     volumn.volumnName, i);

                    // if (!File.Exists (targetLightmapPath) && lightmap.color != null)
                    // {
                    //     string path = AssetDatabase.GetAssetPath (lightmap.color);
                    //     AssetDatabase.CopyAsset (path, targetLightmapPath);
                    //     lightmap.combine = AssetDatabase.LoadAssetAtPath<Texture2D> (targetLightmapPath);
                    // }


                    UnityEngine.LightmapData newItemData = new UnityEngine.LightmapData();

                    if (lightmap.colorCombineShadowMask != null && LightmapCombineManager.Instance.CheckIsUseCombineLightmap())
                    {
                        newItemData.lightmapColor = lightmap.colorCombineShadowMask;
                    }
                    else
                    {
                        newItemData.lightmapColor = lightmap.color;
                    }
                    
                    newItemData.lightmapDir = lightmap.dir;
                    newItemData.shadowMask = lightmap.shadowMask;
                    allLightMaps.Add(newItemData);

                }

                var monos = EditorCommon.GetScripts<MeshRenderObject>(trans.gameObject);
                if (monos.Count > 0)
                {
                    for (int i = 0; i < monos.Count; ++i)
                    {
                        var lo = monos[i] as ILightmapObject;
                        lo.BindLightMap(res, nStartLightmapAdd);
                    }
                }

            }
            else
            {
                if (!EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    EditorCommon.EnumChildObject(trans, param, context.fun);
                }
            }
        }
        private void RecoverLightmapRes()
        {

            /*List<LightmapVolumn> allDatas = new List<LightmapVolumn>();
            EditorCommon.EnumRootObject((tf, data) =>
            {
                LightmapVolumn[] ts = tf.transform.GetComponentsInChildren<LightmapVolumn>();
                if (ts == null || ts.Length == 0) return;
                allDatas.AddRange(ts);
            });*/

            LightmapVolumn.TryCheckLightmapRenderCorrect(FindObjectsOfType<LightmapVolumn>(true));
            LightmapVolumn.RenderLightmaps(FindObjectsOfType<LightmapVolumn>(true));
            //LightmapVolumn.RenderLightmaps(allDatas.ToArray());

            SceneAssets.SceneModify(true);

            //allLightMaps.Clear();

            //lightmapBakeContext.fun = RecoverLightmapRes;
            //EditorCommon.EnumPath (EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);
            //EditorCommon.EnumPath (EditorSceneObjectType.MeshTerrain, lightmapBakeContext.fun, lightmapBakeContext);
            //if (GlobalContex.ee != null)
            //{
            //    GlobalContex.ee.UpdateMatObject ();
            //}
            //LightmapVolumnData lvd;
            //SceneSerialize.LoadLightmapVolumnData (ref sceneContext, false, out lvd);
            //if (lvd != null)
            //{
            //    for (int i = 0; i < lvd.volumns.Count; ++i)
            //    {
            //        var v = lvd.volumns[i];
            //        for (int j = 0; j < v.ligthmapRes.Count; ++j)
            //        {
            //            var res = v.ligthmapRes[j];
            //            if (res.combine != null)
            //            {
            //                res.color = res.combine;
            //            }
            //        }
            //    }
            //    SceneSerialize.SaveLightmapVolumnData (ref sceneContext, lvd);
            //}

            //LightmapSettings.lightmaps = allLightMaps.ToArray();
        }

        private void LoadTerrainLightmap()
        {
            var trans = TerrainSystem.system.GetMeshTerrain();
            if (trans != null)
            {

                var monos = EditorCommon.GetScripts<TerrainObject>(trans.gameObject);
                if (monos.Count > 0)
                {
                    string sceneName = SceneManager.GetActiveScene().name;
                    for (int i = 0; i < monos.Count; ++i)
                    {
                        var obj = monos[i] as TerrainObject;

                        LightmapVolumn volumn;
                        if (obj.TryGetComponent(out volumn))
                        {
                            string folder = SceneAssets.CreateFolder(sceneContext.configDir, "SceneLightmapBackup");
                            folder = SceneAssets.CreateFolder(folder, volumn.volumnName);
                            if (volumn.res == null || volumn.res.Length != 1)
                            {
                                volumn.res = new LigthmapRes[1];
                            }
                            var lr = volumn.res[0];
                            if (lr == null)
                            {
                                lr = new LigthmapRes();
                                volumn.res[0] = lr;
                            }
                            string targetBakeCombineLightmap = string.Format("{0}/{1}_Lightmap_Combine_{2}-0.tga", folder, sceneName, volumn.volumnName);
                            string targetBakeLightmap = string.Format("{0}/{1}_Lightmap_{1}-0.exr", folder, sceneName, volumn.volumnName);
                            if (!File.Exists(targetBakeLightmap))
                            {
                                DebugLog.AddErrorLog2("terrain lightmap not exist:{0}", targetBakeLightmap);
                            }
                            if (File.Exists(targetBakeCombineLightmap))
                            {
                                lr.colorCombineShadowMask = AssetDatabase.LoadAssetAtPath<Texture2D>(targetBakeLightmap);
                                lr.color = lr.colorCombineShadowMask;
                            }
                            else
                            {
                                lr.combine = AssetDatabase.LoadAssetAtPath<Texture2D>(targetBakeLightmap);
                                lr.color = lr.combine;
                            }
                            
                            obj.SetLightmapRes(lr.combine, null, null, null);
                            obj.LightmapVolumnIndex = 0;
                            obj.SetLightmapData(0, new Vector4(1, 1, 0, 0));
                        }
                    }
                }
            }
        }
        private void SetReflectionProbe(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            MeshRenderer mr;
            if (trans.TryGetComponent(out mr))
            {
                //GameObjectUtility.SetStaticEditorFlags(trans.gameObject, StaticEditorFlags.ReflectionProbeStatic);
            }
            EditorCommon.EnumChildObject(trans, param, context.fun);
        }
        private void SetReflectionProbe()
        {
            lightmapBakeContext.fun = SetReflectionProbe;
            EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);
            EditorCommon.EnumPath(EditorSceneObjectType.Prefab, lightmapBakeContext.fun, lightmapBakeContext);
            EditorCommon.EnumPath(EditorSceneObjectType.Instance, lightmapBakeContext.fun, lightmapBakeContext);
            EditorCommon.EnumPath(EditorSceneObjectType.MeshTerrain, lightmapBakeContext.fun, lightmapBakeContext);
        }
        #endregion

        #region light probes
        void TestExportLightProbes()
        {
            var bakevolumn = lightmapBakeContext.lightmapVolumns[lightmapBakeContext.currentBakeIndex];
            var volumn = bakevolumn.volumn;
            string folder = SceneAssets.CreateFolder(sceneContext.configDir, "SceneLightmapBackup");
            folder = SceneAssets.CreateFolder(folder, volumn.volumnName);
            ExportLightProbes(volumn, folder);
        }
        void ExportLightProbes(LightmapVolumn volumn, string dir)
        {
            var currentProbes = LightmapSettings.lightProbes;
            if (currentProbes != null)
            {
                if (string.IsNullOrEmpty(dir))
                {
                    dir = sceneContext.configDir;
                }
                var newProbe = UnityEngine.Object.Instantiate(currentProbes);
                newProbe.name = string.Format("{0}_LightProbes", volumn.name);
                string path = string.Format("{0}/{1}.asset", dir, newProbe.name);
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
                EditorCommon.CreateAsset<LightProbes>(path, ".asset", newProbe);
                //if (volumn != null && volumn.chunkIndex >= 0)
                //{
                //    int x = volumn.chunkIndex % widthCount;
                //    int z = volumn.chunkIndex / widthCount;
                //    var probes = LightMapSystem.system.resData.workspace;
                //    if (probes != null)
                //    {
                //        string name = string.Format("LightProbeArea_{0}_{1}", x.ToString(), z.ToString());
                //        var trans = probes.Find(name);
                //        if (trans != null)
                //        {
                //            LightprobeArea lpa;
                //            if (trans.TryGetComponent(out lpa))
                //            {
                //                lpa.lightProbes = AssetDatabase.LoadAssetAtPath<LightProbes>(path);
                //            }
                //        }
                //    }
                //}
                volumn.probes = AssetDatabase.LoadAssetAtPath<LightProbes>(path);
            }
        }
        #endregion

        #region serialize



        private void PreBindLightmap(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
            if (volumn != null && volumn.res != null)
            {
                volumn.dataIndex = -1;
                var lvd = context.lvd;
                var ld = new LightmapData();
                volumn.dataIndex = lvd.volumns.Count;

                var monos = EditorCommon.GetScripts<MeshRenderObject>(trans.gameObject);
                for (int j = 0; j < monos.Count; ++j)
                {
                    var mro = monos[j] as MeshRenderObject;
                    mro.BindLightMap(volumn.dataIndex);
                }

                lvd.volumns.Add(ld);
                if (volumn.res != null)
                {
                    ld.name = volumn.name;
                    ld.ligthmapRes.AddRange(volumn.res);
                }
            }
            else
            {
                if (!EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    EditorCommon.EnumChildObject(trans, param, context.fun);
                }
            }
        }
        private void SaveTerrainLightmap(Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn>();
            TerrainObject to = trans.GetComponent<TerrainObject>();
            if (volumn != null && volumn.res != null && to != null)
            {

                var lvd = context.lvd;
                volumn.dataIndex = lvd.volumns.Count;
                to.BindLightMap(volumn.dataIndex);
                var ld = new LightmapData();
                lvd.volumns.Add(ld);
                if (volumn.res != null)
                {
                    ld.name = volumn.volumnName;
                    ld.ligthmapRes.AddRange(volumn.res);
                }
            }
        }

        private void PreBindLightmap()
        {
            lightmapBakeContext.fun = PreBindLightmap;
            lightmapBakeContext.sceneName = sceneContext.name;
            if (lightmapBakeContext.lvd == null)
            {
                lightmapBakeContext.lvd = LightmapVolumnData.CreateInstance<LightmapVolumnData>();
            }
            lightmapBakeContext.lvd.volumns.Clear();
            EditorCommon.EnumPath(EditorSceneObjectType.StaticPrefab, lightmapBakeContext.fun, lightmapBakeContext);
            lightmapBakeContext.fun = SaveTerrainLightmap;
            EditorCommon.EnumPath(EditorSceneObjectType.MeshTerrain, lightmapBakeContext.fun, lightmapBakeContext);

            SceneSerialize.SaveLightmapVolumnData(ref sceneContext, lightmapBakeContext.lvd);
        }

        #endregion

        #region debug

        #endregion

        private static void CombineLightmap(string path, Texture2D color, Texture2D shadowmask)
        {

            Material tmp = new Material(AssetsConfig.instance.CombineLightmap);
            tmp.SetTexture("_LightmapColor", color);
            tmp.SetTexture("_ShadowMask", shadowmask);
            RenderTexture rt0 = new RenderTexture(color.width, color.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "LightmapCombine Tex",
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                autoGenerateMips = false,
                useMipMap = false
            };
            rt0.Create();
            TextureAssets.BeginDrawRT();
            TextureAssets.DrawRT(rt0, tmp);
            TextureAssets.EndDrawRT();
            CommonAssets.CreateAsset<Texture2D>(path, ".exr", rt0);
            UnityEngine.Object.DestroyImmediate(rt0);
            UnityEngine.Object.DestroyImmediate(tmp);
        }

        void UpdateLightmap()
        {
            switch (lightmapOp)
            {
                case OpLightmapType.OpClearBake:
                    ClearBake();
                    break;
                case OpLightmapType.OpCancelBake:
                    CancelBake();
                    break;
                case OpLightmapType.OpTestPrepare:
                    PrepareBakeLightMap();
                    break;
                case OpLightmapType.OpTestBake:
                    SetBakedPipelineType(lightmapBakeContext.currentBakeIndex);
                    BakeCurrentVolumn();
                    break;
                case OpLightmapType.OpTryBind:
                    RecoverLightmapRes();
                    break;
                case OpLightmapType.OpLoadTerrainLightmap:
                    LoadTerrainLightmap();
                    break;
                case OpLightmapType.OpSetReflectionProbe:
                    SetReflectionProbe();
                    break;
                case OpLightmapType.OpBakeAll:
                    BakeAllVolumns();
                    break;
                case OpLightmapType.OpTestExportLightProbes:
                    TestExportLightProbes();
                    break;
                case OpLightmapType.OpUseCombineLightmap:
                    UseCombineLightmap();
                    break;
                case OpLightmapType.OpUseDefaultLightmap:
                    UseDefaultLightmap();
                    break;
                case OpLightmapType.OpConvertLightmap2Combine:
                    ConvertLightmap();
                    break;
                case OpLightmapType.OpRecoverOrginLightmap:
                    RecoverOrginLightmap();
                    break;
                case OpLightmapType.OpFormatLightmap:
                    FormatLightmap();
                    break;

            }
            lightmapOp = OpLightmapType.OpNone;

            if (lightmapBakeContext != null && lightmapBakeContext.bakeFinishIndex >= 0)
            {
                lightmapBakeContext.bakeFinishIndex = -1;
                FinishBakeObjct(lightmapBakeContext.currentBakeVolumn);
                
                if (isUseDoubleBakePipeline && curBakedState == NewBakedState.singleSideState)
                {
                    curBakedState = NewBakedState.doubleSideState;
                    BakeCurrentVolumn();
                }
                else
                {
                    bool bakeing = false;
                    if (lightmapBakeContext.bakeType == LightmapBakingType.Total)
                    {
                        bakeing = BakeNext();
                    }

                    if (!bakeing)
                    {
                        lightmapBakeContext.isBaking = false;
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Finish", "Bake Finish!", "OK");
                    }
                }
            }
        }

        private void OnLightmapBakeGUI(string info)
        {
            sceneLocalConfig.bakeFolder = EditorGUILayout.Foldout(sceneLocalConfig.bakeFolder, info);
            if (!sceneLocalConfig.bakeFolder)
                return;

            EditorCommon.BeginGroup("Bake Setting");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ClearBake", GUILayout.MaxWidth(100)))
            {
                lightmapOp = OpLightmapType.OpClearBake;
            }
            if (GUILayout.Button("Bake", GUILayout.MaxWidth(100)))
            {
                lightmapOp = OpLightmapType.OpBake;
            }

            if (GUILayout.Button("CancelBake", GUILayout.MaxWidth(100)))
            {
                lightmapOp = OpLightmapType.OpCancelBake;
            }
            // if (GUILayout.Button("UseCombineLightmap", GUILayout.MaxWidth(150)))
            // {
            //     lightmapOp = OpLightmapType.OpUseCombineLightmap;
            // }
            // if (GUILayout.Button("UseDefaultLightmap", GUILayout.MaxWidth(150)))
            // {
            //     lightmapOp = OpLightmapType.OpUseDefaultLightmap;
            // }
            if (GUILayout.Button("ConvertLightmap", GUILayout.MaxWidth(150)))
            {
                lightmapOp = OpLightmapType.OpConvertLightmap2Combine;
            }
            if (GUILayout.Button("RecoverOrginLightmap", GUILayout.MaxWidth(150)))
            {
                lightmapOp = OpLightmapType.OpRecoverOrginLightmap;
            }
            if (GUILayout.Button("FormatLightmap", GUILayout.MaxWidth(150)))
            {
                lightmapOp = OpLightmapType.OpFormatLightmap;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorCommon.EndGroup();

            EditorCommon.BeginGroup("Deug Setting");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("TryBindLightmap", GUILayout.MaxWidth(160)))
            {
                lightmapOp = OpLightmapType.OpTryBind;
            }
            if (GUILayout.Button("LoadTerrainLightmap", GUILayout.MaxWidth(160)))
            {
                lightmapOp = OpLightmapType.OpLoadTerrainLightmap;
            }
            if (GUILayout.Button("SetReflectionProbe", GUILayout.MaxWidth(160)))
            {
                lightmapOp = OpLightmapType.OpSetReflectionProbe;
            }
            // if (GUILayout.Button ("TryBindBakeLightmap", GUILayout.MaxWidth (160)))
            // {
            //     lightmapOp = OpLightmapType.OpTryBindBake;
            // }
            // displayBakeryTool = EditorGUILayout.ToggleLeft("Display Bakery Tool", displayBakeryTool, GUILayout.MaxWidth(160));
            // doubleSidedGI = EditorGUILayout.ToggleLeft("DoubleSidedGI", doubleSidedGI, GUILayout.MaxWidth(130));
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("PrepareBake", GUILayout.MaxWidth(160)))
            {
                lightmapOp = OpLightmapType.OpTestPrepare;
            }
            sceneLocalConfig.clearLightmapAfterBake = EditorGUILayout.Toggle("ClearSystemLightmap", sceneLocalConfig.clearLightmapAfterBake);
            EditorGUILayout.EndHorizontal();

            if (lightmapBakeContext.lightmapVolumns.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("BakeAll", GUILayout.MaxWidth(80)))
                {
                    lightmapOp = OpLightmapType.OpBakeAll;
                }

                if (GUILayout.Button(strToggleAllBakeDes, GUILayout.MaxWidth(80)))
                {
                    ToggleAllBakeArea();
                }
                
                EditorGUILayout.EndHorizontal();

                EditorCommon.BeginScroll(ref volumnScroll, lightmapBakeContext.lightmapVolumns.Count);
                for (int i = 0; i < lightmapBakeContext.lightmapVolumns.Count; ++i)
                {
                    var volumn = lightmapBakeContext.lightmapVolumns[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(volumn.name, GUILayout.MaxWidth(200));
                    /*if (GUILayout.Button ("Test", GUILayout.MaxWidth (80)))
                    {
                        lightmapBakeContext.currentBakeIndex = i;
                        lightmapBakeContext.realBake = false;
                        lightmapBakeContext.bakeType = LightmapBakingType.Single;
                        lightmapOp = OpLightmapType.OpTestBake;
                    }*/

                    if (GUILayout.Button("Bake", GUILayout.MaxWidth(80)))
                    {
                        lightmapBakeContext.currentBakeIndex = i;
                        lightmapBakeContext.realBake = true;
                        lightmapBakeContext.bakeType = LightmapBakingType.Single;
                        lightmapOp = OpLightmapType.OpTestBake;
                    }
                    var v = volumn.volumn;
                    v.configName = EditorGUILayout.TextField(v.configName, GUILayout.MaxWidth(200));
                    volumn.bake = EditorGUILayout.Toggle("", volumn.bake, GUILayout.MaxWidth(60));

                    if (GUILayout.Button("ExportLightProbe", GUILayout.MaxWidth(160)))
                    {
                        lightmapBakeContext.currentBakeIndex = i;
                        lightmapOp = OpLightmapType.OpTestExportLightProbes;
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorCommon.EndScroll();
            }

            //if (displayBakeryTool)
            //{
            //    EditorGUILayout.BeginVertical("BOX");
            //    BakeryWindow.DrawWindow();
            //    EditorGUILayout.EndVertical();
            //}
            EditorCommon.EndGroup();
            if (lightmapConfigData != null)
            {
                EditorCommon.BeginGroup("LightmapConfig");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
                {
                    EditorCommon.SaveAsset(lightmapConfigData);
                }
                if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                {
                    lightmapConfigData.configs.Add(new LightmapConfig());
                }
                EditorGUILayout.EndHorizontal();
                EditorCommon.BeginScroll(ref lightmapConfigScroll, lightmapConfigData.configs.Count);
                int removeIndex = -1;
                LightmapConfig editconfig = null;
                for (int i = 0; i < lightmapConfigData.configs.Count; ++i)
                {
                    var config = lightmapConfigData.configs[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(config.name, GUILayout.MaxWidth(200));
                    bool edit = i == sceneLocalConfig.editLightmapConfig;

                    if (edit)
                    {
                        if (GUILayout.Button("UnEdit", GUILayout.MaxWidth(80)))
                        {
                            sceneLocalConfig.editLightmapConfig = -1;
                            UnityEngine.Rendering.Universal.Skybox.EditorOverrideMaterial = null;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Edit", GUILayout.MaxWidth(80)))
                        {
                            sceneLocalConfig.editLightmapConfig = i;
                        }
                    }
                    edit = i == sceneLocalConfig.editLightmapConfig;
                    if (edit)
                    {
                        editconfig = config;
                    }
                    if (GUILayout.Button("Test", GUILayout.MaxWidth(80)))
                    {
                        SetLightmapConfig(editconfig);
                    }
                    if (GUILayout.Button("Remove", GUILayout.MaxWidth(80)))
                    {
                        removeIndex = i;
                        if (edit)
                            editconfig = null;
                    }
                    EditorGUILayout.EndHorizontal();

                }
                EditorCommon.EndScroll();
                if (editconfig != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    editconfig.name = EditorGUILayout.TextField("LightmapConfigName", editconfig.name, GUILayout.MaxWidth(350));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    editconfig.useDoubleBakePipeline = EditorGUILayout.Toggle("UseDoubleBakePipeline", editconfig.useDoubleBakePipeline);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    editconfig.blurShadowmask = EditorGUILayout.Toggle("BlurShadowmask", editconfig.blurShadowmask);
                    EditorGUILayout.EndHorizontal();

                    if (editconfig.blurShadowmask)
                    {
                        // if (EditorGUILayout.Foldout(editconfig.blurShadowmask, "BlurParams"))
                        {
                            EditorGUI.indentLevel += 1;
                            editconfig.blurIterationCount = EditorGUILayout.IntSlider("BlurIterationCount", editconfig.blurIterationCount,1,10, GUILayout.MaxWidth(300f));
                            editconfig.blurOffset = EditorGUILayout.Slider("BlurOffset", editconfig.blurOffset, 0f, 10f, GUILayout.MaxWidth(300f));
                            EditorGUI.indentLevel -= 1;
                        }
                    }
                    
                    
                    EditorGUILayout.BeginHorizontal();
                    editconfig.ambientType = (UnityEngine.Rendering.AmbientMode)EditorGUILayout.EnumPopup("AmbientType", editconfig.ambientType);
                    EditorGUILayout.EndHorizontal();

                    if (editconfig.ambientType == UnityEngine.Rendering.AmbientMode.Flat)
                    {
                        EditorGUILayout.BeginHorizontal();
                        editconfig.ambientLight = EditorGUILayout.ColorField(new GUIContent("Ambient Color"), editconfig.ambientLight, false, false, true);
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (editconfig.ambientType == UnityEngine.Rendering.AmbientMode.Trilight)
                    {
                        EditorGUILayout.BeginHorizontal();
                        editconfig.ambientSkyColor = EditorGUILayout.ColorField(new GUIContent("Sky Color"), editconfig.ambientSkyColor, false, false, true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        editconfig.ambientEquatorColor = EditorGUILayout.ColorField(new GUIContent("Equator Color"), editconfig.ambientEquatorColor, false, false, true);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        editconfig.ambientGroundColor = EditorGUILayout.ColorField(new GUIContent("Ground Color"), editconfig.ambientGroundColor, false, false, true);
                        EditorGUILayout.EndHorizontal();
                    }
                    else if (editconfig.ambientType == UnityEngine.Rendering.AmbientMode.Skybox)
                    {
                        EditorGUILayout.BeginHorizontal();
                        editconfig.skyIntensity = EditorGUILayout.Slider("Intensity Multiplier", editconfig.skyIntensity, 0, 8);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        editconfig.skyMaterial = EditorGUILayout.ObjectField("SkyBox", editconfig.skyMaterial, typeof(Material), false) as Material;
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    editconfig.mis = EditorGUILayout.Toggle("Multiple Importance Sampling", editconfig.mis);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.directSampleCount = EditorGUILayout.IntField("Direct Samples", editconfig.directSampleCount);
                    if (editconfig.directSampleCount < 1)
                        editconfig.directSampleCount = 1;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.indirectSampleCount = EditorGUILayout.IntField("Indirect Samples", editconfig.indirectSampleCount);
                    if (editconfig.indirectSampleCount < 8)
                        editconfig.indirectSampleCount = 8;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.environmentSampleCount = EditorGUILayout.IntField("Environment Samples", editconfig.environmentSampleCount);
                    if (editconfig.environmentSampleCount < 8)
                        editconfig.environmentSampleCount = 8;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.bounces = EditorGUILayout.IntSlider("Bounces", editconfig.bounces, 0, 4);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.bakeResolution = EditorGUILayout.FloatField("Lightmap Resolution", editconfig.bakeResolution);
                    if (editconfig.bakeResolution < 0.0001f)
                        editconfig.bakeResolution = 0.0001f;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.padding = EditorGUILayout.IntField("Lightmap Padding", editconfig.padding);
                    if (editconfig.padding < 2)
                        editconfig.padding = 2;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.maxAtlasSize = EditorGUILayout.IntPopup("Lightmap Size", editconfig.maxAtlasSize, LightmapSizeStr, LightmapSizeValues);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.enableAmbientOcclusion = EditorGUILayout.Toggle("Ambient Occlusion", editconfig.enableAmbientOcclusion);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.aoMaxDistance = EditorGUILayout.FloatField("Max Distance", editconfig.aoMaxDistance);
                    if (editconfig.aoMaxDistance < 0)
                        editconfig.aoMaxDistance = 0;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.aoExponentIndirect = EditorGUILayout.Slider("Indirect Contribution", editconfig.aoExponentIndirect, 0.0f, 10.0f);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.aoExponentDirect = EditorGUILayout.Slider("Direct Contribution", editconfig.aoExponentDirect, 0.0f, 10.0f);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.indirectOutputScale = EditorGUILayout.Slider("Indirect Intensity", editconfig.indirectOutputScale, 0, 5);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.bounceBoost = EditorGUILayout.Slider("Albedo Boost", editconfig.bounceBoost, 1, 10);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    editconfig.enableShadowMask = EditorGUILayout.Toggle("ShadowMask", editconfig.enableShadowMask);
                    EditorGUILayout.EndHorizontal();

                }
                if (removeIndex >= 0)
                {
                    lightmapConfigData.configs.RemoveAt(removeIndex);
                }
                EditorCommon.EndGroup();
            }

        }

        private bool bToggleAllBakeState = true;
        private string strToggleAllBakeDes="隐藏";

        private void ToggleAllBakeArea()
        {
            bToggleAllBakeState = !bToggleAllBakeState;
            strToggleAllBakeDes = bToggleAllBakeState ? "隐藏" : "显示";

            for (int i = 0; i < lightmapBakeContext.lightmapVolumns.Count; ++i)
            {
                var volumn = lightmapBakeContext.lightmapVolumns[i];
                var v = volumn.volumn;
                if (string.IsNullOrEmpty(v.configName)&&bToggleAllBakeState) continue;

                volumn.bake = bToggleAllBakeState;
            }
        }
    }
}
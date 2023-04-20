using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public class LightMapSystem : SceneResProcess
    {
        class LightmapBakeVolumn
        {
            public string name;
            public bool bake = true;
            public LightmapVolumn volumn = null;
            public List<ILightmapObject> bakeObjects = new List<ILightmapObject> ();
            public int[] aroundVolumn = new int[9];
            public LightmapBakeVolumn ()
            {
                for (int i = 0; i < aroundVolumn.Length; ++i)
                {
                    aroundVolumn[i] = -1;
                }
            }
        }

        class LightmapBakeContext
        {
            public List<LightmapBakeVolumn> lightmapVolumns = new List<LightmapBakeVolumn> ();
            public List<MeshRenderer> unBakeObjects = new List<MeshRenderer> ();
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
            public List<Transform> transTmp = new List<Transform> ();
            public string sceneName;
            public Vector2 xMinMax = new Vector2 (0, 0);
            public Vector2 zMinMax = new Vector2 (0, 0);
            //public List<RenderBatch> lightProbeEditBatch = new List<RenderBatch> ();
        }
        private LightmapBakeContext context;
        private LightmapConfigData config;
        private AreaData areaData;

        public static LightMapSystem system;
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            context = new LightmapBakeContext ();
            context.lightmapBakeEnd = LightMapBakeEnd;
            string configPath = string.Format ("{0}/{1}_LightmapConfig.asset",
                sceneContext.configDir, sceneContext.name);
            config = EditorCommon.LoadAsset<LightmapConfigData> (configPath);
            if (!File.Exists (configPath))
            {
                EditorCommon.SaveAsset (configPath, config);
            }
            string path = string.Format ("{0}/{1}_AreaData.asset", sceneContext.configDir, sceneContext.name);
            if (File.Exists (path))
            {
                areaData = EditorCommon.LoadAsset<AreaData> (path);
            }
            system = this;
        }

        public override void PreSerialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            context.fun = SerializeObjLightmap;
            context.sceneName = sceneContext.name;
            if (context.lvd == null)
            {
                context.lvd = LightmapVolumnData.CreateInstance<LightmapVolumnData> ();
            }
            context.lvd.volumns.Clear ();
            EditorCommon.EnumPath (SceneResConfig.instance.bakeObjFolderName, context.fun, context);
            context.fun = SerializeTerrainLightmap;
            EditorCommon.EnumPath (SceneResConfig.instance.bakeTerrainFolderName, context.fun, context);
            SceneSerialize.SaveLightmapVolumnData (ref sceneContext, context.lvd);
        }

        private void LightMapBakeEnd ()
        {
            Lightmapping.bakeCompleted -= context.lightmapBakeEnd;
            context.bakeFinishIndex = context.currentBakeIndex;
            UnityEngine.Rendering.Universal.Skybox.EditorOverrideMaterial = null;
            if (context.bakeFinishIndex < context.lightmapVolumns.Count)
            {
                var volumn = context.lightmapVolumns[context.bakeFinishIndex];
                DebugLog.AddEngineLog2 ("{0} bake finish", volumn.name);
            }
        }

        private void SerializeObjLightmap (Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn> ();
            if (volumn != null && volumn.res != null)
            {
                volumn.dataIndex = -1;
                var lvd = context.lvd;
                var ld = new LightmapData ();
                volumn.dataIndex = lvd.volumns.Count;
                var monos = EditorCommon.GetScripts<MeshRenderObject> (trans.gameObject);
                for (int j = 0; j < monos.Count; ++j)
                {
                    var mro = monos[j] as MeshRenderObject;
                    mro.BindLightMap (volumn.dataIndex);
                }

                lvd.volumns.Add (ld);
                if (volumn.res != null)
                {
                    ld.name = volumn.name;
                    ld.ligthmapRes.AddRange (volumn.res);
                }
            }
            else
            {
                if (!EditorCommon.IsPrefabOrFbx (trans.gameObject))
                {
                    EditorCommon.EnumChildObject (trans, param, context.fun);
                }
            }
        }

        private void SerializeTerrainLightmap (Transform trans, object param)
        {
            var context = param as LightmapBakeContext;
            LightmapVolumn volumn = trans.GetComponent<LightmapVolumn> ();
            TerrainObject to = trans.GetComponent<TerrainObject> ();
            if (volumn != null && volumn.res != null && to != null)
            {
                var lvd = context.lvd;
                volumn.dataIndex = lvd.volumns.Count;
                to.BindLightMap (volumn.dataIndex);
                var ld = new LightmapData ();
                lvd.volumns.Add (ld);
                if (volumn.res != null)
                {
                    ld.name = volumn.volumnName;
                    ld.ligthmapRes.AddRange (volumn.res);
                }
            }
        }

        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;
            SceneSerialize.LoadLightmapVolumnData (ref sceneContext, false, out ssc.lightMapData);
        }
    }
}
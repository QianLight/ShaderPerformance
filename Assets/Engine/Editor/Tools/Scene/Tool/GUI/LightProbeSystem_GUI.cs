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
    public partial class LightProbeSystem : SceneResProcess
    {
        enum OpType
        {
            OpNone,
            OpBakeLightProbeArea,
            OpEditLightProbeArea,
            OpAddLightProbes,
            OpClearLightProbes,
            OpSaveLightProbes,
            OpPreviewLightProbes,
            OpUnPreviewLightProbes,
            OpHideUnTerrainLayer
        }
        class LightProbeBakeContext
        {
            public Vector2 xMinMax = new Vector2(0, 0);
            public Vector2 zMinMax = new Vector2(0, 0);
        }

        private LightProbeBakeContext lightprobeBakeContext;
        private OpType opType = OpType.OpNone;
        private LightprobeAreaEdit lpae;
        private AreaData areaData;
        private List<Transform> _needhideobjct;
        public override bool HasGUI { get { return true; } }
        public override void InitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            string path = string.Format("{0}/{1}_AreaData.asset", 
                sceneContext.configDir, sceneContext.name);
            if (File.Exists(path))
            {
                areaData = EditorCommon.LoadAsset<AreaData>(path);
            }
            lightprobeBakeContext = new LightProbeBakeContext();
        }
        public override void Update(ref SceneContext sceneContext, object param)
        {
            var editorContext = param as SceneEditContext;
            switch (opType)
            {
                case OpType.OpBakeLightProbeArea:
                    BakeLightProbeArea(ref sceneContext, editorContext);
                    break;
                case OpType.OpEditLightProbeArea:
                    EditLightProbeArea();
                    break;
                case OpType.OpAddLightProbes:
                    AddLightProbes(editorContext);
                    break;
                case OpType.OpClearLightProbes:
                    ClearLightProbes();
                    break;
                case OpType.OpPreviewLightProbes:
                    PreviewLightProbes(true, editorContext);
                    break;
                case OpType.OpUnPreviewLightProbes:
                    PreviewLightProbes(false, editorContext);
                    break;
                case OpType.OpSaveLightProbes:
                    SaveLightProbes();
                    break;
                case OpType.OpHideUnTerrainLayer:
                    HideUnTerrainLayer();
                    break;
            }
            opType = OpType.OpNone;
        }
        public override void OnGUI(ref SceneContext sceneContext, object param, ref Rect rect)
        {
            EditorGUI.indentLevel++;
            var editorContext = param as SceneEditContext;
            var localConfig = editorContext.localConfig;
            var config = editorContext.sc;

            localConfig.lightProbeFolder = EditorGUILayout.Foldout(localConfig.lightProbeFolder, "LightProbe");
            if (localConfig.lightProbeFolder)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Bake LightProbe Area", GUILayout.MaxWidth(160)))
                {
                    opType = OpType.OpBakeLightProbeArea;
                }
                if (areaData != null)
                {
                    if (GUILayout.Button((lpae != null && lpae.editLightProbeArea) ?
                            "UnEdit LightProbe Area" : "Edit LightProbe Area", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpEditLightProbeArea;
                    }
                    if (GUILayout.Button("Add Light Probes", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpAddLightProbes;
                    }
                    if (GUILayout.Button("Clear Light Probes", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpClearLightProbes;
                    }
                    if (GUILayout.Button("Save Probes Data", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpSaveLightProbes;
                    }
                }

                EditorGUILayout.EndHorizontal();
                if (areaData != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Preview Probes Data", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpPreviewLightProbes;
                    }
                    if (GUILayout.Button("UnPreview Probes Data", GUILayout.MaxWidth(160)))
                    {
                        opType = OpType.OpUnPreviewLightProbes;
                    }
                    if (lpae != null && lpae.editLightProbeArea)
                    {
                        if (GUILayout.Button((lpae.showhideunterrainlayer) ?
                         "HideUnTerrainLayers" : "ShowUnTerrainLayers", GUILayout.MaxWidth(160)))
                        {
                            opType = OpType.OpHideUnTerrainLayer;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

            }

            EditorGUI.indentLevel--;
        }

        public override void OnSceneGUI(ref SceneContext sceneContext, object param)
        {
            if (lpae != null)
            {
                lpae.OnSceneGUI();
            }
        }

        void GetLightprobeAreaEdit()
        {
            var go = resData.workspace.gameObject;
            lpae = null;
            if (!go.TryGetComponent(out lpae))
            {
                lpae = go.AddComponent<LightprobeAreaEdit>();
            }
        }

        void BakeLightProbeArea(ref SceneContext sceneContext, SceneEditContext sec)
        {

            lightprobeBakeContext.xMinMax = new Vector2(0, sec.ssc.chunkWidth * sec.ssc.widthCount);
            lightprobeBakeContext.zMinMax = new Vector2(0, sec.ssc.chunkHeight * sec.ssc.heightCount);
            EditorCommon.EnumTransform funPrefabs = null;
            funPrefabs = (trans, param) =>
            {
                var context = param as LightProbeBakeContext;
                MeshRenderObject mro;
                if (trans.TryGetComponent(out mro))
                {
                    {
                        var b = mro.bounds;
                        var min = b.min;
                        var max = b.max;
                        if (min.x < context.xMinMax.x)
                        {
                            context.xMinMax.x = min.x;
                        }
                        if (max.x > context.xMinMax.y)
                        {
                            context.xMinMax.y = max.x;
                        }

                        if (min.z < context.zMinMax.x)
                        {
                            context.zMinMax.x = min.z;
                        }
                        if (max.z > context.zMinMax.y)
                        {
                            context.zMinMax.y = max.z;
                        }
                    }
                }
                EditorCommon.EnumChildObject(trans, param, funPrefabs);
            };

            Transform t = StaticObjectSystem.system.resData.workspace;

            EditorCommon.EnumChildObject(t, lightprobeBakeContext, (trans, param) =>
            {
                funPrefabs(trans, param);
            });

            int xMin = Mathf.FloorToInt(lightprobeBakeContext.xMinMax.x / EngineContext.LightProbeAreaSize);
            int xMax = Mathf.FloorToInt(lightprobeBakeContext.xMinMax.y / EngineContext.LightProbeAreaSize);
            int zMin = Mathf.FloorToInt(lightprobeBakeContext.zMinMax.x / EngineContext.LightProbeAreaSize);
            int zMax = Mathf.FloorToInt(lightprobeBakeContext.zMinMax.y / EngineContext.LightProbeAreaSize);
            GetLightprobeAreaEdit();
            string path = string.Format("{0}/{1}_AreaData.asset", sceneContext.configDir, sceneContext.name);
            lpae.ReSize(xMin, xMax, zMin, zMax, path);
        }

        void EditLightProbeArea()
        {
            GetLightprobeAreaEdit();
            lpae.editLightProbeArea = !lpae.editLightProbeArea;

            if (lpae.editLightProbeArea)
            {
                ShowHideUnusedObject(false);
                lpae.areaData = areaData;
                lpae.Init();
                TerrainSystem.system.PreviewMeshTerrain(true);

                EditorCommon.EnumTransform funPrefabs = null;
                funPrefabs = (trans, param) =>
                {
                    if (!trans.gameObject.activeInHierarchy)
                        return;
                    if (trans.gameObject.layer == DefaultGameObjectLayer.TerrainLayer)
                    {
                        var context = param as LightprobeAreaEdit;
                        MeshRenderObject mro;
                        TerrainObject to;
                        if (trans.TryGetComponent(out mro))
                        {
                            var rb = new RenderBatch();
                            rb.mpbRef = new MaterialPropertyBlock();
                            rb.mesh = mro.GetMesh();
                            rb.matrix = trans.localToWorldMatrix;
                            context.batchs.Add(rb);
                            MeshCollider mc;
                            if (!trans.TryGetComponent(out mc))
                            {
                                mc = trans.gameObject.AddComponent<MeshCollider>();
                            }
                            mc.sharedMesh = rb.mesh;
                        }
                        else if (trans.TryGetComponent(out to))
                        {
                            var rb = new RenderBatch();
                            rb.mpbRef = new MaterialPropertyBlock();
                            rb.mesh = to.GetMesh();
                            rb.matrix = trans.localToWorldMatrix;
                            context.batchs.Add(rb);
                            MeshCollider mc;
                            if (!trans.TryGetComponent(out mc))
                            {
                                mc = trans.gameObject.AddComponent<MeshCollider>();
                            }
                            mc.sharedMesh = rb.mesh;
                        }
                    }
                    else
                    {
                        if (trans.GetComponent<MeshRenderer>())
                            _needhideobjct.Add(trans);

                    }

                    EditorCommon.EnumChildObject(trans, param, funPrefabs);
                };
                Transform t = StaticObjectSystem.system.resData.workspace;

                EditorCommon.EnumChildObject(t, lpae, (trans, param) =>
                {
                    funPrefabs(trans, param);
                });
                //t = commonContext.editorSceneGos[(int) EditorSceneObjectType.Prefab].transform;
                //EditorCommon.EnumChildObject (t, lpae, (trans, param) =>
                //{
                //    funPrefabs (trans, param);
                //});

                t = TerrainSystem.system.GetMeshTerrain();
                EditorCommon.EnumChildObject(t, lpae, (trans, param) =>
                {
                    funPrefabs(trans, param);
                });


            }
            else
            {
                ShowHideUnusedObject(true);
            }
        }
        void ShowHideUnusedObject(bool _active)
        {
            ShowHideCollider(_active);
            if (_needhideobjct == null)
            {
                _needhideobjct = new List<Transform>();
            }
            if (!lpae.showhideunterrainlayer)
                lpae.showhideunterrainlayer = true;
            ShowHideOtherObject(lpae.showhideunterrainlayer);
            _needhideobjct.Clear();
        }

        void ShowHideCollider(bool _active)
        {
            GameObject t = ColliderSystem.system.resData.workspace.gameObject;
            if (t)
            {
                t.SetActive(_active);
            }
        }
        void ShowHideOtherObject(bool _active)
        {
            if (_needhideobjct != null && _needhideobjct.Count > 0)
            {
                for (int i = 0; i < _needhideobjct.Count; i++)
                {
                    _needhideobjct[i].gameObject.SetActive(_active);
                }
            }
        }


        void AddLightProbes(SceneEditContext sec)
        {
            var meshTerrain = TerrainSystem.system.GetMeshTerrain();
            Bounds[] chunkBounds = new Bounds[sec.ssc.heightCount * sec.ssc.widthCount];
            if (meshTerrain != null)
            {
                for (int z = 0; z < sec.ssc.heightCount; ++z)
                {
                    for (int x = 0; x < sec.ssc.widthCount; ++x)
                    {
                        string chunkName = string.Format("Chunk_{0}_{1}", x.ToString(), z.ToString());
                        var chunkTrans = meshTerrain.Find(chunkName);
                        if (chunkTrans != null)
                        {
                            var r = chunkTrans.GetComponent<Renderer>();
                            chunkBounds[x + z * sec.ssc.widthCount] = r.bounds;
                        }
                    }
                }
            }

            EditorCommon.EnumTransform funPrefabs = null;
            funPrefabs = (trans, param) =>
            {
                var bounds = param as Bounds[];
                if (trans.TryGetComponent(out MeshRenderObject mro))
                {
                    if (mro.layer != 0)
                    {
                        trans.gameObject.layer = mro.layer;
                    }
                    int chunkID = mro.chunkInfo.chunkID;
                    if (chunkID >= 0 && chunkID < bounds.Length)
                    {
                        ref var bound = ref bounds[chunkID];
                        var min = bound.min;
                        var max = bound.max;
                        var b = mro.bounds;
                        var objMin = b.min;
                        var objMax = b.max;
                        if (objMin.y < min.y)
                        {
                            min.y = objMin.y;
                        }
                        if (objMax.y > max.y)
                        {
                            max.y = objMax.y;
                        }
                        bound.min = min;
                        bound.max = max;
                    }
                }
                EditorCommon.EnumChildObject(trans, param, funPrefabs);
            };

            Transform t = StaticObjectSystem.system.resData.workspace;

            EditorCommon.EnumChildObject(t, chunkBounds, (trans, param) =>
            {
                funPrefabs(trans, param);
            });

            var unitiyTerrain = TerrainSystem.system.GetUnityTerrain();
            if (unitiyTerrain != null)
            {
                TerrainCollider tc;
                if (unitiyTerrain.TryGetComponent(out tc))
                {
                    tc.enabled = false;
                }
            }

            var probes = resData.workspace;
            if (probes != null)
            {
                for (int z = 0; z < sec.ssc.heightCount; ++z)
                {
                    for (int x = 0; x < sec.ssc.widthCount; ++x)
                    {
                        string name = string.Format("LightProbeArea_{0}_{1}", x.ToString(), z.ToString());
                        var trans = probes.Find(name);
                        if (trans == null)
                        {
                            GameObject go = new GameObject(name);
                            trans = go.transform;
                            trans.parent = probes;
                        }
                        trans.gameObject.layer = DefaultGameObjectLayer.InVisiblityLayer;
                        ref var bound = ref chunkBounds[x + z * sec.ssc.widthCount];
                        //Vector3 pos = new Vector3 (x * chunkWidth + chunkWidth * 0.5f, 0, z * chunkHeight + chunkHeight * 0.5f);
                        trans.position = bound.center;
                        BoxCollider bc = null;
                        if (!trans.TryGetComponent(out bc))
                        {
                            bc = trans.gameObject.AddComponent<BoxCollider>();
                        }
                        bc.size = bound.size;
                        LightprobeArea lpa = null;
                        if (!trans.TryGetComponent(out lpa))
                        {
                            lpa = trans.gameObject.AddComponent<LightprobeArea>();
                        }
                        lpa.chunkID = x + z * sec.ssc.widthCount;
                        lpa.GenProbes(areaData, x, z);
                    }
                }
            }
            PreviewLightProbes(true,sec);
        }

        private void ClearLightProbes()
        {
            if (areaData != null)
            {
                if (areaData.lightProbeData != null)
                {
                    for (int i = 0; i < areaData.lightProbeData.Length; ++i)
                    {
                        ref var lpd = ref areaData.lightProbeData[i];
                        lpd.flag = 0;
                        lpd.height = new Vector4(-1, -1, -1, -1);
                    }
                }
            }
        }

        void HideUnTerrainLayer()
        {
            lpae.showhideunterrainlayer = !lpae.showhideunterrainlayer;
            ShowHideOtherObject(lpae.showhideunterrainlayer);

        }
        void PreviewLightProbes(bool preview, SceneEditContext sec)
        {
            var probes = resData.workspace;
            if (probes != null)
            {
                for (int z = 0; z < sec.ssc.heightCount; ++z)
                {
                    for (int x = 0; x < sec.ssc.widthCount; ++x)
                    {
                        string name = string.Format("LightProbeArea_{0}_{1}", x.ToString(), z.ToString());
                        var trans = probes.Find(name);
                        if (trans != null)
                        {
                            LightprobeArea lpa = null;
                            if (trans.TryGetComponent(out lpa))
                            {
                                if (preview)
                                {

                                    lpa.Sample(areaData);
                                    lpa.GenBatch();
                                    lpa.drawProbes = true;
                                }
                                else
                                {
                                    lpa.drawProbes = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        void SaveLightProbes()
        {
            EditorCommon.SaveAsset(areaData);
        }

    }
}
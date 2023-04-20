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
    public class StaticObjectSystem : SceneResProcess
    {

        public static StaticObjectSystem system;
        public override void Init(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init(ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
            serialize = SerializeCb;
            if (system == null)
                system = this;
            if (resData.workspace != null)
            {
                if (!resData.workspace.parent.TryGetComponent<StaticCaster>(out var sc))
                {
                    resData.workspace.parent.gameObject.AddComponent<StaticCaster>();
                }
            }
        }

        ////////////////////////////PreSerialize////////////////////////////
        public static void SetName(Transform trans, SceneSerializeContext context)
        {
            Dictionary<string, int> objMap = context.objIDMap;
            UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject);
            string path = AssetDatabase.GetAssetPath(prefab);
            int count = 0;
            objMap.TryGetValue(path, out count);
            string ext = Path.GetExtension(path).ToLower();
            if (ext == ".prefab")
            {
                trans.name = string.Format("{0}_prefab_{1}", prefab.name, count.ToString());
            }
            else if (ext == ".fbx")
            {
                trans.name = string.Format("{0}_fbx_{1}", prefab.name, count.ToString());
            }
            count++;
            objMap[path] = count;
        }
        
        protected static void RefreshMeshObj(Transform trans, SceneSerializeContext context)
        {
            SceneGroupObject group = null;
            GameObject lodPrefab = null;
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject) as GameObject;
            string dir = AssetsPath.GetAssetDir(prefab, out var path);
            string lodpath = string.Format("{0}/{1}_lod1.fbx", dir, prefab.name);
            if (File.Exists(lodpath))
            {
                lodPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lodpath);
            }
            if (lodPrefab != null)
            {
                if (!trans.TryGetComponent(out group))
                {
                    group = trans.gameObject.AddComponent<SceneGroupObject>();
                }
                group.Reset();
            }
            float h = -100;
            MeshRenderObject singleMro = null;
            AABB totalAABB = new AABB();
            List<Renderer> renders = EditorCommon.GetRenderers(trans.gameObject);
            for (int i = 0; i < renders.Count; ++i)
            {
                Renderer r = renders[i];
                Transform transform = r.transform;
                //init
                MeshRenderObject mro;
                if (!transform.TryGetComponent(out mro))
                {
                    mro = transform.gameObject.AddComponent<MeshRenderObject>();
                }
                if (renders.Count == 1)
                {
                    singleMro = mro;
                }
                mro.prefabName = prefab.name;
                mro.id = -1;
                mro.areaMask = 0xffffffff;
                mro.flag.Reset();
                //mro.globalObject = false;
                //GameObjectUtility.SetStaticEditorFlags(r.gameObject, (StaticEditorFlags)0);

                MeshFilter mf = r.GetComponent<MeshFilter>();
                Mesh m = mf != null ? mf.sharedMesh : null;
                Bounds aabb = new Bounds();
                if (m != null)
                {
                    aabb = MeshAssets.CalcBounds(m, transform.localToWorldMatrix);
                }
                else
                {
                    aabb = r.bounds;
                    //SceneResConfig.instance.AddErrorLog (string.Format ("null mesh render:{0}", r.name), "StaticObject");
                }
                mro.aabb.Init(ref aabb);
                if (i == 0)
                {
                    totalAABB.Init(ref aabb);
                }
                else
                {
                    totalAABB.Encapsulate(ref aabb);
                }
                if (mro.aabb.max.y > h)
                {
                    h = mro.aabb.max.y;
                }
                if (r.sharedMaterial != null)
                    mro.shader = r.sharedMaterial.shader;

                //trans.gameObject.layer = mro.layer;
                mro.group = group;
                if (group != null && lodPrefab != null)
                {
                    if (i == 0)
                    {
                        group.Init(ref mro.aabb);

                    }
                    else
                    {
                        group.Add(ref mro.aabb);
                    }
                    mro.flag.SetFlag(SceneObject.FadeEffect, group.fadeEffect);
                    group.CalcLod(context.sceneConfig, lodPrefab);
                }
                else
                {
                    SceneGroupObject.CalcLod(context.sceneConfig, ref mro.aabb, ref mro.lodDist);
                }
                SceneAssets.CalcBlock(EngineContext.instance, ref mro.aabb, ref mro.chunkInfo, mro.forceGlobalObj);
                if (mro.chunkSubMesh != null)
                {
                    for (int j = 0; j < mro.chunkSubMesh.subMesh.Count; ++j)
                    {
                        var subMesh = mro.chunkSubMesh.subMesh[j];
                        subMesh.id = -1;
                        SceneGroupObject.CalcLod(context.sceneConfig, ref subMesh.aabb, ref subMesh.lodDist);
                    }
                }
               
            }
            if (group != null)
            {
                group.CalcLod(context.sceneConfig, lodPrefab);
                SceneAssets.CalcBlock(EngineContext.instance, ref group.aabb, ref group.chunkInfo, group.forceGlobalObj);
                if (group.IsValid())
                {
                    var chunk = context.sd.GetChunk(group.chunkInfo.chunkID);
                    if (chunk != null)
                    {
                        var go = new GroupObject()
                        {
                            aabb = group.aabb,
                        };
                        go.lodData.Reset();
                        go.lodData.Copy(ref group.lodData);
                        group.index = chunk.groupObjects.Count;
                        chunk.groupObjects.Add(go);
                    }
                }
            }

            //if (h > 0 && (singleMro != null || group != null))
            //{
            //    var p = totalAABB.max;
            //    bool globalObject = false;
            //    if (totalAABB.sizeY > context.sceneConfig.lodHeight * 2)
            //    {
            //        globalObject = true;
            //    }
            //    RaycastHit hitinfo;
            //    if (Physics.Raycast(p, Vector3.down, out hitinfo, 801, 1 << DefaultGameObjectLayer.TerrainLayer))
            //    {
            //        if ((h - hitinfo.point.y) > context.sceneConfig.lodHeight)
            //        {
            //            globalObject = true;  
            //        }
            //    }
            //    if(globalObject)
            //    {
            //        if (group != null)
            //        {
            //            group.globalObject = true;
            //        }
            //        else if (singleMro != null)
            //        {
            //            singleMro.globalObject = true;
            //        }
            //    }
            //}

        }
        protected static void PreSerializeCb(Transform trans, object param)
        {
            SceneSerializeContext context = param as SceneSerializeContext;
            if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
            {
                SetName(trans, context);
                RefreshMeshObj(trans, context);
            }
            else
            {
                EditorCommon.EnumChildObject(trans, param, context.preSerialize);
            }
        }

        ////////////////////////////Serialize////////////////////////////

        private static void FillMeshObj2Chunks(SceneSerializeContext ssContext, Transform trans, int prefabId)
        {
            //game object info
            var goi = GreatePrefabInstance(ssContext, trans, prefabId);
            //sub obj info
            List<Renderer> renders = EditorCommon.GetRenderers(trans.gameObject);
            var parent = trans.parent;
            int shadowObjectID = -1;
            for (int i = 0; i < renders.Count; ++i)
            {
                Renderer r = renders[i];
                Transform t = r.transform;
                MeshRenderObject mro;
                if (t.TryGetComponent(out mro))
                {
                    var chunkSubMesh = mro.chunkSubMesh;
                    if (chunkSubMesh == null || chunkSubMesh.subMesh.Count < 2)
                    {
                        SceneObjectData so = new SceneObjectData();
                        so.ID = mro.id;
                        so.areaMask = mro.areaMask;
                        so.renderIndex = i;
                        var mesh = mro.GetMesh();
                        string path = AssetDatabase.GetAssetPath(mesh);
                        if (path.StartsWith(LoadMgr.singleton.editorResPath))
                        {
                            so.resName = mro.prefabName.ToLower();
                        }
                        else
                        {
                            so.resName = string.Format("{0}_{1}", mro.prefabName.ToLower(), i.ToString());
                        }

                        so.mat = r.sharedMaterial;
                        so.aabb = mro.aabb;


                        so.pos = t.position;
                        so.rotate = t.rotation;
                        so.scale = t.lossyScale;
                        so.localScale = t.localScale;

                        so.blockId = mro.chunkInfo.blockID;

                        so.lightMapScale = mro.lightmapComponent.lightMapScale;
                        so.lightMapIndex = mro.lightmapComponent.lightMapIndex;
                        so.lightMapVolumnIndex = mro.lightmapComponent.lightMapVolumnIndex;
                        so.lightmapUVST = mro.lightmapComponent.lightmapUVST;
                        so.exString = mro.exString;

                        so.flag.SetFlag(mro.flag.flag);
                        so.flag.SetFlag(SceneObject.GameObjectActiveInHierarchy, r.gameObject.activeInHierarchy);
                        so.flag.SetFlag(SceneObject.GameObjectActive, r.gameObject.activeSelf);
                        so.flag.SetFlag(SceneObject.RenderEnable, r.enabled);
                        so.flag.SetFlag(SceneObject.IgnoreShadowCaster, r.shadowCastingMode == ShadowCastingMode.Off);
                        so.flag.SetFlag(SceneObject.OnlyCastShadow, r.shadowCastingMode == ShadowCastingMode.ShadowsOnly);
                        so.flag.SetFlag(SceneObject.HideRender, r.shadowCastingMode == ShadowCastingMode.ShadowsOnly);
                        so.flag.SetFlag(SceneObject.FadeEffect, mro.fadeEffect);
                        so.flag.SetFlag(SceneObject.ExString, !string.IsNullOrEmpty(mro.exString));
                        so.flag.SetFlag(SceneObject.NotCulled,
                            r.gameObject.layer == DefaultGameObjectLayer.TerrainLayer ||
                            mro.notCull);
                        so.flag.SetFlag(SceneObject.ForceCastShadow, mro.forceCastShadow);

                        so.gameObjectID = goi.ID;
                        ColliderSystem.Save(ssContext, t, so.ID);

                        bool isGlobal = mro.chunkInfo.isGlobalObject;
                        int chunkID = mro.chunkInfo.chunkID;
                        so.virtualChunkID = mro.chunkInfo.virtualChunkID;
                        so.flag.SetFlag(SceneObject.GlobalObject, mro.chunkInfo.isGlobalObject);
                        if (mro.group != null && mro.group.IsValid())
                        {
                            chunkID = mro.group.chunkInfo.chunkID;
                            isGlobal = mro.group.chunkInfo.isGlobalObject;
                            so.virtualChunkID = mro.group.chunkInfo.virtualChunkID;
                            so.groupObjectIndex = mro.group.index;
                            so.flag.SetFlag(SceneObject.GlobalObject, mro.group.chunkInfo.isGlobalObject);
                        }
                        else
                        {
                            so.lodDist.Copy(ref mro.lodDist);
                        }
                        if (isGlobal && r.shadowCastingMode != ShadowCastingMode.Off)
                        {
                            if (shadowObjectID < 0)
                            {
                                shadowObjectID = SceneObjectData.shadowObjectID++;
                            }
                            so.shadowID = shadowObjectID;
                        }

                        var chunk = ssContext.sd.GetChunk(chunkID);
                        chunk.sceneObjects.Add(so);
                    }
                    else
                    {
                        for (int j = 0; j < chunkSubMesh.subMesh.Count; ++j)
                        {
                            var subMesh = chunkSubMesh.subMesh[j];
                            if (subMesh.m != null)
                            {
                                SceneObjectData so = new SceneObjectData();
                                so.ID = subMesh.id;
                                so.areaMask = mro.areaMask;
                                so.resName = subMesh.m.name;
                                so.mat = r.sharedMaterial;
                                so.aabb = subMesh.aabb;

                                so.pos = Vector3.zero;
                                so.rotate = Quaternion.identity;
                                so.scale = Vector3.one;
                                so.localScale = Vector3.one;

                                so.blockId = mro.chunkInfo.blockID;

                                so.lightMapScale = mro.lightmapComponent.lightMapScale;
                                so.lightMapIndex = mro.lightmapComponent.lightMapIndex;
                                so.lightMapVolumnIndex = mro.lightmapComponent.lightMapVolumnIndex;
                                so.lightmapUVST = mro.lightmapComponent.lightmapUVST;
                                so.exString = mro.exString;

                                so.flag.SetFlag(mro.flag.flag);
                                so.flag.SetFlag(SceneObject.GameObjectActiveInHierarchy, r.gameObject.activeInHierarchy);
                                so.flag.SetFlag(SceneObject.GameObjectActive, r.gameObject.activeSelf);
                                so.flag.SetFlag(SceneObject.RenderEnable, r.enabled);
                                so.flag.SetFlag(SceneObject.IgnoreShadowCaster, true);
                                so.flag.SetFlag(SceneObject.OnlyCastShadow, r.shadowCastingMode == ShadowCastingMode.ShadowsOnly);
                                so.flag.SetFlag(SceneObject.HideRender, r.shadowCastingMode == ShadowCastingMode.ShadowsOnly);
                                so.flag.SetFlag(SceneObject.FadeEffect, mro.fadeEffect);
                                so.flag.SetFlag(SceneObject.ExString, !string.IsNullOrEmpty(mro.exString));
                                so.flag.SetFlag(SceneObject.NotCulled, true);
                                so.flag.SetFlag(SceneObject.ForceCastShadow, mro.forceCastShadow);
                                so.gameObjectID = goi.ID;
                                ColliderSystem.Save(ssContext, t, so.ID);
                                so.virtualChunkID = -1;
                                
                                so.lodDist.Copy(ref subMesh.lodDist);

                                int chunkID = subMesh.chunkInfo.chunkID;

                                var chunk = ssContext.sd.GetChunk(chunkID);
                                chunk.sceneObjects.Add(so);
                            }
                           
                        }
                    }
                    // FillMeshObjReflectionProbe (scp, mro, r, subPoi);
                }
            }

        }

        protected static void SerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject) as GameObject;
                if (prefab != null)
                {
                    SavePrefabInfo(ssContext, prefab, out var prefabId);
                    FillMeshObj2Chunks(ssContext, trans, prefabId);
                }
            }
            else
            {
                EnumFolder(trans, ssContext, ssContext.serialize);
            }
        }

        ////////////////////////////PreSave////////////////////////////

        private void PreSaveChunk(ref SceneContext sceneContext, SceneSaveContext ssc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            for (int j = 0; j < chunk.sceneObjects.Count; ++j)
            {
                var so = chunk.sceneObjects[j];

                if ((so.flag.HasFlag(SceneObject.GameObjectActiveInHierarchy) &&
                     so.flag.HasFlag(SceneObject.RenderEnable) ||
                     so.flag.HasFlag(SceneObject.HasAnim))) //&& so.scale.x > 0 && so.scale.y > 0 && so.scale.z > 0)
                {
                    if (so.flag.HasFlag(SceneObject.IsSfx))
                    {
                        ssc.AddResName(so.resName);
                        so.matId = ushort.MaxValue;
                        saveChunk.sceneObjects.Add(so);
                    }
                    else
                    {
                        string meshPath = string.Format("{0}{1}.asset",
                            LoadMgr.singleton.editorResPath,
                            so.resName);
                        if (so.mat != null && File.Exists(meshPath))
                        {
                            bool isVisible = so.flag.HasFlag(SceneObject.GameObjectActiveInHierarchy) &&
                                             so.flag.HasFlag(SceneObject.RenderEnable);
                            if (!isVisible)
                                so.flag.SetFlag(SceneObject.HideRender, true);
                            bool fisrtFind = ssc.matSaveData.FindOrAddMatInfo(so.mat, out int matIndex, out so.matHash);
                            so.matId = matIndex >= 0 ? (ushort) matIndex : ushort.MaxValue;
                            if (fisrtFind)
                                ssc.AddRes("mat", so.matHash, AssetDatabase.GetAssetPath(so.mat));

                            ssc.AddResName(so.resName);
                            ssc.resAsset.AddResReDirct(
                                LoadMgr.singleton.editorResPath,
                                string.Format("{0}.asset", so.resName),
                                ReDirectRes.LogicPath_Common);

                            if (ssc.lightMapData != null)
                            {
                                ssc.lightMapData.GetLightMap(so.lightMapVolumnIndex, so.lightMapIndex,
                                    out var volumnName, out var lightmap, out var shadowmask);
                                string lightmapPath = string.Format("{0}/SceneLightmapBackup/{1}/",
                                    ssc.sceneContext.configDir,
                                    volumnName);

                                if (lightmap != null)
                                {
                                    so.lightmapName = lightmap.name;
                                    so.flag.SetFlag(SceneObject.HasLightmap, true);
                                    ssc.AddResName(lightmap.name);
                                    string lightmapNameWithExt = string.Format("{0}.exr", so.lightmapName);
                                    ssc.resAsset.AddResReDirct(
                                        lightmapPath,
                                        lightmapNameWithExt,
                                        ReDirectRes.LogicPath_SceneRes);
                                }

                                if (shadowmask != null)
                                {
                                    //  so.shadowmaskName = shadowmask.name;
                                    so.flag.SetFlag(SceneObject.HasShadowMask, true);
                                    string shadowMaskNameWithExt = string.Format("{0}.png", so.lightmapName);
                                    ssc.resAsset.AddResReDirct(
                                        lightmapPath,
                                        shadowMaskNameWithExt,
                                        ReDirectRes.LogicPath_SceneRes);
                                }

                            }

                            so.reflectionProbeName = "";
                            if (so.groupObjectIndex >= 0)
                            {
                                if (so.groupObjectIndex >= chunk.groupObjects.Count)
                                {
                                    DebugLog.AddErrorLog2("out of group count:{0}", so.resName);
                                }
                                else
                                {
                                    var go = chunk.groupObjects[so.groupObjectIndex];
                                    if (go.lodData.prefab != null)
                                    {
                                        ssc.resAsset.AddResReDirct(
                                            LoadMgr.singleton.editorResPath,
                                            string.Format("{0}_lod1.asset", so.resName),
                                            ReDirectRes.LogicPath_Common);
                                    }
                                }
                            }

                            // if (spoi.reflectionProbeIndex >= 0)
                            // {
                            //     if (spoi.reflectionProbeIndex < ecd.localReflectionProbes.Count)
                            //     {
                            //         var lrp = ecd.localReflectionProbes[spoi.reflectionProbeIndex];
                            //         if (lrp.cube != null)
                            //         {
                            //             so.ReflectionProbeName = lrp.cube.name;
                            //             ssci.AddResName (lrp.cube.name);
                            //             ssci.resAsset.AddResReDirct (
                            //                 string.Format ("{0}/{1}/",
                            //                     ssci.context.dir,
                            //                     ssci.context.name),
                            //                 string.Format ("{0}.exr", lrp.cube.name),
                            //                 ReDirectRes.LogicPath_SceneRes);
                            //         }

                            //     }
                            // }

                            saveChunk.sceneObjects.Add(so);
                        }
                        else
                        {
                            DebugLog.AddEngineLog2("mesh not export:{0}", meshPath);
                        }
                    }

                }

                // if (so.scale.x < 0 || so.scale.y < 0 || so.scale.z < 0)
                // {
                //     DebugLog.AddErrorLog2("not support negative scale:{0}", so.resName);
                // }
            }

        }

        public override void PreSave(ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;
            PreSaveChunk(ref sceneContext, ssc, ssc.sd.global, ssc.saveSD.global, -1);
            var chunks = ssc.sd.chunks;
            for (int i = 0; i < chunks.Count; ++i)
            {
                var chunk = ssc.sd.chunks[i];
                var saveChunk = ssc.saveSD.chunks[i];
                PreSaveChunk(ref sceneContext, ssc, chunk, saveChunk, i);
            }
        }

        public override void PreSaveChunk(ref SceneContext sceneContext, BaseSceneContext bsc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            var ssc = bsc as SceneSaveContext;
            saveChunk.sceneObjects.AddRange(saveChunk.instanceObjects);
            if (i >= 0)
            {
                for (int j = 0; j < saveChunk.sceneObjects.Count; ++j)
                {
                    var so = saveChunk.sceneObjects[j];
                    IQuadTreeObject quadTreeObj = so as IQuadTreeObject;
                    quadTreeObj.QuadNodeId = j;
                    if (quadTreeObj.BlockId >= 0)
                        SceneQuadTree.Add2QuadTree(ssc.treeContext, quadTreeObj, quadTreeObj.bounds);

                }
            }
        }

        ////////////////////////////Save////////////////////////////
        private static void SaveSceneObjectBasicData(BinaryWriter bw,
            SceneSaveContext ssc, SceneObjectData so, bool globalObject)
        {
            uint id = so.ID >= 0 ? (uint)so.ID : uint.MaxValue;
            bw.Write(id);
            bw.Write(so.flag.flag);
            bw.Write(so.areaMask);
            var matrix = Matrix4x4.TRS(so.pos, so.rotate, so.scale);
            EditorCommon.WriteMatrix(bw, matrix);

            // if (flag.HasFlag (SceneObject.FadeEffect))
            // {
            //     bw.Write ((short) so.groupID);
            // }

            EditorCommon.SaveAABB(bw, ref so.aabb);
            ssc.SaveStringIndex(bw, so.resName);
            bw.Write(so.matId);
            if (so.matId != ushort.MaxValue)
            {
                if (so.flag.HasFlag(SceneObject.HasLightmap))
                {
                    ssc.SaveStringIndex(bw, so.lightmapName);
                    EditorCommon.WriteVector(bw, so.lightmapUVST);                
                }

                if (so.flag.HasFlag(SceneObject.HasReflectionProbe))
                {
                    ssc.SaveStringIndex(bw, so.reflectionProbeName);
                }
                bw.Write(so.groupObjectIndex);
                if (so.groupObjectIndex < 0)
                {
                    bw.Write(so.lodDist.lodDist2);
                    bw.Write(so.lodDist.fadeDist2);
                }
                if (globalObject)
                {
                    bw.Write(so.virtualChunkID);
                    bw.Write(so.shadowID);
                }                   
            }
            if (so.flag.HasFlag(SceneObject.ExString))
            {
                uint hash = EngineUtility.XHashLowerRelpaceDot(0, so.exString);
                bw.Write(hash);
            }
        }

        public static void SaveChunkSceneObject(
            BinaryWriter bw,
            SceneSaveContext ssc,
            ChunkData chunk, ChunkSaveData saveChunk,bool isGlobal = false)
        {
            var sceneObjects = saveChunk.sceneObjects;
            ushort objCount = (ushort)(sceneObjects.Count - saveChunk.instanceCount);
            bw.Write(objCount);
            for (int j = 0; j < objCount; ++j)
            {
                var so = sceneObjects[j];
                SaveSceneObjectBasicData(bw, ssc, so, isGlobal);
            }
            ushort instanceObjCount = (ushort)saveChunk.instanceCount;
            bw.Write(instanceObjCount);
            for (int j = objCount; j < sceneObjects.Count; ++j)
            {
                var so = sceneObjects[j];
                SaveSceneObjectBasicData(bw, ssc, so, false);
                var iod = so as InstanceObjectData;
                bw.Write(iod.instanceCount);
            }
        }

        public static void SaveChunkGroupObject(BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            ushort objCount = (ushort)(chunk.groupObjects.Count);
            bw.Write(objCount);
            for (int j = 0; j < objCount; ++j)
            {
                var go = chunk.groupObjects[j];
                EditorCommon.SaveAABB(bw, ref go.aabb);
                bw.Write(go.lodData.lodDist.lodDist2);
                bw.Write(go.lodData.lodDist.fadeDist2);
                bw.Write(go.lodData.prefab != null);
            }
        }
    }

    public class PrefabObjectSystem : StaticObjectSystem
    {
        public override void PreSaveChunk(ref SceneContext sceneContext, BaseSceneContext bsc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        { }
        public override void PreSave(ref SceneContext sceneContext, BaseSceneContext bsc) { }
    }
}
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
    public class MultiLayerSystem : SceneResProcess
    {
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
            serialize = SerializeCb;
        }

        ////////////////////////////PreSerialize////////////////////////////
        protected static void PreSerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<MultiLayer>(out var ml))
            {
                ml.areaMask = 0xffffffff;
            }
            else
            {
                EnumFolder(trans, ssContext, ssContext.preSerialize);
            }
        }
        
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject) as GameObject;
                if (prefab != null)
                {
                    SavePrefabInfo(ssContext, prefab, out var prefabId);
                    if (trans.TryGetComponent<MultiLayer>(out var ml))
                    {
                        ml.Save();
                        StaticObjectSystem.SetName(trans, ssContext);
                        if (ml.m != null && ml.mat != null)
                        {
                            var goi = GreatePrefabInstance(ssContext, trans, prefabId);

                            InstanceObjectData so = new InstanceObjectData();
                            so.ID = SceneObjectData.objectID++;
                            so.areaMask = ml.areaMask;
                            so.instanceCount = (short)ml.quality;
                            so.indexCount = ml.m.GetIndexCount(0);
                            //string path = AssetDatabase.GetAssetPath(ml.m);
                            so.resName = string.Format("{0}_0", prefab.name.ToLower());

                            so.mat = ml.mat;
                            trans.TryGetComponent<MeshRenderer>(out var mr);
                            if (mr != null)
                                so.aabb = AABB.Create(mr.bounds);

                            so.pos = trans.position;
                            so.rotate = trans.rotation;
                            so.scale = trans.lossyScale;
                            so.localScale = trans.localScale;

                            var chunkInfo = new ChunkInfo();
                            SceneAssets.CalcBlock(EngineContext.instance, ref so.aabb, ref chunkInfo);
                            so.blockId = chunkInfo.blockID;

                            so.flag.SetFlag(SceneObject.GameObjectActiveInHierarchy, trans.gameObject.activeInHierarchy);
                            so.flag.SetFlag(SceneObject.GameObjectActive, trans.gameObject.activeSelf);
                            if (mr != null)
                                so.flag.SetFlag(SceneObject.RenderEnable, mr.enabled);
                            so.flag.SetFlag(SceneObject.IgnoreShadowCaster, true);
                            so.flag.SetFlag(SceneObject.OnlyCastShadow, false);
                            so.flag.SetFlag(SceneObject.IsMultiLayer, true);
                            so.gameObjectID = goi.ID;
                            ColliderSystem.Save(ssContext, trans, so.ID);

                            int chunkID = chunkInfo.chunkID;
                            var chunk = ssContext.sd.GetChunk(chunkID);
                            chunk.multiLayerObjects.Add(so);

                        }
                    }
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
            for (int j = 0; j < chunk.multiLayerObjects.Count; ++j)
            {
                var so = chunk.multiLayerObjects[j];

                if (so.flag.HasFlag(SceneObject.GameObjectActiveInHierarchy))
                {
                    string meshPath = string.Format("{0}{1}.asset",
                                                LoadMgr.singleton.editorResPath,
                                                so.resName);
                    if (so.mat != null && File.Exists(meshPath))
                    {
                        bool fisrtFind = ssc.matSaveData.FindOrAddMatInfo(so.mat, out int matIndex, out so.matHash);
                        so.matId = matIndex >= 0 ? (ushort)matIndex : ushort.MaxValue;
                        if (fisrtFind)
                            ssc.AddRes("mat", so.matHash, AssetDatabase.GetAssetPath(so.mat));
                        so.flag.SetFlag(SceneObject.IsMultiLayer, true);
                        SceneGroupObject.CalcLod(ssc.sceneConfig, ref so.aabb, ref so.lodDist);
                        ssc.AddResName(so.resName);
                        ssc.resAsset.AddResReDirct(
                            LoadMgr.singleton.editorResPath,
                            string.Format("{0}.asset", so.resName),
                            ReDirectRes.LogicPath_Common);
                        if (saveChunk.argArray == null)
                        {
                            saveChunk.argArray = new List<uint>();
                        }
                        saveChunk.argArray.Add(so.indexCount);
                        saveChunk.argArray.Add((uint)so.instanceCount);
                        saveChunk.argArray.Add(0);
                        saveChunk.argArray.Add(0);
                        saveChunk.argArray.Add(0);

                        saveChunk.instanceObjects.Add(so);
                        saveChunk.instanceCount++;
                    }
                    else
                    {
                        DebugLog.AddEngineLog2("mesh not export:{0}", meshPath);
                    }
                }
            }

        }
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
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

        ////////////////////////////Save////////////////////////////
        public static void SaveChunk (BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            int argCount = saveChunk.argArray != null ? saveChunk.argArray.Count : 0;
            bw.Write(argCount);
            for (int j = 0; j < argCount; ++j)
            {
                bw.Write(saveChunk.argArray[j]);
            }
            DebugLog.DebugStream(bw, "ChunkMultiLayer");
        }
    }
}
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

    class InstancePack
    {
        public InstanceMeshMat imm;
        public List<InstanceData> instanceObjects = new List<InstanceData> ();
    }
    class InstanceBlock
    {
        public Dictionary<int, InstancePack> packs = new Dictionary<int, InstancePack> ();
    }
    public class InstanceSystem : SceneResProcess
    {
        private Dictionary<uint, int> instanceType = new Dictionary<uint, int> ();
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
            serialize = SerializeCb;
        }

        ////////////////////////////PreSerialize////////////////////////////
        protected static void PreSerializeCb(Transform trans, object param)
        {
            SceneSerializeContext context = param as SceneSerializeContext;
            if (!trans.TryGetComponent<InstanceObject>(out var io))
                io = trans.gameObject.AddComponent<InstanceObject>();
            io.areaMask = 0xffffffff;
            EditorCommon.EnumChildObject(trans, param, context.preSerialize);
        }
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext context = param as SceneSerializeContext;

            if (trans.TryGetComponent<MeshFilter> (out var mf) &&
                trans.TryGetComponent<MeshRenderer> (out var mr))
            {
                var mesh = mf.sharedMesh;
                var mat = mr.sharedMaterial;
                if (mesh != null && mat != null)
                {

                    string meshName = string.Format ("{0}_0", mesh.name);
                    string matPath = AssetDatabase.GetAssetPath (mat);

                    uint hash = EngineUtility.XHashLowerRelpaceDot (0, meshName);
                    hash = EngineUtility.XHashLowerRelpaceDot (hash, matPath);

                    if (!trans.TryGetComponent<InstanceObject> (out var io))
                        io = trans.gameObject.AddComponent<InstanceObject> ();

                    hash = EngineUtility.XHashLowerRelpaceDot (hash, io.areaMask.ToString ());

                    SceneAssets.SortSceneObjectName (io, meshName, context.objIDMap);

                    io.hash = hash;
                    var aabb = MeshAssets.CalcBounds (mesh, trans.localToWorldMatrix);
                    var system = context.resProcess as InstanceSystem;
                    if (!system.instanceType.TryGetValue (hash, out var instanceIndex))
                    {
                        instanceIndex = context.sd.instances.Count;
                        context.sd.instances.Add (new InstanceMeshMat ()
                        {
                            hash = hash,
                                mesh = mesh,
                                material = mat,
                                areaMask = io.areaMask,
                        });
                        system.instanceType.Add (hash, instanceIndex);
                    }
                    //calc chunk block
                    Vector3 pos = trans.position;
                    int chunkID = SceneQuadTree.FindChunkIndex (pos,
                        context.chunkWidth, context.widthCount, context.heightCount, out var x, out var z);
                    io.chunkIndex = chunkID;
                    io.worldPosOffset = new Vector4 (x * context.chunkWidth, z * context.chunkHeight, 1.0f / context.chunkWidth, 1.0f / context.chunkHeight);
                    float halfWidth = context.chunkWidth * 0.5f;
                    Vector2 chunkCorner = new Vector4 (x * context.chunkWidth, z * context.chunkHeight);
                    int blockIndex = SceneQuadTree.FindChunkIndex (new Vector3 (pos.x - chunkCorner.x, 0, pos.z - chunkCorner.y),
                        halfWidth, 2, 2, out x, out z);

                    var chunk = context.sd.GetChunk (chunkID, true);
                    if (chunk != null)
                    {
                        var id = new InstanceData ()
                        {
                        pos = pos,
                        rot = trans.rotation,
                        scale = trans.localScale.x,
                        aabb = AABB.Create (aabb),
                        instanceIndex = instanceIndex,
                        blockId = blockIndex
                        };
                        id.flag.SetFlag (SceneObject.GameObjectActive, mr.enabled && trans.gameObject.activeSelf);
                        id.flag.SetFlag (SceneObject.GameObjectActiveInHierarchy, trans.gameObject.activeInHierarchy);
                        chunk.instanceObjects.Add (id);
                    }
                }
            }

            EditorCommon.EnumChildObject (trans, param, context.serialize);
        }
        public override void Serialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            if (preSerialize != null)
            {
                instanceType.Clear ();
                base.Serialize (ref sceneContext, ssContext);

            }
        }
        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;

            InstanceBlock[] blocks = new InstanceBlock[4];
            for (int i = 0; i < ssc.sd.chunks.Count; ++i)
            {
                var chunk = ssc.sd.chunks[i];
                var saveChunk = ssc.saveSD.chunks[i];
                for (int j = 0; j < blocks.Length; ++j)
                {
                    if (blocks[j] != null)
                    {
                        blocks[j].packs.Clear ();
                    }
                }
                for (int j = 0; j < chunk.instanceObjects.Count; ++j)
                {
                    var iod = chunk.instanceObjects[j];
                    if (iod.flag.HasFlag (SceneObject.GameObjectActive) &&
                        iod.flag.HasFlag (SceneObject.GameObjectActiveInHierarchy))
                    {
                        var imm = ssc.sd.instances[iod.instanceIndex];
                        if (imm.mesh != null && imm.material != null)
                        {
                            string meshName = string.Format ("{0}_0",
                                imm.mesh.name).ToLower ();
                            string meshPath = string.Format ("{0}{1}.asset",
                                LoadMgr.singleton.editorResPath,
                                meshName);
                            if (File.Exists (meshPath))
                            {
                                imm.meshName = meshName;
                                if (imm.indexCount == 0)
                                {
                                    imm.indexCount = imm.mesh.GetIndexCount (0);
                                }
                                var block = blocks[iod.blockId];
                                if (block == null)
                                {
                                    block = new InstanceBlock ();
                                    blocks[iod.blockId] = block;
                                }
                                if (!block.packs.TryGetValue (iod.instanceIndex, out var pack))
                                {
                                    pack = new InstancePack ()
                                    {
                                        imm = imm,
                                    };
                                    block.packs.Add (iod.instanceIndex, pack);
                                }
                                pack.instanceObjects.Add (iod);
                            }
                        }
                    }
                }
                for (int j = 0; j < blocks.Length; ++j)
                {
                    var block = blocks[j];
                    if (block != null)
                    {
                        var it = block.packs.GetEnumerator ();
                        while (it.MoveNext ())
                        {
                            var v = it.Current.Value;
                            if (saveChunk.instanceInfos == null)
                            {
                                saveChunk.instanceInfos = new List<InstanceInfo> ();
                            }
                            if (saveChunk.argArray == null)
                            {
                                saveChunk.argArray = new List<uint> ();
                            }
                            AABB aabb = new AABB ();
                            for (int xx = 0; xx < v.instanceObjects.Count; ++xx)
                            {
                                var data = v.instanceObjects[xx];
                                saveChunk.instanceInfos.Add (
                                    new InstanceInfo ()
                                    {
                                        posScale = new Vector4 (data.pos.x, data.pos.y, data.pos.z, data.scale),
                                            rot = new Vector4 (data.rot.x, data.rot.y, data.rot.z, data.rot.w)
                                    });

                                if (xx == 0)
                                {
                                    aabb.Init (ref data.aabb);
                                }
                                else
                                {
                                    aabb.Encapsulate (ref data.aabb);
                                }
                            }
                            saveChunk.argArray.Add (v.imm.indexCount);
                            saveChunk.argArray.Add ((uint) v.instanceObjects.Count);
                            saveChunk.argArray.Add (0);
                            saveChunk.argArray.Add (0);
                            saveChunk.argArray.Add (0);

                            InstanceObjectData iod = new InstanceObjectData ();
                            iod.instanceCount = (short) v.instanceObjects.Count;
                            iod.aabb = aabb;
                            iod.pos = aabb.center;
                            iod.rotate = Quaternion.identity;
                            iod.scale = Vector3.one;
                            iod.localScale = Vector3.one;

                            iod.blockId = j;
                            iod.mat = v.imm.material;
                            iod.resName = v.imm.meshName;
                            iod.areaMask = v.imm.areaMask;
                            iod.flag.SetFlag(SceneObject.IgnoreShadowCaster, true);
                            SceneGroupObject.CalcLod(ssc.sceneConfig, ref aabb, ref iod.lodDist);

                            ssc.AddResName(v.imm.meshName);
                            ssc.resAsset.AddResReDirct(
                                LoadMgr.singleton.editorResPath,
                                string.Format ("{0}.asset", iod.resName),
                                ReDirectRes.LogicPath_Common);

                            bool fisrtFind = ssc.matSaveData.FindOrAddMatInfo (iod.mat, out int matIndex, out iod.matHash);
                            iod.matId = matIndex >= 0 ? (ushort) matIndex : ushort.MaxValue;
                            if (fisrtFind)
                                ssc.AddRes ("mat", iod.matHash, AssetDatabase.GetAssetPath (iod.mat));

                            saveChunk.instanceObjects.Add (iod);
                            saveChunk.instanceCount++;
                        }
                    }
                }
            }
        }

        ////////////////////////////Save////////////////////////////
        public static void SaveChunk (BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            int posCount = saveChunk.instanceInfos != null ? saveChunk.instanceInfos.Count : 0;
            bw.Write (posCount);
            for (int j = 0; j < posCount; ++j)
            {
                var ii = saveChunk.instanceInfos[j];
                EditorCommon.WriteVector (bw, ii.posScale);
                EditorCommon.WriteVector (bw, ii.rot);
            }
            int argCount = saveChunk.argArray != null ? saveChunk.argArray.Count : 0;
            bw.Write(argCount);
            for (int j = 0; j < argCount; ++j)
            {
                bw.Write(saveChunk.argArray[j]);
            }
            DebugLog.DebugStream (bw, "ChunkInstance");
        }
    }
}
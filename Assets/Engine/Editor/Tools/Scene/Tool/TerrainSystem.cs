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
    public partial class TerrainSystem : SceneResProcess
    {
        private float[] h;
        private int terrainGridCount;
        private int gridCount;

        static readonly float originalBlockSize = 8f;

        public static TerrainSystem system;
        public override void Init(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init(ref sceneContext, ssContext);
            var t = ssContext.root.Find("UnityTerrain");
            if (t == null)
            {
                t = new GameObject("UnityTerrain").transform;
                t.parent = ssContext.root;
            }
            serialize = SerializeCb;
            system = this;
        }
        public override void UnInit(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            system = null;
        }
        ////////////////////////////PreSerialize////////////////////////////

        ////////////////////////////Serialize////////////////////////////、
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent (out TerrainObject to))
            {
                var process = ssContext.resProcess as TerrainSystem;
                int startOffset = to.chunkID * process.gridCount;
                SceneConfig.TerrainChunk chunk = ssContext.sceneConfig.chunks[to.chunkID];
                chunk.terrainObjData.Copy (to.terrainObjData);
                if (to != null && to.terrainObjData.isValid)
                {
                    if (to.TryGetComponent<MeshFilter> (out var mf))
                    {
                        Mesh m = mf.sharedMesh;
                        if (m != null)
                        {
                            Vector3[] pos = m.vertices;
                            float offsetX = to.chunkID % ssContext.widthCount * ssContext.chunkWidth;
                            float offsetZ = to.chunkID / ssContext.widthCount * ssContext.chunkHeight;

                            for (int j = 0; j < pos.Length; ++j)
                            {
                                Vector3 p = pos[j];
                                p.x += offsetX;
                                p.z += offsetZ;
                                process.h[startOffset + j] = p.y;
                            }
                        }
                    }

                }
                int x = to.chunkID % ssContext.widthCount;
                int z = to.chunkID / ssContext.widthCount;
                float startX = x * ssContext.chunkWidth;
                float startZ = z * ssContext.chunkHeight;
                float gridSize = originalBlockSize * 0.25f;
                for (int posz = 0; posz < process.terrainGridCount; ++posz)
                {
                    for (int posx = 0; posx < process.terrainGridCount; ++posx)
                    {
                        int offset = posx + posz * process.terrainGridCount;
                        Vector3 p = new Vector3 (startX + posx * gridSize, 800, startZ + posz * gridSize);
                        RaycastHit hitinfo;
                        if (Physics.Raycast (p, Vector3.down, out hitinfo, 801, 1 << DefaultGameObjectLayer.TerrainLayer))
                        {
                            if (hitinfo.point.y > process.h[startOffset + offset])
                                process.h[startOffset + offset] = hitinfo.point.y;
                        }
                    }
                }
            }
            EnumFolder (trans, ssContext, ssContext.serialize);
        }
        public override void Serialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            terrainGridCount = EngineContext.terrainGridCount + 1;
            gridCount = terrainGridCount * terrainGridCount;
            int chunkCount = ssContext.widthCount * ssContext.heightCount;

            h = new float[gridCount * chunkCount];
            for (int i = 0; i < h.Length; ++i)
            {
                h[i] = -1;
            }
            base.Serialize (ref sceneContext, ssContext);
            try
            {
                string terrainHeightPath = string.Format ("{0}/TerrainVertex.bytes",
                    sceneContext.configDir);
                using (FileStream fs = new FileStream (terrainHeightPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter (fs);
                    bw.Write (chunkCount);
                    for (int i = 0; i < chunkCount; ++i)
                    {
                        int startOffset = i * gridCount;
                        for (int j = 0; j < gridCount; ++j)
                        {
                            bw.Write (h[startOffset + j]);
                        }
                    }
                }
                AssetDatabase.ImportAsset (terrainHeightPath, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception e)
            {
                Debug.LogError (e.Message);
            }

        }

        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;
            string path = string.Format ("{0}/TerrainVertex.bytes", sceneContext.configDir);
            if (File.Exists (path))
            {
                using (FileStream fs = new FileStream (path, FileMode.Open))
                {
                    BinaryReader br = new BinaryReader (fs);
                    int count = br.ReadInt32 ();
                    int gridCount = EngineContext.terrainGridCount + 1;
                    gridCount *= gridCount;
                    if (ssc.h == null || ssc.h.Length != gridCount * count)
                        ssc.h = new float[gridCount * count];
                    for (int i = 0; i < count; ++i)
                    {
                        int startOffset = i * gridCount;
                        for (int j = 0; j < gridCount; ++j)
                        {
                            ssc.h[startOffset + j] = br.ReadSingle ();
                        }
                    }
                }
            }

            int splatCount = ssc.sceneConfig.bundles.Count;
            for (int i = 0; i < splatCount; ++i)
            {
                SceneConfig.TextureInfo ti = ssc.sceneConfig.bundles[i];
                if (ti.tex != null)
                    ssc.resAsset.AddResReDirct (ti.tex, ti.tex.name);
                if (ti.pbs != null)
                    ssc.resAsset.AddResReDirct (ti.pbs, ti.pbs.name);
            }
        }
        ////////////////////////////Save////////////////////////////
        public override void PreSaveChunk (ref SceneContext sceneContext, BaseSceneContext bsc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            if (i < 0)
                return;
            var ssc = bsc as SceneSaveContext;
            int x = i % ssc.widthCount;
            int z = i / ssc.widthCount;
            var tod = ssc.sceneConfig.chunks[i].terrainObjData;
            if (tod.isValid)
            {
                int texCount = 0;
                byte splatCount = (byte) ssc.sceneConfig.bundles.Count;
                var terrainChunk = ssc.sceneConfig.chunks[i];
                for (int j = 0; j < terrainChunk.splatID.Length; ++j)
                {
                    byte splatID = terrainChunk.splatID[j];
                    if (splatID != 255 && splatID >= splatCount)
                    {

                    }
                    else
                    {
                        texCount++;
                    }
                }

                if (texCount > 1)
                {
                    ssc.resAsset.AddResReDirct (
                        string.Format ("{0}/", sceneContext.terrainDir),
                        string.Format ("Chunk_{0}_{1}_Blend.tga", x, z),
                        ReDirectRes.LogicPath_SceneRes);
                }
                ssc.resAsset.AddResReDirct (
                    string.Format ("{0}/", sceneContext.terrainDir),
                    string.Format ("Chunk_{0}_{1}.asset", x, z),
                    ReDirectRes.LogicPath_SceneRes);
            }
            if (ssc.lightMapData != null)
            {
                ssc.lightMapData.GetLightMap(tod.lightmapComponent.lightMapVolumnIndex, tod.lightmapComponent.lightMapIndex,
                                 out var volumnName, out var lightmap, out var shadowmask);
                string lightmapPath = string.Format("{0}/SceneLightmapBackup/{1}/",
                   sceneContext.configDir,
                   volumnName);
                if (lightmap != null)
                {
                    saveChunk.terrainLightmapIndex = (byte) tod.lightmapComponent.lightMapIndex;
                    saveChunk.terrainLightmapST = tod.lightmapComponent.lightmapUVST;

                    string lightmapNameWithExt = string.Format ("Lightmap_Terrain_Chunk_{0}_{1}-{2}.exr",
                        x.ToString (), z.ToString (),
                        tod.lightmapComponent.lightMapIndex.ToString ());
                    ssc.resAsset.AddResReDirct (
                        lightmapPath,
                        lightmapNameWithExt,
                        ReDirectRes.LogicPath_SceneRes);
                }
                if (shadowmask != null)
                {
                    string shadowMaskNameWithExt = string.Format("Lightmap_Terrain_Chunk_{0}_{1}-{2}.png",
                      x.ToString(), z.ToString(),
                      tod.lightmapComponent.lightMapIndex.ToString());
                    ssc.resAsset.AddResReDirct(
                        lightmapPath,
                        shadowMaskNameWithExt,
                        ReDirectRes.LogicPath_SceneRes);
                }
            }
        }

        public static void SaveHead (BinaryWriter bw, SceneSaveContext ssc)
        {
            byte splatCount = (byte) ssc.sceneConfig.bundles.Count;
            bw.Write (splatCount);
            for (int i = 0; i < splatCount; ++i)
            {
                SceneConfig.TextureInfo ti = ssc.sceneConfig.bundles[i];
                string name = ti.tex != null ? ti.tex.name : "";
                bw.Write (name);
                bw.Write (ti.scale);
                string normalname = ti.pbs != null ? ti.pbs.name : "";
                bw.Write (normalname);
            }
            if (splatCount > 0)
                EditorCommon.WriteVector (bw, ssc.sceneConfig.terrainParam);
            DebugLog.DebugStream (bw, "TerrainHead");
        }

        public static void SaveChunk (BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            bool hasTerrain = false;
            var tt = ssc.sceneConfig.chunks[i];
            if (tt != null && tt.mesh != null)
            {
                byte splatCount = (byte) ssc.sceneConfig.bundles.Count;
                var tod = tt.terrainObjData;
                bool valid = tod.isValid && splatCount > 0;
                bw.Write (valid);
                if (valid)
                {
                    hasTerrain = true;
                    int x = i % ssc.widthCount;
                    int z = i / ssc.widthCount;
                    float offsetX = x * ssc.chunkWidth;
                    float offsetZ = z * ssc.chunkHeight;
                    Bounds aabb0 = tt.aabb0;
                    aabb0.center = aabb0.center + new Vector3 (offsetX, 0, offsetZ);
                    Bounds aabb1 = tt.aabb1;
                    aabb1.center = aabb1.center + new Vector3 (offsetX, 0, offsetZ);
                    Bounds aabb2 = tt.aabb2;
                    aabb2.center = aabb2.center + new Vector3 (offsetX, 0, offsetZ);
                    Bounds aabb3 = tt.aabb3;
                    aabb3.center = aabb3.center + new Vector3 (offsetX, 0, offsetZ);
                    EditorCommon.SaveAABB (bw, ref aabb0);
                    if (!(ssc.widthCount == 1 && ssc.heightCount == 1))
                    {
                        EditorCommon.SaveAABB (bw, ref aabb1);
                        EditorCommon.SaveAABB (bw, ref aabb2);
                        EditorCommon.SaveAABB (bw, ref aabb3);
                    }
                    // uint normalFlag = 0;
                    for (int j = 0; j < tt.splatID.Length; ++j)
                    {
                        byte splatID = tt.splatID[j];
                        if (splatID != 255 && splatID >= splatCount)
                        {
                            splatID = 255;
                            DebugLog.AddWarningLog2 ("splat count error, total count:{0} chunk splat index:{1} chunk id:{2}",
                                splatCount.ToString (), splatID.ToString (), i.ToString ());
                        }
                        bw.Write (splatID);
                    }

                    bw.Write (saveChunk.terrainLightmapIndex);
                    if (saveChunk.terrainLightmapIndex != 255)
                        EditorCommon.WriteVector (bw, saveChunk.terrainLightmapST);

                }

            }
            else
            {
                bw.Write (false);
            }

            if (!hasTerrain)
            {
                bw.Write (saveChunk.terrainLightmapIndex);
                if (saveChunk.terrainLightmapIndex != 255)
                    EditorCommon.WriteVector (bw, saveChunk.terrainLightmapST);
            }
            DebugLog.DebugStream (bw, "ChunkTerrain");
            int gridCount = EngineContext.terrainGridCount + 1;
            gridCount *= gridCount;

            int startOffset = i * gridCount;
            int vertexCount = gridCount;
            bw.Write (vertexCount);
            if (vertexCount > 0)
            {
                for (int j = 0; j < vertexCount; ++j)
                {
                    bw.Write (ssc.h[j + startOffset]);
                }
            }
            DebugLog.DebugStream (bw, "ChunkHeight");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class SceneSaveContext : BaseSceneContext
    {
        public int[] ChunkStreamOffset;
        public int chunkStreamPos;
        public int chunkPvsStreamPos;
        public ResAssets resAsset = new ResAssets ();
        public MatSaveData matSaveData = new MatSaveData ();
        public LightmapVolumnData lightMapData = null;
        public QuadTreeContext treeContext = new QuadTreeContext ();
        public Dictionary<string, Dictionary<uint, string>> resPath = new Dictionary<string, Dictionary<uint, string>> ();
        public float[] h;
        public bool AddResName (string str)
        {
            return resAsset.AddResName (str) >= 0;
        }

        public void AddRes (string type, uint hash, string str)
        {
            if (!resPath.TryGetValue (type, out var list))
            {
                list = new Dictionary<uint, string> ();
                resPath.Add (type, list);
            }
            str = str.ToLower ();
            if (!list.ContainsKey (hash))
            {
                list.Add (hash, str);
            }
        }

        public void SaveStringIndex (BinaryWriter bw, string str)
        {
            resAsset.SaveStringIndex (bw, str);
        }
    }

    public class DynamicSceneSaveContext : BaseSceneContext
    {
        public DynamicSceneData dsd;
    }

    public partial class SceneSerialize
    {
        public static void InitQuadTree (SceneSaveContext ssc, ChunkSaveData saveChunk, int i)
        {
            int x = i % ssc.widthCount;
            int z = i / ssc.widthCount;
            Vector3 chunkOffset = new Vector3 (x * ssc.chunkWidth, 0, z * ssc.chunkHeight);
            SceneQuadTree.InitQuadTree (ssc.treeContext, ssc.chunkWidth, ssc.chunkHeight, ref chunkOffset);
            var terrainChunk = ssc.sceneConfig.chunks[i];
            if (terrainChunk.mesh != null)
            {
                SceneQuadBlock sceneQuadBlock = new SceneQuadBlock ();
                Bounds bound = terrainChunk.mesh.bounds;
                Vector3 center = bound.center;
                Vector3 size = bound.size;
                if (size.y == 0)
                    size.y = 0.1f;

                center.x = (x + 0.5f) * ssc.chunkWidth;
                center.z = (z + 0.5f) * ssc.chunkHeight;
                bound.center = center;
                bound.size = size;
                sceneQuadBlock.aabb.Init (ref bound);
                saveChunk.sceneQuadBlocks[0] = sceneQuadBlock;
                EditorQuardTreeNode node = ssc.treeContext.treeNodes[0];
                node.aabb = AABB.Create (bound);
            }
        }

        public static void PostInitQuadTree (SceneSaveContext ssc, ChunkSaveData saveChunk, int i)
        {
            SceneQuadTree.EndQuadTree (ssc.treeContext);

            for (int j = 0; j < ssc.treeContext.treeNodes.Length; ++j)
            {
                EditorQuardTreeNode node = ssc.treeContext.treeNodes[j];
                if (node.hasData)
                {
                    SceneQuadBlock sqb = new SceneQuadBlock ();
                    sqb.quadIndex = (byte) j;
                    sqb.aabb.Init (ref node.aabb);
                    if (node.data.Count > 0)
                    {
                        sqb.sceneObjectGroupIndex = (short) saveChunk.sceneObjectIndex.Count;
                        saveChunk.sceneObjectIndex.Add ((ushort) node.data.Count);
                        for (int kk = 0; kk < node.data.Count; ++kk)
                        {
                            var data = node.data[kk];
                            saveChunk.sceneObjectIndex.Add ((ushort) data.QuadNodeId);
                        }
                    }
                    saveChunk.sceneQuadBlocks[sqb.quadIndex] = sqb;
                }
            }
        }
    }
}
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
namespace CFEngine.Editor
{
    [Serializable]
    public class MergeMesh
    {
        public Mesh mesh;
        public int blockID;
        public Material material;
    }

    [Serializable]
    public class ChunkMergeData
    {
        public List<MergeMesh> mergeMeshs = new List<MergeMesh> ();
    }

    [Serializable]
    public class ChunkMergeMesh : ScriptableObject
    {
        public List<ChunkMergeData> chunks = new List<ChunkMergeData> ();
    }
}
#endif
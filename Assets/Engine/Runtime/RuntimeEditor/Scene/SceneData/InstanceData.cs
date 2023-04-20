#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [System.Serializable]
    public class InstanceMeshMat
    {
        public uint hash;
        public Mesh mesh;
        public Material material;
        public uint areaMask = 0xffffffff;
        [NonSerialized]
        public uint indexCount = 0;
        [NonSerialized]
        public string meshName = "";
    }

    [System.Serializable]
    public class InstanceData
    {
        public Vector4 pos = Vector4.zero;
        public Quaternion rot = Quaternion.identity;
        public float scale = 1;
        public AABB aabb;
        public FlagMask flag;
        public int instanceIndex;
        public int blockId = 0;
    }
    [System.Serializable]
    public class InstanceObjectData : SceneObjectData
    {
        public short instanceCount = 0;
        public uint indexCount = 0;
    }

}
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class MergedMeshRef
    {
        public Mesh mergeMesh;
        public int blockID = -1;
        public AABB mergeAABB;
        public List<MeshRenderObject> mergeObjets = new List<MeshRenderObject> ();
        public List<MeshRenderObject> smallMergeObjets = new List<MeshRenderObject> ();
        public int targetMergeGroupIndex = -1;

    }
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class MaterialGroup : MonoBehaviour
    {
        public Material material;
        public List<MergedMeshRef> mergeMesh = new List<MergedMeshRef> ();
        public int meshCount = 0;

        [System.NonSerialized]
        public static MaterialGroup currentMG;

        [System.NonSerialized]
        public static int blockID = -1;

    }
}
#endif
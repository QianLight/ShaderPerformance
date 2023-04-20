#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent(typeof(MeshCollider))]
    public class PVSCollider : MonoBehaviour
    {
        public int index;
        public MeshRenderObject mroRef;
        public int subIndex = -1;
        public bool debug;

        public void DrawGizmos()
        {
            if (mroRef != null && this.gameObject.activeInHierarchy)
            {
                var m = mroRef.GetMesh();
                if (m != null)
                {
                    var trans = mroRef.transform;
                    Gizmos.DrawWireMesh(m, trans.position, trans.rotation, trans.lossyScale);
                }
            }
        }
    }
}
#endif
using UnityEngine;
using System;
using System.Collections.Generic;

namespace MeshLOD
{
    [Serializable]
    public class MeshLODDataGroup // 序列化用
    {
        public List<MeshLODData> meshLODDataList;

        public MeshLODDataGroup(List<MeshLODData> meshLODDataList)
        {
            this.meshLODDataList = meshLODDataList;
        }
    }
    
    [Serializable]
    public class MeshLODData
    {
        public Mesh mesh;
        public Mesh additionalVertexStreamMesh;

        public MeshLODData(Transform targetTransform, MeshRenderNode meshRenderNode, bool isNeedBakeVertexColor)
        {
            MeshFilter meshFilter = targetTransform.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                mesh = meshFilter.sharedMesh;
            }
#if UNITY_EDITOR
            if (isNeedBakeVertexColor && meshRenderNode.meshFilter != null)
            {
                BakeMeshVertexColor(meshRenderNode.meshFilter.sharedMesh);
            }
            else
            {
                additionalVertexStreamMesh = null;
            }
#endif
        }

#if UNITY_EDITOR
        private void BakeMeshVertexColor(Mesh meshLOD0)
        {
            if (mesh == null || meshLOD0 == null)
            {
                Debug.Log("Bake Mesh src or dst mesh is Null");
                return;
            }

            additionalVertexStreamMesh = MeshLODUtility.BakeMeshVertexColor(meshLOD0, mesh);
        }
#endif
    }
}
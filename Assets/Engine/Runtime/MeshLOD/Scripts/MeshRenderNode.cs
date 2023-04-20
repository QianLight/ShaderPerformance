using System;
using UnityEngine;

namespace MeshLOD
{
    [Serializable]
    public class MeshRenderNode
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public MeshRenderNode(Transform targetTransform)
        {
            meshFilter = targetTransform.GetComponent<MeshFilter>();
            meshRenderer = targetTransform.GetComponent<MeshRenderer>();
        }

        public void SetRender(MeshLODData meshLODData)
        {
            if (meshLODData == null)
            {
                return;
            }

            if (meshFilter != null)
            {
                meshFilter.sharedMesh = meshLODData.mesh;
            }

            if (meshRenderer != null)
            {
                meshRenderer.forceRenderingOff = false;
                meshRenderer.additionalVertexStreams = meshLODData.additionalVertexStreamMesh;
            }
        }

        public void Culling()
        {
            if (meshRenderer != null)
            {
                meshRenderer.forceRenderingOff = true;
            }
        }
    }
}
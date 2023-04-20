using Impostors.Managers;
using UnityEngine;

namespace Impostors.RenderInstructions
{
    public sealed class DrawMeshInstruction : IRenderInstruction
    {
        public readonly Mesh Mesh;
        public readonly Matrix4x4 Matrix;
        public readonly Material m_Material;
        public readonly int SubmeshIndex;
        public readonly int ShaderPass;
        public readonly MaterialPropertyBlock PropertyBlock;
        
        
        
        public DrawMeshInstruction(Mesh mesh, Matrix4x4 matrix, Material mMaterial, int submeshIndex, int shaderPass,
            MaterialPropertyBlock propertyBlock)
        {
            Mesh = mesh;
            Matrix = matrix;


            m_Material = mMaterial;

            SubmeshIndex = submeshIndex;
            ShaderPass = shaderPass;
            PropertyBlock = propertyBlock;
        }

        public void ApplyCommandBuffer(CommandBufferProxy bufferProxy)
        {
            bufferProxy.CommandBuffer.DrawMesh(Mesh, Matrix, m_Material, SubmeshIndex, ShaderPass, PropertyBlock);
        }
    }
}
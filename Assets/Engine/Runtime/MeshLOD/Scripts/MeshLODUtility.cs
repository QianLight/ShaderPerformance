using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshLOD
{
    public class MeshLODUtility
    {
        public static float CalculateScreenMultiplier(float fov, float lodBias)
        {
            float multiplier = 2 * Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) / lodBias;
            return multiplier;
        }

#if UNITY_EDITOR

        struct MeshData
        {
            public Vector4 meshColor;
            public Vector2 meshUV;
        }

        private static int _kernelCsMain64Thread;

        private static readonly string computerShaderPath = "Assets/Engine/Runtime/MeshLOD/Shaders/MeshVertexColorBaker.compute";
        private static readonly int DstMeshDataBufferID = Shader.PropertyToID("_DstMeshDataBuffer");
        private static readonly int SrcMeshDataBufferID = Shader.PropertyToID("_SrcMeshDataBuffer");
        private static readonly int ResultColorBufferID = Shader.PropertyToID("_ResultColorBuffer");
        private static readonly int LOd0VertexCountID = Shader.PropertyToID("_LOD0VertexCount");
        private static readonly int DstVertexCountID = Shader.PropertyToID("_DstVertexCount");

        private static ComputeShader meshBakeComputeShader;


        public static Mesh BakeMeshVertexColor(Mesh srcMesh, Mesh dstMesh)
        {
            if (srcMesh.colors.Length <= 0)
            {
                return null;
            }

            if (meshBakeComputeShader == null)
            {
                meshBakeComputeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(computerShaderPath);
                if (meshBakeComputeShader == null)
                {
                    Debug.Log("ComputeShader 为空");
                    return null;
                }
            }

            if (dstMesh == null)
            {
                Debug.Log("dstMesh 为空");
                return null;
            }

            if (srcMesh == null)
            {
                Debug.Log("srcMesh 为空");
                return null;
            }


            Mesh outputMesh = new Mesh
            {
                vertices = dstMesh.vertices,
                colors = dstMesh.colors,
                uv = dstMesh.uv,
                triangles = dstMesh.triangles,
                normals = dstMesh.normals,
                tangents = dstMesh.tangents
            };
            outputMesh.name = dstMesh.name + "_BakeVC";

            int srcMeshVertexCount = srcMesh.vertexCount;
            int dstMeshVertexCount = dstMesh.vertexCount;

            _kernelCsMain64Thread = meshBakeComputeShader.FindKernel("CSMain64Thread");
            ComputeBuffer srcMeshDataBuffer = new ComputeBuffer(srcMeshVertexCount, sizeof(float) * 6, ComputeBufferType.Structured);
            ComputeBuffer dstMeshDataBuffer = new ComputeBuffer(dstMeshVertexCount, sizeof(float) * 6, ComputeBufferType.Structured);
            ComputeBuffer resultColorBuffer = new ComputeBuffer(dstMeshVertexCount, sizeof(float) * 4, ComputeBufferType.Structured);
            MeshData[] srcMeshDatas = new MeshData[srcMeshVertexCount];
            MeshData[] dstMeshDatas = new MeshData[dstMeshVertexCount];
            Vector4[] resultColors = new Vector4[dstMeshVertexCount];
            for (int i = 0; i < resultColors.Length; i++)
            {
                resultColors[i] = Vector4.one;
            }

            for (int i = 0; i < srcMeshVertexCount; i++)
            {
                srcMeshDatas[i] = new MeshData()
                {
                    meshColor = srcMesh.colors[i],
                    meshUV = srcMesh.uv[i]
                };
            }

            for (int i = 0; i < dstMeshVertexCount; i++)
            {
                dstMeshDatas[i] = new MeshData()
                {
                    meshColor = Color.white,
                    meshUV = dstMesh.uv[i]
                };
            }

            srcMeshDataBuffer.SetData(srcMeshDatas);
            dstMeshDataBuffer.SetData(dstMeshDatas);
            resultColorBuffer.SetData(resultColors);
            meshBakeComputeShader.SetBuffer(_kernelCsMain64Thread, SrcMeshDataBufferID, srcMeshDataBuffer);
            meshBakeComputeShader.SetBuffer(_kernelCsMain64Thread, DstMeshDataBufferID, dstMeshDataBuffer);
            meshBakeComputeShader.SetBuffer(_kernelCsMain64Thread, ResultColorBufferID, resultColorBuffer);
            meshBakeComputeShader.SetInt(LOd0VertexCountID, srcMeshVertexCount);
            meshBakeComputeShader.SetInt(DstVertexCountID, dstMeshVertexCount);

            int threadX = dstMeshVertexCount / 64 + 1;
            meshBakeComputeShader.Dispatch(_kernelCsMain64Thread, threadX, 1, 1);

            resultColorBuffer.GetData(resultColors);
            Color[] replaceColors = new Color[dstMeshVertexCount];
            for (int i = 0; i < dstMeshVertexCount; i++)
            {
                replaceColors[i] = resultColors[i];
            }

            outputMesh.SetColors(replaceColors);
            outputMesh.UploadMeshData(false);

            srcMeshDataBuffer.Dispose();
            dstMeshDataBuffer.Dispose();
            resultColorBuffer.Dispose();

            return outputMesh;
        }
        
#endif
    }
}
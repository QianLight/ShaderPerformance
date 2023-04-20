using System;
using System.Collections;
using System.Collections.Generic;
using Impostors.ImpostorsChunkMesh;
using Impostors.Structs;
using Impostors.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace CFEngine
{
    public class MultiLayerJob
    {

        struct VertexTemplate
        {
            public float3 pos;
            public float3 normal;
            public float4 tangent;
            public float2 uv0;
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct MeshDataJob : IJobParallelFor
        {
            [ReadOnly] public Mesh.MeshDataArray inputMeshes;
            public Mesh.MeshData outputMesh;

            public NativeArray<bool> meshShowState;
            [ReadOnly] public NativeArray<int> vertexStarts;
            [ReadOnly] public NativeArray<int> indexStarts;
            //[DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float4x4> localToWorldMatrixes;
            public NativeArray<float3x2> bounds;

            public void InitInputArrays(int meshCount)
            {
                
                Debug.Log("MeshDataJob:InitInputArrays "+meshCount);
                vertexStarts =
                    new NativeArray<int>(meshCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                indexStarts =
                    new NativeArray<int>(meshCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                //localToWorldMatrixes = new NativeArray<float4x4>(meshCount, Allocator.Persistent,
                //    NativeArrayOptions.UninitializedMemory);
                bounds = new NativeArray<float3x2>(meshCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
                
                meshShowState = new NativeArray<bool>(meshCount, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
            }

            void IJobParallelFor.Execute(int index)
            {
                
                if(!meshShowState[index]) return;
                
                //handle vertices
                Mesh.MeshData inputMesh = inputMeshes[index];
                int vertexCount = inputMesh.vertexCount;
                //float4x4 matrix = localToWorldMatrixes[index];
                int vStart = vertexStarts[index];

                NativeArray<VertexTemplate> inputVertices = inputMesh.GetVertexData<VertexTemplate>();
                NativeArray<VertexTemplate> outputVertices = outputMesh.GetVertexData<VertexTemplate>();

                VertexTemplate tempVertex = new VertexTemplate();
                float3x2 bound = bounds[index];
                for (int i = 0; i < vertexCount; ++i)
                {
                    VertexTemplate oneVertex = inputVertices[i];
                    float3 pos = oneVertex.pos;
                    //pos = math.mul(matrix, new float4(pos, 1)).xyz;
                    tempVertex.pos = pos;
                    tempVertex.normal = oneVertex.normal;
                    tempVertex.tangent = oneVertex.tangent;

                    tempVertex.uv0 = oneVertex.uv0;
                    outputVertices[i + vStart] = tempVertex;

                    //recalculate bound
                    if (i == 0)
                    {
                        bound.c0 = pos;
                        bound.c1 = pos;
                    }
                    else
                    {
                        bound.c0 = math.min(bound.c0, pos);
                        bound.c1 = math.max(bound.c1, pos);
                    }
                }

                bounds[index] = bound;

                //handle indices
                //input indices' format is UInt16, so use ushort
                int iStart = indexStarts[index];
                int indexCount = inputMesh.GetSubMesh(0).indexCount;
                NativeArray<ushort> inputIndices = inputMesh.GetIndexData<ushort>();
                NativeArray<int> outputIndices = outputMesh.GetIndexData<int>();
                for (int i = 0; i < indexCount; ++i)
                {
                    outputIndices[i + iStart] = vStart + inputIndices[i];
                }
            }

            public void Dispose()
            {
               // Debug.Log("MeshDataJob:Dispose!");
                
                //if (Application.isPlaying)
                {
                    if (vertexStarts.IsCreated)
                        vertexStarts.Dispose();
                
                    if (indexStarts.IsCreated)
                        indexStarts.Dispose();
                }
                
                
                if (meshShowState.IsCreated)
                    meshShowState.Dispose();
                if (bounds.IsCreated)
                    bounds.Dispose();
                
                inputMeshes.Dispose();
            }
        }

        private MeshDataJob m_MeshDataJob;
        //private Mesh.MeshDataArray outputMeshArray;

        class MeshDataCache
        {
            public Mesh mesh;
            public int vertexCount;
            public int indexCount;
            
            public MeshDataCache(Mesh m)
            {
                mesh = m;
                vertexCount = mesh.vertexCount;
                indexCount = (int)mesh.GetIndexCount(0);
            }
        }

        private List<MeshDataCache> m_AllMesh;
        private Mesh m_CombinedMesh;
        private VertexAttributeDescriptor[] vertexAttributeDescriptor;

        public void Init(MeshFilter[] filters)
        {
            m_MeshDataJob = new MeshDataJob();
            m_MeshDataJob.InitInputArrays(filters.Length);
            m_AllMesh = new List<MeshDataCache>(filters.Length);

            List<Mesh> tmpMesh = new List<Mesh>(filters.Length);

            for (int meshIndex = 0; meshIndex < filters.Length; ++meshIndex)
            {
                MeshFilter mf = filters[meshIndex];
                tmpMesh.Add(mf.sharedMesh);
                m_AllMesh.Add(new MeshDataCache(mf.sharedMesh));
                m_MeshDataJob.meshShowState[meshIndex] = false;
            }

            m_MeshDataJob.inputMeshes = Mesh.AcquireReadOnlyMeshData(tmpMesh);

            m_CombinedMesh = new Mesh { name = "CombinedMesh" };
            vertexAttributeDescriptor = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
            };
            
            //outputMeshArray = Mesh.AllocateWritableMeshData(1);
        }

        public Mesh ShowAllMesh()
        {
            for (int meshIndex = 0; meshIndex < m_MeshDataJob.inputMeshes.Length; ++meshIndex)
            {
                m_MeshDataJob.meshShowState[meshIndex] = true;
            }
            
            return GetCombineMesh();
        }

        public void SetRefreshMesh(int nIndex, bool state)
        {
            m_MeshDataJob.meshShowState[nIndex] = state;
        }

        private MeshDataCache m_MeshDataCache;
        public Mesh GetCombineMesh()
        {

            Mesh.MeshDataArray outputMeshArray = Mesh.AllocateWritableMeshData(1);
            int meshCount = m_MeshDataJob.inputMeshes.Length;
            int vertexStart = 0;
            int indexStart = 0;

            for (int meshIndex = 0; meshIndex < meshCount; ++meshIndex)
            {
                if (!m_MeshDataJob.meshShowState[meshIndex]) continue;

                m_MeshDataCache = m_AllMesh[meshIndex];
                m_MeshDataJob.vertexStarts[meshIndex] = vertexStart;
                m_MeshDataJob.indexStarts[meshIndex] = indexStart;
                
                vertexStart += m_MeshDataCache.vertexCount;
                indexStart += m_MeshDataCache.indexCount;
            }

            if (vertexStart == 0) return null;


            m_MeshDataJob.outputMesh = outputMeshArray[0];
            m_MeshDataJob.outputMesh.SetVertexBufferParams(vertexStart,vertexAttributeDescriptor);
            m_MeshDataJob.outputMesh.SetIndexBufferParams(indexStart, IndexFormat.UInt32);
            
            JobHandle jobHandle = m_MeshDataJob.Schedule(meshCount, 16);
            jobHandle.Complete();



            SubMeshDescriptor subMeshDesc = new SubMeshDescriptor
            {
                indexStart = 0,
                indexCount = indexStart,
                topology = MeshTopology.Triangles,
                firstVertex = 0,
                vertexCount = vertexStart,
            };

            //calculate bound     
            float3x2 bounds = new float3x2(new float3(Mathf.Infinity), new float3(Mathf.NegativeInfinity));
            for (var i = 0; i < meshCount; ++i)
            {
                if (!m_MeshDataJob.meshShowState[i]) continue;

                var b = m_MeshDataJob.bounds[i];
                bounds.c0 = math.min(bounds.c0, b.c0);
                bounds.c1 = math.max(bounds.c1, b.c1);
            }


            subMeshDesc.bounds = new Bounds((bounds.c0 + bounds.c1) * 0.5f, bounds.c1 - bounds.c0);

            m_MeshDataJob.outputMesh.subMeshCount = 1;
            m_MeshDataJob.outputMesh.SetSubMesh(0, subMeshDesc,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            

            Mesh.ApplyAndDisposeWritableMeshData(outputMeshArray, m_CombinedMesh,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);


            m_CombinedMesh.bounds = subMeshDesc.bounds;

            return m_CombinedMesh;
        }

        public void OnDestory()
        {
            m_MeshDataJob.Dispose();
        }

        // public  Mesh CreateMesh_MeshData(List<MeshFilter> meshFilters)
        // {
        //
        //     MeshDataJob job = new MeshDataJob();
        //     int meshCount = meshFilters.Count;
        //     job.InitInputArrays(meshCount);
        //     int vertexStart = 0;
        //     int indexStart = 0;
        //     List<Mesh> inputMeshes = new List<Mesh>(meshCount);
        //     for (int meshIndex = 0; meshIndex < meshCount; ++meshIndex)
        //     {
        //         MeshFilter mf = meshFilters[meshIndex];
        //         job.vertexStarts[meshIndex] = vertexStart;
        //         job.indexStarts[meshIndex] = indexStart;
        //         job.localToWorldMatrixes[meshIndex] = Matrix4x4.identity;//  mf.gameObject.transform.localToWorldMatrix;
        //         Mesh mesh = mf.sharedMesh;
        //         vertexStart += mesh.vertexCount;
        //         indexStart += (int)mesh.GetIndexCount(0);
        //         inputMeshes.Add(mesh);
        //     }
        //
        //     //get input meshes
        //     job.inputMeshes = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        //     //create output combined mesh
        //     Mesh.MeshDataArray outputMeshArray = Mesh.AllocateWritableMeshData(1);
        //     job.outputMesh = outputMeshArray[0];
        //     //vertexStart equal to vertex count
        //     //indexStart equal to index count
        //     job.outputMesh.SetVertexBufferParams(vertexStart,
        //         new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        //         new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        //         new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
        //         new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
        //     job.outputMesh.SetIndexBufferParams(indexStart, IndexFormat.UInt32);
        //
        //     JobHandle jobHandle = job.Schedule(meshCount, 16);
        //     jobHandle.Complete();
        //
        //     Mesh combinedMesh = new Mesh { name = "CombinedMesh" };
        //     SubMeshDescriptor subMeshDesc = new SubMeshDescriptor
        //     {
        //         indexStart = 0,
        //         indexCount = indexStart,
        //         topology = MeshTopology.Triangles,
        //         firstVertex = 0,
        //         vertexCount = vertexStart,
        //     };
        //     //calculate bound     
        //     float3x2 bounds = new float3x2(new float3(Mathf.Infinity), new float3(Mathf.NegativeInfinity));
        //     for (var i = 0; i < meshCount; ++i)
        //     {
        //         var b = job.bounds[i];
        //         bounds.c0 = math.min(bounds.c0, b.c0);
        //         bounds.c1 = math.max(bounds.c1, b.c1);
        //     }
        //
        //     subMeshDesc.bounds = new Bounds((bounds.c0 + bounds.c1) * 0.5f, bounds.c1 - bounds.c0);
        //
        //     job.outputMesh.subMeshCount = 1;
        //     job.outputMesh.SetSubMesh(0, subMeshDesc,
        //         MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
        //         MeshUpdateFlags.DontNotifyMeshUsers);
        //     Mesh.ApplyAndDisposeWritableMeshData(outputMeshArray, combinedMesh,
        //         MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
        //         MeshUpdateFlags.DontNotifyMeshUsers);
        //     combinedMesh.bounds = subMeshDesc.bounds;
        //     // MeshFilter meshFilter = combinedMeshGo.GetComponent<MeshFilter>();
        //     // meshFilter.sharedMesh = combinedMesh;
        //
        //     job.inputMeshes.Dispose();
        //     job.bounds.Dispose();
        //     return combinedMesh;
        //     //UnityEngine.Debug.Log(string.Format("CombineMeshDots cost:{0}ms", sw.ElapsedMilliseconds));
        //
        // }

//     public static Mesh CreateMesh_MeshData(List<MeshFilter> meshFilters)
        //     {
        //
        //         var jobs = new ProcessMeshDataJob();
        //         jobs.CreateInputArrays(meshFilters.Count);
        //         var inputMeshes = new List<Mesh>(meshFilters.Count);
        //
        //         var vertexStart = 0;
        //         var indexStart = 0;
        //         var meshCount = 0;
        //         for (var i = 0; i < meshFilters.Count; ++i)
        //         {
        //             var mf = meshFilters[i];
        //             var go = mf.gameObject;
        //             var mesh = mf.sharedMesh;
        //             inputMeshes.Add(mesh);
        //             jobs.vertexStart[meshCount] = vertexStart;
        //             jobs.indexStart[meshCount] = indexStart;
        //             jobs.xform[meshCount] = go.transform.localToWorldMatrix;
        //             vertexStart += mesh.vertexCount;
        //             indexStart += (int)mesh.GetIndexCount(0);
        //             jobs.bounds[meshCount] = new float3x2(new float3(Mathf.Infinity), new float3(Mathf.NegativeInfinity));
        //             ++meshCount;
        //         }
        //
        //         jobs.meshData = Mesh.AcquireReadOnlyMeshData(inputMeshes);
        //
        //         var outputMeshData = Mesh.AllocateWritableMeshData(1);
        //         jobs.outputMesh = outputMeshData[0];
        //         jobs.outputMesh.SetIndexBufferParams(indexStart, IndexFormat.UInt32);
        //         // jobs.outputMesh.SetVertexBufferParams(vertexStart,
        //         //     new VertexAttributeDescriptor(VertexAttribute.Position),
        //         //     new VertexAttributeDescriptor(VertexAttribute.Normal));
        //         
        //         jobs.outputMesh.SetVertexBufferParams(vertexStart,
        //             new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
        //             new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
        //             new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
        //             new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
        //
        //         var handle = jobs.Schedule(meshCount, 4);
        //
        //         var newMesh = new Mesh();
        //         newMesh.name = "JobCloudMesh";
        //         var sm = new SubMeshDescriptor(0, indexStart, MeshTopology.Triangles);
        //         sm.firstVertex = 0;
        //         sm.vertexCount = vertexStart;
        //
        //         handle.Complete();
        //
        //         var bounds = new float3x2(new float3(Mathf.Infinity), new float3(Mathf.NegativeInfinity));
        //         for (var i = 0; i < meshCount; ++i)
        //         {
        //             var b = jobs.bounds[i];
        //             bounds.c0 = math.min(bounds.c0, b.c0);
        //             bounds.c1 = math.max(bounds.c1, b.c1);
        //         }
        //
        //         sm.bounds = new Bounds((bounds.c0 + bounds.c1) * 0.5f, bounds.c1 - bounds.c0);
        //         jobs.outputMesh.subMeshCount = 1;
        //         jobs.outputMesh.SetSubMesh(0, sm,
        //             MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
        //             MeshUpdateFlags.DontNotifyMeshUsers);
        //         Mesh.ApplyAndDisposeWritableMeshData(outputMeshData, new[] { newMesh },
        //             MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
        //             MeshUpdateFlags.DontNotifyMeshUsers);
        //         newMesh.bounds = sm.bounds;
        //         //newMesh.RecalculateNormals();
        //         jobs.meshData.Dispose();
        //         jobs.bounds.Dispose();
        //         
        //         return newMesh;
        //     }
        //
        //     [BurstCompile]
        // struct ProcessMeshDataJob : IJobParallelFor
        // {
        //     [ReadOnly] public Mesh.MeshDataArray meshData;
        //     public Mesh.MeshData outputMesh;
        //     [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> vertexStart;
        //     [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> indexStart;
        //     [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float4x4> xform;
        //     public NativeArray<float3x2> bounds;
        //
        //     [NativeDisableContainerSafetyRestriction] NativeArray<float3> tempVertices;
        //     [NativeDisableContainerSafetyRestriction] NativeArray<float3> tempNormals;
        //
        //     public void CreateInputArrays(int meshCount)
        //     {
        //         vertexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        //         indexStart = new NativeArray<int>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        //         xform = new NativeArray<float4x4>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        //         bounds = new NativeArray<float3x2>(meshCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        //     }
        //
        //     public void Execute(int index)
        //     {
        //         var data = meshData[index];
        //         var vCount = data.vertexCount;
        //         var mat = xform[index];
        //         var vStart = vertexStart[index];
        //
        //         // Allocate temporary arrays for input mesh vertices/normals
        //         if (!tempVertices.IsCreated || tempVertices.Length < vCount)
        //         {
        //             if (tempVertices.IsCreated) tempVertices.Dispose();
        //             tempVertices = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        //         }
        //         if (!tempNormals.IsCreated || tempNormals.Length < vCount)
        //         {
        //             if (tempNormals.IsCreated) tempNormals.Dispose();
        //             tempNormals = new NativeArray<float3>(vCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        //         }
        //         // Read input mesh vertices/normals into temporary arrays -- this will
        //         // do any necessary format conversions into float3 data
        //         data.GetVertices(tempVertices.Reinterpret<Vector3>());
        //         data.GetNormals(tempNormals.Reinterpret<Vector3>());
        //
        //         var outputVerts = outputMesh.GetVertexData<Vector3>();
        //         var outputNormals = outputMesh.GetVertexData<Vector3>();
        //
        //         // Transform input mesh vertices/normals, write into destination mesh,
        //         // compute transformed mesh bounds.
        //         var b = bounds[index];
        //         for (var i = 0; i < vCount; ++i)
        //         {
        //             var pos = tempVertices[i];
        //             pos = math.mul(mat, new float4(pos, 1)).xyz;
        //             outputVerts[i+vStart] = pos;
        //             var nor = tempNormals[i];
        //             nor = math.normalize(math.mul(mat, new float4(nor, 0)).xyz);
        //             outputNormals[i+vStart] = nor;
        //             b.c0 = math.min(b.c0, pos);
        //             b.c1 = math.max(b.c1, pos);
        //         }
        //         bounds[index] = b;
        //
        //         // Write input mesh indices into destination index buffer
        //         var tStart = indexStart[index];
        //         var tCount = data.GetSubMesh(0).indexCount;
        //         var outputTris = outputMesh.GetIndexData<int>();
        //         if (data.indexFormat == IndexFormat.UInt16)
        //         {
        //             var tris = data.GetIndexData<ushort>();
        //             for (var i = 0; i < tCount; ++i)
        //                 outputTris[i + tStart] = vStart + tris[i];
        //         }
        //         else
        //         {
        //             var tris = data.GetIndexData<int>();
        //             for (var i = 0; i < tCount; ++i)
        //                 outputTris[i + tStart] = vStart + tris[i];
        //         }
        //     }
        //}
    }
}

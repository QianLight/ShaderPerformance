#if UNITY_EDITOR
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace CFEngine
{
    public struct PVSGenCellSamplePointsJob : IJobParallelFor
    {
        [ReadOnly] public int NumRays;
        [ReadOnly] public AABB InputCellBounds;
        [ReadOnly] public NativeArray<AABB> InputMeshBounds;
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float> RandomNumbers;
        [NativeDisableParallelForRestriction] public NativeArray<bool> BakeResulst;
        [NativeDisableParallelForRestriction] public NativeArray<Vector3> RaySampleResults;   // length = JobCount * RayNum * enumRaySampleData.Size


        public void Execute(int index)
        {
            BakeResulst[index] = false;
            var MeshBounds = InputMeshBounds[index];

            Vector3 meshToCellCenter = InputCellBounds.center - MeshBounds.center;
            float meshToCellDistance = meshToCellCenter.magnitude;

            float meshSize = (MeshBounds.size * 0.5f).magnitude;
            float sizeRatio = meshSize / meshToCellDistance;

            // Treat meshes whose projected angle is greater than 90 degrees as visible, since it becomes overly costly to determine if these are visible
            // (consider a large close mesh that only has a tiny part visible)
            if (sizeRatio > 1.0f)
            {
                BakeResulst[index] = true;
                return;
            }

            NativeArray<Vector3> facemin = new NativeArray<Vector3>(12, Allocator.Temp);
            NativeArray<Vector3> extents = new NativeArray<Vector3>(12, Allocator.Temp);
            NativeArray<Vector3> normals = new NativeArray<Vector3>(6, Allocator.Temp); // left right bottom top back forward

            normals[0] = new Vector3(-1.0f, 0.0f, 0.0f);
            normals[1] = new Vector3(1.0f, 0.0f, 0.0f);
            normals[2] = new Vector3(0.0f, -1.0f, 0.0f);
            normals[3] = new Vector3(0.0f, 1.0f, 0.0f);
            normals[4] = new Vector3(0.0f, 0.0f, -1.0f);
            normals[5] = new Vector3(0.0f, 0.0f, 1.0f);

            int idx = 0;
            facemin[idx + 0] = new Vector3(InputCellBounds.min.x, InputCellBounds.min.y, InputCellBounds.min.z);
            facemin[idx + 1] = new Vector3(InputCellBounds.max.x, InputCellBounds.min.y, InputCellBounds.min.z);
            facemin[idx + 2] = new Vector3(InputCellBounds.min.x, InputCellBounds.min.y, InputCellBounds.min.z);
            facemin[idx + 3] = new Vector3(InputCellBounds.min.x, InputCellBounds.max.y, InputCellBounds.min.z);
            facemin[idx + 4] = new Vector3(InputCellBounds.min.x, InputCellBounds.min.y, InputCellBounds.min.z);
            facemin[idx + 5] = new Vector3(InputCellBounds.min.x, InputCellBounds.min.y, InputCellBounds.max.z);
            extents[idx + 0] = new Vector3(0, InputCellBounds.sizeY, InputCellBounds.sizeZ);
            extents[idx + 1] = new Vector3(0, InputCellBounds.sizeY, InputCellBounds.sizeZ);
            extents[idx + 2] = new Vector3(InputCellBounds.sizeX, 0, InputCellBounds.sizeZ);
            extents[idx + 3] = new Vector3(InputCellBounds.sizeX, 0, InputCellBounds.sizeZ);
            extents[idx + 4] = new Vector3(InputCellBounds.sizeX, InputCellBounds.sizeY, 0);
            extents[idx + 5] = new Vector3(InputCellBounds.sizeX, InputCellBounds.sizeY, 0);

            idx = 6;
            facemin[idx + 0] = new Vector3(MeshBounds.min.x, MeshBounds.min.y, MeshBounds.min.z);
            facemin[idx + 1] = new Vector3(MeshBounds.max.x, MeshBounds.min.y, MeshBounds.min.z);
            facemin[idx + 2] = new Vector3(MeshBounds.min.x, MeshBounds.min.y, MeshBounds.min.z);
            facemin[idx + 3] = new Vector3(MeshBounds.min.x, MeshBounds.max.y, MeshBounds.min.z);
            facemin[idx + 4] = new Vector3(MeshBounds.min.x, MeshBounds.min.y, MeshBounds.min.z);
            facemin[idx + 5] = new Vector3(MeshBounds.min.x, MeshBounds.min.y, MeshBounds.max.z);
            extents[idx + 0] = new Vector3(0, MeshBounds.sizeY, MeshBounds.sizeZ);
            extents[idx + 1] = new Vector3(0, MeshBounds.sizeY, MeshBounds.sizeZ);
            extents[idx + 2] = new Vector3(MeshBounds.sizeX, 0, MeshBounds.sizeZ);
            extents[idx + 3] = new Vector3(MeshBounds.sizeX, 0, MeshBounds.sizeZ);
            extents[idx + 4] = new Vector3(MeshBounds.sizeX, MeshBounds.sizeY, 0);
            extents[idx + 5] = new Vector3(MeshBounds.sizeX, MeshBounds.sizeY, 0);

            int numVisibleCellFace = 0;
            int numVisibleMeshFace = 0;

            NativeArray<int> visibleCellFaceIndexs = new NativeArray<int>(6, Allocator.Temp);
            NativeArray<int> visibleMeshFaceIndexs = new NativeArray<int>(6, Allocator.Temp);

            for (int i = 0; i < 6; i++)
            {
                float dotProduct = -Vector3.Dot(meshToCellCenter, normals[i]);

                if (dotProduct > 0.0f)
                {
                    visibleCellFaceIndexs[numVisibleCellFace++] = i;
                }

                if (Vector3.Dot(meshToCellCenter, normals[i]) > 0.0f)
                {
                    visibleMeshFaceIndexs[numVisibleMeshFace++] = i;
                }
            }

            if (numVisibleCellFace == 0 || numVisibleMeshFace == 0)
            {
                BakeResulst[index] = true;
                normals.Dispose();
                facemin.Dispose();
                extents.Dispose();
                return;
            }

            int randomCellFaceIndex, randomMeshFaceIndex;
            int randomVisibileCellFaceIndex, randomVisibileMeshFaceIndex;

            int randomIndex = 0;
            int writeIndex = index * (3 * NumRays);
            Vector3 randomCellVector, randomMeshVector, origin, destination, direction;
            //for (int cellSampleIndex = 0; cellSampleIndex < NumCellSample; ++cellSampleIndex)
            //{
            //    for (int meshSampleIndex = 0; meshSampleIndex < NumMeshSample; ++meshSampleIndex)
            //    {
            for (int i = 0; i < NumRays; ++i)
            {
                //cell 随机向量
                randomCellVector.x = RandomNumbers[randomIndex++];
                randomCellVector.y = RandomNumbers[randomIndex++];
                randomCellVector.z = RandomNumbers[randomIndex++];

                randomMeshVector.x = RandomNumbers[randomIndex++];
                randomMeshVector.y = RandomNumbers[randomIndex++];
                randomMeshVector.z = RandomNumbers[randomIndex++];

                randomCellFaceIndex = Mathf.FloorToInt(RandomNumbers[randomIndex++] * numVisibleCellFace);
                randomMeshFaceIndex = Mathf.FloorToInt(RandomNumbers[randomIndex++] * numVisibleMeshFace);

                randomVisibileCellFaceIndex = visibleCellFaceIndexs[randomCellFaceIndex];
                randomVisibileMeshFaceIndex = visibleMeshFaceIndexs[randomMeshFaceIndex];

                origin = facemin[randomVisibileCellFaceIndex] + Vector3.Scale(extents[randomVisibileCellFaceIndex], randomCellVector);
                destination = facemin[randomVisibileMeshFaceIndex + 6] + Vector3.Scale(extents[randomVisibileMeshFaceIndex + 6], randomMeshVector);
                direction = destination - origin;

                RaySampleResults[writeIndex++] = origin;
                RaySampleResults[writeIndex++] = destination;
                RaySampleResults[writeIndex++] = direction;
            }

            //    }
            //}
            facemin.Dispose();
            extents.Dispose();
            normals.Dispose();
        }
    }

    public class RaycastCommandBaker
    {
        public NativeArray<bool> m_BakeResults;
        public NativeArray<Vector3> m_RaySampleResults;
        static int Origin = 0;
        static int Direction = 2;
        static int Size = 3;


        public static int PVSOccludeeLayer = LayerMask.NameToLayer("PVSOccludee");
        public static int PVSOccluderLayer = LayerMask.NameToLayer("PVSOccluder");
        public void Bake(int numRay, int objCount)
        {
            for (int objIdx = 0; objIdx < objCount; ++objIdx)
            {
                if (m_BakeResults[objIdx] == true)
                    continue;//采样阶段已经确定能看到了，不用发射线了  

                NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(numRay, Allocator.TempJob);
                NativeArray<RaycastCommand> raycastCommands = new NativeArray<RaycastCommand>(numRay, Allocator.TempJob);

                // 该物体在射线数组中的偏移（每条射线保存数据：origin、destination, direction）
                int offsetOfObjectRays = objIdx * (numRay * Size);

                for (int rayIndex = 0; rayIndex < numRay; ++rayIndex)
                {
                    int offsetOfCurrentRay = offsetOfObjectRays + rayIndex * Size;

                    raycastCommands[rayIndex] = new RaycastCommand(
                        m_RaySampleResults[offsetOfCurrentRay + Origin],
                        m_RaySampleResults[offsetOfCurrentRay + Direction].normalized,
                        m_RaySampleResults[offsetOfCurrentRay + Direction].magnitude,
                        1 << PVSOccluderLayer,
                        1);
                }
                JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, numRay, default(JobHandle));
                handle.Complete();

                m_BakeResults[objIdx] = false;

                for (int i = 0; i < numRay; ++i)
                {
                    if (raycastHits[i].collider == null)
                    {
                        //看到了
                        m_BakeResults[objIdx] = true;
                        break;
                    }
                    else if (raycastHits[i].collider.TryGetComponent<PVSCollider>(out var pc))
                    {
                        if (pc.index == objIdx)
                        {
                            m_BakeResults[objIdx] = true;
                            break;
                        }
                    }
                    //else
                    //{
                    //    int index = offsetOfObjectRays + 3 * i;
                    //    m_RaySampleResults[index + 1] = raycastHits[i].point;
                    //    Debug.Log($"objIdx : {objIdx}, hit colldier = {raycastHits[i].collider.name}");
                    //}
                }
                raycastHits.Dispose();
                raycastCommands.Dispose();
            }

        }
    }
}
#endif
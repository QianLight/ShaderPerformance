using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    struct VoxelRange
    {
        public ushort min;
        public ushort max;
    }
    class UnitVoxel
    {
        public float unitOffsetX;
        public float unitOffsetZ;
        public List<int> voxel = new List<int> ();
        public List<VoxelRange> voxelRange = new List<VoxelRange> ();
    }
    class GridVoxel
    {
        public int min = 1000;
        public int max = 0;
        public int chunkID = 0;
        public int subchunkID = 0;
        public int localGridIndex = 0;
        // public int gridOffsetX = 0;
        // public int gridOffsetZ = 0;
        public float gridOffsetX;
        public float gridOffsetZ;
        public List<UnitVoxel> unitVoxel = new List<UnitVoxel> ();
        public UnitVoxel GetUnitVoxel (int index, int unitX, int unitZ, int xCount, float unitSize)
        {
            if (unitVoxel.Count == 0)
            {
                int count = xCount * xCount;
                for (int i = 0; i < count; ++i)
                {
                    unitVoxel.Add (null);
                }
            }
            UnitVoxel unit = unitVoxel[index];
            if (unit == null)
            {
                unit = new UnitVoxel ();
                unit.unitOffsetX = unitSize * unitX + gridOffsetX;
                unit.unitOffsetZ = unitSize * unitZ + gridOffsetZ;
                unitVoxel[index] = unit;
            }
            return unit;
        }
    }
    class SceneChunkVoxel
    {
        public int maxCount = 0;
        public int x;
        public int z;
        public float chunkOffsetX;
        public float chunkOffsetZ;
        public List<TerrainDrawInfo> terrainCollision = new List<TerrainDrawInfo> ();
    }

    class MeshVertexInfo
    {
        public List<Vector3> cloud = new List<Vector3> ();
    }

    struct VoxelInfo
    {
        public Vector4Int index;
        public Vector4Int offset;
    }
    class CollisionObjInfo
    {
        public string name = "";
        public string error;
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] triangles;
        public Matrix4x4 matrix;
        public List<VoxelInfo> voxelIndex = new List<VoxelInfo> ();
    }
    struct TerrainVertexInfo
    {
        public int index;
        public float h;
    }
    class TerrainDrawInfo
    {
        public List<TerrainVertexInfo> vList = new List<TerrainVertexInfo> ();
        public ComputeBuffer vertexBuffer = null;
        public MaterialPropertyBlock mpb = null;
        public int count = 0;

        public void Reset ()
        {
            vList.Clear ();
            count = 0;
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose ();
                vertexBuffer = null;
            }
        }
    }

    class TerrainObjInfo
    {
        public string name = "";
        public Mesh mesh;
        public Vector3[] vertices;
        public int[] triangles;
        public Matrix4x4 matrix;
        public int chunkID = -1;
        public Dictionary<int, TerrainDrawInfo> terrainVertex = new Dictionary<int, TerrainDrawInfo> ();

    }

    struct MeshInfoCache
    {
        public Vector3[] vertices;
        public int[] triangles;
    }

    class MeshCloudJob : IJobProcessor
    {
        public CollisionContext cc;
        public void Prepare (int jobID)
        {
            CollisionObjInfo coi = cc.collisionObjects[jobID];
            coi.name = coi.mesh.name;
            MeshInfoCache mic;
            if (!cc.meshInfoCache.TryGetValue (coi.mesh, out mic))
            {
                mic.vertices = coi.mesh.vertices;
                mic.triangles = coi.mesh.triangles;
                cc.meshInfoCache[coi.mesh] = mic;
            }
            var v = mic.vertices;
            coi.vertices = new Vector3[v.Length];
            EngineUtility.MemCpy (v, coi.vertices);
            var i = mic.triangles;
            coi.triangles = new int[i.Length];
            EngineUtility.MemCpy (i, coi.triangles);
            if (coi.vertices == null || coi.triangles == null)
            {
                Debug.LogError ("vertex null:" + coi.mesh.name);
            }
        }

        public void Execute (int jobID)
        {
            CollisionObjInfo coi = cc.collisionObjects[jobID];
            if (coi.vertices != null && coi.triangles != null)
            {
                Vector3 chunkPos = new Vector3 (0, 0, 0);
                Vector3 gridPos = new Vector3 (0, 0, 0);
                // MeshVertexInfo mvi = new MeshVertexInfo();
                // cc.meshCloud.Add(mvi);
                for (int i = 0; i < coi.triangles.Length; i += 3)
                {
                    Vector3 p0 = coi.matrix.MultiplyPoint (coi.vertices[coi.triangles[i]]);
                    Vector3 p1 = coi.matrix.MultiplyPoint (coi.vertices[coi.triangles[i + 1]]);
                    Vector3 p2 = coi.matrix.MultiplyPoint (coi.vertices[coi.triangles[i + 2]]);
                    float dist01 = Vector3.Distance (p0, p1);
                    float dist02 = Vector3.Distance (p0, p2);
                    int count01 = (int) (dist01 * cc.stepSize) + 1;
                    int count02 = (int) (dist02 * cc.stepSize) + 1;
                    float percent01 = 1.0f / count01;
                    float percent02 = 1.0f / count02;
                    for (int step01 = 0; step01 <= count01; ++step01)
                    {
                        float p01 = percent01 * step01;
                        Vector3 v01 = Vector3.Lerp (p0, p1, p01);
                        for (int step02 = 0; step02 <= count02; ++step02)
                        {
                            float p02 = percent02 * step02;
                            Vector3 v = Vector3.Lerp (v01, p2, p02);
                            int x;
                            int z;
                            int chunkIndex = SceneQuadTree.FindChunkIndex (v, EngineContext.ChunkSize, cc.xCount, cc.zCount, out x, out z);

                            Vector2 chunkCorner = new Vector4 (x * EngineContext.ChunkSize, z * EngineContext.ChunkSize);
                            chunkPos.x = v.x - chunkCorner.x;
                            chunkPos.z = v.z - chunkCorner.y;
                            int gridX;
                            int gridZ;
                            int gridIndex = SceneQuadTree.FindChunkIndex (chunkPos,
                                EngineContext.terrainGridSize, EngineContext.terrainGridCount, EngineContext.terrainGridCount,
                                out gridX, out gridZ);

                            Vector2 gridCorner = new Vector4 (gridX * EngineContext.terrainGridSize, gridZ * EngineContext.terrainGridSize);
                            gridPos.x = chunkPos.x - gridCorner.x;
                            gridPos.z = chunkPos.z - gridCorner.y;
                            int unitIndex = SceneQuadTree.FindChunkIndex (gridPos,
                                cc.unitSize, cc.unitCount, cc.unitCount, out x, out z);
                            int h = (int) (v.y / cc.unitSize);
                            coi.voxelIndex.Add (
                                new VoxelInfo ()
                                {
                                    index = new Vector4Int (chunkIndex, gridIndex, unitIndex, h),
                                        offset = new Vector4Int (gridX, gridZ, x, z),
                                });
                            // mvi.cloud.Add(v);
                        }
                    }
                }
            }
            else
            {
                coi.error = "vertex null " + coi.name;
            }
        }
        public void Collection (int jobID)
        {
            Vector3 center = Vector3.zero;
            CollisionObjInfo coi = cc.collisionObjects[jobID];
            if (!string.IsNullOrEmpty (coi.error))
            {
                Debug.LogError (coi.error);
            }
            else
            {
                float halfUnit = cc.unitSize * 0.5f;
                for (int i = 0; i < coi.voxelIndex.Count; ++i)
                {
                    VoxelInfo index = coi.voxelIndex[i];
                    var chunk = cc.chunkVoxel[index.index.x];
                    var grid = cc.GetGridVoxel (index.index.x, index.index.y, index.offset.x, index.offset.y, chunk);
                    var unit = grid.GetUnitVoxel (index.index.z, index.offset.z, index.offset.w, cc.unitCount, cc.unitSize);

                    int h = index.index.w;
                    if (!unit.voxel.Contains (h))
                    {
                        unit.voxel.Add (h);
                        if (h < grid.min)
                        {
                            grid.min = h;
                        }
                        if (h > grid.max)
                        {
                            grid.max = h;
                        }
                        center.x = unit.unitOffsetX + halfUnit;
                        center.z = unit.unitOffsetZ + halfUnit;
                        center.y = h * cc.unitSize + halfUnit;
                        cc.clouds.Add (center);
                    }
                }
            }

        }

        public int GetJobCount ()
        {
            return cc.collisionObjects.Count;
        }
    }
    class MeshCloudCompressJob : IJobProcessor
    {
        public CollisionContext cc;
        public void Prepare (int jobID)
        {
            var grid = cc.gridVoxel[jobID];
            for (int i = 0; i < grid.unitVoxel.Count; ++i)
            {
                var unit = grid.unitVoxel[i];
                if (unit != null && unit.voxel.Count > 1)
                {
                    unit.voxel.Sort ();
                }
            }
        }

        public void Execute (int jobID)
        {
            var grid = cc.gridVoxel[jobID];
            for (int i = 0; i < grid.unitVoxel.Count; ++i)
            {
                var unit = grid.unitVoxel[i];
                if (unit != null && unit.voxel.Count > 1)
                {
                    VoxelRange vr;
                    vr.min = (ushort) unit.voxel[0];
                    vr.max = (ushort) unit.voxel[0];
                    for (int j = 1; j < unit.voxel.Count; ++j)
                    {
                        int h = unit.voxel[j];
                        if ((h - vr.max) < cc.playerh)
                        {
                            vr.max = (ushort) h;
                        }
                        else
                        {
                            unit.voxelRange.Add (vr);
                            vr.min = (ushort) h;
                            vr.max = (ushort) h;
                        }
                    }
                    unit.voxelRange.Add (vr);
                }
            }
        }

        public void Collection (int jobID)
        {
            var grid = cc.gridVoxel[jobID];
            var chunk = cc.chunkVoxel[grid.chunkID];
            for (int i = 0; i < grid.unitVoxel.Count; ++i)
            {
                var unit = grid.unitVoxel[i];
                if (unit != null && unit.voxelRange.Count > 0)
                {
                    if (unit.voxelRange.Count > chunk.maxCount)
                        chunk.maxCount = unit.voxelRange.Count;
                }
            }
        }
        public int GetJobCount ()
        {
            return cc.gridVoxel.Count;
        }
    }

    class TerrainVertexJob : IJobProcessor
    {
        public CollisionContext cc;
        public void Prepare (int jobID)
        {
            var terrainObj = cc.terrainObjects[jobID];
            if (terrainObj.mesh != null)
            {
                var v = terrainObj.mesh.vertices;
                terrainObj.vertices = new Vector3[v.Length];
                EngineUtility.MemCpy (v, terrainObj.vertices);
                if (terrainObj.chunkID < 0)
                {
                    var i = terrainObj.mesh.triangles;
                    terrainObj.triangles = new int[i.Length];
                    EngineUtility.MemCpy (i, terrainObj.triangles);
                }
            }
        }

        public void Execute (int jobID)
        {
            var terrainObj = cc.terrainObjects[jobID];
            if (terrainObj.chunkID >= 0)
            {
                TerrainDrawInfo tdi = new TerrainDrawInfo ();
                for (int i = 0; i < terrainObj.vertices.Length; ++i)
                {
                    var v = terrainObj.vertices[i];
                    tdi.vList.Add (new TerrainVertexInfo ()
                    {
                        index = i,
                            h = v.y
                    });
                }
                terrainObj.terrainVertex.Add (terrainObj.chunkID, tdi);
            }
            else
            {
                for (int i = 0; i < terrainObj.triangles.Length; i += 3)
                {
                    Vector3 p0 = terrainObj.matrix.MultiplyPoint (terrainObj.vertices[terrainObj.triangles[i]]);
                    Vector3 p1 = terrainObj.matrix.MultiplyPoint (terrainObj.vertices[terrainObj.triangles[i + 1]]);
                    Vector3 p2 = terrainObj.matrix.MultiplyPoint (terrainObj.vertices[terrainObj.triangles[i + 2]]);

                    Vector3 v0 = p0;
                    Vector3 v1 = p1;
                    Vector3 v2 = p2;

                    if (p0.x < p1.x)
                    {
                        if (p0.x < p2.x)
                        {
                            v0 = p0; //min
                            if (p1.x < p2.x)
                            {
                                v1 = p2; //max
                                v2 = p1;
                            }
                            else
                            {
                                v1 = p1; //max
                                v2 = p2;
                            }
                        }
                        else
                        {
                            v0 = p2; //min
                            v1 = p1; //max
                            v2 = p0;
                        }
                    }
                    else
                    {
                        if (p1.x > p2.x)
                        {
                            v0 = p2; //min
                            v1 = p0; //max
                            v2 = p1;
                        }
                        else
                        {
                            v0 = p1; //min
                            if (p0.x < p2.x)
                            {
                                v1 = p2; //max
                                v2 = p0;
                            }
                            else
                            {
                                v1 = p0; //max
                                v2 = p2;
                            }
                        }
                    }

                    int x0 = (int) (v0.x / EngineContext.terrainGridSize);
                    if (x0 * EngineContext.terrainGridSize < v0.x)
                    {
                        x0 += 1;
                    }
                    int x1 = (int) (v1.x / EngineContext.terrainGridSize);
                    if (x0 < x1)
                    {
                        float delta01 = v1.x - v0.x;
                        for (int x = x0; x <= x1; ++x)
                        {
                            float p01 = (x * EngineContext.terrainGridSize - v0.x) / delta01;
                            Vector3 v01 = Vector3.Lerp (v0, v1, p01);

                            Vector3 zmin = v01;
                            Vector3 zmax = v2;
                            if (v01.z > v2.z)
                            {
                                zmin = v2;
                                zmax = v01;
                            }
                            else
                            {
                                zmin = v01;
                                zmax = v2;
                            }
                            int z0 = (int) (zmin.z / EngineContext.terrainGridSize);
                            if (z0 * EngineContext.terrainGridSize < zmin.z)
                            {
                                z0 += 1;
                            }
                            int z1 = (int) (zmax.z / EngineContext.terrainGridSize);
                            if (z0 < z1)
                            {
                                float delta02 = zmax.z - zmin.z;
                                for (int z = z0; z <= z1; ++z)
                                {
                                    float p02 = (z * EngineContext.terrainGridSize - zmin.z) / delta02;
                                    Vector3 v02 = Vector3.Lerp (zmin, zmax, p02);
                                    int xx;
                                    int zz;
                                    int chunkIndex = SceneQuadTree.FindChunkIndex (v02, EngineContext.ChunkSize, cc.xCount, cc.zCount, out xx, out zz);
                                    TerrainDrawInfo tdi;
                                    if (!terrainObj.terrainVertex.TryGetValue (chunkIndex, out tdi))
                                    {
                                        tdi = new TerrainDrawInfo ();
                                        terrainObj.terrainVertex.Add (chunkIndex, tdi);
                                    }
                                    tdi.vList.Add (new TerrainVertexInfo ()
                                    {
                                        index = x - xx * EngineContext.terrainGridCount +
                                            (z - zz * EngineContext.terrainGridCount) * EngineContext.terrainGridCount,
                                            h = v02.y
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Collection (int jobID)
        {
            var terrainObj = cc.terrainObjects[jobID];
            if (terrainObj.chunkID >= 0)
            {
                var it = terrainObj.terrainVertex.GetEnumerator ();
                if (it.MoveNext ())
                {
                    cc.vertexShare.Clear ();

                    var tdi = it.Current.Value;
                    tdi.count = EngineContext.terrainGridCount * EngineContext.terrainGridCount * 2;
                    tdi.vertexBuffer = new ComputeBuffer (tdi.count, 9 * sizeof (float));
                    for (int z = 0; z < EngineContext.terrainGridCount; ++z)
                    {
                        for (int x = 0; x < EngineContext.terrainGridCount; ++x)
                        {
                            int v0 = x + z * (EngineContext.terrainGridCount + 1);
                            int v1 = v0 + 1;
                            int v2 = x + (z + 1) * (EngineContext.terrainGridCount + 1);
                            int v3 = v2 + 1;
                            float xoffset = x * EngineContext.terrainGridSize;
                            float zoffset = z * EngineContext.terrainGridSize;

                            cc.vertexShare.Add (new Vector3 (xoffset, tdi.vList[v0].h, zoffset));

                            cc.vertexShare.Add (new Vector3 (xoffset + EngineContext.terrainGridSize, tdi.vList[v3].h, zoffset + EngineContext.terrainGridSize));
                            cc.vertexShare.Add (new Vector3 (xoffset + EngineContext.terrainGridSize, tdi.vList[v1].h, zoffset));

                            cc.vertexShare.Add (new Vector3 (xoffset, tdi.vList[v0].h, zoffset));

                            cc.vertexShare.Add (new Vector3 (xoffset, tdi.vList[v2].h, zoffset + EngineContext.terrainGridSize));
                            cc.vertexShare.Add (new Vector3 (xoffset + EngineContext.terrainGridSize, tdi.vList[v3].h, zoffset + EngineContext.terrainGridSize));
                        }
                    }
                    tdi.vertexBuffer.SetData (cc.vertexShare);
                    tdi.mpb = new MaterialPropertyBlock ();
                    tdi.mpb.SetBuffer ("points", tdi.vertexBuffer);
                    var scv = cc.chunkVoxel[terrainObj.chunkID];
                    tdi.mpb.SetVector ("offset", new Vector2 (scv.chunkOffsetX, scv.chunkOffsetZ));
                }
            }
            else
            {
                for (int i = 0; i < terrainObj.terrainVertex.Count; ++i)
                {
                    // var tv = terrainObj.terrainVertex[i];
                    // var scv = cc.chunkVoxel[tv.chunkID];
                    // if (scv.terrainGridH == null)
                    // {
                    //     scv.terrainGridH = new Dictionary<int, float> ();
                    // }

                }
            }
        }

        public int GetJobCount ()
        {
            return cc.terrainObjects.Count;
        }
    }

    class CollisionContext
    {
        public float stepSize = 10; //0.1f;
        public float unitSize = 0.25f; //2/16
        public int unitCount = 8; //2/unitSize
        public int playerh = 8;
        public int subChunkCount = 4; //4x4
        public int xCount = 1;
        public int zCount = 1;
        public List<SceneChunkVoxel> chunkVoxel = new List<SceneChunkVoxel> ();
        public Dictionary<Mesh, MeshInfoCache> meshInfoCache = new Dictionary<Mesh, MeshInfoCache> ();
        public List<CollisionObjInfo> collisionObjects = new List<CollisionObjInfo> ();
        public List<GridVoxel> gridVoxel = new List<GridVoxel> ();
        public List<Vector3> clouds = new List<Vector3> ();
        public Dictionary<int, int> gridIndexMap = new Dictionary<int, int> ();
        public List<TerrainObjInfo> terrainObjects = new List<TerrainObjInfo> ();
        public List<Vector3> vertexShare = new List<Vector3> ();
        public List<MeshVertexInfo> meshCloud = new List<MeshVertexInfo> ();
        public MeshCloudJob meshJob = new MeshCloudJob ();
        public MeshCloudCompressJob meshCompressJob = new MeshCloudCompressJob ();
        public TerrainVertexJob terrainVertexJob = new TerrainVertexJob ();
        public ThreadManager threadManager = null;
        public bool gizmoDraw = false;
        public MaterialPropertyBlock mpb = null;
        public ComputeBuffer pointsBuffer = null;
        public bool threadRun = false;
        public void Init (int widthCount, int heightCount)
        {
            if (threadManager == null)
            {
                threadManager = new ThreadManager ();
            }
            playerh = (int) (4 / unitSize);
            meshJob.cc = this;
            meshCompressJob.cc = this;
            terrainVertexJob.cc = this;
            if (mpb == null)
            {
                mpb = new MaterialPropertyBlock ();
            }
            chunkVoxel.Clear ();
            for (int z = 0; z < heightCount; ++z)
            {
                for (int x = 0; x < widthCount; ++x)
                {
                    var scv = new SceneChunkVoxel ();
                    scv.x = x;
                    scv.z = z;
                    scv.chunkOffsetX = EngineContext.ChunkSize * x;
                    scv.chunkOffsetZ = EngineContext.ChunkSize * z;
                    chunkVoxel.Add (scv);
                }
            }
            xCount = widthCount;
            zCount = heightCount;
        }

        public void Reset ()
        {
            threadManager.ClearJob ();
            collisionObjects.Clear ();
            meshInfoCache.Clear ();
            meshCloud.Clear ();
            clouds.Clear ();
            gridVoxel.Clear ();
            gridIndexMap.Clear ();
            for (int i = 0; i < terrainObjects.Count; ++i)
            {
                var to = terrainObjects[i];
                var it = to.terrainVertex.GetEnumerator ();
                while (it.MoveNext ())
                {
                    var tdi = it.Current.Value;
                    tdi.Reset ();
                }

            }
            terrainObjects.Clear ();
            threadRun = false;
        }
        public void ResetJobs()
        {

        }
        public void Release ()
        {
            if (threadManager != null)
            {
                threadManager.ClearJob ();
            }
            if (pointsBuffer != null)
            {
                pointsBuffer.Dispose ();
                pointsBuffer = null;
            }
        }
        public GridVoxel GetGridVoxel (int chunkIndex, int index, int gridX, int gridZ, SceneChunkVoxel scv)
        {
            int gridIndex = EngineContext.terrainGridCount * EngineContext.terrainGridCount * chunkIndex + index;
            GridVoxel grid = null;
            int i;
            if (gridIndexMap.TryGetValue (gridIndex, out i))
            {
                grid = gridVoxel[i];
            }
            else
            {
                gridIndexMap.Add (gridIndex, gridVoxel.Count);
                grid = new GridVoxel ();
                grid.gridOffsetX = EngineContext.terrainGridSize * gridX + scv.chunkOffsetX;
                grid.gridOffsetZ = EngineContext.terrainGridSize * gridZ + scv.chunkOffsetZ;
                grid.chunkID = chunkIndex;

                int gridXBlock = gridX / EngineContext.chunkGridCount + scv.x * subChunkCount;
                int gridZBlock = gridZ / EngineContext.chunkGridCount + scv.z * subChunkCount;
                grid.subchunkID = gridXBlock + gridZBlock * xCount * subChunkCount;
                int localGridX = gridX % subChunkCount;
                int localGridZ = gridZ % subChunkCount;
                int gridLineCount = EngineContext.terrainGridCount / subChunkCount;
                grid.localGridIndex = localGridX + localGridZ * gridLineCount;
                gridVoxel.Add (grid);
            }
            return grid;
        }

        public void UpdateJobs()
        {
            // int jobCount = collisionContext.collisionObjects.Count + collisionContext.terrainObjects.Count;
            // if (!collisionContext.gizmoDraw &&
            //     collisionContext.threadManager != null &&
            //     jobCount > 0 && collisionContext.threadRun)
            // {
            //     int count = collisionContext.threadManager.CollectionJobs ();
            //     if (count > 0)
            //     {
            //         EditorUtility.DisplayProgressBar ("calc collision",
            //             string.Format ("{0}/{1}", count, jobCount), (float) count / jobCount);
            //     }
            //     else
            //     {
            //         collisionContext.threadManager.ClearJob ();
            //         //var cb = GetPreviewCB (ref cbContext.collisionVexelCB, true);
            //         if (collisionContext.pointsBuffer != null)
            //         {
            //             collisionContext.pointsBuffer.Dispose ();
            //             collisionContext.pointsBuffer = null;
            //         }
            //         // if (collisionContext.clouds.Count > 0)
            //         // {
            //         //     collisionContext.pointsBuffer = new ComputeBuffer (collisionContext.clouds.Count, 3 * sizeof (float));
            //         //     collisionContext.pointsBuffer.SetData (collisionContext.clouds);
            //         //     collisionContext.mpb.SetBuffer ("points", collisionContext.pointsBuffer);
            //         //     cb.DrawProcedural (Matrix4x4.identity,
            //         //         AssetsConfig.GlobalAssetsConfig.PreviewMeshCloud,
            //         //         0, MeshTopology.Points, collisionContext.clouds.Count, 1, collisionContext.mpb);
            //         // }
            //         // for (int i = 0; i < collisionContext.terrainObjects.Count; ++i)
            //         // {
            //         //     var to = collisionContext.terrainObjects[i];
            //         //     var it = to.terrainVertex.GetEnumerator ();
            //         //     while (it.MoveNext ())
            //         //     {
            //         //         var tdi = it.Current.Value;
            //         //         cb.DrawProcedural (Matrix4x4.identity,
            //         //             AssetsConfig.GlobalAssetsConfig.PreviewTerrainQuad,
            //         //             0, MeshTopology.Triangles, 3, tdi.count, tdi.mpb);
            //         //         var scv = collisionContext.chunkVoxel[it.Current.Key];
            //         //         scv.terrainCollision.Add (tdi);
            //         //     }
            //         // }
            //         // PreviewCollisionVoxel ();
            //         collisionContext.threadRun = false;

            //         //output data
            //         collisionContext.gridVoxel.Sort ((x, y) =>
            //         {
            //             int delta = x.subchunkID - y.subchunkID;
            //             if (delta == 0)
            //             {
            //                 return x.localGridIndex - y.localGridIndex;
            //             }
            //             return delta;
            //         });

            //         int[] dataSize = new int[collisionContext.xCount * collisionContext.zCount * collisionContext.subChunkCount * collisionContext.subChunkCount * 2];
            //         for (int i = 0; i < dataSize.Length; ++i)
            //         {
            //             dataSize[i] = 0;
            //         }
            //         int gridXCount = EngineContext.terrainGridCount / collisionContext.subChunkCount;
            //         int[] gridOffset = new int[gridXCount * gridXCount];
            //         List<ushort> subChunkData = new List<ushort> ();
            //         int lastSubChunkId = -1;
            //         int startGrid = -1;
            //         string targetSceneDir = string.Format ("{0}/EditorSceneRes/Scene/{1}", AssetsConfig.GlobalAssetsConfig.ResourcePath, sceneContext.name);
            //         EditorCommon.CreateDir (targetSceneDir);
            //         using (FileStream fs = new FileStream (string.Format ("{0}/EditorSceneRes/Scene/{1}/{1}_Collision.bytes",
            //             AssetsConfig.GlobalAssetsConfig.ResourcePath, sceneContext.name), FileMode.Create))
            //         {
            //             BinaryWriter bw = new BinaryWriter (fs);
            //             for (int i = 0; i < dataSize.Length; ++i)
            //             {
            //                 bw.Write (dataSize[i]);
            //             }
            //             for (int i = 0; i < collisionContext.gridVoxel.Count; ++i)
            //             {
            //                 var grid = collisionContext.gridVoxel[i];
            //                 if (grid.subchunkID != lastSubChunkId)
            //                 {
            //                     if (lastSubChunkId >= 0)
            //                     {
            //                         int startOffset = (int) bw.BaseStream.Position;
            //                         EditorUtility.DisplayProgressBar ("save collision",
            //                             string.Format ("subGridOffset:{0}", lastSubChunkId), (float) i / collisionContext.gridVoxel.Count);

            //                         dataSize[lastSubChunkId * 2] = startOffset;
            //                         for (int j = 0; j < gridOffset.Length; ++j)
            //                         {
            //                             gridOffset[j] = -1;
            //                         }
            //                         subChunkData.Clear ();
            //                         for (int j = startGrid; j < i; ++j)
            //                         {
            //                             var chunkGrid = collisionContext.gridVoxel[j];
            //                             ushort voxelCount = 0;
            //                             for (int k = 0; k < chunkGrid.unitVoxel.Count; ++k)
            //                             {
            //                                 var voxel = chunkGrid.unitVoxel[k];
            //                                 if (voxel != null)
            //                                 {
            //                                     voxelCount++;
            //                                 }
            //                             }
            //                             if (voxelCount > 0)
            //                             {
            //                                 if (chunkGrid.localGridIndex < 0 || chunkGrid.localGridIndex > gridOffset.Length)
            //                                 {
            //                                     Debug.LogErrorFormat ("grid out of index:{0}", chunkGrid.localGridIndex);
            //                                 }
            //                                 gridOffset[chunkGrid.localGridIndex] = subChunkData.Count;
            //                                 subChunkData.Add (voxelCount);
            //                                 int voxelIndexOffset = subChunkData.Count;
            //                                 for (ushort k = 0; k < chunkGrid.unitVoxel.Count; ++k)
            //                                 {
            //                                     var voxel = chunkGrid.unitVoxel[k];
            //                                     if (voxel != null)
            //                                     {
            //                                         subChunkData.Add (k);
            //                                     }
            //                                 }
            //                                 int voxelDataIndexOffset = subChunkData.Count;
            //                                 for (int k = 0; k < voxelCount; ++k)
            //                                 {
            //                                     ushort blockCount = 0;
            //                                     subChunkData.Add (blockCount);
            //                                 }
            //                                 ushort voxelDataOffsetStart = 0;
            //                                 for (int k = 0, voxelOffset = 0; k < chunkGrid.unitVoxel.Count; ++k)
            //                                 {
            //                                     var voxel = chunkGrid.unitVoxel[k];
            //                                     if (voxel != null)
            //                                     {
            //                                         ushort blockCount = (ushort) voxel.voxelRange.Count;
            //                                         subChunkData[voxelOffset + voxelDataIndexOffset] = voxelDataOffsetStart;
            //                                         subChunkData.Add (blockCount);
            //                                         for (int ii = 0; ii < blockCount; ++ii)
            //                                         {
            //                                             var range = voxel.voxelRange[ii];
            //                                             subChunkData.Add (range.min);
            //                                             subChunkData.Add (range.max);
            //                                         }
            //                                         voxelOffset++;
            //                                         int delta = 1 + blockCount * 2;
            //                                         if (delta + voxelDataOffsetStart > ushort.MaxValue)
            //                                         {
            //                                             Debug.LogErrorFormat ("out of voxel range:{0}!", subChunkData.Count);
            //                                             break;
            //                                         }
            //                                         else
            //                                         {
            //                                             voxelDataOffsetStart += (ushort) delta;
            //                                         }
            //                                     }

            //                                 }
            //                             }

            //                         }
            //                         for (int j = 0; j < gridOffset.Length; ++j)
            //                         {
            //                             bw.Write (gridOffset[j]);
            //                         }
            //                         for (int j = 0; j < subChunkData.Count; ++j)
            //                         {
            //                             bw.Write (subChunkData[j]);
            //                         }
            //                         dataSize[lastSubChunkId * 2 + 1] = (int) bw.BaseStream.Position - startOffset;
            //                     }
            //                     lastSubChunkId = grid.subchunkID;
            //                     startGrid = i;
            //                 }
            //                 else
            //                 {

            //                 }

            //             }
            //             bw.Seek (0, SeekOrigin.Begin);
            //             for (int i = 0; i < dataSize.Length; ++i)
            //             {
            //                 bw.Write (dataSize[i]);
            //             }
            //         }

            //         EditorUtility.ClearProgressBar ();
            //         EditorUtility.DisplayDialog ("Finish", "Calc Finish!", "OK");
            //     }

            // }
        }
    }
}
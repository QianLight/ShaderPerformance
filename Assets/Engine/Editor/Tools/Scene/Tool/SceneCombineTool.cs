using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    enum OpCombineType
    {
        OpNone,
        GroupingObjects,
        MergeSmallObjects,
        CombineObjects
    }

    public partial class SceneEditTool : CommonToolTemplate
    {
        class MeshCombineContext
        {
            public CommonContext conmmonContext;
            public EditorCommon.EnumTransform enumPrefab;
            public MeshCombine meshCombine;
            public Mesh shpereMesh;
        }

        MeshCombineContext meshCombineContext;
        //private OpCombineType combineOp = OpCombineType.OpNone;
        //private void PostInitCombine ()
        //{
        //    meshCombineContext = new MeshCombineContext ();
        //    meshCombineContext.conmmonContext = commonContext;
        //    meshCombineContext.enumPrefab = EnumGroupObject;
        //}
        #region group objects
        //void EnumGroupObject (Transform trans, object param)
        //{
        //    MeshCombineContext mcc = param as MeshCombineContext;
        //    MeshRenderObject mro = trans.GetComponent<MeshRenderObject> ();
        //    if (mro != null)
        //    {
        //        Renderer r = mro.GetRenderer ();
        //        Mesh m = mro.GetMesh ();

        //        if (r != null && m != null)
        //        {
        //            Material mat = r.sharedMaterial;
        //            if (mat != null)
        //            {
        //                Shader s = mat.shader;
        //                // if (s.name.Contains ("Uber"))
        //                // {
        //                //     int chunkID = mro.chunkID;
        //                //     int mergeChunkID = chunkID >= 0 ? chunkID : mcc.meshCombine.mergeObjects.Length - 1;
        //                //     MergeObject mergeObject = mcc.meshCombine.mergeObjects[mergeChunkID];

        //                //     MaterialGroup mg = mergeObject.matGroups.Find ((xx) => { return xx.material == mat; });
        //                //     if (mg == null)
        //                //     {
        //                //         GameObject go = new GameObject (mat.name);
        //                //         Transform t = go.transform;
        //                //         t.parent = mergeObject.transform;
        //                //         MeshFilter mf = go.AddComponent<MeshFilter> ();
        //                //         MeshRenderer mr = go.AddComponent<MeshRenderer> ();
        //                //         mf.sharedMesh = mcc.shpereMesh;
        //                //         mr.sharedMaterial = mat;

        //                //         mg = go.AddComponent<MaterialGroup> ();
        //                //         mg.material = mat;
        //                //         mergeObject.matGroups.Add (mg);
        //                //     }
        //                //     Vector3 pos = mro.transform.position;
        //                //     if (chunkID >= 0)
        //                //     {
        //                //         // int x;
        //                //         // int z;
        //                //         // Vector2 chunkCorner = new Vector4 (mro.x * chunkWidth, mro.z * chunkHeight);
        //                //         // SceneQuadTree.FindChunkIndex (new Vector3 (pos.x - chunkCorner.x, 0, pos.z - chunkCorner.y),
        //                //         //     chunkWidth * 0.25f, 4, 4, out x, out z);
        //                //         // mro.localBlockID = x + z * 4;
        //                //     }
        //                //     else
        //                //     {
        //                //         // float sceneCenterX = widthCount * chunkWidth * 0.5f;
        //                //         // float sceneCenterZ = widthCount * chunkWidth * 0.5f;
        //                //         // if (pos.x > sceneCenterX)
        //                //         // {
        //                //         //     if (pos.z > sceneCenterZ)
        //                //         //     {
        //                //         //         mro.localBlockID = 0;
        //                //         //     }
        //                //         //     else
        //                //         //     {
        //                //         //         mro.localBlockID = 3;
        //                //         //     }
        //                //         // }
        //                //         // else
        //                //         // {
        //                //         //     if (pos.z > sceneCenterZ)
        //                //         //     {
        //                //         //         mro.localBlockID = 1;
        //                //         //     }
        //                //         //     else
        //                //         //     {
        //                //         //         mro.localBlockID = 2;
        //                //         //     }
        //                //         // }
        //                //     }

        //                //     // MergedMeshRef mmr = mg.mergeMesh.Find ((xx) => { return xx.blockID == mro.localBlockID; });
        //                //     // if (mmr == null)
        //                //     // {
        //                //     //     mmr = new MergedMeshRef ()
        //                //     //     {
        //                //     //     blockID = mro.localBlockID,
        //                //     //     };
        //                //     //     mg.mergeMesh.Add (mmr);
        //                //     //     mmr.mergeAABB.Init (ref mro.sceneAABB);
        //                //     // }
        //                //     // else
        //                //     // {
        //                //     //     mmr.mergeAABB.Encapsulate (ref mro.sceneAABB);
        //                //     // }
        //                //     // mmr.mergeObjets.Add (mro);
        //                //     // mg.meshCount++;
        //                // }
        //            }
        //        }

        //    }
        //    EditorCommon.EnumChildObject (trans, param, mcc.enumPrefab);
        //}

        //void GroupingObjects ()
        //{
        //    BeginEdit (true);
        //    meshCombineContext.shpereMesh = AssetsConfig.instance.sphereMesh;
        //    Transform meshCombine = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshCombine].transform;
        //    MeshCombine mc = meshCombine.GetComponent<MeshCombine> ();
        //    if (mc == null)
        //    {
        //        mc = meshCombine.gameObject.AddComponent<MeshCombine> ();
        //    }

        //    int mergeGroupCount = widthCount * heightCount + 1;
        //    if (mc.mergeObjects == null || mc.mergeObjects.Length != mergeGroupCount)
        //    {
        //        mc.mergeObjects = new MergeObject[mergeGroupCount];
        //    }
        //    meshCombineContext.meshCombine = mc;
        //    EditorCommon.DestroyChildObjects (meshCombine.gameObject);
        //    for (int i = 0; i < mergeGroupCount; ++i)
        //    {
        //        var mo = mc.mergeObjects[i];
        //        if (mo == null)
        //        {
        //            string str = i < mergeGroupCount - 1 ? i.ToString () : "Global";
        //            GameObject go = new GameObject ("MergeChunk_" + str);
        //            mo = go.AddComponent<MergeObject> ();
        //            mc.mergeObjects[i] = mo;
        //        }

        //        Transform t = mo.transform;
        //        t.parent = meshCombine;
        //        if (i < mergeGroupCount - 1)
        //        {
        //            int x = i % widthCount;
        //            int z = i / widthCount;
        //            t.position = new Vector3 ((x + 0.5f) * chunkWidth, -100, (z + 0.5f) * chunkHeight);
        //        }
        //        else
        //        {
        //            t.position = new Vector3 (widthCount * chunkWidth * 0.5f, -200, heightCount * chunkHeight * 0.5f);
        //        }

        //    }
        //    Transform staticPrefab = commonContext.editorSceneGos[(int) EditorSceneObjectType.StaticPrefab].transform;
        //    EditorCommon.EnumChildObject (staticPrefab, meshCombineContext, (trans, param) =>
        //    {
        //        MeshCombineContext mcc = param as MeshCombineContext;
        //        mcc.enumPrefab (trans, mcc);
        //    });
        //    Transform prefab = commonContext.editorSceneGos[(int) EditorSceneObjectType.Prefab].transform;
        //    EditorCommon.EnumChildObject (prefab, meshCombineContext, (trans, param) =>
        //    {
        //        MeshCombineContext mcc = param as MeshCombineContext;
        //        mcc.enumPrefab (trans, mcc);
        //    });
        //    //sort
        //    mc.Sort ();

        //}
        #endregion
        #region merge small objects
        //void TryMerge2OtherGroup (MaterialGroup mg, int index, MergedMeshRef mmr)
        //{
        //    float minDist = float.MaxValue;
        //    int mergeIndex = -1;
        //    for (int i = index; i < mg.mergeMesh.Count; ++i)
        //    {
        //        var otherMMR = mg.mergeMesh[i];
        //        AABB mergeAABB = otherMMR.mergeAABB;
        //        mergeAABB.Encapsulate (ref mmr.mergeAABB);
        //        Vector2 deltaCenter = new Vector2 (
        //            otherMMR.mergeAABB.centerX - mergeAABB.centerX,
        //            otherMMR.mergeAABB.centerZ - mergeAABB.centerZ);

        //        float distSqr = deltaCenter.x * deltaCenter.x + deltaCenter.y * deltaCenter.y;

        //        float halfWidth = otherMMR.mergeAABB.sizeX * 0.5f;
        //        float halfHeight = otherMMR.mergeAABB.sizeZ * 0.5f;
        //        float distSqrSrc = halfWidth * halfWidth + halfHeight * halfHeight;
        //        if (distSqr < distSqrSrc * sceneConfig.mergeSizePercentThreshold || distSqr < 16) //4*4
        //        {
        //            if (distSqr < minDist)
        //            {
        //                minDist = distSqr;
        //                mergeIndex = i;
        //            }
        //        }
        //    }
        //    if (mergeIndex >= 0)
        //    {
        //        mmr.targetMergeGroupIndex = mergeIndex;
        //        mg.mergeMesh[mergeIndex].smallMergeObjets.AddRange (mmr.mergeObjets);
        //    }
        //}
        //bool NeedMerge (MergedMeshRef mmr)
        //{
        //    Vector3 size = mmr.mergeAABB.size;
        //    float max = Math.Max (size.x, size.y);
        //    max = Math.Max (max, size.z);
        //    return mmr.mergeObjets.Count <= sceneConfig.mergeCountThreshold &&
        //        max < sceneConfig.mergeSizeThreshold;
        //}

        //void MergeSmallObjects ()
        //{
        //    Transform meshCombine = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshCombine].transform;
        //    MeshCombine mc = meshCombine.GetComponent<MeshCombine> ();
        //    if (mc != null)
        //    {
        //        if (mc.mergeObjects != null)
        //        {
        //            for (int i = 0; i < mc.mergeObjects.Length; ++i)
        //            {
        //                var mo = mc.mergeObjects[i];
        //                if (mo != null)
        //                {
        //                    for (int j = 0; j < mo.matGroups.Count; ++j)
        //                    {
        //                        var mg = mo.matGroups[j];
        //                        for (int k = 0; k < mg.mergeMesh.Count; ++k)
        //                        {
        //                            var mmr = mg.mergeMesh[k];
        //                            mmr.targetMergeGroupIndex = -1;
        //                            mmr.smallMergeObjets.Clear ();
        //                        }
        //                        for (int k = 0; k < mg.mergeMesh.Count; ++k)
        //                        {
        //                            var mmr = mg.mergeMesh[k];

        //                            if (NeedMerge (mmr))
        //                            {
        //                                TryMerge2OtherGroup (mg, k + 1, mmr);
        //                            }
        //                            else
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion
        #region merge objects
        //void MergeObjects (MergedMeshRef mmr, Material material, ChunkMergeData cmd, List<MeshRenderObject> tmpMergeList)
        //{
        //    if (mmr.targetMergeGroupIndex == -1)
        //    {
        //        tmpMergeList.AddRange (mmr.mergeObjets);
        //        tmpMergeList.AddRange (mmr.smallMergeObjets);
        //        if (tmpMergeList.Count > 1)
        //        {
        //            Mesh combineMesh = MeshAssets.Merge (tmpMergeList, mmr.blockID, sceneContext.name);
        //            if (combineMesh != null)
        //            {
        //                var mm = new MergeMesh ()
        //                {
        //                mesh = combineMesh,
        //                blockID = mmr.blockID,
        //                material = material
        //                };
        //                cmd.mergeMeshs.Add (mm);
        //                mmr.mergeMesh = combineMesh;
        //            }
        //            else
        //            {
        //                DebugLog.AddErrorLog ("no mesh combine");
        //            }

        //        }
        //        else if (tmpMergeList.Count == 1)
        //        {
        //            var mm = new MergeMesh ()
        //            {
        //            mesh = tmpMergeList[0].GetRuntimeMesh (),
        //            blockID = mmr.blockID,
        //            material = material
        //            };
        //            cmd.mergeMeshs.Add (mm);
        //        }
        //        tmpMergeList.Clear ();
        //    }

        //}
        //void CombineObjects ()
        //{
        //    string name = sceneContext.name + "_MergeMesh";
        //    string path = string.Format ("{0}/{1}_MergeMesh.asset",
        //        sceneContext.configDir,
        //        name);

        //    ChunkMergeMesh chunkMergeMesh = AssetDatabase.LoadAssetAtPath<ChunkMergeMesh> (path);
        //    bool create = false;
        //    if (chunkMergeMesh == null)
        //    {
        //        chunkMergeMesh = ScriptableObject.CreateInstance<ChunkMergeMesh> ();
        //        chunkMergeMesh.name = name;
        //        create = true;

        //    }
        //    chunkMergeMesh.chunks.Clear ();
        //    GameObject go = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshCombine];
        //    if (go != null)
        //    {
        //        MeshCombine mc = go.GetComponent<MeshCombine> ();
        //        if (mc != null && mc.mergeObjects != null)
        //        {
        //            List<MeshRenderObject> tmpMergeList = new List<MeshRenderObject> ();
        //            for (int i = 0; i < mc.mergeObjects.Length; ++i)
        //            {
        //                ChunkMergeData cmd = new ChunkMergeData ();
        //                chunkMergeMesh.chunks.Add (cmd);
        //                var mo = mc.mergeObjects[i];
        //                if (mo != null)
        //                {
        //                    for (int j = 0; j < mo.matGroups.Count; ++j)
        //                    {
        //                        var mg = mo.matGroups[j];
        //                        for (int k = 0; k < mg.mergeMesh.Count; ++k)
        //                        {
        //                            var mmr = mg.mergeMesh[i];
        //                            MergeObjects (mmr, mg.material, cmd, tmpMergeList);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (create)
        //    {
        //        chunkMergeMesh = CommonAssets.CreateAsset<ChunkMergeMesh> (sceneContext.configDir,
        //            name, ".asset", chunkMergeMesh);
        //    }
        //    else
        //    {
        //        CommonAssets.SaveAsset (chunkMergeMesh);
        //    }
        //}
        #endregion
        //void SplitObject ()
        //{
        //    EditorCommon.EnumTransform funPrefabs = null;
        //    funPrefabs = (trans, param) =>
        //    {
        //        if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
        //        {
        //            SaveChunkContext scc = param as SaveChunkContext;
        //            scc.commonContext.tmpObjList.Clear ();
        //            string meshDir = LoadMgr.singleton.editorResPath;
        //            trans.GetComponentsInChildren<MeshRenderObject> (true, scc.commonContext.tmpObjList);
        //            for (int i = 0; i < scc.commonContext.tmpObjList.Count; ++i)
        //            {
        //                var oc = scc.commonContext.tmpObjList[i];
        //                var render = oc.GetRenderer ();
        //                if (render.enabled && render.gameObject.activeInHierarchy)
        //                {
        //                    // if (oc.chunkID == -1)
        //                    {
        //                        if (scc.splitTrans == null || scc.splitTrans != null && scc.splitTrans == render.transform)
        //                        {
        //                            scc.splitObjectInfo.Clear ();
        //                            scc.vertexChunkMap.Clear ();
        //                            scc.crossEdgeVertex.Clear ();
        //                            Mesh mesh = oc.GetMesh ();
        //                            Matrix4x4 matrix = render.transform.localToWorldMatrix;
        //                            if (mesh != null)
        //                            {
        //                                Vector3[] pos = mesh.vertices;
        //                                Vector3[] worldpos = new Vector3[pos.Length];
        //                                Vector3[] normal = mesh.normals;
        //                                Vector4[] tangent = mesh.tangents;
        //                                Vector2[] uv = mesh.uv;
        //                                Vector2[] uv2 = mesh.uv2;
        //                                int[] index = mesh.triangles;
        //                                for (int j = 0; j < index.Length; j += 3)
        //                                {
        //                                    int i0 = index[j];
        //                                    int i1 = index[j + 1];
        //                                    int i2 = index[j + 2];
        //                                    Vector3 p0 = matrix.MultiplyPoint (pos[i0]);
        //                                    Vector3 p1 = matrix.MultiplyPoint (pos[i1]);
        //                                    Vector3 p2 = matrix.MultiplyPoint (pos[i2]);
        //                                    worldpos[i0] = p0;
        //                                    worldpos[i1] = p1;
        //                                    worldpos[i2] = p1;

        //                                    int x;
        //                                    int z;
        //                                    int index0 = SceneQuadTree.FindChunkIndex (p0, chunkWidth, widthCount, heightCount, out x, out z);
        //                                    int index1 = SceneQuadTree.FindChunkIndex (p1, chunkWidth, widthCount, heightCount, out x, out z);
        //                                    int index2 = SceneQuadTree.FindChunkIndex (p2, chunkWidth, widthCount, heightCount, out x, out z);
        //                                    int chunkIndex = -1;

        //                                    if (index0 != index1 || index0 != index2 || index1 != index2)
        //                                    {
        //                                        chunkIndex = -1;
        //                                        CrossEdgeVertexInfo crossEdgeVertexInfo = new CrossEdgeVertexInfo ()
        //                                        {
        //                                            index0 = i0,
        //                                            index1 = i1,
        //                                            index2 = i2,
        //                                            chunkIndex0 = index0,
        //                                            chunkIndex1 = index1,
        //                                            chunkIndex2 = index2,

        //                                        };
        //                                        scc.crossEdgeVertex.Add (crossEdgeVertexInfo);
        //                                    }
        //                                    else
        //                                    {
        //                                        chunkIndex = index0;
        //                                    }
        //                                    if (chunkIndex >= 0)
        //                                    {
        //                                        ChunkVertexInfo chunkVertexInfo;
        //                                        if (!scc.splitObjectInfo.TryGetValue (chunkIndex, out chunkVertexInfo))
        //                                        {
        //                                            chunkVertexInfo = new ChunkVertexInfo ();
        //                                            chunkVertexInfo.x = x;
        //                                            chunkVertexInfo.z = z;
        //                                            chunkVertexInfo.centerRect = new Rect (x * chunkWidth + 15, z * chunkHeight + 15, chunkWidth - 30, chunkHeight - 30);
        //                                            scc.splitObjectInfo.Add (chunkIndex, chunkVertexInfo);
        //                                        }
        //                                        chunkVertexInfo.AddVertex (pos[i0], normal[i0], tangent[i0], uv[i0], uv2[i0], i0);
        //                                        chunkVertexInfo.AddVertex (pos[i1], normal[i1], tangent[i1], uv[i1], uv2[i1], i1);
        //                                        chunkVertexInfo.AddVertex (pos[i2], normal[i2], tangent[i2], uv[i2], uv2[i2], i2);
        //                                    }
        //                                }
        //                                EditorCommon.DestroyChildObjects (render.gameObject);
        //                                int meshCount = scc.splitObjectInfo.Count;
        //                                if (meshCount > 1)
        //                                {
        //                                    for (int j = 0; j < scc.crossEdgeVertex.Count; ++j)
        //                                    {
        //                                        var cev = scc.crossEdgeVertex[j];
        //                                        Vector3 center = (worldpos[cev.index0] + worldpos[cev.index1] + worldpos[cev.index2]) / 3;
        //                                        ChunkVertexInfo cvi0;
        //                                        scc.splitObjectInfo.TryGetValue (cev.chunkIndex0, out cvi0);
        //                                        ChunkVertexInfo cvi1;
        //                                        scc.splitObjectInfo.TryGetValue (cev.chunkIndex1, out cvi1);
        //                                        ChunkVertexInfo cvi2;
        //                                        scc.splitObjectInfo.TryGetValue (cev.chunkIndex2, out cvi2);

        //                                        float weight0 = cvi0 != null ? cvi0.DistWeight (center) : 0;
        //                                        float weight1 = cvi1 != null ? cvi1.DistWeight (center) : 0;
        //                                        float weight2 = cvi2 != null ? cvi2.DistWeight (center) : 0;
        //                                        ChunkVertexInfo cvi = null;
        //                                        if (weight0 > weight1)
        //                                        {
        //                                            if (weight0 > weight2)
        //                                            {
        //                                                cvi = cvi0;
        //                                            }
        //                                            else
        //                                            {
        //                                                cvi = cvi2;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (weight1 > weight2)
        //                                            {
        //                                                cvi = cvi1;
        //                                            }
        //                                            else
        //                                            {
        //                                                cvi = cvi2;
        //                                            }
        //                                        }
        //                                        if (cvi != null)
        //                                        {
        //                                            cvi.AddVertex (pos[cev.index0], normal[cev.index0], tangent[cev.index0], uv[cev.index0], uv2[cev.index0], cev.index0);
        //                                            cvi.AddVertex (pos[cev.index1], normal[cev.index1], tangent[cev.index1], uv[cev.index1], uv2[cev.index1], cev.index1);
        //                                            cvi.AddVertex (pos[cev.index2], normal[cev.index2], tangent[cev.index2], uv[cev.index2], uv2[cev.index2], cev.index2);
        //                                        }
        //                                        else
        //                                        {
        //                                            Debug.LogError ("edge chunk not find");
        //                                        }
        //                                    }
        //                                    var it = scc.splitObjectInfo.GetEnumerator ();

        //                                    while (it.MoveNext ())
        //                                    {
        //                                        var kvp = it.Current;
        //                                        var value = kvp.Value;
        //                                        if (value.index.Count < 1000 && !value.hasCenterVertex)
        //                                        {
        //                                            bool merged = false;
        //                                            for (int j = 0; j < value.index.Count; j += 3)
        //                                            {
        //                                                int i0 = value.index[j];
        //                                                int i1 = value.index[j + 1];
        //                                                int i2 = value.index[j + 2];
        //                                                Vector3 p0 = value.pos[i0];
        //                                                Vector3 p1 = value.pos[i1];
        //                                                Vector3 p2 = value.pos[i2];
        //                                                Vector3 center = (worldpos[i0] + worldpos[i1] + worldpos[i2]) / 3;
        //                                                Vector2 c = new Vector2 (center.x, center.z);
        //                                                float minDist = float.MaxValue;
        //                                                var find = scc.splitObjectInfo.GetEnumerator ();
        //                                                ChunkVertexInfo chunkVertexInfo = null;
        //                                                while (find.MoveNext ())
        //                                                {
        //                                                    var findKvp = find.Current;
        //                                                    var findValue = findKvp.Value;
        //                                                    if (findValue != value &&
        //                                                        findValue.pos.Count > 0 &&
        //                                                        Mathf.Abs (findValue.x - value.x) <= 1 && Mathf.Abs (findValue.z - value.z) <= 1)
        //                                                    {
        //                                                        if (findValue.x == value.x || findValue.z == value.z)
        //                                                        {
        //                                                            chunkVertexInfo = findValue;
        //                                                            break;
        //                                                        }
        //                                                        float d = Vector2.Distance (findValue.centerRect.center, c);
        //                                                        if (d < minDist)
        //                                                        {
        //                                                            minDist = d;
        //                                                            chunkVertexInfo = findValue;
        //                                                        }
        //                                                    }
        //                                                }
        //                                                if (chunkVertexInfo != null)
        //                                                {
        //                                                    chunkVertexInfo.AddVertex (p0, value.normal[i0], value.tangent[i0], value.uv[i0], value.uv2[i0]);
        //                                                    chunkVertexInfo.AddVertex (p1, value.normal[i1], value.tangent[i1], value.uv[i1], value.uv2[i1]);
        //                                                    chunkVertexInfo.AddVertex (p2, value.normal[i2], value.tangent[i2], value.uv[i2], value.uv2[i2]);
        //                                                    merged = true;
        //                                                }
        //                                            }
        //                                            if (merged)
        //                                                value.pos.Clear ();
        //                                        }

        //                                    }

        //                                    it = scc.splitObjectInfo.GetEnumerator ();
        //                                    while (it.MoveNext ())
        //                                    {
        //                                        var kvp = it.Current;
        //                                        if (kvp.Value.pos.Count > 0)
        //                                        {
        //                                            // Debug.LogError(kvp.Value.pos.Count);
        //                                            Mesh splitMesh = new Mesh ();
        //                                            splitMesh.name = string.Format ("{0}_Chunk_{1}", mesh.name, kvp.Key);
        //                                            splitMesh.vertices = kvp.Value.pos.ToArray ();
        //                                            splitMesh.normals = kvp.Value.normal.ToArray ();
        //                                            splitMesh.tangents = kvp.Value.tangent.ToArray ();
        //                                            splitMesh.uv = kvp.Value.uv.ToArray ();
        //                                            splitMesh.uv2 = kvp.Value.uv2.ToArray ();
        //                                            splitMesh.triangles = kvp.Value.index.ToArray ();
        //                                            // splitMesh.RecalculateTangents();
        //                                            splitMesh.UploadMeshData (true);
        //                                            MeshUtility.SetMeshCompression (splitMesh, ModelImporterMeshCompression.Medium);
        //                                            Mesh m = CommonAssets.CreateAsset<Mesh> (meshDir, splitMesh.name, ".asset", splitMesh);
        //                                            GameObject gameObject = new GameObject (splitMesh.name);
        //                                            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter> ();
        //                                            meshFilter.sharedMesh = m;
        //                                            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();
        //                                            meshRenderer.sharedMaterial = render.sharedMaterial;
        //                                            gameObject.transform.parent = render.transform;
        //                                            gameObject.transform.localPosition = Vector3.zero;
        //                                            gameObject.transform.localRotation = Quaternion.identity;
        //                                            gameObject.transform.localScale = Vector3.one;
        //                                        }

        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            EditorCommon.EnumChildObject (trans, param, funPrefabs);
        //        }

        //    };

        //    string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.StaticPrefab];
        //    EditorCommon.EnumTargetObject (path, (trans, param) =>
        //    {
        //        funPrefabs (trans, saveChunkContext);
        //    });
        //}
        //void DrawCombineGizmo ()
        //{
        //    if (MaterialGroup.currentMG != null)
        //    {
        //        var mmList = MaterialGroup.currentMG.mergeMesh;
        //        Gizmos.color = Color.white;
        //        for (int i = 0; i < mmList.Count; ++i)
        //        {
        //            var mmr = mmList[i];
        //            if (MaterialGroup.blockID == -1 || MaterialGroup.blockID == mmr.blockID)
        //            {
        //                // for (int j = 0; j < mmr.mergeObjets.Count; ++j)
        //                // {
        //                //     var mo = mmr.mergeObjets[j];
        //                //     Gizmos.DrawWireCube (mo.sceneAABB.center, mo.sceneAABB.size);
        //                // }
        //            }
        //        }
        //    }
        //}

        //void UpdateCombine ()
        //{
        //    switch (combineOp)
        //    {
        //        case OpCombineType.GroupingObjects:
        //            GroupingObjects ();
        //            break;
        //        case OpCombineType.MergeSmallObjects:
        //            MergeSmallObjects ();
        //            break;
        //        case OpCombineType.CombineObjects:
        //            CombineObjects ();
        //            break;
        //    }
        //    combineOp = OpCombineType.OpNone;
        //}

        //void OnCombineGuUI ()
        //{
        //    EditorCommon.BeginGroup ("Mesh Combine");
        //    GUILayout.BeginHorizontal ();
        //    if (GUILayout.Button ("GroupingObjects", GUILayout.MaxWidth (160)))
        //    {
        //        combineOp = OpCombineType.GroupingObjects;
        //    }
        //    if (GUILayout.Button ("MergeSmallObjects", GUILayout.MaxWidth (160)))
        //    {
        //        combineOp = OpCombineType.MergeSmallObjects;
        //    }

        //    if (GUILayout.Button ("CombineObjects", GUILayout.MaxWidth (160)))
        //    {
        //        combineOp = OpCombineType.CombineObjects;
        //    }
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.mergeCountThreshold = EditorGUILayout.IntSlider ("MergeMaxCount", sceneConfig.mergeCountThreshold, 1, 3);
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.mergeSizeThreshold = EditorGUILayout.Slider ("MergeSizeThreshold", sceneConfig.mergeSizeThreshold, 1, 20);
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.mergeSizePercentThreshold = EditorGUILayout.Slider ("MergeDistPercentThreshold", sceneConfig.mergeSizePercentThreshold, 0.1f, 1);
        //    GUILayout.EndHorizontal ();
        //    EditorCommon.EndGroup ();
        //}
    }
}
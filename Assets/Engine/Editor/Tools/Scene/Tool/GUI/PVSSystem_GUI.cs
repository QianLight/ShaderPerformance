using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.Jobs;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{

    public partial class PVSSystem : SceneResProcess
    {
        enum OpType
        {
            OpNone,
            OpGenCell,
            OpGenCollider,
            OpBakePVS,
            OpClear
        }
        enum OpWorkType
        {
            OpNone,
            OpBaking,
        }
        class PVSContext
        {
            public AABB sceneAABB;
            public NativeArray<float> sRandomNumbers;
            public NativeArray<bool> sCellBakeResults;
            public NativeArray<AABB> sInputMeshBounds;
            public NativeArray<Vector3> sRaySampleResults;
            public List<AABB> meshAABBs = new List<AABB>();
            public List<PVSCollider> colliders = new List<PVSCollider>();
            public int currentChunkIndex = 0;
            public int currentCellIndex = 0;

            public void Clear()
            {
                meshAABBs.Clear();
                colliders.Clear();
                if (sRandomNumbers.IsCreated)
                {
                    sRandomNumbers.Dispose();
                }
                if (sCellBakeResults.IsCreated)
                {
                    sCellBakeResults.Dispose();
                }
                if (sInputMeshBounds.IsCreated)
                {
                    sInputMeshBounds.Dispose();
                }
                if (sRaySampleResults.IsCreated)
                {
                    sRaySampleResults.Dispose();
                }
                currentChunkIndex = 0;
                currentCellIndex = 0;
        }
        }
        class PVSColliderContext
        {
            public Transform parent;
            public Dictionary<MeshRenderObject, PVSCollider> colliderMap = new Dictionary<MeshRenderObject, PVSCollider>();
        }
        
        public List<PVSChunk> pvsChunks = new List<PVSChunk>();
        private OpType opType = OpType.OpNone;
        private OpWorkType opWorkType = OpWorkType.OpNone;
        private PVSContext pvsContext = new PVSContext();
        private PVSColliderContext pvsCollinerContext = new PVSColliderContext();
        private int debugChunkIndex = -1;
        private int debugCellIndex = -1;
        private int debugMeshSample = -1;
        public override bool HasGUI { get { return true; } }

        public override void InitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            InitChunks();
        }

        private void InitChunks()
        {
            pvsChunks.Clear();
            for (int i = 0; i < resData.workspace.childCount; ++i)
            {
                var child = resData.workspace.GetChild(i);
                if (child.TryGetComponent<PVSChunk>(out var pc))
                {
                    pvsChunks.Add(pc);
                }
            }
        }
        public override void UnInitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            pvsContext.Clear();
        }
        
        public override void OnGUI(ref SceneContext sceneContext, object param, ref Rect rect)
        {
            EditorGUI.indentLevel++;
            var editorContext = param as SceneEditContext;
            var localConfig = editorContext.localConfig;
            var config = editorContext.sc;

            localConfig.pvsFolder = EditorGUILayout.Foldout(localConfig.pvsFolder, "PVS");
            if (localConfig.pvsFolder)
            {
                GUILayout.BeginHorizontal();
                config.cellSize = (PVSCellSize)EditorGUILayout.EnumPopup("CellSize", config.cellSize, GUILayout.MaxWidth(300));
                PVSChunk.gizmosCellSize = (int)config.cellSize;
                if (GUILayout.Button("Clear", GUILayout.MaxWidth(100)))
                {
                    opType = OpType.OpClear;
                }
                if (GUILayout.Button("GenCell", GUILayout.MaxWidth(100)))
                {
                    opType = OpType.OpGenCell;
                }
                if (GUILayout.Button("GenCollider", GUILayout.MaxWidth(100)))
                {
                    opType = OpType.OpGenCollider;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                localConfig.sampleNum = EditorGUILayout.IntSlider("sampleNum", localConfig.sampleNum, 1, 256, GUILayout.MaxWidth(300));
                if (GUILayout.Button("BakePVS", GUILayout.MaxWidth(100)))
                {
                    opType = OpType.OpBakePVS;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                debugChunkIndex = EditorGUILayout.IntSlider("DebugChunkIndex", debugChunkIndex, -1, pvsChunks.Count - 1, GUILayout.MaxWidth(300));
                if (debugChunkIndex >= 0 && debugChunkIndex < pvsChunks.Count)
                {
                    var pc = pvsChunks[debugChunkIndex];
                    EditorGUILayout.LabelField(pc.name, GUILayout.MaxWidth(120));
                    debugCellIndex = EditorGUILayout.IntField("DebugCellIndex", debugCellIndex, GUILayout.MaxWidth(300));
                }
                GUILayout.EndHorizontal();

                if(debugCellIndex>=0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("MeshCount:{0}", pvsContext.colliders.Count.ToString()), GUILayout.MaxWidth(300));
                    debugMeshSample = EditorGUILayout.IntField("DebugMeshSample", debugMeshSample, GUILayout.MaxWidth(300));
                    GUILayout.EndHorizontal();
                }

            }

            EditorGUI.indentLevel--;
        }

        public override void OnDrawGizmos(ref SceneContext sceneContext, object param)
        {
            var editorContext = param as SceneEditContext;
            var localConfig = editorContext.localConfig;
            if (pvsContext.sRaySampleResults.IsCreated)
            {
                int startIndex = debugMeshSample * (3 * localConfig.sampleNum);
                int endIndex = startIndex + 3 * localConfig.sampleNum;
                if (startIndex >= 0 && startIndex < pvsContext.sRaySampleResults.Length &&
                    endIndex < pvsContext.sRaySampleResults.Length)
                {
                    if (pvsContext.sCellBakeResults[debugMeshSample])
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;
                    }
                    var pc = pvsContext.colliders[debugMeshSample];
                    pc.DrawGizmos();
                    for (int i = startIndex; i < endIndex; i += 3)
                    {
                        var origin = pvsContext.sRaySampleResults[i];
                        var destination = pvsContext.sRaySampleResults[i + 1];
                        Gizmos.DrawLine(origin, destination);
                    }
                }
            }
        }        
        
        public override void Update(ref SceneContext sceneContext, object param)
        {
            switch (opType)
            {
                case OpType.OpGenCell:
                    PrepareGenCell(ref sceneContext, param as SceneEditContext);
                    break;
                case OpType.OpGenCollider:
                    PrepareCollider(ref sceneContext, param as SceneEditContext);
                    break;
                case OpType.OpBakePVS:
                    BakePVS(ref sceneContext, param as SceneEditContext);
                    break;
                case OpType.OpClear:
                    ClearPVS(ref sceneContext, param as SceneEditContext);
                    break;
            }
            opType = OpType.OpNone;
            InnerUpdate(ref sceneContext, param as SceneEditContext);
        }
        private bool BakeCell(PVSChunk pc, int cellIndex,
            float cellSize, float halfCellSize, ref Vector3 size, SceneConfigData localConfig)
        {
            if (cellIndex < pc.cells.Count)
            {

                EditorUtility.DisplayProgressBar(string.Format("bakeing {0} ...", pc.name),
                                  string.Format("bake {0} cell {1}/{2}",
                                      pc.name,
                                      cellIndex,
                                      pc.cells.Count),
                                    (float)cellIndex / pc.cells.Count);

                var cell = pc.cells[cellIndex];
                int sampleNum = pc.sampleNum > 0 ? pc.sampleNum : localConfig.sampleNum;
                BakeCell(pc, cell, cellSize, halfCellSize, ref size, localConfig.sampleNum);
                return true;
            }
            return false;
        }

        private void InnerUpdate(ref SceneContext sceneContext, SceneEditContext editorContext)
        {
            switch (opWorkType)
            {
                case OpWorkType.OpBaking:
                    {
                        var localConfig = editorContext.localConfig;
                        var config = editorContext.sc;
                        float cellSize = (float)config.cellSize;
                        float halfCellSize = cellSize / 2;
                        Vector3 size = new Vector3(cellSize, cellSize*2, cellSize);
                        if (debugChunkIndex >= 0 && debugChunkIndex < pvsChunks.Count)
                        {
                            var pc = pvsChunks[debugChunkIndex];
                            if (pc != null)
                            {
                                if (debugCellIndex >= 0)
                                {
                                    BakeCell(pc, debugCellIndex, cellSize, halfCellSize, ref size, localConfig);
                                    EditorUtility.ClearProgressBar();
                                    EditorUtility.DisplayDialog("Finish", "Bake Finish!", "OK");
                                    opWorkType = OpWorkType.OpNone;
                                }
                                else
                                {
                                    if (BakeCell(pc, pvsContext.currentCellIndex, cellSize, halfCellSize, ref size, localConfig))
                                    {
                                        pvsContext.currentCellIndex++;
                                    }
                                    else
                                    {
                                        EditorUtility.ClearProgressBar();
                                        EditorUtility.DisplayDialog("Finish", "Bake Finish!", "OK");
                                        opWorkType = OpWorkType.OpNone;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (pvsContext.currentChunkIndex < pvsChunks.Count)
                            {                               
                                var pc = pvsChunks[pvsContext.currentChunkIndex];
                                if (pc != null)
                                {
                                    BakeCell(pc, pvsContext.currentCellIndex, cellSize, halfCellSize, ref size, localConfig);
                                    pvsContext.currentCellIndex++;
                                    if (pvsContext.currentCellIndex >= pc.cells.Count)
                                    {
                                        pvsContext.currentCellIndex = 0;
                                        pvsContext.currentChunkIndex++;
                                    }
                                }
                                else
                                {
                                    pvsContext.currentChunkIndex++;
                                }
                                if (pvsContext.currentChunkIndex >= pvsChunks.Count)
                                {
                                    EditorUtility.ClearProgressBar();
                                    EditorUtility.DisplayDialog("Finish", "Bake Finish!", "OK");
                                    opWorkType = OpWorkType.OpNone;
                                }
                            }
                            else
                            {
                                EditorUtility.ClearProgressBar();
                                EditorUtility.DisplayDialog("Finish", "Bake Finish!", "OK");
                                opWorkType = OpWorkType.OpNone;
                            }
                        }
                        
                        
                    }
                    break;
            }
        }

        private void ClearPVS(ref SceneContext sceneContext, SceneEditContext editorContext)
        {
            for (int i = 0; i < resData.workspace.childCount; ++i)
            {
                var child = resData.workspace.GetChild(i);
                if (child.TryGetComponent<PVSChunk>(out var pc))
                {
                    pc.Clear();
                }
            }
        }
        private void BakeCell(PVSChunk pc, PVSResultCell cell, float cellSize, float halfCellSize, ref Vector3 size,int sampleNum)
        {
            Vector3 cellPos = new Vector3();
            cellPos.x = cell.cellindex.x * cellSize + halfCellSize + pc.xOffset;
            cellPos.y = cell.cellindex.y * cellSize + halfCellSize;
            cellPos.z = cell.cellindex.z * cellSize + halfCellSize + pc.zOffset;
            var cellAABB = new AABB();
            cellAABB.Init(ref cellPos, ref size);
            if (cell.result != null)
            {
                cell.result.Clear();
            }
            if (EngineUtility.IntersectsAABB(ref cellAABB, ref pvsContext.sceneAABB))
            {
                PVSGenCellSamplePointsJob job = new PVSGenCellSamplePointsJob
                {
                    NumRays = sampleNum,
                    InputCellBounds = cellAABB,
                    InputMeshBounds = pvsContext.sInputMeshBounds,
                    RandomNumbers = pvsContext.sRandomNumbers,
                    BakeResulst = pvsContext.sCellBakeResults,
                    RaySampleResults = pvsContext.sRaySampleResults,
                };
                JobHandle handle = job.Schedule(pvsContext.meshAABBs.Count, 8);
                handle.Complete();

                RaycastCommandBaker baker = new RaycastCommandBaker
                {
                    m_BakeResults = pvsContext.sCellBakeResults,
                    m_RaySampleResults = pvsContext.sRaySampleResults,
                };
                baker.Bake(sampleNum, pvsContext.meshAABBs.Count);
                for (int i = 0; i < pvsContext.colliders.Count; ++i)
                {
                    if (pvsContext.sCellBakeResults[i])
                    {
                        if (cell.result == null)
                        {
                            cell.result = new List<PVSCollider>();
                        }
                        cell.result.Add(pvsContext.colliders[i]);
                    }
                }
            }
        }
        private int Pos2CellIndex(ref Vector3 pos, float offsetX, float offsetZ,
            float cellSize, int xCellCount, out int x, out int z)
        {
            x = Mathf.FloorToInt((pos.x - offsetX) / cellSize);
            z = Mathf.FloorToInt((pos.z - offsetZ) / cellSize);
            return x + z * xCellCount;
        }
        private void Pos2BlockOffset(ref Vector3 pos, float size, int maxX, int maxZ, out int x, out int z)
        {
            x = Mathf.FloorToInt(pos.x / size);
            if (x < 0)
            {
                x = 0;
            }
            if (x >= maxX)
            {
                x = maxX - 1;
            }
            z = Mathf.FloorToInt(pos.z / size);
            if (z < 0)
            {
                z = 0;
            }
            if (z >= maxZ)
            {
                z = maxZ - 1;
            }
        }
        private void PrepareGenCell(ref SceneContext sceneContext, SceneEditContext editorContext)
        {
            if (moveGrid != null)
            {
                moveGrid.Load(ref sceneContext);
                var localConfig = editorContext.localConfig;
                var config = editorContext.sc;
                float cellSize = (float)config.cellSize;
                int xCellCount = (int)(EngineContext.ChunkSize / cellSize);
                int xChunkCount = EngineContext.instance.xChunkCount * 4;
                int zChunkCount = EngineContext.instance.zChunkCount * 4;
                Dictionary<int, PVSChunk> chunks = new Dictionary<int, PVSChunk>();
                for (int i = 0; i < resData.workspace.childCount; ++i)
                {
                    var child = resData.workspace.GetChild(i);                    
                    if (child.TryGetComponent<PVSChunk>(out var pc))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                int blockCount = moveGrid.BlockCount;
                for (int i = 0; i < blockCount; ++i)
                {
                    int gridCount = moveGrid.GetGridCount(i);
                    for (int j = 0; j < gridCount; ++j)
                    {
                        Vector3 pos = new Vector3();
                        if (moveGrid.QueryGrid(i, j, ref pos))
                        {
                            Pos2BlockOffset(ref pos, EngineContext.Lod1ChunkSize, xChunkCount, zChunkCount, 
                                out var xIndex, out var zIndex);
                            int chunkIndex = xIndex + zIndex * xChunkCount;
                            if(!chunks.TryGetValue(chunkIndex,out var pvsChunk))
                            {
                                string pvsChunkName = string.Format("PVSChunk_{0}_{1}", xIndex.ToString(), zIndex.ToString());
                                var pvsChunkTran = resData.workspace.Find(pvsChunkName);
                                if (pvsChunkTran == null)
                                {
                                    pvsChunkTran = new GameObject(pvsChunkName).transform;
                                    pvsChunkTran.parent = resData.workspace;
                                }
                                if (!pvsChunkTran.TryGetComponent(out pvsChunk))
                                {
                                    pvsChunk = pvsChunkTran.gameObject.AddComponent<PVSChunk>();
                                }
                                pvsChunkTran.gameObject.SetActive(true);
                                chunks.Add(chunkIndex, pvsChunk);
                                pvsChunk.x = xIndex;
                                pvsChunk.z = zIndex;
                                pvsChunk.xOffset = EngineContext.Lod1ChunkSize * xIndex;
                                pvsChunk.zOffset = EngineContext.Lod1ChunkSize * zIndex;
                                pvsChunkTran.position = new Vector3(pvsChunk.xOffset, 0, pvsChunk.zOffset);
                                pvsChunk.Clear();

                            }
                            int index = Pos2CellIndex(ref pos, pvsChunk.xOffset, pvsChunk.zOffset, cellSize, xCellCount,
                                out var x, out var z);
                            pvsChunk.Add(cellSize, index, ref pos, x, z);
                        }
                    }                    
                }
                foreach(var pc in chunks.Values)
                    pc.PostProcess();
                for (int i = resData.workspace.childCount -1; i >=0 ; --i)
                {
                    var child = resData.workspace.GetChild(i);
                    if (child.TryGetComponent<PVSChunk>(out var pc))
                    {
                        if(!child.gameObject.activeSelf)
                        {
                            GameObject.DestroyImmediate(child.gameObject);
                        }
                    }
                }
            }
            EditorUtility.DisplayDialog("Finish", "Gen Cell Finish!", "OK");
        }

        private void PrepareCollider(ref SceneContext sceneContext, SceneEditContext editorContext)
        {
            var pvsChunkTran = resData.workspace.Find("PVSCollider");
            if (pvsChunkTran == null)
            {
                pvsChunkTran = new GameObject("PVSCollider").transform;
                pvsChunkTran.parent = resData.workspace;
            }
            EditorCommon.EnumTransform fun = null;
            fun = (trans, param) =>
            {
                if (trans.gameObject.activeInHierarchy && 
                    trans.TryGetComponent<MeshRenderObject>(out var mro))
                {
                    if (mro.pvsObjState != PVSOcclude.PVSNone)
                    {
                        string name = mro.name;
                        var pcc = param as PVSColliderContext;
                        Transform mcTrans = null;
                        if (pcc.colliderMap.TryGetValue(mro, out var pvsCollider))
                        {
                            mcTrans = pvsCollider.transform;
                        }
                        else
                        {
                            mcTrans = new GameObject(name).transform;
                            mcTrans.parent = pcc.parent;
                            pvsCollider = mcTrans.gameObject.AddComponent<PVSCollider>();
                            pvsCollider.mroRef = mro;
                        }

                        mcTrans.position = mro.transform.position;
                        mcTrans.rotation = mro.transform.rotation;
                        mcTrans.localScale = mro.transform.lossyScale;
                        if (mro.pvsObjState == PVSOcclude.PVSOccludee)
                        {
                            mcTrans.gameObject.layer = RaycastCommandBaker.PVSOccludeeLayer;
                        }
                        else if (mro.pvsObjState == PVSOcclude.PVSOccluder)
                        {
                            mcTrans.gameObject.layer = RaycastCommandBaker.PVSOccluderLayer;
                        }

                        if (!mcTrans.TryGetComponent<MeshCollider>(out var mc))
                        {
                            mc = mcTrans.gameObject.AddComponent<MeshCollider>();
                        }
                        mc.sharedMesh = mro.GetMesh();
                        mcTrans.gameObject.SetActive(true);
                        if (mro.chunkSubMesh != null)
                        {
                            mc.enabled = false;
                            pvsCollider.enabled = false;
                            for (int i = mcTrans.childCount - 1; i >= 0; --i)
                            {
                                var sub = mcTrans.GetChild(i);
                                GameObject.DestroyImmediate(sub.gameObject);
                            }
                            for (int i = 0; i < mro.chunkSubMesh.subMesh.Count; ++i)
                            {
                                var sm = mro.chunkSubMesh.subMesh[i];
                                if (sm.m != null)
                                {
                                    var subTrans = new GameObject(sm.m.name).transform;
                                    subTrans.localScale = Vector3.one;
                                    subTrans.parent = mcTrans;
                                    subTrans.position = Vector3.zero;
                                    subTrans.rotation = Quaternion.identity;
                                    var submc = subTrans.gameObject.AddComponent<MeshCollider>();
                                    submc.sharedMesh = sm.m;
                                    subTrans.gameObject.layer = mcTrans.gameObject.layer;
                                    var subPVSCollider = submc.gameObject.AddComponent<PVSCollider>();
                                    subPVSCollider.mroRef = mro;
                                    subPVSCollider.subIndex = i;
                                }       
                            }
                            mcTrans.gameObject.layer = DefaultGameObjectLayer.DefaultLayer;
                        }
                    }
                }
                EditorCommon.EnumChildObject(trans, param, fun);
            };
            pvsCollinerContext.parent = pvsChunkTran;
            pvsCollinerContext.colliderMap.Clear();
            for (int i = 0; i < pvsChunkTran.childCount; ++i)
            {
                var child = pvsChunkTran.GetChild(i);
                child.gameObject.SetActive(false);
                if (child.TryGetComponent<PVSCollider>(out var pc))
                {
                    if (pc.mroRef != null)
                        pvsCollinerContext.colliderMap.Add(pc.mroRef, pc);
                }                
            }            
            EditorCommon.EnumChildObject(StaticObjectSystem.system.resData.workspace, pvsCollinerContext, fun);

            for (int i = pvsChunkTran.childCount - 1; i >= 0; --i)
            {
                var child = pvsChunkTran.GetChild(i);
                if(!child.gameObject.activeSelf)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            EditorUtility.DisplayDialog("Finish", "Gen Collider Finish!", "OK");
        }
        private void InitPVSCollider(Transform pvsColliderTrans)
        {
            for (int i = 0; i < pvsColliderTrans.childCount; ++i)
            {
                var child = pvsColliderTrans.GetChild(i);
                if (child.TryGetComponent<PVSCollider>(out var pc))
                {
                    if (pc.mroRef != null)
                    {
                        if (pvsContext.meshAABBs.Count == 0)
                        {
                            pvsContext.sceneAABB.Init(ref pc.mroRef.aabb);
                        }
                        else
                        {
                            pvsContext.sceneAABB.Encapsulate(ref pc.mroRef.aabb);
                        }
                        pc.index = pvsContext.meshAABBs.Count;
                        pvsContext.meshAABBs.Add(pc.mroRef.aabb);
                        pvsContext.colliders.Add(pc);
                    }
                }
                InitPVSCollider(child);
            }
        }
        private void InitBake(SceneEditContext editorContext)
        {
            var pvsChunkTran = resData.workspace.Find("PVSCollider");
            if (pvsChunkTran != null)
            {
                InitChunks();
                pvsContext.Clear();
                InitPVSCollider(pvsChunkTran);                

                if (pvsContext.meshAABBs.Count > 0)
                {
                    var localConfig = editorContext.localConfig;
                    pvsContext.sRaySampleResults = new NativeArray<Vector3>(pvsContext.meshAABBs.Count * (3 * localConfig.sampleNum), Allocator.Persistent);
                    pvsContext.sCellBakeResults = new NativeArray<bool>(pvsContext.meshAABBs.Count, Allocator.Persistent);
                    pvsContext.sInputMeshBounds = new NativeArray<AABB>(pvsContext.meshAABBs.ToArray(), Allocator.Persistent);
                    MTRandomGenerator.Generate(localConfig.sampleNum, 2);
                    pvsContext.sRandomNumbers = new NativeArray<float>(MTRandomGenerator.sRandomNumbers, Allocator.Persistent);
                    MTRandomGenerator.Release();
                }
            }
        }
        private void BakePVS(ref SceneContext sceneContext, SceneEditContext editorContext)
        {
            InitBake(editorContext);
            if (pvsContext.meshAABBs.Count > 0)
            {
                opWorkType = OpWorkType.OpBaking;
            }
        }
    }
}
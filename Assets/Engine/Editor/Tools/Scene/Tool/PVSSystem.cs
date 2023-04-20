using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public partial class PVSSystem : SceneResProcess
    {
        public static PVSSystem system;
        private IMoveGrid moveGrid;
        private int meshCount = 0;
        private int maskCount = 0;

        public override void Init(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init(ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
            serialize = SerializeCb;
            system = this;
            var types = EngineUtility.GetAssemblyType(typeof(IMoveGrid), "IMoveGrid");
            foreach (var t in types)
            {
                moveGrid = Activator.CreateInstance(t) as IMoveGrid;
                if (moveGrid != null)
                {
                    //DebugLog.AddErrorLog2("move grid:{0}", t.Name);
                    break;
                }
            }
        }

        ////////////////////////////PreSerialize////////////////////////////
        protected static void PreSerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<PVSCollider>(out var pvsCollider))
            {
                var mro = pvsCollider.mroRef;
                if (mro != null)
                {
                    if (mro.chunkSubMesh != null &&
                        pvsCollider.subIndex >= 0 &&
                        pvsCollider.subIndex < mro.chunkSubMesh.subMesh.Count)
                    {
                        var subMesh = mro.chunkSubMesh.subMesh[pvsCollider.subIndex];
                        subMesh.id = pvsCollider.index;
                    }
                    else
                    {
                        mro.id = pvsCollider.index;       
                    }
                    if (system.meshCount < pvsCollider.index)
                    {
                        system.meshCount = pvsCollider.index;
                    }
                }
            }
            EditorCommon.EnumChildObject(trans, param, ssContext.preSerialize);
        }

        public override void PreSerialize(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            meshCount = 0;
            maskCount = 0;
            base.PreSerialize(ref sceneContext, ssContext);
            if (meshCount > 0)
            {
                meshCount += 1;
                maskCount = meshCount / 32;
                if (maskCount % 32 > 0)
                {
                    maskCount++;
                }
            }
            ssContext.sd.pvsCellSize = (int)ssContext.sceneConfig.cellSize;
            ssContext.sd.pvsMaskCount = maskCount;
        }

        ////////////////////////////Serialize////////////////////////////
        protected static void SerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<PVSChunk>(out var pvsChunk))
            {
                if (pvsChunk.cells.Count > 0)
                {
                    var pcd = new PVSChunkData()
                    {
                        x = pvsChunk.x,
                        z = pvsChunk.z
                    };
                    for (int i = 0; i < pvsChunk.cells.Count; ++i)
                    {
                        var cell = pvsChunk.cells[i];
                        if (cell.result != null && cell.result.Count > 0)
                        {
                            PVSCellData pvsCell = new PVSCellData()
                            {
                                cellindex = cell.cellindex,
                                visibleMask = new uint[system.maskCount],
                            };
      
                            foreach (var pvsCollider in cell.result)
                            {
    
                                if (pvsCollider.mroRef != null)
                                {
                                    var pvsIndex = pvsCollider.index;
                                    int maskIndex = pvsIndex / 32;
                                    int maskOffset = pvsIndex % 32;
                                    pvsCell.visibleMask[maskIndex] |= (uint)(1 << maskOffset);
                                    if (pvsChunk.debug && pvsChunk.drawCellIndex == i && pvsCollider.debug)
                                    {
                                        DebugLog.AddErrorLog2("mask index:{0} offset:{1} name:{2}",
                                                                           maskIndex.ToString(), maskOffset.ToString(), pvsCollider.mroRef.name);

                                    }

                                }
                            }
                            pcd.cells.Add(pvsCell);
                        }
                    }
                    ssContext.sd.pvsChunkData.Add(pcd);
                }
            }
            EditorCommon.EnumChildObject(trans, param, ssContext.serialize);
        }

        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
           
        }

        public static void SaveChunk(BinaryWriter bw, PVSChunkData pvsChunk, int blockCellCount,int pvsMaskCount)
        {
            int maskGroupCount = 0;
            Dictionary<int, List<PVSCellData>> sortCells = new Dictionary<int, List<PVSCellData>>();
            for (int xx = 0; xx < pvsChunk.cells.Count; ++xx)
            {
                var cell = pvsChunk.cells[xx];
                if (cell.visibleMask != null)
                {
                    int cellIndex = cell.cellindex.x + cell.cellindex.z * blockCellCount;
                    if (!sortCells.TryGetValue(cellIndex, out var cells))
                    {
                        cells = new List<PVSCellData>();
                        sortCells.Add(cellIndex, cells);
                    }
                    cells.Add(cell);
                    maskGroupCount++;
                }
            }
            bw.Write(maskGroupCount * pvsMaskCount);
            var it = sortCells.GetEnumerator();
            while (it.MoveNext())
            {
                var kvp = it.Current;
                var cells = kvp.Value;
                for (int xx = 0; xx < cells.Count; ++xx)
                {
                    var cell = cells[xx];
                    foreach (var mask in cell.visibleMask)
                    {
                        bw.Write(mask);
                    }
                }
            }
            bw.Write((short)sortCells.Count);
            it = sortCells.GetEnumerator();
            while (it.MoveNext())
            {
                var kvp = it.Current;
                int x = kvp.Key % blockCellCount;
                int z = kvp.Key / blockCellCount;
                bw.Write((byte)x);
                bw.Write((byte)z);
                var cells = kvp.Value;
                bw.Write((byte)cells.Count);
                for (int xx = 0; xx < cells.Count; ++xx)
                {
                    var cell = cells[xx];
                    bw.Write((short)cell.cellindex.y);
                }
            }
        }
    }
}
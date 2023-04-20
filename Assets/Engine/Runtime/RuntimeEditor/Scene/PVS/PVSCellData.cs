#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    public enum PVSCellSize
    {
        Cell1x1 = 1,
        Cell2x2 = 2,
        Cell4x4 = 4,
    }

    public enum PVSOcclude
    {
        PVSOccluder,
        PVSOccludee,
        PVSNone,
    }

    [System.Serializable]
    public class PVSCellData
    {
        public Vector3Int cellindex;
        public uint[] visibleMask;
    }

    [System.Serializable]
    public class PVSChunkData
    {
        public int x;
        public int z;
        [NonSerialized]
        public int startOffset;
        [NonSerialized]
        public int length;
        public List<PVSCellData> cells = new List<PVSCellData>();
    }

}
#endif
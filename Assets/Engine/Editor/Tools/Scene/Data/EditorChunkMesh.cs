#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
namespace CFEngine.Editor
{
    [Serializable]
    public class EditorChunkMesh : ScriptableObject
    {
        public List<Mesh> editTerrainMesh = new List<Mesh>();
    }
}
#endif
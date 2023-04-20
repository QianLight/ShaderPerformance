#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine
{

    public class SceneConfig : ScriptableObject
    {
        [System.Serializable]
        public class TextureInfo
        {
            public Texture2D tex;
            public Texture2D pbs;
            public byte scale;
        }

        [System.Serializable]
        public class TerrainChunk
        {
            //public Vector3 pos;
            public Mesh mesh;
            public Bounds aabb0;
            public Bounds aabb1;
            public Bounds aabb2;
            public Bounds aabb3;
            public TerrainObjData terrainObjData = new TerrainObjData ();
            public byte[] splatID = new byte[4];
            public TerrainChunk ()
            {
                splatID[0] = 0;
                splatID[1] = 255;
                splatID[2] = 255;
                splatID[3] = 255;
            }
        }

        public List<TerrainChunk> chunks = new List<TerrainChunk> ();
        public List<TextureInfo> bundles = new List<TextureInfo> ();
        public Vector4 terrainParam = new Vector4 (1, 0.1f, 1, 0);
        public Mesh baseMapMesh;
        public Texture2D baseMapTex;
        public int widthCount = 0;
        public int heightCount = 0;
        public int blendErrorCount = 10;
        public int mergeCountThreshold = 3;
        public float mergeSizeThreshold = 10;
        public float mergeSizePercentThreshold = 0.5f;
        public bool useDistCull = false;
        
        public LodSize lodNearSize = new LodSize () { size = 5, dist = 32, fade = 64 };
        public LodSize lodFarSize = new LodSize () { size = 20, dist = 64, fade = 128 };
        public float lodHeight = 10;
        public List<string> sceneEditorTag = new List<string> ();
        public PVSCellSize cellSize = PVSCellSize.Cell2x2;
    }
}
#endif
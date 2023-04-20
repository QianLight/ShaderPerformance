using System;
using System.Collections.Generic;
using UnityEngine;
using CFEngine;
namespace CFEngine.Editor
{
    public class SceneEditContext
    {
        public SceneConfig sc;
        public SceneConfigData localConfig;
        public SceneSerializeContext ssc;
    }

    public class SceneConfigData : ScriptableObject
    {
        public string editTag = "MainScene";
        public Color terrainGridColor = Color.red;
        public float terrainMergeRange = 1.0f;
        public Vector2Int chunkSrcPoint = new Vector2Int (-1, -1);
        public Vector2Int chunkEndPoint = new Vector2Int (-1, -1);
        // public int lightmapResolution = 1;
        #region lightmap
        public bool clearLightmapAfterBake = false;
        #endregion

        public int chunkLodScale = 2;
        public bool drawObjBox = false;
        public bool drawQuadTreeBox = false;
        public bool drawRootBox = false;
        public bool drawChunkBox = true;
        // public bool drawLodLevelBox = false;
        public bool sceneInfoFolder = true;
        public bool commonOpFolder = true;
        public bool lodOpFolder = true;
        public bool terrainFolder = true;
        public bool mergeFolder = true;
        public bool previewFolder = true;
        public bool bakeFolder = true;
        // public bool prebakeFolder = true;
        public bool systemFolder = true;
        public bool processSelect = false;
        public bool debugHeadData = false;

        // public bool bakeSelect = false;
        public int debugChunkIndex = -1;
        public int editLightmapConfig = -1;

        #region PVS
        public bool pvsFolder = false;       
        public int sampleNum = 100;
        #endregion
        #region LightProbe
        public bool lightProbeFolder = false;
        #endregion
    }
}
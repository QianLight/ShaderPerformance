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
    public partial class TerrainSystem : SceneResProcess
    {
        enum OpType
        {
            OpNone,
            OpInitScene,
            OpConvert,
            OpRefreshTerrainSplat,
        }

        enum TerrainConvertFlag
        {
            None,
            HeightMap = 0x00000001,
            AlphaMap = 0x00000002,
        }
        class TerrainMeshQuadInfo
        {
            public int x = 0;
            public int z = 0;

            public bool canMerge = false;

            public int index = 0;
            public int index0 = 0;
            public int index1 = 0;
            public int index2 = 0;
            public int index3 = 0;
            public int id = -1;
            public bool start = false;
            public Vector3 normal;
        }

        class TerrainMeshBlockInfo
        {
            public int startX;
            public int endX;
            public uint key = uint.MaxValue;
            public List<int> z = new List<int>();
        }

        class TerrainMeshLineContext
        {
            public int lineIndex;
            public List<TerrainMeshQuadInfo> quadInfoList = new List<TerrainMeshQuadInfo>();
        }

        class TerrainMeshContext
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> lodvertices = new List<Vector3>();
            public List<Vector2> uv0s = new List<Vector2>();
            public List<int> indexes = new List<int>();
            public List<int> subIndexes = new List<int>();
            public List<Vector3> normals = new List<Vector3>();
            public List<Vector3> lodnormals = new List<Vector3>();
            public List<Vector4> tangents = new List<Vector4>();
            public List<Vector4> lodtangents = new List<Vector4>();
            public List<List<Vector3>> normalList = new List<List<Vector3>>();
            public List<Vector4Int> vertexParam = new List<Vector4Int>();
            public List<int> paramIndexRef = new List<int>();
            public List<Vector3> tangentXYZ = new List<Vector3>();
            public List<Vector3> tangentW = new List<Vector3>();

            public List<Vector3> projMeshVerts = new List<Vector3>();
            public List<int> projMeshIdxs = new List<int>();
            public List<Vector2> projMeshuv0s = new List<Vector2>();
            public List<Vector3> projMeshNormals = new List<Vector3>();
            public List<Vector4> projMeshTangents = new List<Vector4>();
            public List<int> projMeshBoxIdxs = new List<int>();

            public List<TerrainMeshQuadInfo> meshQuads = new List<TerrainMeshQuadInfo>();
            public Dictionary<int, List<TerrainMeshLineContext>> quadCollection = new Dictionary<int, List<TerrainMeshLineContext>>();
            public Dictionary<int, List<TerrainMeshBlockInfo>> mergeBlock = new Dictionary<int, List<TerrainMeshBlockInfo>>();

            public int xChunkLineCount;
            public int zChunkLineCount;

            public float xStartPos;
            public float zStartPos;
            public int xLineCount;
            public int zLineCount;
            public float blockSize;
            public float terrainWidth;
            public float terrainHeight;

            public bool removeBottom;
            public bool removeTop;
            public bool removeLeft;
            public bool removeRight;

            public Bounds[] aabb = new Bounds[4];

        }

        class ChunkSplatValue : IComparable
        {
            public int count;
            public int layer;
            public int index;
            public int CompareTo(object obj)
            {
                ChunkSplatValue other = obj as ChunkSplatValue;
                return other.count.CompareTo(count);
            }
        }
        class BackupTerrainInfo
        {
            public string name;
            public string path;

        }

        private int chunkWidth = EngineContext.ChunkSize;
        private int chunkHeight = EngineContext.ChunkSize;
        private TerrainMeshContext terrainMeshContext = new TerrainMeshContext();
        private List<BackupTerrainInfo> terrainBackupInfo = new List<BackupTerrainInfo>();
        private bool previewMeshTerrain = false;


        private GameObject unityTerrainGo;
        private Transform unityTerrainFolder;
        private Material terrainEditMat = null;
        private OpType opType = OpType.OpNone;

        private FlagMask flag;
        


        public override bool HasGUI { get { return true; } }

        public override void InitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            var go = GameObject.Find("UnityTerrain");
            unityTerrainFolder = go != null ? go.transform : null;
            if (go != null && go.transform.childCount > 0)
                unityTerrainGo = go.transform.GetChild(0).gameObject;
            terrainEditMat = new Material(AssetsConfig.instance.TerrainEditMat);

            TerrainObject.globalPbsParam = sceneConfig.terrainParam;
            RefreshTerrainDataBackup(ref sceneContext);
        }
        public override void UnInitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            if (terrainEditMat != null)
            {
                UnityEngine.Object.DestroyImmediate(terrainEditMat);
                terrainEditMat = null;
            }
        }
        
        public override void OnGUI(ref SceneContext sceneContext, object param, ref Rect rect)
        {
            EditorGUI.indentLevel++;
            var editorContext = param as SceneEditContext;
            var localConfig = editorContext.localConfig;
            var config = editorContext.sc;


            localConfig.sceneInfoFolder = EditorGUILayout.Foldout(localConfig.sceneInfoFolder, "TerrainInfo");
            if (localConfig.sceneInfoFolder)
            {
                EditorCommon.BeginGroup("Scene");

                GUILayout.BeginHorizontal();
                config.widthCount = EditorGUILayout.IntSlider("WidthCount", config.widthCount, 1, 32);
                EditorGUILayout.IntField(config.widthCount * EngineContext.ChunkSize, GUILayout.MaxWidth(80));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                config.heightCount = EditorGUILayout.IntSlider("HeightCount", config.heightCount, 1, 32);
                EditorGUILayout.IntField(config.heightCount * EngineContext.ChunkSize, GUILayout.MaxWidth(80));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(unityTerrainGo, typeof(GameObject), true, GUILayout.MaxWidth(260));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Init Scene", GUILayout.MaxWidth(160)))
                {
                    opType = OpType.OpInitScene;
                }
                GUILayout.EndHorizontal();
                EditorCommon.EndGroup();
            }
            

            localConfig.terrainFolder = EditorGUILayout.Foldout(localConfig.terrainFolder, "Terrain");
            if (editorContext.localConfig.terrainFolder)
            { 
                EditorCommon.BeginGroup("Terrain Misc");
                string previewMeshTerrainStr = previewMeshTerrain ? "UnityTerrain" : "MeshTerrain";
                if (GUILayout.Button(previewMeshTerrainStr, GUILayout.MaxWidth(160)))
                {
                    previewMeshTerrain = !previewMeshTerrain;
                    PreviewMeshTerrain();
                }
                EditorCommon.EndGroup(false);
                EditorCommon.BeginGroup("Terrain Convert", false);

                GUILayout.BeginHorizontal();
                bool heightMap = GUILayout.Toggle(flag.HasFlag((uint)TerrainConvertFlag.HeightMap),
                    TerrainConvertFlag.HeightMap.ToString(), GUILayout.MaxWidth(100));
                flag.SetFlag((uint)TerrainConvertFlag.HeightMap, heightMap);
                bool alphaMap = GUILayout.Toggle(flag.HasFlag((uint)TerrainConvertFlag.AlphaMap),
                    TerrainConvertFlag.AlphaMap.ToString(), GUILayout.MaxWidth(100));
                flag.SetFlag((uint)TerrainConvertFlag.AlphaMap, alphaMap);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                float chunkLod = localConfig.chunkLodScale;
                chunkLod = GUILayout.HorizontalSlider(chunkLod, 1, 4, GUILayout.MaxWidth(260));
                localConfig.chunkLodScale = (int)chunkLod;
                GUILayout.Label(string.Format("Terrain Lod Scale:{0}", localConfig.chunkLodScale), GUILayout.MaxWidth(260));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                config.blendErrorCount = (int)GUILayout.HorizontalSlider(config.blendErrorCount, 0, 50f, GUILayout.MaxWidth(260));
                GUILayout.Label(string.Format("Terrain Blend Error Count:{0}", config.blendErrorCount), GUILayout.MaxWidth(260));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Convert", GUILayout.MaxWidth(160)))
                {
                    opType = OpType.OpConvert;
                }
                GUILayout.EndHorizontal();

                EditorCommon.EndGroup();
                EditorCommon.BeginGroup("Terrain Info");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(160)))
                {
                    opType = OpType.OpRefreshTerrainSplat;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(config.baseMapMesh, typeof(Mesh), false);
                EditorGUILayout.ObjectField(config.baseMapTex, typeof(Texture2D), false);
                GUILayout.EndHorizontal();
                var bundles = config.bundles;
                for (int i = 0; i < bundles.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    SceneConfig.TextureInfo ti = bundles[i];
                    ti.tex = (Texture2D)EditorGUILayout.ObjectField(ti.tex, typeof(Texture2D), false);
                    ti.pbs = (Texture2D)EditorGUILayout.ObjectField(ti.pbs, typeof(Texture2D), false);
                    ti.scale = (byte)EditorGUILayout.IntField("Scale", ti.scale);
                    GUILayout.EndHorizontal();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.BeginHorizontal();
                config.terrainParam.x = EditorGUILayout.Slider("SpecScale", config.terrainParam.x, 0.01f, 5);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                config.terrainParam.y = EditorGUILayout.Slider("BlendThreshold", config.terrainParam.y, 0.01f, 0.5f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                config.terrainParam.z = EditorGUILayout.Slider("IBLScale", config.terrainParam.z, 0.1f, 2f);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                config.terrainParam.w = EditorGUILayout.Slider("NormalScale", config.terrainParam.w, 0.1f, 4f);
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    TerrainObject.globalPbsParam = config.terrainParam;
                    if (GlobalContex.ee != null)
                    {
                        GlobalContex.ee.UpdateMatObject();
                    }
                }

                EditorCommon.EndGroup();
                EditorCommon.BeginGroup("Backup TerrainData");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Backup", GUILayout.MaxWidth(160)))
                {
                    CreateBackup(ref sceneContext, null);
                }
                if (GUILayout.Button("ClearAll", GUILayout.MaxWidth(160)))
                {
                    for (int i = 0; i < terrainBackupInfo.Count; ++i)
                    {
                        BackupTerrainInfo bti = terrainBackupInfo[i];
                        AssetDatabase.DeleteAsset(bti.path);
                    }
                    terrainBackupInfo.Clear();
                }
                GUILayout.EndHorizontal();

                for (int i = 0; i < terrainBackupInfo.Count; ++i)
                {
                    BackupTerrainInfo bti = terrainBackupInfo[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(bti.name, GUILayout.MaxWidth(400));

                    if (GUILayout.Button("Recover", GUILayout.MaxWidth(160)))
                    {
                        if (EditorUtility.DisplayDialog("Recover", "Is Recover TerrainData ", "OK", "Cancel"))
                        {
                            RecoverBackup(ref sceneContext, bti);
                            break;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorCommon.EndGroup();
            }

            EditorGUI.indentLevel--;
        }

        public override void Update(ref SceneContext sceneContext, object param)
        {
            switch (opType)
            {
                case OpType.OpInitScene:
                    InitEditScene(ref sceneContext, param as SceneEditContext);
                    break;
                case OpType.OpConvert:
                    Convert(ref sceneContext, param);
                    break;
            }
            opType = OpType.OpNone;
        }

        #region create
        private void InitTerrain(ref SceneContext sceneContext, GameObject srcTerrainGo,TerrainData td, string tagName, Transform editorTerrainTrans)
        {
            if (!string.IsNullOrEmpty(tagName))
            {
                bool createTerrainPrefab = false;
                if (EditorCommon.IsPrefabOrFbx(srcTerrainGo))
                {
                    string terrainName = srcTerrainGo.name;
                    if (terrainName.EndsWith(tagName))
                    {
                        //ok
                        srcTerrainGo.transform.parent = editorTerrainTrans;
                        return;
                    }
                    else
                    {
                        createTerrainPrefab = true;
                    }
                }
                else
                {
                    //init
                    createTerrainPrefab = true;
                }

                if (createTerrainPrefab)
                {
                    string name = string.Format("{0}_{1}", sceneContext.name, tagName);

                    string targetTerrainDataPath = string.Format("{0}/{1}.asset", sceneContext.terrainDir, name);
                    TerrainData targetTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(targetTerrainDataPath);
                    if (targetTerrainData == null)
                    {
                        string srcPath = AssetDatabase.GetAssetPath(td);
                        AssetDatabase.CopyAsset(srcPath, targetTerrainDataPath);
                        targetTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(targetTerrainDataPath);
                        targetTerrainData.name = name;
                    }
                    GameObject go = Terrain.CreateTerrainGameObject(targetTerrainData);
                    go.name = name;
                    string targetTerrainGoPath = string.Format("{0}/{1}.prefab", sceneContext.terrainDir, name);
                    GameObject prefab = PrefabAssets.SavePrefab(targetTerrainGoPath, go);
                    GameObject newTerrainGo = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    GameObject.DestroyImmediate(go);
                    GameObject.DestroyImmediate(srcTerrainGo);
                    newTerrainGo.transform.parent = editorTerrainTrans;
                    newTerrainGo.layer = DefaultGameObjectLayer.TerrainLayer;
                }
            }
        }
        private static void CreateTerrainChunkGo(GameObject chunks, string meshName, Vector3 pos, int index, Material mat)
        {
            //if (chunk.mesh != null)
            {
                Transform terrainmeshTrans = chunks.transform.Find(meshName);
                GameObject terrainmeshGo = null;
                if (terrainmeshTrans == null)
                    terrainmeshGo = new GameObject(meshName);
                else
                    terrainmeshGo = terrainmeshTrans.gameObject;
                terrainmeshGo.transform.parent = chunks.transform;
                terrainmeshGo.transform.position = pos;
                terrainmeshGo.layer = DefaultGameObjectLayer.TerrainLayer;

                if (GameObjectUtility.GetStaticEditorFlags(terrainmeshGo) == 0)
                GameObjectUtility.SetStaticEditorFlags(terrainmeshGo, StaticEditorFlags.ContributeGI);
                

                MeshFilter mf = terrainmeshGo.GetComponent<MeshFilter>();
                if (mf == null)
                    mf = terrainmeshGo.AddComponent<MeshFilter>();

                MeshRenderer mr = terrainmeshGo.GetComponent<MeshRenderer>();
                if (mr == null)
                    mr = terrainmeshGo.AddComponent<MeshRenderer>();
                mr.enabled = false;
                TerrainObject to = terrainmeshGo.GetComponent<TerrainObject>();
                if (to == null)
                    to = terrainmeshGo.AddComponent<TerrainObject>();

                to.chunkID = index;
                // to.RefreshRender ();

                CommonAssets.BakeRender(mr);
                // mf.sharedMesh = chunk.mesh;
                mr.sharedMaterial = mat;
                mr.enabled = false;
            }
        }
        private void InitChunks(ref SceneContext sceneContext, SceneConfig sceneConfig, 
            int widthCount, int heightCount, Transform meshTerrainTrans)
        {
            string resSceneDir = string.Format("{0}/Scene", AssetsConfig.instance.ResourcePath);
            SceneAssets.CreateFolder(AssetsConfig.instance.ResourcePath, "Scene");
            SceneAssets.CreateFolder(resSceneDir, sceneContext.name);

            int chunkCount = widthCount * heightCount;
            if (sceneConfig.chunks.Count != chunkCount)
            {
                sceneConfig.chunks.Clear();
                for (int i = 0; i < chunkCount; ++i)
                {
                    sceneConfig.chunks.Add(new SceneConfig.TerrainChunk());
                }
            }
            string indexStr = "";
            Material terrainChunkMat = AssetsConfig.instance.TerrainPreviewMat[0];
            GameObject meshTerrainChunks = meshTerrainTrans.gameObject; 

            {
                EditorCommon.DestroyChildObjects(meshTerrainChunks);
            }

            for (int z = 0; z < heightCount; ++z)
            {
                for (int x = 0; x < widthCount; ++x)
                {
                    int i = z * widthCount + x;
                    SceneConfig.TerrainChunk chunk = sceneConfig.chunks[i];
                    if (chunk == null)
                    {
                        chunk = new SceneConfig.TerrainChunk();
                        sceneConfig.chunks[i] = chunk;
                    }
                    indexStr = string.Format("{0}_{1}", x, z);

                    Vector3 pos = new Vector3(x * chunkWidth, 0, z * chunkHeight);
                    string goName = string.Format("{0}{1}", AssetsConfig.instance.SceneChunkStr, indexStr);
                    CreateTerrainChunkGo(meshTerrainChunks, goName, pos, i, terrainChunkMat);
                    EditorUtility.DisplayProgressBar(string.Format("CreateChunk-{0}/{1}", i, chunkCount), indexStr, (float)i / chunkCount);
                }
            }

            EditorUtility.ClearProgressBar();
            CommonAssets.SaveAsset(sceneConfig);
        }

        public void CreateEditScene(ref SceneContext sceneContext, SceneSerializeContext ssc)
        {
            var td = GlobalContex.terrainData;
            if (td != null)
            {
                EngineContext context = EngineContext.instance;
                int x = EngineContext.ChunkSize * context.xChunkCount;
                int z = EngineContext.ChunkSize * context.zChunkCount;
                Vector3 size = td.size;
                size.x = x;
                size.z = z;
                td.size = size;

                if (ssc.sceneConfig == null)
                {
                    ssc.sceneConfig = SceneSerialize.CreateSceneConfig(ref sceneContext, context.xChunkCount, context.zChunkCount, SceneContext.MainTagName);
                    InitTerrain(ref sceneContext, GlobalContex.terrainGo,td, SceneContext.MainTagName, unityTerrainFolder);
                    InitChunks(ref sceneContext, ssc.sceneConfig, context.xChunkCount, context.zChunkCount, resData.workspace);
                }
            }
            else
            {
                DebugLog.AddErrorLog("no terrain,create terrain first");
            }

            
        }


        public void InitEditScene(ref SceneContext sceneContext, SceneEditContext sec)
        {
            var td = GlobalContex.terrainData;
            if (td != null)
            {
                var sc = sec.sc;
                int x = EngineContext.ChunkSize * sc.widthCount;
                int z = EngineContext.ChunkSize * sc.heightCount;
                Vector3 size = td.size;
                size.x = x;
                size.z = z;
                td.size = size;

                EngineContext context = EngineContext.instance;
                context.Width = (int)size.x;
                context.Height = (int)size.z;
                context.xChunkCount = sc.widthCount;
                context.zChunkCount = sc.heightCount;

                var ssContext = sec.ssc;
                ssContext.chunkWidth = EngineContext.ChunkSize;
                ssContext.chunkHeight = EngineContext.ChunkSize;
                ssContext.widthCount = context.xChunkCount;
                ssContext.heightCount = context.zChunkCount;

                InitTerrain(ref sceneContext, GlobalContex.terrainGo, td, SceneContext.MainTagName, unityTerrainFolder);
                InitChunks(ref sceneContext, sc, context.xChunkCount, context.zChunkCount, resData.workspace);

                var go = GameObject.Find("UnityTerrain");
                unityTerrainFolder = go != null ? go.transform : null;
                if (go != null && go.transform.childCount > 0)
                    unityTerrainGo = go.transform.GetChild(0).gameObject;
            }
        }
        #endregion

        #region TerrainData
        private TerrainData LoadTerrainData(ref SceneContext sceneContext)
        {
            string editTerrainName = string.IsNullOrEmpty(sceneContext.suffix) ? sceneContext.name : sceneContext.name + "_" + sceneContext.suffix;
            string editTerrainDataPath = string.Format("{0}/{1}.asset", sceneContext.terrainDir, editTerrainName);
            return AssetDatabase.LoadAssetAtPath<TerrainData>(editTerrainDataPath);
        }
        public Transform GetMeshTerrain()
        {
            if (resData != null)
            {
                return resData.workspace;
            }
            return null;
        }

        public GameObject GetUnityTerrain()
        {
            return unityTerrainGo;
        }

        public T FindMeshTerrain<T>(Transform meshTerrain, string chunkName) where T : Component
        {
            if (meshTerrain != null)
            {
                Transform t = meshTerrain.Find(chunkName);
                if (t != null)
                {
                    return t.GetComponent<T>();
                }
            }
            return null;
        }

        public void PreviewMeshTerrain(bool force = false)
        {
            if(force)
            {
                previewMeshTerrain = true;
            }
            if (unityTerrainGo != null)
            {
                if(unityTerrainGo.TryGetComponent<Terrain>(out var terrain))
                {
                    terrain.enabled = !previewMeshTerrain;
                    if (terrain.enabled)
                    {
                        terrain.materialTemplate = terrainEditMat;
                    }
                }
            }
            Transform meshTerrain = GetMeshTerrain();
            if (meshTerrain != null)
            {                
                for (int i = 0; i < meshTerrain.childCount; ++i)
                {
                    Transform child = meshTerrain.GetChild(i);
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null)
                        r.enabled = previewMeshTerrain;

                }
            }
        }

        private GameObject GetTerrainObject(ref TerrainData td)
        {
            Terrain t = null;
            GameObject go = GetUnityTerrain();
            if (go == null)
            {
                t = Terrain.activeTerrain;
                go = t != null ? t.gameObject : null;
            }
            else
            {
                t = go.GetComponent<Terrain>();
            }
            if (t != null)
            {
                td = t.terrainData;
            }
            return go;
        }

        #endregion

        #region Convert
        private void InitChunkMeshContext(int x, int z, float blockSize)
        {
            terrainMeshContext.blockSize = blockSize;
            terrainMeshContext.xChunkLineCount = (int)((chunkWidth / terrainMeshContext.blockSize) + 1);
            terrainMeshContext.zChunkLineCount = (int)((chunkHeight / terrainMeshContext.blockSize) + 1);

            terrainMeshContext.xStartPos = terrainMeshContext.removeLeft ? (x * chunkWidth - terrainMeshContext.blockSize) : 0;
            terrainMeshContext.zStartPos = terrainMeshContext.removeBottom ? (z * chunkHeight - terrainMeshContext.blockSize) : 0;

            terrainMeshContext.xLineCount = terrainMeshContext.xChunkLineCount;
            terrainMeshContext.zLineCount = terrainMeshContext.zChunkLineCount;
            if (terrainMeshContext.removeBottom)
                terrainMeshContext.zLineCount++;
            if (terrainMeshContext.removeTop)
                terrainMeshContext.zLineCount++;

            if (terrainMeshContext.removeLeft)
                terrainMeshContext.xLineCount++;
            if (terrainMeshContext.removeRight)
                terrainMeshContext.xLineCount++;
        }
        private void CreateChunkMesh(Terrain terrain, TerrainMeshContext tmc)
        {
            tmc.vertices.Clear();
            tmc.uv0s.Clear();
            tmc.indexes.Clear();
            tmc.normals.Clear();
            tmc.tangents.Clear();
            tmc.tangentXYZ.Clear();
            tmc.tangentW.Clear();
            tmc.normalList.Clear();
            int u = tmc.xChunkLineCount - 1;
            int v = tmc.zChunkLineCount - 1;
            for (int z = 0; z < tmc.zLineCount; ++z)
            {
                for (int x = 0; x < tmc.xLineCount; ++x)
                {
                    int index = tmc.vertices.Count;
                    Vector3 pos = new Vector3(tmc.xStartPos + x * tmc.blockSize, 0, tmc.zStartPos + z * tmc.blockSize);
                    pos.y = terrain.SampleHeight(pos);
                    tmc.uv0s.Add(new Vector2(Mathf.Clamp01(pos.x / u), Mathf.Clamp01(pos.z / v)));
                    tmc.vertices.Add(pos);
                    if (x > 0 && z < tmc.zLineCount - 1)
                    {
                        tmc.indexes.Add(index);
                        tmc.indexes.Add(index - 1);
                        tmc.indexes.Add((z + 1) * tmc.xLineCount + x);

                        tmc.indexes.Add(index - 1);
                        tmc.indexes.Add((z + 1) * tmc.xLineCount + x - 1);
                        tmc.indexes.Add((z + 1) * tmc.xLineCount + x);
                    }
                }
            }

            //calc normal
            for (int i = 0; i < tmc.vertices.Count; i++)
            {
                tmc.normalList.Add(new List<Vector3>());
            }

            for (int j = 0; j < tmc.indexes.Count; j += 3)
            {
                Vector3 val = tmc.vertices[tmc.indexes[j]];
                Vector3 val2 = tmc.vertices[tmc.indexes[j + 1]];
                Vector3 val3 = tmc.vertices[tmc.indexes[j + 2]];
                Vector3 val4 = val2 - val;
                Vector3 val5 = val3 - val;
                Vector3 item = Vector3.Cross(val4, val5);
                item.Normalize();
                tmc.normalList[tmc.indexes[j]].Add(item);
                tmc.normalList[tmc.indexes[j + 1]].Add(item);
                tmc.normalList[tmc.indexes[j + 2]].Add(item);
            }
            for (int k = 0; k < tmc.vertices.Count; k++)
            {
                Vector3 val6 = Vector3.zero;
                for (int l = 0; l < tmc.normalList[k].Count; l++)
                {
                    val6 += tmc.normalList[k][l];
                }
                tmc.normals.Add(val6 / (float)tmc.normalList[k].Count);
            }

            //calc tangent
            for (int i = 0; i < tmc.vertices.Count; i++)
            {
                tmc.tangentXYZ.Add(Vector3.zero);
                tmc.tangentW.Add(Vector3.zero);
            }
            for (int j = 0; j < tmc.indexes.Count; j += 3)
            {
                int index0 = tmc.indexes[j];
                int index1 = tmc.indexes[j + 1];
                int index2 = tmc.indexes[j + 2];
                Vector3 pos0 = tmc.vertices[index0];
                Vector3 pos1 = tmc.vertices[index1];
                Vector3 pos2 = tmc.vertices[index2];

                Vector2 uv0 = tmc.uv0s[index0];
                Vector2 uv1 = tmc.uv0s[index1];
                Vector2 uv2 = tmc.uv0s[index2];

                float detlaX0 = pos1.x - pos0.x;
                float detlaX1 = pos2.x - pos0.x;
                float detlaY0 = pos1.y - pos0.y;
                float detlaY1 = pos2.y - pos0.y;
                float detlaZ0 = pos1.z - pos0.z;
                float detlaZ1 = pos2.z - pos0.z;

                float detlaU0 = uv1.x - uv0.x;
                float detlaU1 = uv2.x - uv0.x;
                float detlaV0 = uv1.y - uv0.y;
                float detlaV1 = uv2.y - uv0.y;
                float delta = 0.0001f;

                if (detlaU0 * detlaV1 - detlaU1 * detlaV0 != 0f)
                {
                    delta = 1f / (detlaU0 * detlaV1 - detlaU1 * detlaV0);
                }

                Vector3 tangentXYZ = new Vector3(
                    (detlaV1 * detlaX0 - detlaV0 * detlaX1) * delta,
                    (detlaV1 * detlaY0 - detlaV0 * detlaY1) * delta,
                    (detlaV1 * detlaZ0 - detlaV0 * detlaZ1) * delta);
                Vector3 tangentW = new Vector3(
                    (detlaU0 * detlaX1 - detlaU1 * detlaX0) * delta,
                    (detlaU0 * detlaY1 - detlaU1 * detlaY0) * delta,
                    (detlaU0 * detlaZ1 - detlaU1 * detlaZ0) * delta);

                Vector3 XYZ0 = tmc.tangentXYZ[index0];
                Vector3 XYZ1 = tmc.tangentXYZ[index1];
                Vector3 XYZ2 = tmc.tangentXYZ[index2];
                tmc.tangentXYZ[index0] = XYZ0 + tangentXYZ;
                tmc.tangentXYZ[index1] = XYZ1 + tangentXYZ;
                tmc.tangentXYZ[index2] = XYZ2 + tangentXYZ;

                Vector3 W0 = tmc.tangentW[index0];
                Vector3 W1 = tmc.tangentW[index1];
                Vector3 W2 = tmc.tangentW[index2];
                tmc.tangentW[index0] = W0 + tangentW;
                tmc.tangentW[index1] = W1 + tangentW;
                tmc.tangentW[index2] = W2 + tangentW;
            }
            for (int j = 0; j < tmc.vertices.Count; j++)
            {
                Vector3 normal = tmc.normals[j];
                Vector3 tangentXYZ = tmc.tangentXYZ[j];
                Vector3.OrthoNormalize(ref normal, ref tangentXYZ);
                Vector4 tangent = new Vector4(tangentXYZ.x, tangentXYZ.y, tangentXYZ.z, 1);
                tangent.w = (Vector3.Dot(Vector3.Cross(normal, tangentXYZ), tmc.tangentW[j]) < 0f) ? -1f : 1f;
                tmc.tangents.Add(tangent);
            }

            //remove vertex
            int startLineIndex = tmc.removeTop ? tmc.zLineCount - 2 : tmc.zLineCount - 1;
            int endLineIndex = tmc.removeBottom ? 1 : 0;
            if (tmc.removeTop)
            {
                int index = tmc.vertices.Count - tmc.xLineCount;
                tmc.vertices.RemoveRange(index, tmc.xLineCount);
                tmc.normals.RemoveRange(index, tmc.xLineCount);
                tmc.tangents.RemoveRange(index, tmc.xLineCount);
            }
            if (tmc.removeLeft && tmc.removeRight)
            {
                for (int z = startLineIndex; z >= endLineIndex; --z)
                {
                    int right = z * tmc.xLineCount + tmc.xLineCount - 1;
                    int left = z * tmc.xLineCount;
                    tmc.vertices.RemoveAt(right);
                    tmc.normals.RemoveAt(right);
                    tmc.tangents.RemoveAt(right);

                    tmc.vertices.RemoveAt(left);
                    tmc.normals.RemoveAt(left);
                    tmc.tangents.RemoveAt(left);
                }
            }
            else if (tmc.removeLeft)
            {
                for (int z = startLineIndex; z >= endLineIndex; --z)
                {
                    int left = z * tmc.xLineCount;

                    tmc.vertices.RemoveAt(left);
                    tmc.normals.RemoveAt(left);
                    tmc.tangents.RemoveAt(left);
                }
            }
            else if (tmc.removeRight)
            {
                for (int z = startLineIndex; z >= endLineIndex; --z)
                {
                    int right = z * tmc.xLineCount + tmc.xLineCount - 1;
                    tmc.vertices.RemoveAt(right);
                    tmc.normals.RemoveAt(right);
                    tmc.tangents.RemoveAt(right);
                }
            }
            if (tmc.removeBottom)
            {
                tmc.vertices.RemoveRange(0, tmc.xLineCount);
                tmc.normals.RemoveRange(0, tmc.xLineCount);
                tmc.tangents.RemoveRange(0, tmc.xLineCount);
            }

            if (tmc.removeLeft)
            {
                tmc.xLineCount--;
            }
            if (tmc.removeRight)
            {
                tmc.xLineCount--;
            }
            if (tmc.removeTop)
            {
                tmc.zLineCount--;
            }
            if (tmc.removeBottom)
            {
                tmc.zLineCount--;
            }
        }
        private int InInHeightLodRange(int x, int z, int startX, int endX, int startZ, int endZ, int scale)
        {
            if (x >= startX && x <= endX &&
                z >= startZ && z <= endZ)
            {
                return 1;
            }
            startX -= scale;
            endX += scale;
            startZ -= scale;
            endZ += scale;
            if (x >= startX && x <= endX &&
                z >= startZ && z <= endZ)
            {
                return 2;
            }

            return 0;
        }
        private void CreateMeshLod(TerrainMeshContext tmc)
        {
            int u = tmc.xChunkLineCount - 1;
            int v = tmc.zChunkLineCount - 1;
            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int index = x + z * tmc.xChunkLineCount;
                    Vector3 pos = tmc.vertices[index];
                    pos.x = x * tmc.blockSize;
                    pos.z = z * tmc.blockSize;
                    tmc.lodvertices.Add(pos);
                    Vector2 uv = new Vector2(Mathf.Clamp01((float)x / u), Mathf.Clamp01((float)z / v));
                    if (x == 0 || x == tmc.xChunkLineCount - 1 || z == 0 || z == tmc.zChunkLineCount - 1)
                    {
                        uv.x += 100;
                        uv.y += 100;
                    }
                    tmc.uv0s.Add(uv);
                    tmc.lodnormals.Add(tmc.normals[index]);
                    tmc.lodtangents.Add(tmc.tangents[index]);

                }
            }

        }
        private void CreateMeshLod0(TerrainMeshContext tmc, int quadrant, int halfX, int halfZ, int scale)
        {
            tmc.vertexParam.Clear();
            tmc.paramIndexRef.Clear();

            int startX = (quadrant % 2) * halfX;
            int endX = startX + halfX;
            int startZ = (quadrant / 2) * halfZ;
            int endZ = startZ + halfZ;

            //index pass
            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int rangeType = InInHeightLodRange(x, z, startX, endX, startZ, endZ, scale);
                    int index = x + z * tmc.xChunkLineCount;
                    tmc.vertexParam.Add(new Vector4Int(x, z, rangeType, index));

                }
            }
            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int index = x + z * tmc.xChunkLineCount;
                    Vector4Int param = tmc.vertexParam[index];
                    if (x > 0 && z < tmc.zChunkLineCount - 1 && param.z == 1)
                    {
                        Vector4Int leftParam = tmc.vertexParam[index - 1];
                        Vector4Int upParam = tmc.vertexParam[(z + 1) * tmc.xChunkLineCount + x];
                        Vector4Int upLeftParam = tmc.vertexParam[(z + 1) * tmc.xChunkLineCount + x - 1];
                        if (leftParam.z == 1 && upParam.z == 1)
                        {
                            tmc.indexes.Add(param.w);
                            tmc.indexes.Add(leftParam.w);
                            tmc.indexes.Add(upParam.w);

                            tmc.indexes.Add(leftParam.w);
                            tmc.indexes.Add(upLeftParam.w);
                            tmc.indexes.Add(upParam.w);
                        }
                    }
                }
            }
        }
        private void CreateMeshLod1(TerrainMeshContext tmc, int halfX, int halfZ, int scale)
        {
            tmc.vertexParam.Clear();
            tmc.paramIndexRef.Clear();
            //index pass

            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int index = x + z * tmc.xChunkLineCount;
                    if (x % scale == 0 && z % scale == 0)
                    {
                        int rangeType = 0;
                        // if (x <= scale && tmc.removeLeft ||
                        //     (x == tmc.xChunkLineCount - 1) && tmc.removeRight ||
                        //     z == 0 && tmc.removeBottom ||
                        //     (z >= tmc.zChunkLineCount - 1 - scale) && tmc.removeTop ||
                        //     x == halfX || z == halfZ)
                        if (x == scale && (z < tmc.zChunkLineCount - 1) ||
                            (x == tmc.xChunkLineCount - 1) && (z < tmc.zChunkLineCount - 1) ||
                            z == 0 ||
                            (z == tmc.zChunkLineCount - 1 - scale) ||
                            x == halfX || z == halfZ)
                            rangeType = 2;

                        tmc.vertexParam.Add(new Vector4Int(x, z, rangeType, index));
                    }
                    else
                    {
                        tmc.vertexParam.Add(new Vector4Int(-1, 0, 0, index));
                    }

                }
            }
            int centerRightX = halfX + scale;
            int centerBottomZ = halfZ - scale;
            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int index = x + z * tmc.xChunkLineCount;
                    Vector4Int param = tmc.vertexParam[index];
                    if (param.x >= 0 && x > 0)
                    {
                        if (param.z == 0)
                        {
                            if (x != centerRightX &&
                                z != centerBottomZ &&
                                z < tmc.xChunkLineCount - 1)
                            {
                                Vector4Int leftParam = tmc.vertexParam[index - scale];
                                Vector4Int upParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                Vector4Int upLeftParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upParam.w);

                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upParam.w);

                            }
                        }
                        else if (param.z == 2)
                        {
                            if (x == scale && (z == 0 || z == tmc.zChunkLineCount - 1 - scale) ||
                                x == tmc.xChunkLineCount - 1 && (z == 0 || z == tmc.zChunkLineCount - 1 - scale))
                            {

                                Vector4Int v0 = tmc.vertexParam[index - scale];
                                Vector4Int v1 = tmc.vertexParam[index - scale / 2];
                                Vector4Int v3 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v4 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v5 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];
                                Vector4Int v6 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v7 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v8 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v1.w);

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v5.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v7.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v8.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v8.w);

                            }
                            else if (x == scale)
                            {

                                Vector4Int v0 = tmc.vertexParam[index - scale];
                                Vector4Int v1 = tmc.vertexParam[index - scale / 2];

                                Vector4Int v3 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v4 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale / 2];

                                Vector4Int v6 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v7 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v8 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v1.w);

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v7.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v8.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v8.w);

                            }
                            else if (x == tmc.xChunkLineCount - 1)
                            {
                                Vector4Int v0 = tmc.vertexParam[index - scale];
                                Vector4Int v1 = tmc.vertexParam[index - scale / 2];

                                Vector4Int v4 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v5 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];

                                Vector4Int v6 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v7 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v8 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v1.w);

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v7.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v5.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v8.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v8.w);
                            }
                            else if (z == 0)
                            {
                                Vector4Int v0 = tmc.vertexParam[index - scale];
                                Vector4Int v1 = tmc.vertexParam[index - scale / 2];
                                Vector4Int v3 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v4 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v5 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];
                                Vector4Int v6 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v8 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v1.w);

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v5.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(v1.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v8.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v8.w);
                                tmc.indexes.Add(v5.w);
                            }
                            else if (z == tmc.zChunkLineCount - 1 - scale)
                            {
                                Vector4Int v0 = tmc.vertexParam[index - scale];
                                Vector4Int v3 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v4 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v5 = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];
                                Vector4Int v6 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int v7 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int v8 = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v0.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v4.w);

                                tmc.indexes.Add(v3.w);
                                tmc.indexes.Add(v6.w);
                                tmc.indexes.Add(v7.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v8.w);
                                tmc.indexes.Add(v5.w);

                                tmc.indexes.Add(v4.w);
                                tmc.indexes.Add(v7.w);
                                tmc.indexes.Add(v8.w);

                            }
                            else if (x == halfX && z == halfZ)
                            {
                                Vector4Int upParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                Vector4Int bottomParam = tmc.vertexParam[(z - scale) * tmc.xChunkLineCount + x];
                                Vector4Int leftParam = tmc.vertexParam[index - scale];
                                Vector4Int rightParam = tmc.vertexParam[index + scale];

                                Vector4Int upLeftParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int bottomLeftParam = tmc.vertexParam[(z - scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int upRightParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x + scale];
                                Vector4Int bottomRightParam = tmc.vertexParam[(z - scale) * tmc.xChunkLineCount + x + scale];

                                Vector4Int upMidParam = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];
                                Vector4Int bottomMidParam = tmc.vertexParam[(z - scale / 2) * tmc.xChunkLineCount + x];
                                Vector4Int leftMidParam = tmc.vertexParam[z * tmc.xChunkLineCount + x - scale / 2];
                                Vector4Int rightMidParam = tmc.vertexParam[z * tmc.xChunkLineCount + x + scale / 2];

                                //0                            
                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(bottomLeftParam.w);
                                tmc.indexes.Add(leftMidParam.w);

                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(bottomParam.w);
                                tmc.indexes.Add(bottomLeftParam.w);

                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(bottomLeftParam.w);
                                tmc.indexes.Add(leftParam.w);

                                //1
                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(rightMidParam.w);
                                tmc.indexes.Add(bottomRightParam.w);

                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(bottomRightParam.w);
                                tmc.indexes.Add(bottomParam.w);

                                tmc.indexes.Add(bottomRightParam.w);
                                tmc.indexes.Add(rightMidParam.w);
                                tmc.indexes.Add(rightParam.w);

                                tmc.indexes.Add(bottomMidParam.w);
                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(rightMidParam.w);

                                //2
                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upMidParam.w);

                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upLeftParam.w);

                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upParam.w);

                                //3
                                tmc.indexes.Add(rightMidParam.w);
                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upRightParam.w);

                                tmc.indexes.Add(rightMidParam.w);
                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(upMidParam.w);

                                tmc.indexes.Add(rightMidParam.w);
                                tmc.indexes.Add(upRightParam.w);
                                tmc.indexes.Add(rightParam.w);

                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upParam.w);
                                tmc.indexes.Add(upRightParam.w);

                            }
                            else if (x < halfX || x > halfX + scale)
                            {
                                Vector4Int leftParam = tmc.vertexParam[index - scale];
                                Vector4Int upParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                Vector4Int upLeftParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int leftMidParam = tmc.vertexParam[z * tmc.xChunkLineCount + x - scale / 2];

                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(upParam.w);

                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upParam.w);

                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upLeftParam.w);

                                Vector4Int bottomParam = tmc.vertexParam[(z - scale) * tmc.xChunkLineCount + x];
                                Vector4Int bottomLeftParam = tmc.vertexParam[(z - scale) * tmc.xChunkLineCount + x - scale];
                                tmc.indexes.Add(bottomParam.w);
                                tmc.indexes.Add(leftMidParam.w);
                                tmc.indexes.Add(param.w);

                                tmc.indexes.Add(bottomParam.w);
                                tmc.indexes.Add(bottomLeftParam.w);
                                tmc.indexes.Add(leftMidParam.w);

                                tmc.indexes.Add(bottomLeftParam.w);
                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(leftMidParam.w);
                            }
                            else if (z < halfZ - scale || z > halfZ && z < tmc.xChunkLineCount - 1)
                            {
                                Vector4Int leftParam = tmc.vertexParam[index - scale];
                                Vector4Int upParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x];
                                Vector4Int upLeftParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x - scale];
                                Vector4Int upMidParam = tmc.vertexParam[(z + scale / 2) * tmc.xChunkLineCount + x];

                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upMidParam.w);

                                tmc.indexes.Add(leftParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upMidParam.w);

                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upLeftParam.w);
                                tmc.indexes.Add(upParam.w);

                                Vector4Int rightParam = tmc.vertexParam[index + scale];
                                Vector4Int upRightParam = tmc.vertexParam[(z + scale) * tmc.xChunkLineCount + x + scale];

                                tmc.indexes.Add(param.w);
                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(rightParam.w);

                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upRightParam.w);
                                tmc.indexes.Add(rightParam.w);

                                tmc.indexes.Add(upMidParam.w);
                                tmc.indexes.Add(upParam.w);
                                tmc.indexes.Add(upRightParam.w);
                            }
                        }
                    }
                }
            }
        }

        private void CreateChunkSubMesh(TerrainMeshContext tmc,
            ref Vector4Int subIndex)
        {
            int scale = 2;
            int halfX = (tmc.zChunkLineCount - 1) / scale;
            int halfZ = (tmc.zChunkLineCount - 1) / scale;
            tmc.lodvertices.Clear();
            tmc.lodnormals.Clear();
            tmc.lodtangents.Clear();
            tmc.uv0s.Clear();
            tmc.indexes.Clear();
            CreateMeshLod(tmc);
            CreateMeshLod0(tmc, 0, halfX, halfZ, scale);
            subIndex[0] = tmc.indexes.Count;
            CreateMeshLod0(tmc, 1, halfX, halfZ, scale);
            subIndex[1] = tmc.indexes.Count;
            CreateMeshLod0(tmc, 2, halfX, halfZ, scale);
            subIndex[2] = tmc.indexes.Count;
            CreateMeshLod0(tmc, 3, halfX, halfZ, scale);
            subIndex[3] = tmc.indexes.Count;
            CreateMeshLod1(tmc, halfX, halfZ, scale);
        }
        private Mesh CreateChunkLodMesh2(Terrain terrain, string meshDir, string meshName,
           TerrainMeshContext tmc, int xChunk, int zChunk, string editExt, EditorChunkMesh ecm)
        {
            Mesh mesh = new Mesh();
            mesh.name = meshName + editExt;
            InitChunkMeshContext(xChunk, zChunk, originalBlockSize * 0.25f);
            CreateChunkMesh(terrain, tmc);
            tmc.uv0s.Clear();
            tmc.indexes.Clear();

            //protect edge
            int u = tmc.xChunkLineCount - 1;
            int v = tmc.zChunkLineCount - 1;
            Bounds aabb = new Bounds();
            bool init = true;
            for (int z = 0; z < tmc.zChunkLineCount; ++z)
            {
                for (int x = 0; x < tmc.xChunkLineCount; ++x)
                {
                    int index = x + z * tmc.xChunkLineCount;
                    Vector3 pos = tmc.vertices[index];
                    pos.x = x * tmc.blockSize;
                    pos.z = z * tmc.blockSize;

                    if (init)
                    {
                        aabb.center = pos;
                        init = false;
                    }
                    else
                    {
                        aabb.Encapsulate(pos);
                    }
                    tmc.vertices[index] = pos;
                    // pos.z = z * tmc.blockSize;
                    Vector2 uv = new Vector2(Mathf.Clamp01((float)x / u), Mathf.Clamp01((float)z / v));
                    // if (x == 0 || x == tmc.xChunkLineCount - 1 || z == 0 || z == tmc.zChunkLineCount - 1)
                    // {
                    //     uv.x += 100;
                    //     uv.y += 100;
                    // }
                    tmc.uv0s.Add(uv);
                    if (x > 0 && z < tmc.zChunkLineCount - 1)
                    {
                        tmc.indexes.Add(index);
                        tmc.indexes.Add(index - 1);
                        tmc.indexes.Add((z + 1) * tmc.xChunkLineCount + x);

                        tmc.indexes.Add(index - 1);
                        tmc.indexes.Add((z + 1) * tmc.xChunkLineCount + x - 1);
                        tmc.indexes.Add((z + 1) * tmc.xChunkLineCount + x);
                    }
                }
            }
            tmc.aabb[0] = aabb;
            mesh.vertices = tmc.vertices.ToArray();
            mesh.triangles = tmc.indexes.ToArray();
            mesh.uv = tmc.uv0s.ToArray();
            mesh.normals = tmc.normals.ToArray();
            mesh.tangents = tmc.tangents.ToArray();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            Mesh newMesh = mesh;
            if (string.IsNullOrEmpty(editExt) && Directory.Exists(meshDir))
            {
                mesh.UploadMeshData(true);
                MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Medium);
                newMesh = CommonAssets.CreateAsset<Mesh>(meshDir, mesh.name, ".asset", mesh);
            }
            else
            {
                mesh.UploadMeshData(false);
                MeshUtility.SetMeshCompression(mesh, ModelImporterMeshCompression.Off);
                ecm.editTerrainMesh[ecm.editTerrainMesh.Count - 1] = mesh;
                AssetDatabase.AddObjectToAsset(mesh, ecm);
            }

            return newMesh;
        }
        private Mesh CreateChunkLodMesh(Terrain terrain, string meshDir, string meshName, TerrainMeshContext tmc, int x, int z)
        {
            Mesh mesh = new Mesh();
            mesh.name = meshName;
            InitChunkMeshContext(x, z, originalBlockSize * 0.25f);
            CreateChunkMesh(terrain, tmc);

            Vector4Int subIndex = new Vector4Int(0, 0, 0, 0);
            CreateChunkSubMesh(tmc, ref subIndex);

            mesh.vertices = tmc.lodvertices.ToArray();
            mesh.uv = tmc.uv0s.ToArray();
            mesh.normals = tmc.lodnormals.ToArray();
            mesh.tangents = tmc.lodtangents.ToArray();

            mesh.subMeshCount = 5;
            int index = 0;
            for (int i = 0; i < mesh.subMeshCount && i < 4; ++i)
            {
                tmc.subIndexes.Clear();
                bool init = true;
                Bounds aabb = new Bounds();
                for (; index < subIndex[i]; ++index)
                {
                    Vector3 p = tmc.lodvertices[tmc.indexes[index]];
                    if (init)
                    {
                        aabb.center = p;
                        init = false;
                    }
                    else
                    {
                        aabb.Encapsulate(p);
                    }
                    tmc.subIndexes.Add(tmc.indexes[index]);
                }
                tmc.aabb[i] = aabb;
                mesh.SetTriangles(tmc.subIndexes, i, true);
            }
            tmc.subIndexes.Clear();
            for (; index < tmc.indexes.Count; ++index)
            {
                tmc.subIndexes.Add(tmc.indexes[index]);
            }
            mesh.SetTriangles(tmc.subIndexes, 4, true);

            Mesh newMesh = mesh; //LODMaker.MakeLODMesh(mesh, 2.0f, 1, 1, 1, 1, 0, false);
            newMesh.name = mesh.name;
            //recover edge
            Vector2[] uvs = newMesh.uv;
            for (int i = 0; i < uvs.Length; ++i)
            {
                Vector2 uv = uvs[i];
                if (uv.x > 50)
                {
                    uv.x -= 100;
                    uv.y -= 100;
                }
                uvs[i] = uv;
            }
            newMesh.uv = uvs;
            newMesh.RecalculateTangents();
            newMesh.RecalculateBounds();
            newMesh.UploadMeshData(true);
            MeshUtility.SetMeshCompression(newMesh, ModelImporterMeshCompression.Medium);
            newMesh = CommonAssets.CreateAsset<Mesh>(meshDir, meshName, ".asset", newMesh);
            return newMesh;
        }

        private void SyncHeightmap(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {
            GameObject go = GetUnityTerrain();
            Transform meshTerrain = GetMeshTerrain();
            if (go != null)
            {
                Terrain terrain = go.GetComponent<Terrain>();
                if (terrain != null)
                {
                    AssetDatabase.DeleteAsset(string.Format("{0}/editorChunkMesh.asset", sceneContext.terrainDir));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    EditorChunkMesh ecm = EditorChunkMesh.CreateInstance<EditorChunkMesh>();
                    ecm = CommonAssets.CreateAsset<EditorChunkMesh>(sceneContext.terrainDir,
                        "editorChunkMesh", ".asset", ecm);
                    string chunkMeshDir = sceneContext.terrainDir;
                    EngineContext context = EngineContext.instance;

                    terrainMeshContext.terrainWidth = context.xChunkCount * EngineContext.ChunkSize;
                    terrainMeshContext.terrainHeight = context.zChunkCount * EngineContext.ChunkSize;
                    int chunkCount = context.xChunkCount * context.zChunkCount;
                    int processCount = 0;
                    ecm.editTerrainMesh.Clear();

                    for (int z = 0; z < context.zChunkCount; ++z)
                    {
                        for (int x = 0; x < context.xChunkCount; ++x)
                        {
                            ecm.editTerrainMesh.Add(null);
                            int i = z * context.xChunkCount + x;
                            //if (!sceneLocalConfig.processSelect ||
                            //    commonContext.selectChunksMap.ContainsKey(i))
                            {
                                processCount++;
                                SceneConfig.TerrainChunk chunk = sceneConfig.chunks[i];
                                if (chunk != null)
                                {
                                    string chunkName = string.Format("Chunk_{0}_{1}", x, z);
                                    TerrainObject to = FindMeshTerrain<TerrainObject>(meshTerrain, chunkName);
                                    if (to != null && to.terrainObjData.isValid)
                                    {

                                        terrainMeshContext.removeBottom = z != 0;
                                        terrainMeshContext.removeTop = z != (context.zChunkCount - 1);
                                        terrainMeshContext.removeLeft = x != 0;
                                        terrainMeshContext.removeRight = x != (context.xChunkCount - 1);

                                        bool size1x1 = context.zChunkCount == 1 && context.xChunkCount == 1;

                                        Mesh mesh1 = CreateChunkLodMesh2(terrain, chunkMeshDir, chunkName, terrainMeshContext, x, z, "_editor", ecm);
                                        if (size1x1)
                                        {
                                            Mesh newMesh = new Mesh();
                                            newMesh.name = chunkName;
                                            newMesh.vertices = mesh1.vertices;
                                            newMesh.triangles = mesh1.triangles;
                                            newMesh.uv = mesh1.uv;
                                            newMesh.normals = mesh1.normals;
                                            newMesh.tangents = mesh1.tangents;
                                            newMesh.UploadMeshData(true);
                                            MeshUtility.SetMeshCompression(newMesh, ModelImporterMeshCompression.Medium);
                                            newMesh = CommonAssets.CreateAsset<Mesh>(chunkMeshDir, chunkName, ".asset", newMesh);
                                            chunk.mesh = newMesh;
                                        }
                                        else
                                        {
                                            chunk.mesh = CreateChunkLodMesh(terrain, chunkMeshDir, chunkName, terrainMeshContext, x, z);
                                        }

                                        chunk.aabb0 = terrainMeshContext.aabb[0];
                                        chunk.aabb1 = terrainMeshContext.aabb[1];
                                        chunk.aabb2 = terrainMeshContext.aabb[2];
                                        chunk.aabb3 = terrainMeshContext.aabb[3];
                                        MeshFilter mf = FindMeshTerrain<MeshFilter>(meshTerrain, chunkName);
                                        if (mf != null)
                                        {
                                            mf.sharedMesh = mesh1;
                                        }
                                        MeshCollider mc;
                                        if (!to.gameObject.TryGetComponent(out mc))
                                        {
                                            mc = to.gameObject.AddComponent<MeshCollider>();
                                        }
                                        mc.sharedMesh = mf.sharedMesh;
                                    }
                                    EditorUtility.DisplayProgressBar(string.Format("SyncHeightmap-{0}/{1}", i, chunkCount), string.Format("{0}_{1}", x, z), (float)i / chunkCount);
                                }
                            }
                        }
                    }
                    if (processCount == 0)
                    {
                        EditorUtility.DisplayDialog("", "No Chunk Processed!", "OK");
                    }
                    EditorUtility.ClearProgressBar();
                    CommonAssets.SaveAsset(sceneConfig);
                    CommonAssets.SaveAsset(ecm);

                }
            }
        }

        private void LoadAlphaMap(TerrainData terrainData, out Texture2D[] alphaTexs, out int alphmapSizeNoPowOf2, out int alphamapSizeX, out int alphamapSizeZ)
        {
            alphaTexs = null;
            alphamapSizeX = 0;
            alphamapSizeZ = 0;
            alphmapSizeNoPowOf2 = 0;
            if (terrainData != null)
            {
                EngineContext context = EngineContext.instance;
                Texture2D[] alphaMaps = terrainData.alphamapTextures;
                // RenderTexture[] tmpRT = new RenderTexture[alphaMaps.Length];
                alphaTexs = new Texture2D[alphaMaps.Length];
                int alphamapSize = terrainData.alphamapWidth;
                int powerof2 = (int)Mathf.Log(alphamapSize / (float)context.xChunkCount, 2); // + 1;
                alphmapSizeNoPowOf2 = (int)Mathf.Pow(2, powerof2);
                if (alphmapSizeNoPowOf2 * context.xChunkCount < terrainData.alphamapWidth)
                {
                    powerof2 += 1;
                    alphmapSizeNoPowOf2 = (int)Mathf.Pow(2, powerof2);
                }
                alphamapSizeX = alphmapSizeNoPowOf2 * context.xChunkCount;
                alphamapSizeZ = alphmapSizeNoPowOf2 * context.zChunkCount;
                for (int i = 0; i < alphaMaps.Length; ++i)
                {
                    RenderTexture rt = new RenderTexture(alphamapSizeX, alphamapSizeZ, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                    {
                        name = "alphamapScaleRt",
                        hideFlags = HideFlags.DontSave,
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        anisoLevel = 0,
                        autoGenerateMips = false,
                        useMipMap = false
                    };
                    rt.Create();

                    TextureAssets.BlitTex2RT(alphaMaps[i], rt);

                    Texture2D tex = new Texture2D(alphamapSizeX, alphamapSizeZ, TextureFormat.ARGB32, false, true)
                    {
                        name = "alphamapScaleTex",
                        hideFlags = HideFlags.DontSave,
                        anisoLevel = 0,
                        wrapMode = TextureWrapMode.Clamp,
                        filterMode = FilterMode.Bilinear
                    };
                    alphaTexs[i] = tex;
                    // tex.SetPixels(alphaMaps[i].GetPixels(0), 0);
                    TextureAssets.BlitRT2Tex(rt, tex, false);
                    //CommonAssets.CreateAsset<Texture2D>(SceneAssets.scene_subname_directory, "alphamapTex" + i.ToString(), ".png", tex);
                    rt.Release();
                }
            }
        }

        private void RefreshTerrainSplat(TerrainData terrainData, SceneConfig sceneConfig)
        {
            if (terrainData != null)
            {
                TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                var bundles = sceneConfig.bundles;
                if (bundles.Count > terrainLayers.Length)
                {
                    for (int i = bundles.Count - 1; i >= terrainLayers.Length; --i)
                    {
                        bundles.RemoveAt(i);
                    }
                }
                else if (bundles.Count < terrainLayers.Length)
                {
                    for (int i = bundles.Count; i < terrainLayers.Length; ++i)
                    {
                        bundles.Add(new SceneConfig.TextureInfo());
                    }
                }
                for (int i = 0; i < bundles.Count; ++i)
                {
                    SceneConfig.TextureInfo ti = bundles[i];
                    ti.tex = terrainLayers[i].diffuseTexture;
                    ti.scale = (byte)terrainLayers[i].tileSize.x;
                }
            }
        }

        private float GetColor(List<Color[]> texColors, int layerIndex, int pixelOffset)
        {
            int texIndex = layerIndex / 4;
            int colorOffset = layerIndex % 4;
            Color[] colors = texColors[texIndex];
            Color c = colors[pixelOffset];
            return c[colorOffset];

        }
        private void SyncAlphamap(ref SceneContext sceneContext, TerrainData terrainData, SceneConfig sceneConfig, Texture2D[] alphaTexs, int alphmapSizeNoPowOf2, int alphamapSizeX, int alphamapSizeZ)
        {
            if (alphaTexs != null && alphamapSizeX > 0 && alphamapSizeZ > 0)
            {
                RefreshTerrainSplat(terrainData, sceneConfig);
                var bundles = sceneConfig.bundles;
                List<ChunkSplatValue> chunkSplatSort = new List<ChunkSplatValue>();
                ChunkSplatValue[] chunkSplatBlend = new ChunkSplatValue[4];
                Dictionary<int, ChunkSplatValue> layerMap = new Dictionary<int, ChunkSplatValue>();
                int layerCount = terrainData.alphamapLayers;
                string targetSceneDir = sceneContext.terrainDir;
                for (int i = 0; i < layerCount; ++i)
                {
                    chunkSplatSort.Add(new ChunkSplatValue());
                }
                List<Color[]> alpnaColors = new List<Color[]>();

                for (int i = 0; i < alphaTexs.Length; ++i)
                {
                    Texture2D tex = alphaTexs[i];
                    Color[] colors = tex.GetPixels(0);

                    for (int x = alphmapSizeNoPowOf2; x < alphamapSizeX - alphmapSizeNoPowOf2; x += alphmapSizeNoPowOf2)
                    {
                        for (int z = 0; z < alphamapSizeZ; ++z)
                        {
                            int index = x + z * alphamapSizeX;
                            int index0 = index - 1;
                            if ((z + 1) % alphamapSizeZ == 0)
                            {
                                if (z + 1 < alphamapSizeZ)
                                {
                                    int index1 = x + (z + 1) * alphamapSizeX;
                                    int index2 = index1 - 1;
                                    Color c = (colors[index] + colors[index0] + colors[index1] + colors[index2]) * 0.25f;
                                    colors[index] = c;
                                    colors[index0] = c;
                                    colors[index1] = c;
                                    colors[index2] = c;
                                }

                            }
                            else if (z % alphamapSizeZ == 0)
                            {

                            }
                            else
                            {
                                Color c = (colors[index] + colors[index0]) * 0.5f;
                                colors[index] = c;
                                colors[index0] = c;
                            }
                        }
                    }
                    for (int z = alphmapSizeNoPowOf2; z < alphamapSizeZ - alphmapSizeNoPowOf2; z += alphmapSizeNoPowOf2)
                    {
                        for (int x = 0; x < alphamapSizeX; ++x)
                        {

                            if ((x + 1) % alphamapSizeX == 0 || x % alphamapSizeX == 0)
                            {

                            }
                            else
                            {
                                int index = x + z * alphamapSizeX;
                                int index0 = x + (z - 1) * alphamapSizeX;
                                Color c = (colors[index] + colors[index0]) * 0.5f;
                                colors[index] = c;
                                colors[index0] = c;
                            }
                        }
                    }
                    alpnaColors.Add(colors);

                }
                Texture2D chunkTex = new Texture2D(alphmapSizeNoPowOf2, alphmapSizeNoPowOf2, TextureFormat.ARGB32, false, true)
                {
                    name = "chunkBlendTex",
                    hideFlags = HideFlags.DontSave,
                    anisoLevel = 0,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                Transform t = GetMeshTerrain();
                EngineContext context = EngineContext.instance;
                for (int z = 0; z < context.zChunkCount; ++z)
                {
                    for (int x = 0; x < context.xChunkCount; ++x)
                    {
                        int index = z * context.xChunkCount + x;
                        SceneConfig.TerrainChunk chunk = sceneConfig.chunks[index];
                        string chunkName = string.Format("Chunk_{0}_{1}", x, z);
                        Transform chunkGo = t.Find(chunkName);
                        TerrainObject terrainObject = chunkGo != null ? chunkGo.GetComponent<TerrainObject>() : null;
                        if (chunk != null && terrainObject != null && terrainObject.terrainObjData.isValid)
                        {
                            int startX = x * alphmapSizeNoPowOf2;
                            int endX = x * alphmapSizeNoPowOf2 + alphmapSizeNoPowOf2;
                            int startZ = z * alphmapSizeNoPowOf2;
                            int endZ = z * alphmapSizeNoPowOf2 + alphmapSizeNoPowOf2;

                            for (int i = 0; i < chunkSplatSort.Count; ++i)
                            {
                                chunkSplatSort[i].count = 0;
                                chunkSplatSort[i].layer = i;
                                chunkSplatSort[i].index = -1;
                            }
                            //count pixels
                            for (int v = startZ; v < endZ; ++v)
                            {
                                for (int u = startX; u < endX; ++u)
                                {
                                    int colorIndex = u + v * alphamapSizeX;
                                    for (int i = 0; i < layerCount; ++i)
                                    {
                                        if (GetColor(alpnaColors, i, colorIndex) > 0)
                                        {
                                            chunkSplatSort[i].count++;
                                        }
                                    }
                                }
                            }
                            //sort layer pixel
                            for (int i = 0; i < 4; ++i)
                            {
                                chunkSplatBlend[i] = null;
                            }
                            chunkSplatSort.Sort();
                            for (int i = 0; i < chunkSplatSort.Count && i < 4; ++i)
                            {
                                ChunkSplatValue csv = chunkSplatSort[i];
                                if (csv.count > sceneConfig.blendErrorCount)
                                    chunkSplatBlend[i] = csv;
                            }

                            layerMap.Clear();
                            // Vector4 blendMask = new Vector4 ((float) x / widthCount, (float) z / heightCount, widthCount, heightCount);
                            Vector4 blendTexScale = new Vector4(0.125f, 0.125f, 0.125f, 0.125f);

                            terrainObject.terrainObjData.splatCount = 0;
                            for (int i = 0; i < 4; ++i)
                            {
                                terrainObject.terrainObjData.splatTex[i] = null;
                                terrainObject.terrainObjData.pbsTex[i] = null;
                            }
                            int texCount = 0;
                            for (int i = 0; i < chunk.splatID.Length; ++i)
                            {
                                chunk.splatID[i] = 255;
                            }
                            for (int i = 0; i < chunkSplatBlend.Length; ++i)
                            {
                                ChunkSplatValue csv = chunkSplatBlend[i];
                                if (csv != null)
                                {
                                    layerMap.Add(csv.layer, csv);
                                    csv.index = i;
                                    var bundle = bundles[csv.layer];
                                    terrainObject.terrainObjData.splatTex[texCount] = bundle.tex;
                                    terrainObject.terrainObjData.pbsTex[texCount] = bundle.pbs;
                                    terrainObject.terrainObjData.splatCount++;
                                    chunk.splatID[texCount] = (byte)csv.layer;
                                    float scale = bundle.scale;
                                    if (scale == 0)
                                        scale = 0.01f;
                                    blendTexScale[texCount] = 1 / scale;
                                    texCount++;
                                    // if (texCount == 1)
                                    // {
                                    //     terrainObject.terrainObjData.normalTex = splatPrototypes[csv.layer].normalMap;
                                    // }
                                }
                            }
                            terrainObject.terrainObjData.blendTexScale = blendTexScale;
                            terrainObject.GetRenderer();
                            Material mat = terrainObject.render.sharedMaterial;
                            if (texCount > 1)
                            {

                                for (int v = startZ; v < endZ; ++v)
                                {
                                    for (int u = startX; u < endX; ++u)
                                    {
                                        int xOffset = u - startX;
                                        int zOffset = v - startZ;
                                        Color c = Color.clear;
                                        int colorIndex = u + v * alphamapSizeX;

                                        for (int i = 0; i < 4; ++i)
                                        {
                                            ChunkSplatValue csv = chunkSplatBlend[i];
                                            if (csv != null)
                                            {
                                                c[i] = GetColor(alpnaColors, csv.layer, colorIndex);
                                            }
                                        }
                                        chunkTex.SetPixel(xOffset, zOffset, c);
                                    }
                                }
                                chunkTex.Apply();
                                Texture2D blendTex = CommonAssets.CreateAsset<Texture2D>(targetSceneDir, string.Format("Chunk_{0}_{1}_Blend", x, z), ".tga", chunkTex);
                                terrainObject.terrainObjData.blendTex = blendTex;
                            }
                            terrainObject.Refresh(RenderingManager.instance);
                        }
                        chunk.terrainObjData.Copy(terrainObject.terrainObjData);
                    }
                }
                UnityEngine.Object.DestroyImmediate(chunkTex);
                for (int i = 0; i < alphaTexs.Length; ++i)
                {
                    UnityEngine.Object.DestroyImmediate(alphaTexs[i]);
                }
                alphaTexs = null;
            }

        }

        private void Convert(ref SceneContext sceneContext, object param)
        {
            var editorContext = param as SceneEditContext;
            var localConfig = editorContext.localConfig;
            var config = editorContext.sc;
            TerrainData terrainData = LoadTerrainData(ref sceneContext);
            if (terrainData != null)
            {
                if (flag.HasFlag((uint)TerrainConvertFlag.HeightMap))
                {
                    SyncHeightmap(ref sceneContext, config);
                }
                if (flag.HasFlag((uint)TerrainConvertFlag.AlphaMap))
                {
                    Texture2D[] alphaTexs;
                    int alphamapSizeX;
                    int alphamapSizeZ;
                    int alphmapSizeNoPowOf2;
                    LoadAlphaMap(terrainData, out alphaTexs, out alphmapSizeNoPowOf2, out alphamapSizeX, out alphamapSizeZ);
                    SyncAlphamap(ref sceneContext,terrainData, config, alphaTexs, alphmapSizeNoPowOf2, alphamapSizeX, alphamapSizeZ);
                }
                CommonAssets.SaveAsset(config);
            }
        }

        private void ValidTerrain()
        {
            Transform meshTerrain = GetMeshTerrain();
            EngineContext context = EngineContext.instance;
            for (int z = 0; z < context.zChunkCount; ++z)
            {
                for (int x = 0; x < context.xChunkCount; ++x)
                {
                    string chunkName = string.Format("Chunk_{0}_{1}", x, z);
                    TerrainObject to = FindMeshTerrain<TerrainObject>(meshTerrain, chunkName);
                    to.terrainObjData.isValid = true;
                }
            }

        }
        private void InvalidTerrain()
        {
            Transform meshTerrain = GetMeshTerrain();
            EngineContext context = EngineContext.instance;
            for (int z = 0; z < context.zChunkCount; ++z)
            {
                for (int x = 0; x < context.xChunkCount; ++x)
                {
                    string chunkName = string.Format("Chunk_{0}_{1}", x, z);
                    TerrainObject to = FindMeshTerrain<TerrainObject>(meshTerrain, chunkName);
                    to.terrainObjData.isValid = false;
                }
            }

        }

        #endregion

        #region backup

        private void RefreshTerrainDataBackup(ref SceneContext sceneContext)
        {
            DirectoryInfo di = new DirectoryInfo(sceneContext.terrainDir + "/SceneTerrainBackup");
            if (di.Exists)
            {
                FileInfo[] files = di.GetFiles("*.asset", SearchOption.TopDirectoryOnly);
                terrainBackupInfo.Clear();
                for (int i = 0; i < files.Length; ++i)
                {
                    FileInfo fi = files[i];
                    BackupTerrainInfo bti = new BackupTerrainInfo();
                    bti.name = fi.Name;
                    bti.path = sceneContext.terrainDir + "/SceneTerrainBackup/" + fi.Name;
                    terrainBackupInfo.Add(bti);
                }

            }
        }

        private void CreateBackup(ref SceneContext sceneContext,TerrainData ourTerrainData)
        {
            //string dir = string.Format("{0}/SceneTerrainBackup", SceneAssets.scene_subname_directory);
            SceneAssets.CreateFolder(sceneContext.terrainDir, "SceneTerrainBackup");
            string ourDataPath = string.Format("{0}/{1}_{2}.asset", sceneContext.terrainDir, sceneContext.name, sceneContext.suffix);
            if (File.Exists(ourDataPath))
            {
                string backupDataPath = string.Format("{0}/SceneTerrainBackup/{1}_{2}_{3}.asset",
                    sceneContext.terrainDir, sceneContext.name, sceneContext.suffix,
                    DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss"));
                AssetDatabase.CopyAsset(ourDataPath, backupDataPath);
                RefreshTerrainDataBackup(ref sceneContext);
            }
        }

        private void RecoverBackup(ref SceneContext sceneContext, BackupTerrainInfo bti)
        {
            TerrainData ourTerrainData = LoadTerrainData(ref sceneContext);
            TerrainData backupTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(bti.path);
            if (ourTerrainData != null && backupTerrainData != null)
            {
                ourTerrainData.SetHeights(0, 0, backupTerrainData.GetHeights(0, 0, backupTerrainData.heightmapResolution, backupTerrainData.heightmapResolution));
                ourTerrainData.terrainLayers = backupTerrainData.terrainLayers;
                ourTerrainData.SetAlphamaps(0, 0, backupTerrainData.GetAlphamaps(0, 0, backupTerrainData.alphamapWidth, backupTerrainData.alphamapHeight));
            }
        }
        #endregion
    }
}
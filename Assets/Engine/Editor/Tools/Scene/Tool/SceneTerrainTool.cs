using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public partial class SceneEditTool : CommonToolTemplate
    {
        //class TerrainMergeContext
        //{
        //    public string theirTag = "";
        //    public TerrainData theirTerrainData = null;
        //    public TerrainData ourTerrainData = null;
        //    // public TerrainData baseTerrainData = null;
        //    public TerrainCollider terrainCollider = null;

        //    public float[, ] theirHeights = null;
        //    public float[, ] ourHeights = null;
        //    // public float[, ] baseHeights = null;
        //    public float[, ] mergeHeights = null;
        //    public Color[] conflictHeight = null;
        //    public int heightmapWdith;
        //    public int heightmapHeight;

        //    public float[, , ] theirSplats = null;
        //    public float[, , ] ourSplats = null;
        //    // public float[, , ] baseSplats = null;
        //    public float[, , ] mergeSplats = null;
        //    public Color[] conflictSplat = null;

        //    public int splatmapWdith;
        //    public int splatmapHeight;

        //    public TerrainLayer[] ourTerrainLayers;
        //    // public SplatPrototype[] baseSplatPrototypes;
        //    public TerrainLayer[] theirTerrainLayers;
        //    public int splatIndex = 0;
        //    public Vector2Int splatLayerCount;
        //    public Texture2D mergeTex = null;
        //    public RenderTexture mergeRT = null;
        //    public RenderTexture mergeSwitchRT = null;
        //    public int mergeIndex = 0;
        //    public int mergeFlag = 0;
        //    public bool preview = false;

        //    public void Reset ()
        //    {
        //        if (mergeTex != null)
        //        {
        //            UnityObject.DestroyImmediate (mergeTex);
        //            mergeTex = null;
        //        }
        //        if (mergeRT != null)
        //        {
        //            mergeRT.Release ();
        //            mergeRT = null;
        //        }
        //        if (mergeSwitchRT != null)
        //        {
        //            mergeSwitchRT.Release ();
        //            mergeSwitchRT = null;
        //        }
        //    }
        //}

        //private Material terrainEditMat = null;

        //private void PostInitTerrain (SceneConfig sceneConfig)
        //{
        //    Terrain terrain = Terrain.activeTerrain;
        //    if (terrain != null)
        //    {
        //        terrainGo = terrain.gameObject;
        //    }
        //    else
        //    {
        //        terrainGo = GetUnityTerrain ();
        //    }
        //    sceneMergeContext = new TerrainMergeContext ();
        //    terrainEditMat = new Material (AssetsConfig.instance.TerrainEditMat);
        //    terrainMergeMat = new Material (AssetsConfig.instance.TerrainMergeMat);
        //    terrainMeshMat = new Material (AssetsConfig.instance.TerrainMeshMat);
        //    if (sceneLocalConfig != null)
        //    {
        //        terrainEditMat.SetColor ("_EdgeColor", sceneLocalConfig.terrainGridColor);
        //    }
        //    TerrainObject.globalPbsParam = sceneConfig.terrainParam;
        //    RefreshTerrainDataBackup ();
        //}
        //private void UnInitTerrain ()
        //{
        //    if (terrainEditMat != null)
        //    {
        //        UnityEngine.Object.DestroyImmediate (terrainEditMat);
        //        terrainEditMat = null;
        //    }
        //    if (terrainMergeMat != null)
        //    {
        //        UnityEngine.Object.DestroyImmediate (terrainMergeMat);
        //        terrainMergeMat = null;
        //    }
        //    if (terrainMeshMat != null)
        //    {
        //        UnityEngine.Object.DestroyImmediate (terrainMeshMat);
        //        terrainMeshMat = null;
        //    }
        //    if (sceneMergeContext != null)
        //        sceneMergeContext.Reset ();
        //}
        //private GameObject GetTerrainObject (ref TerrainData td)
        //{
        //    Terrain t = Terrain.activeTerrain;
        //    if (t == null)
        //    {
        //        GameObject currentTerrainGo = GetUnityTerrain ();
        //        t = currentTerrainGo.GetComponent<Terrain> ();
        //        if (t != null)
        //        {
        //            td = t.terrainData;
        //            return currentTerrainGo;
        //        }
        //        return null;
        //    }
        //    td = t.terrainData;
        //    return t.gameObject;
        //}

        //private GameObject GetUnityTerrain ()
        //{
        //    if (commonContext.editorSceneGos[(int) EditorSceneObjectType.UnityTerrain] != null)
        //    {
        //        Transform t = commonContext.editorSceneGos[(int) EditorSceneObjectType.UnityTerrain].transform;
        //        if (t.childCount > 0)
        //        {
        //            return t.GetChild (0).gameObject;
        //        }
        //    }
        //    return null;
        //}
       
        //private static void CreateTerrainChunkGo (GameObject chunks, string meshName, Vector3 pos, int index, Material mat)
        //{
        //    //if (chunk.mesh != null)
        //    {
        //        Transform terrainmeshTrans = chunks.transform.Find (meshName);
        //        GameObject terrainmeshGo = null;
        //        if (terrainmeshTrans == null)
        //            terrainmeshGo = new GameObject (meshName);
        //        else
        //            terrainmeshGo = terrainmeshTrans.gameObject;
        //        terrainmeshGo.transform.parent = chunks.transform;
        //        terrainmeshGo.transform.position = pos;
        //        terrainmeshGo.layer = DefaultGameObjectLayer.TerrainLayer;

        //        GameObjectUtility.SetStaticEditorFlags (terrainmeshGo, StaticEditorFlags.ContributeGI);

        //        MeshFilter mf = terrainmeshGo.GetComponent<MeshFilter> ();
        //        if (mf == null)
        //            mf = terrainmeshGo.AddComponent<MeshFilter> ();

        //        MeshRenderer mr = terrainmeshGo.GetComponent<MeshRenderer> ();
        //        if (mr == null)
        //            mr = terrainmeshGo.AddComponent<MeshRenderer> ();
        //        mr.enabled = false;
        //        TerrainObject to = terrainmeshGo.GetComponent<TerrainObject> ();
        //        if (to == null)
        //            to = terrainmeshGo.AddComponent<TerrainObject> ();

        //        to.chunkID = index;
        //        // to.RefreshRender ();

        //        CommonAssets.BakeRender (mr);
        //        // mf.sharedMesh = chunk.mesh;
        //        mr.sharedMaterial = mat;
        //        mr.enabled = false;
        //    }
        //}
        //private void InitTerrain (GameObject srcTerrainGo, string tagName, GameObject editorTerrainGo)
        //{
        //    if (srcTerrainGo != null)
        //    {
        //        Terrain terrain = srcTerrainGo.GetComponent<Terrain> ();
        //        if (terrain != null && !string.IsNullOrEmpty (tagName))
        //        {
        //            bool createTerrainPrefab = false;
        //            if (EditorCommon.IsPrefabOrFbx (srcTerrainGo))
        //            {
        //                string terrainName = srcTerrainGo.name;
        //                if (terrainName.EndsWith (tagName))
        //                {
        //                    //ok
        //                    srcTerrainGo.transform.parent = editorTerrainGo.transform;
        //                    return;
        //                }
        //                else
        //                {
        //                    createTerrainPrefab = true;
        //                }
        //            }
        //            else
        //            {
        //                //init
        //                createTerrainPrefab = true;
        //            }

        //            if (createTerrainPrefab)
        //            {
        //                string name = string.Format ("{0}_{1}", sceneContext.name, tagName);
        //                TerrainData srcTerrainData = terrain.terrainData;
        //                string targetTerrainDataPath = string.Format ("{0}/{1}.asset", sceneContext.terrainDir, name);
        //                TerrainData targetTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData> (targetTerrainDataPath);
        //                if (targetTerrainData == null)
        //                {
        //                    string srcPath = AssetDatabase.GetAssetPath (srcTerrainData);
        //                    AssetDatabase.CopyAsset (srcPath, targetTerrainDataPath);
        //                    targetTerrainData = AssetDatabase.LoadAssetAtPath<TerrainData> (targetTerrainDataPath);
        //                    targetTerrainData.name = name;
        //                }
        //                GameObject go = Terrain.CreateTerrainGameObject (targetTerrainData);
        //                go.name = name;
        //                string targetTerrainGoPath = string.Format ("{0}/{1}.prefab", sceneContext.terrainDir, name);
        //                GameObject prefab = PrefabAssets.SavePrefab (targetTerrainGoPath, go);
        //                GameObject newTerrainGo = PrefabUtility.InstantiatePrefab (prefab) as GameObject;
        //                GameObject.DestroyImmediate (go);
        //                GameObject.DestroyImmediate (srcTerrainGo);
        //                newTerrainGo.transform.parent = editorTerrainGo.transform;
        //                newTerrainGo.layer = DefaultGameObjectLayer.TerrainLayer;
        //            }
        //        }
        //    }
        //}

        //private void InitChunks (SceneConfig sceneConfig, int widthCount, int heightCount, int chunkWidth, int chunkHeight, GameObject meshTerrainGo)
        //{
        //    string resSceneDir = string.Format ("{0}/Scene", AssetsConfig.instance.ResourcePath);
        //    SceneAssets.CreateFolder (AssetsConfig.instance.ResourcePath, "Scene");
        //    // SceneAssets.CreateFolder (resSceneDir, "Res");
        //    SceneAssets.CreateFolder (resSceneDir, sceneContext.name);

        //    // string targetSceneDir = string.Format ("{0}/Scene/{1}",
        //    //     AssetsConfig.GlobalAssetsConfig.ResourcePath, sceneContext.name);

        //    int chunkCount = widthCount * heightCount;
        //    if (sceneConfig.chunks.Count != chunkCount)
        //    {
        //        sceneConfig.chunks.Clear ();
        //        for (int i = 0; i < chunkCount; ++i)
        //        {
        //            sceneConfig.chunks.Add (new SceneConfig.TerrainChunk ());
        //        }
        //    }
        //    string indexStr = "";
        //    Material terrainChunkMat = AssetsConfig.instance.TerrainPreviewMat[0];
        //    GameObject meshTerrainChunks = meshTerrainGo; //commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain];

        //    // if (initTerrainChunks)
        //    {
        //        EditorCommon.DestroyChildObjects (meshTerrainChunks);
        //    }

        //    for (int z = 0; z < heightCount; ++z)
        //    {
        //        for (int x = 0; x < widthCount; ++x)
        //        {
        //            int i = z * widthCount + x;
        //            SceneConfig.TerrainChunk chunk = sceneConfig.chunks[i];
        //            if (chunk == null)
        //            {
        //                chunk = new SceneConfig.TerrainChunk ();
        //                sceneConfig.chunks[i] = chunk;
        //            }
        //            indexStr = string.Format ("{0}_{1}", x, z);

        //            Vector3 pos = new Vector3 (x * chunkWidth, 0, z * chunkHeight);
        //            string goName = string.Format ("{0}{1}", AssetsConfig.instance.SceneChunkStr, indexStr);
        //            // string meshName = goName;
        //            // if (initTerrainChunks) { }
        //            // else
        //            // {
        //            //     chunk.mesh = AssetDatabase.LoadAssetAtPath<Mesh> (targetSceneDir + "/" + meshName + ".asset");
        //            // }
        //            CreateTerrainChunkGo (meshTerrainChunks, goName, pos, i, terrainChunkMat);
        //            EditorUtility.DisplayProgressBar (string.Format ("CreateChunk-{0}/{1}", i, chunkCount), indexStr, (float) i / chunkCount);
        //        }
        //    }

        //    EditorUtility.ClearProgressBar ();
        //    CommonAssets.SaveAsset (sceneConfig);
        //}

        //class ChunkSplatValue : IComparable
        //{
        //    public int count;
        //    public int layer;
        //    public int index;
        //    public int CompareTo (object obj)
        //    {
        //        ChunkSplatValue other = obj as ChunkSplatValue;
        //        return other.count.CompareTo (count);
        //    }
        //}

        //private void BindTerrainData ()
        //{
        //    BindHeightMap ();
        //    BindAlphaMap ();
        //    SceneAssets.SceneModify ();
        //}

        //private void SetSplatTex (Material mat, int index, int id, TerrainLayer[] splats, float width, float height, ref Vector2 scale)
        //{
        //    if (index < splats.Length)
        //    {
        //        var splat = splats[index];
        //        Texture2D mainTex = splat.diffuseTexture;
        //        mat.SetTexture ("_MainTex" + id.ToString (), mainTex);
        //        scale.x = width / splat.tileSize.x;
        //        scale.y = height / splat.tileSize.y;
        //    }
        //    else
        //    {
        //        mat.SetTexture ("_MainTex" + id.ToString (), Texture2D.whiteTexture);
        //    }

        //}

        //private void BakeBaseMap ()
        //{
        //    // TerrainData ourTerrainData = LoadTerrainData (sceneContext.suffix);
        //    GameObject go = GetUnityTerrain ();
        //    Terrain terrain = go != null ? go.GetComponent<Terrain> () : null;
        //    if (terrain != null)
        //    {
        //        TerrainData ourTerrainData = terrain.terrainData;
        //        if (ourTerrainData != null)
        //        {
        //            EditorUtility.DisplayProgressBar ("BakeBaseMap", "BaseMapTex", 0.3f);
        //            float width = ourTerrainData.size.x;
        //            float height = ourTerrainData.size.z;
        //            Texture2D[] alphaTex = ourTerrainData.alphamapTextures;
        //            TerrainLayer[] terrainLayers = ourTerrainData.terrainLayers;
        //            int blendIndex = 0;
        //            RenderTexture rt = new RenderTexture (512, 512, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
        //            {
        //                name = "TerrainBaseMap",
        //                hideFlags = HideFlags.DontSave,
        //                filterMode = FilterMode.Bilinear,
        //                wrapMode = TextureWrapMode.Clamp,
        //                anisoLevel = 0,
        //                autoGenerateMips = false,
        //                useMipMap = false
        //            };
        //            Material tmp = new Material (AssetsConfig.instance.TerrainBakeBaseMap);
        //            TextureAssets.BeginDrawRT ();
        //            for (int i = 0; i < terrainLayers.Length; i += 4)
        //            {
        //                Texture2D blendTex = alphaTex[blendIndex];
        //                tmp.SetTexture ("_BlendTex", blendTex);
        //                Vector2 scalexy = Vector2.one;
        //                Vector4 scale = Vector4.one;
        //                SetSplatTex (tmp, i, 0, terrainLayers, width, height, ref scalexy);
        //                scale.x = scalexy.x;
        //                scale.y = scalexy.y;
        //                SetSplatTex (tmp, i + 1, 1, terrainLayers, width, height, ref scalexy);
        //                scale.z = scalexy.x;
        //                scale.w = scalexy.y;
        //                tmp.SetVector ("_Scale0", scale);
        //                SetSplatTex (tmp, i + 2, 2, terrainLayers, width, height, ref scalexy);
        //                scale.x = scalexy.x;
        //                scale.y = scalexy.y;
        //                SetSplatTex (tmp, i + 3, 3, terrainLayers, width, height, ref scalexy);
        //                scale.z = scalexy.x;
        //                scale.w = scalexy.y;
        //                tmp.SetVector ("_Scale1", scale);
        //                TextureAssets.DrawRT (rt, tmp, blendIndex == 0 ? 0 : 1);
        //                blendIndex++;
        //            }
        //            TextureAssets.EndDrawRT ();
        //            sceneConfig.baseMapTex = CommonAssets.CreateAsset<Texture2D> (string.Format ("{0}/Scene/{1}",
        //                AssetsConfig.instance.ResourcePath,
        //                sceneContext.name), "BaseMap", ".tga", rt);
        //            UnityEngine.Object.DestroyImmediate (rt);
        //            UnityEngine.Object.DestroyImmediate (tmp);

        //            EditorUtility.DisplayProgressBar ("BakeBaseMap", "BaseMapMesh", 0.5f);
        //            Mesh mesh = new Mesh ();
        //            mesh.name = "BaseMap";
        //            float blockSize = originalBlockSize * 2;
        //            int xCount = (int) (width / blockSize + 1);
        //            int zCount = (int) (height / blockSize + 1);

        //            // int u = xCount - 1;
        //            // int v = zCount - 1;
        //            terrainMeshContext.vertices.Clear ();
        //            terrainMeshContext.uv0s.Clear ();
        //            terrainMeshContext.normalList.Clear ();
        //            terrainMeshContext.normals.Clear ();
        //            for (int z = 0; z < zCount; ++z)
        //            {
        //                for (int x = 0; x < xCount; ++x)
        //                {
        //                    int index = terrainMeshContext.vertices.Count;
        //                    Vector3 pos = new Vector3 (x * blockSize, 0, z * blockSize);
        //                    pos.y = terrain.SampleHeight (pos);
        //                    terrainMeshContext.uv0s.Add (new Vector2 (Mathf.Clamp01 (pos.x / width), Mathf.Clamp01 (pos.z / height)));
        //                    terrainMeshContext.vertices.Add (pos);
        //                    terrainMeshContext.normalList.Add (new List<Vector3> ());
        //                    if (x > 0 && z < zCount - 1)
        //                    {
        //                        terrainMeshContext.indexes.Add (index);
        //                        terrainMeshContext.indexes.Add (index - 1);
        //                        terrainMeshContext.indexes.Add ((z + 1) * xCount + x);

        //                        terrainMeshContext.indexes.Add (index - 1);
        //                        terrainMeshContext.indexes.Add ((z + 1) * xCount + x - 1);
        //                        terrainMeshContext.indexes.Add ((z + 1) * xCount + x);
        //                    }
        //                }
        //            }
        //            for (int j = 0; j < terrainMeshContext.indexes.Count; j += 3)
        //            {
        //                Vector3 val = terrainMeshContext.vertices[terrainMeshContext.indexes[j]];
        //                Vector3 val2 = terrainMeshContext.vertices[terrainMeshContext.indexes[j + 1]];
        //                Vector3 val3 = terrainMeshContext.vertices[terrainMeshContext.indexes[j + 2]];
        //                Vector3 val4 = val2 - val;
        //                Vector3 val5 = val3 - val;
        //                Vector3 item = Vector3.Cross (val4, val5);
        //                item.Normalize ();
        //                terrainMeshContext.normalList[terrainMeshContext.indexes[j]].Add (item);
        //                terrainMeshContext.normalList[terrainMeshContext.indexes[j + 1]].Add (item);
        //                terrainMeshContext.normalList[terrainMeshContext.indexes[j + 2]].Add (item);
        //            }
        //            for (int k = 0; k < terrainMeshContext.vertices.Count; k++)
        //            {
        //                Vector3 val6 = Vector3.zero;
        //                for (int l = 0; l < terrainMeshContext.normalList[k].Count; l++)
        //                {
        //                    val6 += terrainMeshContext.normalList[k][l];
        //                }
        //                terrainMeshContext.normals.Add (val6 / (float) terrainMeshContext.normalList[k].Count);
        //            }

        //            mesh.vertices = terrainMeshContext.vertices.ToArray ();
        //            mesh.triangles = terrainMeshContext.indexes.ToArray ();
        //            mesh.uv = terrainMeshContext.uv0s.ToArray ();
        //            mesh.normals = terrainMeshContext.normals.ToArray ();
        //            mesh.RecalculateTangents ();
        //            mesh.RecalculateBounds ();
        //            mesh.UploadMeshData (true);
        //            MeshUtility.SetMeshCompression (mesh, ModelImporterMeshCompression.Off);
        //            Mesh newMesh = mesh;
        //            newMesh = CommonAssets.CreateAsset<Mesh> (string.Format ("{0}/Scene/{1}",
        //                    AssetsConfig.instance.ResourcePath,
        //                    sceneContext.name),
        //                mesh.name, ".asset", mesh);
        //            sceneConfig.baseMapMesh = newMesh;
        //            SaveConfig ();
        //        }

        //    }

        //    EditorUtility.ClearProgressBar ();
        //}

        
        //private void LoadTerrainColliderData ()
        //{
        //    try
        //    {
        //        string path = string.Format ("{0}/TerrainVertex.bytes", sceneContext.configDir);
        //        if (File.Exists (path))
        //        {
        //            int terrainGridCount = EngineContext.terrainGridCount + 1;
        //            int gridCount = terrainGridCount * terrainGridCount;
        //            using (FileStream fs = new FileStream (path, FileMode.Open))
        //            {
        //                BinaryReader br = new BinaryReader (fs);
        //                int chunkCount = br.ReadInt32 ();
        //                if (chunkCount == widthCount * heightCount)
        //                {
        //                    if (previewContext.groundFlags == null ||
        //                        previewContext.groundFlags.Length != gridCount * gridCount)
        //                    {
        //                        previewContext.groundFlags = new byte[gridCount * chunkCount];
        //                    }
        //                    if (previewContext.h == null ||
        //                        previewContext.h.Length != gridCount * gridCount)
        //                    {
        //                        previewContext.h = new float[gridCount * chunkCount];
        //                    }
        //                    for (int i = 0; i < chunkCount; ++i)
        //                    {
        //                        int startOffset = i * gridCount;
        //                        for (int j = 0; j < gridCount; ++j)
        //                        {
        //                            previewContext.groundFlags[startOffset + j] = br.ReadByte ();
        //                        }
        //                        for (int j = 0; j < gridCount; ++j)
        //                        {
        //                            previewContext.h[startOffset + j] = br.ReadSingle ();
        //                        }
        //                    }
        //                    previewContext.drawTerrainCollider = true;
        //                }
        //            }
        //            // float gridSize = originalBlockSize * 0.25f;
        //            // if (previewContext.terrainMeshPointBuffer == null)
        //            // {
        //            //     previewContext.terrainMeshPointBuffer = new ComputeBuffer(4, Marshal.SizeOf(typeof(TerrainPoint)));
        //            //     TerrainPoint[] points = new TerrainPoint[4]
        //            //     {
        //            //         new TerrainPoint()
        //            //         {
        //            //             vertex = new Vector3(0,0,0)
        //            //         },
        //            //         new TerrainPoint()
        //            //         {
        //            //             vertex = new Vector3(gridSize,0,0)
        //            //         },
        //            //         new TerrainPoint()
        //            //         {
        //            //             vertex = new Vector3()
        //            //         },
        //            //         new TerrainPoint()
        //            //         {
        //            //             vertex = new Vector3()
        //            //         },
        //            //     };
        //            //     previewContext.terrainMeshPointBuffer.SetData (points);
        //            // }
        //            // CommandBuffer cb = GetPreviewCB ();
        //            // for (int ii = 0; ii < saveChunkContext.selectTos.Count; ++ii)
        //            // {
        //            //     var to = saveChunkContext.selectTos[ii];
        //            //     int xIndex = to.chunkID % widthCount;
        //            //     int zIndex = to.chunkID / widthCount;
        //            //     if (to.terrainMeshMpb == null)
        //            //     {
        //            //         to.terrainMeshMpb = new MaterialPropertyBlock ();
        //            //     }
        //            //     if (previewContext.terrainMeshHeightBuffer == null)
        //            //     {
        //            //         previewContext.terrainMeshHeightBuffer = new ComputeBuffer (previewContext.h.Length, sizeof (float));
        //            //     }
        //            //     previewContext.terrainMeshHeightBuffer.SetData (previewContext.h);

        //            //     to.terrainMeshMpb.SetBuffer ("vertexHeight", previewContext.terrainMeshHeightBuffer);
        //            //     to.terrainMeshMpb.SetVector ("_PosOffset", new Vector4 (xIndex * chunkWidth, 0, zIndex * chunkWidth, gridSize));
        //            //     to.terrainMeshMpb.SetVector ("_GridSize", new Vector4 (chunkWidth, chunkHeight, 1.0f / chunkWidth, 1.0f / chunkHeight));
        //            //     to.terrainMeshMpb.SetInt ("_LineVertexCount", EngineContext.terrainGridCount + 1);
        //            //     to.terrainMeshMpb.SetInt ("_LineBlockCount", EngineContext.terrainGridCount);
        //            //     to.terrainMeshMpb.SetInt ("_HeightOffset", to.chunkID * gridCount);
        //            //     cb.DrawProcedural (Matrix4x4.identity, terrainMeshMat, 0, MeshTopology.Triangles, 3, EngineContext.terrainGridCount * EngineContext.terrainGridCount * 2, to.terrainMeshMpb);

        //            // }
        //            // if (SceneView.lastActiveSceneView != null &&
        //            //     SceneView.lastActiveSceneView.camera != null)
        //            // {
        //            //     SceneView.lastActiveSceneView.camera.RemoveAllCommandBuffers();
        //            //     SceneView.lastActiveSceneView.camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, previewContext.cb);
        //            // }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError (e.Message);
        //    }
        //}

        //private TerrainData LoadTerrainData (string configName)
        //{
        //    string editTerrainName = string.IsNullOrEmpty (configName) ? sceneContext.name : sceneContext.name + "_" + configName;
        //    string editTerrainDataPath = string.Format ("{0}/{1}.asset", sceneContext.terrainDir, editTerrainName);
        //    return AssetDatabase.LoadAssetAtPath<TerrainData> (editTerrainDataPath);
        //}

        //private float GetHeight (int x, int z, float[, ] heights)
        //{
        //    float height = heights[x, z] + heights[x + 1, z] + heights[x, z + 1] + heights[x + 1, z + 1];
        //    return height * 0.25f;
        //}

        //private bool CalcTerrainPos (Vector2 mousePos, out Vector4 pos)
        //{
        //    pos = Vector4.zero;
        //    if (sceneMergeContext.terrainCollider != null)
        //    {
        //        RaycastHit hit;
        //        Ray mouseRay = HandleUtility.GUIPointToWorldRay (mousePos);
        //        if (sceneMergeContext.terrainCollider.Raycast (mouseRay, out hit, Mathf.Infinity))
        //        {
        //            pos = hit.point;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private bool PrepareTerrainData (string configName)
        //{
        //    mergeType = SceneMergeType.None;
        //    sceneMergeContext.theirTag = configName;

        //    sceneMergeContext.theirTerrainData = null;
        //    sceneMergeContext.ourTerrainData = null;
        //    // sceneMergeContext.baseTerrainData = null;

        //    sceneMergeContext.theirHeights = null;
        //    sceneMergeContext.ourHeights = null;
        //    // sceneMergeContext.baseHeights = null;

        //    sceneMergeContext.heightmapWdith = 0;
        //    sceneMergeContext.heightmapHeight = 0;

        //    sceneMergeContext.theirSplats = null;
        //    sceneMergeContext.ourHeights = null;
        //    // sceneMergeContext.baseHeights = null;

        //    sceneMergeContext.splatmapWdith = 0;
        //    sceneMergeContext.splatmapHeight = 0;
        //    sceneMergeContext.splatIndex = -1;
        //    sceneMergeContext.splatLayerCount = Vector2Int.zero;

        //    TerrainData theirTerrainData = LoadTerrainData (configName);
        //    if (theirTerrainData != null)
        //    {
        //        TerrainData ourTerrainData = LoadTerrainData (sceneContext.suffix);
        //        if (ourTerrainData != null)
        //        {
        //            // TerrainData baseTerrainData = LoadTerrainData ("");
        //            // if (baseTerrainData != null)
        //            // {
        //            previewMeshTerrain = false;
        //            PreviewMeshTerrain ();
        //            Terrain terrain = Terrain.activeTerrain;
        //            if (terrain != null)
        //            {
        //                sceneMergeContext.terrainCollider = terrain.GetComponent<TerrainCollider> ();
        //                terrain.materialTemplate = terrainEditMat;
        //            }
        //            sceneMergeContext.theirTerrainData = theirTerrainData;
        //            sceneMergeContext.ourTerrainData = ourTerrainData;
        //            // sceneMergeContext.baseTerrainData = baseTerrainData;
        //            CreateBackup (ourTerrainData);
        //            return true;
        //            // }
        //        }
        //    }
        //    return false;
        //}

        //private void PrepareMergeTex (int width, int height)
        //{
        //    if (sceneMergeContext.mergeTex == null ||
        //        sceneMergeContext.mergeTex.width != width ||
        //        sceneMergeContext.mergeTex.height != height)
        //    {

        //        Texture2D tex = new Texture2D (width, height, TextureFormat.ARGB32, false, true)
        //        {
        //        name = "mergeTex",
        //        hideFlags = HideFlags.DontSave,
        //        anisoLevel = 0,
        //        wrapMode = TextureWrapMode.Clamp,
        //        filterMode = FilterMode.Point
        //        };
        //        sceneMergeContext.mergeTex = tex;
        //    }

        //    if (sceneMergeContext.mergeRT == null ||
        //        sceneMergeContext.mergeRT.width != width ||
        //        sceneMergeContext.mergeRT.height != height)
        //    {
        //        if (sceneMergeContext.mergeRT != null)
        //        {
        //            sceneMergeContext.mergeRT.Release ();
        //        }
        //        RenderTexture rt = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        //        {
        //            name = "mergeRt",
        //            hideFlags = HideFlags.DontSave,
        //            filterMode = FilterMode.Point,
        //            wrapMode = TextureWrapMode.Clamp,
        //            anisoLevel = 0,
        //            autoGenerateMips = false,
        //            useMipMap = false
        //        };
        //        rt.Create ();
        //        sceneMergeContext.mergeRT = rt;
        //        RenderTexture switchRt = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
        //        {
        //            name = "mergeSwitchRt",
        //            hideFlags = HideFlags.DontSave,
        //            filterMode = FilterMode.Point,
        //            wrapMode = TextureWrapMode.Clamp,
        //            anisoLevel = 0,
        //            autoGenerateMips = false,
        //            useMipMap = false
        //        };
        //        switchRt.Create ();
        //        sceneMergeContext.mergeSwitchRT = switchRt;
        //    }
        //    TextureAssets.BeginDrawRT ();
        //    terrainEditMat.SetTexture ("_AddTex", sceneMergeContext.mergeRT);
        //}
        // private void SetGridBlock (XEditor.Level.LevelMapData levelMapData, Vector3 p, byte[] groundFlags, int offset)
        // {
        //     QuadTreeElement grid = null;
        //     var block = levelMapData.GetBlockByCoord (p);
        //     if (block != null)
        //     {
        //         grid = block.GetGridByCoord (p);
        //     }
        //     if (grid != null)
        //     {
        //         float d = Mathf.Abs (grid.pos.y - p.y);
        //         if (d > 0.5f)
        //         {
        //             groundFlags[offset] = (byte) EGridFlag.EHasMultiLayer;
        //         }
        //     }
        //     else
        //     {
        //         groundFlags[offset] = (byte) EGridFlag.EHasBlock;
        //     }
        // }
        //private void SaveTerrainData ()
        //{

        //    Transform t = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain].transform;

        //    int terrainGridCount = EngineContext.terrainGridCount + 1;
        //    int gridCount = terrainGridCount * terrainGridCount;
        //    int chunkCount = heightCount * widthCount;
        //    byte[] groundFlags = new byte[gridCount * chunkCount];
        //    float[] h = new float[gridCount * chunkCount];
        //    float gridSize = originalBlockSize * 0.25f;
        //    for (int z = 0; z < heightCount; ++z)
        //    {
        //        for (int x = 0; x < widthCount; ++x)
        //        {
        //            string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
        //            TerrainObject to = FindMeshTerrain<TerrainObject> (t, chunkName);
        //            int i = x + z * widthCount;
        //            int startOffset = i * gridCount;
        //            SceneConfig.TerrainChunk chunk = sceneConfig.chunks[i];
        //            if (to != null)
        //            {
        //                chunk.terrainObjData.Copy (to.terrainObjData);
        //            }
        //            if (to != null && to.terrainObjData.isValid)
        //            {
        //                MeshFilter meshFilter = to.GetComponent<MeshFilter> ();
        //                Mesh m = meshFilter.sharedMesh;
        //                if (m != null)
        //                {
        //                    Vector3[] pos = m.vertices;
        //                    float offsetX = to.chunkID % widthCount * chunkWidth;
        //                    float offsetZ = to.chunkID / widthCount * chunkHeight;

        //                    for (int j = 0; j < pos.Length; ++j)
        //                    {
        //                        Vector3 p = pos[j];
        //                        p.x += offsetX;
        //                        p.z += offsetZ;
        //                        h[startOffset + j] = p.y;
        //                    }
        //                }
        //            }
        //            // else
        //            {

        //                float startX = x * chunkWidth;
        //                float startZ = z * chunkHeight;
        //                for (int posz = 0; posz < terrainGridCount; ++posz)
        //                {
        //                    for (int posx = 0; posx < terrainGridCount; ++posx)
        //                    {
        //                        int offset = posx + posz * terrainGridCount;
        //                        Vector3 p = new Vector3 (startX + posx * gridSize, 800, startZ + posz * gridSize);
        //                        RaycastHit hitinfo;
        //                        if (Physics.Raycast (p, Vector3.down, out hitinfo, 801, 1 << DefaultGameObjectLayer.TerrainLayer))
        //                        {
        //                            if (hitinfo.point.y > h[startOffset + offset])
        //                                h[startOffset + offset] = hitinfo.point.y;
        //                        }
        //                    }
        //                }
        //                //to.terrainObjData.needCollider = isHasCollider;
        //            }
        //        }
        //    }

        //    try
        //    {
        //        using (FileStream fs = new FileStream (string.Format ("{0}/TerrainVertex.bytes", sceneContext.configDir), FileMode.Create))
        //        {
        //            BinaryWriter bw = new BinaryWriter (fs);
        //            bw.Write (chunkCount);
        //            for (int i = 0; i < chunkCount; ++i)
        //            {
        //                int startOffset = i * gridCount;
        //                for (int j = 0; j < gridCount; ++j)
        //                {
        //                    bw.Write (h[startOffset + j]);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError (e.Message);
        //    }
        //}
        ///////////////////////////HeightMap///////////////////////////
        //private void MergeHeightMap (string configName)
        //{
        //    if (PrepareTerrainData (configName))
        //    {
        //        LoadHeightMap (sceneMergeContext.theirTerrainData, out sceneMergeContext.theirHeights, out sceneMergeContext.heightmapWdith, out sceneMergeContext.heightmapHeight, false);
        //        LoadHeightMap (sceneMergeContext.ourTerrainData, out sceneMergeContext.ourHeights, out sceneMergeContext.heightmapWdith, out sceneMergeContext.heightmapHeight, false);
        //        //LoadHeightMap (sceneMergeContext.baseTerrainData, out sceneMergeContext.baseHeights, out sceneMergeContext.heightmapWdith, out sceneMergeContext.heightmapHeight, false);
        //        PrepareMergeTex (sceneMergeContext.heightmapWdith, sceneMergeContext.heightmapHeight);
        //        float scale = sceneMergeContext.ourTerrainData.heightmapScale.y;
        //        if (sceneMergeContext.mergeHeights == null)
        //        {
        //            sceneMergeContext.mergeHeights = new float[sceneMergeContext.heightmapWdith + 1, sceneMergeContext.heightmapHeight + 1];
        //        }
        //        if (sceneMergeContext.conflictHeight == null)
        //        {
        //            sceneMergeContext.conflictHeight = new Color[sceneMergeContext.heightmapWdith * sceneMergeContext.heightmapHeight];
        //        }
        //        mergeType = SceneMergeType.HeightMap;
        //        for (int z = 0; z <= sceneMergeContext.heightmapHeight; ++z)
        //        {
        //            for (int x = 0; x <= sceneMergeContext.heightmapWdith; x++)
        //            {
        //                float theirHeight = sceneMergeContext.theirHeights[x, z];
        //                float ourHeight = sceneMergeContext.ourHeights[x, z];
        //                // float baseHeight = sceneMergeContext.baseHeights[x, z];
        //                float mergeHeight = ourHeight;
        //                if (theirHeight != ourHeight)
        //                {
        //                    mergeHeight = theirHeight;
        //                }
        //                // if (theirHeight == baseHeight && ourHeight != baseHeight)
        //                // {
        //                //     mergeHeight = ourHeight;
        //                // }
        //                // else if (theirHeight != baseHeight && ourHeight == baseHeight)
        //                // {
        //                //     mergeHeight = theirHeight;
        //                // }
        //                // else if (theirHeight != baseHeight && ourHeight != baseHeight)
        //                // {
        //                //     mergeHeight = ourHeight;
        //                // }
        //                sceneMergeContext.mergeHeights[x, z] = mergeHeight;
        //                if (x != sceneMergeContext.heightmapWdith &&
        //                    z != sceneMergeContext.heightmapHeight)
        //                {
        //                    Color c = Color.black;
        //                    float theirColorHeight = GetHeight (x, z, sceneMergeContext.theirHeights);
        //                    float ourColorHeight = GetHeight (x, z, sceneMergeContext.ourHeights);
        //                    // float baseColorHeight = GetHeight (x, z, sceneMergeContext.baseHeights);
        //                    // float deltaTheir = Math.Abs (theirColorHeight - baseColorHeight) * scale;
        //                    // float deltaOur = Math.Abs (ourColorHeight - baseColorHeight) * scale;

        //                    // if (deltaOur < 0.1f && deltaTheir > 0.1f)
        //                    // {
        //                    //     c = Color.green;
        //                    // }
        //                    // else if (deltaOur > 0.1f && deltaTheir < 0.1f)
        //                    // {
        //                    //     c = Color.blue;
        //                    // }
        //                    // else if (deltaOur > 0.1f && deltaTheir > 0.1f)
        //                    // {
        //                    //     c = Color.red;
        //                    // }
        //                    if (Math.Abs (theirColorHeight - ourColorHeight) * scale > 0.1f)
        //                    {
        //                        c = Color.red;
        //                    }
        //                    sceneMergeContext.conflictHeight[x * sceneMergeContext.heightmapHeight + z] = c;
        //                }
        //            }
        //        }
        //        sceneMergeContext.ourTerrainData.SetHeights (0, 0, sceneMergeContext.mergeHeights);
        //        sceneMergeContext.mergeTex.SetPixels (sceneMergeContext.conflictHeight);
        //        sceneMergeContext.mergeTex.Apply ();

        //        TextureAssets.BlitTex2RT (sceneMergeContext.mergeTex, sceneMergeContext.mergeRT);
        //        TextureAssets.BlitTex2RT (sceneMergeContext.mergeTex, sceneMergeContext.mergeSwitchRT);

        //    }
        //}

        //private void ApplyHeightmapMerge (bool apply, bool preview = false)
        //{
        //    if (apply)
        //    {
        //        TextureAssets.BlitRT2Tex (sceneMergeContext.mergeRT, sceneMergeContext.mergeTex);
        //        sceneMergeContext.conflictHeight = sceneMergeContext.mergeTex.GetPixels ();
        //        for (int z = 0; z <= sceneMergeContext.heightmapHeight; ++z)
        //        {
        //            for (int x = 0; x <= sceneMergeContext.heightmapWdith; x++)
        //            {
        //                float theirHeight = sceneMergeContext.theirHeights[x, z];
        //                float ourHeight = sceneMergeContext.ourHeights[x, z];

        //                if (x != sceneMergeContext.heightmapWdith &&
        //                    z != sceneMergeContext.heightmapHeight)
        //                {
        //                    Color c = sceneMergeContext.conflictHeight[x * sceneMergeContext.heightmapHeight + z];
        //                    if (c == Color.blue)
        //                    {
        //                        sceneMergeContext.mergeHeights[x, z] = ourHeight;
        //                    }
        //                    else if (c == Color.green)
        //                    {
        //                        sceneMergeContext.mergeHeights[x, z] = theirHeight;
        //                    }
        //                }
        //            }
        //        }
        //        sceneMergeContext.ourTerrainData.SetHeights (0, 0, sceneMergeContext.mergeHeights);
        //        SceneAssets.SceneModify ();
        //    }
        //    else
        //    {
        //        sceneMergeContext.ourTerrainData.SetHeights (0, 0, sceneMergeContext.ourHeights);

        //    }
        //    if (preview)
        //    {
        //        sceneMergeContext.preview = !sceneMergeContext.preview;
        //        if (sceneMergeContext.preview)
        //            terrainEditMat.SetTexture ("_AddTex", null);
        //        else
        //            terrainEditMat.SetTexture ("_AddTex", sceneMergeContext.mergeRT);

        //    }
        //    else
        //    {
        //        sceneMergeContext.theirHeights = null;
        //        sceneMergeContext.ourHeights = null;
        //        // sceneMergeContext.baseHeights = null;

        //        sceneMergeContext.heightmapWdith = 0;
        //        sceneMergeContext.heightmapHeight = 0;
        //        terrainEditMat.SetTexture ("_AddTex", null);
        //        terrainEditMat.SetVector ("_MouseWorldPos", Vector4.zero);
        //        terrainMergeMat.SetTexture ("_MainTex", null);
        //        TextureAssets.EndDrawRT ();
        //    }
        //}
        //private void BindHeightMap ()
        //{
        //    EditorChunkMesh ecm = AssetDatabase.LoadAssetAtPath<EditorChunkMesh> (
        //        string.Format ("{0}/editorChunkMesh.asset", sceneContext.terrainDir));
        //    if (ecm != null)
        //    {
        //        Transform meshTerrain = GetMeshTerrain ();
        //        if (meshTerrain != null)
        //        {
        //            for (int z = 0; z < heightCount; ++z)
        //            {
        //                for (int x = 0; x < widthCount; ++x)
        //                {
        //                    string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
        //                    MeshFilter mf = FindMeshTerrain<MeshFilter> (meshTerrain, chunkName);
        //                    if (mf != null)
        //                    {
        //                        mf.sharedMesh = ecm.editTerrainMesh[x + z * widthCount];
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}

        ///////////////////////////AlphaMap///////////////////////////
        //private void MergeSplats (int index)
        //{
        //    switch (index)
        //    {
        //        case 0:
        //            sceneMergeContext.theirTerrainData.terrainLayers = sceneMergeContext.ourTerrainLayers;
        //            break;
        //        case 1:
        //            sceneMergeContext.ourTerrainData.terrainLayers = sceneMergeContext.theirTerrainLayers;
        //            break;
        //    }

        //    int theirLayerCount = sceneMergeContext.theirTerrainData.alphamapLayers;
        //    int ourLayerCount = sceneMergeContext.ourTerrainData.alphamapLayers;
        //    sceneMergeContext.splatLayerCount[0] = ourLayerCount;
        //    sceneMergeContext.splatLayerCount[1] = theirLayerCount;

        //    MergeAlphaMap (index);
        //}

        //private void MergeAlphaMap (int splatIndex)
        //{
        //    if (splatIndex != sceneMergeContext.splatIndex)
        //    {
        //        sceneMergeContext.splatIndex = splatIndex;
        //        SceneAssets.SceneModify (true);
        //        LoadAlphaMap (sceneMergeContext.theirTerrainData, out sceneMergeContext.theirSplats, out sceneMergeContext.splatmapWdith, out sceneMergeContext.splatmapHeight);
        //        LoadAlphaMap (sceneMergeContext.ourTerrainData, out sceneMergeContext.ourSplats, out sceneMergeContext.splatmapWdith, out sceneMergeContext.splatmapHeight);
        //        //LoadAlphaMap (sceneMergeContext.baseTerrainData, out sceneMergeContext.baseSplats, out sceneMergeContext.splatmapWdith, out sceneMergeContext.splatmapHeight);
        //        PrepareMergeTex (sceneMergeContext.splatmapWdith, sceneMergeContext.splatmapHeight);
        //        if (sceneMergeContext.conflictSplat == null)
        //        {
        //            sceneMergeContext.conflictSplat = new Color[sceneMergeContext.splatmapWdith * sceneMergeContext.splatmapHeight];
        //        }

        //        int layerCount = sceneMergeContext.splatLayerCount[splatIndex];
        //        sceneMergeContext.mergeSplats = new float[sceneMergeContext.splatmapWdith, sceneMergeContext.splatmapHeight, layerCount];

        //        for (int z = 0; z < sceneMergeContext.splatmapHeight; ++z)
        //        {
        //            for (int x = 0; x < sceneMergeContext.splatmapWdith; x++)
        //            {
        //                // bool theirDiff = false;
        //                // bool ourDiff = false;
        //                // bool bothDiff = false;
        //                bool diff = false;
        //                for (int i = 0; i < layerCount; ++i)
        //                {
        //                    try
        //                    {
        //                        float theirAlpha = sceneMergeContext.theirSplats[x, z, i];
        //                        float ourAlpha = sceneMergeContext.ourSplats[x, z, i];
        //                        // float baseAlpha = sceneMergeContext.baseSplats[x, z, i];

        //                        // if (theirAlpha == baseAlpha && ourAlpha != baseAlpha)
        //                        // {
        //                        //     ourDiff = true;
        //                        // }
        //                        // else if (theirAlpha != baseAlpha && ourAlpha == baseAlpha)
        //                        // {
        //                        //     theirDiff = true;
        //                        // }
        //                        // else if (theirAlpha != baseAlpha && ourAlpha != baseAlpha)
        //                        // {
        //                        //     bothDiff = true;
        //                        // }
        //                        if (Mathf.Abs (ourAlpha - theirAlpha) > 0.1f)
        //                        {
        //                            diff = true;
        //                        }
        //                    }
        //                    catch
        //                    {
        //                        Debug.LogError (string.Format ("{0},{1} Layer{2}", x, z, i));
        //                    }
        //                }
        //                Color c = Color.black;
        //                if (diff)
        //                {
        //                    c = Color.red;
        //                }

        //                // if (bothDiff)
        //                // {
        //                //     c = Color.red;

        //                // }
        //                // else if (ourDiff)
        //                // {
        //                //     c = Color.blue;
        //                // }
        //                // else if (theirDiff)
        //                // {
        //                //     c = Color.green;
        //                // }
        //                sceneMergeContext.conflictSplat[x * sceneMergeContext.splatmapHeight + z] = c;
        //                for (int i = 0; i < layerCount; ++i)
        //                {
        //                    if (diff)
        //                    {
        //                        sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.theirSplats[x, z, i];
        //                    }
        //                    else
        //                    {
        //                        sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.ourSplats[x, z, i];
        //                    }
        //                    // if (bothDiff)
        //                    // {
        //                    //     sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.ourSplats[x, z, i];

        //                    // }
        //                    // else if (ourDiff)
        //                    // {
        //                    //     sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.ourSplats[x, z, i];
        //                    // }
        //                    // else if (theirDiff)
        //                    // {
        //                    //     sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.theirSplats[x, z, i];
        //                    // }
        //                    // else
        //                    // {
        //                    //     sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.baseSplats[x, z, i];
        //                    // }
        //                }
        //            }
        //        }

        //        sceneMergeContext.ourTerrainData.SetAlphamaps (0, 0, sceneMergeContext.mergeSplats);
        //        sceneMergeContext.mergeTex.SetPixels (sceneMergeContext.conflictSplat);
        //        sceneMergeContext.mergeTex.Apply ();

        //        TextureAssets.BlitTex2RT (sceneMergeContext.mergeTex, sceneMergeContext.mergeRT);
        //        TextureAssets.BlitTex2RT (sceneMergeContext.mergeTex, sceneMergeContext.mergeSwitchRT);
        //    }
        //}

        //private void MergeAlphaMap (string configName)
        //{
        //    if (PrepareTerrainData (configName))
        //    {
        //        mergeType = SceneMergeType.AlphaMap;
        //        int theirLayerCount = sceneMergeContext.theirTerrainData.alphamapLayers;
        //        int ourLayerCount = sceneMergeContext.ourTerrainData.alphamapLayers;
        //        // int baseLayerCount = sceneMergeContext.baseTerrainData.alphamapLayers;
        //        sceneMergeContext.splatLayerCount[0] = ourLayerCount;
        //        // sceneMergeContext.splatLayerCount[1] = baseLayerCount;
        //        sceneMergeContext.splatLayerCount[1] = theirLayerCount;

        //        sceneMergeContext.ourTerrainLayers = sceneMergeContext.ourTerrainData.terrainLayers;
        //        // sceneMergeContext.baseSplatPrototypes = sceneMergeContext.baseTerrainData.splatPrototypes;
        //        sceneMergeContext.theirTerrainLayers = sceneMergeContext.theirTerrainData.terrainLayers;
        //        // if (theirLayerCount == ourLayerCount && theirLayerCount == baseLayerCount)
        //        if (theirLayerCount == ourLayerCount)
        //        {
        //            MergeAlphaMap (0);
        //        }
        //    }
        //}

        //private void ApplyAlphaMap (bool apply, bool preview = false)
        //{
        //    if (apply)
        //    {
        //        if (sceneMergeContext.splatIndex >= 0)
        //        {
        //            int layerCount = sceneMergeContext.splatLayerCount[sceneMergeContext.splatIndex];
        //            TextureAssets.BlitRT2Tex (sceneMergeContext.mergeRT, sceneMergeContext.mergeTex);
        //            sceneMergeContext.conflictSplat = sceneMergeContext.mergeTex.GetPixels ();
        //            for (int z = 0; z < sceneMergeContext.splatmapHeight; ++z)
        //            {
        //                for (int x = 0; x < sceneMergeContext.splatmapWdith; x++)
        //                {

        //                    Color c = sceneMergeContext.conflictSplat[x * sceneMergeContext.splatmapHeight + z];
        //                    bool isBlue = c.b > 0;
        //                    bool isGreen = c.g > 0;
        //                    bool isRed = c.r > 0;
        //                    if (isBlue)
        //                    {
        //                        for (int i = 0; i < layerCount; ++i)
        //                        {
        //                            sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.ourSplats[x, z, i];
        //                        }
        //                    }
        //                    else if (isGreen)
        //                    {
        //                        for (int i = 0; i < layerCount; ++i)
        //                        {
        //                            sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.theirSplats[x, z, i];
        //                        }
        //                    }
        //                    else if (isRed)
        //                    {
        //                        for (int i = 0; i < layerCount; ++i)
        //                        {
        //                            sceneMergeContext.mergeSplats[x, z, i] = sceneMergeContext.mergeSplats[x, z, i];
        //                        }
        //                    }
        //                }
        //            }
        //            sceneMergeContext.ourTerrainData.SetAlphamaps (0, 0, sceneMergeContext.mergeSplats);
        //            SceneAssets.SceneModify ();
        //        }

        //    }
        //    else
        //    {
        //        sceneMergeContext.ourTerrainData.SetAlphamaps (0, 0, sceneMergeContext.ourSplats);
        //        sceneMergeContext.theirTerrainData.terrainLayers = sceneMergeContext.theirTerrainLayers;
        //        // sceneMergeContext.baseTerrainData.splatPrototypes = sceneMergeContext.baseSplatPrototypes;
        //        sceneMergeContext.ourTerrainData.terrainLayers = sceneMergeContext.ourTerrainLayers;

        //    }
        //    if (preview)
        //    {
        //        sceneMergeContext.preview = !sceneMergeContext.preview;
        //        if (sceneMergeContext.preview)
        //            terrainEditMat.SetTexture ("_AddTex", null);
        //        else
        //            terrainEditMat.SetTexture ("_AddTex", sceneMergeContext.mergeRT);
        //    }
        //    else
        //    {
        //        sceneMergeContext.theirSplats = null;
        //        sceneMergeContext.ourSplats = null;
        //        // sceneMergeContext.baseSplats = null;

        //        sceneMergeContext.splatmapWdith = 0;
        //        sceneMergeContext.splatmapHeight = 0;
        //        terrainEditMat.SetTexture ("_AddTex", null);
        //        terrainEditMat.SetVector ("_MouseWorldPos", Vector4.zero);
        //        terrainMergeMat.SetTexture ("_MainTex", null);
        //        TextureAssets.EndDrawRT ();

        //    }

        //}
        //private void BindAlphaMap ()
        //{
        //    Transform meshTerrain = GetMeshTerrain ();
        //    if (meshTerrain != null)
        //    {
        //        EditorChunkData ecd;
        //        SceneSerialize.LoadEditorChunkData (ref sceneContext, SceneContext.MainTagName, true, out ecd);
        //        for (int z = 0; z < heightCount; ++z)
        //        {
        //            for (int x = 0; x < widthCount; ++x)
        //            {
        //                string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
        //                TerrainObject to = FindMeshTerrain<TerrainObject> (meshTerrain, chunkName);
        //                if (to != null)
        //                {
        //                    var chunk = sceneConfig.chunks[x + z * widthCount];
        //                    to.Copy (chunk.terrainObjData);
        //                }
        //            }
        //        }
        //    }
        //}
        //private void OnChunkMergeGUI (string info)
        //{
        //    sceneLocalConfig.mergeFolder = EditorGUILayout.Foldout (sceneLocalConfig.mergeFolder, info);
        //    if (!sceneLocalConfig.mergeFolder)
        //        return;

        //    List<string> sceneEditorTag = sceneConfig.sceneEditorTag;

        //    GUILayout.BeginHorizontal ();
        //    editTag = GUILayout.TextField (editTag, GUILayout.MaxWidth (160));
        //    if (GUILayout.Button ("Add Edit Tag", GUILayout.MaxWidth (160)) && !string.IsNullOrEmpty (editTag))
        //    {
        //        sceneEditorTag.Add (editTag);
        //        string sceneSrcPath = string.Format ("{0}/{1}.unity", sceneContext.dir, sceneContext.name);
        //        string scenePath = string.Format ("{0}/{1}_{2}.unity", sceneContext.dir, sceneContext.name, editTag);
        //        AssetDatabase.CopyAsset (sceneSrcPath, scenePath);
        //        editTag = "";
        //    }
        //    GUILayout.EndHorizontal ();
        //    EditorCommon.BeginGroup ("Terrain Merge");

        //    for (int i = 0; i < sceneEditorTag.Count; ++i)
        //    {
        //        GUILayout.BeginHorizontal ();
        //        GUILayout.Label (sceneEditorTag[i], GUILayout.MaxWidth (160));
        //        if (sceneContext.suffix == SceneContext.MainTagName)
        //        {
        //            string editTag = sceneEditorTag[i];
        //            if (editTag != sceneContext.suffix)
        //            {
        //                for (int j = (int) SceneMergeType.HeightMap; j <= (int) SceneMergeType.AlphaMap; ++j)
        //                {
        //                    if (GUILayout.Button (mergeText[j], GUILayout.MaxWidth (120)))
        //                    {
        //                        MergeScene (editTag, (SceneMergeType) j);
        //                    }
        //                }
        //            }
        //        }

        //        GUILayout.EndHorizontal ();
        //    }
        //    GUILayout.BeginHorizontal ();
        //    string previewMeshTerrainStr = previewMeshTerrain ? "UnityTerrain" : "MeshTerrain";
        //    if (GUILayout.Button (previewMeshTerrainStr, GUILayout.MaxWidth (160)))
        //    {
        //        previewMeshTerrain = !previewMeshTerrain;
        //        PreviewMeshTerrain ();
        //    }
        //    GUILayout.EndHorizontal ();
        //    EditorCommon.EndGroup (false);
        //    EditorCommon.BeginGroup ("Terrain Save", false);
        //    GUILayout.BeginHorizontal ();

        //    for (int j = (int) SceneMergeType.HeightMap; j <= (int) SceneMergeType.AlphaMap; ++j)
        //    {
        //        int flag = 1 << j;
        //        bool select = (sceneMergeContext.mergeFlag & flag) != 0;
        //        bool newSelect = GUILayout.Toggle (select, mergeText[j], GUILayout.MaxWidth (100));
        //        if (select != newSelect)
        //        {
        //            if (newSelect)
        //            {
        //                sceneMergeContext.mergeFlag |= flag;
        //            }
        //            else
        //            {
        //                sceneMergeContext.mergeFlag &= ~flag;
        //            }
        //        }
        //    }
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    float chunkLod = sceneLocalConfig.chunkLodScale;
        //    chunkLod = GUILayout.HorizontalSlider (chunkLod, 1, 4, GUILayout.MaxWidth (260));
        //    sceneLocalConfig.chunkLodScale = (int) chunkLod;
        //    GUILayout.Label (string.Format ("Terrain Lod Scale:{0}", sceneLocalConfig.chunkLodScale), GUILayout.MaxWidth (260));
        //    GUILayout.EndHorizontal ();

        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.blendErrorCount = (int) GUILayout.HorizontalSlider (sceneConfig.blendErrorCount, 0, 50f, GUILayout.MaxWidth (260));
        //    GUILayout.Label (string.Format ("Terrain Blend Error Count:{0}", sceneConfig.blendErrorCount), GUILayout.MaxWidth (260));
        //    GUILayout.EndHorizontal ();

        //    GUILayout.BeginHorizontal ();
        //    // if (sceneContext.suffix == SceneContext.MainTagName)
        //    {
        //        if (GUILayout.Button ("Push", GUILayout.MaxWidth (160)))
        //        {
        //        }
        //    }

        //    if (sceneContext.suffix != SceneContext.MainTagName)
        //    {
        //        if (GUILayout.Button ("Pull", GUILayout.MaxWidth (160)))
        //        {
        //            if (EditorUtility.DisplayDialog ("Pull", "Is Pull? ", "OK", "Cancel"))
        //            {
        //            }
        //        }
        //    }

        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    if (GUILayout.Button ("BindTerrainData", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpBindTerrainData;
        //    }
        //    if (GUILayout.Button ("BakeBaseMap", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpBakeBaseMap;
        //    }
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();

        //    if (GUILayout.Button ("ValidTerrain", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpValidTerrain;
        //    }

        //    if (GUILayout.Button ("InvalidTerrain", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpInvalidTerrain;
        //    }
        //    GUILayout.EndHorizontal ();
        //    EditorCommon.EndGroup ();

        //    if (mergeType != SceneMergeType.None)
        //    {
        //        EditorCommon.BeginGroup ("Merge...");
        //        if (mergeType == SceneMergeType.AlphaMap)
        //        {
        //            string useSplat = "";
        //            if (sceneMergeContext.splatIndex == 0)
        //            {
        //                useSplat = sceneContext.suffix;
        //            }
        //            // else if (sceneMergeContext.splatIndex == 1)
        //            // {
        //            //     useSplat = "Base";
        //            // }
        //            else
        //            {
        //                useSplat = sceneMergeContext.theirTag;
        //            }
        //            GUILayout.Label ("Use Splats:" + useSplat);
        //            GUILayout.BeginHorizontal ();
        //            //int splatIndex = sceneMergeContext.splatIndex;
        //            bool use = GUILayout.Toggle (sceneMergeContext.splatIndex == 0,
        //                string.Format ("Use {0}({1})", sceneContext.suffix, sceneMergeContext.splatLayerCount[0]), GUILayout.MaxWidth (160));
        //            if (use && 0 != sceneMergeContext.splatIndex)
        //            {
        //                MergeSplats (0);
        //            }
        //            // use = GUILayout.Toggle (sceneMergeContext.splatIndex == 1, string.Format ("Use Base({0})", sceneMergeContext.splatLayerCount[1]), GUILayout.MaxWidth (160));
        //            // if (use && 1 != sceneMergeContext.splatIndex)
        //            // {
        //            //     MergeSplats (1);
        //            // }
        //            use = GUILayout.Toggle (sceneMergeContext.splatIndex == 1,
        //                string.Format ("Use {0}({1})", sceneMergeContext.theirTag, sceneMergeContext.splatLayerCount[1]), GUILayout.MaxWidth (160));
        //            if (use && 1 != sceneMergeContext.splatIndex)
        //            {
        //                MergeSplats (1);

        //            }

        //            GUILayout.EndHorizontal ();
        //        }

        //        GUILayout.BeginHorizontal ();
        //        Color revertTo = GUI.color;
        //        GUI.color = Color.blue;

        //        bool select = GUILayout.Toggle (sceneMergeContext.mergeIndex == 0, "Use " + sceneContext.suffix, GUILayout.MaxWidth (100));
        //        if (select && 0 != sceneMergeContext.mergeIndex)
        //        {
        //            sceneMergeContext.mergeIndex = 0;
        //            terrainMergeMat.SetColor ("_Color", Color.blue);
        //        }
        //        // GUI.color = revertTo;
        //        // select = GUILayout.Toggle (sceneMergeContext.mergeIndex == 1, "Use Base", GUILayout.MaxWidth (100));
        //        // if (select && 1 != sceneMergeContext.mergeIndex)
        //        // {
        //        //     sceneMergeContext.mergeIndex = 1;
        //        //     terrainMergeMat.SetColor ("_Color", Color.black);
        //        // }
        //        GUI.color = Color.green;
        //        select = GUILayout.Toggle (sceneMergeContext.mergeIndex == 1, "Use " + sceneMergeContext.theirTag, GUILayout.MaxWidth (100));
        //        if (select && 1 != sceneMergeContext.mergeIndex)
        //        {
        //            sceneMergeContext.mergeIndex = 1;
        //            terrainMergeMat.SetColor ("_Color", Color.green);
        //        }
        //        GUI.color = revertTo;
        //        GUILayout.EndHorizontal ();

        //        GUILayout.BeginHorizontal ();
        //        sceneLocalConfig.terrainMergeRange = GUILayout.HorizontalSlider (sceneLocalConfig.terrainMergeRange, 0, 100f, GUILayout.MaxWidth (260));
        //        GUILayout.Label (string.Format ("Terrain Merge Range:{0}", sceneLocalConfig.terrainMergeRange), GUILayout.MaxWidth (260));
        //        GUILayout.EndHorizontal ();

        //        if (sceneMergeContext.mergeRT != null)
        //        {
        //            GUILayout.BeginHorizontal ();
        //            GUILayout.Label ("Merge Tex");
        //            GUILayout.EndHorizontal ();
        //            GUILayout.Space (270);
        //            Rect r = GUILayoutUtility.GetLastRect ();
        //            r.y += 10;
        //            r.width = 256;
        //            r.height = 256;
        //            EditorGUI.DrawPreviewTexture (r, sceneMergeContext.mergeRT);
        //        }

        //        GUILayout.BeginHorizontal ();
        //        if (GUILayout.Button (sceneMergeContext.preview ? "UnPreviewApply" : "PreviewApply", GUILayout.MaxWidth (160)))
        //        {
        //            switch (mergeType)
        //            {
        //                case SceneMergeType.HeightMap:
        //                    ApplyHeightmapMerge (true, true);
        //                    break;
        //                case SceneMergeType.AlphaMap:
        //                    ApplyAlphaMap (true, true);
        //                    break;
        //            }
        //        }
        //        if (GUILayout.Button ("CancelEdit", GUILayout.MaxWidth (160)))
        //        {
        //            switch (mergeType)
        //            {
        //                case SceneMergeType.HeightMap:
        //                    ApplyHeightmapMerge (false);
        //                    break;
        //                case SceneMergeType.AlphaMap:
        //                    ApplyAlphaMap (false, false);
        //                    break;
        //            }
        //            mergeType = SceneMergeType.None;
        //        }
        //        if (GUILayout.Button ("ApplyEdit", GUILayout.MaxWidth (160)))
        //        {
        //            switch (mergeType)
        //            {
        //                case SceneMergeType.HeightMap:
        //                    ApplyHeightmapMerge (true);
        //                    break;
        //                case SceneMergeType.AlphaMap:
        //                    ApplyAlphaMap (true, false);
        //                    break;
        //            }
        //            mergeType = SceneMergeType.None;
        //        }

        //        GUILayout.EndHorizontal ();
        //        EditorCommon.EndGroup ();
        //    }

        //    EditorCommon.BeginGroup ("Terrain Info");
        //    GUILayout.BeginHorizontal ();
        //    if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpRefreshTerrainSplat;
        //    }
        //    if (GUILayout.Button ("Save", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpSaveConfig;
        //    }
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    EditorGUILayout.ObjectField (sceneConfig.baseMapMesh, typeof (Mesh), false);
        //    EditorGUILayout.ObjectField (sceneConfig.baseMapTex, typeof (Texture2D), false);
        //    GUILayout.EndHorizontal ();
        //    var bundles = sceneConfig.bundles;
        //    for (int i = 0; i < bundles.Count; ++i)
        //    {
        //        GUILayout.BeginHorizontal ();
        //        SceneConfig.TextureInfo ti = bundles[i];
        //        ti.tex = (Texture2D) EditorGUILayout.ObjectField (ti.tex, typeof (Texture2D), false);
        //        ti.pbs = (Texture2D) EditorGUILayout.ObjectField (ti.pbs, typeof (Texture2D), false);
        //        ti.scale = (byte) EditorGUILayout.IntField ("Scale", ti.scale);
        //        GUILayout.EndHorizontal ();
        //    }

        //    EditorGUI.BeginChangeCheck ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.terrainParam.x = EditorGUILayout.Slider ("SpecScale", sceneConfig.terrainParam.x, 0.01f, 5);
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.terrainParam.y = EditorGUILayout.Slider ("BlendThreshold", sceneConfig.terrainParam.y, 0.01f, 0.5f);
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.terrainParam.z = EditorGUILayout.Slider ("IBLScale", sceneConfig.terrainParam.z, 0.1f, 2f);
        //    GUILayout.EndHorizontal ();
        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.terrainParam.w = EditorGUILayout.Slider ("NormalScale", sceneConfig.terrainParam.w, 0.1f, 4f);
        //    GUILayout.EndHorizontal ();
        //    if (EditorGUI.EndChangeCheck ())
        //    {
        //        TerrainObject.globalPbsParam = sceneConfig.terrainParam;
        //        if (GlobalContex.ee != null)
        //        {
        //            GlobalContex.ee.UpdateMatObject ();
        //        }
        //    }

        //    EditorCommon.EndGroup ();
        //    EditorCommon.BeginGroup ("Backup TerrainData");
        //    GUILayout.BeginHorizontal ();
        //    if (GUILayout.Button ("Backup", GUILayout.MaxWidth (160)))
        //    {
        //        CreateBackup (null);
        //    }
        //    if (GUILayout.Button ("ClearAll", GUILayout.MaxWidth (160)))
        //    {
        //        for (int i = 0; i < terrainBackupInfo.Count; ++i)
        //        {
        //            BackupTerrainInfo bti = terrainBackupInfo[i];
        //            AssetDatabase.DeleteAsset (bti.path);
        //        }
        //        terrainBackupInfo.Clear ();
        //    }
        //    GUILayout.EndHorizontal ();

        //    for (int i = 0; i < terrainBackupInfo.Count; ++i)
        //    {
        //        BackupTerrainInfo bti = terrainBackupInfo[i];
        //        GUILayout.BeginHorizontal ();
        //        GUILayout.Label (bti.name, GUILayout.MaxWidth (400));

        //        if (GUILayout.Button ("Recover", GUILayout.MaxWidth (160)))
        //        {
        //            if (EditorUtility.DisplayDialog ("Recover", "Is Recover TerrainData ", "OK", "Cancel"))
        //            {
        //                RecoverBackup (bti);
        //                break;
        //            }
        //        }
        //        GUILayout.EndHorizontal ();
        //    }
        //    EditorCommon.EndGroup ();
        //}

         }
}
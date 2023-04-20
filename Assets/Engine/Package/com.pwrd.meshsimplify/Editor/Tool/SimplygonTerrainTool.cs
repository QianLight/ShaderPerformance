using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Athena.MeshSimplify
{
    /// <summary>
    /// PT地形方案（不适用所有）
    /// </summary>
    public static class SimplygonTerrainTool
    {
        private class TerrainMeshData
        {
            public string name;
            public float meshChunkOffsetHorizontal;
            public float meshChunkOffsetVertical;
            public Mesh highMesh;
            public Mesh lowMesh;
            public string highMeshPath;
            public string lowMeshPath;
        }

        // /// <summary>
        // /// 地形分块后转换成减面后的mesh再进行减面
        // /// </summary>
        // public static List<string> ConvertTerrainToFBX(SimplygonData defaultReduceData, SimplygonData simplygonData, int chunkSize, Material material, string layerName = null, bool useTerrainName = false)
        // {
        //     var terrain = simplygonData.target.GetComponent<Terrain>();
        //     if (terrain == null) return null;
        //     simplygonData.isTerrain = true;
        //     var sceneName = SceneManager.GetActiveScene().name.ToLower();
        //     var outputPath = simplygonData.outputFolder + "/" + sceneName;
        //     if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
        //
        //     var tempObj = ConvertToReduceTerrain(terrain, defaultReduceData, chunkSize, material, layerName, useTerrainName);
        //     string lodPath = outputPath + "/" + tempObj.name;
        //     
        //     simplygonData.target = tempObj;
        //     var objs = SimplygonTool.ReduceToFbx(simplygonData);
        //     SimplygonTool.DeleteTempAsset();
        //     GameObject.DestroyImmediate(tempObj);
        //     AssetDatabase.DeleteAsset(lodPath);
        //     SimplygonTool.Dispose();
        //     return objs;
        // }
        
        /// <summary>
        /// 地形分块后转换成减面后的mesh再进行减面
        /// </summary>
        public static GameObject ReduceTerrainToBlockModel(SimplygonData defaultReduceData, SimplygonData simplygonData, int chunkSize, int chunkUnit, Material material, ReduceMethod method, string layerName = null, bool useTerrainName = false)
        {
            var terrain = simplygonData.target.GetComponent<Terrain>();
            if (terrain == null) return null;
            simplygonData.isTerrain = true;
            var sceneName = SceneManager.GetActiveScene().name.ToLower();
            var outputPath = simplygonData.outputFolder + "/" + sceneName;
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            GameObject obj = null;
            try
            {
                // default reduce
                DisplayProgreess($"Convert To Mesh", 0.0f);
                var tempObj = ConvertToReduceTerrain(terrain, defaultReduceData, chunkSize, chunkUnit, material, method, layerName,
                    useTerrainName);
                // lod reduce
                DisplayProgreess($"LOD Ruduce Terrain", 0.5f);
                string lodPath = outputPath + "/" + tempObj.name;
                simplygonData.target = tempObj;
                var objs = SimplygonTool.Reduce(simplygonData, method);

                DisplayProgreess($"Collect Terrain Datas", 0.85f);
                var mfs = objs[0].GetComponentsInChildren<MeshFilter>();
                var mfs1 = objs[1].GetComponentsInChildren<MeshFilter>();
                TerrainMeshData[] terrainMeshDatas = new TerrainMeshData[mfs.Length];
                for (int i = 0; i < mfs.Length; i++)
                {
                    terrainMeshDatas[i] = new TerrainMeshData()
                    {
                        name = mfs[i].name,
                        highMesh = SimplygonTerrainConvert.ConvertMeshToIndexFormat16(Object.Instantiate(mfs[i].sharedMesh)), //转成uint16 
                        lowMesh = SimplygonTerrainConvert.ConvertMeshToIndexFormat16(Object.Instantiate(mfs1[i].sharedMesh)),
                        meshChunkOffsetHorizontal = mfs[i].transform.parent.localPosition.x,
                        meshChunkOffsetVertical = mfs[i].transform.parent.localPosition.z,
                        highMeshPath = AssetDatabase.GetAssetPath(mfs[i].sharedMesh),
                        lowMeshPath = AssetDatabase.GetAssetPath(mfs1[i].sharedMesh),
                    };
                }

                DisplayProgreess($"Save Terrain Datas", 0.9f);
                obj = CreateTerrainObjByDatas(terrainMeshDatas, outputPath, material, layerName);
                DisplayProgreess($"OK", 1f);
                AssetDatabase.DeleteAsset(lodPath);
            }
            catch(Exception e)
            {
                Debug.Log($"Convert Error, Error Message: {e.Message}");
            }

            SimplygonTool.DeleteTempAsset();
            AssetDatabase.SaveAssets();
            SimplygonTool.Dispose();
            return obj;
        }
        
        /// <summary>
        /// 地形转换成减面后的模型
        /// </summary>
        public static GameObject ConvertToReduceTerrain(Terrain terrain, SimplygonData defaultReduceData, int chunkSize, int chunkUnit, Material material, ReduceMethod method, string layerName = null, bool useTerrainName = false)
        {
            var obj = ConvertTerrainToModel(terrain, chunkSize, chunkUnit, material, SimplygonTool.SimplygonTempPath, layerName, useTerrainName);
            defaultReduceData.target = obj;
            defaultReduceData.outputFolder = SimplygonTool.SimplygonTempPath;
            defaultReduceData.isTerrain = true;
            DisplayProgreess($"First Reduce Terrain", 0.2f);
            var reduceObj = SimplygonTool.Reduce(defaultReduceData, method);
            GameObject.DestroyImmediate(obj);
            return reduceObj[0];
        }
        
        private static GameObject CreateTerrainObjByDatas(TerrainMeshData[] terrainMeshDatas, string outputPath, Material material, string layerName = "")
        {
            GameObject terraParentObj = new GameObject("Terrain");
            terraParentObj.transform.localPosition = Vector3.zero;
            terraParentObj.transform.localRotation = Quaternion.identity;
            terraParentObj.transform.localScale = Vector3.one;
            
            for (int i = 0; i < terrainMeshDatas.Length; i++)
            {
                var data = terrainMeshDatas[i];
                string terraObjName = data.name + "_LOD";
                GameObject terraObj = new GameObject(terraObjName);
                terraObj.transform.parent = terraParentObj.transform;
                
                terraObj.transform.localPosition = new Vector3(data.meshChunkOffsetHorizontal, 0f, data.meshChunkOffsetVertical);
                terraObj.transform.localRotation = Quaternion.identity;
                terraObj.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(layerName))
                {
                    var layer = LayerMask.NameToLayer(layerName);
                    if (layer >= 0)
                    {
                        terraObj.layer = layer;
                    }
                }

                string terraMeshObjName = data.name;
                GameObject terraMeshObj = new GameObject(terraMeshObjName);
                terraMeshObj.transform.parent = terraObj.transform;
                terraMeshObj.transform.localPosition = Vector3.zero;
                terraMeshObj.transform.localRotation = Quaternion.identity;
                terraMeshObj.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(layerName)) terraMeshObj.layer = LayerMask.NameToLayer(layerName);
                
                MeshFilter mf = terraMeshObj.AddComponent<MeshFilter>();
                MeshRenderer mr = terraMeshObj.AddComponent<MeshRenderer>();
                MeshCollider mc = terraMeshObj.AddComponent<MeshCollider>();
                mr.sharedMaterials = new Material[1] { material };
                mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                
                var meshPath = outputPath + "/" + terraMeshObjName + ".asset";
                if (!data.highMeshPath.Equals(meshPath) && File.Exists(meshPath)) AssetDatabase.DeleteAsset(meshPath);
                AssetDatabase.CreateAsset(data.highMesh, meshPath);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                mf.sharedMesh = mesh;
                mc.enabled = true;
                mc.sharedMesh = mesh;
                
                string terrLowMeshObjName = data.name + "_low";
                GameObject terrLowMeshObj = new GameObject(terrLowMeshObjName);
                terrLowMeshObj.transform.parent = terraObj.transform;
                terrLowMeshObj.transform.localPosition = Vector3.zero;
                terrLowMeshObj.transform.localRotation = Quaternion.identity;
                terrLowMeshObj.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(layerName)) terrLowMeshObj.layer = LayerMask.NameToLayer(layerName);
                
                MeshFilter lowMF = terrLowMeshObj.AddComponent<MeshFilter>();
                var lowMeshPath = outputPath + "/" + terraMeshObjName + "_low" + ".asset";
                if (!data.lowMeshPath.Equals(lowMeshPath) && File.Exists(lowMeshPath)) AssetDatabase.DeleteAsset(lowMeshPath);
                AssetDatabase.CreateAsset(data.lowMesh, lowMeshPath);
                Mesh lowMesh = AssetDatabase.LoadAssetAtPath<Mesh>(lowMeshPath);
                lowMF.sharedMesh = lowMesh;
                
                MeshRenderer lowMR = terrLowMeshObj.AddComponent<MeshRenderer>();
                lowMR.sharedMaterials = new Material[1] { TryGetLowMaterial(material) };
                lowMR.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                
                LODGroup lodGroup = terraObj.AddComponent<LODGroup>();
                LOD[] lods = new LOD[2];
                lods[0] = new LOD(0.8f, new Renderer[]
                    {
                        mr
                    });
                lods[1] = new LOD(0f, new Renderer[]
                    {
                        lowMR
                    });
                lodGroup.SetLODs(lods);
                var prefabPath = outputPath + "/" + terraObj.name + ".prefab";
                PrefabUtility.SaveAsPrefabAssetAndConnect(terraObj, prefabPath, InteractionMode.AutomatedAction);
            }
            // var prefabPath = outputPath + "/" + terraParentObj.name + ".prefab";
            // PrefabUtility.SaveAsPrefabAsset(terraParentObj, prefabPath);
            AssetDatabase.SaveAssets();
            // GameObject.DestroyImmediate(terraParentObj);
            // GameObject obj = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
            return terraParentObj;
        }

        /// <summary>
        /// 转换地形成模型
        /// </summary>
        public static GameObject ConvertTerrainToModel(Terrain _terrain, int chunkSize, int chunkUnit, Material material, string outputFolder, string layerName = null, bool useTerrainName = false)
        {
            TerrainConvertInfo _terrainConvertInfo = new TerrainConvertInfo();
            _terrainConvertInfo.chunkCountHorizontal = Mathf.CeilToInt(_terrain.terrainData.size.x / chunkSize);
            _terrainConvertInfo.chunkCountVertical = Mathf.CeilToInt(_terrain.terrainData.size.z / chunkSize);
            _terrainConvertInfo.vertexCountHorizontal = (int)(chunkSize / chunkUnit) + 1;
            _terrainConvertInfo.vertexCountVertical = (int)(chunkSize / chunkUnit) + 1;
            
            MeshConvertInfo[] allMeshConvertInfos = SimplygonTerrainConvert.ConvertToInfo(_terrain, _terrainConvertInfo, false);

            GameObject terraParentObj = new GameObject("Terrain");
            terraParentObj.transform.localPosition = Vector3.zero;
            terraParentObj.transform.localRotation = Quaternion.identity;
            terraParentObj.transform.localScale = Vector3.one;

            var sceneName = SceneManager.GetActiveScene().name.ToLower();
            var terrainName = _terrain.name;
            var outputPath = outputFolder + "/" + sceneName;
            if (!System.IO.Directory.Exists(outputPath))
            {
                System.IO.Directory.CreateDirectory(outputPath);
            }
            
            for (int i = 1; i <= allMeshConvertInfos.Length; i++)
            {
                var meshConvertInfo = allMeshConvertInfos[i - 1];
                Mesh convertMesh = meshConvertInfo.mesh;
                float meshChunkOffsetHorizontal = meshConvertInfo.GetChunkOffsetHorizontal();
                float meshChunkOffsetVertical = meshConvertInfo.GetChunkOffsetVertical();
                
                string terraObjName = string.Format("{0}_terrain_{1}_LOD", sceneName, string.Format("{0:D2}", i));
                if (useTerrainName) terraObjName = string.Format("{0}_{1}_LOD", terrainName, string.Format("{0:D2}", i));
                GameObject terraObj = new GameObject(terraObjName);
                terraObj.transform.parent = terraParentObj.transform;
                
                terraObj.transform.localPosition = new Vector3(meshChunkOffsetHorizontal, 0f, meshChunkOffsetVertical);
                terraObj.transform.localRotation = Quaternion.identity;
                terraObj.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(layerName) && LayerMask.NameToLayer(layerName) != -1)
                {
                    terraObj.layer = LayerMask.NameToLayer(layerName);
                }
                
                string terraMeshObjName = string.Format("{0}_terrain_{1}", sceneName, string.Format("{0:D2}", i));
                if (useTerrainName) terraMeshObjName = string.Format("{0}_{1}", terrainName, string.Format("{0:D2}", i));
                GameObject terraMeshObj = new GameObject(terraMeshObjName);
                terraMeshObj.transform.parent = terraObj.transform;
                terraMeshObj.transform.localPosition = Vector3.zero;
                terraMeshObj.transform.localRotation = Quaternion.identity;
                terraMeshObj.transform.localScale = Vector3.one;
                if (!string.IsNullOrEmpty(layerName) && LayerMask.NameToLayer(layerName) != -1)
                {
                    terraMeshObj.layer = LayerMask.NameToLayer(layerName);
                }

                MeshFilter mf = terraMeshObj.AddComponent<MeshFilter>();
                MeshRenderer mr = terraMeshObj.AddComponent<MeshRenderer>();
                mr.sharedMaterials = new Material[1] { material };
                mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                convertMesh.name = terraMeshObjName;
                var meshPath = outputPath + "/" + terraMeshObjName + ".asset";
                AssetDatabase.CreateAsset(convertMesh, meshPath);
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                mf.sharedMesh = mesh;
            }

            var prefabPath = outputPath + "/" + terraParentObj.name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(terraParentObj, prefabPath);
            AssetDatabase.SaveAssets();
            GameObject.DestroyImmediate(terraParentObj);
            return PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)) as GameObject;
        }
        
        private static Material TryGetLowMaterial(Material material)
        {
            Material lowMat = null;
            try
            {
                var materialPath = AssetDatabase.GetAssetPath(material).Replace("\\", "/");
                var lowMaterialPath = materialPath.Remove(materialPath.LastIndexOf("/")) + "/" + material.name + "_low.mat";
                lowMat = AssetDatabase.LoadAssetAtPath<Material>(lowMaterialPath);
            }
            catch { lowMat = material; }
            if (lowMat == null) lowMat = material;
            return lowMat;
        }
        
        private static void DisplayProgreess(string info, float progress)
        {
            EditorUtility.DisplayProgressBar("Terrain Convert", info, progress);
        }
    }
}
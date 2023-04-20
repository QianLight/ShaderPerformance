using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using CFEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;


public static class CheckMaterialMissingTool
{
    public class AssetResult
    {
        public string assetPath;
        public List<string> paths = new List<string>();
    }

    [MenuItem("Assets/统计工具/材质丢失统计/All")]
    public static void CheckAllMaterialMissing()
    {
        CheckMaterialMissing(true, true);
    }

    [MenuItem("Assets/统计工具/材质丢失统计/Scene")]
    public static void CheckSceneMaterialMissing()
    {
        CheckMaterialMissing(true, false);
    }

    [MenuItem("Assets/统计工具/材质丢失统计/Prefabs")]
    public static void CheckPrefabMaterialMissing()
    {
        CheckMaterialMissing(false, true);
    }

    private enum MaterialErrorCode
    {
        None = 0,
        MaterialMissing = 1,
        ShaderMissing = 2,
        LegacyShader = 3,
    }
    
    public static void CheckMaterialMissing(bool checkScenes, bool checkPrefabs, Func<string, bool> filter = null)
    {
        if (!checkScenes && !checkPrefabs)
            return;

        List<GameObject> roots = new List<GameObject>();
        List<Material> materials = new List<Material>();
        List<Dictionary<string, AssetResult>> locations = new List<Dictionary<string, AssetResult>>();

        static bool IsMaterialValid(Material material, out MaterialErrorCode code)
        {
            if (!material)
                code = MaterialErrorCode.MaterialMissing;
            else if (!material.shader)
                code = MaterialErrorCode.ShaderMissing;
            else
            {
                if (material.shader.name.StartsWith("Custom/"))
                    code = MaterialErrorCode.LegacyShader;
                else
                    code = MaterialErrorCode.None;
            }
            return code == MaterialErrorCode.None;
        }

        void AddLocation(string assetPath, Component component, MaterialErrorCode code)
        {
            while ((int)code > locations.Count)
            {
                locations.Add(new Dictionary<string, AssetResult>());
            }

            var l = locations[(int)code - 1];
            if (!l.TryGetValue(assetPath, out var sceneLocation))
            {
                sceneLocation = new AssetResult();
                sceneLocation.assetPath = assetPath;
                l.Add(assetPath, sceneLocation);
            }

            sceneLocation.paths.Add(EditorCommon.GetSceneObjectPath(component.transform));
        }

        void CheckGameObject(GameObject root, string objectPath)
        {
            if (!root)
                return;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.GetSharedMaterials(materials);
                if (renderer is ParticleSystemRenderer)
                {
                    var particleSystem = renderer.GetComponent<ParticleSystem>();
                    if (particleSystem)
                    {
                        var particleRenderer = renderer as ParticleSystemRenderer;
                        
                        bool trailMaterialError = !IsMaterialValid(particleRenderer.trailMaterial, out var trailCode) && particleSystem.trails.enabled;
                        if (trailMaterialError)
                        {
                            AddLocation(objectPath, renderer, trailCode);
                        }
                        bool particleMaterialError = !IsMaterialValid(materials[0], out var particleCode)
                                                       && particleRenderer.enabled
                                                       && particleSystem.emission.enabled
                                                       && particleRenderer.renderMode != ParticleSystemRenderMode.None;
                        if (particleMaterialError)
                        {
                            AddLocation(objectPath, renderer, particleCode);
                        }
                    }
                }
                else
                {
                    foreach (Material material in materials)
                    {
                        if (!IsMaterialValid(material, out var code))
                        {
                            AddLocation(objectPath, renderer, code);
                            break;
                        }
                    }
                }
            }
        }

        if (checkScenes)
        {
            string[] sceneGUIDs = AssetDatabase.FindAssets("t:scene");
            for (int i = 0; i < sceneGUIDs.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUIDs[i]);
                if (filter != null && !filter(scenePath))
                    continue;
                var scene = EditorSceneManager.OpenScene(scenePath);
                scene.GetRootGameObjects(roots);
                foreach (GameObject root in roots)
                    CheckGameObject(root, scenePath);
                EditorUtility.DisplayProgressBar("查找丢失材质资源", scenePath, (float) i / sceneGUIDs.Length);
            }
        }

        if (checkPrefabs)
        {
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:prefab");
            for (int i = 0; i < prefabGUIDs.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
                if (filter != null && !filter(prefabPath))
                    continue;
                // role runtime prefab 一定没有材质。
                if (prefabPath.StartsWith("Assets/BundleRes/Runtime/Prefab/"))
                    continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                CheckGameObject(prefab, prefabPath);
                EditorUtility.DisplayProgressBar("查找丢失材质资源", prefabPath, (float) i / prefabGUIDs.Length);
            }
        }

        EditorUtility.ClearProgressBar();

        for (int i = 0; i < locations.Count; i++)
        {
            var code = (MaterialErrorCode)(i + 1);
            var l = locations[i];
            int index = 0;
            string resultPath;
            do
                resultPath = $"Assets/_MaterialMissing/{code}{index++}.tab";
            while (File.Exists(resultPath));
            string directory = Path.GetDirectoryName(resultPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            UnityEngine.Object fileAsset = Export(l.Values, resultPath);
            Debug.Log($"检查完成, 文件名 = {resultPath}", fileAsset);    
        }
    }

    private static UnityEngine.Object Export(Dictionary<string, AssetResult>.ValueCollection locationsValues,
        string filePath)
    {
        StringBuilder sb = new StringBuilder();
        foreach (AssetResult ar in locationsValues)
        {
            string assetPath = ar.assetPath;
            foreach (string objPath in ar.paths)
            {
                sb.Append(assetPath);
                sb.Append('\t');
                sb.Append(objPath);
                sb.Append('\n');
            }
        }

        string content = sb.ToString();
        File.WriteAllText(filePath, content);
        AssetDatabase.ImportAsset(filePath);
        var result = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
        Selection.activeObject = result;
        return result;
    }
}
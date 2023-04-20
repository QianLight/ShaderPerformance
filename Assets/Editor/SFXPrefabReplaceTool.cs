using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SFXPrefabReplaceTool : EditorWindow
{
    private static readonly SavedAsset<Shader> srcShader = new SavedAsset<Shader>($"{nameof(SFXPrefabReplaceTool)}.{nameof(srcShader)}");
    private static readonly SavedAsset<Shader> dstShader = new SavedAsset<Shader>($"{nameof(SFXPrefabReplaceTool)}.{nameof(dstShader)}");
    private static readonly SavedString searchDirectory = new SavedString($"{nameof(SFXPrefabReplaceTool)}.{nameof(searchDirectory)}");
    private static readonly SavedInt searchCount = new SavedInt($"{nameof(SFXPrefabReplaceTool)}.{nameof(searchCount)}");
    private static readonly List<GameObject> replaceResult = new List<GameObject>();
    private static readonly List<GameObject> targets = new List<GameObject>();
    private static readonly SavedVector2 view = new SavedVector2($"{nameof(SFXPrefabReplaceTool)}.{nameof(view)}");
    private static readonly Dictionary<GameObject, GameObject> prefabMap = new Dictionary<GameObject, GameObject>();
    public static readonly SavedBool generatePrefab = new SavedBool($"{nameof(SFXPrefabReplaceTool)}.{nameof(generatePrefab)}");
    private static int comparingIndex = -1;

    private const string WindowName = "特效替换测试工具";

    //[MenuItem("Assets/Tool/Sfx_ReplaceNew")]
    public static void ReplaceMaterial()
    {
        List<Material> materials = new List<Material>();
        foreach (Object obj in Selection.objects)
        {
            if (AssetDatabase.Contains(obj))
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
                foreach (string dependencyPath in dependencies)
                {
                    Object dependencyAsset = AssetDatabase.LoadAssetAtPath<Object>(dependencyPath);
                    var material = dependencyAsset as Material;
                    if (material)
                    {
                        ReplaceMaterial(material);
                        materials.Add(material);
                    }
                }
            }
        }
        Selection.objects = materials.ToArray();
    }

    [MenuItem("Tools/引擎/" + WindowName)]
    public static void Open()
    {
        var window = GetWindow<SFXPrefabReplaceTool>();
        window.titleContent = new GUIContent(WindowName);
        window.Show();
    }

    private void OnGUI()
    {
        srcShader.Value = EditorGUILayout.ObjectField("原Shader", srcShader.Value, typeof(Shader), false) as Shader;
        dstShader.Value = EditorGUILayout.ObjectField("新Shader", dstShader.Value, typeof(Shader), false) as Shader;

        GUILayout.BeginHorizontal();
        Object dirAsset = AssetDatabase.LoadAssetAtPath<Object>(searchDirectory.Value);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("搜索目录", dirAsset, typeof(Object), false);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("选择", GUILayout.Width(35)))
        {
            string newDir = EditorUtility.OpenFolderPanel("选择搜索目录", "Assets/BundleRes/Effects/Prefabs", "");
            newDir = newDir.Substring(Application.dataPath.Length - "Assets".Length);
            bool exist = !string.IsNullOrEmpty(newDir) && Directory.Exists(newDir);
            if (exist && newDir != searchDirectory.Value)
            {
                searchDirectory.Value = newDir;
                comparingIndex = -1;
            }
        }
        GUILayout.EndHorizontal();

        searchCount.Value = EditorGUILayout.IntField("搜索数量(用于定量测试)", searchCount.Value);

        using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(searchDirectory.Value) || searchCount.Value <= 0))
        {
            if (GUILayout.Button($"查找特效"))
            {
                string[] prefabs = Directory.GetFiles(searchDirectory.Value, "*.prefab", SearchOption.AllDirectories);
                int count = Mathf.Min(prefabs.Length, searchCount.Value);
                targets.Clear();
                targets.Capacity = (count);
                for (int i = 0; i < count; i++)
                {
                    targets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[i]));
                }
            }
        }

        if (targets.Count > 0)
        {
            view.Value = EditorGUILayout.BeginScrollView(view.Value);
            for (int i = 0; i < targets.Count; i++)
            {
                Color backup = default;
                if (i == comparingIndex)
                {
                    backup = GUI.color;
                    GUI.color = Color.green;
                }
                GUILayout.BeginHorizontal();

                GUILayout.Label($"{i:000} / {targets.Count:000}", GUILayout.Width(70));
                EditorGUILayout.ObjectField(targets[i], typeof(GameObject), false);
                using (new EditorGUI.DisabledGroupScope(!prefabMap.TryGetValue(targets[i], out GameObject newPrefab)))
                {
                    if (GUILayout.Button("对比"))
                    {
                        comparingIndex = i;
                        GenerateComparisions(targets[i], newPrefab);
                    }
                }
                GUILayout.EndHorizontal();

                if (i == comparingIndex)
                {
                    GUI.color = backup;
                }
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUI.BeginDisabledGroup(targets.Count == 0);
        if (GUILayout.Button("生成测试特效"))
        {
            prefabMap.Clear();
            CopyPrefabs(targets, prefabMap);
        }
        if (GUILayout.Button("修改特效材质") && EditorUtility.DisplayDialog("警告", "*确定修改原特效的材质吗？会修改原材质的Shader和属性。", "确定", "取消"))
        {
            ReplaceMats(targets);
        }
        EditorGUI.EndDisabledGroup();
    }

    private void ReplaceMats(List<GameObject> targets)
    {
        AssetDatabase.StartAssetEditing();
        foreach (var target in targets)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (var pr in renderers)
            {
                var materials = pr.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    ReplaceMaterial(material);
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.StopAssetEditing();
    }

    private static void ReplaceMaterial(Material material)
    {
        if (material && material.shader == srcShader.Value)
        {
            var copy = new Material(material);
            MaterialDeserizer deserizer = new MaterialDeserizer(material);
            deserizer.Clear(MaterialDeserizer.ClearFlag.Properties);
            deserizer.Apply();
            material.shader = dstShader.Value;
            SFXMaterialReplace.Replace(copy, material);
            EditorUtility.SetDirty(material);
        }
    }

    private void GenerateComparisions(GameObject prefab, GameObject newPrefab)
    {
        Scene scene = SceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        GameObject root = null;
        const string ROOT_NAME = "SFX Compare";
        foreach (var item in roots)
        {
            if (item.name == ROOT_NAME)
            {
                root = item;
                break;
            }
        }

        if (!root)
        {
            root = new GameObject(ROOT_NAME, typeof(ParticleSystem));
        }

        Transform rawRoot = GetParent(root, "raw", Vector3.forward);
        Transform newRoot = GetParent(root, "new", Vector3.back);

        GameObject raw = PrefabUtility.InstantiatePrefab(prefab, rawRoot) as GameObject;
        GameObject @new = PrefabUtility.InstantiatePrefab(newPrefab, newRoot) as GameObject;

        Selection.activeObject = root;
        root.GetComponent<ParticleSystem>().Play(true);
    }

    private static Transform GetParent(GameObject root, string name, Vector3 defaultPos)
    {
        Transform rawRoot = root.transform.Find(name);
        if (!rawRoot)
        {
            rawRoot = new GameObject(name).transform;
            rawRoot.parent = root.transform;
            rawRoot.transform.localPosition = defaultPos;
        }
        while (rawRoot.childCount > 0)
        {
            Transform child = rawRoot.GetChild(0);
            DestroyImmediate(child.gameObject);
        }
        return rawRoot;
    }

    private class MaterialBinding
    {
        public GameObject instance;
        public string newMaterialPath;
        public string particlePath;
        public int materialSlot;
        public int rendererSlotCount;
        public Material rawMaterial;
    }

    private class PrefabBinding
    {
        public string prefabPath;
        public GameObject prefab;
        public GameObject instance;
        public List<MaterialBinding> materialBindings = new List<MaterialBinding>();
    }

    //[MenuItem("SFXOptimizeTest/Copy Selected Prefab")]
    public static void CopySelectedPrefabs()
    {
        Object[] selections = Selection.objects;
        List<GameObject> prefabs = new List<GameObject>();
        foreach (Object obj in selections)
        {
            if (obj is GameObject && AssetDatabase.Contains(obj))
            {
                prefabs.Add(obj as GameObject);
            }
        }

        if (prefabs.Count > 0)
        {
            CopyPrefabs(prefabs);
        }
    }

    private static void CopyPrefabs(List<GameObject> prefabs, Dictionary<GameObject, GameObject> result = null)
    {
        Object[] selections;
        #region Load and create material binding info.
        Dictionary<GameObject, PrefabBinding> prefabBindings = new Dictionary<GameObject, PrefabBinding>();
        List<string> prefabPaths = new List<string>(prefabs.Count);
        List<string> newPrefabPaths = new List<string>(prefabs.Count);
        List<GameObject> ins = new List<GameObject>(prefabs.Count);
        HashSet<string> generatedMaterials = new HashSet<string>();
        foreach (GameObject prefab in prefabs)
        {
            GameObject instance = Instantiate(prefab);
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            prefabPaths.Add(prefabPath);
            ins.Add(instance);
            var prefabBinding = new PrefabBinding()
            {
                prefabPath = prefabPath,
                prefab = prefab,
                instance = instance,
            };
            prefabBindings.Add(instance, prefabBinding);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (var pr in renderers)
            {
                string prPath = CFEngine.EditorCommon.GetSceneObjectPath(pr.transform, false);
                var materials = pr.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    var rawMaterial = materials[i];
                    if (rawMaterial && rawMaterial.shader == srcShader.Value)
                    {
                        string newAssetPath = GetAssetNewPath(prefab, rawMaterial);
                        MaterialBinding binding = new MaterialBinding();
                        binding.newMaterialPath = newAssetPath;
                        binding.instance = instance;
                        binding.particlePath = prPath;
                        binding.materialSlot = i;
                        binding.rawMaterial = rawMaterial;
                        prefabBindings[instance].materialBindings.Add(binding);
                    }
                }
            }
        }
        #endregion

        #region Create and bind materials.

        // Delete exist materials.
        AssetDatabase.StartAssetEditing();
        foreach (var prefabBinding in prefabBindings)
        {
            foreach (MaterialBinding matBinding in prefabBinding.Value.materialBindings)
            {
                if (File.Exists(matBinding.newMaterialPath))
                {
                    AssetDatabase.DeleteAsset(matBinding.newMaterialPath);
                }
            }
        }
        AssetDatabase.StopAssetEditing();

        // Create new materials
        AssetDatabase.StartAssetEditing();
        foreach (var prefabBinding in prefabBindings)
        {
            GameObject instance = prefabBinding.Key;
            foreach (MaterialBinding matBinding in prefabBinding.Value.materialBindings)
            {
                Material newMaterial;
                if (generatedMaterials.Add(matBinding.newMaterialPath))
                {
                    newMaterial = new Material(dstShader.Value);
                    newMaterial.shader = dstShader.Value;
                    SFXMaterialReplace.Replace(matBinding.rawMaterial, newMaterial);
                    EditorUtility.SetDirty(newMaterial);
                    AssetDatabase.CreateAsset(newMaterial, matBinding.newMaterialPath);
                }
                else
                {
                    newMaterial = AssetDatabase.LoadAssetAtPath<Material>(matBinding.newMaterialPath);
                }

                Renderer newRenderer = instance.transform.Find(matBinding.particlePath).GetComponent<Renderer>();
                Material[] materials = newRenderer.sharedMaterials;
                if (matBinding.materialSlot < 0 || matBinding.materialSlot >= materials.Length)
                {
                    Debug.LogError($"替换Prefab出错，材质索引越界！\nPrefab={prefabBinding.Value.prefabPath}\nRenderer={matBinding.particlePath}\n索引={matBinding.materialSlot}\n新材质={matBinding.newMaterialPath}", newRenderer);
                }
                else
                {
                    materials[matBinding.materialSlot] = newMaterial;
                    newRenderer.sharedMaterials = materials;
                }
            }
        }
        AssetDatabase.StopAssetEditing();
        #endregion

        #region Generate new prefab & Refresh selections.
        selections = new Object[prefabs.Count];
        int prefabIndex = 0;
        AssetDatabase.StartAssetEditing();
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject instance = ins[i];
            string prefabPath = prefabPaths[i];
            EditorUtility.DisplayProgressBar("生成Prefab", $"({prefabIndex + 1}/{prefabs.Count}) {prefabPath}", (float)prefabIndex / prefabs.Count);
            string newPrefabPath = GetNewAssetPathAndCreateDirectory(instance, prefabPath);
            newPrefabPaths.Add(newPrefabPath);
            GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(instance, newPrefabPath, out bool success);
            selections[prefabIndex] = newPrefab;
            replaceResult.Add(newPrefab);
            if (success)
            {
                Debug.Log($"[{prefabIndex}/{prefabs.Count}] Generate success: {prefabPath}", newPrefab);
            }
            else
            {
                Debug.LogError($"[{prefabIndex}/{prefabs.Count}] Generate fail: {prefabPath}", newPrefab);
            }
            prefabIndex++;
            DestroyImmediate(instance);
        }
        AssetDatabase.StopAssetEditing();
        EditorUtility.ClearProgressBar();
        Selection.objects = selections;
        #endregion

        if (result != null)
        {
            for (int i = 0; i < prefabs.Count; i++)
            {
                result.Add(prefabs[i], AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPaths[i]));
            }
        }
    }

    private static string GetAssetNewPath(GameObject source, Material raw)
    {
        string assetPath = AssetDatabase.GetAssetPath(raw);
        string newAssetPath = GetNewAssetPathAndCreateDirectory(source, assetPath);
        string dir = Path.GetDirectoryName(newAssetPath);
        if (File.Exists(newAssetPath))
        {
            File.Delete(newAssetPath);
            File.Delete(newAssetPath + ".meta");
        }
        return newAssetPath;
    }

    private static string GetNewAssetPathAndCreateDirectory(GameObject source, string rawAssetPath)
    {
        string rawFileName = Path.GetFileName(rawAssetPath);
        string assetName = source.name.Replace("(Clone)", "");
        string newFileName = rawFileName.Insert(rawFileName.LastIndexOf('.'), "_optimize");
        string newAssetPath = $"Assets/SFXOptimize/{assetName}/{newFileName}";
        string directory = Path.GetDirectoryName(newAssetPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return newAssetPath;
    }
}

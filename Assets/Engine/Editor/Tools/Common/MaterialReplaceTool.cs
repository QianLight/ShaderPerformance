using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using PropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;

namespace CFEngine.Editor
{
    public class MaterialReplaceTool : EditorWindow
    {
        private const string TITLE = "材质替换工具";

        private static readonly SavedAsset<MaterialReplaceConfig> savedConfig
            = new SavedAsset<MaterialReplaceConfig>($"{nameof(MaterialReplaceTool)}.{nameof(savedConfig)}");

        private static readonly SavedVector2 propertyViewPos =
            new SavedVector2($"{nameof(MaterialReplaceTool)}.{nameof(propertyViewPos)}");

        private static readonly SavedVector2 materialViewPos =
            new SavedVector2($"{nameof(MaterialReplaceTool)}.{nameof(materialViewPos)}");

        private static readonly SavedVector2 referenceViewPos =
            new SavedVector2($"{nameof(MaterialReplaceTool)}.{nameof(referenceViewPos)}");

        private static readonly SavedString srcSelectedAddName =
            new SavedString($"{nameof(MaterialReplaceTool)}.{nameof(srcSelectedAddName)}");

        private static readonly SavedString dstSelectedAddName =
            new SavedString($"{nameof(MaterialReplaceTool)}.{nameof(dstSelectedAddName)}");

        private static readonly SavedInt pannelIndex =
            new SavedInt($"{nameof(MaterialReplaceTool)}.{nameof(pannelIndex)}");

        private static readonly SavedAsset<MaterialReplaceGroup> group =
            new SavedAsset<MaterialReplaceGroup>($"{nameof(MaterialReplaceTool)}.{nameof(@group)}");

        private const string newMaterialDir = "Assets/MaterialReplace";

        GUIPage<string> materialsPage = new GUIPage<string>(
            $"{nameof(MaterialReplaceTool)}.{nameof(materialsPage)}",
            null, GUIMaterial);

        private string[] srcPropertyNames;
        private string[] dstPropertyNames;

        private static readonly SavedString selectedMaterialPath =
            new SavedString($"{nameof(MaterialReplaceTool)}.{nameof(selectedMaterialPath)}");

        private List<string> materialPaths = new List<string>();

        private string[] toolbars = new string[] {"Shader替换配置(TA)", "材质替换配置(美术)"};
        private static Dictionary<string, List<string>> referenceMap;
        private static string toRemoveMaterialPath;

        private static bool configDirty = false;

        [MenuItem("Tools/常用命令/ClearProgressBar")]
        private static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Tools/引擎/材质替换工具", priority = 0)]
        public static void Open()
        {
            MaterialReplaceTool window = GetWindow<MaterialReplaceTool>();
            window.titleContent = new GUIContent(TITLE);
            window.Show();
        }

        private void OnEnable()
        {
            ShaderUtility.onShaderReimport += OnShaderReimport;
        }

        private void OnDisable()
        {
            ShaderUtility.onShaderReimport -= OnShaderReimport;
        }

        private void OnShaderReimport(Shader shader)
        {
            var config = savedConfig.Value;
            if (config)
            {
                if (config && shader == config.srcShader)
                    srcPropertyNames = GetPopupItems(config, true);
                if (config && shader == config.dstShader)
                    dstPropertyNames = GetPopupItems(config, false);
            }
        }

        void DrawTitle(string text)
        {
            GUILayout.Box(text, GUILayout.ExpandWidth(true));
        }

        private void OnGUI()
        {
            if (MaterialReplaceHelper.isProgressBarRunning.Value && GUILayout.Button("强制清除进度条"))
            {
                EditorUtility.ClearProgressBar();
                MaterialReplaceHelper.isProgressBarRunning.Value = false;
            }

            pannelIndex.Value = GUILayout.Toolbar(pannelIndex.Value, toolbars);

            switch (pannelIndex.Value)
            {
                case 0:
                    DrawRepleaceConfigs();
                    break;
                case 1:
                    DrawSerachConfigs();
                    break;
                default:
                    break;
            }
        }

        private static string[] processorDisplayNames;
        private static Type[] processorTypes;
        private static string[] processorTypeNames;

        [InitializeOnLoadMethod]
        private static void OnAssimbliesLoaded()
        {
            List<Type> types = new List<Type>();

            var rootType = typeof(MaterialReplaceProcessor);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(rootType);
            Type[] allTypes = assembly.GetTypes();
            for (int i = 0; i < allTypes.Length; i++)
                if (allTypes[i].IsSubclassOf(rootType))
                    types.Add(allTypes[i]);

            processorDisplayNames = new string[types.Count + 1];
            processorTypes = new Type[types.Count + 1];
            processorTypeNames = new string[types.Count + 1];
            processorDisplayNames[0] = "None";
            processorTypeNames[0] = "null";
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                MaterialReplaceProcessor processor = Activator.CreateInstance(type) as MaterialReplaceProcessor;
                processorDisplayNames[i + 1] = $"{processor.DisplayName} ({type.FullName})";
                processorTypes[i + 1] = type;
                processorTypeNames[i + 1] = type.FullName;
            }
        }

        private void DrawCustomMaterialProcessor(MaterialReplaceConfig config)
        {
            GUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();

            DrawTitle("自定义材质处理流程");
            var typeName = config.Processor == null ? string.Empty : config.Processor.GetType().FullName;
            int lastIndex = Array.IndexOf(processorTypeNames, typeName);
            lastIndex = Mathf.Clamp(lastIndex, 0, processorTypeNames.Length);

            int newIndex = EditorGUILayout.Popup(lastIndex, processorDisplayNames);
            if (newIndex == 0)
            {
                config.Processor = null;
            }
            else if (config.Processor == null && newIndex > 0)
            {
                config.Processor = Activator.CreateInstance(processorTypes[newIndex]) as MaterialReplaceProcessor;
            }

            GUILayout.EndVertical();
        }

        private void ReplaceProperties(MaterialReplaceGroup group, bool silence)
        {
            foreach (MaterialReplaceConfig config in group.configs)
            {
                if (config)
                    ReplaceProperties(config, true);
            }

            if (!silence)
            {
                AssetDatabase.SaveAssets();
                string desc =
                    $"共替换{materialPaths.Count}个材质。" +
                    $"\n当前材质和引用搜索结果都已经被清空，因为换完Shader后已不符合当前条件。" +
                    $"\n如果有需要请重新搜索。" +
                    $"\n如果想要撤销修改，请在Git中手动还原。";
                EditorUtility.DisplayDialog("替换完成", desc, "OK");
                materialPaths.Clear();
                referenceMap?.Clear();
            }
        }
        
        private void ReplaceProperties(MaterialReplaceConfig config, bool silence)
        {
            List<Material> materials = new List<Material>(materialPaths.Count);
            foreach (string path in materialPaths)
                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(path));
            MaterialReplaceHelper.ReplaceMaterials(config, materials);
            if (!silence)
            {
                AssetDatabase.SaveAssets();
                string desc =
                    $"共替换{materialPaths.Count}个材质。" +
                    $"\n当前材质和引用搜索结果都已经被清空，因为换完Shader后已不符合当前条件。" +
                    $"\n如果有需要请重新搜索。" +
                    $"\n如果想要撤销修改，请在Git中手动还原。";
                EditorUtility.DisplayDialog("替换完成", desc, "OK");
                materialPaths.Clear();
                referenceMap?.Clear();
            }
        }

        private void RevertPrefabs(MaterialReplaceConfig config, List<MaterialLocation> failLocations)
        {
            for (int i = 0; i < config.matReplaceLocations.Count; i++)
            {
                var location = config.matReplaceLocations[i];
                if (!location.assetPath.EndsWith(".prefab"))
                    continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(location.assetPath);
                if (!prefab)
                {
                    failLocations.Add(location);
                    continue;
                }

                Transform componentTs = prefab.transform.Find(location.componentPath);
                if (!componentTs)
                {
                    failLocations.Add(location);
                    continue;
                }

                Renderer renderer = componentTs.GetComponent<Renderer>();
                if (!renderer)
                {
                    failLocations.Add(location);
                    continue;
                }

                var sms = renderer.sharedMaterials;
                if (sms.Length < location.materialIndex)
                {
                    failLocations.Add(location);
                    continue;
                }

                var mat = AssetDatabase.LoadAssetAtPath<Material>(location.matPath);
                if (!mat)
                {
                    failLocations.Add(location);
                    continue;
                }

                sms[location.materialIndex] = mat;
                renderer.sharedMaterials = sms;

                if (i + 1 == config.matReplaceLocations.Count
                    || config.matReplaceLocations[i + 1].assetPath != location.assetPath)
                    PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        private static void RevertScenes(MaterialReplaceConfig config, List<MaterialLocation> failLocations)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (int i = 0; i < config.matReplaceLocations.Count; i++)
            {
                var location = config.matReplaceLocations[i];

                if (!location.assetPath.EndsWith(".unity"))
                    continue;
                Scene scene = SceneManager.GetActiveScene().path != location.assetPath
                    ? EditorSceneManager.OpenScene(location.assetPath)
                    : SceneManager.GetActiveScene();

                GameObject componentGO = GameObject.Find(location.componentPath);
                if (!componentGO)
                {
                    failLocations.Add(location);
                    continue;
                }

                Renderer renderer = componentGO.GetComponent<Renderer>();
                if (!renderer)
                {
                    failLocations.Add(location);
                    continue;
                }

                var sms = renderer.sharedMaterials;
                if (sms.Length < location.materialIndex)
                {
                    failLocations.Add(location);
                    continue;
                }

                var mat = AssetDatabase.LoadAssetAtPath<Material>(location.matPath);
                if (!mat)
                {
                    failLocations.Add(location);
                    continue;
                }

                sms[location.materialIndex] = mat;
                renderer.sharedMaterials = sms;

                if (i + 1 == config.matReplaceLocations.Count
                    || config.matReplaceLocations[i + 1].assetPath != location.assetPath)
                    EditorSceneManager.SaveScene(scene, scene.path);
            }
        }

        private void ReplaceReferences(MaterialReplaceConfig config, HashSet<string> scenes, HashSet<string> prefabs)
        {
            HashSet<string> matSet = new HashSet<string>(materialPaths);
            AssetDatabase.StartAssetEditing();
            foreach (string refPath in scenes)
                ModifyScene(config.matReplaceLocations, refPath, matSet, GetCopyMatPath);
            foreach (string refPath in prefabs)
                ModifyPrefab(config.matReplaceLocations, refPath, matSet, GetCopyMatPath);
            AssetDatabase.StopAssetEditing();
        }

        private void ModifyPrefab(List<MaterialLocation> locations, string refPath, HashSet<string> targets,
            Func<string, string> newPathGetter)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(refPath);
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            bool prefabDirty = false;
            foreach (Renderer renderer in renderers)
            {
                var sms = renderer.sharedMaterials;
                bool rendererDirty = false;
                for (int i = 0; i < sms.Length; i++)
                {
                    Material m = sms[i];
                    if (m)
                    {
                        string mp = AssetDatabase.GetAssetPath(m);
                        if (targets.Contains(mp))
                        {
                            string newPath = newPathGetter(mp);
                            Material nm = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                            sms[i] = nm;
                            rendererDirty = true;
                            string componentPath = EditorCommon.GetSceneObjectPath(renderer.transform, true);
                            locations.Add(new MaterialLocation(mp, refPath, componentPath, i));
                        }
                    }
                }

                if (rendererDirty)
                {
                    renderer.sharedMaterials = sms;
                }
            }

            if (prefabDirty)
            {
                EditorUtility.SetDirty(prefab);
            }
        }

        private static void ModifyScene(List<MaterialLocation> locations, string refPath, HashSet<string> targets,
            Func<string, string> newPathGetter)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            Scene scene = EditorSceneManager.OpenScene(refPath);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    var sms = renderer.sharedMaterials;
                    bool rendererDirty = false;
                    for (int i = 0; i < sms.Length; i++)
                    {
                        Material m = sms[i];
                        if (m)
                        {
                            string mp = AssetDatabase.GetAssetPath(m);
                            if (targets.Contains(mp))
                            {
                                string newPath = newPathGetter(mp);
                                Material nm = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                                sms[i] = nm;
                                rendererDirty = true;
                                string componentPath = EditorCommon.GetSceneObjectPath(renderer.transform, true);
                                locations.Add(new MaterialLocation(mp, refPath, componentPath, i));
                            }
                        }
                    }

                    if (rendererDirty)
                    {
                        renderer.sharedMaterials = sms;
                        EditorUtility.SetDirty(renderer);
                    }
                }
            }

            EditorSceneManager.SaveScene(scene);
        }

        private static string GetCopyMatPath(string srcPath)
        {
            return newMaterialDir + srcPath.Substring("Assets".Length);
        }

        private void DrawSerachConfigs()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(GUIContent.none, GUILayout.ExpandHeight(true), GUILayout.Width(2));

            #region Materials page

            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            
            #region Replace config group

            DrawTitle("1.选择要替换的Shader");
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            group.Value =
                EditorGUILayout.ObjectField("Shader组", @group.Value, typeof(MaterialReplaceGroup), false) as
                    MaterialReplaceGroup;
            if (EditorGUI.EndChangeCheck())
                materialPaths.Clear();
            if (GUILayout.Button("New", GUILayout.Width(50)))
                group.Value = CreateConfigAsset<MaterialReplaceGroup>();
            GUILayout.EndHorizontal();
            GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));
            #endregion
            
            DrawTitle("2.选择要替换的材质");

            #region Buttons

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加选中目录下的材质"))
            {
                if (Selection.activeObject && AssetDatabase.Contains(Selection.activeObject))
                {
                    string directoryAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if (Directory.Exists(directoryAssetPath))
                    {
                        List<string> result = MaterialReplaceHelper.FindMaterials(@group.Value, directoryAssetPath);
                        foreach (string path in result)
                        {
                            if (!materialPaths.Contains(path))
                            {
                                materialPaths.Add(path);
                            }
                        }
                    }
                }
            }
            if (GUILayout.Button("全工程搜索材质"))
            {
                string directoryAssetPath = "Assets";
                if (Directory.Exists(directoryAssetPath))
                {
                    List<string> result = MaterialReplaceHelper.FindMaterials(@group.Value, directoryAssetPath);
                    foreach (string path in result)
                    {
                        if (!materialPaths.Contains(path))
                        {
                            materialPaths.Add(path);
                        }
                    }
                }
            }

            if (GUILayout.Button("添加选中资源引用的材质"))
            {
                var objs = Selection.objects;
                foreach (UnityEngine.Object obj in objs)
                {
                    AddAssetDependencyMaterial(obj, @group.Value);
                }
            }

            if (GUILayout.Button("添加当前场景材质"))
            {
                string path = SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(path))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    AddAssetDependencyMaterial(obj, @group.Value);
                }
            }

            if (GUILayout.Button("清空列表"))
            {
                materialPaths.Clear();
            }

            GUILayout.EndHorizontal();

            #endregion

            materialsPage.Draw(materialPaths);
            if (!string.IsNullOrEmpty(toRemoveMaterialPath))
            {
                materialPaths.Remove(toRemoveMaterialPath);
                toRemoveMaterialPath = null;
            }

            GUILayout.EndVertical();

            #endregion

            GUILayout.Box(GUIContent.none, GUILayout.ExpandHeight(true), GUILayout.Width(2));

            GUILayout.EndHorizontal();
            
            
            #region Bottom buttons

            int count = materialPaths == null ? 0 : materialPaths.Count;
            bool isValid = MaterialReplaceHelper.IsValid(@group.Value, out List<string> invalidReason);
            if (!isValid)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Replace fail, see follow:");
                foreach (string reason in invalidReason)
                    stringBuilder.AppendLine(reason);
                EditorGUILayout.HelpBox(stringBuilder.ToString(), MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!isValid);
            if (@group.Value)
            {
                DrawTitle("3.批处理");

                if (GUILayout.Button($"3.替换属性 ({count}个材质)"))
                {
                    ReplaceProperties(@group.Value, false);
                }
            }

            EditorGUI.EndDisabledGroup();

            #endregion

        }

        private void AddAssetDependencyMaterial(Object obj, MaterialReplaceGroup group)
        {
            if (!AssetDatabase.Contains(obj))
                return;
            
            HashSet<Shader> shaders = new HashSet<Shader>();
            foreach (var config in group.configs)
                if (config && config.srcShader)
                    shaders.Add(config.srcShader);

            string path = AssetDatabase.GetAssetPath(obj);
            if (path.EndsWith(".mat"))
            {
                Material assetAsMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (assetAsMaterial && shaders.Contains(assetAsMaterial.shader))
                    materialPaths.Add(path);
            }
            else
            {
                string[] depPaths = AssetDatabase.GetDependencies(path, true);
                foreach (var depPath in depPaths)
                {
                    if (!depPath.EndsWith(".mat"))
                        continue;
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(depPath);
                    if (material && shaders.Contains(material.shader))
                    {
                        if (!materialPaths.Contains(depPath))
                        {
                            materialPaths.Add(depPath);
                        }
                    }
                }
            }
        }

        private static void DrawCenterLabel(string text)
        {
            EditorGUILayout.LabelField($"<Color=FFFFFFFF>{text}</Color>",
                new GUIStyle() {alignment = TextAnchor.MiddleCenter, richText = true}, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }

        private static void GUIMaterial(IList<string> paths, string path, int index)
        {
            GUILayout.BeginHorizontal();
            DrawLoadablePath(path);
            if (referenceMap != null && referenceMap.Count != 0)
            {
                if (selectedMaterialPath.Value == path)
                {
                    Color c = GUI.color;
                    GUI.color = Color.green;
                    if (GUILayout.Button("正在查看", GUILayout.MaxWidth(80)))
                        selectedMaterialPath.Value = null;
                    GUI.color = c;
                }
                else if (GUILayout.Button("查看引用", GUILayout.MaxWidth(80)))
                {
                    selectedMaterialPath.Value = path;
                }
            }

            if (GUILayout.Button("×", GUILayout.Width(23)))
                toRemoveMaterialPath = path;
            GUILayout.EndHorizontal();
        }

        private static void DrawLoadablePath(string path)
        {
            bool loaded = AssetDatabase.IsMainAssetAtPathLoaded(path);
            if (loaded)
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), false, GUILayout.ExpandWidth(true));
            }
            else if (GUILayout.Button(path))
            {
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
        }

        private static void GUIReference(IList<string> paths, string path, int index)
        {
            bool loaded = AssetDatabase.IsMainAssetAtPathLoaded(path);
            if (loaded)
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                EditorGUILayout.ObjectField(asset, typeof(UnityEngine.Object), false);
            }
            else if (GUILayout.Button(path))
            {
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
        }

        private void DrawRepleaceConfigs()
        {
            #region Config line

            DrawTitle("替换资源配置");
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            savedConfig.Value =
                EditorGUILayout.ObjectField(savedConfig.Value, typeof(MaterialReplaceConfig), false) as
                    MaterialReplaceConfig;
            if (EditorGUI.EndChangeCheck())
            {
                RefreshPopupItems(savedConfig.Value);
            }

            if (GUILayout.Button("New", GUILayout.Width(50)))
                savedConfig.Value = CreateConfigAsset<MaterialReplaceConfig>();
            
            EditorGUI.BeginDisabledGroup(!configDirty);
            if (savedConfig.Value && GUILayout.Button("Save", GUILayout.Width(50)))
            {
                AssetDatabase.SaveAssets();
                configDirty = false;
            }
            EditorGUI.EndDisabledGroup();
            
            if (savedConfig.Value && savedConfig.Value.srcShader)
            {
                string oldPath = AssetDatabase.GetAssetPath(savedConfig.Value);
                string fileName = Path.GetFileNameWithoutExtension(oldPath);
                string shaderName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(savedConfig.Value.srcShader)); 
                if (fileName != shaderName
                    && GUILayout.Button("Rename", GUILayout.Width(50)))
                {
                    string newPath = $"{Path.GetDirectoryName(oldPath)}/{shaderName}.asset";
                    AssetDatabase.MoveAsset(oldPath, newPath);
                    savedConfig.Value.name = shaderName;
                }
            }
            
            EditorGUILayout.EndHorizontal();

            #endregion

            var config = savedConfig.Value;
            if (config)
            {
                EditorGUI.BeginChangeCheck();

                #region Calculate gui widths

                float windowEdge = 15f;
                float interval = 4f;
                float buttonWidth = 50f;
                float sideWithButtonWidth = (EditorGUIUtility.currentViewWidth - buttonWidth - 13) * 0.5f;
                float descWidth = 40;
                float nameWidth = sideWithButtonWidth - descWidth - interval;
                float shaderNameWidth = 16f;
                float shaderFieldWidth =
                    (EditorGUIUtility.currentViewWidth - windowEdge - (shaderNameWidth + interval) * 2) * 0.5f;

                #endregion

                #region Shader line

                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                GUILayout.Label("旧", GUILayout.Width(shaderNameWidth));
                Shader newSrcShader = EditorGUILayout.ObjectField(config.srcShader, typeof(Shader), false,
                    GUILayout.Width(shaderFieldWidth)) as Shader;
                if (EditorGUI.EndChangeCheck() && (config.properties.Count == 0 ||
                                                   EditorUtility.DisplayDialog("更换旧Shader",
                                                       "换Shader配置会导致配置好的属性清空。\n建议新建一个配置，确定继续更换吗?", "确定", "取消")))
                {
                    config.srcShader = newSrcShader;
                    srcPropertyNames = GetPopupItems(config, true);
                    config.properties.Clear();
                }

                EditorGUI.BeginChangeCheck();
                GUILayout.Label("新", GUILayout.Width(shaderNameWidth));
                var newDstShader = EditorGUILayout.ObjectField(config.dstShader, typeof(Shader), false,
                    GUILayout.Width(shaderFieldWidth)) as Shader;
                if (EditorGUI.EndChangeCheck() && (config.properties.Count == 0 ||
                                                   EditorUtility.DisplayDialog("更换新Shader",
                                                       "换Shader配置会导致配置好的属性清空。\n建议新建一个配置，确定继续更换吗?", "确定", "取消")))
                {
                    config.dstShader = newDstShader;
                    dstPropertyNames = GetPopupItems(config, false);
                    config.properties.Clear();
                }

                GUILayout.EndHorizontal();

                bool srcCompileError = config.srcShader && ShaderUtil.ShaderHasError(config.srcShader);
                if (srcCompileError)
                    EditorGUILayout.HelpBox("旧Shader有编译报错", MessageType.Error);
                bool dstCompileError = config.dstShader && ShaderUtil.ShaderHasError(config.dstShader);
                if (dstCompileError)
                    EditorGUILayout.HelpBox("新Shader有编译报错", MessageType.Error);
                if (srcCompileError || dstCompileError)
                    return;

                #endregion

                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));

                DrawCustomMaterialProcessor(savedConfig.Value);

                if (config.srcShader && config.dstShader)
                {
                    if (srcPropertyNames == null || dstPropertyNames == null)
                    {
                        RefreshPopupItems(config);
                    }

                    GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));

                    #region Add line

                    DrawTitle("添加替换属性");
                    if (srcPropertyNames.Length > 0 && dstPropertyNames.Length > 0)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            int index = Mathf.Clamp(Array.IndexOf(srcPropertyNames, srcSelectedAddName.Value), 0,
                                srcPropertyNames.Length - 1);
                            int newIndex = EditorGUILayout.Popup(index, srcPropertyNames,
                                GUILayout.Width(sideWithButtonWidth));
                            srcSelectedAddName.Value = srcPropertyNames[newIndex];
                        }
                        {
                            int index = Mathf.Clamp(Array.IndexOf(dstPropertyNames, dstSelectedAddName.Value), 0,
                                dstPropertyNames.Length - 1);
                            int newIndex = EditorGUILayout.Popup(index, dstPropertyNames,
                                GUILayout.Width(sideWithButtonWidth));
                            dstSelectedAddName.Value = dstPropertyNames[newIndex];
                        }
                        if (GUILayout.Button("添加", GUILayout.Width(buttonWidth)))
                        {
                            PropertyReplaceConfig prc = new PropertyReplaceConfig();
                            prc.srcName = srcSelectedAddName.Value;
                            prc.dstName = dstSelectedAddName.Value;
                            config.properties.Add(prc);
                            RefreshPopupItems(config);
                        }

                        GUILayout.EndHorizontal();
                    }

                    #endregion

                    GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(2));

                    #region Property pair lines

                    DrawTitle("替换属性列表");
                    propertyViewPos.Value = GUILayout.BeginScrollView(propertyViewPos.Value);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("src proeprty name", GUILayout.Width(nameWidth));
                    GUILayout.Label("desc", GUILayout.Width(descWidth));
                    GUILayout.Label("dst proeprty name", GUILayout.Width(nameWidth));
                    GUILayout.Label("desc", GUILayout.Width(descWidth));
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < config.properties.Count; i++)
                    {
                        GUILayout.BeginHorizontal();

                        void DrawWithDesc(ref string propertyName, string[] items, ref string desc)
                        {
                            int index = Array.IndexOf(items, propertyName);
                            int newIndex = Mathf.Clamp(index, 0, items.Length - 1);
                            newIndex = EditorGUILayout.Popup(newIndex, items, GUILayout.Width(nameWidth));
                            if (newIndex != index)
                                propertyName = items[newIndex];

                            if (!MaterialReplaceHelper.IsDescValid(desc))
                            {
                                Color c = GUI.color;
                                GUI.color = Color.red;
                                desc = EditorGUILayout.TextField(desc, GUILayout.Width(descWidth));
                                GUI.color = c;
                            }
                            else
                            {
                                desc = EditorGUILayout.TextField(desc, GUILayout.Width(descWidth));
                            }
                        }

                        void DrawPropertyNamePopup(ref string propertyName, string[] items)
                        {
                            int index = Array.IndexOf(items, propertyName);
                            int newIndex = Mathf.Clamp(index, 0, items.Length - 1);
                            newIndex = EditorGUILayout.Popup(newIndex, items, GUILayout.Width(sideWithButtonWidth));
                            if (newIndex != index)
                                propertyName = items[newIndex];
                        }

                        PropertyReplaceConfig prc = config.properties[i];
                        if (MaterialReplaceHelper.SupportDesc(config.srcShader, prc.srcName))
                            DrawWithDesc(ref prc.srcName, srcPropertyNames, ref prc.srcDesc);
                        else
                            DrawPropertyNamePopup(ref prc.srcName, srcPropertyNames);

                        if (MaterialReplaceHelper.SupportDesc(config.dstShader, prc.dstName))
                            DrawWithDesc(ref prc.dstName, dstPropertyNames, ref prc.dstDesc);
                        else
                            DrawPropertyNamePopup(ref prc.dstName, dstPropertyNames);

                        if (GUILayout.Button("Delete", GUILayout.Width(buttonWidth)))
                        {
                            config.properties.RemoveAt(i--);
                            RefreshPopupItems(config);
                        }

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndScrollView();

                    #endregion
                }
            }
            
            if (EditorGUI.EndChangeCheck() && savedConfig.Value)
            {
                EditorUtility.SetDirty(savedConfig.Value);
                configDirty = true;
            }
        }

        private void RefreshPopupItems(MaterialReplaceConfig config)
        {
            srcPropertyNames = GetPopupItems(config, true);
            dstPropertyNames = GetPopupItems(config, false);
        }

        private string[] GetPopupItems(MaterialReplaceConfig config, bool isSrc)
        {
            Shader shader = isSrc ? config.srcShader : config.dstShader;

            if (!shader)
                return null;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
            List<string> names = new List<string>();
            for (int i = 0; i < propertyCount; i++)
            {
                string name = ShaderUtil.GetPropertyName(shader, i);
                names.Add(name);
            }

            return names.ToArray();
        }

        private static T CreateConfigAsset<T>() where T : ScriptableObject
        {
            string path;
            int index = 0;
            do
            {
                path = $"Assets/Editor/EditorResources/{typeof(T).Name}/{typeof(T).Name}_{index++}.asset";
            } while (File.Exists(path));

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            T instance = CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }

    public static class MaterialReplaceHelper
    {
        private class PropertyValue
        {
            private PropertyType type;
            private float floatValue;
            private Vector4 vector4Value;
            private Color colorValue;
            private Texture textureValue;
            private Vector4 float4Mask;

            public static PropertyValue GetPropertyValue(Material material, string propertyName, string desc)
            {
                PropertyValue property = new PropertyValue();
                ShaderUtility.GetPropertyInfo(material.shader, propertyName, out var info);
                property.type = info.type;
                switch (property.type)
                {
                    case PropertyType.Color:
                        if (string.IsNullOrEmpty(desc))
                        {
                            property.colorValue = material.GetColor(propertyName);
                        }
                        else
                        {
                            Color value = material.GetColor(propertyName);
                            property.float4Mask = property.GetMaskValue(value, desc);
                        }

                        break;
                    case PropertyType.Vector:
                        if (string.IsNullOrEmpty(desc))
                        {
                            property.vector4Value = material.GetVector(propertyName);
                        }
                        else
                        {
                            Vector4 value = material.GetVector(propertyName);
                            property.float4Mask = property.GetMaskValue(value, desc);
                        }

                        break;
                    case PropertyType.Float:
                        property.floatValue = material.GetFloat(propertyName);
                        break;
                    case PropertyType.Range:
                        property.floatValue = material.GetFloat(propertyName);
                        break;
                    case PropertyType.TexEnv:
                        property.textureValue = material.GetTexture(propertyName);
                        break;
                    default:
                        break;
                }

                return property;
            }

            private Vector4 GetMaskValue(Vector4 value, string desc)
            {
                Vector4 result = default;
                if (desc[0] == '.')
                {
                    for (int i = 1; i < desc.Length; i++)
                    {
                        int maskIndex = GetMaskIndex(desc[i]);
                        result[i - 1] = value[maskIndex];
                    }
                }

                return result;
            }

            private Vector4 SetMaskValue(Vector4 rawValue, string desc)
            {
                if (desc[0] == '.')
                {
                    for (int i = 1; i < desc.Length; i++)
                    {
                        int maskIndex = GetMaskIndex(desc[i]);
                        rawValue[maskIndex] = float4Mask[i - 1];
                    }
                }

                return rawValue;
            }

            public bool ApplyPropertyData(MaterialReplaceConfig config, Material material, string name, string desc)
            {
                ShaderUtility.GetPropertyInfo(config.dstShader, name, out var dstPropertyInfo);
                switch (type)
                {
                    case PropertyType.Color:
                        // Color to float
                        if (dstPropertyInfo.type == PropertyType.Float || dstPropertyInfo.type == PropertyType.Range)
                        {
                            material.SetFloat(name, float4Mask.x);
                        }
                        // Color to color
                        else if (dstPropertyInfo.type == PropertyType.Color)
                        {
                            // Full
                            if (string.IsNullOrEmpty(desc))
                            {
                                material.SetColor(name, colorValue);
                            }
                            // Masked
                            else
                            {
                                Color rawValue = material.GetColor(name);
                                Color newValue = SetMaskValue(rawValue, desc);
                                material.SetColor(name, newValue);
                            }
                        }
                        // Color to vector
                        else if (dstPropertyInfo.type == PropertyType.Vector)
                        {
                            // Full
                            if (string.IsNullOrEmpty(desc))
                            {
                                material.SetVector(name, colorValue);
                            }
                            // Masked
                            else
                            {
                                Vector4 rawValue = material.GetVector(name);
                                Vector4 newValue = SetMaskValue(rawValue, desc);
                                material.SetVector(name, newValue);
                            }
                        }

                        break;
                    case PropertyType.Vector:
                        // Vector to float
                        if (dstPropertyInfo.type == PropertyType.Float || dstPropertyInfo.type == PropertyType.Range)
                        {
                            material.SetFloat(name, float4Mask.x);
                        }
                        // Vector to color
                        else if (dstPropertyInfo.type == PropertyType.Color)
                        {
                            // Full
                            if (string.IsNullOrEmpty(desc))
                            {
                                material.SetColor(name, vector4Value);
                            }
                            // Masked
                            else
                            {
                                Vector4 rawValue = material.GetColor(name);
                                Vector4 newValue = SetMaskValue(rawValue, desc);
                                material.SetColor(name, newValue);
                            }
                        }
                        // Vector to vector
                        else if (dstPropertyInfo.type == PropertyType.Vector)
                        {
                            // Full
                            if (string.IsNullOrEmpty(desc))
                            {
                                material.SetVector(name, vector4Value);
                            }
                            // Masked
                            else
                            {
                                Vector4 rawValue = material.GetVector(name);
                                Vector4 newValue = SetMaskValue(rawValue, desc);
                                material.SetVector(name, newValue);
                            }
                        }

                        break;
                    case PropertyType.Float:
                        // Float to color
                        if (dstPropertyInfo.type == PropertyType.Color)
                        {
                            int maskIndex = GetMaskIndex(desc[1]);
                            Color color = material.GetColor(name);
                            color[maskIndex] = floatValue;
                            material.SetColor(name, color);
                        }
                        // Float to vector
                        else if (dstPropertyInfo.type == PropertyType.Vector)
                        {
                            int maskIndex = GetMaskIndex(desc[1]);
                            Vector4 vector = material.GetVector(name);
                            vector[maskIndex] = floatValue;
                            material.SetVector(name, vector);
                        }
                        // Float to float
                        else
                        {
                            material.SetFloat(name, floatValue);
                        }

                        break;
                    case PropertyType.Range:
                        material.SetFloat(name, floatValue);
                        break;
                    case PropertyType.TexEnv:
                        material.SetTexture(name, textureValue);
                        break;
                    default:
                        return false;
                }

                return true;
            }
        }

        public static SavedBool isProgressBarRunning =
            new SavedBool($"{nameof(MaterialReplaceTool)}.{nameof(isProgressBarRunning)}");

        private static int GetMaskIndex(char c)
        {
            return "xrygzbwa".IndexOf(c) / 2;
        }

        public static bool SupportDesc(Shader shader, string property)
        {
            ShaderUtility.GetPropertyInfo(shader, property, out var propertyInfo);
            return propertyInfo.type == PropertyType.Color || propertyInfo.type == PropertyType.Vector;
        }

        public static bool IsValid(MaterialReplaceGroup group, out List<string> invalidReason)
        {
            if (!group)
            {
                invalidReason = new List<string>() {"Shader组未设置"};
                return false;
            }
            
            foreach (var config in group.configs)
            {
                if (!IsValid(config, out invalidReason))
                {
                    return false;
                }
            }

            invalidReason = new List<string>();
            return true;
        }

        public static bool IsValid(MaterialReplaceConfig config, out List<string> invalidReason)
        {
            invalidReason = new List<string>();
            if (!config)
            {
                invalidReason.Add("config is null.");
                return false;
            }

            if (!config.srcShader)
            {
                invalidReason.Add("srcShader is null.");
            }

            if (!config.dstShader)
            {
                invalidReason.Add("dstShader is null.");
            }

            if (config.properties == null)
            {
                invalidReason.Add("properties is null.");
            }

            if (invalidReason.Count == 0)
            {
                ShaderUtility.GetShaderInfo(config.srcShader, out var srcShaderInfo);
                ShaderUtility.GetShaderInfo(config.dstShader, out var dstShaderInfo);
                for (int i = 0; i < config.properties.Count; i++)
                {
                    PropertyReplaceConfig prc = config.properties[i];
                    bool srcPropertyExist = srcShaderInfo.properties.TryGetValue(prc.srcName, out var srcPropertyInfo);
                    if (!srcPropertyExist)
                    {
                        invalidReason.Add($"src.properties[{i}] does not exist.");
                    }

                    bool dstPropertyExist = dstShaderInfo.properties.TryGetValue(prc.dstName, out var dstPropertyInfo);
                    if (!dstPropertyExist)
                    {
                        invalidReason.Add($"dstPropertyInfo[{i}] does not exist.");
                    }

                    if (srcPropertyExist && dstPropertyExist)
                    {
                        bool isSrcTexture = srcPropertyInfo.type == PropertyType.TexEnv;
                        bool isDstTexture = dstPropertyInfo.type == PropertyType.TexEnv;
                        if (isSrcTexture != isDstTexture)
                        {
                            invalidReason.Add(
                                $"property type not match, index = {i}, srcName = {srcPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                        }
                    }

                    bool srcSupportDesc = srcPropertyInfo.type == PropertyType.Vector ||
                                          srcPropertyInfo.type == PropertyType.Color;
                    bool srcDescValid = IsDescValid(prc.srcDesc);
                    if (srcSupportDesc && !srcDescValid)
                    {
                        invalidReason.Add(
                            $"property src desc incorrect, index = {i}, srcName = {srcPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                    }

                    bool dstSupportDesc = dstPropertyInfo.type == PropertyType.Vector ||
                                          dstPropertyInfo.type == PropertyType.Color;
                    bool dstDescValid = IsDescValid(prc.dstDesc);
                    if (dstSupportDesc && !dstDescValid)
                    {
                        invalidReason.Add(
                            $"property src desc incorrect, index = {i}, srcName = {dstPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                    }

                    if (srcSupportDesc && dstSupportDesc && srcDescValid && dstDescValid)
                    {
                        if (prc.srcDesc.Length != prc.dstDesc.Length)
                        {
                            invalidReason.Add(
                                $"property desc length not match, index = {i}, srcName = {dstPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                        }
                    }

                    if (srcPropertyExist && dstPropertyExist)
                    {
                        bool isSrcFloat = srcPropertyInfo.type == PropertyType.Float ||
                                          srcPropertyInfo.type == PropertyType.Range;
                        bool isDstFloat = dstPropertyInfo.type == PropertyType.Float ||
                                          dstPropertyInfo.type == PropertyType.Range;
                        bool isSrcVector = srcPropertyInfo.type == PropertyType.Vector ||
                                           srcPropertyInfo.type == PropertyType.Color;
                        bool isDstVector = dstPropertyInfo.type == PropertyType.Vector ||
                                           dstPropertyInfo.type == PropertyType.Color;
                        if (isSrcFloat && isDstVector && (!dstDescValid || string.IsNullOrEmpty(prc.dstDesc) ||
                                                          prc.dstDesc.Length != 2))
                        {
                            invalidReason.Add(
                                $"float to float4 component desc not match, index = {i}, srcName = {srcPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                        }
                        else if (isSrcVector && isDstFloat && (!srcDescValid || string.IsNullOrEmpty(prc.srcDesc) ||
                                                               prc.srcDesc.Length != 2))
                        {
                            invalidReason.Add(
                                $"float4 component to float desc not match, index = {i}, srcName = {srcPropertyInfo.name}, dstName = {dstPropertyInfo.name}.");
                        }
                    }
                }
            }

            return invalidReason.Count == 0;
        }

        public static bool IsDescValid(string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return true;

            if (desc[0] == '.')
            {
                if (desc.Length <= 1 && desc.Length > 5)
                {
                    return false;
                }

                for (int i = 1; i < desc.Length; i++)
                {
                    int maskIndex = GetMaskIndex(desc[i]);
                    if (maskIndex < 0)
                        return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Ensure that config already checked by MaterialReplaceHelper.IsValid.
        /// </summary>
        public static void ReplaceMaterials(MaterialReplaceConfig config, IList<Material> materials)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                float progress = i / (float) materials.Count;
                Material material = materials[i];
                isProgressBarRunning.Value = true;
                if (EditorUtility.DisplayCancelableProgressBar("Replace materials",
                    $"({progress}/{materials.Count}) {AssetDatabase.GetAssetPath(material)})", progress))
                    break;
                if (material.shader == config.srcShader)
                {
                    ReplaceMateiral(config, material);
                }
            }

            EditorUtility.ClearProgressBar();
            isProgressBarRunning.Value = false;
        }

        /// <summary>
        /// Ensure that config already checked by MaterialReplaceHelper.IsValid.
        /// </summary>
        private static void ReplaceMateiral(MaterialReplaceConfig config, Material material)
        {
            List<PropertyValue> propertyDatas = new List<PropertyValue>(config.properties.Count);

            // Get property data.
            foreach (PropertyReplaceConfig prc in config.properties)
                propertyDatas.Add(PropertyValue.GetPropertyValue(material, prc.srcName, prc.srcDesc));


            config.Processor?.PreProcessMaterial(material);
            // Replace shader
            material.shader = config.dstShader;

            // Apply property value to new property name
            for (int i = 0; i < propertyDatas.Count; i++)
            {
                var prc = config.properties[i];
                propertyDatas[i].ApplyPropertyData(config, material, prc.dstName, prc.dstDesc);
            }

            config.Processor?.PostProcessMaterial(material);

            MaterialDeserizer md = new MaterialDeserizer(material);
            md.RemoveUnusedProperties();
            md.Apply();

            EditorUtility.SetDirty(material);
        }

        public static List<string> FindMaterials(MaterialReplaceGroup group, string folder)
        {
            HashSet<Shader> shaders = new HashSet<Shader>();
            foreach (var config in group.configs)
                if (config && config.srcShader)
                    shaders.Add(config.srcShader);
            
            List<string> result = new List<string>();
            string[] materials = AssetDatabase.FindAssets("t:material", new string[] {folder});
            for (int i = 0; i < materials.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(materials[i]);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (shaders.Contains(material.shader))
                {
                    result.Add(path);
                }
            }

            return result;
        }
    }

    public static class ReferenceUtility
    {
        /// <summary>
        /// Find reference list for each asset as dictionary.
        /// </summary>
        /// <param name="resources">assets that want to find reference</param>
        /// <param name="refFolders">key is input resources, value is reference list.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> FindReferencesForAssets(IList<string> resources,
            List<string> refFolders)
        {
            HashSet<string> tempSet = new HashSet<string>();
            foreach (var item in resources)
                tempSet.Add(item);
            string[] guids = AssetDatabase.FindAssets("", refFolders.ToArray());

            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string[] dependencies = AssetDatabase.GetDependencies(path);
                foreach (string dep in dependencies)
                {
                    if (tempSet.Contains(dep))
                    {
                        if (!map.TryGetValue(dep, out var refList))
                        {
                            refList = new List<string>();
                            map[dep] = refList;
                        }

                        refList.Add(path);
                    }
                }

                if (i % 10 == 0)
                {
                    MaterialReplaceHelper.isProgressBarRunning.Value = true;
                    if (EditorUtility.DisplayCancelableProgressBar("查找引用", path, i / (float) guids.Length))
                        break;
                }
            }

            MaterialReplaceHelper.isProgressBarRunning.Value = false;
            EditorUtility.ClearProgressBar();
            return map;
        }
    }

    public abstract class MaterialReplaceProcessor
    {
        /// <summary>
        /// 界面上的备注名，用于Popup显示。格式：{DisplayName} ({GetType().FullName})
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// 在修改材质之前的回调。
        /// </summary>
        public virtual void PreProcessMaterial(Material material)
        {
        }

        /// <summary>
        /// 在修改材质之后的回调。
        /// </summary>
        public virtual void PostProcessMaterial(Material material)
        {
        }
    }
}
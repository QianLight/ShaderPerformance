using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(BandposeData))]
    public class BandposeDataEditor : UnityEngineEditor
    {
        private bool previewHeightGradient;
        private GUIStyle style;
        private GUIContent shaderContent = new GUIContent("Shader");
        private GUIContent shaderNameContent = new GUIContent("name");
        private static CustomShaderSelectionDropdown dropdown;
        BandposeData bd;
        GameObject viewRoot;

        List<string> prefabName = new List<string>();
        int artistSelect = -1;
        int playSelected = -1;
        int designerSelected = -1;

        private static GUIStyle redText = null;
        // private static GUIStyle whiteText = null;
        private static List<BandposeData> dirtyBandposeList = new List<BandposeData>();

        [InitializeOnLoadMethod]
        private static void RegisterEditorUpdate()
        {
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (dirtyBandposeList.Count > 0)
            {
                ForeceAttachDependancyAssets();
            }
        }

        [MenuItem("Tools/角色/检查Prefab重名资源")]
        private static void CheckAssetNameConflict()
        {
            CheckAssetNameConflict(false);
        }
        
        private static void CheckAssetNameConflict(bool dialog)
        {
            Dictionary<string, HashSet<string>> assetNameMap = new Dictionary<string, HashSet<string>>();
            string[] prefabResGUIDList = AssetDatabase.FindAssets($"t:{nameof(CFEngine.PrefabRes)}");

            void CheckAssetNameUnique(string assetPath, string extension)
            {
                if (!assetPath.EndsWith(extension))
                    assetPath += extension;
                string fileName = Path.GetFileName(assetPath);
                assetNameMap.ForceGetValue(fileName).Add(assetPath);
            }

            for (int i = 0; i < prefabResGUIDList.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabResGUIDList[i]);
                
                // prefabRes的name和prefabName相同，所以这里直接用prefabRes的fileName来判断Prefab重名。
                CheckAssetNameUnique(assetPath, ".asset");
                
                // 检查mesh和material重名
                CFEngine.PrefabRes prefabRes = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes>(assetPath);
                foreach (EditorMeshInfo meshInfo in prefabRes.meshes)
                {
                    CheckAssetNameUnique(meshInfo.meshPath, ".asset");
                    CheckAssetNameUnique(meshInfo.matPath, ".mat");
                }
            }
            
            StringBuilder stringBuilder = new StringBuilder();
            int index = 0;
            foreach (KeyValuePair<string, HashSet<string>> kvp in assetNameMap)
            {
                if (kvp.Value.Count > 1)
                {
                    stringBuilder.AppendLine($"冲突文件名[{index++}]: {kvp.Key}");
                    int subIndex = 0;
                    foreach (string conflictAssetPath in kvp.Value)
                        stringBuilder.AppendLine($"\t重名路径[{subIndex++}]: {conflictAssetPath}");
                    Debug.Log(stringBuilder.ToString());
                    stringBuilder.Clear();       
                }
            }
        }

        private static void ForeceAttachDependancyAssets()
        {
            AssetDatabase.StartAssetEditing();
            foreach (BandposeData bd in dirtyBandposeList)
            {
                bool CheckAndModifyRenderer(int i, GameObject prefab, EditorMeshInfo m, Mesh mesh)
                {
                    if (!prefab)
                    {
                        Debug.LogError($"Prefab is null", bd);
                        return false;
                    }

                    if (i >= prefab.transform.childCount)
                    {
                        Debug.LogError(
                            $"Child index >= child count, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {prefab.name}, mesh index = {i}",
                            bd);
                        return false;
                    }

                    if (!mesh)
                    {
                        Debug.LogError(
                            $"Refresh mesh fail, mesh is null, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {prefab.name}, mesh index = {i}",
                            bd);
                    }

                    Transform child = prefab.transform.GetChild(i);
                    if (m.isSkin)
                    {
                        SkinnedMeshRenderer smr = child.GetComponent<SkinnedMeshRenderer>();
                        if (!smr)
                        {
                            Debug.LogError(
                                $"Get skinned mesh renderer fail, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {prefab.name}, mesh index = {i}",
                                bd);
                            return false;
                        }

                        smr.sharedMesh = mesh;
                        smr.enabled = m.active;
                    }
                    else
                    {
                        MeshFilter mf = child.GetComponent<MeshFilter>();
                        if (!mf)
                        {
                            Debug.LogError(
                                $"Get skinned mesh filter fail, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {prefab.name}, mesh index = {i}",
                                bd);
                            return false;
                        }

                        MeshRenderer mr = child.GetComponent<MeshRenderer>();
                        if (!mr)
                        {
                            Debug.LogError(
                                $"Get skinned mesh renderer fail, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {prefab.name}, mesh index = {i}",
                                bd);
                            return false;
                        }

                        mr.enabled = m.active;

                        mf.sharedMesh = mesh;
                    }

                    EditorUtility.SetDirty(prefab);
                    string path = AssetDatabase.GetAssetPath(prefab);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                    return true;
                }

                if (bd.exportConfig == null)
                {
                    Debug.LogError($"Export config is null, bandpose = {AssetDatabase.GetAssetPath(bd)}", bd);
                    continue;
                }

                foreach (PrefabExportConfig ec in bd.exportConfig)
                {
                    if (!ec.prefabRef)
                    {
                        Debug.LogError(
                            $"PrefabRef missing, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {ec.prefabName}", bd);
                        continue;
                    }

                    if (!ec.res)
                    {
                        Debug.LogError(
                            $"Prefab res missing, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {ec.prefabName}", bd);
                        continue;
                    }

                    if (ec.res.meshes == null)
                    {
                        Debug.LogError(
                            $"Prefab res meshes missing, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {ec.prefabName}",
                            bd);
                        continue;
                    }

                    for (int i = 0; i < ec.res.meshes.Length; i++)
                    {
                        EditorMeshInfo m = ec.res.meshes[i];
                        string meshPath = m.meshPath + ".asset";
                        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                        if (!mesh)
                        {
                            Debug.LogError(
                                $"Could not load mesh, bandpose = {AssetDatabase.GetAssetPath(bd)}, prefab = {ec.prefabName}, meshPath = {meshPath}");
                            continue;
                        }

                        CheckAndModifyRenderer(i, ec.prefabRef, m, mesh);
                    }
                }
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            dirtyBandposeList.Clear();
        }

        public GUIStyle GetRedTextSyle()
        {
            if (redText == null)
            {
                redText = new GUIStyle("Label");
                redText.normal.textColor = Color.red;
            }
            return redText;
        }

        private void ItemSelected(CustomShaderSelectionDropdown dropdown)
        {
            if (dropdown.SelectShader != null)
                dropdown.mat.shader = dropdown.SelectShader;
        }

        public override void OnInspectorGUI()
        {
            if (style == null)
            {
                style = new GUIStyle("MiniPulldown");
            }
            bd = target as BandposeData;

            EditorGUILayout.ObjectField("FbxRef", bd.fbxRef, typeof(GameObject), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("onekey", GUILayout.MaxWidth(80)))
            {
                Onekey(bd, false);
            }

            if (GUILayout.Button("Reset", GUILayout.MaxWidth(80)))
            {
                bd.RefreshMaterial();
            }
            if (GUILayout.Button("Rebind", GUILayout.MaxWidth(100)))
            {
                bd.RefreshMaterial(false);
            }
            if (GUILayout.Button("Export", GUILayout.MaxWidth(80)))
            {
                bd.RefreshMaterial();
                bd.Export();
            }

            if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
            {
                EditorCommon.SaveAsset(bd);
            }

            if (GUILayout.Button("Batch", GUILayout.MaxWidth(80)))
            {
                BandposeDataBatchEditor.Open();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            DrawEditorPrefabInspector();

            DrawArtistInspector();
            DrawDesignerInspector();
            DrawPlayInspector();
            DrawDetailInspector();
        }

        public static void Onekey(BandposeData bd, bool batch)
        {
            bd.RefreshMaterial();
            bd.Export();
            bd.MakePrefab(-1, -1, true, true, batch);
            EditorCommon.SaveAsset(bd);
            dirtyBandposeList.Add(bd);
        }

        private void DrawEditorPrefabInspector()
        {
            if (EditorCommon.BeginFolderGroup("-------------------------------------编辑用Prefab-------------------------------------------", ref bd.EditorPrefabFolder))
            {
                GUILayout.BeginHorizontal();

                bd.editorPrefab = EditorGUILayout.ObjectField(bd.editorPrefab, typeof(GameObject), false) as GameObject;

                if (!bd.editorPrefab)
                {
                    if (GUILayout.Button("创建Prefab", GUILayout.Width(70)))
                    {
                        string configPath = AssetDatabase.GetAssetPath(bd);
                        string fileName = Path.GetFileNameWithoutExtension(configPath).Replace("_bandpose", string.Empty);
                        string directory = Path.GetDirectoryName(Path.GetDirectoryName(configPath));
                        string editorPrefabPath = $"{directory}/{fileName}_editor_prefab.prefab".Replace('\\', '/');
                        GameObject instance = Instantiate(bd.fbxRef);
                        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, editorPrefabPath, out bool success);
                        if (success)
                        {
                            bd.editorPrefab = prefab;
                        }
                        DestroyImmediate(instance);
                    }
                }
                else
                {
                    if (GUILayout.Button("创建实例", GUILayout.Width(70)))
                    {
                        GameObject instance = PrefabUtility.InstantiatePrefab(bd.editorPrefab) as GameObject;
                        Selection.activeGameObject = instance;
                    }
                }

                GUILayout.EndHorizontal();
                EditorCommon.EndFolderGroup();
            }
        }

        private bool DrawToggleMinimal(string context, bool value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(context, GUILayout.Width(EditorGUIUtility.labelWidth));
            bool result = EditorGUILayout.Toggle(value, GUILayout.Width(46));
            EditorGUILayout.Space(10, true);
            EditorGUILayout.EndHorizontal();
            return result;
        }

        public void DrawArtistInspector()
        {
            if (EditorCommon.BeginFolderGroup("-------------------------------------模型美术专区-------------------------------------------", ref bd.ArtistFolder))
            {
                EditorGUI.indentLevel++;
                
                EditorCommon.FoldoutGroup<BandposeDataEditor>("功能设置", DrawModelBaseSetting);
                if (FillModelPartConfigs())
                    EditorCommon.FoldoutGroup<BandposeDataEditor>("Mesh配置", DrawModelInspector);
                EditorCommon.FoldoutGroup<BandposeDataEditor>("facemap部件", DrawFaceParts);
                EditorCommon.FoldoutGroup<BandposeDataEditor>("cloak部件", DrawCloakParts);
                EditorCommon.FoldoutGroup<BandposeDataEditor>("高度渐变", DrawHeightGradient);
                EditorCommon.FoldoutGroup<BandposeDataEditor>("Prefab", DrawPrefabConfigs);
                
                EditorGUI.indentLevel--;

                EditorCommon.EndFolderGroup();
            }
        }

        private void DrawModelBaseSetting()
        {
            bd.needUV2 = DrawToggleMinimal("NeedUV2(石化)", bd.needUV2);
            bd.needOutline = DrawToggleMinimal("NeedOutline(描边)", bd.needOutline);
            bd.renameMesh = DrawToggleMinimal("RenameMesh(场景物件mesh自动命名)", bd.renameMesh);
            bd.sceneMesh = DrawToggleMinimal("SceneMesh(复用场景物件的mesh)", bd.sceneMesh);
            bd.removeAnimator = DrawToggleMinimal("RemoveAnimator(这个prefab不需要animator)", bd.removeAnimator);
            bd.testFade = DrawToggleMinimal("FadeEffect(是否半透明)", bd.testFade);
            bd.weightedNormal = DrawToggleMinimal("weightedNormal(加权法线)", bd.weightedNormal);
        }

        private void DrawPrefabConfigs()
        {
            if (!bd.IsExportCorrectly())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("还未导出mesh", GetRedTextSyle(), new GUILayoutOption[] {GUILayout.ExpandWidth(true)});
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                DrawPrefabInspector(0, ref artistSelect);
            }
        }

        private void DrawHeightGradient()
        {
            EditorGUI.BeginChangeCheck();
            HeightGradient gradient = bd.gradient;
            EditorGUI.BeginChangeCheck();
            previewHeightGradient = EditorGUILayout.Toggle("预览参数效果", previewHeightGradient);
            DrawHeightGradient(gradient);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                bd.SetAllPrefabDirty(true);

                RefreshGradientPreview(gradient);
            }
        }

        private void DrawModelInspector()
        {
            void DrawMeshConfig(ModelPartConfig mpc)
            {
                bool DrawFeatureToggle(string label, uint flag)
                {
                    bool lastValue = mpc.flags.HasFlag(flag);
                    EditorGUI.BeginChangeCheck();
                    bool newValue = EditorGUILayout.Toggle(label, lastValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        mpc.flags.SetFlag(flag, newValue);
                        foreach (PrefabExportConfig pec in bd.exportConfig)
                            pec.dirty = true;
                        EditorUtility.SetDirty(bd);
                    }

                    return newValue;
                }

                // Vertex Color
                bool deleteColor = DrawFeatureToggle("Delete Vertex Color", ModelPartConfig.Flag_DeleteColor);
                EditorGUI.BeginDisabledGroup(deleteColor);
                if (deleteColor)
                {
                    mpc.flags.SetFlag(ModelPartConfig.Flag_Outline, false);
                    mpc.flags.SetFlag(ModelPartConfig.Flag_HeightGradient, false);
                }
                DrawFeatureToggle("Outline (Color.rgb)", ModelPartConfig.Flag_Outline);
                DrawFeatureToggle("Height Gradient (Color.a)", ModelPartConfig.Flag_HeightGradient);
                EditorGUI.EndDisabledGroup();
            }

            foreach (ModelPartConfig mpc in bd.modelPartConfigs)
            {
                EditorCommon.FoldoutGroup<BandposeDataEditor>(mpc.partName, () => DrawMeshConfig(mpc));
            }
        }

        private bool FillModelPartConfigs()
        {
            GameObject fbx = bd.fbxRef;
            if (!fbx)
                return false;

            List<string> meshNames = new List<string>();

            string fbxPath = AssetDatabase.GetAssetPath(fbx);
            if (string.IsNullOrEmpty(fbxPath))
                return false;

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (Object asset in assets)
            {
                Mesh mesh = asset as Mesh;
                if (mesh)
                    meshNames.Add(mesh.name);
            }

            foreach (string meshName in meshNames)
            {
                bool contains = bd.modelPartConfigs.Any(mpc =>
                    string.Equals(mpc.partName, meshName, StringComparison.CurrentCultureIgnoreCase));
                if (!contains)
                {
                    ModelPartConfig mpc = new ModelPartConfig
                    {
                        partName = meshName
                    };
                    bd.modelPartConfigs.Add(mpc);
                    EditorUtility.SetDirty(bd);
                }
            }

            return true;
        }

        private void DrawFaceParts()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            for (int i = 0; i < bd.facemapParts.Count; i++)
            {
                GUILayout.BeginHorizontal();
                bd.facemapParts[i] = EditorGUILayout.TextField($"脸部part名[{i}]", bd.facemapParts[i]);
                if (GUILayout.Button("X", GUILayout.Width(23)))
                    bd.facemapParts.RemoveAt(i--);
                GUILayout.EndHorizontal();

                if (bd.fbxRef)
                {
                    bool contains = false;
                    List<Transform> recursiveStack = new List<Transform>();
                    recursiveStack.Add(bd.fbxRef.transform);
                    while (recursiveStack.Count > 0)
                    {
                        Transform next = recursiveStack[recursiveStack.Count - 1];
                        if (next.name == bd.facemapParts[i])
                        {
                            contains = true;
                            break;
                        }

                        recursiveStack.RemoveAt(recursiveStack.Count - 1);
                        for (int j = 0; j < next.childCount; j++)
                            recursiveStack.Add(next.GetChild(j));
                    }

                    if (!contains)
                    {
                        EditorGUILayout.HelpBox("部件名不存在。", MessageType.Error);
                    }
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("添加脸部路径"))
                bd.facemapParts.Add(string.Empty);
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                bd.SetAllPrefabDirty(true);
            }
        }
        
        private void DrawCloakParts()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical();
            for (int i = 0; i < bd.cloakParts.Count; i++)
            {
                GUILayout.BeginHorizontal();
                bd.cloakParts[i] = EditorGUILayout.TextField($"披风part名[{i}]", bd.cloakParts[i]);
                if (GUILayout.Button("X", GUILayout.Width(23)))
                    bd.cloakParts.RemoveAt(i--);
                GUILayout.EndHorizontal();

                if (bd.fbxRef)
                {
                    bool contains = false;
                    List<Transform> recursiveStack = new List<Transform>();
                    recursiveStack.Add(bd.fbxRef.transform);
                    while (recursiveStack.Count > 0)
                    {
                        Transform next = recursiveStack[recursiveStack.Count - 1];
                        if (next.name == bd.cloakParts[i])
                        {
                            contains = true;
                            break;
                        }

                        recursiveStack.RemoveAt(recursiveStack.Count - 1);
                        for (int j = 0; j < next.childCount; j++)
                            recursiveStack.Add(next.GetChild(j));
                    }

                    if (!contains)
                    {
                        EditorGUILayout.HelpBox("部件名不存在。", MessageType.Error);
                    }
                }
            }

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("添加披风路径"))
                bd.cloakParts.Add(string.Empty);
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                bd.SetAllPrefabDirty(true);
            }
        }

        private void RefreshGradientPreview(HeightGradient gradient)
        {
            Vector4 debugParam = new Vector4(
                gradient.bottomHeight,
                gradient.topHeight,
                gradient.fade,
                previewHeightGradient ? 1 : 0
            );
            Color debugColor = gradient.enable ? gradient.color : Color.white;

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            Animator[] animators = FindObjectsOfType<Animator>(false);
            List<Renderer> renderers = new List<Renderer>();
            foreach (Animator animator in animators)
            {
                if (!bd)
                    continue;
                foreach (PrefabExportConfig config in bd.exportConfig)
                {
                    if (config == null || !animator.gameObject.name.StartsWith(config.prefabName))
                        continue;
                    animator.GetComponentsInChildren(true, renderers);
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (!material)
                                continue;
                            material.SetVector("_HeightGradientPreviewParam", debugParam);
                            material.SetVector("_HeightGradientPreivewColor", debugColor);
                        }
                    }

                    renderers.Clear();
                    break;
                }
            }
        }

        public static void DrawHeightGradient(HeightGradient gradient)
        {
            Color c = GUI.color;
            GUI.color = Color.red;
            EditorGUILayout.LabelField("注意：只在onekey时保存。");
            GUI.color = c;

            gradient.enable = EditorGUILayout.ToggleLeft("使用高度渐变", gradient.enable);
            if (gradient.enable)
            {
                GUILayout.BeginVertical("box");
                gradient.fade = Mathf.Max(0, EditorGUILayout.FloatField("过渡", gradient.fade));
                gradient.bottomHeight = EditorGUILayout.FloatField("淡入高度", gradient.bottomHeight);
                gradient.topHeight = EditorGUILayout.FloatField("淡出高度", gradient.topHeight);
                GUILayout.EndVertical();
                gradient.color = EditorGUILayout.ColorField(new GUIContent("颜色"), gradient.color, false, false, false);
            }
        }

        private void SetPreviewState(Transform target, bool enable)
        {
            if (target)
            {
                bool exist = target.TryGetComponent<BandposePreview>(out var previewer);
                // Add component if needed.
                if (enable && !exist)
                {
                    System.Action<BandposePreview> onAddComponent = (BandposePreview p) =>
                    {
                        previewer = p;
                        Debug.Log($"onAdd:{p},{previewer}");
                    };
                    BandposePreview.runOnceOnReset += onAddComponent;
                    target.gameObject.AddComponent<BandposePreview>();
                }
                previewer.previewHeightGradient = enable;
                if (enable)
                {
                    previewer.heightGradientGetter = () => bd ? bd.gradient : default;
                }
                else
                {
                    previewer.heightGradientGetter = null;
                }
            }
        }

        private void SetupPrefabName(int source)
        {
            prefabName.Clear();
            for (int i = 0; i < bd.exportConfig.Count; ++i)
            {
                if (bd.exportConfig[i].source == source)
                {
                    prefabName.Add(bd.exportConfig[i].prefabName);
                }
            }
        }

        private void DrawPrefabInspector(int source, ref int selected)
        {
            EditorGUILayout.BeginHorizontal();

            SetupPrefabName(source);

            GUILayout.Space(20);
            if (GUILayout.Button("保存", GUILayout.MaxWidth(80)))
            {
                bd.MakePrefab(-1, source, true, true, false);
            }
            GUILayout.Space(30);

            EditorGUILayout.EndHorizontal();

            //EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前Prefab列表", new GUILayoutOption[] { GUILayout.MaxWidth(120) });

            if (bd.exportMesh == null || bd.exportMesh.Length == 0)
            {
                EditorGUILayout.LabelField("export mesh first!!!");

                if (GUILayout.Button("导出", GUILayout.MaxWidth(80)))
                {
                    bd.Export();
                }
            }
            else
            {
                if (GUILayout.Button("新增", GUILayout.MaxWidth(80)))
                {
                    bd.exportConfig.Add(new PrefabExportConfig(bd) { source = source });
                    selected = prefabName.Count;
                }

                if (selected >= 0)
                {
                    if (GUILayout.Button("删除", GUILayout.MaxWidth(80)))
                    {
                        RemovePrefabConfig(source, selected);
                        selected = -1;
                    }
                    if (GUILayout.Button("收起", GUILayout.MaxWidth(80)))
                    {
                        selected = -1;
                    }

                    if (source == 0)
                    {
                        if (GUILayout.Button("剧情", GUILayout.MaxWidth(80)))
                        {
                            SetPrefabSource(source, selected, 1);
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

            selected = GUILayout.SelectionGrid(selected, prefabName.ToArray(), 2, new GUILayoutOption[] { GUILayout.Width(400) });
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20);
            if (selected >= 0)
            {
                PrefabExportConfig pec = FindPrefabConfig(source, selected);
                if (pec == null) return;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name", GUILayout.MaxWidth(80));
                string name = pec.prefabName;
                pec.prefabName = EditorGUILayout.TextField(pec.prefabName, new GUILayoutOption[] { GUILayout.Width(200) });
                if (name != pec.prefabName) pec.dirty = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Editor", GUILayout.MaxWidth(80));
                EditorGUILayout.ObjectField(pec.prefabRef, typeof(GameObject), false);
                EditorGUILayout.LabelField("Runtime", GUILayout.MaxWidth(80));
                EditorGUILayout.ObjectField(pec.runTimePrefabRef, typeof(GameObject), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Res", GUILayout.MaxWidth(80));
                if (pec.res == null)
                {
                    string versionPath = string.Format("{0}Prefab/{1}.asset",
                        LoadMgr.singleton.editorResPath, pec.prefabName.ToLower());
                    pec.res = AssetDatabase.LoadAssetAtPath<CFEngine.PrefabRes>(versionPath);
                }
                EditorGUILayout.ObjectField(pec.res, typeof(CFEngine.PrefabRes), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("----PartEnable----");
                if (bd.exportMesh == null || bd.exportMesh.Length == 0)
                {
                    EditorGUILayout.LabelField("export mesh first!!!");
                }
                else
                {
                    bd._ResizePartConfig(ref pec);
                    for (int j = 0; j < bd.exportMesh.Length; ++j)
                    {
                        var em = bd.exportMesh[j];

                        if (j >= pec.partMaterial.Count)
                        {
                            pec.partMaterial.Add(null);
                        }

                        if (em.m != null)
                        {
                            PrefabPartConfig ppc = pec.FindPartConfig(em.m.name);
                            if (ppc != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                
                                bool oldEnable = ppc.partEnable;
                                ppc.partEnable = EditorGUILayout.Toggle(ppc.partEnable, GUILayout.Width(46));

                                string meshName = em.m.name;
                                bool error = !EditorCommon.IsAssetNameValid(meshName);
                                meshName = error ? "包含特殊字符 : " + meshName : meshName;

                                using (new GUIColorScope(Color.red, error))
                                {
                                    GUILayout.Label(meshName, GUILayout.ExpandWidth(true));
                                }

                                if (oldEnable != ppc.partEnable) pec.dirty = true;

                                if (ppc.partEnable)
                                {
                                    bool oldActive = ppc.partActive;
                                    ppc.partActive = EditorGUILayout.Toggle(ppc.partActive, GUILayout.Width(46));
                                    GUILayout.Label("Active", GUILayout.Width(40));
                                    if (oldActive != ppc.partActive) pec.dirty = true;
                                }
                                else
                                {
                                    GUILayout.Space(104);
                                }

                                var mat = pec.partMaterial[j];
                                if (mat == null) mat = em.mat;
                                EditorGUI.BeginChangeCheck();
                                mat = EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(200)) as Material;
                                if (EditorGUI.EndChangeCheck())
                                {
                                    pec.partMaterial[j] = mat;
                                }
                                
                                EditorGUILayout.EndHorizontal();
                            }
                        }

                    }
                }
            }
        }

        private PrefabExportConfig FindPrefabConfig(int source, int index)
        {
            int iter = 0;
            for (int i = 0; i < bd.exportConfig.Count; ++i)
            {
                if (bd.exportConfig[i].source == source)
                {
                    if (iter++ == index) return bd.exportConfig[i];
                }
            }

            return null;
        }

        private void RemovePrefabConfig(int source, int index)
        {
            int iter = 0;
            for (int i = 0; i < bd.exportConfig.Count; ++i)
            {
                if (bd.exportConfig[i].source == source)
                {
                    if (iter++ == index)
                    {
                        bd.exportConfig.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        private void SetPrefabSource(int source, int index, int newsource)
        {
            int iter = 0;
            for (int i = 0; i < bd.exportConfig.Count; ++i)
            {
                if (bd.exportConfig[i].source == source)
                {
                    if (iter++ == index)
                    {
                        bd.exportConfig[i].source = newsource;
                        return;
                    }
                }
            }
        }

        public void DrawDesignerInspector()
        {
            if (EditorCommon.BeginFolderGroup("-------------------------------------关卡/战斗策划专区-------------------------------------------", ref bd.DesignerFolder))
            {
                ActiveEditorTracker.sharedTracker.isLocked = true;
                //EnsureViewBandpose();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("保存", GUILayout.MaxWidth(80)))
                {
                    bd.MakePrefab(-1, -1, true, true, false);
                    EditorCommon.SaveAsset(bd);
                }
                EditorGUILayout.EndHorizontal();

                SetupPrefabName(0);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                designerSelected = GUILayout.SelectionGrid(designerSelected, prefabName.ToArray(), 2, new GUILayoutOption[] { GUILayout.Width(400) });
                EditorGUILayout.EndHorizontal();

                if (designerSelected >= 0)
                {
                    PrefabExportConfig pec = FindPrefabConfig(0, designerSelected);
                    if (viewRoot != null && viewRoot.name != pec.prefabName)
                    {
                        GameObject.DestroyImmediate(viewRoot);
                        viewRoot = null;
                    }

                    if (viewRoot == null)
                    {
                        viewRoot = GameObject.Instantiate(pec.prefabRef);
                        viewRoot.name = pec.prefabName;
                        Selection.activeObject = viewRoot;
                    }
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                    {
                        if (IsChildOfViewRoot(Selection.activeGameObject))
                        {
                            SfxData sfx = new SfxData();
                            sfx.path = EditorCommon.GetSceneObjectPath(Selection.activeGameObject.transform, false);
                            pec.sfxData.Add(sfx);
                            GameObject go = new GameObject();
                            go.transform.parent = Selection.activeGameObject.transform;
                            go.transform.localPosition = Vector3.zero;
                            go.transform.localRotation = Quaternion.identity;
                            go.transform.localScale = Vector3.one;
                            go.name = "sfx";
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    int removeIndex = -1;

                    for (int i = 0; i < pec.sfxData.Count; ++i)
                    {
                        var sfx = pec.sfxData[i];
                        string searchPath = sfx.path + "/sfx";
                        if (string.IsNullOrEmpty(sfx.path)) searchPath = "sfx";
                        Transform sfxNode = viewRoot.transform.Find(searchPath);

                        EditorGUILayout.BeginHorizontal();
                        if (sfxNode == null)
                        {
                            Transform goPath = viewRoot.transform.Find(sfx.path);

                            if (goPath != null)
                            {
                                GameObject go = new GameObject();
                                go.transform.parent = goPath;
                                go.transform.localPosition = Vector3.zero;
                                go.transform.localRotation = Quaternion.identity;
                                go.transform.localScale = Vector3.one;
                                go.name = "sfx";
                                sfxNode = go.transform;
                                EditorGUILayout.LabelField(sfx.path);
                            }
                            else
                                EditorGUILayout.LabelField(sfx.path, GetRedTextSyle());
                        }
                        else
                        {
                            EditorGUILayout.LabelField(sfx.path);
                        }

                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();

                        GameObject old = sfx.sfx;
                        sfx.sfx = EditorGUILayout.ObjectField("", sfx.sfx, typeof(GameObject), false) as GameObject;
                        if (sfx.sfx != null && sfx.sfx != old)
                        {
                            if (sfxNode.childCount > 0) GameObject.DestroyImmediate(sfxNode.GetChild(0).gameObject);
                            GameObject fx = GameObject.Instantiate(sfx.sfx);
                            fx.transform.localPosition = Vector3.zero;
                            fx.transform.localRotation = Quaternion.identity;
                            fx.transform.localScale = Vector3.one;
                            fx.transform.parent = sfxNode;
                        }

                        if (sfxNode != null)
                        {
                            if (GUILayout.Button("选中", GUILayout.MaxWidth(80)))
                            {
                                Selection.activeGameObject = sfxNode.gameObject;
                            }
                        }

                        if (GUILayout.Button("删除", GUILayout.MaxWidth(80)))
                        {
                            removeIndex = i;
                        }
                        EditorGUILayout.EndHorizontal();

                        if (sfxNode != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            sfx.offset = EditorGUILayout.Vector3Field("Offset", sfx.offset);
                            sfx.rotation.eulerAngles = EditorGUILayout.Vector3Field("Rot", sfx.rotation.eulerAngles);
                            sfx.scale = EditorGUILayout.Vector3Field("Scale", sfx.scale);
                            if (EditorGUI.EndChangeCheck())
                            {
                                sfxNode.transform.localPosition = sfx.offset;
                                sfxNode.transform.localRotation = sfx.rotation;
                                sfxNode.transform.localScale = sfx.scale;
                            }

                            sfx.offset = sfxNode.transform.localPosition;
                            sfx.rotation = sfxNode.transform.localRotation;
                            sfx.scale = sfxNode.transform.localScale;
                        }
                        pec.dirty = true;
                    }

                    if (removeIndex >= 0)
                    {
                        var sfx = pec.sfxData[removeIndex];
                        string searchPath = sfx.path + "/sfx";
                        if (string.IsNullOrEmpty(sfx.path)) searchPath = "sfx";
                        Transform sfxNode = viewRoot.transform.Find(searchPath);
                        if (sfxNode != null) GameObject.DestroyImmediate(sfxNode.gameObject);

                        pec.sfxData.RemoveAt(removeIndex);
                    }
                }

                EditorCommon.EndFolderGroup();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            else
            {
                ActiveEditorTracker.sharedTracker.isLocked = false;
                //if (viewRoot != null)
                //{
                //    GameObject.DestroyImmediate(viewRoot);
                //    viewRoot = null;
                //}
            }
        }

        private bool IsChildOfViewRoot(GameObject o)
        {
            if (viewRoot == null) return false;
            if (o == null) return false;

            Transform t = o.transform;

            while (t != null)
            {
                if (t.gameObject == viewRoot) return true;
                t = t.parent;
            }

            return false;

        }

        #region play area
        public void DrawPlayInspector()
        {
            if (EditorCommon.BeginFolderGroup("-------------------------------------剧情策划美术专区-------------------------------------------", ref bd.PlayFolder))
            {
                DrawPrefabInspector(1, ref playSelected);
                EditorCommon.EndFolderGroup();
            }
        }
        #endregion

        #region detail

        private void PartFlag(string name, uint flag, PartProcess pp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool enable = EditorGUILayout.Toggle(pp.flag.HasFlag(flag), GUILayout.Width(EditorGUIUtility.singleLineHeight));
            GUILayout.Label(name);
            if (EditorGUI.EndChangeCheck())
            {
                pp.flag.SetFlag(flag, enable);
            }
            EditorGUILayout.EndHorizontal();
        }
        public void DrawDetailInspector()
        {
            if (EditorCommon.BeginFolderGroup("-------------------------------------详情-------------------------------------", ref bd.resExportFolder))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.ObjectField(bd.avatar, typeof(Avatar), false);
                bd.controller = EditorGUILayout.ObjectField(bd.controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
                bd.meshRot = EditorGUILayout.Vector3Field("MeshRot", bd.meshRot);
                EditorGUILayout.BeginHorizontal();
                bd.partTag = EditorGUILayout.TextField("PartSuffix", bd.partTag);
                if (GUILayout.Button("Update", GUILayout.MaxWidth(80)))
                {
                    bd.UpdateMask();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (bd.exportMesh != null)
                {
                    for (int i = 0; i < bd.exportMesh.Length; ++i)
                    {
                        var mm = bd.exportMesh[i];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(mm.m, typeof(Mesh), false);
                        EditorGUILayout.ObjectField(mm.mat, typeof(Material), false);
                        // if (GUILayout.Button ("Export", GUILayout.MaxWidth (60)))
                        // {
                        //     if (mm.m != null)
                        //         bd.Export (mm.m.name);
                        // }
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        if (mm.renderIndex >= 0)
                        {
                            EditorGUILayout.LabelField("renderIndex:" + mm.renderIndex.ToString(), GUILayout.MaxWidth(120));
                        }
                        else
                        {
                            EditorGUILayout.LabelField("path:" + mm.path, GUILayout.MaxWidth(120));
                        }
                        EditorGUILayout.LabelField("mask:" + mm.partMask.ToString(), GUILayout.MaxWidth(160));
                        EditorGUILayout.LabelField("isShadow:" + mm.isShadow, GUILayout.MaxWidth(100));
                        //if (mm.m != null)
                        //{
                        //    if (GUILayout.Button ("RemoveColor", GUILayout.MaxWidth (120)))
                        //    {
                        //        mm.m.colors = null;
                        //        CommonAssets.SaveAsset (mm.m);
                        //    }
                        //}

                        EditorGUILayout.EndHorizontal();
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUILayout.Space();


                EditorGUILayout.LabelField("PartsProcess", EditorStyles.boldLabel);
                if (GUILayout.Button("Add", GUILayout.MaxWidth(120)))
                {
                    bd.partsProcess.Add(new PartProcess());
                }
                int deleteIndex = ToolsUtility.BeginDelete();
                for (int i = 0; i < bd.partsProcess.Count; ++i)
                {
                    var part = bd.partsProcess[i];
                    EditorGUILayout.BeginHorizontal();
                    part.folder = EditorGUILayout.Foldout(part.folder, part.name);

                    ToolsUtility.DeleteButton(ref deleteIndex, i);
                    EditorGUILayout.EndHorizontal();
                    if (part.folder)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginChangeCheck();
                        var name = EditorGUILayout.TextField("", part.name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            part.name = name.ToLower();

                        }
                        EditorGUILayout.EndHorizontal();
                        PartFlag("UnCompress", PartProcess.Flag_UnCompress, part);
                        PartFlag("KeepColor", PartProcess.Flag_KeepColor, part);
                        PartFlag("KeepUV2", PartProcess.Flag_KeepUV2, part);
                        EditorGUI.indentLevel--;
                    }
                }
                ToolsUtility.EndDelete(deleteIndex, bd.partsProcess);
                EditorGUI.indentLevel--;
                EditorCommon.EndFolderGroup();
            }
            EditorGUILayout.Space();
        }
        #endregion

        private void OnDisable()
        {
            if (!(target is BandposeData bandpose) || !bandpose)
                return;
            
            previewHeightGradient = false;
            RefreshGradientPreview(bandpose.gradient);
        }
    }
}

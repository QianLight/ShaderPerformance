using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Athena.MeshSimplify
{
    public enum ReduceMethod
    {
        Simplygon,
        UE4,
    }
    public class SimplygonWindow : EditorWindow
    {
        private bool enableMultipleSelection = false;
        private GameObject lastSelectTarget;
        private GameObject selectTarget;
        private Terrain selectTerrain;
        private int terrainChunkSize = 100;
        private int terrainChunkUnit = 1;
        private Material terrainMat;
        private string multipleSelectionPath;
        private string outputPath;
        private string nameExt = "_low";
        private string terrainLayerName = "MoveLayer";
        private bool useTerrainName = false;
        private SimplygonData obj_simplygonData;
        private SimplygonData terrain_defaultSimplygonData;
        private SimplygonData terrain_simplygonData;
        private Vector2 dataScroll;
        private Color warningColor = new Color(1.0f, 50.0f/255.0f, 50.0f/255.0f);
        private Vector2 scroll;
        
        private ReduceMethod _reduceMethod;
        
        [MenuItem("*Athena*/Simplygon/地形分块及压缩批处理工具", priority = 50)]
        public static void ShowGUI()
        {
            var window = GetWindow<SimplygonWindow>();
            window.minSize = new Vector2(500, 750);
            window.Show();
        }
        
        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawTitle();
            GUILayout.Space(5);

            _reduceMethod = (ReduceMethod)EditorGUILayout.EnumPopup("减面方法", _reduceMethod);
            GUILayout.Space(5);

            if (!enableMultipleSelection)
            {
                DrawSingleSelection();
            }
            else
            {
                DrawMultipleSelection();
            }
            EditorGUILayout.EndScrollView();
        }
        
        void DrawTitle()
        {
            var color = GUI.color;
            GUI.color = warningColor;
            GUILayout.Label("注意：第一次运行时，请先点击下载最新Simplygon并完成注册");
            GUI.color = color;
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            enableMultipleSelection = EditorGUILayout.Toggle("开启多选：", enableMultipleSelection, new GUILayoutOption[0]);
            var enableAutoCheckUpdateSimplygonVersionInt = PlayerPrefs.HasKey("EnableAutoCheckUpdateSimplygonVersion") ? PlayerPrefs.GetInt("EnableAutoCheckUpdateSimplygonVersion") : 0;
            var enableAutoCheckUpdateSimplygonVersionBool = GUILayout.Toggle(enableAutoCheckUpdateSimplygonVersionInt == 1, new GUIContent("开启自动更新Simplygon"), GUILayout.MaxWidth(150));
            var newEnableAutoCheckUpdateSimplygonVersionInt = enableAutoCheckUpdateSimplygonVersionBool ? 1 : 0;
            if (enableAutoCheckUpdateSimplygonVersionInt != newEnableAutoCheckUpdateSimplygonVersionInt)
                PlayerPrefs.SetInt("EnableAutoCheckUpdateSimplygonVersion", newEnableAutoCheckUpdateSimplygonVersionInt);
            if (GUILayout.Button(new GUIContent("下载最新版Simplygon", "请在版本过旧时先进行下载更新"), GUILayout.MaxWidth(150)))
            {
                SimplygonTool.DownloadSimplygon();
            }
            if (GUILayout.Button(new GUIContent("注册Simplygon", "请在第一次使用时先进行注册"), GUILayout.MaxWidth(150)))
            {
                SimplygonTool.RegistSimplygon();
            }
            EditorGUILayout.EndHorizontal();
            color = GUI.color;
            GUI.color = warningColor;
            GUILayout.BeginHorizontal();
            if (!enableMultipleSelection) EditorGUILayout.LabelField(" 注：支持模型压缩和地形分块压缩两种格式", new GUILayoutOption[0]);
            else EditorGUILayout.LabelField(" 注：仅支持模型压缩", new GUILayoutOption[0]);
            GUI.color = color;
            GUILayout.EndHorizontal();
        }

        void DrawSingleSelection()
        {
            selectTarget = EditorGUILayout.ObjectField("Selection：", selectTarget, typeof(GameObject), true, new GUILayoutOption[0]) as GameObject;
            if (selectTarget == null) EditorGUILayout.HelpBox("请先选择对象", MessageType.Warning);
            if (selectTarget) selectTerrain = selectTarget.GetComponent<Terrain>();
            
            GUILayout.Space(5);

            if (selectTerrain)
            {
                DrawTerrain();
            }
            else
            {
                DrawObj(selectTarget);
            }
        }

        void DrawMultipleSelection()
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            multipleSelectionPath = EditorGUILayout.TextField("模型路径：", multipleSelectionPath);
            if (GUILayout.Button("选择模型路径", GUILayout.MaxWidth(200)))
            {
                multipleSelectionPath = EditorUtility.SaveFolderPanel(
                  "选择模型路径",
                  "", "");
                multipleSelectionPath = multipleSelectionPath.Replace(Application.dataPath, "Assets");
            }
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrWhiteSpace(multipleSelectionPath)) EditorGUILayout.HelpBox("请先选择模型路径", MessageType.Warning);
            if (obj_simplygonData == null)
            {
                InitSimplygonData();
            }
            DrawSimplygonData(obj_simplygonData, true);

            nameExt = EditorGUILayout.TextField("模型命名后缀", nameExt, new GUILayoutOption[0]);
            if (GUILayout.Button("模型减面") && !string.IsNullOrWhiteSpace(multipleSelectionPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { multipleSelectionPath });
                List<GameObject> targets = new List<GameObject>();
                for (int i = 0; i < guids.Length; i++)
                {
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    targets.Add(obj);
                }
                foreach (var target in targets)
                {
                    obj_simplygonData.target = target;
                    obj_simplygonData.nameExt = nameExt;
                    obj_simplygonData.outputFolder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)).Replace(Application.dataPath, "Assets").Replace("\\", "/");
                    SimplygonTool.ReduceToFbx(obj_simplygonData, _reduceMethod);
                }
            }
        }

        void DrawObj(GameObject target)
        {
            if (selectTarget != lastSelectTarget)
            {
                lastSelectTarget = selectTarget;
                if (selectTarget)
                {
                    string localPath = AssetDatabase.GetAssetPath(selectTarget);
                    if(!string.IsNullOrEmpty(localPath))
                        outputPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selectTarget));
                }
            }
            
            if (obj_simplygonData == null)
            {
                InitSimplygonData();
            }
            
            DrawSimplygonData(obj_simplygonData, true);

            GUILayout.Space(5);
            
            DrawOutputPath();
            
            nameExt = EditorGUILayout.TextField("模型命名后缀", nameExt, new GUILayoutOption[0]);
            GUI.enabled = target != null;
            if (GUILayout.Button("模型减面"))
            {
                obj_simplygonData.target = selectTarget;
                obj_simplygonData.outputFolder = outputPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                obj_simplygonData.nameExt = nameExt;
                SimplygonTool.ReduceToFbx(obj_simplygonData, _reduceMethod);
            }
            GUI.enabled = true;
        }
        
        void DrawTerrain()
        {
            if (terrain_simplygonData == null)
            {
                InitSimplygonData();
            }
            
            EditorGUILayout.LabelField("PT地形分块方案（地形分高低模两种模式）：", new GUILayoutOption[0]);
            EditorGUILayout.LabelField("当前场景宽度：" + selectTerrain.terrainData.size.x.ToString(), new GUILayoutOption[0]);
            EditorGUILayout.LabelField("当前场景长度：" + selectTerrain.terrainData.size.z.ToString(), new GUILayoutOption[0]);
            EditorGUILayout.LabelField("建议：地块的尺寸必须能被当前场景长宽整除", new GUILayoutOption[0]);
            EditorGUILayout.LabelField("建议：地块顶点间距决定了顶点的数量，地块顶点间距必须能被地块的尺寸整除", new GUILayoutOption[0]);
            EditorGUILayout.LabelField("建议：开启高精度网格后请适量降低优化系数 否则顶点数过高", new GUILayoutOption[0]);
            terrainChunkSize = EditorGUILayout.IntField("地块的尺寸", terrainChunkSize, new GUILayoutOption[0]);
            terrainChunkUnit = EditorGUILayout.IntField("地块顶点间距：", terrainChunkUnit);
            terrainChunkUnit = Mathf.Clamp(terrainChunkUnit, 1, (int)Mathf.Min(selectTerrain.terrainData.size.x, selectTerrain.terrainData.size.z));
            terrainMat = EditorGUILayout.ObjectField("地形材质", terrainMat, typeof(Material), false) as Material;
            terrainLayerName = EditorGUILayout.TextField("Layer层级：", terrainLayerName);
            useTerrainName = EditorGUILayout.Toggle("是否使用地形名称：", useTerrainName);
            
            //支持32位后，不需要限制
            var checkOutSize = CheckOutSize(selectTerrain, terrainChunkSize, terrainChunkUnit, out int chunkCount, out int chunSize, out int totalSize);
            EditorGUILayout.HelpBox($"切块数量：{chunkCount}, 单块模型顶点数: {chunSize}, 总面数: {totalSize}", MessageType.Info);
            
            GUILayout.Space(5);
            
            DrawSimplygonData(terrain_simplygonData, false, terrain_defaultSimplygonData, chunSize);
            

            GUILayout.Space(5);
            
            DrawOutputPath();
            if (GUILayout.Button("拆分地形并进行减面"))
            {
                terrain_simplygonData.target = selectTarget;
                terrain_simplygonData.outputFolder = outputPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                SimplygonTerrainTool.ReduceTerrainToBlockModel(terrain_defaultSimplygonData, terrain_simplygonData, terrainChunkSize, terrainChunkUnit, terrainMat, _reduceMethod, terrainLayerName, useTerrainName);
            }
        }
        
        private bool CheckOutSize(Terrain _terrain, int chunkSize, int chunkUnit, out int chunkCount, out int chunSize, out int totalSize)
        {
            int chunkCountHorizontal = Mathf.CeilToInt(_terrain.terrainData.size.x / chunkSize);
            int chunkCountVertical = Mathf.CeilToInt(_terrain.terrainData.size.z / chunkSize);
            int vertexCountHorizontal = (int)(chunkSize / chunkUnit) + 1;
            int vertexCountVertical = (int)(chunkSize / chunkUnit) + 1;
            int vertexCount = vertexCountHorizontal * vertexCountVertical;

            chunkCount = chunkCountHorizontal * chunkCountVertical;
            chunSize = vertexCount;
            totalSize = chunSize * chunkCount;
            return vertexCount > 65000;
        }

        void DrawOutputPath()
        {
            EditorGUILayout.BeginHorizontal();
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                EditorGUILayout.LabelField(new GUIContent("", "output path is null or empty!"), new GUIStyle("CN EntryWarnIconSmall"), GUILayout.Width(15));
            }
            outputPath = EditorGUILayout.DelayedTextField("导出路径：", outputPath);
            if (GUILayout.Button("选择导出路径", GUILayout.MaxWidth(200)))
            {
                outputPath = EditorUtility.SaveFolderPanel(
                  "选择导出路径",
                  "", "");
                outputPath = outputPath.Replace(Application.dataPath, "Assets");
            }

            if (!string.IsNullOrEmpty(outputPath))
            {
                outputPath = outputPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawSimplygonData(SimplygonData simplygonData, bool drawCount, SimplygonData defaultSimplygonData = null, int chunSize = 1)
        {
            if (GUILayout.Button("Reset Setting"))
            {
                InitSimplygonData();
            }

            float terrainChunkSize = 0;
            float reduceWeigtRatio = _reduceMethod == ReduceMethod.Simplygon ? 0.1f : 0.0f;
            if (defaultSimplygonData != null)
            {
                terrainChunkSize = chunSize * (defaultSimplygonData.lodSimplygonItemDatas[0].triangleRatio + reduceWeigtRatio);
                GUILayout.Label($"地形转模型默认减面系数： 单块模型顶点数约为: {chunSize}* (减面系数{defaultSimplygonData.lodSimplygonItemDatas[0].triangleRatio} + 保留轮廓权重{reduceWeigtRatio}) ≈ {terrainChunkSize}, 0.1表示保留轮廓细节后的估值");
                DrawSimplygonItemData(defaultSimplygonData.lodSimplygonItemDatas[0], true, true);
                GUILayout.Space(5);
            }
            
            GUILayout.Label($"LOD减面列表： ");
            if (drawCount)
            {
                simplygonData.lodCount = EditorGUILayout.DelayedIntField("LOD数量：", simplygonData.lodCount, new GUILayoutOption[0]);
                if (simplygonData.lodCount < 1) simplygonData.lodCount = 1;
                var delta = simplygonData.lodCount - simplygonData.lodSimplygonItemDatas.Count;
                if (delta > 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        simplygonData.lodSimplygonItemDatas.Add((ReductionSetting)simplygonData.lodSimplygonItemDatas[simplygonData.lodSimplygonItemDatas.Count - 1].Clone());
                    }
                }
                else if(delta < 0)
                {
                    for (int j = 0; j < -delta; j++)
                    {
                        simplygonData.lodSimplygonItemDatas.RemoveAt(simplygonData.lodSimplygonItemDatas.Count - 1);
                    }
                }
            }
            
            dataScroll = EditorGUILayout.BeginScrollView(dataScroll, (_reduceMethod == ReduceMethod.Simplygon) ? GUILayout.MinHeight(400) : GUILayout.MinHeight(250));
            for (int i = 0; i < simplygonData.lodSimplygonItemDatas.Count; i++)
            {
                if (defaultSimplygonData != null)
                    GUILayout.Label($"LOD {i} ：   单块模型顶点数约为: {terrainChunkSize}* {simplygonData.lodSimplygonItemDatas[i].triangleRatio} ≈ {terrainChunkSize*simplygonData.lodSimplygonItemDatas[i].triangleRatio}");
                else
                    GUILayout.Label($"LOD {i} ：");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                var itemData = simplygonData.lodSimplygonItemDatas[i];
                DrawSimplygonItemData(itemData, true, true);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSimplygonItemData(ReductionSetting itemData, bool showDetail = false, bool showScreenSize = false)
        {
            EditorGUILayout.BeginVertical("box");
            itemData.reductionUnit = (ReductionUnit)EditorGUILayout.EnumPopup(new GUIContent("减面单位：", "Default: 按选中对象为单位；\n\rMesh：按选中对象的每个mesh为单位"), itemData.reductionUnit, new GUILayoutOption[0]);
            itemData.triangleRatio = EditorGUILayout.Slider("* 减面比例：", itemData.triangleRatio, 0, 1, new GUILayoutOption[0]);
            if (showDetail)
            {
                itemData.lockGeometricBorder = EditorGUILayout.Toggle("锁定边界：", itemData.lockGeometricBorder, new GUILayoutOption[0]);
                EditorGUILayout.BeginHorizontal();
                itemData.enableMaxEdgeLength = EditorGUILayout.Toggle("启用三角面边的最大长度：", itemData.enableMaxEdgeLength,
                    new GUILayoutOption[0]);
                if (itemData.enableMaxEdgeLength)
                    itemData.maxEdgeLength = EditorGUILayout.Slider("三角面边的最大长度：", itemData.maxEdgeLength, 0, 20,
                        new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                itemData.enableMaxDeviation =
                    EditorGUILayout.Toggle("启用允许的最大表面偏差：", itemData.enableMaxDeviation, new GUILayoutOption[0]);
                if (itemData.enableMaxDeviation)
                    itemData.maxDeviation = EditorGUILayout.FloatField("允许的最大表面偏差：", itemData.maxDeviation, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
                
                if (_reduceMethod == ReduceMethod.Simplygon)
                {
                    itemData.geometryImportance = EditorGUILayout.Slider("轮廓重要性：", itemData.geometryImportance, 0, 10, new GUILayoutOption[0]);
                    itemData.shadingImportance = EditorGUILayout.Slider("边缘尖锐度：", itemData.shadingImportance, 0, 10, new GUILayoutOption[0]);
                    itemData.textureImportance = EditorGUILayout.Slider("UV重要性：", itemData.textureImportance, 0, 10, new GUILayoutOption[0]);
                }
                else
                {
                    itemData.weldingThreshold = EditorGUILayout.Slider("融合阈值：", itemData.weldingThreshold, 0, 1);
                }
            }
            else if (showScreenSize)
            {
                EditorGUILayout.BeginHorizontal();
                itemData.enableMaxDeviation = EditorGUILayout.Toggle("启用允许的最大表面偏差：", itemData.enableMaxDeviation, new GUILayoutOption[0]);
                if (itemData.enableMaxDeviation)
                    itemData.maxDeviation = EditorGUILayout.FloatField("允许的最大表面偏差：", itemData.maxDeviation, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        
        void InitSimplygonData()
        {
            //预设地形减面数据
            terrain_defaultSimplygonData = new SimplygonData(selectTarget, outputPath, new ReductionSetting()
            {
                reductionUnit = ReductionUnit.Default,
                triangleRatio = 0.2f,
                lockGeometricBorder = true,
                enableScreenSize = true,
                screenSize = 600,
                enableMaxEdgeLength = true,
                maxEdgeLength = 10,
                geometryImportance = 10,
                shadingImportance = 10,
                weldingThreshold = 0,
            });
            
            //terrain default simplygon data
            terrain_simplygonData = new SimplygonData(selectTarget, outputPath, new ReductionSetting[2]
            {
                new ReductionSetting()
                {
                    reductionUnit = ReductionUnit.Default,
                    triangleRatio = 0.8f,
                    lockGeometricBorder = true,
                    enableScreenSize = true,
                    screenSize = 600,
                    enableMaxEdgeLength = false,
                    maxEdgeLength = 7,
                    geometryImportance = 5,
                    shadingImportance = 5,
                    weldingThreshold = 0,
                },
                new ReductionSetting()
                {
                    reductionUnit = ReductionUnit.Default,
                    triangleRatio = 0.5f,
                    lockGeometricBorder = true,
                    enableScreenSize = true,
                    screenSize = 600,
                    enableMaxEdgeLength = false,
                    maxEdgeLength = 7,
                    geometryImportance = 5,
                    shadingImportance = 5,
                    weldingThreshold = 0,
                }
            });
            //obj default simplygon data
            obj_simplygonData = new SimplygonData(selectTarget, outputPath, new ReductionSetting[1]
            {
                new ReductionSetting()
                {
                    reductionUnit = ReductionUnit.Default,
                    triangleRatio = 0.5f,
                    lockGeometricBorder = true,
                    enableScreenSize = true,
                    screenSize = 600,
                    enableMaxEdgeLength = true,
                    maxEdgeLength = 7,
                    geometryImportance = 3,
                    shadingImportance = 2,
                    weldingThreshold = 0,
                },
            });
        }
    }
}
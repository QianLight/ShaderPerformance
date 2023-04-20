using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Athena.MeshSimplify;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using com.pwrd.hlod;
using UnityEngine.SceneManagement;

namespace com.pwrd.hlod.editor
{
    public class HLODWindow : EditorWindow
    {
        [SerializeField] TreeViewState _treeViewState;
        HLODTreeView _treeView;

        private Rect _contentRect;
        private Rect _toolBarRect;
        private Rect _splitRect;
        private Rect _inspectorRect;
        private Rect _treeViewRect;

        private float k_ToolBarHeight = 50;
        private float k_MinWidthInspectorColumns = 250;
        public float k_MinWidthTwoColumns = 600;
        public float k_MinHeight = 400;


        private float k_DefaultInspectorWidth = 600;
        private float k_SplitBarWidth = 5;
        private float _splitBarOffset;

        private bool init = false;

        //当前选择的 LOD Level
        private TreeViewItem m_curSelectedItem;

        private bool m_ProviderHasChanged;

        private bool showRoots = true;
        private bool showBakeShaderList = false;
        private bool showLayerSetting = true;
        private bool showCluster = true;
        private bool showImport = true;
        
        private Vector2 scrollPos;
        
        private static GUIContent importSetttingContent = new GUIContent("导入配置", "导入配置");
        private static GUIContent exportSetttingContent = new GUIContent("导出配置", "导出配置");
        private static GUIContent bakeRTSizeContent = new GUIContent("纹理烘焙RT尺寸", "用来被烘焙出每一个模型的颜色和光照信息的图的大小。尺寸越小，烘焙速度越快，但最终贴图质量下降");
        private static GUIContent useCustomBakeLayerMaskContent = new GUIContent("自定义烘焙Layer", "开启后，需要指定烘焙所在的Layer，尽量选择无效或者运行时用的Layer，防止在烘焙贴图时出现错误。");
        private static GUIContent bakeLayerMaskContent = new GUIContent("纹理烘焙Layer", "烘焙单个物体的时候需要把这个物体的layer设置到一个独立的layer，烘焙摄像机也只会显示这个layer，防止烘焙物体和其他物体穿插造成烘焙结果不正确。");
        private static GUIContent defaultBakeShaderContent = new GUIContent("默认烘焙shader", "相机烘焙纹理时对renderer使用的shader，默认使用自带烘焙shader，以lightmapUV进行albedo的读取。");
        private static GUIContent showBakeShaderListContent = new GUIContent("烘焙shader映射列表", "可指定原模型为特定shader时的特定烘焙shader");
        private static GUIContent targetParentContent = new GUIContent("父节点", "hlod生成后的父级，可以为空，默认为HLODRoot");
        private static GUIContent showRootsContent = new GUIContent("Roots", "需要烘焙hlod的根节点");
        private static GUIContent hlodMethodContent = new GUIContent("烘焙方式", "支持Simplygon和pwrd两种方式，同比例下效果相差不大，可根据需要自行选择");
        private static GUIContent useVoxelContent = new GUIContent("使用体素", "把所有网格体素化，然后在转换成模型进行减面生成uv。即使减面比例非常低，也能保持原模型的轮廓。适合在比较远的地方使用，但是拉近了看略显粗糙。");
        private static GUIContent proxyMapTypeContent = new GUIContent("代理网格映射方式", "支持LODGroup与Prefab两种方式，分簇时按不同方式，进行整体收集（如使用Prefab方式：prefab下a、b距离很远，但必定打进一个bundle内，且运行时以prefab单位进行映射管理）");
        private static GUIContent textureChannelContent = new GUIContent("代理网格纹理", "目前只支持Albedo，后续功能根据需要进行动态开放");
        private static GUIContent useOverrideSettingContent = new GUIContent("是否进行重写", "使用独立场景参数，否则使用全局场景参数");
        private static GUIContent clusterMethodContent = new GUIContent("分簇方式", "通过不同的方式对场景模型进行划分，并收集，可根据需要自行选择分簇方式");
        private static GUIContent useMeshReductionContent = new GUIContent("开启减面", "默认开启，关闭后仅包含合并功能，请谨慎使用");
        private static GUIContent slpitAlphaTestContent = new GUIContent("拆分AlphaTest", "按透明情况，对分簇对象进行筛选");
        private static GUIContent useLODIndexContent = new GUIContent("使用原始模型LOD索引", "使用LODGroup中的index为分簇对象，如1就是以LOD1中的对象为基础进行分簇。");
        private static GUIContent useHighRendererLightmapContent = new GUIContent("低模使用高模Lightmap", "使用此项时，请确保高低模名字匹配，如：高模为\"xxx\", 低模为\"xxx\\_xxx\"，即低模比高模多一个任意后缀");
        private static GUIContent clusterMinDiameterContent = new GUIContent("簇最小包围盒边长", "UE_Tripe & DeepFirst表示目标分簇的包围盒边长大小; \n其他方式表示原始模型的包围盒边长大小");
        private static GUIContent clusterMaxDiameterContent = new GUIContent("簇最大包围盒边长", "UE_Tripe & DeepFirst表示目标分簇的包围盒边长大小; \n其他方式表示原始模型的包围盒边长大小; \nChunkArea分簇方式且仅一层时最大边长建议无穷大。");
        private static GUIContent tree_UseSelectBoundsContent = new GUIContent("采用选中物体的Bounds", "若开启，则采用选中物体的包围盒；若关闭，则使用自定义包围盒。");
        private static GUIContent tree_CenterContent = new GUIContent("custom bounds.center", "自定义bounds的中心坐标");
        private static GUIContent tree_SizeContent = new GUIContent("custom bounds.center", "自定义bounds的尺寸，也就是长宽高（xyz）");
        private static GUIContent tree_DepthContent = new GUIContent("深度", "随深度增加，划分的越细致，越小");
        private static GUIContent startPosContent = new GUIContent("起点坐标", "空间中bounds的起点坐标，也就是包围盒的左下后坐标；区域划分模式，没有对y轴的划分，所以可忽略，只需要给定xz平面的起点坐标与终点坐标即可。");
        private static GUIContent endPosContent = new GUIContent("终点坐标", "空间中bounds的终点坐标，也就是包围盒的右上前坐标；区域划分模式，没有对y轴的划分，所以可忽略，只需要给定xz平面的起点坐标与终点坐标即可。");
        private static GUIContent horizonalChunckCountContent = new GUIContent("横向块数量", "沿x轴方式分块的数量");
        private static GUIContent verticalChunckCountContent = new GUIContent("纵向块数量", "沿z轴方式分块的数量");
        private static GUIContent clusterCountContent = new GUIContent("分簇数量", "指定要簇的个数");
        private static GUIContent maxIterationsContent = new GUIContent("迭代次数", "迭代次数越多，分簇约精确，速度越慢，迭代数达到一定值后效果不在改变");
        private static GUIContent overlayPercentageContent = new GUIContent("最低填充百分比", "若两个bounds加起来之后的屏占比小于最低填充百分比，则不进行合并。");
        private static GUIContent mergeRendererMinCountContent = new GUIContent("最小的合并数量", "簇的最小renderer数量，少于这个值自动忽略。");
        private static GUIContent qualityContent = new GUIContent("*减面比例", "预设减面比例（无其他参数时为绝对减面比例），与其他参数叠加进行减面，以最终减面比例为准");
        private static GUIContent lockGeometricBorderContent = new GUIContent("*锁定边界", "开启后，将锁定边界顶点保持不变，能得到较好效果，但实际减面比例将有较高提升");
        private static GUIContent enableMaxEdgeLengthContent = new GUIContent("启用三角面边的最大长度", "将限制三角形的最大边长长度，防止某个三角形的边长过长，导致mipmap时出现问题");
        private static GUIContent enableMaxDeviationContent = new GUIContent("启用允许的最大表面偏差", "允许的最大表面偏，差数值越大，效果越好");
        private static GUIContent geometryImportanceContent = new GUIContent("轮廓重要性", "几何体的轮廓的保留系数");
        private static GUIContent screenSizeContent = new GUIContent("屏幕大小", "通过屏幕像素差异和模型大小综合计算而得到的一个最大偏差值。当减面过程中如果偏差大于这个值，减面就会中断，即使减面比例还没有达到预期。");
        private static GUIContent weldingThresholdContent = new GUIContent("融合阈值", "默认为0。当减面时遇到一些模型中间被撕裂开，或者产生一些碎片时，可以通过调大这个值缓解。\n但是对一些比较圆滑的模型，或者硬边比较少的情况，这个值调大了会造成结果一些如凹凸不平等奇怪现象。");
        private static GUIContent smoothNormalsContent = new GUIContent("平滑法线", "优化法线，使模型法线平滑（部分项目不适用）。");
        private static GUIContent enableMeshOptimationContent = new GUIContent("启用Mesh压缩", "将顶点从32位压缩为16位，法线从32位3通道压缩16位2通道并删除切线；需依赖Athena.package，且目标shader支持才可使用。");
        private static GUIContent targetShaderContent = new GUIContent("目标Shader", "hlod资源指定的shader，为空时，指定为默认运行时shader");
        private static GUIContent fixMaterialQueueContent = new GUIContent("固定材质队列", "启用后将指定hlod资源材质球的RenderQueue，防止srpbatch打断等");
        private static GUIContent useOcclusionContent = new GUIContent("可见性剔除", "根据角度参数进行可见性剔除，仅支持Simplygon烘焙方式");
        private static GUIContent yawContent = new GUIContent("yaw", "偏航角，欧拉角向量的y轴");
        private static GUIContent pitchContent = new GUIContent("pitch", "俯仰角，欧拉角向量的x轴");
        private static GUIContent coverageContent = new GUIContent("coverage", "覆盖角度，相机视野覆盖的角度");
        private static GUIContent albedoSizeContent = new GUIContent("贴图大小", "目标贴图的分辨率，贴图格式固定为ASTC 6x6。");
        private static GUIContent useCullingContent = new GUIContent("过滤小物体", "根据culling值与模型的屏占比进行比较，进行剔除");
        private static GUIContent useProxyScreenPercentContent = new GUIContent("代理网格切换百分比", "屏占比，通过此值计算生成分簇的运行时显示距离。");

        public HLODProvider newProvider
        {
            get { return HLODProvider.Instance; }
        }

        void OnEnable()
        {
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();
            _treeView = new HLODTreeView(_treeViewState);
            _treeView.data = newProvider.data;
            _treeView.Reload();
            minSize = new Vector2(k_MinWidthTwoColumns, k_MinHeight);

            RegisterMessage(HLODMesssages.REPAINT_WINDOW, OnGetRepaintWindowMessage);
            RegisterMessage(HLODMesssages.REBUILD_TREE_VIEW, (p) => ReLoadTreeView());
            RegisterMessage(HLODMesssages.SCENE_EDITOR_DATA_DESTORYED, (p) => { EditorApplication.delayCall += () => { _treeView.data = newProvider.data; }; });
            RegisterMessage(HLODMesssages.HLOD_SUB_CLUSTER_DOUBLE_CLICK, (p) => { var arr = p as object[];SelectionClusterItem((int)arr[0], (int)arr[1]); });
            // RegisterMessage(HLODMesssages.HLOD_TREE_LAYER_ITEM_CHANGED, (p) => { SetSelectLOD((int)p);});
        }

        private void OnDisable()
        {
            UnRegisterAllMessage();
        }


        void HandleSplitter()
        {
            if (!init)
            {
                _splitBarOffset = position.width - k_DefaultInspectorWidth;
                init = true;
            }

            _splitRect = new Rect(_splitBarOffset, 0, k_SplitBarWidth, position.height);
            _splitRect = Reflection.Call<Rect>("UnityEditor.EditorGUIUtility, UnityEditor", "HandleHorizontalSplitter", _splitRect, position.width,
                k_MinWidthTwoColumns - k_MinWidthInspectorColumns, k_MinWidthInspectorColumns);
            _splitBarOffset = _splitRect.x;
        }

        void DrawSplitter()
        {
            Reflection.Call("UnityEditor.EditorGUIUtility, UnityEditor", "DrawHorizontalSplitter", _splitRect);
        }

        public static void BeginBoxVertical(params GUILayoutOption[] options)
        {
            var style = GUI.skin.box;
            var content = GUIContent.none;
            var groupType = Type.GetType("UnityEngine.GUILayoutGroup, UnityEngine.IMGUIModule");
            // "UnityEngine.GUILayoutGroup, UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            //UnityEngine.GUILayoutUtility, UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null

            var aInstance = Reflection.Call<object>("UnityEngine.GUILayoutUtility, UnityEngine.IMGUIModule", "BeginLayoutGroup", style, options, groupType);

            Reflection.SetField(aInstance, groupType.FullName, "isVertical", true);
            Rect rect = Reflection.GetField<Rect>(aInstance, "UnityEngine.GUILayoutGroup, UnityEngine.IMGUIModule", "rect");
            rect.height = rect.height - 5;
            rect.y += 3;
            GUI.Box(rect, content, style);
        }

        void CalculateRects()
        {
            _toolBarRect = new Rect(0, 0, position.width, k_ToolBarHeight);
            _contentRect = new Rect(0, _toolBarRect.height, position.width, position.height - _toolBarRect.height);
            _treeViewRect = new Rect(0, 0, _splitBarOffset, position.height - _toolBarRect.height);
            _inspectorRect = new Rect(_splitBarOffset + _splitRect.width, 0, position.width - (_splitBarOffset + _splitRect.width), position.height - _toolBarRect.height);
        }

        private State state
        {
            get { return newProvider.data.state; }
            set { newProvider.data.state = value; }
        }

        void DrawButton(Rect rect, string text, Color color, bool enabled, Action action)
        {
            var c = GUI.backgroundColor;
            var e = GUI.enabled;
            GUI.backgroundColor = color;
            GUI.enabled = enabled;
            if (GUI.Button(rect, text))
            {
                if (action != null)
                    action.Invoke();
            }

            GUI.backgroundColor = c;
            GUI.enabled = e;
        }

        void DrawToolBar()
        {
            using (new GUI.GroupScope(_toolBarRect))
            {
                GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none);
                float split = 10;
                float offset = 10;
                float width = 150;
                float height = 30;

                DrawButton(new Rect(offset, 10, width, height), "生成分簇", Color.green, state == State.None, () =>
                {
                    state = State.Cluster;
                    newProvider.GenerateClusters();
                    _treeView.Reload();
                    EditorUtility.DisplayDialog("", "生成分簇完成,请注意保存场景,以免数据丢失", "确定");
                });
                offset = offset + split + width;
                DrawButton(new Rect(offset, 10, width, height), "选中成簇", Color.green, true, () =>
                {
                    newProvider.GenerateClustersBySelection(Selection.gameObjects);
                    _treeView.Reload();
                    EditorUtility.DisplayDialog("", "生成分簇完成,请注意保存场景,以免数据丢失", "确定");
                });
                offset = offset + split + width;
                DrawButton(new Rect(offset, 10, width, height), "生成网格", Color.yellow, true, () =>
                {
                    newProvider.BuildProxyMesh();
                    _treeView.Reload();
                });
                offset = offset + split + width;
                DrawButton(new Rect(offset, 10, width, height), "合并图集", Color.cyan, state == State.ProxyMesh && !newProvider.data.hasMergeAtlas,
                    () => { newProvider.TryMergeProxyAtlas(); });
                offset = offset + split + width;
                DrawButton(new Rect(offset, 10, width, height), "清除数据", Color.grey, true, () =>
                {
                    if (newProvider.HasProxyMeshData())
                    {
                        var result = EditorUtility.DisplayDialog("", "当前场景已经有Proxy网格数据了,确认要删除吗", "确认", "取消");
                    
                        if (!result)
                            return;
                    }

                    state = State.None;
                    Selection.objects = null;
                    newProvider.Clear();
                    _treeView.Reload();
                });

                DrawDebugBtns();
            }
        }

        #region DebugBtns

        private int m_debugHistoryIndex = 0;
        private List<GameObject> m_debugHistoryList;

        private void DrawDebugBtns()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(newProvider.data.debug ? "调试中...点击关闭" : "调试"))
            {
                newProvider.data.debug = !newProvider.data.debug;
            }

            if (newProvider.data.debug)
            {
                if (GUILayout.Button("保存为历史记录"))
                {
                    // var root = GameObject.Find("HLODEditorDataHistory");
                    // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    // var go = GameObject.Instantiate(newProvider.data.gameObject, root.transform);
                    // go.hideFlags = HideFlags.DontSaveInBuild;
                    // go.name = DateTime.Now.ToString("yyyy-M-d HH-mm-ss");
                    // var data = go.GetComponent<HLODSceneEditorData>();
                    // data.scenes = new List<SceneNode>(newProvider.data.scenes);
                    // //data.resultList = new List<ResultData>(data.resultList);
                    // GetHistoryDataList().Add(go);
                }

                var list = GetHistoryDataList();
                if (GUILayout.Button("加载记录"))
                {
                    // if (list.Count <= 0)
                    // {
                    //     EditorUtility.DisplayDialog("", "没有记录", "确定");
                    //     return;
                    //     ;
                    // }
                    //
                    // var provider = newProvider;
                    // bool isDebug = provider.data.debug;
                    // DestroyImmediate(provider.data.gameObject);
                    //
                    // var go = list[m_debugHistoryIndex];
                    // var newGo = GameObject.Instantiate(go);
                    // newGo.name = "HLODEditorData";
                    //
                    // provider.data = newGo.GetComponent<HLODSceneEditorData>();
                    // var data = provider.data;
                    // data.debug = isDebug;
                    // data.scenes = new List<SceneNode>(newProvider.data.scenes);
                    // _treeView.Reload();
                }

                if (list.Count > 0)
                {
                    var arr = new string[list.Count];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (list[i] == null)
                            continue;
                        arr[i] = list[i].name;
                    }

                    m_debugHistoryIndex = EditorGUILayout.Popup(m_debugHistoryIndex, arr);
                }
                else
                {
                    GUILayout.Label("没有记录");
                }
            }

            GUILayout.EndHorizontal();
        }

        private List<GameObject> GetHistoryDataList()
        {
            var root = GameObject.Find("HLODEditorDataHistory");
            if (root != null && m_debugHistoryList != null)
            {
                return m_debugHistoryList;
            }

            m_debugHistoryList = new List<GameObject>();

            if (root == null)
            {
                root = new GameObject("HLODEditorDataHistory");
                root.hideFlags = HideFlags.DontSaveInBuild;
            }

            for (var i = 0; i < root.transform.childCount; ++i)
            {
                var go = root.transform.GetChild(i).gameObject;
                if (go != null && go.GetComponent<HLODSceneEditorData>() != null)
                    m_debugHistoryList.Add(go);
            }

            return m_debugHistoryList;
        }

        #endregion

        void DrawTreeView()
        {
            if (m_ProviderHasChanged)
            {
                OnEnable();
                m_ProviderHasChanged = false;
            }

            _treeView.OnGUI(_treeViewRect);
        }

        void DrawInspector()
        {
            GUILayout.BeginArea(_inspectorRect);
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
                
                TreeViewItem item = GetSelectTreeViewItem();
                
                var layerItem = item as LayerTreeViewItem;
                var clusterItem = item as ClusterTreeViewItem;
                var sceneItem = item as SceneTreeViewItem;
                
                //导入导出
                showImport = EditorGUILayout.Foldout(showImport, "Setting Import&Export GlobalSetting", true);
                if (showImport)
                {
                    GUILayout.BeginVertical();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    DrawInspector_ImportAndExport();
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }

                if (clusterItem != null)
                {
                    showCluster = EditorGUILayout.Foldout(showCluster, clusterItem.cluster.name, true);
                    if (showCluster)
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.BeginVertical();
                        DrawInspector_Cluster(clusterItem.cluster);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }
                else if (layerItem != null)
                {
                    showLayerSetting = EditorGUILayout.Foldout(showLayerSetting, layerItem.displayName, true);
                    if (showLayerSetting)
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.BeginVertical();
                        DrawInspector_Layer(layerItem.layer);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }
                else
                {
                    DrawInspector_GlobalSetting();
                    DrawInspector_SceneSetting(sceneItem?.scene);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();


            return;
        }
        
        private TreeViewItem GetSelectTreeViewItem()
        {
            var selection = _treeView.GetSelection();
            TreeViewItem item = null;
            
            if (selection.Count == 1)
            {
                var id = selection.First();
                foreach (var treeViewItem in _treeView.items)
                {
                    if (treeViewItem.id == id)
                    {
                        item = treeViewItem;
                    }
                }
            }
            return item;
        }

        /// <summary>
        /// 绘制shader烘焙设置
        /// </summary>
        private void DrawInspector_RendererBakerSetting()
        {
            if (newProvider.data.rendererBakerSetting == null)
            {
                newProvider.data.rendererBakerSetting = new RendererBakerSetting();
            }

            var config = newProvider.data.rendererBakerSetting;
            
            config.bakeRTSize = (BakeRTSize)EditorGUILayout.EnumPopup(bakeRTSizeContent, config.bakeRTSize);
            EditorGUILayout.BeginHorizontal();
            config.useCustomBakeLayerMask = EditorGUILayout.Toggle(useCustomBakeLayerMaskContent, config.useCustomBakeLayerMask);
            int unusedLayer = GetUnusedLayer();
            if (unusedLayer == -1)
            {
                config.useCustomBakeLayerMask = true;
            }
            if (config.useCustomBakeLayerMask)
            {
                config.bakeLayerMask = EditorGUILayout.IntField(bakeLayerMaskContent, config.bakeLayerMask);
                config.bakeLayerMask = Mathf.Clamp(config.bakeLayerMask, 0, 31);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制shader烘焙映射设置
        /// </summary>
        private void DrawInspector_ShaderBindConfigSetting()
        {
            if (newProvider.data.shaderBindConfig == null)
            {
                newProvider.data.shaderBindConfig = new ShaderBindConfig();
#if HLOD_USE_URP
                newProvider.data.shaderBindConfig.defaultBakeShader = Shader.Find("Athena/Bake/Lit");
#else
                newProvider.data.shaderBindConfig.defaultBakeShader = Shader.Find("Athena/Bake/Unlit");
#endif
            }

            var config = newProvider.data.shaderBindConfig;

            config.defaultBakeShader = EditorGUILayout.ObjectField(defaultBakeShaderContent, config.defaultBakeShader, typeof(Shader), true, GUILayout.MinWidth(0)) as Shader;
            showBakeShaderList = EditorGUILayout.Foldout(showBakeShaderList, showBakeShaderListContent, true);
            if (showBakeShaderList)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();
                config.bakeShaderList = config.bakeShaderList ?? new List<ShaderBindConfig.Tuple>();
                DrawListSize(config.bakeShaderList, new ShaderBindConfig.Tuple());

                for (int i = 0; i < config.bakeShaderList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("originShader:");
                    config.bakeShaderList[i].originShader = EditorGUILayout.ObjectField("", config.bakeShaderList[i].originShader, typeof(Shader), true, GUILayout.MinWidth(0)) as Shader;
                    GUILayout.Label("bakeShader:");
                    config.bakeShaderList[i].bakeShader = EditorGUILayout.ObjectField("", config.bakeShaderList[i].bakeShader, typeof(Shader), true, GUILayout.MinWidth(0)) as Shader;
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制场景模型
        /// </summary>
        private void DrawInspector_SceneModel(SceneNode sceneNode)
        {
            sceneNode.targetParent = EditorGUILayout.ObjectField(targetParentContent, sceneNode.targetParent, typeof(GameObject), true) as GameObject;
            
            showRoots = EditorGUILayout.Foldout(showRoots, showRootsContent, true);
            if (showRoots)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical();

                sceneNode.roots = sceneNode.roots ?? new List<GameObject>();
                DrawListSize(sceneNode.roots, null);
                
                for (int i = 0; i < sceneNode.roots.Count; i++)
                {
                    sceneNode.roots[i] = EditorGUILayout.ObjectField("", sceneNode.roots[i], typeof(GameObject), true, GUILayout.MinWidth(0)) as GameObject;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制Layer
        /// </summary>
        /// <param name="sceneNode"></param>
        private void DrawInspector_AddLayers()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            BeginBoxVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), GUILayout.Width(25),
                GUILayout.Height(25)))
            {
                newProvider.AddNewLayer();
                // newProvider.AddNewLayer(sceneNode);
                _treeView.Reload();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), GUILayout.Width(25),
                GUILayout.Height(25)))
            {
                newProvider.RemoveLayer();
                // if (sceneNode.layers.Count > 1)
                //     sceneNode.layers.RemoveAt(sceneNode.layers.Count - 1);
                _treeView.Reload();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void DrawInspector_GlobalSetting()
        {
            GUILayout.Label("全局设置：", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            {
                newProvider.data.hlodMethod = (HlodMethod)EditorGUILayout.EnumPopup(hlodMethodContent, newProvider.data.hlodMethod);
                if (newProvider.data.hlodMethod == HlodMethod.AthenaSimplify)
                {
                    newProvider.data.useVoxel = EditorGUILayout.Toggle(useVoxelContent, newProvider.data.useVoxel);
                }
                newProvider.data.proxyMapType = (ProxyMapType)EditorGUILayout.EnumPopup(proxyMapTypeContent, newProvider.data.proxyMapType);
                newProvider.data.textureChannel = (TextureChannel)EditorGUILayout.EnumFlagsField(textureChannelContent, newProvider.data.textureChannel);
                // newProvider.data.textureChannel = newProvider.data.textureChannel | TextureChannel.Albedo;   //TODO select
                newProvider.data.textureChannel = TextureChannel.Albedo;
                DrawInspector_RendererBakerSetting();
                DrawInspector_ShaderBindConfigSetting();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();            
            GUILayout.EndVertical();
        }

        private void DrawInspector_SceneSetting(SceneNode sceneNode = null)
        {
            GUILayout.Label("场景设置：", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            {
                SceneSetting setting = newProvider.GetSceneSetting(sceneNode);
                if (sceneNode != null)
                {
                    if (setting.layerSettings.Count != sceneNode.layers.Count)
                    {
                        var delta = setting.layerSettings.Count - sceneNode.layers.Count;
                        if (delta > 0)
                        {
                            for (int i = 0; i < delta; i++)
                            {
                                sceneNode.layers.Add(new Layer());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < -delta; i++)
                            {
                                sceneNode.layers.RemoveAt(sceneNode.layers.Count - 1);
                            }
                        }
                        _treeView.Reload();
                    }
                    
                    sceneNode.useOverrideSetting = EditorGUILayout.Toggle(useOverrideSettingContent, sceneNode.useOverrideSetting);
                    if (sceneNode.useOverrideSetting)
                    {
                        if (sceneNode.firstChangeOverrideState)
                        {
                            sceneNode.sceneSetting = setting.Clone();
                            sceneNode.firstChangeOverrideState = false;
                        }
                        sceneNode.sceneSetting = sceneNode.sceneSetting ?? setting.Clone();
                        setting = sceneNode.sceneSetting;
                    }
                }
                GUILayout.Space(5);
                setting.clusterMethod = (GenerateClusterMethod) EditorGUILayout.EnumPopup(clusterMethodContent, setting.clusterMethod);
                setting.useMeshReduction = EditorGUILayout.Toggle(useMeshReductionContent, setting.useMeshReduction);
                if (setting.useMeshReduction == false)
                {
                    EditorGUILayout.HelpBox("-_-!! 真的吗？真的不减面？真的只是合并吗？", MessageType.Warning);
                }

                if (newProvider.data.hlodMethod == HlodMethod.AthenaSimplify)
                {
                    setting.useBakeMaterialInfo = EditorGUILayout.Toggle("烘焙材质", setting.useBakeMaterialInfo);
                    EditorGUILayout.HelpBox("烘焙材质信息可以将模型在场景中的渲染效果烘焙到贴图上，再进行合并操作，不烘材质将直接使用材质的原贴图进行合并操作", MessageType.Info);
                }

                setting.slpitAlphaTest = EditorGUILayout.Toggle(slpitAlphaTestContent, setting.slpitAlphaTest);
                
                setting.useLODIndex = EditorGUILayout.IntField(useLODIndexContent, setting.useLODIndex);
                if (setting.useLODIndex < 0) setting.useLODIndex = 0;
                if (setting.useLODIndex > 0)
                {
                    setting.useHighRendererLightmap = EditorGUILayout.Toggle(useHighRendererLightmapContent, setting.useHighRendererLightmap);
                }

                if (sceneNode != null) DrawInspector_SceneModel(sceneNode);

                //绘制Layer面板
                DrawInspector_Layers(setting);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();            
            GUILayout.EndVertical();
        }

        private int GetUnusedLayer()
        {
	        int layer = -1;
	        for (int i = 8; i < 32; i++)
	        {
		        var name = LayerMask.LayerToName(i);
		        if (string.IsNullOrWhiteSpace(name))
		        {
			        layer = i;
			        break;
		        }
	        }
	        return layer;
        }

        private void DrawInspector_Layers(SceneSetting setting)
        {
            //绘制Layer面板
            showLayerSetting = EditorGUILayout.Foldout(showLayerSetting, "Layers", true);
            if (showLayerSetting)
            {
                DrawInspector_AddLayers();
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                var layerSettings = setting.layerSettings;
                for (int i = 0; i < layerSettings.Count; i++)
                {
                    GUILayout.Label("Layer" + i + ":");
                    DrawInspector_LayerSetting(layerSettings[i], setting.clusterMethod, setting.useMeshReduction);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }
        
        private void DrawInspector_Layer(Layer layer)
        {
            var sceneNode = newProvider.GetSceneNode(layer);
            var sceneSetting = newProvider.GetSceneSetting(sceneNode);
            if (sceneNode != null)
            {
                var setting = newProvider.GetLayerSetting(layer);
                layer.useOverrideSetting = EditorGUILayout.Toggle(useOverrideSettingContent, layer.useOverrideSetting);
                if (layer.useOverrideSetting)
                {
                    if (layer.firstChangeOverrideState)
                    {
                        layer.layerSetting = setting.Clone();
                        layer.firstChangeOverrideState = false;
                    }
                    layer.layerSetting = layer.layerSetting ?? setting.Clone();
                    setting = layer.layerSetting;
                }
                GUILayout.Space(5);
                DrawInspector_LayerSetting(setting, sceneSetting.clusterMethod, sceneSetting.useMeshReduction);
            }
        }
        
        private void DrawInspector_LayerSetting(LayerSetting setting, GenerateClusterMethod clusterMethod, bool useMeshReduction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            DrawInspector_ClusterSetting(clusterMethod, setting.clusterSetting);
            if (useMeshReduction) DrawInspector_MeshReductionSetting(setting.meshReductionSetting);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        
        private void DrawInspector_Cluster(Cluster cluster)
        {
            if (newProvider.data.hasMergeAtlas)
            {
                GUILayout.Label("合并图集之后不能再调整代理网格");
                GUI.enabled = false;
            }
            
            bool canEdit = !newProvider.data.hasMergeAtlas;
            GUILayout.BeginVertical("box");
            using (new EditorGUI.DisabledScope(!canEdit))
            {
                var layer = newProvider.GetLayer(cluster);
                var sceneSetting = newProvider.GetSceneSetting(newProvider.GetSceneNode(layer));
                if (layer != null)
                {
                    var meshReductionSetting = newProvider.GetLayerSetting(layer).meshReductionSetting;
                    cluster.useOverrideSetting = EditorGUILayout.Toggle(useOverrideSettingContent, cluster.useOverrideSetting);
                    if (cluster.useOverrideSetting)
                    {
                        if (cluster.firstChangeOverrideState)
                        {
                            cluster.meshReductionSetting = meshReductionSetting.Clone();
                            cluster.firstChangeOverrideState = false;
                        }
                        cluster.meshReductionSetting = cluster.meshReductionSetting ?? meshReductionSetting.Clone();
                        meshReductionSetting = cluster.meshReductionSetting;
                    }
                    GUILayout.Space(5);
                    if (sceneSetting.useMeshReduction) DrawInspector_MeshReductionSetting(meshReductionSetting);
                }

                if (GUILayout.Button("更新Cluster信息"))
                {
                    newProvider.UpdateProxyMesh(cluster);
                }

                GUILayout.Space(10);
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
        }

        private void DrawInspector_ImportAndExport()
        {
            GUILayout.BeginVertical();
            //GUILayout.Box("", GUILayout.Height(25), GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(importSetttingContent))
            {
                var path = EditorUtility.OpenFilePanel("导入", HLODRecordTool.configPath, "asset");
                var config = AssetDatabase.LoadAssetAtPath(GetRelativePathWithAssets(path), typeof(HLODConfigSetting)) as HLODConfigSetting;
                if (config == null)
                {
                    EditorUtility.DisplayDialog("失败", "打开的文件不是一个hlod配置文件", "确定");
                    return;
                }
                newProvider.data.InitData(config, false, true);
                OnEnable();
            }

            if (GUILayout.Button(exportSetttingContent))
            {
                var path = EditorUtility.SaveFilePanel("导出", HLODRecordTool.configPath, "HLODConfig", "asset");
                if (string.IsNullOrEmpty(path) == false)
                {
                    HLODRecordTool.SaveData(newProvider.data, GetRelativePathWithAssets(path));
                }
            }

            GUILayout.EndHorizontal();
        }
        
        private void DrawInspector_ClusterSetting(GenerateClusterMethod clusterMethod, ClusterSetting clusterSetting)
        {
            GUILayout.Label("分簇设置：", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            {
                // GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
                clusterSetting.clusterMinDiameter = Mathf.Max(0, EditorGUILayout.FloatField(clusterMinDiameterContent, clusterSetting.clusterMinDiameter));
                clusterSetting.clusterMaxDiameter = Mathf.Max(0, EditorGUILayout.FloatField(clusterMaxDiameterContent, clusterSetting.clusterMaxDiameter));
                if (clusterMethod == GenerateClusterMethod.QuadTree || clusterMethod == GenerateClusterMethod.Octree)
                {
                    clusterSetting.tree_UseSelectBounds = EditorGUILayout.Toggle(tree_UseSelectBoundsContent, clusterSetting.tree_UseSelectBounds);
                    if (!clusterSetting.tree_UseSelectBounds)
                    {
                        clusterSetting.tree_Center = EditorGUILayout.Vector3Field(tree_CenterContent, clusterSetting.tree_Center);
                        clusterSetting.tree_Size = EditorGUILayout.Vector3Field(tree_SizeContent, clusterSetting.tree_Size);
                    }
                    clusterSetting.tree_Depth = EditorGUILayout.IntField(tree_DepthContent, clusterSetting.tree_Depth);
                }
                else if (clusterMethod == GenerateClusterMethod.ChunkArea)
                {
                    clusterSetting.startPos = EditorGUILayout.Vector3Field(startPosContent, clusterSetting.startPos);
                    clusterSetting.endPos = EditorGUILayout.Vector3Field(endPosContent, clusterSetting.endPos);
                    clusterSetting.horizonalChunckCount = EditorGUILayout.IntField(horizonalChunckCountContent, clusterSetting.horizonalChunckCount);
                    clusterSetting.verticalChunckCount = EditorGUILayout.IntField(verticalChunckCountContent, clusterSetting.verticalChunckCount);
                }
                else if (clusterMethod == GenerateClusterMethod.DP)
                {
                    clusterSetting.dp_BestValue = (DPBestValue)EditorGUILayout.EnumPopup("最佳收集策略", clusterSetting.dp_BestValue);
                }
                else if (clusterMethod == GenerateClusterMethod.KMeans)
                {
                    clusterSetting.clusterCount = EditorGUILayout.IntField(clusterCountContent, clusterSetting.clusterCount);
                    clusterSetting.maxIterations = EditorGUILayout.IntField(maxIterationsContent, clusterSetting.maxIterations);
                    clusterSetting.maxIterations = Mathf.Max(clusterSetting.maxIterations, clusterSetting.clusterCount + 1);
                }
                else if (clusterMethod == GenerateClusterMethod.BVHTree)
                {
                    clusterSetting.bvh_SplitCount = EditorGUILayout.IntField(maxIterationsContent, clusterSetting.bvh_SplitCount);
                    clusterSetting.bvh_Depth = EditorGUILayout.IntField(tree_DepthContent, clusterSetting.bvh_Depth);
                }
                else
                {
                    clusterSetting.overlayPercentage = Mathf.Clamp(EditorGUILayout.IntField(overlayPercentageContent, clusterSetting.overlayPercentage), 0, 100);
                    clusterSetting.mergeRendererMinCount = Mathf.Max(0, EditorGUILayout.IntField(mergeRendererMinCountContent, clusterSetting.mergeRendererMinCount));
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();            
            GUILayout.EndVertical();
        }

        private void DrawInspector_MeshReductionSetting(MeshReductionSetting setting)
        {
            var meshReductionSetting = setting;
            GUILayout.Label("减面设置：", EditorStyles.boldLabel);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            {
                meshReductionSetting.quality = EditorGUILayout.Slider(qualityContent, meshReductionSetting.quality, 0.0f, 1.0f);
                if (newProvider.data.hlodMethod == HlodMethod.Simplygon)
                {
                    meshReductionSetting.lockGeometricBorder =
                        EditorGUILayout.Toggle(lockGeometricBorderContent, meshReductionSetting.lockGeometricBorder);
                    EditorGUILayout.BeginHorizontal();
                    meshReductionSetting.enableMaxEdgeLength = EditorGUILayout.Toggle(enableMaxEdgeLengthContent, meshReductionSetting.enableMaxEdgeLength);
                    if (meshReductionSetting.enableMaxEdgeLength)
                        meshReductionSetting.maxEdgeLength =
                            EditorGUILayout.Slider("", meshReductionSetting.maxEdgeLength, 0, 20);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    meshReductionSetting.enableMaxDeviation = EditorGUILayout.Toggle(enableMaxDeviationContent,
                        meshReductionSetting.enableMaxDeviation, new GUILayoutOption[0]);
                    if (meshReductionSetting.enableMaxDeviation)
                        meshReductionSetting.maxDeviation = EditorGUILayout.FloatField("最大表面偏差", meshReductionSetting.maxDeviation);

                    EditorGUILayout.EndHorizontal();
                    meshReductionSetting.geometryImportance = EditorGUILayout.Slider(geometryImportanceContent,
                        meshReductionSetting.geometryImportance, 0.0f, 10.0f);
                }
                else
                {
                    meshReductionSetting.screenSize = EditorGUILayout.IntSlider(screenSizeContent, meshReductionSetting.screenSize, 1, 1200);
                    meshReductionSetting.weldingThreshold = EditorGUILayout.Slider(weldingThresholdContent, meshReductionSetting.weldingThreshold, 0, 1);
                }

                meshReductionSetting.smoothNormals = EditorGUILayout.Toggle(smoothNormalsContent, meshReductionSetting.smoothNormals);
                meshReductionSetting.enableMeshOptimation = EditorGUILayout.Toggle(enableMeshOptimationContent, meshReductionSetting.enableMeshOptimation);
                meshReductionSetting.targetShader = (Shader) EditorGUILayout.ObjectField(targetShaderContent, meshReductionSetting.targetShader, typeof(Shader), false);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(fixMaterialQueueContent);
                meshReductionSetting.fixMaterialQueue = EditorGUILayout.Toggle("", meshReductionSetting.fixMaterialQueue);
                if (meshReductionSetting.fixMaterialQueue)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("RenderQueue:");
                    meshReductionSetting.materialQueue = EditorGUILayout.IntField("", meshReductionSetting.materialQueue);
                }

                EditorGUILayout.EndHorizontal();
                meshReductionSetting.albedoSize = EditorGUILayout.Vector2IntField(albedoSizeContent, meshReductionSetting.albedoSize);
                meshReductionSetting.useOcclusion =
                    EditorGUILayout.Toggle("可见性剔除", meshReductionSetting.useOcclusion);
                if (meshReductionSetting.useOcclusion)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();
                    if (newProvider.data.hlodMethod == HlodMethod.Simplygon)
                    {
                        meshReductionSetting.yaw =
                            Mathf.Clamp(EditorGUILayout.FloatField(yawContent, meshReductionSetting.yaw), 0f, 360f);
                        meshReductionSetting.pitch =
                            Mathf.Clamp(EditorGUILayout.FloatField(pitchContent, meshReductionSetting.pitch), 0f, 360f);
                        meshReductionSetting.coverage =
                            Mathf.Clamp(EditorGUILayout.FloatField(coverageContent, meshReductionSetting.coverage), 0f, 360f);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("注：如果size为0的话，使用默认点，默认点为上半球9个观测点。");
                        DrawListSize(meshReductionSetting.cameraPos, Vector3.up);

                        for (int i = 0; i < meshReductionSetting.cameraPos.Count; i++)
                        {
                            meshReductionSetting.cameraPos[i] = EditorGUILayout.Vector3Field("", meshReductionSetting.cameraPos[i], GUILayout.MinWidth(0));
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

                // GUILayout.Box("", GUILayout.Height(2), GUILayout.ExpandWidth(true));
                meshReductionSetting.useCulling = EditorGUILayout.Toggle(useCullingContent, meshReductionSetting.useCulling);
                GUI.enabled = meshReductionSetting.useCulling;
                meshReductionSetting.culling =
                    EditorGUILayout.Slider("过滤百分比", meshReductionSetting.culling, 0.0f, 1.0f);

                GUI.enabled = true;
                meshReductionSetting.useProxyScreenPercent = EditorGUILayout.Slider(useProxyScreenPercentContent, meshReductionSetting.useProxyScreenPercent, 0.0f, 1.0f);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();            
            GUILayout.EndVertical();
        }
        
        private void DrawListSize<T>(List<T> list, T defaultValue)
        {
            int count = EditorGUILayout.DelayedIntField("Size：", list.Count);
            int delta = count - list.Count;
            if (count < 0) count = 0;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    list.Add(defaultValue);
                }
            }
            else if (delta < 0)
            {
                for (int j = 0; j < -delta; j++)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        //包含Assets
        private static string GetRelativePathWithAssets(string fullPath)
        {
            return fullPath.Substring(Application.dataPath.Length - 6);
        }
        
        public void ReLoadTreeView()
        {
            _treeView.Reload();
        }

        public void SelectionClusterItem(int layerID, int id)
        {
            if (!_treeView.IsExpanded(layerID))
            {
                var list = new List<int>(_treeView.GetExpanded());
                list.Add(layerID);
                _treeView.SetExpanded(list);
            }

            _treeView.SetSelection(new List<int>()
            {
                id
            });
        }

        void DrawContent()
        {
            using (new GUI.GroupScope(_contentRect))
            {
                DrawTreeView();
                DrawSplitter();
                DrawInspector();
            }
        }

        void OnGUI()
        {
            HandleSplitter();
            CalculateRects();
            DrawToolBar();
            DrawContent();
        }

        private void OnFocus()
        {
            base.Repaint();
        }


        private void OnGetRepaintWindowMessage(object param)
        {
            Repaint();
        }

        private List<(string, Action<object>)> m_receiver = new List<(string, Action<object>)>();

        private void RegisterMessage(string message, Action<object> action)
        {
            m_receiver.Add((message, action));
            HLODMessageCenter.Register(message, action);
        }

        private void UnRegisterAllMessage()
        {
            foreach (var tuple in m_receiver)
            {
                HLODMessageCenter.UnRegister(tuple.Item1, tuple.Item2);
            }
        }

        [MenuItem("*Athena*/HLOD/HLOD Window")]
        static void ShowWindow()
        {
            var window = GetWindow<HLODWindow>();
            window.titleContent = new GUIContent("HLOD Window");
            window.Show();
            // HLODProvider.Instance.data.InitDefaultData();
        }
    }
}
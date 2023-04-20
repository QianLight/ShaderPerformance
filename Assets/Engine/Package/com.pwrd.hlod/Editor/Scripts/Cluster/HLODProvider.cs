using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;
using Athena.MeshSimplify;
#if _HLOD_USE_ATHENA_ATLASING_
using Athena;
using Athena.TextureAtlas;
using System.Reflection;
#endif
using UnityEngine.Rendering;


namespace com.pwrd.hlod.editor
{
    // HLOD数据提供者
    public class HLODProvider
    {
        #region property & instance
        
        private static HLODProvider m_Instance;

        public static HLODProvider Instance
        {
            get
            {
                if (m_Instance == null || m_Instance.data == null || m_Instance.data.gameObject == null || m_Instance.data.gameObject.scene.name == null || m_Instance.data.gameObject.scene != EditorSceneManager.GetActiveScene())
                    m_Instance = Create();
                return m_Instance;
            }
        }

        public enum AlphaTestType
        {
            All,
            OnlyAlphaTest,
            OnlyNotAlphaTest,
        }

        public HLODSceneEditorData data;
        public Layer curSelectLayer;
        protected IHLODProviderEntitiyCollector entitiyCollector;

        public HLODProvider(HLODSceneEditorData data, IHLODProviderEntitiyCollector collector)
        {
            this.data = data;
            this.entitiyCollector = collector;
        }
        
        private static HLODProvider Create()
        {
            var oldGo = GameObject.Find("HLODEditorData");
            if (oldGo != null)
            {
                if (oldGo.scene.name == null || oldGo.scene != EditorSceneManager.GetActiveScene())
                {
                    GameObject.DestroyImmediate(oldGo);
                }
            }
            var com = oldGo == null ? null : oldGo.GetComponent<HLODSceneEditorData>();
            var editorData = com != null
                ? com
                : GameObject.FindObjectOfType(typeof(HLODSceneEditorData)) as HLODSceneEditorData;

            if (editorData == null)
            {
                GameObject go = new GameObject("HLODEditorData");
                editorData = go.AddComponent<HLODSceneEditorData>();
                go.hideFlags = HideFlags.DontSaveInEditor;
                //go.hideFlags = HideFlags.NotEditable;
            }

            var provider = new HLODProvider(editorData, new HLODProviderEntityColector());

            return provider;
        }
        #endregion

        #region output interface

        /// <summary> 增加Layer,默认配置 </summary>
        public void AddNewLayer()
        {
            var sceneSetting = GetGlobalSetting();
            
            var defaultSettings = LayerSetting.GetDefaultSetting();
            int recommondIndex = Mathf.Min(sceneSetting.layerSettings.Count, defaultSettings.Count - 1);
            
            var cloneSetting = defaultSettings[recommondIndex].Clone();
            sceneSetting.layerSettings.Add(cloneSetting);
            foreach (var sceneNode in data.scenes)
            {
                if (!sceneNode.useOverrideSetting)
                {
                    sceneNode.layers.Add(new Layer() { });
                }
            }
        }
        
        /// <summary> 删除Layer </summary>
        public void RemoveLayer()
        {
            var sceneSetting = GetGlobalSetting();
            if (sceneSetting.layerSettings.Count > 1)
            {
                sceneSetting.layerSettings.RemoveAt(sceneSetting.layerSettings.Count - 1);
            }
            foreach (var sceneNode in data.scenes)
            {
                if (!sceneNode.useOverrideSetting)
                {
                    if (sceneNode.layers.Count > 1) sceneNode.layers.RemoveAt(sceneNode.layers.Count - 1);
                }
            }
        }
        
        /// <summary> 增加Layer,默认配置 </summary>
        public void AddNewLayer(SceneNode sceneNode)
        {
            var sceneSetting = GetSceneSetting(sceneNode);
            
            var defaultSettings = LayerSetting.GetDefaultSetting();
            int recommondIndex = Mathf.Min(sceneSetting.layerSettings.Count, defaultSettings.Count - 1);

            var cloneSetting = defaultSettings[recommondIndex].Clone();
            sceneSetting.layerSettings.Add(cloneSetting);
            sceneNode.layers.Add(new Layer() { layerSetting = cloneSetting });
        }
        
        /// <summary> 删除Layer </summary>
        public void RemoveLayer(SceneNode sceneNode)
        {
            var sceneSetting = GetSceneSetting(sceneNode);
            if (sceneSetting.layerSettings.Count > 1) sceneSetting.layerSettings.RemoveAt(sceneSetting.layerSettings.Count - 1);
            if (sceneNode.layers.Count > 1) sceneNode.layers.RemoveAt(sceneNode.layers.Count - 1);
        }

        /// <summary> 清除数据 </summary>
        public void Clear()
        {
            foreach (var sceneNode in data.scenes)
            {
                foreach (var layer in sceneNode.layers)
                    layer.clusters.Clear();

                // sceneNode.resultList.Clear();

                var root = GetHLODRoot(sceneNode);
                if (root) GameObject.DestroyImmediate(root);
                
                var path = GetSceneOutputPath(sceneNode);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    AssetDatabase.Refresh();
                }

                data.hasMergeAtlas = false;
                EditorSceneManager.MarkSceneDirty(sceneNode.scene);
            }
        }

        /// <summary> 清除簇 </summary>
        public void RemoveCluster(Cluster cluster)
        {
            foreach (var sceneNode in data.scenes)
            {
                foreach (var layer in sceneNode.layers)
                {
                    layer.clusters.RemoveAll(s => s.Equals(cluster));
                }
            }
        }

        /// <summary> 生成分簇 </summary>
        public void GenerateClusters()
        {
            foreach (var sceneNode in data.scenes)
            {
                if (sceneNode.roots == null) return;
                var sceneSetting = GetSceneSetting(sceneNode);
                bool splitAlpha = sceneSetting.slpitAlphaTest;
                if (!splitAlpha)
                {
                    CollectData(sceneNode, sceneNode.layers);
                }
                else
                {
                    CollectDataConsiderAlphaTest(sceneNode);
                }

                //顺便把贴花信息也记录下来.
                CollectDecalTags(sceneNode);
                EditorSceneManager.MarkSceneDirty(sceneNode.scene);
            }
        }

        /// <summary> 选中成簇 </summary>
        public void GenerateClustersBySelection(GameObject[] selectObjs)
        {
            Dictionary<string, List<GameObject>> scene_obj_dic = new Dictionary<string, List<GameObject>>();
            foreach (var selectObj in selectObjs)
            {
                if (scene_obj_dic.ContainsKey(selectObj.scene.path))
                {
                    scene_obj_dic[selectObj.scene.path].Add(selectObj);
                }
                else
                {
                    scene_obj_dic.Add(selectObj.scene.path, new List<GameObject>(){ selectObj });
                }
            }

            foreach (var scene_obj in scene_obj_dic)
            {
                var sceneNode = data.scenes.Find(s => s.scenePath.Equals(scene_obj.Key));
                var sceneSetting = GetSceneSetting(sceneNode);
                if (sceneNode == null) continue;
                
                var allrenders = new HashSet<Renderer>();
                foreach (var obj in selectObjs)
                {
                    if (obj.scene.path.Equals(sceneNode.scenePath))
                    {
                        allrenders.UnionWith(obj.GetComponentsInChildren<Renderer>());
                    }
                }
                var renderers = new HashSet<Renderer>();
                foreach (var renderer in allrenders)
                {
                    if (renderer == null) continue;
                    if (renderers.Contains(renderer)) continue;
                    var lodGroup = renderer.GetComponentInParent<LODGroup>();
                    if (lodGroup)
                    {
                        var lods = lodGroup.GetLODs();
                        if (lods.Length > sceneSetting.useLODIndex && lods[sceneSetting.useLODIndex].renderers != null)
                            renderers.UnionWith(lods[sceneSetting.useLODIndex].renderers);
                    }
                    else
                    {
                        renderers.UnionWith(new List<Renderer>(){renderer});
                    }
                }

                List<Entity> entities = new List<Entity>();
                foreach (var renderer in renderers)
                {
                    if (renderer == null) continue;
                    Entity entity = new Entity(renderer.gameObject);
                    entities.Add(entity);

                    foreach (var layer in sceneNode.layers)
                    {
                        foreach (var layerCluster in layer.clusters)
                        {
                            layerCluster.entities.RemoveAll(s => s.manifest.Equals(renderer.gameObject));
                        }
                    }
                }
                Cluster cluster = new Cluster(entities);
                for (int i = 0; i < sceneNode.layers.Count; i++)
                {
                    var layer = sceneNode.layers[i];
                    cluster.name = string.Format("Cluster-{0}-{1}", i, layer.clusters.Count);
                    layer.clusters.Add(cluster);
                }
            }
        }

        /// <summary> 生成代理网格 </summary>
        public void BuildProxyMesh()
        {
            foreach (var sceneNode in data.scenes)
            {
                var sceneSetting = GetSceneSetting(sceneNode);
                var startTime = DateTime.Now;
                var allList = new List<(List<Renderer>, AggregateParam)>();

                for (int i = sceneNode.layers.Count - 1; i >= 0; i--)
                {
                    var layer = sceneNode.layers[i];
                    var layerSetting = GetLayerSetting(layer);
                    for (int j = 0; j < layer.clusters.Count; j++)
                    {
                        var cluster = layer.clusters[j];
                        if (cluster.ignoreGenerator) continue;
                        int triangleCount = 0;
                        int materialCount = 0;
                        cluster.CollectStatics(ref triangleCount, ref materialCount);
                        if (materialCount <= 0)
                            continue;

                        cluster.usePercent = layerSetting.meshReductionSetting.useProxyScreenPercent;
                        List<Renderer> renderers = new List<Renderer>();
                        cluster.CollectRenderers(renderers);
                        FilterLODRender(renderers, cluster.bounds, sceneSetting.clusterMethod, layerSetting);
                        if (renderers.Count <= 1)
                        {
                            HLODDebug.Log("[HLOD][BuildProxyMesh] skip:" + cluster.name);
                            continue;
                        }

                        string logout = "";
                        foreach (var renderer in renderers)
                            logout += renderer.name + " , ";
                        HLODDebug.Log(logout);
                        var param = GetAggregateParam(cluster, layerSetting.meshReductionSetting);
                        param.cluster = cluster;
                        param.outputName = "HLOD_" + GetSceneName(sceneNode) + cluster.name;
                        param.outputPath = GetSceneOutputPath(sceneNode);
                        param.useAlphaTest = cluster.openAlphaTest;
                        param.useHighRendererLightmap = sceneSetting.useHighRendererLightmap;
                        param.useMeshReduction = sceneSetting.useMeshReduction;
                        param.useBakeMaterialInfo = sceneSetting.useBakeMaterialInfo;
                        param.useLODIndex = sceneSetting.useLODIndex;
                        param.rendererBakerSetting = data.rendererBakerSetting;
                        param.shaderBindConfig = data.shaderBindConfig;
                        param.textureChannel = data.textureChannel;
                        param.useVoxel = data.useVoxel;
                        param.decalTagList = sceneNode.decalTagList;
                        allList.Add((renderers, param));
                    }
                }

                if (!data.debug)
                {
                    //发送Renderer给simplygon生成简化模型
                    var results = RunAggregate(allList);
                    // sceneNode.resultList = results;
                    foreach (var result in results)
                        result.cluster.hlodResult = result;

                    //debug
                    //AggregateBaked.RunAggregateAsync(allList);
                }

                GenerateProxy(sceneNode);
                EndProxyMeshGenerate(sceneNode, DateTime.Now - startTime);
            }
        }

        /// <summary> （指定簇）生成代理网格 </summary>
        public void BuildProxyMesh(Cluster cluster, SceneNode sceneNode = null)
        {
            if (sceneNode == null) sceneNode = GetSceneNode(GetLayer(cluster));
            if (sceneNode == null) return;
            var sceneSetting = GetSceneSetting(sceneNode);
            var startTime = DateTime.Now;
            var allList = new List<(List<Renderer>, AggregateParam)>();

            for (int i = sceneNode.layers.Count - 1; i >= 0; i--)
            {
                var layer = sceneNode.layers[i];
                var layerSetting = GetLayerSetting(layer);
                for (int j = 0; j < layer.clusters.Count; j++)
                {
                    if (cluster.Equals(layer.clusters[j]))
                    {
                        int triangleCount = 0;
                        int materialCount = 0;
                        cluster.CollectStatics(ref triangleCount, ref materialCount);
                        if (materialCount <= 0)
                            continue;

                        cluster.usePercent = layerSetting.meshReductionSetting.useProxyScreenPercent;
                        List<Renderer> renderers = new List<Renderer>();
                        cluster.CollectRenderers(renderers);
                        FilterLODRender(renderers, cluster.bounds, sceneSetting.clusterMethod, layerSetting);
                        if (renderers.Count <= 1)
                        {
                            HLODDebug.Log("[HLOD][BuildProxyMesh] skip:" + cluster.name);
                            continue;
                        }

                        string logout = "";
                        foreach (var renderer in renderers)
                            logout += renderer.name + " , ";
                        var param = GetAggregateParam(cluster, layerSetting.meshReductionSetting);
                        param.cluster = cluster;
                        param.outputName = "HLOD_" + GetSceneName(sceneNode) + cluster.name;
                        param.outputPath = GetSceneOutputPath(sceneNode);
                        param.useAlphaTest = cluster.openAlphaTest;
                        param.useHighRendererLightmap = sceneSetting.useHighRendererLightmap;
                        param.useMeshReduction = sceneSetting.useMeshReduction;
                        param.useBakeMaterialInfo = sceneSetting.useBakeMaterialInfo;
                        param.useLODIndex = sceneSetting.useLODIndex;
                        param.rendererBakerSetting = data.rendererBakerSetting;
                        param.shaderBindConfig = data.shaderBindConfig;
                        param.textureChannel = data.textureChannel;
                        param.useVoxel = data.useVoxel;
                        param.decalTagList = sceneNode.decalTagList;
                        allList.Add((renderers, param));
                    }
                }
            }

            if (!data.debug)
            {
                //发送Renderer给simplygon生成简化模型
                var results = RunAggregate(allList);
                // sceneNode.resultList = results;
                foreach (var result in results)
                    result.cluster.hlodResult = result;

                //debug
                //AggregateBaked.RunAggregateAsync(allList);
            }

            GenerateProxy(sceneNode, cluster);
            EndProxyMeshGenerate(sceneNode, DateTime.Now - startTime);
        }

        /// <summary> 合并图集 </summary>
        public void TryMergeProxyAtlas()
        {
#if _HLOD_USE_ATHENA_ATLASING_
            var stack = new Stack<List<Proxy>>();
            var resultList = sceneNode.resultList;
            if (resultList == null)
                return;

            var root = GetHLODRoot(sceneNode);
            if (root == null)
                return;
            var proxyManager = root.GetComponent<ProxyManager>();
            if (proxyManager == null)
                return;

            DoMergeProxyAtlasRecursive(proxyManager.proxies, proxyManager.transform);

            data.hasMergeAtlas = true;
#else
            EditorUtility.DisplayDialog("", "未定义宏_HLOD_USE_ATHENA_ATLASING_, 不支持合并图集", "确定");
#endif
        }

        /// <summary> 清楚图集合并结果 </summary>
        public void ClearAtlasingResult()
        {
#if _HLOD_USE_ATHENA_ATLASING_
            var root = GetHLODRoot(sceneNode);
            if (root == null)
                return;

            var coms = root.GetComponentsInChildren<TextureAtlasing>();
            var atlasDst = Path.Combine(GetSceneOutputPathRelative(), "Atlasing_" + root.name);
            foreach (var cur in coms)
            {
                TextureAtlasingEditor.ClearBakedData(cur, atlasDst);
            }

            data.hasMergeAtlas = false;
#endif
        }

        /// <summary> 更新单个Cluster的代理网格 </summary>
        public void UpdateProxyMesh(Cluster cluster)
        {
            var sceneNode = GetSceneNode(GetLayer(cluster));;
            if (sceneNode == null) return;
            var sceneSetting = GetSceneSetting(sceneNode);
            if (cluster == null || cluster.hlodResult == null || cluster.hlodResult.instance == null)
            {
                return;
            }

            var proxy = cluster.hlodResult.instance.GetComponent<Proxy>();
            var go = proxy.gameObject;
            if (proxy == null)
            {
                return;
            }

            var layerSetting = GetLayerSetting(GetLayer(cluster));
            var param = GetAggregateParam(cluster, layerSetting.meshReductionSetting);
            param.cluster = cluster;
            param.useAlphaTest = cluster.openAlphaTest;
            param.useHighRendererLightmap = sceneSetting.useHighRendererLightmap;
            param.useMeshReduction = sceneSetting.useMeshReduction;
            param.useBakeMaterialInfo = sceneSetting.useBakeMaterialInfo;
            param.useLODIndex = sceneSetting.useLODIndex;
            param.rendererBakerSetting = data.rendererBakerSetting;
            param.shaderBindConfig = data.shaderBindConfig;
            param.textureChannel = data.textureChannel;
            param.useVoxel = data.useVoxel;
            param.decalTagList = sceneNode.decalTagList;
            param.outputName = "HLOD_" + GetSceneName(sceneNode) + cluster.name;

            param.outputPath = Path.Combine(GetSceneOutputPath(sceneNode), "temp");

            List<Renderer> renderers = new List<Renderer>();
            cluster.CollectRenderers(renderers);
            FilterLODRender(renderers, cluster.bounds, sceneSetting.clusterMethod, layerSetting);
            if (renderers.Count <= 1)
            {
                return;
            }

            //会删除老数据
            var results = RunAggregate(new List<(List<Renderer>, AggregateParam)>()
                {(renderers, param)});
            var r = results.First();
            if (r != null)
            {
                cluster.hlodResult = r;
                r.instance = go;
                //copy
                var sourcePath = Path.Combine(param.outputPath, param.outputName);
                //要把之前的prefab,mesh,和texture删掉
                var targetPath = Path.Combine(GetSceneOutputPath(sceneNode), param.outputName);
                var directory = new DirectoryInfo(sourcePath);

                foreach (var fileInfo in directory.GetFiles())
                {
                    if (fileInfo.Extension == ".png" || fileInfo.Extension == ".obj" || fileInfo.Extension == ".asset" || fileInfo.Extension == ".tga" || fileInfo.Extension == ".fbx")
                    {
                        var targetFullPath = Path.Combine(targetPath, fileInfo.Name);
                        if (File.Exists(targetFullPath))
                        {
                            File.Delete(targetFullPath);
                        }

                        File.Copy(fileInfo.FullName, targetFullPath, true);
                        if (fileInfo.Extension == ".png" || fileInfo.Extension == ".tga")
                        {
                            var albedo =
                                AssetDatabase.LoadAssetAtPath<Texture>(HLODTool.GetRelativePath(targetFullPath));
                            var texPath = AssetDatabase.GetAssetPath(albedo);
#if _HLOD_USE_ATHENA_ATLASING_
                            AggregateBakedUtils.ImportTexture(texPath,
                                Mathf.Max(param.textureHeight, param.textureWidth),
                                TextureUtility.TexturePlatformType.TPT_ALBEDO);
#else
                            AggregateBakedUtils.ImportTexture(texPath,
                                Mathf.Max(param.textureHeight, param.textureWidth));
#endif
                        }
                    }
                }

                AssetDatabase.Refresh();
                proxy.CalculateVisibleDistance(param.useProxyScreenPercent);
            }

            HLODMessageCenter.SendMessage(HLODMesssages.REBUILD_TREE_VIEW);
        }
        
        public List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList)
        {
            IHLODBuilder builder = null;
            switch (data.hlodMethod)
            {
                case HlodMethod.AthenaSimplify:
                    builder = new AthenaBuilder();
                    break;
                case HlodMethod.Simplygon:
                default:
                    builder = new SimplygonBuilder();
                    break;
            }

            return builder.RunAggregate(aggregateList);
        }

        /// <summary> 包含代理数据 </summary>
        public bool HasProxyMeshData()
        {
            bool has = true;
            foreach (var sceneNode in data.scenes)
            {
                var root = GetHLODRoot(sceneNode, false);
                if (root == null)
                {
                    has = false;
                    break;
                }
            }
            return has;
        }
        
        /// <summary> 获取场景节点 </summary>
        public SceneNode GetSceneNode(Layer layer)
        {
            if (layer == null) return null;
            SceneNode m_sceneNode = null;
            foreach (var sceneNode in data.scenes)
            {
                var m_layer = sceneNode.layers.Find(s => s.Equals(layer));
                if (m_layer != null)
                {
                    m_sceneNode = sceneNode;
                    break;
                }
            }
            return m_sceneNode;
        }

        /// <summary> 获取层节点 </summary>
        public Layer GetLayer(Cluster cluster)
        {
            if (cluster == null) return null;
            Layer m_layer = null;
            foreach (var sceneNode in data.scenes)
            {
                foreach (var layer in sceneNode.layers)
                {
                     var m_cluster = layer.clusters.Find(s => s.Equals(cluster));
                     if (m_cluster != null)
                     {
                         m_layer = layer;
                         break;
                     }
                }
            }
            return m_layer;
        }

        public SceneSetting GetGlobalSetting()
        {
            return GetSceneSetting(null);
        }
        
        /// <summary> 获取场景设置 </summary>
        public SceneSetting GetSceneSetting(SceneNode sceneNode)
        {
            if (sceneNode != null && sceneNode.useOverrideSetting && !sceneNode.firstChangeOverrideState)
            {
                return sceneNode.sceneSetting;
            }
            return data.globalSetting;
        }
        
        /// <summary> 获取层设置 </summary>
        public LayerSetting GetLayerSetting(Layer layer)
        {
            if (layer != null && layer.useOverrideSetting && !layer.firstChangeOverrideState)
            {
                return layer.layerSetting;
            }

            var sceneNode = GetSceneNode(layer);
            return GetSceneSetting(sceneNode).layerSettings[layer != null ? layer.index : 0];
        }

        #endregion

        private void FilterLODRender(List<Renderer> list, Bounds bigBounds, GenerateClusterMethod clusterMethod, LayerSetting setting)
        {
            if (clusterMethod == GenerateClusterMethod.DeepFirst || clusterMethod == GenerateClusterMethod.UE4_Triple)
            {
                FilterLODRender(list, bigBounds, setting.meshReductionSetting);
            }
        }
        
        public void FilterLODRender(List<Renderer> list, Bounds bigBounds, MeshReductionSetting meshReductionSetting)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var render = list[i];
                var lodGroup = render.GetComponentInParent<LODGroup>();
                float culling = 0.0f;
                var bounds = new Bounds();
                if (lodGroup != null)
                {
                    var lods = lodGroup.GetLODs();
                    bool init = false;
                    culling = 1f;
                    foreach (var lod in lods)
                    {
                        foreach (var lodRenderer in lod.renderers)
                        {
                            if (lodRenderer == null) continue;
                            
                            if (!init)
                            {
                                bounds = lodRenderer.bounds;
                                init = true;
                                continue;
                            }

                            bounds.Encapsulate(lodRenderer.bounds);
                        }

                        culling = Mathf.Min(lod.screenRelativeTransitionHeight, culling);
                    }
                }
                else if (meshReductionSetting.useCulling)
                {
                    culling = meshReductionSetting.culling;
                    bounds = render.bounds;
                }

                var length = Mathf.Max(Mathf.Max(bounds.size.x, bounds.size.y), bounds.size.z);
                var bigLength = Mathf.Max(Mathf.Max(bigBounds.size.x, bigBounds.size.y), bigBounds.size.z);
                var percent = length / (bigLength / meshReductionSetting.useProxyScreenPercent);
                if (percent < culling)
                {
                    list.RemoveAt(i);
                }
            }
        }

        private AggregateParam GetAggregateParam(Cluster cluster, MeshReductionSetting meshReductionSetting)
        {
            if (cluster.useOverrideSetting) meshReductionSetting = cluster.meshReductionSetting;
            
            cluster.quality = meshReductionSetting.quality;

            return new AggregateParam()
            {
                textureWidth = meshReductionSetting.albedoSize.x,
                textureHeight = meshReductionSetting.albedoSize.y,
                useOcclusion = meshReductionSetting.useOcclusion,
                cameraPos = meshReductionSetting.cameraPos,
                camYawAngle = meshReductionSetting.yaw,
                camPitchAngle = meshReductionSetting.pitch,
                camCoverage = meshReductionSetting.coverage,
                useProxyScreenPercent = meshReductionSetting.useProxyScreenPercent,
                smoothNormals = meshReductionSetting.smoothNormals,
                enableMeshOptimation = meshReductionSetting.enableMeshOptimation,
                fixMaterialQueue = meshReductionSetting.fixMaterialQueue,
                materialQueue = meshReductionSetting.materialQueue,
                targetShader = meshReductionSetting.targetShader,
                reductionSetting = new ReductionSetting()
                {
                    triangleRatio  = cluster.quality,
                    enableMaxEdgeLength = meshReductionSetting.enableMaxEdgeLength,
                    maxEdgeLength = meshReductionSetting.maxEdgeLength,
                    enableScreenSize = meshReductionSetting.enableScreenSize,
                    screenSize = (uint)meshReductionSetting.screenSize,
                    enableMaxDeviation = meshReductionSetting.enableMaxDeviation,
                    maxDeviation = meshReductionSetting.maxDeviation,
                    lockGeometricBorder = meshReductionSetting.lockGeometricBorder,
                    geometryImportance = meshReductionSetting.geometryImportance,
                    albedoSize = meshReductionSetting.albedoSize,
                    weldingThreshold = meshReductionSetting.weldingThreshold
                },
            };
        }

        private void EndProxyMeshGenerate(SceneNode sceneNode, TimeSpan span = new TimeSpan())
        {
            HLODMessageCenter.SendMessage(HLODMesssages.REBUILD_TREE_VIEW);
            EditorSceneManager.MarkSceneDirty(sceneNode.scene);
            var info = "生成网格完成,耗时:" + span.ToString(@"hh\:mm\:ss") + "请注意保存场景,以免数据丢失";
            //EditorUtility.DisplayDialog("",info , "确定");
            HLODDebug.Log("[HLOD]" + info);
        }


#if _HLOD_USE_ATHENA_ATLASING_
        private void DoMergeProxyAtlasRecursive(List<Proxy> proxies, Transform parent)
        {
            if (proxies == null || proxies.Count == 0)
                return;

            var rendererList = new List<GameObject>();
            Shader shader = null;
            foreach (var proxy in proxies)
            {
                rendererList.Add(proxy.proxyRenderer.gameObject);
                shader = shader ?? proxy.proxyRenderer.sharedMaterial.shader;
            }

            var go = parent.gameObject;
            var filter = go.AddComponent<AtlasingMaterialFilter>();
            if (filter != null)
            {
                filter.Initialize();
                filter.targetShader = shader;
                filter.LayerMask = -1;
            }

            TextureAtlasing textureAtalsing = null;
            if (filter.m_Target != null)
            {
                textureAtalsing = filter.m_Target;
            }
            else
            {
                textureAtalsing = go.AddComponent<TextureAtlasing>();
                filter.m_Target = textureAtalsing;
            }

            Assembly assembly = Assembly.GetAssembly(typeof(MaterialFilterEditor));
            dynamic e1 = assembly.CreateInstance(typeof(MaterialFilterEditor).ToString()); // 创建类的实例 
            var editor1 = e1 as MaterialFilterEditor;
            //var editor2 = assembly.CreateInstance(typeof(TextureAtlasingEditor).ToString()) as TextureAtlasingEditor;
            //var editor2 = assembly.CreateInstance(typeof(TextureAtlasingEditor).ToString()) as TextureAtlasingEditor;

            // //editor1.xxx
            // MaterialFilterEditor.FilterGameObjectToAtlasTool(filter, rendererList.ToArray());
            editor1.FilterGameObjectToAtlasTool(filter, rendererList.ToArray());
            // //editor2.xxx
            // TextureAtlasingEditor.ApplyAtlasingToScene(textureAtalsing);
            var atlasDst = Path.Combine(GetSceneOutputPathRelative(), "Atlasing_" + parent.name);
            TextureAtlasingEditor.ApplyAtlasingToScene(textureAtalsing, atlasDst);
            //GameObject.DestroyImmediate(go);


            foreach (var proxy in proxies)
            {
                DoMergeProxyAtlasRecursive(proxy.children, proxy.transform);
            }
        }
#endif
        private void GenerateProxy(SceneNode sceneNode, Cluster cluster = null)
        {
            // var resultList = sceneNode.resultList;
            // if (resultList == null)
                // return;

            var root = GetHLODRoot(sceneNode);
            if (cluster != null)
            {
                var cluseterTrans = root.transform.Find("HLOD_" + GetSceneName(sceneNode) + cluster.name);
                if (cluseterTrans)
                {
                    GameObject.DestroyImmediate(cluseterTrans.gameObject);
                }
            }
            else
            {
                if (root) GameObject.DestroyImmediate(root);
                root = CreateHLODRoot(sceneNode);
            }
            if (sceneNode.targetParent) root.transform.parent = sceneNode.targetParent.transform;
            ProxyManager proxyManager = root.GetComponent<ProxyManager>();
            if (proxyManager == null) proxyManager = root.AddComponent<ProxyManager>();

            generateErrorList.Clear();
            HashSet<Cluster> caches = new HashSet<Cluster>();
            for (int i = sceneNode.layers.Count - 1; i >= 0; i--)
            {
                foreach (var c in sceneNode.layers[i].clusters)
                {
                    if (cluster != null && cluster != c)
                    {
                        continue;
                    }
                    
                    Proxy proxy = null;
                    try
                    {
                        proxy = GenerateProxy(root.transform, c, caches);
                    }
                    catch (Exception e)
                    {
                        HLODDebug.LogWarning("[hlod][GenerateProxy] cluster:" + c.name + " is null\n" + e.Message);
                    }

                    if (proxy != null)
                        proxyManager.proxies.Add(proxy);

                    //for体验，把显示状态跟别的一样
                    if (cluster != null)
                    {
                        foreach (var VARIABLE in proxyManager.proxies)
                        {
                            if (VARIABLE != null && VARIABLE.GetVisibility())
                            {
                                proxy.SetVisibilityRecursive(true, true, true);
                                break;
                            }
                        }
                    }
                }
            }
            proxyManager.proxies.RemoveAll(s => s == null);

            var flags = StaticEditorFlags.OccludeeStatic;
            Transform[] allChilds = root.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChilds)
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);
            }

            GameObjectUtility.SetStaticEditorFlags(root, flags);

            if (generateErrorList.Count != 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[HLOD][GenerateProxyErrors] There are some prefabs is null:");
                foreach (var error in generateErrorList)
                {
                    sb.AppendLine(error);
                }

                HLODDebug.LogError(sb.ToString());
            }
        }

        private List<string> generateErrorList = new List<string>();

        private Proxy GenerateProxy(Transform anchor, Cluster cluster,
            HashSet<Cluster> caches)
        {
            if (caches.Contains(cluster))
                return null;
            caches.Add(cluster);

            GameObject go = null;
            if (data.debug)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetParent(anchor, true);
                go.transform.position = cluster.bounds.center;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = cluster.bounds.size;
                go.GetComponent<Renderer>().forceRenderingOff = true;
                go.name = cluster.name;
            }
            else
            {
                go = PrefabUtility.InstantiatePrefab(cluster.hlodResult.prefab, anchor.transform) as GameObject;
                go.transform.localPosition = cluster.hlodResult.prefab.transform.position;
                cluster.hlodResult.instance = go;
            }

            if (go == null)
            {
                generateErrorList.Add(" ClusterName:" + cluster.name);
                return null;
            }

            var proxy = go.AddComponent<Proxy>();
            
            proxy.bounds = cluster.bounds;
            proxy.proxyRenderer = go.GetComponentInChildren<Renderer>();
            var manifestList = GetManifests(cluster);
            proxy.mainfests = manifestList.ToArray();
            
            foreach (var child in cluster.clusters)
            {
                var proxyChild = GenerateProxy(proxy.transform, child, caches);
                if (proxyChild != null)
                    proxy.children.Add(proxyChild);
            }
            
            proxy.CalculateVisibleDistance(cluster.usePercent);
            proxy.Init();

            return proxy;
        }

        private List<Manifest> GetManifests(Cluster cluster)
        {
            var sceneSetting = GetSceneSetting(GetSceneNode(GetLayer(cluster)));
            
            var manifestList = new List<Manifest>();
            var manifestSet = new HashSet<GameObject>();
            foreach (var entity in cluster.entities)
            {
                GameObject contollNode = null;
                switch (data.proxyMapType)
                {
                    case ProxyMapType.LODGroup:
                        contollNode = entity.FectchControlNodeByLODGroup();
                        break;
                    case ProxyMapType.PrefabRoot:
                        contollNode = entity.FectchControlNodeByPrefabRoot();
                        break;
                }
                if (contollNode == null)
                {
                    HLODDebug.LogWarning("[HLOD][GenerateProxy] the entity.FectchControlNode is null! cluster:" + cluster.name);
                    continue;
                }
                if (manifestSet.Contains(contollNode)) continue;
                
                var manifest = new Manifest(contollNode);
                manifest.Init();
                manifestList.Add(manifest);
                manifestSet.Add(contollNode);
            }

            return manifestList;
        }

        protected virtual string GetSceneOutputPath(SceneNode sceneNode)
        {
            var scene = sceneNode.scene;
            if (scene != null)
                return Path.Combine(Application.dataPath, "Raw/Scene/HLOD",
                    Path.GetFileNameWithoutExtension(scene.path));
            return null;
        }

        private bool IsIgnoreNode(GameObject curGO)
        {
            if (curGO == null)
                return false;

            var ignoreNode = curGO.GetComponent<HLODIgnoreNode>();
            return ignoreNode != null;
        }

        protected virtual void CollectRenders(SceneNode sceneNode, GameObject curGo, List<(GameObject, LODGroup)> entities, bool CheckSelfNode = true)
        {
            var sceneSetting = GetSceneSetting(sceneNode);
            if (curGo == null)
                return;

            var clusterNode = curGo.GetComponent<HLODClusterNode>();
            if (CheckSelfNode && clusterNode != null)
            {
                return;
            }

            var renderer = curGo.GetComponent<Renderer>();
            var lodGroup = curGo.GetComponentInParent<LODGroup>();
            if (renderer != null)
            {
                if (lodGroup == null)
                {
                    if (curGo.activeInHierarchy && !IsIgnoreNode(curGo))
                        entities.Add((curGo, lodGroup));
                }
                else
                {
                    var lods = lodGroup.GetLODs();
                    if (lods.Length > sceneSetting.useLODIndex && lods[sceneSetting.useLODIndex].renderers.Contains(renderer) && !IsIgnoreNode(lodGroup.gameObject))
                        entities.Add((curGo, lodGroup));
                }
            }

            for (var i = 0; i < curGo.transform.childCount; ++i)
            {
                var go = curGo.transform.GetChild(i).gameObject;

                if (go != null)
                    CollectRenders(sceneNode, go, entities);
            }
        }

        protected virtual List<Entity> CollectEntity(SceneNode sceneNode)
        {
            return entitiyCollector.CollectEntity(sceneNode, GetSceneSetting(sceneNode).useLODIndex);
        }
        
        protected virtual List<Cluster> CollectClustersByPrefab(List<Entity> entities)
        {
            List<List<Entity>> prefabEntities = new List<List<Entity>>();
            foreach (var entity in entities)
            {
                var list = prefabEntities.Find(s => s.First() != null && s.First().prefabRoot != null && s.First().prefabRoot.Equals(entity.prefabRoot));
                if (list != null)
                {
                    list.Add(entity);
                }
                else
                {
                    prefabEntities.Add(new List<Entity>(){ entity });
                }
            }
            List<Cluster> clusters = new List<Cluster>();
            foreach (var list in prefabEntities)
            {
                var cluster = new Cluster(list);
                clusters.Add(cluster);
                cluster.name = cluster.entities.First().manifest.name;
            }
            
            return clusters;
        }

        protected virtual List<Cluster> CollectClusterNode(SceneNode sceneNode)
        {
            var list = new List<Cluster>();
            if (sceneNode == null || sceneNode.roots == null)
                return list;

            int index = 0;
            HashSet<GameObject> set = new HashSet<GameObject>();
            foreach (var root in sceneNode.roots)
            {
                if (root == null || set.Contains(root))
                    continue;

                set.Add(root);

                var nodes = root.GetComponentsInChildren<HLODClusterNode>();
                var nodeList = new List<HLODClusterNode>(nodes);
                var selfNode = root.GetComponent<HLODClusterNode>();
                if (selfNode != null)
                    nodeList.Add(selfNode);
                if (nodeList.Count == 0)
                    continue;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    EditorUtility.DisplayProgressBar("CollectClusterNode", i + "/" + nodes.Length,
                        (float) i / nodes.Length);

                    var renders = new List<(GameObject, LODGroup)>();
                    CollectRenders(sceneNode, node.gameObject, renders, false);
                    var entites = new List<Entity>();
                    foreach (var tuple in renders)
                    {
                        entites.Add(new Entity(tuple.Item1));
                    }

                    var cluster = new Cluster(entites)
                    {
                        name = "Cluster-Node-" + index + "_" + node.name,
                    };
                    index++;

                    list.Add(cluster);
                }
            }

            return list;
        }

        private List<Cluster> CollectClusterByVolume(SceneNode sceneNode, List<Entity> entities)
        {
            var volumeList = GameObject.FindObjectsOfType<HLODClusterVolume>();
            var clusterList = new List<Cluster>();
            var newList = entities;
            for (int t = 0; t < volumeList.Length; t++)
            {
                EditorUtility.DisplayProgressBar("CollectClusterNode", t + "/" + volumeList.Length,
                    (float) t / volumeList.Length);

                var volume = volumeList[t];
                if (volume.gameObject.scene.path.Equals(sceneNode.scenePath))
                {
                    var volumeEntities = new List<Entity>();
                    for (int i = newList.Count - 1; i >= 0; i--)
                    {
                        var entity = newList[i];

                        var renderers = entity.FetchRenderer();
                        bool contain = true;
                        foreach (var renderer in renderers)
                        {
                            if (!volume.Contains(renderer))
                            {
                                contain = false;
                                break;
                            }
                        }

                        if (contain)
                        {
                            volumeEntities.Add(entity);
                        }

                    }

                    if (volumeEntities.Count >= 2)
                    {
                        var cluster = new Cluster(volumeEntities);
                        cluster.name = volume.name;

                        foreach (var volumeEntity in volumeEntities)
                        {
                            newList.Remove(volumeEntity);
                        }

                        clusterList.Add(cluster);
                    }
                }
            }

            return clusterList;
        }

        private void FliterAlphaTestEntity(List<Entity> entities, List<Cluster> clusterNodes,
            List<Cluster> clusterVolumes, AlphaTestType filterType)
        {
            if (filterType == AlphaTestType.All)
                return;
            bool onlyAlphaTest = filterType == AlphaTestType.OnlyAlphaTest;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(entities[i].FetchRenderer());
                if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                {
                    entities.RemoveAt(i);
                }
            }

            foreach (var cluster in clusterNodes)
            {
                for (int i = cluster.entities.Count - 1; i >= 0; i--)
                {
                    bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(cluster.entities[i].FetchRenderer());
                    if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                    {
                        cluster.entities.RemoveAt(i);
                    }
                }
            }

            foreach (var cluster in clusterVolumes)
            {
                for (int i = cluster.entities.Count - 1; i >= 0; i--)
                {
                    bool hasAlphaTest = HLODProviderUtils.HasAlphaTest(cluster.entities[i].FetchRenderer());
                    if (onlyAlphaTest && !hasAlphaTest || !onlyAlphaTest && hasAlphaTest)
                    {
                        cluster.entities.RemoveAt(i);
                    }
                }
            }
        }

        private void CollectData(SceneNode sceneNode, List<Layer> layers, AlphaTestType filterType = AlphaTestType.All)
        {
            var sceneSetting = GetSceneSetting(sceneNode);
            
            var entities = CollectEntity(sceneNode);
            var clusterNodes = CollectClusterNode(sceneNode);
            var clusterVolumes = CollectClusterByVolume(sceneNode, entities);
            if (entities.Count <= 0 && clusterNodes.Count <= 0 && clusterVolumes.Count <= 0)
                return;

            if (sceneSetting.slpitAlphaTest)
            {
                FliterAlphaTestEntity(entities, clusterNodes, clusterVolumes, filterType);
            }

            List<Cluster> lowLevelClusters = new List<Cluster>();
            //按所有renderer
            // foreach (var entity in entities)
            // {
            //     var cluster = new Cluster(entity);
            //     lowLevelClusters.Add(cluster);
            //     cluster.name = cluster.entities.First().manifest.name;
            // }
            //按prefabRoot or self 
            lowLevelClusters = CollectClustersByPrefab(entities);

            for (int i = 0; i < layers.Count; i++)
            {
                var startTime = DateTime.Now;

                var layer = layers[i];
                var layerSetting = GetLayerSetting(layer);
                lowLevelClusters = HLODClusterCalculator.CollectCluster(lowLevelClusters, sceneSetting.clusterMethod, layerSetting.clusterSetting, layer.clusters);
                if (i == 0)
                {
                    lowLevelClusters.AddRange(clusterNodes);
                    layer.clusters.AddRange(clusterNodes);
                    lowLevelClusters.AddRange(clusterVolumes);
                    layer.clusters.AddRange(clusterVolumes);
                }

                int clusterIndex = 0;
                foreach (var cluster in lowLevelClusters)
                {
                    if (string.IsNullOrEmpty(cluster.name))
                        cluster.name = string.Format("Cluster-{0}-{1}", i, clusterIndex++);
                }

                var deltaTime = DateTime.Now - startTime;
                HLODDebug.Log("[HLOD][CollectCluster]Layer" + i + " Used Time:" + deltaTime.ToString(@"hh\:mm\:ss"));
            }

            if (sceneSetting.slpitAlphaTest)
            {
                bool onlyAlphaTest = filterType == AlphaTestType.OnlyAlphaTest;
                foreach (var layer in layers)
                {
                    foreach (var cluster in layer.clusters)
                    {
                        cluster.name = cluster.name +
                                       (onlyAlphaTest ? "_AlphaTest" : "_NotAlphaTest");
                        cluster.openAlphaTest = onlyAlphaTest;
                    }
                }
            }
        }

        private void CollectDataConsiderAlphaTest(SceneNode sceneNode)
        {
            var alphaLayers = new List<Layer>();
            var notAlphaLayers = new List<Layer>();
            for (int i = 0; i < sceneNode.layers.Count; i++)
            {
                var layer = new Layer();
                var layer2 = new Layer();

                layer.layerSetting = GetLayerSetting(sceneNode.layers[i]);
                layer2.layerSetting = GetLayerSetting(sceneNode.layers[i]);

                alphaLayers.Add(layer);
                notAlphaLayers.Add(layer2);
            }

            //1.先过滤ALphaTest,对所有没有AlphaTest的合并一遍
            CollectData(sceneNode, notAlphaLayers, AlphaTestType.OnlyNotAlphaTest);
            //2.再对只考虑AlphaTest的,合并一遍
            CollectData(sceneNode, alphaLayers, AlphaTestType.OnlyAlphaTest);

            //3.将两次结果合并到记录
            for (int i = 0; i < sceneNode.layers.Count; i++)
            {
                sceneNode.layers[i].clusters.AddRange(alphaLayers[i].clusters);
                sceneNode.layers[i].clusters.AddRange(notAlphaLayers[i].clusters);
            }
        }

        void CollectDecalTags(SceneNode sceneNode)
        {
            HashSet<GameObject> set = new HashSet<GameObject>();
            var list = new List<HLODDecalTag>();
            foreach (var root in sceneNode.roots)
            {
                if (set.Contains(root) || root == null)
                    continue;
                set.Add(root);
                var decalTags = root.GetComponentsInChildren<HLODDecalTag>();
                foreach (var decalTag in decalTags)
                {
                    if (!list.Contains(decalTag))
                        list.Add(decalTag);
                }
            }
            sceneNode.decalTagList = list;
        }

        protected virtual string GetSceneName(SceneNode sceneNode)
        {
            var scene = sceneNode.scene;
            if (scene != null)
                return scene.name + "_";
            return "";
        }

        protected virtual GameObject GetHLODRoot(SceneNode sceneNode, bool autoCreateWhenNull = true)
        {
            var HLODRootName = GetHLODRootName();

            var scene = sceneNode.scene;
            
            GameObject hlodRoot = null;
            if (sceneNode.targetParent && sceneNode.targetParent.transform.Find(HLODRootName))
            {
                return sceneNode.targetParent.transform.Find(HLODRootName).gameObject;
            }
            else
            {
                foreach (var gameObject in scene.GetRootGameObjects())
                {
                    if (gameObject.name.Equals(HLODRootName))
                    {
                        hlodRoot = gameObject;
                    }
                }
            }

            if (hlodRoot == null && autoCreateWhenNull)
            {
                hlodRoot = CreateHLODRoot(sceneNode);
            }
            if (hlodRoot && hlodRoot.transform.parent) sceneNode.targetParent = hlodRoot.transform.parent.gameObject;
            return hlodRoot;
        }
        
        protected virtual  GameObject CreateHLODRoot(SceneNode sceneNode)
        {
            GameObject hlodRoot = null;
            foreach (var rootGameObject in sceneNode.scene.GetRootGameObjects())
            {
                if (rootGameObject)
                {
                    hlodRoot = new GameObject(GetHLODRootName());
                    hlodRoot.transform.SetParent(rootGameObject.transform);
                    hlodRoot.transform.SetParent(null);
                    break;
                }
            }
            return hlodRoot;
        }
        
        protected virtual string GetHLODRootName()
        {
            return HLODConstants.HLOD_ROOT_NAME;
        }
    }
}
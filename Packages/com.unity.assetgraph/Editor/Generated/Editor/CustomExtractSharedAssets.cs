using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

#if UNITY_5_6_OR_NEWER
using UnityEngine.Profiling;
#endif

using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using ZeusAssetBuild;

namespace UnityEngine.AssetGraph
{
    [CustomNode("Configure Bundle/Custom Extract Shared Assets", 71)]
    public class CustomExtractSharedAssets : Node
    {
        enum GroupingType : int
        {
            ByFileSize,
            ByRuntimeMemorySize
        };

        [SerializeField] private bool m_pauseRefresh = true;
        [SerializeField] private bool m_generateShaderBundle = false;
        [SerializeField] private string m_generateShaderBundleName = "shared_shader.ab";
        [SerializeField] private bool m_useAggBundle = false;
        [SerializeField] private string m_aggBundleJsonPath = "OuterPackage/aggList.csv";

        [SerializeField] private bool m_materialRedundancy = false;
        [SerializeField] private bool m_prefabRedundancy = false;
        [SerializeField] private string m_bundleNameTemplate;
        [SerializeField] private SerializableMultiTargetInt m_groupExtractedAssets;
        [SerializeField] private SerializableMultiTargetInt m_groupSizeByte;
        [SerializeField] private SerializableMultiTargetInt m_groupingType;

        [SerializeField] private bool m_sharedShader = false;
        public override string ActiveStyle
        {
            get
            {
                return "node 3 on";
            }
        }

        public override string InactiveStyle
        {
            get
            {
                return "node 3";
            }
        }

        public override string Category
        {
            get
            {
                return "Configure";
            }
        }

        public override Model.NodeOutputSemantics NodeInputType
        {
            get
            {
                return Model.NodeOutputSemantics.AssetBundleConfigurations;
            }
        }

        public override Model.NodeOutputSemantics NodeOutputType
        {
            get
            {
                return Model.NodeOutputSemantics.AssetBundleConfigurations;
            }
        }

        public override void Initialize(Model.NodeData data)
        {
            m_bundleNameTemplate = "shared_*";
            m_groupExtractedAssets = new SerializableMultiTargetInt();
            m_groupSizeByte = new SerializableMultiTargetInt();
            m_groupingType = new SerializableMultiTargetInt();
            data.AddDefaultInputPoint();
            data.AddDefaultOutputPoint();
        }

        public override Node Clone(Model.NodeData newData)
        {
            var newNode = new CustomExtractSharedAssets();
            newNode.m_groupExtractedAssets = new SerializableMultiTargetInt(m_groupExtractedAssets);
            newNode.m_groupSizeByte = new SerializableMultiTargetInt(m_groupSizeByte);
            newNode.m_groupingType = new SerializableMultiTargetInt(m_groupingType);
            newNode.m_bundleNameTemplate = m_bundleNameTemplate;
            newData.AddDefaultInputPoint();
            newData.AddDefaultOutputPoint();
            return newNode;
        }

        public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
        {

            EditorGUILayout.HelpBox("Extract Shared Assets: Extract shared assets between asset bundles and add bundle configurations.", MessageType.Info);
            editor.UpdateNodeName(node);

            GUILayout.Space(10f);

            var newValue = EditorGUILayout.TextField("Bundle Name Template", m_bundleNameTemplate);
            if (newValue != m_bundleNameTemplate)
            {
                using (new RecordUndoScope("Bundle Name Template Change", node, true))
                {
                    m_bundleNameTemplate = newValue;
                    onValueChanged();
                }
            }

            GUILayout.Space(10f);

            //Show target configuration tab
            editor.DrawPlatformSelector(node);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = editor.DrawOverrideTargetToggle(node, m_groupSizeByte.ContainsValueOf(editor.CurrentEditingGroup), (bool enabled) =>
                {
                    using (new RecordUndoScope("Remove Target Grouping Size Settings", node, true))
                    {
                        if (enabled)
                        {
                            m_groupExtractedAssets[editor.CurrentEditingGroup] = m_groupExtractedAssets.DefaultValue;
                            m_groupSizeByte[editor.CurrentEditingGroup] = m_groupSizeByte.DefaultValue;
                            m_groupingType[editor.CurrentEditingGroup] = m_groupingType.DefaultValue;
                        }
                        else
                        {
                            m_groupExtractedAssets.Remove(editor.CurrentEditingGroup);
                            m_groupSizeByte.Remove(editor.CurrentEditingGroup);
                            m_groupingType.Remove(editor.CurrentEditingGroup);
                        }
                        onValueChanged();
                    }
                });

                using (disabledScope)
                {
                    var useGroup = EditorGUILayout.ToggleLeft("Allowed redundancy assets by size", m_groupExtractedAssets[editor.CurrentEditingGroup] != 0);
                    if (useGroup != (m_groupExtractedAssets[editor.CurrentEditingGroup] != 0))
                    {
                        using (new RecordUndoScope("Change Grouping Type", node, true))
                        {
                            m_groupExtractedAssets[editor.CurrentEditingGroup] = (useGroup) ? 1 : 0;
                            onValueChanged();
                        }
                    }

                    using (new EditorGUI.DisabledScope(!useGroup))
                    {
                        var newType = (GroupingType)EditorGUILayout.EnumPopup("Grouping Type", (GroupingType)m_groupingType[editor.CurrentEditingGroup]);
                        if (newType != (GroupingType)m_groupingType[editor.CurrentEditingGroup])
                        {
                            using (new RecordUndoScope("Change Grouping Type", node, true))
                            {
                                m_groupingType[editor.CurrentEditingGroup] = (int)newType;
                                onValueChanged();
                            }
                        }

                        var newSizeText = EditorGUILayout.TextField("Size(KB)", m_groupSizeByte[editor.CurrentEditingGroup].ToString());
                        int newSize = 0;
                        Int32.TryParse(newSizeText, out newSize);

                        if (newSize != m_groupSizeByte[editor.CurrentEditingGroup])
                        {
                            using (new RecordUndoScope("Change Grouping Size", node, true))
                            {
                                m_groupSizeByte[editor.CurrentEditingGroup] = newSize;
                                onValueChanged();
                            }
                        }
                    }
                }
            }


            bool NewValue = EditorGUILayout.Toggle("PauseRefresh", m_pauseRefresh);
            if (NewValue != m_pauseRefresh)
            {
                m_pauseRefresh = NewValue;
                if (!NewValue)
                {
                    using (new RecordUndoScope("Modify PauseRefresh", node, true))
                    {
                        onValueChanged();
                    }
                }
            }

            NewValue = EditorGUILayout.Toggle("GenerateShaderBundle", m_generateShaderBundle);
            if (NewValue != m_generateShaderBundle)
            {
                m_generateShaderBundle = NewValue;
                if (!NewValue)
                {
                    using (new RecordUndoScope("Modify GenerateShaderBundle", node, true))
                    {
                        onValueChanged();
                    }
                }
            }
            if (m_generateShaderBundle)
            {
                var newName = EditorGUILayout.TextField("", m_generateShaderBundleName);
                if (newName != m_generateShaderBundleName)
                {
                    m_generateShaderBundleName = newName;
                    onValueChanged();
                }
            }
            
            m_useAggBundle = EditorGUILayout.Toggle("开启资源聚合", m_useAggBundle);
            if (m_useAggBundle)
            {
                var newName = EditorGUILayout.TextField("", m_aggBundleJsonPath);
                if (newName != m_aggBundleJsonPath)
                {
                    m_aggBundleJsonPath = newName;
                    onValueChanged();
                }
            }

            NewValue = EditorGUILayout.Toggle("材质冗余", m_materialRedundancy);
            if (NewValue != m_materialRedundancy)
            {
                m_materialRedundancy = NewValue;
                onValueChanged();
            }
            NewValue = EditorGUILayout.Toggle("Prefab冗余", m_prefabRedundancy);
            if (NewValue != m_prefabRedundancy)
            {
                m_prefabRedundancy = NewValue;
                onValueChanged();
            }
            EditorGUILayout.HelpBox("Bundle Name Template replaces \'*\' with number.", MessageType.Info);
        }

        //每次Graph视图刷新时 Prepare()都会执行
        public override void Prepare(BuildTarget target,
            Model.NodeData node,
            IEnumerable<PerformGraph.AssetGroups> incoming,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output)
        {
            if (m_pauseRefresh && !IsBuild || Output == null || incoming == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(m_bundleNameTemplate))
            {
                throw new NodeException("Bundle Name Template is empty.", "Set valid bundle name template.", node);
            }
            if (m_groupExtractedAssets[target] != 0 && m_groupSizeByte[target] < 0)
            {
                throw new NodeException("Invalid size. Size property must be a positive number.", "Set valid size.", node);
            }
            
            DateTime beginTime = DateTime.Now;
            var destination = (connectionsToOutput == null || !connectionsToOutput.Any()) ?
                null : connectionsToOutput.First();
            var buildMap = AssetBundleBuildMap.GetBuildMap();
            buildMap.Clear();
            
            //资源聚合处理
            if (m_useAggBundle)
            {
                if (!File.Exists(m_aggBundleJsonPath))
                {
                    Debug.LogError($"未能找到聚合文件 : {m_aggBundleJsonPath}!");
                }
                else
                {
                    incoming = GetAggIncoming(incoming);
                    AddBundleToBuildMap(node, incoming.Last()?.assetGroups);
                }
            }
            //bundleName to bundleNode
            var bundleNodeDict = new Dictionary<string, ZeusAssetBuild.ZeusBundleNode>();
            var assetNodeDict = new Dictionary<string, ZeusAssetBuild.ZeusAssetNode>();
            ParseExplicitAssetAndBundle(incoming, bundleNodeDict, assetNodeDict);
            
            foreach (var pair in bundleNodeDict)
            {
                var bundleNode = pair.Value; 
                foreach (var assetNode in bundleNode.AssetList)
                {
                    CollectDependencies(assetNode, bundleNode, assetNodeDict, bundleNodeDict, m_materialRedundancy, m_prefabRedundancy);
                }
            }
            AssetDatabaseEx.SaveDependData();
            DepShaderProcessor(assetNodeDict, bundleNodeDict);
            RemoveInvalidSharedAsset(bundleNodeDict, assetNodeDict);
            CustomExtractAction(bundleNodeDict, assetNodeDict);
            ProcessSharedAsset(target, assetNodeDict, bundleNodeDict);
            OutputBundleDict(bundleNodeDict);
            AddBundleToBuildMap(node, bundleNodeDict);
            Output(destination, TransformToOutDict(bundleNodeDict));
            System.DateTime endTime = System.DateTime.Now;
            UnityEngine.Debug.Log("ExtractSharedAssets cost " + (endTime - beginTime).TotalSeconds);
        }

        private void RemoveInvalidSharedAsset(Dictionary<string, ZeusBundleNode> bundleNodeDict, Dictionary<string, ZeusAssetNode> assetNodeDict)
        {
            var tempAssetList = new List<string>();
            foreach (var pair in assetNodeDict)
            {
                var assetNode = pair.Value;
                if (!assetNode.Flag.HasFlag(ZeusAssetBuild.AssetFlag.ExplicitAsset))
                {
                    if (assetNode.IsExplicitSharedAsset())
                    {
                        var bundleName = assetNode.DepBundleSet.First();
                        assetNode.BundleName = bundleName;
                        bundleNodeDict[bundleName].AssetList.Add(assetNode);
                    }
                    else if (assetNode.DepBundleSet.Count <= 1)
                    {
                        tempAssetList.Add(assetNode.AssetPath);
                    }
                    else
                    {
                        assetNode.Flag = ZeusAssetBuild.AssetFlag.SharedAsset;
                    }
                }
            }
            foreach (var assetPath in tempAssetList)
            {
                assetNodeDict.Remove(assetPath);
            }
        }

        private void DumpAssetsMap(string filePath, Dictionary<string, List<AssetReference>> assetsMap)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach(var pair in assetsMap)
            {
                strBuilder.AppendLine(pair.Key);
                foreach(var ar in pair.Value)
                {
                    strBuilder.AppendLine("    " + ar.importFrom);
                }
            }
            File.WriteAllText(filePath, strBuilder.ToString());
        }


        protected virtual void CollectDependencies(ZeusAssetNode assetNode,
                                     ZeusBundleNode bundleNode,
                                     Dictionary<string, ZeusAssetNode> assetDict,
                                     Dictionary<string, ZeusBundleNode> bundleDict,
                                     bool materialRedundancy = false,
                                     bool prefabRedundancy = false)
        {
            var dependencies = AssetDatabaseEx.GetDependencies(assetNode.AssetPath);
            
            bool isScene = System.IO.Path.GetExtension(assetNode.AssetPath).ToLower() == ".unity";

            ZeusAssetNode tempAssetNode = null;
            ZeusBundleNode tempBundleNode = null;
            Dictionary<string, ZeusBundleNode> bundleDeps = new Dictionary<string, ZeusBundleNode>();

            foreach (var d in dependencies)
            {
                if (d == assetNode.AssetPath)
                    continue;

                if (assetDict.TryGetValue(d, out tempAssetNode) && !string.IsNullOrEmpty(tempAssetNode.BundleName))
                {
                    var ddeps = AssetDatabaseEx.GetDependencies(d);
                    foreach (var dd in ddeps)
                    {
                        if (!bundleDeps.ContainsKey(dd))
                        {
                            bundleDeps.Add(dd, bundleDict[tempAssetNode.BundleName]);
                        }
                    }
                }
            }

            foreach (var d in dependencies)
            {
                if (d == assetNode.AssetPath)
                    continue;

                if (isScene && System.IO.Path.GetExtension(d).ToLower() == ".unity")
                {
                    Debug.LogError(assetNode.AssetPath + " depend scene : " + d);
                }
                tempAssetNode = null;
                //skip explicit asset
                if (assetDict.TryGetValue(d, out tempAssetNode) && !string.IsNullOrEmpty(tempAssetNode.BundleName))
                {
                    continue;
                }
                //skip bundle asset and it's deps
                if (bundleDeps.TryGetValue(d, out tempBundleNode))
                {
                    if(tempAssetNode == null)
                    {
                        tempAssetNode = new ZeusAssetBuild.ZeusAssetNode(d);
                        assetDict.Add(tempAssetNode.AssetPath, tempAssetNode);
                    }
                    tempAssetNode.ExplicitSharedBundle.Add(tempBundleNode.BundleName);
                    tempAssetNode.Flag |= ZeusAssetBuild.AssetFlag.ExplicitSharedAsset;
                    continue;
                }

                // AssetBundle must not include script asset
                if (TypeUtility.GetMainAssetFuzzyTypeAtPath(d) == typeof(MonoScript))
                {
                    continue;
                }
                if (materialRedundancy && TypeUtility.GetMainAssetFuzzyTypeAtPath(d) == typeof(UnityEngine.Material))
                {
                    //Debug.Log(assetPath + " redundancy : " + d);
                    continue;
                }
                if (prefabRedundancy && isScene && System.IO.Path.GetExtension(d).ToLower() == ".prefab")
                {
                    //Debug.Log(assetPath + " redundancy : " + d);
                    continue;
                }

                if (!assetDict.TryGetValue(d, out tempAssetNode))
                {
                    tempAssetNode = new ZeusAssetBuild.ZeusAssetNode(d);
                    assetDict.Add(tempAssetNode.AssetPath, tempAssetNode);
                    tempAssetNode.DepBundleSet.Add(bundleNode.BundleName);
                }
                else
                {
                    tempAssetNode.DepBundleSet.Add(bundleNode.BundleName);
                }
            }
        }

        private long GetSizeOfAsset(AssetReference a, GroupingType t)
        {

            long size = 0;

            // You can not read scene and do estimate
            if (a.isSceneAsset)
            {
                t = GroupingType.ByFileSize;
            }

            if (t == GroupingType.ByRuntimeMemorySize)
            {
                var objects = a.allData;
                foreach (var o in objects)
                {
                    if(o != null)
                    {
#if UNITY_5_6_OR_NEWER
                        size += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(o);
#else
                        size += Profiler.GetRuntimeMemorySize(o);
#endif
                    }

                }

                a.ReleaseData();
            }
            else if (t == GroupingType.ByFileSize)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(a.absolutePath);
                if (fileInfo.Exists)
                {
                    size = fileInfo.Length;
                }
            }

            return size;
        }

        private IEnumerable<PerformGraph.AssetGroups> GetAggIncoming(IEnumerable<PerformGraph.AssetGroups> oriIncoming)
        {
            var aggDictionary = new Dictionary<string, string>();
            using (var sr = new StreamReader(m_aggBundleJsonPath))
            {
                string currentLine;
                while ((currentLine = sr.ReadLine()) != null)
                {
                    var pair = currentLine.Split(',');
                    aggDictionary.Add(pair[0], pair[1]);
                }
            }

            var newIncoming = new List<PerformGraph.AssetGroups>();
            var aggMap = new Dictionary<string, HashSet<AssetReference>>();
            foreach (var ag in oriIncoming)
            {
                var newAssetGroups = new PerformGraph.AssetGroups(ag.connection, new Dictionary<string, List<AssetReference>>());
                foreach (var key in ag.assetGroups.Keys)
                {
                    var assets = ag.assetGroups[key];
                    foreach (var a in assets)
                    {
                        if (aggDictionary.ContainsKey(a.importFrom))
                        {
                            string newBundle = aggDictionary[a.importFrom];
                            if (!aggMap.ContainsKey(newBundle))
                                aggMap[newBundle] = new HashSet<AssetReference> {a};
                            else
                                aggMap[newBundle].Add(a);
                        }
                        else
                        {
                            if (!newAssetGroups.assetGroups.ContainsKey(key))
                                newAssetGroups.assetGroups[key] = new List<AssetReference> {a};
                            else
                                newAssetGroups.assetGroups[key].Add(a);
                        }
                    }
                }

                newIncoming.Add(newAssetGroups);
            }

            var aggList = new Dictionary<string, List<AssetReference>>();
            foreach (var pair in aggMap)
            {
                aggList.Add(pair.Key, pair.Value.ToList());
            }

            var aggAssetGroups = new PerformGraph.AssetGroups(newIncoming[0].connection, aggList);
            newIncoming.Add(aggAssetGroups);
            return newIncoming;
        }

        private void UGUIProcessor(AssetReference asset, Dictionary<string, string> sprite2SpriteAtlas)
        {
            if (TypeUtility.GetMainAssetFuzzyTypeAtPath(asset.importFrom) != typeof(UnityEngine.U2D.SpriteAtlas)) 
                return;
            
            var dependencies = AssetDatabase.GetDependencies(asset.importFrom);
            foreach (var d in dependencies)
            {
                if (TypeUtility.GetMainAssetFuzzyTypeAtPath(d) == typeof(UnityEngine.Texture))
                {
                    // assetPath2Bundle[d] = key;
                    if (sprite2SpriteAtlas.ContainsKey(d))
                    {
                        Debug.Log("Repack all atlas!, " + d + " in " + sprite2SpriteAtlas[d]);
                        Debug.Log("Repack all atlas!, " + d + " in " + asset.importFrom);
                    }
                    sprite2SpriteAtlas.Add(d, asset.importFrom);
                }
            }
        }

        protected virtual void DependencyCollectorActions(Dictionary<string, SortedSet<string>> dependencyCollector,
            Dictionary<string, List<AssetReference>> bundle2AssetList,
            Dictionary<string, string> assetPath2Bundle)
        {
            
        }

        protected virtual void CustomExtractAction(Dictionary<string, ZeusAssetBuild.ZeusBundleNode> bundleDict,
            Dictionary<string, ZeusAssetBuild.ZeusAssetNode> assetDict)
        {

        }

        protected virtual Dictionary<string, List<AssetReference>> FinalShareProcessor(BuildTarget target, 
            Dictionary<string, List<AssetReference>> sharedDependency, 
            Dictionary<string,int> sharedDependencyCount)
        {
            var finalSharedDependency = new Dictionary<string, List<AssetReference>>();
            
            StringBuilder nameBuilder = new StringBuilder();
            foreach (var pair in sharedDependency)
            {
                var assets = pair.Value;
                nameBuilder.Clear();
                foreach (var a in assets)
                {
                    nameBuilder.AppendLine(a.path);
                }

                string tempName = nameBuilder.ToString();
                uint nameHashCode = StringUtility.GetHashCode(tempName);
                var newName = m_bundleNameTemplate.Replace("*", nameHashCode.ToString("X4").ToLower());
                if (newName == m_bundleNameTemplate)
                {
                    newName = m_bundleNameTemplate + nameHashCode.ToString("X4").ToLower();
                }

                finalSharedDependency.Add(newName, assets);
                sharedDependencyCount[newName] = sharedDependencyCount[pair.Key];
            }

            if (m_groupExtractedAssets[target] != 0)
            {
                List<string> devidingBundleNames = new List<string>(finalSharedDependency.Keys);
                StringBuilder _builder = new StringBuilder();
                long szGroup = m_groupSizeByte[target] * 1024;
                foreach (var bundleName in devidingBundleNames)
                {
                    var assets = finalSharedDependency[bundleName];
                    long szGroupCount = 0;
                    foreach (var a in assets)
                    {
                        szGroupCount += GetSizeOfAsset(a, (GroupingType) m_groupingType[target]);
                    }

                    int depCount = sharedDependencyCount[bundleName];

                    if (szGroupCount * (depCount - 1) <= szGroup)
                    {
                        finalSharedDependency.Remove(bundleName);
                        _builder.AppendLine(bundleName);
                        foreach (var a in assets)
                        {
                            _builder.AppendLine("    " + a.importFrom);
                        }

                        _builder.AppendLine();
                    }
                }

                File.WriteAllText("ZeusReduantDependency.txt", _builder.ToString());
            }

            return finalSharedDependency;
        }

        private void DepShaderProcessor(Dictionary<string, ZeusAssetBuild.ZeusAssetNode> assetNodeDict,
            Dictionary<string, ZeusAssetBuild.ZeusBundleNode> bundleNodeDict)
        {
            if (m_generateShaderBundle)
            {
                if (string.IsNullOrEmpty(m_generateShaderBundleName))
                {
                    m_generateShaderBundleName = "shared_shader.ab";
                }
                ZeusAssetBuild.ZeusBundleNode shaderBundleNode = null;
                if (!bundleNodeDict.TryGetValue(m_generateShaderBundleName, out shaderBundleNode))
                {
                    shaderBundleNode = new ZeusAssetBuild.ZeusBundleNode(m_generateShaderBundleName, new List<ZeusAssetBuild.ZeusAssetNode>());
                }
                foreach (var pair in assetNodeDict)
                {
                    var assetNode = pair.Value;
                    if (!assetNode.Flag.HasFlag(ZeusAssetBuild.AssetFlag.ExplicitAsset))
                    {
                        if (assetNode.AssetPath.EndsWith(".shader"))
                        {
                            shaderBundleNode.AssetList.Add(assetNode);
                            assetNode.BundleName = shaderBundleNode.BundleName;
                            assetNode.Flag = ZeusAssetBuild.AssetFlag.ExplicitAsset;
                        }
                    }
                }
                if (shaderBundleNode.AssetList.Count > 0 && !bundleNodeDict.ContainsKey(shaderBundleNode.BundleName))
                {
                    bundleNodeDict.Add(shaderBundleNode.BundleName, shaderBundleNode);
                }
            }
        }

        protected virtual void DepShareProcessor(Dictionary<string, SortedSet<string>> dependencyCollector,
            Dictionary<string, List<AssetReference>> sharedDependency, 
            Dictionary<string,int> sharedDependencyCount,
            Dictionary<string, List<AssetReference>> bundle2AssetList)
        {
            StringBuilder outResBuilder = new StringBuilder();
            StringBuilder sharedBuilder = new StringBuilder();
            foreach (var entry in dependencyCollector)
            {
                var joinedName = string.Join("-", entry.Value.ToArray());
                if (!sharedDependency.ContainsKey(joinedName))
                {
                    sharedDependency[joinedName] = new List<AssetReference>();
                    sharedDependencyCount[joinedName] = entry.Value.Count;
                }

                sharedDependency[joinedName].Add(AssetReference.CreateReference(entry.Key));
                sharedBuilder.AppendLine(entry.Key);
                foreach (var bundle in entry.Value)
                {
                    sharedBuilder.AppendLine("    " + bundle);
                    foreach (var ass in bundle2AssetList[bundle])
                    {
                        sharedBuilder.AppendLine("        " + ass.importFrom);
                    }
                }

                sharedBuilder.AppendLine();
                

                if (!entry.Key.StartsWith("Assets/Game/") && !entry.Key.StartsWith("Assets/Raw/"))
                {
                    outResBuilder.AppendLine(entry.Key);
                }
            }

            File.WriteAllText("ZeusOutAssetList.txt", outResBuilder.ToString());
            File.WriteAllText("ZeusDepAssetList.txt", sharedBuilder.ToString());
        }

        protected virtual void ProcessSharedAsset(BuildTarget target, Dictionary<string, ZeusAssetNode> assetNodeDict,
            Dictionary<string, ZeusBundleNode> bundleNodeDict)
        {
            StringBuilder outResBuilder = new StringBuilder();
            StringBuilder sharedBuilder = new StringBuilder();
            Dictionary<string, List<ZeusAssetNode>> sharedNodeDict = new Dictionary<string, List<ZeusAssetNode>>();
            foreach (var pair in assetNodeDict)
            {
                var assetNode = pair.Value;
                if (assetNode.IsExplicitAsset())
                    continue;
                if (assetNode.IsExplicitSharedAsset())
                    continue;

                var joinedName = string.Join("-", assetNode.DepBundleSet);
                if (!sharedNodeDict.ContainsKey(joinedName))
                {
                    sharedNodeDict[joinedName] = new List<ZeusAssetNode>();
                }
                sharedNodeDict[joinedName].Add(assetNode);
                if (!assetNode.AssetPath.StartsWith("Assets/Game/") && !assetNode.AssetPath.StartsWith("Assets/Raw/"))
                {
                    outResBuilder.AppendLine(assetNode.AssetPath);
                }
            }
            ////////////////////////////////////////////
            var reduantBundleSet = new HashSet<string>();
            StringBuilder _builder = new StringBuilder();
            if (m_groupExtractedAssets[target] != 0)
            {
                long szGroup = m_groupSizeByte[target] * 1024;
                foreach (var pair in sharedNodeDict)
                {
                    var assetNodeList = pair.Value;
                    long szGroupMemoryCount = 0;
                    foreach (var assetNode in assetNodeList)
                    {
                        szGroupMemoryCount += GetSizeOfAsset(AssetReference.CreateReference(assetNode.AssetPath), (GroupingType)m_groupingType[target]);
                    }

                    if (szGroupMemoryCount * (assetNodeList.First().DepBundleSet.Count - 1) <= szGroup)
                    {
                        _builder.AppendLine(pair.Key + "  " + szGroupMemoryCount/1024.0f + "KB");
                        foreach (var assetNode in assetNodeList)
                        {
                            _builder.AppendLine("    " + assetNode.AssetPath);
                        }
                        _builder.AppendLine();
                        reduantBundleSet.Add(pair.Key);
                    }
                }
            }
            File.WriteAllText("ZeusReduantDependency.txt", _builder.ToString());

            StringBuilder nameBuilder = new StringBuilder();
            foreach (var pair  in sharedNodeDict)
            {
                if (reduantBundleSet.Contains(pair.Key))
                    continue;
                var assetNodeList = pair.Value;
                var assets = pair.Value;
                nameBuilder.Clear();
                sharedBuilder.AppendLine(string.Join(" | ", assetNodeList.First().DepBundleSet));
                foreach (var assetNode in assetNodeList)
                {
                    nameBuilder.AppendLine(assetNode.AssetPath);
                    sharedBuilder.AppendLine("    " + assetNode.AssetPath);
                }
                string tempName = nameBuilder.ToString();
                uint nameHashCode = StringUtility.GetHashCode(tempName);
                var newName = m_bundleNameTemplate.Replace("*", nameHashCode.ToString("X4").ToLower());
                if (newName == m_bundleNameTemplate)
                {
                    newName = m_bundleNameTemplate + nameHashCode.ToString("X4").ToLower();
                }
                var bundleNode = new ZeusBundleNode(newName, assetNodeList);
                bundleNode.BundleType = BundleType.ExtractSharedBundle;
                foreach (var assetNode in assetNodeList)
                {
                    assetNode.BundleName = newName;
                }
                bundleNodeDict.Add(bundleNode.BundleName, bundleNode);
            }

            File.WriteAllText("ZeusOutAssetList.txt", outResBuilder.ToString());
            File.WriteAllText("ZeusDepAssetList.txt", sharedBuilder.ToString());
        }

        protected virtual void OutputBundleDict(Dictionary<string, ZeusBundleNode> bundleNodeDict)
        {
            StringBuilder outResBuilder = new StringBuilder();
            foreach (var pair in bundleNodeDict)
            {
                var bundleNode = pair.Value;
                outResBuilder.AppendLine(bundleNode.BundleName);
                foreach(var assetNode in bundleNode.AssetList)
                {
                    outResBuilder.AppendLine("    " + assetNode.AssetPath);
                }
            }
            File.WriteAllText("ZeusBundleAssetList.txt", outResBuilder.ToString());
        }

        private void AddBundleToBuildMap(Model.NodeData node, Dictionary<string,List<AssetReference>> assetGroups)
        {
            foreach (var bundleName in assetGroups.Keys)
            {
                var bundleConfig = AssetBundleBuildMap.GetBuildMap().GetAssetBundleWithNameAndVariant(node.Id, bundleName, string.Empty);
                bundleConfig.AddAssets(node.Id, assetGroups[bundleName].Select(a => a.importFrom));
            }
        }

        private void AddBundleToBuildMap(Model.NodeData node, Dictionary<string, ZeusBundleNode> assetGroups)
        {
            foreach (var bundleName in assetGroups.Keys)
            {
                var bundleConfig = AssetBundleBuildMap.GetBuildMap().GetAssetBundleWithNameAndVariant(node.Id, bundleName, string.Empty);
                IEnumerable<string> assets = null;
                if (assetGroups[bundleName].BundleType == BundleType.ExplicitBundle)
                {
                    assets = from asset in assetGroups[bundleName].AssetList
                                 where asset.IsExplicitAsset()
                                 select asset.AssetPath;
                }
                else
                {
                    assets = from asset in assetGroups[bundleName].AssetList
                             select asset.AssetPath;
                }
                bundleConfig.AddAssets(node.Id, assets);
            }
        }

        private Dictionary<string, List<AssetReference>> TransformToOutDict(Dictionary<string, ZeusBundleNode> bundleDict)
        {
            Dictionary<string, List<AssetReference>> output = new Dictionary<string, List<AssetReference>>();
            foreach (var pair in bundleDict)
            {
                var bundleNode = pair.Value;
                if (!output.ContainsKey(bundleNode.BundleName))
                {
                    output[bundleNode.BundleName] = new List<AssetReference>();
                }
                foreach (var assetNode in bundleNode.AssetList)
                {
                    output[bundleNode.BundleName].Add(AssetReference.CreateReference(assetNode.AssetPath));
                }
            }
            return output;
        }

        private void ParseExplicitAssetAndBundle(IEnumerable<PerformGraph.AssetGroups> assetGroups, Dictionary<string, ZeusBundleNode> bundleNodeDict, Dictionary<string, ZeusAssetNode> assetNodeDict)
        {
            var sprite2SpriteAtlas = new Dictionary<string, string>();
            foreach (var ag in assetGroups)
            {
                foreach (var key in ag.assetGroups.Keys)
                {
                    var assets = ag.assetGroups[key];
                    var assetList = new List<ZeusAssetBuild.ZeusAssetNode>();
                    foreach (var a in assets)
                    {
                        var assetNode = new ZeusAssetBuild.ZeusAssetNode(a.importFrom, key);
                        assetList.Add(assetNode);
                        assetNode.Flag = ZeusAssetBuild.AssetFlag.ExplicitAsset;
                        assetNodeDict.Add(assetNode.AssetPath, assetNode);
                        UGUIProcessor(a, sprite2SpriteAtlas);
                    }
                    var bundleNode = new ZeusAssetBuild.ZeusBundleNode(key, assetList);
                    bundleNode.BundleType = ZeusAssetBuild.BundleType.ExplicitBundle;
                    bundleNodeDict.Add(key, bundleNode);
                }
            }
        }
    }
}
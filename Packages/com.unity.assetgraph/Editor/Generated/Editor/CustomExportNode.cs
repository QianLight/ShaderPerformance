using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine.AssetGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;
using V1 = AssetBundleGraph;

[CustomNode("Custom/Export To Directory", 1000)]
public class CustomExportNode : Exporter
{
    [SerializeField] private bool m_exportAllManifest = false;
    /**
	 * Build is called when Unity builds assets with AssetBundle Graph. 
	 */
    public override void Build(BuildTarget target,
        Model.NodeData nodeData,
        IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput,
        PerformGraph.Output outputFunc,
        Action<Model.NodeData, string, float> progressFunc)
    {
        string mainManifest = BuildTargetUtility.TargetToAssetBundlePlatformName(target) + ".manifest";
        List<PerformGraph.AssetGroups> outputList = new List<PerformGraph.AssetGroups>();
        foreach (var ag in incoming)
        {
            Dictionary<string, List<AssetReference>> assetGroup = new Dictionary<string, List<AssetReference>>();
            PerformGraph.AssetGroups group = new PerformGraph.AssetGroups(ag.connection, assetGroup);
            outputList.Add(group);
            foreach (var groupKey in ag.assetGroups.Keys)
            {
                var inputSources = ag.assetGroups[groupKey];
                List<AssetReference> groupAssetList = new List<AssetReference>();
                assetGroup.Add(groupKey, groupAssetList);
                foreach (var source in inputSources)
                {
                    if (!m_exportAllManifest)
                    {
                        var destinationSourcePath = source.importFrom;
                        //filter .manifest
                        if (destinationSourcePath.EndsWith(".manifest") && !destinationSourcePath.EndsWith(mainManifest))
                        {
                            continue;
                        }
                    }
                    groupAssetList.Add(source);
                }
            }
        }
        base.Build(target, nodeData, outputList, connectionsToOutput, outputFunc, progressFunc);
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) 
    {
        base.OnInspectorGUI(node, streamManager, editor, onValueChanged);
        m_exportAllManifest = EditorGUILayout.ToggleLeft("Export All Manifest", m_exportAllManifest);
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

using V1=AssetBundleGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph {

	[CustomNode("Load Assets/CustomLoad", 1000)]
	public class CustomeLoad : Node {

		public override string ActiveStyle {
			get {
				return "node 0 on";
			}
		}

		public override string InactiveStyle {
			get {
				return "node 0";
			}
		}
			
		public override string Category {
			get {
				return "Load";
			}
		}

		public override Model.NodeOutputSemantics NodeInputType {
			get {
				return Model.NodeOutputSemantics.None;
			}
		}

		public override void Initialize(Model.NodeData data) {

			data.AddDefaultOutputPoint();
		}

		public override Node Clone(Model.NodeData newData) {
			var newNode = new CustomeLoad();

			newData.AddDefaultOutputPoint();
			return newNode;
		}

		public override bool OnAssetsReimported(
			Model.NodeData nodeData,
			AssetReferenceStreamManager streamManager,
			BuildTarget target, 
            AssetPostprocessorContext ctx,
            bool isBuilding)
		{

            return false;
		}

		public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) {

		}


		public override void Prepare (BuildTarget target, 
			Model.NodeData node, 
			IEnumerable<PerformGraph.AssetGroups> incoming, 
			IEnumerable<Model.ConnectionData> connectionsToOutput, 
			PerformGraph.Output Output) 
		{
			Load(target, node, connectionsToOutput, Output);
		}
		
		void Load (BuildTarget target, 
			Model.NodeData node, 
			IEnumerable<Model.ConnectionData> connectionsToOutput, 
			PerformGraph.Output Output) 
		{

			if(connectionsToOutput == null || Output == null) {
				return;
			}

            var outputSource = new List<AssetReference>();

            List<AssetBundleBuild> list = CFEngine.Editor.BuildBundleConfig.instance.BuildBundle("", -1, CFEngine.Editor.BuildType.PreBuild, false);

            string excludeFile = Application.dataPath + "/Editor/Platform/AssetGraph/Exclude.txt";
            string[] exclude = System.IO.File.ReadAllLines(excludeFile);

            foreach (var bundle in list) {
                if (bundle.assetNames.Length > 1) continue;
                for (int k = 0; k < bundle.assetNames.Length; ++k)
                {
                    var name = bundle.assetNames[k];

                    bool excludeFlag = false;
                    for (int j = 0; j < exclude.Length; ++j)
                    {
                        if (name.ToLower().Contains(exclude[j].ToLower()))
                        {
                            excludeFlag = true;
                            break;
                        }
                    }
                    if (excludeFlag) continue;

                    var guid = AssetDatabase.AssetPathToGUID(name);
                    var targetFilePath = AssetDatabase.GUIDToAssetPath(guid);

                    if (!TypeUtility.IsLoadingAsset(targetFilePath))
                    {
                        continue;
                    }

                    var r = AssetReferenceDatabase.GetReference(targetFilePath);

                    if (r != null)
                    {
                        outputSource.Add(AssetReferenceDatabase.GetReference(targetFilePath));
                    }
                }
			}

			var output = new Dictionary<string, List<AssetReference>> {
				{"0", outputSource}
			};

			var dst = (connectionsToOutput == null || !connectionsToOutput.Any())? 
				null : connectionsToOutput.First();
			Output(dst, output);
		}

		public static void ValidateSearchCondition (string currentCondition, Action NullOrEmpty) {
			if (string.IsNullOrEmpty(currentCondition)) NullOrEmpty();
		}

		private string GetLoaderFullLoadPath(BuildTarget g) {
			return FileUtility.PathCombine(Application.dataPath);
		}
	}
}
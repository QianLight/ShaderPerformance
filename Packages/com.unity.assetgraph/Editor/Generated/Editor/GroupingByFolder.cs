
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

using V1=AssetBundleGraph;
using Model=UnityEngine.AssetGraph.DataModel.Version2;

namespace UnityEngine.AssetGraph
{
	[CustomNode("Group Assets/Group By Folder", 42)]
	public class GroupingByFolder : Node {

        [SerializeField] private SerializableMultiTargetString m_groupNameFormat;

        public override string ActiveStyle {
			get {
				return "node 2 on";
			}
		}

		public override string InactiveStyle {
			get {
				return "node 2";
			}
		}

		public override string Category {
			get {
				return "Group";
			}
		}

		public override void Initialize(Model.NodeData data) {
            m_groupNameFormat = new SerializableMultiTargetString();

			data.AddDefaultInputPoint();
			data.AddDefaultOutputPoint();
		}

		public override Node Clone(Model.NodeData newData) {
            var newNode = new GroupingByFolder();
            newNode.m_groupNameFormat = new SerializableMultiTargetString(m_groupNameFormat);

            newData.AddDefaultInputPoint();
			newData.AddDefaultOutputPoint();
			return newNode;
		}

		public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged) {

			EditorGUILayout.HelpBox("Group By File Path Hash: Create group per individual asset.", MessageType.Info);
			editor.UpdateNodeName(node);

            GUILayout.Space(4f);

            //Show target configuration tab
            editor.DrawPlatformSelector(node);
            using (new EditorGUILayout.VerticalScope (GUI.skin.box)) {
                var disabledScope = editor.DrawOverrideTargetToggle (node, m_groupNameFormat.ContainsValueOf (editor.CurrentEditingGroup), (bool enabled) => {
                    using (new RecordUndoScope ("Remove Target Grouping Settings", node, true)) {
                        if (enabled) {
                            m_groupNameFormat [editor.CurrentEditingGroup] = m_groupNameFormat.DefaultValue;
                        } else {
                            m_groupNameFormat.Remove (editor.CurrentEditingGroup);
                        }
                        onValueChanged ();
                    }
                });

                using (disabledScope) {
                    var newGroupNameFormat = EditorGUILayout.TextField ("Group Name Format", m_groupNameFormat [editor.CurrentEditingGroup]);
                    EditorGUILayout.HelpBox (
                        "You can customize group name. You can use variable {OldGroup} for old group name and {NewGroup} for current matching name.You can also use {FileName} and {FileExtension} {FilePathHash}.", 
                        MessageType.Info);

                    if (newGroupNameFormat != m_groupNameFormat [editor.CurrentEditingGroup]) {
                        using (new RecordUndoScope ("Change Group Name", node, true)) {
                            m_groupNameFormat [editor.CurrentEditingGroup] = newGroupNameFormat;
                            onValueChanged ();
                        }
                    }
                }
            }
		}

		public override void Prepare (BuildTarget target, 
			Model.NodeData node, 
			IEnumerable<PerformGraph.AssetGroups> incoming, 
			IEnumerable<Model.ConnectionData> connectionsToOutput, 
			PerformGraph.Output Output) 
		{
            if(connectionsToOutput == null || Output == null) {
                return;
            }

            var outputDict = new Dictionary<string, List<AssetReference>>();

            if(incoming != null) {
                int i = 0;
                foreach(var ag in incoming) {
                    foreach (var g in ag.assetGroups.Keys) {
                        var assets = ag.assetGroups [g];
                        foreach(var a in assets) {
                            var key = i.ToString ();
                            var folder = System.IO.Path.GetDirectoryName(a.path);
                            folder = folder.Replace("\\", "/");
                            
                            var lastFolder = folder.Substring(folder.LastIndexOf('/')+1);
                            lastFolder = lastFolder.Replace(" ", "");
                            if(lastFolder.Length > 8)
                            {
                                lastFolder = lastFolder.Substring(lastFolder.Length - 8);
                            }
                            lastFolder = lastFolder + StringUtility.GetHashCode(folder).ToString("X4").ToLower();
                            key = lastFolder;
                            
                            List<AssetReference> assetRefList = null;
                            if(!outputDict.TryGetValue(key, out assetRefList)){
                                assetRefList = new List<AssetReference>();
                                outputDict.Add(key, assetRefList);
                            }
                            assetRefList.Add (a);
                            ++i;
                        }
                    }
                }
            }

            var dst = (connectionsToOutput == null || !connectionsToOutput.Any())? 
                null : connectionsToOutput.First();
            Output(dst, outputDict);
        }
	}
}
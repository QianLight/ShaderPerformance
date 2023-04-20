
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
	[CustomNode("Group Assets/Group By Custom File Path Hash", 42)]
	public class GroupingByCustomFilePathHash : Node {

        [SerializeField] private SerializableMultiTargetString m_groupNameFormat;
        [SerializeField] private string m_abandonedSuffix;
        [SerializeField] private string m_splitKey;
        private List<string> m_abandonedSuffixList = new List<string>();
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
            m_abandonedSuffix = null;
            m_splitKey = null;
			data.AddDefaultInputPoint();
			data.AddDefaultOutputPoint();
		}

		public override Node Clone(Model.NodeData newData) {
            var newNode = new GroupingByCustomFilePathHash();
            newNode.m_groupNameFormat = new SerializableMultiTargetString(m_groupNameFormat);
            newNode.m_abandonedSuffix = m_abandonedSuffix;
            newNode.m_splitKey = m_splitKey;
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

                var newAbandonedSuffix = EditorGUILayout.TextField ("ignore Path Suffix", m_abandonedSuffix);
                if(newAbandonedSuffix != m_abandonedSuffix){
                    m_abandonedSuffix = newAbandonedSuffix;
                    onValueChanged ();
                }

                var newSplitKey = EditorGUILayout.TextField ("split key", m_splitKey);
                if(newSplitKey != m_splitKey){
                    m_splitKey = newSplitKey;
                    onValueChanged ();
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
            //process abandoned suffix
            m_abandonedSuffixList.Clear();
            if (!string.IsNullOrEmpty(m_abandonedSuffix))
            {
                string[] suffixs = m_abandonedSuffix.Split(';');
                foreach(var suffix in suffixs)
                {
                    if(!string.IsNullOrEmpty(suffix) && !string.IsNullOrWhiteSpace(suffix))
                    {
                        m_abandonedSuffixList.Add(suffix);
                    }
                }
            }
            if(incoming != null) {
                int i = 0;
                foreach(var ag in incoming) {
                    foreach (var g in ag.assetGroups.Keys) {
                        var assets = ag.assetGroups [g];
                        foreach(var a in assets) {  
                            var path = a.path;
                            if(System.IO.Path.HasExtension(path))
                            {
                                var extension = System.IO.Path.GetExtension(path);
                                path = path.Substring(0, path.Length - extension.Length);
                            }
                            if(m_abandonedSuffixList.Count > 0)
                            {
                                foreach(var suffix in m_abandonedSuffixList)
                                {
                                    if (path.EndsWith(suffix))
                                    {
                                        path = path.Substring(0, path.Length - suffix.Length);
                                        break;
                                    }
                                }
                            }
                            if(!string.IsNullOrEmpty(m_splitKey))
                            {
                                int index = path.IndexOf(m_splitKey);
                                if(index > 0)
                                {
                                    path = path.Substring(0, index);
                                }
                            }
                            var key = StringUtility.GetHashCode(path).ToString("X4").ToLower();
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
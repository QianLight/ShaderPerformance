using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    [Serializable]
    public class NodeConfigData
    {
        [SerializeField]
        public int NodeID;
        [SerializeField]
        public Vector2 Position;
        [SerializeField]
        public string TitleName;
        [SerializeField]
        public string BackgroundText;
        [SerializeField]
        public string CustomNote;

        [SerializeField]
        public bool Expand = true;

        [SerializeField]
        public bool ShowNote = true;

        [SerializeField]
        public string CustomData;
        [SerializeField]
        public string CustomData1;
        public string Tag;

        public NodeConfigData Copy ()
        {
            NodeConfigData d = new NodeConfigData ();
            d.NodeID = NodeID;
            d.Position = Position;
            d.TitleName = TitleName;
            d.BackgroundText = BackgroundText;
            d.Tag = Tag;
            d.CustomNote = CustomNote;
            d.CustomData = CustomData;
            d.CustomData1 = CustomData1;
            return d;
        }
    }

    [Serializable]
    public class AreaCommentData
    {
        [SerializeField]
        public List<int> NodeList = new List<int> ();

        [SerializeField]
        public string Text;

        [SerializeField]
        public Vector4 Offset;

    }
    [Serializable]
    public class ResRedirectData
    {
        public string nameWithExt;
        public string physicPath;
    }
    
    [Serializable]
    public class GraphConfigData
    {
        [SerializeField]
        public int graphID;

        [SerializeField]
        public string tag = "";

        [SerializeField]
        public List<NodeConfigData> NodeConfigList = new List<NodeConfigData> ();

        [SerializeField]
        public List<AreaCommentData> AreaCommentData = new List<AreaCommentData> ();

        [SerializeField]
        public List<ResRedirectData> resMap = new List<ResRedirectData>();

        public NodeConfigData GetConfigDataByID (int nodeID)
        {
            for (int i = 0; i < NodeConfigList.Count; ++i)
            {
                if (NodeConfigList[i].NodeID == nodeID)
                    return NodeConfigList[i];
            }

            return null;
        }

        public void ClearData ()
        {
            NodeConfigList.Clear ();
            AreaCommentData.Clear ();
            resMap.Clear();
        }

        public void Copy(GraphConfigData data)
        {
            tag = data.tag;
        }
    }

    [Serializable]
    public class EditorConfigData
    {
        [SerializeField]
        public string SceneName = "";
        [SerializeField]
        public List<GraphConfigData> GraphEditorConfigList = new List<GraphConfigData> ();

        public GraphConfigData GetGraphConfigByID (int graphID)
        {
            for (int i = 0; i < GraphEditorConfigList.Count; ++i)
            {
                if (GraphEditorConfigList[i].graphID == graphID)
                    return GraphEditorConfigList[i];
            }

            return null;
        }
    }

}
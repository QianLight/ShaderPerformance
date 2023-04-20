using System;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CFEngine.Editor
{

    public enum EditMode
    {
        ShaderParam,
        EffectTemplate
    }

    public class MatEffectView : CanvasView
    {
        private MatEffectBoard board;
        private MatEffectGraph meGraph;

        public MatEffectView (EditorWindow window, string back) : base (window, back)
        {
            elementResized = OnElementResized;
        }

        void OnElementResized (VisualElement element)
        {
            if (element is IGraphResizable)
            {
                (element as IGraphResizable).OnResized ();
            }
        }

        protected override SerializedProperty GetNodeProperty (AbstractNode node)
        {
            //if (meGraph.nodeList.TryGetValue (node.GetType ().Name, out var np))
            //{
            //    return np.sp;
            //}

            return meGraph.nodesSp;
        }
        //private void AddNode (Type t,
        //    Dictionary<AbstractNode, NodeView> nodeMap)
        //{
        //    if (meGraph.nodeList.TryGetValue (t.Name, out var np))
        //    {
        //        for (int i = 0; i < np.nodes.Count; i++)
        //        {
        //            var node = np.nodes[i] as AbstractNode;
        //            var element = AddNodeView (node, np.sp, i);
        //            nodeMap.Add (node, element);
        //        }
        //    }
        //}

        private void RemoveNode (Dictionary<AbstractNode, NodeView> nodeMap)
        {
            foreach (var v in nodeMap.Values)
            {
                RemoveElement (v);
            }
            nodeMap.Clear ();
        }

        protected override void AddCustomNode (Graph graph)
        {
            meGraph = graph as MatEffectGraph;

            if (board == null)
            {
                board = new MatEffectBoard (this, graph as MatEffectGraph);
            }
            Add(board);
            //for (int i = 0; i < meGraph.nodes.Count; i++)
            //{
            //    var node = meGraph.nodes[i];
            //    var element = AddNodeView(node, meGraph.nodesSp, i);
            //    matEffectMap.Add(node, element);
            //}
            //SyncEdge(matEffectMap);
        }

        protected override void AddCustomModule ()
        {
            m_Search.AddModule ("MatEffect");
        }

        protected override int OnAddNode (AbstractNode node)
        {
            if (meGraph.nodeList.TryGetValue (node.GetType ().Name, out var np))
            {
                //if(np.nodes!=null)
                //{
                //    np.nodes.Add(node);
                //}
                return meGraph.nodes.Count - 1;
            }
            return -1;

        }

        protected override void OnPostAddNode (AbstractNode node, NodeView nodeView)
        {
            //if (meGraph.nodeList.TryGetValue(node.GetType().Name, out var np))
            //{
            //    if (np.nodes != null)
            //    {
            //        np.nodes.Add(node);
            //    }
            //}
        }

        protected override void OnRemoveNode (AbstractNode node)
        {
            //if (meGraph.nodeList.TryGetValue(node.GetType().Name, out var np))
            //{
            //    if (np.nodes != null)
            //    {
            //        np.nodes.Remove(node);
            //    }
            //}
        }
    }
}
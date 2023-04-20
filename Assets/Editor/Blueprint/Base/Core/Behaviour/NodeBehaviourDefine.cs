using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BluePrint
{
    //public enum NodeState
    //{
    //    Error = 0x0000001,
    //    MouseOver = 0x0000002,
    //    Selected = 0x0000004,
    //    Executed = 0x0000008,
    //}
    public interface INodeBehaviourDefine
    {
        void OnMouseLeftDown(BluePrintNode node);

        void ShowTipFrame(BluePrintNode node, Rect boxRect);
    }

    public class DefaultBehaviour : INodeBehaviourDefine
    {
        BluePrintNode LastClickNode;
        double LastClickTime = 0;
        public virtual void OnMouseLeftDown(BluePrintNode node)
        {
            double clickTime = EditorApplication.timeSinceStartup;
            if (!node.IsSelected)
            {
                node.Root.ClearMultiselect();

                node.Root.UnSelectCurrentNode();

                node.IsSelected = true;
                node.Root.selectNode = node;
                node.OnSelected();
            }

            if((clickTime - LastClickTime < 0.2) && (LastClickNode == node))
            {
                LastClickTime = 0;
                LastClickNode = null;
                OnMouseDoubleClick(node);
            }
            else
            {
                LastClickTime = clickTime;
                LastClickNode = node;
            }
        }

        public virtual void OnMouseDoubleClick(BluePrintNode node)
        {
            //Debug.Log("DoubleClick");
            BluePrintSubGraphNode subGraphNode = node as BluePrintSubGraphNode;
            if(subGraphNode == null) return;
            
            if(string.IsNullOrEmpty(subGraphNode.bindingGraph.GraphName))
            {
                node.Root.ShowNotification(new GUIContent("请先设置子图名称"));
                return;
            }
            if (subGraphNode != null)
            {
                node.Root.editorWindow.OpenGraph(subGraphNode.bindingGraph);
            }
        }

        public virtual void ShowTipFrame(BluePrintNode node, Rect boxRect)
        {
            if (node.hasError)
            {
                DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighter6, 20);
            }
            else
            {
                if (node.IsMouseOver)
                {
                    DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighter3, 20);
                }
            }

            if (node.IsSelected)
            {
                DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighter2, 20);
            }
        }
    }

    public class SimulationBehaviour : DefaultBehaviour
    {
        public override void ShowTipFrame(BluePrintNode node, Rect boxRect)
        {
            if (node.IsExecuted())
            {
                DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighterExecute, 20);
            }
            if (node.IsSelected)
            {
                DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighter2, 20);
            }
        }
    }

    public class CommentEditBehaviour : INodeBehaviourDefine
    {
        BlueprintAreaComment source;
        public void SetCommentSource(BlueprintAreaComment comment)
        {
            source = comment;
        }
        public void OnMouseLeftDown(BluePrintNode node)
        {

        }

        public void ShowTipFrame(BluePrintNode node, Rect boxRect)
        {
            if(source != null)
            {
                if(source.ContainWidget(node))
                {
                    DrawTool.DrawStretchBox(boxRect, BlueprintStyles.BoxHighlighter6, 20);
                }
            }
        }
    }

    public class NodeBehaviourFactory
    {
        public static DefaultBehaviour defaultBehaviour = new DefaultBehaviour();
        public static SimulationBehaviour simulationBehaviour = new SimulationBehaviour();
        public static CommentEditBehaviour commentBehaviour = new CommentEditBehaviour();
    }
}

using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XLevel;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace LevelEditor
{
    public enum LevelOperationType
    {
        None=0,
        AddNode,
        DeleteNode,
        PinConnect,
        PinBreak,
        NodeData
    }

    struct LevelChangeInfo
    {
        public LevelOperationType opType;
        public int graphID;
        public PinChangeInfo[] pinArray;
        public BluePrintNode node;
    }

    struct PinChangeInfo
    {
        public int startNodeID;
        public int startPinID;
        public int endNodeID;
        public int endPinID;
    }

    class LevelOperationStack
    {
        public static LevelOperationStack Instance { get; } = new LevelOperationStack();

        private Stack<LevelChangeInfo> stack = new Stack<LevelChangeInfo>();

        public void ClearOpStack()
        {
            stack.Clear();
        }


        /// <summary>
        /// 增删连线时调用 包括快速连线 输入列表中一定是outputpin和inputpin成对 [0]->[1] [2]->[3]以此类推
        /// </summary>
        /// <param name="opType"></param>
        /// <param name="nodeList"></param>
        public void PushOperation(LevelOperationType opType,List<BluePrintPin> pinList,int graphID)
        {
            //至少有两个pin成对 否则为不处理
            if (pinList.Count < 2)
                return;
            LevelChangeInfo info = default;
            info.opType = opType;
            info.graphID = graphID;
            info.pinArray = new PinChangeInfo[pinList.Count / 2];
            for (var i = 0; i < pinList.Count; i += 2) 
            {
                BluePrintNode startNode = pinList[i].GetNode<BluePrintNode>();
                BluePrintNode endNode = pinList[i + 1].GetNode<BluePrintNode>();
                var startData = startNode.GetType().GetField("HostData").GetValue(startNode);
                var endData = endNode.GetType().GetField("HostData").GetValue(endNode);
                PinChangeInfo pinInfo = new PinChangeInfo()
                {
                    startNodeID = (int)startData.GetType().GetField("NodeID").GetValue(startData),
                    startPinID = pinList[i].pinID,
                    endNodeID = (int)endData.GetType().GetField("NodeID").GetValue(endData),
                    endPinID = pinList[i + 1].pinID
                };
                info.pinArray[i / 2] = pinInfo;
            }
            stack.Push(info);
        }

        /// <summary>
        /// //增删节点时调用 当节点被删除的时候其随带的连线也被删除 因此需要记下所有连线的信息
        /// </summary>
        /// <param name="opType"></param>
        /// <param name="node"></param>
        public void PushOperation(LevelOperationType opType, BluePrintNode node, int graphID)
        {
            LevelChangeInfo info = default;
            info.opType = opType;
            info.node = node;
            info.graphID = graphID;
            if(opType==LevelOperationType.DeleteNode)
            {
                int linkCount = 0, linkIndex = 0;
                foreach(var pin in node.pinList)
                {
                    linkCount += pin.connections.Count + pin.reverseConnections.Count;
                }
                info.pinArray = new PinChangeInfo[linkCount];
                var curNodeData = node.GetType().GetField("HostData").GetValue(node);
                var curNodeID = curNodeData.GetType().GetField("NodeID").GetValue(curNodeData);
                foreach (var pin in node.pinList)
                {                   
                    foreach(var c in pin.connections)
                    {
                        var endNode = c.connectEnd.GetNode<BluePrintNode>();
                        var endNodeData = endNode.GetType().GetField("HostData").GetValue(endNode);
                        var endNodeID = endNodeData.GetType().GetField("NodeID").GetValue(endNodeData);
                        PinChangeInfo pinInfo = new PinChangeInfo()
                        {
                            startNodeID = (int)curNodeID,
                            startPinID = pin.pinID,
                            endNodeID = (int)endNodeID,
                            endPinID = c.connectEnd.pinID
                        };
                        info.pinArray[linkIndex] = pinInfo;
                        linkIndex++;
                    }
                    foreach(var c in pin.reverseConnections)
                    {
                        var startNode = c.reverseConnectEnd.GetNode<BluePrintNode>();
                        var startNodeData = startNode.GetType().GetField("HostData").GetValue(startNode);
                        var startNodeID = startNodeData.GetType().GetField("NodeID").GetValue(startNodeData);
                        PinChangeInfo pinInfo = new PinChangeInfo()
                        {
                            startNodeID = (int)startNodeID,
                            startPinID = c.reverseConnectEnd.pinID,
                            endNodeID = (int)curNodeID,
                            endPinID = pin.pinID
                        };
                        info.pinArray[linkIndex] = pinInfo;
                        linkIndex++;
                    }
                }
            }
            stack.Push(info);
        }

        public void PopOperation()
        {
            if (stack.Count <= 0)
                return;
            LevelChangeInfo info = stack.Pop();
            switch(info.opType)
            {
                case LevelOperationType.AddNode:
                    PopAddNodeOp(info);
                    break;
                case LevelOperationType.DeleteNode:
                    PopDeleteNodeOp(info);
                    break;
                case LevelOperationType.PinConnect:
                    PopPinConnectOp(info);
                    break;
                case LevelOperationType.PinBreak:
                    PopPinBreakOp(info);
                    break;
            }
        }


        /// 回退增加节点的操作
        private void PopAddNodeOp(LevelChangeInfo info)
        {
            LevelGraph graph = LevelEditor.Instance.GetGraph(info.graphID) as LevelGraph;
            var nodeData = info.node.GetType().GetField("HostData").GetValue(info.node);
            var nodeID = nodeData.GetType().GetField("NodeID").GetValue(nodeData);
            FocusOnNode(info.graphID, (int)nodeID,info.node.nodeEditorData.NodeID);
            graph.widgetList.Remove(info.node);//不调用删除的接口 避免回退死循环  
            if(info.node is BluePrintSubGraphNode)
            {
                var g = LevelEditor.Instance.GetGraph((info.node as BluePrintSubGraphNode).HostData.GraphID);
                LevelEditor.Instance.Graphs.Remove(g);
            }
        }

        //回退删除节点的操作
        private void PopDeleteNodeOp(LevelChangeInfo info)
        {
            LevelGraph graph = LevelEditor.Instance.GetGraph(info.graphID) as LevelGraph;
            graph.widgetList.Add(info.node);//不调用增加节点的接口 避免nodeid++以及onadded把回退操作存储为增加操作 导致回退的死循环 
            var nodeData = info.node.GetType().GetField("HostData").GetValue(info.node);
            var nodeID = nodeData.GetType().GetField("NodeID").GetValue(nodeData);
            FocusOnNode(info.graphID, (int)nodeID, info.node.nodeEditorData.NodeID);
            info.node.IsSelected = false;
            graph.selectNode = null;
            if (info.node is BluePrintSubGraphNode)
            {
                LevelGraph g = LevelEditor.Instance.NewGraph<LevelGraph>(
                    (info.node as BluePrintSubGraphNode).HostData.GraphID, (info.node as BluePrintSubGraphNode).HostData.GraphName, false);
                g.graphData = LevelEditor.Instance.fullData.GraphDataList.Find(gData => gData.graphID == (info.node as BluePrintSubGraphNode).HostData.GraphID);
                g.graphConfigData = LevelEditor.Instance.editorConfigData.GetGraphConfigByID((info.node as BluePrintSubGraphNode).HostData.GraphID);
                LevelEditor.Instance.Graphs.Add(g);
            }
            if (info.pinArray == null)
                return;
            for (var i = 0; i < info.pinArray.Length; i++)
            {
                RevertConnection(info.pinArray[i], info.graphID, false, false);
            }
        }

        //回退连线操作
        private void PopPinConnectOp(LevelChangeInfo info)
        {
            for (var i = 0; i < info.pinArray.Length; i++) 
            {
                RevertConnection(info.pinArray[i], info.graphID, true, i == info.pinArray.Length - 1);
            }
        }

        //回退断线操作
        private void PopPinBreakOp(LevelChangeInfo info)
        {
            for(var i=0;i<info.pinArray.Length;i++)
            {
                RevertConnection(info.pinArray[i], info.graphID, false, i == info.pinArray.Length - 1);
            }
        }

        private void RevertConnection(PinChangeInfo info, int graphID, bool delete, bool focus)
        {
            LevelGraph graph = LevelEditor.Instance.GetGraph(graphID) as LevelGraph;
            BluePrintNode startNode = graph.GetNode(info.startNodeID);
            BluePrintNode endNode = graph.GetNode(info.endNodeID);
            BluePrintPin startPin = startNode.pinList.Find(p => p.pinID == info.startPinID);
            BluePrintPin endPin = endNode.pinList.Find(p => p.pinID == info.endPinID);
            if(!delete)
                startNode.ConnectPin(startPin, endPin);
            else
            {
                startPin.connections.RemoveAll(c => c.connectStart == startPin && c.connectEnd == endPin);
                endPin.reverseConnections.RemoveAll(c => c.reverseConnectEnd == startPin && c.reverseConnectStart == endPin);
            }
            if (focus)
                FocusOnNode(graphID, info.endNodeID);
        }

        private void FocusOnNode(int graphID, int nodeID, int editorID = 0)
        {
            LevelGraph levelGraph = LevelEditor.Instance.GetGraph(graphID) as LevelGraph;
            if (LevelEditor.Instance.CurrentGraph.GraphID != graphID)
                LevelEditor.Instance.CurrentGraph = levelGraph;
            if (!LevelEditor.Instance.OpenGrahps.Contains(levelGraph))
                LevelEditor.Instance.OpenGrahps.Add(levelGraph);
            BluePrintNode node = levelGraph.GetNode(nodeID);
            if (node == null)
                node = levelGraph.GetNode(editorID);
            if (node == null)
                return;
            levelGraph.FocusNode(node);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.GenericMenu;

public class CommonNodeGraph <TNode> where TNode:CommonNode, new ()
{
    public List<CommonConnection> m_connections = new List<CommonConnection>();
    public List<TNode> m_nodes = new List<TNode>();
    private List<List<TNode>> m_sortedNodes = new List<List<TNode>>();
    private Vector3 m_startPosition;
    private float m_scale = 1;
    private TNode m_currentSelectedNode;
    private CommonConnection m_currentSelectedConnection;
    private TNode m_currentDraggingNode;
    private int m_drawLimit = 100;
    private List<TNode> m_visibleNodes = new List<TNode>();
    private HashSet<CommonConnection> m_visibleConnections = new HashSet<CommonConnection>();
    private Func<string, Color> m_colorFunc;
    private Dictionary<int, List<TNode>> m_tmpGroupedNodesByDepth = new Dictionary<int, List<TNode>>();
    private GUISkin m_guiSkin;
    private Texture2D m_texDot;
    private GenericMenu m_commonContextMenu;
    private GenericMenu m_connectionMenu;
    private GenericMenu m_nodeMenu;
    private List<ContextMenuItem> m_menuItems = new List<ContextMenuItem>();
    private TNode m_newConnectionStartNode;
    private Vector2? m_newConnectionStartPos;
    private Vector2 m_newConnectionEndPos;

    private Stack<ICommand> m_commandStask = new Stack<ICommand>();
    private Stack<ICommand> m_canceledCommandStack = new Stack<ICommand>();
    private Vector2 m_originMousePosition;

    public void SetColorFunc(Func<string, Color> colorFunc)
    {
        m_colorFunc = colorFunc;
    }

    public virtual void Init(bool withWindow = false)
    {
        CommonNodeManager<TNode>.Instance.Reset();
        for (var i = 0; i < m_nodes.Count; i++)
        {
            CommonNodeManager<TNode>.Instance.Add(m_nodes[i]);
            var col = m_colorFunc?.Invoke(m_nodes[i].Name);
            m_nodes[i].m_color = col.HasValue ? col.Value : Color.gray;
        }
        CommonConnectionManager<TNode>.Instance.Reset();
        for (var i = 0; i < m_connections.Count; i++)
        {
            var connection = m_connections[i];
            CommonConnectionManager<TNode>.Instance.Add(connection);
            connection.InitNodes(CommonNodeManager<TNode>.Instance.GetNode(connection.m_from), CommonNodeManager<TNode>.Instance.GetNode(connection.m_to));
        }
        RefreshVisibleInfosByBaseNode(m_nodes[0]);
        if (withWindow)
        {
            //SortNodes();
            //GroupAndInitNodeRects();
            InitSkins();
            CreateMenus();
        }
    }

    internal void DeleteNodeInGraph(TNode node)
    {
        m_nodes.Remove(node);
        m_visibleNodes.Remove(node);
        node.IsVisible = false;
    }

    internal void RemoveConnection(CommonConnection connection)
    {
        m_connections.Remove(connection);
        m_visibleConnections.Remove(connection);
        if (m_currentSelectedConnection == connection)
        {
            m_currentSelectedConnection = null;
        }
    }

    internal void AddInitedConnection(CommonConnection connection)
    {
        m_connections.Add(connection);
        m_visibleConnections.Add(connection);
    }

    internal void AddNode(TNode node)
    {
        m_visibleNodes.Add(node);
        m_nodes.Add(node);
        node.IsVisible = true;
    }

    internal void AddNewNode(TNode node)
    {
        m_visibleNodes.Add(node);
        m_nodes.Add(node);
        node.m_rect = new Rect(m_originMousePosition.x, m_originMousePosition.y, m_nodeWidth, m_nodeHeight);
        node.IsVisible = true;
        m_currentSelectedNode = node;
    }

    private void InitSkins()
    {
        var connectionLabelTexName = "Animation.Record@2x";
        var tex = EditorGUIUtility.FindTexture(connectionLabelTexName);
        m_connectionLabelGUIContent = EditorGUIUtility.IconContent("Animation.Record@2x");
        m_guiSkin = AssetDatabase.LoadAssetAtPath<GUISkin>("Packages/com.unity.assetgraph/Editor/GUI/GraphicResources/CommonNodeStyle.guiskin");
        m_texDot = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.unity.assetgraph/Editor/GUI/GraphicResources/ConnectionPoint.png");
    }

    private void SetSelected(object selectedObj)
    {
        m_currentSelectedConnection = null;
        m_currentSelectedNode = null;
        var node = selectedObj as TNode;
        if (null != node)
        {
            m_currentSelectedNode = node;
            return;
        }
        var connection = selectedObj as CommonConnection;
        if (null != connection)
        {
            m_currentSelectedConnection = connection;
            return;
        }
    }

    private void SortNodes()
    {
        m_sortedNodes.Clear();
        HashSet<TNode> m_starters = new HashSet<TNode>();
        for (var i = 0; i < m_nodes.Count; i++)
        {
            if (m_nodes[i].Inputs.Count < 1)
            {
                m_starters.Add(m_nodes[i]);
            }
        }
        foreach (var node in m_starters)
        {
            var list = new List<TNode>() { node };
            m_sortedNodes.Add(list);
            GenerateSortedNodes(node, list, m_sortedNodes);
        }
        m_sortedNodes.Sort((x, y) =>
        {
            if (x.Count == y.Count)
            {
                return 0;
            }
            if (x.Count > y.Count)
            {
                return -1;
            }
            return 1;
        });
        var nodes = new HashSet<TNode>();
        for (var i = 0; i < m_sortedNodes.Count; i++)
        {
            var nodeList = m_sortedNodes[i];
            for (var j = nodeList.Count - 1; j >= 0; j--)
            {
                var node = nodeList[j];
                if (nodes.Contains(node))
                {
                    nodeList[j] = null;
                }
                nodes.Add(node);
            }
        }
    }

    private void GenerateSortedNodes(TNode node, List<TNode> currentList, List<List<TNode>> sortedNodes)
    {
        if (node.Outputs.Count > 0)
        {
            var connection = node.Outputs[0];
            var toNode = CommonNodeManager<TNode>.Instance.GetNode(connection.m_to);
            currentList.Add(toNode);
            GenerateSortedNodes(toNode, currentList, sortedNodes);
        }
        for (var i = 1; i < node.Outputs.Count; i++)
        {
            var list = new List<TNode>(currentList);
            m_sortedNodes.Add(list);
            var toNode = CommonNodeManager<TNode>.Instance.GetNode(node.Outputs[i].m_to);
            list.Add(toNode);
            GenerateSortedNodes(toNode, list, sortedNodes);
        }
    }

    private void DrawSelectedInfo()
    {
        if (null != m_currentSelectedNode)
        {
            //GUILayout.Label(m_currentSelectedNode.Name);
            m_currentSelectedNode.OnGUI();
        }
    }

    private void RefreshVisibleInfosByBaseNode(TNode baseNode)
    {
        ResetOffsetAndScale();
        for(var i = 0; i < m_nodes.Count; i++)
        {
            m_nodes[i].IsVisible = false;
        }
        baseNode.m_rect = new Rect(0, 0, m_nodeWidth, m_nodeHeight);
        baseNode.IsVisible = true;
        baseNode.tmpDepth = 0;
        var visibleCount = 0;
        var depth = 0;
        SetInputNodesVisible(baseNode, ref visibleCount, depth);
        SetOutputNodesVisible(baseNode, ref visibleCount, depth);
        m_visibleNodes.Clear();
        m_visibleConnections.Clear();
        m_tmpGroupedNodesByDepth.Clear();
        for (var i = 0; i < m_nodes.Count; i++)
        {
            if (!m_nodes[i].IsVisible)
            {
                continue;
            }
            m_visibleNodes.Add(m_nodes[i]);
            foreach (var input in m_nodes[i].Inputs)
            {
                m_visibleConnections.Add(input);
            }
            foreach (var output in m_nodes[i].Outputs)
            {
                m_visibleConnections.Add(output);
            }
            if (!m_tmpGroupedNodesByDepth.TryGetValue(m_nodes[i].tmpDepth, out var list))
            {
                list = m_tmpGroupedNodesByDepth[m_nodes[i].tmpDepth] = new List<TNode>();
            }
            list.Add(m_nodes[i]);
        }

        foreach(var pair in m_tmpGroupedNodesByDepth)
        {
            for(var i = 0; i < pair.Value.Count; i++)
            {
                var nodeDepth = pair.Key;
                RefreshNodeRect(baseNode.m_rect, pair.Value, nodeDepth);
            }
        }
    }

    private void SetVisibile(TNode node, bool visible)
    {
        node.IsVisible = visible;
        if (visible && !m_visibleNodes.Contains(node))
        {
            m_visibleNodes.Add(node);
        }
        if(!visible)
        {
            m_visibleNodes.Remove(node);
        }
    }


    public void ResetOffsetAndScale()
    {
        m_startPosition = Vector3.zero;
        m_scale = 1;
    }

    public void Filter(string key)
    {
        for (var i = 0; i < m_nodes.Count; i++)
        {
            if (m_nodes[i].Name.Contains(key))
            {
                m_currentSelectedNode = m_nodes[i];
                RefreshVisibleInfosByBaseNode(m_currentSelectedNode);
                break;
            }
        }
    }

    public void Draw(EditorWindow window)
    {
        DrawSelectedInfo();
        var groupRect = new Rect(0, 100, window.maxSize.x, window.maxSize.y - 100);
        GUI.BeginGroup(groupRect, m_guiSkin.GetStyle("Box"));
        DrawItems(window);
        //Handles.Label(Vector3.zero, "Handles.Label");
        //GUI.Label(new Rect(0, 0, 100, 100), "Label");
        //EditorGUI.DrawRect(new Rect(0, 0, 200, 200), Color.blue);
        //GUI.Box(new Rect(0, 0, 200, 200), "box");
        //Handles.DrawBezier(Vector3.zero, new Vector3(1024, 1024, 0), new Vector3(256, 512, 0), new Vector3(512, 256, 0),  Color.blue, null, 1);
        GUI.EndGroup();
        ProcessEvents(window, groupRect);
    }

    private void ProcessEvents(EditorWindow window, Rect eventRect)
    {
        var evt = Event.current;
        if (!eventRect.Contains(evt.mousePosition))
        {
            return;
        }
        var mousePosition = new Vector2(evt.mousePosition.x - eventRect.x, evt.mousePosition.y - eventRect.y);
        m_originMousePosition = mousePosition;
        switch (evt.type)
        {
            case EventType.MouseDrag:
                var delta = evt.delta / m_scale;
                if (null != m_currentDraggingNode)
                {
                    var rect = m_currentDraggingNode.m_rect;
                    rect.x += delta.x;
                    rect.y += delta.y;
                    m_currentDraggingNode.m_rect = rect;
                    if(evt.control)
                    {
                        foreach (var output in m_currentDraggingNode.Outputs)
                        {
                            var nextNode = CommonNodeManager<TNode>.Instance.GetNode(output.m_to);
                            rect = nextNode.m_rect;
                            rect.x += delta.x;
                            rect.y += delta.y;
                            nextNode.m_rect = rect;
                        }
                        foreach (var input in m_currentDraggingNode.Inputs)
                        {
                            var preNode = CommonNodeManager<TNode>.Instance.GetNode(input.m_from);
                            rect = preNode.m_rect;
                            rect.x += delta.x;
                            rect.y += delta.y;
                            preNode.m_rect = rect;
                        }

                    }
                }
                else if(null != m_newConnectionStartPos)
                {
                    m_newConnectionEndPos = mousePosition;
                }
                else
                {
                    if (evt.delta.sqrMagnitude > 0.1f)
                    {
                        m_startPosition.x += evt.delta.x;
                        m_startPosition.y += evt.delta.y;
                    }
                }
                window.Repaint();
                break;
            case EventType.MouseDown:
                foreach (var node in m_visibleNodes)
                {
                    var rect = GetRealRect(node.m_rect);
                    if (rect.Contains(mousePosition))
                    {
                        if (mousePosition.x > rect.xMax - rect.width * 0.2f && Mathf.Abs(mousePosition.y - rect.center.y) < rect.height * 0.1f)
                        {
                            m_newConnectionStartNode = node;
                            m_newConnectionStartPos = mousePosition;
                            m_newConnectionEndPos = mousePosition;
                        }
                        else
                        {
                            SetSelected(node);
                            m_currentDraggingNode = node;
                        }
                        window.Repaint();
                        break;
                    }
                }
                break;
            case EventType.MouseUp:
                m_currentDraggingNode = null;
                if(null != m_newConnectionEndPos)
                {
                    var endNode = GetNodeInPosition(mousePosition);
                    if(null != endNode && null != m_newConnectionStartNode && m_newConnectionStartNode != endNode)
                    {
                        var exist = false;
                        if(!exist)
                        {
                            var addConnectionCmd = new AddConnectionCommand<TNode>(this, m_newConnectionStartNode, endNode);
                            ExecuteCommand(addConnectionCmd);
                        }
                    }
                    m_newConnectionStartPos = null;
                    m_newConnectionStartNode = null;
                    window.Repaint();
                }
                break;
            case EventType.ScrollWheel:
                var oldScale = m_scale;
                m_scale -= evt.delta.y * 0.01f;
                m_scale = Mathf.Max(0.2f, m_scale);
                m_scale = Mathf.Min(5, m_scale);
                var offset = new Vector2(m_startPosition.x, m_startPosition.y) - mousePosition;
                m_startPosition = mousePosition + offset * (m_scale / oldScale);
                window.Repaint();
                break;
            case EventType.ContextClick:
                var nodeUnderMouse = GetNodeInPosition(mousePosition);
                if(null != nodeUnderMouse)
                {
                    m_nodeMenu.ShowAsContext();
                }
                else
                {
                    m_commonContextMenu.ShowAsContext();
                }
                break;
            case EventType.KeyDown:
                switch (evt.keyCode)
                {
                    case KeyCode.C:
                        if(evt.control)
                        {
                            Debug.Log("Copy");
                        }
                        break;
                    case KeyCode.V:
                        if(evt.control)
                        {
                            Debug.Log("Paste");
                        }
                        break;
                    case KeyCode.Z:
                        if(evt.control && evt.shift)
                        {
                            RedoCommand(); 
                        }
                        else if(evt.control)
                        {
                            CancelCommand();
                        }
                        break;
                }
                break;
        }
    }

    private TNode GetNodeInPosition(Vector3 position)
    {
        foreach (var node in m_visibleNodes)
        {
            var rect = GetRealRect(node.m_rect);
            if (rect.Contains(position))
            {
                return node;
            }
        }
        return null;
    }

    private void ExecuteCommand(ICommand cmd)
    {
        var succeed = cmd?.Do();
        if(succeed.HasValue && succeed.Value)
        {
            m_commandStask.Push(cmd);
            m_canceledCommandStack.Clear();
        }
    }

    public void CancelCommand()
    {
        if(m_commandStask.Count > 0)
        {
            var cmd = m_commandStask.Pop();
            if (cmd.Undo())
            {
                m_canceledCommandStack.Push(cmd);
            }
        }    
    }

    public void RedoCommand()
    {
        if(m_canceledCommandStack.Count > 0)
        {
            var cmd = m_canceledCommandStack.Pop();
            m_commandStask.Push(cmd);
            cmd.Do();
        }
    }

    protected void CreateMenus()
    {
        for(var i = 0; i < 5; i++)
        {
            m_menuItems.Add(new ContextMenuItem
            {
                m_title = "Create " + i,
                m_toolTip = "CreateTip" + i,
                m_action = () =>
                {
                    var addNodeCmd = new AddNodeCommand<TNode>(this);
                    ExecuteCommand(addNodeCmd);
                }
            });
        }
        m_commonContextMenu = new GenericMenu(); 
        for(var i = 0; i < m_menuItems.Count; i++)
        {
            var item = m_menuItems[i];
            m_commonContextMenu.AddItem(new GUIContent(text:item.m_title, tooltip: item.m_toolTip), false, item.m_action);
        }
        m_connectionMenu = new GenericMenu();
        m_connectionMenu.AddItem(new GUIContent(text: "Delete", tooltip: ""), true, () =>
        {
            if(null != m_currentSelectedConnection)
            {
                m_connections.Remove(m_currentSelectedConnection);
                m_visibleConnections.Remove(m_currentSelectedConnection);
                m_currentSelectedConnection = null;
            }
        });
        m_nodeMenu = new GenericMenu();
        m_nodeMenu.AddItem(new GUIContent(text: "Delete", tooltip: ""), true, () =>
        {
            if(null != m_currentSelectedNode)
            {
                var deleteNodeCmd = new DeleteNodeCommand<TNode>(this, m_currentSelectedNode);
                ExecuteCommand(deleteNodeCmd);
            }
        });
    }

    private class ContextMenuItem
    {
        public string m_title;
        public string m_toolTip;
        public MenuFunction m_action;
    }

    private void DrawItems(EditorWindow window)
    {
        window.BeginWindows();
        DrawNodes(window);
        window.EndWindows();
        DrawConnections(window);
        DrawConnectionCounts();
        DrawNewConnection(window);
    }

    private void DrawNewConnection(EditorWindow window)
    {
        if(m_newConnectionStartPos.HasValue)
        {
            if(Vector2.Distance(m_newConnectionStartPos.Value, m_newConnectionEndPos) > 10)
            {
                DrawBezier(m_newConnectionStartPos.Value, m_newConnectionEndPos, Color.white);
            }
        }
    }

    private void SetInputNodesVisible(TNode currentNode, ref int visibleCount, int depth)
    {
        var inputs = currentNode.Inputs;
        for(var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var node = CommonNodeManager<TNode>.Instance.GetNode(input.m_from);
            node.tmpDepth = depth - 1;
            node.IsVisible = true;
        }
        visibleCount += inputs.Count;
        if(visibleCount > m_drawLimit)
        {
            return;
        }
        for(var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var node = CommonNodeManager<TNode>.Instance.GetNode(input.m_from);
            SetInputNodesVisible(node, ref visibleCount, depth - 1);
        }
    }


    private void SetOutputNodesVisible(TNode currentNode, ref int visibleCount, int depth)
    {
        var outputs = currentNode.Outputs;
        for(var i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            var node = CommonNodeManager<TNode>.Instance.GetNode(output.m_from);
            node.tmpDepth = depth + 1;
            node.IsVisible = true;
        }
        visibleCount += outputs.Count;
        if(visibleCount > m_drawLimit)
        {
            return;
        }
        for(var i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            var node = CommonNodeManager<TNode>.Instance.GetNode(output.m_to);
            SetOutputNodesVisible(node, ref visibleCount, depth + 1);
        }
    }

    private void RefresNodeRectWithBase(Rect currentRect, List<TNode> nodes, bool right)
    {
        var start = currentRect.center.y -  nodes.Count / 2 * m_nodeHeight * 2;
        for(var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var y = start + i * m_nodeHeight * 2;
            var x = right ? currentRect.center.x + m_nodeWidth * 2 : currentRect.center.x - m_nodeWidth * 2;
            node.m_rect = new Rect(x, y, currentRect.width, currentRect.height);
        }
    }

    private void RefreshNodeRect(Rect centerRect, List<TNode> nodes, int depth)
    {
        var start = centerRect.center.y -  nodes.Count / 2 * m_nodeHeight * 2;
        for(var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var y = start + i * m_nodeHeight * 2;
            node.m_rect = new Rect(centerRect.center.x + depth * m_nodeWidth * 2, y, centerRect.width, centerRect.height);
        }
    }

    private void DrawNodes(EditorWindow window)
    {
        for(var i = 0; i < m_visibleNodes.Count; i++)
        {
            var node = m_visibleNodes[i];
            var rect = GetRealRect(node.m_rect);
            if(rect.max.x < -10 && rect.max.y < -10)
            {
                continue;
            }
            if(rect.min.x > window.maxSize.x && rect.min.y > window.maxSize.y)
            {
                continue;
            }
            DrawNode(window, node, i);
        }
    }

    private void DrawDots(EditorWindow window)
    {
        for(var i = 0; i < m_visibleNodes.Count; i++)
        {
            var node = m_visibleNodes[i];
            var rect = GetRealRect(node.m_rect);
            if(rect.max.x < -10 && rect.max.y < -10)
            {
                continue;
            }
            if(rect.min.x > window.maxSize.x && rect.min.y > window.maxSize.y)
            {
                continue;
            }
            DrawNodeDot(window, node, i);
        }
    }

    private void DrawNodeDot(EditorWindow window, TNode node, int index)
    {
        var rect = GetRealRect(node.m_rect);
        var scale = 0.1f;
        if(GUI.Button(new Rect(rect.x, rect.y, rect.width * scale, rect.height * scale), node.Name))
        {
            m_connectionMenu.ShowAsContext();
        }
    }

    private void DrawNode(EditorWindow window, TNode node, int index)
    {
        var rect = GetRealRect(node.m_rect);
        if (m_currentSelectedNode == node)
        {
            var style = m_guiSkin.GetStyle("node 1");
            GUI.Window(index, rect, NodeWindowFunction, node.Name, style);
        }
        else
        {
            GUI.Window(index, rect, NodeWindowFunction, node.Name, m_guiSkin.GetStyle("node 0"));
        }
    }

    private void NodeWindowFunction(int windowId)
    {
        if(windowId < 0)
        {
            Debug.LogError($"windowId is less than zero : {windowId}");
            return;
        }
        if(windowId > m_visibleNodes.Count)
        {
            Debug.LogError($"window id is too large : {windowId}, should be less than {m_visibleNodes.Count}");
            return;
        }
        var node = m_visibleNodes[windowId];
        GUILayout.Label(node.Desc);
    }

    private void DrawConnectionCounts()
    {
        foreach (var node in m_visibleNodes)
        {
            var rect = GetRealRect(node.m_rect);
            GUI.Label(new Rect(rect.xMin, rect.center.y, rect.width, rect.height), node.Inputs.Count.ToString());
            GUI.Label(new Rect(rect.xMax, rect.center.y, rect.width, rect.height), node.Outputs.Count.ToString());
        }
    }

    private Rect GetRealRect(Rect src)
    {
        var rect = src;
        rect.x *= m_scale;
        rect.y *= m_scale;
        rect.x += m_startPosition.x;
        rect.y += m_startPosition.y;
        rect.width *= m_scale;
        rect.height *= m_scale;
        return rect;
    }

    private float m_connectionLabelSize = 12;
    GUIContent m_connectionLabelGUIContent;
    private Color m_selectedConnectionColor = Color.white;
    private void DrawConnections(EditorWindow window)
    {
        foreach (var connection in m_visibleConnections)
        {
            var fromNode = CommonNodeManager<TNode>.Instance.GetNode(connection.m_from);
            var toNode = CommonNodeManager<TNode>.Instance.GetNode(connection.m_to);
            if(!fromNode.IsVisible || !toNode.IsVisible)
            {
                continue;
            }
            var from = m_startPosition + fromNode.FromPos * m_scale;
            var to = m_startPosition + toNode.ToPos * m_scale;
            var center = (from + to) / 2;
            var trans1 = new Vector3(center.x, from.y);
            var trans2 = new Vector3(center.x, to.y);
            if (m_currentSelectedConnection == connection)
            {
                DrawBezier(from, to, m_selectedConnectionColor);
                //Handles.DrawBezier(from, to, trans1, trans2, m_selectedConnectionColor, null, 4);
            }
            else
            {
                DrawBezier(from, to, Color.gray);
                //Handles.DrawBezier(from, to, trans1, trans2, Color.gray, null, 3);
            }
            DrawDot(from);
            DrawDot(to);
            var bezierCenter = (from + to) / 2;
            var size = m_scale * m_connectionLabelSize;
            if (GUI.Button(new Rect(bezierCenter.x - size * 0.5f, bezierCenter.y - size * 0.5f, size, size), ""))
            {
                SetSelected(connection);
                m_connectionMenu.ShowAsContext();
            }
        }
    }

    private void DrawBezier(Vector3 from, Vector3 to, Color col)
    {
        var center = (from + to) / 2;
        var trans1 = new Vector3(center.x, from.y);
        var trans2 = new Vector3(center.x, to.y);
        Handles.DrawBezier(from, to, trans1, trans2, col, null, 3);
    }

    private void DrawDot(Vector2 pos)
    {
        var dotSize = 38 * 0.3f;
        GUI.DrawTexture(new Rect(pos.x - dotSize * 0.5f, pos.y - dotSize * 0.5f, dotSize, dotSize), m_texDot);
    }

    private Vector2 GetBezier(Vector2 start, Vector2 end, Vector2 trans1, Vector2 trans2, float percent)
    {
        var oneMinusPercent = 1 - percent;
        return oneMinusPercent * oneMinusPercent * oneMinusPercent * start + 3 * oneMinusPercent * oneMinusPercent * percent * trans1 + 3 * oneMinusPercent * percent * percent * trans2 + percent * percent * percent * end;
    }

    public void GetStarters(List<TNode> startNodes)
    {
        foreach(var node in m_nodes)
        {
            if(node.Inputs.Count <1)
            {
                startNodes.Add(node);
            }
        }
    }

    public void GetFinalNodes(List<TNode> finalnodes)
    {
        foreach(var node in m_nodes)
        {
            if(node.Outputs.Count < 1)
            {
                finalnodes.Add(node);
            }
        }
    }

    float m_nodeWidth = 100;
    float m_nodeHeight = 60;
    float m_offset = 20;
}


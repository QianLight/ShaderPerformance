//#define ENABLE_CONFIG_NODE_LOG
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zeus.Core.Util
{

    public class ConfigNodeGraph
    {
        public List<ConfigNodeConnection> m_connections = new List<ConfigNodeConnection>();
        public List<ConfigNode> m_nodes = new List<ConfigNode>();
        private List<List<ConfigNode>> m_sortedNodes = new List<List<ConfigNode>>();
        public Dictionary<string, Dictionary<string, List<string>>> m_columnConfigs;
        private HashSet<ConfigNode> m_nextNodes = new HashSet<ConfigNode>();
        private HashSet<ConfigNode> m_tmpNodes = new HashSet<ConfigNode>();
        private Vector3 m_startPosition;
        private float m_scale = 1;
        private ConfigNode m_currentSelectedNode;
        private ConfigNodeConnection m_currentSelectedConnection;
        private ConfigNode m_currentDraggingNode;

        public void Init(bool withWindow = false)
        {
            ConfigNodeManager.Instance.Reset();
            for(var i = 0; i < m_nodes.Count; i++)
            {
                ConfigNodeManager.Instance.Add(m_nodes[i]);
            }
            ConfigNodeConnectionManager.Instance.Reset();
            for(var i = 0; i < m_connections.Count; i++)
            {
                var connection = m_connections[i];
                ConfigNodeConnectionManager.Instance.Add(connection);
                connection.InitNodes(ConfigNodeManager.Instance.GetNode(connection.From), ConfigNodeManager.Instance.GetNode(connection.To));
            }
            if(withWindow)
            {
                SortNodes();
                GroupAndInitNodeRects();
                InitSkins();
            }
        }

        private void InitSkins()
        {
            var connectionLabelTexName = "Animation.Record@2x";
            var tex = EditorGUIUtility.FindTexture(connectionLabelTexName);
            m_connectionLabelGUIContent = EditorGUIUtility.IconContent("Animation.Record@2x");
        }

        private void SetSelected(object selectedObj)
        {
            m_currentSelectedConnection = null;
            m_currentSelectedNode = null;
            var node = selectedObj as ConfigNode;
            if(null != node)
            {
                m_currentSelectedNode = node;
                return;
            }
            var connection = selectedObj as ConfigNodeConnection;
            if(null != connection)
            {
                m_currentSelectedConnection = connection;
                return;
            }
        }

        private void SortNodes()
        {
            m_sortedNodes.Clear();
            HashSet<ConfigNode> m_starters = new HashSet<ConfigNode>();
            for(var i = 0; i < m_nodes.Count; i++)
            {
                if(m_nodes[i].Inputs.Count < 1)
                {
                    m_starters.Add(m_nodes[i]);
                }
            }
            foreach(var node in m_starters)
            {
                var list = new List<ConfigNode>() { node};
                m_sortedNodes.Add(list);
                GenerateSortedNodes(node, list, m_sortedNodes);
            }
            m_sortedNodes.Sort((x, y) =>
            {
                if(x.Count == y.Count)
                {
                    return 0;
                }
                if(x.Count > y.Count)
                {
                    return -1;
                }
                return 1;
            });
            var nodes = new HashSet<ConfigNode>();
            for(var i = 0; i< m_sortedNodes.Count; i++)
            {
                var nodeList = m_sortedNodes[i];
                for(var j = nodeList.Count - 1; j >= 0; j--)
                {
                    var node = nodeList[j];
                    if(nodes.Contains(node))
                    {
                        nodeList[j] = null;
                    }
                    nodes.Add(node);
                }
            }
        }

        private void GenerateSortedNodes(ConfigNode node, List<ConfigNode> currentList, List<List<ConfigNode>> sortedNodes)
        {
            if(node.Outputs.Count > 0)
            {
                var connection = node.Outputs[0];
                var toNode = ConfigNodeManager.Instance.GetNode(connection.To);
                currentList.Add(toNode);
                GenerateSortedNodes(toNode, currentList, sortedNodes);
            }
            for(var i = 1; i < node.Outputs.Count; i++)
            {
                var list = new List<ConfigNode>(currentList);
                m_sortedNodes.Add(list);
                var toNode = ConfigNodeManager.Instance.GetNode(node.Outputs[i].To);
                list.Add(toNode);
                GenerateSortedNodes(toNode, list, sortedNodes);
            }
        }

        private void DrawSelectedInfo()
        {
            if(null != m_currentSelectedNode)
            {
                GUILayout.Label(m_currentSelectedNode.Name);
                m_currentSelectedNode.OnGUI();
            }
        }

        private void DrawMenu()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Save"))
                {

                }
            }
        }

        public void Draw(EditorWindow window)
        {
            DrawMenu();
            DrawSelectedInfo();
            var groupRect = new Rect(0, 100, window.maxSize.x, window.maxSize.y - 100);
            GUI.BeginGroup(groupRect);
            DrawItems();
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
            switch (evt.type)
            {
                case EventType.MouseDrag:
                    if (null != m_currentDraggingNode)
                    {
                        var rect = m_currentDraggingNode.m_rect;
                        rect.x += evt.delta.x;
                        rect.y += evt.delta.y;
                        m_currentDraggingNode.m_rect = rect;
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
                    foreach (var node in m_nodes)
                    {
                        var rect = GetRealRect(node.m_rect);
                        if (rect.Contains(mousePosition))
                        {
                            SetSelected(node);
                            m_currentDraggingNode = node;
                            window.Repaint();
                            break;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    m_currentDraggingNode = null;
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
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent { text = "AddNode", tooltip = "Tip" }, false, () =>
                      {
                          m_nodes.Add(ConfigNodeManager.Instance.CreateNode("New"));
                          SortNodes();
                      });
                    menu.ShowAsContext();
                    break;
            }
        }

        private void DrawItems()
        {
            DrawNodes();
            DrawConnections();
        }

        private void DrawNodes()
        {
            foreach(var node in m_nodes)
            {
                var rect = GetRealRect(node.m_rect);
                if(m_currentSelectedNode == node)
                {
                    EditorGUI.DrawRect(rect, m_selectedNodeColor);
                }
                else
                {
                    EditorGUI.DrawRect(rect, node.m_color);
                }
                GUI.Box(rect, node.Name);
            }
        }

        private Rect GetRealRect(Rect src)
        {
            var rect = src;
            rect.x += m_startPosition.x;
            rect.y += m_startPosition.y;
            rect.x *= m_scale;
            rect.y *= m_scale;
            rect.width *= m_scale;
            rect.height *= m_scale;
            return rect;
        }

        private float m_connectionLabelSize = 12;
        GUIContent m_connectionLabelGUIContent;
        private Color m_selectedConnectionColor = Color.white;
        private Color m_selectedNodeColor = Color.white;
        private void DrawConnections()
        {
            foreach (var connection in m_connections)
            {
                var fromNode = ConfigNodeManager.Instance.GetNode(connection.From);
                var toNode = ConfigNodeManager.Instance.GetNode(connection.To);
                var from = (m_startPosition + fromNode.FromPos) * m_scale;
                var to = (m_startPosition + toNode.ToPos) * m_scale;
                var center = (from + to) / 2;
                //var trans1 = new Vector3(center.x, from.y);
                var trans1 = new Vector3(from.x + m_offset * 0.5f, from.y);
                //var trans2 = new Vector3(center.x, to.y);
                var trans2 = new Vector3(from.x + m_offset * 0.5f, to.y);
                if(m_currentSelectedConnection == connection)
                {
                    Handles.DrawBezier(from, to, trans1, trans2, m_selectedConnectionColor, null, 2);
                }
                else
                {
                    Handles.DrawBezier(from, to, trans1, trans2, fromNode.m_color, null, 2);
                }
                var bezierCenter = GetBezier(from, to, trans1, trans2, 0.5f);
                var size = m_scale * m_connectionLabelSize;
                if(GUI.Button(new Rect(bezierCenter.x - size * 0.5f, bezierCenter.y - size * 0.5f, size, size), m_connectionLabelGUIContent))
                {
                    SetSelected(connection);
                }
            }
        }

        private Vector2 GetBezier(Vector2 start, Vector2 end, Vector2 trans1, Vector2 trans2, float percent)
        {
            var oneMinusPercent = 1 - percent;
            return oneMinusPercent * oneMinusPercent * oneMinusPercent * start + 3 * oneMinusPercent * oneMinusPercent * percent * trans1 + 3 * oneMinusPercent * percent * percent * trans2 + percent * percent * percent * end;
        }

        float m_nodeWidth = 100;
        float m_nodeHeight = 60;
        float m_offset = 20;

        private void GroupAndInitNodeRects()
        {
            for(var listIndex = 0; listIndex < m_sortedNodes.Count; listIndex++)
            {
                for(var nodeIndex = 0; nodeIndex < m_sortedNodes[listIndex].Count; nodeIndex++)
                {
                    var node = m_sortedNodes[listIndex][nodeIndex];
                    if(null == node)
                    {
                        continue;
                    }
                    var posx = (m_nodeWidth + m_offset) * nodeIndex + m_startPosition.x;
                    var posY = listIndex * (m_nodeHeight + m_offset) + m_startPosition.y + nodeIndex * 20;
                    var rect = new Rect(posx * m_scale, posY * m_scale, m_nodeWidth * m_scale, m_nodeHeight * m_scale);
                    node.m_rect = rect;
                    var r = (float)listIndex / m_sortedNodes.Count;
                    var g = (float)nodeIndex / m_sortedNodes[listIndex].Count;
                    node.m_color = new Color(r, g, 1);
                }
            }
        }

        public HashSet<string> Execute(Dictionary<string, Dictionary<string, List<string>>> columnConfigs, HashSet<string> extraInputs = null)
        {
            m_columnConfigs = columnConfigs;
            var result = new HashSet<string>();
            var currentNodes = new HashSet<ConfigNode>();
            foreach (var node in m_nodes)
            {
                if(node.Inputs.Count < 1)
                {
                    if(null != extraInputs)
                    {
                        node.SetExtraInputs(extraInputs);
                    }
                    currentNodes.Add(node);
                }
            }
            Execute(currentNodes, result);
            return result;
        }

        private void Execute(HashSet<ConfigNode> currentNodes, HashSet<string> result)
        {
            m_tmpNodes.Clear();
            foreach(var node in currentNodes)
            {
                node.Execute(m_columnConfigs);
            }
            foreach(var node in currentNodes)
            {
                if(node.Outputs.Count < 1)
                {
                    var nodeResult = node.GetResult();
                    foreach(var val in nodeResult)
                    {
                        result.Add(val);
                    }
                    continue;
                }
                foreach(var connection in node.Outputs)
                {
                    var nextNode = ConfigNodeManager.Instance.GetNode(connection.To);
                    m_tmpNodes.Add(nextNode);
                }
            }
            if(m_tmpNodes.Count < 1)
            {
                return;  
            }
            m_nextNodes.Clear();
            foreach(var node in m_tmpNodes)
            {
                m_nextNodes.Add(node);
            }
            Execute(m_nextNodes, result);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public class ConfigNodeManager
    {
        private static ConfigNodeManager m_instance = new ConfigNodeManager();
        public static ConfigNodeManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public void Reset()
        {
            m_configNodes.Clear();
        }

        Dictionary<string, ConfigNode> m_configNodes = new Dictionary<string, ConfigNode>();

        public void Add(ConfigNode node)
        {
            m_configNodes[node.Id] = node;
        }

        public void GetAllNodes(List<ConfigNode> result)
        {
            foreach(var pair in m_configNodes)
            {
                result.Add(pair.Value);
            }
        }

        public ConfigNode GetNode(string nodeId)
        {
            if(!m_configNodes.TryGetValue(nodeId, out var configNode))
            {
                return null;
            }
            return configNode;
        }

        public ConfigNode CreateNode(string name)
        {
            var node = new ConfigNode
            {
                Name = name,
            };
            m_configNodes.Add(node.Id, node);
            return node;
        }

        public ConfigNode CreateNode(string name, LoopSelector selector)
        {
            var node = new ConfigNode
            {
                Name = name,
                LoopSelector = selector,
            };
            m_configNodes.Add(node.Id, node);
            return node;
        }

        public ConfigNode CreateNode(string name, CommonSelector selector)
        {
            var node = new ConfigNode
            {
                Name = name,
                ConfigSelector = selector,
            };
            m_configNodes.Add(node.Id, node);
            return node;
        }

    }

    public class ConfigNodeConnectionManager
    {
        private static ConfigNodeConnectionManager m_instance = new ConfigNodeConnectionManager();
        public static ConfigNodeConnectionManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        private HashSet<ConfigNodeConnection> m_connections = new HashSet<ConfigNodeConnection>();
        public void Reset()
        {
            m_connections.Clear();
        }

        public void Add(ConfigNode from, ConfigNode to)
        {
            var connection = new ConfigNodeConnection(from, to);
            m_connections.Add(connection);
            from.Outputs.Add(connection);
            to.Inputs.Add(connection);
        }

        public void Connect(params ConfigNode[] nodes)
        {
            for(var i = 0; i < nodes.Length - 1; i++)
            {
                Add(nodes[i], nodes[i + 1]);
            }
        }

        public void Add(ConfigNodeConnection connection)
        {
            m_connections.Add(connection);
        }

        public void GetAllConnection(List<ConfigNodeConnection> result)
        {
            foreach(var connection in m_connections)
            {
                result.Add(connection);
            }
        }

        public void GetConnectionsByFrom(string from, List<ConfigNodeConnection> result)
        {
            foreach(var connection in m_connections)
            {
                if(connection.From.Equals(from))
                {
                    result.Add(connection);
                }
            }
        }

        public void GetConnectionsByTo(string to, List<ConfigNodeConnection> result)
        {
            foreach(var connection in m_connections)
            {
                if(connection.To.Equals(to))
                {
                    result.Add(connection);
                }
            }
        }

        public void Remove(ConfigNodeConnection connection)
        {
            m_connections.Remove(connection);
        }
    }

    [Serializable]
    public class ConfigNodeConnection
    {
        private ConfigNode FromNode;
        private ConfigNode ToNode;
        public string From;
        public string To;
        public HashSet<string> Datas = new HashSet<string>();
        public Rect m_rect { get; set; }

        private ConfigNodeConnection() { }
        public ConfigNodeConnection(ConfigNode from , ConfigNode to)
        {
            FromNode = from;
            ToNode = to;
            From = from.Id;
            To = to.Id;
        }

        public void InitNodes(ConfigNode from, ConfigNode to)
        {
            from.Outputs.Add(this);
            to.Inputs.Add(this);
            FromNode = from;
            ToNode = to;
        }

        public override string ToString()
        {
            if(null != FromNode && null != ToNode)
            {
                return $"connection {FromNode.Name}->{ToNode.Name}";
            }
            else
            {
                return $"connection {From}->{To}";
            }
        }
    }

    public enum SelectorCondition
    {
        None,
        Equals,
        NotEquals,
        Range,
    }

    [Serializable]
    public class Filter
    {
        public string ColumnName;
        public SelectorCondition Condition;
        public List<string> Params;

        public void OnGUI()
        {
            ColumnName = EditorGUILayout.TextField("ColumnName:", ColumnName, GUILayout.Width(50));
            Condition = (SelectorCondition)EditorGUILayout.EnumPopup(Condition, GUILayout.Width(50));
        }
    }

    public class EqualToFunc
    {
        private HashSet<string> BaseSet = new HashSet<string>();

        public void Add(string val)
        {
            BaseSet.Add(val);
        } 

        public bool IsContains(string val)
        {
            return BaseSet.Contains(val);
        }
    }

    public class NotEqualToFunc
    {
        private HashSet<string> BaseSet = new HashSet<string>();

        public void Add(string val)
        {
            BaseSet.Add(val);
        }

        public bool IsNotContains(string val)
        {
            return !BaseSet.Contains(val);
        }
    }

    public class RangeFunc
    {
        private int m_start;
        private int m_end;

        public void Init(string start, string end)
        {
            m_start = int.Parse(start);
            m_end = int.Parse(end);
        }

        public bool IsInRange(string val)
        {
            if(string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(val))
            {
                return false;
            }
            var intVal = int.Parse(val);
            return intVal >= m_start && intVal <= m_end;
        }
    }


    [Serializable]
    public class CommonSelector : ISelector
    {
        public string TableName;
        public List<Filter> Filters;
        public List<string> OutPutColumns;

        public void OnGUI()
        {
            EditorGUILayout.TextField("TableName:", TableName, GUILayout.Width(50));
            foreach(var filter in Filters)
            {
                filter.OnGUI();
            }
        }

        private List<ColumnFilter> GetFilter(List<Filter> filters)
        {
            if(null == filters)
            {
                return null;
            }
            var result = new List<ColumnFilter>();
            foreach(var filter in filters)
            {
                Func<string, bool> filterFunc = null;
                switch (filter.Condition)
                {
                    case SelectorCondition.Equals:
                        var equalToFunc = new EqualToFunc();
                        foreach(var para in filter.Params)
                        {
                            equalToFunc.Add(para);
                        }
                        filterFunc = equalToFunc.IsContains;
                        break;
                    case SelectorCondition.NotEquals:
                        var notEqualToFunc = new NotEqualToFunc();
                        foreach(var para in filter.Params)
                        {
                            notEqualToFunc.Add(para);
                        }
                        filterFunc = notEqualToFunc.IsNotContains;
                        break;
                    case SelectorCondition.Range:
                        var rangeFunc = new RangeFunc();
                        rangeFunc.Init(filter.Params[0], filter.Params[1]);
                        filterFunc = rangeFunc.IsInRange;
                        break;
                }
                var columnFilter = new ColumnFilter
                {
                    ColumnTitle = filter.ColumnName,
                    Filter = filterFunc,
                };
                result.Add(columnFilter);
            }
            return result;
        }

        public HashSet<string> Execute(Dictionary<string, Dictionary<string, List<string>>> columnConfigs, HashSet<string> inputs)
        {
            var result = new HashSet<string>();
            var filters = GetFilter(Filters);
            for (var i = 0; i < OutPutColumns.Count; i++)
            {
                var selector = new ColumnSelector
                {
                    Column = OutPutColumns[i],
                    Filters = filters,
                    FilterLink = ColumnSelectorFilterLink.Or,
                };
                var vals = BundleConfigUtil.SelectVals(columnConfigs, TableName, selector, inputs);
                foreach (var val in vals)
                {
                    result.Add(val);
                }
            }
            return result;
        }

        private string CollectionToString(IEnumerable<string> vals)
        {
            if(null == vals)
            {
                return "";
            }
            var result = "";
            foreach(var val in vals)
            {
                result += val + ",";
            }
            return result;

        }
    }

    [Serializable]
    public class LoopSelector : ISelector
    {
        public string TableName;
        public string ColumnTitle;

        public HashSet<string> Execute(Dictionary<string, Dictionary<string, List<string>>> columnConfigs, HashSet<string> inputs)
        {
            if(null == inputs)
            {
                Debug.LogError("inputs should not be null for LoopSelector");
                return null;
            }
            var result = new HashSet<string>();
            foreach(var val in inputs)
            {
                result.Add(val);
            }
            while(true)
            {
                var lastCount = result.Count;
                var selectedVals = BundleConfigUtil.SelectVals(columnConfigs, TableName, result, new List<string> { ColumnTitle });
                foreach(var val in selectedVals)
                {
                    result.Add(val);
                }
                if(lastCount == result.Count)
                {
                    break;
                }
            }
            return result;
        }

        public void OnGUI()
        {
            TableName = EditorGUILayout.TextField("TableName:", TableName, GUILayout.Width(50));
            ColumnTitle = EditorGUILayout.TextField("ColumnTitle", ColumnTitle, GUILayout.Width(50));
        }

        private string HashSetToString(HashSet<string> vals)
        {
            if(null == vals)
            {
                return "";
            }
            var result = "";
            foreach(var val in vals)
            {
                result += val + ",";
            }
            return result;
        }
    }

    public interface ISelector
    {
        HashSet<string> Execute(Dictionary<string, Dictionary<string, List<string>>> columnConfigs, HashSet<string> inputs);
    }

    [Serializable]
    public class ConfigNode
    {
        public Color m_color { get; set; }
        public Rect m_rect
        {
            get;
            set;
        }
        public Vector3 FromPos
        {
            get
            {
                return new Vector3(m_rect.xMax, m_rect.center.y);
            }
        }

        public Vector3 ToPos
        {
            get
            {
                return new Vector3(m_rect.xMin, m_rect.center.y);
            }
        }

        public string Id;
        public string Name;
        [NonSerialized]
        public List<ConfigNodeConnection> Inputs = new List<ConfigNodeConnection>();
        [NonSerialized]
        public List<ConfigNodeConnection> Outputs = new List<ConfigNodeConnection>();
        public ISelector ConfigSelector
        {
            get
            {
                if(null != LoopSelector && !string.IsNullOrEmpty(LoopSelector.TableName))
                {
                    return LoopSelector;
                }
                else
                {
                    return Selector;
                }
            }
            set
            {
                if(value is LoopSelector)
                {
                    LoopSelector = value as LoopSelector;
                }
                else
                {
                    Selector = value as CommonSelector;
                }
            }
        }
        public LoopSelector LoopSelector;
        public CommonSelector Selector;

        private HashSet<string> m_result;
        private HashSet<string> m_extraInputs = new HashSet<string>();

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Name);
            if(null != LoopSelector)
            {
                LoopSelector.OnGUI();
            }
            else if(null != Selector)
            {
                Selector.OnGUI();
            }
            GUILayout.EndHorizontal();
        }

        public void SetExtraInputs(IEnumerable<string> extraInputs)
        {
            m_extraInputs.Clear();
            foreach(var input in extraInputs)
            {
                m_extraInputs.Add(input);
            }
        }

        public HashSet<string> GetResult()
        {
            return m_result;
        }

        public ConfigNode()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public void Execute(Dictionary<string, Dictionary<string, List<string>>> columnConfigs)
        {
            var inputSet = new HashSet<string>();
            foreach(var input in Inputs)
            {
                foreach(var data in input.Datas)
                {
                    inputSet.Add(data);
                }
            }
            if(null != m_extraInputs)
            {
                foreach(var input in m_extraInputs)
                {
                    inputSet.Add(input);
                }
            }
            m_result = ConfigSelector.Execute(columnConfigs, inputSet);
#if ENABLE_CONFIG_NODE_LOG
            var inputStr = "";
            foreach(var input in inputSet)
            {
                inputStr += input + ",";
            }
            var outputStr = "";
            foreach(var output in m_result)
            {
                outputStr += output + ",";
            }
            Debug.Log($"Execute { Name} with input {inputStr} output {outputStr}");
#endif

            if(Outputs.Count > 0)
            {
                foreach(var connection in Outputs)
                {
                    connection.Datas = m_result;
                }
            }
        }
    }

    public class ConfigFlow
    {
        public ConfigNode StartNode;

        public ConfigNodeGraph MakeSkillXmlBaseToFxNameGraph()
        {
            ConfigNodeManager.Instance.Reset();
            ConfigNodeConnectionManager.Instance.Reset();
            var selectSkillXmlBaseNextIdsNode = ConfigNodeManager.Instance.CreateNode("selectSkillXmlBaseNextIdsNode", new LoopSelector
                {
                    ColumnTitle = "AutoNextSkillID",
                    TableName = "SkillXmlBase",
                });
            var selectPerformIdsInSkillXmlBaseNode = ConfigNodeManager.Instance.CreateNode("selectPerformIdsNode", new CommonSelector
                {
                    OutPutColumns = new List<string> { "PerformMatchID" },
                    TableName = "SkillXmlBase",
                });

            var selectPerformEventOutputColumns = new List<string>();
            for (var i = 1; i <= 10; i++)
            {
                selectPerformEventOutputColumns.Add("EventId" + i);
            }
            var selectPerformEventIdsInSkillPerformNode = ConfigNodeManager.Instance.CreateNode("selectPerformEventIdsNode", new CommonSelector
            {
                OutPutColumns = selectPerformEventOutputColumns,
                TableName = "SkillPerform"
            });
            ConfigNodeConnectionManager.Instance.Connect(
                selectSkillXmlBaseNextIdsNode,
                selectPerformIdsInSkillXmlBaseNode,
                selectPerformEventIdsInSkillPerformNode);
            var selectEffectNameNode = ConfigNodeManager.Instance.CreateNode("selectEffectNameNode", new CommonSelector
            {
                TableName = "Effect",
                OutPutColumns = new List<string> { "EffectName" },
            });

            for(var i = 1; i <= 10; i++)
            {
                var filter = new Filter
                {
                    ColumnName = "EventType" + i,
                    Condition = SelectorCondition.Equals,
                    Params = new List<string> { "1", "0" },
                };
                var selectPerformEventParaWithType1Node = ConfigNodeManager.Instance.CreateNode("selectPerformEventParaWithType1Node", new CommonSelector
                {
                    Filters = new List<Filter> { filter },
                    OutPutColumns = new List<string> { "EventPara" + i },
                    TableName = "SkillPerformEvent"
                });
                ConfigNodeConnectionManager.Instance.Connect(selectPerformEventIdsInSkillPerformNode, selectPerformEventParaWithType1Node, selectEffectNameNode);
            }

            var type7filters = new List<Filter>();
            for(var i = 1; i <= 10; i++)
            {
                var filter = new Filter
                {
                    ColumnName = "EventType" + i,
                    Condition = SelectorCondition.Equals,
                    Params = new List<string> { "7" },
                };
                type7filters.Add(filter);
            }
            var selectPerformEventparamWithType7Node = ConfigNodeManager.Instance.CreateNode("selectPerformEventparamWithType7Node", new CommonSelector
            {
                Filters = type7filters,
                OutPutColumns = new List<string> { "EventPara1" },
                TableName = "SkillPerformEvent"
            });
            var selectBulletNode = ConfigNodeManager.Instance.CreateNode("selectBulletNode", new CommonSelector
            {
                TableName = "Bullet",
                OutPutColumns = new List<string> { "EffectID", "ArriveEffectID" },
            });
            ConfigNodeConnectionManager.Instance.Connect(
                selectPerformEventIdsInSkillPerformNode,
                selectPerformEventparamWithType7Node,
                selectBulletNode,
                selectEffectNameNode);
            var selectAnimIdsInSkillPerformNode = ConfigNodeManager.Instance.CreateNode("selectAnimIds", new CommonSelector
            {
                TableName = "SkillPerform",
                OutPutColumns = new List<string>
                    {
                        "StartAniID",
                        "StartAniIDFemale",
                        "JumpAniID",
                        "JumpAniIDFemale",
                    },
            });
            var selectAnimFxIdsInAnimationNode = ConfigNodeManager.Instance.CreateNode("selectAnimFxIdsNode", new CommonSelector
            {
                TableName = "Animation",
                OutPutColumns = new List<string> { "StartEffect01", "StartEffect02", "StartEffect03", "StartEffect04", "StartEffect05", "StartEffect06" },
            });

            ConfigNodeConnectionManager.Instance.Connect(
                selectPerformIdsInSkillXmlBaseNode,
                selectAnimIdsInSkillPerformNode,
                selectAnimFxIdsInAnimationNode,
                selectEffectNameNode);
            ConfigNodeGraph graph = new ConfigNodeGraph();

            ConfigNodeManager.Instance.GetAllNodes(graph.m_nodes);
            ConfigNodeConnectionManager.Instance.GetAllConnection(graph.m_connections);

            var json = JsonUtility.ToJson(graph);
            Debug.Log(json);
            File.WriteAllText("d:/flow.json", json);
            return graph;
        }

        public ConfigNodeGraph LoadSkillFlow()
        {
            var json = File.ReadAllText("d:/flow.json");
            var graph = JsonUtility.FromJson<ConfigNodeGraph>(json);
            graph.Init();
            return graph;
        }
    }
}
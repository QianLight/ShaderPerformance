using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using BluePrint;
using EditorNode;
using System.Collections.Generic;
using EcsData;
using System.Reflection;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Xml;

namespace TDTools.SkillEditorTemplate {
    
    /// <summary>
    /// 存储技能节点间连线的数据结构
    /// </summary>
    public struct SkillConnectionData {
        public int BeginNodeID;
        public int BeginPinID;

        public int EndNodeID;
        public int EndPinID;
    }

    /// <summary>
    /// 技能编辑器模板工具的模板编辑界面
    /// 模板编辑和存读取写在这里
    /// </summary>
    public class SkillEditorTemplateEditorWindow : EditorWindow {

        #region 变量

        /// <summary>
        /// 目前编辑中的模板
        /// </summary>
        SkillEditorTemplate buffer;
        ListView listView;

        /// <summary>节点
        /// 被选中的
        /// </summary>
        BaseSkillNode selected;

        /// <summary>
        /// 用来画节点变量的IMGUI容器
        /// </summary>
        IMGUIContainer container;

        #endregion

        #region 序列化逻辑

        /// <summary>
        /// 新版的序列化程序，比老的更稳定
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static MemoryStream NewSerilize(SkillEditorTemplate template) {
            MemoryStream stream = new MemoryStream();
            try {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                XmlTextWriter xw = new XmlTextWriter(stream, Encoding.UTF8);
                xw.Formatting = Formatting.Indented;
                xw.Indentation = 4;

                HashSet<int> set = new HashSet<int>();

                xw.WriteStartDocument();
                xw.WriteStartElement("Template");

                xw.WriteElementString("Name", template.TemplateName);
                xw.WriteElementString("Type", template.TemplateType);
                xw.WriteElementString("Author", template.TemplateAuthor);
                xw.WriteElementString("Description", template.TemplateDescription);
                xw.WriteElementString("NodeCount", template.TemplateNodeCount);
                xw.Flush();
                stream.WriteByte(((byte)'\n'));

                xw.WriteStartElement("Nodes");
                for (int i = 0; i < template.Nodes.Length; i++) {
                    set.Add(template.Nodes[i].nodeEditorData.NodeID);
                    if (i > 0) {
                        xw.Flush();
                        stream.WriteByte(((byte)'\n'));
                    }
                    xw.WriteStartElement("Node");
                    xw.WriteAttributeString("type", template.Nodes[i].GetType().AssemblyQualifiedName);

                    XmlSerializer xml = new XmlSerializer(template.Nodes[i].nodeEditorData.GetType());
                    xml.Serialize(xw, template.Nodes[i].nodeEditorData, ns);

                    var data = template.Nodes[i].GetHosterData<XBaseData>();
                    xw.WriteElementString("DataType", data.GetType().AssemblyQualifiedName);
                    xml = new XmlSerializer(data.GetType());
                    xml.Serialize(xw, data, ns);

                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                xw.Flush();
                stream.WriteByte(((byte)'\n'));

                List<SkillConnectionData> connections = new List<SkillConnectionData>();
                for (int i = 0; i < template.Nodes.Length; i++) {
                    for (int j = 0; j < template.Nodes[i].pinList.Count; j++) {
                        if (template.Nodes[i].pinList[j].HasConnection()) {
                            for (int k = 0; k < template.Nodes[i].pinList[j].connections.Count; k++) {
                                var start = template.Nodes[i].pinList[j].connections[k].connectStart;
                                var end = template.Nodes[i].pinList[j].connections[k].connectEnd;
                                if (set.Contains(start.GetNode<BaseSkillNode>().nodeEditorData.NodeID) && set.Contains(end.GetNode<BaseSkillNode>().nodeEditorData.NodeID))
                                    connections.Add(new SkillConnectionData {
                                        BeginNodeID = start.GetNode<BaseSkillNode>().nodeEditorData.NodeID,
                                        BeginPinID = start.pinID,
                                        EndNodeID = end.GetNode<BaseSkillNode>().nodeEditorData.NodeID,
                                        EndPinID = end.pinID
                                    });
                            }
                        }
                    }
                }

                xw.WriteStartElement("Connections");
                XmlSerializer xml2 = new XmlSerializer(typeof(SkillConnectionData));
                for (int i = 0; i < connections.Count; i++) {
                    if (i > 0) {
                        xw.Flush();
                        stream.WriteByte(((byte)'\n'));
                    }

                    xml2.Serialize(xw, connections[i], ns);
                }

                for (int i = 0; i < template.Connections?.Count; i++) {
                    if (i > 0) {
                        xw.Flush();
                        stream.WriteByte(((byte)'\n'));
                    };
                    xml2.Serialize(xw, template.Connections[i], ns);
                }
                xw.WriteEndElement();

                xw.Flush();
                stream.WriteByte(((byte)'\n'));

                xw.WriteEndElement();
                xw.WriteEndDocument();

                xw.Flush();

                stream.Position = 0;

            } catch (Exception e){
                Debug.Log(e.Message);

            }
            return stream;
        }


        public static SkillEditorTemplate Deserilize(Stream stream) {
            XmlTextReader xr = new XmlTextReader(stream);
            xr.WhitespaceHandling = WhitespaceHandling.None;

            SkillEditorTemplate template = new SkillEditorTemplate();

            xr.ReadStartElement();

            template.TemplateName = xr.ReadElementContentAsString();
            template.TemplateType = xr.ReadElementContentAsString();
            template.TemplateAuthor = xr.ReadElementContentAsString();
            template.TemplateDescription = xr.ReadElementContentAsString();
            int nodeCount = xr.ReadElementContentAsInt();
            template.TemplateNodeCount = nodeCount.ToString();
            xr.ReadStartElement();

            template.Nodes = new BaseSkillNode[nodeCount];
            for (int i = 0; i < nodeCount; i++) {
                var type = Type.GetType(xr.GetAttribute(0));
                template.Nodes[i] = (BaseSkillNode)Activator.CreateInstance(type);

                xr.ReadStartElement();

                var r = xr.ReadSubtree();
                XmlSerializer xml = new XmlSerializer(typeof(NodeConfigData));
                template.Nodes[i].nodeEditorData = (NodeConfigData)xml.Deserialize(r);
                xr.ReadEndElement();

                Type dataType = Type.GetType(xr.ReadElementContentAsString());
                xml = new XmlSerializer(dataType);
                var r2 = xr.ReadSubtree();
                var data = xml.Deserialize(r2);
                template.Nodes[i].SetPublicField("HosterData", data);
                xr.ReadEndElement();
                xr.ReadEndElement();
            }
            xr.ReadEndElement();


            List<SkillConnectionData> cons = new List<SkillConnectionData>();
            while (xr.Read()) {
                if (xr.NodeType == XmlNodeType.EndElement)
                    break;

                XmlSerializer xml = new XmlSerializer(typeof(SkillConnectionData));
                var r = xr.ReadSubtree();
                cons.Add((SkillConnectionData)xml.Deserialize(r));
            }
            xr.ReadEndElement();

            template.Connections = cons;

            return template;
        }



        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="path">模板文件位置</param>
        /// <returns></returns>
        public static SkillEditorTemplate Deserilize(string path) {
            FileStream stream = new FileStream(path, FileMode.Open);
            var result = Deserilize(stream);
            result.path = path;
            stream.Close();
            return result;
        }




        /// <summary>
        /// 将模板保存到文件
        /// </summary>
        /// <param name="path">目标文件地址</param>
        void Save(string path) {
            if (buffer == null)
                return;

            SkillEditorTemplate template = new SkillEditorTemplate();

            template.TemplateName = rootVisualElement.Q<TextField>("TFTemplateName").value;
            template.TemplateType = rootVisualElement.Q<TextField>("TFTemplateType").value;
            template.TemplateAuthor = rootVisualElement.Q<TextField>("TFTemplateAuthor").value;
            template.TemplateDescription = rootVisualElement.Q<TextField>("TFTemplateDes").value;

            if (File.Exists($"{path}{template.TemplateName}.xml") && !EditorUtility.DisplayDialog("保存", "该模板已经存在，确认要覆盖吗？", "保存", "取消")) {
                return;
            }

            FileStream stream = new FileStream($"{path}{template.TemplateName}.xml", FileMode.Create);
            SkillEditor wnd = (SkillEditor)GetWindow(typeof(SkillEditor));
            if (wnd.CurrentGraph.widgetList != null && wnd.CurrentGraph.widgetList.Count > 0) {
                List<BaseSkillNode> t = new List<BaseSkillNode>();
                for (int i = 0; i < wnd.CurrentGraph.widgetList.Count; i++)
                    if (wnd.CurrentGraph.widgetList[i] is BaseSkillNode node) {
                        t.Add(node);
                    }
                template.Nodes = t.ToArray();
                template.TemplateNodeCount = t.Count.ToString();
            }

            NewSerilize(template).CopyTo(stream);

            stream.Close();
            GetWindow<SkillEditorTemplateWindow>().Refresh();
        }

        #endregion

        #region 界面逻辑

        /// <summary>
        /// 设置模板的各项参数到输入框内
        /// </summary>
        /// <param name="template"></param>
        public void SetTemplate(SkillEditorTemplate template) {
            rootVisualElement.Q<TextField>("TFTemplateName").value = template.TemplateName;
            rootVisualElement.Q<TextField>("TFTemplateType").value = template.TemplateType;
            rootVisualElement.Q<TextField>("TFTemplateAuthor").value = template.TemplateAuthor;
            rootVisualElement.Q<TextField>("TFTemplateDes").value = template.TemplateDescription;
        }

        /// <summary>
        /// 显示模板编辑器界面
        /// </summary>
        /// <param name="template">需要显示的模板</param>
        /// <returns></returns>
        public static SkillEditorTemplateEditorWindow ShowWindow(ref SkillEditorTemplate template) {

            SkillEditorTemplateEditorWindow wnd = GetWindow<SkillEditorTemplateEditorWindow>();
            wnd.buffer = template;
            wnd.titleContent = new GUIContent("保存模板");
            wnd.listView.itemsSource = template.Nodes;
            wnd.rootVisualElement.Clear();
            wnd.CreateGUI();
            wnd.Focus();
            return wnd;
        }


        /// <summary>
        /// 挂钩处理 Imgui的 OnGUI
        /// </summary>
        private void Handle() {
            if (selected != null) {
                var data = selected.GetHosterData<XBaseData>();
                var fields = data.GetType().GetFields();

                for (int i = 0; i < fields.Length; i++) {
                    if (fields[i].Name.CompareTo("Index") == 0)
                        continue;

                    if (fields[i].FieldType == typeof(string)) {
                        fields[i].SetValue(data, EditorGUILayout.TextField(fields[i].Name, (string)fields[i].GetValue(data)));
                    } else if (fields[i].FieldType == typeof(float)) {
                        fields[i].SetValue(data, EditorGUILayout.FloatField(fields[i].Name, (float)fields[i].GetValue(data)));
                    } else if (fields[i].FieldType == typeof(int)) {
                        fields[i].SetValue(data, EditorGUILayout.IntField(fields[i].Name, (int)fields[i].GetValue(data)));
                    } else if (fields[i].FieldType == typeof(bool)) {
                        fields[i].SetValue(data, EditorGUILayout.Toggle(fields[i].Name, (bool)fields[i].GetValue(data)));
                    }
                }
            }
        }

        private void CreateGUI() {
            VisualElement root = rootVisualElement;

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TDTools/SkillEditorTemplate/SkillEditorTemplateEditorWindow.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            labelFromUXML.style.flexGrow = 1;
            root.Add(labelFromUXML);

            container = root.Q<IMGUIContainer>();
            container.onGUIHandler = Handle;

            ToolbarButton buttonLocal = root.Q<ToolbarButton>("ButtonSaveLocal");
            ToolbarButton buttonProject = root.Q<ToolbarButton>("ButtonSaveProject");

            buttonLocal.clicked += () => Save($"{Application.dataPath}/Editor/TDTools/SkillEditorTemplate/LocalTemplate/");
            buttonProject.clicked += () => Save($"{Application.dataPath}/Editor/TDTools/SkillEditorTemplate/ProjectTemplate/");

            listView = root.Q<ListView>();
            listView.makeItem = MakeItem;
            listView.bindItem = BindItem;
            if (buffer != null)
                listView.itemsSource = buffer.Nodes;
            else
                listView.itemsSource = null;
            listView.itemHeight = 48;

            listView.onSelectionChange += (obj) => {
                selected = buffer.Nodes[listView.selectedIndex];
            };

            VisualElement MakeItem() {
                VisualElement result = new VisualElement();

                Label labelName = new Label();
                labelName.name = "LabelName";
                labelName.style.height = 32;
                labelName.style.flexGrow = 100;

                result.Add(labelName);
                result.style.width = 300;
                result.style.height = 300;
                return result;
            }

            void BindItem(VisualElement ve, int index) {
                Label labelName = ve.Q<Label>("LabelName");
                labelName.text = buffer.Nodes[index].NodeName;
            }

            listView.Rebuild();
        }

    }
    #endregion
}

#region Old Ser
/// <summary>
/// 将目标节点序列化
/// 连线必须要起始、终止节点都在选中列表中才可以
/// </summary>
/// <param name="nodes">将要保存的节点</param>
/// <param name="EmptyHeader">是否需要空表头</param>
/// <returns></returns>
//public static MemoryStream Serilize(BaseSkillNode[] nodes, bool EmptyHeader = true) {
//    MemoryStream stream = new MemoryStream();
//    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
//    ns.Add(string.Empty, string.Empty);

//    HashSet<int> set = new HashSet<int>();

//    if (EmptyHeader) {
//        byte[] header = Encoding.UTF8.GetBytes("\t");
//        for (int i = 0; i < 5; i++) {
//            stream.WriteByte(header[0]);
//        }
//    }

//    byte[] nodeCountBytes = Encoding.ASCII.GetBytes($"{nodes.Length}");
//    stream.Write(nodeCountBytes, 0, nodeCountBytes.Length);

//    for (int i = 0; i < nodes.Length; i++) {
//        var x = nodes[i].GetHosterData<XBaseData>();
//        var t = nodes[i].nodeEditorData;

//        set.Add(t.NodeID);

//        byte[] nodeTypeName = Encoding.ASCII.GetBytes($"\t{nodes[i].GetType().AssemblyQualifiedName}\t");
//        stream.Write(nodeTypeName, 0, nodeTypeName.Length);

//        byte[] typeName = Encoding.ASCII.GetBytes($"{x.GetType().AssemblyQualifiedName}\t");
//        stream.Write(typeName, 0, typeName.Length);

//        MemoryStream ms1 = new MemoryStream();
//        XmlTextWriter xw1 = new XmlTextWriter(ms1, Encoding.UTF8);
//        XmlSerializer xml = new XmlSerializer(x.GetType());
//        //xml.Serialize(stream, x, ns);
//        xml.Serialize(xw1, x, ns);
//        ms1.Position = 3;
//        ms1.CopyTo(stream);

//        byte[] typeName2 = Encoding.ASCII.GetBytes($"\t{t.GetType().AssemblyQualifiedName}\t");
//        stream.Write(typeName2, 0, typeName2.Length);

//        MemoryStream ms2 = new MemoryStream();
//        XmlTextWriter xw2 = new XmlTextWriter(ms2, Encoding.UTF8);

//        XmlSerializer xml2 = new XmlSerializer(t.GetType());
//        //xml2.Serialize(stream, t, ns);
//        xml2.Serialize(xw2, t, ns);
//        ms2.Position = 3;
//        ms2.CopyTo(stream);
//    }

//    List<SkillConnectionData> connections = new List<SkillConnectionData>();
//    for (int i = 0; i < nodes.Length; i++) {
//        for (int j = 0; j < nodes[i].pinList.Count; j++) {
//            if (nodes[i].pinList[j].HasConnection()) {
//                for (int k = 0; k < nodes[i].pinList[j].connections.Count; k++) {
//                    var start = nodes[i].pinList[j].connections[k].connectStart;
//                    var end = nodes[i].pinList[j].connections[k].connectEnd;
//                    if (set.Contains(start.GetNode<BaseSkillNode>().nodeEditorData.NodeID) && set.Contains(end.GetNode<BaseSkillNode>().nodeEditorData.NodeID))
//                        connections.Add(new SkillConnectionData { 
//                            BeginNodeID = start.GetNode<BaseSkillNode>().nodeEditorData.NodeID,
//                            BeginPinID = start.pinID,
//                            EndNodeID = end.GetNode<BaseSkillNode>().nodeEditorData.NodeID,
//                            EndPinID = end.pinID
//                        });
//                }
//            }
//        }
//    }

//    byte[] connectionCountBytes = Encoding.ASCII.GetBytes($"\t");
//    stream.Write(connectionCountBytes, 0, connectionCountBytes.Length);
//    XmlSerializer xxx = new XmlSerializer(typeof(List<SkillConnectionData>), String.Empty);
//    xxx.Serialize(stream, connections, ns);

//    stream.Position = 0;
//    return stream;
//}

/// <summary>
/// 反序列化
/// </summary>
/// <param name="stream">读取模板的流</param>
/// <param name="path">传给模板的path,其实没有用</param>
/// <returns></returns>
//public static SkillEditorTemplate Deserilize(Stream stream, string path = "") {
//    string ReadString() {
//        List<byte> b = new List<byte>();
//        while (stream.CanRead) {
//            byte by = (byte)stream.ReadByte();

//            if (by == 9)
//                break;

//            b.Add(by);
//        }
//        return Encoding.UTF8.GetString(b.ToArray());
//    }

//    //t没有用，这里只不过是把表头读掉
//    SkillEditorTemplate result = new SkillEditorTemplate {
//        TemplateName = ReadString(),
//        TemplateType = ReadString(),
//        TemplateAuthor = ReadString(),
//        TemplateDescription = ReadString(),
//        TemplateNodeCount = ReadString(),
//        path = path
//    };

//    byte[] bytes = new byte[stream.Length - stream.Position];
//    for (int i = 0; i < bytes.Length; i++) {
//        bytes[i] = (byte)stream.ReadByte();
//    }

//    string[] lines = Encoding.UTF8.GetString(bytes).Split('\t');


//    //Debug.Log(s);
//    Dictionary<int, BaseSkillNode> dic = new Dictionary<int, BaseSkillNode>();
//    int nodeCount = int.Parse(lines[0]);
//    BaseSkillNode[] nodes = new BaseSkillNode[nodeCount];
//    for (int i = 0; i < nodeCount; i++) {

//        Type nodeType = Type.GetType(lines[i * 5 + 1]);
//        Type dataType = Type.GetType(lines[i * 5 + 2]);
//        // i + 2 是data
//        XmlSerializer se1 = new XmlSerializer(dataType, string.Empty);

//        MemoryStream ms1 = new MemoryStream(Encoding.UTF8.GetBytes(lines[i * 5 + 3]));
//        XmlTextReader xr1 = new XmlTextReader(ms1);
//        var data = se1.Deserialize(xr1);

//        Type nodeDataType = Type.GetType(lines[i * 5 + 4]);
//        XmlSerializer se2 = new XmlSerializer(nodeDataType, string.Empty);
//        MemoryStream stream2 = new MemoryStream(Encoding.UTF8.GetBytes(lines[i * 5 + 5]));
//        XmlTextReader xr2 = new XmlTextReader(stream2);
//        var nodeData = se2.Deserialize(xr2);
//        // i + 4 是node data

//        BaseSkillNode node = (BaseSkillNode)Activator.CreateInstance(nodeType);
//        node.SetPublicField("HosterData", data);
//        node.nodeEditorData = (NodeConfigData)nodeData;
//        //Debug.Log($"{nodeType} {dataType} {nodeDataType}");
//        nodes[i] = node;

//        dic[node.nodeEditorData.NodeID] = node;
//    }

//    MemoryStream ss = new MemoryStream(Encoding.ASCII.GetBytes(lines[nodeCount * 5 + 1]));
//    List<SkillConnectionData> connections;
//    XmlSerializer x = new XmlSerializer(typeof(List<SkillConnectionData>), String.Empty);
//    connections = (List<SkillConnectionData>)x.Deserialize(ss);

//    result.Nodes = nodes;
//    result.Connections = connections;

//    return result;
//}   
#endregion
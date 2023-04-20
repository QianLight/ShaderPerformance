using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;
using BluePrint;
using EditorNode;
using EcsData;
using System.Xml.Serialization;
using System.IO;
using A;
using System.Reflection;
using System.Text;

namespace TDTools.SkillEditorTemplate {

    /// <summary>
    /// 技能编辑器 模板工具 主界面
    /// </summary>
    public class SkillEditorTemplateWindow : EditorWindow {

        #region 变量

        ListView localListView;
        ListView projectListView;

        List<SkillEditorTemplate> listLocal;
        List<SkillEditorTemplate> listProject;

        #endregion

        #region 模板操作

        /// <summary>
        /// 将目前再技能编辑器中选中的节点序列化，单独显示一个技能编辑器界面，并导入到模板编辑界面 ，以便于保存
        /// </summary>
        public static void AddToTemplateEditor() {
            SkillEditor wnd = (SkillEditor)GetWindow(typeof(SkillEditor));
            if (wnd.CurrentGraph.selectNodeList != null && wnd.CurrentGraph.selectNodeList.Count > 0) {
                BaseSkillNode[] t = new BaseSkillNode[wnd.CurrentGraph.selectNodeList.Count];
                for (int i = 0; i < wnd.CurrentGraph.selectNodeList.Count; i++)
                    t[i] = ((BaseSkillNode)wnd.CurrentGraph.selectNodeList[i]);

                SkillEditorTemplate tt = new SkillEditorTemplate();
                tt.Nodes = t;
                tt.TemplateName = "Test Template";
                tt.TemplateType = "test type";
                tt.TemplateNodeCount = t.Length.ToString();
                //因为SkillNode没有拷贝程序，直接序列化再反序列化来复制
                SkillEditorTemplate template = SkillEditorTemplateEditorWindow.Deserilize(SkillEditorTemplateEditorWindow.NewSerilize(tt));
                SkillEditorTemplateEditorWindow.ShowWindow(ref template);
                OpenTemplateGraph(template);
            }
        }


        /// <summary>
        /// 将模板文件导入技能编辑器
        /// </summary>
        public void ImportTemplate() {
            if (localListView.selectedIndex == -1 && projectListView.selectedIndex == -1)
                return;
            SkillEditorTemplate template;
            if (localListView.selectedIndex != -1)
                template = listLocal[localListView.selectedIndex];
            else
                template = listProject[projectListView.selectedIndex];

            AddToCurrentGraph(template);
        }

        /// <summary>
        /// 删除模板文件
        /// </summary>
        public void DeleteTemplate() {
            if (localListView.selectedIndex == -1 && projectListView.selectedIndex == -1)
                return;

            SkillEditorTemplate template;
            if (localListView.selectedIndex != -1) {
                template = listLocal[localListView.selectedIndex];
                if (!EditorUtility.DisplayDialog("删除模板", $"确认要删除模板{template.TemplateName}", "删除", "取消")) {
                    return;
                }
                listLocal.RemoveAt(localListView.selectedIndex);
                localListView.Refresh();
            } else {
                template = listProject[projectListView.selectedIndex];
                if (!EditorUtility.DisplayDialog("删除模板", $"确认要删除模板{template.TemplateName}", "删除", "取消")) {
                    return;
                }
                listProject.RemoveAt(projectListView.selectedIndex);
                projectListView.Refresh();
            }

            try {
                File.Delete(template.path);
                File.Delete($"{template.path.Remove(template.path.Length - 4)}.meta");
            } catch { 
            }
        }

        /// <summary>
        /// 将列表中选定的模板文件导入技能编辑器和模板编辑器
        /// </summary>
        public void EditTemplate() {
            if (localListView.selectedIndex == -1 && projectListView.selectedIndex == -1)
                return;

            SkillEditorTemplate template;
            if (localListView.selectedIndex != -1)
                template = listLocal[localListView.selectedIndex];
            else
                template = listProject[projectListView.selectedIndex];
            //var nodes = SkillEditorTemplateEditorWindow.Deserilize(template.path);
            OpenTemplateGraph(template);

            var wnd = SkillEditorTemplateEditorWindow.ShowWindow(ref template);
            wnd.SetTemplate(template);
        }

        /// <summary>
        /// 将模板用读取到技能编辑器的图里
        /// </summary>
        /// <param name="template">要读取的模板</param>
        /// <param name="position">模板的时间插入位置</param>
        /// <param name="yPos">模板的Y轴插入位置</param>
        public static void OpenTemplateGraph(SkillEditorTemplate template) {


            SkillGraph graph = new SkillGraph();
            SkillEditor wnd = (SkillEditor)GetWindow(typeof(SkillEditor));
            graph.Init(wnd);
            graph.configData.Length = 360;
            for (int i = 0; i < template.Nodes.Length; i++) {
                //Type type = template.Nodes[i].GetType();
                //BaseSkillNode a = (BaseSkillNode)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                var x = template.Nodes[i].GetHosterData<XBaseData>();
                var t = template.Nodes[i].nodeEditorData;
                //a.nodeEditorData = t.Copy();
                template.Nodes[i].SetPublicField("HosterData", x);
                //a.Init(graph, t.Position + new Vector2(position, yPos));
                template.Nodes[i].Init(graph, t.Position);
                graph.AddNode(template.Nodes[i]);
                graph.graphConfigData.NodeConfigList.Add(template.Nodes[i].nodeEditorData);
            }

            for (int i = 0; i < template.Connections.Count; i++) {
                var start = graph.GetNode(template.Connections[i].BeginNodeID);
                var startPin = start.GetPin(template.Connections[i].BeginPinID);

                var end = graph.GetNode(template.Connections[i].EndNodeID);
                var endPin = end.GetPin(template.Connections[i].EndPinID);

                startPin.AddConnection(new BlueprintConnection(startPin, endPin));
                endPin.AddReversceConnection(new BlueprintReverseConnection(endPin, startPin));
            }

            wnd.OpenGraph(graph);
        }

        /// <summary>
        /// 将模板导入到技能编辑器中目前打开的技能图里
        /// </summary>
        /// <param name="template">需要导入的模板</param>
        public void AddToCurrentGraph(SkillEditorTemplate template) {
            SkillEditor wnd = (SkillEditor)GetWindow(typeof(SkillEditor));
            //var nodes = SkillEditorTemplateEditorWindow.Deserilize(template.path);

            float position = rootVisualElement.Q<Slider>("TemplateTimePosition").value;
            float yPos = rootVisualElement.Q<Slider>("TemplateYPosition").value;

            Dictionary<int, int> indexTable = new Dictionary<int, int>();

            for (int i = 0; i < template.Nodes.Length; i++) {
                Type type = template.Nodes[i].GetType();
                BaseSkillNode a = (BaseSkillNode)type.GetConstructor(new Type[0]).Invoke(new object[0]);
                XBaseData x = (XBaseData)template.Nodes[i].GetHosterData<XBaseData>().Clone();
                var t = template.Nodes[i].nodeEditorData.Copy();
                
                if (x.TimeBased)
                    x.At += position / ((SkillGraph)wnd.CurrentGraph).FrameCount;
                if (x.TimeBased && x.At > ((SkillGraph)wnd.CurrentGraph).configData.Length)
                    Debug.Log("导入的节点超过技能时常！这可能会导致点击技能节点时左右横跳");

                a.nodeEditorData = t;
                int oldId = a.nodeEditorData.NodeID;
                a.nodeEditorData.NodeID = 0;
                a.SetPublicField("HosterData", x);
                a.Init(wnd.CurrentGraph, t.Position + new Vector2(position * 10, yPos));
                wnd.CurrentGraph.AddNode(a);
                wnd.CurrentGraph.graphConfigData.NodeConfigList.Add(a.nodeEditorData);
                indexTable[oldId] = a.nodeEditorData.NodeID;
            }

            for (int i = 0; i < template.Connections.Count; i++) {
                var start = wnd.CurrentGraph.GetNode(indexTable[template.Connections[i].BeginNodeID]);
                var startPin = start.GetPin(template.Connections[i].BeginPinID);

                var end = wnd.CurrentGraph.GetNode(indexTable[template.Connections[i].EndNodeID]);
                var endPin = end.GetPin(template.Connections[i].EndPinID);

                startPin.AddConnection(new BlueprintConnection(startPin, endPin));
                endPin.AddReversceConnection(new BlueprintReverseConnection(endPin, startPin));
            }
        }

        #endregion

        #region 界面操作

        /// <summary>
        /// 显示主界面
        /// </summary>
        [MenuItem("Tools/TDTools/技能工具/技能模板")]
        public static void ShowWindow() {
            SkillEditorTemplateWindow wnd = GetWindow<SkillEditorTemplateWindow>();
            wnd.titleContent = new GUIContent("技能模板");
            //wnd.position = new Rect(256, 256, 512, 512);
            wnd.Focus();
        }

        /// <summary>
        /// 刷新一下列表中的模板文件
        /// </summary>
        public void Refresh() {
            listLocal = new List<SkillEditorTemplate>();
            listProject = new List<SkillEditorTemplate>();
            projectListView = rootVisualElement.Q<ListView>("ProjectListView");
            localListView = rootVisualElement.Q<ListView>("LocalListView");
            if (projectListView == null) {
                CreateGUI();
                return;
            }
            projectListView.itemsSource = listProject;
            localListView.itemsSource = listLocal;

            ReadList($"{Application.dataPath}/Editor/TDTools/SkillEditorTemplate/LocalTemplate", listLocal, localListView);
            ReadList($"{Application.dataPath}/Editor/TDTools/SkillEditorTemplate/ProjectTemplate", listProject, projectListView);

            void ReadList(string path, List<SkillEditorTemplate> list, ListView listView) {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                FileInfo[] files = directoryInfo.GetFiles();
                for (int i = 0; i < files.Length; i++) {
                    if (files[i].Extension.CompareTo(".xml") != 0) {
                        continue;
                    }
                    try {
                        using FileStream stream = new FileStream(files[i].FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var t = SkillEditorTemplateEditorWindow.Deserilize(stream);
                        t.path = files[i].FullName;
                        list.Add(t);
                    } catch (Exception e) {
                        Debug.Log(e.Message);
                    }
                }
                listView.Refresh();
                listView.style.height = 48 * list.Count;
            }
        }

        private void OnFocus() {
            Refresh();
        }

        public void CreateGUI() {
            var root = rootVisualElement;
            root.Clear();

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TDTools/SkillEditorTemplate/SkillEditorTemplateWindow.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            VisualTreeAsset itemVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/TDTools/SkillEditorTemplate/SkillEditorTemplateMenuTemplateListItem.uxml");


            projectListView = root.Q<ListView>("ProjectListView");
            localListView = root.Q<ListView>("LocalListView");

            listLocal = new List<SkillEditorTemplate>();
            listProject = new List<SkillEditorTemplate>();

            TextField textFieldAuthor = root.Q<TextField>("TFTemplateAuthor");
            TextField textFieldDescription = root.Q<TextField>("TFTemplateDescription");
            TextField textFieldNodes = root.Q<TextField>("TFTemplateNodes");

            Label labelTemplateName = root.Q<Label>("LabelTemplateName");

            VisualElement MakeItem() {
                VisualElement ve = itemVisualTree.Instantiate();
                return ve;
            }

            void BindItemProject(VisualElement ve, int index) {

                Label labelName = ve.Q<Label>("TemplateName");
                Label labelType = ve.Q<Label>("TemplateType");
                Label labelNodeCount = ve.Q<Label>("TemplateNodeCount");
                Label labelDescription = ve.Q<Label>("TemplateDes");

                labelName.text = listProject[index].TemplateName;
                labelType.text = listProject[index].TemplateType;
                labelNodeCount.text = listProject[index].TemplateNodeCount;
                labelDescription.text = listProject[index].TemplateDescription;
            }

            void BindItemLocal(VisualElement ve, int index) {
                Label labelName = ve.Q<Label>("TemplateName");
                Label labelType = ve.Q<Label>("TemplateType");
                Label labelNodeCount = ve.Q<Label>("TemplateNodeCount");
                Label labelDescription = ve.Q<Label>("TemplateDes");

                labelName.text = listLocal[index].TemplateName;
                labelType.text = listLocal[index].TemplateType;
                labelNodeCount.text = listLocal[index].TemplateNodeCount;
                labelDescription.text = listLocal[index].TemplateDescription;
            }

            projectListView.makeItem = MakeItem;
            projectListView.bindItem = BindItemProject;
            projectListView.itemsSource = listProject;
            projectListView.itemHeight = 48;
            projectListView.onSelectionChange += obj => {
                if (projectListView.selectedIndex == -1)
                    return;
                localListView.ClearSelection();
                labelTemplateName.text = listProject[projectListView.selectedIndex].TemplateName;
                textFieldAuthor.value = listProject[projectListView.selectedIndex].TemplateAuthor;
                textFieldDescription.value = listProject[projectListView.selectedIndex].TemplateDescription;
                //FileStream stream = new FileStream(listProject[projectListView.selectedIndex].path, FileMode.Open);
                //SkillEditorTemplate t = SkillEditorTemplateEditorWindow.Deserilize(stream);
                //t.path
                //stream.Close();
                textFieldNodes.value = "";
                for (int i = 0; i < listProject[projectListView.selectedIndex].Nodes.Length; i++)
                    textFieldNodes.value = $"{textFieldNodes.value}{listProject[projectListView.selectedIndex].Nodes[i].NodeName} \n";
            };


            localListView.makeItem = MakeItem;
            localListView.bindItem = BindItemLocal;
            localListView.itemsSource = listLocal;
            localListView.itemHeight = 48;
            localListView.onSelectionChange += obj => {
                if (localListView.selectedIndex == -1)
                    return;
                projectListView.ClearSelection();
                labelTemplateName.text = listLocal[localListView.selectedIndex].TemplateName;
                textFieldAuthor.value = listLocal[localListView.selectedIndex].TemplateAuthor;
                textFieldDescription.value = listLocal[localListView.selectedIndex].TemplateDescription;
                //FileStream stream = new FileStream(listLocal[localListView.selectedIndex].path, FileMode.Open);
                //SkillEditorTemplate t = SkillEditorTemplateEditorWindow.Deserilize(stream);
                //stream.Close();
                textFieldNodes.value = "";
                for (int i = 0; i < listLocal[localListView.selectedIndex].Nodes.Length; i++)
                    textFieldNodes.value = $"{textFieldNodes.value}{listLocal[localListView.selectedIndex].Nodes[i].NodeName} \n";
            };

            ToolbarButton ButtonTestLoad = root.Q<ToolbarButton>("ButtonOpenSkillEditor");
            ButtonTestLoad.clicked += ()=> {
                SkillEditor wnd = (SkillEditor)GetWindow(typeof(SkillEditor));
                wnd.Focus();
            };

            ToolbarButton ButtonOpenTemplateFolder = root.Q<ToolbarButton>("ButtonOpenTemplateFolder");
            ButtonOpenTemplateFolder.clicked += () => {
                System.Diagnostics.Process.Start($"{Application.dataPath}/Editor/TDTools/SkillEditorTemplate/");
            };

            Button buttonCreateTemplate = root.Q<Button>("ButtonCreateTemplate");
            Button buttonImportTemplate = root.Q<Button>("ButtonImportTemplate");
            Button buttonEditTemplate = root.Q<Button>("ButtonEditTemplate");
            Button buttonDeleteTemplate = root.Q<Button>("ButtonDeleteTemplate");

            buttonCreateTemplate.clicked += AddToTemplateEditor;
            buttonImportTemplate.clicked += ImportTemplate;
            buttonEditTemplate.clicked += EditTemplate;
            buttonDeleteTemplate.clicked += DeleteTemplate;

            Refresh();
        }
    }
    #endregion
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Xml;
using BluePrint;
using CFUtilPoolLib;

namespace MonsterEditor
{
    class MonsterEditor:EditorWindow
    {
        public static MonsterEditor Instance { get; set; }
        public static GUILayoutOption[] ToolbarButton = new GUILayoutOption[] { GUILayout.Width(75f) };
        private static int layoutWidth = 3;

        [MenuItem("Window/Monster")]
        public static void InitWindow()
        {
            var window = GetWindowWithRect<MonsterEditor>(new Rect(0,0,800,600));
            window.titleContent = new GUIContent("MonsterEditor");
            window.Show();
            Instance = window;
        }

        private string curFilePath;
        private bool presentation;

        private XEntityStatistics.RowData staData;
        private XEntityPresentation.RowData preData;

        private void OnGUI()
        {
            DrawToolbarArea();
            DrawDataArea();
        }

        private void OnEnable()
        {
            Instance = this;
            curFilePath = string.Empty;
            presentation = true;
        }

        private void Update()
        {
            Instance = this;
        }

        private void DrawDataArea()
        {
            var type = presentation?typeof(XEntityPresentation.RowData):typeof(XEntityStatistics.RowData);
            var members=type.GetMembers();
            int widthCount =1;
            for(var i=0;i<members.Length;i++)
            {              
                if(members[i].MemberType==System.Reflection.MemberTypes.Field)
                {
                    if (widthCount == 1)
                        GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent(members[i].Name));
                    widthCount += 1;
                    if(widthCount>layoutWidth)
                    {
                        widthCount = 1;
                        GUILayout.EndHorizontal();
                    }
                }
            }
        }

        private void DrawToolbarArea()
        {
            GUILayout.BeginHorizontal(BluePrint.BlueprintStyles.Toolbar());
            if (GUILayout.Button("New", ToolbarButton))
                CreateNewFile();
            if (GUILayout.Button("Load", ToolbarButton))
                LoadFile();
            if (GUILayout.Button("Save", ToolbarButton))
                SaveFile();
            GUILayout.Label(string.Format("当前选表:{0}", curFilePath == string.Empty ? 
                string.Empty : presentation ? "XEntityPresentation" : "XEntityStatistics"), new GUILayoutOption[] { GUILayout.Width(200) });
            GUILayout.Label(string.Format("文件路径:{0}",curFilePath), new GUILayoutOption[] { GUILayout.Width(250) });           
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void CreateNewFile()
        {
            
        }
        
        private void LoadFile()
        {
            curFilePath = EditorUtility.OpenFilePanel("选择文件", Application.dataPath+"/Table/MonsterConfig", "xml");
            if (curFilePath.EndsWith("sta"))
            {
                staData = DataIO.DeserializeData<XEntityStatistics.RowData>(curFilePath);
                presentation = false;
            }
            else if (curFilePath.EndsWith("pre"))
            {
                preData = DataIO.DeserializeData<XEntityPresentation.RowData>(curFilePath);
                presentation = true;
            }
            else
                ShowNotification(new GUIContent("不支持的后缀文件"),3);
        }

        /// <summary>
        /// 区分两个表的内容，命令格式为xxxx_id.sta or pre,pre表id是presentid，sta表id是id,其中xxxx自定义
        /// </summary>
        private void SaveFile()
        {
            curFilePath = EditorUtility.SaveFilePanel("保存文件", Application.dataPath + "/Table/MonsterConfig", 
                string.Format("_{0}",presentation?preData.PresentID:staData.ID), presentation ? "pre" : "sta");
        }
    }
}

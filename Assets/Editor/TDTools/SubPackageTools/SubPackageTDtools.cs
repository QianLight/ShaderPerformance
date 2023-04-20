using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.ComponentModel;
using System.IO;
using System.Security.Policy;
using System.Text;
using LevelEditor;
using TDTools;
using UnityEditor.UIElements;
using UnityEngine.CFUI;
using UnityEngine.UIElements;
using System.Linq;

namespace TDTools
{
    public class SubPackageTdTools: EditorWindow
    {

        private string FullFileNamePath = "";
        private List<int> SpawnIDList = new List<int>();
        private List<int> SpawnIDList1;
        private List<uint> PresentIDList;
        private List<string> ArtLocation;
        private List<string> ArtFileLocation;
        private LevelEditorData fullData;
        private EditorConfigData editorConfigData;
        private FileSelectorData SelectFsd;
        private List<LevelWordReplaceNodeData> NodeList = new List<LevelWordReplaceNodeData>();
        private ListView listView;
        public static string TargetFile;

        [MenuItem("Tools/TDTools/通用工具/分包工具")]
        static void Init()
        {
            //显示窗口
            SubPackageTdTools SubPackageTdTool = (SubPackageTdTools)EditorWindow.GetWindow(typeof(SubPackageTdTools), false, "SubPackageTdTool", true);
            SubPackageTdTool.Show();
        }
        private void OnEnable()
        {
            TargetFile = $"{Application.dataPath}/Editor/TDTools/SubPackageTools/TargetFile.txt";
        }

        private void OnGUI()
        {
            //filenamePath输入
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("FileNamePath", FullFileNamePath);
            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                FullFileNamePath = EditorUtility.OpenFilePanel("选择目录", Application.dataPath + "/BundleRes/Table/Level/", "cfg");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("分包"))
            {
                GetSpawnID();
                GetFilePath();
                FindFiles();
                WriteFiles();
                //SpawnIDList.Clear();
                //ArtLocation.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }

        public void GetSpawnID()
        {
            fullData = DataIO.DeserializeData<LevelEditorData>(FullFileNamePath);
            editorConfigData = DataIO.DeserializeData<EditorConfigData>(FullFileNamePath.Replace(".cfg", ".ecfg"));
            foreach (var graph in fullData.GraphDataList)
            {
                //var editorData = editorConfigData.GetGraphConfigByID(graph.graphID);
                foreach (var node in graph.WaveData)
                {
                    //var nodeEditorData = editorData.GetConfigDataByID(node.NodeID);
                    SpawnIDList.Add(node.SpawnID);
                }
            }
            //SpawnIDList1 = (List<int>)SpawnIDList.Distinct();
        }

        public void GetFilePath()
        {
            ArtLocation.Clear();
            XEntityStatisticsReader.Reload();
            XEntityPresentationReader.Reload();
            List<uint> PresentIDList1;
            for (int i = 0; i < SpawnIDList.Count; i++)
            {
                uint ID = (uint)SpawnIDList[i];
                PresentIDList.Add(XEntityStatisticsReader.GetPresentid(ID));
            }
            PresentIDList1 = PresentIDList;
            for (int i = 0; i < PresentIDList1.Count; i++)
            {
                uint ID = PresentIDList1[i];
                ArtLocation.Add($"{Application.dataPath}/BundleRes/Animation/" + XEntityPresentationReader.GetAnimLocationByPresentId(ID));
            }
            //list去重
            for (int i = 0; i < ArtLocation.Count; i++)
            {
                for (int j = ArtLocation.Count - 1; j > i; j--)
                {
                    if (ArtLocation[i] == ArtLocation[j])
                    {
                        ArtLocation.Remove(ArtLocation[j]);
                    }
                }
            }
        }
        public void FindFiles()
        {
            //查找路径文件
            for (int i = 0; i < ArtLocation.Count; i++)
            {
                var list = Directory.GetFiles(ArtLocation[i], "*", SearchOption.AllDirectories);
                for (int j = 0; j < list.Length; j++)
                {
                    ArtFileLocation.Add(list[j]);
                }
            }
            
        }
        public void WriteFiles()
        {
            StringBuilder sb = new StringBuilder();
            //StringBuilder sb2 = new StringBuilder();
            sb.Clear();
            //sb2.Clear();
            //ArtLocation.Distinct();
            for (int i = 0; i < ArtFileLocation.Count; i++)
            {
                sb.AppendLine(ArtFileLocation[i]);
            }
            using (var file = File.Open(TargetFile, FileMode.Create))
            {
                var info = Encoding.UTF8.GetBytes(sb.ToString());
                file.Write(info, 0, info.Length);
            }
        }
    }
}

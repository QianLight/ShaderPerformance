using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace TableEditor
{
    [System.Serializable]
    public class TableConfigEditor : EditorWindow
    {
        public static TableConfigEditor Instance { get; set; }

        public TableCreateConfigData curData = new TableCreateConfigData();
        private TableConfig config = new TableConfig();
        private TableConfigKey keyConfig = new TableConfigKey();
        public TableConfigData curConfig = new TableConfigData();
        public string curFilePath = "";
        private string curTableName = "";
        private Vector2 scrollPos;
        private bool isFirstStep = true;
        private List<bool> toggleList = new List<bool>();

        public static void InitWindow()
        {
            TableConfigEditor window = EditorWindow.GetWindowWithRect<TableConfigEditor>(new Rect(0, 0, 400, 500));//宽 高
            window.titleContent = new GUIContent("TableConfigEditor");
            window.Show();
            Instance = window;
            Instance.LoadExcel();
        }
        public static void InitWindow(string filePath)
        {
            TableConfigEditor window = EditorWindow.GetWindowWithRect<TableConfigEditor>(new Rect(0, 0, 400, 500));//宽 高
            window.titleContent = new GUIContent("TableConfigEditor");
            window.Show();
            Instance = window;
            Instance.curFilePath = filePath;
            Instance.LoadExcel();
        }
        private void OnGUI()
        {
            DrawConfigEditor();
        }

        private void DrawConfigEditor()
        {
            GUILayout.Box(new GUIContent("当前打开文件:  " + curTableName));

            if(isFirstStep)
            {
                GUILayout.Box(new GUIContent("请选择该表的索引值:  "));
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                for (var i = 0; i < curData.nameList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent(curData.nameList[i])))
                    {    
                        isFirstStep = false;
                        curConfig.keyName = curData.nameList[i];
                    }
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Box(new GUIContent("请选择该表需要在首页显示的序列:  "));
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                for (var i = 0; i < curData.nameList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    curData.toggleList[i] = EditorGUILayout.ToggleLeft(new GUIContent(curData.nameList[i]), curData.toggleList[i]);
                    EditorGUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                if(GUILayout.Button(new GUIContent("完成配置")))
                {
                    CreateConfig();
                }
            }
        }

        private void CreateConfig()
        {
            for (var i = 0; i < curData.nameList.Count; i++)
            {
                if (curData.toggleList[i])
                    curConfig.tagList.Add(curData.nameList[i]);
            }

            //xml文件不存在
            if (!File.Exists(Application.dataPath + TableEditor.TableConfigPath))
            {
                config.configList.Add(curConfig);

                DataIO.SerializeData<TableConfig>(Application.dataPath + TableEditor.TableConfigPath, config);
            }
            else
            {
                config = DataIO.DeserializeData<TableConfig>(Application.dataPath + TableEditor.TableConfigPath);

                bool isRemain = false;

                for(int i=0;i<config.configList.Count;i++)
                {
                    if(config.configList[i].tableName==curConfig.tableName)
                    {
                        config.configList[i] = curConfig;
                        isRemain = true;
                        DataIO.SerializeData<TableConfig>(Application.dataPath + TableEditor.TableConfigPath, config);
                    }
                }
                if (isRemain == false)  //xml文件没有该配置
                {
                    config.configList.Add(curConfig);

                    DataIO.SerializeData<TableConfig>(Application.dataPath + TableEditor.TableConfigPath, config);
                }
            }
            TableEditor.Instance.ShowNotice("配置成功");
            TableEditor.Instance.ReadConfig();
            TableEditor.Instance.LoadTableData(curFilePath);
            Instance.Close();
        }
          
        private void LoadExcel()
        {
            if(string.IsNullOrEmpty(curFilePath))
                curFilePath = EditorUtility.OpenFilePanel("选取表格以创建配置", TableEditor.TablePath, "txt");
            if (!string.IsNullOrEmpty(curFilePath))
            {
                curTableName = curFilePath.Substring(curFilePath.LastIndexOf('/') + 1).Replace(".txt", string.Empty);
                curConfig.tableName= curTableName;
                TableReader.ReadTableByFileStream(curFilePath, ref curData);
            }
            InitialData();
            LoadXml();
        }
        public void LoadXml()
        {
            keyConfig= DataIO.DeserializeData<TableConfigKey>(Application.dataPath + TableEditor.TableKeyConfigPath);
            if (keyConfig.tableKey.ContainsKey(curTableName))
            {
                isFirstStep = false;
                curConfig.keyName = keyConfig.tableKey[curTableName];
            }
        }

        private void InitialData()
        {

            for (var i = 0; i < curData.nameList.Count; i++)
            {
                toggleList.Add(false);
                curData.toggleList.Add(false);
            }
        }
    }
}
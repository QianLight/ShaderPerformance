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
    public class TableDataLoader : EditorWindow
    {

        public static TableDataLoader Instance { get; set; }


        public static void InitWindow()
        {
            TableDataLoader window = EditorWindow.GetWindowWithRect<TableDataLoader>(new Rect(0, 0, 400, 500));//宽 高
            window.titleContent = new GUIContent("读取表格");
            window.Show();
            Instance = window;
        }
        private void OnGUI()
        {
            DrawWindow();
        }

        private void DrawWindow()
        {
            if(TableEditor.Instance.fullConfig!=null)
            {
                for (var i = 0; i < TableEditor.Instance.fullConfig.configList.Count; i++)
                {
                    if (GUILayout.Button(new GUIContent(TableEditor.Instance.fullConfig.configList[i].tableName)))
                    {
                        string filePath = TableEditor.Instance.GetTableFilePath(TableEditor.Instance.fullConfig.configList[i].tableName);                       
                        TableEditor.Instance.LoadTableData(filePath);
                    }
                }
            }
        }
    }
}
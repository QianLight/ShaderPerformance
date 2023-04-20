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
    class TableTempDataWindow : EditorWindow
    {
        public static TableTempDataWindow Instance { get; set; }

        public static void InitWindow()
        {
            TableTempDataWindow window = EditorWindow.GetWindow<TableTempDataWindow>();
            window.titleContent = new GUIContent("选择要清除的缓存");
            window.Show();
            Instance = window;
        }
        private void OnGUI()
        {
            DrawTempData();
        }
        private void DrawTempData()
        {
            var path = Application.dataPath + TableEditor.XmlPath;
            var list = Directory.GetFiles(path, "*.txt", SearchOption.TopDirectoryOnly);
            if (!string.IsNullOrEmpty(list[0]))
            {
                for (var i = 0; i < list.Length; i++)
                {
                    if (GUILayout.Button(new GUIContent(Path.GetFileName(list[i]))))
                        ClearSingleTemp(list[i]);
                }
            }
        }
        public void ClearSingleTemp(string tempDataPath)
        {
            TableEditor.Instance.tempFullData.Reset();
            string tableName = tempDataPath.Substring(tempDataPath.LastIndexOf('/') + 1);
            DataIO.SerializeData<TableTempFullData>(Application.dataPath + TableEditor.XmlPath + tableName, TableEditor.Instance.tempFullData);
            TableEditor.Instance.LoadTableData(TableEditor.Instance.curFilePath);

            TableEditor.Instance.RefreshListView();
            TableEditor.Instance.SetTablebarButtonChanged(tableName.Replace(".txt", string.Empty),true);
            TableEditor.Instance.ShowNotice("清除成功！");
        }
    }
}
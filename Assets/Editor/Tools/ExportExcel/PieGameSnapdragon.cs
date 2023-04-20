using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Quantify
{
    public partial class PieGameExcel
    {
        private static string Excel_Snapdragon_Tag = "Excel_Snapdragon_Tag";

        public static string Excel_Snapdragon
        {
            get { return PlayerPrefs.GetString(Excel_Snapdragon_Tag); }
            set { PlayerPrefs.SetString(Excel_Snapdragon_Tag, value); }
        }

        private void InitConfigData_Snapdragon()
        {
            
        }

        void OnGUI_Snapdragon()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Snapdragon:" + Excel_Snapdragon, new GUILayoutOption[] {GUILayout.Width(450f)});
            if (GUILayout.Button("Snapdragon", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFilePanel("选择需要显示的数据", Excel_Snapdragon, "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    Function_Snapdragon(path);
                    Excel_Functions = path;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void Function_Snapdragon(string path)
        {
            List<string[]> _tables = CsvTool.LoadFile(path);
        }
    }
}

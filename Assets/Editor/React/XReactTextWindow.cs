#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    class XReactTextWindow : EditorWindow
    {
        XReactEntranceWindow window;
        string text = null;
        Vector2 posLeft;
        public void Init(XReactEntranceWindow parent)
        {
            window = parent;
        }

        string GetText(string pathwithname)
        {
            string text = System.IO.File.ReadAllText(pathwithname);
            return text;
        }

        private string GetDataFileWithPath()
        {
            return window.ReactDataSet.ReactDataExtra.ScriptPath + window.ReactDataSet.ReactDataExtra.ScriptFile + ".bytes";
        }

        void OnGUI()
        {
            posLeft = GUILayout.BeginScrollView(posLeft, true, true);
            {
                if (text == null)
                {
                    text = GetText(GetDataFileWithPath());
                }
                if (text != null)
                {
                    GUI.color = Color.black;
                    GUILayout.Label(text);
                }
            }
            GUILayout.EndArea();
        }
    }
}

#endif
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class BuildEmojiBundle : EditorWindow
    {
        private string m_dir = string.Empty;

        [MenuItem("Tools/Build/BuidEmoji")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(BuildEmojiBundle), true, "BuildEmojiBundle");
        }


        void OnGUI()
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("PackEmoji");
            EditorGUILayout.Space();
            m_dir = EditorGUILayout.TextField("Dir", m_dir);
            SelectEmojiDir();
            GUILayout.EndVertical();
        }

        private void SelectEmojiDir()
        {
            if (mouseOverWindow == this)
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (Event.current.type == EventType.DragExited)
                {
                    Focus();
                    if (DragAndDrop.paths != null)
                    {
                        int len = DragAndDrop.paths.Length;
                        //for (int i = 0; i < len; i++)
                        {
                            if (len > 0)
                            {
                                //Debug.Log(DragAndDrop.paths[0]);
                                m_dir = DragAndDrop.paths[0];
                            }
                        }
                    }
                }
            }
        }
    }
}
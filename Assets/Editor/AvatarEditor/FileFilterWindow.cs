using System.IO;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    [ExecuteInEditMode]
    public class FileFilterWindow : EditorWindow
    {

        private string filter = "*cutscene*";
        private string path = "/Editor/EditorResources/Server/Animation/";

        [MenuItem("Tools/Asset/FileFilter")]
        static void AnimExportTool()
        {
            if (XEditorUtil.MakeNewScene())
            {
                EditorWindow.GetWindowWithRect(typeof(FileFilterWindow), new Rect(0, 0, 400, 400), true, "FileFilter");
            }
        }


        void OnGUI()
        {
            EditorGUILayout.Space();
            filter = EditorGUILayout.TextField("filter", filter);
            EditorGUILayout.LabelField(path);
            EditorGUILayout.Space();
            if (GUILayout.Button(XEditorUtil.Config.delete))
            {
                string npath = Application.dataPath + path;
                DirectoryInfo dir = new DirectoryInfo(npath);
                FileInfo[] files = dir.GetFiles(filter);
                for (int i = 0; i < files.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(i + "/" + files.Length, files[i].Name, (float)i / files.Length);
                    files[i].Delete();
                }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("tip", "done", "ok");
            }
            GUILayout.Space(300);
            GUILayout.BeginHorizontal();
            GUILayout.Space(250);
            GUILayout.Label("(c) powered by huailiang");
            GUILayout.EndHorizontal();
        }
    }
}
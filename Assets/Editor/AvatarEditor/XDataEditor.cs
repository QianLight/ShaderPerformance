using System.IO;
using UnityEditor;
using UnityEngine;

namespace XEditor
{
    public class XDataEditor<T> : Editor where T : ScriptableObject 
    {
        protected string search;
        private string mTitle;
        private GUIContent icon;

        protected T odData;

        public T Data
        {
            get { return odData; }
        }

        protected static void CreateAsset(string path)
        {
            if (!File.Exists(path))
            {
                T od = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(od, path);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("The asset has exist " + path);
            }
        }


        protected void Init(string ico, string title)
        {
            var ic = AssetDatabase.LoadAssetAtPath<Texture2D>(ico);
            Init(ic, title);
        }

        protected void Init(Texture2D ico, string title)
        {
            icon = new GUIContent(ico);
            mTitle = title;
            odData = target as T;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(icon, LODGUI.Styles.LODRendererButton, GUILayout.MaxWidth(32), GUILayout.MaxHeight(32));
            GUILayout.Label(mTitle, XEditorUtil.titleLableStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            search = XEditorUtil.GuiSearch(search);
            InnerGUI();
            GuiButtons();
            GUILayout.EndVertical();
        }
        

        private void GuiButtons()
        {
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                OnAdd();
            }
            if (GUILayout.Button("Save"))
            {
                OnSave();
            }
            GUILayout.EndHorizontal();
        }
        

        protected virtual void InnerGUI()
        {
        }

        protected virtual void OnAdd()
        {
        }

        protected virtual void OnSave()
        {
        }

    }
}
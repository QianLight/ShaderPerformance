using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;

namespace LevelEditor
{
    
    class LevelEditorScriptTool : EditorWindow
    {
        public static LevelEditorScriptTool Instance = null;

        private string RootFolder;
        [MenuItem(@"XEditor/LevelEditorScriptTool")]
        static void ShowWindow()
        {
            //EditorWindow.GetWindowWithRect<XReactLineEditor>(new Rect(800f, 100f, 800f, 800f), false, @"React Line Editor", true);
            var window = EditorWindow.GetWindow<LevelEditorScriptTool>(@"LevelEditorScriptTool", true);
            window.position = new Rect(500, 400, 1000, 500);
            window.wantsMouseMove = true;
            window.Show();
            window.Repaint();
        }

        public virtual void OnEnable()
        {
            RootFolder = Application.dataPath + "/BundleRes/Table/Level";
        }

        void OnGUI()
        {
            if (GUILayout.Button("ReSave", new GUILayoutOption[] { GUILayout.Width(120f) }))
            {
                DirectoryInfo di = new DirectoryInfo(RootFolder);
                FileInfo[] fileInfos = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);

                for (int i = 0; i < fileInfos.Length; ++i)
                {
                    var fi = fileInfos[i];
                    if (fi.Extension == ".cfg")
                    {
                        //if(fi.FullName.Contains("OP_enieslobby_mainisland4"))
                        ReSave(fi.FullName);
                    }
                }
            }
        }

        private void ReSave(string file)
        {
            LevelEditorData fullData = DataIO.DeserializeData<LevelEditorData>(file);
            RefreshWallData(ref fullData);
            DataIO.SerializeData<LevelEditorData>(file, fullData);
        }

        private void RefreshWallData(ref LevelEditorData fullData)
        {
            for(int i = 0; i < fullData.LevelWallData.Count; ++i)
            {
                LevelWallData data = fullData.LevelWallData[i];

                //if (data.rightup != Vector3.zero || data.leftdown != Vector3.zero) continue;

                Vector3 pos = data.position;
                float theta = data.rotation;
                Vector3 size = data.size;

                float rightX = pos.x + size.x / 2 * Mathf.Cos(-theta * Mathf.PI / 180);
                float rightY = size.y;
                float rightZ = pos.z + size.x / 2 * Mathf.Sin(-theta * Mathf.PI / 180);

                float leftX = pos.x - size.x / 2 * Mathf.Cos(-theta * Mathf.PI / 180);
                float leftY = 2 * pos.y - size.y;
                float leftZ = pos.z - size.x / 2 * Mathf.Sin(-theta * Mathf.PI / 180);

                data.rightup = new Vector3(rightX, rightY, rightZ);
                data.leftdown = new Vector3(leftX, leftY, leftZ);

            }

            fullData.Version = 3;
        }

    }
}

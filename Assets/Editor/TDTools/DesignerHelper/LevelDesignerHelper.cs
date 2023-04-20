using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TDTools
{
    public class LevelDesignerHelper : BaseDesignerHelper
    {
        protected override void BindCommonArea()
        {
            base.BindCommonArea();
            var addBtn = new Button(AddScene)
            {
                text = "Add",
                name = "AddSceneBtn",
                style =
                {
                    width = 80f,
                    height = 22f,
                }
            };
            CommonBtnRoot.Add(addBtn);
            cmElement.Add(addBtn);
        }

        protected override void BindSceneArea()
        {
            base.BindSceneArea();
            if(isRuntime)
            {

            }
            else
            {
                SceneContainer.onGUIHandler = DrawSceneList;
            }
        }

        protected override void BindTableArea()
        {
            base.BindTableArea();
        }
        void DrawSceneList()
        {
            EditorGUILayout.LabelField("自定义场景区:");
            for (int i = 0; i < Config.Scene.Count; ++i)
            {
                var item = Config.Scene[i];
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(item.SceneName, GUILayout.Width(320f)))
                {
                    EditorSceneManager.OpenScene(item.ScenePath);
                }
                GUILayout.Space(20f);
                if (GUILayout.Button("x", GUILayout.Width(30f)))
                {
                    Config.Scene.RemoveAt(i);
                    WriteConfigToDisk();
                }
                GUILayout.Space(10f);
                if (i > 0)
                {
                    if (GUILayout.Button("↑", GUILayout.Width(30f)))
                    {
                        item.index = i - 1;
                        Config.Scene[i - 1].index = i;
                        SortSceneList();
                        WriteConfigToDisk();
                    }
                }
                else
                {
                    GUILayout.Space(33f);
                }
                GUILayout.Space(10f);
                if (i < Config.Scene.Count - 1)
                {
                    if (GUILayout.Button("↓", GUILayout.Width(30f)))
                    {
                        item.index = i + 1;
                        Config.Scene[i + 1].index = i;
                        SortSceneList();
                        WriteConfigToDisk();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void AddScene()
        {
            var path = EditorUtility.OpenFilePanel("选择场景文件", $"{Application.dataPath}/Scenes/Scenelib", "unity");
            if(path.Length > 0)
            {
                string name = Path.GetFileNameWithoutExtension(path);
                path = path.Replace(Application.dataPath, "Assets");
                int index = Config.Scene.Count == 0? 0 : Config.Scene.Last().index + 1;
                Config.Scene.Add(new CustomizationSceneData()
                {
                    SceneName = name,
                    ScenePath = path,
                    index = index
                });
                WriteConfigToDisk();
            }
        }

        protected void DrawTableList()
        {
            
        }
    }
}

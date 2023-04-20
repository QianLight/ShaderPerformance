using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeshFace
{
    public class WindowMeshCuont : ScriptableWizard
    {
        public string msg = "";
        private bool ApplyBool;
        
        static int GetAll = 0;
    
        //显示窗体
        [MenuItem("Tools/场景/场景面数")]
        private static void ShowWindow()
        {
            ScriptableWizard.DisplayWizard<WindowMeshCuont>("场景面数", "确定", "取消");
        }
        private void OnEnable()
        {
            Debug.Log("OnEnable");
            if (GetAll==0)
            {
                GameObject obj = new GameObject("WindowMeshCuont");
                obj.AddComponent<GetAllMashFace> ();
                GetAll++;
            }
        }
        private void OnWizardUpdate()
        {
            // Debug.Log("OnWizardUpdate");
            //
            // if (string.IsNullOrEmpty(msg))
            // {
            //     errorString = "请输入信息内容";//错误提示
            //     helpString = "";//帮助提示
            // }
            // else
            // {
            //     errorString = "";
            //     helpString = "请点击确认按钮";
            // }
        }
        private void OnWizardCreate()
        {
            //Debug.Log("OnWizardCreate");
        }
        private void OnWizardOtherButton()
        {
            //Debug.Log("OnWizardOtherButton");
        }
        private void OnDisable()
        {
            //Debug.Log("OnDisable");
        }
        private void OnDestroy()
        {
            //Debug.Log("OnDestroy");
            GetAll = 0;
        }
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal("box");
            {
                GUILayout.Label("最高面数范围：");
                GetAllMashFace.count = EditorGUILayout.IntField(GetAllMashFace.count);
                GetAllMashFace.isBool = GUILayout.Button("遍历");
                GetAllMashFace.isSave = GUILayout.Button("数据保存到本地");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal("box");
            {
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("角色");
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("总面数：");
                        GUILayout.TextField(GetAllMashFace.skinMaxTris.ToString());
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.TextArea(GetAllMashFace.outputChar);
                }
                EditorGUILayout.EndVertical();
    
                EditorGUILayout.BeginVertical();
                {
                    GUILayout.Label("场景");
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("总面数：");
                        GUILayout.TextField(GetAllMashFace.meshFilterMaxTris.ToString());
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.TextArea(GetAllMashFace.outputScene);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}


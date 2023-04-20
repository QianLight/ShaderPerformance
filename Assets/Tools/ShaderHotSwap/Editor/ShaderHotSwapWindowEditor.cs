using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace UsingTheirs.ShaderHotSwap
{

    [CustomEditor(typeof(ShaderHotSwapWindow), true)]
    public class ShaderHotSwapWindowEditor : Editor
    {
        private SerializedProperty shaderList;

        public override void OnInspectorGUI()
        {
            shaderList = serializedObject.FindProperty("shaderDataList");
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("LoadGameShaders", EditorStyles.toolbarButton))
            {
                shaderList.ClearArray();

                InsertShaderData("URP/Role/Cartoon");
                InsertShaderData("URP/Role/Hair");
                InsertShaderData("URP/Role/Head");
                InsertShaderData("URP/SFX/UVEffect_Manual");
                InsertShaderData("URP/Scene/Uber");
                InsertShaderData("URP/Scene/TreeLeaf");
                InsertShaderData("URP/Scene/TreeTrunk02");
                InsertShaderData("URP/Common_Unlit");
                InsertShaderData("URP/Scene/UniformWater");
                InsertShaderData("URP/Scene/Skybox");
                InsertShaderData("URP/SFX/UVEffect_AboveWater");
                InsertShaderData("URP/Scene/UberGrass");
                InsertShaderData("URP/Scene/StylizedTreeLeaf");
                InsertShaderData("URP/Scene/Cloud");
            }

            if (GUILayout.Button("全选", EditorStyles.toolbarButton))
            {
                ToggleShader(true);
            }
            
            if (GUILayout.Button("全不选", EditorStyles.toolbarButton))
            {
                ToggleShader(false);
            }

            GUILayout.EndHorizontal();
            
            
            EditorGUILayout.PropertyField(shaderList, new GUIContent("Shaders"), true);

            serializedObject.ApplyModifiedProperties();


        }
        public void ToggleShader(bool b)
        {
            for(int i=0;i<shaderList.arraySize;i++)
            {
                SerializedProperty itm= shaderList.GetArrayElementAtIndex(i);
                itm.FindPropertyRelative("enable").boolValue = b;
            }
        }

        private void InsertShaderData( string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            int nIndex = shaderList.arraySize;
            shaderList.InsertArrayElementAtIndex(nIndex);
            SerializedProperty itm= shaderList.GetArrayElementAtIndex(nIndex);

            // ShaderData newData = new ShaderData();
            // newData.name=shader.name;
            // newData.shader = shader;
            // itm.objectReferenceValue = newData;
            
            itm.FindPropertyRelative("name").stringValue = shader.name;
            itm.FindPropertyRelative("shader").objectReferenceValue = shader;
        }
    }

}

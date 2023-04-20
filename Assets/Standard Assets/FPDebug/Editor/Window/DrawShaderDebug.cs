using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public partial class FPDebugWindow
{
    private void drawShaderDebug()
    {
        if (GUILayout.Button("获取Shader列表"))
        {
            refreshShaderCacheListBegin();
            selectIndex = -1;
        }
        GUILayout.Label("手机全屏显示使用的材质:");
        if (debugShaders != null && debugShaders.Count > 0)
        {
            GUILayout.Space(20);
            GUILayout.Label("Shader List:");
            for (int i = 0; i < debugShaders.Count; i++)
            {
                string tmp = debugShaders[i];
                EditorGUILayout.BeginHorizontal();
                if (selectIndex == i)
                {
                    GUILayout.Label(tmp);
                }
                else
                {
                    if (GUILayout.Button(tmp))
                    {
                        selectIndex = i;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(20);
            otherRender = GUILayout.Toggle(otherRender, "保持其他渲染");
            getShareMat = GUILayout.Toggle(getShareMat, "获取ShareMaterial");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("物体结构"))
            {
                ClientMessage.GetObjectList(delegate (SCList obj)
                {
                    getFail(obj);
                    getObjects(obj);
                });
            }
            if (GUILayout.Button("清空列表"))
            {
                clearShaderDebugList();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("显示Shader"))
            {
                if (selectIndex > -1)
                {
                    showShader(selectIndex, 1);
                }
            }
            if (GUILayout.Button("关闭Shader"))
            {
                showShader(selectIndex, 2);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("关闭Shader物体"))
            {
                if (selectIndex > -1)
                {
                    showShader(selectIndex, 3);
                }
            }
            if (GUILayout.Button("打开Shader物体"))
            {
                if (selectIndex > -1)
                {
                    showShader(selectIndex, 4);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("选择Shader:", GUILayout.Width(80));
            FPShaderFix.ColorShader = (Shader)EditorGUILayout.ObjectField("", FPShaderFix.ColorShader, typeof(Shader), true);
            if (GUILayout.Button("开始替换") && FPShaderFix.ColorShader != null)
            {
                if (selectIndex > -1)
                {
                    FPShaderFix.UploadColorShader(selectIndex, FPShaderFix.ColorShader);
                }
            }
            if (GUILayout.Button("还原替换"))
            {
                showShader(selectIndex, 6);
            }
            EditorGUILayout.EndHorizontal();

        }
    }

    void clearShaderDebugList()
    {
        if (debugShaders != null)
            debugShaders.Clear();
        selectIndex = -1;
    }

    void refreshShaderCacheListBegin()
    {
        ClientMessage.GetShadertList(getShareMat, delegate (FPDebugShaderList res)
        {
            if (res != null)
                debugShaders = res.Shaders;
        });
    }
    void showShader(int id, int state)
    {
        ClientMessage.ShowShader(id, state, otherRender, delegate (string res)
        {
            if (res != null)
                Debug.Log(res);
        });
    }
    private bool otherRender = true;
    private bool getShareMat = true;
    private int selectIndex = -1;
    private List<string> debugShaders = null;

}


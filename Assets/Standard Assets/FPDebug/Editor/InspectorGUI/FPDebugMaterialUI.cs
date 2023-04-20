using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

public sealed class FPDebugMaterialUI : FPDebugUIBase
{
    FPDebugObjectItem obj;
    int setMatState = 0;
    RenderMat renderMat = null;
    GlobalPara globalPara = null;
    public FPDebugMaterialUI(FPDebugObjectItem _obj)
    {
        obj = _obj;
        OnEnable();
    }
    public void OnDisable()
    {
        renderMat = null;
        FPShaderFix.NeedUpdateShader = false;
        FPShaderFix.UpdateShader = null;
    }

    public void OnEnable()
    {
        setMatState = 0;
        globalPara = new GlobalPara();
    }

    public bool OnInspectorGUI()
    {
        switch (setMatState)
        {
            case 0:
                {
                    if (GUILayout.Button("获取信息"))
                    {
                        setMatState = 1;
                        ClientMessage.GetMaterialPara(obj.RemoteID, OnMaterialParaGetCallBack);
                    }
                    break;
                }
            case 1:
                {
                    GUILayout.Label("正在获取信息中...");
                    break;
                }
            case 2:
                {
                    if (GUILayout.Button("修改信息"))
                    {
                        setMatState = 3;
                        ClientMessage.SetMaterialPara(renderMat, OnMaterialParaSaveCallBack);
                    }
                    DrawRenderMat();
                    break;
                }
            case 3:
                {
                    GUILayout.Label("正在修改信息...");
                    break;
                }
        }
        return true;
    }
    void OnMaterialParaGetCallBack(RenderMat rm)
    {
        renderMat = rm;
        setMatState = 2;
    }
    void OnMaterialParaSaveCallBack(object obj)
    {
        if ((bool)obj)
        {
            Debug.LogWarning("修改参数成功！->OnMaterialParaSaveCallBack");
            setMatState = 2;
        }
    }
    void OnGlobalParaSaveCallBack(object obj)
    {
        if ((bool)obj)
        {
            Debug.LogWarning("修改参数成功！->OnGlobalParaSaveCallBack");
            setMatState = 2;
        }
    }
    void DrawRenderMat()
    {
        int labWidth = 130;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Layer:", GUILayout.Width(labWidth));
        renderMat.Layer = EditorGUILayout.IntField(renderMat.Layer);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Type:", GUILayout.Width(labWidth));
        switch (renderMat.Type)
        {
            case 0:
                {
                    GUILayout.Label("None");
                    break;
                }
            case 1:
                {
                    GUILayout.Label("MeshRender");
                    break;
                }
            case 2:
                {
                    GUILayout.Label("SkinMeshRender");
                    break;
                }
            case 3:
                {
                    GUILayout.Label("Light");
                    break;
                }
        }
        GUILayout.EndHorizontal();

        switch (renderMat.Type)
        {
            case 1:
            case 2:
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("MeshName:", GUILayout.Width(labWidth));
                    GUILayout.Label(renderMat.MeshName);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("CastShadow:", GUILayout.Width(labWidth));
                    renderMat.CastShadow = GUILayout.Toggle(renderMat.CastShadow, "");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("LightProbe:", GUILayout.Width(labWidth));
                    UnityEngine.Rendering.LightProbeUsage LightProbe = (UnityEngine.Rendering.LightProbeUsage)renderMat.LightProbeUsage;
                    LightProbe = (UnityEngine.Rendering.LightProbeUsage)EditorGUILayout.EnumPopup(LightProbe, "");
                    renderMat.LightProbeUsage = (int)LightProbe;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ReflectionProbe:", GUILayout.Width(labWidth));
                    UnityEngine.Rendering.ReflectionProbeUsage ReflectionProbe = (UnityEngine.Rendering.ReflectionProbeUsage)renderMat.ReflectionProbeUsage;
                    ReflectionProbe = (UnityEngine.Rendering.ReflectionProbeUsage)EditorGUILayout.EnumPopup(ReflectionProbe, "");
                    renderMat.ReflectionProbeUsage = (int)ReflectionProbe;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("MatName:", GUILayout.Width(labWidth));
                    GUILayout.Label(renderMat.MatName);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ShaderName:", GUILayout.Width(labWidth));
                    GUILayout.Label(renderMat.ShaderName);
                    if (FPShaderFix.NeedUpdateShader)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Label("编辑器将会切换到 " + FPShaderFix.ShaderPlatform + " 平台");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Shader:", GUILayout.Width(labWidth));

                        FPShaderFix.UpdateShader = (Shader)EditorGUILayout.ObjectField("", FPShaderFix.UpdateShader, typeof(Shader), true);
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("热更Shader") && FPShaderFix.UpdateShader != null)
                        {
                            FPShaderFix.Upload(obj.RemoteID, FPShaderFix.UpdateShader);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("热更Shader"))
                        {
                            FPShaderFix.NeedUpdateShader = true;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Keywords:", GUILayout.Width(labWidth));
                    renderMat.Keywords = GUILayout.TextField(renderMat.Keywords);
                    GUILayout.EndHorizontal();


                    FPEditorGUIUtility.ToggleGroup("Vectors", ref viewVector);

                    if (viewVector)
                    {
                        for (int i = 0; i < renderMat.Vectors.Count; i++)
                        {
                            RenderMatVector rmv = renderMat.Vectors[i];
                            GUILayout.Label(rmv.Name, GUILayout.Width(labWidth));
                            rmv.Value = EditorGUILayout.Vector4Field("", rmv.Value);
                        }
                    }

                    FPEditorGUIUtility.ToggleGroup("Floats", ref viewFloat);

                    if (viewFloat)
                    {
                        for (int i = 0; i < renderMat.Floats.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            RenderMatFloat rmf = renderMat.Floats[i];
                            GUILayout.Label(rmf.Name, GUILayout.Width(labWidth));
                            rmf.Value = EditorGUILayout.FloatField("", rmf.Value);
                            GUILayout.EndHorizontal();
                        }
                    }
                    break;
                }

            case 3:
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("CullingMask:", GUILayout.Width(labWidth));
                    renderMat.LightCullingMask = EditorGUILayout.IntField(renderMat.LightCullingMask);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color:", GUILayout.Width(labWidth));
                    renderMat.LightColor = EditorGUILayout.ColorField(renderMat.LightColor);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("LightmapBakeType:", GUILayout.Width(labWidth));
                    LightmapBakeType lightmapBakeType = (LightmapBakeType)renderMat.LightmapBakeType;
                    lightmapBakeType = (LightmapBakeType)EditorGUILayout.EnumPopup(lightmapBakeType, "");
                    renderMat.LightmapBakeType = (int)lightmapBakeType;
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("LightShadows:", GUILayout.Width(labWidth));
                    LightShadows lightShadows = (LightShadows)renderMat.LightShadows;
                    lightShadows = (LightShadows)EditorGUILayout.EnumPopup(lightShadows, "");
                    renderMat.LightShadows = (int)lightShadows;
                    GUILayout.EndHorizontal();

                    GUILayout.Space(50);
                    GUILayout.Label("------------设置全局参数------------");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Vector Name:", GUILayout.Width(labWidth));
                    globalPara.VectorName = EditorGUILayout.TextField(globalPara.VectorName);
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Vector:", GUILayout.Width(labWidth));
                    GUILayout.BeginHorizontal();
                    globalPara.Vector = EditorGUILayout.Vector4Field("", globalPara.Vector);
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Set Global Vector"))
                    {
                        globalPara.Type = 1;
                        setMatState = 3;
                        ClientMessage.SetGlobalPara(globalPara, OnGlobalParaSaveCallBack);
                    }
                    GUILayout.Space(50);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Float Name:", GUILayout.Width(labWidth));
                    globalPara.FloatName = EditorGUILayout.TextField(globalPara.FloatName);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Float:", GUILayout.Width(labWidth));
                    globalPara.Float = EditorGUILayout.FloatField("", globalPara.Float);
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Set Global Float"))
                    {
                        globalPara.Type = 0;
                        setMatState = 3;
                        ClientMessage.SetGlobalPara(globalPara, OnGlobalParaSaveCallBack);
                    }
                    GUILayout.Space(50);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Keyword:", GUILayout.Width(labWidth));
                    globalPara.Keyword = EditorGUILayout.TextField("", globalPara.Keyword);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enable:", GUILayout.Width(labWidth));
                    globalPara.EnableKeyword = EditorGUILayout.Toggle("", globalPara.EnableKeyword);
                    GUILayout.EndHorizontal();
                    if (GUILayout.Button("Set Global Keyword"))
                    {
                        globalPara.Type = 2;
                        setMatState = 3;
                        ClientMessage.SetGlobalPara(globalPara, OnGlobalParaSaveCallBack);
                    }
                    break;
                }
            default:
                {

                    break;
                }
        }
    }
    bool viewVector = true, viewFloat = true, saveVector = true, saveFloat = false;
}

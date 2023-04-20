using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

public partial class FPDebugWindow
{
    private bool pause = false;
    private bool closeLog = false;
    private bool closeCamera = false;
    private bool closeSrpBatch = false;
    private void drawRenderSetting()
    {
        if (modelInfo != null)
        {
            modelInfo = GUILayout.TextArea(modelInfo);
        }

        if (renderInfo != null)
        {
            EditorGUILayout.EnumPopup("Color Space:", renderInfo.AactiveColorSpace);
            EditorGUILayout.LabelField("Driver Platform:", renderInfo.Platform);
            renderInfo.LightCount = EditorGUILayout.IntField("Light Count:", renderInfo.LightCount);
            renderInfo.TextureLimit = EditorGUILayout.IntSlider("Texture Quality:", renderInfo.TextureLimit, 0, 2);
            renderInfo.ShadowmaskMode = (ShadowmaskMode)EditorGUILayout.EnumPopup("Shadowmask Mode:", renderInfo.ShadowmaskMode);
            renderInfo.Shadows = EditorGUILayout.TextField("Shadow Type:", renderInfo.Shadows);
            renderInfo.ShadowResolution = EditorGUILayout.TextField("Shadow Resolution:", renderInfo.ShadowResolution);
            renderInfo.ShadowProjection = EditorGUILayout.TextField("Shadow ShadowProjection:", renderInfo.ShadowProjection);
            renderInfo.ShadowDistance = EditorGUILayout.FloatField("Shadow Distance:", renderInfo.ShadowDistance);
            renderInfo.ShadowNearPlane = EditorGUILayout.FloatField("Shadow Near Plane:", renderInfo.ShadowNearPlane);
            renderInfo.ShadowCascades = EditorGUILayout.IntSlider("Shadow Cascades:", renderInfo.ShadowCascades, 1, 4);
            renderInfo.SyncCount = EditorGUILayout.IntSlider("V sync Count:", renderInfo.SyncCount, 0, 2);
            renderInfo.TargetFrame = EditorGUILayout.IntField("Target Frame:", renderInfo.TargetFrame);
            renderInfo.AA = EditorGUILayout.IntSlider("AA:", renderInfo.AA, 0, 4);
            renderInfo.PostScale = EditorGUILayout.IntSlider("后处理分辨率(%):", renderInfo.PostScale, 1, 100);
        }

        EditorGUILayout.BeginHorizontal();
        if (getFps != 0 && GUILayout.Button("关闭FPS", GUILayout.Width(80)))
        {
            fps = null;
            getFps = 0;
        }
        if (getFps == 0 && GUILayout.Button("开启FPS", GUILayout.Width(80)))
        {
            fps = new string[] { "0", "0", "0" };
            ClientMessage.GetFPS(true, delegate (string[] mFps)
            {
                fps = mFps;
                getFail(mFps);
            });
            getFps = 1;
        }
        if (getFps != 0)
        {
            if (GUILayout.Button("清理", GUILayout.Width(80)))
            {
                fps = new string[] { "0", "0", "0" };
                ClientMessage.GetFPS(true, delegate (string[] mFps)
                {
                    fps = mFps;
                    getFail(mFps);
                });
            }
            EditorGUILayout.LabelField("FPS:" + fps[0], GUILayout.Width(60));
            EditorGUILayout.LabelField(" Min:" + fps[1], GUILayout.Width(60));
            EditorGUILayout.LabelField(" Max:" + fps[2], GUILayout.Width(60));
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        //
        if (GUILayout.Button(closeLog ? "打开日志" : "关闭日志"))
        {
            closeLog = !closeLog;
            ClientMessage.SetLog(closeLog ? "0" : "1", delegate (object ok)
            {
                getFail(ok);
            });
        }
        if (GUILayout.Button(closeCamera ? "打开主相机" : "关闭主相机"))
        {
            closeCamera = !closeCamera;
            ClientMessage.SetCamera(closeCamera ? "0" : "1", delegate (object ok)
            {
                getFail(ok);
            });
        }
        if (GUILayout.Button(closeSrpBatch ? "打开SRPBatch" : "关闭SRPBatch"))
        {
            closeSrpBatch = !closeSrpBatch;
            ClientMessage.SetSrpBatch(closeSrpBatch ? "0" : "1", delegate (object ok)
            {
                getFail(ok);
            });
        }
        if (GUILayout.Button(CFEngine.EngineContext.optimizeRenderState ? "关闭HSR" : "打开HSR"))
            CFEngine.EngineContext.optimizeRenderState = !CFEngine.EngineContext.optimizeRenderState;
        if (GUILayout.Button(pause ? "继续游戏" : "暂停游戏"))
        {
            pause = !pause;
            ClientMessage.PauseGame(pause ? "0" : "1", delegate (object ok)
            {
                getFail(ok);
            });
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("物体结构"))
        {
            ClientMessage.GetObjectList(delegate (SCList obj)
            {
                getFail(obj);
                getObjects(obj);
            });
        }
        if (GUILayout.Button("获取渲染参数"))
        {
            ClientMessage.GetRenderInfo(delegate (RenderInfo obj)
            {
                getFail(obj);
                renderInfo = obj;
            });
        }
        if (GUILayout.Button("设置渲染参数"))
        {
            ClientMessage.SetRenderInfo(renderInfo, delegate (object ok)
            {
                getFail(ok);
            });
        }
        EditorGUILayout.EndHorizontal();
    }

}

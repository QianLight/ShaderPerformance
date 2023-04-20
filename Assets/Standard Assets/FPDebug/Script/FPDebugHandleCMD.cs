using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class FPDebugHandle
{
    Shader getShader(byte[] dat)
    {
        Shader shader = null;
        if (lastAssetBundle != null)
        {
            lastAssetBundle.Unload(false);
            lastAssetBundle = null;
        }
        if (dat != null)
        {
            try
            {
                lastAssetBundle = AssetBundle.LoadFromMemory(dat);
            }
            catch (Exception ex)
            {
                //Debug.Log(task.Data[0] + "," + task.Data[1] + "," + task.Data[task.Data.Length - 2] + "," + task.Data[task.Data.Length - 1]);
                Debug.LogError(ex);
            }
        }
        if (lastAssetBundle != null)
        {
            Shader[] shaders = lastAssetBundle.LoadAllAssets<Shader>();
            if (shaders.Length > 0)
            {
                shader = shaders[0];
                shaders = null;
            }
        }
        return shader;
    }
    void doCmd(ThreadTask task)
    {
        string result = "";
        switch (task.Para["cmd"])
        {
            case "start":
                {
                    result = "ok";
                    break;
                }
            case "info":
                {
                    result = FPDeviceInfo.GetDeviceInformation();
                    break;
                }
            case "fps":
                {
                    string clear = task.Para["clear"];
                    if (clear == "True")
                    {
                        minFps = maxFps = fps;
                    }
                    result = string.Concat(fps, ",", minFps, ",", maxFps);

                    break;
                }
            case "getrender":
                {
                    RenderInfo info = FPDeviceInfo.GetRenderInfo();
                    result = JsonUtility.ToJson(info);
                    break;
                }
            case "setrender":
                {
                    string render = task.Para["render"];
                    RenderInfo info = JsonUtility.FromJson<RenderInfo>(render);
                    FPDeviceInfo.SetRenderInfo(info);
                    result = "ok";
                    break;
                }
            case "getobject":
                {
                    SCList sList = FPGameObject.GetObjectList();
                    result = JsonUtility.ToJson(sList);
                    break;
                }
            case "object":
                {
                    string sid = task.Para["id"];
                    string action = task.Para["action"];
                    int id = 0;
                    int.TryParse(sid, out id);
                    bool actionBool = false;
                    if (action == "enable")
                    {
                        actionBool = true;
                    }
                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        if (obj.activeSelf != actionBool)
                        {
                            obj.SetActive(actionBool);
                        }
                    }
                    result = "ok";
                    break;
                }
            case "pause":
                {
                    string state = task.Para["state"];
                    if (state == "0")
                    {
                        Time.timeScale = 0;
                    }
                    else if (state == "1")
                    {
                        Time.timeScale = 1;
                    }
                    result = "ok";
                    break;
                }
            case "log":
                {
                    string state = task.Para["state"];
                    if (state == "0")
                    {
                        //GameObject lastDebugObj = GameObject.Find("Canvas/root");
                        //if (lastDebugObj != null)
                        //{
                        //    lastDebug = lastDebugObj.GetComponent("CFClient.DebugBehaivour") as MonoBehaviour;
                        //    if (lastDebug != null)
                        //        lastDebug.enabled = false;
                        //}
                        Debug.unityLogger.logEnabled = false;
                    }
                    else if (state == "1")
                    {
                        //if (lastDebug != null)
                        //{
                        //    lastDebug.enabled = true;
                        //    lastDebug = null;
                        //}
                        Debug.unityLogger.logEnabled = true;
                    }
                    result = "ok";
                    break;
                }
            case "camera":
                {
                    string state = task.Para["state"];
                    Camera cam = Camera.main;
                    if (cam != null)
                    {
                        if (state == "0")
                        {
                            lastCameraMask = cam.cullingMask;
                            cam.cullingMask = 0;
                        }
                        else if (state == "1")
                        {
                            cam.cullingMask = lastCameraMask;
                        }
                    }
                    result = "ok";
                    break;
                }
            case "srpbatch":
                {
                    string state = task.Para["state"];
                    if (state == "0")
                    {
                        FPDeviceInfo.SeSrpBatch(false);
                    }
                    else if (state == "1")
                    {
                        FPDeviceInfo.SeSrpBatch(true);
                    }
                    result = "ok";
                    break;
                }
            case "shaderlist":
                {
                    bool share = task.Para["share"] == "1";
                    FPDebugShaderList shaderList = FPMaterialView.GetShaderList(share, FPGameObject.CurrentList);
                    result = JsonUtility.ToJson(shaderList);
                    break;
                }
            case "showshader":
                {
                    bool other = task.Para["other"] == "1";
                    int id = 0;
                    int state = 0;
                    int.TryParse(task.Para["id"], out id);
                    int.TryParse(task.Para["state"], out state);
                    FPMaterialView.ShowShader(id, state, other, FPGameObject.CurrentList, null);
                    result = "ok";
                    break;
                }
            //case "getmesh":
            //    {
            //        bool share = task.Para["share"] == "1";
            //        string ids = task.Para["ids"];
            //        string[] idArray = ids.Split(new char[] { ',' });
            //        ReplaceShaderList sl = new ReplaceShaderList();
            //        sl.Values = new List<ReplaceShaderInfo>();
            //        for (int i = 0; i < idArray.Length; i++)
            //        {
            //            int id = 0;
            //            int.TryParse(idArray[i], out id);
            //            if (id != 0)
            //            {
            //                if (id != 0 && currentList.ContainsKey(id))
            //                {
            //                    GameObject obj = currentList[id];
            //                    getShaderName(obj.transform, share, sl, true, false);
            //                }
            //            }
            //        }
            //        result = JsonUtility.ToJson(sl);
            //        break;
            //    }
            case "replaceshader":
                {
                    string sid = task.Para["id"];
                    int id = 0;
                    int.TryParse(sid, out id);

                    Shader shader = getShader(task.Data);
                    if (shader != null)
                    {
                        if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                        {
                            GameObject obj = FPGameObject.CurrentList[id];
                            if (obj != null)
                            {
                                Renderer render = obj.GetComponent<Renderer>();
                                if (render != null)
                                {
                                    Material mat = render.sharedMaterial;
                                    mat.shader = shader;
                                }
                            }
                        }
                        result = "ok";
                    }
                    else
                    {
                        result = "fail";
                    }
                    break;
                }
            case "replacecolorshader":
                {
                    int id = 0;
                    int.TryParse(task.Para["id"], out id);

                    Shader shader = getShader(task.Data);
                    if (shader != null)
                    {
                        FPMaterialView.ShowShader(id, 5, true, FPGameObject.CurrentList, shader);
                        result = "ok";
                    }
                    else
                    {
                        result = "fail";
                    }
                    break;
                }
            case "post":
                {
                    string sid = task.Para["id"];
                    string name = task.Para["name"];
                    string action = task.Para["action"];
                    int id = 0;
                    int.TryParse(sid, out id);
                    bool actionBool = false;
                    if (action == "enable")
                    {
                        actionBool = true;
                    }

                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        UnityEngine.Rendering.VolumeComponent com = FPVolumeHelp.GetVolumeComponent(obj, name);
                        if (com != null && com.active != actionBool)
                        {
                            com.active = actionBool;
                        }
                    }
                    result = "ok";
                    break;
                }
            case "getpostpara":
                {
                    string sid = task.Para["id"];
                    string name = task.Para["name"];

                    int id = 0;
                    int.TryParse(sid, out id);
                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        UnityEngine.Rendering.VolumeComponent com = FPVolumeHelp.GetVolumeComponent(obj, name);
                        PVP pvp = FPVolumeHelp.GetVolumeParameter(com);
                        result = JsonUtility.ToJson(pvp);
                    }
                    else
                    {
                        result = "fail";
                    }
                    break;
                }
            case "setpostpara":
                {
                    string sid = task.Para["id"];
                    string name = task.Para["name"];
                    string para = task.Para["para"];
                    string action = task.Para["action"];
                    string value = task.Para["value"];
                    int id = 0;
                    int.TryParse(sid, out id);
                    bool actionBool = false;
                    if (action == "enable")
                    {
                        actionBool = true;
                    }

                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        UnityEngine.Rendering.VolumeComponent com = FPVolumeHelp.GetVolumeComponent(obj, name);
                        FPVolumeHelp.SetVolumeParameter(com, para, actionBool, value);
                    }
                    result = "ok";
                    break;
                }
            case "getmaterialpara":
                {
                    string sid = task.Para["id"];
                    int id = 0;
                    int.TryParse(sid, out id);
                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        RenderMat rm = FPMaterialHelp.GetMaterialPara(obj, id);
                        result = JsonUtility.ToJson(rm);
                    }
                    else
                    {
                        result = "fail";
                    }
                    break;
                }
            case "setmaterialpara":
                {
                    string sid = task.Para["id"];
                    string matStr = task.Para["mat"];

                    int id = 0;
                    int.TryParse(sid, out id);
                    if (id != 0 && FPGameObject.CurrentList.ContainsKey(id))
                    {
                        GameObject obj = FPGameObject.CurrentList[id];
                        RenderMat rm = JsonUtility.FromJson<RenderMat>(matStr);
                        FPMaterialHelp.SetMaterialPara(obj, rm);
                        result = "ok";
                    }
                    else
                    {
                        result = "fail";
                    }
                    break;
                }
            case "setglobalpara":
                {
                    string maraStr = task.Para["para"];
                    GlobalPara para = JsonUtility.FromJson<GlobalPara>(maraStr);
                    if (para.Type == 0)
                    {
                        if (!string.IsNullOrEmpty(para.FloatName))
                        {
                            Shader.SetGlobalFloat(para.FloatName, para.Float);
                        }
                    }
                    else if (para.Type == 1)
                    {
                        if (!string.IsNullOrEmpty(para.VectorName))
                        {
                            Shader.SetGlobalVector(para.VectorName, para.Vector);
                        }
                    }
                    else if (para.Type == 2)
                    {
                        if (!string.IsNullOrEmpty(para.Keyword))
                        {
                            if (para.EnableKeyword)
                                Shader.EnableKeyword(para.Keyword);
                            else
                                Shader.DisableKeyword(para.Keyword);
                        }
                    }
                    result = "ok";
                    break;
                }
            case "end":
                {
                    result = "ok";
                    break;
                }
        }
        byte[] buff = Encoding.UTF8.GetBytes(result);
        task.Output.Write(buff, 0, buff.Length);
        task.Output.Close();
        task.Output.Dispose();
    }
    private int lastCameraMask = 0;
    private MonoBehaviour lastDebug = null;
}
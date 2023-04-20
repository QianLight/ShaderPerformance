using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class FPShaderFix
{
    public static string ShaderPlatform;
    public static bool NeedUpdateShader = false;
    public static Shader UpdateShader = null;
    public static Shader ColorShader = null;
    public static void Upload(int remoteId, Shader shader)
    {
        byte[] ab = buildAssetbundle(shader);
        ClientMessage.ReplaceShader(remoteId.ToString(), ab, delegate (object o)
        {
            Debug.LogWarning(o);
        });
    }
    public static void UploadColorShader(int index, Shader shader)
    {
        byte[] ab = buildAssetbundle(shader);
        ClientMessage.ReplaceColorShader(index, ab, delegate (object o)
        {
            Debug.LogWarning(o);
        });
    }
    static byte[] buildAssetbundle(Shader shader)
    {
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        string anName = "replaceShaderBundle.ab";
        buildMap[0].assetBundleName = anName;
        string path = Application.persistentDataPath + "/tmpShaderUpload";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string[] enemyAsset = new string[1];
        string[] namesAsset = new string[1];
        enemyAsset[0] = AssetDatabase.GetAssetPath(shader);
        namesAsset[0] = shader.name;
        buildMap[0].assetNames = enemyAsset;
        buildMap[0].addressableNames = namesAsset;
        BuildPipeline.BuildAssetBundles(path, buildMap, BuildAssetBundleOptions.None, getTarget());
        byte[] bs = File.ReadAllBytes(path + "/" + anName);
        return bs;
    }

    static BuildTarget getTarget()
    {
        switch (ShaderPlatform)
        {
            case "Android":
                {
                    return BuildTarget.Android;
                }
            case "IPhonePlayer":
                {
                    return BuildTarget.iOS;
                }
            case "WindowsEditor":
                {
                    return BuildTarget.StandaloneWindows;
                }
            case "OSXEditor":
                {
                    return BuildTarget.StandaloneOSX;
                }
        }
        return BuildTarget.StandaloneWindows;
    }
    //void getShaderObject(UnityEngine.Object[] handleObjs)
    //{
    //    int addCount = 0;
    //    for (int i = 0; i < handleObjs.Length; i++)
    //    {
    //        UnityEngine.Object handleObj = handleObjs[i];
    //        GameObject obj = handleObj as GameObject;

    //        if (obj != null)
    //        {
    //            FPDebugObjectItem item = obj.GetComponent<FPDebugObjectItem>();
    //            if(item != null && item.RemoteID != 0)
    //            {
    //                shaderDebugList.Add(obj);
    //                addCount++;
    //            }
    //        }
    //    }
    //    if(addCount > 0)
    //    {
    //        refreshShaderCacheListBegin();
    //    }
    //}

    //void refreshShaderCacheListEnd()
    //{
    //    if(shaderList == null)
    //    {
    //        return;
    //    }
    //    for (int j = 0; j < shaderList.Values.Count; j++)
    //    {
    //        string tmpName = shaderList.Values[j].Name;
    //        ShaderReplace replace = new ShaderReplace();
    //        replace.ShaderName = tmpName;
    //        for (int i = 0; i < lastShaderDebugCacheList.Count; i++)
    //        {
    //            ShaderReplace tmp = lastShaderDebugCacheList[i]; 
    //            if(tmpName == tmp.ShaderName)
    //            {
    //                replace.ReplaceShader = tmp.ReplaceShader;
    //            }
    //        }
    //        shaderDebugCacheList.Add(replace);
    //    }
    //    lastShaderDebugCacheList.Clear();
    //}

    //private List<ShaderReplace> shaderDebugCacheList = new List<ShaderReplace>();
    //private List<ShaderReplace> lastShaderDebugCacheList = new List<ShaderReplace>();

    //Ö÷Ìæ»»º¯Êý
    //void uploadShader()
    //{
    //    ReplaceShaderList list = new ReplaceShaderList();
    //    list.Values = new List<ReplaceShaderInfo>();
    //    List<Shader> shaderList = new List<Shader>();
    //    for (int i = 0; i < shaderDebugCacheList.Count; i++)
    //    {
    //        ShaderReplace tmp = shaderDebugCacheList[i];
    //        if(tmp.ReplaceShader != null)
    //        {
    //            ReplaceShaderInfo info = new ReplaceShaderInfo(tmp.ShaderName);
    //            info.ReplaceTo = tmp.ReplaceShader.name;
    //            list.Values.Add(info);
    //            shaderList.Add(tmp.ReplaceShader);
    //        }
    //    }
    //    if(shaderList.Count > 0)
    //    {
    //        byte[] ab = buildAssetbundle(shaderList);
    //        Debug.Log(ab[0] + "," + ab[1] + "," + ab[ab.Length - 2] + "," + ab[ab.Length - 1]);
    //        if (ab.Length > 0)
    //        {
    //            List<int> ids = new List<int>();
    //            for (int i = 0; i < shaderDebugList.Count; i++)
    //            {
    //                GameObject tmp = shaderDebugList[i];
    //                getItemRemoteIds(tmp.transform, ids);
    //            }
    //            if (ids.Count > 0)
    //            {
    //                StringBuilder sb = new StringBuilder();
    //                for (int i = 0; i < ids.Count; i++)
    //                {
    //                    if (i == 0)
    //                    {
    //                        sb.Append(ids[i]);
    //                    }
    //                    else
    //                    {
    //                        sb.Append(",");
    //                        sb.Append(ids[i]);
    //                    }
    //                }
    //                shaderList = null;

    //                ClientMessage.ReplaceShader(getShareShader, sb.ToString(), list, ab, delegate (object o)
    //                {
    //                    string result = o as string;
    //                    if(result == "ok")
    //                    {
    //                        for (int i = 0; i < shaderDebugCacheList.Count; i++)
    //                        {
    //                            ShaderReplace tmp = shaderDebugCacheList[i];
    //                            tmp.ShaderName = tmp.ReplaceShader.name;
    //                        }
    //                    }
    //                });
    //            }

    //        }
    //    }
    //}

    //void getItemRemoteIds(Transform root, List<int> ids)
    //{
    //    if(!root.gameObject.activeSelf)
    //    {
    //        return;
    //    }
    //    foreach (Transform t in root)
    //    {
    //        getItemRemoteIds(t, ids);
    //    }
    //    FPDebugObjectItem item = root.GetComponent<FPDebugObjectItem>();
    //    if (item != null && item.RemoteID != 0)
    //    {
    //        ids.Add(item.RemoteID);
    //    }
    //}
    //public class ShaderReplace
    //{
    //    public string ShaderName;
    //    public Shader ReplaceShader;
    //}
}

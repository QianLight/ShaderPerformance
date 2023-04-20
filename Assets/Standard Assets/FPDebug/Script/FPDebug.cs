#if SHOW_FPS_FLAG
using System;
using CFUtilPoolLib;
using UnityEngine;
using UsingTheirs.ShaderHotSwap;
using ServerHttpJsonPost = UsingTheirs.RemoteInspector.ServerHttpJsonPost;

public class FPDebug : XSingleton<FPDebug>, IFpDebug
{
    public bool Deprecated { get; set; }

    // 加快冷启动时间 使用反射获取
    private bool isOpenHotSwap = false;

    public void SetFpDebugActive(bool active)
    {
#if SHOW_FPS_FLAG
        FPDebugHandle.Instance.SetFpDebugActive(active);
#endif
    }


    public bool StartShaderSwap(string[] values)
    {
        
        Debug.Log("FPDebug StartShaderSwap:"+string.Join(",",values));
        
#if SHOW_FPS_FLAG
        if (!isOpenHotSwap)
        {
            GameObject go = new GameObject("RemoteInspectorServer");
            go.AddComponent<UrpGameDebug>();
            
            GameObject.DontDestroyOnLoad(go);
            var http1 = go.AddComponent<ServerHttpJsonPost>();
            http1.listenPort = 8080;
            go.AddComponent<UsingTheirs.RemoteInspector.ServerRemoteInspector>();

            
            GameObject go1 = new GameObject("ShaderHotSwap");
            GameObject.DontDestroyOnLoad(go1);
            var http2 = go1.AddComponent<UsingTheirs.ShaderHotSwap.ServerHttpJsonPost>();
            http2.listenPort = 8090;

            go1.AddComponent<ServerShaderHotSwap>();
        }

        isOpenHotSwap = true;
#endif
        return true;
    }
}
#endif
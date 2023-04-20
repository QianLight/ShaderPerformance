using System;
using System.Collections.Generic;
using System.Reflection;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;

public class CFHooks
{
    //[RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        RenderTextureSafeRelease.Init();
        RenderTexture2DArrayCheck.Init();
    }
}

public class RenderTexture2DArrayCheck
{
    public static void Init()
    {
        MethodInfo setter = typeof(RenderTexture).GetProperty(nameof(RenderTexture.dimension)).GetSetMethod();
        MethodInfo replace = typeof(RenderTexture2DArrayCheck).GetMethod(nameof(SetDimension));
        MethodInfo proxy = typeof(RenderTexture2DArrayCheck).GetMethod(nameof(SetDimensionProxy));
        CFHookManager.Install(setter, replace, proxy);
    }

    public void SetDimension(TextureDimension dimension)
    {
        if (dimension == TextureDimension.Tex2DArray || dimension == TextureDimension.CubeArray)
            Debug.LogError($"{this}.dimension = {dimension}");
        SetDimensionProxy(dimension);
    }

    public void SetDimensionProxy(TextureDimension dimension)
    {
        
    }
}

public class RenderTextureSafeRelease
{
    // <RenderTexture, Camera>
    private static Dictionary<object, object> activeRts = new Dictionary<object, object>();

    private static MethodInfo get_targetTexture;

    public static void Init()
    {
        get_targetTexture = typeof(Camera).GetProperty(nameof(Camera.targetTexture)).GetGetMethod();

        // Add Camera.set_targetTexture.
        MethodInfo targetTexture = typeof(Camera).GetProperty(nameof(Camera.targetTexture)).GetSetMethod();
        MethodInfo replace = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetCameraTargetTexture));
        MethodInfo proxy = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetCameraTargetTextureProxy));
        CFHookManager.Install(targetTexture, replace, proxy);

        // Add Camera.SetTargetBuffers
        Type[] methodParamTypes = { typeof(RenderBuffer), typeof(RenderBuffer) };
        MethodInfo setTargetBuffers = typeof(Camera).GetMethod(nameof(Camera.SetTargetBuffers), methodParamTypes);
        MethodInfo setTargetBuffersReplace = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetTargetBuffersReplace));
        MethodInfo setTargetBuffersProxy = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetTargetBuffersProxy));
        CFHookManager.Install(setTargetBuffers, setTargetBuffersReplace, setTargetBuffersProxy);
        Type[] methodParamTypes2 = { typeof(RenderBuffer[]), typeof(RenderBuffer) };
        MethodInfo setTargetBuffers2 = typeof(Camera).GetMethod(nameof(Camera.SetTargetBuffers), methodParamTypes2);
        MethodInfo setTargetBuffersReplace2 = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetTargetBuffersReplace2));
        MethodInfo setTargetBuffersProxy2 = typeof(RenderTextureSafeRelease).GetMethod(nameof(SetTargetBuffersProxy2));
        CFHookManager.Install(setTargetBuffers2, setTargetBuffersReplace2, setTargetBuffersProxy2);

        // Add RenderTexture.Release.
        MethodInfo release = typeof(RenderTexture).GetMethod(nameof(RenderTexture.Release));
        MethodInfo releaseReplace = typeof(RenderTextureSafeRelease).GetMethod(nameof(ReleaseReplace));
        MethodInfo releaseProxy = typeof(RenderTextureSafeRelease).GetMethod(nameof(ReleaseProxy));
        CFHookManager.Install(release, releaseReplace, releaseProxy);

        // Add RenderTexture.ReleaseTemporary.
        MethodInfo releaseTemp = typeof(RenderTexture).GetMethod(nameof(RenderTexture.ReleaseTemporary));
        MethodInfo releaseTempReplace = typeof(RenderTextureSafeRelease).GetMethod(nameof(ReleaseTempReplace));
        MethodInfo releaseTempProxy = typeof(RenderTextureSafeRelease).GetMethod(nameof(ReleaseTempProxy));
        CFHookManager.Install(releaseTemp, releaseTempReplace, releaseTempProxy);
    }

    public void SetTargetBuffersReplace(RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        Debug.Log("SetTargetBuffers invoked!");
        SetTargetBuffersProxy(colorBuffer, depthBuffer);
    }
    
    public void SetTargetBuffersProxy(RenderBuffer colorBuffer, RenderBuffer depthBuffer)
    {
        
    }

    public void SetTargetBuffersReplace2(RenderBuffer[] colorBuffer, RenderBuffer depthBuffer)
    {
        Debug.Log("SetTargetBuffers invoked!");
        SetTargetBuffersProxy2(colorBuffer, depthBuffer);
    }
    
    public void SetTargetBuffersProxy2(RenderBuffer[] colorBuffer, RenderBuffer depthBuffer)
    {
        
    }

    public void SetCameraTargetTexture(RenderTexture texture)
    {
        object[] parameters = ArrayPool<object>.Get(0);
        RenderTexture renderTexture = get_targetTexture.Invoke(this, parameters) as RenderTexture;
        ArrayPool<object>.Release(parameters);
        if (texture == renderTexture)
            return;
        if (renderTexture)
            activeRts.Remove(renderTexture);
        if (texture)
            activeRts[texture] = this;
        SetCameraTargetTextureProxy(texture);
    }

    public void SetCameraTargetTextureProxy(RenderTexture texture)
    {
    }

    public void ReleaseReplace()
    {
        if (activeRts.TryGetValue(this, out object obj) && obj is Camera camera)
        {
            if (camera)
                camera.targetTexture = null;
            activeRts.Remove(this);
        }

        ReleaseProxy();
    }

    public void ReleaseProxy()
    {
    }

    public static void ReleaseTempReplace(RenderTexture renderTexture)
    {
        if (activeRts.TryGetValue(renderTexture, out object obj) && obj is Camera camera && camera)
        {
            camera.targetTexture = null;
            activeRts.Remove(renderTexture);
        }

        ReleaseTempProxy(renderTexture);
    }

    public static void ReleaseTempProxy(RenderTexture renderTexture)
    {
    }
}
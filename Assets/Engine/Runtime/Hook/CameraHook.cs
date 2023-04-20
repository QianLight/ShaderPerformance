using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
public class CameraHook : CFDebugHook
{
    public override string Name => $"{typeof(Camera).FullName}.clearFlags";
    protected override MethodInfo Source => typeof(Camera).GetProperty("clearFlags").GetSetMethod();
    protected override MethodInfo Replace => GetType().GetMethod(nameof(SetCameraHookClearFlagsReplace));
    protected override MethodInfo Proxy  => GetType().GetMethod(nameof(SetCameraHookClearFlagsProxy));

    public void SetCameraHookClearFlagsReplace(CameraClearFlags flag)
    {
        Camera camera =  (object)this as Camera;
        Log($"name:{camera.name}  old flag:{camera.clearFlags} new flag:{flag}");
        SetCameraHookClearFlagsProxy(flag);

    }   
    public void SetCameraHookClearFlagsProxy(CameraClearFlags flag)
    {
        
    }
    
}
#endif
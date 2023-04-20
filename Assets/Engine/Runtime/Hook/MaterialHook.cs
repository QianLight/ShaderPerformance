using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
public class MaterialHook : CFDebugHook
{
    public override string Name => $"{typeof(ParticleSystemRenderer).FullName}.material";
    protected override MethodInfo Source => typeof(ParticleSystemRenderer).GetProperty("material").GetSetMethod();
    protected override MethodInfo Replace => GetType().GetMethod(nameof(SetCameraHookClearFlagsReplace));
    protected override MethodInfo Proxy  => GetType().GetMethod(nameof(SetCameraHookClearFlagsProxy));

    public void SetCameraHookClearFlagsReplace(Material mat)
    {
        ParticleSystemRenderer renderer =  (object)this as ParticleSystemRenderer;
        if (renderer.gameObject.name.Contains("Monster_Crocodile_knife"))
        {
            Log($"name:{renderer.name}  old flag:{renderer.material} new flag:{mat}");
            SetCameraHookClearFlagsProxy(mat);
        }

    }   
    public void SetCameraHookClearFlagsProxy(Material mat)
    {
        // ParticleSystemRenderer r;
    }
    
}
#endif
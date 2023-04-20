using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light)), ExecuteInEditMode]
public class LightParameterEntity : MonoBehaviour
{
    public VolumeProfile profile;
    public LightFieldType type;
    private Light light;
    private bool _hasLinked = false;
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            EditorApplication.update += EditorSync;
    }
    
    private void EditorSync()
    {
        if (Application.isPlaying)
            return;
    
        if (!this)
        {
            EditorApplication.update -= EditorSync;
            return;
        }
    
        if (light == null)
        {
            light = GetComponent<Light>();
        }
        // Try find profile if missing.
        if (!profile)
        {
            Volume[] volumes = FindObjectsOfType<Volume>();
            foreach (Volume volume in volumes)
            {
                var p = volume.profile;
                if (p && p.TryGet(out Lighting l)
                      && GetLightParameter(l, out var param)
                      && param.light == light)
                {
                    profile = p;
                }
            }
        }
    
        // Kill itself if no profile matched.
        if (!profile)
        {
            if (Application.isPlaying)
                Destroy(this);
            else
                DestroyImmediate(this);
            EditorApplication.update -= EditorSync;
            return;
        }
        RealSync();
    }
#endif

    private void OnEnable()
    {
        light = GetComponent<Light>();
        if (!light)
            return;
    }

    private void Update()
    {
        if (Application.isPlaying && !_hasLinked)
        {
            if (profile != null)
            {
                Volume[] volumes = FindObjectsOfType<Volume>();
                foreach (Volume volume in volumes)
                {
                    var p = volume.profileRef;
                    if (p!=null && p.name == profile.name 
                                && p.TryGet(out Lighting l)
                                && GetLightParameter(l, out var param))
                    {
                        profile = p;
                        _hasLinked = true;
                    }
                }
            }
        }

        if (_hasLinked)
        {
            RealSync();
            Destroy(this);
        }
    }

    private void RealSync()
    {
        if (profile && profile.TryGet(out Lighting lighting)
            && GetLightParameter(lighting, out LightParameter lightParameter))
        {
            Sync(ref lighting, ref lightParameter, light);
            SetLightParameter(ref lighting, ref lightParameter);
            for (int i = 0; i < profile.components.Count; i++)
            {
                if (profile.components[i] is Lighting)
                {
                    profile.components[i] = lighting;
                }
            }
        }
    }
    
    private bool GetLightParameter(Lighting lighting, out LightParameter lightParameter)
    {
        switch (type)
        {
            case LightFieldType.AddLight:
                lightParameter = lighting.addLight;
                break;
            case LightFieldType.MainLight:
                lightParameter = lighting.mainLight;
                break;
            case LightFieldType.WaterLight:
                lightParameter = lighting.waterLight;
                break;
            default:
                lightParameter = null;
                break;
        }
    
        return lightParameter != null;
    }
    private bool SetLightParameter(ref Lighting lighting, ref LightParameter lightParameter)
    {
        switch (type)
        {
            case LightFieldType.AddLight:
                lighting.addLight = lightParameter;
                break;
            case LightFieldType.MainLight:
                lighting.mainLight = lightParameter;
                break;
            case LightFieldType.WaterLight:
                lighting.waterLight = lightParameter;
                break;
        }
        return lightParameter != null;
    }
    
    private bool Sync(ref Lighting lighting, ref LightParameter parameter, Light light)
    {
        parameter.light = light;
        LightInfo info = parameter.value;
        Vector3 lightDir = -light.transform.forward;
        bool dirty = Vector3.Dot(info.GetDirection(), lightDir) < 0.999f
                     || info.color != light.color * light.intensity;
        if (dirty)
        {
            info.SetDirection(lightDir);
            info.color = light.color * light.intensity;
        }
    
        if (!Application.isPlaying)
        {
            if (parameter.mainLight)
            {
#if UNITY_EDITOR
                light.shadows = LightShadows.Hard;
                light.lightmapBakeType = LightmapBakeType.Mixed;
#endif
                light.enabled = true;
            }
            else
            {
#if UNITY_EDITOR
                light.shadows = LightShadows.None;
                light.lightmapBakeType = LightmapBakeType.Baked;
#endif
                light.enabled = false;
            }
        }
    
        if (dirty)
        {
            parameter.value = info;
            // parameter.Update();
#if UNITY_EDITOR
            EditorUtility.SetDirty(profile);
#endif
        }
    
        return dirty;
    }
    
    // TODO: 删除灯光时希望能清空Light颜色，但是OnDestroy的时候Reset会导致进游戏的时候情况profile的值，需要拿PlayMode判断一下。
    private void OnDestroy()
    {
        // if (profile
        //     && profile.TryGet(out Lighting lighting)
        //     && GetLightParameter(lighting, out LightParameter lightParameter))
        // {
        //     lightParameter.value = LightInfo.CreateDefault();
        // }
    }
}

#ifndef URP_CORE_INPUT
#define URP_CORE_INPUT
#include "OPPCore.hlsl"

half3 GetNormal(half2 tex, half scale = 1.0h)
{
#ifdef _NORMALMAP
    half3 n;
    n.xy = tex;
    n.xy = n.xy * 2.0f - 1.0f;
    n.z = sqrt(1 - saturate(dot(n.xy, n.xy)));
    n.xy *= scale;
    return n;
#else
    return GetDefaultNormal();
#endif
}

half3 GetEmission(half4 mainTex, half4 emissionColor)
{
//#ifdef _EMISSION_ON
//    return emissionColor.xyz * mainTex.a;
//#else
//    return half3(0.0h, 0.0h, 0.0h); 
//#endif
        if (_Emission > 0)
        {
            return emissionColor.xyz * mainTex.a * lerp(_SceneColor.rgb, 1, _SceneColor.a);
        }
        else
        {
            return half3(0.0h, 0.0h, 0.0h);
        }
}

half GetOcclusion(half val, half strength)
{
//#ifdef _OCCLUSION_ON
//    return 1.0h - strength + val * strength;
//#else
//    return 1.0h;
//#endif
    if (_Occlusion > 0)
    {
        return 1.0h - strength + val * strength;
    }
    else
    {
        return 1.0h;
    }
}

half GetSmoothness(half val, half roughnessMin, half roughnessMax)
{
    return clamp(1.0h - val, 1.0h - roughnessMax, 1.0h - roughnessMin);
}

half GetDefaultOcclusion() 
{
    return 1.0h;
}

half GetMetallic(half val, half metallicMin, half metallicMax)
{
	return _Metallic * clamp(val, metallicMin, metallicMax);
    //if (_Metallic > 0)
    //{
    //    return clamp(val, metallicMin, metallicMax);
    //}
    //else
    //{
    //    return 0.0h;
    //}
}

#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED

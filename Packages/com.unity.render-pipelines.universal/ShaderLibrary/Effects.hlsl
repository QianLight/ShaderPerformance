#ifndef EFFECTS_INCLUDED
#define EFFECTS_INCLUDED

#ifndef PBS_PCH_INCLUDE
    #if !defined(UNITY_PERMATERIAL_DECLARED)
        float4 _SceneColor;
    #endif
    float4 _AmbientParam1;
    float4 _AmbientParam2;
    #define _ContrastLight _AmbientParam1.w
    #define _ContrastInsity _AmbientParam2.w
#endif

inline void CalcSceneColor(inout half3 color,half srcShadow)
{	
    #ifdef _SCENE_EFFECT		
    half3 targetColor = color.xyz * _SceneColor.rgb;
    #else
    half3 targetColor = color*lerp(_SceneColor.rgb, 1, _SceneColor.a);		
    #endif
    targetColor = lerp(targetColor, 0, _ContrastLight);
    half contrastRatio = lerp(0, srcShadow, _ContrastLight);
    color.xyz = lerp(targetColor, Luminance(color.xyz * _ContrastInsity) * _SceneColor.rgb, contrastRatio);
}

#endif

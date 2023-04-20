#ifndef WINDTREELEAVESPASS
#define WINDTREELEAVESPASS
/// 储存树叶的渲染PASS
#include "WindTreeLeavesInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"



FLOAT4 _AmbientWindParam;
#define _AmbientWindDir _AmbientWindParam.xyz
#define _AmbientWindSpeed _AmbientWindParam.w
FLOAT4 _AmbientWindParam1;
#define _AmbientWindFrequency _AmbientWindParam1.x
#define _AmbientTreeWindSpeed _AmbientWindParam1.y
#define _AmbientWindDir2 _AmbientWindParam1.zw
//TEX2D_SAMPLER(_AmbientWind);
TEXTURE2D(_AmbientWind);
SAMPLER(sampler_AmbientWind);

// FLOAT4 _CollisionCenter;
// #define _CollisionRadiusInv _CollisionCenter.w

half _ImpostorEnable;
#define IMPOSTORENABLE _ImpostorEnable>0
half _ImpostorAlpha;
FLOAT4  _InteractParam;
#define _GRASS_COLLISION _InteractParam.x>0.5

#define smp SamplerState_Point_Repeat
SAMPLER(smp);

TYPE_POSITION3 CustomWindEffect( inout TYPE2 uv3,TYPE_POSITION3 WorldPosition)
{
    FLOAT3 offset = 0;
    FLOAT2 WindUV = (WorldPosition.xz*_AmbientWindFrequency*0.01)+(_Time.y *_AmbientTreeWindSpeed * 0.1);
    //FLOAT WindTex = SAMPLE_TEXTURE2D_LOD(_AmbientWind,sampler_AmbientWind,WindUV,0).r;
    //WindTex = WindTex * 2 - 1;
    FLOAT spaceOffset = -WorldPosition.x * _AmbientWindDir.x - WorldPosition.z * _AmbientWindDir.z;	 
    FLOAT weightedTime = fmod(_Frenquency * (spaceOffset * _ModelScaleCorrection + _Time.y), PI);
    FLOAT sway = max(sin(weightedTime + 0.5 * sin(weightedTime)), sin(weightedTime + PI + sin(weightedTime + PI))) - 0.9;
    FLOAT yoffset = abs(cos(sway)) * _AmbientWindDir.y;
    FLOAT horizontalOffset = sin(sway);
    FLOAT xoffset = horizontalOffset * _AmbientWindDir.x;
    FLOAT zoffset = horizontalOffset * _AmbientWindDir.z;
    //使用顶点色计算定点的偏移系数 effect ratio
    offset += float3(xoffset, yoffset , zoffset) *uv3.y * _ModelScaleCorrection * _Magnitude; 
    // Input.color.x = lerp(Input.color.x,1,_MaskRange);
    // offset *= Input.color.x; 
    _OffsetCorrection.xz += sin(_OffsetCorrection.xz);
    FLOAT3 blend = pow(abs(uv3.y),_Blend)+_OffsetCorrection*uv3.y;
    offset += blend;
    FLOAT Stable =lerp(uv3.y , 0 , _StableRange);
    offset *=Stable;
    return offset;
}


v2f vert(appdata v)
{
    v2f o;
    INITIALIZE_OUTPUT(v2f,o);
    //o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
    o.uv = v.uv;
    o.normalWS = TransformObjectToWorldNormal(v.normalOS);
    o.baseNormal = TransformObjectToWorldNormal(v.tangentOS);
    o.normalOS.xyz = v.normalOS;

    TYPE_POSITION3 v_posWorld;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
    v_posWorld = vertexInput.positionWS;

    v_posWorld += CustomWindEffect(v.uv3,v_posWorld);
    // 计算垂直颜色变化
    half height; 
    // #ifdef _LERPCOLORXY
    // height= clamp(0,1,v.positionOS.x);
    // #else
    // height= clamp(0,1,v.positionOS.z);
    // #endif
    //#ifdef _SHADER_LEVEL_HIGH
    height= v.uv3.y;
    o.treeParam = saturate((height - _TreeLerpRoot) / (_TreeLerpTop - _TreeLerpRoot) * _TreeLerpIntensity);
    //#endif

    //o.treeParam = height;

    // 导出Debug风的数据
    #if _DEBUGMODE
        o.debugWind = CustomWindEffect(v.uv3,v_posWorld);
        o.VertexColor = v.color;
    #endif

    o.ambient.rgb = 0;
    
    #ifdef _LIGHTPROBE
    // 计算环境光
        o.ambient.rgb = SampleSH(o.normalWS);
    #endif
    
    // 环境光的A通道存入顶点色的G通道，作为模拟AO
    //o.ambient.a = v.color.g;
    o.ambient.a = v.uv2.y;

    half vertexNdotL = max(0,dot(o.normalWS,_MainLightPosition.xyz));
    o.ambient.rgb += lerp(0,_MainLightColor.rgb*_SHColorIntensity,vertexNdotL);

    o.positionWS = v_posWorld;
    o.positionHCS = mul(UNITY_MATRIX_VP, float4(v_posWorld, 1));

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    //o.shadowCoord = GetShadowCoord(vertexInput);
    o.shadowCoord = TransformWorldToShadowCoord(o.positionWS);
    #endif

    return o;
}

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x
half4 frag(v2f i) : SV_Target0
{
    // Base Color
    half4 baseColor = 1;
    half4 baseColorMask = 1;
    #if defined(_COLORORTEX)
    baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    baseColor.rgb *= _BaseColor1.rgb;
    baseColor.a = step(_stepA,baseColor.a);
    #else
    baseColorMask = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
    baseColorMask.a = step(_stepA,baseColorMask.a);
    baseColor = half4(lerp(1,baseColorMask.b,_ColorDetails) * _BaseColor1.rgb,baseColorMask.a);
    #endif

    half3 viewDir = SafeNormalize(GetCameraPositionWS() - i.positionWS.xyz);

    // dither + 垂直clip ------
    // (勿删，用于减少插片十字感，美术需要再添加回来)
    // 
    // float clipPart = 1 - abs(dot(i.baseNormal, viewDir));
    // float dis = distance(float3(_WorldSpaceCameraPos.x, 0, _WorldSpaceCameraPos.z), float3(i.positionWS.x, 0, i.positionWS.z)/*posObj*/);
    // float clipValue = 0;
    // Unity_Dither(smoothstep(_DitherAmountMin,_DitherAmountMax, dis), i.positionHCS.xy, clipValue);
    //clip(min((baseColor.a * _CutIntensity - clipPart), clipValue));
    // dither + 垂直clip ------
    
    #if defined(_ALPHATEST_ON)
    clip(step(_stepA,baseColor.a) - _Cutoff);
    #endif
    
    half3 tintColor = 1;
    //#ifdef _SHADER_LEVEL_HIGH
    half3 tintColorTop = lerp(half3(1, 1, 1), _BaseColor.rgb, _BaseColor.a);
    half3 tintColorRoot = lerp(half3(1, 1, 1), _LerpColor.rgb, _LerpColor.a);
    tintColor = lerp(tintColorRoot, tintColorTop, i.treeParam);
    //#endif
    baseColor.rgb *= tintColor;
    
    // Light
    //Light light = GetMainLight(shadowCoord);
    //Light light = GetMainLight();
    half4 shadowCoord;
        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        shadowCoord = i.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        shadowCoord = TransformWorldToShadowCoord(i.positionWS);
        #else
        shadowCoord = float4(0, 0, 0, 0);
        #endif
    Light light = GetMainLight(shadowCoord,i.positionWS,1,i.normalWS);
    //light.shadowAttenuation =  MainLightRealtimeShadow(shadowCoord);
    half shadowAtten = light.shadowAttenuation;
    //shadowAtten = lerp(1,shadowAtten,_shadowAttenInt);
    shadowAtten = 1;
    half3 lightColor = light.color.rgb;

    half NoL = dot(normalize(i.normalWS), light.direction);
    half ao = saturate(i.ambient.a / _AORange);
    half4 f_finalColor = half4(baseColor.rgb,1);
    f_finalColor.rgb *= lightColor;// 平行光叠加
    half positiveNL = saturate((NoL - _ToonCutPos) /** _ToonCutSharpness*/);
    half darkPart = positiveNL * shadowAtten;
    half GrayPart = saturate(lerp(1, i.ambient.a, _FaceLightGrayIntensity));

    half subSurfaceTerm = 0;
    #ifdef _SUBSURFACE
        half VoL = saturate(-dot(light.direction, viewDir));
        VoL = saturate(VoL*VoL*VoL);
        subSurfaceTerm = saturate(VoL * saturate(NoL - _SubSurfaceScale)) * _SubSurfaceGain * ao;

        #ifndef _COLORORTEX
        half subSurfaceTermInt = lerp(1,baseColorMask.g,_subSurfaceTermInt);
        subSurfaceTerm*=subSurfaceTermInt;
        #endif

    #endif

    TYPE SmartShadow = 1;
    #if defined(_SMARTSOFTSHADOW_ON)
    half noiseCol = SAMPLE_TEXTURE2D(_noiseTexTree, sampler_noiseTexTree, (i.uv*_noiseTexTree_ST.xy+_noiseTexTree_ST.zw)+half2(abs(sin(_Time.y))*_noiseOffestTree,0)).x;
    SmartShadow =  GetSmartShadow(_MainLightPosition.xyz, i.normalWS, half4(i.positionWS.xyz,1), _SmartShadowIntensity,noiseCol,_noiseIntTree);
    #endif

    TYPE darkInt = lerp(_DarkColor.r,_LightIntensity, min(darkPart + GrayPart * saturate(NoL*0.5+ 0.5) * _FaceLightGrayIntensity * shadowAtten + subSurfaceTerm,1));
    f_finalColor.rgb *= min(darkInt,lerp(1,SmartShadow,_SmartShadowInt)); // darkColor,这里*2是为了提亮暗部的亮叶
    // 控制颜色饱和度
    TYPE gray = 0.21 * f_finalColor.x + 0.72 * f_finalColor.y + 0.072 * f_finalColor.z;
    f_finalColor.rgb = lerp(TYPE3(gray, gray, gray), f_finalColor.rgb, _saturate); // 饱和度
    f_finalColor.rgb *= lerp(_AOTint.rgb, 1, ao); // AO 想让AO更可控一些
    f_finalColor.rgb += max(baseColor.rgb *  min(i.ambient.rgb,2) * _SHIntensity,0) ;
    f_finalColor.rgb *= lerp(1,min(1,lightColor),max(0,NoL-_LightSHPow));
    f_finalColor.a = baseColor.a;
    UNITY_FLATTEN
    if(IMPOSTORENABLE)
    {
        f_finalColor.a += _ImpostorAlpha;
    }
    /*
     * 效果不显著 决定暂时关闭 需要的时候再开启
    TYPE hardRimMask = 0;
    TYPE hardRim = 0;
    TYPE hardRimScale = 0;
    #if defined(_FX_LEVEL_HIGH) &&  defined (_HARDRIM)
        float2 HardRimScreenUV = (i.positionHCS.xy / _ScreenParams.xy - 0.5) * (1 + _HardRimWidth) + 0.5;
        #if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
        float depthTex = 1-SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, HardRimScreenUV).r;
        #else
        float depthTex = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, HardRimScreenUV).r;
        #endif
        hardRim = 1 - depthTex.xxx * max(0, _HardRimDistanceIntensity);
        hardRimMask = 1 - step(i.positionHCS.z * 50, _LodMask);
        float distance = length(_WorldSpaceCameraPos - i.positionWS);
        hardRimScale = saturate(max(NoL,0) * hardRim);
        hardRimScale *= step(distance,20);//超过这个距离不显示 防止impostor错误
    //return frac(depthTex);
    #else
        hardRimScale = 0;
    #endif

    f_finalColor.rgb = lerp(f_finalColor.rgb, _HardRimTint.rgb, hardRimScale);
    */
    
    CalcSceneColor(f_finalColor.rgb, light.shadowAttenuation);
    
    f_finalColor.rgb *= _SceneExposure;
    
    APPLY_FOG(f_finalColor, i.positionWS.xyz);

    
    // Debug 用
    #if _DEBUGMODE
        switch(_Debug)
        {
            case 0:
            f_finalColor.rgb =  lerp(_AOTint.rgb, 1, ao);//lerp(float3(1,0,0),float3(0,1,0),shadowAtten)
            break;
            case 1:
            f_finalColor.rgb = subSurfaceTerm;//subSurfaceTerm
            break;
            case 2:
            f_finalColor.rgb = darkPart + subSurfaceTerm;
            break;
            case 3:
            f_finalColor.rgb = lightColor;
            break;
            case 4:
            f_finalColor.rgb = i.ambient.rgb * _SHIntensity;
            break;
            case 5:
            f_finalColor.rgb = i.debugWind;
            break;
            case 6:
            f_finalColor.rgb = i.treeParam;
            break;
            case 7:
            f_finalColor.rgb = 1;
            break;
            case 8:
            f_finalColor.rgb = i.VertexColor.rrr;
            break;
            case 9:
            f_finalColor.rgb = i.VertexColor.ggg;
            break;
            case 10:
            f_finalColor.rgb = SmartShadow;
            break;
            case 11:
            f_finalColor.rgb = shadowAtten;
            break;
        }
    #endif

    
    
    return half4(f_finalColor);
}

#endif

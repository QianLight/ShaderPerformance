#ifndef ROLE_STENCIL_SHADOW_CASTER_INCLUDED
#define ROLE_STENCIL_SHADOW_CASTER_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#if !defined(UNIFORM_PCH_OFF)
float4 _GlobalFaceShadowParam;
#endif
float4 _MainLightDir1;

#define _GlobalFaceShadowOffset _GlobalFaceShadowParam.xy
#define _UseGlobalFaceShadowOffset _GlobalFaceShadowParam.z

struct Attributes
{
    float4 positionOS : POSITION;
};

struct Varings
{
    float4 positionCS : SV_POSITION;
};

Varings vert(Attributes input)
{
    Varings output;
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float4 positionCS = TransformWorldToHClip(positionWS);
    float3 lightDirCS = TransformWorldToHClipDir(-_MainLightDir1.xyz);
    positionCS.xy += lerp(lightDirCS.xy, _GlobalFaceShadowOffset, _UseGlobalFaceShadowOffset) * _FaceShadowParam.x;
    output.positionCS = positionCS;
    return output;
}

float4 frag(Varings input) : SV_Target
{
    return 1;
}

#endif

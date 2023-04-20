#ifndef ROLE_PLANAR_SHADOW_INCLUDED
#define ROLE_PLANAR_SHADOW_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Role_Head.hlsl"

#define _ShadowHight _ShadowPos.y

half4 _PlanarShadowColor;
half4 _PlanarShadowParam;
#define _PlanarShadowDir _PlanarShadowParam.xyz
#define _PlanarShadowFalloff _PlanarShadowParam.w

struct appdata
{
    half4 vertex : POSITION;
    half2 uv : TEXCOORD0;
};

struct v2f
{
    half4 vertex : SV_POSITION;
    half4 color : COLOR;
};

half3 ShadowProjectPos(half3 worldPos)
{
    half3 shadowPos;
    shadowPos.y = min(worldPos.y, _ShadowHight);
    shadowPos.xz = worldPos.xz - _PlanarShadowDir.xz * max(0, worldPos.y - _ShadowHight) / _PlanarShadowDir.y;
    return shadowPos;
}

v2f vert(appdata v)
{
    v2f o;
    half3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    half3 shadowPos = ShadowProjectPos(worldPos);
    o.vertex = mul(unity_MatrixVP, half4(shadowPos, 1));
    half3 temp = cross(half3(0,1,0), _PlanarShadowDir.xyz);
    half3 shadowUp = cross(_PlanarShadowDir.xyz, temp);
    half shadowAlpha = 1 - saturate(dot(shadowUp, worldPos.xyz - GetRootPos().xyz) / _PlanarShadowFalloff);

    // 战斗场景中使用这个算法：
    // o.color.rgb = lerp(1, _PlanarShadowColor, shadowAlpha);
    // o.color.a = 1;
    
    // RT中使用这个算法： 
    o.color = _PlanarShadowColor;
    o.color.a *= shadowAlpha;
    
    return o;
}

half4 frag(v2f i) : SV_Target
{
    return i.color;
}

#endif
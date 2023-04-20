// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef CUSTOM_STANDARD_META_INCLUDED
#define CUSTOM_STANDARD_META_INCLUDED

// Functionality for Standard shader "meta" pass
// (extracts albedo/emission for lightmapper etc.)

#include "UnityCG.cginc"
#include "UnityStandardInput.cginc"
#include "UnityMetaPass.cginc"
#include "UnityStandardCore.cginc"

struct v2f_meta
{
    float4 uv       : TEXCOORD0;
    float4 pos      : SV_POSITION;
    float3 worldPos : TEXCOORD1;
};

v2f_meta vert_meta (VertexInput v)
{
    v2f_meta o = (v2f_meta)0;
    o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
    o.uv = TexCoords(v);
    o.worldPos  == mul(unity_ObjectToWorld, v.vertex).xyz;
    return o;
}

// Albedo for lightmapping should basically be diffuse color.
// But rough metals (black diffuse) still scatter quite a lot of light around, so
// we want to take some of that into account too.
half3 UnityLightmappingAlbedo (half3 diffuse, half3 specular, half smoothness)
{
    half roughness = SmoothnessToRoughness(smoothness);
    half3 res = diffuse;
    res += specular * roughness * 0.5;
    return res;
}
half4 _Color0;
#define _MainColor _Color0
sampler2D _ProcedureTex0;

half4 _Param0;
#define _ParamRoughness _Param0.z
#define _ParamMetallic _Param0.w

// sampler2D _MainTex;
sampler2D _MainTex1;
sampler2D _MainTex2;
sampler2D _MainTex3;

#define _TerrainScale _Param0

half4 _uvST;
half4 _uvST1;
half4 CustomAlbedo(float4 texcoords,float3 worldPos)
{
    #ifdef _TERRAIN
        half4 splat = half4(0,0,0,0);
        #ifdef _SPLAT1
            half2 uv0 = worldPos.xz*_TerrainScale.x;		
            splat += tex2D(_MainTex, uv0);
        #endif
        #ifdef _SPLAT2
            half4 blend = tex2D(_BlendTex, texcoords.xy);
            half2 uv0 = worldPos.xz*_TerrainScale.x;
            half2 uv1 = worldPos.xz*_TerrainScale.y;

            splat += tex2D(_MainTex, uv0)*blend.r;
            splat += tex2D(_MainTex1, uv1)*blend.g;
        #endif
        #ifdef _SPLAT3
            half4 blend = tex2D(_BlendTex, texcoords.xy);
            half2 uv0 = worldPos.xz*_TerrainScale.x;
            half2 uv1 = worldPos.xz*_TerrainScale.y;
            half2 uv2 = worldPos.xz*_TerrainScale.z;
            splat += tex2D(_MainTex, uv0)*blend.r;
            splat += tex2D(_MainTex1, uv1)*blend.g;
            splat += tex2D(_MainTex2, uv2)*blend.b;
        #endif
        #ifdef _SPLAT4
            half4 blend = tex2D(_BlendTex, texcoords.xy);
            half2 uv0 = worldPos.xz*_TerrainScale.x;
            half2 uv1 = worldPos.xz*_TerrainScale.y;
            half2 uv2 = worldPos.xz*_TerrainScale.z;
            half2 uv3 = worldPos.xz*_TerrainScale.w;
            splat += tex2D(_MainTex, uv0)*blend.r;
            splat += tex2D(_MainTex1, uv1)*blend.g;
            splat += tex2D(_MainTex2, uv2)*blend.b;
            splat += tex2D(_MainTex3, uv3)*blend.a;
        #endif
        half4 albedo = splat;// half4(0, blend.g, 0, 1);	
    #else
		#ifdef _MESH_BLEND
			half4 splat = half4(0,0,0,0);
			half2 uv0 = texcoords.xy*_uvST.xy+_uvST.wz;
			half2 uv1 = texcoords.xy*_uvST1.xy+_uvST1.wz;
			half4 blend = tex2D(_BlendTex,texcoords.xy);
			half4 base0 = tex2D(_BaseTex,uv0);
			half4 base1 = tex2D(_MainTex1,uv1);				
			splat += base0*blend.r*_MainColor*_MainColor.a*10;
			splat += base1*blend.g*_Color0*_Color0.a*10;
		//	MaterialData.BlendColor = blend;
			//MaterialData.MetallicScale = blend.r*_SpecScale0+blend.g*_SpecScale1;	
				half4 albedo = splat;
		#else//!_MESH_BLEND
				half4 albedo = _MainColor * tex2D (_MainTex, texcoords.xy);
		#endif
    #endif
    return albedo;
}



half2 CustomMetallicRough(float2 uv)
{
    half2 mg = half2(0,1);
#if defined(_PBS_FROM_PARAM)
		mg.g = 1-_ParamRoughness;
		mg.r = _ParamMetallic;
#else//!_PBS_FROM_PARAM
		float4 pbs = tex2D(_ProcedureTex0, uv);
        mg.r = 1-pbs.w;
        mg.g = pbs.z;
    #if defined(_PBS_HALF_FROM_PARAM)
        mg.g = 1-_ParamRoughness;
        mg.r = _ParamMetallic;
    #endif//_PBS_HALF_FROM_PARAM
#endif//_PBS_FROM_PARAM

    return mg;
}

inline FragmentCommonData CustomRoughnessSetup(float4 i_tex,float3 i_worldPos)
{
    half2 metallicGloss = CustomMetallicRough(i_tex.xy);
    half metallic = metallicGloss.x;
    half smoothness = metallicGloss.y; // this is 1 minus the square root of real roughness m.

    half oneMinusReflectivity;
    half3 specColor;
    half4 albedo = CustomAlbedo(i_tex,i_worldPos);
    half3 diffColor = DiffuseAndSpecularFromMetallic(albedo, metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);
    half alpha = albedo.a;

    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    o.alpha = alpha;
    return o;
}
float4 _Param1;
#define _Cutout _Param1.w
float4 frag_meta (v2f_meta i) : SV_Target
{
    // we're interested in diffuse & specular colors,
    // and surface roughness to produce final albedo.
    FragmentCommonData data = CustomRoughnessSetup (i.uv,i.worldPos);
#if defined(_ALPHATEST_ON)
    clip (data.alpha - _Cutoff);
#endif
    UnityMetaInput o;
    UNITY_INITIALIZE_OUTPUT(UnityMetaInput, o);

#if defined(EDITOR_VISUALIZATION)
    o.Albedo = data.diffColor;
#else
    o.Albedo = UnityLightmappingAlbedo (data.diffColor, data.specColor, data.smoothness);
#endif
    o.SpecularColor = data.specColor;
    o.Emission = Emission(i.uv.xy);

    return UnityMetaFragment(o);
}

#endif // CUSTOM_STANDARD_META_INCLUDED

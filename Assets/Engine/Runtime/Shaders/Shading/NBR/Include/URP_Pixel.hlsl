#ifndef SHADER_STAGE_FRAGMENT
#define SHADER_STAGE_FRAGMENT
#endif
#include "DefaultDebug.hlsl"
#include "Fog.hlsl"
#include "MaterialTemplate.hlsl" 
#if defined(URP_BASE)
#include "SRP_Lighting.hlsl" 
#else
// #include "Lighting.hlsl" 
#endif


#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Role/Role_Additional_Light_Input.hlsl" 
#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Role/Role_Additional_Light.hlsl" 

#ifndef PBS_PIXEL_INCLUDE
#define PBS_PIXEL_INCLUDE

FLOAT _DitherTransparency;
#ifndef _NOIMPOSTOR
FLOAT _ImpostorEnable;
#define IMPOSTORENABLE _ImpostorEnable>0
FLOAT _ImpostorAlpha;
#endif


half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

void Frag(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout REAL4 rt0,inout REAL4 rt1, REAL facing)
{
		half3 baseColor = 0;
#ifdef _CUSTOM_PS
		rt0 = CustomPS(Interpolants,SvPosition,rt1);
		baseColor = rt0.xyz;
#else//_CUSTOM_PS
		DEBUG_CUSTOMDATA
		FFragData FragData = GetFragData(Interpolants, SvPosition);
		FragData.facing = facing;
		FMaterialData MaterialData = GetMaterialData(FragData);
	#ifdef _FRAMEBUFFER_FETCH
		MaterialData.FrameBuffer = outColor;
	#endif//_FRAMEBUFFER_FETCH
		REAL4 Color = CalcLighting(FragData, MaterialData DEBUG_PARAM);
		DEBUG_CUSTOMDATA_PARAM(LightOutput, Color.xyz)
	

		//Rim/Add
		Color.xyz = CalcColorEffect(MaterialData, Color.xyz);

		rt0 = Color;
#ifndef _NO_MRT
     
		rt1.xyz = EncodeFloatRGB(FragData.depth01);
		rt1.w = EncodeAlpha(MaterialData.BloomIntensity, _IsRt1zForUIRT);
#endif
		DEBUG_PBS_COLOR(rt0, FragData, MaterialData)

		baseColor = MaterialData.BaseColor.xyz;
#endif//_CUSTOM_PS

#ifdef _ROLE_ADDITIONAL_LIGHT_ON
		half3 addlight = RoleAdditionalLighting(Interpolants,baseColor);
		rt0.rgb += addlight;
#endif	
	// UNITY_BRANCH
	// if (_ScatterEnable)
	// {
	// 	float depth01;
	// 	float3 scatter = GetScatterRGB(normalize(Interpolants.WorldPosition - _WorldSpaceCameraPos), Interpolants.WorldPosition, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
	// 	rt0.rgb = lerp(rt0.rgb, scatter, saturate(depth01 * 2));
	// }
	APPLY_FOG(rt0, Interpolants.WorldPosition.xyz);
	#if defined(OUTLINE_DEFINED) || defined(_Role_Lighting)
		DitherTransparent(SvPosition.xy, _DitherTransparency);
	#else
		SphereDitherTransparent(SvPosition, _DitherTransparency);
	#endif
	
	#ifdef _SCENE_EFFECT
	rt0.rgb *= _SceneExposure;
	#endif
	
		#if defined(_MIN_MAX_LIGHT)
    	//rt0.rgb = min(rt0.rgb,0.65f*rt0.rgb);
    	rt0.rgb = max(rt0.rgb,MaterialData.BaseColor*0.7f);
	rt0.rgb*= 1.5f;
    	rt0.rgb = min(rt0.rgb,MaterialData.BaseColor*1.27f);
    	#endif

	#if defined( _SHADER_DEBUG) && defined(_SRP_DEBUG)
		half4 debugColor = GetDebugColor(FragData, MaterialData);
		rt0 = debugColor;
	#endif
}

#ifdef _FRAMEBUFFER_FETCH
	void fragForwardBase(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position,
		inout FLOAT4 rt0 : SV_Target0,in FLOAT3 rt1 : SV_Target1)
{
	rt1.w = _IsRt1zForUIRT;
	Frag(vs2ps,SvPosition,rt0,rt1);
	#ifndef _NOIMPOSTOR
	rt0.w += _ImpostorAlpha;
	#endif
}

#else//_FRAMEBUFFER_FETCH
		REAL4 fragForwardBase(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, REAL facing : VFACE) : SV_Target
		{
			REAL4 rt0 = 0;
			REAL4 rt1 = REAL4(0, 0, 0, _IsRt1zForUIRT);
			Frag(vs2ps,SvPosition,rt0,rt1,facing);
			#ifndef _NOIMPOSTOR
			UNITY_FLATTEN
			if(IMPOSTORENABLE)
			{
				rt0.w += _ImpostorAlpha;
			}
			#endif

			
			// #if _ALPHA_TEST_IMPOSTOR
			// rt0.w += _ImpostorAlpha;
			// #endif

			// Add by Takeshi:
			// For MatEffect Dark. ID: 1200
			// Just effect on situation of Alpha in 1;
			#ifdef ROLE_EFFECT
			rt0.rgb = _Color.a < 1? rt0.rgb : lerp(0.05, rt0.rgb, _Color.r);
			#endif
			// End Add
			return rt0;
		}
#endif//_FRAMEBUFFER_FETCH
#endif //PBS_PIXEL_INCLUDE

half4 fragForwardPreDepth(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, REAL facing : VFACE) : SV_Target
{
	REAL2 uv = vs2ps.TexCoords[0].xy;
	REAL4 color =  SAMPLE_TEX2D(_MainTex, uv);
	#ifndef _ALLOW_CROSS_FACE	//如果不允许十字面，则通过ddx,ddy（顶点到摄像机的距离）来裁剪平行于摄像机的屏幕
		// REAL dist = length(vs2ps.WorldPosition.xyz - _WorldSpaceCameraPos);
		// // REAL _ddx = ddx(dist);
		// // REAL _ddy = ddy(dist);
		// // REAL ddsq = _ddx * _ddx + _ddy * _ddy;
		// REAL2 _ddxddy = REAL2(ddx(dist),ddy(dist));
		// REAL ddsq = dot(_ddxddy,_ddxddy);
		// REAL ddxy = saturate(ddsq / dist /dist * 100000);
		// clip(color.a - ddxy - 0.3); //在华为P40上会被完全裁掉不显示,具体原因不明

		//效果没有ddx,ddy好,但兼容性好
		REAL dist = length(vs2ps.WorldPosition.xyz - _WorldSpaceCameraPos);
		half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - vs2ps.WorldPosition.xyz);
		float4 zAxis = GetWorldToViewMatrix()[2];
		half zAxisThreshold =  dot(viewDir, zAxis.xyz) * 0.5 + 0.5;
		half disThreshold = smoothstep(0,15, dist);
		half cutThreshold = saturate(lerp(zAxisThreshold, _Cutout, disThreshold));
		clip(color.a - cutThreshold);
	#else
		clip(color.a - _Cutout);
	#endif

	#if defined(OUTLINE_DEFINED) || defined(_Role_Lighting)
	DitherTransparent(SvPosition.xy, _DitherTransparency);
	#else
	SphereDitherTransparent(SvPosition, _DitherTransparency);
	#endif
	
	return REAL4(0.3,1,0.2,1);
}
Shader "Hidden/Custom/Tools/ShadowCaster" 
{
	Properties
	{
		_MainTex ("Base Tex", 2D) = "white" {}
	}

	HLSLINCLUDE
	// #define _SHADOW_CASTER
	ENDHLSL

	Subshader 
	{
		Tags{ "RenderType" = "Opaque" "IgnoreProjector" = "True"}	
		Pass
		{
			Name "ShadowCaster"
			ZTest LEqual
			ColorMask 0
			Cull off
			HLSLPROGRAM
			#pragma target 3.0

			#include "../StdLib.hlsl"
			#include "../Include/PCH.hlsl"			
			#include "../Include/LightingHead.hlsl"
			#include "../Include/ShadowLib.hlsl"

			#pragma shader_feature_local _ _ALPHA_TEST
			#pragma vertex vertCustomCast
			#pragma fragment vertCustomFrag

			ENDHLSL
		}
		
		Pass
		{
			Name "ShadowCasterSelf"
			ZTest LEqual
			// Cull front

			HLSLPROGRAM
			#pragma target 3.0

			#define _SELF_SHADOW

			#include "../StdLib.hlsl"
			#include "../Include/PCH.hlsl"
			#include "../Include/ShadowLib.hlsl"
			#include "../Include/LightingHead.hlsl"

			#pragma shader_feature_local _ _ALPHA_TEST

			#pragma vertex vertSelfShadowCast
			#pragma fragment vertCustomFrag

			FLOAT4 _SelfShadowDir;
			#define _DepthBias _SelfShadowParam.x
			#define _NormalBias _SelfShadowParam.y
			
			FLOAT GetSlopeScaleBias(FLOAT baseBias, FLOAT maxBias, FLOAT3 normal, FLOAT3 lightDir)
			{
				FLOAT cos_val = saturate(dot(normal, lightDir));
				FLOAT sin_val = sqrt(1 - cos_val * cos_val);
				FLOAT tan_val = sin_val/cos_val;
				FLOAT bias = baseBias + clamp(tan_val, 0, maxBias);

				//FLOAT NdotL = saturate(dot(normal, lightDir));
				//FLOAT bias = baseBias * tan(acos(NdotL));
				//bias = clamp(bias, 0, maxBias);

				return bias;
			}

			FLOAT4 ApplyDepthBias(FLOAT4 clipPos, FLOAT depthBias)
			{
#if defined(UNITY_REVERSED_Z)
				clipPos.z -= saturate(depthBias.x / clipPos.w);
#else
				clipPos.z += saturate(depthBias.x / clipPos.w);
#endif

				return clipPos;
			}

			FLOAT3 ApplyNormalBias(FLOAT3 worldPos, FLOAT3 worldNormal, FLOAT3 shadowLightForwardDir, FLOAT normalBias)
			{
				FLOAT shadowCos = dot(worldNormal, _SelfShadowDir.xyz);
				FLOAT shadowSine = sqrt(1 - shadowCos * shadowCos);
				normalBias = normalBias * shadowSine;
				worldPos.xyz -= worldNormal * normalBias;
				return worldPos;
			}

			VaryingsShadow vertSelfShadowCast(AttributesShadow v)
			{
				VaryingsShadow o;
				FLOAT3 worldPos = mul(unity_ObjectToWorld, FLOAT4(v.vertex.xyz, 1.0f)).xyz;
				FLOAT3 worldNormal = mul(FLOAT4(v.normal, 1.0f), unity_WorldToObject).xyz;
				worldNormal = normalize(worldNormal);

				FLOAT3 worldLightDir = _SelfShadowDir.xyz;

				worldPos = ApplyNormalBias(worldPos, worldNormal, worldLightDir, _NormalBias);

				FLOAT4 clipPos = mul(_SelfShadowVP, FLOAT4(worldPos, 1));
				FLOAT maxBias = 0.005;
				FLOAT slopeBias = GetSlopeScaleBias(_DepthBias, maxBias, worldNormal, worldLightDir);

				clipPos = ApplyDepthBias(clipPos, slopeBias);
				o.vertex = clipPos;

#ifdef _ALPHA_TEST
				o.uv = v.uv;
#endif

				return o;
			}
			ENDHLSL
		}

		Pass
		{
			Name "ShadowCasterSelf2"
			ZTest LEqual
			// Cull front

			HLSLPROGRAM
			#pragma target 3.0

			#define _SELF_SHADOW2

			#include "../StdLib.hlsl"
			#include "../Include/PCH.hlsl"
			#include "../Include/ShadowLib.hlsl"
			#include "../Include/LightingHead.hlsl"

			#pragma shader_feature_local _ _ALPHA_TEST

			#pragma vertex vertSelfShadowCast
			#pragma fragment vertCustomFrag

			FLOAT4 _SelfShadowDir;
			#define _DepthBias _SelfShadowParam.x
			#define _NormalBias _SelfShadowParam.y
			
			FLOAT GetSlopeScaleBias(FLOAT baseBias, FLOAT maxBias, FLOAT3 normal, FLOAT3 lightDir)
			{
				FLOAT cos_val = saturate(dot(normal, lightDir));
				FLOAT sin_val = sqrt(1 - cos_val * cos_val);
				FLOAT tan_val = sin_val/cos_val;
				FLOAT bias = baseBias + clamp(tan_val, 0, maxBias);

				//FLOAT NdotL = saturate(dot(normal, lightDir));
				//FLOAT bias = baseBias * tan(acos(NdotL));
				//bias = clamp(bias, 0, maxBias);

				return bias;
			}

			FLOAT4 ApplyDepthBias(FLOAT4 clipPos, FLOAT depthBias)
			{
#if defined(UNITY_REVERSED_Z)
				clipPos.z -= saturate(depthBias.x / clipPos.w);
#else
				clipPos.z += saturate(depthBias.x / clipPos.w);
#endif

				return clipPos;
			}

			FLOAT3 ApplyNormalBias(FLOAT3 worldPos, FLOAT3 worldNormal, FLOAT3 shadowLightForwardDir, FLOAT normalBias)
			{
				FLOAT shadowCos = dot(worldNormal, _SelfShadowDir.xyz);
				FLOAT shadowSine = sqrt(1 - shadowCos * shadowCos);
				normalBias = normalBias * shadowSine;
				worldPos.xyz -= worldNormal * normalBias;
				return worldPos;
			}
			
			VaryingsShadow vertSelfShadowCast(AttributesShadow v)
			{
				VaryingsShadow o;
				FLOAT3 worldPos = mul(unity_ObjectToWorld, FLOAT4(v.vertex.xyz, 1.0f)).xyz;
				FLOAT3 worldNormal = mul(FLOAT4(v.normal, 1.0f), unity_WorldToObject).xyz;
				worldNormal = normalize(worldNormal);

				FLOAT3 worldLightDir = _SelfShadowDir.xyz;

				worldPos = ApplyNormalBias(worldPos, worldNormal, worldLightDir, _NormalBias);

				FLOAT4 clipPos = mul(_SFVP, FLOAT4(worldPos, 1));
				FLOAT maxBias = 0.005;
				FLOAT slopeBias = GetSlopeScaleBias(_DepthBias, maxBias, worldNormal, worldLightDir);

				clipPos = ApplyDepthBias(clipPos, slopeBias);
				o.vertex = clipPos;

#ifdef _ALPHA_TEST
				o.uv = v.uv;
#endif

				return o;
			}
			ENDHLSL
		}
	}
}
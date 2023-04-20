Shader "Custom/Scene/Skybox" 
{
	Properties 
	{
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)	    
	    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
	    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
		// _RainbowDir("Rainbow Direction",Vector) = (1,0,0)
		_RotateSpeed("Rotate Speed",Float) = 0.5
	}

	HLSLINCLUDE
		#define _VERTEX_FOG
	ENDHLSL

	SubShader 
	{
	    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }

	    Cull Off ZWrite Off
	    Pass 
	    {
			Name "SKYEFFECT"
			Tags{"LightMode"="ForwardBase"}
			HLSLPROGRAM
			#pragma target 3.0
			// #pragma only_renderers d3d11
	        // #pragma shader_feature WEATHER_EFFECT_ON
	        // #pragma shader_feature RAIN_EFFECT_ON
	        // #pragma shader_feature RAINBOW_ON
	        // #pragma shader_feature THUNDER_ON
	        // #pragma shader_feature STAR_ON
			
			half4 _MainLightColor0;
			half4 _MainLightDir0;

			#include "Scene_SkyPCH.hlsl" 
			
			void GetData(inout FLOAT4 tex,inout FLOAT3 tint,inout FLOAT exposure,in v2f i)
			{
				tex = SAMPLE_TEXCUBE (_Tex, i.texcoord);
				tint = _Tint.rgb;
				exposure = _Exposure;
			}
			#include "Scene_Sky.hlsl"
			 
			#pragma vertex vert
	        #pragma fragment frag	
			
			
			ENDHLSL	       
	    }
		Pass
		{
			Name "OverdrawSkybox"
			Tags{"LightMode" = "OverdrawSkybox" "Queue"="Background" "RenderType"="Background"}
			
			ZWrite Off
			Cull Off
			Blend One One
			HLSLPROGRAM
			#include "../Scene/Scene_SkyPCH.hlsl"
			#include "../Include/Head.hlsl"
			// #pragma only_renderers d3d11
			// #include "../../Scene/Scene_Sky.hlsl"

			#pragma vertex vertOverdraw
			#pragma fragment fragskyoverdraw
			v2f vertOverdraw (appdata_t v)
			{
				v2f o;
				// FLOAT3 rotated = RotateAroundYInDegrees(v.vertex.xyz,_Time.y*_RotateSpeed);
				FLOAT4 worldPos = mul(unity_ObjectToWorld, FLOAT4(v.vertex.xyz, 1.0));
				o.vertex = mul(unity_MatrixVP, worldPos);
				o.texcoord = v.vertex.xyz;
				o.depth01 = o.vertex.zw;
				o.WorldPosition = worldPos;
				return o;
			}		
			MRTOutput fragskyoverdraw (v2f i)// : SV_Target
			{
				
				DECLARE_OUTPUT(MRTOutput, mrt);
				mrt.rt0 = half4(0.1, 0.04, 0.02, 0);
				mrt.rt1.xyz = EncodeFloatRGB(i.depth01.x/i.depth01.y);
				SET_BLOOM(mrt, EncodeAlpha(1, _IsRt1zForUIRT));
				return mrt;
			}

			ENDHLSL
		}
	}


	Fallback Off

}

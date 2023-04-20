Shader "URP/Scene/Skybox" 
{
	Properties
	{
		_BaseTex ("Base Tex", Cube) = "white" {}
		_MaskTex("MaskTex", Cube) = "black" {}
	}

	HLSLINCLUDE
		//float _LightingOnBool;
		// #define PRECOMPUTE_SCATTER
		// #define PRECOMPUTE_SCATTER_LEGACY
		#define FOG_NOISE_OFF

		#ifdef _FAKELIGHT
		half4 _FakeLightDir;
		// #else
		#endif
		half4 _MainLightDir0;
		half4 _MainLightColor0;
	
		#include "../StdLib.hlsl"
		#include "../../Include/PrecomputedScatter.hlsl"
		#include "../Include/Fog.hlsl"

		TEXCUBE_SAMPLER(_BaseTex);
		TEXCUBE_SAMPLER(_MaskTex);

		TEXCUBE_SAMPLER(_TestCurrentBaseTex);
		TEXCUBE_SAMPLER(_TestTargetBaseTex);
	
		TEXCUBE_SAMPLER(_TestCurrentMaskTex);
		TEXCUBE_SAMPLER(_TestTargetMaskTex);

		half4 _ExplorationParams;
	
	FLOAT4 _SinCosDegree;
		
		// FLOAT3 _SssColor;

		// FLOAT4 _Tint;
		FLOAT4 _SkyboxSunColor;
		FLOAT4 _CurvesA;
		FLOAT4 _CurvesB;
		FLOAT4 _CurvesC;
		FLOAT4 _SkyboxParams0;
		float4 _CloudDarkColor;
		float4 _CloudLightColor;
		#define _FadeStart (_SkyboxParams0.xz)
		#define _FadeEnd (_SkyboxParams0.yw)
		FLOAT4 _SkyboxParams1;
		#define _SunSize (_SkyboxParams1.x)
		#define _RotateSpeed (_SkyboxParams1.y)
		#define _SunBloomIntensity (_SkyboxParams1.z)
		#define _CloudBloomIntensity (_SkyboxParams1.w)
		half4 _SceneColorAdjustmentParams;
		#define _SceneExposure _SceneColorAdjustmentParams.x

		FLOAT _SkyboxFogStart;
		FLOAT _SkyboxFogEnd;
		FLOAT _FogPower;
		float4 _BorderColor;
		float _BorderEmissionRange;
		float _BorderRange;
		float _SkyScatterFalloff;

		FLOAT4 _SceneColor;
		FLOAT _SunFlareFalloff;
		FLOAT4 _SunFlareColor;

		FLOAT4 _SkyLowerColor;
		FLOAT4 _SkyUpperColor;
		FLOAT _SkyHorizon;
		FLOAT _SkyHorizonTilt;

		FLOAT _AtmosStrength;
		// FLOAT _AtmospherScattering;

	#ifdef _LIGHTING_ON
		float _LightingPTRotX;
		float _LightingPTRotY;
		float _LightingPTSize;
		float _FlashFrequency;
		float4 _LightingColor;
	#endif
		float _SkyFinalExposure;
	
		struct Attributes
		{
			FLOAT4 vertex : POSITION;
		};

		struct Varyings
		{
			FLOAT4 Position : SV_POSITION;
			FLOAT3 WorldPos : TEXCOORD0;
			FLOAT3 uv : TEXCOORD1;
		};
		float3 CalcRotation( float RotX, float RotY )
		{
			RotY = RotY / 180 * 3.141592;
			RotX = RotX / 180 * 3.141592;
			float z = cos(RotY) * cos(RotX);
			float x = sin(RotY) * cos(RotX);
			float y = sin(RotX);
			float3 pos = float3(x,y,z);
			return normalize(pos);
		}
		FLOAT3 RotateAroundYInDegrees (FLOAT3 vertex,FLOAT degree)
		{
			FLOAT sina = _SinCosDegree.x, cosa = _SinCosDegree.y;
			sincos(degree * PI / 180.0, sina, cosa);
			FLOAT2x2 m = FLOAT2x2(cosa, -sina, sina, cosa);
			return FLOAT3(mul(m, vertex.xz), vertex.y).xzy;
		}
		
        Varyings vert (Attributes v)
        {
            Varyings Output;
			FLOAT3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Time.y*_RotateSpeed);
			Output.WorldPos = mul(unity_ObjectToWorld, FLOAT4(rotated, 1.0)).xyz;
			Output.Position = mul(unity_MatrixVP, FLOAT4(Output.WorldPos, 1.0));
			Output.uv = v.vertex.xyz;
            return Output;
        }

		half4 SkyboxCubemap(FLOAT3 cubeUV, FLOAT3 vdir, FLOAT vdl, FLOAT3 scatter, out FLOAT fog,float4 baseTex,float4 cloudTex)
		{
			float svdl = max(0, vdl);
			// float3 tex = scatter;
			// FLOAT4 tex = SAMPLE_TEXCUBE(_BaseTex, cubeUV);
			half2 fades = linearstep(_FadeEnd, _FadeStart, vdir.y);
			fades *= fades;
			float4 combine = FLOAT4(1, svdl, fades);
		
			float4 curves = reciprocalcurve(_CurvesA, _CurvesB, _CurvesC, combine);
		
			#define sssAlphaFade (curves.x)
			#define sssDirectionalFade (curves.y)
			#define fogFade (curves.z)
			#define scatterFade (curves.w)
			// FLOAT sss = sssDirectionalFade * sssAlphaFade;
			half4 color;
		
			half4 cloudColor = lerp(_CloudDarkColor,_CloudLightColor,cloudTex.z);
			cloudColor =lerp(baseTex,cloudColor * baseTex,baseTex.a);
			// float4 baseColor = lerp(float4(tex,1),cloudColor,pow(cloudTex.a,_BorderRange));
			// color.rgb = mad(sss, _SssColor, baseTex.rgb) * _Tint.rgb;
			color.rgb = baseTex.rgb;
			color.rgb = lerp(color.rgb,cloudColor,baseTex.a);
			// color.rgb = lerp(scatter.rgb * baseTex.rgb, color.rgb, _AtmospherScattering/60);
		#ifndef _FX_LEVEL_LOW
			color.rgb = lerp(lerp(baseTex.rgb, scatter.rgb * baseTex.rgb, saturate(_AtmospherScattering/12)), color.rgb, scatterFade);
		#endif
			color.rgb = lerp(color.rgb, cloudColor.rgb, saturate(pow(baseTex.a,_BorderRange)));
			color.a = baseTex.a/* * _Tint.a*/;
		
			fog = fogFade;
			return half4(color.rgb,color.a);
		}
		

		FLOAT4 Sun(FLOAT vdl)
		{
			FLOAT4 sun = _SkyboxSunColor;
			sun.a *= vdl > 1 - _SunSize;
			return sun;
		}

		FLOAT2 Rotate(FLOAT2 uv, FLOAT2 center, FLOAT rotation)
		{
			float sinR = sin(rotation);
			float cosR = cos(rotation);
			float2x2 matR = float2x2(cosR, sinR, -sinR, cosR);
			uv -= center;
			uv = mul(matR, uv);
			return uv;
		}
		FLOAT3 calSunFlare(float2 sunSpaceUV, half rotation)
		{
			half2 SunFlare_Temp2 = min( sunSpaceUV / half2(sunSpaceUV.y, sunSpaceUV.x), rotation.xx );
			half sunFlare_Temp3 = 2.0 - ( SunFlare_Temp2.x + SunFlare_Temp2.y );
			half sunFlare_Temp4 = sunSpaceUV.x + sunSpaceUV.y;
			return saturate( ( ( sunFlare_Temp3 * sunFlare_Temp3 - 1.5 ) * sunFlare_Temp3  + 1.0 ) / sunFlare_Temp4  * sunFlare_Temp4 );
		}
		struct FragOut 
		{
			half4 color : SV_Target;
		// #ifdef _FX_LEVEL_HIGH
		// 	float depth : SV_Depth;
		// #endif
			
		};


// 		FragOut fragRe (Varyings Input)
// 		{
// 			FLOAT3 total;
// 			FLOAT3 viewDir = normalize(Input.WorldPos - _WorldSpaceCameraPos);
// 			
// 			/*===  Light Dir  ===*/
//         	float3 realLightDir = _MainLightDir0;
// 			#ifdef _FAKELIGHT
//         	realLightDir = _FakeLightDir.xyz;
//         	#endif
// 			FLOAT vdl = dot(viewDir, realLightDir.xyz);
// 			FLOAT svdl = max(0, vdl);
// 			float3 posWS = normalize( float3(Input.WorldPos.x,Input.WorldPos.y *3.0,Input.WorldPos.z) );
//
// 			/*==========  Fade  ==========*/
// 			FLOAT2 fades = linearstep(_FadeEnd, _FadeStart, viewDir.y);
// 			fades *= fades;
// 			FLOAT4 combine = FLOAT4(1, svdl, fades);
// 			//sss sssDirectional fog scatter
// 			FLOAT4 curves = reciprocalcurve(_CurvesA, _CurvesB, _CurvesC, combine);
// 			#define fogFade (curves.z)
// 			#define scatterFade (curves.w)
//
// 			
// 			/*=========  Cloud  =========*/
// 			FLOAT4 cloud = SAMPLE_TEXCUBE(_BaseTex, Input.uv);
// 			cloud.rgb = cloud.rgb * cloud.a;
//
// 			/*==========  Sun  ==========*/
// 			float sunInfluenceCloudRange = pow(max(0, dot(normalize(Input.WorldPos), realLightDir.xyz)),_BorderEmissionRange);
// 			FLOAT4 sun = Sun(vdl);
// 			sun.a *= 1 - cloud.a/* * 0.995*/;//太阳被云挡住
//
// 			FLOAT3 backgroundScatter = ScatterNoneNegative(pow(vdl * 0.5 + 0.5, _SkyScatterFalloff), viewDir.y);
//
// 			/*======== Sun flare ========*/
//    			float3 sunSideDir = cross(realLightDir, float3(1,0,0));
//    			float3 sunUpDir = cross(realLightDir, sunSideDir);
// 			half2 sunSpaceUV = abs(half2(dot( half4( viewDir, 0.0 ), sunUpDir ), dot( half4( viewDir, 0.0 ), sunSideDir ) ) );
// 			sunSpaceUV = max(sunSpaceUV, Rotate(sunSpaceUV, 0, 0.3925));
// 			half sunFlareRotation = _SinTime.w * 0.25 + 0.95;
// 			half3 sun_flare = _SunFlareColor.rgb * _SunFlareColor.a * saturate(sunInfluenceCloudRange * sunInfluenceCloudRange - _SunFlareFalloff);
// 			sun_flare *= calSunFlare(sunSpaceUV, sunFlareRotation);
// 			
// 			/*======= Background  =======*/
// 			float3 mulLight = realLightDir * _SkyHorizonTilt;
// 			float3 horizonFalloff = float3(mulLight.x, _SkyLowerColor.a, mulLight.z);
// 			float vdhf = dot(viewDir, horizonFalloff);
// 			float3 background = _SkyUpperColor + (_SkyLowerColor - _SkyUpperColor) * pow(saturate( _SkyHorizon - vdhf ), 1.0 );
// 			background = lerp(background, backgroundScatter, fades.y * 0.5 *_AtmosStrength) + sun_flare;
// 			
//
// 			/*====== Cloud Sun Mix ======*/
// 			float4 cloudData = SAMPLE_TEXCUBE(_MaskTex,Input.uv);
// 			float cloudLightenEdgeMask = cloudData.y;
// 			FLOAT3 cloudLightenBorder = cloudLightenEdgeMask * background;
// 			FLOAT3 cloudSunBorder = cloudLightenBorder /** sunInfluenceCloudRange*/ * _BorderColor;
// 			// float3 cloudLitColor = lerp(_CloudDarkColor,_CloudLightColor,cloudData.z) * cloud.rgb;
// 			FLOAT3 baseCloud = cloud.rgb;
// 			cloud.rgb = lerp(cloud.rgb, cloud.rgb * _CloudLightColor.rgb, cloudData.z * _CloudLightColor.a);
// 			cloud.rgb = lerp(cloud.rgb, cloud.rgb * _CloudDarkColor.rgb,saturate(cloud.a * (1 - cloudData.z) * _CloudDarkColor.a * (1 - cloudData.y)));
// 			FLOAT3 cloudNormal = float3(cloudData.x, -cloudData.w, 1);
// 			FLOAT3 cloudBi = cross(viewDir, float3(0,1,0));
// 			FLOAT3 cloudN = cross(viewDir, cloudBi);
// 			cloudNormal = mul(cloudNormal, half3x3(cloudBi, cloudN, viewDir));
// 			float cndl = dot(cloudNormal, -realLightDir);
// 			cloud.rgb +=  _CloudLightColor.rgb *saturate(cndl.xxx) * cloud.a;
// 			// total = cloudData.z * _CloudDarkColor.a * (1 - cloudData.y);
// 			// cloud.rgb *= (1-cloudLightenEdgeMask);
// 			// cloud.rgb += cloudLightenEdgeMask * baseCloud.rgb * _BorderColor * cloud.a;
// 			// total = cloud;
// 			total = cloud.rgb * _SkyFinalExposure + background * (1 - cloud.a) * _SkyFinalExposure * 0.2 + cloudSunBorder;
// 			total = lerp(total, sun.rgb * _SkyFinalExposure, sun.a);
// 			total = lerp(total,_FogEndColor.rgb, fogFade);
//    			total *= _SceneExposure;
// 			
// 			APPLY_FOG(total, _WorldSpaceCameraPos + normalize(viewDir) * LinearEyeDepth(1));
// 			
// 			FragOut output;
// #if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
// 			output.depth = /*sun.g * (1 - tex.a) > 0.5f ? 0.9999f :*/ 1;
// #else
// 			output.depth = /*sun.g * (1 - tex.a) > 0.5f ? 0.0001f : */0;
// #endif	 
// 			output.color = FLOAT4(total, 1);
// 			return output;
// 		}
		FragOut frag (Varyings Input)
        {
            float3 viewDir = normalize(Input.WorldPos - _WorldSpaceCameraPos);
        	half3 realLight = _MainLightDir0;
        	#ifdef _FAKELIGHT
        	realLight = _FakeLightDir.xyz;
        	#endif
            float vdl = dot(viewDir, realLight.xyz);
			float sunBorder = pow(max(0, dot(normalize(Input.WorldPos), realLight.xyz)),_BorderEmissionRange);
        #ifndef _FX_LEVEL_LOW
            half3 scatter = ScatterNoneNegative(pow(vdl * 0.5 + 0.5, _SunFlareFalloff), viewDir.y);
		#else
			half3 scatter = 1;
		#endif
			/*===  Background  ===*/
			half4 tex = 0;
            if (_ExplorationParams.x)
            {
	            half4 texCurrent = SAMPLE_TEXCUBE(_TestCurrentBaseTex, Input.uv);
				half4 texTarget = SAMPLE_TEXCUBE(_TestTargetBaseTex, Input.uv);
            	tex = lerp(texCurrent, texTarget, _ExplorationParams.y);
            }
			else
            {
				tex = SAMPLE_TEXCUBE(_BaseTex, Input.uv);
            }
			
			half3 mulLight = realLight * _SkyHorizonTilt;
			half3 horizonFalloff = float3(mulLight.x, _SkyLowerColor.a, mulLight.z);
			half vdhf = dot(viewDir, horizonFalloff);
		#if defined( _CUSTOM_BACKGROUND) && !defined(_FX_LEVEL_LOW)
			tex.rgb = tex.rgb * tex.a;
			float3 background = _SkyUpperColor + (_SkyLowerColor - _SkyUpperColor) * pow(saturate( _SkyHorizon - vdhf ), 1.0 );
			tex.rgb += background * (1-tex.a);
		// #else
		// 	half3 background = tex.rgb/* * (1-tex.a)*/;
		// 	tex.rgb = background;
		#endif
			
			half4 cloudTex = 0;
			if (_ExplorationParams.x)
            {
	            half4 cloudCurrentMaskTex = SAMPLE_TEXCUBE(_TestCurrentMaskTex,Input.uv);
				half4 cloudTargetMaskTex = SAMPLE_TEXCUBE(_TestTargetMaskTex,Input.uv);
				cloudTex = lerp( cloudCurrentMaskTex, cloudTargetMaskTex, _ExplorationParams.y);
            }
            else
            {
            	cloudTex = SAMPLE_TEXCUBE(_MaskTex,Input.uv);
            }
			
			float4 borderMask = cloudTex.y * _BorderColor * sunBorder;
			half4 sun = Sun(vdl);
			FLOAT fog;
            half4 sky = SkyboxCubemap(Input.uv, viewDir, vdl, scatter, fog,tex,cloudTex);
			// a = sky.rgb;
			sun.a *= 1 - tex.a * 0.995;//太阳被云挡住
			// 			FragOut output;
			// output.depth = 1;
			// output.color = float4(sky.rgb,1);
			// return output;
        	

			half3 sun_flare = 0;
		#ifndef _FX_LEVEL_LOW
			/*===  Sun Flare  ===*/
			half3 sunForwardDir = realLight.xyz;
        	half3 sunSideDir = cross(sunForwardDir, float3(1,0,0));
        	half3 sunUpDir = cross(sunForwardDir, sunSideDir);
        	half2 sunSpaceUV = abs(half2(dot( half4( viewDir, 0.0 ), sunUpDir ), dot( half4( viewDir, 0.0 ), sunSideDir ) ) );
			sunSpaceUV = max(sunSpaceUV, Rotate(sunSpaceUV, 0, 0.3925));
        	half sunFlareRotation = _SinTime.w * 0.25 + 0.95;
			sun_flare = _SunFlareColor.rgb * _SunFlareColor.a * scatter * saturate(sunBorder * sunBorder * sunBorder * sunBorder - 0.2);
        	sun_flare *= calSunFlare(sunSpaceUV, sunFlareRotation);
		#endif


			half3 color = lerp(sky.rgb + sun_flare, sun.rgb, sun.a);
        	color = lerp(color, sky.rgb, sky.a);

			//直接叠加最远距雾
			color.rgb = lerp(color.rgb,_FogEndColor.rgb,  fog);
        	color.rgb *= _SceneExposure;
			color.rgb += borderMask.xyz;//加入rim效果
		#ifdef _FX_LEVEL_HIGH
        	#ifdef _LIGHTING_ON
        		float3 posWS = normalize( float3(Input.WorldPos.x,Input.WorldPos.y *3.0,Input.WorldPos.z) );
        		//闪电效果
        		float3 calcRot = CalcRotation( _LightingPTRotX , _LightingPTRotY );
        		float LightingPointPos = smoothstep( 0.0 , _LightingPTSize, saturate( distance( posWS, calcRot) ));

        		//mask的a通道存放闪电效果图
        		half Lighting = saturate( 1.0 - LightingPointPos  ) * cloudTex.a;

        		Lighting *= abs(frac(  _TimeParameters.x *  _FlashFrequency  ) - 0.5) * 2.0;
        		half3 LightCol = Lighting * _LightingColor.rgb ;
  
        		color.rgb = lerp(color.rgb,LightCol ,Lighting * _LightingColor.a );
        	#endif
		#endif
        	color.rgb *= lerp(_SceneColor.rgb, 1, _SceneColor.a);
			// UNITY_BRANCH
		 //    if (_ScatterEnable)
		 //    {
		 //        float depth01;
		 //        float3 scatter = GetScatterRGB(posWS - _WorldSpaceCameraPos, posWS, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
		 //        // APPLY_FOG(scatter, inputData.positionWS);
		 //        color.rgb = lerp(color.rgb, scatter, saturate(depth01 * 2));
		 //    }
   			// float3 oldcolor = color.rgb;
			APPLY_FOG(color.rgb, _WorldSpaceCameraPos + normalize(viewDir) * LinearEyeDepth(1));
   			// color.rgb = lerp(oldcolor, color.rgb, fog);
			FragOut output;
// #ifdef _FX_LEVEL_HIGH
// 	#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
// 			output.depth = sun.g * (1 - tex.a) > 0.5f ? 0.999999f : 1;
// 	#else
// 			output.depth = sun.g * (1 - tex.a) > 0.5f ? 0.000001f : 0;
// 	#endif
// #endif
			output.color = FLOAT4(color * _SkyFinalExposure, 1);
			return output;
			//return FLOAT4(color, 1);
        }
            
	ENDHLSL

	SubShader 
	{
	    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }


		
	    Pass 
	    {
			Name "SKYBOX"
			Cull back
			ZWrite On
		    //Ztest GEqual
			//Blend One Zero
			HLSLPROGRAM
		    
			#pragma multi_compile _ _FAKELIGHT
			#pragma multi_compile _ _CUSTOM_BACKGROUND
			#pragma multi_compile _ _FX_LEVEL_HIGH _FX_LEVEL_LOW
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
			Blend One Zero
			HLSLPROGRAM
			// #include "../Scene/Scene_SkyPCH.hlsl"
			// #include "../Include/Head.hlsl"
			// #include "../../Scene/Scene_Sky.hlsl"

			#pragma vertex vertOverdraw
			#pragma fragment fragskyoverdraw
			struct appdata_t 
			{
				FLOAT4 vertex : POSITION;
			};
			struct v2f 
			{
				FLOAT4 vertex : SV_POSITION;
				FLOAT3 texcoord : TEXCOORD0;
				FLOAT2 depth01  : TEXCOORD1;
				FLOAT4 WorldPosition :TEXCOORD2;
				
			};
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
			half4 fragskyoverdraw (v2f i) : SV_Target
			{
				
				// DECLARE_OUTPUT(MRTOutput, mrt);
				// mrt.rt0 = half4(0.1, 0.04, 0.02, 0);
				// mrt.rt1.xyz = EncodeFloatRGB(i.depth01.x/i.depth01.y);
				// SET_BLOOM(mrt, EncodeAlpha(1, _IsRt1zForUIRT));
				return half4(0, 0, 0, 1);
			}

			ENDHLSL
		}
	}

	Fallback Off
}

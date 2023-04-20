Shader "URP/Scene/New_URP_Water03"
{
	Properties
	{
		[NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _ProcedureTex0 ("Normal", 2D) = "bump" {}
		[NoScaleOffset]_ProcedureTex1 ("WaveTexture", 2D) = "black" {}
        [NoScaleOffset]_LocalEnvCube ("Env Cube", Cube) =  "" {}
		_WaterMask("WaterBorderMask",2D) = "black"{}
		_WaterDensity("WaterBorderMaskDensity",float) =  0.5
		_WaterMaskBox("WaterSizeBox",vector) = (1,1,1,1)
		_waterMaskBoxSize("WaterMaskSize",float) = 128
		_Color0("Main Color", Color) = (1,1,1,1)
		_Color1("CausticsColor", Color) = (1,1,1,1)
		[HDR]_Color2("SpecularColor", Color) = (1,1,1,1)
		_Color3("FoamLineColor", Color) = (1,1,1,1)
		_Param0 ("x:Speed y:Refrac z:SpecularRange w:SpecularInt", vector) = (0,0,0,0)
        _Param1 ("x:UVTiling y:reflectionInt z:NormalScale w:", vector) = (0,0,0,0)
        _Param2 ("x:FoamLineTiling y:FoamLineSpeed z:FoamLineDisturbInt w:" , vector) =(0,0,0,0)
		_Param3 ("LightDri:" , vector) =(0,0,0,0)
		_Param4 ("x:CausticsTiling y:CausticsInt z:CausticsSpeed w:CausticsDisturbInt" , vector) =(0,0,0,0)
		_Param5 ("x:WaveFrency y:WaveSpeed z:WavePower w:" , vector) =(0,0,0,0)
		// _Param6 ("x:rotation y: z: w:" , vector) =(0,0,0,0)
		[HideInInspector]_DebugMode("__debugMode", Float) = 0.0
		[HideInInspector]_UVST0("", Vector) = (1,1,0,0)
	}

	SubShader
	{
		Tags{"RenderType" = "Transparent" "Queue" = "Transparent-1""RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }

		LOD 100
	
		Pass
		{
			Name "FORWARD"
			Tags{"LightMode" = "UniversalForward" }
			Cull Off
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha 
			

			HLSLPROGRAM

			//----------------------
			#define URP_BASE
			#define REDEFINE_URP

			// Universal Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION



			#ifdef _SM_4
				#pragma target 5.0
			#else//!_SM_4
				#pragma target 3.0
			#endif//_SM_4
			sampler2D _MainTex;
			sampler2D _ProcedureTex0;
			sampler2D _ProcedureTex1;
			samplerCUBE _LocalEnvCube;		
			float4 _Color0;	
			float4 _Color1;	
			float4 _Color2;	
			float4 _Color3;
			float4 _Param0;				
			float4 _Param1;
			float4 _Param2;
			float4 _Param3;
			float4 _Param4;
			float4 _Param5;	
			float4 _WaterLightDir;
			float4 _WaterMaskBox;
			float _waterMaskBoxSize;
			float _WaterDensity;
			sampler2D _WaterMask;
			sampler2D _CameraDepthTexture;
			#define _MainColor _Color0
			#define _WaterNormal _ProcedureTex0
			#define _WaveTex _ProcedureTex1
			#define _MainTex _MainTex
			#define _SpecularColor _Color2
			#define _CausticsColor _Color1.xyz
			#define _FoamLineColor _Color3
			#define _Speed _Param0.x
			#define _Refrac _Param0.y
			#define _SpecularRange _Param0.z
			#define _SpecularInt _Param0.w
			#define _UVTiling _Param1.x
			#define _reflectionInt _Param1.y
		    #define _NormalScale _Param1.z	
            #define _ColorDepth _Param1.w
			#define _FoamLineTiling _Param2.x
			#define _FoamLineSpeed _Param2.y
			#define _FoamLineDisturbInt _Param2.z
			#define _FoamLineInt _Param2.w
			#define _Depth _Param3.w
			#define _LightDri _Param3.xyz
			#define _CausticsTiling _Param4.x
			#define _CausticsInt _Param4.y
			#define _CausticsSpeed _Param4.z
			#define _CausticsDisturbInt _Param4.w
			#define _CausticsRange _Color1.w
			#define _FoamLineOnAlpha _Param5.w

			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW

			#if defined(_FX_LEVEL_HIGH)
				#define USE_DEPTH_TEXTURE
            #endif 

			// TEX2D_SAMPLER(_CameraDepthRT);		
			#include "../../Include/ShaderLevel.hlsl"
			#include "../Scene/URP_Scene_Water_Input03.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		 	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "../Scene/URP_Scene_Water_Vertex03.hlsl"
			#include "../Scene/URP_Scene_Water_Pixel03.hlsl"

			#pragma vertex vert
			#pragma fragment Frag
			ENDHLSL
        }		
		
    }
}

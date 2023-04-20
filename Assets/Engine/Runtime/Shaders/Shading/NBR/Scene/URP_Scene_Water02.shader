Shader "URP/Scene/New_URP_Water02"
{
	Properties
	{
		[NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _ProcedureTex0 ("Normal", 2D) = "bump" {}
		[NoScaleOffset]_ProcedureTex1 ("WaveTexture", 2D) = "black" {}
        [NoScaleOffset]_LocalEnvCube ("Env Cube", Cube) =  "" {}
		_Color0("Main Color", Color) = (1,1,1,1)
		_Color ("WaveColor", Color) = (1,1,1,1)
		_Color1("CausticsColor", Color) = (1,1,1,1)
		_Param0 ("x:speed y:reflect z:distroy w:distroyInt", vector) = (0,0,0,0)
        _Param1 ("xy:firstuv zw:seconduv", vector) = (0,0,0,0)
        _Param2 ("xy:waveuv z:reflection w:disturb" , vector) =(0,0,0,0)
		_Param3 ("x:roughness y:Metallic z:SpecularInt w:" , vector) =(0,0,0,0)
		_Param4 ("x:CausticsTiling y:CausticsInt z:CausticsSpeed w:CausticsDisturbInt" , vector) =(0,0,0,0)
		[HideInInspector]_DebugMode("__debugMode", Float) = 0.0
		[HideInInspector]_UVST0("", Vector) = (1,1,0,0)
	}

	HLSLINCLUDE


//define
	ENDHLSL

	SubShader
	{
		Tags{"RenderType" = "Transparent" "Queue" = "Transparent-1" "PerformanceChecks" = "False" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
		LOD 100
		Cull Off
		ZWrite off

		Pass
		{
			Name "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#define URP_BASE
			#define REDEFINE_URP
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			
			sampler2D _ProcedureTex0;
			sampler2D _ProcedureTex1;
			samplerCUBE _LocalEnvCube;
			sampler2D _MainTex;
			sampler2D _CameraDepthRT;
			float4 _Color0;
			float4 _Color;
			float4 _Color1;
			float4 _Param0;
			float4 _Param1;
			float4 _Param2;
			float4 _Param3;
			float4 _Param4;			

			float4 _WaterLightDir;


			#define _MainColor _Color0
			#define _WaterNormal _ProcedureTex0
			#define _WaveTex _ProcedureTex1
			#define _MainTex _MainTex
			#define _Speed _Param0.x
			#define _Reflect _Param0.y
			#define _Distory _Param0.z
			#define _DistoryInt _Param0.w
			#define _FistTexTilingX _Param1.x
			#define _FistTexTilingY _Param1.y
			#define _SecTexTilingX _Param1.z
			#define _SecTexTilingY _Param1.w
			#define _WaveTilingX _Param2.x
			#define _WaveTilingY _Param2.y
			#define _Reflection _Param2.z
			#define _Disturb _Param2.w
			#define _Roughness _Param3.x
			#define _Metallic _Param3.y
			#define _SpecularInt _Param3.z
			#define _WaveColor _Color
			#define _WakeSpec FLOAT4(0.04, 0.04, 0.04, 1.0 - 0.04)
            //Caustics
			#define _CausticsColor _Color1
			#define _CausticsTiling _Param4.x
			#define _CausticsInt _Param4.y
			#define _CausticsSpeed _Param4.z
			#define _CausticsDisturbInt _Param4.w
			#include "../Scene/URP_Scene_Water_Input02.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		 	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			// #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "../Scene/URP_Scene_Water_Vertex02.hlsl"
			#include "../Scene/URP_Scene_Water_Pixel02.hlsl"


			#pragma vertex vert
			#pragma fragment Frag
			ENDHLSL
        }	
		/*Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDCG
		}*/
    }
}

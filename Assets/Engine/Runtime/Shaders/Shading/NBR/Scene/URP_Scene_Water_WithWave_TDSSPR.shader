Shader "URP/Scene/Water_WithWave_TDSSPR"
{
	Properties
	{
		//normal
		[NoScaleOffset]_ProcedureTex0("NormalTex", 2D) = "" {}
		[NoScaleOffset]_MainTex("NoiseTex", 2D) = "" {}
		_UVST0("NormalParam", Vector) = (0.36,40,0.0,0.0)	
		//_SDF("SDF3D",3D)=""{}
		[NoScaleOffset]_SDF("SDF", 3D) = "" {}
		_SDFParam("SDFParam", Vector) = (0,0,0,0)
		_SDFBox("SDFBox", Vector) = (0,0,0,0)
		//Scattering
		_Color("Diffuse", Color) = (0.0, 0.0124, 0.566, 1.0)
		_Color0("Diffuse Grazing", Color) = (0.184, 0.393, 0.519, 1)
		_Color1("SubSurfaceColor", Color) = (0.184, 0.393, 0.519, 1)
	
		_Color2("Diffuse  High", Color) = (0.184, 0.393, 0.519, 1)
		_Color3("Diffuse Grazing High", Color) = (0.184, 0.393, 0.519, 1)
		


		_Param0("WaterParam0", Vector) = (0.3,1.0,0.6,0.8)	
		_Param1("WaterParam1", Vector) = (4.0,3.0,1.0,0.1)		
		_Param2("WaterParam2", Vector) = (1.0,3.0,128,5.0)
		_Param3("WaterParam3", Vector) = (0.1,0.1,1,1)


		_Param4("FoamParams1", Vector) = (0,0,1,1)
		_Param5("FoamNoiseParam", Vector) = (0,0.01,1,1)
		_Param6("FoamTexParam", Vector) = (0,0,0,0)
		_Param7("FoamParams2", Vector) = (0,0,0,0)
		_Param8("FoamModulateParams", Vector) = (0.3,1.0,0.6,0.8)	

		_Param9("GrazingPow:x FoamAttenPow:y AttenDistance:z", Vector) = (6,3,100,0)	
		_Param10("FoamIntensity:x FoamSize:y FoamSmooth:z FoamPow:w", Vector) = (0.6,0.9,0.2,20)	
		_Param11("MinCameraY:x ThresholdHeight:y ChangeRate:z FoamTexScale:w", Vector) = (8,20,2,0.5)	
		[NoScaleOffset]_FoamTex("FoamTex", 2D) = "white" {} 

		_ParamA("WaveA", Vector) = (0,0,1,1)
		_ParamB("WaveB", Vector) = (0,0,1,1)
		_ParamC("WaveC", Vector) = (0,0,1,1)
		_Wave1("Wave1", Vector) = (0,0,1,1)
		_Wave2("Wave2", Vector) = (0,0,1,1)
		_Wave3("Wave3", Vector) = (0,0,1,1)
		_Wave4("Wave4", Vector) = (0,0,1,1)
		_Wave5("Wave5", Vector) = (0,0,1,1)
		_Wave6("Wave6", Vector) = (0,0,1,1)
		_Wave7("Wave7", Vector) = (0,0,1,1)
		_Wave8("Wave8", Vector) = (0,0,1,1)
		_Wave9("Wave9", Vector) = (0,0,1,1)
		_SteepnessFadeout("SteepnessFadeout", Vector) = (0,0,300,2)	
		_WaveNormalDensity("WavePPNormalDensity",float) = 1
		_WaveSpecColor("WavePPSpecColor",Color) = (1,1,1,1)
		_WaveVertexDensity("WavePPVertexDensity",float) = 0.1
		_MarchParam("TDSSPR",vector) = (0,0,0,0)
	}

	// CGPROGRAM

	// ENDCG

	SubShader
	{
		Tags{"RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
		LOD 100
		
		Pass
		{
			Name "FORWARD"
			Tags{"LightMode" = "UniversalForward"}

			Blend one zero

			HLSLPROGRAM
			// #define _OUTPUT_UV_COUNT 2
			#pragma target 3.0
			//uv
			//feature
			//#define _CGINC
			// #define _NO_MRT
			// #define _VERTEX_FOG
			#define _SCREEN_POS
			//lighting		
			#define _VERTEX_GI
			#define _NO_COLOR_EFFECT
			#define _CUSTOM_INTERPOLANTS	
			#define _PBS_NO_IBL			
			#define _WATER
			#define _SCREEN_COLOR
			#define _SCREEN_POS
			// #define _PPWave
			////___________________________
			#define URP_BASE
			#define REDEFINE_URP
			//#define MAIN_LIGHT_CALCULATE_SHADOWS
			////___________________________
			//#pragma shader_feature _ _FRAMEBUFFER_FETCH
			//#pragma shader_feature_local _ _DEBUG_APP

			// Universal Pipeline keywords
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			//#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "../Include/PCH.hlsl"
			#include "../Scene/URP_Water_Effect.hlsl"
			#include "../Include/URP_Vertex.hlsl"
			#include "../Include/URP_Pixel.hlsl"

			#pragma vertex vertForwardBase
			#pragma fragment fragForwardBase

			ENDHLSL
		}
		Pass
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
		}
	}
	//CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

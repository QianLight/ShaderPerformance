Shader "URP/Scene/Water_WithWave"
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
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile _ _IgnoreSDF

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "../Include/PCH.hlsl"
			#include "../Scene/URP_Water_Effect.hlsl"
			#include "../Include/Fog.hlsl"

			#pragma vertex vert
			#pragma fragment frag

			struct FMobileShadingVSToPS
			{
				FInterpolantsVSToPS Interpolants;
				FLOAT4 Position : SV_POSITION;
			};

			void vert(FVertexInput Input INSTANCE_INPUT, out FMobileShadingVSToPS Output)
			{  
				INITIALIZE_OUTPUT(FMobileShadingVSToPS, Output);

				REAL4 localPos = Input.Position;

				FLOAT4 rot = 0;

				FLOAT4 WorldPosition =  mul(unity_ObjectToWorld, localPos);// INSTANCE_WPOS(localPos, rot)

				FLOAT4 projPos = 0;

				Output.Interpolants = CustomInterpolantsVSToPS(Input, WorldPosition, projPos, rot);	//URP_Water_Effect.hlsl

				Output.Position = projPos;
				//Output.Interpolants.WorldPosition.w = Output.Position.w;	
			} 
			// 來自MaterialTemplate.hlsl
			FFragData GetFragData(FInterpolantsVSToPS Interpolants, FLOAT4 SvPosition)
			{
				DECLARE_OUTPUT(FFragData, FragData);
				GET_VS_DEPTH(Interpolants);
				FragData.SvPosition = SvPosition;
				FragData.ScreenPosition = FLOAT4(Interpolants.ScreenPosition.xy/Interpolants.ScreenPositionW.x,Interpolants.ScreenPosition.zw/Interpolants.ScreenPositionW.y);
				FragData.WorldPosition = Interpolants.WorldPosition;
				FragData.WorldPosition_CamRelative = Interpolants.WorldPosition.xyz - _CameraPos.xyz;
				FragData.TangentToWorld = REAL3x3(Interpolants.TangentWS.xyz, Interpolants.BitangentWS.xyz, Interpolants.NormalWS.xyz);
				FragData.CameraVector = SafeNormalize(FLOAT3(Interpolants.NormalWS.w,Interpolants.TangentWS.w,Interpolants.BitangentWS.w));
				FragData.LocalPosition = Interpolants.LocalPosition;
				FragData.TexCoords[0] = Interpolants.TexCoords[0];
				FragData.Ambient = Interpolants.DiffuseGI;

				return FragData;
			}

			//来自PCH.hlsl
			//FLOAT4 _AmbientParam1;//xyz hdr decode w maxMip
			//FLOAT4 _SceneColor;
			//FLOAT4 _AmbientParam2;
			//#define _ContrastLight _AmbientParam1.w
			//#define _ContrastInsity _AmbientParam2.w

			// 來自MaterialTemplate.hlsl
			inline FMaterialData GetMaterialData(FFragData FragData)
			{
				DECLARE_OUTPUT(FMaterialData, MaterialData)

				CustomMaterial(FragData, MaterialData);
				MaterialData.BloomIntensity = 1;

				return MaterialData;
			}

			// 來自SRP_Lighting.hlsl
			//这一段中的_ContrastInsity,_SceneColor都来自于_AmbientParam2
			//_AmbientParam2来自场景中的Ambient.cs
			//by陈思敏
			inline void CalcSceneColor(inout FLOAT3 color,FLOAT srcShadow)
			{	
				FLOAT3 targetColor = color*lerp(_SceneColor.rgb, 1, _SceneColor.a);		
							
				targetColor = lerp(targetColor, 0, _ContrastLight);
				FLOAT contrastRatio = lerp(0, srcShadow, _ContrastLight);
				color.xyz = lerp(targetColor, Luminance(color.xyz * _ContrastInsity) * _SceneColor.rgb, contrastRatio);
			}

			// 來自SRP_Lighting.hlsl
			FLOAT4 CalcLighting(FFragData FragData, FMaterialData MaterialData DEBUG_ARGS)
			{
				FLOAT3 DirectDiffuse = FLOAT3(0, 0, 0);
				FLOAT3 DirectSpecular = FLOAT3(0, 0, 0);
				FLOAT3 Color = FLOAT3(0, 0, 0);
				FShadowData ShadowData = DefaultShadowData();// CalcShadow(FragData,MaterialData DEBUG_PARAM);

				FLOAT ShadowMask = 1;

				CalcWaterLighting(FragData, MaterialData, ShadowData, ShadowMask, DirectDiffuse, DirectSpecular DEBUG_PARAM);

				Color += DirectDiffuse + DirectSpecular;

				//==========================Color Modify==========================
				CalcSceneColor(Color, GetSrcShadow(ShadowData));

				return FLOAT4(Color,0);
			}

			FLOAT4 frag(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, FLOAT facing : VFACE) : SV_Target
			{
				FFragData FragData = GetFragData(vs2ps, SvPosition);
				FMaterialData MaterialData = GetMaterialData(FragData);

				FLOAT4 Color = CalcLighting(FragData, MaterialData DEBUG_PARAM);

				APPLY_FOG(Color, vs2ps.WorldPosition.xyz);

				return Color;
			}

			ENDHLSL
		}
	}
	//CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

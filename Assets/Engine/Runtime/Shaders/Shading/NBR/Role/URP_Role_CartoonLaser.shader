Shader "URP/Role/CartoonLaser"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		_Color0("Main Color", Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,1,1,0)
		_Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
		//mask
		[NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow ", 2D) =  "black" {}
		_Param2("x:maskEnable g:emissive b:shadowMask a:toonAoScale", Vector) = (0,0,0,0) 

		//ramp
		[NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}  
		_Color3("ShadowColorMultiply",Color)=(1,1,1,1)

		//Dark Rim 		
		_Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)
		
		//emissive
		[HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

		//outline	
		_ColorOutline("Outline Color", Color) = (0,0,0,0.003)
		_Param3("x:outlineZOffset y:VerticalGradient z:ShadowInt w:_VerticalGradientOffset", Vector) = (0,-1,0,-1) 
		//_ColorOutline2("Outline Color2", Color) = (0,0,0,0)
		[HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

		//VerticalGradient
		//_GradientColor("VerticalGradientColor",Color)=(1,1,1,1)

		//Laser	
		_LaserColor("LaserColor", Color) = (0,0,0,0)
		_Param4("x:LaserContrast y:LaserInt z:LaserTiling w:", Vector) = (0,0,0,0)     
				
		_Param("", Vector) = (0,0,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		[HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
		_HeightGradientColor("Gradient Color", Color) = (0,0,0,0)
		
		_CustomSpecCube("Spec Cube", Cube) = "black" {}

		[HideInInspector] _Position("pos", Vector) = (0,1,0,1)

		[HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
		[HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
		[HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
		
		_SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
		
		//CBuffer Fill
		[HideInInspector] _DebugMode("_DebugMode", FLOAT) = 1.0
		[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
		[HideInInspector]_Color6("_Color6", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_UVST0("_UVST0", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1)
		[HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
		[HideInInspector]_UVST3("_UVST3", Vector) = (1,1,1,1)
		[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
	}
	
	HLSLINCLUDE
		#define ROLE_EFFECT
		//feature
		#define _MAIN_COLOR
		#define _UV_SCALE2	
		//lighting
		#define _VERTEX_GI
		#define _SELF_SHADOW
	    #define _CARTOON		
		#define _SM_DARK_RIM
		#define _CSM3
		#if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
			#define _ADD_LIGHT
		#endif
		#ifdef _SPHERE_DITHER_FADE
			#define _ALPHA_TEST
			#define _DITHER_TRANSPARENT
		#endif

		#define _SM_RIM
		float4 _RimParam;
		float4 _LightFadePram;
		#define _RimNormalMapWeight _LightFadePram.x
		#define _RimOrientLight _LightFadePram.y
		#define _RimDisFadeBegin _LightFadePram.z
		#define _RimDisFadeEnd _LightFadePram.w
		#define _RimIntensity _Param4.w
		#define _RimCutLocation _RimParam.x
		#define _RimCutSmoothness _RimParam.y
		
		#define _RoleRimColor (_RimColortoon.xyz)
		#define _RoleRimMultiply (_RimColortoon.w)
		#ifdef _SM_DARK_RIM
			#define _DarkRimCutLocation _RimParam.z
			#define _DarkRimCutSmoothness _RimParam.w
			#define _DarkRimLightColor _Color4
		#endif

		//----------------------
		#define URP_BASE
		#define REDEFINE_URP
		#define BUILTIN_SM_OFF

	ENDHLSL

		  
	SubShader
	{
		Tags{"RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }

		Pass
		{
			Name "UniversalForward"
			Tags{ "LightMode" = "UniversalForward" }
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			Cull Back
			Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }
			
			HLSLPROGRAM
			// -------------------------------------
			// Universal Pipeline keywords
			// #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			// #pragma multi_compile _ _SMARTSOFTSHADOW_ON
			#define SMARTSOFTSHADOW_ON
			#pragma shader_feature _ _CUSTOM_SPEC_CUBE
			
			#pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
			
			#define _VERTEX_COLOR
			#define _CUSTOM_EFFECT
			#define _CUSTOM_VERTEX_PARAM
			#define _ROLE_HEIGHT_GRADIENT
			#define _ROLE_ADDITIONAL_LIGHT_ON
			//----------------------
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#include "../Include/Pragam_Head.hlsl"
			#ifdef _SM_4
				#pragma target 5.0
			#else//!_SM_4
				#pragma target 3.0
			#endif//_SM_4	

			#define _CUSTOM_LIGHT
             
			#define _LaserContrast _Param4.x
			#define _LaserInt _Param4.y
			#define _LaserTiling _Param4.z

			#include "Role_Head.hlsl"

			TEXTURECUBE(_CustomSpecCube);
			SAMPLER(sampler_CustomSpecCube);
			
			#include "../Include/URP_LightingHead.hlsl" 

			inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
			{
				TransformHeightGradient(Input, Interpolants);
				SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
			}
			
			#include "../Include/URP_Vertex.hlsl"
			
			void CustomLighting(FFragData FragData, inout FMaterialData MaterialData,
				FShadowData ShadowData,FLOAT ShadowMask,
				inout REAL3 DirectDiffuse,inout REAL3 DirectSpecular DEBUG_ARGS)
			{
				FLOAT2 Shadow = ShadowData.Shadow.xy;
				REAL3 shadowLightColor = lerp(_MainLightColor.xyz, _RoleShadowColor.rgb, 1 - Shadow.y);
				FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
				_MainLightDir.xyz, shadowLightColor.xyz,1, ShadowData,_ShadowColor.xyz);
				SM_Ramp01(MaterialData,LC,ShadowMask,FragData,DirectDiffuse DEBUG_PARAM);					
				SM_StandardSpec(LC,DirectSpecular DEBUG_PARAM);
				//use Laser
				REAL2 uv = GET_FRAG_UV;
				REAL Mask = SAMPLE_TEX2D(_MainTex, GET_FRAG_UV).a;
				REAL3 LaserColor = _LaserColor.xyz;
				REAL3 ViewDir = FragData.CameraVector;
				REAL3 WorldNormal = MaterialData.WorldNormal;
				REAL Tdir = dot(ViewDir, WorldNormal) * _LaserTiling;
				REAL3 NormalColor = REAL3(0.5, 0.5, 1);
				REAL3 ColorChang01 = cos(Tdir) * NormalColor;
				REAL3 ColorChang02 = cross(normalize(REAL3(1, 1, 1)), NormalColor) * sin(Tdir);
				REAL3 ColorChang03 = normalize(REAL3(1, 1, 1)) * dot(normalize(REAL3(1, 1, 1)), NormalColor);
				REAL3 RampColor = ColorChang01 + ColorChang02 + (1 - cos(Tdir)) * ColorChang03 * _LaserContrast;
				RampColor *= LaserColor * Mask; 
				DirectSpecular =lerp( DirectSpecular , DirectSpecular+RampColor ,_LaserInt);//end Laser				   
			    
				DEBUG_CUSTOMDATA_PARAM(LC0, LC)
			}

			#include "../Include/URP_Pixel.hlsl"

			//only editor use
			//#pragma shader_feature_local _ _DEBUG_APP
			#pragma shader_feature_local _ _PBS_FROM_PARAM
			//render type
			#pragma shader_feature_local _ _ALPHA_BLEND_ALPHA_TEST

			//lighting
			//#pragma shader_feature_local _ _SHADOW_MAP
			//#pragma shader_feature_local _ _NO_CSM

			//#pragma shader_feature_local _ _ADD_LIGHT
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY

			#pragma vertex  vertForwardBase
			#pragma fragment fragForwardBase

			ENDHLSL
		}

		Pass
		{
			Name "Outline"
			Offset 1,[_OutlineZOffset]
			Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			HLSLPROGRAM
				#pragma target 3.0
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

				#include "../Tools/Outline.hlsl"
				//#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
				#pragma multi_compile_local _ _DITHER_TRANSPARENCY
			
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			ENDHLSL
		}
		        
        Pass
        {
            Name "PlanarShadow"
            
            Tags { "LightMode" = "PlanarShadow" }

        	ZWrite Off
        	
        	// 在UI界面中：因为没有背景alpha和没有背景颜色，需要特殊处理。
        	Blend SrcAlpha Zero
        	// 如果要用在正常战斗中，有背景颜色，得使用正片叠底：
        	//Blend DstColor Zero, SrcAlpha OneMinusSrcAlpha

            //深度稍微偏移防止阴影与地面穿插
            Offset -1 , 0
            
            Stencil
            {
                Ref [_StencilRef_PlanarShadow]
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Role_PlanarShadow.hlsl"

            ENDHLSL
        }
		
//		Pass
//        {
//            Name "ScreenSpaceRim"
//            
//            Tags { "LightMode" = "ScreenSpaceRim" }
//
//            ZTest Equal
//        	ZWrite Off
//            ColorMask RGB
//            Blend One One
//        	
//            
////            Stencil
////            {
////                Ref [_StencilRef_StencilShadow]
////                Comp NotEqual
////                Pass Keep
////                Fail Keep
////                ZFail Keep
////            }
//
//            HLSLPROGRAM
//            #pragma vertex ScreenRimVert
//            #pragma fragment ScreenRimFrag
//            #pragma multi_compile _ _SMARTSOFTSHADOW_ON
//            
//			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "../Include/Pragam_Head.hlsl"
//            #include "Role_Head.hlsl"
//            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//			// TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
//
//            
//			 #include "Role_Simple_Screen_Space_Rim.hlsl"
//            
//            ENDHLSL
//        }
		
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

            #include "URP_Role_ShadowCaster.hlsl"

			ENDHLSL
		}
		
		Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
		
			/*
		Pass
		{
			Name "OverdrawForwardBase"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite[_ZWrite]
			Stencil
			{
				Ref[_StencilRef_Role]
				Comp equal
			}
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "../StdLib.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
			};
			
			struct Varyings
			{
				FLOAT4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}


		Pass
		{
			Name "OverdrawOutline"
			Tags{"LightMode" = "OverdrawOutline"}

			Blend One One
			Cull Front
			Offset 1,1
			Stencil
			{
				Ref[_StencilRef_Outline]
				Comp equal
			}
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "../StdLib.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
			};
			
			struct Varyings
			{
				FLOAT4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}*/
		//UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"	
	}
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

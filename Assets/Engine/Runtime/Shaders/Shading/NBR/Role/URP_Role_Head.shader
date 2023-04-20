Shader "URP/Role/Head"
{
    Properties
    {
        //pbr
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)

        [NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "" {}
        _UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
        _Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,0,1,0)
        _Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
        _PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
        //mask
        [NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow ", 2D) = "black" {}
        _Param2("x:maskEnable g:emissive b:shadowMask a:toonAoScale", Vector) = (0,0,0,0)

        _Param3("r:ShdowRange g:ShadowSmooth b:ShadowInt a: ", Vector) = (0,0,0.5,0)
        //ramp
        [NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}
        _Color3("ShadowColorMultiply",Color)=(1,1,1,1)

        _ShadowDircontrol("r:Left-Right g:Up-Down",Vector)=(0,0,0,0)
        //Dark Rim 		
        _Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)
        _Color6("Rim dri",Color)=(1,0,0,1)
        [HDR]_RimColortoon("RimColor",Color) = (0.6,0.6,0.6,1)

        //emissive
        [HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

        //outline	
        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        _ColorOutline2("Outline Color2", Color) = (0,0,0,0)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

        _Param("", Vector) = (1,1,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)
        _HeightGradientColor("Gradient Color", Color) = (0,0,0,0)
        _ScreenSpaceRimParam("Screen Space Rim", Vector) = (5,5,0,0)
        _FaceLightmap("Face Lightmap", 2D) = "white" {}
		_HeightGradientPreviewParam("Gradient PreviewParam", Vector) = (0,0,0,0)
		_HeightGradientPreivewColor("Gradient PreivewColor", Vector) = (0,0,0,0)
		_RootPosWS ("RootPosWS", Vector) = (0,0,0,0)
    	_SceneColor("SceneColor", Vector) = (0,0,0,1)
    	_FaceShadowParam("FaceShadowParam", Color) = (0,0,0,0)
        
        [HideInInspector]_Color1("", Color) = (0,0,0,0)

        [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
        [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
        [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        // [HideInInspector]_UVST0("", Vector) = (1,1,0,0)
    	_SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
    	[HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
    	[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }
    //

    HLSLINCLUDE

		#define ROLE_EFFECT
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#define UNIFORM_PCH_OFF
		#define ROLE_SRP_BATCH

        CBUFFER_START(UnityPerMaterial)
            float4 _Color0;
            float4 _UVST1;
            float4 _Param0;
            float4 _Param1;
            float4 _PBRParam;
            float4 _Param2;
            float4 _Color3;
            float4 _Color4;
            float4 _Color5;
            float4 _ColorOutline;
            float4 _MatEffectOutlineParam;
            float4 _Param3;
            float4 _Color6;
            float4 _Param;
            float4 _Color;
            float4 _RimColortoon;
            float4 _HeightGradientColor;
            float4 _HeightGradientPreviewParam;
            float4 _HeightGradientPreivewColor;
            float4 _Color1;
            float4 _RootPosWS;
			float4 _ShadowPos;
			float4 _ScreenSpaceRimParam;
    		float4 _FaceShadowParam;
		    float4 _SceneColor;
			float4 _SSRimColor;
			float4 _SSRimParam0;
		CBUFFER_END
		#define SCREEN_SPACE_RIM_IS_IN_COMMON_CBUFFER
		#define UNITY_PERMATERIAL_DECLARED
            half4 _FaceLightingParam;

        #define _MainColor _Color0
        TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
        TEXTURE2D(_MainTex1);SAMPLER(sampler_MainTex1);
        TEXTURE2D(_ProcedureTex3);SAMPLER(sampler_ProcedureTex3);
        TEXTURE2D(_ProcedureTex2);SAMPLER(sampler_ProcedureTex2);
        TEXTURE2D(_ProcedureTex0);SAMPLER(sampler_ProcedureTex0);

    
    //feature
    #define _MAIN_COLOR
    #define _UV_SCALE2

    //lighting
    #define _VERTEX_GI
    #define _SELF_SHADOW
    #define _SCREEN_POS
    #define _CARTOON		
    #define _SM_DARK_RIM
    #define _HEAD
    #define _CSM3
    #define	_CUSTOM_SHADOW_MAP_MASK
    #if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
    #define _ADD_LIGHT
    #endif

    half4 _RimParam;
    half4 _LightFadePram;
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

    #define _FaceLightingUvMulX _FaceLightingParam.x
    #define _FaceLightingUvAddX _FaceLightingParam.y
    #define _FaceLightingCompare _FaceLightingParam.z

    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "UniversalForward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            // TODO:在新版卡通渲染中丢弃SSRim/SSShadow相关像素以提升性能。
            //ColorMask 0
            Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }
            HLSLPROGRAM
            
            // -------------------------------------
            // Universal Pipeline keywords
            //	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #define _SM_RIM
            // #pragma multi_compile _ _SMARTSOFTSHADOW_ON
            #define _SMARTSOFTSHADOW_ON
            #define _ROLE_ADDITIONAL_LIGHT_ON
            // #pragma multi_compile _ _PARKOUR
            #define _VERTEX_COLOR
            #define _CUSTOM_EFFECT
            #define _CUSTOM_VERTEX_PARAM
            #define _ROLE_HEIGHT_GRADIENT
            #define _INPUT_UV2_4
            #define _CUSTOM_VERTEX_PARAM
            #define _CUSTOM_LIGHT
            #define ROLE_SHADER_LEVEL
            #define _SRP_DEBUG

            #include "../../Include/ShaderLevel.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/Pragam_Head.hlsl"
            
            #ifdef _SM_4
                #pragma target 5.0
            #else//!_SM_4
                #pragma target 3.0
            #endif//_SM_4
                        
            #include "URP_Role_Head.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            #pragma shader_feature_local _ _PBS_FROM_PARAM
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting
            //#pragma shader_feature_local _ _SHADOW_MAP
            //#pragma shader_feature_local _ _EXTRA_SHADOW
            //#pragma shader_feature_local _ _NO_CSM

            //#pragma shader_feature_local _ _ADD_LIGHT
            // #pragma shader_feature_local _ _ARRAY_LIGHTBUFFER
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY

            //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

            // #pragma shader_feature_local _ _TOON_AO_FACE
            #pragma shader_feature _SHADER_DEBUG

            #pragma vertex  vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }
        
        Pass
        {
            Name "Outline"
            Offset 1,[_OutlineZOffset]
        	ZWrite[_OLZWrite]
            Tags
            {
                "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent"
            }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma target 3.0

            #define _FACEOUTLINE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "../Tools/Outline.hlsl"
            //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            // #pragma multi_compile _ _PARKOUR
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
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
//			#include "Role_Simple_Screen_Space_Rim.hlsl" 
//            ENDHLSL
//        }
        
		Pass
        {
            Name "FaceShadow"
            
            Tags { "LightMode" = "FaceShadow" }

            ZTest Equal
        	ZWrite Off
            ColorMask RGB
            Blend One Zero
        	//Blend DstColor Zero, SrcAlpha OneMinusSrcAlpha
            //Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_StencilRef_StencilShadow]
                Comp Equal
                Pass Keep
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM

            // #pragma multi_compile _ _SMARTSOFTSHADOW_ON
            #define _SMARTSOFTSHADOW_ON
            
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            #define _VERTEX_COLOR
            #define _CUSTOM_EFFECT
            #define _CUSTOM_VERTEX_PARAM
            #define _ROLE_HEIGHT_GRADIENT
            #define _INPUT_UV2_4
            #define _CUSTOM_VERTEX_PARAM
            #define _CUSTOM_LIGHT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/Pragam_Head.hlsl"
            
            #ifdef _SM_4
                #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4

            #define FACE_SHADOW_PASS

            #include "URP_Role_Head.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP
            // #pragma shader_feature_local _ _PBS_FROM_PARAM
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting
            //#pragma shader_feature_local _ _SHADOW_MAP
            //#pragma shader_feature_local _ _EXTRA_SHADOW
            //#pragma shader_feature_local _ _NO_CSM

            //#pragma shader_feature_local _ _ADD_LIGHT
            // #pragma shader_feature_local _ _ARRAY_LIGHTBUFFER
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY

            //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

            // #pragma shader_feature_local _ _TOON_AO_FACE

            #pragma vertex  vertForwardBase
            #pragma fragment fragForwardBase

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
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY
			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
        
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			float3 _LightDirection;

			struct Attributes
			{
			    float4 positionOS   : POSITION;
			    float3 normalOS     : NORMAL;
			};

			struct Varyings
			{
			    float4 positionCS   : SV_POSITION;
			};

			float4 GetShadowPositionHClip(Attributes input)
			{
			    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
			    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

			    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

				#if UNITY_REVERSED_Z
				    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#else
				    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
				#endif

			    return positionCS;
			}

			Varyings ShadowPassVertex(Attributes input)
			{
			    Varyings output;
			    output.positionCS = GetShadowPositionHClip(input);
			    return output;
			}

			half4 ShadowPassFragment(Varyings input) : SV_TARGET
			{
				return 0;
			}
				
			ENDHLSL
		}

        ////UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"
        //Pass
        //{
        //	Name "OverdrawForwardBase"
        //	Tags{"LightMode" = "OverdrawForwardBase"}

        //	Blend One One
        //	ZWrite[_ZWrite]
        //	Stencil
        //	{
        //		Ref[_StencilRef_Outline]
        //		Comp equal
        //	}
        //	CGPROGRAM

        //	#pragma vertex Vert
        //	#pragma fragment Frag

        //	#include "UnityCG.cginc"

        //	struct Attributes
        //	{
        //		float4 vertex : POSITION;
        //	};
        //	
        //	struct Varyings
        //	{
        //		float4 vertex : SV_POSITION;
        //	};
        //	Varyings Vert(Attributes v)
        //	{
        //		Varyings o;
        //		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
        //		o.vertex = mul(unity_MatrixVP, WorldPosition);
        //		return o;
        //	}

        //	half4 Frag(Varyings i) : SV_Target
        //	{
        //		return half4(0.1, 0.04, 0.02, 1);
        //	}

        //	ENDCG
        //}
        //Pass
        //{
        //	Name "OverdrawOutline"
        //	Tags{"LightMode" = "OverdrawOutline"}

        //	Blend One One
        //	Cull Front
        //	Offset 1,1
        //	Stencil
        //	{
        //		Ref[_StencilRef_Outline]
        //		Comp equal
        //	}
        //	HLSLPROGRAM

        //	#pragma vertex Vert
        //	#pragma fragment Frag

        //	#include "UnityCG.cginc"

        //	struct Attributes
        //	{
        //		float4 vertex : POSITION;
        //	};
        //	
        //	struct Varyings
        //	{
        //		float4 vertex : SV_POSITION;
        //	};
        //	Varyings Vert(Attributes v)
        //	{
        //		Varyings o;
        //		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
        //		o.vertex = mul(unity_MatrixVP, WorldPosition);
        //		return o;
        //	}

        //	half4 Frag(Varyings i) : SV_Target
        //	{
        //		return half4(0.1, 0.04, 0.02, 1);
        //	}
        //	
        //	ENDHLSL
        //}
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

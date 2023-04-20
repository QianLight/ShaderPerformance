Shader "URP/Role/CartoonDirty"
{
    Properties
    {
        //pbr
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)

        [NoScaleOffset]_ProcedureTex0 ("Normal:rg Mask:ba", 2D) = "" {}
        _UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
        _Param0("AO:x normalscale:y w:metallicMask", Vector) = (0,1,1,0)
        _Param1("IBLScale:z Cutout:w", Vector) = (0.1,0.5,0,0.5)
        _PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
        //mask
        [NoScaleOffset]_MainTex1("r:rim mask g: emissive b: shadow ", 2D) = "black" {}
        _Param2("x:maskEnable g:emissive b:shadowMask a:toonAoScale", Vector) = (0,0,0,0)

        //ramp
        [NoScaleOffset]_ProcedureTex2 ("Ramp", 2D) = "" {}
        _Color3("ShadowColorMultiply",Color)=(1,1,1,1)

        //dritymap
        [NoScaleOffset]_MainTex2 ("r:mask g:noise b:normalscale a", 2D) = "white" {}
        [NoScaleOffset]_ProcedureTex1 ("rg:DirtyNormal ba", 2D) = "" {}
        _Param3("r:dirtyint g:noiseint b:NoiseDisturbInt a:NoiseDisturbSpeed", Vector) = (0,0,0,0)
        _Param4("r:dirtnormalscale g:dirtnormalsint b:Expand a:", Vector) = (0,0,0,0)
        _Color6("noisecolor",Color)=(1,1,1,1)
        _Color2("linecolor",Color)=(1,1,1,1)

        //Dark Rim 		
        _Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)

        //emissive
        [HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

        //outline	
        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        _ColorOutline2("Outline Color2", Color) = (0,0,0,0)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

        _Param("", Vector) = (1,1,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)
        [HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)
        _HeightGradientColor("Gradient Color", Color) = (0,0,0,0)
        _ScreenSpaceRimParam("Screen Space Rim", Vector) = (5,5,0,0)
        _FaceLightmap("Face Lightmap", 2D) = "white" {}

        [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
        [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
        [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
        //[HideInInspector]_UVST0("", Vector) = (1,1,0,0)
        
        _SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
		_SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
        
        
        
        //CBuffer Fill
		[HideInInspector] _DebugMode("_DebugMode", FLOAT) = 1.0
		[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
        
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_UVST0("_UVST0", Vector) = (1,1,1,1)
        [HideInInspector]_UVST3("_UVST3", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
        [HideInInspector]_LaserColor("LaserColor", Color) = (0,0,0,0)
        [HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1) 
        [HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
        [HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
        
        [HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
        [HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }

    HLSLINCLUDE

    #define ROLE_EFFECT
    //feature
    #define _MAIN_COLOR
    #define _UV_SCALE2	

    #define _DIRTY_NORMAL
    #define _DirtyNormalTexs _ProcedureTex1
    #define _DirtyNormalScale _Param4.x
    #define _DirtyNormalInt _Param4.y

    //lighting
    #define _VERTEX_GI

    #define _SELF_SHADOW
    #define _SHADOW_MAP
    #define _CARTOON		
    #define _SM_DARK_RIM
    #define _CSM3
    #if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
    #define _ADD_LIGHT
    #endif

    #define _DIRTY
    #define _MaskTexDirty _MainTex2
    #define _NoiseDisturbInt _Param3.z
    #define _NoiseDisturbSpeed _Param3.w
    #define _ColorInt _Color6.w

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
    // half4 _RimColortoon;
    #define _RoleRimColor (_RimColortoon.xyz)
    #define _RoleRimMultiply (_RimColortoon.w)
    #ifdef _SM_DARK_RIM
    #define _DarkRimCutLocation _RimParam.z
    #define _DarkRimCutSmoothness _RimParam.w
    #define _DarkRimLightColor _Color4
    #endif

    #ifdef _SPHERE_DITHER_FADE
			#define _ALPHA_TEST
			#define _DITHER_TRANSPARENT
    #endif

    //----------------------
    #define URP_BASE
    #define REDEFINE_URP
    #define BUILTIN_SM_OFF
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
            Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            // -------------------------------------
            // Universal Pipeline keywords
            #define _SMARTSOFTSHADOW_ON
            #define _ROLE_ADDITIONAL_LIGHT_ON
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            // #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            // #pragma shader_feature_local _ _TOON_AO_FACE

            // ------------------------------------- 
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON

            #include "../Include/Pragam_Head.hlsl"
            
            #ifdef _SM_4
                #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4

            #include "URP_Role_CartoonDirty.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP
            #pragma shader_feature_local _ _PBS_FROM_PARAM
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting
            //#pragma shader_feature_local _ _SHADOW_MAP
            //#pragma shader_feature_local _ _NO_CSM

            //#pragma shader_feature_local _ _ADD_LIGHT
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
            

            #pragma vertex  vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent"
            }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite[_OLZWrite]
//            Offset 1,[_OutlineZOffset]

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

            // -------------------------------------
            // Universal Pipeline keywords
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            // #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            // #pragma shader_feature_local _ _TOON_AO_FACE

            // ------------------------------------- 
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON

            #include "../Include/Pragam_Head.hlsl"
            
            #ifdef _SM_4
                #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4

            #define FACE_SHADOW_PASS

            #include "URP_Role_CartoonDirty.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP
            // #pragma shader_feature_local _ _PBS_FROM_PARAM
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting
            //#pragma shader_feature_local _ _SHADOW_MAP
            //#pragma shader_feature_local _ _NO_CSM

            //#pragma shader_feature_local _ _ADD_LIGHT
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
            

            #pragma vertex  vertForwardBase
            #pragma fragment fragForwardBase

            ENDHLSL
        }
        
        //        Pass
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
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "../Include/Pragam_Head.hlsl"
//            #include "Role_Head.hlsl" 
//            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//			// TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
//
//			#include "Role_Simple_Screen_Space_Rim.hlsl"
//            
//            ENDHLSL
//        }
        
        Pass
        {
            Name "PlanarShadow"

            Tags
            {
                "LightMode" = "PlanarShadow"
            }

            ZWrite Off

            // 正片叠底
            Blend DstColor Zero

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
        
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

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
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY

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
    }*/
        //UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"	
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

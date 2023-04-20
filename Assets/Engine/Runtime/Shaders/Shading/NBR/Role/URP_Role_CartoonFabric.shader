Shader "URP/Role/CartoonFabric" {
    Properties {
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

        //Dark Rim 		
        _Color4("Dark Rim Color", Color) = (0.8,0.8,0.8,1)

        //emissive
        [HDR]_Color5("EmissionColor",Color)=(1,1,1,1)

        //fabric
        [NoScaleOffset] _ProcedureTex3 ("TangentMap", 2D) = "" {}
        _UVST3("Tangent Scale Offset", Vector) = (1,1,0,0)
        _AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        _FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        _FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)


        //outline
        //_ParamOutline("x:ouline fall start y:ouline fall end z:outline width falloff ", Vector) = (0.0,0.2,0.2,0.5)
        _ColorOutline("Outline Color", Color) = (0,0,0,0.003)
        _ColorOutline2("Outline Color2", Color) = (0,0,0,0)
        [HideInInspector]_MatEffectOutlineParam("Outline Color", Color) = (0,0,0,0)

        _Param("", Vector) = (1,1,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)
        [HDR]_RimColortoon("RimColor",Color) = (1,1,1,1)

        [HideInInspector] _SrcBlend("__src", FLOAT) = 1.0
        [HideInInspector] _DstBlend("__dst", FLOAT) = 0.0
        [HideInInspector] _ZWrite("__zw", FLOAT) = 1.0
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        
        _SSRimColor ("SSRim Color",Color) = (1,1,1,0.3)
        _SSRimParam0("x:Distance Threshold w:RimScale",Vector) = (0.1,0,0,2)
    	
    	//CBuffer Fill
		[HideInInspector] _DebugMode("_DebugMode", FLOAT) = 1.0
		[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
		[HideInInspector]_Color6("_Color6", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
    	[HideInInspector]_HeightGradientColor("_HeightGradientColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
    	[HideInInspector]_UVST0("_UVST0", Vector) = (1,1,1,1)
    	[HideInInspector]_Param3("_Param3", Vector) = (1,1,1,1)
    	[HideInInspector]_Param4("_Param4", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1) 
    	[HideInInspector]_LaserColor("_LaserColor",color) = (1,1,1,1) 
    	[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
    	[HideInInspector] _OLZWrite ("_OutlineZWrite", Float) = 0
    	[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }

    HLSLINCLUDE

    #define ROLE_EFFECT
    //feature
    #define _MAIN_COLOR
    #define _UV_SCALE2	
    //lighting
    #define _VERTEX_GI
    // #define _NO_AO
    #define _NO_EMISSIVE
    //#define _TICK_NORMAL

    // Begin By MJ //
    // #define _SHADOW_MAP
    #define _SELF_SHADOW
    #define _ANISOTROPY
    // #define _PBS_FROM_PARAM
    #define _CARTOON
    #define _CSM3
    // End By MJ //
    #if !defined(_ADD_LIGHT) && !defined(SHADER_API_MOBILE)
    #define _ADD_LIGHT
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
    // half4 _RimColortoon;
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

    SubShader {
        Tags {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }

        Pass {
            Name "UniversalForward"
            Tags {
                "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
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
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            // #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            // #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

            // ------------------------------------- 
            // Unity defined keywords
            // #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            // #pragma multi_compile _ LIGHTMAP_ON
            

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

            #include "Role_Head.hlsl"
            #include "../Include/URP_LightingHead.hlsl"
            
		    inline void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
            {
                TransformHeightGradient(Input, Interpolants);
            	SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
			}
            
            //Lighting
            #define _CUSTOM_LIGHT

            #define _UseDynamicHairAO _HeadForwardWorld.w
            FLOAT4 _HeadUpWorld;
            FLOAT4 _HeadForwardWorld;
            FLOAT4 _HairAOUvOffsetAndScale;

            inline void SM_Fabric(in FLightingContext LC,
                                  in FFragData FragData, in FMaterialData MaterialData,
                                  inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
            {
                #ifdef _ANISOTROPY
                FLOAT3 normal = MaterialData.WorldNormal;
                FLOAT3 tangent = MaterialData.WorldTangent;
                FLOAT3 binormal = MaterialData.WorldBinormal;
                FLOAT3 lightDir = LC.LightDir;
                FLOAT3 viewDir = FragData.CameraVector;
                FLOAT3 halfDir = LC.H;
                FLOAT nl = LC.FixNdotL;
                FLOAT nv = MaterialData.NdotV;
                FLOAT nh = LC.NdotH;
                FLOAT th = dot(tangent, LC.H);
                FLOAT bh = dot(binormal, LC.H);
                FLOAT tv = dot(tangent, FragData.CameraVector);
                FLOAT bv = dot(binormal, FragData.CameraVector);
                FLOAT tl = dot(tangent, LC.LightDir);
                FLOAT bl = dot(binormal, LC.LightDir);

                FLOAT roughness = lerp(MaterialData.Roughness, 1.0, _AnisotropyScale);
                FLOAT warpRoughness = lerp(1.0, _WarpRoughness, _AnisotropyScale) * max(0.1, MaterialData.Roughness);
                FLOAT weftRoughness = lerp(1.0, _WeftRoughness, _AnisotropyScale) * max(0.1, MaterialData.Roughness);

                FLOAT3 F = F_Schlick(LC.SpecularColor, LC.LdotH);
                FLOAT3 warpSpecular = D_GGXaniso2(roughness, warpRoughness, nh, th, bh) * V_SmithJointGGXAniso(
                    tv, bv, nv, tl, bl, nl, roughness, warpRoughness) * nl;
                warpSpecular *= _WarpIntensity * _WarpColor;

                FLOAT3 weftSpecular = D_GGXaniso2(weftRoughness, roughness, nh, th, bh) * V_SmithJointGGXAniso(
                    tv, bv, nv, tl, bl, nl, weftRoughness, roughness) * nl;

                FLOAT3 normalLeft = normalize(normal - tangent * _WeftBendFactor);
                FLOAT3 tangentLeft = cross(binormal, normalLeft);
                FLOAT nl2 = saturate(dot(normalLeft, lightDir));
                FLOAT nv2 = abs(dot(normalLeft, viewDir));
                FLOAT nh2 = saturate(dot(normalLeft, halfDir));
                FLOAT th2 = dot(tangentLeft, halfDir);
                FLOAT tv2 = dot(tangentLeft, viewDir);
                FLOAT tl2 = dot(tangentLeft, lightDir);
                FLOAT3 weftLeftSpecular = D_GGXaniso2(weftRoughness, roughness, nh2, th2, bh) * V_SmithJointGGXAniso(
                    tv2, bv, nv2, tl2, bl, nl2, weftRoughness, roughness) * nl2;

                FLOAT3 normalRight = normalize(normal + tangent * _WeftBendFactor);
                FLOAT3 tangentRight = cross(binormal, normalRight);
                FLOAT nl3 = saturate(dot(normalRight, lightDir));
                FLOAT nv3 = abs(dot(normalRight, viewDir));
                FLOAT nh3 = saturate(dot(normalRight, halfDir));
                FLOAT th3 = dot(tangentRight, halfDir);
                FLOAT tv3 = dot(tangentRight, viewDir);
                FLOAT tl3 = dot(tangentRight, lightDir);
                FLOAT3 weftRightSpecular = D_GGXaniso2(weftRoughness, roughness, nh3, th3, bh) * V_SmithJointGGXAniso(
                    tv3, bv, nv3, tl3, bl, nl3, weftRoughness, roughness) * nl3;

                weftSpecular = lerp(weftSpecular, lerp(weftLeftSpecular, weftRightSpecular, 0.5), 0.5) * _WeftIntensity
                    * _WeftColor;
                DirectSpecular = min(lerp(warpSpecular, weftSpecular, 0.5), 5);
                DirectSpecular *= _SpecMult * PI * F * LC.LightColor;
                // const float kMaxSpecular = 3.0;
                // DirectSpecular = normalize(DirectSpecular) * min(kMaxSpecular, Luminance(DirectSpecular));	
                DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
                #endif
            }

            void CustomLighting(FFragData FragData, FMaterialData MaterialData,
                                FShadowData ShadowData, FLOAT ShadowMask,
                                inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
            {
                FLOAT2 Shadow = ShadowData.Shadow.xy;
                FLOAT3 shadowLightColor = lerp(_MainLightColor.xyz, _RoleShadowColor.rgb, 1 - saturate(Shadow.y));
                FLightingContext LC = InitLight(FragData, MaterialData,_SpecMult,
                                                _MainLightDir.xyz, shadowLightColor, 1, ShadowData, _ShadowColor.xyz);
                DEBUG_CUSTOMDATA_PARAM(LC0, LC)

                SM_Ramp(FragData, MaterialData, LC, Shadow.y, DirectDiffuse DEBUG_PARAM);
                SM_Fabric(LC, FragData, MaterialData, DirectDiffuse, DirectSpecular DEBUG_PARAM);

                // UNITY_BRANCH
                // if (_AddLightEnable)
                // {
                //     #define specScale1 _SpecMult*0.5f
                //     FLightingContext LC1 = InitLight(FragData, MaterialData,specScale1,
                //                                      _AddLightDir.xyz,_AddLightColor.xyz, 1, DefaultShadowData(), 1);
                //     SM_SupplementDiffuse(LC1, DirectDiffuse DEBUG_PARAM);
                //     //SM_SupplementSpec(LC1,DirectSpecular DEBUG_PARAM);
                //     FLOAT3 Spec = 0;
                //     SM_Fabric(LC1, FragData, MaterialData, DirectDiffuse, Spec DEBUG_PARAM);
                //     DirectSpecular += Spec;
                //     DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
                // }
                DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, shadowLightColor)
            }

            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"

            //only editor use
            //#pragma shader_feature_local _ _DEBUG_APP
            #pragma shader_feature_local _ _PBS_FROM_PARAM
            #pragma shader_feature_local _ _NO_TANGENTMAP
            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //lighting
            //#pragma shader_feature_local _ _SHADOW_MAP
            #pragma shader_feature_local _ _EXTRA_SHADOW
            //#pragma shader_feature_local _ _NO_CSM

            //#pragma shader_feature_local _ _ADD_LIGHT
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            // #pragma shader_feature_local _ _ARRAY_LIGHTBUFFER

            // //#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

            #pragma vertex vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }
        
        Pass {
            Name "Outline"
//            Offset 1,[_OutlineZOffset]
            Tags{ "LightMode" = "Outline" "RenderType" = "Transparent" "Queue" = "Transparent" }
            Cull Front
        	Blend SrcAlpha OneMinusSrcAlpha
        	ZWrite[_OLZWrite]
        	
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
        
//        Pass
//        {
//            Name "ScreenSpaceRim"
//            
//            Tags { "LightMode" = "ScreenSpaceRim" }
//
//            ZTest Equal
//        	ZWrite Off
//            ColorMask RGB
//            Blend One one
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
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "../Include/Pragam_Head.hlsl"
//            #include "Role_Head.hlsl"
//   //          #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//   //
//			// TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
//            
//			#include "Role_Simple_Screen_Space_Rim.hlsl"
//            
//            ENDHLSL
//        }
        
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

			// -------------------------------------
			// Material Keywords
			#pragma shader_feature_local_fragment _ALPHATEST_ON
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma vertex ShadowPassVertex
			#pragma fragment ShadowPassFragment

            #include "URP_Role_ShadowCaster.hlsl"
			
			ENDHLSL
		}

        //UsePass "Hidden/Custom/Tools/OutLine/OUTLINE"
        
//        Pass {
//            Name "OverdrawForwardBase"
//            Tags {
//                "LightMode" = "OverdrawForwardBase"
//            }
//
//            Blend One One
//            ZWrite[_ZWrite]
//            Stencil {
//                Ref[_StencilRef_Role]
//                Comp equal
//            }
//            CGPROGRAM
//            #pragma vertex Vert
//            #pragma fragment Frag
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//
//            struct Attributes
//            {
//                float4 vertex : POSITION;
//            };
//
//            struct Varyings
//            {
//                float4 vertex : SV_POSITION;
//            };
//
//            Varyings Vert(Attributes v)
//            {
//                Varyings o;
//                float4 WorldPosition = mul(GetObjectToWorldMatrix(), v.vertex);
//                o.vertex = mul(GetWorldToHClipMatrix(), WorldPosition);
//                return o;
//            }
//
//            half4 Frag(Varyings i) : SV_Target
//            {
//                return half4(0.1, 0.04, 0.02, 1);
//            }
//            ENDCG
//        }
//        
//        Pass {
//            Name "OverdrawOutline"
//            Tags {
//                "LightMode" = "OverdrawOutline"
//            }
//
//            Blend One One
//            Cull Front
//            Offset 1,1
//            Stencil {
//                Ref[_StencilRef_Outline]
//                Comp equal
//            }
//            HLSLPROGRAM
//            #pragma vertex Vert
//            #pragma fragment Frag
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//
//            struct Attributes
//            {
//                float4 vertex : POSITION;
//            };
//
//            struct Varyings
//            {
//                float4 vertex : SV_POSITION;
//            };
//
//            Varyings Vert(Attributes v)
//            {
//                Varyings o;
//                float4 WorldPosition = mul(GetObjectToWorldMatrix(), v.vertex);
//                o.vertex = mul(GetWorldToHClipMatrix(), WorldPosition);
//                return o;
//            }
//
//            half4 Frag(Varyings i) : SV_Target
//            {
//                return half4(0.1, 0.04, 0.02, 1);
//            }
//            ENDHLSL
//        }
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

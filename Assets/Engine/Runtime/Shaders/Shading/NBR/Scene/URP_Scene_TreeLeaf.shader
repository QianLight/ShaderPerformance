Shader "URP/Scene/TreeLeaf" {
    Properties {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        [GAMMA]_Color0("Main Color", Color) = (1,1,1,1)

        _Param("", Vector) = (0,0,0,0)
        _Param0("tree center", Vector) = (0,0,0,0)
        _Param1("x:RimPower y:RimIntensity z:FresnelPower w:cutoff ", Vector) = (5,0.5,4,1)

        //wind
        _Param2("x:Magnitude y:Frenquency z:ModelScaleCorrection w:MaskRange ",Vector) = (0.5,0.5,0.2,0)
        _Param3("xyz:OffsetCorrection w:blend",Vector) = (1,1,0,0)
        _Param4("x:Range y:FresnelIntensity zw",Vector) = (1,1,0,0)

        _Param5("---",Vector) = (1,1,0,0)
        _Param6("---",Vector) = (1,1,0,0)

        _PBRParam("x:InverseLightIntensity y: z: w:", Vector) = (0,0.5,0,1)
        [HDR]_Color1("AmbientSky", Color) = (0.5,0.5,0.5,0)
        [HDR]_Color2("AmbientEquator", Color) = (0.5,0.5,0.5,0)
        [HDR]_Color3("AmbientGround", Color) = (0.5,0.5,0.5,0)

        [HDR]_Color4("---", Color) = (0.5,0.5,0.5,0)
        [HDR]_Color5("---", Color) = (0.5,0.5,0.5,0)

        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _DebugMode("__debugMode", Float) = 0.0

        [HideInInspector] _ZWriteImpostor("ZWriteImpostor", Int) = 0.0
        [HideInInspector] _ZTestImpostor("ZTestImpostor", Int) = 3.0 // Equal
    }

    HLSLINCLUDE
    //input layout
    #define _INPUT_UV2		
    #define _VERTEX_COLOR
    //feature
    #define _MAIN_COLOR
    #define _SCENE_EFFECT	
    #define _NO_NORMAL_MAP
    #define _OUTPUT_VERTEX_COLOR

    //lighting		
    #define _NO_METALLIC
    #define _NO_COMMON_EFFECT
    #define _NO_LIGHTMAP
    #define _NO_ADDLIGHT
    #define _GAMMA_CORRECT

    #define _CUSTOM_LOD

    #define _NO_MRT
    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Transparent" "Queue" = "Transparent+300" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }
        ZWrite[_ZWrite]
        Cull Off
        LOD 100

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode" = "UniversalForward"
            }
            ZWrite [_ZWriteImpostor]
            ZTest [_ZTestImpostor]
            Blend One Zero

            HLSLPROGRAM

            //----------------------
            #define URP_BASE
            #define REDEFINE_URP
            #define ENABLE_CLOUD

            //#define _ALPHA_TEST

            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile_fragment _ _SMARTSOFTSHADOW_ON

            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile  _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW

            //#pragma multi_compile _ _PARKOUR

            #pragma shader_feature _SHADER_DEBUG

            #if defined(_SHADER_LEVEL_HIGH)
                #define _CUSTOM_VERTEX_OFFSET
                #define _CUSTOM_VERTEX_PARAM
                #define _DETIAL_TREE_MOVE
            #elif defined(_SHADER_LEVEL_MEDIUM)
                #define _CUSTOM_VERTEX_PARAM
            #endif 

            #define _SHADOW_COLOR_ENABLE
			#define _SHADOW_RANDOM_OFFSET

            #define _SRP_DEBUG
			
            //开启阴影，这个未来可以集成到阴影的开关
            #define _LOD0

            //----------------------
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "../Include/Pragam_Head.hlsl"

            #ifdef _SM_4
            #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4

            #define UNIFORM_PCH_OFF
            CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Param;
                        float4 _Param0;
                        float4 _Param1;
                        float4 _Param2;
                        float4 _Param3;
                        float4 _Param4;
                        float4 _Param5;
                        float4 _Param6;
                        float4 _Color0;
                        float4 _Color1;
                        float4 _Color2;
                        float4 _Color3;
                        float4 _Color4;
                        float4 _Color5;
                        float4 _PBRParam;
                        float _DebugMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
            TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

            #define _MainColor _Color0
            #include "../Scene/Scene_Head.hlsl"
            #include "../Scene/Tree_Effect.hlsl"
            #include "../Include/URP_LightingHead.hlsl"

            #ifdef _CUSTOM_VERTEX_PARAM
			void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
			{
				#define _TreeCenterPos _Param0.xyz

				#define _AmbientSky _Color1.xyz
				#define _AmbientEquator _Color2.xyz
				#define _AmbientGround _Color3.xyz

				Interpolants.CustomData.xyz = mul(_objectToWorld, FLOAT4(_TreeCenterPos, 1)).xyz;

				FLOAT3 leaveNormal = normalize(Interpolants.WorldPosition.xyz - Interpolants.CustomData.xyz);
				const FLOAT3 worldUp = FLOAT3(0,1,0);
				FLOAT ndotUp = dot(leaveNormal, worldUp);
				FLOAT GroundToSky =  ndotUp * 0.5 + 0.5;
				FLOAT3 baseLerp = lerp(_AmbientGround, _AmbientSky, GroundToSky) ;
				FLOAT squareNdotUp = ndotUp * ndotUp;
				FLOAT equatorIntensity = lerp(0.3, 0.55, squareNdotUp);
				Interpolants.CustomData1.xyz = lerp(_AmbientEquator, baseLerp, equatorIntensity);
			}
            #endif


            inline void TreeBaseColor(in FFragData FragData, inout FMaterialData MaterialData)
            {
                FLOAT2 uv = GET_FRAG_UV;
                FLOAT4 color = SAMPLE_TEX2D(_MainTex, uv);
                MaterialData.BaseColor = color * _MainColor;
                MaterialData.DyeColor = MaterialData.BaseColor.rgb;
            }

            #define CalcCustomBaseColor TreeBaseColor
            #define _CUSTOM_BASECOLOR

            //---------------------------------------------------------------------------------
            inline FLOAT3 TreePBS(in FFragData FragData, inout FMaterialData MaterialData, out FLOAT2 rm)
            {
                rm = FLOAT2(1, 0);
                return FLOAT3(0.5, 0.5, 1);
            }

            #define CalcCustomPBS TreePBS
            #define _CUSTOM_PBS
            //---------------------------------------------------------------------------------

            FLOAT GetTreeShadowMapMask(in FFragData FragData, in FMaterialData MaterialData)
            {
                return 1;
            }

            #define GetCustumShadowMapMask GetTreeShadowMapMask
            #define _CUSTOM_SHADOW_MAP_MASK

            //---------------------------------------------------------------------------------

            FLOAT CalcTreeCustomAddNoL(in FFragData FragData, in FMaterialData MaterialData,FLOAT3 lightDir)
            {
                return 1;
            }

            #define CalcCustomAddNoL CalcTreeCustomAddNoL
            #define _CUSTOM_ADDNOL
            //---------------------------------------------------------------------------------

            void CalcTreeLighting(FFragData FragData, FMaterialData MaterialData, FShadowData ShadowData,
                                  FLOAT ShadowMask, inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
            {
                #define _TreeCenterPosBias _Color2.w
                #define _TreeRimPower _Param1.x
                #define _TreeRimIntensity _Param1.y
                #define _TreeFresnelPower _Param1.z
                #define _TreeFresnelIntensity _Param4.y
                #define _DackIntensity _Param4.z
                #define _DackRange _Param4.w

                #define _InverseLightIntensity _PBRParam.x
                #define _TargetLightIntensity _PBRParam.y
                #define _LightFlatness _PBRParam.z
                #define _SubSurfaceGain _PBRParam.w
                #define _ShadowOffset _Color1.w
                #define _FackLight _Color3.w

                #ifdef _CUSTOM_VERTEX_PARAM
				FLOAT3 TreeCenter = FragData.CustomData.xyz;
				FLOAT3 ambient = FragData.CustomData1.xyz;
                #else
                FLOAT3 TreeCenter = 0;
                FLOAT3 ambient = _Color2;//float3(0.3,0.7,0.2);
                #endif

                // NdotL
                FLOAT NdotL = dot(MaterialData.WorldNormal, _MainLightPosition.xyz); 
                FLOAT positiveNL = saturate(NdotL);
                FLOAT negativeNL = saturate(-NdotL);
                negativeNL *= _InverseLightIntensity;

                FLOAT3 diffuseTerm = lerp(positiveNL, _TargetLightIntensity, _LightFlatness);
                positiveNL = smoothstep(0, 0.5, positiveNL);
                diffuseTerm = lerp(diffuseTerm * _DackIntensity, diffuseTerm, positiveNL);

                // Ao
                //diffuseTerm = lerp(diffuseTerm*0.5,diffuseTerm, FragData.VertexColor.x);

                // subSurface
                FLOAT lv = saturate(-dot(_MainLightDir.xyz, FragData.CameraVector));
                FLOAT subSurfaceTerm = saturate(pow(lv, 2) * GetSrcShadow(ShadowData)) * _SubSurfaceGain;

				
				//阴影

				float shadow = GetFadeShadow(ShadowData);
				
                // Rim
                FLOAT3 adn = normalize(FragData.WorldPosition.xyz - TreeCenter);
                FLOAT adNv = saturate(abs(dot(adn, FragData.CameraVector) + _TreeCenterPosBias));
                FLOAT rim = pow(1 - adNv, _TreeRimPower) * _TreeRimIntensity;
                rim = lerp(0, rim, shadow);

                diffuseTerm = lerp(diffuseTerm * (_FackLight
#ifdef _SHADOW_COLOR_ENABLE
 + urp_ShadowColor.rgb
#endif
						), diffuseTerm, shadow);

                // Fresnel
                FLOAT fresnel = pow(1 - MaterialData.NdotV, _TreeFresnelPower) * _TreeFresnelIntensity;
                fresnel *= shadow;

				
				
                DirectDiffuse = (diffuseTerm * _MainLightColor.xyz ) + ambient + negativeNL;
                DirectDiffuse *= pow(abs(MaterialData.DiffuseColor), 0.4545) * kDieletricSpec.w;
                DirectSpecular = (rim + diffuseTerm + fresnel + subSurfaceTerm) * _MainLightColor.xyz * kDieletricSpec.x;
			            }

            #define CustomLighting CalcTreeLighting
            #define _CUSTOM_LIGHT

            #include "../../Include/ShaderDebug.hlsl"
            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"


            //#pragma shader_feature_local _ _DEBUG_APP
            //render type
            //#pragma shader_feature_local _ _ALPHA_TEST
            //#pragma multi_compile _ _ALPHA_TEST_IMPOSTOR
            #pragma shader_feature_local _ _ALPHA_BLEND

            #pragma vertex vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }
        
        Pass {
            Name "ShadowCaster"
            Tags {
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
            #define _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
			            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Param;
                        float4 _Param0;
                        float4 _Param1;
                        float4 _Param2;
                        float4 _Param3;
                        float4 _Param4;
                        float4 _Param5;
                        float4 _Param6;
                        float4 _Color0;
                        float4 _Color1;
                        float4 _Color2;
                        float4 _Color3;
                        float4 _Color4;
                        float4 _Color5;
                        float4 _PBRParam;
                        float _DebugMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
            TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

            #define _BaseMap _MainTex 
            #define _BaseMap_ST _MainTex_ST
            #define sampler_BaseMap sampler_MainTex
            #define _Cutoff _Param1.w
            #include "../../Include/OPPShadowCaster.hlsl"

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            ENDHLSL
        }
           
        Pass{
            Name "ShadowBake"
            Tags{"LightMode" = "ShadowBake"}

            ZWrite On
            ZTest LEqual

            Cull Off

            HLSLPROGRAM

            #pragma target 4.5
            #define SMART_SHADOW_DEPTH_OUTPUT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Param;
                        float4 _Param0;
                        float4 _Param1;
                        float4 _Param2;
                        float4 _Param3;
                        float4 _Param4;
                        float4 _Param5;
                        float4 _Param6;
                        float4 _Color0;
                        float4 _Color1;
                        float4 _Color2;
                        float4 _Color3;
                        float4 _Color4;
                        float4 _Color5;
                        float4 _PBRParam;
                        float _DebugMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
            TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

            #define _BaseMap _MainTex 
            #define _BaseMap_ST _MainTex_ST
            #define sampler_BaseMap sampler_MainTex

            #include "../../Include/SmartShadow.hlsl"
            #pragma vertex object_vert
            #pragma fragment object_frag

            ENDHLSL
        }
        
        Pass {
            
            Name "DepthPrepass"
            Tags {
                "LightMode" = "DepthPrepass"
            }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            HLSLPROGRAM
 

            //----------------------
            #define URP_BASE
            #define REDEFINE_URP

            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
			#pragma multi_compile _ _ALLOW_CROSS_FACE
            #pragma multi_compile _ _DITHER_TRANSPARENCY
            // #pragma multi_compile _ _PARKOUR

             #if defined(_SHADER_LEVEL_HIGH)
                #define _CUSTOM_VERTEX_OFFSET
                #define _CUSTOM_VERTEX_PARAM
                #define _DETIAL_TREE_MOVE
            #elif defined(_SHADER_LEVEL_MEDIUM)
                #define _CUSTOM_VERTEX_PARAM
            #endif 

            //----------------------
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "../Include/Pragam_Head.hlsl"

            #ifdef _SM_4
            #pragma target 5.0
            #else//!_SM_4
				#pragma target 3.0
            #endif//_SM_4

            #define UNIFORM_PCH_OFF
            CBUFFER_START(UnityPerMaterial)
                        float4 _MainTex_ST;
                        float4 _Param;
                        float4 _Param0;
                        float4 _Param1;
                        float4 _Param2;
                        float4 _Param3;
                        float4 _Param4;
                        float4 _Param5;
                        float4 _Param6;
                        float4 _Color0;
                        float4 _Color1;
                        float4 _Color2;
                        float4 _Color3;
                        float4 _Color4;
                        float4 _Color5;
                        float4 _PBRParam;
                        float _DebugMode;
            CBUFFER_END

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_ProcedureTex3);            SAMPLER(sampler_ProcedureTex3);
            TEXTURE2D(_ProcedureTex4);            SAMPLER(sampler_ProcedureTex4);

            #define _MainColor _Color0
            #include "../Scene/Scene_Head.hlsl"
            #include "../Scene/Tree_Effect.hlsl"
            #include "../Include/URP_LightingHead.hlsl"

            #ifdef _CUSTOM_VERTEX_PARAM
			void CustomVertex(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
			{
				#define _TreeCenterPos _Param0.xyz

				Interpolants.CustomData.xyz = mul(_objectToWorld, FLOAT4(_TreeCenterPos, 1)).xyz;
				Interpolants.CustomData1.xyz = 0.5;
			}
            #endif

            #define _ALPHA_TEST
            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"


            //#pragma shader_feature_local _ _DEBUG_APP
            //render type
            //#pragma shader_feature_local _ _ALPHA_TEST
            #pragma shader_feature_local _ _ALPHA_BLEND

            #pragma vertex vertForwardBase
            #pragma fragment fragForwardPreDepth
            
            ENDHLSL
        }
    }
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
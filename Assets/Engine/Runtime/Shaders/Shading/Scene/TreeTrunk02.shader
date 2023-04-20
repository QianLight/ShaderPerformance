Shader "URP/Scene/TreeTrunk02"
{
    Properties
    {
        //[HideInInspector] _WorkflowMode("WorkflowMode", Float) = 1.0
        [Header(Base Setting)]
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}

        [Toggle] _AlphaTest("Alpha Test", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        //[Toggle(_NEEDIMPOSTOR)] _NeedImpostor("Need Impostor", Float) = 0.0
        [XToggle] _NeedImpostor("Need Impostor", Float) = 0.0
        
        [XToggle] _Emission("Emission", Float) = 0.0
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)

        [NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Bump Scale", Float) = 1.0

        _RoughnessMin("Roughness Min", Range(0.0, 1.0)) = 0.0
        _RoughnessMax("Roughness Max", Range(0.0, 1.0)) = 1.0
        [XToggle] _Metallic("Metallic", Float) = 0.0
        _MetallicMin("Metallic Min", Range(0.0, 1.0)) = 0.0
        _MetallicMax("Metallic Max", Range(0.0, 1.0)) = 1.0

        [XToggle] _Occlusion("Occlusion", Float) = 0.0
        _OcclusionScale("Occlusion Strength", Range(0.0, 1.0)) = 1.0

        [Header(Environment)]
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections("Environment Reflections", Float) = 1.0

        [Header(Advance)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2.0

        _ReceiveShadows("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
        [HideInInspector] _DitherTransparency("_DitherTransparency", Range(0.0, 1.0)) = 1.0
            
        // Will set "ENABLE_FANCY" shader keyword when set
        //[Toggle(ENABLE_FANCY)] _Fancy("Fancy?", Float) = 0
        //[KeywordEnum(None, Add, Multiply)] _Overlay ("Overlay mode", Float) = 0
        //[PowerSlider(3.0)] _Shininess("Shininess", Range(0.01, 1)) = 0.08
        //[Space] _Prop1("Prop1", Float) = 0
    }

        SubShader
        {
            // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
            // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
            // material work with both Universal Render Pipeline and Builtin Unity Pipeline
            Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "Queue" = "Geometry-100" }
            LOD 300

            // ------------------------------------------------------------------
            //  Forward pass. Shades all light in a single pass. GI + emission + Fog
            Pass
            {
                // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
                // no LightMode tag are also rendered by Universal Render Pipeline
                Name "ForwardLit"
                Tags{"LightMode" = "UniversalForward"}

                Blend[_SrcBlend][_DstBlend]
                ZWrite[_ZWrite]
                ZTest[_ZTest]
                Cull[_Cull]

                HLSLPROGRAM
            // Required to compile gles 2.0 with standard SRP library
            // All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
            //#pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #define _NORMALMAP
			#define _SHADOW_COLOR_ENABLE
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _EMISSION_ON
            //#pragma shader_feature_local_fragment _METALLIC_ON
            //#pragma shader_feature_local_fragment _OCCLUSION_ON
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            //#pragma shader_feature_local_fragment _NEEDIMPOSTOR
                
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            //-#pragma multi_compile_fragment _ _SHADOWS_SOFT
            //#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY
            // #pragma multi_compile _ _PARKOUR

            #pragma shader_feature _SHADER_DEBUG
            // -------------------------------------
            // Unity defined keywords
            //#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //#pragma multi_compile _ LIGHTMAP_ON
            //#pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            //#pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            //-#pragma multi_compile _ _FORCE_SHADOWMASK

        #define LIGHTMAP_ON
        #define SHADOWS_SHADOWMASK
        #if defined(LIGHTMAP_ON)
            #pragma multi_compile _ _SMARTSOFTSHADOW_ON
            #define _FORCE_SHADOWMASK
            #define _SHADOWS_SOFT
        #else
            #define _SHADOWS_SOFT
        #endif
                
        #define _URP_DEBUG

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            //#define _NORMALMAP

            #include "../Include/ShaderDebug.hlsl"
            #include "TreeTrunk02ForwardPass.hlsl"
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
            //#pragma exclude_renderers gles gles3 glcore
            //#pragma target 4.5

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "TreeTrunkInput.hlsl"
            #include "../Include/OPPShadowCaster.hlsl"
            ENDHLSL
        }
            /*
                Pass
            {
                Name "DepthOnly"
                Tags{"LightMode" = "DepthOnly"}

                ZWrite On
                ColorMask 0
                Cull[_Cull]

                HLSLPROGRAM
                //#pragma exclude_renderers gles gles3 glcore
                #pragma target 4.5

                #pragma vertex DepthOnlyVertex
                #pragma fragment DepthOnlyFragment

                // -------------------------------------
                // Material Keywords
                #pragma shader_feature_local_fragment _ALPHATEST_ON
                //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

                //--------------------------------------
                // GPU Instancing
                #pragma multi_compile_instancing
                #pragma multi_compile _ DOTS_INSTANCING_ON

                #include "UberInput.hlsl"
                #include "../Include/OPPShadowCaster.hlsl"
                ENDHLSL
            }

                // This pass it not used during regular rendering, only for lightmap baking.
                Pass
            {
                Name "Meta"
                Tags{"LightMode" = "Meta"}

                Cull Off

                HLSLPROGRAM
                //#pragma exclude_renderers gles gles3 glcore
                #pragma target 4.5

                #pragma vertex UniversalVertexMeta
                #pragma fragment UniversalFragmentMeta

                #pragma shader_feature_local_fragment _SPECULAR_SETUP
                #pragma shader_feature_local_fragment _EMISSION
                #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
                #pragma shader_feature_local_fragment _ALPHATEST_ON
                //#pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
                #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

                #pragma shader_feature_local_fragment _SPECGLOSSMAP

                #include "UberInput.hlsl"
                #include "UberMetaPass.hlsl"

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
                    #include "UberInput.hlsl"
                    #include "../Include/SmartShadow.hlsl"
                    #pragma vertex object_vert
                    #pragma fragment object_frag

                    ENDHLSL
            }
            */
            
        }
            FallBack "Hidden/Universal Render Pipeline/FallbackError"
            //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UberShader"
}

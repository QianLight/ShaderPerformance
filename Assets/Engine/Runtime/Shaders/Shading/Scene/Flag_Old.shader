Shader "URP/Scene/Flag_old"
{
    Properties
    {
         _Color0("Main Color", Color) = (1,1,1,1)
        _MainTex ("FirstTexture", 2D) = "white" {}
        _FlagValue("_FlagWaveInt,_FlagWaveSpeed,_FlagWaveMove,_FlagFixed",Vector) = (0.1,1,0,0)
        
        [Header(Advance)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2.0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Trasnparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Off  
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite]

            HLSLPROGRAM

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
			#pragma multi_compile_local _ _DITHER_TRANSPARENCY

            #pragma vertex vert
            #pragma fragment frag

            #include "../Include/OPPCore.hlsl"
            #include "../NBR/Include/Fog.hlsl"

            CBUFFER_START(UnityPerMaterial)
                    float4 _MainTex_ST;
                    half4 _FlagValue;
                    half4 _Color0;
                    float _DitherTransparency;
            CBUFFER_END
            
            half4 _SceneColorAdjustmentParams;
            #define _SceneExposure _SceneColorAdjustmentParams.x

            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);

            struct appdata_t {
                half4 vertex : POSITION;
                half4 color : COLOR;
                half2 texcoord : TEXCOORD0;
               
            };
            struct v2f {
                half4 vertex : SV_POSITION;
                half4 color : COLOR;
                half2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };           

            v2f vert (appdata_t v)
            {
                v2f o;
                o.uv.x = smoothstep(_FlagValue.w,1,v.texcoord.x);          
                v.vertex.y += _FlagValue.x  * sin(v.vertex.x +_Time.z*_FlagValue.y) *o.uv.x;
                v.vertex.z += _FlagValue.x  * sin(v.vertex.x +_Time.z*_FlagValue.y) *o.uv.x;
                v.vertex.z += _FlagValue.z *o.uv.x ;
                o.positionWS = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformObjectToHClip(v.vertex.xyz);                   
                o.uv = v.texcoord.xy;     
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEX2D(_MainTex, i.uv * _MainTex_ST.xy + _MainTex_ST.zw) * _Color0;

                APPLY_FOG(col.rgb, i.positionWS);
                
                SphereDitherTransparent(i.vertex, _DitherTransparency);

                col.rgb *= _SceneExposure;

                return col;
            }
            
            ENDHLSL
        }

//        Pass
//        {
//            Name "ShadowCaster"
//            Tags{"LightMode" = "ShadowCaster"}
//
//            ZWrite On
//            ZTest LEqual
//            ColorMask 0
//            Cull[_Cull]
//
//            HLSLPROGRAM
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//
//            CBUFFER_START(UnityPerMaterial)
//                    float4 _MainTex_ST;
//                    half4 _FlagValue;
//                    half4 _Color0;
//                    float _DitherTransparency;
//            CBUFFER_END
//
//            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
//
//            #define _BaseMap _MainTex 
//            #define _BaseMap_ST _MainTex_ST
//            #define sampler_BaseMap sampler_MainTex
//            #define _Cutoff 0.5
//            #include "../Include/OPPShadowCaster.hlsl"
//
//            // -------------------------------------
//            // Material Keywords
//            #pragma shader_feature_local_fragment _ALPHATEST_ON
//            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
//
//            //--------------------------------------
//            // GPU Instancing
//            #pragma multi_compile_instancing
//            #pragma multi_compile _ DOTS_INSTANCING_ON
//
//            #pragma vertex ShadowPassVertex
//            #pragma fragment ShadowPassFragment
//
//            ENDHLSL
//        }
    }
//    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
Shader "URP/Scene/UberGrass"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)
        _AmbientWind ("_AmbientWind", 2D) = "white" {}
        _Cutoff("_Cutoff",Range(0,1))=0.5
        _Transvalue("混合过渡范围大小",Range(0.01,1)) =0.4
        _ColorR("混合颜色1", Color) = (0.73,0.72,0,1)
        _WeightRG("颜色1和2混合",Range(0.1,1))=0.67
        _ColorG("混合颜色2", Color) = (0,0.5,0,1)
        _WeightGB("颜色2和3混合",Range(0,0.9))=0.33
        _ColorB("混合颜 色3", Color) = (1,0.75,0,1)
        _ColorSSS("3s半透颜色",Color)=(1,1,1,1)
        _ColorSpecular("高光反射颜色",Color)=(1,1,1,1)
        _SpecularRamp("3s半透上下过渡值",Range(1,10))=10
        _SSSSpecular("3s半透亮度",Range(1,10))=1
        _Specular("反射高光亮度",Range(1,10))=1
        _Smoothness("高光和3s范围",Range(1,30))=10

        _BottomPersent("地表吸色比重",Range(0,1))=0.3
        _BottomScale("地表过渡软硬",Range(0,1))=0.2
        [Header(WindControl)]
        [Space(3)]
        _PushAngle("花互动弯曲角度",Range(0,180))=180
        _PushValue("草互动移动值",Float)=0.75
        _AmbientWindDirx("x方向频率",Range(-10,10))=3
        _AmbientWindDirz("z方向频率",Range(-10,10))=1
        _AmbientWindSpeed("风的速度",Range(0,10))=3
        _GustingStrength("摆动强度(左右)",Range(-100,100))=20
        _GustingFrequency("摆动频率",Float)=1
        [ToggleOff] _IsFlower("_isFlower", Float) = 1.0
        [HideInInspector] _AlphaTest("AlphaTest", Float) = 0.5
        
    }

    HLSLINCLUDE
    #define _INPUT_UV2
    #define _MAIN_COLOR
    #define _WORLD_UV_OFFSET
    #define _NO_NORMAL_MAP
    #define _SIMPLE_NORMAL
    #define _SCENE_EFFECT
    #define _VERTEX_GI
    #define _NO_COMMON_EFFECT
    #define _USE_CUSTOM_SHADOW_BIAS
    #define _CUSTOM_SHADOW_BIAS 0.01
    #define _CUSTOM_LOD	
    #define _NO_MRT
    #define _ALPHA_BLEND
    #define _CUSTOM_VERTEX_OFFSET
    #define LIGHTMAP_ON
    ENDHLSL

    SubShader
    {
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "DepthPrepass"
            Tags
            {
                "LightMode" = "DepthPrepass"
            }
            Cull Off
            ColorMask 0
            HLSLPROGRAM
            #pragma target 4.5
            #define _SHADOW_COLOR_ENABLE
            #define URP_BASE
            #define REDEFINE_URP

            #define _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile_instancing
            #pragma multi_compile _ _INSTANCING_HIGH
            #pragma shader_feature_local _ISFLOWER_OFF

            #define DEPTH_PRE_PASS

            #include "../../Include/URP_Grass_ForwardPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }
        Pass
        {
            Name "FORWARD"
            Tags
            {
                "LightMode" = "GrassForward"
            }
            //ZWrite[_ZWriteHSR]        
            //ZTest[_ZTestHSR]
            ZWrite Off
            ZTest Equal
        
            Cull Off

            HLSLPROGRAM
            #pragma target 4.5
            #define _SHADOW_COLOR_ENABLE
            #define URP_BASE
            #define REDEFINE_URP

            //#define _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _SHADER_LEVEL_HIGH

            #pragma multi_compile_instancing
            #pragma multi_compile _ _INSTANCING_HIGH
            #pragma shader_feature_local _ISFLOWER_OFF

            #include "../../Include/URP_Grass_ForwardPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            Cull Off
            ColorMask 0
            HLSLPROGRAM
            #pragma target 4.5
            #define _SHADOW_COLOR_ENABLE
            #define URP_BASE
            #define REDEFINE_URP

            #define _ALPHATEST_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile_instancing
            #pragma multi_compile _ _INSTANCING_HIGH
            #pragma shader_feature_local _ISFLOWER_OFF

            #define DEPTH_PRE_PASS

            #include "../../Include/URP_Grass_ForwardPass.hlsl"
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            ENDHLSL
        }
    }
}
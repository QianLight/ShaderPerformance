Shader "URP/Scene/UberGrass1" {
    Properties {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        _Color0("Main Color", Color) = (1,1,1,1)
	    _AmbientWind ("_AmbientWind", 2D) = "white" {}
		_Cutoff("_Cutoff",Range(0,1))=0.5

	//	 [NoScaleOffset]_CustomLightmap("_CustomLightmap",2D)="white"{}
	//	 [NoScaleOffset]_CustomGroundColor("_CustomGroundColor", 2D) = "" {}
	 //  _GrassNormalDir("统一法线方向",Vector)=(0,0,0,0)
	      _Transvalue("混合过渡范围大小",Range(0.01,1)) =0.4
	     _ColorR("混合颜色1", Color) = (0.73,0.72,0,1)
		 _WeightRG("颜色1和2混合",Range(0.1,1))=0.67
	     _ColorG("混合颜色2", Color) = (0,0.5,0,1)
		 _WeightGB("颜色2和3混合",Range(0,0.9))=0.33
	     _ColorB("混合颜色3", Color) = (1,0.75,0,1)
		 _ColorSSS("3s半透颜色",Color)=(1,1,1,1)
		  _ColorSpecular("高光反射颜色",Color)=(1,1,1,1)
		 _SpecularRamp("3s半透上下过渡值",Range(1,10))=10
	     _SSSSpecular("3s半透亮度",Range(1,10))=1
		 _Specular("反射高光亮度",Range(1,10))=1
	     _Smoothness("高光和3s范围",Range(1,30))=10

	//	_SSSlightcontrol("灯光微调:xyz方向，w距离",Vector)=(0,0,0,0)	  
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
        //[HideInInspector] _SrcBlend("__src", Float) = 1.0
        //[HideInInspector] _DstBlend("__dst", Float) = 0.0
        //[HideInInspector] _ZWrite("__zw", Float) = 1.0
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
    #define _CUSTOM_VERTEX_OFFSET
	#pragma multi_compile _ _SMARTSOFTSHADOW_ON
	#define LIGHTMAP_ON
    ENDHLSL

    SubShader {
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
                Tags {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }

		Pass
		{
			Name "FORWARD"
			Tags{"LightMode" = "UniversalForward" }
            Cull Off
			
			HLSLPROGRAM

				#pragma target 4.5
				/////////////////////////////////Lod/////////////////////////////////
				//High
			//	#if defined(_LOD0)
				//	#define _CUSTOM_VERTEX_OFFSET
				//    #define _EXTRA_SHADOW
				//	#define _GRASS_COLLISION
			//	#elif defined(_LOD1)
				//	#define _CUSTOM_VERTEX_OFFSET
			//	#endif

				//Mid
			//	#if defined(_LOD_M0)
					//#define _CUSTOM_VERTEX_OFFSET
			//	#endif
			    #define _SHADOW_COLOR_ENABLE
	            #define URP_BASE
	            #define REDEFINE_URP
		
				//---------------------------------------------------------------------------------
				//debug
				//#pragma shader_feature_local _ _DEBUG_APP

			    // Universal Pipeline keywords
	            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
	            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

		        //#pragma multi_compile_fragment _ _SMARTSOFTSHADOW_ON
	    		#pragma multi_compile _ _SMARTSOFTSHADOW_ON

	            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
	            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
	            #pragma multi_compile _ SHADOWS_SHADOWMASK

				//#pragma multi_compile_local _ _DITHER_TRANSPARENCY

	            //----------------------
				//render type
			//	#pragma shader_feature_local _ _ALPHA_TEST
			//	#pragma shader_feature_local _ _ALPHA_BLEND
				//#pragma shader_feature_local _ _INSTANCE

			#pragma multi_compile_instancing
			//#pragma multi_compile _ DOTS_INSTANCING_ON



				#pragma shader_feature_local _ISFLOWER_OFF
				#pragma shader_feature_local _ _LOD0 _LOD1 _LOD2 _LOD_FAR
				#pragma shader_feature_local _ _LOD_M0 _LOD_M1 _LOD_M2 _LOD_MFAR
				#pragma shader_feature_local _ _LOD_L0 _LOD_L1 _LOD_L2 _LOD_LFAR

				#include "../../Include/URP_Grass_ForwardPass.hlsl"
				#pragma vertex LitPassVertex
				#pragma fragment LitPassFragment

			ENDHLSL
		}	

		      //Pass{
        //        Name "ShadowBake"
        //        Tags{"LightMode" = "ShadowBake"}

        //        ZWrite On
        //        ZTest LEqual

        //        Cull Off

        //        HLSLPROGRAM

        //        #pragma target 4.5
        //        #define SMART_SHADOW_DEPTH_OUTPUT
        //        #include "../../Scene/UberInput.hlsl"
        //        #include "../../Include/SmartShadow.hlsl"
        //        #pragma vertex object_vert
        //        #pragma fragment object_frag_all

        //        ENDHLSL
        //    }

    }
}
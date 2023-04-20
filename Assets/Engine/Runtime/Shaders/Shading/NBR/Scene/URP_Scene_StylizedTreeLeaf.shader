Shader "URP/Scene/StylizedTreeLeaf"
{
    Properties
    {
        [Header(BaseColor)]
        [Toggle(_COLORORTEX)] _ColorOrTex("ColorOrTex",int) = 0
        //_BaseColor3("Color3", Color) = (1,1,1,1)
        _BaseColor ("Color", Color) = (1,1,1,1)
        _saturate("饱和度",range(0,1)) = 1
        _BaseColor1("BaseColor", Color) = (1,1,1,1)
        _ColorDetails("Color细节强度",range(0,1)) = 0.5
        //[Toggle(_LERPCOLORXY)] _LerpColorXY("XorY",int) = 0
        _LerpColor ("垂直染色", Color) = (1,1,1,1)
        _TreeLerpTop("树冠染色顶端高度", Range(0,10)) = 0
        _TreeLerpRoot("树冠染色底端高度", Range(0,1)) = 1
        _TreeLerpIntensity("树冠染色强度",range(1,100)) = 20
		_MainTex ("Albedo", 2D) = "white" {}//[MainTexture]
        
        [Header(Cutoff)]
        _stepA("去黑边", Range(0.0, 1.0)) = 0.84
        _Cutoff("Alpha Cutoff(阴影)", Range(0.0, 1.0)) = 0.5
        _CutIntensity("立面剔除强度", Range(0, 5)) = 0.95
        
        [Header(NPR)]
        _LightIntensity("亮部强度",range(1,20)) = 1
        _FaceLightGrayIntensity("亮部灰阶强度",range(0,7)) = 1
        _DarkColor ("暗部颜色", Color) = (0,0,0,1)
        _ToonCutPos("明暗交界线位置",range(-1,1)) = 0
        _SHIntensity("LightProbes强度",range(0,10)) = 1
        _SHColorIntensity("LightProbes染色LightColor强度",range(0,1)) = 1
        _LightSHPow("染色光的范围",range(-1,1)) = 0
        
        [Header(Shadow)]
		_LocalShadowDepthBias("影子偏移值", float) = 0.25
        _shadowAttenInt("影子投影强度(近处)", Range(0, 1)) = 0.5
        _SmartShadowInt("SmartShadow强度(远处)", Range(0, 1)) = 0.5
        _noiseTexTree("noiseTex", 2D) = "white" {}
        _noiseIntTree("noiseInt",range(0,0.015)) = 0
        _noiseOffestTree("noiseOffest",range(0,0.15)) = 0
        
        [Header(AO)]
        _AOTint("AO颜色",color) = (1,1,1,1)
        _AORange("AO范围", Range(0.1,1)) = 1

        [Header(Subsurface)]
        _SubSurfaceGain("透光强度", Range(0,20)) = 1
        _SubSurfaceScale("透光范围", Range(-1,1)) = -1
        _subSurfaceTermInt("透光细节强度",range(0,1)) = 0.5

        //[Header(Debug)]
        [HideInInspector][Toggle(_DEBUGMODE)]_DebugMode("Debug Mode", Float) = 0
        [HideInInspector]_Debug("Debug AO", Float) = 1

        [Space(20)]
        [Header(HardRim)]
        //[Toggle(_HARDRIM)] _UsingHardRim("使用HardRim",int) = 0
        //[HDR]_HardRimTint ("HardRimColor", Color) = (1,1,1,1)
        //_LodMask("LodMask",range(0,1)) = 0.5       // 主要由于颜色渐变导致的灰度值域需要剔除
        //_HardRimDistanceIntensity("HardRim距离颜色变化 ",float) = 2000  // 主要实现颜色渐变
        //_HardRimWidth("HardRim宽度",range(0,0.05)) = 0.01

        [Header(Wind)]
        _Magnitude("随风漂移强度", float) = .5
        _Frenquency("随风飘动频率", float) = .5
        _ModelScaleCorrection("模型规模调整", float) = .5
        _MaskRange("MaskRange", float) = .5
        _OffsetCorrection("Offset纠正", vector) = (0,0,0,0)
        _Blend("_Blend", float) = .5
        _StableRange("_StableRange", float) = .5

//        [Space(20)]
//        _CustomBloomIntensity("_CustomBloomIntensity", Range(0, 2)) = 1.0
//        _CustomBloomAlphaOffset("_CustomBloomAlphaOffset", Range(-1, 1)) = 0

        [Space(20)]
        [Header(Dither)]
        _DitherAmountMax("DitherMax", Range(0, 25)) = 0
        _DitherAmountMin("DitherMin", Range(0, 25)) = 0
        
        [HideInInspector] _ZWriteImpostor("ZWriteImpostor", Int) = 0.0
        [HideInInspector] _ZTestImpostor("ZTestImpostor", Int) = 3.0 // Equal
    	[HideInInspector] _DitherTransparency("_DitherTransparency", Range(0.0, 1.0)) = 1.0

    }
    SubShader
    {
        Tags {	"RenderType"="AlphaTest"
                "Queue" = "AlphaTest"
				"PerformanceChecks"="False"
				"RenderPipeline" = "UniversalPipeline"
				"IgnoreProjector" = "True" }
		LOD 200

        Pass
        {
			Name "ForwardLitTreeLeaves"
            Tags {"LightMode"="UniversalForward"}
            Cull Off
            ZWrite [_ZWriteImpostor]
            ZTest [_ZTestImpostor]
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            //#pragma multi_compile_instancing  /// 树固定开 instancing,但是这里不能精简否则 bundle 里有问题（星空遗留的情况，不清楚现在是否还这样）
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #define _SHADOWS_SOFT
            //#pragma multi_compile_fragment _ _HUAWEI
            //#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            //#pragma multi_compile _ SHADOWS_SHADOWMASK
            #define _SMARTSOFTSHADOW_ON 1
            #pragma multi_compile _ _SHADER_LEVEL_HIGH
            #pragma multi_compile _ _FX_LEVEL_HIGH
            #pragma shader_feature _ _COLORORTEX
            #pragma shader_feature _ _DEBUGMODE
            //#pragma shader_feature _ _HARDRIM
            #pragma multi_compile_fragment _ _ALPHATEST_ON //impostor 需要使用
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            //#define _DITHER_TRANSPARENCY
            #define _STYTREELEAF 1
            #define _SUBSURFACE 1
            #define _LIGHTPROBE 1
        // Shader分档
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			#include "WindTreeLeavesPassNew.hlsl"
            ENDHLSL
        }
        Pass
        {
			Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            Cull Off
        	ColorMask 0
			HLSLPROGRAM
            #pragma target 3.0
            //#pragma multi_compile_instancing  /// 树固定开 instancing，但是这里不能精简否则 bundle 里有问题（星空遗留的情况，不清楚现在是否还这样）
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "WindTreeLeavesPassNew.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#define _ALPHATEST_ON
			#define _TREELEAF

            #define _BaseMap _MainTex 
            #define _BaseMap_ST _MainTex_ST
            #define sampler_BaseMap sampler_MainTex
            #define _Cutoff 0.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

			v2f DepthOnlyVertex(appdata input)
			{
				v2f output;
				INITIALIZE_OUTPUT(v2f,output);
   
			    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
			    float3 positionWS = TransformObjectToWorld(input.positionOS);
			    positionWS += CustomWindEffect(input.uv3,positionWS);
                output.positionHCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1));
                //output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
				return output;
			}
            half4 DepthOnlyFragment(v2f i): SV_Target
			{
			    half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				
				#if defined(_ALPHATEST_ON)
			    clip(step(_stepA,col.a) - _Cutoff);//step(_stepA,col.a)
				#endif

			    return 0;
			}
            ENDHLSL
        }
        Pass
    	{
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
            // #pragma multi_compile_instancing
            // #pragma multi_compile _ DOTS_INSTANCING_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "WindTreeLeavesPassNew.hlsl"
            float3 _LightDirection;
            #define _BaseMap _MainTex 
            #define _BaseMap_ST _MainTex_ST
            #define sampler_BaseMap sampler_MainTex
            #define _Cutoff 0.5

            float4 GetShadowPositionHClip(appdata input)
			{
			    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
			    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
				positionWS += CustomWindEffect(input.uv3,positionWS);
			    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

			#if UNITY_REVERSED_Z
			    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#else
			    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
			#endif

			    return positionCS;
			}
			v2f ShadowPassVertex(appdata input)
			{
				v2f output;
            	INITIALIZE_OUTPUT(v2f,output);
            	
            	//UNITY_SETUP_INSTANCE_ID(input);
			    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
            	output.positionHCS = GetShadowPositionHClip(input);
				return output;
			}
            half4 ShadowPassFragment(v2f i): SV_Target
			{
			    half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
				#if defined(_ALPHATEST_ON)
			    clip(step(_stepA,col.a) - _Cutoff);//step(_stepA,col.a)
				#endif

			    return 0;
			}

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            ENDHLSL
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
            #define _ALPHATEST_ON
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
            //#define _DITHER_TRANSPARENCY
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "WindTreeLeavesPassNew.hlsl"
            #pragma vertex TreePreVert
            #pragma fragment TreePreFrag
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                float2 uv3 : TEXCOORD2;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionCS   : SV_POSITION;
            };

			Varyings TreePreVert(Attributes input)
			{
				Varyings output;

			    output.uv = input.texcoord;
			    float3 positionWS = TransformObjectToWorld(input.positionOS);
			    positionWS += CustomWindEffect(input.uv3,positionWS);
                output.positionCS = mul(UNITY_MATRIX_VP, float4(positionWS, 1));
			    
				return output;
			}

            half4 TreePreFrag(Varyings input) : SV_TARGET
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            	#if defined(_ALPHATEST_ON)
                clip(step(_stepA,color.a) - _Cutoff);
            	#endif
				SphereDitherTransparent(input.positionCS, _DitherTransparency);
	            return 0;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowBake"
            Tags{"LightMode" = "ShadowBake"}

            ZWrite On
            ZTest LEqual

            Cull Off

            HLSLPROGRAM

            #pragma target 4.5
            #define SMART_SHADOW_DEPTH_OUTPUT
            #define BASEMAP_DEFINED
            #include "WindTreeLeavesInput.hlsl"
            #include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

                    #define _BaseMap _MainTex 
                    #define _BaseMap_ST _MainTex_ST
                    #define sampler_BaseMap sampler_MainTex
            
            #pragma vertex object_vert
            #pragma fragment object_frag

            ENDHLSL
        }
    }
	CustomEditor "TreeLeavesShaderGUI"
}

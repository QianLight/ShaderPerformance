Shader "URP/Scene/Glass" {
    Properties {
		[HDR]_Color0("Main Color", Color) = (1,1,1,1)
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		[NoScaleOffset] _ProcedureTex0 ("xy:Normal z:ThicknessTex ", 2D) = "" {}
        [NoScaleOffset] _ProcedureTex1 ("MatCapTex", 2D) = "white" {}

		[HDR]_Color1("SpColor", Color) = (1,1,1,1)
			
		_Param0("x:_FresnelEdge y:_FresnelIntensity z:_RefIntensity w:_BumpMapInt", Vector) = (1,-1,3,1)
		_Param1("x:LaserContrast y:LaserInt z:LaserTiling w:", Vector) = (0,0,0,0)		
		[HideInInspector] _DebugMode("__debugMode", FLOAT) = 0.0
    }

    HLSLINCLUDE
   	//feature
	#define _ALPHA_BLEND
	#define _VERTEX_COLOR
	#define _OUTPUT_VERTEX_COLOR
	//lighting
	#define _PBS_NO_IBL
	#define _NO_LIGHTMAP
	#define _NO_COLOR_EFFECT
	#define _NO_CLAC_DEPTH
    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }
        Cull Off
        ZWrite Off

        Pass {
            Name "FORWARD"
            Tags {
                "LightMode" = "UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            //----------------------
            #define URP_BASE
            #define REDEFINE_URP
            //----------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            				#include "../Include/Pragam_Head.hlsl"
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#include "../Scene/Scene_Head.hlsl"
				#include "../Include/Common_Glass.hlsl"
				
				#include "../Include/URP_Vertex.hlsl"
				#include "../Include/URP_Pixel.hlsl"

				//#pragma shader_feature_local _ _DEBUG_APP

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
            ENDHLSL
        }
    	
       	Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDCG
		}
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

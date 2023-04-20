/*
 *Made by； Takeshi
 *Date；2021/08/25
 */
Shader "Custom/UI/SemiLuminosity" 
{
    Properties
    {
        [PerRendererData]_MainTex ("Image", 2D) = "white" {}
        _Opacity ("Opacity",range( 0, 1 )) = 1
    	_Exposure ("Exposure", float) = 1
        [HideInInspector]_Color ("Tint", Color) = (1, 1, 1, 1)
        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector]_ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        Pass
        {
			Name "LUMINOSITY"
			Blend One One
			Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM
            
            #pragma target 4.5
            #define _UI_GAMMA

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#define DISCOLOR(color) color.x*0.299 + color.y*0.587 + color.z*0.114
			
			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
            CBUFFER_START( UnityPerMaterial )
                float4 _MainTex_TexelSize;
			    float4 _Color;
				float4 _BGColor;
				float _Opacity;
				float _Exposure;
            CBUFFER_END
			
			struct Attributes
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
			};
			
            struct Varyings
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
				half4  color : COLOR;
			};  

            
			
            Varyings vert(Attributes IN)

			{
				Varyings o;
            	
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz); 
                o.vertex = vertexInput.positionCS;

				o.uv = IN.uv;
				o.color = IN.color * _Color;

            	float4 clipPos = TransformObjectToHClip((IN.vertex).xyz);
				float4 screenPos = ComputeScreenPos(clipPos);
				o.screenPos = screenPos;

				return o;
			}

			
            half4 frag(Varyings IN) : SV_Target
            {


            	/*Additive mode*/
            	float4 image = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,IN.uv);
            	float value = DISCOLOR(image);
            	float imValue = pow(abs(value * (value + _Exposure)),1.5);
            	float4 color = imValue*_Opacity*image.w*0.25*IN.color*IN.color.a;
                return color;
            }
            ENDHLSL
        }
    }
}
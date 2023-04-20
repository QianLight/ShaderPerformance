Shader "Custom/UI/SceneBlurForAVG" 
{
    Properties
    {
        [PerRendererData]_MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0, 0, 0, 1)
//        _StencilComp ("Stencil Comparison", Float) = 8
//        _Stencil ("Stencil ID", Float) = 0
//        _StencilOp ("Stencil Operation", Float) = 0
//        _StencilWriteMask ("Stencil Write Mask", Float) = 255
//        _StencilReadMask ("Stencil Read Mask", Float) = 255
//
//        _ColorMask ("Color Mask", Float) = 15

//        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

	
    SubShader
    {

    	Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        LOD 100

       
        
//		Stencil
//        {
//            Ref [_Stencil]
//            Comp [_StencilComp]
//            Pass [_StencilOp] 
//            ReadMask [_StencilReadMask]
//            WriteMask [_StencilWriteMask]
//        }

        Cull Off
        Lighting Off
        ZWrite on
        
        
//        ColorMask [_ColorMask]

        Pass
        {
			Name "StaticSceneBlurUI"
        	Blend One OneMinusSrcAlpha
            //Blend SrcAlpha OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            
            #pragma target 4.5
            // #define UI_PIXEL_DEPEND_OFF
            // #define UI_PIXEL_COLOR_TO_GREY_SWITCH_OFF
            // #define GAMMA_FIX_OFF
            #pragma vertex vert
            #pragma fragment uiFrag
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma vertex uiVert
            #pragma fragment uiFrag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Engine/Runtime/Shaders/API/CommonAPI.hlsl"
            
            
				TEX2D_SAMPLER(_StaticSceneBlurTexture);
				TEX2D_SAMPLER(_MainTex);
                half4 _TextureSampleAdd;
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            // struct appdata
			struct Attributes
			{
				float4 vertex	: POSITION;
				half4 color		: COLOR;
			};

			// struct v2f
            struct UIInterpolantsVSToPS
			{
				float4 vertex	: SV_POSITION;
				float2 screenUV : TEXCOORD0;
				float4 posCS : TEXCOORD1;
				half4  color	: COLOR;
			};
            
			// v2f vert(appdata IN)
            UIInterpolantsVSToPS vert(Attributes IN)

			{
                // v2f o;
				UIInterpolantsVSToPS o;
                // o.vertex = TransformObjectToClip(IN.vertex);
				// o.vertex = UnityObjectToClipPos(IN.vertex);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz); 
                o.posCS = vertexInput.positionCS;
                o.vertex = vertexInput.positionCS;
				float4 screenPos = vertexInput.positionNDC;
				o.screenUV = screenPos.xy / screenPos.w;
				o.color = _Color ;

				return o;
			}
		      
            
			FLOAT4 uiFrag(UIInterpolantsVSToPS IN) : SV_Target
			{
				float4 screenUV = ComputeScreenPos(IN.posCS);
				float2 uv = screenUV.xy / screenUV.w;
				
			    FLOAT4 color;
            	color = SAMPLE_TEX2D(_StaticSceneBlurTexture, uv);
            	FLOAT4 mainTex = SAMPLE_TEX2D(_MainTex, uv);
            	half opacity = IN.color * mainTex.a;
            	mainTex = FLOAT4(linear_to_sRGB(mainTex),1);
				color = (color )*(1-opacity) + mainTex * opacity;
            	color = FLOAT4(color.rgb,1);
				
				#ifdef UNITY_UI_CLIP_RECT
					color.a *= UIClip(Interpolants.worldPosition.xy, _ClipRect);
				#endif
				
				
				#ifdef UNITY_UI_ALPHACLIP
				    clip (color.a - 0.001);
				#endif
				
				return color;
			}
            ENDHLSL
        }
    }
}
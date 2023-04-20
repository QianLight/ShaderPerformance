Shader "Custom/UI/TextOutlineV2" 
{
    Properties
    {
        [PerRendererData]_MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Int) = 1
		[HideInInspector]_Opacity("Opacity",int)=1
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

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
			Name "OUTLINE"
        	Blend One OneMinusSrcAlpha
            //Blend SrcAlpha OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #define UNITY_UI_CLIP_RECT
            #pragma target 4.5
            #define UI_PIXEL_DEPEND_OFF
            /* 根据Outline特性重写 ColorToGrey ，重新安排位置*/
            #define UI_PIXEL_COLOR_TO_GREY_SWITCH_OFF
            #pragma vertex vert
            #pragma fragment uiFrag
            //#pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            // #pragma vertex uiVert
            // #pragma fragment uiFrag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Engine/Runtime/Shaders/API/CommonAPI.hlsl"
            
            
				TEX2D_SAMPLER(_MainTex);
                half4 _TextureSampleAdd;
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float4 _OutlineColor;
                half4 _Color;
				float _Opacity;
                int _OutlineWidth;
            CBUFFER_END
				float4 _ClipRect;
				float _FADEIN_X = 0.0f;
				float _FADEIN_Y = 0.0f;
            // struct appdata
			struct Attributes
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
				half4 color : COLOR;
			};

			// struct v2f
            struct UIInterpolantsVSToPS
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
                float2 uvOriginXY : TEXCOORD1;
                float2 uvOriginZW : TEXCOORD2;
            	float4 worldPosition : TEXCOORD3;
				half4  color : COLOR;
			};  

            

			// v2f vert(appdata IN)
            UIInterpolantsVSToPS vert(Attributes IN)

			{
                // v2f o;
				UIInterpolantsVSToPS o;
                // o.vertex = TransformObjectToClip(IN.vertex);
				// o.vertex = UnityObjectToClipPos(IN.vertex);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz); 
                o.vertex = vertexInput.positionCS;
				o.worldPosition = IN.vertex;
				o.texcoord = IN.texcoord;
                o.uvOriginXY = IN.texcoord1;
                o.uvOriginZW = IN.texcoord2;
				o.color = IN.color * _Color ;

				return o;
			}
            
			inline FLOAT UIClip(in FLOAT2 position, in FLOAT4 clipRect)
			{
				/*FLOAT2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
				return inside.x * inside.y;*/
				FLOAT halfWidth = (clipRect.w - clipRect.y) * 0.5;
				FLOAT mid = (clipRect.w + clipRect.y) * 0.5;
				FLOAT yy = saturate((halfWidth - abs(mid - position.y)) / (_FADEIN_Y + 0.1f));

				halfWidth = (clipRect.z - clipRect.x) * 0.5;
				mid = (clipRect.z + clipRect.x) * 0.5;
				FLOAT xx = saturate((halfWidth - abs(mid - position.x))/ (_FADEIN_X + 0.1f));
				return (length(clipRect.xy-clipRect.zw) < 1e-10 || isinf(clipRect.x) || isinf(clipRect.y) || isinf(clipRect.z) || isinf(clipRect.w) ) ? 1.0 : yy * xx + step(20000, halfWidth);
			}
            
            // 剔除多余像素
			half IsInRect(float2 pPos, float2 pClipRectXY, float2 pClipRectZW)
			{
				pPos = step(pClipRectXY, pPos) * step(pPos, pClipRectZW);
				return pPos.x * pPos.y;
			}

            half SampleAlpha(int pIndex, UIInterpolantsVSToPS IN)
            {
                const half sinArray[12] = { 0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5 };
                const half cosArray[12] = { 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866 };
                float2 pos = IN.texcoord + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * _OutlineWidth;
				return IsInRect(pos, IN.uvOriginXY, IN.uvOriginZW) * (SAMPLE_TEX2D(_MainTex, pos) + _TextureSampleAdd).w * _OutlineColor.w;
            }

            /* 根据Outline特性重写 ColorToGrey ，重新安排位置*/
			void ColorToGreySwitch(inout FLOAT4 color,FLOAT4 vertexColor)
            {
				FLOAT isToGrey = 1 - sign(vertexColor.r + vertexColor.g + vertexColor.b);			
				vertexColor.b = vertexColor.b < 6.0 /255 && vertexColor.r == 0 && vertexColor.g == 0? 0 : vertexColor.b;			
				color.rgb = lerp(color.rgb * FLOAT3(vertexColor.rgb), FLOAT3(0.1,0.1,0.1), isToGrey);
            }
            void ColorToGreySwitch(inout FLOAT4 color,FLOAT4 vertexColor, inout FLOAT4 outlineColor)
            {
				FLOAT isToGrey = 1 - sign(vertexColor.r + vertexColor.g + vertexColor.b);			
				vertexColor.b = vertexColor.b < 6.0 /255 && vertexColor.r == 0 && vertexColor.g == 0? 0 : vertexColor.b;		
				color.rgb = lerp(color.rgb * FLOAT3(vertexColor.rgb), FLOAT3(0.1,0.1,0.1), isToGrey);
            	outlineColor.rgb = lerp(outlineColor.rgb, FLOAT3(1,1,1), isToGrey);
            }
            
            
            #define INITIALIZE_COLOR(Interpolants,color) InitializeColor(Interpolants,color)
			inline void InitializeColor(UIInterpolantsVSToPS IN, out FLOAT4 color)
			{
				color = half4((SAMPLE_TEX2D(_MainTex, IN.texcoord) + _TextureSampleAdd)/* * IN.color */);
            
                UNITY_BRANCH if (_OutlineWidth * _Opacity > 0) 
                {
                    color.w *= IsInRect(IN.texcoord, IN.uvOriginXY, IN.uvOriginZW);
					
                    // color.w *= IN.color.a;//
            
                    half4 val = half4(_OutlineColor.x, _OutlineColor.y, _OutlineColor.z,0);
            
                    val.w += SampleAlpha(0, IN);
                    val.w += SampleAlpha(1, IN);
                    val.w += SampleAlpha(2, IN);
                    val.w += SampleAlpha(3, IN);
                    val.w += SampleAlpha(4, IN);
                    val.w += SampleAlpha(5, IN);
                    val.w += SampleAlpha(6, IN);
                    val.w += SampleAlpha(7, IN);
                    val.w += SampleAlpha(8, IN);
                    val.w += SampleAlpha(9, IN);
                    val.w += SampleAlpha(10, IN);
                    val.w += SampleAlpha(11, IN);
            
                    // val.w = clamp(val.w, 0, 1); 
                    val.w = saturate(val.w) ;//
                	
                	ColorToGreySwitch(color,IN.color,val);
                	
                    // half4 colorO = (val * (1.0 - color.a)) + (color * color.a);
                    //color = half4(lerp(val.xyz,IN.color.rgb/*_Color.rgb*/,color.w),val.w * IN.color.a);
                	
                	color.rgb = half3(lerp(val.xyz,color.rgb/*_Color.rgb*/,color.w));
                	color.a = lerp(val.w * _Opacity, 1, color.a);
                }
                else
                {
                    color = (SAMPLE_TEX2D(_MainTex, IN.texcoord) + _TextureSampleAdd) /* * IN.color*/ ;
                	ColorToGreySwitch(color,IN.color);
                }
			}
            
			#include "UIPixel.hlsl"
            
            // half4 frag(UIInterpolantsVSToPS IN) : SV_Target
            // {
            //
            //     // UNITY_SETUP_INSTANCE_ID(IN);
            //     // UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
            //
            //     // half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
            //     half4 color = half4((tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * (IN.color.rgb,1));
            //
            //     if (_OutlineWidth > 0) 
            //     {
            //         color.w *= IsInRect(IN.texcoord, IN.uvOriginXY, IN.uvOriginZW);
            //
            //         // color.w *= IN.color.a;//
            //
            //         half4 val = half4(_OutlineColor.x, _OutlineColor.y, _OutlineColor.z,0);
            //
            //         val.w += SampleAlpha(0, IN);
            //         val.w += SampleAlpha(1, IN);
            //         val.w += SampleAlpha(2, IN);
            //         val.w += SampleAlpha(3, IN);
            //         val.w += SampleAlpha(4, IN);
            //         val.w += SampleAlpha(5, IN);
            //         val.w += SampleAlpha(6, IN);
            //         val.w += SampleAlpha(7, IN);
            //         val.w += SampleAlpha(8, IN);
            //         val.w += SampleAlpha(9, IN);
            //         val.w += SampleAlpha(10, IN);
            //         val.w += SampleAlpha(11, IN);
            //
            //         // val.w = clamp(val.w, 0, 1); 
            //         val.w = saturate(val.w) ;//
            //                             
            //         // half4 colorO = (val * (1.0 - color.a)) + (color * color.a);
            //         color = half4(lerp(val.xyz,color,color.w),val.w*IN.color.a);
            //     }
            //     else
            //     {
            //         color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
            //     }
            //     return color;
            // }
            ENDHLSL
        }
    }
}
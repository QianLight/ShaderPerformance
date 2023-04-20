Shader "URP/SFX/UVEffect_water" {
    Properties {
        _MainColor ("Main Color", Color) = (0.5,0.5,0.5,1)
        _MainTextureRGB ("Main Texture(RGB)", 2D) = "white" {}
        _DistortionTextureX ("Distortion TextureX", 2D) = "white" {}
        _PowerX ("PowerX", Range(0, 1)) = 0.3543534
        _DistortionTextureY ("Distortion TextureY", 2D) = "white" {}
        _PowerY ("PowerY", Range(0, 1)) = 0.3543534
        _Outside ("Outside", Range(0, 10)) = 1
        _Inside ("Inside", Range(0, 10)) = 0.2050667
        _Texture ("Texture", 2D) = "white" {}
        _MaskR ("Mask(R)", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="UniversalForward"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // #define UNITY_PASS_FORWARDBASE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // #pragma multi_compile_fwdbase_fullshadows
            // #pragma multi_compile_fog
            // #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal xboxone ps4 psp2 n3ds wiiu
            #pragma target 3.0
            sampler2D _MainTextureRGB;
            sampler2D _DistortionTextureX;
            sampler2D _DistortionTextureY;
            sampler2D _Texture;
            sampler2D _MaskR;
            
            CBUFFER_START(UnityPerMaterial)
            half4 _MainTextureRGB_ST;
            half4 _DistortionTextureX_ST;
            half _PowerX;
            half4 _DistortionTextureY_ST;
            half _PowerY;
            half4 _MainColor;
            half _Outside;
            half _Inside;
            half4 _Texture_ST;
            half4 _MaskR_ST;
            CBUFFER_END
            struct VertexInput {
                half4 vertex : POSITION;
                half2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                half4 pos : SV_POSITION;
                half2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = TransformObjectToHClip( v.vertex );
                return o;
            }
            half4 frag(VertexOutput i) : COLOR {
                half4 node_8166 = _Time;
                half2 node_5558 = i.uv0+node_8166.g*half2(0,0.6);
                half4 _DistortionTextureY_var = tex2D(_DistortionTextureY,TRANSFORM_TEX(node_5558, _DistortionTextureY));
                half2 node_677 = i.uv0+node_8166.g*half2(0,0.7);
                half4 _DistortionTextureX_var = tex2D(_DistortionTextureX,TRANSFORM_TEX(node_677, _DistortionTextureX));
                half2 node_8278 = _DistortionTextureY_var.r*_PowerY+_DistortionTextureX_var.r*_PowerX+i.uv0+node_8166.g*half2(0,0.3);
                half4 _MainTextureRGB_var = tex2D(_MainTextureRGB,TRANSFORM_TEX(node_8278, _MainTextureRGB));
                half node_8241 = _Outside*_MainTextureRGB_var.g+_Inside*_MainTextureRGB_var.r;
                half2 node_1409 = i.uv0+node_8166.g*half2(0,0.3);
                half4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(node_1409, _Texture));
                half3 emissive = _MainColor.rgb*node_8241*_Texture_var.b;
                half3 finalColor = emissive;
                half4 _MaskR_var = tex2D(_MaskR,TRANSFORM_TEX(i.uv0, _MaskR));
                half4 finalRGBA = half4(finalColor,node_8241*_MaskR_var.r);
                return finalRGBA;
            }
            ENDHLSL
        }
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBaseT"}

			Blend One One
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				half4 vertex : POSITION;
			};
			
			struct Varyings
			{
				half4 vertex : SV_POSITION;
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

			ENDHLSL
		}
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}


Shader "URP/SFX/Flag" {
    Properties {
        _MianColor ("TintColor", Color) = (1.0,1.0,1.0,1.0)
        _MainTex ("FirstTexture", 2D) = "white" {}   
        _WaveInt("WaveInt",Range(0,3)) =0.1   
        _WaveSpeed("WaveSpeed",Range(-10,10)) =1
        _WaveSin("WaveSin",Range(1,8)) =2
        _WaveMove("WaveMove",Range(-10,10)) =0 
        _Fixed("Fixed",Range(0,1)) =0 
        [Toggle]_ZWrite("ZWrite",Float) = 0
    }

    SubShader {
        Tags 
        { 
            "Queue"="Transparent"  
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
       
        Cull Off  
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite [_ZWrite]
        
        Pass {
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _MainTex;
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _MianColor;
            half _WaveInt;
            half _WaveSpeed;
            half _WaveMove;
            half _Fixed,_WaveSin;
            CBUFFER_END
            struct appdata_t {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
               
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };           

            v2f vert (appdata_t v)
            {
                v2f o;
                o.uv.x = smoothstep(_Fixed,1,v.texcoord.x);          
                v.vertex.z += _WaveInt * sin((v.texcoord.x*_WaveSin) + _Time.z*_WaveSpeed) * o.uv.x;
                v.vertex.y += _WaveMove *o.uv.x ;
                o.vertex = TransformObjectToHClip(v.vertex);                   
                o.uv = v.texcoord.xy;
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv*_MainTex_ST.xy+_MainTex_ST.zw)*_MianColor;
                return col;
            }
            ENDHLSL
        }
//		Pass
//		{
//			Name "OverdrawF"
//			Tags{"LightMode" = "OverdrawForwardBaseT"}
//
//			Blend One One
//			HLSLPROGRAM
//
//			#pragma vertex Vert
//			#pragma fragment Frag
//
//			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
//
//			struct Attributes
//			{
//				float4 vertex : POSITION;
//			};
//			
//			struct Varyings
//			{
//				float4 vertex : SV_POSITION;
//			};
//			Varyings Vert(Attributes v)
//			{
//				Varyings o;
//				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
//				o.vertex = mul(unity_MatrixVP, WorldPosition);
//				return o;
//			}
//
//			half4 Frag(Varyings i) : SV_Target
//			{
//				return half4(0.1, 0.04, 0.02, 1);
//			}
//
//			ENDHLSL
//		}
    }
   
}
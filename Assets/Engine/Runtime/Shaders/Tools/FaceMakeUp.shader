﻿Shader "Hidden/Custom/Tools/FaceMakeUp"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white"{}
		//[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest Mode", Float) = 2
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			#define DEFINE_PART(id) sampler2D _Part##id##_Tex; \
			float4 _Part##id##_Tex_TexelSize; \
			float2 _Part##id##_Offset; \
			float3 _Part##id##_RotScale; float3 _Part##id##_HSB;


			#define BEGIN_PART_CAL(id,c,chsv)	\
				pos = startPos - _Part##id##_Offset;\
				Rot2d(_Part##id##_RotScale,pos);\
				pos = pos * _Part##id##_Tex_TexelSize.xy + 0.5;\
				if(pos.x >= 0.0 && pos.x <= 1.0 && pos.y >= 0.0 && pos.y <= 1.0){ \
					c = tex2D(_Part##id##_Tex, pos);\
					chsv = _Part##id##_HSB;

			#define END_PART_CAL(id) }

			sampler2D _MainTex;
			float4    _MainTex_TexelSize;
			
			DEFINE_PART(1)
			DEFINE_PART(2)
			DEFINE_PART(3)
			DEFINE_PART(4)
			DEFINE_PART(5)
			
			float3 LayerOverlay(float3 A, float3 B)
			{
				float3 C = (B < 0.5) ? (2 * A * B) : (1 - 2 * (1 - A) * (1 - B));

				return C;
			}
		
			float3 LayerLinearLight(float3 A, float3 B)
			{
				return saturate((B + 2 * A) - 1);
			}

			#define EPSILON         1.0e-4
			#define PI              3.1415926
			#define Deg2Rad         (PI / 180.0f)
			#define Rad2Deg         (180.0f / PI)

			float3 RgbToHsv(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = EPSILON;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 HsvToRgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
			}

			void Rot2d(float3 rotscale,inout float2 pos)
			{
				float2 sc;
				sincos(Deg2Rad*rotscale.x,sc.x,sc.y);

				float2x2 mat = float2x2(float2(sc.y,-sc.x) * rcp(rotscale.y),sc * rcp(rotscale.z));

				pos = mul(mat,pos);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 startPos = i.uv * _MainTex_TexelSize.zw;
				
				float4 col = tex2D(_MainTex, i.uv);
				col.rgb = pow(col.rgb,0.454545);
				float2 pos;
				float4 c;
				float3 chsv;

				BEGIN_PART_CAL(1,c,chsv)
					c.a = pow(c.a,4);
					float3 hsv = RgbToHsv(c.rgb);
					hsv.x += chsv.x + 0.5;
					hsv.x = frac(hsv.x);

					float y = chsv.y * 2 - 1;
					hsv.y *= y < 0 ? lerp(1,0,-y)
					: lerp(1,3,y);
					
					col.rgb = LayerLinearLight(lerp(0.5,HsvToRgb(hsv),chsv.z*2),col.rgb);
				END_PART_CAL(1)

				BEGIN_PART_CAL(2,c,chsv)
					c.a = pow(c.a,4);
					float3 hsv = RgbToHsv(c.rgb);
					hsv.x += chsv.x + 0.5;
					hsv.x = frac(hsv.x);
					float y = chsv.y * 2 - 1;
					hsv.y *= y < 0 ? lerp(1,0,-y)
					: lerp(1,3,y);	
					col.rgb = LayerLinearLight(lerp(0.5,HsvToRgb(hsv),chsv.z*2),col.rgb);
					//col.rgb =LayerLinearLight(c.rgb,col.rgb);
				END_PART_CAL(2)

				BEGIN_PART_CAL(3,c,chsv)
					c.a = pow(c.a,4);
					float3 hsv = RgbToHsv(c.rgb);
					hsv.x = chsv.x+0.5;
					hsv.x = frac(hsv.x);
					hsv.y = smoothstep(0.05,0.9,chsv.z) * saturate(hsv.y + hsv.y*(chsv.y*2-1) )* c.a * 2;
					hsv.z = saturate(hsv.z - (chsv.z-dot(col,0.333))* c.a*0.2*col.g); 
					//hsv.z= saturate(hsv.z - chsv.z* c.a);
					col.rgb = LayerLinearLight(HsvToRgb(hsv),col.rgb);
				END_PART_CAL(3)

				BEGIN_PART_CAL(4,c,chsv)
					c.a = pow(c.a,4);
					float3 hsv = RgbToHsv(c.rgb);
					hsv.x += chsv.x + 0.5;
					hsv.x = frac(hsv.x);

					float y = chsv.y * 2 - 1;
					hsv.y *= y < 0 ? lerp(1,0,-y)
					: lerp(1,3,y);
					
					col.rgb = LayerLinearLight(lerp(0.5,HsvToRgb(hsv),chsv.z*2),col.rgb);
				END_PART_CAL(4)

				BEGIN_PART_CAL(5,c,chsv)
					c.a = pow(c.a,4);
					float3 hsv = RgbToHsv(c.rgb);
					hsv.x += chsv.x + 0.5;
					hsv.x = frac(hsv.x);

					float y = chsv.y * 2 - 1;
					hsv.y *= y < 0 ? lerp(1,0,-y)
					: lerp(1,3,y);
					
					col.rgb = LayerLinearLight(lerp(0.5,HsvToRgb(hsv),chsv.z*2),col.rgb);
				END_PART_CAL(5)
				col.rgb = pow(col.rgb,2.2);
				return col;
			}
			ENDCG
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
}

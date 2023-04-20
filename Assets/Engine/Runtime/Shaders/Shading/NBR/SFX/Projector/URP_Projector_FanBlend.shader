Shader "URP/SFX/FanBlend" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_OutlineColor ("Outline Color", Color) = (1,1,1,1)
		_Arg("x:color transition y:color scale z:outline width w: outline scale",Vector) = (1.1,1,0.1,1)
		_Angle("Angle Control ",range(-1,1)) = 0
	}
	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off ZWrite Off Fog{ Mode Off }
	
		SubShader 
		{
			Pass 
			{
				Tags{ "LightMode" = "UniversalForward" }
				Blend SrcAlpha OneMinusSrcAlpha
			
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				struct appdata_t 
				{
					half4 vertex : POSITION;
					half4 color : COLOR;
					half2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					half4 vertex : SV_POSITION;
					half4 color : COLOR;
					half2 uv : TEXCOORD0;
				};

				CBUFFER_START(UnityPerMaterial)
				half4 _TintColor;
				half4 _OutlineColor;
				half4 _Arg;
				half _Angle;
				CBUFFER_END

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = TransformObjectToHClip(v.vertex);
					o.color = v.color;
					o.uv = v.texcoord-0.5;
					return o;
				}

				half4 frag (v2f i) : SV_Target
				{				
					_Arg=max(0,_Arg);
					//half circle = saturate((0.25 - ( i.uv.x* i.uv.x+ i.uv.y* i.uv.y))*4);
					half circle = saturate(1 - 4 * i.uv.x * i.uv.x - 4 * i.uv.y * i.uv.y);

					//half mask = saturate(sign(circle*(normalize(i.uv).y - _Angle))) ;
					half mask = (circle * (normalize(i.uv).y - _Angle)) > 0 ? 1 : 0;
					
					half4 c=float4(0,0,0,1);
					half2 uv =floor(i.uv+1) ;
					//c=(_Arg.z-saturate((normalize(i.uv).y - _Angle)*5))*_Arg.w;
					c = _Arg.z * _Arg.w - _Arg.w * saturate(5 * normalize(i.uv).y - 5 * _Angle);
					c=saturate(c) *mask;

					half cull = saturate(_Arg.z-circle)*_Arg.w;
					half4 color = _TintColor;

					half dis = _Arg.y*(max(_Arg.x- circle,c.a));

					//float4 col=(color+ max(cull,c )*_OutlineColor)*mask;
					half4 col = mask * color + mask * max(cull, c) * _OutlineColor;
					col.a=saturate(col.a*dis);

					return col;
				}
				ENDHLSL
			}
			Pass
			{
				Name "OverdrawZ"
				Tags{"LightMode" = "OverdrawPreZ" "RenderType"="Opaque" "PerformanceChecks" = "False"}

				Blend One One
				HLSLPROGRAM

				#pragma only_renderers d3d11
				// #pragma exclude_renderers d3d11_9x/*gles gles3 glcore vulkan metal*/
				#pragma vertex Vert
				#pragma fragment Frag

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
				
				ENDHLSL
			}
		}	
	}
}
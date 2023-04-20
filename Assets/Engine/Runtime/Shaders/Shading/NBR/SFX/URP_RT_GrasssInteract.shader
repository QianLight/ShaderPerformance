Shader "URP/SFX/GrassInteract"
{
	Properties
	{
		
	}
	SubShader
	{
	
		Cull Off ZWrite on 
		Pass
		{
			//CGPROGRAM
			 Tags  { "RenderType"="Opaque" "LightMode" = "UniversalForward" }
			     HLSLPROGRAM

			 #pragma target 3.0
	         #include "../StdLib.hlsl"	 
	         #include "../Include/Common.hlsl"
			#pragma target 3.0	
			#pragma vertex vert
			#pragma fragment frag
		

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

			sampler2D _CollisionRT;

			float3 offset;
			float radius;
		    float deltaTime;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			   inline float IsInner(half2 uv)
			{
				return all(step(float4(0.01, 0.01, uv.xy), float4(uv.xy, 0.99, 0.99)));
			}

			float4 frag(v2f i) : SV_Target
			{
			    float radiuss=radius;
				float2 UVOffset = i.uv-(float2(offset.x,offset.z)+float2(0.5, 0.5));		    
				float2 uvoffset1= (normalize(UVOffset) + 1) / 2;
				float coordinateLength = length(UVOffset);
				float  lengthvalue=clamp(coordinateLength/radiuss, 0, 1);
				float impact = 1-lengthvalue*lengthvalue*lengthvalue*lengthvalue;			
				float4 collision = tex2D(_CollisionRT, i.uv);
				float4 result = 0;
				float localarea=step(coordinateLength,radiuss);
	    		//	result.z = max(collision.z, stable);
				if (impact>collision.z) {
					result.z = impact;
				}
				else {
		      		result.z = clamp(collision.z - 0.02,0,1);
				}
				result.z=result.z*IsInner(i.uv);
				result.xy =uvoffset1*localarea+float2(collision.x,collision.y)*(1-localarea);
				//float curve=cos(_TimeParameters.x*10)*0.03;
			//	result.xy =result.xy+curve;
				return float4(result.xyz,1);
				}
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			HLSLPROGRAM

			#pragma only_renderers d3d11
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

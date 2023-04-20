
Shader "Hidden/CreatRampTex"
{
	Properties
	{
		_MainTex ("Base Tex", 2D) = "white" {}
		// _Color("Main Color", Color) = (1,1,1,1)
	}
	
	SubShader
	{
		
		
		// Tags { "RenderType"="Opaque" }
		// LOD 100

		// CGINCLUDE
		// #pragma target 5.0
		// ENDCG
		// Blend Off
		// Cull Back 
		// ColorMask RGBA
		// ZWrite On
		// ZTest LEqual
		// Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			
			CGPROGRAM

// #ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
// 		//only defining to not throw compilation error over Unity 5.5
// 		#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
// #endif
			#pragma vertex vert
			#pragma fragment frag
			// #pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				// float4 color : COLOR;
				 float4 uv:TEXCOORD0;
				
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				
			};
			uniform sampler2D _MainTex;
			uniform float4 _Color;

			uniform int _gradientColorsLength;
			uniform float4 _gradientColors[32];
			uniform float _gradientColorTimes[32];
			
			v2f vert ( appdata v )
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f input ) : SV_Target
			{
				float4 finalColor=_gradientColors[0];
				float colorTime=_gradientColorTimes[0];
				float UVX=input.uv.x;

				for(int i=0;i<_gradientColorsLength;i++)
				{
					//float alpha =lerp(0,1,smoothstep( _gradientColorTimes[max(0,i-1) ],_gradientColorTimes[i],UVX));
					float alpha =saturate((UVX-_gradientColorTimes[max(0,i-1)])/(_gradientColorTimes[i]-_gradientColorTimes[max(0,i-1)]));
					finalColor=lerp(finalColor,_gradientColors[i],pow(alpha,1));
				}

				return pow(finalColor,2.2);
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// #pragma multi_compile_instancing
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
				// float4 color : COLOR;
				 float4 uv:TEXCOORD0;
				
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
				
			};
			uniform sampler2D _MainTex;
			
			v2f vert ( appdata v )
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f input ) : SV_Target
			{
				fixed4 col = tex2D( _MainTex, input.uv);
				return pow(col,2.2);
			}
			ENDCG
		}

		
	}
	
}

Shader "URP/SFX/UVEffect_outline"
{
	Properties
	{
		_Width("Width", Range( 0 , 0.1)) = 0.01
		[HDR]_MainColor("Main Color", Color) = (0,0,0,1)
		_DitherTransparency("_DitherTransparency", Range(0,1)) = 1
	}

	SubShader
	{
	
		Pass{
		
		Tags
		{ 
		    "RenderType" = "Opaque"  
		    "Queue" = "Geometry+0" 
			"LightMode" = "UniversalForward"
		}
		Cull Front
		HLSLPROGRAM
		#pragma target 3.0
		#pragma vertex Vert
		#pragma fragment Frag
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		#pragma multi_compile_local _ _DITHER_TRANSPARENCY

		CBUFFER_START(UnityPerMaterial)
		half _Width;
		half4 _MainColor;
		float _DitherTransparency;
		CBUFFER_END
		struct Attributes
		{
			float4 vertex : POSITION;
			float4 normal : NORMAL;
		};
		
		struct Varyings
		{
			float4 vertex : SV_POSITION;
		};
		
		Varyings Vert(Attributes v)
		{
			Varyings o;
			o.vertex = TransformObjectToHClip(v.vertex + v.normal * _Width);
			// float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
			// o.vertex = mul(unity_MatrixVP, WorldPosition);
			return o;
		}

		half4 Frag(Varyings i) : SV_Target
		{
			DitherTransparent(i.vertex.xy, _DitherTransparency);
			return _MainColor;
		}
		
		/*void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _Width;
			v.vertex.xyz += ( v.normal * _Width );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _MainColor.rgb;
		}
		
		
		struct Input
		{
			half filler;
		};

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Alpha = 1;
		}*/

		ENDHLSL
		}
	}
	Fallback "Hidden/InternalErrorShader"
}
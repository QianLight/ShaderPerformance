Shader "URP/SFX/UVEffect_fire"
{
	Properties
	{
//		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
//		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Enum(Custom Data,0,Material,1)]_ShaderMode("Shader Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
		[Enum(On,1,Off,0)]_DepthMode("Depth Mode", Float) = 0
		[Toggle]_UseTurbulence("Use Turbulence", Range( 0 , 1)) = 0
		[Toggle]_UseClip("Use Clip", Range( 0 , 1)) = 0
		[Toggle]_UseMask("UseMask", Range( 0 , 1)) = 0
		_CUTOUT("CUTOUT", Range( 0 , 1)) = 0.5
		_Brightness("Brightness", Float) = 1
		_Contrast("Contrast", Float) = 1
		[HDR]_main("Main Color", Color) = (1,1,1,1)
		[Toggle]_UseBackColor("Use BackColor", Range( 0 , 1)) = 0
		[HDR]_BackColor("BackColor", Color) = (1,1,1,1)
		_MainTex("Main Tex", 2D) = "white" {}
		[Toggle]_AlphaR("Alpha R", Range( 0 , 1)) = 0
		_MainPannerX("Main Panner X", Float) = 0
		_MainPannerY("Main Panner Y", Float) = 0
		_TurbulenceTex("Turbulence Tex", 2D) = "white" {}
		_DistortPower("Distort Power", Float) = 0
		_PowerU("Power U", Float) = 0
		_PowerV("Power V", Float) = 0
		_TurbulenceRampTex("Turbulence Ramp Tex", 2D) = "white" {}
		[Toggle]_UseTurbulenceRamp("Use Turbulence Ramp", Range( 0 , 1)) = 0
		[Toggle]_FlipDissolve("Flip Dissolve", Range( 0 , 1)) = 0
		_MaskTex("Mask Tex", 2D) = "white" {}
		_Hardness("Hardness", Range( 0 , 0.99)) = 0
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		[HDR]_WidthColor("WidthColor", Color) = (1,1,1,1)
		_EdgeWidth("EdgeWidth", Range( 0 , 1)) = 0
//		[ASEEnd]_Alpha("Alpha", Range( 0 , 10)) = 1
	}

	SubShader
	{
		LOD 0

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull [_CullMode]
		AlphaToMask Off
		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha [_BlendMode], One Zero
			ZWrite [_DepthMode]
			ZTest [_ZTest]
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma shader_feature _ _USETURBULENCERAMP_ON
			#pragma shader_feature _ _USEMASK_ON
			#pragma target 4.0
			#pragma multi_compile_instancing
			
			// #define ASE_SRP_VERSION 100500

			// #pragma prefer_hlslcc gles

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 custom1Data : TEXCOORD1;
				float4 custom2Data : TEXCOORD2;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif

				float4 uv : TEXCOORD3;
				float4 custom2Data : TEXCOORD4;
				float4 custom1Data : TEXCOORD5;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _main;
			float4 _TurbulenceRampTex_ST;
			float4 _MainTex_ST;
			float4 _BackColor;
			float4 _WidthColor;
			float4 _TurbulenceTex_ST;
			float4 _MaskTex_ST;
			float _Hardness;
			// float _Alpha;
			float _PowerV;
			float _FlipDissolve;
			// float _UseMask;
			float _UseClip;
			float _Contrast;
			// float _UseTurbulenceRamp;
			float _AlphaR;
			float _BlendMode;
			float _UseTurbulence;
			float _CUTOUT;
			float _DistortPower;
			float _Dissolve;
			float _MainPannerY;
			float _MainPannerX;
			float _Brightness;
			float _UseBackColor;
			float _ShaderMode;
			float _EdgeWidth;
			float _ZTest;
			float _CullMode;
			float _PowerU;
			float _DepthMode;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainTex;
			sampler2D _TurbulenceTex;
			sampler2D _TurbulenceRampTex;
			sampler2D _MaskTex;
					


			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				
				UNITY_BRANCH
				if(_ShaderMode)
				{
					o.custom2Data = float4(1.0 , 1.0 , _EdgeWidth , 1.0);
				}else
				{
					o.custom2Data = float4(v.custom2Data.x , v.custom2Data.y , v.custom2Data.z ,  v.custom2Data.w + 1.0 );
				}
				// o.custom2Data = v.custom2Data;
				o.uv.xy = v.uv.xy;

				UNITY_BRANCH
				if (_ShaderMode)
				{
					o.custom1Data = float4(frac( _MainPannerX * ( _TimeParameters.x ) ) , frac( ( _TimeParameters.x ) * _MainPannerY ) , _Dissolve , _DistortPower);
				}else
				{
					o.custom1Data = float4(v.custom1Data.x , v.custom1Data.y , v.custom1Data.z , v.custom1Data.w);
				}
				// o.custom1Data = v.custom1Data;
				
				o.color = v.color;
				
				o.uv.zw = 0;
				

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.clipPos = positionCS;
				return o;
			}

			float4 frag ( VertexOutput IN , float ase_vface : VFACE ) : SV_Target
			{
				
				// float4 custom2Data = IN.custom2Data;

				float4 custom2 = IN.custom2Data;
				float4 custom1 = IN.custom1Data;
				
				
				float edgebrightness152 = custom2.w;
				float4 backColor = lerp(_main, _BackColor, _UseBackColor > 0.0);
				float4 color = lerp( backColor , _main , max( ase_vface , 0.0 ));
				custom2.xy = custom2.xy - 1;
				float2 uv_MainTex = IN.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

				
				
				float2 uv_TurbulenceTex = IN.uv.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				float2 uv_TurbulenceRampTex = IN.uv.xy * _TurbulenceRampTex_ST.xy + _TurbulenceRampTex_ST.zw;
				
				float turbulenceRamp = 1;
			#ifdef _USETURBULENCERAMP_ON
				// UNITY_BRANCH 
				// if( _UseTurbulenceRamp > 0.0 )
					turbulenceRamp = tex2D( _TurbulenceRampTex, uv_TurbulenceRampTex ).r;
			#endif
				
				float turbulence = ( tex2D( _TurbulenceTex, ( uv_TurbulenceTex + frac( float2(_PowerU , _PowerV) * ( _TimeParameters.x ) ) ) ).r * turbulenceRamp );

				float ifLocalVar211 = 0;
				UNITY_BRANCH 
				if( _UseTurbulence > 0.0 )
				ifLocalVar211 = ( ( turbulence - 0.5 ) * custom1.w );
				
				float2 mainUV =  ( ( ( IN.uv.xy * custom2 ) + ( custom2 * float2( -0.5,-0.5 ) ) ) + ( ( uv_MainTex + custom1.xy ) + ifLocalVar211 ) ) ;
				float4 main = tex2D( _MainTex, mainUV);
				color = ( color * _Brightness * pow( main , _Contrast ) * IN.color );
				
				UNITY_BRANCH 
				if( _FlipDissolve > 0.0 )
					turbulence = ( 1.0 - turbulence );
				
				turbulence = ( turbulence + 1.0 );
				float dissolve = custom1.z;
				float edgewidth150 = custom2.z;
				dissolve = dissolve + dissolve * edgewidth150;
				float rHardness = ( 1.0 - _Hardness );
				
				float2 alphagroup = 1;
				UNITY_BRANCH 
				if( _UseClip >  0.0 )
					alphagroup = float2(saturate((turbulence - dissolve - dissolve * rHardness - _Hardness) * (1.0 / (1.0 - _Hardness))), saturate((turbulence - dissolve + edgewidth150 - dissolve * rHardness + rHardness * edgewidth150 - _Hardness) / (1.0 - _Hardness)));
				
				
				color = lerp( ( edgebrightness152 * _WidthColor * color ) , color , alphagroup.x);
				
				float ifLocalVar206 = 0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
				ifLocalVar206 = main.a;
				else
				ifLocalVar206 = main.r;
				
				float2 uv_MaskTex = IN.uv.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				
				float mask = 1;
			#ifdef _USEMASK_ON
				// UNITY_BRANCH 
				// if( _UseMask > 0.0 )
					mask = tex2D( _MaskTex, uv_MaskTex ).r;
			#endif
				
				float alpha = min( ( IN.color.a * _main.a * ifLocalVar206 * alphagroup.y * mask ) , 1.0 );
				clip( alpha - min( _CUTOUT , _DepthMode ));
				alpha = lerp( alpha , 1 , _DepthMode);
				
				float3 Color = color.rgb;
				float Alpha = alpha;
				
				return float4( Color, Alpha );
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

			float4 Frag(Varyings i) : SV_Target
			{
				return float4(0.1, 0.04, 0.02, 1);
			}
			ENDHLSL
		}	

	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
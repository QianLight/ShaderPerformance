Shader "URP/SFX/UVEffect_AboveWater"
{
	Properties
	{
		[Enum(Custom Data,0,Material,1)]_ShaderMode("Shader Mode", float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", float) = 0
		[Enum(On,1,Off,0)]_DepthMode("Depth Mode", float) = 0
		_CUTOUT("CUTOUT", Range( 0 , 1)) = 0.5
		[Toggle(_USETURBULENCE_ON)] _UseTurbulence("Use Turbulence", float) = 0
		[Toggle(_USEMASK_ON)] _UseMask("Use Mask", float) = 0
		[Toggle(_USECLIP_ON)] _UseClip("Use Clip", float) = 0
		_Brightness("Brightness", float) = 1
		_Contrast("Contrast", float) = 1
		[HDR]_MainColor("Main Color", Color) = (1,1,1,1)
		[HDR]_BackColor("BackColor", Color) = (1,1,1,1)
		[Toggle(_USEBACKCOLOR_ON)] _UseBackColor("Use BackColor", float) = 0
		_MainTex("Main Tex", 2D) = "white" {}
		_MainPannerX("Main Panner X", float) = 0
		_MainPannerY("Main Panner Y", float) = 0
		[Toggle(_ALPHAR_ON)] _AlphaR("Alpha R", float) = 0
		_TurbulenceTex("Turbulence Tex", 2D) = "white" {}
		_DistortPower("Distort Power", float) = 0
		_PowerU("Power U", float) = 0
		_PowerV("Power V", float) = 0
		_MaskTex("Mask Tex", 2D) = "white" {}
		_Hardness("Hardness", Range( 0 , 0.99)) = 0
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		[HDR]_WidthColor("WidthColor", Color) = (1,1,1,1)
		_EdgeWidth("EdgeWidth", Range( 0 , 1)) = 0
		_Alpha("Alpha", Range( 0 , 10)) = 1
		_StencilRef("StencilRef", Int) = 0

        _ParamA("WaveA", Vector) = (0,0,1,1)
		_ParamB("WaveB", Vector) = (0,0,1,1)
		_ParamC("WaveC", Vector) = (0,0,1,1)
		_Wave1("Wave1", Vector) = (0,0,1,1)
		_Wave2("Wave2", Vector) = (0,0,1,1)
		_Wave3("Wave3", Vector) = (0,0,1,1)
		_Wave4("Wave4", Vector) = (0,0,1,1)
		_Wave5("Wave5", Vector) = (0,0,1,1)
		_Wave6("Wave6", Vector) = (0,0,1,1)
		_Wave7("Wave7", Vector) = (0,0,1,1)
		_Wave8("Wave8", Vector) = (0,0,1,1)
		_Wave9("Wave9", Vector) = (0,0,1,1)
        _SteepnessFadeout("SteepnessFadeout", Vector) = (0,0,300,2)	

		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("StencilComp", Int) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Int) = 6
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite [_DepthMode]
		ZTest on
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="UniversalForward" }
			Blend SrcAlpha [_BlendMode]
			HLSLPROGRAM

#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
		//only defining to not throw compilation error over Unity 5.5
		#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#pragma shader_feature _USETURBULENCE_ON
			#pragma shader_feature _USEBACKCOLOR_ON
			#pragma shader_feature _USECLIP_ON
			#pragma shader_feature _ALPHAR_ON
			#pragma shader_feature _USEMASK_ON

            #define TWO_PI          6.28318530718
            #define _matrixVP unity_MatrixVP

			CBUFFER_START(UnityPerMaterial)
            float4 _ParamA;
            float4 _ParamB;
            float4 _ParamC;
            float4 _Wave1;
            float4 _Wave2;
            float4 _Wave3;
            float4 _Wave4;
            float4 _Wave5;
            float4 _Wave6;
            float4 _Wave7;
            float4 _Wave8;
            float4 _Wave9;
            float4 _SteepnessFadeout;

			half _EdgeWidth;
			half _ShaderMode;
			half _BlendMode;
			half _CullMode;
			half _DepthMode;
			half _CUTOUT;
			float _UseTurbulence;
			float _UseMask;
			half _UseClip;
			half _Brightness;
			half _Contrast;
			float4 _MainColor;
			float4 _BackColor;
			float _UseBackColor;
			float4 _MaskTex_ST;
			half _MainPannerX;
			half _MainPannerY;
			float _AlphaR;
			float4 _TurbulenceTex_ST;
			half _DistortPower;
			half _PowerU;
			half _PowerV;
			half _Hardness;
			half _Dissolve;
			float4 _WidthColor;
			half _Alpha;
			int _StencilRef;
			float4 _MainTex_ST;
			CBUFFER_END

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				// UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 uv0 : TEXCOORD0;
				float4 customData2 : TEXCOORD2;
				float4 customData1 : TEXCOORD1;
                

			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				// UNITY_VERTEX_INPUT_INSTANCE_ID
				// UNITY_VERTEX_OUTPUT_STEREO
				float4 uv0 : TEXCOORD0;
				float4 customData1 : TEXCOORD1;
				float4 customData2 : TEXCOORD2;
				float4 color : COLOR;
			};


			
			uniform int _ZTest;
			uniform int _StencilComp;
			uniform sampler2D _MainTex;
			
			
			uniform sampler2D _TurbulenceTex;
			uniform sampler2D _MaskTex;
			float4 _ObstructSphere;

            inline float InRange(float2 uv, float4 ranges)
            {
                float2 value = step(ranges.xy, uv) * step(uv, ranges.zw);
                return value.x * value.y;
            }

            float3 GerstnerWave(
                float4 wave,
                float3 p, 
                float time) 
            {			
                    float steepness = wave.z;
                    steepness *= saturate(1- pow(length(p.xz-_SteepnessFadeout.xy)/_SteepnessFadeout.z,_SteepnessFadeout.w));
                    float wavelength = wave.w;
                    float k = TWO_PI / wavelength;
                    float c = sqrt(9.8 / k);
                    float2 d = normalize(wave.xy);
                    float f = k * (dot(d, p.xz) - c * time);
                    float a = steepness / k;

                    float _sin = 0;
                    float _cos = 0;
                    sincos(f,_sin,_cos);

                    //return _cos;
                    return float3(
                        d.x * ( a* _cos),
                        a * _sin,
                        d.y * (a * _cos)
                    );
            }

            void CustomSFXInterpolantsVSToPS(
                inout float4 WorldPosition)
            {
                float3 gridPoint = WorldPosition;
                //float3 tangent = float3(1, 0, 0);
                //float3 binormal = float3(0, 0, 1);
                float3 p = gridPoint;
                float time = _Time.y;
                p += GerstnerWave(_ParamA, gridPoint, time);
                p += GerstnerWave(_ParamB, gridPoint, time);
                p += GerstnerWave(_ParamC, gridPoint, time);
                p += GerstnerWave(_Wave1, gridPoint, time);
                p += GerstnerWave(_Wave2, gridPoint, time);
                p += GerstnerWave(_Wave3, gridPoint, time);
                p += GerstnerWave(_Wave4, gridPoint, time);
                p += GerstnerWave(_Wave5, gridPoint, time);
                p += GerstnerWave(_Wave6, gridPoint, time);
                p += GerstnerWave(_Wave7, gridPoint, time);
                p += GerstnerWave(_Wave8, gridPoint, time);
                p += GerstnerWave(_Wave9, gridPoint, time);			

                WorldPosition = float4(p,1);
            }

			
			

			 void spherePushOut(inout float4 WorldPos,in float4 Sphere)
            {
				float R=Sphere.w;
				float x0=length(WorldPos.xz-Sphere.xz);
				float y0=WorldPos.y-Sphere.y;
				float x = sqrt(R*R-y0*y0)-x0;
				float2 offset=x*normalize(WorldPos.xz-Sphere.xz);
				WorldPos.xz =length( WorldPos.xyz-Sphere.xyz)>Sphere.w ? WorldPos.xz :WorldPos.xz+offset;
            }


			v2f vert ( appdata v )
			{
				v2f o;
				// UNITY_SETUP_INSTANCE_ID(v);
				// UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				// UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.uv0.xy = v.uv0.xy;
				o.customData2 = v.customData2;
				o.customData1 = v.customData1;
				o.color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.uv0.zw = 0;

                float4 WorldPosition= mul(unity_ObjectToWorld, v.vertex);
				
                CustomSFXInterpolantsVSToPS(WorldPosition);
				spherePushOut(WorldPosition,_ObstructSphere);

                o.vertex = mul(_matrixVP, float4(WorldPosition.xyz, 1.0));
				return o;
			}
			
			half4 frag (v2f i , half ase_vface : VFACE) : SV_Target
			{
				// UNITY_SETUP_INSTANCE_ID(i);
				// UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				half4 finalColor;
				
				float4 appendResult123 = (float4(i.customData2.x , i.customData2.y , i.customData2.z , ( i.customData2.w + 1.0 )));
				float4 appendResult141 = (float4(1.0 , 1.0 , _EdgeWidth , 1.0));
				float4 customData2 = lerp( appendResult123 , appendResult141 , _ShaderMode);
				customData2.xy = customData2.xy - 1;
				
				float2 uv0_MainTex = i.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 appendResult129 = (float4(i.customData1.x , i.customData1.y , i.customData1.z , i.customData1.w));
				float4 appendResult137 = (float4(( _MainPannerX * _Time.y ) , ( _Time.y * _MainPannerY ) , _Dissolve , _DistortPower));
				float4 customData1 = lerp( appendResult129 , appendResult137 , _ShaderMode);
				
				float2 uv0_TurbulenceTex = i.uv0.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				float4 turbulenceColor = tex2D( _TurbulenceTex, ( uv0_TurbulenceTex + ( float2(_PowerU , _PowerV) * _Time.y ) ) );
				half Distort148 = customData1.w;

				float turbulence = 0.0;
				UNITY_BRANCH 
				if( _UseTurbulence > 0.0 )
				turbulence = ( ( turbulenceColor.r - 0.5 ) * Distort148 );

				float4 mainColor = tex2D( _MainTex, ( ( ( i.uv0.xy * customData2.xy ) + ( customData2.xy * float2( -0.5,-0.5 ) ) ) + ( ( uv0_MainTex + customData1.xy ) + turbulence ) ) );

				float4 backColor = _MainColor;
				UNITY_BRANCH 
				if( _UseBackColor > 0.0 )
					backColor = _BackColor;

				float4 modelColor = lerp( backColor , _MainColor , max( ase_vface , 0.0 ));
				modelColor = ( pow( mainColor , _Contrast ) * _Brightness * modelColor * i.color );
				half edgebrightness = customData2.w;
				turbulenceColor.r = ( turbulenceColor.r + 1.0 );
				half dissolve = customData1.z;
				half edgewidth = customData2.z;
				float edgeDissolve = ( dissolve * ( 1.0 + edgewidth ) );
				float rHardness = ( 1.0 - _Hardness );
				float2 temp = float2(edgeDissolve, edgeDissolve - edgewidth ) * ( 1.0 + rHardness );
				float2 clipXY = saturate( ( turbulenceColor.r - temp  - _Hardness ) / rHardness );
				
				UNITY_BRANCH 
				if( _UseClip <= 0.0 )
					clipXY = 1;
				
				float3 color = lerp( ( _WidthColor * modelColor * edgebrightness ) , modelColor , clipXY.x);

				float alpha=0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
					alpha = mainColor.a;
				else
				alpha = mainColor.r;

				float2 uv_MaskTex = i.uv0.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;

				float mask = 1;
				UNITY_BRANCH 
				if( _UseMask > 0.0 )
					mask = tex2D( _MaskTex, uv_MaskTex ).r;

				alpha = min( ( i.color.a * _MainColor.a * alpha * clipXY.y * _Alpha * mask ) , 1.0 );
				clip( alpha - min( _CUTOUT , _DepthMode ));
				alpha = lerp( alpha , 1 , _DepthMode);
				
               // finalColor= i.testColor;
				return float4(color, alpha);
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

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDHLSL
		}
	}
}

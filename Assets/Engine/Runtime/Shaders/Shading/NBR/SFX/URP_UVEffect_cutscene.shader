Shader "URP/SFX/UVEffect_cutscene"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Enum(Custom Data,0,Material,1)]_ShaderMode("Shader Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
		[Enum(On,1,Off,0)]_DepthMode("Depth Mode", Float) = 0
		[Toggle]_UseAddTex("Use AddTex", Range( 0 , 1)) = 0
		[Toggle]_UseTurbulence("Use Turbulence", Range( 0 , 1)) = 0
		[Toggle]_UseClip("Use Clip", Range( 0 , 1)) = 0
		[Toggle]_UseMask("UseMask", Range( 0 , 1)) = 0
		_CUTOUT("CUTOUT", Range( 0 , 1)) = 0.5
		_Brightness("Brightness", Float) = 1
		_Contrast("Contrast", Float) = 1
		[HDR]_MainColor("Main Color", Color) = (1,1,1,1)
		[Toggle]_UseBackColor("Use BackColor", Range( 0 , 1)) = 0
		[HDR]_BackColor("BackColor", Color) = (1,1,1,1)
		_MainTex("Main Tex", 2D) = "white" {}
		[Toggle]_AlphaR("Alpha R", Range( 0 , 1)) = 0
		_MainPannerY("Main Panner Y", Float) = 0
		_TurbulenceTex("Turbulence Tex", 2D) = "white" {}
		_AddTex("AddTex", 2D) = "white" {}
		_AddOffset("AddOffset", Vector) = (1,1,0,0)
		_AddTexMask("AddTexMask", 2D) = "white" {}
		_AddColor("AddColor", Color) = (0,1,0.03857636,0)
		_AddTexInt("AddTexInt", Range( 0 , 3)) = 0
		_AddTexU("AddTexU", Float) = 0
		_AddTexV("AddTexV", Float) = 0
		_DistortPower("Distort Power", Float) = 0
		_PowerU("Power U", Float) = 0
		_PowerV("Power V", Float) = 0
		_MaskTex("Mask Tex", 2D) = "white" {}
		_Hardness("Hardness", Range( 0 , 0.99)) = 0
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		[HDR]_WidthColor("WidthColor", Color) = (1,1,1,1)
		_EdgeWidth("EdgeWidth", Range( 0 , 1)) = 0
		_Alpha("Alpha", Range( 0 , 10)) = 1
		[Toggle]_UseDepthFade("UseDepthFade", Range( 0 , 1)) = 0
		_FadeLength("FadeLength", Range( 0 , 10)) = 0.5
		[Toggle]_Usefresnel("Use fresnel", Range( 0 , 1)) = 0
		_fresnelpower("fresnel power", Range( 0 , 20)) = 1
		_fresnelmultiply("fresnel multiply", Range( 0 , 3)) = 1
		[Toggle]_Flip("Flip", Range( 0 , 1)) = 0
		[Toggle]_UseRamp("UseRamp", Range( 0 , 1)) = 0
		_Color0("Color 0", Color) = (1,0,0,0)
		_Color1("Color 1", Color) = (0,1,0.03857636,0)
		_Color2("Color 2", Color) = (0,0.4486432,1,0)
		_RampParam("RampParam", Vector) = (0.3,0.33,0.6,0.66)
		[Toggle]_CameraFade("CameraFade", Range( 0 , 1)) = 0
		_CameraFadeOffset("CameraFadeOffset", Float) = 0
		[ASEEnd]_CameraFadeLength("CameraFadeLength", Float) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		LOD 0
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull [_CullMode]
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0
		
		ENDHLSL

		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha [_BlendMode]
			ZWrite [_DepthMode]
			ZTest [_ZTest]
			Offset 0 , 0
			ColorMask RGBA
			
			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 100500

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR


			struct VertexInput
			{
				half4 vertex : POSITION;
				half3 ase_normal : NORMAL;
				half4 ase_texcoord2 : TEXCOORD2;
				half4 ase_texcoord : TEXCOORD0;
				half4 ase_texcoord1 : TEXCOORD1;
				half4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				half4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				half4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				half fogFactor : TEXCOORD2;
				#endif
				half4 ase_texcoord3 : TEXCOORD3;
				half4 ase_texcoord4 : TEXCOORD4;
				half4 ase_texcoord5 : TEXCOORD5;
				half4 ase_texcoord6 : TEXCOORD6;
				half4 ase_color : COLOR;
				half4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _AddColor;
			half4 _AddOffset;
			half4 _MainTex_ST;
			half4 _AddTexMask_ST;
			half4 _MainColor;
			half4 _BackColor;
			half4 _WidthColor;
			half4 _Color2;
			half4 _RampParam;
			half4 _TurbulenceTex_ST;
			half4 _Color0;
			half4 _MaskTex_ST;
			half4 _Color1;
			half _PowerV;
			half _UseAddTex;
			half _AddTexU;
			half _AddTexV;
			half _CUTOUT;
			half _UseMask;
			half _AlphaR;
			half _AddTexInt;
			half _Hardness;
			half _PowerU;
			half _Contrast;
			half _UseClip;
			half _Alpha;
			half _BlendMode;
			half _fresnelmultiply;
			half _DistortPower;
			half _CullMode;
			half _ZTest;
			half _CameraFade;
			half _CameraFadeLength;
			half _CameraFadeOffset;
			half _UseRamp;
			half _UseDepthFade;
			half _EdgeWidth;
			half _ShaderMode;
			half _UseBackColor;
			half _Brightness;
			half _MainPannerY;
			half _Dissolve;
			half _fresnelpower;
			half _DepthMode;
			half _Flip;
			half _Usefresnel;
			half _UseTurbulence;
			half _FadeLength;
			CBUFFER_END
			sampler2D _MainTex;
			sampler2D _TurbulenceTex;
			sampler2D _AddTex;
			sampler2D _AddTexMask;
			sampler2D _MaskTex;
			sampler2D _CameraDepthRT;


			half MyCustomExpression261( half2 enc )
			{
				 return dot(enc,half2(1,1/2047.0));
			}
			
			half MyCustomExpression271( half z, half4 zBufferParams )
			{
				z*=zBufferParams.x;
				return 1/(z+zBufferParams.y);
			}
			
			
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				half3 customSurfaceDepth324 = v.vertex.xyz;
				half customEye324 = -TransformWorldToView(TransformObjectToWorld(customSurfaceDepth324)).z;
				o.ase_texcoord3.x = customEye324;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.yzw = ase_worldNormal;
				half4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				half4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord7 = screenPos;
				
				o.ase_texcoord4 = v.ase_texcoord2;
				o.ase_texcoord5.xy = v.ase_texcoord.xy;
				o.ase_texcoord6 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				
				o.ase_texcoord5.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					half3 defaultVertexValue = v.vertex.xyz;
				#else
					half3 defaultVertexValue = half3(0, 0, 0);
				#endif
				half3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				half4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( positionCS.z );
				#endif
				o.clipPos = positionCS;
				return o;
			}

		
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN , half ase_vface : VFACE ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				half customEye324 = IN.ase_texcoord3.x;
				half cameraDepthFade324 = ( customEye324 -_ProjectionParams.y - _CameraFadeOffset ) * (1.0 / _CameraFadeLength);
				half4 appendResult311 = half4(1.0 , 1.0 , 1.0 , saturate( cameraDepthFade324 ));
				half3 ase_worldNormal = IN.ase_texcoord3.yzw;
				half dotResult230 = dot( _MainLightPosition.xyz , ase_worldNormal );
				half temp_output_235_0 = 1.0 - saturate(dotResult230 * 0.5 + 0.5);
				half smoothstepResult238 = smoothstep( _RampParam.x , _RampParam.y , temp_output_235_0);
				half4 lerpResult240 = lerp( _Color0 , _Color1 , smoothstepResult238);
				half smoothstepResult241 = smoothstep( _RampParam.z , _RampParam.w , temp_output_235_0);
				half4 lerpResult242 = lerp( lerpResult240 , _Color2 , smoothstepResult241);
				half4 texCoord61 = IN.ase_texcoord4;
				half4 appendResult123 = half4(texCoord61.x , texCoord61.y , texCoord61.z , texCoord61.w + 1.0);
				half4 appendResult141 = half4(1.0 , 1.0 , _EdgeWidth , 1.0);
				half4 lerpResult143 = lerp( appendResult123 , appendResult141 , _ShaderMode);
				half4 break144 = lerpResult143;
				half edgebrightness152 = break144.w;
				half4 ifLocalVar205 = 0;
				UNITY_BRANCH 
				if( _UseBackColor <= 0.0 )
				ifLocalVar205 = _MainColor;
				else
				ifLocalVar205 = _BackColor;
				half4 lerpResult162 = lerp( ifLocalVar205 , _MainColor , max( ase_vface , 0.0 ));
				half2 texCoord65 = IN.ase_texcoord5.xy;
				half2 appendResult62 = half2(break144.x , break144.y) - 1;
				half2 uv_MainTex = IN.ase_texcoord5.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float3 ase_worldViewDir = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
				half fresnelNdotV192 = dot( ase_worldNormal, ase_worldViewDir );
				half fresnelNode192 = pow( 1.0 - fresnelNdotV192, _fresnelpower );
				half temp_output_196_0 = min( fresnelNode192 , 1.0 ) * _fresnelmultiply;
				half lerpResult221 = lerp( temp_output_196_0 , ( 1.0 - temp_output_196_0 ) , _Flip);
				half lerpResult213 = lerp( 1.0 , lerpResult221 , _Usefresnel);
				half4 appendResult137 = half4(0.0, _TimeParameters.x * _MainPannerY, _Dissolve * max(1.0, 1.0 - lerpResult213), _DistortPower);
				half4 lerpResult125 = lerp(  IN.ase_texcoord6 , appendResult137 , _ShaderMode);
				half4 break128 = lerpResult125;
				half2 appendResult21 = half2(break128.x , break128.y);
				half2 uv_TurbulenceTex = IN.ase_texcoord5.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				half2 appendResult36 = half2(_PowerU, _PowerV);
				half4 tex2DNode31 = tex2D(_TurbulenceTex, uv_TurbulenceTex + appendResult36 * _TimeParameters.x);
				half Distort148 = break128.w;
				half ifLocalVar211 = 0;
				UNITY_BRANCH 
				if( _UseTurbulence <= 0.0 )
				ifLocalVar211 = 0.0;
				else
				ifLocalVar211 = tex2DNode31.r * Distort148 - 0.5 * Distort148;
				half4 tex2DNode3 = tex2D( _MainTex, texCoord65 * appendResult62 + appendResult62 * half2(-0.5,-0.5) + uv_MainTex + appendResult21 + ifLocalVar211);
				half2 appendResult336 = (half2(_AddTexU , _AddTexV));
				half2 uv_AddTexMask = IN.ase_texcoord5.xy * _AddTexMask_ST.xy + _AddTexMask_ST.zw;
				half4 temp_cast_0 = (0.0).xxxx;
				half4 temp_cast_1 = (0.0).xxxx;
				half4 ifLocalVar345 = 0;
				UNITY_BRANCH 
				if( _UseAddTex <= 0.0 )
				ifLocalVar345 = temp_cast_1;
				else
				ifLocalVar345 = ( ( tex2D( _AddTex, ( ( ( uv_TurbulenceTex + ( appendResult336 * ( _TimeParameters.x ) ) ) * (_AddOffset).xy ) + (_AddOffset).zw ) ).r * tex2D( _AddTexMask, uv_AddTexMask ).r ) * _AddTexInt * _AddColor );
				half4 temp_cast_2 = (_Contrast).xxxx;
				half4 temp_output_8_0 = ( lerpResult162 * _Brightness * pow( ( tex2DNode3 + ifLocalVar345 ) , temp_cast_2 ) * IN.ase_color );
				half temp_output_87_0 = ( tex2DNode31.r + 1.0 );
				half dissolve146 = break128.z;
				half edgewidth150 = break144.z;
				half temp_output_116_0 = dissolve146 * 1.0 + dissolve146 * edgewidth150;
				half hardness89 = _Hardness;
				half temp_output_91_0 = ( 1.0 - hardness89 );
				half2 appendResult158 = half2(saturate((temp_output_87_0 - temp_output_116_0 - temp_output_116_0 * temp_output_91_0 - hardness89) * (1.0 / (1.0 - hardness89))) , saturate((temp_output_87_0 - temp_output_116_0 + edgewidth150 - temp_output_116_0 * temp_output_91_0 + temp_output_91_0 * edgewidth150 - hardness89 ) / ( 1.0 - hardness89)));
				half2 temp_cast_3 = (1.0).xx;
				half2 temp_cast_4 = (1.0).xx;
				half2 ifLocalVar219 = 0;
				UNITY_BRANCH 
				if( _UseClip <= 0.0 )
				ifLocalVar219 = temp_cast_4;
				else
				ifLocalVar219 = appendResult158;
				half2 break159 = ifLocalVar219;
				half4 lerpResult109 = lerp( ( edgebrightness152 * _WidthColor * temp_output_8_0 ) , temp_output_8_0 , break159.x);
				half ifLocalVar206 = 0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
				ifLocalVar206 = tex2DNode3.a;
				else
				ifLocalVar206 = tex2DNode3.r;
				half2 uv_MaskTex = IN.ase_texcoord5.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				half ifLocalVar201 = 0;
				UNITY_BRANCH 
				if( _UseMask <= 0.0 )
				ifLocalVar201 = 1.0;
				else
				ifLocalVar201 = tex2D( _MaskTex, uv_MaskTex ).r;
				half temp_output_74_0 = min( ( IN.ase_color.a * _MainColor.a * ifLocalVar206 * break159.y * _Alpha * ifLocalVar201 * lerpResult213 ) , 1.0 );
				half4 appendResult173 = (half4(lerpResult109.rgb , temp_output_74_0));
				clip( temp_output_74_0 - min( _CUTOUT , _DepthMode ));
				half3 appendResult171 = (half3(lerpResult109.rgb));
				half4 appendResult172 = (half4(appendResult171 , 1.0));
				half4 lerpResult170 = lerp( appendResult173 , appendResult172 , _DepthMode);
				half4 appendResult112 = (half4(lerpResult170));
				half4 screenPos = IN.ase_texcoord7;
				half4 ase_screenPosNorm = screenPos * (1.0 / screenPos.w);
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				half2 enc261 = (tex2D( _CameraDepthRT, ase_screenPosNorm.xy )).rg;
				half localMyCustomExpression261 = MyCustomExpression261( enc261 );
				half z271 = localMyCustomExpression261;
				half4 zBufferParams271 = _ZBufferParams;
				half localMyCustomExpression271 = MyCustomExpression271( z271 , zBufferParams271 );
				half depthToLinear300 = Linear01Depth(ase_screenPosNorm.z,_ZBufferParams);
				half smoothstepResult290 = smoothstep( 0.0 , ( _FadeLength * 0.01 ) , abs( ( localMyCustomExpression271 - depthToLinear300 ) ));
				half4 appendResult187 = (half4(half3(1,1,1) , smoothstepResult290));
				half4 ifLocalVar218 = 0;
				UNITY_BRANCH 
				if( _UseDepthFade <= 0.0 )
				ifLocalVar218 = appendResult112;
				else
				ifLocalVar218 = ( appendResult112 * appendResult187 );
				half4 appendResult246 = (half4(( lerpResult242 * ifLocalVar218 ).rgb , (ifLocalVar218).w));
				half4 ifLocalVar243 = 0;
				UNITY_BRANCH 
				if( _UseRamp <= 0.0 )
				ifLocalVar243 = ifLocalVar218;
				else
				ifLocalVar243 = appendResult246;
				half4 ifLocalVar306 = 0;
				UNITY_BRANCH 
				if( _CameraFade <= 0.0 )
				ifLocalVar306 = ifLocalVar243;
				else
				ifLocalVar306 = ( appendResult311 * ifLocalVar243 );
				half4 clampResult363 = clamp( ifLocalVar306 , half4( 0,0,0,0 ) , half4( 3,3,3,1 ) );
				
				half3 BakedAlbedo = 0;
				half3 BakedEmission = 0;
				half3 Color = clampResult363.xyz;
				half Alpha = clampResult363.w;
				half AlphaClipThreshold = 0.5;
				half AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		Pass
		{
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#define ASE_SRP_VERSION 100500

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR

			struct VertexInput
			{
				half4 vertex : POSITION;
				half3 ase_normal : NORMAL;
				half4 ase_texcoord2 : TEXCOORD2;
				half4 ase_texcoord : TEXCOORD0;
				half4 ase_texcoord1 : TEXCOORD1;
				half4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				half4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				half4 shadowCoord : TEXCOORD1;
				#endif
				half4 ase_texcoord2 : TEXCOORD2;
				half4 ase_texcoord3 : TEXCOORD3;
				half4 ase_texcoord4 : TEXCOORD4;
				half4 ase_texcoord5 : TEXCOORD5;
				half4 ase_color : COLOR;
				half4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _AddColor;
			half4 _AddOffset;
			half4 _MainTex_ST;
			half4 _AddTexMask_ST;
			half4 _MainColor;
			half4 _BackColor;
			half4 _WidthColor;
			half4 _Color2;
			half4 _RampParam;
			half4 _TurbulenceTex_ST;
			half4 _Color0;
			half4 _MaskTex_ST;
			half4 _Color1;
			half _PowerV;
			half _UseAddTex;
			half _AddTexU;
			half _AddTexV;
			half _CUTOUT;
			half _UseMask;
			half _AlphaR;
			half _AddTexInt;
			half _Hardness;
			half _PowerU;
			half _Contrast;
			half _UseClip;
			half _Alpha;
			half _BlendMode;
			half _fresnelmultiply;
			half _DistortPower;
			half _CullMode;
			half _ZTest;
			half _CameraFade;
			half _CameraFadeLength;
			half _CameraFadeOffset;
			half _UseRamp;
			half _UseDepthFade;
			half _EdgeWidth;
			half _ShaderMode;
			half _UseBackColor;
			half _Brightness;
			half _MainPannerY;
			half _Dissolve;
			half _fresnelpower;
			half _DepthMode;
			half _Flip;
			half _Usefresnel;
			half _UseTurbulence;
			half _FadeLength;
			#ifdef TESSELLATION_ON
				half _TessPhongStrength;
				half _TessValue;
				half _TessMin;
				half _TessMax;
				half _TessEdgeLength;
				half _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainTex;
			sampler2D _TurbulenceTex;
			sampler2D _AddTex;
			sampler2D _AddTexMask;
			sampler2D _MaskTex;
			sampler2D _CameraDepthRT;


			half MyCustomExpression261( half2 enc )
			{
				 return dot(enc,half2(1,1/2047.0));
			}
			
			half MyCustomExpression271( half z, half4 zBufferParams )
			{
				z*=zBufferParams.x;
				return 1/(z+zBufferParams.y);
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				half3 customSurfaceDepth324 = v.vertex.xyz;
				half customEye324 = -TransformWorldToView(TransformObjectToWorld(customSurfaceDepth324)).z;
				o.ase_texcoord2.x = customEye324;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.yzw = ase_worldNormal;
				half4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				half4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord6 = screenPos;
				
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_texcoord5 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				
				o.ase_texcoord4.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					half3 defaultVertexValue = v.vertex.xyz;
				#else
					half3 defaultVertexValue = half3(0, 0, 0);
				#endif
				half3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = TransformWorldToHClip( positionWS );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				half4 vertex : INTERNALTESSPOS;
				half3 ase_normal : NORMAL;
				half4 ase_texcoord2 : TEXCOORD2;
				half4 ase_texcoord : TEXCOORD0;
				half4 ase_texcoord1 : TEXCOORD1;
				half4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				half edge[3] : SV_TessFactor;
				half inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord2 = v.ase_texcoord2;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_texcoord1 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				half4 tf = 1;
				half tessValue = _TessValue; half tessMin = _TessMin; half tessMax = _TessMax;
				half edgeLength = _TessEdgeLength; half tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, half3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				half3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				half phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN , half ase_vface : VFACE ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif

				half customEye324 = IN.ase_texcoord2.x;
				half cameraDepthFade324 = (customEye324 -_ProjectionParams.y - _CameraFadeOffset ) * (1.0 / _CameraFadeLength);
				half4 appendResult311 = (half4(1.0 , 1.0 , 1.0 , saturate( cameraDepthFade324 )));
				half3 ase_worldNormal = IN.ase_texcoord2.yzw;
				half dotResult230 = dot( _MainLightPosition.xyz , ase_worldNormal );
				half temp_output_235_0 = ( 1.0 - saturate( ( ( dotResult230 * 0.5 ) + 0.5 ) ) );
				half smoothstepResult238 = smoothstep( _RampParam.x , _RampParam.y , temp_output_235_0);
				half4 lerpResult240 = lerp( _Color0 , _Color1 , smoothstepResult238);
				half smoothstepResult241 = smoothstep( _RampParam.z , _RampParam.w , temp_output_235_0);
				half4 lerpResult242 = lerp( lerpResult240 , _Color2 , smoothstepResult241);
				half4 texCoord61 = IN.ase_texcoord3;
				texCoord61.xy = IN.ase_texcoord3.xy * half2( 1,1 ) + half2( 0,0 );
				half4 appendResult123 = (half4(texCoord61.x , texCoord61.y , texCoord61.z , ( texCoord61.w + 1.0 )));
				half4 appendResult141 = (half4(1.0 , 1.0 , _EdgeWidth , 1.0));
				half4 lerpResult143 = lerp( appendResult123 , appendResult141 , _ShaderMode);
				half4 break144 = lerpResult143;
				half edgebrightness152 = break144.w;
				half4 ifLocalVar205 = 0;
				UNITY_BRANCH 
				if( _UseBackColor <= 0.0 )
				ifLocalVar205 = _MainColor;
				else
				ifLocalVar205 = _BackColor;
				half4 lerpResult162 = lerp( ifLocalVar205 , _MainColor , max( ase_vface , 0.0 ));
				half2 texCoord65 = IN.ase_texcoord4.xy * half2( 1,1 ) + half2( 0,0 );
				half2 appendResult62 = (half2(break144.x , break144.y));
				half2 temp_output_63_0 = ( appendResult62 + half2( -1,-1 ) );
				half2 uv_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				half4 texCoord19 = IN.ase_texcoord5;
				texCoord19.xy = IN.ase_texcoord5.xy * half2( 1,1 ) + half2( 0,0 );
				half4 appendResult129 = (half4(texCoord19.x , texCoord19.y , texCoord19.z , texCoord19.w));
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				half fresnelNdotV192 = dot( ase_worldNormal, ase_worldViewDir );
				half fresnelNode192 = pow(1.0 - fresnelNdotV192, _fresnelpower);
				half temp_output_196_0 = ( min( fresnelNode192 , 1.0 ) * _fresnelmultiply );
				half lerpResult221 = lerp( temp_output_196_0 , ( 1.0 - temp_output_196_0 ) , _Flip);
				half lerpResult213 = lerp( 1.0 , lerpResult221 , _Usefresnel);
				half fresnel223 = lerpResult213;
				half4 appendResult137 = (half4(( 0.0 * ( _TimeParameters.x ) ) , ( ( _TimeParameters.x ) * _MainPannerY ) , ( _Dissolve * max( 1.0 , ( 1.0 - fresnel223 ) ) ) , _DistortPower));
				half4 lerpResult125 = lerp( appendResult129 , appendResult137 , _ShaderMode);
				half4 break128 = lerpResult125;
				half2 appendResult21 = (half2(break128.x , break128.y));
				half2 uv_TurbulenceTex = IN.ase_texcoord4.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				half2 appendResult36 = (half2(_PowerU , _PowerV));
				half4 tex2DNode31 = tex2D( _TurbulenceTex, ( uv_TurbulenceTex + ( appendResult36 * ( _TimeParameters.x ) ) ) );
				half Distort148 = break128.w;
				half ifLocalVar211 = 0;
				UNITY_BRANCH 
				if( _UseTurbulence <= 0.0 )
				ifLocalVar211 = 0.0;
				else
				ifLocalVar211 = ( ( tex2DNode31.r - 0.5 ) * Distort148 );
				half4 tex2DNode3 = tex2D( _MainTex, ( ( ( texCoord65 * temp_output_63_0 ) + ( temp_output_63_0 * half2( -0.5,-0.5 ) ) ) + ( ( uv_MainTex + appendResult21 ) + ifLocalVar211 ) ) );
				half2 appendResult336 = (half2(_AddTexU , _AddTexV));
				half2 uv_AddTexMask = IN.ase_texcoord4.xy * _AddTexMask_ST.xy + _AddTexMask_ST.zw;
				half4 ifLocalVar345 = 0;
				UNITY_BRANCH 
				if( _UseAddTex <= 0.0 )
				ifLocalVar345 = 0;
				else
				ifLocalVar345 = ( ( tex2D( _AddTex, ( ( ( uv_TurbulenceTex + ( appendResult336 * ( _TimeParameters.x ) ) ) * (_AddOffset).xy ) + (_AddOffset).zw ) ).r * tex2D( _AddTexMask, uv_AddTexMask ).r ) * _AddTexInt * _AddColor );
				half4 temp_cast_2 = (_Contrast).xxxx;
				half4 temp_output_8_0 = ( lerpResult162 * _Brightness * pow( ( tex2DNode3 + ifLocalVar345 ) , temp_cast_2 ) * IN.ase_color );
				half temp_output_87_0 = ( tex2DNode31.r + 1.0 );
				half dissolve146 = break128.z;
				half edgewidth150 = break144.z;
				half temp_output_116_0 = dissolve146 *  edgewidth150 + dissolve146;
				half hardness89 = _Hardness;
				half temp_output_91_0 = ( 1.0 - hardness89 );
				half2 appendResult158 = (half2(saturate( ( ( ( temp_output_87_0 - ( temp_output_116_0 * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) ) , saturate( ( ( ( temp_output_87_0 - ( ( temp_output_116_0 - edgewidth150 ) * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) )));
				half2 ifLocalVar219 = 0;
				UNITY_BRANCH 
				if( _UseClip <= 0.0 )
				ifLocalVar219 = 1;
				else
				ifLocalVar219 = appendResult158;
				half2 break159 = ifLocalVar219;
				half4 lerpResult109 = lerp( ( edgebrightness152 * _WidthColor * temp_output_8_0 ) , temp_output_8_0 , break159.x);
				half ifLocalVar206 = 0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
				ifLocalVar206 = tex2DNode3.a;
				else
				ifLocalVar206 = tex2DNode3.r;
				half2 uv_MaskTex = IN.ase_texcoord4.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				half ifLocalVar201 = 0;
				UNITY_BRANCH 
				if( _UseMask <= 0.0 )
				ifLocalVar201 = 1.0;
				else
				ifLocalVar201 = tex2D( _MaskTex, uv_MaskTex ).r;
				half temp_output_74_0 = min( ( IN.ase_color.a * _MainColor.a * ifLocalVar206 * break159.y * _Alpha * ifLocalVar201 * fresnel223 ) , 1.0 );
				half4 appendResult173 = (half4(lerpResult109.rgb , temp_output_74_0));
				clip( temp_output_74_0 - min( _CUTOUT , _DepthMode ));
				half3 appendResult171 = (half3(lerpResult109.rgb));
				half4 appendResult172 = (half4(appendResult171 , 1.0));
				half4 lerpResult170 = lerp( appendResult173 , appendResult172 , _DepthMode);
				half4 appendResult112 = (half4(lerpResult170));
				half4 screenPos = IN.ase_texcoord6;
				half4 ase_screenPosNorm = screenPos * (1.0 / screenPos.w);
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				half2 enc261 = (tex2D( _CameraDepthRT, ase_screenPosNorm.xy )).rg;
				half localMyCustomExpression261 = MyCustomExpression261( enc261 );
				half z271 = localMyCustomExpression261;
				half4 zBufferParams271 = _ZBufferParams;
				half localMyCustomExpression271 = MyCustomExpression271( z271 , zBufferParams271 );
				half depthToLinear300 = Linear01Depth(ase_screenPosNorm.z,_ZBufferParams);
				half smoothstepResult290 = smoothstep( 0.0 , ( _FadeLength * 0.01 ) , abs( ( localMyCustomExpression271 - depthToLinear300 ) ));
				half4 appendResult187 = (half4(half3(1,1,1) , smoothstepResult290));
				half4 ifLocalVar218 = 0;
				UNITY_BRANCH 
				if( _UseDepthFade <= 0.0 )
				ifLocalVar218 = appendResult112;
				else
				ifLocalVar218 = ( appendResult112 * appendResult187 );
				half4 appendResult246 = (half4(( lerpResult242 * ifLocalVar218 ).rgb , (ifLocalVar218).w));
				half4 ifLocalVar243 = 0;
				UNITY_BRANCH 
				if( _UseRamp <= 0.0 )
				ifLocalVar243 = ifLocalVar218;
				else
				ifLocalVar243 = appendResult246;
				half4 ifLocalVar306 = 0;
				UNITY_BRANCH 
				if( _CameraFade <= 0.0 )
				ifLocalVar306 = ifLocalVar243;
				else
				ifLocalVar306 = ( appendResult311 * ifLocalVar243 );
				half4 clampResult363 = clamp( ifLocalVar306 , half4( 0,0,0,0 ) , half4( 3,3,3,1 ) );
				
				half Alpha = clampResult363.w;
				half AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
}
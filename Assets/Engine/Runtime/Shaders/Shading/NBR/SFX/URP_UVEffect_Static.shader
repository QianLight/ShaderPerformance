// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/SFX/UVEffect_static"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin][Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
		_CUTOUT("CUTOUT", Range( 0 , 1)) = 0.5
		_Brightness("Brightness", Float) = 1
		_Contrast("Contrast", Float) = 1
		[HDR]_MainColor("Main Color", Color) = (1,1,1,1)
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
		_MaskTex("Mask Tex", 2D) = "white" {}
		_Hardness("Hardness", Range( 0 , 0.99)) = 0
		_Dissolve("Dissolve", Range( 0 , 1)) = 0
		[HDR]_WidthColor("WidthColor", Color) = (1,1,1,1)
		_EdgeWidth("EdgeWidth", Range( 0 , 1)) = 0
		_Alpha("Alpha", Range( 0 , 10)) = 1
		_FadeLength("FadeLength", Range( 0 , 10)) = 0.5
		_fresnelpower("fresnel power", Range( 0 , 20)) = 1
		_fresnelmultiply("fresnel multiply", Range( 0 , 3)) = 1
		[Toggle]_Flip("Flip", Range( 0 , 1)) = 0
		_Color0("Color 0", Color) = (1,0,0,0)
		_Color1("Color 1", Color) = (0,1,0.03857636,0)
		_Color2("Color 2", Color) = (0,0.4486432,1,0)
		_RampParam("RampParam", Vector) = (0.3,0.33,0.6,0.66)
		_CameraFadeOffset("CameraFadeOffset", Float) = 0
		[Toggle(_CAMERAFADE_ON)] _CameraFade("CameraFade", Float) = 0
		[Toggle(_USERAMP_ON)] _UseRamp("UseRamp", Float) = 0
		[Toggle(_USEFRESNEL_ON)] _Usefresnel("Usefresnel", Float) = 0
		[Toggle(_USEDEPTHFADE_ON)] _UseDepthFade("UseDepthFade", Float) = 0
		_CameraFadeLength("CameraFadeLength", Float) = 5
		[Toggle(_SHADERMODE_ON)] _ShaderMode("ShaderMode", Float) = 0
		[Toggle(_USECLIP_ON)] _UseClip("UseClip", Float) = 0
		[Toggle(_USEMASK_ON)] _UseMask("UseMask", Float) = 0
		[Toggle(_USETURBULENCE_ON)] _UseTurbulence("UseTurbulence", Float) = 0
		[ASEEnd][Toggle(_DEPTHMODE_ON)] _DepthMode("DepthMode", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull [_CullMode]
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "ForwardBase"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha [_BlendMode]
			ZWrite [_DepthMode]
			ZTest [_ZTest]
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 100500
			#define REQUIRE_DEPTH_TEXTURE 1

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

			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_COLOR
			#define ASE_NEEDS_VERT_POSITION
			#pragma multi_compile_local __ _USERAMP_ON
			#pragma multi_compile_local __ _USEDEPTHFADE_ON
			#pragma multi_compile_local __ _DEPTHMODE_ON
			#pragma multi_compile_local __ _SHADERMODE_ON
			#pragma multi_compile_local __ _USEFRESNEL_ON
			#pragma multi_compile_local __ _USETURBULENCE_ON
			#pragma multi_compile_local __ _USECLIP_ON
			#pragma multi_compile_local __ _USEMASK_ON
			#pragma multi_compile_local __ _CAMERAFADE_ON


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD2;
				#endif
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_color : COLOR;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _TurbulenceTex_ST;
			float4 _Color2;
			float4 _RampParam;
			float4 _Color1;
			float4 _WidthColor;
			float4 _Color0;
			float4 _BackColor;
			float4 _MainColor;
			float4 _MainTex_ST;
			float4 _MaskTex_ST;
			half _Contrast;
			half _Hardness;
			float _AlphaR;
			float _FadeLength;
			half _CUTOUT;
			float _PowerV;
			half _Alpha;
			half _CullMode;
			half _DistortPower;
			float _CameraFadeLength;
			float _Flip;
			half _fresnelmultiply;
			half _fresnelpower;
			half _Dissolve;
			float _MainPannerY;
			float _MainPannerX;
			half _Brightness;
			float _UseBackColor;
			half _EdgeWidth;
			float _ZTest;
			half _BlendMode;
			float _PowerU;
			float _CameraFadeOffset;
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
			sampler2D _MaskTex;
			uniform float4 _CameraDepthTexture_TexelSize;


			float4 MyCustomExpression446( float3 Color, float Alpha, float Cutout )
			{
				#if _DEPTHMODE_ON
					clip(Alpha-Cutout);
					return float4(Color.rgb,1);
				#else
					return float4(Color.rgb, Alpha);
				#endif
			}
			
			
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord6.xyz = ase_worldNormal;
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord7 = screenPos;
				
				float3 customSurfaceDepth324 = v.vertex.xyz;
				float customEye324 = -TransformWorldToView(TransformObjectToWorld(customSurfaceDepth324)).z;
				o.ase_texcoord4.z = customEye324;
				
				o.ase_texcoord3 = v.ase_texcoord2;
				o.ase_texcoord4.xy = v.ase_texcoord.xy;
				o.ase_texcoord5 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord6.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

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

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
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
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
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
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord2 = patch[0].ase_texcoord2 * bary.x + patch[1].ase_texcoord2 * bary.y + patch[2].ase_texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_texcoord1 = patch[0].ase_texcoord1 * bary.x + patch[1].ase_texcoord1 * bary.y + patch[2].ase_texcoord1 * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
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

			half4 frag ( VertexOutput IN , half ase_vface : VFACE ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif
				#ifdef _DEPTHMODE_ON
				float staticSwitch453 = 1.0;
				#else
				float staticSwitch453 = 1.0;
				#endif
				float4 texCoord61 = IN.ase_texcoord3;
				texCoord61.xy = IN.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
				float4 appendResult123 = (float4(texCoord61.x , texCoord61.y , texCoord61.z , ( texCoord61.w + 1.0 )));
				float4 appendResult141 = (float4(1.0 , 1.0 , _EdgeWidth , 1.0));
				#ifdef _SHADERMODE_ON
				float4 staticSwitch397 = appendResult141;
				#else
				float4 staticSwitch397 = appendResult123;
				#endif
				float4 break144 = staticSwitch397;
				half edgebrightness152 = break144.w;
				float4 ifLocalVar205 = 0;
				UNITY_BRANCH 
				if( _UseBackColor <= 0.0 )
				ifLocalVar205 = _MainColor;
				else
				ifLocalVar205 = _BackColor;
				float4 lerpResult162 = lerp( ifLocalVar205 , _MainColor , max( ase_vface , 0.0 ));
				float2 texCoord65 = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult62 = (float2(break144.x , break144.y));
				float2 temp_output_63_0 = ( appendResult62 + float2( -1,-1 ) );
				float2 uv_MainTex = IN.ase_texcoord4.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 texCoord19 = IN.ase_texcoord5;
				texCoord19.xy = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float4 appendResult129 = (float4(texCoord19.x , texCoord19.y , texCoord19.z , texCoord19.w));
				float2 appendResult380 = (float2(( _MainPannerX * ( _TimeParameters.x ) ) , ( ( _TimeParameters.x ) * _MainPannerY )));
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldNormal = IN.ase_texcoord6.xyz;
				float fresnelNdotV192 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode192 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV192, _fresnelpower ) );
				float temp_output_196_0 = ( min( fresnelNode192 , 1.0 ) * _fresnelmultiply );
				float lerpResult221 = lerp( temp_output_196_0 , ( 1.0 - temp_output_196_0 ) , _Flip);
				#ifdef _USEFRESNEL_ON
				float staticSwitch424 = lerpResult221;
				#else
				float staticSwitch424 = 1.0;
				#endif
				float fresnel223 = staticSwitch424;
				float4 appendResult137 = (float4(frac( appendResult380 ) , ( _Dissolve * max( 1.0 , ( 1.0 - fresnel223 ) ) ) , _DistortPower));
				#ifdef _SHADERMODE_ON
				float4 staticSwitch442 = appendResult137;
				#else
				float4 staticSwitch442 = appendResult129;
				#endif
				float4 break128 = staticSwitch442;
				float2 appendResult21 = (float2(break128.x , break128.y));
				float2 uv_TurbulenceTex = IN.ase_texcoord4.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				float2 appendResult36 = (float2(_PowerU , _PowerV));
				float4 tex2DNode31 = tex2Dbias( _TurbulenceTex, float4( ( uv_TurbulenceTex + frac( ( appendResult36 * ( _TimeParameters.x ) ) ) ), 0, 4.0) );
				half Distort148 = break128.w;
				#ifdef _USETURBULENCE_ON
				float staticSwitch409 = ( ( tex2DNode31.r - 0.5 ) * Distort148 );
				#else
				float staticSwitch409 = 0.0;
				#endif
				float Turbulence410 = staticSwitch409;
				float4 tex2DNode3 = tex2D( _MainTex, ( ( ( texCoord65 * temp_output_63_0 ) + ( temp_output_63_0 * float2( -0.5,-0.5 ) ) ) + ( ( uv_MainTex + appendResult21 ) + Turbulence410 ) ) );
				float4 temp_cast_0 = (_Contrast).xxxx;
				float4 temp_output_8_0 = ( lerpResult162 * _Brightness * pow( tex2DNode3 , temp_cast_0 ) * IN.ase_color );
				float2 temp_cast_1 = (1.0).xx;
				float TurBefore412 = tex2DNode31.r;
				float temp_output_87_0 = ( TurBefore412 + 1.0 );
				half dissolve146 = break128.z;
				half edgewidth150 = break144.z;
				float temp_output_116_0 = ( dissolve146 * ( 1.0 + edgewidth150 ) );
				half hardness89 = _Hardness;
				float temp_output_91_0 = ( 1.0 - hardness89 );
				float2 appendResult158 = (float2(saturate( ( ( ( temp_output_87_0 - ( temp_output_116_0 * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) ) , saturate( ( ( ( temp_output_87_0 - ( ( temp_output_116_0 - edgewidth150 ) * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) )));
				#ifdef _USECLIP_ON
				float2 staticSwitch404 = appendResult158;
				#else
				float2 staticSwitch404 = temp_cast_1;
				#endif
				float2 break159 = staticSwitch404;
				float clipX405 = break159.x;
				float4 lerpResult109 = lerp( ( edgebrightness152 * _WidthColor * temp_output_8_0 ) , temp_output_8_0 , clipX405);
				float3 Color446 = lerpResult109.rgb;
				float ifLocalVar206 = 0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
				ifLocalVar206 = tex2DNode3.a;
				else
				ifLocalVar206 = tex2DNode3.r;
				float clipY406 = break159.y;
				float2 uv_MaskTex = IN.ase_texcoord4.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				#ifdef _USEMASK_ON
				float staticSwitch398 = tex2D( _MaskTex, uv_MaskTex ).r;
				#else
				float staticSwitch398 = 1.0;
				#endif
				float Mask399 = staticSwitch398;
				float Alpha446 = min( ( IN.ase_color.a * _MainColor.a * ifLocalVar206 * clipY406 * _Alpha * fresnel223 * Mask399 ) , 1.0 );
				float Cutout446 = _CUTOUT;
				float4 localMyCustomExpression446 = MyCustomExpression446( Color446 , Alpha446 , Cutout446 );
				float4 appendResult112 = (float4(( staticSwitch453 * localMyCustomExpression446 )));
				float4 MainAfter420 = appendResult112;
				float4 screenPos = IN.ase_texcoord7;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth384 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float depthToLinear300 = LinearEyeDepth(ase_screenPosNorm.z,_ZBufferParams);
				float smoothstepResult290 = smoothstep( 0.0 , ( _FadeLength * 1.0 ) , abs( ( eyeDepth384 - depthToLinear300 ) ));
				float4 appendResult187 = (float4(float3(1,1,1) , smoothstepResult290));
				float4 MainWithDepth419 = ( MainAfter420 * appendResult187 );
				#ifdef _USEDEPTHFADE_ON
				float4 staticSwitch417 = MainWithDepth419;
				#else
				float4 staticSwitch417 = MainAfter420;
				#endif
				float4 MainAfterDepth428 = staticSwitch417;
				float dotResult230 = dot( _MainLightPosition.xyz , ase_worldNormal );
				float temp_output_235_0 = ( 1.0 - saturate( ( ( dotResult230 * 0.5 ) + 0.5 ) ) );
				float smoothstepResult238 = smoothstep( _RampParam.x , _RampParam.y , temp_output_235_0);
				float4 lerpResult240 = lerp( _Color0 , _Color1 , smoothstepResult238);
				float smoothstepResult241 = smoothstep( _RampParam.z , _RampParam.w , temp_output_235_0);
				float4 lerpResult242 = lerp( lerpResult240 , _Color2 , smoothstepResult241);
				float4 appendResult246 = (float4(( lerpResult242 * MainAfterDepth428 ).rgb , (MainAfterDepth428).w));
				float4 Ramp426 = appendResult246;
				#ifdef _USERAMP_ON
				float4 staticSwitch425 = Ramp426;
				#else
				float4 staticSwitch425 = MainAfterDepth428;
				#endif
				float3 temp_output_434_0 = (staticSwitch425).xyz;
				float grayscale387 = Luminance(temp_output_434_0);
				float temp_output_394_0 = ( grayscale387 + 1E-07 );
				
				float customEye324 = IN.ase_texcoord4.z;
				float cameraDepthFade324 = (( customEye324 -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
				#ifdef _CAMERAFADE_ON
				float staticSwitch432 = saturate( cameraDepthFade324 );
				#else
				float staticSwitch432 = 1.0;
				#endif
				float CameraFade440 = staticSwitch432;
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = max( ( ( (temp_output_434_0).xyz / temp_output_394_0 ) * min( temp_output_394_0 , 4.0 ) ) , float3( 0,0,0 ) );
				float Alpha = max( 0.0 , ( (staticSwitch425).w * CameraFade440 ) );
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

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

	
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18908
2663.2;319.3333;1851.2;1025.4;-2300.489;845.3282;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;402;-2096.351,3950.812;Inherit;False;2292.059;847.6155;Fresnel;11;215;223;200;221;194;196;197;222;192;193;424;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;193;-2035.672,4115.654;Half;False;Property;_fresnelpower;fresnel power;25;0;Create;True;0;0;0;False;0;False;1;2;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;192;-1714.357,4076.397;Inherit;True;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-1697.934,4310.146;Half;False;Property;_fresnelmultiply;fresnel multiply;26;0;Create;True;0;0;0;False;0;False;1;0.5;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;222;-1398.931,4188.719;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-1277.526,4230.083;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;194;-1123.092,4326.625;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;215;-1268.767,4645.371;Float;False;Property;_Flip;Flip;27;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-879.6871,4177.097;Half;False;Constant;_fresnelfloat;fresnel float;35;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;221;-894.2418,4286.211;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;424;-509.8566,4221.643;Inherit;False;Property;_Usefresnel;Usefresnel;35;0;Create;True;0;0;0;False;0;False;1;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-133.6463,4219.644;Float;False;fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-4392.041,358.0438;Inherit;False;223;fresnel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-4785.052,66.01691;Float;False;Property;_MainPannerY;Main Panner Y;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-4790.698,-167.5979;Float;False;Property;_MainPannerX;Main Panner X;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;133;-4781.957,-80.49101;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;225;-4145.042,360.0438;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-4451.684,-125.1541;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-4474.65,-21.55399;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-4309.276,-856.0739;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;140;-4285.733,-647.2078;Half;False;Constant;_UVRpeat;UVRpeat;24;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;160;-4012.656,-703.3108;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-4272.845,225.3929;Half;False;Property;_Dissolve;Dissolve;20;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-4312.174,-566.217;Half;False;Property;_EdgeWidth;EdgeWidth;22;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;414;-5626.812,2147.652;Inherit;False;2458.503;806.9854;Turbulence;18;412;57;41;149;410;409;71;31;383;30;40;39;382;38;37;36;35;33;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;380;-4264.859,-79.43437;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;249;-3975.132,334.1109;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-5573.634,2412.791;Float;False;Property;_PowerU;Power U;16;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-4261.394,-355.6454;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-5573.89,2499.976;Float;False;Property;_PowerV;Power V;17;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;141;-3839.927,-672.4958;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FractNode;381;-4000.766,-85.23906;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;136;-4240.157,134.8238;Half;False;Property;_DistortPower;Distort Power;15;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;-3816.041,190.1209;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;123;-3840.928,-828.7457;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TimeNode;37;-5588.573,2587.071;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;36;-5313.861,2452.98;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;137;-3637.896,-86.09697;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;129;-3746.064,-337.3059;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;397;-3631.799,-790.2786;Inherit;False;Property;_ShaderMode;ShaderMode;38;0;Create;True;0;0;0;False;0;False;1;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;442;-3476.373,-221.596;Inherit;False;Property;_ShaderMode;ShaderMode;43;0;Create;True;0;0;0;False;0;False;1;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-5118.571,2558.716;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;415;-3023.579,1916.031;Inherit;False;3875.218;1210.358;Clip;32;413;87;406;405;159;59;404;158;106;81;104;80;78;105;79;103;107;101;88;83;100;114;99;92;91;116;90;151;115;89;50;147;;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;144;-3303.908,-788.3748;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FractNode;382;-4952.359,2575.56;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-5168.628,2373.866;Inherit;False;0;30;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;50;-1234.229,2451.705;Half;False;Property;_Hardness;Hardness;19;0;Create;True;0;0;0;False;0;False;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-2992.328,-652.5964;Half;False;edgewidth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;128;-3192.913,-158.9324;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-938.7309,2438.394;Half;False;hardness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;147;-2972.291,2710.53;Inherit;False;150;edgewidth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-4766.974,2556.549;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;30;-4924.737,2227.516;Float;True;Property;_TurbulenceTex;Turbulence Tex;14;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;383;-4721.578,2785.212;Inherit;False;Constant;_Float0;Float 0;45;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;-2885.624,13.11249;Half;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;31;-4495.861,2543.947;Inherit;True;Property;_TextureSample2;Texture Sample 2;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;151;-2772.879,2471.428;Inherit;False;146;dissolve;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-2663.91,2589.456;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-2771.289,2826.089;Inherit;False;89;hardness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-2510.914,2470.397;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;412;-3653.476,2658.285;Inherit;False;TurBefore;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;148;-2901.44,96.49587;Half;False;Distort;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;91;-2524.142,2840.9;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;-2338.256,2527.692;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;114;-2208.752,2672.708;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-2256.821,2810.31;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;71;-4118.688,2407.742;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-4135.026,2525.794;Inherit;False;148;Distort;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;413;-1779.078,2455.512;Inherit;False;412;TurBefore;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-3904.274,2409.504;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-2135.384,2462.041;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-1491.367,2449.898;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-3967.241,2241.047;Half;False;Constant;_turbulencefloat;turbulence float;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-2027.972,2669.597;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;88;-1339.785,2221.027;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;-2971.731,-787.2756;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-923.5358,2710.852;Inherit;False;89;hardness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;409;-3686.916,2242.068;Inherit;False;Property;_UseTurbulence;UseTurbulence;41;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;-1317.491,2623.813;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-2928.941,-159.4378;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;105;-619.122,2713.248;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;79;-648.268,2444.855;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;410;-3403.162,2244.32;Inherit;False;Turbulence;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-2674.126,-642.593;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;78;-653.092,2348.29;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;103;-614.236,2617.673;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;65;-2952.962,-1041.212;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-2977.686,-387.7958;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;411;-2459.381,87.39552;Inherit;False;410;Turbulence;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-2427.702,-171.3269;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-2468.767,-500.343;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-0.5,-0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;104;-451.602,2634.098;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-2486.799,-819.9025;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;80;-491.0271,2387.469;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;106;-319.1001,2604.637;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-2148.561,-86.30107;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-2201.187,-722.22;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;403;-2016.781,3441.493;Inherit;False;1159.022;458.9237;Mask;5;399;398;55;24;23;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;81;-326.542,2389.63;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;158;3.071995,2416.956;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-1926.312,-305.8669;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1968.374,-647.6528;Float;True;Property;_MainTex;Main Tex;10;0;Create;True;0;0;0;False;0;False;None;4692ba94be591ae4a97d0eafb094425c;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;59;6.678928,2325.185;Half;False;Constant;_Clipfloat;Clip float;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;23;-1954.179,3665.043;Float;True;Property;_MaskTex;Mask Tex;18;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;24;-1684.641,3664.231;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;404;220.8909,2389.006;Inherit;False;Property;_UseClip;UseClip;39;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;9;-153.2925,-2030.688;Float;False;Property;_MainColor;Main Color;7;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;164;-158.0261,-2234.258;Float;False;Property;_BackColor;BackColor;9;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;203;-161.2018,-2435.239;Float;False;Property;_UseBackColor;Use BackColor;8;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-1709.973,-335.5461;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FaceVariableNode;161;214.9813,-1909.477;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-1538.577,3542.412;Half;False;Constant;_Maskfloat;Mask float;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;159;451.7901,2393.214;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;5;-336.6835,-1085.096;Half;False;Property;_Contrast;Contrast;6;0;Create;True;0;0;0;False;0;False;1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;361;-925.9382,-942.5132;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;205;183.6137,-2236.896;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;163;328.0681,-1913.001;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;398;-1363.013,3604.982;Inherit;False;Property;_UseMask;UseMask;40;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-766.5992,-418.3558;Float;False;Property;_AlphaR;Alpha R;11;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;22;813.9332,-1026.046;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;162;496.5851,-2024.007;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;406;626.8919,2448.54;Inherit;False;clipY;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;682.702,-1571.411;Half;False;Property;_Brightness;Brightness;5;0;Create;True;0;0;0;False;0;False;1;1.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;399;-1093.269,3606.829;Inherit;False;Mask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;7;-153.4279,-1191.416;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-3008.551,-575.359;Half;False;edgebrightness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;405;630.8919,2334.54;Inherit;False;clipX;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;1012.882,-1449.162;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;206;-268.3388,-387.8521;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;110;1429.24,-1858.036;Float;False;Property;_WidthColor;WidthColor;21;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;153;1477.559,-2069.687;Inherit;False;152;edgebrightness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;563.5672,434.233;Half;False;Property;_Alpha;Alpha;23;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;401;1482.412,295.568;Inherit;False;223;fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;418;1598.149,2635.277;Inherit;False;3306.813;938.4063;DepthFade;20;189;269;271;180;270;261;187;188;290;191;183;289;184;288;300;384;304;179;419;421;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;332;287.842,-712.9236;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;408;1443.434,154.6066;Inherit;False;406;clipY;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;400;1484.659,387.4086;Inherit;False;399;Mask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;1690.135,80.70615;Inherit;False;7;7;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;407;1740.315,-273.6801;Inherit;False;405;clipX;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;451;2845.658,-481.7413;Inherit;False;768.3333;260;防止裁剪;4;448;447;449;453;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;1921.611,-1471.448;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;179;1664.628,2952.493;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;109;2033.168,-171.6838;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;168;2305.44,206.2302;Half;False;Property;_CUTOUT;CUTOUT;4;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;74;2017.75,70.57481;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;304;2005.297,3175.709;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;447;2897.658,-431.7413;Inherit;False;Constant;_Float4;Float 4;45;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;448;2901.658,-334.7413;Inherit;False;Constant;_Float5;Float 5;45;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;446;2885.215,-157.3061;Inherit;False;#if _DEPTHMODE_ON$	clip(Alpha-Cutout)@$	return float4(Color.rgb,1)@$#else$	return float4(Color.rgb, Alpha)@$#endif$;4;Create;3;True;Color;FLOAT3;0,0,0;In;;Inherit;False;True;Alpha;FLOAT;0;In;;Inherit;False;True;Cutout;FLOAT;0;In;;Inherit;False;My Custom Expression;True;False;0;;False;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;453;3098.167,-390.824;Inherit;False;Property;_DepthMode;DepthMode;43;0;Create;False;0;0;0;False;0;False;1;0;1;True;_DEPTHMODE_ON;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;384;2846.928,2764.408;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LinearDepthNode;300;2809.311,3186.193;Inherit;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;449;3450.658,-391.7413;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;184;2975.304,3288.48;Float;False;Property;_FadeLength;FadeLength;24;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;288;3061.978,2946.57;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;430;1536.63,3744.734;Inherit;False;2920.431;973.8779;Ramp;22;429;426;248;229;227;246;244;242;241;240;239;236;237;238;235;234;233;232;231;230;379;228;;1,1,1,1;0;0
Node;AmplifyShaderEditor.AbsOpNode;289;3312.849,2961.156;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;379;1616.105,4006.395;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;112;3571.838,193.5905;Inherit;True;FLOAT4;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;183;3262.235,3202.192;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;228;1678.253,4400.181;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;3269.689,3293.563;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;290;3468.935,3123.518;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;230;2109.252,4309.181;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;420;3820.391,184.2038;Inherit;False;MainAfter;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector3Node;188;3471.91,2938.022;Float;False;Constant;_Vector0;Vector 0;29;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;421;3662.752,2963.32;Inherit;False;420;MainAfter;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;231;2249.686,4309.356;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;187;3674.277,3096.311;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;189;3907.315,3072.945;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;232;2391.528,4305.811;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;233;2513.34,4306.954;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;419;4163.137,3116.117;Inherit;False;MainWithDepth;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.OneMinusNode;235;2661.027,4305.811;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;422;4143.51,183.7349;Inherit;False;420;MainAfter;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector4Node;234;2456.953,4445.754;Float;False;Property;_RampParam;RampParam;31;0;Create;True;0;0;0;False;0;False;0.3,0.33,0.6,0.66;0.1,0.44,0.8,0.91;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;423;4110.129,306.4214;Inherit;False;419;MainWithDepth;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;417;4406.688,186.1511;Inherit;False;Property;_UseDepthFade;UseDepthFade;36;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;236;2962.618,3865.679;Float;False;Property;_Color0;Color 0;28;0;Create;True;0;0;0;False;0;False;1,0,0,0;0.8537736,0.93826,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;237;2960.129,4028.333;Float;False;Property;_Color1;Color 1;29;0;Create;True;0;0;0;False;0;False;0,1,0.03857636,0;0.4685831,0.6050593,0.735849,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;238;2957.054,4219.547;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;241;2936.544,4521.028;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;240;3289.252,4012.244;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;428;4693.155,185.8264;Inherit;False;MainAfterDepth;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;239;2935.628,4358.267;Float;False;Property;_Color2;Color 2;30;0;Create;True;0;0;0;False;0;False;0,0.4486432,1,0;0.4295122,0.5751134,0.7169812,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;429;3430.137,4565.742;Inherit;False;428;MainAfterDepth;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;242;3569.239,4321.311;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;244;3812.519,4343.768;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;248;3848.666,4487.682;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;246;4044.809,4419.035;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;426;4258.545,4415.722;Inherit;False;Ramp;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;441;1056.067,4950.747;Inherit;False;1296.738;469.3635;CameraFade;8;440;432;438;326;324;322;325;321;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;321;1137.463,5317.67;Float;False;Property;_CameraFadeOffset;CameraFadeOffset;32;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;325;1165.433,5054.419;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;427;4735.328,316.1843;Inherit;False;426;Ramp;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;322;1144.193,5216.591;Float;False;Property;_CameraFadeLength;CameraFadeLength;37;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;425;4963.847,184.1393;Inherit;False;Property;_UseRamp;UseRamp;34;0;Create;True;0;0;0;False;0;False;1;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CameraDepthFade;324;1396.373,5207.174;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;326;1648.344,5219.354;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;438;1560,5056.173;Inherit;False;Constant;_Float2;Float 2;44;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;434;5216.514,-99.24549;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCGrayscale;387;5542.069,-47.75781;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;432;1823.388,5067.736;Inherit;False;Property;_CameraFade;CameraFade;33;0;Create;True;0;0;0;False;0;False;1;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;393;5536.41,-203.277;Inherit;False;True;True;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;394;5730.67,-65.09289;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1E-07;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;440;2117.847,5074.674;Inherit;False;CameraFade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;435;5234.35,473.9361;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;388;5789.069,-210.7578;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMinOpNode;391;5810.069,175.2422;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;439;5420.489,583.4233;Inherit;False;440;CameraFade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;437;5777.85,485.1143;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;390;6013.069,12.24224;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;270;2522.208,3037.41;Float;False;Global;_ZBufferParams;_ZBufferParams;40;0;Fetch;True;0;0;0;False;0;False;0,0,0,0;199999,1,115.4698,0.000579834;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;395;6048.729,467.6872;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;229;1889.253,4240.181;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-6370.131,334.8212;Half;False;Property;_BlendMode;Blend Mode;0;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;180;1978.284,2923.185;Inherit;True;Global;_CameraDepthRT;_CameraDepthRT;1;1;[HDR];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;178;-6354.122,55.75672;Float;False;Property;_ZTest;ZTest;3;1;[Enum];Create;True;0;1;Option1;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;269;2305.209,2925.475;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;261;2526.596,2926.257;Float;False; return dot(enc,float3(1.0, 1 / 255.0, 1 / 65025.0))@;1;Create;1;True;enc;FLOAT3;0,0,0;In;;Float;False;My Custom Expression;True;False;0;;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-6356.948,240.4752;Half;False;Property;_CullMode;Cull Mode;2;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;452;2871.167,-620.824;Inherit;False;Property;_DepthMode;DepthMode;42;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;227;1675.161,4238.711;Float;False;Global;_MainLightDir0;_MainLightDir0;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5242523,0.621394,0.582262,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;396;6212.729,21.68736;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CustomExpressionNode;271;2783.966,2929.106;Float;False;z*=zBufferParams.z@$return 1/(z+zBufferParams.w)@;1;Create;2;True;z;FLOAT;0;In;;Float;False;True;zBufferParams;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;True;False;0;;False;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;375;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;372;5536.92,62.15678;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;373;6427.449,19.91515;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;URP/SFX/UVEffect_static;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;ForwardBase;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;18;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;True;2;5;False;-1;10;True;70;0;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;452;True;3;True;178;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;  Blend;2;Two Sided;1;Cast Shadows;0;  Use Shadow Threshold;0;Receive Shadows;0;GPU Instancing;1;LOD CrossFade;0;Built-in Fog;0;DOTS Instancing;0;Meta Pass;0;Extra Pre Pass;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;5;False;True;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;376;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;374;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;192;3;193;0
WireConnection;222;0;192;0
WireConnection;196;0;222;0
WireConnection;196;1;197;0
WireConnection;194;0;196;0
WireConnection;221;0;196;0
WireConnection;221;1;194;0
WireConnection;221;2;215;0
WireConnection;424;1;200;0
WireConnection;424;0;221;0
WireConnection;223;0;424;0
WireConnection;225;0;224;0
WireConnection;138;0;130;0
WireConnection;138;1;133;2
WireConnection;139;0;133;2
WireConnection;139;1;131;0
WireConnection;160;0;61;4
WireConnection;380;0;138;0
WireConnection;380;1;139;0
WireConnection;249;1;225;0
WireConnection;141;0;140;0
WireConnection;141;1;140;0
WireConnection;141;2;98;0
WireConnection;141;3;140;0
WireConnection;381;0;380;0
WireConnection;226;0;82;0
WireConnection;226;1;249;0
WireConnection;123;0;61;1
WireConnection;123;1;61;2
WireConnection;123;2;61;3
WireConnection;123;3;160;0
WireConnection;36;0;33;0
WireConnection;36;1;35;0
WireConnection;137;0;381;0
WireConnection;137;2;226;0
WireConnection;137;3;136;0
WireConnection;129;0;19;1
WireConnection;129;1;19;2
WireConnection;129;2;19;3
WireConnection;129;3;19;4
WireConnection;397;1;123;0
WireConnection;397;0;141;0
WireConnection;442;1;129;0
WireConnection;442;0;137;0
WireConnection;38;0;36;0
WireConnection;38;1;37;2
WireConnection;144;0;397;0
WireConnection;382;0;38;0
WireConnection;150;0;144;2
WireConnection;128;0;442;0
WireConnection;89;0;50;0
WireConnection;40;0;39;0
WireConnection;40;1;382;0
WireConnection;146;0;128;2
WireConnection;31;0;30;0
WireConnection;31;1;40;0
WireConnection;31;2;383;0
WireConnection;115;1;147;0
WireConnection;116;0;151;0
WireConnection;116;1;115;0
WireConnection;412;0;31;1
WireConnection;148;0;128;3
WireConnection;91;0;90;0
WireConnection;92;1;91;0
WireConnection;114;0;116;0
WireConnection;114;1;147;0
WireConnection;99;1;91;0
WireConnection;71;0;31;1
WireConnection;41;0;71;0
WireConnection;41;1;149;0
WireConnection;83;0;116;0
WireConnection;83;1;92;0
WireConnection;87;0;413;0
WireConnection;100;0;114;0
WireConnection;100;1;99;0
WireConnection;88;0;87;0
WireConnection;88;1;83;0
WireConnection;62;0;144;0
WireConnection;62;1;144;1
WireConnection;409;1;57;0
WireConnection;409;0;41;0
WireConnection;101;0;87;0
WireConnection;101;1;100;0
WireConnection;21;0;128;0
WireConnection;21;1;128;1
WireConnection;105;1;107;0
WireConnection;79;1;89;0
WireConnection;410;0;409;0
WireConnection;63;0;62;0
WireConnection;78;0;88;0
WireConnection;78;1;89;0
WireConnection;103;0;101;0
WireConnection;103;1;107;0
WireConnection;16;0;15;0
WireConnection;16;1;21;0
WireConnection;67;0;63;0
WireConnection;104;0;103;0
WireConnection;104;1;105;0
WireConnection;66;0;65;0
WireConnection;66;1;63;0
WireConnection;80;0;78;0
WireConnection;80;1;79;0
WireConnection;106;0;104;0
WireConnection;43;0;16;0
WireConnection;43;1;411;0
WireConnection;69;0;66;0
WireConnection;69;1;67;0
WireConnection;81;0;80;0
WireConnection;158;0;81;0
WireConnection;158;1;106;0
WireConnection;145;0;69;0
WireConnection;145;1;43;0
WireConnection;24;0;23;0
WireConnection;404;1;59;0
WireConnection;404;0;158;0
WireConnection;3;0;2;0
WireConnection;3;1;145;0
WireConnection;159;0;404;0
WireConnection;361;0;3;0
WireConnection;205;0;203;0
WireConnection;205;2;164;0
WireConnection;205;3;9;0
WireConnection;205;4;9;0
WireConnection;163;0;161;0
WireConnection;398;1;55;0
WireConnection;398;0;24;1
WireConnection;162;0;205;0
WireConnection;162;1;9;0
WireConnection;162;2;163;0
WireConnection;406;0;159;1
WireConnection;399;0;398;0
WireConnection;7;0;361;0
WireConnection;7;1;5;0
WireConnection;152;0;144;3
WireConnection;405;0;159;0
WireConnection;8;0;162;0
WireConnection;8;1;4;0
WireConnection;8;2;7;0
WireConnection;8;3;22;0
WireConnection;206;0;207;0
WireConnection;206;2;3;1
WireConnection;206;3;3;4
WireConnection;206;4;3;4
WireConnection;332;0;9;4
WireConnection;113;0;22;4
WireConnection;113;1;332;0
WireConnection;113;2;206;0
WireConnection;113;3;408;0
WireConnection;113;4;72;0
WireConnection;113;5;401;0
WireConnection;113;6;400;0
WireConnection;120;0;153;0
WireConnection;120;1;110;0
WireConnection;120;2;8;0
WireConnection;109;0;120;0
WireConnection;109;1;8;0
WireConnection;109;2;407;0
WireConnection;74;0;113;0
WireConnection;304;0;179;3
WireConnection;446;0;109;0
WireConnection;446;1;74;0
WireConnection;446;2;168;0
WireConnection;453;1;447;0
WireConnection;453;0;448;0
WireConnection;384;0;179;0
WireConnection;300;0;304;0
WireConnection;449;0;453;0
WireConnection;449;1;446;0
WireConnection;288;0;384;0
WireConnection;288;1;300;0
WireConnection;289;0;288;0
WireConnection;112;0;449;0
WireConnection;191;0;184;0
WireConnection;290;0;289;0
WireConnection;290;1;183;0
WireConnection;290;2;191;0
WireConnection;230;0;379;0
WireConnection;230;1;228;0
WireConnection;420;0;112;0
WireConnection;231;0;230;0
WireConnection;187;0;188;0
WireConnection;187;3;290;0
WireConnection;189;0;421;0
WireConnection;189;1;187;0
WireConnection;232;0;231;0
WireConnection;233;0;232;0
WireConnection;419;0;189;0
WireConnection;235;0;233;0
WireConnection;417;1;422;0
WireConnection;417;0;423;0
WireConnection;238;0;235;0
WireConnection;238;1;234;1
WireConnection;238;2;234;2
WireConnection;241;0;235;0
WireConnection;241;1;234;3
WireConnection;241;2;234;4
WireConnection;240;0;236;0
WireConnection;240;1;237;0
WireConnection;240;2;238;0
WireConnection;428;0;417;0
WireConnection;242;0;240;0
WireConnection;242;1;239;0
WireConnection;242;2;241;0
WireConnection;244;0;242;0
WireConnection;244;1;429;0
WireConnection;248;0;429;0
WireConnection;246;0;244;0
WireConnection;246;3;248;0
WireConnection;426;0;246;0
WireConnection;425;1;428;0
WireConnection;425;0;427;0
WireConnection;324;2;325;0
WireConnection;324;0;322;0
WireConnection;324;1;321;0
WireConnection;326;0;324;0
WireConnection;434;0;425;0
WireConnection;387;0;434;0
WireConnection;432;1;438;0
WireConnection;432;0;326;0
WireConnection;393;0;434;0
WireConnection;394;0;387;0
WireConnection;440;0;432;0
WireConnection;435;0;425;0
WireConnection;388;0;393;0
WireConnection;388;1;394;0
WireConnection;391;0;394;0
WireConnection;437;0;435;0
WireConnection;437;1;439;0
WireConnection;390;0;388;0
WireConnection;390;1;391;0
WireConnection;395;1;437;0
WireConnection;229;0;227;0
WireConnection;180;1;179;0
WireConnection;269;0;180;0
WireConnection;261;0;269;0
WireConnection;396;0;390;0
WireConnection;271;0;261;0
WireConnection;271;1;270;0
WireConnection;373;2;396;0
WireConnection;373;3;395;0
ASEEND*/
//CHKSM=B4B19F7BD5B778B563E2786DED7CDD7FB85F750A
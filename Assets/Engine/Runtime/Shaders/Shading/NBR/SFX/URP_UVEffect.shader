// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/SFX/UVEffect"
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
		_MainPannerX("Main Panner X", Float) = 0
		_MainPannerY("Main Panner Y", Float) = 0
		_MainRamp("Main Ramp", 2D) = "white" {}
		[Toggle]_UseMainRamp("Use MainRamp", Range( 0 , 1)) = 0
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

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR


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
			float4 _MainTex_ST;
			float4 _MaskTex_ST;
			float4 _MainColor;
			float4 _BackColor;
			float4 _Color0;
			float4 _Color1;
			float4 _RampParam;
			float4 _Color2;
			float4 _TurbulenceTex_ST;
			float4 _MainRamp_ST;
			float4 _WidthColor;
			float _Usefresnel;
			half _DistortPower;
			float _UseTurbulence;
			float _PowerU;
			half _Contrast;
			float _Flip;
			half _UseClip;
			half _Hardness;
			float _AlphaR;
			half _Alpha;
			float _UseMask;
			half _CUTOUT;
			float _PowerV;
			half _fresnelmultiply;
			half _BlendMode;
			half _Dissolve;
			half _CullMode;
			float _ZTest;
			float _CameraFade;
			float _CameraFadeLength;
			float _CameraFadeOffset;
			float _UseRamp;
			float _UseDepthFade;
			half _EdgeWidth;
			half _ShaderMode;
			float _UseMainRamp;
			float _UseBackColor;
			half _Brightness;
			half _DepthMode;
			float _MainPannerX;
			float _MainPannerY;
			half _fresnelpower;
			float _FadeLength;
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _MainRamp;
			sampler2D _MainTex;
			sampler2D _TurbulenceTex;
			sampler2D _MaskTex;
			uniform float4 _CameraDepthTexture_TexelSize;


						
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 customSurfaceDepth324 = v.vertex.xyz;
				float customEye324 = -TransformWorldToView(TransformObjectToWorld(customSurfaceDepth324)).z;
				o.ase_texcoord3.x = customEye324;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord3.yzw = ase_worldNormal;
				float4 ase_clipPos = TransformObjectToHClip((v.vertex).xyz);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord7 = screenPos;
				
				o.ase_texcoord4 = v.ase_texcoord2;
				o.ase_texcoord5.xy = v.ase_texcoord.xy;
				o.ase_texcoord6 = v.ase_texcoord1;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord5.zw = 0;
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
				float customEye324 = IN.ase_texcoord3.x;
				float cameraDepthFade324 = (( customEye324 -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
				float4 appendResult311 = (float4(1.0 , 1.0 , 1.0 , saturate( cameraDepthFade324 )));
				float3 ase_worldNormal = IN.ase_texcoord3.yzw;
				float dotResult230 = dot( _MainLightPosition.xyz , ase_worldNormal );
				float temp_output_235_0 = ( 1.0 - saturate( ( ( dotResult230 * 0.5 ) + 0.5 ) ) );
				float smoothstepResult238 = smoothstep( _RampParam.x , _RampParam.y , temp_output_235_0);
				float4 lerpResult240 = lerp( _Color0 , _Color1 , smoothstepResult238);
				float smoothstepResult241 = smoothstep( _RampParam.z , _RampParam.w , temp_output_235_0);
				float4 lerpResult242 = lerp( lerpResult240 , _Color2 , smoothstepResult241);
				float4 texCoord61 = IN.ase_texcoord4;
				texCoord61.xy = IN.ase_texcoord4.xy * float2( 1,1 ) + float2( 0,0 );
				float4 appendResult123 = (float4(texCoord61.x , texCoord61.y , texCoord61.z , ( texCoord61.w + 1.0 )));
				float4 appendResult141 = (float4(1.0 , 1.0 , _EdgeWidth , 1.0));
				float4 lerpResult143 = lerp( appendResult123 , appendResult141 , _ShaderMode);
				float4 break144 = lerpResult143;
				half edgebrightness152 = break144.w;
				float2 uv_MainRamp = IN.ase_texcoord5.xy * _MainRamp_ST.xy + _MainRamp_ST.zw;
				float4 temp_cast_0 = (1.0).xxxx;
				float4 temp_cast_1 = (1.0).xxxx;
				float4 ifLocalVar365 = 0;
				UNITY_BRANCH 
				if( _UseMainRamp <= 0.0 )
				ifLocalVar365 = temp_cast_1;
				else
				ifLocalVar365 = tex2D( _MainRamp, uv_MainRamp );
				float4 ifLocalVar205 = 0;
				UNITY_BRANCH 
				if( _UseBackColor <= 0.0 )
				ifLocalVar205 = _MainColor;
				else
				ifLocalVar205 = _BackColor;
				float4 lerpResult162 = lerp( ifLocalVar205 , _MainColor , max( ase_vface , 0.0 ));
				float2 texCoord65 = IN.ase_texcoord5.xy * float2( 1,1 ) + float2( 0,0 );
				float2 appendResult62 = (float2(break144.x , break144.y));
				float2 temp_output_63_0 = ( appendResult62 + float2( -1,-1 ) );
				float2 uv_MainTex = IN.ase_texcoord5.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 texCoord19 = IN.ase_texcoord6;
				texCoord19.xy = IN.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				float4 appendResult129 = (float4(texCoord19.x , texCoord19.y , texCoord19.z , texCoord19.w));
				float2 appendResult380 = (float2(( _MainPannerX * ( _TimeParameters.x ) ) , ( ( _TimeParameters.x ) * _MainPannerY )));
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float fresnelNdotV192 = dot( ase_worldNormal, ase_worldViewDir );
				float fresnelNode192 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV192, _fresnelpower ) );
				float temp_output_196_0 = ( min( fresnelNode192 , 1.0 ) * _fresnelmultiply );
				float lerpResult221 = lerp( temp_output_196_0 , ( 1.0 - temp_output_196_0 ) , _Flip);
				float lerpResult213 = lerp( 1.0 , lerpResult221 , _Usefresnel);
				float fresnel223 = lerpResult213;
				float4 appendResult137 = (float4(frac( appendResult380 ) , ( _Dissolve * max( 1.0 , ( 1.0 - fresnel223 ) ) ) , _DistortPower));
				float4 lerpResult125 = lerp( appendResult129 , appendResult137 , _ShaderMode);
				float4 break128 = lerpResult125;
				float2 appendResult21 = (float2(break128.x , break128.y));
				float2 uv_TurbulenceTex = IN.ase_texcoord5.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				float2 appendResult36 = (float2(_PowerU , _PowerV));
				float4 tex2DNode31 = tex2Dbias( _TurbulenceTex, float4( ( uv_TurbulenceTex + frac( ( appendResult36 * ( _TimeParameters.x ) ) ) ), 0, 4.0) );
				half Distort148 = break128.w;
				float ifLocalVar211 = 0;
				UNITY_BRANCH 
				if( _UseTurbulence <= 0.0 )
				ifLocalVar211 = 0.0;
				else
				ifLocalVar211 = ( ( tex2DNode31.r - 0.5 ) * Distort148 );
				float4 tex2DNode3 = tex2D( _MainTex, ( ( ( texCoord65 * temp_output_63_0 ) + ( temp_output_63_0 * float2( -0.5,-0.5 ) ) ) + ( ( uv_MainTex + appendResult21 ) + ifLocalVar211 ) ) );
				float4 temp_cast_2 = (_Contrast).xxxx;
				float4 temp_output_371_0 = ( ifLocalVar365 * ( lerpResult162 * _Brightness * pow( tex2DNode3 , temp_cast_2 ) * IN.ase_color ) );
				float temp_output_87_0 = ( tex2DNode31.r + 1.0 );
				half dissolve146 = break128.z;
				half edgewidth150 = break144.z;
				float temp_output_116_0 = ( dissolve146 * ( 1.0 + edgewidth150 ) );
				half hardness89 = _Hardness;
				float temp_output_91_0 = ( 1.0 - hardness89 );
				float2 appendResult158 = (float2(saturate( ( ( ( temp_output_87_0 - ( temp_output_116_0 * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) ) , saturate( ( ( ( temp_output_87_0 - ( ( temp_output_116_0 - edgewidth150 ) * ( 1.0 + temp_output_91_0 ) ) ) - hardness89 ) / ( 1.0 - hardness89 ) ) )));
				float2 temp_cast_3 = (1.0).xx;
				float2 temp_cast_4 = (1.0).xx;
				float2 ifLocalVar219 = 0;
				UNITY_BRANCH 
				if( _UseClip <= 0.0 )
				ifLocalVar219 = temp_cast_4;
				else
				ifLocalVar219 = appendResult158;
				float2 break159 = ifLocalVar219;
				float4 lerpResult109 = lerp( ( edgebrightness152 * _WidthColor * temp_output_371_0 ) , temp_output_371_0 , break159.x);
				float ifLocalVar206 = 0;
				UNITY_BRANCH 
				if( _AlphaR <= 0.0 )
				ifLocalVar206 = tex2DNode3.a;
				else
				ifLocalVar206 = tex2DNode3.r;
				float2 uv_MaskTex = IN.ase_texcoord5.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
				float ifLocalVar201 = 0;
				UNITY_BRANCH 
				if( _UseMask <= 0.0 )
				ifLocalVar201 = 1.0;
				else
				ifLocalVar201 = tex2D( _MaskTex, uv_MaskTex ).r;
				float temp_output_74_0 = min( ( IN.ase_color.a * _MainColor.a * ifLocalVar206 * break159.y * _Alpha * ifLocalVar201 * fresnel223 ) , 1.0 );
				float4 appendResult173 = (float4(lerpResult109.rgb , temp_output_74_0));
				clip( temp_output_74_0 - min( _CUTOUT , _DepthMode ));
				float3 appendResult171 = (float3(lerpResult109.rgb));
				float4 appendResult172 = (float4(appendResult171 , 1.0));
				float4 lerpResult170 = lerp( appendResult173 , appendResult172 , _DepthMode);
				float4 appendResult112 = (float4(lerpResult170));
				float4 screenPos = IN.ase_texcoord7;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth384 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( ase_screenPosNorm.xy ),_ZBufferParams);
				float depthToLinear300 = LinearEyeDepth(ase_screenPosNorm.z,_ZBufferParams);
				float smoothstepResult290 = smoothstep( 0.0 , ( _FadeLength * 1.0 ) , abs( ( eyeDepth384 - depthToLinear300 ) ));
				float4 appendResult187 = (float4(float3(1,1,1) , smoothstepResult290));
				float4 ifLocalVar218 = 0;
				UNITY_BRANCH 
				if( _UseDepthFade <= 0.0 )
				ifLocalVar218 = appendResult112;
				else
				ifLocalVar218 = ( appendResult112 * appendResult187 );
				float4 appendResult246 = (float4(( lerpResult242 * ifLocalVar218 ).rgb , (ifLocalVar218).w));
				float4 ifLocalVar243 = 0;
				UNITY_BRANCH 
				if( _UseRamp <= 0.0 )
				ifLocalVar243 = ifLocalVar218;
				else
				ifLocalVar243 = appendResult246;
				float4 ifLocalVar306 = 0;
				UNITY_BRANCH 
				if( _CameraFade <= 0.0 )
				ifLocalVar306 = ifLocalVar243;
				else
				ifLocalVar306 = ( appendResult311 * ifLocalVar243 );
				float grayscale387 = Luminance(ifLocalVar306.xyz);
				float temp_output_394_0 = ( grayscale387 + 1E-07 );
				
				float4 clampResult385 = clamp( ifLocalVar306 , float4( 0,0,0,0 ) , float4( 3,3,3,1 ) );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = max( ( ( (ifLocalVar306).xyz / temp_output_394_0 ) * min( temp_output_394_0 , 4.0 ) ) , float3( 0,0,0 ) );
				float Alpha = max( 0.0 , clampResult385.w );
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

		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBaseT"}

			Blend One One
			ZWrite [_DepthMode]
			ZTest [_ZTest]
			Offset 0 , 0
			ColorMask RGBA
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
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18908
7;139;1920;713;-10975.05;44.27267;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;193;-5123.501,5348.885;Half;False;Property;_fresnelpower;fresnel power;34;0;Create;True;0;0;0;False;0;False;1;1;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;192;-4802.187,5309.627;Inherit;True;Standard;WorldNormal;ViewDir;True;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;222;-4486.761,5421.949;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;197;-4785.764,5543.376;Half;False;Property;_fresnelmultiply;fresnel multiply;35;0;Create;True;0;0;0;False;0;False;1;1;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-4365.357,5463.313;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;215;-4254.723,6752.373;Float;False;Property;_Flip;Flip;36;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;194;-4210.923,5559.855;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;221;-3982.073,5519.442;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-3967.518,5410.327;Half;False;Constant;_fresnelfloat;fresnel float;35;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;214;-3624.771,6725.381;Float;False;Property;_Usefresnel;Use fresnel;33;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;213;-3454.19,5437.253;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-3168.177,5395.674;Float;False;fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-5381.128,-2372.498;Inherit;False;223;fresnel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;133;-5771.044,-2811.033;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;131;-5774.139,-2664.525;Float;False;Property;_MainPannerY;Main Panner Y;18;0;Create;True;0;0;0;False;0;False;0;-0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;130;-5779.785,-2898.14;Float;False;Property;_MainPannerX;Main Panner X;17;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-5463.737,-2752.096;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-5440.771,-2855.696;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;225;-5134.129,-2370.498;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-5241.597,-4060.484;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;160;-4944.978,-3907.721;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;140;-5218.054,-3851.618;Half;False;Constant;_UVRpeat;UVRpeat;24;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;249;-4964.219,-2396.431;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-5261.932,-2505.149;Half;False;Property;_Dissolve;Dissolve;27;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-5244.495,-3770.627;Half;False;Property;_EdgeWidth;EdgeWidth;29;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;380;-5253.946,-2809.976;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-5952.93,159.387;Float;False;Property;_PowerU;Power U;23;0;Create;True;0;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;381;-4989.854,-2815.781;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-5250.481,-3086.187;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;136;-5229.244,-2595.718;Half;False;Property;_DistortPower;Distort Power;22;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;141;-4772.249,-3876.906;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;123;-4773.25,-4033.156;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;126;-4784.096,-5467.187;Half;False;Property;_ShaderMode;Shader Mode;0;1;[Enum];Create;True;0;2;Custom Data;0;Material;1;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;-4805.129,-2540.421;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-5953.186,246.5723;Float;False;Property;_PowerV;Power V;24;0;Create;True;0;0;0;False;0;False;0;-0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;36;-5693.157,199.5769;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;137;-4626.984,-2816.639;Inherit;False;FLOAT4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;129;-4735.152,-3067.848;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TimeNode;37;-5967.869,333.6686;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;143;-4331.201,-4064.386;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;125;-4340.04,-2893.839;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.BreakToComponentsNode;144;-3993.96,-4110.063;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-5497.867,305.3135;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FractNode;382;-5331.656,322.1559;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;150;-3617.019,-4036.802;Half;False;edgewidth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;39;-5547.924,120.4628;Inherit;False;0;30;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;128;-4182.001,-2889.474;Inherit;True;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;50;-3707.357,2615.103;Half;False;Property;_Hardness;Hardness;26;0;Create;True;0;0;0;False;0;False;0;0;0;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-5146.271,303.1464;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;89;-3411.859,2601.792;Half;False;hardness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;30;-5304.034,-25.88846;Float;True;Property;_TurbulenceTex;Turbulence Tex;21;0;Create;True;0;0;0;False;0;False;None;9e0d900637f45f84bb84880c7f35bad7;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;146;-3848.756,-2609.61;Half;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;147;-5417.657,2846.17;Inherit;False;150;edgewidth;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;383;-5100.874,531.8093;Inherit;False;Constant;_Float0;Float 0;45;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;31;-4875.157,290.5444;Inherit;True;Property;_TextureSample2;Texture Sample 2;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;MipBias;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;115;-5137.038,2752.854;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;151;-5246.007,2634.827;Inherit;False;146;dissolve;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;148;-3898.515,-2436.378;Half;False;Distort;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;90;-5244.417,2989.488;Inherit;False;89;hardness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;71;-3962.547,137.3129;Inherit;False;2;0;FLOAT;0.5;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-3228.454,134.6442;Inherit;False;148;Distort;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;116;-4984.041,2633.796;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;91;-4997.269,3004.299;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;62;-3182.257,-3448.918;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;-4811.384,2691.091;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-3139.467,-2821.08;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-4729.949,2973.708;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-3245.881,230.9476;Half;False;Constant;_turbulencefloat;turbulence float;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;65;-3163.488,-3702.854;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;114;-4681.88,2836.106;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-2884.652,-3304.235;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-2997.701,21.61698;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;209;-3135.715,-351.7894;Float;False;Property;_UseTurbulence;Use Turbulence;6;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-3188.212,-3049.438;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-4501.1,2832.996;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-2638.228,-2832.969;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;-2679.293,-3161.985;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-0.5,-0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ConditionalIfNode;211;-2676.709,-27.37058;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-4068.476,1080.786;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-4608.512,2625.439;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-2697.325,-3481.545;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;88;-3812.913,2384.426;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-2411.713,-3383.862;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;101;-3957.24,2792.299;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;107;-3396.664,2874.25;Inherit;False;89;hardness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-2359.087,-2747.943;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-2136.838,-2967.509;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;103;-3087.364,2781.072;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;79;-3121.396,2608.254;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1926.7,-2852.995;Float;True;Property;_MainTex;Main Tex;15;0;Create;True;0;0;0;False;0;False;None;466436ddca156c8449e277e0f034e2b4;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleSubtractOpNode;105;-3092.25,2876.646;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;78;-3126.22,2511.688;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;161;214.9813,-1909.477;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-153.2925,-2030.688;Float;False;Property;_MainColor;Main Color;12;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,0.4471698,0.4471698,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;203;-161.2018,-2435.239;Float;False;Property;_UseBackColor;Use BackColor;13;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-1709.973,-335.5461;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;80;-2964.155,2550.868;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;164;-158.0261,-2234.258;Float;False;Property;_BackColor;BackColor;14;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;104;-2924.73,2797.497;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;361;-925.9382,-942.5132;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;81;-2799.67,2553.029;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-522.5835,-990.1956;Half;False;Property;_Contrast;Contrast;11;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;163;328.0681,-1913.001;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;205;183.6137,-2236.896;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;106;-2792.228,2768.036;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;162;496.5851,-2024.007;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;362;1061.861,-2328.583;Float;False;Property;_UseMainRamp;Use MainRamp;20;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;7;-153.4279,-1191.416;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;220;-2488.164,1001.835;Half;False;Property;_UseClip;Use Clip;7;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;22;813.9332,-1026.046;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;23;-4539.725,4871.868;Float;True;Property;_MaskTex;Mask Tex;25;0;Create;True;0;0;0;False;0;False;None;f220a11932877034a9b8b42ca79ce89a;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;363;729.6223,-2186.907;Inherit;True;Property;_MainRamp;Main Ramp;19;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;158;-2470.056,2580.354;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;366;1323.043,-2000.146;Half;False;Constant;_rampfloat;ramp float;14;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-2480.272,2753.818;Half;False;Constant;_Clipfloat;Clip float;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;682.702,-1571.411;Half;False;Property;_Brightness;Brightness;10;0;Create;True;0;0;0;False;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;55;-3941.431,5043.284;Half;False;Constant;_Maskfloat;Mask float;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;365;1715.64,-2328.854;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-3822.187,4613.464;Float;False;Property;_UseMask;UseMask;8;1;[Toggle];Create;True;0;0;0;True;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;3047.172,1298.192;Half;False;Property;_DepthMode;Depth Mode;5;1;[Enum];Create;True;0;2;On;1;Off;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;207;-766.5992,-418.3558;Float;False;Property;_AlphaR;Alpha R;16;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;24;-4270.186,4871.056;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;219;-2277.287,2514.684;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;152;-3633.242,-3959.565;Half;False;edgebrightness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;1012.882,-1449.162;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;110;2417.442,-1819.534;Float;False;Property;_WidthColor;WidthColor;28;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;332;287.842,-712.9236;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;371;1890.693,-1792.095;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;330;3087.913,808.1866;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;153;2465.761,-2031.186;Inherit;False;152;edgebrightness;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;563.5672,434.233;Half;False;Property;_Alpha;Alpha;30;0;Create;True;0;0;0;False;0;False;1;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;201;-3444.109,4777.581;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;159;-1969.337,2526.613;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ConditionalIfNode;206;-268.3388,-387.8521;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;179;4088.851,-922.0974;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;2909.813,-1432.947;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;113;1690.135,80.70615;Inherit;False;7;7;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;168;2331.41,316.2896;Half;False;Property;_CUTOUT;CUTOUT;9;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;228;6882.054,-641.3204;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;331;2634.673,661.4191;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;379;6819.906,-1035.106;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;304;4429.521,-698.8807;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;176;2612.745,316.8277;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;74;2017.75,70.57481;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;109;3070.523,-1050.978;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;230;7313.054,-732.3205;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClipNode;167;2747.032,222.6688;Inherit;False;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;231;7453.488,-732.1445;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;384;5271.152,-1110.182;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LinearDepthNode;300;5233.535,-688.3963;Inherit;False;0;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;288;5486.202,-928.0206;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;171;2955.294,226.6826;Inherit;False;FLOAT3;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;184;5399.528,-586.1086;Float;False;Property;_FadeLength;FadeLength;32;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;232;7595.329,-735.6905;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;233;7717.142,-734.5474;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;177;3240.122,943.6703;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;289;5737.073,-913.4349;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;5686.459,-672.3974;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;173;3109.092,35.35209;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;172;3112.327,228.029;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;5693.913,-581.0256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;290;5893.159,-751.0715;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;188;5896.135,-936.5684;Float;False;Constant;_Vector0;Vector 0;29;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;170;3317.38,206.9465;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector4Node;234;7660.754,-595.7471;Float;False;Property;_RampParam;RampParam;41;0;Create;True;0;0;0;False;0;False;0.3,0.33,0.6,0.66;0.3,0.33,0.6,0.66;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;235;7864.828,-735.6905;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;112;3571.838,193.5905;Inherit;True;FLOAT4;4;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;187;6098.501,-778.2789;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SmoothstepOpNode;238;8160.856,-821.9542;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;237;8163.931,-1013.168;Float;False;Property;_Color1;Color 1;39;0;Create;True;0;0;0;False;0;False;0,1,0.03857636,0;0,1,0.03857636,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;236;8166.419,-1175.822;Float;False;Property;_Color0;Color 0;38;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;239;8139.43,-683.2342;Float;False;Property;_Color2;Color 2;40;0;Create;True;0;0;0;False;0;False;0,0.4486432,1,0;0,0.4486431,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;217;6373.386,-1559.362;Float;False;Property;_UseDepthFade;UseDepthFade;31;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;240;8493.054,-1029.257;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;241;8140.346,-520.473;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.2;False;2;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;189;6082.519,198.8894;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ConditionalIfNode;218;6562.126,241.2121;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;242;8773.04,-720.1902;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;244;9016.319,-697.733;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;321;9999.81,-386.0604;Float;False;Property;_CameraFadeOffset;CameraFadeOffset;43;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;325;10027.78,-649.3117;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;322;10006.54,-487.1401;Float;False;Property;_CameraFadeLength;CameraFadeLength;44;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;248;8972.466,-563.819;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;9345.604,-1508.701;Float;False;Property;_UseRamp;UseRamp;37;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;324;10258.72,-496.5569;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;246;9248.608,-622.4665;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;326;10510.69,-484.3758;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;243;9770.79,184.0759;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;311;10683.31,-442.3817;Inherit;False;FLOAT4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WireNode;329;10326.34,-42.64247;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;305;10819.42,-1538.883;Float;False;Property;_CameraFade;CameraFade;42;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;309;10852.35,-337.5297;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ConditionalIfNode;306;11277.53,147.1773;Inherit;False;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TFHCGrayscale;387;11482.39,48.28217;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;394;11670.99,30.94705;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1E-07;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;393;11476.73,-107.2371;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;385;11614.01,555.5079;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;3,3,3,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMinOpNode;391;11750.39,271.2822;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;388;11729.39,-114.7178;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;377;11780.33,561.8634;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;390;11953.39,108.2822;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;395;11989.05,563.7273;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;261;4950.82,-948.3329;Float;False; return dot(enc,float3(1.0, 1 / 255.0, 1 / 65025.0))@;1;Create;1;True;enc;FLOAT3;0,0,0;In;;Float;False;My Custom Expression;True;False;0;;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;227;6878.962,-802.7894;Float;False;Global;_MainLightDir0;_MainLightDir0;0;0;Create;True;0;0;0;False;0;False;0,0,0,0;-0.09124176,0.8149841,0.572255,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;270;4946.432,-837.18;Float;False;Global;_ZBufferParams;_ZBufferParams;40;0;Fetch;True;0;0;0;False;0;False;0,0,0,0;-4,1,-0.1333333,0.03333333;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;-6370.131,334.8212;Half;False;Property;_BlendMode;Blend Mode;1;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;229;7093.054,-801.3204;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;180;4402.508,-951.4051;Inherit;True;Global;_CameraDepthRT;_CameraDepthRT;2;1;[HDR];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-6356.948,240.4752;Half;False;Property;_CullMode;Cull Mode;3;1;[Enum];Create;True;0;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;271;5208.19,-945.4847;Float;False;z*=zBufferParams.z@$return 1/(z+zBufferParams.w)@;1;Create;2;True;z;FLOAT;0;In;;Float;False;True;zBufferParams;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;True;False;0;;False;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;-6354.122,55.75672;Float;False;Property;_ZTest;ZTest;4;1;[Enum];Create;True;0;1;Option1;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;269;4729.433,-949.1151;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;396;12153.05,117.7273;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;373;12367.77,115.9551;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;URP/SFX/UVEffect;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;ForwardBase;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;True;True;0;True;18;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;0;0;True;True;2;5;False;-1;10;True;70;0;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;True;2;True;75;True;3;True;178;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;22;Surface;1;  Blend;2;Two Sided;1;Cast Shadows;0;  Use Shadow Threshold;0;Receive Shadows;0;GPU Instancing;1;LOD CrossFade;0;Built-in Fog;0;DOTS Instancing;0;Meta Pass;0;Extra Pre Pass;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Vertex Position,InvertActionOnDeselection;1;0;5;False;True;False;False;False;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;374;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;375;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;372;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;376;11477.24,158.1968;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;192;3;193;0
WireConnection;222;0;192;0
WireConnection;196;0;222;0
WireConnection;196;1;197;0
WireConnection;194;0;196;0
WireConnection;221;0;196;0
WireConnection;221;1;194;0
WireConnection;221;2;215;0
WireConnection;213;0;200;0
WireConnection;213;1;221;0
WireConnection;213;2;214;0
WireConnection;223;0;213;0
WireConnection;139;0;133;2
WireConnection;139;1;131;0
WireConnection;138;0;130;0
WireConnection;138;1;133;2
WireConnection;225;0;224;0
WireConnection;160;0;61;4
WireConnection;249;1;225;0
WireConnection;380;0;138;0
WireConnection;380;1;139;0
WireConnection;381;0;380;0
WireConnection;141;0;140;0
WireConnection;141;1;140;0
WireConnection;141;2;98;0
WireConnection;141;3;140;0
WireConnection;123;0;61;1
WireConnection;123;1;61;2
WireConnection;123;2;61;3
WireConnection;123;3;160;0
WireConnection;226;0;82;0
WireConnection;226;1;249;0
WireConnection;36;0;33;0
WireConnection;36;1;35;0
WireConnection;137;0;381;0
WireConnection;137;2;226;0
WireConnection;137;3;136;0
WireConnection;129;0;19;1
WireConnection;129;1;19;2
WireConnection;129;2;19;3
WireConnection;129;3;19;4
WireConnection;143;0;123;0
WireConnection;143;1;141;0
WireConnection;143;2;126;0
WireConnection;125;0;129;0
WireConnection;125;1;137;0
WireConnection;125;2;126;0
WireConnection;144;0;143;0
WireConnection;38;0;36;0
WireConnection;38;1;37;2
WireConnection;382;0;38;0
WireConnection;150;0;144;2
WireConnection;128;0;125;0
WireConnection;40;0;39;0
WireConnection;40;1;382;0
WireConnection;89;0;50;0
WireConnection;146;0;128;2
WireConnection;31;0;30;0
WireConnection;31;1;40;0
WireConnection;31;2;383;0
WireConnection;115;1;147;0
WireConnection;148;0;128;3
WireConnection;71;0;31;1
WireConnection;116;0;151;0
WireConnection;116;1;115;0
WireConnection;91;0;90;0
WireConnection;62;0;144;0
WireConnection;62;1;144;1
WireConnection;92;1;91;0
WireConnection;21;0;128;0
WireConnection;21;1;128;1
WireConnection;99;1;91;0
WireConnection;114;0;116;0
WireConnection;114;1;147;0
WireConnection;63;0;62;0
WireConnection;41;0;71;0
WireConnection;41;1;149;0
WireConnection;100;0;114;0
WireConnection;100;1;99;0
WireConnection;16;0;15;0
WireConnection;16;1;21;0
WireConnection;67;0;63;0
WireConnection;211;0;209;0
WireConnection;211;2;41;0
WireConnection;211;3;57;0
WireConnection;211;4;57;0
WireConnection;87;0;31;1
WireConnection;83;0;116;0
WireConnection;83;1;92;0
WireConnection;66;0;65;0
WireConnection;66;1;63;0
WireConnection;88;0;87;0
WireConnection;88;1;83;0
WireConnection;69;0;66;0
WireConnection;69;1;67;0
WireConnection;101;0;87;0
WireConnection;101;1;100;0
WireConnection;43;0;16;0
WireConnection;43;1;211;0
WireConnection;145;0;69;0
WireConnection;145;1;43;0
WireConnection;103;0;101;0
WireConnection;103;1;107;0
WireConnection;79;1;89;0
WireConnection;105;1;107;0
WireConnection;78;0;88;0
WireConnection;78;1;89;0
WireConnection;3;0;2;0
WireConnection;3;1;145;0
WireConnection;80;0;78;0
WireConnection;80;1;79;0
WireConnection;104;0;103;0
WireConnection;104;1;105;0
WireConnection;361;0;3;0
WireConnection;81;0;80;0
WireConnection;163;0;161;0
WireConnection;205;0;203;0
WireConnection;205;2;164;0
WireConnection;205;3;9;0
WireConnection;205;4;9;0
WireConnection;106;0;104;0
WireConnection;162;0;205;0
WireConnection;162;1;9;0
WireConnection;162;2;163;0
WireConnection;7;0;361;0
WireConnection;7;1;5;0
WireConnection;158;0;81;0
WireConnection;158;1;106;0
WireConnection;365;0;362;0
WireConnection;365;2;363;0
WireConnection;365;3;366;0
WireConnection;365;4;366;0
WireConnection;24;0;23;0
WireConnection;219;0;220;0
WireConnection;219;2;158;0
WireConnection;219;3;59;0
WireConnection;219;4;59;0
WireConnection;152;0;144;3
WireConnection;8;0;162;0
WireConnection;8;1;4;0
WireConnection;8;2;7;0
WireConnection;8;3;22;0
WireConnection;332;0;9;4
WireConnection;371;0;365;0
WireConnection;371;1;8;0
WireConnection;330;0;75;0
WireConnection;201;0;202;0
WireConnection;201;2;24;1
WireConnection;201;3;55;0
WireConnection;201;4;55;0
WireConnection;159;0;219;0
WireConnection;206;0;207;0
WireConnection;206;2;3;1
WireConnection;206;3;3;4
WireConnection;206;4;3;4
WireConnection;120;0;153;0
WireConnection;120;1;110;0
WireConnection;120;2;371;0
WireConnection;113;0;22;4
WireConnection;113;1;332;0
WireConnection;113;2;206;0
WireConnection;113;3;159;1
WireConnection;113;4;72;0
WireConnection;113;5;201;0
WireConnection;113;6;223;0
WireConnection;331;0;330;0
WireConnection;304;0;179;3
WireConnection;176;0;168;0
WireConnection;176;1;331;0
WireConnection;74;0;113;0
WireConnection;109;0;120;0
WireConnection;109;1;371;0
WireConnection;109;2;159;0
WireConnection;230;0;379;0
WireConnection;230;1;228;0
WireConnection;167;0;109;0
WireConnection;167;1;74;0
WireConnection;167;2;176;0
WireConnection;231;0;230;0
WireConnection;384;0;179;0
WireConnection;300;0;304;0
WireConnection;288;0;384;0
WireConnection;288;1;300;0
WireConnection;171;0;167;0
WireConnection;232;0;231;0
WireConnection;233;0;232;0
WireConnection;177;0;75;0
WireConnection;289;0;288;0
WireConnection;173;0;109;0
WireConnection;173;3;74;0
WireConnection;172;0;171;0
WireConnection;191;0;184;0
WireConnection;290;0;289;0
WireConnection;290;1;183;0
WireConnection;290;2;191;0
WireConnection;170;0;173;0
WireConnection;170;1;172;0
WireConnection;170;2;177;0
WireConnection;235;0;233;0
WireConnection;112;0;170;0
WireConnection;187;0;188;0
WireConnection;187;3;290;0
WireConnection;238;0;235;0
WireConnection;238;1;234;1
WireConnection;238;2;234;2
WireConnection;240;0;236;0
WireConnection;240;1;237;0
WireConnection;240;2;238;0
WireConnection;241;0;235;0
WireConnection;241;1;234;3
WireConnection;241;2;234;4
WireConnection;189;0;112;0
WireConnection;189;1;187;0
WireConnection;218;0;217;0
WireConnection;218;2;189;0
WireConnection;218;3;112;0
WireConnection;218;4;112;0
WireConnection;242;0;240;0
WireConnection;242;1;239;0
WireConnection;242;2;241;0
WireConnection;244;0;242;0
WireConnection;244;1;218;0
WireConnection;248;0;218;0
WireConnection;324;2;325;0
WireConnection;324;0;322;0
WireConnection;324;1;321;0
WireConnection;246;0;244;0
WireConnection;246;3;248;0
WireConnection;326;0;324;0
WireConnection;243;0;245;0
WireConnection;243;2;246;0
WireConnection;243;3;218;0
WireConnection;243;4;218;0
WireConnection;311;3;326;0
WireConnection;329;0;243;0
WireConnection;309;0;311;0
WireConnection;309;1;329;0
WireConnection;306;0;305;0
WireConnection;306;2;309;0
WireConnection;306;3;243;0
WireConnection;306;4;243;0
WireConnection;387;0;306;0
WireConnection;394;0;387;0
WireConnection;393;0;306;0
WireConnection;385;0;306;0
WireConnection;391;0;394;0
WireConnection;388;0;393;0
WireConnection;388;1;394;0
WireConnection;377;0;385;0
WireConnection;390;0;388;0
WireConnection;390;1;391;0
WireConnection;395;1;377;3
WireConnection;261;0;269;0
WireConnection;229;0;227;0
WireConnection;180;1;179;0
WireConnection;271;0;261;0
WireConnection;271;1;270;0
WireConnection;269;0;180;0
WireConnection;396;0;390;0
WireConnection;373;2;396;0
WireConnection;373;3;395;0
ASEEND*/
//CHKSM=9A647B7D0889A0DC0A615B15CE754154F7EED840
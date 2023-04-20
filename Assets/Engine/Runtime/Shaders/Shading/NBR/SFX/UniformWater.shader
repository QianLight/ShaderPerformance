// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "URP/Scene/UniformWater"
{
	Properties
	{
		[Header(Albedo)]
		_DeepNear("近景颜色", Color) = (0.1529412,0.7058824,0.6588235,1)
		_DeepMid("中景颜色", Color) = (0.11,0.45,0.55,1)
		_DeepFar("远景颜色", Color) = (0.07058824,0.2078432,0.4431373,1)
		_DistanceFade("x近水距离,y近水羽化，z中水距离，w中水羽化", Vector) = (30,5,150,20)
	//	_Transparency("不透明度", Range( 0 , 2)) = 0.15
		[NoScaleOffset]_DepthTex("深度图(仅中低配使用，黑浅白深)", 2D) = "white" {}
		
		[Header(Shadow)]
		[Toggle]_EnableRealtimeShadow("开启实时阴影", Int) = 1
		[Toggle]_EnableSmartShadow("开启SmartShadow", Int) = 1
		[Toggle]_EnableCloudShadow("开启云阴影", Int) = 1
		_ShadowIntensity("阴影强度", Range( 0 , 1)) = 0.6
		[Toggle]_ApplyFog("接受雾", Int) = 1
		
		[Header(Wave)]
		_Height("波纹1高度", Range( 0 , 2)) = 0
		_Wave("波纹1(xy方向，zw周期)", Vector) = (0,1,0,0)
		_Height1("波纹2高度", Range( 0 , 2)) = 0
		_Wave1("波纹2(xy方向，zw周期)", Vector) = (0,1,0,0)
		[Enum(LocalXY,0,LocalXZ,1,LocalYZ,2,World,3)]_UseWorldPosAsUV("使用什么坐标作为波纹uv", Float) = 3
		
		[Header(Normal)]
		_NormalTex("NormalTex", 2D) = "bump" {}
		_NormalSpeed("NormalSpeed", Vector) = (0,0,0,0)
		
		[Header(Specular)]
		[HDR]_Specular("高光颜色", Color) = (1,1,1,0)
		_SpecularPower("高光指数", Range( 10 , 5000)) = 20
		_NormalScale("高光法线强度", Range( 0 , 1)) = 0.1
		
		[Header(Reflection)]
		_EnvCube("环境盒", CUBE) = "black" {}
		[HDR]_ReflectionColor("近景反射颜色", Color) = (0.75,0.75,0.75,0)
		[HDR]_ReflectionColorMid("中景反射颜色", Color) = (0.75,0.75,0.75,0)
		[HDR]_ReflectionColorFar("远景反射颜色", Color) = (0.75,0.75,0.75,0)
		_NormalScale2("反射法线强度", Range( 0 , 1)) = 0.1
		
		[HDR]_FresnelColor("菲涅尔颜色", Color) = (0.172549,0.7803922,0.9019608,1)
		_FresnelDist("菲涅尔x距离,y羽化,z指数,w法线", Vector) = (200,100,2,0.1)

		[Header(Scatter)]
		_DiffuseGrazing("SSS颜色", Color) = (0.172549,0.7803922,0.9019608,1)
			
		[Header(Foam)]
		_FoamTex("白沫贴图", 2D) = "white" {}
		[HDR]_FoamColor("白沫颜色", Color) = (1,1,1,0)
		_FoamParam("x深度,y强度,z软化,w速度", Vector) = (2,1,3,0.2)

	}

	SubShader
	{
		LOD 0
		
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
		
		Cull Back
		AlphaToMask Off
		HLSLINCLUDE
		#pragma target 2.0
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" "Queue"="Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile_instancing
			// #pragma multi_compile_fog
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS

			#if defined(_FX_LEVEL_HIGH)
				#define REQUIRE_DEPTH_TEXTURE 1
				#define USE_DEPTH_TEX
				#define CALC_VERTEX_NORMAL
            #elif defined(_FX_LEVEL_MEDIUM)
                #define CALC_VERTEX_NORMAL
            #else
				#define IGNORE_SHADOW
			#endif
			
			#define MAIN_LIGHT_CALCULATE_SHADOWS
			#define _SMARTSOFTSHADOW_ON 1
		//	#define LIGHTMAP_SHADOW_MIXING
		//	#define _MIXED_LIGHTING_SUBTRACTIVE

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
			
			#pragma vertex vert
			#pragma fragment frag
						
			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 uv : TEXCOORD1;
				
				float4 normalHeight : TEXCOORD2;//xyz是Gastner计算的法线,w是改变的高度
				
				float4 normal : TEXCOORD3;
				float4 tangent : TEXCOORD4;
				float4 bitangent : TEXCOORD5;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DeepNear;
			float4 _DeepMid;
			float4 _DeepFar;
			float4 _DistanceFade;
		//	float _Transparency;
			
			float _EnableRealtimeShadow;
			float _EnableSmartShadow;
			float _EnableCloudShadow;
			float _ShadowIntensity;
			float _ApplyFog;

			float4 _Wave;
			float _Height;
			float4 _Wave1;
			float _Height1;
			float _UseWorldPosAsUV;

			float4 _NormalTex_ST;
			float4 _NormalSpeed;
			
			float4 _Specular;
			float _SpecularPower;
			float _NormalScale;
			
			float4 _ReflectionColor;
			float4 _ReflectionColorMid;
			float4 _ReflectionColorFar;
			float _NormalScale2;

			float4 _FresnelColor;
			float4 _FresnelDist;
			
			float4 _DiffuseGrazing;
			
			float4 _FoamTex_ST;
			float4 _FoamParam;
			float4 _FoamColor;
			
			CBUFFER_END
			
			uniform float4 _CameraDepthTexture_TexelSize;
			samplerCUBE _EnvCube;
			sampler2D _NormalTex;
			sampler2D _DepthTex;
			sampler2D _FoamTex;
			float4 _WaterLightDir;//静态


			float FastGestner( float2 uv, float4 Wave, float Height )
			{
				float v = sin((uv.x * Wave.z * 1.41 + Wave.x * _Time.y) * 4.9739 )
				* cos((uv.y  * Wave.w * 1.19 + Wave.y * _Time.y) * 7.1391) 
				+ cos((uv.x * Wave.z * 1.33 + Wave.x * _Time.y) * 5.6727 ) 
				* sin((uv.y * Wave.w * 1.25 + Wave.y * _Time.y)* 6.2173 );
				return v * Height;
			}
		
			float3x3 Inverse3x3(float3x3 input)
			{
				float3 a = input._11_21_31;
				float3 b = input._12_22_32;
				float3 c = input._13_23_33;
				return float3x3(cross(b,c), cross(c,a), cross(a,b)) * (1.0 / dot(a,cross(b,c)));
			}
			
			
			VertexOutput VertexFunction ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				//使用世界坐标当uv还是模型坐标当uv
				float3 srcWorldPos = TransformObjectToWorld( v.vertex.xyz);
				float2 worldPos2UV = 0;
				UNITY_BRANCH
				if(_UseWorldPosAsUV == 0){
					worldPos2UV = v.vertex.xy;
				}else if(_UseWorldPosAsUV == 1){
					worldPos2UV = v.vertex.xz;
				}else if(_UseWorldPosAsUV == 2){
					worldPos2UV = v.vertex.yz;
				}else{
					worldPos2UV = srcWorldPos.xz;
				}
	
				float deltaHeight = FastGestner( worldPos2UV , _Wave , _Height ) + FastGestner( worldPos2UV , _Wave1 , _Height1 );
				//顶点法线一并计算了。用十字叉乘
				//顶点法线用于计算贴图法线，以及菲涅尔
				#ifdef CALC_VERTEX_NORMAL
					float offset = 0.5;
					float heightRight = FastGestner( worldPos2UV + float2(offset,0), _Wave , _Height ) + FastGestner( worldPos2UV + float2(offset,0), _Wave1 , _Height1 );
					float heightUp = FastGestner( worldPos2UV + float2(0,offset), _Wave , _Height ) + FastGestner( worldPos2UV + float2(0,offset), _Wave1 , _Height1 );
					float3 newNormal = cross(float3(0, heightUp - deltaHeight, offset), float3(offset, heightRight - deltaHeight,0));
					float viewDist = length(srcWorldPos - _WorldSpaceCameraPos);
					float fade = saturate(2 - viewDist / 50);// 因为模型远处顶点数不够，所以50米外顶点法线不再生效
					newNormal =  lerp(float3(0,1,0), newNormal, fade); 
					newNormal = normalize(newNormal);
					deltaHeight = lerp(0, deltaHeight, fade);
				#else
					float3 newNormal = float3(0,1,0);
				#endif
				o.normalHeight = float4(newNormal, deltaHeight);
				
				o.normal = float4(TransformObjectToWorldNormal(v.normal), 0);
				o.tangent = float4(TransformObjectToWorldDir(v.tangent.xyz), 0);
				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				o.bitangent.xyz = cross(o.normal.xyz, o.tangent.xyz) * tangentSign;

				float3 positionWS = srcWorldPos + float3(0,1,0) * deltaHeight;
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;
				o.clipPos = positionCS;
				
				o.uv = v.uv;
				return o;
			}

			
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				//基本参数
				float3 worldPosition = IN.worldPos;
				float3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz - worldPosition );
				float3 lightDir = _WaterLightDir.xyz;
				
				float3 H = normalize( lightDir.xyz + worldViewDir );

				float4 clipPos = TransformWorldToHClip(IN.worldPos);
				float4 screenPos = ComputeScreenPos(clipPos);
				float2 screenUV = screenPos.xy / screenPos.w;
				
				//固有色
				float EyeDist = distance( worldPosition , _WorldSpaceCameraPos );
				float4 albedo = lerp(_DeepNear, _DeepMid, saturate((EyeDist - _DistanceFade.x - _DistanceFade.y) / (2 * _DistanceFade.y)));
				albedo = lerp(albedo, _DeepFar, saturate((EyeDist - _DistanceFade.z - _DistanceFade.w) / (2 * _DistanceFade.w)));
				float3 refColor = lerp(_ReflectionColor, _ReflectionColorMid, saturate((EyeDist - _DistanceFade.x - _DistanceFade.y) / (2 * _DistanceFade.y)));
				refColor = lerp(refColor, _ReflectionColorFar, saturate((EyeDist - _DistanceFade.z - _DistanceFade.w) / (2 * _DistanceFade.w)));
				
				//深度
				#ifdef USE_DEPTH_TEX
					float eyeDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( screenUV, 0.0 , 0.0 ).xy ),_ZBufferParams);
					float deltaDepth = ( eyeDepth - clipPos.w );
				#else
					float deltaDepth = tex2D( _DepthTex, IN.uv.xy).r * 100;
				#endif

				//法线贴图
				float2 uv_NormalTex = IN.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw;
				float3 tangentNormal = BlendNormal( UnpackNormal( tex2D( _NormalTex, ( uv_NormalTex + _NormalSpeed.xy * _TimeParameters.x ) )) , UnpackNormal( tex2D( _NormalTex, uv_NormalTex + _NormalSpeed.zw * _TimeParameters.x  ) ) );

				float3 worldTangent = IN.tangent.xyz;
				float3 worldNormal = IN.normalHeight.xyz;// IN.normal.xyz;
				float3 worldBitangent = IN.bitangent.xyz;
				float3 tanToWorld0 = float3( worldTangent.x, worldBitangent.x, worldNormal.x );
				float3 tanToWorld1 = float3( worldTangent.y, worldBitangent.y, worldNormal.y );
				float3 tanToWorld2 = float3( worldTangent.z, worldBitangent.z, worldNormal.z );

				float3x3 worldToTangent = float3x3(worldTangent,worldBitangent,worldNormal);
				float3x3 tangentToWorld = Inverse3x3( worldToTangent );
				
				//高光考虑顶点法线，否则高光太死
				//环境反射不考虑顶点法线，不然太油
				//高光
				float3 normalForSpec = lerp( float3(0,0,1) , tangentNormal , _NormalScale);
				normalForSpec = mul( tangentToWorld, normalForSpec );
				float NdotH = dot( H , normalForSpec );
				float3 specular = pow( saturate( NdotH ) , _SpecularPower ) * _Specular.rgb * _MainLightColor.rgb;

				//环境反射
				float3 normalForRefl = lerp(IN.normal.xyz, tangentNormal.xzy , _NormalScale2);
				float3 worldRefDir = normalize( reflect( -worldViewDir, normalForRefl));
				float3 reflection = texCUBE( _EnvCube, worldRefDir).rgb * refColor;
				
				//菲涅尔
				float3 normalForFresnel = lerp(IN.normalHeight.xyz,  mul( tangentToWorld, tangentNormal), _FresnelDist.w);
				float3 fresnel = saturate(pow(1 - dot(normalize(normalForFresnel), worldViewDir), 4) ) * _FresnelColor.rgb;
				fresnel *= pow(saturate((_FresnelDist.x - EyeDist) / (abs(_FresnelDist.y) + 0.01)), _FresnelDist.z);//远景不要叠加菲涅尔，会曝

	
				//漫反射
				float3 diffuse = albedo.rgb * _MainLightColor.rgb;
				
				//散射
				float3 scatter = _DiffuseGrazing.rgb * saturate( IN.normalHeight.w);
				
				
				float shadow = 1;
		
				//实时阴影
				#ifdef _MAIN_LIGHT_SHADOWS
				UNITY_BRANCH
				if(_EnableRealtimeShadow > 0){
					float4 ShadowCoords = TransformWorldToShadowCoord( worldPosition );
					half realtimeShadow = saturate(MainLightRealtimeShadow(ShadowCoords));
					half shadowFade = GetShadowFade(worldPosition);
					shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				}
				#endif
				
				//SmartShadow阴影
				UNITY_BRANCH
				if(_EnableSmartShadow > 0){
					float smartShadow = GetSmartShadow(_MainLightPosition.xyz, IN.normalHeight.xyz, float4(worldPosition,1), _SmartShadowIntensity);
					shadow = min(shadow, smartShadow);
				}
				
				//云阴影
				UNITY_BRANCH
				if(_EnableCloudShadow > 0){
					float cloud = CloudShadowColor(worldPosition);
					shadow = min(cloud, shadow);//zh
				}
				
				shadow = lerp(1, shadow, _ShadowIntensity);
				
				float3 Color = (diffuse + specular) * shadow + reflection + fresnel + scatter;
			//	float Alpha = 0.005 + saturate( albedo.a * ( deltaDepth * albedo.a - 0.05));	//近处1，中处0.5，远处0.1
				float Alpha = saturate((deltaDepth - 0.05) / ((1 - albedo.a) * (1 - albedo.a) * 3000 + 0.01));
				
				//加白沫
				float foamDepth = saturate(deltaDepth + _FoamParam.x);
				float foamIntensity =  saturate( 1.0 - deltaDepth + _FoamParam.x) * pow(saturate(deltaDepth), _FoamParam.z);// * ( 1.0 - deltaDepth * _FoamParam.x);//pow(foamDepth, _FoamParam.z) * ( 1.0 - foamDepth);
				foamIntensity *= _FoamParam.y;
				
				float2 uv_FoamTex = IN.uv.xy * _FoamTex_ST.xy + _FoamTex_ST.zw;
				float foam = tex2D( _FoamTex, ( uv_FoamTex * float2(-0.89,1.11) + _TimeParameters.x * float2(0.97,1.03) * _FoamParam.w)).r
							* tex2D( _FoamTex, ( uv_FoamTex * float2(-1.09,0.91)+ _TimeParameters.x * float2(-1.05,0.95) * _FoamParam.w)).r ;
				float foamAlpha = saturate( foamIntensity * foam);
				
				Color = lerp( Color, _FoamColor.rgb, foamAlpha);
				Alpha = saturate(Alpha + foamAlpha);
	
				//雾
				UNITY_BRANCH
				if(_ApplyFog > 0){
					APPLY_FOG(Color.rgb, IN.worldPos.xyz);
				}

				return half4( Color, Alpha);
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
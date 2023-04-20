// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UniformWater"
{
	Properties
	{
		[Header(Albedo)]
		_ShallowNear("近景浅水", Color) = (0.4470589,0.8392158,0.7450981,1)
		_DeepNear("近景深水", Color) = (0.1529412,0.7058824,0.6588235,1)
		_ShallowFar("远景浅水", Color) = (0.1411765,0.3647059,0.6039216,1)
		_DeepFar("远景深水", Color) = (0.07058824,0.2078432,0.4431373,1)
		_DistanceFadeNear("DistanceFadeNear", Float) = 50
		_DistanceFadeFar("DistanceFadeFar", Float) = 200
		_ShadowIntensity("阴影强度", Range( 0 , 1)) = 0.6
		
		[Header(Refraction)]
		_Transparency("不透明度", Range( 0 , 2)) = 0.15
		_RefractionDistort("折射(x密度，y速度，z强度)【高配】", Vector) = (2,1,0.01,0)
		
		[Header(Wave)]
		_Wave("波纹1(xy方向，zw周期)", Vector) = (0,1,0,0)
		_Height("波纹1高度", Range( 0 , 2)) = 0
		_Wave1("波纹2(xy方向，zw周期)", Vector) = (0,1,0,0)
		_Height1("波纹2高度", Range( 0 , 2)) = 0
		[Toggle]_UseWorldPosAsUV("使用世界坐标作为波纹uv", int) = 1
		
		[Header(Normal)]
		_NormalTex("NormalTex", 2D) = "bump" {}
		_NormalSpeed("NormalSpeed", Vector) = (0,0,0,0)
		
		[Header(Specular)]
		[HDR]_Specular("高光颜色", Color) = (1,1,1,0)
		_SpecularPower("高光指数", Range( 10 , 5000)) = 20
		_NormalScale("高光法线强度", Range( 0 , 1)) = 0.1
		
		[Header(Reflection)]
		_EnvCube("环境盒", CUBE) = "black" {}
		[HDR]_ReflectionColor("环境反射颜色", Color) = (0.75,0.75,0.75,0)
		_NormalScale2("反射法线强度", Range( 0 , 1)) = 0.1
		
		[HDR]_FresnelColor("菲涅尔颜色", Color) = (0.172549,0.7803922,0.9019608,1)
		_NormalScale3("菲涅尔法线强度", Range( 0 , 1)) = 0.1

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
			
			Blend One Zero, One Zero
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#define MAIN_LIGHT_CALCULATE_SHADOWS

			#if defined(_FX_LEVEL_HIGH)
				#define REQUIRE_OPAQUE_TEXTURE 1
				#define REQUIRE_DEPTH_TEXTURE 1
            #elif defined(_FX_LEVEL_MEDIUM)
                
            #endif 

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"

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
			float4 _DeepFar;
			float4 _ShallowNear;
			float4 _ShallowFar;
			float4 _RefractionDistort;
			float4 _Wave1;
			float4 _Wave;
			float4 _NormalTex_ST;
			float4 _NormalSpeed;
			float4 _ReflectionColor;
			float4 _Specular;
			float4 _FresnelColor;
			float4 _DiffuseGrazing;
			float4 _FoamTex_ST;
			float4 _FoamParam;
			float4 _FoamColor;
			float _Height;
			float _Height1;
			float _UseWorldPosAsUV;
			float _DistanceFadeNear;
			float _DistanceFadeFar;
			float _ShadowIntensity;
			float _Transparency;
			float _SpecularPower;
			float _NormalScale;
			float _NormalScale2;
			float _NormalScale3;
		
			CBUFFER_END
			
			uniform float4 _CameraDepthTexture_TexelSize;
			samplerCUBE _EnvCube;
			sampler2D _NormalTex;
			sampler2D _FoamTex;


			float FastGestner( float2 uv, float4 Wave, float Height )
			{
				float v = sin((uv.x * Wave.z * 1.41 + Wave.x * _Time.y) * 4.9739 )
				* cos((uv.y  * Wave.w * 1.19 + Wave.y * _Time.y) * 7.1391) 
				+ cos((uv.x * Wave.z * 1.33 + Wave.x * _Time.y) * 5.6727 ) 
				* sin((uv.y * Wave.w * 1.25 + Wave.y * _Time.y)* 6.2173 );
				return v * Height;
			}

			float2 FastNoise(float2 uv, float scale, float speed, float intensity){
				float u = sin((uv.x * scale + speed * _Time.y) * 13.4573 )
				* cos((uv.y  * scale + speed * _Time.y) * 11.1391) 
				+ cos((uv.x * scale + speed * _Time.y) * 15.6727 ) 
				* sin((uv.y * scale + speed * _Time.y)* 17.2173 );
				float v = sin((uv.x * scale + speed * _Time.y) * 14.9739 )
				* cos((uv.y  * scale + speed * _Time.y) * 17.1391) 
				+ cos((uv.x * scale + speed * _Time.y) * 12.6173 ) 
				* sin((uv.y * scale + speed * _Time.y)* 11.2727 );
				return float2(u,v) * intensity;
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

				//TODO 使用世界坐标当uv还是模型坐标当uv
				float3 srcWorldPos = TransformObjectToWorld( v.vertex.xyz );
				float2 worldPos2UV = srcWorldPos.xz;
	
				float deltaHeight = FastGestner( worldPos2UV , _Wave , _Height ) + FastGestner( worldPos2UV , _Wave1 , _Height1 );
				//TODO 法线一并计算了。用十字叉乘
				//法线用于计算贴图法线，以及菲涅尔
				
				o.uv = v.uv;

				float offset = 0.5;
				float heightRight = FastGestner( worldPos2UV + float2(offset,0), _Wave , _Height ) + FastGestner( worldPos2UV + float2(offset,0), _Wave1 , _Height1 );
				float heightUp = FastGestner( worldPos2UV + float2(0,offset), _Wave , _Height ) + FastGestner( worldPos2UV + float2(0,offset), _Wave1 , _Height1 );
				float3 newNormal = cross(float3(0, heightUp - deltaHeight, offset), float3(offset, heightRight - deltaHeight,0));
				float viewDist = length(srcWorldPos - _WorldSpaceCameraPos);
				float fade = saturate(2 - viewDist / 50);// 因为模型远处顶点数不够，所以100米外不再生效
				newNormal =  lerp(float3(0,1,0), newNormal, fade); 
				newNormal = normalize(newNormal);
				deltaHeight = lerp(0, deltaHeight, fade);
				o.normalHeight = float4(newNormal, deltaHeight);
				
				o.normal = float4(TransformObjectToWorldNormal(v.normal), 0);
				o.tangent = float4(TransformObjectToWorldDir(v.tangent.xyz), 0);
				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				o.bitangent.xyz = float4(cross(o.normal.xyz, o.tangent.xyz) * tangentSign, 0);

				float3 positionWS = srcWorldPos + float3(0,1,0) * deltaHeight;
				float4 positionCS = TransformWorldToHClip( positionWS );

				o.worldPos = positionWS;
				o.clipPos = positionCS;
				
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
				float3 WorldPosition = IN.worldPos;
				float3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz - WorldPosition );
				float3 H = normalize( _MainLightPosition.xyz + worldViewDir );

				float4 clipPos = TransformWorldToHClip(IN.worldPos);
				float4 screenPos = ComputeScreenPos(clipPos);
				float2 screenUV = screenPos.xy / screenPos.w;
				
				//折射
				screenUV += FastNoise(screenUV, _RefractionDistort.x, _RefractionDistort.y, _RefractionDistort.z);
				float4 grabColor = float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( screenUV ), 1.0 );
				
				//深度
				float eyeDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( screenUV, 0.0 , 0.0 ).xy ),_ZBufferParams);
				float deltaDepth = ( eyeDepth - clipPos.w );
				
				//diffuse
				float4 nearColor = lerp( _ShallowNear , _DeepNear , saturate(deltaDepth * 0.6));
				float4 farColor = lerp( _ShallowFar , _DeepFar , saturate(deltaDepth * 0.3));
				float distanceFade = saturate( ( ( distance( WorldPosition , _WorldSpaceCameraPos ) - _DistanceFadeNear ) / ( _DistanceFadeFar - _DistanceFadeNear ) ) );
				float4 albedo = lerp( nearColor , farColor , distanceFade);

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
				float3 specular = pow( saturate( NdotH ) , _SpecularPower ) * _Specular * _MainLightColor;

				//环境反射
				float3 normalForRefl = lerp(IN.normal, tangentNormal.xzy , _NormalScale2);
				float3 worldRefDir = normalize( reflect( -worldViewDir, normalForRefl));
				float3 reflection = texCUBE( _EnvCube, worldRefDir ) * _ReflectionColor;
				
				//菲涅尔
				float3 normalForFresnel = lerp(IN.normalHeight.xyz,  mul( tangentToWorld, tangentNormal), _NormalScale3);
				float3 fresnel = saturate(pow(1 - dot(normalize(normalForFresnel), worldViewDir), 4) ) * _FresnelColor * (1 - distanceFade);//远景不要叠加菲涅尔，会曝
	
				//漫反射
				float3 diffuse = albedo * _MainLightColor;
				
				//散射
				float3 scatter = _DiffuseGrazing * saturate( IN.normalHeight.w);
				
				float4 ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				//以前的水的影子
				//float shadow = MainLightRealtimeShadow(ShadowCoords);
				//新的影子，包含云
				Light mainLight = GetMainLight(ShadowCoords, WorldPosition, 1,IN.normal.xyz);
				
				float shadow = mainLight.distanceAttenuation * mainLight.shadowAttenuation;
				
				shadow = lerp(1, shadow, _ShadowIntensity);//
				
				float3 Color = lerp( grabColor * shadow, ((diffuse + specular) * shadow + reflection + fresnel + scatter) , saturate( _Transparency * ( deltaDepth - 0.1 )));//-0.1是为了让白沫看起来在水“上面”而不是水“里面”
				
				//加白沫
				//TODO白沫可以配置范围(深度，强度，软化，速度）
				float foamDepth = saturate(deltaDepth + _FoamParam.x);
				float foamIntensity =  pow(saturate(deltaDepth), _FoamParam.z) * ( 1.0 - deltaDepth * _FoamParam.x);//pow(foamDepth, _FoamParam.z) * ( 1.0 - foamDepth);
				float2 uv_FoamTex = IN.uv.xy * _FoamTex_ST.xy + _FoamTex_ST.zw;
				float foam = tex2D( _FoamTex, ( uv_FoamTex + _TimeParameters.x * _FoamParam.w) ) * tex2D( _FoamTex, ( uv_FoamTex + _TimeParameters.x * -_FoamParam.w) ) ;
				Color = lerp( Color , _FoamColor , saturate( foamIntensity * foam * _FoamParam.y));

	
				//雾
				APPLY_FOG(Color.rgb, IN.worldPos.xyz);

				return half4( Color, 1.0 );
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
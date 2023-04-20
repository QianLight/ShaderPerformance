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
		[Toggle]_CSharpSync("同步C#,精确时间，忽略衰减", Int) = 0
		
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

		_ReflectionSpeed("环境盒旋转速度",float)=0.26
		_ReflectionOffset("环境盒与天空的偏移同步",float)=-718
		
		_WaveVertexDensity("粒子顶点法线波动系数",float) = 1
		_WaveNormalDensity("粒子像素法线波动系数",float) = 1
		_PlannarRelfectDensity("平面反射强度",range(0,1)) = 0.6
		_PlannarRelfectDistorb("平面反射扰动速度（xy为方向及速度，z为缩放，w为扰动强度）",Vector) = (0.05,0,0.25,0.15)
		[Enum(On,1,Off,0)]_DepthMode("Depth Mode", Float) = 0
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
			ZWrite [_DepthMode]
			Cull Back
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#pragma multi_compile _PPWave
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			// #pragma multi_compile _PARKOUR

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
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
			#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"

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
			#define UNITY_PI            3.14159265359f
			CBUFFER_START(UnityPerMaterial)
			half4 _DeepNear;
			half4 _DeepMid;
			half4 _DeepFar;
			half4 _DistanceFade;
			half4 _Wave;
			half4 _Wave1;
			half4 _PlannarRelfectDistorb;
			half4 _FresnelColor;
			half4 _FresnelDist;
			half4 _WaveCameraPos;
			half4 _DiffuseGrazing;
			half4 _NormalTex_ST;
			half4 _NormalSpeed;
			half4 _ReflectionColor;
			half4 _ReflectionColorMid;
			half4 _ReflectionColorFar;
			half4 _Specular;
			half4 _FoamTex_ST;
			half4 _FoamParam;
			half4 _FoamColor;
		//	half _Transparency;
			half _EnableRealtimeShadow;
			half _EnableSmartShadow;
			half _EnableCloudShadow;
			half _ShadowIntensity;
			half _ApplyFog;
			half _Height;
			half _Height1;
			half _UseWorldPosAsUV;
			half _SpecularPower;
			half _NormalScale;
			half _PlannarRelfectDensity;
			half _NormalScale2;
			half _ReflectionSpeed;
			half _ReflectionOffset;
			half _WaveVertexDensity;
			half _WaveNormalDensity;
			half _CSharpSync;
			CBUFFER_END

			TEXTURE2D(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);
			uniform half4 _CameraDepthTexture_TexelSize;
			samplerCUBE _EnvCube;
			sampler2D _NormalTex;
			sampler2D _FoamTex;
			sampler2D _WaveTex;
			sampler2D _SSPlanarReflectionTexture;
			half4 _WaterLightDir;//静态
			float _RealTime;


			float FastGestner( half2 uv, half4 Wave, float Height )
			{
				float time = _CSharpSync == 1 ? _RealTime : _Time.y;

				float v = sin((uv.x * Wave.z * 1.41 + Wave.x * time) * 4.9739 )
				* cos((uv.y  * Wave.w * 1.19 + Wave.y * time) * 7.1391) 
				+ cos((uv.x * Wave.z * 1.33 + Wave.x * time) * 5.6727 ) 
				* sin((uv.y * Wave.w * 1.25 + Wave.y * time)* 6.2173 );
				return v * Height;
			}
		
			float3x3 Inverse3x3(float3x3 input)
			{
				float3 a = input._11_21_31;
				float3 b = input._12_22_32;
				float3 c = input._13_23_33;
				return float3x3(cross(b,c), cross(c,a), cross(a,b)) * (1.0 / dot(a,cross(b,c)));
			}
			
			half3 SampleReflection(half3 positionWS, half3 normalWS, half2 normalDistorb,half3 viewDirectionWS, half2 screenUV, half3 worldRefDir,half3 refColor)
			{
    			half3 distortNormal = 0;
				distortNormal.xz =  normalDistorb.xy ;
				distortNormal.y = normalWS.y;
				
    			half2 reflectionUV = screenUV + distortNormal.zx * half2(0.1, 0.3) * _PlannarRelfectDistorb.w;
				half4 sample = 0;
			#if defined(_FX_LEVEL_HIGH)
				sample = tex2D(_SSPlanarReflectionTexture, reflectionUV.xy);
				sample.rgb =lerp(texCUBE( _EnvCube, worldRefDir).rgb ,sample.rgb, saturate(sample.a) * _PlannarRelfectDensity ) * refColor;
			#else
				sample.rgb = texCUBE( _EnvCube, worldRefDir).rgb * refColor;
			#endif

    			return sample.rgb ;
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
	
				half deltaHeight = FastGestner( worldPos2UV , _Wave , _Height ) + FastGestner( worldPos2UV , _Wave1 , _Height1 );
				//顶点法线一并计算了。用十字叉乘
				//顶点法线用于计算贴图法线，以及菲涅尔
				#ifdef CALC_VERTEX_NORMAL
					half offset = 0.5;
					half heightRight = FastGestner( worldPos2UV + half2(offset,0), _Wave , _Height ) + FastGestner( worldPos2UV + half2(offset,0), _Wave1 , _Height1 );
					half heightUp = FastGestner( worldPos2UV + half2(0,offset), _Wave , _Height ) + FastGestner( worldPos2UV + half2(0,offset), _Wave1 , _Height1 );
					float3 newNormal = cross(half3(0, heightUp - deltaHeight, offset), half3(offset, heightRight - deltaHeight,0));
					half viewDist = length(srcWorldPos - _WorldSpaceCameraPos);
					half fade = _CSharpSync == 1 ? 1 : saturate(2 - viewDist / 150);// 因为模型远处顶点数不够，所以50米外顶点法线不再生效
					newNormal =  lerp(half3(0,1,0), newNormal, fade); 
					newNormal = normalize(newNormal);
					deltaHeight = lerp(0, deltaHeight, fade);
				#else
					half3 newNormal = half3(0,1,0);
				#endif
				o.normalHeight = float4(newNormal, deltaHeight);
				
				o.normal = float4(TransformObjectToWorldNormal(v.normal), 0);
				o.tangent = float4(TransformObjectToWorldDir(v.tangent.xyz), 0);
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				o.bitangent.xyz = cross(o.normal.xyz, o.tangent.xyz) * tangentSign;

				half3 positionWS = srcWorldPos + half3(0,1,0) * deltaHeight;
				
				o.worldPos = positionWS;
				UNITY_BRANCH
				if(_IsParkour == 1)
				{
					o.worldPos= ParkourDistortVertex(o.worldPos);
					o.clipPos = TransformWorldToHClip(o.worldPos);
				}
				else
				{
					o.clipPos = TransformWorldToHClip( o.worldPos );
				}

				o.uv = v.uv;
				return o;
			}

			float3 RotateAroundYInDegrees(float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
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
				half3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz - worldPosition );//eyeVec
				half3 lightDir = _WaterLightDir.xyz;

				
				half3 H = normalize( lightDir.xyz + worldViewDir );

				half4 clipPos = TransformWorldToHClip(IN.worldPos);
				half4 screenPos = ComputeScreenPos(clipPos);
				half2 screenUV = screenPos.xy / screenPos.w;
				
				//固有色
				half EyeDist = distance( worldPosition , _WorldSpaceCameraPos );
				half4 albedo = lerp(_DeepNear, _DeepMid, saturate((EyeDist - _DistanceFade.x - _DistanceFade.y) / (2 * _DistanceFade.y)));
				albedo = lerp(albedo, _DeepFar, saturate((EyeDist - _DistanceFade.z - _DistanceFade.w) / (2 * _DistanceFade.w)));
				half3 refColor = lerp(_ReflectionColor, _ReflectionColorMid, saturate((EyeDist - _DistanceFade.x - _DistanceFade.y) / (2 * _DistanceFade.y)));
				refColor = lerp(refColor, _ReflectionColorFar, saturate((EyeDist - _DistanceFade.z - _DistanceFade.w) / (2 * _DistanceFade.w)));

				//法线贴图
				float3 worldPos = IN.worldPos.xyz;
				float worldWaveuvX = (worldPos.x  - _WaveCameraPos.x + _WaveCameraPos.w) / ((_WaveCameraPos.x + _WaveCameraPos.w) - (_WaveCameraPos.x - _WaveCameraPos.w));
				worldWaveuvX = saturate(worldWaveuvX);
				float worldWaveuvY = (worldPos.z  - _WaveCameraPos.z + _WaveCameraPos.w) / ((_WaveCameraPos.z + _WaveCameraPos.w) - (_WaveCameraPos.z - _WaveCameraPos.w));
				worldWaveuvY = saturate(worldWaveuvY);
				float2 worldWaveUV = half2(worldWaveuvX, worldWaveuvY);
				half border = (ceil(worldWaveuvX - 0.01)) * (ceil (-worldWaveuvX + 0.99)) * (ceil(worldWaveuvY - 0.01))* (ceil(-worldWaveuvY + 0.99));
				float3 normalDetail;
				
				half2 uv_NormalTex = IN.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw;
				
				#if defined(_PPWave) && defined(_FX_LEVEL_HIGH)
					half3 WavePPTex = 	tex2D( _WaveTex,  worldWaveUV) - tex2D( _WaveTex, (worldWaveUV ) + half2( 0.01,0.01 ) );
					WavePPTex *= border;
					WavePPTex.xy = WavePPTex.xy * _WaveNormalDensity;
					
					half3 tangentNormal = BlendNormal( UnpackNormal( tex2D( _NormalTex, ( uv_NormalTex + _NormalSpeed.xy * _TimeParameters.x ) )) , UnpackNormal( tex2D( _NormalTex, uv_NormalTex + _NormalSpeed.zw * _TimeParameters.x  ) ) );
					// WavePPTex = WavePPTex * 2 - 1;
					tangentNormal = half3((tangentNormal.x + WavePPTex.x), (tangentNormal.y + WavePPTex.y), tangentNormal.z+1);
					tangentNormal = normalize(tangentNormal);	
				#else
					half3 tangentNormal = BlendNormal( UnpackNormal( tex2D( _NormalTex, ( uv_NormalTex + _NormalSpeed.xy * _TimeParameters.x ) )) , UnpackNormal( tex2D( _NormalTex, uv_NormalTex + _NormalSpeed.zw * _TimeParameters.x  ) ) );
				#endif
			
				half3 worldTangent = IN.tangent.xyz;
				half3 worldNormal = IN.normalHeight.xyz;// IN.normal.xyz;
				half3 worldBitangent = IN.bitangent.xyz;
				half3 tanToWorld0 = half3( worldTangent.x, worldBitangent.x, worldNormal.x );
				half3 tanToWorld1 = half3( worldTangent.y, worldBitangent.y, worldNormal.y );
				half3 tanToWorld2 = half3( worldTangent.z, worldBitangent.z, worldNormal.z );

				half3x3 worldToTangent = half3x3(worldTangent,worldBitangent,worldNormal);
				half3x3 tangentToWorld = Inverse3x3( worldToTangent );
				
				//高光考虑顶点法线，否则高光太死
				//环境反射不考虑顶点法线，不然太油
				//高光
				half3 normalForSpec = lerp( half3(0,0,1) , tangentNormal , _NormalScale);
				normalForSpec = mul( tangentToWorld, normalForSpec );
				float NdotH = dot( H , normalForSpec );
				half3 specular = pow( saturate( NdotH ) , _SpecularPower ) * _Specular.rgb * _MainLightColor.rgb;

				//环境反射
				half3 normalForRefl = lerp(IN.normal.xyz, tangentNormal.xzy , _NormalScale2);
				half3 worldRefDir = normalize( reflect( -worldViewDir, normalForRefl));
				worldRefDir = RotateAroundYInDegrees(worldRefDir,(_Time.y + _ReflectionOffset) * (-_ReflectionSpeed));
				
				half2 uv_NormalTexForRelfect =  screenUV.xy * _PlannarRelfectDistorb.z;
				half3 tangentReflectNormal  = UnpackNormal( tex2D( _NormalTex, ( uv_NormalTexForRelfect + _PlannarRelfectDistorb.xy * _TimeParameters.x ) ));
				half3 reflection = SampleReflection(worldPosition, worldNormal, tangentReflectNormal.xy, worldViewDir, screenUV, worldRefDir, refColor);

				//菲涅尔
				half3 normalForFresnel = lerp(IN.normalHeight.xyz,  mul( tangentToWorld, tangentNormal), _FresnelDist.w);
				half3 fresnel = saturate(pow(1 - dot(normalize(normalForFresnel), worldViewDir), 4) ) * _FresnelColor.rgb;
				fresnel *= pow(saturate((_FresnelDist.x - EyeDist) / (abs(_FresnelDist.y) + 0.01)), _FresnelDist.z);//远景不要叠加菲涅尔，会曝
				
	
				//漫反射
				half3 diffuse = albedo.rgb * _MainLightColor.rgb;
				
				//散射
				half3 scatter = _DiffuseGrazing.rgb * saturate( IN.normalHeight.w);
				
				half shadow = 1;
			#ifndef IGNORE_SHADOW
				//实时阴影
				#ifdef _MAIN_LIGHT_SHADOWS
				UNITY_BRANCH
				if(_EnableRealtimeShadow > 0){
					half4 ShadowCoords = TransformWorldToShadowCoord( worldPosition );
					half realtimeShadow = saturate(MainLightRealtimeShadow(ShadowCoords));
					half shadowFade = GetShadowFade(worldPosition);
					shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				}
				#endif
				
				//SmartShadow阴影
				UNITY_BRANCH
				if(_EnableSmartShadow > 0){
					half smartShadow = GetSmartShadow(_MainLightPosition.xyz, IN.normalHeight.xyz, half4(worldPosition,1), _SmartShadowIntensity);
					shadow = min(shadow, smartShadow);
				}
				
				//云阴影
				UNITY_BRANCH
				if(_EnableCloudShadow > 0){
					half cloud = CloudShadowColor(worldPosition);
					shadow = min(cloud, shadow);//zh
				}
				
				shadow = lerp(1, shadow, _ShadowIntensity);
			#endif	
				half3 Color = (diffuse + specular) * shadow + reflection + fresnel + scatter;

				//深度
			#ifdef USE_DEPTH_TEX
				half sampleDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV.xy);
				half eyeDepth = LinearEyeDepth(sampleDepth,_ZBufferParams);
				half deltaDepth = ( eyeDepth - clipPos.w );
				half Alpha = saturate((deltaDepth - 0.05) / ((1 - albedo.a) * (1 - albedo.a) * 3000 + 0.01));
			
				//加白沫
				half foamDepth = saturate(deltaDepth + _FoamParam.x);
				half foamIntensity =  saturate( 1.0 - deltaDepth + _FoamParam.x) * pow(saturate(deltaDepth), _FoamParam.z);// * ( 1.0 - deltaDepth * _FoamParam.x);//pow(foamDepth, _FoamParam.z) * ( 1.0 - foamDepth);
				foamIntensity *= _FoamParam.y;
				
				half2 uv_FoamTex = IN.uv.xy * _FoamTex_ST.xy + _FoamTex_ST.zw;
				half foam = tex2D( _FoamTex, ( uv_FoamTex * half2(-0.89,1.11) + _TimeParameters.x * half2(0.97,1.03) * _FoamParam.w)).r
							* tex2D( _FoamTex, ( uv_FoamTex * half2(-1.09,0.91)+ _TimeParameters.x * half2(-1.05,0.95) * _FoamParam.w)).r ;
				half foamAlpha = saturate( foamIntensity * foam);
				
				Color = lerp( Color, _FoamColor.rgb, foamAlpha);
			
				Alpha = saturate(Alpha + foamAlpha);
			#else
				half Alpha = 1;
			#endif

				Color *= lerp(_SceneColor.rgb, 1, _SceneColor.a);
				
				APPLY_FOG(Color.rgb, IN.worldPos.xyz);

				return half4( Color, Alpha);
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
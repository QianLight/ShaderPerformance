Shader "URP/Scene/UniformWater Bigmap Effect"
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
		_Height("大波高度", Range( 0 , 10)) = 0
		_Wave("大波波纹1(xy方向，zw周期)", Vector) = (0,1,0,0)
		_Height1("中波高度", Range( 0 , 5)) = 0
		_Wave1("中波波纹2(xy方向，zw周期)", Vector) = (0,1,0,0)
		_Height2("小波高度", Range( 0 , 2)) = 0
		_Wave2("小波波纹2(xy方向，zw周期)", Vector) = (0,1,0,0)
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
		_DiffuseGrazing("大波SSS颜色", Color) = (0.172549,0.7803922,0.9019608,1)
		_DiffuseGrazing1("中波SSS颜色", Color) = (0.172549,0.7803922,0.9019608,1)
		_DiffuseGrazing2("小波SSS颜色", Color) = (0.172549,0.7803922,0.9019608,1)
			
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
		
		_MaskTex("遮罩贴图",2D) = "white"{}
		_NoiseTex("噪音贴图",2D) = "black"{}
		_NoiseColor("噪音颜色",Color) = (1,1,1,1)
		_NoiseParam("XY:方向,Z:速度,W:强度",vector) = (-1,0,2,1)
		_Alpha("透明度",range(0,1)) = 1
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

			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
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
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
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
				float2 uv : TEXCOORD0;
				float4 tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				
				float3 normalCalc : TEXCOORD2;//xyz是Gastner计算的法线
				float3 height : TEXCOORD3;//x是大波改变的高度，y是中波改变的高度，z是小波高度
				
				float4 normal : TEXCOORD4;
				float4 tangent : TEXCOORD5;
				float4 bitangent : TEXCOORD6;
				float2 uv2: TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			#define UNITY_PI            3.14159265359f
			CBUFFER_START(UnityPerMaterial)
			float4 _DeepNear;
			float4 _DeepMid;
			float4 _DeepFar;
			float4 _DistanceFade;
			float4 _Wave;
			float4 _Wave1;
			float4 _Wave2;
			float4 _NormalTex_ST;
			float4 _NormalSpeed;
			float4 _ReflectionColor;
			float4 _ReflectionColorMid;
			float4 _ReflectionColorFar;
			float4 _Specular;
			float4 _PlannarRelfectDistorb;
			float4 _FresnelColor;
			float4 _FresnelDist;
			float4 _DiffuseGrazing;
			float4 _DiffuseGrazing1;
			float4 _DiffuseGrazing2;
			float4 _FoamTex_ST;
			float4 _FoamParam;
			float4 _FoamColor;
			float4 _WaveCameraPos;
			float4 _MaskTex_ST;
			float4 _NoiseTex_ST;
		//	float _Transparency;
			float _EnableRealtimeShadow;
			float _EnableSmartShadow;
			float _EnableCloudShadow;
			float _ShadowIntensity;
			float _ApplyFog;
			float _Height;
			float _Height1;
			float _Height2;
			float _UseWorldPosAsUV;
			float _SpecularPower;
			float _NormalScale;
			float _PlannarRelfectDensity;
			float _NormalScale2;
			float _ReflectionSpeed;
			float _ReflectionOffset;
			float _WaveVertexDensity;
			float _WaveNormalDensity;
			float _CSharpSync;
			CBUFFER_END
			
			uniform float4 _CameraDepthTexture_TexelSize;
			samplerCUBE _EnvCube;
			sampler2D _NormalTex;
			sampler2D _FoamTex;
			sampler2D _WaveTex;
			sampler2D _SSPlanarReflectionTexture;
			float4 _WaterLightDir;//静态
			float _RealTime;
			
			sampler2D _MaskTex;
			sampler2D _NoiseTex;
			float4 _NoiseParam;
			float4 _NoiseColor;
			float _Alpha;
			//大波
			float FastGestner( float2 uv, float4 Wave, float Height )
			{
				float time = _CSharpSync == 1 ? _RealTime : _Time.y;

				float v = (1 - abs(sin((uv.x * Wave.z * 1.41 + Wave.x * time) * 4.9739 )))
				* (1 - abs(cos((uv.y  * Wave.w * 1.19 + Wave.y * time) * 7.1391)))
				+ (1 - abs(cos((uv.x * Wave.z * 1.33 + Wave.x * time * 0.9) * 5.6727 ))) 
				* (1 - abs(sin((uv.y * Wave.w * 1.25 + Wave.y * time * 1.1)* 6.2173 )));
				return v * Height;
			}
			
			//中波
			float FastGestner1( float2 uv, float4 Wave, float Height )
			{
				float time = _CSharpSync == 1 ? _RealTime : _Time.y;

				float v = (saturate(sin((uv.x * Wave.z * 1.41 + Wave.x * time) * 4.9739 )))
				* (saturate(cos((uv.y  * Wave.w * 1.19 + Wave.y * time) * 7.1391)))
				+ (saturate(cos(((uv.x + 0.5) * Wave.z * 1.33 + Wave.x * time * 0.7) * 5.6727 ))) 
				* (saturate(sin(((uv.y + 0.5) * Wave.w * 1.25 + Wave.y * time * 1.3)* 6.2173 )));
				
				return v * Height;
			}
			
			//小波
			float FastGestner2( float2 uv, float4 Wave, float Height )
			{
				float time = _CSharpSync == 1 ? _RealTime : _Time.y;

				float v = (saturate(sin((uv.x * Wave.z * 1.41 + Wave.x * time) * 4.9739 )))
				* (saturate(cos((uv.y  * Wave.w * 1.19 + Wave.y * time) * 7.1391)))
				* saturate(cos(uv.x  * Wave.w * 1.19 + Wave.y * time) * sin((uv.y * Wave.w * 1.25 + Wave.y * time * 1.1)));
				return v * Height;
			}
		
			float3x3 Inverse3x3(float3x3 input)
			{
				float3 a = input._11_21_31;
				float3 b = input._12_22_32;
				float3 c = input._13_23_33;
				return float3x3(cross(b,c), cross(c,a), cross(a,b)) * (1.0 / dot(a,cross(b,c)));
			}
			
			half3 SampleReflection(half3 positionWS, half3 normalWS, half2 normalDistorb,half3 viewDirectionWS, half2 screenUV, float3 worldRefDir,float3 refColor)
			{
				half3 reflection = 0;
    			half3 distortNormal = 0;
				distortNormal.xz =  normalDistorb.xy ;
				distortNormal.y = normalWS.y;
				
    			half3 reflectVector = reflect(-viewDirectionWS, distortNormal);
    			half2 reflectionUV = screenUV + distortNormal.zx * half2(0.1, 0.3) * _PlannarRelfectDistorb.w;
				half4 sample = 0;
				#if defined(_FX_LEVEL_HIGH)
				sample = tex2D(_SSPlanarReflectionTexture, reflectionUV.xy);//planar reflection
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
	
				float deltaHeightLarge = FastGestner( worldPos2UV , _Wave , _Height );
				float deltaHeightMid = FastGestner1( worldPos2UV , _Wave1 , _Height1);
				float deltaHeightSmall = FastGestner2( worldPos2UV , _Wave2 , _Height2);
				float deltaHeight = deltaHeightLarge + deltaHeightMid;
				//顶点法线一并计算了。用十字叉乘
				//顶点法线用于计算贴图法线，以及菲涅尔
				#ifdef CALC_VERTEX_NORMAL
					float offset = 0.5;
					float heightRight = FastGestner( worldPos2UV + float2(offset,0), _Wave , _Height ) + FastGestner1( worldPos2UV + float2(offset,0), _Wave1 , _Height1 );
					float heightUp = FastGestner( worldPos2UV + float2(0,offset), _Wave , _Height ) + FastGestner1( worldPos2UV + float2(0,offset), _Wave1 , _Height1 );
					float3 newNormal = cross(float3(0, heightUp - deltaHeight, offset), float3(offset, heightRight - deltaHeight,0));
					float viewDist = length(srcWorldPos - _WorldSpaceCameraPos);
					float fade = 1;//_CSharpSync == 1 ? 1 : saturate(2 - viewDist / 50);// 因为模型远处顶点数不够，所以50米外顶点法线不再生效
					newNormal =  lerp(float3(0,1,0), newNormal, fade); 
					newNormal = normalize(newNormal);
					deltaHeight = lerp(0, deltaHeight, fade);
				#else
					float3 newNormal = float3(0,1,0);
					float fade = 1;
				#endif
				o.normalCalc = float3(newNormal);
				o.height = float3(deltaHeightLarge, deltaHeightMid, deltaHeightSmall);
				
				o.normal = float4(TransformObjectToWorldNormal(v.normal), 0);
				o.tangent = float4(TransformObjectToWorldDir(v.tangent.xyz), 0);
				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				o.bitangent.xyz = cross(o.normal.xyz, o.tangent.xyz) * tangentSign;

				float3 positionWS = srcWorldPos + float3(0,1,0) * (deltaHeightLarge + deltaHeightMid + deltaHeightSmall) * fade;

				o.worldPos = positionWS;
				UNITY_BRANCH
				if(_IsParkour == 1)
				{
					o.worldPos = ParkourDistortVertex(o.worldPos );
					o.clipPos = TransformWorldToHClip(o.worldPos );
				}
				else{
					o.clipPos = TransformWorldToHClip( o.worldPos );
				}

				o.uv = v.uv;
				o.uv2 = TRANSFORM_TEX(v.uv,_NoiseTex) + _NoiseParam.xy * _Time * _NoiseParam.z;
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
				float3 worldViewDir = normalize( _WorldSpaceCameraPos.xyz - worldPosition );//eyeVec
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

				//法线贴图
				float3 worldPos = IN.worldPos.xyz;
				float worldWaveuvX = (worldPos.x  - _WaveCameraPos.x + _WaveCameraPos.w) / ((_WaveCameraPos.x + _WaveCameraPos.w) - (_WaveCameraPos.x - _WaveCameraPos.w));
				worldWaveuvX = saturate(worldWaveuvX);
				float worldWaveuvY = (worldPos.z  - _WaveCameraPos.z + _WaveCameraPos.w) / ((_WaveCameraPos.z + _WaveCameraPos.w) - (_WaveCameraPos.z - _WaveCameraPos.w));
				worldWaveuvY = saturate(worldWaveuvY);
				float2 worldWaveUV = float2(worldWaveuvX,worldWaveuvY);
				float border = (ceil(worldWaveuvX - 0.01)) * (ceil (-worldWaveuvX + 0.99)) * (ceil(worldWaveuvY - 0.01))* (ceil(-worldWaveuvY + 0.99));
				FLOAT3 normalDetail;
				
				float2 uv_NormalTex = IN.uv.xy * _NormalTex_ST.xy + _NormalTex_ST.zw;
				
				#if defined(_PPWave) && defined(_FX_LEVEL_HIGH)
					float3 WavePPTex = 	tex2D( _WaveTex,  worldWaveUV) - tex2D( _WaveTex, (worldWaveUV ) + float2( 0.01,0.01 ) );
					WavePPTex *= border;
					WavePPTex.xy = WavePPTex.xy * _WaveNormalDensity;
					
					float3 tangentNormal = BlendNormal( UnpackNormal( tex2D( _NormalTex, ( uv_NormalTex + _NormalSpeed.xy * _TimeParameters.x ) )) , UnpackNormal( tex2D( _NormalTex, uv_NormalTex + _NormalSpeed.zw * _TimeParameters.x  ) ) );
					// WavePPTex = WavePPTex * 2 - 1;
					tangentNormal = float3((tangentNormal.x + WavePPTex.x), (tangentNormal.y + WavePPTex.y), tangentNormal.z+1);
					tangentNormal = normalize(tangentNormal);	
				#else
					float3 tangentNormal = BlendNormal( UnpackNormal( tex2D( _NormalTex, ( uv_NormalTex + _NormalSpeed.xy * _TimeParameters.x ) )) , UnpackNormal( tex2D( _NormalTex, uv_NormalTex + _NormalSpeed.zw * _TimeParameters.x  ) ) );

				#endif
			
				float3 worldTangent = IN.tangent.xyz;
				float3 worldNormal = IN.normalCalc.xyz;// IN.normal.xyz;
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

			//	return float4(specular,1);
				//环境反射
				float3 normalForRefl = lerp(IN.normal.xyz, tangentNormal.xzy , _NormalScale2);
				float3 worldRefDir = normalize( reflect( -worldViewDir, normalForRefl));
				worldRefDir = RotateAroundYInDegrees(worldRefDir,(_Time.y + _ReflectionOffset) * (-_ReflectionSpeed));
				
				float2 uv_NormalTexForRelfect =  screenUV.xy * _PlannarRelfectDistorb.z;
				float3 tangentReflectNormal  = UnpackNormal( tex2D( _NormalTex, ( uv_NormalTexForRelfect + _PlannarRelfectDistorb.xy * _TimeParameters.x ) ));
				float3 reflection = SampleReflection(worldPosition,worldNormal,tangentReflectNormal.xy,worldViewDir,screenUV,worldRefDir,refColor);
				///return float4(reflection,1);
				//return float4(ddx(uv_NormalTexForRelfect.x).xxx,1);
				//菲涅尔
				float3 normalForFresnel = lerp(IN.normalCalc.xyz,  mul( tangentToWorld, tangentNormal), _FresnelDist.w);
				float fresnelPower = 10;//4;
				float3 fresnel = saturate(pow(1 - dot(normalize(normalForFresnel), worldViewDir), fresnelPower) ) * _FresnelColor.rgb;
				fresnel *= pow(saturate((_FresnelDist.x - EyeDist) / (abs(_FresnelDist.y) + 0.01)), _FresnelDist.z);//远景不要叠加菲涅尔，会曝
				
	
				//漫反射
				float3 diffuse = albedo.rgb * _MainLightColor.rgb;
				
				//散射
				float3 scatter = _DiffuseGrazing.rgb * saturate( IN.height.x) 
				+ _DiffuseGrazing1.rgb * saturate( IN.height.y)
				+ _DiffuseGrazing2.rgb * saturate( IN.height.z);
				
				
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
					float smartShadow = GetSmartShadow(_MainLightPosition.xyz, IN.normalCalc.xyz, float4(worldPosition,1), _SmartShadowIntensity);
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

				//深度
				#ifdef USE_DEPTH_TEX
					float eyeDepth = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( float4( screenUV, 0.0 , 0.0 ).xy ),_ZBufferParams);
					float deltaDepth = ( eyeDepth - clipPos.w );
					
					float Alpha = saturate((deltaDepth - 0.05) / ((1 - albedo.a) * (1 - albedo.a) * 3000 + 0.01));
				
					//加白沫
					float foamDepth = saturate(deltaDepth + _FoamParam.x);
					float foamIntensity =  saturate( 1.0 - deltaDepth + _FoamParam.x) * pow(saturate(deltaDepth), _FoamParam.z);// * ( 1.0 - deltaDepth * _FoamParam.x);//pow(foamDepth, _FoamParam.z) * ( 1.0 - foamDepth);
					foamIntensity *= _FoamParam.y;
					
					float2 uv_FoamTex = IN.uv.xy * _FoamTex_ST.xy + _FoamTex_ST.zw;
					float foam1 = tex2D( _FoamTex, ( uv_FoamTex * float2(-0.89,1.11) + _TimeParameters.x * float2(0.97,1.03) * _FoamParam.w)).r;
					float foam2 = tex2D( _FoamTex, ( uv_FoamTex * float2(-1.09,0.91)+ _TimeParameters.x * float2(-1.05,0.95) * _FoamParam.w)).r ;
					float foamAlpha = saturate( foamIntensity * foam1 * foam2);
					
					Color += saturate(saturate(foam1 - 0.5) * foam2 * IN.height.y * 1.5) * saturate(IN.height.x - 2);
					Color += saturate(saturate(foam2 - 0.5) * foam1 * IN.height.z * 3) * saturate(IN.height.x - 2); 
					Color = lerp( Color, _FoamColor.rgb, foamAlpha);
				
					Alpha = saturate(Alpha + foamAlpha);
				#else
					float Alpha = 1;
				#endif

				Color *= lerp(_SceneColor.rgb, 1, _SceneColor.a);
				
				//雾
				UNITY_BRANCH
				if(_ApplyFog > 0){
					APPLY_FOG(Color.rgb, IN.worldPos.xyz);
				}

				//特效贴图
				float mask = tex2D(_MaskTex,IN.uv).r;
				float noise = tex2D(_NoiseTex,IN.uv2).r;
				float3 noiseCol = noise * _NoiseColor;
				Color = lerp(Color,noiseCol,clamp(noise *_NoiseParam.w,0,1));
				Alpha *= mask *_Alpha ;

				return half4( Color, Alpha);
			}

			ENDHLSL
		}
	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
Shader "Effect/UVEffect_test1"
{
	Properties
	{
		[Enum(Custom Data,0,Material,1)]_ShaderMode("Shader Mode", Float) = 1
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
		_CameraFadeLength("CameraFadeLength", Float) = 5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		CGINCLUDE
		#pragma target 4.0
		ENDCG
		Blend SrcAlpha [_BlendMode]
		Cull [_CullMode]
		ColorMask RGBA
		ZWrite [_DepthMode]
		ZTest [_ZTest]
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
		//only defining to not throw compilation error over Unity 5.5
		#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
#endif
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				//UNITY_VERTEX_INPUT_INSTANCE_ID
				//UNITY_VERTEX_OUTPUT_STEREO
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
				float4 color : COLOR;
				float4 worldPos : TEXCOORD5;
				float3 viewPos: TEXCOORD6;
			};

			uniform float _UseMask;
			uniform half _BlendMode;
			uniform float _ZTest;
			uniform half _CullMode;
			uniform float _CameraFade;
			uniform float _CameraFadeLength;
			uniform float _CameraFadeOffset;
			uniform float _UseRamp;
			uniform float4 _Color0;
			uniform float4 _Color1;
			uniform float4 _RampParam;
			uniform float4 _MainLightDir0;
			uniform float4 _Color2;
			uniform float _UseDepthFade;
			uniform half _EdgeWidth;
			uniform half _ShaderMode;
			uniform float4 _WidthColor;
			uniform float _UseBackColor;
			uniform float4 _BackColor;
			uniform float4 _MainColor;
			uniform half _Brightness;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform half _MainPannerX;
			uniform half _MainPannerY;
			uniform half _Dissolve;
			uniform half _fresnelpower;
			uniform half _fresnelmultiply;
			uniform float _Flip;
			uniform float _Usefresnel;
			uniform half _DistortPower;
			uniform float _UseTurbulence;
			uniform sampler2D _TurbulenceTex;
			uniform float4 _TurbulenceTex_ST;
			uniform half _PowerU;
			uniform half _PowerV;
			uniform half _Contrast;
			uniform half _UseClip;
			uniform half _Hardness;
			uniform float _AlphaR;
			uniform half _Alpha;
			uniform sampler2D _MaskTex;
			uniform float4 _MaskTex_ST;
			uniform half _CUTOUT;
			uniform half _DepthMode;
			uniform float _FadeLength;
			uniform sampler2D _CameraDepthRT;

			#define USE_MASK _UseMask>0
			#define DEPTH_MODE _DepthMode>0
			#define USE_CLIP _UseClip>0
			#define USE_DEPTHFADE _UseDepthFade>0
			#define USE_RAMP _UseRamp>0
			#define USE_CAMERAFADE _CameraFade>0

			half getFresnel(float3 normal , float3 view)
			{
				half result = 0;
				half ndv = dot( normal , view );
				half fresnel = pow(1-saturate(ndv),_fresnelpower);
				result = saturate(fresnel * _fresnelmultiply);
				return result;
			} 

			void getBlendData(half fresnel,half4 inCoord0,half4 inCoord1,out half2 coord0,out half2 coord1,out half edgewidth ,out half edgebrightness,out half dissolve,out half distort)
			{
				inCoord0.w+=1;
				half4 Data0=lerp(inCoord0,half4(1,1,_EdgeWidth,1),_ShaderMode);
				half4 Data1=lerp(inCoord1,half4(half2(_MainPannerX,_MainPannerY)*_Time.y,_Dissolve*(1-fresnel),_DistortPower),_ShaderMode);
				coord0 = Data0.xy;
				edgewidth = Data0.z;
				edgebrightness = Data0.w;
				coord1 = Data1.xy;
				dissolve = Data1.z;
				distort = Data1.w;
			}

			fixed getTurbulence(half2 coord)
			{
				fixed turbulence=0;
				half2 uv_TurbulenceTex = coord.xy * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw;
				fixed4 turbulenceTex = tex2D( _TurbulenceTex, uv_TurbulenceTex );
				turbulence = turbulenceTex.r;
				return turbulence;
			}

			half2 getClipData(half dissolve,half edgewidth,fixed mainTexC)
			{
				half2 result = 0;

				half data0 = saturate((mainTexC-dissolve*(edgewidth+1)*(1-_Hardness+1)-_Hardness)/(1-_Hardness));
				half data1 = saturate((mainTexC-(dissolve*(edgewidth+1)-edgewidth)*(1-_Hardness+1)-_Hardness)/(1-_Hardness));
				result.x=data0;
				result.y=data1;
				return result;
			}

			float DecodeFloatRG_Custom( float2 enc )
			{
				 return dot(enc,float2(1,1/2047.0));
			}
			
			float MyCustomExpression271( float z , float4 zBufferParams )
			{
				//z*=zBufferParams.x;
				return 1/(z*zBufferParams.x+zBufferParams.y);
			}

			half depthFade(float4 screenPos)
			{
				screenPos =screenPos/screenPos.w;
				fixed4 depthTex = tex2D( _CameraDepthRT, screenPos.xy );
				half HDepth=DecodeFloatRG_Custom(depthTex.xy);
				HDepth = Linear01Depth(HDepth);
				half fade = smoothstep(0,_FadeLength*0.01, abs(HDepth-Linear01Depth(screenPos.z)));
				return fade;
			}

			half3 getCustomRamp(half3 nor)
			{
				half gradient = 1-saturate (dot(nor,_MainLightDir0.xyz)*0.5+0.5);
				half3 blendCol = lerp(_Color0.xyz,_Color1.xyz,smoothstep(_RampParam.x,_RampParam.y,gradient));
				blendCol = lerp(blendCol,_Color2.xyz,smoothstep(_RampParam.z,_RampParam.w,gradient));
				return blendCol;
			}

			half getCameraDepthFade(half viewPosZ)
			{
				half cameraDepthFade = saturate(( viewPosZ -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
				return cameraDepthFade;
			}


			
			
			v2f vert ( appdata v )
			{
				v2f o;
				//UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				//UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.color = v.color;
				float3 viewPos = UnityObjectToViewPos( v.vertex.xyz);
				o.viewPos = viewPos;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldNormal = worldNormal;
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldPos.xyz = worldPos;
				float4 clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(clipPos);
				o.screenPos = screenPos;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				float3 vertexValue =  float3(0,0,0) ;
				#if ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i , half vface : VFACE) : SV_Target
			{
				//UNITY_SETUP_INSTANCE_ID(i);
				//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float3 worldViewDir = UnityWorldSpaceViewDir(i.worldPos);
				
				half Fresnel = getFresnel(i.worldNormal,normalize(worldViewDir));
				Fresnel = lerp(Fresnel,1-Fresnel, _Flip);
				Fresnel = lerp(1,Fresnel,_Usefresnel);

				fixed Mask=1;
				
				UNITY_BRANCH 
				if( USE_MASK )
				{
					half2 uv_MaskTex = i.texcoord.xy * _MaskTex_ST.xy + _MaskTex_ST.zw;
					fixed4 maskTex = tex2D( _MaskTex, uv_MaskTex );
					Mask = maskTex.r;
				}
				
				half2 Coord0,Coord1;
				half Edgewidth , Edgebrightness , Dissolve , Distort ;
				getBlendData(Fresnel,i.texcoord2,i.texcoord1,Coord0,Coord1,Edgewidth , Edgebrightness , Dissolve , Distort);

				fixed Turbulence =getTurbulence(i.texcoord);
				fixed turb = lerp(0,(Turbulence-0.5)*Distort,_UseTurbulence);

				half2 mainUV=i.texcoord* _MainTex_ST.xy + _MainTex_ST.zw;
				half2 blendUV=(Coord0-half2(1,1))*( i.texcoord-half2(0.5,0.5))+mainUV    +Coord1+turb.xx;
				fixed4 mainTex = tex2D( _MainTex, blendUV );
				fixed mainTexChannel=lerp(mainTex.a,mainTex.r,_AlphaR);

				fixed4 blendColor=lerp(lerp(_BackColor,_MainColor,max(0,vface)),_MainColor,_UseBackColor);
				finalColor= blendColor*_Brightness*pow(mainTex,0.8)*i.color;

				half2 ClipData =1;

				UNITY_BRANCH 
				if( USE_CLIP )
				{
					ClipData = getClipData( Dissolve , Edgewidth , mainTexChannel);
				}

				half alpha =saturate (i.color.a*_MainColor.a*mainTexChannel*_Alpha*ClipData.y*Mask*Fresnel);

				UNITY_BRANCH 
				if( DEPTH_MODE )
				{
					clip(alpha-_CUTOUT);
				}
				else
				{
					finalColor.a=alpha;
				}

				half DepthFade=1;
				UNITY_BRANCH 
				if( USE_DEPTHFADE )
				{
					DepthFade = depthFade(i.screenPos);
					finalColor.a*=DepthFade;
				}

				UNITY_BRANCH 
				if( USE_RAMP )
				{
					half3 RampColor = getCustomRamp(i.worldNormal);
					finalColor.xyz*=RampColor;
				}

				UNITY_BRANCH 
				if( USE_CAMERAFADE )
				{
					finalColor.a*=getCameraDepthFade(-i.viewPos.z);
				}
				
				
				return  finalColor ;
			}
			ENDCG
		}
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

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

			ENDCG
		}
	}
}

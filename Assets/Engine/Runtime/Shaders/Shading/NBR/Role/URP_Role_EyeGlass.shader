Shader "URP/Role/EyeGlass"
{
	Properties
	{
		[HDR]_Color0("Main Color", Color) = (1,1,1,1)
		// [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		[NoScaleOffset] _ProcedureTex0 ("xy:Normal z:ThicknessTex ", 2D) = "" {}
        [NoScaleOffset] _ProcedureTex1 ("MatCapTex", 2D) = "white" {}
		[NoScaleOffset]_LocalEnvCube ("Env Cube", Cube) =  "" {}
      
        [HideInInspector]_UVST0("Base Scale Offset", Vector) = (1,1,0,0)
		[HDR]_Color("SpColor", Color) = (1,1,1,1)

		_Param0("x:_FenierEdge y:_FenierIntensity z:_Refintensity w:_BumpMapInt", Vector) = (1,-1,3,1)
		_Param1("x:Speculrint y:Speculrmaskint z:Reflectint w:", Vector) = (1,1,1,1)
		[HideInInspector]_Param(" ", Vector) = (1,0,0,0)
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		
		//CBuffer Fill
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
		[HideInInspector]_UVST1("_UVST1", Vector) = (1,1,1,1)
		[HideInInspector]_Color1("_Color1", Color) = (1,1,1,1)
		[HideInInspector]_Color2("_Color2", Color) = (1,1,1,1)
		[HideInInspector]_Color3("_Color3", Color) = (1,1,1,1)
		[HideInInspector]_Color4("_Color3", Color) = (1,1,1,1)
		[HideInInspector]_Color5("_Color3", Color) = (1,1,1,1)
		[HideInInspector]_Color6("_Color3", Color) = (1,1,1,1)
		[HideInInspector]_ColorOutline("_ColorOutline", Color) = (1,1,1,1)
		[HideInInspector]_GrassCollisionParam("_GrassCollisionParam", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientColor("_HeightGradientColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreivewColor("_HeightGradientPreivewColor", Vector) = (1,1,1,1)
		[HideInInspector]_HeightGradientPreviewParam("_HeightGradientPreviewParam", Vector) = (1,1,1,1)
		[HideInInspector]_LightMapUVST("_LightMapUVST", Vector) = (1,1,1,1)
		[HideInInspector]_RimColortoon("_RimColortoon", Vector) = (1,1,1,1)
		[HideInInspector]_Param2("_Param2", Vector) = (1,1,1,1)
		[HideInInspector]_Param3("_Param3", Vector) = (1,1,1,1)
		[HideInInspector]_Param4("_Param4", Vector) = (1,1,1,1)
		[HideInInspector]_Param5("_Param5", Vector) = (1,1,1,1)
		[HideInInspector]_ProcedureTex1_TexelSize("_ProcedureTex1_TexelSize", Vector) = (1,1,1,1)
		[HideInInspector]_OutlineScale("_OutlineScale", Vector) = (1,1,1,1)
		[HideInInspector]_LaserColor("LaserColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimColor("_SSRimColor", Color) = (0,0,0,0)
		[HideInInspector]_SSRimParam0("_SSRimParam0", Color) = (0,0,0,0)
		[HideInInspector]_MatEffectOutlineParam("_MatEffectOutlineParam", Vector) = (1,1,1,1)
		[HideInInspector]_PBRParam("_PBRParam", Vector) = (1,1,1,1)
		[HideInInspector]_EdgeColor("EdgeColor",color) = (1,1,1,1)
		[HideInInspector]_BackColor("BackColor",color) = (1,1,1,1) 
		[HideInInspector]_AnisotropyParam ("AnisotropyScale:x WarpIntensity:y WarpIntensity:z WarpBendFactor:w", Vector) = (1,3,5,1.5)
        [HideInInspector]_FabricColor1 ("WarpColor:rgb, WarpRoughness:a", Color) = (1,1,1,1)
        [HideInInspector]_FabricColor2 ("WeftColor:rgb, WeftRoughness:a", Color) = (1,1,1,1)
		[HideInInspector]_UVST3("_UVST3", Vector) = (1,1,1,1)
		[HideInInspector] _LocalOffset("pos", Vector) = (0,1,0,1)
		[HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
	}

	HLSLINCLUDE
	
		#define ROLE_EFFECT
		//feature
		#define _UV_SCALE
		#define _ALPHA_BLEND
		#define _VERTEX_COLOR
		#define _OUTPUT_VERTEX_COLOR
		//lighting
		#define _PBS_NO_IBL
		#define _NO_LIGHTMAP
		#define _NO_COLOR_EFFECT
		#define _CUSTOM_MRT
	
		//----------------------
		#define URP_BASE
		#define REDEFINE_URP
		#define BUILTIN_SM_OFF
	
	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Transparent" "PerformanceChecks" = "False" "Queue" = "Transparent" 
			"RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"  }
		LOD 100
		
		Pass
		{
			Name "UniversalForward"
			Tags{ "LightMode" = "UniversalForward" }
			ZWrite off		
			Blend SrcAlpha OneMinusSrcAlpha 
            Stencil
            {
                Ref [_StencilRef_Role]
                Comp Always
                Pass Replace
            }
			HLSLPROGRAM

				// -------------------------------------
				// Universal Pipeline keywords
				#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
				#pragma multi_compile _ _SMARTSOFTSHADOW_ON
	            #pragma multi_compile _ _DITHER_TRANSPARENCY			
			
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "../Include/Pragam_Head.hlsl"
				#ifdef _SM_4
					#pragma target 5.0
				#else//!_SM_4
					#pragma target 3.0
				#endif//_SM_4

				#include "../Scene/Scene_Head.hlsl"
				#include "../Include/URP_LightingHead.hlsl" 

				#define _CUSTOM_MATERIAL
				#define _Normal _ProcedureTex0
                #define _MatCapTex _ProcedureTex1
             	#define _SpColor _Color
				#define _FenierEdge _Param0.x
				#define _FenierIntensity _Param0.y
				#define _Refintensity _Param0.z
				#define _BumpMapInt _Param0.w

				#define _Speculrint _Param1.x
				#define _Speculrmaskint _Param1.y
				#define _Reflectint _Param1.z
               
            //function:			   
			float2 MaTCapUV (in float3 N )
            {
                
                float3 viewnormal = mul((float3x3)unity_WorldToCamera, N );
                // float3 viewdir = normalize(viewpos);
                // float3 viewcross = cross(viewdir,viewnormal);
                // viewnormal= float3(-viewcross.y , viewcross.x , 0.0);
                float2 matcapuv = viewnormal.xy*0.5+0.5;
                return matcapuv;
            }

            float3 RFlerpColor (in float3 rfmatcap , in float thickness )
            {
                float3 c1 = _MainColor.rgb*0.5;
                float3 c2 = rfmatcap*_MainColor.rgb;
                float cmask =thickness;
                return lerp(c1,c2,cmask);
            }

			inline void CustomMaterial(in FFragData FragData,inout FMaterialData MaterialData)
				{
					//diffuse
					FLOAT2 uv = GET_FRAG_UV;
					// FLOAT4 Color =  SAMPLE_TEX2D(_MainTex, uv)*_MainColor;
					// MaterialData.BaseColor = Color;
					// MaterialData.DyeColor = Color.xyz;

					//normal 
					//VertexNormal = FragData.TangentToWorld[2].xyz;
					FLOAT3x3 TangentToWorld = FragData.TangentToWorld;
					FLOAT3 Normal =  SAMPLE_TEX2D(_Normal,uv ).xyz;
					//FLOAT3 Mask =  SAMPLE_TEX2D(_Normal,uv ).w;
					FLOAT2 Normaloff = (Normal.xy*2-FLOAT2(1,1))*_BumpMapInt;
					FLOAT3 NormalLocal = FLOAT3(Normaloff,1);
					FLOAT3 WorldNormal = normalize(mul(NormalLocal, TangentToWorld).xyz);

					//Thickness
					FLOAT Mask =  SAMPLE_TEX2D(_Normal, uv).z;
					FLOAT Thickness = 0*0.75;

                    //Fresnel
					FLOAT3 ViewDir = FragData.CameraVector;
					FLOAT NdV= dot(WorldNormal.xyz,ViewDir);
                    FLOAT EdgeThick = saturate((NdV-_FenierEdge)*_FenierIntensity);
                      //EdgeThick = 1-EdgeThick*_Thickness;
                    FLOAT Thicknessfix = Thickness+EdgeThick;

					//matcap
					FLOAT2 MatcapUV =  MaTCapUV(WorldNormal);
					FLOAT4 MatCapColor = SAMPLE_TEX2D(_MatCapTex, MatcapUV);
					FLOAT4 SpColor = MatCapColor *_SpColor;

					//speculr
					FLOAT3 H =normalize (ViewDir + _MainLightDir.xyz);
					FLOAT speculr = dot(WorldNormal , H );
					speculr = pow(abs(speculr) ,10)*_Speculrmaskint*Mask;

					//reflect
					FLOAT  Refintensity = Thicknessfix * _Refintensity;
                   // FLOAT3 rfmatcap = tex2D(_MatCapTex, MatcapUV+Refintensity);
				    FLOAT3 rfmatcap = SAMPLE_TEX2D(_MatCapTex, MatcapUV+Refintensity).xyz;
                    FLOAT3 rfmatColor = RFlerpColor(rfmatcap,Thicknessfix);

					//refelect
					FLOAT3 ReflectionDir = reflect(-ViewDir, WorldNormal);
					FLOAT4 Reflect = SAMPLE_TEXCUBE(_LocalEnvCube,ReflectionDir)*_Reflectint;

					//Alpha
                    FLOAT4 Color;
				    Color.xyz =  rfmatColor.rgb;
					Color.a =saturate( max( SpColor.r*_SpColor.a,Thicknessfix)*_MainColor.a);
                    //Color.a =1;
										
					MaterialData.DiffuseColor = Color.xyz;					
					MaterialData.SpecularColor = SpColor.xyz*_Speculrint+speculr+Reflect.xyz;									
					MaterialData.BaseColor.a = Color.a;				
					MaterialData.AO = 1;
				}

				#define _CUSTOM_LIGHT
				void CustomLighting(FFragData FragData, FMaterialData MaterialData,
					FShadowData ShadowData,FLOAT ShadowMask,
					inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
				{
					DirectDiffuse = MaterialData.DiffuseColor;
					DirectSpecular = MaterialData.SpecularColor;
				}
				#include "../Include/URP_Vertex.hlsl"
				#include "../Include/URP_Pixel.hlsl"

				// //#pragma shader_feature_local _ _DEBUG_APP
				//#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase

			ENDHLSL
		}	
		Pass
		{
			Name "OverdrawForwardBase"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			ZWrite Off
			Stencil
			{
				Ref[_StencilRef_Role]
				Comp equal
			}
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			// #include "UnityCG.cginc"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			
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
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

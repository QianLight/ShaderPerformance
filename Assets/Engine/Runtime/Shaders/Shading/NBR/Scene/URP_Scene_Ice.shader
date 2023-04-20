Shader "URP/Scene/Ice" {
    Properties {
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		_UVST0("Base Scale Offset", Vector) = (1,1,0,0)
		_Color0("Main Color", Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex0("Normal:rg Roughness:b Metallic:a", 2D) = "black" {}
		_UVST1("PBR Scale Offset", Vector) = (1,1,0,0)
		_Param0("AO:x NormalScale:y RMAdjust:b MetallicEnable:a ", Vector) = (0,0,0,0)
		_Param1("SpecularScale:x Spec:y IBLScale:z Cutout:w", Vector) = (1,0.5,0,0.5)
		_PBRParam("x: Roughness y: Metallic", Vector) = (0,1,0,1)
		
		_Param2("Emissive", Vector) = (0,0,0,0)
		[HDR]_Color1("Emission Color",Color) = (1,1,1,1)

		//custom effect
		[NoScaleOffset]_ProcedureTex1("Overlay", 2D) = "" {}
		_Param3("Overlay", Vector) = (0,0,0,0)
		[HDR]_Color2("OverLay Color",Color) = (1,1,1,1)

		[NoScaleOffset]_ProcedureTex2("Detail", 2D) = "" {}
		_Param4("DetailUVST", Vector) = (0,0,0,0)

        //MOVE
		_Param5("WindDri", Vector) = (0,0,0,0)
		_Param6("x:Speed yzw:", Vector) = (0,0,0,0)
        
		//Ice
		[NoScaleOffset]_ProcedureTex3 ("NoiseTex", 2D) = "white" {}
		[NoScaleOffset]_LocalEnvCube ("Env Cube", Cube) =  "" {}
		[HDR]_Color01("RimColor", Color) = (1,1,1,1)
		_Param00("x:FresnelRange y:FresnelInt z:RefIntensity w:BumpMapInt", Vector) = (0,0,0,0)
		_Param01("x:IceTiling y:WarpScale z:IceSpeed w:LightInt", Vector) = (0,0,0,0)
		_Param02("x:MaskTiling y:HighOffset z:Alpha w:", Vector) = (0,0,0,0)

		_Param("", Vector) = (0,0,0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		[NoScaleOffset]_CustomLightmap("LightMap", 2D) = "" {}
		
		_LightMapUVST("LightMapUVST", Vector) = (1,1,0,0)
    	[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
    	[HideInInspector] _DitherTransparency_On("DitherTransparencyOn", FLOAT) = 0.0
    }

    HLSLINCLUDE
    #define _NO_MRT
    #define _MAIN_COLOR		
    #define _UV_SCALE
    #define _UV_SCALE2		
    #define _SCENE_EFFECT
    #define _PBS_ADJUST
    #define _SCREEN_POS
    #define _VERTEX_GI
    #define _CUSTOM_LIGHT
    ENDHLSL

    SubShader {
        Tags {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" 
        }
    	
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
    		
            HLSLPROGRAM
            //----------------------
            #define URP_BASE
            #define REDEFINE_URP

            //----------------------
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #include "../Include/Pragam_Head.hlsl"
            #ifdef _SM_4
				#pragma target 5.0
            #else
                #pragma target 3.0
            #endif

            TYPE4 _Adjust;
            
            #define _NoiseTex _ProcedureTex3
            #define _RimColor _Color01
            #define _FresnelRange _Param00.x
            #define _FresnelInt _Param00.y
            #define _RefIntensity _Param00.z
            #define _BumpMapInt _Param00.w
            #define _IceTiling  _Param01.x
            #define _WarpScale _Param01.y
            #define _IceSpeed _Param01.z
            #define _LightInt _Param01.w
            #define _MaskTiling _Param02.x
            #define _HighOffset _Param02.y
            #define _Alpha _Param02.z

            #include "../Scene/Scene_Head.hlsl"
            #include "../Include/URP_LightingHead.hlsl"

            #include "../Include/URP_Vertex.hlsl"

            void CustomLighting(FFragData FragData, FMaterialData MaterialData,
                                FShadowData ShadowData,FLOAT ShadowMask,
                                inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
            {
                FLightingContext LC = InitLight(FragData, MaterialData,_SpecMult,
                                                _MainLightDir.xyz,_MainLightColor.xyz, 1, ShadowData, _ShadowColor.xyz);
                SM_SupplementDiffuse(LC, DirectDiffuse DEBUG_PARAM);
                SM_SupplementSpec(LC, DirectSpecular DEBUG_PARAM);

                FLOAT2 uv = GET_FRAG_UV;
                FLOAT4 color = SAMPLE_TEX2D(_MainTex, uv);
                FLOAT4 WorldPosition = FragData.WorldPosition;
                FLOAT2 distroy = FLOAT2(SAMPLE_TEX2D(_NoiseTex, WorldPosition.xy*_IceTiling-_Time.x*_IceSpeed).r, 0);
                FLOAT4 Color = SAMPLE_TEX2D(_NoiseTex, WorldPosition.xy*_IceTiling+distroy*_WarpScale) * _MainColor *
                    _LightInt;
                FLOAT3 WorldNormal = MaterialData.WorldNormal;
                FLOAT NdL = max(0, dot(WorldNormal.xyz, _MainLightDir.xyz)) * 0.5 + 0.5;

                //Fresnel
                FLOAT3 ViewDir = FragData.CameraVector;
                FLOAT NdV = max(0, dot(WorldNormal.xyz, ViewDir));
                NdV = pow(abs(1 - NdV),_FresnelRange) * _FresnelInt;

                //reflect
                FLOAT3 ReflectionDir = reflect(-ViewDir, WorldNormal);
                FLOAT4 Reflect = SAMPLE_TEXCUBE(_LocalEnvCube, ReflectionDir) * _RefIntensity;

                WorldPosition.y = 1 - (WorldPosition.y - _HighOffset);
                FLOAT MaskrRange = SAMPLE_TEX2D(_NoiseTex, uv*_MaskTiling).r;
                MaskrRange = 1 - saturate(WorldPosition.y - MaskrRange);
                FLOAT3 FinalSpecular = NdV * _RimColor.xyz + Reflect.xyz;
                DirectDiffuse = lerp(DirectDiffuse * NdL * 1.5, Color, MaskrRange);
                DirectSpecular = lerp(0, FinalSpecular, MaskrRange);
            }

            #include "../Include/SRP_Lighting.hlsl"
            #include "../Include/URP_Pixel.hlsl"

            //debug
            //#pragma shader_feature_local _ _DEBUG_APP

            //render type
            #pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST

            //effect
            #pragma shader_feature_local _ _CUSTOM_EFFECT

            #pragma shader_feature_local _ _LOD0 _LOD1 _LOD2 _LOD_FAR
            #pragma shader_feature_local _ _LOD_M0 _LOD_M1 _LOD_M2 _LOD_MFAR
            #pragma shader_feature_local _ _LOD_L0 _LOD_L1 _LOD_L2 _LOD_LFAR

			#pragma multi_compile_local _ _DITHER_TRANSPARENCY
            
            #pragma vertex vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }
//        Pass {
//            Name "OverdrawF"
//            Tags {
//                "LightMode" = "OverdrawForwardBase"
//            }
//
//            Blend One One
//            CGPROGRAM
//            #pragma vertex Vert
//            #pragma fragment Frag
//
//            #include "UnityCG.cginc"
//
//            struct Attributes
//            {
//                float4 vertex : POSITION;
//            };
//
//            struct Varyings
//            {
//                float4 vertex : SV_POSITION;
//            };
//
//            Varyings Vert(Attributes v)
//            {
//                Varyings o;
//                float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
//                o.vertex = mul(unity_MatrixVP, WorldPosition);
//                return o;
//            }
//
//            half4 Frag(Varyings i) : SV_Target
//            {
//                return half4(0.1, 0.04, 0.02, 1);
//            }
//            ENDCG
//        }

    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
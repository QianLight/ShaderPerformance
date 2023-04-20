Shader "URP/Role/Ice"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
        [NoScaleOffset]_ProcedureTex0 ("xy:Normal z:ThicknessTex ", 2D) = "" {}

        [NoScaleOffset]_ProcedureTex3 ("NoiseTex", 2D) = "white" {}
        [NoScaleOffset]_LocalEnvCube ("Env Cube", Cube) = "" {}

        [HDR]_Color00("Main Color", Color) = (1,1,1,1)
        [HDR]_Color01("RimColor", Color) = (1,1,1,1)

        _Param00("x:FresnelRange y:FresnelInt z:RefIntensity w:BumpMapInt", Vector) = (0,0,0,0)
        _Param01("x:IceTiling y:WarpScale z:IceSpeed w:LightInt", Vector) = (0,0,0,0)
        _Param02("x:MaskTiling y:HighOffset z:Alpha w:", Vector) = (0,0,0,0)

        [HideInInspector] _DebugMode("__debugMode", FLOAT) = 0.0
        [HideInInspector] _ShadowPos("ShadowPos", Vector) = (0,0,0,0)
    }

    HLSLINCLUDE
    //feature
    #define _ALPHA_BLEND
    #define _VERTEX_COLOR
    #define _OUTPUT_VERTEX_COLOR
    #define _PBS_NO_IBL
    #define _NO_LIGHTMAP
    #define _NO_COLOR_EFFECT
    #define _NO_CLAC_DEPTH
    
    #define URP_BASE
	#define REDEFINE_URP
    #define BUILTIN_SM_OFF

    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

//        Pass
//        {
//            Name "PreZ"
//            Tags
//            {
//                "LightMode" = "PreZ" "RenderType" = "Opaque" "PerformanceChecks" = "False"
//            }
//            ColorMask 0
//            ZWrite On
//            ZTest LEqual
//
//            HLSLPROGRAM
//            #pragma target 3.0
//
//            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
//            #include "../Include/PCH.hlsl"
//            #include "../Include/URP_LightingHead.hlsl"
//            #include "../Include/URP_Vertex.hlsl"
//
//
//            #define _CUSTOM_PS
//
//            FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout FLOAT4 rt1)
//            {
//                rt1.xyz = EncodeFloatRGB(Interpolants.Depth01.x / Interpolants.Depth01.y);
//                rt1.w = EncodeAlpha(1, 0);
//                return 0;
//            }
//
//            #include "../Include/URP_Pixel.hlsl"
//            //#pragma shader_feature_local _ _DEBUG_APP
//
//            #pragma vertex vertForwardBase
//            #pragma fragment fragForwardBase
//            ENDHLSL
//        }

        Pass
        {
            Name "UniversalForward"
            Tags
            {
                "LightMode" = "UniversalForward" "RenderType" = "Transparent" "Queue" = "Transparent-10"
            }
            Blend SrcAlpha OneMinusSrcAlpha
//            ZWrite Off
//            ZTest Equal

            HLSLPROGRAM

            #define _ROLE_ADDITIONAL_LIGHT_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "../Include/Pragam_Head.hlsl"
            #ifdef _SM_4
                #pragma target 5.0
            #else
				#pragma target 3.0
            #endif

            #include "../Scene/Scene_Head.hlsl"
            #include "../Include/URP_LightingHead.hlsl"

            
            // REAL4 _Color01;
            // REAL4 _Param00;
            // REAL4 _Param01;
            // REAL4 _Param02;
            #define _Normal _ProcedureTex0
            #define _MatCapTex _ProcedureTex1
            #define _NoiseTex _ProcedureTex3
            #define _ColorMain _Color00
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
            #define _CUSTOM_MATERIAL

            //function:			   
            REAL2 MaTCapUV(in REAL3 N)
            {
                REAL3 viewnormal = mul((FLOAT3x3)_matrixV, N);
                REAL2 matcapuv = viewnormal.xy * 0.5 + 0.5;
                return matcapuv;
            }

            REAL3 RFlerpColor(in REAL3 rfmatcap, in REAL thickness)
            {
                REAL3 c1 = _ColorMain.rgb * 0.5;
                REAL3 c2 = rfmatcap * _ColorMain.rgb;
                REAL cmask = thickness;
                return lerp(c1, c2, cmask);
            }

            inline void CustomMaterial(in FFragData FragData, inout FMaterialData MaterialData)
            {
                //diffuse
                REAL2 uv = GET_FRAG_UV;
                FLOAT4 WorldPosition = FragData.WorldPosition;
                REAL4 MainColor = SAMPLE_TEX2D(_MainTex, uv);
                REAL2 distroy = REAL2(SAMPLE_TEX2D(_NoiseTex, WorldPosition.xy*_IceTiling-_Time.x*_IceSpeed).r, 0);
                REAL4 Color = SAMPLE_TEX2D(_NoiseTex, WorldPosition.xy*_IceTiling+distroy*_WarpScale) * _ColorMain *
                    _LightInt;

                //normal 
                REAL3x3 TangentToWorld = FragData.TangentToWorld;
                REAL4 NormalMap =  SAMPLE_TEX2D(_Normal, uv);
                REAL3 Normal = NormalMap.xyz;
                REAL Mask = NormalMap.w;
                REAL2 Normaloff = (Normal.xy * 2 - FLOAT2(1, 1)) * _BumpMapInt;
                REAL3 NormalLocal = FLOAT3(Normaloff, 1);
                REAL3 WorldNormal = normalize(mul(NormalLocal, TangentToWorld).xyz);
                REAL NdL = max(0, dot(WorldNormal.xyz,_MainLightDir)) * 0.5 + 0.5;

                //Fresnel
                REAL3 ViewDir = FragData.CameraVector;
                REAL NdV = max(0, dot(WorldNormal.xyz, ViewDir));
                NdV = pow(abs(1 - NdV),_FresnelRange) * _FresnelInt;

                // //matcap
                // REAL2 MatcapUV =  MaTCapUV(WorldNormal);
                // REAL4 MatCapColor = SAMPLE_TEX2D(_MatCapTex, MatcapUV);
                // REAL4 SpColor = MatCapColor.r *_SpColor;

                //reflect
                REAL3 ReflectionDir = reflect(-ViewDir, WorldNormal);
                REAL4 Reflect = SAMPLE_TEXCUBE(_LocalEnvCube, ReflectionDir) * _RefIntensity;

                //Finalcolor
                //Color.xyz +=  SpColor.xyz;
                WorldPosition.y = 1 - (WorldPosition.y - _HighOffset);
                REAL MaskrRange = SAMPLE_TEX2D(_NoiseTex, uv*_MaskTiling).r;
                MaskrRange = saturate(WorldPosition.y - MaskrRange);

                REAL3 FinalSpecular = NdV * _RimColor.xyz + Reflect.xyz;
                FinalSpecular = lerp(0, FinalSpecular, MaskrRange);
                Color = lerp(MainColor * NdL * 1.5, Color, MaskrRange);

                MaterialData.DiffuseColor = Color.xyz;
                MaterialData.SpecularColor = FinalSpecular;
                MaterialData.BaseColor.a = lerp(1, lerp(Reflect.r, 1,_Alpha), MaskrRange);
                MaterialData.AO = 1;
            }

            #define _CUSTOM_LIGHT

            void CustomLighting(FFragData FragData, FMaterialData MaterialData,
                                FShadowData ShadowData,FLOAT ShadowMask,
                                inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
            {
                DirectDiffuse = MaterialData.DiffuseColor;
                DirectSpecular = MaterialData.SpecularColor;
            }

            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"

            //#pragma shader_feature_local _ _DEBUG_APP

            #pragma vertex vertForwardBase
            #pragma fragment fragForwardBase
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

			ZWrite On
			ZTest LEqual
			ColorMask 0
			Cull[_Cull]

			HLSLPROGRAM

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_local _ _DITHER_TRANSPARENCY
			#include "URP_Role_DepthOnly.hlsl"

            ENDHLSL
        }
        
//        Pass
//        {
//            Name "OverdrawZ"
//            Tags
//            {
//                "LightMode" = "OverdrawPreZ" "RenderType"="Opaque" "PerformanceChecks" = "False"
//            }
//            //				ColorMask 0
//            //				ColorMask RGBA 1
//
//            Blend One One
//            HLSLPROGRAM
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
//            ENDHLSL
//        }
//        Pass
//        {
//            Name "OverdrawForwardTransparent"
//            Tags
//            {
//                "LightMode" = "OverdrawForwardTransparent" "Queue" = "Transparent-10" "RenderType" = "Transparent"
//            }
//
//            Blend One One
//            ZWrite Off
//            ZTest Equal
//
//            HLSLPROGRAM
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
//            ENDHLSL
//        }
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

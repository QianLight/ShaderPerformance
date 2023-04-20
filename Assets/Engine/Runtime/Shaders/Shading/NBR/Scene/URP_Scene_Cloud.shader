Shader "URP/Scene/Cloud"
{
    Properties
    {
        _Color0("Main Color", Color) = (1,1,1,1)
        [NoScaleOffset]_MainTex1("LengthTex", 2D) = "white" {}
        _AlphaTex3D ("AlphaTex3D", 2D) = "white" {}

        _Param0("x:LengthMin  y:LengthMax x:Shading y:Density", Vector) = (0,0.5,1,0)
        _Param1("x:Speed y:NoiseScale z:_FurLengthMin w:_FurLengthMax", Vector) = (0.1,1,0,0)

        _FurFadeParam("x:distance y:width z:maxFade",Vector) = (0,50,0,0)

        _Param2(":", Vector) = (1,4,0,0)
        _Color1("DarkColor", Color) = (0,0,0,0)

        //lightning

        _Color2("EmissiveColor", Color) = (1,1,1,1)
        _Param3("x:EmissiveInt y:EmissiveRange z:EmissiveSpeed w:", Vector) = (0,0,0,0)

        _Param("", Vector) = (0,0,0,0)
        _Color("Effect Color", Color) = (1,1,1,1)

        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _DebugMode("__debugMode", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }

    HLSLINCLUDE
    //feature
    #define _MAIN_COLOR
    #define _NO_NORMAL_MAP
    #define _SCENE_EFFECT
    #define _INSTANCE
    #define _SIMPLE_MATRIX
    #define _GPU_ANIMATION	
    #define _CUSTOM_INSTANCE_ID
    #define _CUSTOM_GPU_POS
    #define _CUSTOM_VERTEX_PARAM
    
    //lighting
    //#define _VERTEX_GI
    #define _NO_COMMON_EFFECT
    #define _NO_ADDLIGHT
    ENDHLSL

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

        ZWrite[_ZWrite]
//        ZWrite Off
        Cull back

        Pass
        {
            Name "FORWARD"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            Blend[_SrcBlend][_DstBlend]



            HLSLPROGRAM
            //----------------------
            #define URP_BASE
            #define REDEFINE_URP
	        #define MAIN_LIGHT_CALCULATE_SHADOWS
            #define _NO_LIGHTMAP
			#define _SMARTSOFTSHADOW_ON 1
            #pragma multi_compile_instancing
            // -------------------------------------
            // Universal Pipeline keywords
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS 
            //  || defined(_SHADER_LEVEL_LOW)
            #pragma multi_compile _SHADER_LEVEL_HIGH _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW _SHADER_LEVEL_VERY_LOW
            #if defined(_SHADER_LEVEL_VERY_HIGH)
                    #define _MAIN_LIGHT_SHADOWS
            #elif defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_MEDIUM)
                    #define _MAIN_LIGHT_SHADOWS
            #endif

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            //#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            //#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            //#pragma multi_compile _ LIGHTMAP_ON
            #define CUMTOM_PRECISION
            #define FOG_NOISE_OFF
            //---------------------- 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Include/Pragam_Head.hlsl"

            #ifdef _SM_4
            #pragma target 5.0
            #else//!_SM_4
					#pragma target 3.0
            #endif//_SM_4

            #include "../Scene/Scene_Head.hlsl"
            #include "../Include/URP_LightingHead.hlsl"

            #define _FurLengthTex _MainTex1
            #define _FurLengthMin _Param1.z
            #define _FurLengthMax _Param1.w

            #define _FurLength _Param2.x
            #define _InstanceCount _Param2.y

            float4 _FurFadeParam;
            float _CloudAlpha,_CloudLength;



            uint GetCustomInstanceID(uint instanceID)
            {
                return instanceID + (uint)_InstanceOffset;
            }

            FLOAT4 CustomGPUPos(in FVertexInput Input,REAL4 localPos INSTANCE_ID)
            {
                FLOAT4 FurLength = SAMPLE_TEX2D_LOD(_FurLengthTex, Input.uv0, 0);

                float fadeDist = _FurFadeParam.x;
                float fadeWidth = _FurFadeParam.y / _InstanceCount * 12;
                // float fadeMaxFur = _FurFadeParam.z;
                float3 worldPos = TransformObjectToWorld(localPos);
                float depth = -TransformWorldToView(worldPos).z;
				float cameraDepthFade = max(0,( depth -_ProjectionParams.y - fadeDist ) / fadeWidth);

                FurLength.r = _FurLengthMin + FurLength.r * (_FurLengthMax - _FurLengthMin);
                FurLength.r *= cameraDepthFade;
                FLOAT3 PosOffset = (_FurLength+_CloudLength) * 0.01 / (_InstanceCount + 1) * FurLength.r * (Input.TangentX.xyz * (
                    FLOAT)instanceID)+ localPos.xyz;
                
                return  FLOAT4(PosOffset, 1);
                
            }

            #define _DarkColor _Color1

            #define _Speed _Param0.x
            #define _NoiseScale _Param0.y
            #define _Density _Param0.z

            #define _FurShading _Param1.x
            #define _BackSSSRange _Param1.y
            #define _BackSSSInt _Color1.w


            #define _EmissiveColor _Color2
            #define _EmissiveInt _Param3.x
            #define _EmissiveSpeed _Param3.z


            TEX2D_SAMPLER(_AlphaTex3D);

            inline void MassiveBaseColor(in FFragData FragData, inout FMaterialData MaterialData)
            {
                REAL2 uv = GET_FRAG_UV;
                REAL3 color = _MainColor.xyz;

                REAL t = (FragData.InstanceID + 1) / (_InstanceCount + 1);
                REAL shading = 1 - t;
                shading *= shading * _FurShading;
                REAL3 c = color - shading.xxx;

                //noise
                REAL2 flowUV = uv * _NoiseScale + _Time.x * _Speed;
                REAL noise = SAMPLE_TEX2D(_AlphaTex3D, flowUV).r;
                noise = pow(noise, 1.5);

                #ifdef _USE_LIGHTNING
						//Emissive
						REAL EmissiveColor = 1-SAMPLE_TEX2D(_AlphaTex3D, flowUV).g;
						//FLOAT Speed = frac(cos( _Time.y*_EmissiveSpeed ));					
						REAL Speed = frac(max(cos( _Time.y*_EmissiveSpeed*5) + sin(_Time.y*_EmissiveSpeed/2) , 0));
						//Speed = Speed * 0.5 + 0.5;
						EmissiveColor *= Speed;
						//EmissiveColor = sin(EmissiveColor * _Time.x );
						EmissiveColor = pow(abs(EmissiveColor),4);
						EmissiveColor = EmissiveColor *shading.x;
						EmissiveColor *=_EmissiveInt*1000; 
						c += EmissiveColor*_EmissiveColor.xyz;
                #endif

                REAL alpha = saturate((noise - t * t * _Density))+_CloudAlpha;
                alpha = saturate(alpha * (1 - t));
             
					REAL shadow = 1;
					
				#ifdef _MAIN_LIGHT_SHADOWS
                    REAL4 ShadowCoords = TransformWorldToShadowCoord(FragData.WorldPosition);
                    half realtimeShadow = SampleShadowNoPcf(ShadowCoords);
                    half shadowFade = GetShadowFade(FragData.WorldPosition);
                    shadow = lerp(realtimeShadow, 1, saturate(shadowFade));
				#endif
                   // float smartShadow = GetSmartShadow(_MainLightPosition.xyz, MaterialData.WorldNormal,FragData.WorldPosition, _SmartShadowIntensity);
				   // shadow = min(shadow, smartShadow);
                //lerp(c,shadow*c,alpha)
                    MaterialData.BaseColor = REAL4(lerp(c, shadow*c, alpha*2), alpha);
                    MaterialData.DyeColor = MaterialData.BaseColor.rgb;
                
            }

            #define _CUSTOM_BASECOLOR
            #define CalcCustomBaseColor MassiveBaseColor

            //---------------------------------------------------------------------------------
            REAL GetMassiveShadowMapMask(in FFragData FragData, in FMaterialData MaterialData)
            {
                return 1;
            }

            #define GetCustumShadowMapMask GetMassiveShadowMapMask
            #define _CUSTOM_SHADOW_MAP_MASK 

            inline void CustomVertex(in FVertexInput Input, inout FInterpolantsVSToPS Interpolants)
			{
            	
			}
            
            void CalcMassiveLighting(FFragData FragData, FMaterialData MaterialData, FShadowData ShadowData,
                                     FLOAT ShadowMask, inout REAL3 DirectDiffuse,
                                     inout REAL3 DirectSpecular DEBUG_ARGS)
            {
                //_MainLightDir.xyz *= -1;
                REAL NL = saturate(dot(MaterialData.WorldNormal, _MainLightDir.xyz));
                REAL3 BackDirector = MaterialData.WorldNormal * _BackSSSRange + _MainLightDir.xyz;
                REAL BackSSS = saturate(dot(FragData.CameraVector, -BackDirector));
                REAL2 Shadow = ShadowData.Shadow.xy;
                //Shadow.x = saturate(lerp(Shadow.x, 1.0, saturate((distance(FragData.WorldPosition, _WorldSpaceCameraPos.xyz) - 20) * 0.1)));

                REAL3 color = MaterialData.BaseColor.xyz;
                REAL t = (FragData.InstanceID + 1) / (_InstanceCount + 1);
                REAL SmoothNL = saturate(pow(abs(NL), 2 - t));
                BackSSS = saturate(pow((BackSSS), 2 + t * 2) * _BackSSSInt);
                REAL SmoothNV = saturate(pow(abs(MaterialData.NdotV), 2 - t));
                //color = lerp( _DarkColor , color , NL)+BackSSS+SmoothNV*0.15;
                color = lerp(_DarkColor.xyz, color, NL);
                DirectDiffuse = (color + BackSSS + SmoothNV * 0.15) * _MainLightColor.xyz;
                DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, MaterialData.WorldNormal)
                DirectSpecular = 0;
                DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
            }

            #define _CUSTOM_LIGHT
            #define CustomLighting CalcMassiveLighting

            #define  CalcSceneColor(a,b) CalculateSceneColor(a,b)
            inline void CalculateSceneColor(inout FLOAT3 color,FLOAT srcShadow)
			{	
				
			}

            #define  CalcIBL(a,b) CalculateIBL(a,b)
            inline FLOAT3 CalculateIBL(in FFragData FragData,in FMaterialData MaterialData DEBUG_ARGS)
			{	
				return  FLOAT3(0,0,0);
			}
            
            //---------------------------------------------------------------------------------
            #include "../Include/URP_Vertex.hlsl"
            #include "../Include/URP_Pixel.hlsl"

            REAL4 fragForwardBaseCloud(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, REAL facing : VFACE) : SV_Target
			{

				//return REAL4(vs2ps.InstanceID*0.03,0,0,1);
				
				//if(vs2ps.InstanceID==_InstanceCount)
				//{
					//return REAL4(1,0,0,1);
				//}
				
				REAL4 rt0 = 0;
				REAL4 rt1 = REAL4(0, 0, 0, _IsRt1zForUIRT);
				Frag(vs2ps,SvPosition,rt0,rt1,facing);
				return rt0;
			}

            void vertForwardBaseCloud(FVertexInput Input, uint instanceID : SV_InstanceID, out FMobileShadingVSToPS Output)
            {
	            vertForwardBase(Input,instanceID,Output);
            	
            }
            
            //debug
            //#pragma shader_feature_local _ _DEBUG_APP

            //render type
            #pragma shader_feature_local _ _USE_LIGHTNING
            #pragma enable_d3d11_debug_symbols
            
            
            #pragma vertex vertForwardBaseCloud
            #pragma fragment fragForwardBaseCloud
            ENDHLSL
        }
    }
   CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
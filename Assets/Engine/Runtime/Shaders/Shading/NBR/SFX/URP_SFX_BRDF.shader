Shader "URP/SFX/BRDF"
{
    Properties
    {
        [Header(baseColor)]
        _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [Toggle] _AlphaTest("Alpha Test", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Header(NORMAL)]
        [NoScaleOffset]_BumpMap("xy:Normal z:Roughness w:Metallic", 2D) = "bump" {}
        _BumpScale("BumpScale", Range(-1, 1.0)) = 1.0
        _BumpZ("_BumpZ",Range(0.0, 1.0))=0
        _BumpW("_BumpW",Range(0.0, 1.0)) = 0
        [XToggle] _Metallic("Metallic", Float) = 0.0
        _MetallicMin("Metallic Min", Range(0.0, 1.0)) = 0.0
        _MetallicMax("Metallic Max", Range(0.0, 1.0)) = 1.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 1.0
        _RoughnessMin("_RoughnessMin", Range(0.0, 1.0)) = 1.0
        _RoughnessMax("_RoughnessMax", Range(0.0, 1.0)) = 1.0
        [Header(AO)]
        [XToggle] _Occlusion("Occlusion", Float) = 0.0
        _OcclusionScale("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        _AoMap ("_AoMap", 2D) = "white" {}
        [Header(Emission)]
        [XToggle] _Emission("Emission", Float) = 0.0
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0)


        [Header(Advance)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(Off, 0, On, 1)]_ZWrite("ZWrite", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            ZTest[_ZTest]
            Cull[_Cull]
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            #define _NORMALMAP

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 tangentOS : TANGENT;
                
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 normalWS : TEXCOORD3;
                float3 vertexSH : TEXCOORD4;
                float4 tangentWS : TEXCOORD5;
            };
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor,_EmissionColor;
            half _BumpScale;
            half _OcclusionScale;
            half _Emission;
            half _Occlusion;
            half _Metallic;
            half _Cutoff;
            half _Smoothness;
            half _BumpZ,_BumpW;
            half _RoughnessMin,_RoughnessMax;
            half _MetallicMin,_MetallicMax;
            CBUFFER_END
            
            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_AoMap);            SAMPLER(sampler_AoMap);

            #include "Assets/Engine/Runtime/Shaders/Shading/Include/OPPInput.hlsl"
            #include "PbrFunction_01.hlsl"
            // 初始化标准照明表面数据
            inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)//Add a input "PositionColor" for vertical Gradient color
            {
                half4 albedo = Sample2D(uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));

                #ifdef _NORMALMAP
                    half4 bump = Sample2D(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
                #else
                    half4 bump =  half4(0.5,0.5, _BumpZ, _BumpW);
                #endif

                outSurfaceData.alpha = GetAlpha(albedo, _Cutoff);
                outSurfaceData.albedo = albedo.rgb * _BaseColor.rgb;
                //outSurfaceData.metallic = bump.w * _Metallic;
                outSurfaceData.metallic = GetMetallic(bump.w, _MetallicMin, _MetallicMax);;
                outSurfaceData.specular = 0;
                outSurfaceData.smoothness = GetSmoothness(bump.z, _RoughnessMin, _RoughnessMax) * _Smoothness;
                //outSurfaceData.smoothness = (1-bump.z)*_Smoothness;
                outSurfaceData.normalTS = GetNormal(bump.xy, _BumpScale);
                //outSurfaceData.occlusion = GetOcclusion(bump.w, _OcclusionScale);
                outSurfaceData.occlusion = SAMPLE_TEXTURE2D(_AoMap, sampler_AoMap, uv).r;
                outSurfaceData.clearCoatMask = 0.0h;
                outSurfaceData.clearCoatSmoothness = 0.0h;
                outSurfaceData.darkValueInRain = 0;

               #ifdef _EMISSION_ON
                    half4 emission = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, uv, 0);
                    outSurfaceData.emission = GetEmission(emission, _EmissionColor);
                #else
                    outSurfaceData.emission = GetEmission(albedo, _EmissionColor);
                #endif
            }
            // 初始化输入数据
            void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
            {
                inputData = (InputData)0;
            #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                inputData.positionWS = input.positionWS;
            #endif

                half3 viewDirWS = SafeNormalize(input.viewDirWS);
            #if defined(_NORMALMAP) || defined(_DETAIL)
                float sgn = input.tangentWS.w;      // should be either +1 or -1
                float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                float3x3 tbn = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
                inputData.normalWS = TransformTangentToWorld(normalTS, tbn);
            #else
                inputData.normalWS = input.normalWS;
            #endif

                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = viewDirWS;

            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                inputData.shadowCoord = input.shadowCoord;
            #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
            #else
                inputData.shadowCoord = float4(0, 0, 0, 0);
            #endif

            }
            
            Varyings vert (Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = (VertexPositionInputs)0;//GetVertexPositionInputs(input.positionOS.xyz);
	            vertexInput.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	            vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = viewDirWS;
                
                #if defined(_NORMALMAP)
                    real sign = input.tangentOS.w * GetOddNegativeScale();
                    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
                    output.tangentWS = tangentWS;
                #endif

                #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
                    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
                    output.viewDirTS = viewDirTS;
                #endif

                OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

                #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                    output.positionWS = vertexInput.positionWS;
                #endif

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif

                output.positionCS = vertexInput.positionCS;

                return output;
            }
            half4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                SurfaceData surfaceData;
                InitializeStandardLitSurfaceData(input.uv, surfaceData);
                //return surfaceData.smoothness;
                InputData inputData;
                InitializeInputData(input, surfaceData.normalTS, inputData);
                half3 ref = reflect(-inputData.viewDirectionWS,inputData.normalWS);
                Light myligth = GetMainLight(inputData.shadowCoord);
                half3 worldLight = normalize(myligth.direction.xyz);
                half3 halfDir = normalize(worldLight + inputData.viewDirectionWS);
                float nl = max(saturate(dot(inputData.normalWS, worldLight)), 0.000001);
                float nv = max(saturate(dot(inputData.normalWS, inputData.viewDirectionWS)), 0.000001);
                float vh = max(saturate(dot(inputData.viewDirectionWS, halfDir)), 0.000001);
                float lh = max(saturate(dot(worldLight, halfDir)), 0.000001);
                float nh = max(saturate(dot(inputData.normalWS, halfDir)), 0.000001);
                float smoothnesss = surfaceData.smoothness;
                float perceptualRoughness = 1-smoothnesss;
                float roughness = perceptualRoughness*perceptualRoughness;
                float squareRoughness = roughness * roughness;
                // DirectLightResult
                float D = D_Function(nh,roughness);
                float G = G_Function(nl,nv,squareRoughness);//return G;//越粗糙吸收能量越多，也就越暗
                float3 F0 = lerp(half3(0.04,0.04,0.04), surfaceData.albedo, surfaceData.metallic);
                float3 F = F_Function(lh,F0);
                float3 BRDFSpeCol = (D * G * F * 0.25);
                float3 SpecularCol =  BRDFSpeCol  * nl * PI * myligth.color.rgb;
                half halfLambert = max(0,dot(worldLight,inputData.normalWS)*0.5+0.5);
                half3 kd = ((1 - F)*(1 - min(surfaceData.metallic,0.5)));//min0.5 是为了提亮金属
                half3 DiffuseCol = nl * surfaceData.albedo * myligth.color.rgb*kd;//能量守恒
                //return half4(kd,1);
                if(_Occlusion)
                {
                    DiffuseCol *= lerp(1,surfaceData.occlusion,_OcclusionScale);
                }
                float3 DirectLightResult = (DiffuseCol + SpecularCol);
                half3 shadowAtt = max(0,myligth.shadowAttenuation);
                DirectLightResult *= shadowAtt;
                // IndirectResult
                float3 SHcolor = SH_IndirectionDiff(inputData.normalWS);
                //float3 SHcolor = SampleSH(inputData.normalWS);
                float3 IndirKS = IndirF_Function(nv,F0,roughness);
                float3 IndirKD = (1-IndirKS) * (1-min(surfaceData.metallic,0.5));//min0.5 提亮金属
                float3 IndirDiffColor = SHcolor  * surfaceData.albedo * IndirKD;
                float3 IndirSpeCubeColor = IndirSpeCube(inputData.normalWS,inputData.viewDirectionWS,roughness);
                float3 IndirSpeCubeFactor = IndirSpeFactor(roughness,smoothnesss,BRDFSpeCol,F0,nv);
                float3 IndirSpeColor = IndirSpeCubeColor * IndirSpeCubeFactor;
                float3 IndirectResult = IndirDiffColor + IndirSpeColor;
                // DirectLightResult + IndirectResult
                float4 result = float4(DirectLightResult + IndirectResult, surfaceData.alpha);
                //return half4(IndirDiffColor,1);
                return result;
            }
            ENDHLSL
        }
    }
}

Shader "Unlit/River"
{
    Properties
    {
        _Opacity("_Opacity",range(0,1)) = 1
        _NormalTex("NormalTex",2D) = "bump" {}
        _NormalParam("XYZ周期和幅度,w:scale",vector) = (0.01,0.02,0.03,0.5)
        _CubeMap("CubeMap",Cube) = ""{}
        _CubeMapSmooth("CubeMapSmooth",float) = 10
        _gloss("gloss",float) = 0.17
        _shininess("shininess",float) = 200
        _Distortion("Distortion",float) = 0.02
        _FresnelPower("FresnelPower",float) = 1.45
        _ShallowCol("_ShallowCol",Color) = (0.5788,0.9387,0.9528,1)
        _DeepCol("DeepCol", Color) = (0.408, 0.677, 0.952, 1)
        _DepthMaxDistance("DepthMaxDistance",float) = 1.48
        _DeepStength("DeepStength",float) = 1
        _FoamNoiseTex("FoamNoiseTex", 2D) = "white" {}
        _FoamCol("FoamCol",Color) = (1,1,1,1)
        _FoamWidth("FoamWidth",float) = 0.4
        _FoamStep("FoamStep",float) = 1
 _FoamSpeed("FoamSpeed",float) = 0.5
        _CausticScale("_CausticScale",float) = 2.5
        _CausticPow("CausticPow",float) = 1.28
    }
        SubShader
        {
            Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent"}
            LOD 100

            Pass
            {

                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off


                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #pragma multi_compile _FX_LEVEL_HIGH

                #if defined(_FX_LEVEL_HIGH)
                    #define USE_DEPTH_TEX
                #endif

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float4 tangent : TANGENT;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 normal : TEXCOORD1;
                    float4 tangent : TEXCOORD2;
                    float4 bitangent : TEXCOORD3;

                };

                CBUFFER_START(UnityPerMaterial)
                float4 _NormalTex_ST;
	            float4 _FoamNoiseTex_ST;
                half4 _NormalParam;
                half4 _ShallowCol;
                half4 _DeepCol;
                half4 _FoamCol;
                half _CubeMapSmooth;
                half _DepthMaxDistance;
                half _gloss;
                half _shininess;
                half _Distortion;
                half _FresnelPower;
                half _FoamStep;
                half _FoamWidth;
                half _DeepStength;
                half _CausticScale;
                half _CausticPow;
                half _FoamSpeed;
                half _Opacity;
                CBUFFER_END
                sampler2D _NormalTex;
                sampler2D _FoamNoiseTex;
                sampler2D _CameraOpaqueTexture;
                samplerCUBE _CubeMap;
                TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);


                float3 SampleWaterNormal(float2 uv) {
                    float3 normal = UnpackNormal(tex2D(_NormalTex, uv + float2(_Time.y * _NormalParam.x, sin(_Time.y) * _NormalParam.y)));
                    normal.xy *= _NormalParam.w;
                    float3 normal2 = UnpackNormal(tex2D(_NormalTex, uv + float2(_Time.y * _NormalParam.z, 0)));
                    normal2.xy *= _NormalParam.w;
                    float3 n = normal + normal2;
                    n = normalize(n);
                    return n;
                }

                half3 Specular(float3 viewDir, float3 normal, float gloss, float shininess, Light mainLight) {
                    float3 halfDir = normalize(mainLight.direction + viewDir);
                    float nl = saturate(dot(halfDir, normal));
                    return gloss * pow(nl, shininess) * mainLight.color;
                }

                half3 Diffuse(float3 normal,Light mainLight) {
                    float nl = saturate(dot(mainLight.direction, normal));
                    return nl * mainLight.color * mainLight.shadowAttenuation;
                }

                half3 SampleSkybox(samplerCUBE cube, float3 normal, float3 viewDir, float smooth) {
                    float3 adjustNormal = float3(normal);
                    adjustNormal.xz /= smooth;
                    float3 refDir = reflect(-viewDir, adjustNormal);
                    half4 color = texCUBE(cube, refDir);
                    return color.rgb;
                }

                half3 WaterDistortion(float2 offset) {

                    float4 distortionCol = tex2D(_CameraOpaqueTexture,  offset);

                    return distortionCol.rgb;
                }

                float Fresnel(float3 viewDir, float3 normal, float fresnelPower) {
                    float a = 1 - dot(viewDir, normal);
                    return pow(a, fresnelPower);
                }

                //环境光
                half3 GetAmbientLight() {
                    return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                }

                //焦散，获取底部的深度
                half3 GetCausticPosWS(float4 ScreenPos,float depth) {
                    float4 NDS = float4(ScreenPos.xy / ScreenPos.w, 0, 0);

                    float3 viewVector = mul(unity_CameraInvProjection, float4(NDS.xy * 2.0 - 1.0, 0, -1));
                    float3 viewDir = mul(unity_CameraToWorld, viewVector);

                    float SceneDepth = LinearEyeDepth(depth, _ZBufferParams);

                    half3 posWS = viewDir * SceneDepth.xxx + _WorldSpaceCameraPos;
                    return posWS;
                }

                inline float2 Unity_Voronoi_RandomVector_float(float2 UV, float offset)
                {
                    float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                    UV = frac(sin(mul(UV, m)));
                    return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
                }

                void Unity_Voronoi_Noise(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
                {
                    float2 g = floor(UV * CellDensity);
                    float2 f = frac(UV * CellDensity);
                    float t = 8.0;
                    float3 res = float3(8.0, 0.0, 0.0);

                    for (int y = -1; y <= 1; y++)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            float2 lattice = float2(x, y);
                            float2 offset = Unity_Voronoi_RandomVector_float(lattice + g, AngleOffset);
                            float d = distance(lattice + offset, f);

                            if (d < res.x)
                            {
                                res = float3(d, offset.x, offset.y);
                                Out = res.x;
                                Cells = res.y;
                            }
                        }
                    }
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = TransformObjectToHClip(v.vertex.xyz);
                    o.uv = v.uv;

                    o.normal = float4(TransformObjectToWorldNormal(v.normal), 0);
                    o.tangent = float4(TransformObjectToWorldDir(v.tangent.xyz), 0);
                    float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                    o.bitangent.xyz = cross(o.normal.xyz, o.tangent.xyz) * tangentSign;
                    float3 posWS = TransformObjectToWorld(v.vertex.xyz);
                    o.normal.w = posWS.x;
                    o.tangent.w = posWS.y;
                    o.bitangent.w = posWS.z;

                    return o;
                }



                float4 frag(v2f i) : SV_Target
                {
                    //光
                    Light mainLight = GetMainLight();
                //法线
                float3 normalTS = SampleWaterNormal(i.uv * _NormalTex_ST.xy + _NormalTex_ST.zw);
                float3x3 TBN = float3x3(i.tangent.xyz, i.bitangent.xyz, i.normal.xyz);
                float3 normalWS = normalize(mul(normalTS, TBN));
                //基本信息
                float3 posWS = float3(i.normal.w, i.tangent.w, i.bitangent.w);
                float3 viewWS = normalize(GetWorldSpaceViewDir(posWS));
                //屏幕坐标
                float4 clipPos = TransformWorldToHClip(posWS);
                float4 screenPos = ComputeScreenPos(clipPos);
                float2 screenUV = screenPos.xy / screenPos.w;

                //光照
                float3 specular = Specular(viewWS, normalWS, _gloss, _shininess, mainLight);
                float3 diffuse = Diffuse(normalWS, mainLight);
                float3 cubemap = SampleSkybox(_CubeMap, normalWS, viewWS, _CubeMapSmooth);
                float3 ambient = GetAmbientLight();

                //菲涅尔
                float fresnel = saturate(Fresnel(viewWS, normalWS, _FresnelPower));
                //反射
                specular += cubemap;
#ifdef _FX_LEVEL_HIGH
                //底部扭曲    
                float2 offset = screenUV + normalTS.xy * _Distortion;
                float3 distortionCol = WaterDistortion(offset);
                //基本色
                half3 color = lerp(distortionCol * diffuse, specular, fresnel);
#else
                half3 color = lerp(diffuse, specular, fresnel);
#endif

#ifdef USE_DEPTH_TEX

                //深度
                float depth0 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
                float depth = LinearEyeDepth(depth0, _ZBufferParams);
                //深度差异
                float depthDifference = depth - screenPos.w;

                //深浅
                float depthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                depthDifference01 = saturate(pow(depthDifference01, _DeepStength));
                float3 waterColor1 = lerp(_ShallowCol * color, _DeepCol * color, depthDifference01);


                //沫子
                float2 uv_WaterNoise = i.uv * _FoamNoiseTex_ST.xy + _FoamNoiseTex_ST.zw + _Time.x * _FoamSpeed;
                float form = tex2D(_FoamNoiseTex, uv_WaterNoise).r;
                float foamStep = saturate((depthDifference / _FoamWidth)) * _FoamStep;
                float surfaceNoise = 1 - step(form, foamStep);
                //焦散                
                float2 causticUV = GetCausticPosWS(screenPos, depth0).xz;
                float caustic;
                float cells;
                Unity_Voronoi_Noise(causticUV, _Time.w, _CausticScale, caustic, cells);
                return float4(waterColor1 + surfaceNoise * _FoamCol + pow(caustic * 1.2, _CausticPow) * (depthDifference01)*pow((1 - fresnel), 5), _Opacity);
#endif
                return float4(color, _Opacity);

            }
                ENDHLSL
        }
        }
}

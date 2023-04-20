Shader "Athena/G_Game/HLOD Unlit"
{
    Properties
    {
        [MainTexture]_BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5
        _Threhold("_Threhold", Range(-1.0, 1.0)) = 0
        _SelfNormalBias("_NormalBias", Range(-3, 3)) = 0
        _SelfDepthBias("_DepthBias", Range(-3, 3)) = 0
        [HideInInspector] _TextureAtlasing_LocalMetric("AtlasParam", Vector) = (0,0,0,0) 

        

        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        // Editmode props
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Blend [_SrcBlend][_DstBlend]
        ZWrite On
        Cull [_Cull]

        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            
            #pragma shader_feature _TONEMAP_CUSTOM
            //#pragma multi_compile _ _PostProcess
            //* vicci
            // #pragma multi_compile _ _NEED_GRAYTINT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            //* vicci
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma shader_feature _TEXTURE_ATLASING_ON
            #pragma shader_feature _TEXTURE_ATLAS_HIGH
            
            #pragma multi_compile _ HEIGHT_FOG
            //#define ATMOSPHERIC_FOG_DAY 1
            //#define ATMOSPHERIC 1
            
            #include "HLOD UnlitInput.hlsl"
            #include "Assets/Engine/Runtime/Shaders/Shading/NBR/Include/Fog.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Athena/Runtime/ShaderLibrary/Atlasing.hlsl"
            // #include "Packages/com.unity.render-pipelines.universal/Shaders/Postprocessing/Tonemapping.hlsl"
            // #include "Packages/com.pwrd.time-of-day/Resources/Shader/Include/FogCore.hlsl"

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
                float3 normalOS         : NORMAL;
#if _TEXTURE_ATLASING_ON             
                float4 color            : COLOR;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float fogCoord  : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float3 positionWS :TEXCOORD2;
                
#if _TEXTURE_ATLASING_ON
                // TEXTURE_ATLASING_COORDS(8)
                float4 vertexColor   : COLOR;
#endif
//* vicci : support height fog
#if HEIGHT_FOG
                float3 viewDirWS : TEXCOORD3;
#endif
//* vicci

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            float _Threhold;
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
//* vicci : support height fog
#if HEIGHT_FOG
                output.viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
#endif
//* vicci
                output.vertex = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                #if _TEXTURE_ATLASING_ON
                // TEXTURE_ATLASING_TRANSFER_EXTRA(output, input);
				output.vertexColor = input.color;
                #endif

                return output;
            }
            
            //* vicci
            void PostFrag(inout float3 color){
            
                //PostColor(color.xyz);
            }
            //* vicci
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half2 uv = input.uv;
            // #if _TEXTURE_ATLASING_ON
            //     half4 texColor = TEXTURE_ATLASING_TEX2D(input.vertexColor, _BaseMap, 0, input.uv,TEXTURE_ATLASING_PACKED(input));
            // #else
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
            // #endif

                //DLDR decode
                //texColor.rgb = texColor.rgb * 4.59f;

                half3 color = texColor.rgb *_BaseColor.rgb;
                half cloud = CloudShadowColor(input.positionWS);
                APPLY_FOG(color.rgb, input.positionWS.xyz);       

                half alpha = texColor.a *_BaseColor.a;
                AlphaDiscard(alpha, _Cutoff);

            #ifdef _ALPHAPREMULTIPLY_ON
                color *= alpha;
            #endif

                PostFrag(color);
                
            //* vicci
            #if HEIGHT_FOG
            //* pwrd zhanglingyun:大气散射雾效
                half4 tempColor = half4(color,alpha);
                color.rgb = ApplyHeightFog(GetHeightFogFactor(input.positionWS), tempColor.rgb, normalize(_MainLightPosition.xyz), normalize(input.viewDirWS));
            //* pwrd zhanglingyun
            #else
                color.rgb = MixFog(color.rgb, input.fogCoord);
            #endif
            //* vicci
                return half4(color, alpha);
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _TEXTURE_ATLAS_HIGH
            //#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #define _NeedVertexColor 1
            #pragma shader_feature _TEXTURE_ATLASING_ON
            
            #include "HLOD UnlitInput.hlsl"
            #include "HLOD ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            

            #include "HLOD UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass 
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _BaseMap;
                float4 _BaseMap_ST;

                v2f vert (appdata_t v)
                {
                    v2f o;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _BaseMap);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag (v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_BaseMap, i.texcoord);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    UNITY_OPAQUE_ALPHA(col.a);
                    return col;
                }
            ENDCG
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}

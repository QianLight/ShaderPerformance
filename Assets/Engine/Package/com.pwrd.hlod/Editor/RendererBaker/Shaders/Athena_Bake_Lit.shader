Shader "Athena/Bake/Lit"
{
    Properties
    {
        [MainTexture][SinglelineTexture(_BaseColor)] _BaseMap("Albedo", 2D) = "white" {}
        [HideInInspector][MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        [NoScaleOffset][SinglelineTexture] _NormalMap("Normal Map", 2D) = "bump" {}
        _BumpScale("BumpScale", Range(0.1, 10)) = 1.0
        
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0

        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _SpecColor("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap("Specular", 2D) = "white" {}
        
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [Gamma][HDR]_EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}
        
        [RenderingMode] _Mode("Blend Mode", Int) = 0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Int) = 0

        [Toggle(RECEIVE_NO_SHADOW)]_ReceiveNoShadow("不接收阴影", int) = 0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("_ZTest", int) = 4

        [HideInInspector] _Stencil ("Stencil Value", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        Pass
        {
        	Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull Off
        	
            HLSLPROGRAM
           // #include "Assets/Engine/Runtime/Shaders/Shading/Scene/UberForwardPass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            
            #pragma vertex vert
            #pragma fragment frag
            


            #pragma multi_compile _ NO_MSA
            #pragma multi_compile _ _ISTERRAINLAYER _VERTEXCOLORMODE
            #pragma shader_feature _ _ENABLE_HLOD_BAKE_ALBEDO _ENABLE_HLOD_BAKE_NORMAL _ENABLE_HLOD_BAKE_METALLIC _ENABLE_HLOD_BAKE_OCCLUSION _ENABLE_HLOD_BAKE_LIGHTMAP _TESTKEYWORD_
            #define SHADOWS_SHADOWMASK
            #define LIGHTMAP_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
    
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 lightmapUV : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 vertexColor : TEXCOORD3;

            };

            
            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _BaseColor;
            sampler2D _NormalMap;
            float _BumpScale;
            float _Smoothness;
            float _GlossMapScale;
            float _Metallic;
            sampler2D _MetallicGlossMap;
            float4 _SpecColor;
            sampler2D _SpecGlossMap;
            float _OcclusionStrength;
            sampler2D _OcclusionMap;
            float4 _EmissionColor;
            sampler2D _EmissionMap;

            #ifdef _ISTERRAINLAYER
            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            sampler2D _BlendTex;
            float4 _BlendTex_ST;
            sampler2D _Layer0Tex;
            float4 _Layer0UVST;
            sampler2D _Layer1Tex;
            float4 _Layer1UVST;
            sampler2D _Layer2Tex;
            float4 _Layer2UVST;
            sampler2D _Layer3Tex;
            float4 _Layer3UVST;
            #endif
            
            //SAMPLER(samplerunity_ShadowMask);
    
            #define Athena_kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04)
            
            v2f vert (appdata v)
            {
                v2f o;
            	o.vertex = float4((v.lightmapUV.x - 0.5) * 2, v.lightmapUV.y * -2 + 1, 0.1, 1);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.lightmapUV = v.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
            	o.vertexColor = v.color;
                return o;
            }
			
            
            half4 frag (v2f i) : SV_Target
            {
                
                
                
                //albedo
                half4 albedo = tex2D(_BaseMap, i.uv) * _BaseColor;
                half3 color = pow(albedo.rgb ,0.45) ;

                
                //normal
                half4 normalMap = tex2D(_NormalMap, i.uv);
                half3 normalCol = UnpackNormal(normalMap);
                normalCol.rgb *= _BumpScale;
    
                //metallic
                half4 metallicMap = tex2D(_MetallicGlossMap, i.uv);
                half metallic = metallicMap.r * _Metallic;
                half oneMinusDielectricSpec = Athena_kDielectricSpec.a;
                half oneMinusReflectivity = oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
                
                //occlusion
                half4 occlusionMap = tex2D(_OcclusionMap, i.uv) * _OcclusionStrength;
                half occlusion = occlusionMap.g;
    
                //smoothness
                half smoothness = metallicMap.a * _Smoothness;
    
                //lightmap
            	half4 lightMap = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, i.lightmapUV);
            	//lightMap = pow(lightMap,0.45);
				//half3 indirectDiffuse = lightMap.rgb * 4.594;

                //shadowmask
                half4 shadowMask = SAMPLE_TEXTURE2D(unity_ShadowMask, samplerunity_ShadowMask, i.lightmapUV);
                
                

                #if _ENABLE_HLOD_BAKE_NORMAL
                    return half4(normalMap.ag, 1, 1);
                #elif _ENABLE_HLOD_BAKE_METALLIC
                    return half4(metallicMap.rgb, 1);
                #elif _ENABLE_HLOD_BAKE_OCCLUSION
                    return half4(occlusionMap.rgb, 1);
                #elif _ENABLE_HLOD_BAKE_LIGHTMAP
                    return half4(lightMap.rgb, 1);
                #endif
                
                //specular
                half3 specular = half3(0.0h, 0.0h, 0.0h);
    
                half3 brdfDiffuse = albedo.rgb  * oneMinusReflectivity * occlusion ;
                half3 brdfSpecular = lerp(Athena_kDielectricSpec.rgb, albedo.rgb, metallic) * occlusion;

            	
                
               // color += lerp(lightMap,0.0,shadowMask.r);
				//color *= lerp(0.2, 1, shadowMask.r);
                
     #ifdef _ISTERRAINLAYER
                float4 blendTex = 0;
                UNITY_BRANCH
                if((i.vertexColor.r + i.vertexColor.g + i.vertexColor.b) > 2.99 )
                {
                    blendTex = tex2D(_BlendTex, i.uv);
                }
                else
                {
                    blendTex = i.vertexColor;
                }
                
                float3 layer0Tex = tex2D(_Layer0Tex, i.uv).rgb; 
                float3 layer1Tex = tex2D(_Layer1Tex, i.uv).rgb; 
                float3 layer2Tex = tex2D(_Layer2Tex, i.uv).rgb;
                
                albedo.rgb = lerp(layer0Tex * _Color0,layer0Tex, _Color0.a) * blendTex.r;
                albedo.rgb += lerp(layer1Tex * _Color1, layer1Tex, _Color1.a) * blendTex.g;
                albedo.rgb += lerp(layer2Tex * _Color2, layer2Tex,  _Color2.a) * blendTex.b;

                float3 directDiffuse = albedo.rgb * saturate(dot(i.worldNormal,_MainLightDir0)) * _MainLightColor0.rgb;
                directDiffuse *= float3(1,0.8,0.6) * 1.2;
                float3 indirectDiffuse = albedo.rgb * lightMap ;

                color = directDiffuse + indirectDiffuse;
                
                color = pow(color,0.45);
                color *= lerp(0.8, 1, shadowMask.r);
                return half4(color,1);
    #endif

                if(shadowMask.g > 0.5) return half4(color,1);

                color += lerp(0, 0.6, brdfDiffuse);
                color *= lerp(0.6, 1, shadowMask.r);
                
                return half4(color,1);

            }
            
            ENDHLSL
        	
	
        	
        }
    }
}

Shader "Athena/Bake/Decal"
{
    Properties
    {
	    [MainTexture][SinglelineTexture(_BaseColor)] _BaseMap("Albedo", 2D) = "white" {}
        [HideInInspector][MainColor] _BaseColor("Color", Color) = (1,1,1,1)
    	
    	_BakedTexture("Baked", 2D) = "white" {}

        [NoScaleOffset][SinglelineTexture] _NormalMap("Normal Map", 2D) = "bump" {}
        _BumpScale("BumpScale", Range(0.1, 10)) = 1.0

        _MetallicScale("Metallic Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _OcclusionScale("Occlusion Scale", Range(0.0, 1.0)) = 1.0

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
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #define SHADOWS_SHADOWMASK
            #define LIGHTMAP_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 lightmapUV : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _BaseColor;
            sampler2D _NormalMap;
            float _BumpScale;
            sampler2D _MSOMap;
            float _MetallicScale;
            float _SmoothnessScale;
            float _OcclusionScale;
            half4 _EmissionColor;
            float _Cutoff;

            float4x4 _DecalTrans;
            float4 _DecalNormal;
            float4 _UVArea;
            TEXTURE2D(_BakedTexture); SAMPLER(sampler_BakedTexture);
            // SAMPLER(samplerunity_ShadowMask);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4((v.lightmapUV.x - 0.5) * 2, v.lightmapUV.y * -2 + 1, 0.1, 1);
            	o.worldPos = TransformObjectToWorld(v.vertex).xyz;
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.lightmapUV = v.lightmapUV * unity_LightmapST.xy + unity_LightmapST.zw;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
				float4 localePos = mul(_DecalTrans, float4(i.worldPos, 1.0));
                localePos = localePos + 0.5;
                localePos.w = 1.0;

            	//albedo
            	half4 albedo;
            	if(localePos.x < 0 || localePos.x > 1 || localePos.y < 0 || localePos.y > 1 || localePos.z < 0 || localePos.z > 1 || dot(i.worldNormal, _DecalNormal.xyz) < 0)
                //if(localePos.x < 0 || localePos.x > 1 || localePos.y < 0 || localePos.y > 1 || localePos.z < 0 || localePos.z > 1)
                {
                    //不显示贴花
                    albedo  = SAMPLE_TEXTURE2D(_BakedTexture, sampler_BakedTexture, i.uv) * _BaseColor;
                }
            	else
            	{
            		half2 uv = 1 - localePos.xz;
            		uv = uv *_BaseMap_ST.xy + _BaseMap_ST.zw;
            		albedo = tex2D(_BaseMap, uv) * _BaseColor;
            		if(albedo.a < _Cutoff)
	                {
	                    albedo = SAMPLE_TEXTURE2D(_BakedTexture, sampler_BakedTexture, i.uv);
	                }
            	}
            	
            	half3 color = albedo.rgb;

                //normal
                half4 normalMap = tex2D(_NormalMap, i.uv);
                half3 normalCol = UnpackNormal(normalMap);
                normalCol.xy *= _BumpScale;
                //mso
                half4 msoMap = tex2D(_MSOMap, i.uv);
                #ifdef NO_MSA
                    msoMap = half4(1.0, albedo.a, normalMap.a, 0.0);
                    _SmoothnessScale = 1;
                    _OcclusionScale = 1;
                #else
                    _MetallicScale = 1;
                    _SmoothnessScale = 1;
                    _OcclusionScale = 1;
                #endif
                
                //specular
                half3 specular = half3(0.0h, 0.0h, 0.0h);
                
                //metalic
                half metallic = msoMap.r * _MetallicScale;
                
                //smoothness
                half smoothness = msoMap.g * _SmoothnessScale;
                
                //occlusion
                half occlusion = msoMap.b * _OcclusionScale;
                
                //emission
                half3 emission = msoMap.a * _EmissionColor.rgb;

				//shadowmask
                half4 shadowMask = SAMPLE_TEXTURE2D(unity_ShadowMask, samplerunity_ShadowMask, i.lightmapUV);

            	//lightmap
            	half4 lightMap = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, i.lightmapUV);
				half3 indirectDiffuse = lightMap.rgb * 4.594;

            	color *= lerp(0.6, 1, shadowMask.b);	//b bakeAO
            	
                return half4(color, albedo.a);
            }
            ENDHLSL
        	
        	
        	
        	
        }
    }
}

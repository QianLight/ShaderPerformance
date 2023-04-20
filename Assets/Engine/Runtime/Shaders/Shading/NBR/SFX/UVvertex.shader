Shader"URP/SFX/UVEffect_Vertex2"
{
    Properties
    {
        [Header(Mode)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
        [Enum(On,1,Off,0)]_ZWrite("Depth Mode", Float) = 0
        [Toggle(_CLIPPING_ON)]_Clipping("Clipping" , int) = 0
        _ClippingInt("ClippingInt" , int) = 0.5
        [Toggle(_SOFTDISSOLVESWITCH_ON)]_SoftDissolveSwitch("软硬溶解" , int) = 0
        [Header(Main)]
        [HDR]_MainColor("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
        [Header(Vertex Offset)]
        _VertexOffsetTex("VertexOffset Tex", 2D) = "black"{}
        _VertexPannerX("VertexOffsetX", Range(-1,1)) = 0.5
        _VertexPannerY("VertexOffsetY", Range(-1,1)) = 0.5
        _VertexOffsetStr("VertexOffsetStr", Range(0,10)) = 1
        [Header(Dissolve)]
        _DissolveTex("Dissolve Tex", 2D) = "white"{}
        _DissolvePannerX("MainPannerX", Float) = 0
        _DissolvePannerY("MainPannerY", Float) = 0
        _SoftDissolveIndensity ("ClipInt" , Range(0,1.05)) = 0
        _SmoothClipInt ("SmoothClipInt" ,float) = 0
        [Header(Warp)]
        _WarpTex("Warp Tex", 2D) = "white"{}
        _WarpPannerX("WarpPannerX", Float) = 0
        _WarpPannerY("WarpPannerY", Float) = 0
		_WarpInt ("WarpInt" ,float) = 0
        [Header(Fade)]
        [Toggle]_UseDepthFade("Use Depth Fade", Float) = 0
        _FadeLength("Fade Length", Range(0, 10)) = 0.5
        [Toggle]_CameraFade("Camera Fade", Float) = 0
        _CameraFadeOffset("Camera Fade Offset", Float) = 0
        _CameraFadeLength("Camera Fade Length", Float) = 5
    }
    SubShader
    {
        Tags 
        { 
            
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "LightMode"="UniversalForward"
        }
        LOD 100
        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull [_CullMode]
        ZTest [_ZTest]
        Offset 0 , 0
		ColorMask RGBA
        AlphaToMask Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma shader_feature_local _ _DEPTHMODE_ON
            #pragma shader_feature_local _ _USEDEPTHFADE_ON
            #pragma shader_feature_local _ _CAMERAFADE_ON
            #pragma shader_feature _SOFTDISSOLVESWITCH_ON
            #pragma shader_feature _ _CLIPPING_ON

			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            //#include "SFXCore.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                float4 screenPos : TEXCOORD4;
            #endif
				UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            Texture2D _VertexOffsetTex;
            SamplerState sampler_VertexOffsetTex;
            Texture2D _DissolveTex;
            SamplerState sampler_DissolveTex;
			Texture2D _WarpTex;
			SamplerState sampler_WarpTex;

    #ifdef _FX_LEVEL_HIGH
        #ifdef _USEDEPTHFADE_ON
            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;
        #endif
    #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST,_DissolveTex_ST;
            float4 _VertexOffsetTex_ST,_WarpTex_ST;
            half _DissolveStr;
            half _VertexPannerX,_VertexPannerY;
            half4 _MainColor;
            float _MainPannerX;
            float _MainPannerY;
            float _DissolvePannerX;
            float _DissolvePannerY,_VertexOffsetStr,_SoftDissolveIndensity,_SmoothClipInt,_ClippingInt;
			float _WarpPannerX,_WarpPannerY,_WarpInt;
        // #ifdef _DEPTHMODE_ON
            half _CUTOUT;
        // #endif
        // #ifdef _USEDEPTHFADE_ON
            half _FadeLength;
        // #endif
        // #ifdef _CAMERAFADE_ON
            half _CameraFadeLength;
            half _CameraFadeOffset;
        // #endif
            CBUFFER_END
            half _IsInUICamera;//Add By: Takeshi

            v2f vert (appdata v)
            {
                v2f o;
                half2 vertexUV = v.uv * _VertexOffsetTex_ST.xy + _VertexOffsetTex_ST.zw + (half2(_VertexPannerX,_VertexPannerY)*_Time.y);

				half2 warpUV =  v.uv * _WarpTex_ST.xy + _WarpTex_ST.zw + (half2(_WarpPannerX,_WarpPannerX)*_Time.y);

				half2 warpVertexUV = SAMPLE_TEXTURE2D_LOD(_WarpTex, sampler_WarpTex,warpUV,0).xy*_WarpInt;

				half vertexTex = SAMPLE_TEXTURE2D_LOD(_VertexOffsetTex, sampler_VertexOffsetTex,vertexUV,0).r;

				vertexTex += warpVertexUV;

                v.vertex += vertexTex * _VertexOffsetStr * normalize(v.normal);//* normalize(v.normal))

                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
            #if defined _USEDEPTHFADE_ON
                float4 clipPos = TransformObjectToHClip(v.vertex);
                o.screenPos = ComputeScreenPos(clipPos);
                o.screenPos = o.screenPos / o.screenPos.w;
                o.screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? o.screenPos.z : o.screenPos.z * 0.5 + 0.5;
            #endif
                half cameraDepthFade = 1;
            #ifdef _CAMERAFADE_ON
                half z = -TransformWorldToView(TransformObjectToWorld(v.vertex.xyz)).z;
                cameraDepthFade = (( z -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
                cameraDepthFade = saturate(cameraDepthFade);
            #endif
                return o;
            }

            half4 frag (v2f i, half vface : VFACE) : SV_Target
            {
                //half2 mainUV = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 panner = float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x);
                half2 mainUV = i.uv * _MainTex_ST.xy  +_MainTex_ST.zw + frac(panner);
                half2 dissUV = i.uv * _DissolveTex_ST.xy + _DissolveTex_ST.zw + frac(float2(_DissolvePannerX * _TimeParameters.x, _DissolvePannerY * _TimeParameters.x));
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV) * _MainColor * i.color;
                #ifdef _SOFTDISSOLVESWITCH_ON
                half dissolveTex =  Luminance(SAMPLE_TEXTURE2D(_DissolveTex,  sampler_DissolveTex, dissUV).rgb);
                dissolveTex += 1;
                dissolveTex += _SoftDissolveIndensity*-2;
                dissolveTex = clamp(dissolveTex,0,1);
                dissolveTex = smoothstep(_SmoothClipInt,1-_SmoothClipInt,dissolveTex);//hard Clip
                color.a *= dissolveTex;
                
                #endif
             #ifdef _CLIPPING_ON
                half dissolveTex =  Luminance(SAMPLE_TEXTURE2D(_DissolveTex,  sampler_DissolveTex, dissUV).rgb);
                color.a *= dissolveTex;
                 clip(color.a - _ClippingInt);
            #endif

        #ifdef _FX_LEVEL_HIGH
            #ifdef _USEDEPTHFADE_ON
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos.xy), _ZBufferParams);
                float objDepth = LinearEyeDepth(i.screenPos.z, _ZBufferParams);
                float delta = smoothstep( 0, _FadeLength,abs(sceneDepth - objDepth));
                color.a *= delta;
            #endif
        #endif
                half luminance = Luminance(color) + 1E-07;
                color = max(color / luminance * min(luminance, 4), 0);
                color = lerp(color,LinearToSRGB(color),_IsInUICamera);//Add by: Takeshi; Fix UI Gamma
                return color;
            }
            ENDHLSL
        }

    }
    CustomEditor "SFXTemplateShaderGUI"
}
Shader"URP/SFX/UVEffect_DoubleFresnel"
{
    Properties
    {
        [Toggle]_ShaderMode("Shader Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
        _CUTOUT("Cutout", Range(0,1)) = 0.5
        _Brightness("Brightness", Float) = 1
        _Contrast("Contrast", Float) = 1
        [HDR]_MainColor("Main Color", Color) = (1, 1, 1, 1)
        [HDR]_BackColor("Back Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
		_MainRamp("Main Ramp", 2D) = "white" {}
        _MaskTex("Mask Tex", 2D) = "white" {}
        
        [HDR]_WidthColor("Width Color", Color) = (1, 1, 1, 1)
        [Toggle]_Usefresnel("Use Fresnel", Float) = 0
        _OutsideFresnelPower("Outside fresnel Power", Range(0,20)) = 1
        _OutsideFresnelMultiply("Outside fresnel Multiply", Range(0,30))= 1
        [HDR]_OutsideFresnelColor("Outside fresnel Color", Color) = (1,1,1,1)
        _InsideFresnelPower("Inside fresnel Power", Range(0,20)) = 1
        _InsideFresnelMultiply("Inside fresnel Multiply", Range(0,30))= 1
        [HDR]_InsideFresnelColor("Inside fresnel Color", Color) = (1,1,1,1)
        [Toggle]_UseRamp("Use Ramp", Float) = 0
        _Color0("Color 0", Color) = (1, 0, 0, 0)
        _Color1("Color 1", Color) = (0, 1, 0, 0)
        _Color2("Color 2", Color) = (0, 0, 1, 0)
        _RampParam("Ramp Param", Vector) = (0.3, 0.33, 0.6, 0.66)
        _DistinguishAlpha("GlobalAlpha", Range(0,1)) = 1
        [Toggle]_UseVertexColor("Use VertexColor", Float) = 1
        [Toggle]_UseFog("Use Fog", Float) = 0
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "LightMode"="UniversalForward"
        }
        LOD 100
        Blend SrcAlpha [_BlendMode]
        ZWrite On
        Cull [_CullMode]
        ZTest [_ZTest]
        Offset 0 , 0
		ColorMask RGBA
        AlphaToMask Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma shader_feature_local _ _SHADERMODE_ON
            #pragma shader_feature_local _ _USEFRESNEL_ON
            #pragma shader_feature_local _ _DEPTHMODE_ON
            #pragma shader_feature_local _ _USERAMP_ON
            #pragma shader_feature_local _ _CAMERAFADE_ON
            #pragma shader_feature_local _ _USEFOG_ON
			#pragma multi_compile _FX_LEVEL_HIGH _FX_LEVEL_MEDIUM _FX_LEVEL_LOW
            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "../Include/Fog.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                half4 color : COLOR;
            // #ifndef _SHADERMODE_ON
                half4 custom1 : TEXCOORD1;
                half4 custom2 : TEXCOORD2;
            // #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                //x:? y:? z:dissolve w:distort
                half4 custom1 : TEXCOORD1;
                //x:? y:? z:edgeWidth w:edgeBrightness
                half4 custom2 : TEXCOORD2;
                //x: clipX y:clipY z:fresnel w:camerafade
                half4 deliver : TEXCOORD3;
            // #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
            //     float4 screenPos : TEXCOORD4;
            // #endif
                float3 worldNormal : TEXCOORD7;
                float3 worldPos : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            Texture2D _MainRamp;
            SamplerState sampler_MainRamp;
            
    #ifdef _FX_LEVEL_HIGH
        #ifdef _USEDEPTHFADE_ON
            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;
        #endif
    #endif

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainRamp_ST;
            half4 _MainColor;
            half _Brightness;
            half4 _WidthColor;
            half _Contrast,_DistinguishAlpha;
            half _OutsideFresnelPower;
            half _OutsideFresnelMultiply;
            half4 _OutsideFresnelColor;
            half _InsideFresnelPower;
            half _InsideFresnelMultiply;
            half4 _InsideFresnelColor;
            float _MainPannerX;
            float _MainPannerY;
            half4 _Color0;
            half4 _Color1;
            half4 _Color2;
            half4 _RampParam;
            // half _CameraFadeLength;
            // half _CameraFadeOffset;
            float4 _MaskTex_ST;
            half _UseVertexColor;
            CBUFFER_END
            half _IsInUICamera;//Add By: Takeshi

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.color = v.color;
                o.uv = v.uv;
                half fresnel = 1;
                o.worldNormal = TransformObjectToWorldNormal(v.normal.xyz, true).xyz;

                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                
            // #ifdef _SHADERMODE_ON
                half2 panner = float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x);
                panner = frac(panner);
                o.custom1 = half4(panner.x, panner.y, max(1 - fresnel, 1) * 0/*_Dissolve*/, 0/*_DistortPower*/);
                o.custom2 = half4(1, 1, 0/*_EdgeWidth*/, 1);
            // #else
            //     o.custom1 = v.custom1;
            //     o.custom2 = half4(v.custom2.x, v.custom2.y, v.custom2.z, v.custom2.w + 1);
            // #endif
                
                half2 xy = 1;

                half cameraDepthFade = 1;
            // #ifdef _CAMERAFADE_ON
            //     half z = -TransformWorldToView(TransformObjectToWorld(v.vertex.xyz)).z;
            //     cameraDepthFade = (( z -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
            //     cameraDepthFade = saturate(cameraDepthFade);
            // #endif
                
                o.deliver = half4(xy.x,xy.y,1,cameraDepthFade);
                return o;
            }

            half4 frag (v2f i, half vface : VFACE) : SV_Target
            {
                half2 clipXY = i.deliver.xy;
                
                float2 customUV = i.custom2.xy - 1;
                float2 mainUV = (i.uv - 0.5) * customUV;/*i.uv * customUV - (customUV * 0.5);*/
                float2 offset = _MainTex_ST.zw + i.custom1.xy;
                float2 uvOffset = i.uv * _MainTex_ST.xy + offset;
                mainUV += uvOffset;
                
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half3 color = pow(main.xyz, _Contrast) * _Brightness * _MainColor.xyz/*lerp(_MainColor.xyz, _MainColor, max(vface, 0))*/;

                UNITY_BRANCH
                if (_UseVertexColor)
                {
                    color *= i.color.rgb;
                }
                color = lerp(color * _WidthColor * i.custom2.w, color, clipXY.x);
            #ifdef _USEMAINRAMP_ON
                float2 uv_MainRamp = i.uv * _MainRamp_ST.xy + _MainRamp_ST.zw;
                half4 mainRamp = SAMPLE_TEXTURE2D(_MainRamp, sampler_MainRamp, uv_MainRamp);//zh
                color.rgb *= mainRamp.rgb;
            #endif
                
                half2 OIFresnel = 1;
                
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                OIFresnel.x = abs(dot(i.worldNormal, viewDir));
                OIFresnel.y = 1 - OIFresnel.x;
                
                OIFresnel.x = pow(abs(1 - OIFresnel.x), _OutsideFresnelPower);
                OIFresnel.y = pow(abs(1 - OIFresnel.y), _InsideFresnelPower);
                
                OIFresnel = min(OIFresnel, 1) * half2(_OutsideFresnelMultiply, _InsideFresnelMultiply);
                
                color.rgb = lerp(color, _OutsideFresnelColor.rgb, saturate(OIFresnel.x * _OutsideFresnelColor.a));

                main.a = _MainColor.a * main.a/*lerp(main.a, main.r, _AlphaR)*/ * clipXY.y /** _Alpha*/ /** fresnel * mask*/;
                
                UNITY_BRANCH
                if(_UseVertexColor)
                {
                    main.a *= i.color.a;
                }
                main.a = min(main.a, 1);
                // main.a = min((_UseVertexColor ? i.color.a : 1) * _MainColor.a * lerp(main.a, main.r, _AlphaR) * clipXY.y * _Alpha * fresnel * mask,1);
                
                clip(main.a - .5/*_CUTOUT*/);
                main.a = 1;

        #ifdef _FX_LEVEL_HIGH
            #if defined _USEDEPTHFADE_ON
                float4 clip = TransformWorldToHClip(i.worldPos);
                float4 screenPos = ComputeScreenPos(clip);
                screenPos.xy = screenPos.xy / screenPos.w;
                screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? screenPos.z : screenPos.z * 0.5 + 0.5;
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos.xy), _ZBufferParams);
                float objDepth = screenPos.w/*LinearEyeDepth(screenPos.z, _ZBufferParams)*/;
                float delta = saturate(smoothstep( 0, _FadeLength, abs(sceneDepth - objDepth)));
                main.a *= delta;
            #endif
        #endif
            #ifdef _USERAMP_ON
                half rate = 1 - saturate(dot(_MainLightPosition.xyz, i.worldNormal) * 0.5 + 0.5);
                half rate1 = smoothstep(_RampParam.x, _RampParam.y, rate);
                half rate2 = smoothstep(_RampParam.z, _RampParam.w, rate);
                half3 mixer = lerp(_Color0.rgb, _Color1.rgb, rate1);
                mixer = lerp(mixer, _Color2.rgb, rate2);
                color *= mixer.xyz;
            #endif
                color = lerp(color, _InsideFresnelColor.rgb, saturate(OIFresnel.y * _InsideFresnelColor.a));


                half luminance = Luminance(color) + 0.001;
                color = max(color / luminance * min(luminance, 4), 0);

            #ifdef _USEFOG_ON
                UNITY_BRANCH
                if (_ScatterEnable)
                {
                    float depth01;
                    float3 scatter = GetScatterRGB(viewDir, i.worldPos, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
                    // APPLY_FOG(scatter, inputData.positionWS);
                    color.rgb = lerp(color.rgb, scatter, saturate(depth01 * 2));
                }     
                APPLY_FOG(color.rgb, i.worldPos)
            #endif
                
                main.a *= i.deliver.w;
                main.a = max(main.a, 0);
                color = lerp(color,LinearToSRGB(color),_IsInUICamera);//Add by: Takeshi; Fix UI Gamma
                main.a *= _DistinguishAlpha;
                return half4(color,main.a);
            }
            ENDHLSL
        }
        Pass
        {
        	Name "OverdrawF"
        	Tags{"LightMode" = "OverdrawForwardBaseT"}

        	Blend One One
        	ZWrite [_DepthMode]
        	ZTest [_ZTest]
        	Offset 0 , 0
        	ColorMask RGBA
        	HLSLPROGRAM

        	#pragma vertex Vert
        	#pragma fragment Frag

        	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
}

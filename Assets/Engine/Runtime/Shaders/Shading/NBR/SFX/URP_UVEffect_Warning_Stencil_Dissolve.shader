Shader"URP/SFX/Warning_Stencil_New"
{
    Properties
    {
        [Header(builtin)]
        [Enum(UnityEngine.Rendering.BlendMode)]_BlendMode("Blend Mode", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4
        //[Toggle]_DepthMode("ZWrite", Float) = 0
        
        [Unity]
        
        [Header(MainControl)]
        //_CUTOUT("裁剪", Range(0,1)) = 0.5
        _ColorInt("ColorInt", Float) = 1
        _AlphaInt("AlphaInt", Float) = 1
        _Contrast("Contrast", Float) = 1
        [HDR]_MainColor("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
        [Toggle]_AlphaR("Alpha / R", Float) = 0
        _MainPannerX("MainPannerX", Float) = 0
        _MainPannerY("MainPannerY", Float) = 0
        
        [Header(Turbulence)]//溶解
        _TurbulenceTex("TurbulenceTex", 2D) = "white" {}
        _DistortPower("DistortPower", Float) = 0
        _PowerU("PowerU", Float) = 0
        _PowerV("PowerV", Float) = 0
        
        [Header(Mask)]
        _MaskTex("Mask Tex", 2D) = "white" {}
        
        [Header(CustomData)]
        //-硬度-溶解-宽度颜色-边宽度
        _Hardness("Hardness", Range(0, 0.99)) = 0
        _Dissolve("Dissolve", Float) = 0    
        [HDR]_WidthColor("Width Color", Color) = (1, 1, 1, 1)
        _EdgeWidth("EdgeWidth", Range(0,1)) = 0
        
        
        /*
        //[Header(DepthFade)]
        //_FadeLength("Fade Length", Range(0, 10)) = 0.5
        */
        /*
        //[Header(Fresnel)]
        //_fresnelpower("fresnel Power", Range(0,20)) = 1
        //_fresnelmultiply("fresnel Multiply", Range(0,30))= 1
        //_Flip("Flip", Range(0,1)) = 0
        */
        /*
        //[Header(Ramp)]
        //_Color0("Color 0", Color) = (1, 0, 0, 0)
        //_Color1("Color 1", Color) = (0, 1, 0, 0)
        //_Color2("Color 2", Color) = (0, 0, 1, 0)
        //_RampParam("Ramp Param", Vector) = (0.3, 0.33, 0.6, 0.66)
        */
        
        [Header(variant)]
        //_CameraFadeOffset("Camera Fade Offset", Float) = 0
        //[Toggle]_CameraFade("Camera Fade", Float) = 0
        //[Toggle]_UseRamp("Use Ramp", Float) = 0
        //[Toggle]_Usefresnel("Use Fresnel", Float) = 0
        //[Toggle]_UseDepthFade("Use Depth Fade", Float) = 0
        //_CameraFadeLength("Camera Fade Length", Float) = 5
        [Toggle]_UseClip("Use Clip", Float) = 0
        [Toggle]_UseTurbulence("Use Turbulence", Float) = 0
        [Toggle]_UseMask("Use Mask", Float) = 0
        [Toggle]_ShaderMode("ShaderMode", float) = 1
        //[Toggle]_AngleTog("AngleTog", Float) = 0
        //[Toggle]_UseBackColor("Use BackColor", Float) = 0
        //[HDR]_BackColor("Back Color", Color) = (1, 1, 1, 1)
        _AlphaPass("AlphaPass", Range(0, 1)) = 1
        [Header(Angle)]
        _Angle("Angle", Range(0, 360)) = 360
        //_Distance("distance",Float)=10
        [Toggle]_UseShadowMask("Use Shadowmask", Float) = 0
        _ShadowStr("Shadow Str", Range(0,1)) = 0.25
        _VanishDistanceV("VanishDistance", Range(5,100)) = 50
    }
    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline" 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
        }
        
        LOD 100
        Blend SrcAlpha [_BlendMode]
        Cull [_CullMode]
        Offset 0 , 0
		ColorMask RGBA
        AlphaToMask Off
        
        Pass
        {
            Tags {"LightMode"="SRPDefaultUnlit"}
            Stencil
            {
                Ref [_StencilRef_Warning]
                //Ref 1
                Comp Greater
            }
            ZTest Greater
            ZWrite Off

            HLSLPROGRAM
            #pragma multi_compile _ _SHADERMODE_ON
            //#pragma multi_compile _ _USEFRESNEL_ON
            #pragma multi_compile _ _USETURBULENCE_ON
            #pragma multi_compile _ _USECLIP_ON
            #pragma multi_compile _ _USEMASK_ON
            #pragma multi_compile _ _DEPTHMODE_ON
            //#pragma multi_compile _ _ANGLETOG_ON
            //#pragma multi_compile _ _USEDEPTHFADE_ON
            //#pragma multi_compile _ _USERAMP_ON
            //#pragma multi_compile _ _CAMERAFADE_ON
			#pragma multi_compile _SHADER_LEVEL_HIGH/* _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW*/
            #pragma shader_feature_local _ _USESHADOWMASK_ON
            #pragma multi_compile MAIN_LIGHT_CALCULATE_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                half4 color : COLOR;
            #ifndef _SHADERMODE_ON
                float4 custom1 : TEXCOORD1;
                float4 custom2 : TEXCOORD2;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                //x:? y:? z:dissolve w:distort
                float4 custom1 : TEXCOORD1;
                //x:? y:? z:edgeWidth w:edgeBrightness
                float4 custom2 : TEXCOORD2;
                //x: clipX y:clipY z:fresnel w:camerafade
                float4 deliver : TEXCOORD3;
/*
            #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                float4 screenPos : TEXCOORD4;
            #endif
*/
/*
            #if defined _USEFRESNEL_ON || defined _USERAMP_ON
                float3 worldNormal : TEXCOORD7;
            #endif
            #ifdef _USERAMP_ON
                half4 mixer : TEXCOORD5;
            #endif
*/
            //#if defined _USEFRESNEL_ON || defined _USESHADOWMASK_ON
                float3 worldPos : TEXCOORD6;
            //#endif
            #ifdef _USESHADOWMASK_ON 
                float4 shadowCoord : TEXCOORD7;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

        #if defined _USETURBULENCE_ON || defined _USECLIP_ON
            Texture2D _TurbulenceTex;
            SamplerState sampler_TurbulenceTex;
        #endif

        #ifdef _USEMASK_ON
            Texture2D _MaskTex;
            SamplerState sampler_MaskTex;
        #endif
            
/*
    #ifdef _SHADER_LEVEL_HIGH
        #ifdef _USEDEPTHFADE_ON
            Texture2D _CameraDepthTexture;
            SamplerState sampler_CameraDepthTexture;
        #endif
    #endif
*/
            
            CBUFFER_START(UnityPerMaterial)
            half _AlphaPass;
            half _Angle;
            float4 _MainTex_ST;
            half _AlphaR;
            //half _UseBackColor;
            //half4 _BackColor;
            half4 _MainColor;
            half _ColorInt;
            half _AlphaInt;
            half4 _WidthColor;
            half _Contrast;
/*内置变体
        #ifdef _DEPTHMODE_ON
            half _CUTOUT;
        #endif
*/

        #if defined _USETURBULENCE_ON || defined _USECLIP_ON
            half _PowerU;
            half _PowerV;
            float4 _TurbulenceTex_ST;
        #endif

        #ifdef _USEFRESNEL_ON
            //float _Distance;
            half _fresnelpower;
            half _fresnelmultiply;
            half _Flip;
        #endif
        #ifdef _SHADERMODE_ON
            float _MainPannerX;
            float _MainPannerY;
            half _DistortPower;
            half _Dissolve;
            half _EdgeWidth;
        #endif

        #ifdef _USECLIP_ON
            half _Hardness;
        #endif

/*
        #ifdef _USEDEPTHFADE_ON
            half _FadeLength;
        #endif
*/
/*
        #ifdef _USERAMP_ON
            half4 _Color0;
            half4 _Color1;
            half4 _Color2;
            half4 _RampParam;
        #endif
*/
/*
        #ifdef _CAMERAFADE_ON
            half _CameraFadeLength;
            half _CameraFadeOffset;
        #endif
*/
        
        #ifdef _USEMASK_ON
            float4 _MaskTex_ST;
        #endif
            float _ShadowStr;
            float _VanishDistanceV;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                half fresnel = 1;
/*
            #if defined _USEFRESNEL_ON || defined _USERAMP_ON
                o.worldNormal = TransformObjectToWorldNormal(v.normal, true);
            #endif
*/
            //#if defined _USEFRESNEL_ON || defined _USESHADOWMASK_ON
                o.worldPos = TransformObjectToWorld(v.vertex);
            //#endif
                

            #ifdef _USESHADOWMASK_ON
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex);
				o.shadowCoord = GetShadowCoord( vertexInput );
            #endif
            #ifdef _SHADERMODE_ON
                float2 panner = float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x);
                panner = frac(panner);
                o.custom1 = float4(panner.x, panner.y, max(1 - fresnel, 1) * _Dissolve, _DistortPower);
                o.custom2 = float4(1, 1, _EdgeWidth, 1);
            #else
                o.custom1 = v.custom1;
                o.custom2 = float4(v.custom2.x, v.custom2.y, v.custom2.z, v.custom2.w + 1);
            #endif
                
                half2 xy = 1;

            #ifdef _USECLIP_ON
                half dissolve = o.custom1.z;
                half edgeWidth = o.custom2.z;
                half rHardness = (2 - _Hardness);
                half edgeDissolve = dissolve * (1 + edgeWidth);
                xy = half2(edgeDissolve, edgeDissolve - edgeWidth);
                xy = xy * (2 - _Hardness);
            #endif

/*
            #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                float4 clipPos = TransformObjectToHClip(v.vertex);
                o.screenPos = ComputeScreenPos(clipPos);
                o.screenPos = o.screenPos / o.screenPos.w;
                o.screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? o.screenPos.z : o.screenPos.z * 0.5 + 0.5;
            #endif
*/
/*
            #ifdef _USERAMP_ON
                half rate = 1 - saturate(dot(_MainLightPosition.xyz, o.worldNormal) * 0.5 + 0.5);
                half rate1 = smoothstep(rate, _RampParam.x, _RampParam.y);
                half rate2 = smoothstep(rate, _RampParam.z, _RampParam.w);
                half4 mixer = lerp(_Color0, _Color1, rate1);
                mixer = lerp(mixer, _Color2, rate2);
                o.mixer = mixer;
            #endif
*/
                half cameraDepthFade = 1;
/*
            #ifdef _CAMERAFADE_ON
                cameraDepthFade = (( o.screenPos.z -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
                cameraDepthFade = saturate(cameraDepthFade);
            #endif
*/
                o.deliver = half4(xy.x,xy.y,1,cameraDepthFade);
                return o;
            }

            half4 frag (v2f i, half vface : VFACE) : SV_Target
            {

                #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                    float2 turbulenceUV = i.uv * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw + frac(half2(_PowerU, _PowerV) * _TimeParameters.x);
                    half turbulenceTex = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, turbulenceUV);
                #endif

                half2 clipXY = i.deliver.xy;
                #ifdef _USECLIP_ON
                    clipXY = 1 + turbulenceTex - clipXY;
                    clipXY = saturate((clipXY - _Hardness) / (1 - _Hardness));
                #endif
                
                half mask = 1;
                #ifdef _USEMASK_ON
                    mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw);
                #endif

                float2 customUV = i.custom2.xy - 1;
                float2 mainUV = (i.uv - 0.5) * customUV;/*i.uv * customUV - (customUV * 0.5);*/
                mainUV += i.uv * _MainTex_ST.xy + _MainTex_ST.zw + i.custom1.xy;
                //return half4(i.uv,0,1);

                #ifdef _USETURBULENCE_ON
                    half turbulence = (turbulenceTex - 0.5) * i.custom1.w;
                    mainUV += turbulence;
                    //mainUV = lerp(mainUV,turbulenceTex, i.custom1.w);
                #endif

                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half3 color = pow(main.rgb, _Contrast) * _ColorInt  * i.color.rgb*_MainColor.rgb;
                color = lerp(color * _WidthColor * i.custom2.w, color, clipXY.x);

                half fresnel = 1;
                /*
                            #ifdef _USEFRESNEL_ON
                                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                                fresnel = dot(i.worldNormal, viewDir);
                                fresnel = pow(1 - fresnel, _fresnelpower);
                                fresnel = min(fresnel, 1) * _fresnelmultiply;
                                fresnel = lerp(fresnel, 1-fresnel, _Flip);
                            #endif
                */
                main.a = min(i.color.a * _MainColor.a * lerp(main.a, main.r, _AlphaR) * clipXY.y * _AlphaInt * fresnel * mask,1);
                /*
                            #ifdef _DEPTHMODE_ON
                                clip(main.a - _CUTOUT);
                            #endif
                */
                /*
                        #ifdef _SHADER_LEVEL_HIGH
                            #ifdef _USEDEPTHFADE_ON
                                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos.xy), _ZBufferParams);
                                float objDepth = LinearEyeDepth(i.screenPos.z, _ZBufferParams);
                                float delta = smoothstep( 0, _FadeLength,abs(sceneDepth - objDepth));
                                main.a *= delta;
                            #endif
                        #endif
                            #ifdef _USERAMP_ON
                                color *= i.mixer.xyz;
                                main.a *= i.mixer.w;
                            #endif
                */

             #ifdef _USESHADOWMASK_ON
                half4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	               shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	               shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                #endif
                half shadow = 1;
				#ifdef _MAIN_LIGHT_SHADOWS
					shadow = MainLightRealtimeShadow(shadowCoord);
				#endif
                color.rgb *= (shadow * _ShadowStr + 1 - _ShadowStr);
             #endif
                half luminance = Luminance(color) + 1E-07;
                color = max(color / luminance * min(luminance, 4), 0);
                main.a *= i.deliver.w;
                main.a = max(main.a, 0);
                
                //#ifdef _ANGLETOG_ON
                half A= 1-saturate(abs(-2*i.uv.x+1));
                half B= _Angle/360;
                half AB = A-B;
                half C = 1- ceil(saturate(AB));
                main.a *= C;
                //#endif

                main.a *= _AlphaPass;
                
                //float distance = length(_WorldSpaceCameraPos - i.worldPos);
                // if(distance>10)
                // {
                //     main.a=0;
                // }
               //main.a *= step(distance,_VanishDistanceV);

                
                return half4(color,main.a);
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags {"LightMode"="UniversalForward"}
            ZTest [_ZTest]
            ZTest LEqual
            //ZWrite [_DepthMode]
            ZWrite Off
            HLSLPROGRAM
            #pragma multi_compile _ _SHADERMODE_ON
            //#pragma multi_compile _ _USEFRESNEL_ON
            #pragma multi_compile _ _USETURBULENCE_ON
            #pragma multi_compile _ _USECLIP_ON
            #pragma multi_compile _ _USEMASK_ON
            #pragma multi_compile _ _DEPTHMODE_ON
            #pragma shader_feature_local _ _USESHADOWMASK_ON
            //#pragma multi_compile _ _ANGLETOG_ON
            //#pragma multi_compile _ _USEDEPTHFADE_ON
            //#pragma multi_compile _ _USERAMP_ON
            //#pragma multi_compile _ _CAMERAFADE_ON
            #pragma multi_compile MAIN_LIGHT_CALCULATE_SHADOWS
			#pragma multi_compile _SHADER_LEVEL_HIGH/* _SHADER_LEVEL_MEDIUM _SHADER_LEVEL_LOW*/
            #pragma multi_compile_instancing
            #pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
                half4 color : COLOR;
            #ifndef _SHADERMODE_ON
                float4 custom1 : TEXCOORD1;
                float4 custom2 : TEXCOORD2;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                //x:? y:? z:dissolve w:distort
                float4 custom1 : TEXCOORD1;
                //x:? y:? z:edgeWidth w:edgeBrightness
                float4 custom2 : TEXCOORD2;
                //x: clipX y:clipY z:fresnel w:camerafade
                float4 deliver : TEXCOORD3;
                /*
                            #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                                float4 screenPos : TEXCOORD4;
                            #endif
                */
                /*
                            #if defined _USEFRESNEL_ON || defined _USERAMP_ON
                                float3 worldNormal : TEXCOORD7;
                            #endif
                            #ifdef _USERAMP_ON
                                half4 mixer : TEXCOORD5;
                            #endif
                */
                #if defined _USEFRESNEL_ON || defined _USESHADOWMASK_ON
                    float3 worldPos : TEXCOORD6;
                #endif
            #ifdef _USESHADOWMASK_ON 
                float4 shadowCoord : TEXCOORD7;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                Texture2D _TurbulenceTex;
                SamplerState sampler_TurbulenceTex;
            #endif

            #ifdef _USEMASK_ON
                Texture2D _MaskTex;
                SamplerState sampler_MaskTex;
            #endif
            
            /*
                #ifdef _SHADER_LEVEL_HIGH
                    #ifdef _USEDEPTHFADE_ON
                        Texture2D _CameraDepthTexture;
                        SamplerState sampler_CameraDepthTexture;
                    #endif
                #endif
            */
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half _AlphaR;
            //half _UseBackColor;
            //half4 _BackColor;
            half4 _MainColor;
            half _ColorInt;
            half _AlphaInt;
            half4 _WidthColor;
            half _Contrast;
            half _Angle;
            /*内置变体
                    #ifdef _DEPTHMODE_ON
                        half _CUTOUT;
                    #endif
            */
            #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                half _PowerU;
                half _PowerV;
                float4 _TurbulenceTex_ST;
            #endif
            #ifdef _USEFRESNEL_ON
                half _fresnelpower;
                half _fresnelmultiply;
                half _Flip;
            #endif
            #ifdef _SHADERMODE_ON
                float _MainPannerX;
                float _MainPannerY;
                half _DistortPower;
                half _Dissolve;
                half _EdgeWidth;
            #endif
            #ifdef _USECLIP_ON
                half _Hardness;
            #endif
            /*
                    #ifdef _USEDEPTHFADE_ON
                        half _FadeLength;
                    #endif
            */
            /*
                    #ifdef _USERAMP_ON
                        half4 _Color0;
                        half4 _Color1;
                        half4 _Color2;
                        half4 _RampParam;
                    #endif
            */
            /*
                    #ifdef _CAMERAFADE_ON
                        half _CameraFadeLength;
                        half _CameraFadeOffset;
                    #endif
            */
            #ifdef _USEMASK_ON
                float4 _MaskTex_ST;
            #endif
            float _ShadowStr;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                /*
                            #if defined _USEFRESNEL_ON || defined _USERAMP_ON
                                o.worldNormal = TransformObjectToWorldNormal(v.normal, true);
                            #endif
                */
                #if defined _USEFRESNEL_ON || defined _USESHADOWMASK_ON
                    o.worldPos = TransformObjectToWorld(v.vertex);
                #endif
                #ifdef _USESHADOWMASK_ON
				    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex);
				    o.shadowCoord = GetShadowCoord( vertexInput );
                #endif
                    
                #ifdef _SHADERMODE_ON
                    float2 panner = float2(_MainPannerX * _TimeParameters.x, _MainPannerY * _TimeParameters.x);
                    panner = frac(panner);
                    o.custom1 = float4(panner.x, panner.y, _Dissolve, _DistortPower);
                    o.custom2 = float4(1, 1, _EdgeWidth, 1);
                #else
                    o.custom1 = v.custom1;
                    o.custom2 = float4(v.custom2.xyz, v.custom2.w + 1);
                #endif
                
                half2 xy = 1;

                #ifdef _USECLIP_ON
                    half dissolve = o.custom1.z;
                    half edgeWidth = o.custom2.z;
                    half rHardness = (2 - _Hardness);
                    half edgeDissolve = dissolve * (1 + edgeWidth);
                    xy = half2(edgeDissolve, edgeDissolve - edgeWidth);
                    xy = xy * (2 - _Hardness);
                #endif
                /*
                            #if defined _USEDEPTHFADE_ON || defined _CAMERAFADE_ON
                                float4 clipPos = TransformObjectToHClip(v.vertex);
                                o.screenPos = ComputeScreenPos(clipPos);
                                o.screenPos = o.screenPos / o.screenPos.w;
                                o.screenPos.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? o.screenPos.z : o.screenPos.z * 0.5 + 0.5;
                            #endif
                */
                /*
                            #ifdef _USERAMP_ON
                                half rate = 1 - saturate(dot(_MainLightPosition.xyz, o.worldNormal) * 0.5 + 0.5);
                                half rate1 = smoothstep(rate, _RampParam.x, _RampParam.y);
                                half rate2 = smoothstep(rate, _RampParam.z, _RampParam.w);
                                half4 mixer = lerp(_Color0, _Color1, rate1);
                                mixer = lerp(mixer, _Color2, rate2);
                                o.mixer = mixer;
                            #endif
                */
                half cameraDepthFade = 1;
                /*
                            #ifdef _CAMERAFADE_ON
                                cameraDepthFade = (( o.screenPos.z -_ProjectionParams.y - _CameraFadeOffset ) / _CameraFadeLength);
                                cameraDepthFade = saturate(cameraDepthFade);
                            #endif
                */
                o.deliver = half4(xy.xy,1,cameraDepthFade);
                return o;
            }

            half4 frag (v2f i, half vface : VFACE) : SV_Target
            {
                #if defined _USETURBULENCE_ON || defined _USECLIP_ON
                    float2 turbulenceUV = i.uv * _TurbulenceTex_ST.xy + _TurbulenceTex_ST.zw + frac(half2(_PowerU, _PowerV) * _TimeParameters.x);
                    half turbulenceTex = SAMPLE_TEXTURE2D(_TurbulenceTex, sampler_TurbulenceTex, turbulenceUV);
                #endif

                half2 clipXY = i.deliver.xy;
                #ifdef _USECLIP_ON
                    clipXY = 1 + turbulenceTex - clipXY;
                    clipXY = saturate((clipXY - _Hardness) / (1 - _Hardness));
                #endif
                
                half mask = 1;
                #ifdef _USEMASK_ON
                    mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv * _MaskTex_ST.xy + _MaskTex_ST.zw);
                #endif

                float2 customUV = i.custom2.xy - 1;
                float2 mainUV = (i.uv - 0.5) * customUV;/*i.uv * customUV - (customUV * 0.5);*/
                mainUV += i.uv * _MainTex_ST.xy + _MainTex_ST.zw + i.custom1.xy;
                //return half4(i.uv,0,1);

                #ifdef _USETURBULENCE_ON
                    half turbulence = (turbulenceTex - 0.5) * i.custom1.w;
                    mainUV += turbulence;
                    //mainUV = lerp(mainUV,turbulenceTex, i.custom1.w);
                #endif

                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainUV);
                half3 color = pow(main.rgb, _Contrast) * _ColorInt  * i.color.rgb*_MainColor.rgb;
                color = lerp(color * _WidthColor * i.custom2.w, color, clipXY.x);

                half fresnel = 1;
                /*
                            #ifdef _USEFRESNEL_ON
                                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                                fresnel = dot(i.worldNormal, viewDir);
                                fresnel = pow(1 - fresnel, _fresnelpower);
                                fresnel = min(fresnel, 1) * _fresnelmultiply;
                                fresnel = lerp(fresnel, 1-fresnel, _Flip);
                            #endif
                */
                main.a = min(i.color.a * _MainColor.a * lerp(main.a, main.r, _AlphaR) * clipXY.y * _AlphaInt * fresnel * mask,1);
                /*
                            #ifdef _DEPTHMODE_ON
                                clip(main.a - _CUTOUT);
                            #endif
                */
                /*
                        #ifdef _SHADER_LEVEL_HIGH
                            #ifdef _USEDEPTHFADE_ON
                                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos.xy), _ZBufferParams);
                                float objDepth = LinearEyeDepth(i.screenPos.z, _ZBufferParams);
                                float delta = smoothstep( 0, _FadeLength,abs(sceneDepth - objDepth));
                                main.a *= delta;
                            #endif
                        #endif
                            #ifdef _USERAMP_ON
                                color *= i.mixer.xyz;
                                main.a *= i.mixer.w;
                            #endif
                */
            #ifdef _USESHADOWMASK_ON
                half4 shadowCoord = float4(0, 0, 0, 0);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	               shadowCoord = i.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	               shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                #endif
                half shadow = MainLightRealtimeShadow(shadowCoord);
                color.rgb *= (shadow * _ShadowStr + 1 - _ShadowStr);
             #endif
                half luminance = Luminance(color) + 1E-07;
                color = max(color / luminance * min(luminance, 4), 0);
                main.a *= i.deliver.w;
                main.a = max(main.a, 0);

                //#ifdef _ANGLETOG_ON
                half A= 1-saturate(abs(-2*i.uv.x+1));
                half B= _Angle/360;
                half AB = A-B;
                half C = 1- ceil(saturate(AB));
                main.a *=C;
                //#endif

                return half4(color,main.a);
            }
            ENDHLSL
        }
        /*
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
        */
    }
}

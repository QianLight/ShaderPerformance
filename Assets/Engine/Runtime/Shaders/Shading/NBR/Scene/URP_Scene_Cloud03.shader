Shader "URP/Scene/Cloud03"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _UVST0("Base Scale Offset", Vector) = (1,1,0,0)
		_DarkColor ("Dark Color", Color) = (0.8, 0.8, 0.8, 1)
        [HDR] _BrightColor ("Bright Color", Color) = (1,1,1,1)
		_Param0 ("xy:UVSpeed z:AlphaSmoothness w:DepthFade", vector) = (0.02, 0, 0.5, 1)
		_Param1 ("xy:UVSpeedNoise", vector) = (0, 0, 0.5, 0.5)
	}

	SubShader
	{
        Tags{"RenderType" = "Transparent" "Queue" = "Transparent"  "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" }
        Cull Back
		ZWrite Off
		
		
		Pass
		{
			Name "FORWARD"
            Tags{"LightMode" = "UniversalForward" }

			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
                //----------------------
                #define URP_BASE
                #define REDEFINE_URP

                //----------------------
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

                #include "../Include/Pragam_Head.hlsl"
                #ifdef _SM_4
                    #pragma target 5.0
                #else
                    #pragma target 3.0
                #endif

                #define _NO_MRT
                #define _SCREEN_POS
                #define _CUSTOM_INTERPOLANTS
                #define _CUSTOM_LOD	
                #define _CLOUD03	
                #include "../Scene/Scene_Head.hlsl"
                #include "../Include/URP_LightingHead.hlsl"
                #include "../Include/SRP_Lighting.hlsl"

                #define _UVSpeed (_Param0.xy)
                #define _UVSpeedNoiseOffset (_Param1.xy)
                #define _UVSpeedNoiseScale (_Param1.zw)

                FInterpolantsVSToPS CustomInterpolantsVSToPS(in FVertexInput Input, in FLOAT4 WorldPosition, out FLOAT4 projPos, in REAL4 instanceRot)
                {
                    DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);

                    projPos = mul(_matrixVP, WorldPosition);

                    Interpolants.NormalWS.xyz = TransformObjectToWorldDir(Input.TangentX, instanceRot);
                    FLOAT3 viewDirWS = _CameraPos.xyz - WorldPosition.xyz;
                    Interpolants.NormalWS.w = viewDirWS.x;
                    Interpolants.TangentWS.w = viewDirWS.y;
                    Interpolants.BitangentWS.w = viewDirWS.z;

                    FLOAT4 centerWS = mul(unity_ObjectToWorld, FLOAT4(0, 0, 0, 1));
                    FLOAT4 centerPS = mul(unity_MatrixVP, centerWS);

                    FLOAT2 uv = mad(projPos.xy - centerPS.xy, _UVST0.xy, _UVST0.zw);

                    float2 uvSpeedNoise = (frac(centerWS.xz + centerWS.yy) - _UVSpeedNoiseOffset) * _UVSpeed;
                    uvSpeedNoise = lerp(normalize(uvSpeedNoise) * _UVSpeed, uvSpeedNoise, _UVSpeedNoiseScale);

                    uv = mad(uvSpeedNoise, _Time.y, uv);
                    Interpolants.TexCoords[0].xy = uv;
                    SET_VS_DEPTH(Interpolants, projPos.zw);
                    return Interpolants;
                }

                FLOAT4 _DarkColor;
                FLOAT4 _BrightColor;               
                #define _AlphaSmoothness (_Param0.z)
                #define _DepthFade (_Param0.w)

                #define _SCENE_EFFECT
                #define _CUSTOM_PS	

                FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout FLOAT4 rt1)
                {
                    FLOAT3 normalWS = normalize(Interpolants.NormalWS.xyz);
                    FLOAT3 viewDirWS = SafeNormalize(FLOAT3(Interpolants.NormalWS.w, Interpolants.TangentWS.w, Interpolants.BitangentWS.w));

                    // Shape Fade
                    FLOAT3 col = SAMPLE_TEX2D(_MainTex, Interpolants.TexCoords[0].xy).xyz;
                    FLOAT ndv = dot(normalWS, viewDirWS);
                	FLOAT minAlpha = min(_DarkColor.w,_BrightColor.w);
                    FLOAT alpha = ndv * ndv * minAlpha;
                    alpha = smoothstep(0, _AlphaSmoothness, alpha);
                    FLOAT ndotL = dot(normalWS, _MainLightDir.xyz) * 0.5 + 0.5;
                    // Lighting
                    col *= lerp(_DarkColor.xyz, _BrightColor.xyz, ndotL);

#ifdef _SOFT_PARTICLE
                    // SoftParticle
                    FLOAT4 screenPos = FLOAT4(Interpolants.ScreenPosition.xy / Interpolants.ScreenPositionW.x, Interpolants.ScreenPosition.zw / Interpolants.ScreenPositionW.y);
                    FLOAT4 depthTex = SAMPLE_TEX2D(_CameraDepthTexture, screenPos.xy);
                    FLOAT sceneDepth = Linear01DepthPers(DecodeFloatRGB(depthTex.xyz));
                    FLOAT depth = Linear01DepthPers(Interpolants.Depth01.x / Interpolants.Depth01.y);
                    FLOAT fade = saturate((sceneDepth - depth) * _DepthFade * 100);
                    alpha *= fade;
#endif
                    
                    CalcSceneColor(col, ndotL);

                    return FLOAT4(col, alpha);
                }

                #include "../Include/URP_Vertex.hlsl"
                #include "../Include/URP_Pixel.hlsl"

                #pragma shader_feature_local _ _LOD0
                #pragma shader_feature_local _ _LOD_M0
                #pragma shader_feature_local _ _LOD_L0

                #pragma vertex vertForwardBase
                #pragma fragment fragForwardBase

			ENDHLSL
        }
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

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

			ENDCG
		}

    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

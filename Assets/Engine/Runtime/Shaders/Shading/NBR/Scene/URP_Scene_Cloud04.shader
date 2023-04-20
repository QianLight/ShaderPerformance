Shader "URP/Scene/Cloud04"
{
	Properties
	{
		_NoiseTex ("NoiseTex", 2D) = "white" {} 
        [HDR]_BrightColor ("Bright Color", Color) = (1,1,1,1) 
		_DarkColor1 ("Dark Color", Color) = (0.8, 0.8, 0.8, 1)
		_Adjust ("x:Height y:Speed z:DepthFade w:", vector) = (0,0,0,0)
	
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
                #define _CUSTOM_CLOUD04
                #include "../Scene/Scene_Head.hlsl"
                #include "../Include/URP_LightingHead.hlsl"
                #include "../Include/SRP_Lighting.hlsl"
                TEX2D_SAMPLER(_NoiseTex);
				FLOAT4 _BrightColor,_DarkColor1,_Adjust;
				#define _DepthFade _Adjust.z
                #define _Speed _Adjust.y
                //#define _AlphaSmoothness _Param0.w

                FInterpolantsVSToPS CustomInterpolantsVSToPS(in FVertexInput Input, in FLOAT4 WorldPosition, out FLOAT4 projPos, in REAL4 instanceRot)
                {
                    DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);

                    projPos = mul(_matrixVP, WorldPosition);               
                    Interpolants.TexCoords[0].xy = Input.uv0;
                    SET_VS_DEPTH(Interpolants, projPos.zw);
                    return Interpolants;
                }

                #define _SCENE_EFFECT
                #define _CUSTOM_PS	
                FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout FLOAT4 rt1)
                {
                    // Shape Fade
                    FLOAT alpha =  _BrightColor.a;

                    // Lighting
                    FLOAT2 uv1 = Interpolants.TexCoords[0].xy + _Time.x * _Speed;
					FLOAT2 uv2 = Interpolants.TexCoords[0].xy - _Time.x * _Speed;
					FLOAT NoisePosx = SAMPLE_TEX2D(_NoiseTex, uv1).r;
					FLOAT NoisePosy = SAMPLE_TEX2D(_NoiseTex, uv2).r;
                    FLOAT3 Normalocal = (NoisePosx * NoisePosy).xxx;
                    REAL3x3 TangentToWorld = REAL3x3(Interpolants.TangentWS.xyz, Interpolants.BitangentWS.xyz, Interpolants.NormalWS.xyz);
                    FLOAT3 normalDri = normalize (mul(Normalocal, TangentToWorld).xyz);
                    FLOAT NL = max( dot( Normalocal , _MainLightDir.xyz ) , 0);                   

                    // SoftParticle
                    FLOAT4 screenPos = FLOAT4(Interpolants.ScreenPosition.xy / Interpolants.ScreenPositionW.x, Interpolants.ScreenPosition.zw / Interpolants.ScreenPositionW.y);
                    FLOAT4 depthTex = SAMPLE_TEX2D(_CameraDepthTexture, screenPos.xy);
                    FLOAT sceneDepth = Linear01DepthPers(DecodeFloatRGB(depthTex.xyz));
                    FLOAT depth = Linear01DepthPers(Interpolants.Depth01.x / Interpolants.Depth01.y);
                    FLOAT fade = saturate((sceneDepth - depth) * _DepthFade * 100);
                    alpha *= fade;//end

                    FLOAT4 Color = _BrightColor * NoisePosx * NoisePosy + _DarkColor1 * (1-NoisePosx * NoisePosy);
                    return FLOAT4(Color.xyz, alpha);
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
   //CustomEditor "CFEngine.Editor.PBSShaderGUI"
}

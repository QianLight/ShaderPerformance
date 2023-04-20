Shader "URP/Role/StencilShadowCaster"
{
    Properties
    {
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"
        }

		Pass
        {
            Name "FaceShadowCaster"
            
            Tags { "LightMode" = "FaceShadowCaster" "Queue"="Opaque" "RenderType"="Opaque" }

        	ZWrite Off
        	
        	ColorMask 0

            Stencil
            {
                Ref [_StencilRef_StencilShadow]
                Comp NotEqual
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #ifdef UNIFORM_PCH_OFF
				#undef UNIFORM_PCH_OFF
            #endif

            float4 _FaceShadowParam;

			#include "Role_StencilShadowCaster.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
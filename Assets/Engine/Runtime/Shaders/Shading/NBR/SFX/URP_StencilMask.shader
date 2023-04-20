Shader "URP/SFX/URP_StencilMask"
{
    Properties
    {
        _MainColor("MainColor", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white"{}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "LightMode"="StencilMask"    
        }
        LOD 100

        Pass
        {
            ColorMask 0
            stencil
            {
                Ref [_StencilRef_PostProcessMask]
                Comp Always
                Fail Replace
                Pass Replace
            }
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            half4 frag (Varyings input) : SV_Target
            {
                return 0;
			}
            

            ENDHLSL
        }
    }
}

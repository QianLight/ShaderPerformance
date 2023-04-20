Shader "Unlit/EmojiText"
{
       Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EmojiTex ("Emoji Texture", 2D) = "white" {}
		 
		 _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    	[Toggle(_IS_NOT_IN_UI_CAMERA)] _isNotInUICamera ("It isn't in UI-Camera", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        ColorMask [_ColorMask]

        Pass
        {
            
			Blend One OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM

				#pragma target 4.5
				#define _UI_GAMMA
                #define UI_PIXEL_COLOR_TO_GREY_SWITCH_OFF
                
                sampler2D _EmojiTex;
				#include "UIVertex.hlsl"				
				#pragma multi_compile __ UNITY_UI_CLIP_RECT
				#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			    //#pragma multi_compile _ _IS_NOT_IN_UI_CAMERA
				#pragma vertex uiVert
				#pragma fragment uiFrag
                
                #define INITIALIZE_COLOR(Interpolants,color) InitializeColor(Interpolants,color)
                inline void InitializeColor(UIInterpolantsVSToPS Interpolants, out FLOAT4 color)
                {                    
                    float flag=(1-step(Interpolants.uv0.x,0))*(1-step(Interpolants.uv0.y,0));
                    //flag==0表示是emoji  1表示是文字
                    color = (1-flag)*(tex2D(_EmojiTex, -Interpolants.uv0))*float4(1,1,1,Interpolants.color.a);                                     
	                color += (flag)*(SAMPLE_TEX2D(_MainTex, Interpolants.uv0) + _TextureSampleAdd)*Interpolants.color;                 
                   
                }
            #include "UIPixel.hlsl" 
			ENDHLSL
        }
    }
    	
}

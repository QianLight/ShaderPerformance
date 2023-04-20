// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/Custom/Editor/TerrainEdit_Custom" {
    Properties {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
    }

    SubShader {
        Tags {
            "Queue" = "Geometry-100" 
            "RenderType" = "Opaque"
            "LightMode"="ForwardBase"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor01 finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.
        #pragma target 3.0
        // needs more than 8 texcoords
        //#pragma exclude_renderers gles
        #include "UnityPBSLighting.cginc" 

        #pragma multi_compile_local __ _NORMALMAP

        #define TERRAIN_STANDARD_SHADER
        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
        #include "CustomTerrainSplatmapCommon.cginc"
        #include "OP_Shadow.cginc"

        void SplatmapFinalColor01(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
        {
            color *= o.Alpha;
            #ifdef TERRAIN_SPLAT_ADDPASS
                UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
            #else
                UNITY_APPLY_FOG(IN.fogCoord, color);
            #endif
            color*=float4(shadowCal(IN),1);
        }

        half _Metallic0;
        half _Metallic1;
        half _Metallic2;
        half _Metallic3;

        half _Smoothness0;
        half _Smoothness1;
        half _Smoothness2;
        half _Smoothness3;
        // void SplatmapFinalColor01(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
        // {
        //     color *= o.Alpha;
        //     #ifdef TERRAIN_SPLAT_ADDPASS
        //         UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
        //     #else
        //         UNITY_APPLY_FOG(IN.fogCoord, color);
        //     #endif
        //     color*=float4(shadowCal(IN),1);
        // }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            half4 splat_control;
            half weight;
            fixed4 mixedDiffuse;
            half4 defaultSmoothness = half4(0, _Smoothness1, _Smoothness2, _Smoothness3);
            SplatmapMix(IN, defaultSmoothness, splat_control, weight, mixedDiffuse, o.Normal);
            o.Albedo = mixedDiffuse;
            o.Alpha = weight;
            o.Smoothness = mixedDiffuse.a;
            o.Metallic = dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        }
        ENDCG

         UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
         UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

   Dependency "AddPassShader"    = "Hidden/TerrainEngine/Splatmap/Standard-AddPass_Custom"
    Dependency "BaseMapShader"    = "Hidden/TerrainEngine/Splatmap/Standard-Base"
    Dependency "BaseMapGenShader" = "Hidden/TerrainEngine/Splatmap/Standard-BaseGen"

   // Fallback "Nature/Terrain/Diffuse"
}

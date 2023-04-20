#ifndef __PRECOMPUTED_SCATTER__
#define __PRECOMPUTED_SCATTER__

    #define MOON 0

    // Constants
    #define INTERNAL_PI 3.1415926535
    #define Pi316 0.0596831
    #define Pi14 0.07957747
    #define MieG float3(0.4375f, 1.5625f, 1.5f)

    // Uniforms
    uniform float _AtmospherFogScatteringScale;
    uniform float4x4 _AtmospherUpDirectionMatrix;
    uniform int    _AtmospherScatteringMode;
    uniform float3 _AtmospherRayleigh;
    uniform float3 _AtmospherMie;
    uniform float  _AtmospherScattering;
    uniform float  _AtmospherExposure;
    uniform float4 _AtmospherRayleighColor;
    uniform float4 _AtmospherMieColor;
    uniform float  _AtmospherMieDepth;
    uniform float  _AtmospherLuminance;
    uniform float3 _AtmospherMoonDirection;
    uniform float3 _AtmospherSunDirection;

    uniform float4 _FogScatterColor;
    
    #if MOON
        #define ROW_COUNT 6.0
    #else
        #define ROW_COUNT 4.0
    #endif

    #define FOG_SUN_VDL_SCATTER 0.0
    #define FOG_SUN_DEPTH_SCATTER 1.0
    #define SKYBOX_VDL_SCATTER 2.0
    #define SKYBOX_HEIGHT_SCATTER 3.0
    #define FOG_MOON_VDL_SCATTER 4.0
    #define FOG_MOON_DEPTH_SCATTER 5.0

    float3 ComputeFogSunVdlScatter(float vdl)
    {
        float sunRise = saturate(_MainLightDir0.y * 10);
        float  rayPhase = 2.0 + 0.5 * vdl * vdl;
        float  miePhase = MieG.x / pow(max(0, MieG.y - MieG.z * vdl), 1.5);
        float3 BrTheta  = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        float3 BmTheta  = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
        float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        float3 inScatter = BrmTheta * _AtmospherScattering * sunRise;
        inScatter = lerp(inScatter, dot(inScatter, float3(0.2126729, 0.7151522, 0.0721750)) * _FogScatterColor.rgb, _FogScatterColor.a);
        return inScatter;
    }

    float3 ComputeFogSunDepthScatter(float depth)
    {
        float zenith = acos(saturate(-depth)) * _AtmospherFogScatteringScale;
        float z = (cos(zenith) + 0.15 * pow(max(0, 93.885 - ((zenith * 180.0f) / INTERNAL_PI)), -1.253));
        float2 srsm = float2(8400.0, 1200.0) / z;
        float3 fex = exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
        float sunset = clamp(_MainLightDir0.y, 0.0, 0.5);
        float3 Esun = lerp(fex, 1.0 - fex, sunset);
        float mieDepth = saturate(lerp(depth * 4, 1.0, _AtmospherMieDepth));
        float3 scatter = Esun * (1.0 - fex) * mieDepth;
        return scatter;
    }

    float3 ComputeSkyboxVdlScatterNoneNegative(float vdl)
    {
        float sunRise = saturate(_MainLightDir0.y * 10.0);
        vdl = (vdl-0.5)*2;
        float rayPhase = 2.0 + 0.5 * vdl * vdl;
        float miePhase = MieG.x / pow(max(0, MieG.y - MieG.z * vdl), 1.5);
        float3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        float3 BmTheta = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
        float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        float3 scatter = BrmTheta * sunRise;
        return scatter;
    }
    float3 ComputeSkyboxVdlScatter(float vdl)
    {
        float sunRise = saturate(_MainLightDir0.y * 10.0);
        float rayPhase = 2.0 + 0.5 * vdl * vdl;
        float miePhase = MieG.x / pow(max(0, MieG.y - MieG.z * vdl), 1.5);
        float3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        float3 BmTheta = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
        float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        float3 scatter = BrmTheta * sunRise;
        return scatter;
    }

    float3 ComputeSkyboxHeightScatter(float height)
    {
        float zenith = acos(saturate(height)) * _AtmospherFogScatteringScale;
        float z = (cos(zenith) + 0.15 * pow(max(0, 93.885 - ((zenith * 180.0f) / INTERNAL_PI)), -1.253));
        float SR = 8400.0 / z;
        float SM = 1200.0 / z;
        float3 fex = exp(-(_AtmospherRayleigh * SR  + _AtmospherMie * SM));
        float sunset = clamp(dot(float3(0.0, 1.0, 0.0), _MainLightDir0.xyz), 0.0, 0.5);
        float3 Esun = lerp(fex, (1.0 - fex), sunset);
        float3 scatter = Esun * _AtmospherScattering * (1.0 - fex);
        return scatter;
    }
    
    float3 ComputeFogMoonVdlScatter(float vdm)
    {
        float sunRise = saturate(_MainLightDir0.y * 10.0);
        float moonRise = saturate(_AtmospherMoonDirection.y * 10.0);
        float rayPhase = 2.0 + 0.5 * vdm * vdm;
        float miePhase = MieG.x / pow(max(0, MieG.y - MieG.z * vdm), 1.5);
        float3 BrTheta  = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        float3 BmTheta  = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * moonRise; // * mieDepth; TODO : Check differences
        float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        float3 moonInScatter = BrmTheta * _AtmospherScattering * 0.1;
        moonInScatter *= 1.0 - sunRise;
        return moonInScatter;
    }
    
    float3 ComputeFogMoonDepthScatter(float depth)
    {
        float zenith = acos(saturate(-depth)) * _AtmospherFogScatteringScale;
        float z = (cos(zenith) + 0.15 * pow(max(0, 93.885 - ((zenith * 180.0f) / INTERNAL_PI)), -1.253));
        float2 srsm = float2(8400.0, 1200.0) / z;
        float3 fex = exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
        float moonset = clamp(_AtmospherMoonDirection.y, 0.0, 0.5);
        float3 Esun = lerp(fex, 1.0 - fex, moonset);
        float mieDepth = saturate(lerp(depth * 4, 1.0, _AtmospherMieDepth));
        float3 scatter = Esun * (1.0 - fex) * mieDepth;
        return scatter;
    }

	#if defined(PRECOMPUTE_SCATTER) && !defined(PRECOMPUTE_SCATTER_LEGACY)

        #define FOG_VDL_COUNT 32
        #define FOG_DEPTH_COUNT 32
        #define SKYBOX_VDL_COUNT 32
        #define SKYBOX_HEIGHT_COUNT 32

        float4 _ScatterFogVdl[FOG_VDL_COUNT];
        float4 _ScatterFogDepth[FOG_DEPTH_COUNT];
        float4 _ScatterSkyboxVdl[SKYBOX_VDL_COUNT];
        float4 _ScatterSkyboxHeight[SKYBOX_HEIGHT_COUNT];


        // #define ARRAY_EVALUATE(array, v) array[floor(v)].xyz
        #define ARRAY_EVALUATE(array, v) lerp(array[floor(v)].xyz, array[ceil(v)].xyz, frac(v))

        float3 GetFogSunVdlScatter(float vdl)
        {
            vdl = vdl * 0.5 + 0.5;
            vdl *= vdl;
            vdl *= vdl;
            vdl *= FOG_VDL_COUNT - 1;
            return ARRAY_EVALUATE(_ScatterFogVdl, vdl);
        }

        float3 GetFogSunDepthScatter(float depth)
        {
            depth *= FOG_DEPTH_COUNT - 1;
            return ARRAY_EVALUATE(_ScatterFogDepth, depth);
        }

        float3 GetSkyboxVdlScatterNoneNegative(float vdl)
        {
            vdl *= vdl;
            vdl *= vdl;
            vdl *= FOG_VDL_COUNT - 1;
            return ARRAY_EVALUATE(_ScatterSkyboxVdl, vdl);
        }
                
        float3 GetSkyboxVdlScatter(float vdl)
        {
            vdl = vdl * 0.5f + 0.5f;
            vdl *= vdl;
            vdl *= vdl;
            vdl *= FOG_VDL_COUNT - 1;
            return ARRAY_EVALUATE(_ScatterSkyboxVdl, vdl);
        }
                
        float3 GetSkyboxHeightScatter(float viewY)
        {
            viewY = (viewY * 0.5 + 0.5) * (SKYBOX_HEIGHT_COUNT - 1);
            return ARRAY_EVALUATE(_ScatterSkyboxHeight, viewY);
        }

    #elif defined(PRECOMPUTE_SCATTER)
    
        // Precompute
        sampler2D _PrecomputeScatterTex;

        #define GETV(row) (1.0 / ROW_COUNT * (row + 0.5))

        float3 GetFogSunVdlScatter(float vdl)
        {
            const float v = GETV(FOG_SUN_VDL_SCATTER);
            float2 uv = float2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        float3 GetFogSunDepthScatter(float depth)
        {
            const float v = GETV(FOG_SUN_DEPTH_SCATTER);
            float2 uv = float2(depth, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }
        
        float3 GetFogMoonVdlScatter(float vdl)
        {
            const float v = GETV(FOG_MOON_VDL_SCATTER);
            float2 uv = float2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        float3 GetFogMoonDepthScatter(float depth)
        {
            const float v = GETV(FOG_MOON_DEPTH_SCATTER);
            float2 uv = float2(depth, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        float3 GetSkyboxVdlScatterNoneNegative(float vdl)
        {
            const float v = GETV(SKYBOX_VDL_SCATTER);
            float2 uv = float2(vdl, v);
            return  tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        float3 GetSkyboxVdlScatter(float vdl)
        {
            const float v = GETV(SKYBOX_VDL_SCATTER);
            float2 uv = float2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }
        
        float3 GetSkyboxHeightScatter(float viewY)
        {
            const float v = GETV(SKYBOX_HEIGHT_SCATTER);
            float2 uv = float2(viewY * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

	#else
    	
        float3 GetFogSunVdlScatter(float vdl)
        {
            return ComputeFogSunVdlScatter(vdl);
        }

        float3 GetFogSunDepthScatter(float depth)
        {
            return ComputeFogSunDepthScatter(depth);
        }
        
        float3 GetSkyboxVdlScatterNoneNegative(float vdl)
        {
            return ComputeSkyboxVdlScatterNoneNegative(vdl);
        }
        float3 GetSkyboxVdlScatter(float vdl)
        {
            return ComputeSkyboxVdlScatter(vdl);
        }
        
        float3 GetSkyboxHeightScatter(float height)
        {
            return ComputeSkyboxHeightScatter(height);
        }

        float3 GetFogMoonVdlScatter(float vdm)
        {
            return ComputeFogMoonVdlScatter(vdm);
        }
        
        float3 GetFogMoonDepthScatter(float depth)
        {
            return ComputeFogMoonDepthScatter(depth);
        }

	#endif

        float3 ScatterNoneNegative(FLOAT vdl, FLOAT height)
        {
            float3 sunVdlScatter = GetSkyboxVdlScatterNoneNegative(vdl);
            float3 sunHeightScatter = GetSkyboxHeightScatter(height);
            float3 scatter = sunVdlScatter * sunHeightScatter;
            scatter = saturate(1.0 - exp(-_AtmospherExposure * scatter));
            return scatter;
        }
        float3 Scatter(FLOAT vdl, FLOAT height)
        {
            float3 sunVdlScatter = GetSkyboxVdlScatter(vdl);
            float3 sunHeightScatter = GetSkyboxHeightScatter(height);
            float3 scatter = sunVdlScatter * sunHeightScatter;
            scatter = saturate(1.0 - exp(-_AtmospherExposure * scatter));
            return scatter;
        }

        float3 GetScatterRGB(float3 viewDirectionWS, float3 posWS, float3 cameraPosWS, float3 mainLightDir, out float depth01, float4 projectionParams, float4 cameraBackward) {
            float vdl = dot(viewDirectionWS, mainLightDir);
            float3 delta = posWS - cameraPosWS;          
            float cameraBackwardDotDelta = dot(cameraBackward.xyz, delta);
            depth01 = saturate((cameraBackwardDotDelta - projectionParams.y) / (projectionParams.z - projectionParams.y));
            return saturate(Scatter(-vdl, -viewDirectionWS.y));
        }
#endif
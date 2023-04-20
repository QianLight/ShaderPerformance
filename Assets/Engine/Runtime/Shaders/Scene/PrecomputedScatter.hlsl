#ifndef __PRECOMPUTED_SCATTER__
#define __PRECOMPUTED_SCATTER__

    #define MOON 0

    // Constants
    #define INTERNAL_PI 3.1415926535
    #define Pi316 0.0596831
    #define Pi14 0.07957747
    #define MieG half3(0.4375f, 1.5625f, 1.5f)

    // Uniforms
    uniform half _AtmospherFogScatteringScale;
    uniform half4x4 _AtmospherUpDirectionMatrix;
    uniform int    _AtmospherScatteringMode;
    uniform half3 _AtmospherRayleigh;
    uniform half3 _AtmospherMie;
    uniform half  _AtmospherScattering;
    uniform half  _AtmospherExposure;
    uniform half4 _AtmospherRayleighColor;
    uniform half4 _AtmospherMieColor;
    uniform half  _AtmospherMieDepth;
    uniform half  _AtmospherLuminance;
    uniform half3 _AtmospherMoonDirection;
    uniform half3 _AtmospherSunDirection;

    uniform half4 _FogScatterColor;
    
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

    half3 ComputeFogSunVdlScatter(half vdl)
    {
        half sunRise = saturate(_MainLightDir0.y * 10);
        half  rayPhase = 2.0 + 0.5 * vdl * vdl;
        half  miePhase = MieG.x / pow(max(0, MieG.y - MieG.z * vdl), 1.5);
        half3 BrTheta  = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        half3 BmTheta  = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
        half3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        half3 inScatter = BrmTheta * _AtmospherScattering * sunRise;
        inScatter = lerp(inScatter, Luminance(inScatter) * _FogScatterColor.rgb, _FogScatterColor.a);
        return inScatter;
    }

    half3 ComputeFogSunDepthScatter(half depth)
    {
        half zenith = acos(saturate(-depth)) * _AtmospherFogScatteringScale;
        half z = (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0f) / INTERNAL_PI), -1.253));
        half2 srsm = half2(8400.0, 1200.0) / z;
        half3 fex = exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
        half sunset = clamp(_MainLightDir0.y, 0.0, 0.5);
        half3 Esun = lerp(fex, 1.0 - fex, sunset);
        half mieDepth = saturate(lerp(depth * 4, 1.0, _AtmospherMieDepth));
        half3 scatter = Esun * (1.0 - fex) * mieDepth;
        return scatter;
    }
    
    half3 ComputeSkyboxVdlScatter(half vdl)
    {
        half sunRise = saturate(_MainLightDir0.y * 10.0);
        half rayPhase = 2.0 + 0.5 * vdl * vdl;
        half miePhase = MieG.x / pow(MieG.y - MieG.z * vdl, 1.5);
        half3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        half3 BmTheta = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
        half3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        half3 scatter = BrmTheta * sunRise;
        return scatter;
    }

    half3 ComputeSkyboxHeightScatter(half height)
    {
        half zenith = acos(saturate(height)) * _AtmospherFogScatteringScale;
        half z = (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0f) / INTERNAL_PI), -1.253));
        half SR = 8400.0 / z;
        half SM = 1200.0 / z;
        half3 fex = exp(-(_AtmospherRayleigh * SR  + _AtmospherMie * SM));
        half sunset = clamp(dot(half3(0.0, 1.0, 0.0), _MainLightDir0.xyz), 0.0, 0.5);
        half3 Esun = lerp(fex, (1.0 - fex), sunset);
        half3 scatter = Esun * _AtmospherScattering * (1.0 - fex);
        return scatter;
    }
    
    half3 ComputeFogMoonVdlScatter(half vdm)
    {
        half sunRise = saturate(_MainLightDir0.y * 10.0);
        half moonRise = saturate(_AtmospherMoonDirection.y * 10.0);
        half rayPhase = 2.0 + 0.5 * vdm * vdm;
        half miePhase = MieG.x / pow(MieG.y - MieG.z * vdm, 1.5);
        half3 BrTheta  = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
        half3 BmTheta  = Pi14  * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * moonRise; // * mieDepth; TODO : Check differences
        half3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        half3 moonInScatter = BrmTheta * _AtmospherScattering * 0.1;
        moonInScatter *= 1.0 - sunRise;
        return moonInScatter;
    }
    
    half3 ComputeFogMoonDepthScatter(half depth)
    {
        half zenith = acos(saturate(-depth)) * _AtmospherFogScatteringScale;
        half z = (cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0f) / INTERNAL_PI), -1.253));
        half2 srsm = half2(8400.0, 1200.0) / z;
        half3 fex = exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
        half moonset = clamp(_AtmospherMoonDirection.y, 0.0, 0.5);
        half3 Esun = lerp(fex, 1.0 - fex, moonset);
        half mieDepth = saturate(lerp(depth * 4, 1.0, _AtmospherMieDepth));
        half3 scatter = Esun * (1.0 - fex) * mieDepth;
        return scatter;
    }

	#ifdef PRECOMPUTE_SCATTER
    
        // Precompute
        sampler2D _PrecomputeScatterTex;

        #define GETV(row) (1.0 / ROW_COUNT * (row + 0.5))

        half3 GetFogSunVdlScatter(half vdl)
        {
            const half v = GETV(FOG_SUN_VDL_SCATTER);
            half2 uv = half2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        half3 GetFogSunDepthScatter(half depth)
        {
            const half v = GETV(FOG_SUN_DEPTH_SCATTER);
            half2 uv = half2(depth, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }
        
        half3 GetFogMoonVdlScatter(half vdl)
        {
            const half v = GETV(FOG_MOON_VDL_SCATTER);
            half2 uv = half2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        half3 GetFogMoonDepthScatter(half depth)
        {
            const half v = GETV(FOG_MOON_DEPTH_SCATTER);
            half2 uv = half2(depth, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

        half3 GetSkyboxVdlScatter(half vdl)
        {
            const half v = GETV(SKYBOX_VDL_SCATTER);
            half2 uv = half2(vdl * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }
        
        half3 GetSkyboxHeightScatter(half viewY)
        {
            const half v = GETV(SKYBOX_HEIGHT_SCATTER);
            half2 uv = half2(viewY * 0.5 + 0.5, v);
            return tex2D(_PrecomputeScatterTex, uv).xyz;
        }

	#else
    	
        half3 GetFogSunVdlScatter(half vdl)
        {
            return ComputeFogSunVdlScatter(vdl);
        }

        half3 GetFogSunDepthScatter(half depth)
        {
            return ComputeFogSunDepthScatter(depth);
        }
        
        half3 GetSkyboxVdlScatter(half vdl)
        {
            return ComputeSkyboxVdlScatter(vdl);
        }
        
        half3 GetSkyboxHeightScatter(half height)
        {
            return ComputeSkyboxHeightScatter(height);
        }

        half3 GetFogMoonVdlScatter(half vdm)
        {
            return ComputeFogMoonVdlScatter(vdm);
        }
        
        half3 GetFogMoonDepthScatter(half depth)
        {
            return ComputeFogMoonDepthScatter(depth);
        }

	#endif

#endif
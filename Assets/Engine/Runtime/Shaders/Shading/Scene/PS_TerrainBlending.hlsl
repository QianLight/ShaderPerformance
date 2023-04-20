#ifndef ATHENA_TERRAIN_BLENDING_INCLUDE
#define ATHENA_TERRAIN_BLENDING_INCLUDE
#if defined(_ENABLE_TERRAIN_BLENDING)
    TEXTURE2D(_TerrainAlbedo);
    TEXTURE2D(_TerrainNormal);
    TEXTURE2D_X_FLOAT(_TerrainDepth);
    SAMPLER(sampler_TerrainAlbedo);
    SAMPLER(sampler_TerrainNormal);
    SAMPLER(sampler_TerrainDepth);

    float _BlendDistance;

    inline float remap(float x, float t1, float t2, float s1, float s2)
    {
        return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
    }

    void TerrainBlending(float2 screenUV, float wDepth, inout half3 albedo,inout half occlusion,  inout half smoothness, inout half3 normal)
    {
        UNITY_BRANCH
        if(wDepth<=_BlendDistance)
        {
            //#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
            //float depthTex = 1-SAMPLE_DEPTH_TEXTURE(_TerrainDepth, sampler_TerrainDepth, screenUV);
            //#else
            //float depthTex = SAMPLE_DEPTH_TEXTURE(_TerrainDepth, sampler_TerrainDepth, screenUV);
            //#endif
            float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_TerrainDepth, sampler_TerrainDepth, screenUV), _ZBufferParams) - wDepth;
            float diff = saturate(abs(sceneZ) * remap(_BlendOffset, 0.0, 1.0, 30, 0.25));

            diff = pow(diff, _BlendFactor);
            diff = sqrt(diff);

            //diff = max(diff,0);
             	    
            float4 terrain = SAMPLE_TEXTURE2D(_TerrainAlbedo, sampler_TerrainAlbedo, screenUV);
            float4 terrainNormal = SAMPLE_TEXTURE2D(_TerrainNormal, sampler_TerrainNormal, screenUV);
            albedo = lerp(terrain.xyz*_ColorTerr.rgb, albedo, diff);
            //albedo = terrain.xyz*_ColorTerr.rgb;
            occlusion = lerp(terrain.w, occlusion, diff);
            normal = lerp(terrainNormal.xyz, normal, diff);//+0.129f
            //normal = normal;
            //normal = terrainNormal.xyz;
            smoothness = lerp(terrainNormal.w, smoothness, diff);
        }
    }
    #else
    void TerrainBlending(float2 screenUV, float wDepth, inout half3 albedo,inout half occlusion, inout half smoothness, inout half3 normal ){}
    #endif                  // _ENABLE_TERRAIN_BLENDING //
#endif                       // ATHENA_TERRAIN_BLENDING_INCLUDE //

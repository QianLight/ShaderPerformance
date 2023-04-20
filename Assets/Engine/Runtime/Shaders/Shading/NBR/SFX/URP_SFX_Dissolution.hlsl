#ifndef __SFX_DISSOLUTION__
#define __SFX_DISSOLUTION__

    FLOAT4 PrepareDissolution2Layer(FLOAT progress, FLOAT softness, FLOAT edgeWidth, FLOAT edgeSoftness)
    {
        FLOAT2 fixedSoftness = clamp(FLOAT2(softness, edgeSoftness), 1e-4, 1);
        FLOAT scale = 1 + max(fixedSoftness.x, edgeWidth + fixedSoftness.y);
        FLOAT2 fixedProgress;
        fixedProgress.x = progress * scale;
        fixedProgress.y = fixedProgress.x - edgeWidth;
        FLOAT2 mulTerm = FLOAT2(1 / fixedSoftness.x, 1 / fixedSoftness.y);
        FLOAT2 softness2 = FLOAT2(fixedSoftness.x, fixedSoftness.y);
        FLOAT2 addTerm = mulTerm * (softness2 - fixedProgress);
        return FLOAT4(mulTerm, addTerm);
    }

    FLOAT4 PrepareDissolution1Layer(FLOAT progress, FLOAT softness)
    {
        softness = clamp(softness, 1e-4, 1);
        FLOAT fixedProgress = progress * (1 + softness);
        FLOAT mulTerm = 1 / softness;
        FLOAT addTerm = (softness - fixedProgress) * mulTerm;
        return FLOAT4(mulTerm, addTerm, 0, 0);
    }

    FLOAT4 PrepareDissolution(bool edgeEnable, FLOAT progress, FLOAT softness, FLOAT edgeWidth, FLOAT edgeSoftness)
    {
        progress = 1 - progress;
        [branch]
        if (edgeEnable)
        {
            return PrepareDissolution2Layer(progress, softness, edgeWidth, edgeSoftness);
        }
        else
        {
            return PrepareDissolution1Layer(progress, softness);
        }
    }

    FLOAT4 Dissolution1Layer(FLOAT4 color, FLOAT tex, FLOAT4 dissolutionData)
    {
        color.a *= saturate(tex * dissolutionData.x + dissolutionData.y);
        return color;
    }

    FLOAT4 Dissolution2Layer(FLOAT4 color, FLOAT2 tex, FLOAT4 dissolutionData, FLOAT4 edgeColor)
    {
        FLOAT2 layers = saturate(tex * dissolutionData.xy + dissolutionData.zw);
        color.a *= layers.x;
        edgeColor.a *= layers.y;
        color = lerp(edgeColor, color, color.a);
        return color;
    }

    FLOAT4 Dissolution(bool edgeEnable, FLOAT4 color, FLOAT tex, FLOAT4 dissolutionData, FLOAT4 edgeColor)
    {
        [branch]
        if (edgeEnable)
        {
            return Dissolution2Layer(color, tex, dissolutionData, edgeColor * color);
        }
        else
        {
            return Dissolution1Layer(color, tex, dissolutionData);
        }
    }

#endif

#ifndef VERTICAL_COLOR_INCLUDE
#define VERTICAL_COLOR_INCLUDE

void VerticalColor(
    half4 albedo,
    half3 positionWS,
    half4 gradientEnd,
    half4 gradientBegin,
    half3 worldPositionOffset,
    half3 worldPositionScale,
    int gradientSwitch,
    int blendMode,
    half gradientScale,
    half blendIntensity,
    out half4 color
)

{
    half3 positionMove = worldPositionOffset-(worldPositionScale*0.5);
    half3 newposition = positionWS-positionMove;
    half gradient = pow(saturate(newposition.y/worldPositionScale.y) , gradientScale);
    half4 lerpMode = lerp(gradientEnd ,gradientBegin ,gradient);
    half4 mulMode = lerpMode * albedo;
    half4 addMulMode = (lerpMode + albedo) * albedo;
    half4 blendColor;
    half blendClrAlpha = lerp(gradientEnd.w ,gradientBegin.w ,gradient);
    [branch]if( blendMode == 0 )
    {
        blendColor = mulMode;
    }
    [branch]if( blendMode == 1 )
    {
        blendColor = addMulMode;
    }
    [branch]if( blendMode == 2 )
    {
        blendColor = lerpMode;
    }
    
    half4 finalColor = lerp(albedo , blendColor , blendIntensity * blendClrAlpha);
    
    color =(( gradientSwitch )?( finalColor ):( albedo ));
}

#endif
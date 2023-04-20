
#ifndef POSTPROCESS_DEBUG_INCLUDE
#define POSTPROCESS_DEBUG_INCLUDE

#ifdef _ENABLE_DEBUG

struct FCustomData
{
	FLOAT4 src;
	FLOAT3 bloom;
	FLOAT3 godray;
	FLOAT3 radialBlurArea;
};

//DEBUG_START
#define Debug_None 0
#define Debug_F_Uber_Depth (Debug_None+1)
#define Debug_F_Uber_Lut (Debug_F_Uber_Depth+1)
#define Debug_F_Uber_BloomColor (Debug_F_Uber_Lut+1)
#define Debug_F_Uber_BloomIntensity (Debug_F_Uber_BloomColor+1)
#define Debug_F_Uber_GodRay (Debug_F_Uber_BloomIntensity+1)
#define Debug_F_Uber_HSV (Debug_F_Uber_GodRay+1)
#define Debug_F_Uber_H (Debug_F_Uber_HSV+1)
#define Debug_F_Uber_S (Debug_F_Uber_H+1)
#define Debug_F_Uber_V (Debug_F_Uber_S+1)

#define Debug_F_RadialBlur_Area (Debug_F_Uber_V+1)
#define Debug_Custom_Start (Debug_F_RadialBlur_Area+1)

#define Debug_DebugRT (Debug_Custom_Start)
#define Debug_DebugRTA (Debug_DebugRT+1)

#define Debug_F_Bloom_PreFilter (Debug_DebugRT)
#define Debug_F_Bloom_Down4 (Debug_F_Bloom_PreFilter+1)
#define Debug_F_Bloom_Down8 (Debug_F_Bloom_Down4+1)
#define Debug_F_Bloom_Down16 (Debug_F_Bloom_Down8+1)
#define Debug_F_Bloom_Down32 (Debug_F_Bloom_Down16+1)
#define Debug_F_Bloom_Down64 (Debug_F_Bloom_Down32+1)
#define Debug_F_Bloom_Up32 (Debug_F_Bloom_Down64+1)
#define Debug_F_Bloom_Up16 (Debug_F_Bloom_Up32+1)
#define Debug_F_Bloom_Up8 (Debug_F_Bloom_Up16+1)
#define Debug_F_Bloom_Up4 (Debug_F_Bloom_Up8+1)
#define Debug_F_Bloom_Output (Debug_F_Bloom_Up4+1)

#define Debug_F_GodRay_Threshold (Debug_F_Bloom_Output+1)

#define Debug_F_Dof_CoC (Debug_F_GodRay_Threshold+1)
#define Debug_F_Dof_PrefilterHalfCoC (Debug_F_Dof_CoC+1)
#define Debug_F_Dof_PrefilterHalfColor (Debug_F_Dof_PrefilterHalfCoC+1)
#define Debug_F_Dof_BlurH (Debug_F_Dof_PrefilterHalfColor+1)
#define Debug_F_Dof_BlurV (Debug_F_Dof_BlurH+1)
//DEBUG_END
TEX2D_SAMPLER(_DebugRT);

FLOAT _PPDebugMode;
uint _PPDebugDisplayType;
FLOAT2 _PPSplitAngle;
FLOAT _PPSplitPos;

FLOAT CalcDebugMask(int v, int m)
{
	return 1 - saturate(abs(v - m));
}

FLOAT4 DebugOutputColor(FLOAT4 OutColor, FLOAT2 uv, FCustomData CustomData)
{
	FLOAT4 debugColor = OutColor;
	int debugMode = _PPDebugMode;
	if (debugMode == 0)
	{
		return debugColor;
	}

	debugColor = FLOAT4(0, 0, 0, 0);

	FLOAT3 depth = SAMPLE_TEX2D(_CameraDepthRT, uv).xyz;
	FLOAT mask = CalcDebugMask(debugMode, Debug_F_Uber_Depth);
	debugColor = mask * FLOAT4(DecodeFloatRGB(depth.xyz).xxx*10, 1);

	FLOAT4 lut = SAMPLE_TEX2D(_Lut2D, uv);
	mask = CalcDebugMask(debugMode, Debug_F_Uber_Lut);
	debugColor += mask * lut;

	mask = CalcDebugMask(debugMode, Debug_F_Uber_BloomColor);
	debugColor += mask * FLOAT4(CustomData.bloom, 1);

	mask = CalcDebugMask(debugMode, Debug_F_Uber_BloomIntensity);
	debugColor += mask * FLOAT4(depth.zzz, 1);

	mask = CalcDebugMask(debugMode, Debug_F_Uber_GodRay);
	debugColor += mask * FLOAT4(CustomData.godray, 1);

	if (debugMode > 0)
	{
		float3 hsv = RgbToHsv(OutColor);
		mask = CalcDebugMask(debugMode, Debug_F_Uber_HSV);
		debugColor += mask * FLOAT4(hsv, 1);

		mask = CalcDebugMask(debugMode, Debug_F_Uber_H);
		debugColor += mask * FLOAT4(hsv.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_Uber_S);
		debugColor += mask * FLOAT4(hsv.yyy, 1);

		mask = CalcDebugMask(debugMode, Debug_F_Uber_V);
		debugColor += mask * FLOAT4(hsv.zzz, 1);
	}


	mask = CalcDebugMask(debugMode, Debug_F_RadialBlur_Area);
	debugColor += mask * (OutColor + FLOAT4(CustomData.radialBlurArea, 1));



	if (debugMode >= Debug_Custom_Start)
	{
		FLOAT4 debugRT = SAMPLE_TEX2D(_DebugRT, uv);

		mask = CalcDebugMask(debugMode, Debug_DebugRT);
		debugColor += mask * debugRT;

		mask = CalcDebugMask(debugMode, Debug_DebugRTA);
		debugColor.xyz += mask * debugRT.aaa;
	}

	if (_PPDebugDisplayType == 1)
	{
		FLOAT U = uv.x * 2 - 1;
		FLOAT V = uv.y * 2 - 1;
		FLOAT y = _PPSplitAngle.x * U + _PPSplitPos - V;
		y *= _PPSplitAngle.y;
		return  y < 0 ? debugColor : CustomData.src;
	}
	else
	{
		return  debugColor;
	}
}

#define DEBUG_PP_CUSTOMDATA DECLARE_OUTPUT(FCustomData, CustomData);
#define DEBUG_PP_CUSTOMDATA_PARAM(FieldName,FieldValue) CustomData.##FieldName = FieldValue;
#define DEBUG_PP_COLOR(OutColor, i) OutColor = DebugOutputColor(OutColor,i, CustomData);
#else
#define DEBUG_PP_CUSTOMDATA 
#define DEBUG_PP_CUSTOMDATA_PARAM(FieldName,FieldValue)
#define DEBUG_PP_COLOR(OutColor, i)
#endif//SHADER_API_MOBILE

#endif //POSTPROCESS_DEBUG_INCLUDE
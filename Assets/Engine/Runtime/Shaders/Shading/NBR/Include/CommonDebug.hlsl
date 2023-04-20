
#ifndef PBS_COMMONDEBUG_INCLUDE
#define PBS_COMMONDEBUG_INCLUDE

#ifdef _ENABLE_DEBUG
uint _DebugDisplayType;
FLOAT2 _SplitAngle;
FLOAT _SplitPos;
uint _DebugId;

inline FLOAT CalcDebugMask(int v,int m)
{
	return 1-saturate(abs(v-m));
}

inline FLOAT4 SplitScreen(FLOAT4 color,FLOAT4 debugColor,FLOAT2 SvPosition)
{
	UNITY_BRANCH
	if(_DebugDisplayType==1)
	{
		FLOAT U = SvPosition.x/_ScreenParams.x * 2 - 1;
		FLOAT V = SvPosition.y/_ScreenParams.y * 2 - 1;
		FLOAT y = _SplitAngle.x*U + _SplitPos -V;
		y *= _SplitAngle.y;
		debugColor = y<0?debugColor:color;
	}
	return debugColor;
}

#define DEBUG_CUSTOMDATA DECLARE_OUTPUT(FCustomData, CustomData);
#define DEBUG_CUSTOMDATA_PARAM(FieldName,FieldValue) CustomData.##FieldName = FieldValue;
#define DEBUG_ARGS ,inout FCustomData CustomData
#define DEBUG_PARAM ,CustomData
#else//!_ENABLE_DEBUG
#define DEBUG_CUSTOMDATA 
#define DEBUG_CUSTOMDATA_PARAM(FieldName,FieldValue)
#define DEBUG_ARGS
#define DEBUG_PARAM
#endif//_ENABLE_DEBUG

#endif //PBS_DEBUG_INCLUDE
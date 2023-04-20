#ifndef UNITY_DEPTH_OF_FIELD
#define UNITY_DEPTH_OF_FIELD

	FLOAT4 _DofParams;
	#define _EasyMode _DofParams.x
	#define _FocusDistance _DofParams.y
	#define _FocusRangeNear _DofParams.z
	#define _FocusRangeFar _DofParams.w

	#define _BokehRadius 5.0
	#define _FarScale 20
	#define _NearScale 1

	half CalcCoc(FLOAT2 uv)
	{
		FLOAT2 depthUV = uv * GET_TEX2D_SIZE(_MainTex).zw;
		FLOAT depth = DecodeFloatRGB(LOAD_TEX2D(_CameraDepthRT, depthUV).xyz);
        depth = LinearEyeDepth(depth);

		FLOAT delta = depth - _FocusDistance;
		FLOAT coc;

		UNITY_BRANCH
		if (_EasyMode > 0.5)
		{
			// 慢的算法
			//coc = delta > 0
			//	? delta / (_FocusDistance * (_FocusDistance + 1))
			//	: depth / _FocusDistance - 1;

			// 快的算法
			float scaler = delta > 0 ? _FarScale : _NearScale;
			coc = (depth - _FocusDistance) / (_FocusDistance * scaler);
		}
		else
		{
			coc = (depth - _FocusDistance) / (delta > 0 ? _FocusRangeFar : _FocusRangeNear);
		}

		coc = clamp(coc, -1, 1) * _BokehRadius;

		return coc;
	}

#endif

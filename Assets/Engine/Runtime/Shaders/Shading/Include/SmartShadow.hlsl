#ifndef SMART_SHADOW_INCLUDE
#define SMART_SHADOW_INCLUDE

#if defined(SMART_SHADOW_DEPTH_OUTPUT)

	struct depthAppdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct depthV2f
	{
		float4 vertex : SV_POSITION;
		float depth : TEXCOORD0;
		float2 uv : TEXCOORD1;
	};

	depthV2f object_vert(depthAppdata v)
	{
		depthV2f o;

		o.vertex = mul(unity_MatrixVP, mul(unity_ObjectToWorld, v.vertex));
		o.depth = o.vertex.z / o.vertex.w;
		o.uv = v.uv;
		return o;
	}

	//sampler2D _AlphaTestTex;
	half4 SmartEncodeFloatRGBA(float v)
	{
		return half4(v, v, v, 1);
	}

	#if defined(BASEMAP_DEFINED)
	TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
	#endif

	float _DepthOutputAlpha;

	half4 object_frag(depthV2f i) : SV_Target
	{
		half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
		UNITY_BRANCH
		if (color.a < _DepthOutputAlpha)
		{
			clip(-1);
		}
		return SmartEncodeFloatRGBA(i.depth);
	}

	half4 object_frag_no(depthV2f i) : SV_Target
	{
		clip(-1);
		return 1;
	}
#else


	#if defined(_SMARTSOFTSHADOW_ON)
	#define SHADOW_STEP_COUNT 16.0h
	#define SHADOW_TEPS float2(0.7959264f,0.3337449f), float2(-0.6917467f,0.4299984f), float2(-0.9272978f,-0.1487869f), float2(0.8660969f,-0.2434426f), float2(0.6313812f,0.01367807f), float2(-0.6743931f,0.172285f), float2(-0.2139604f,-0.6011167f), float2(0.5947536f,-0.1896544f), float2(0.3239655f,0.02670089f), float2(-0.4613352f,0.1297973f), float2(-0.07539916f,-0.4849357f), float2(0.3865351f,-0.01647747f), float2(0.003096877f,0.05768799f), float2(-0.1802002f,0.05737141f), float2(-0.001134997f,-0.06556179f), float2(5.539278E-05f,-0.001563253f)
	static const float2 ShadowConst[SHADOW_STEP_COUNT] = { SHADOW_TEPS };
	#endif


	TYPE4 _Parameter0;
	#define _SmartShadowIntensity (_Parameter0.y) 

	#if defined(_TEXARRAY_ON)
	float _Map[512];
	UNITY_DECLARE_TEX2DARRAY(_TexArr);
	#else
	//sampler2D _LightDepthTex;
	float4 _LightDepthTex_TexelSize;
	TEXTURE2D(_LightDepthTex);            SAMPLER(sampler_LightDepthTex);
	#endif

	float4 _LoopBlockSize;
	/*
	half4 SampleShadowTex(float2 uv)
	{
		//return tex2D(_LightDepthTex, uv);
		uv = float2(uv.x, 1 - uv.y);
		float2 count = uv * _Parameter0.xy;
		float2 indexFloor = floor(count);
		int index = indexFloor.y * _Parameter0.x + indexFloor.x;
		uv = frac(count);
		half4 r = UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(uv.x, 1 - uv.y, index));
		//return tex2D(_LightDepthTex, uv);
		return r;
	}
	*/

	//float GetSmartShadow(float3 lightDir, float3 normal, float4 worldPos)
	//{
	//	float4 lightClipPos = mul(_LightProjection, worldPos);
	//	lightClipPos.xyz = lightClipPos.xyz / lightClipPos.w;
	//	float2 uv = lightClipPos.xy * 0.5f + 0.5f;

	//	//half clip = step(0, uv.x) * step(0, uv.y) * step(uv.x, 1) * step(uv.y, 1);
	//	//if (clip == 0) return 1;

	//	fixed4 depthRGBA = tex2D(_LightDepthTex, uv);
	//	float depth = DecodeFloatRGBA(depthRGBA);
	//	return step(depth, lightClipPos.z + GetShadowBias(lightDir, normal, 0.001f));
	//}

	float4 SampleShadowTex(float2 uv)
	{
	#if defined(_TEXARRAY_ON)
		uv = float2(uv.x, 1 - uv.y);
		float2 count = uv * _Parameter0.xy;
		float2 indexFloor = floor(count);
		int index = indexFloor.y * _Parameter0.x + indexFloor.x;
		uv = frac(count);
		int newIndex = _Map[index];
		return newIndex < 0 ? 0 : UNITY_SAMPLE_TEX2DARRAY(_TexArr, float3(uv.x, 1 - uv.y, newIndex));
	#else

		//return SAMPLE_TEXTURE2D(_LightDepthTex, sampler_LightDepthTex, uv);
		return SAMPLE_TEXTURE2D_LOD(_LightDepthTex, sampler_LightDepthTex, uv, 0);//Change By:Takeshi, 角色顶点采图需要lod。
	#endif
	}

	float SmartDecodeFloatRGBA(half4 rgba)
	{
		//return dot(rgba, float4(1, 1 / 255.0f, 1 / 65025.0f, 1 / 16581375.0f));
		//return (rgba.r + rgba.g + rgba.b) / 3.0f;
		return dot(rgba.rgb, half3(1,1,1)) * 0.333333f;
	}


	float GetShadowBias(float3 lightDir, float3 normal, float baseBias = 0.005f)
	{
		return 0.005f;
		// return baseBias;
		//float maxBias = 0.005f;

		//func 1
		//float offsetMod = 1.0 - clamp(dot(normal, lightDir), 0, 1);
		//float offset = baseBias + maxBias * offsetMod;
		//return baseBias;

		//func 2
		//return max(0.05f * (1.0 - dot(normal, lightDir)), maxBias);

		//func 3
		//float cos_val = saturate(dot(lightDir, normal));
		//float sin_val = sqrt(1 - cos_val * cos_val); // sin(acos(L��N))
		//float tan_val = sin_val / cos_val;    // tan(acos(L��N))
		//float bias = baseBias + clamp(tan_val, 0, maxBias);
		//return bias;
	}

	float Tex2DCompare(float2 uv, float compare, float bias)
	{
		float depth = SmartDecodeFloatRGBA(SampleShadowTex(uv));
		return step(compare + bias, depth);
	}

    #define _SHADOWPRO_ON
	float SampleShadowTex(float2 uv, float compare, float bias, inout float count)
	{
	#if defined(_SHADOWPRO_ON)
		float2 size = _LightDepthTex_TexelSize.zw;
		float2 centroidUV = floor(uv * size) / size;
		float2 f = frac(uv * size);
		float lb = Tex2DCompare(centroidUV + _LightDepthTex_TexelSize.xy * float2(0.0f, 0.0f), compare, bias);
		float lt = Tex2DCompare(centroidUV + _LightDepthTex_TexelSize.xy * float2(0.0f, 1.0f), compare, bias);
		float rb = Tex2DCompare(centroidUV + _LightDepthTex_TexelSize.xy * float2(1.0f, 0.0f), compare, bias);
		float rt = Tex2DCompare(centroidUV + _LightDepthTex_TexelSize.xy * float2(1.0f, 1.0f), compare, bias);

		float a = lerp(lb, lt, f.y);
		float b = lerp(rb, rt, f.y);
		float c = lerp(a, b, f.x);
		count += step(0.1f, c);
		return c;

		//float2 size = _LightDepthTex_TexelSize.zw;
		//float2 f = frac(uv * size);
		//float lb = Tex2DCompare(uv + _LightDepthTex_TexelSize.xy * float2(-1.0f, -1.0f), compare, bias);
		//float lt = Tex2DCompare(uv + _LightDepthTex_TexelSize.xy * float2(-1.0f, 1.0f), compare, bias);
		//float rb = Tex2DCompare(uv + _LightDepthTex_TexelSize.xy * float2(1.0f, 1.0f), compare, bias);
		//float rt = Tex2DCompare(uv + _LightDepthTex_TexelSize.xy * float2(1.0f, -1.0f), compare, bias);
		//count += lb + lt + rb + rt;
		//float a = lerp(lb, lt, f.y);
		//float b = lerp(rb, rt, f.y);
		//float c = lerp(a, b, f.x);
		//return c;
		#else
		count += 0;
		return Tex2DCompare(uv, compare, bias);
		#endif
	}

	float SampleShadow(float2 uv, float compare, float bias)
	{
		return Tex2DCompare(uv, compare, bias);
	}

	float SampleSmartShadowCompare(float z, float2 uv)
	{
		return step(SAMPLE_TEXTURE2D_LOD(_LightDepthTex, sampler_LightDepthTex, uv, 0).x, z);
	}

	// 相比GetSmartShadow，没有做PCF计算，直接采样9个点。
	float GetSmartShadow9Point(float4 positionWS, float radius, float intensity)
	{
		#if defined(_SMARTSOFTSHADOW_ON)
			float4 positionLS = mul(_LightProjection, positionWS);
			half2 uv = positionLS.xy * 0.5f + 0.5;
			if (any(step(0.5, abs(uv - 0.5)))) return 1;
			
			half4 uvOffsets;
			uvOffsets.xy = mul((float3x3)_LightProjection, half3(1, 0, 0)).xy;
			uvOffsets.zw = mul((float3x3)_LightProjection, half3(0, 0, 1)).xy;
			uvOffsets *= radius;

			float bias = _Parameter0.w;

			float shadowSample = (Tex2DCompare(uv                                        , positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * -1 + uvOffsets.zw * -1, positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * +1 + uvOffsets.zw * -1, positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * -1 + uvOffsets.zw * +1, positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * +1 + uvOffsets.zw * +1, positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * +1                    , positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.xy * -1                    , positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.zw * +1                    , positionLS.z, bias)
								+ Tex2DCompare(uv + uvOffsets.zw * -1                    , positionLS.z, bias)
							    ) / 9.0;
			return 1 - shadowSample * intensity;
		#else
			return 1;
		#endif
	}

	float GetSmartShadowWithoutFilter(float4 positionWS)
	{
		#if defined(_SMARTSOFTSHADOW_ON)
			float4 positionLS = mul(_LightProjection, positionWS);
			if (any(step(0.5, abs(positionLS.xyxy * 0.5)))) return 1;
			half2 uv = positionLS.xy * 0.5f + 0.5;
			half transformedZ = 1 - positionLS.z - _Parameter0.w;
			return SampleSmartShadowCompare(transformedZ, uv);
		#else
			return 1;
		#endif
	}

	float GetSmartShadow(float3 lightDir, float3 normal, float4 worldPos, float intensity)
	{
	#if defined(_SMARTSOFTSHADOW_ON)
		worldPos.xyz = _LoopBlockSize.w ? float3(worldPos.x % _LoopBlockSize.x, worldPos.y % _LoopBlockSize.y, worldPos.z % _LoopBlockSize.z) : worldPos.xyz;
		float bias = GetShadowBias(lightDir, normal, _Parameter0.w);
		float4 lightClipPos = mul(_LightProjection, worldPos);
		float2 uv = lightClipPos.xy * 0.5f + 0.5f;

		UNITY_BRANCH
		if (any(step(0.5, abs(uv.xyxy - 0.5))) || lightClipPos.z < 0) return 1;

		float2 offset = _LightDepthTex_TexelSize.xy * _Parameter0.z;
		
		#if defined(_SMARTSOFTSHADOW_ON)
		    float count = 0;
		/*
			float shadow = SampleShadowTex(uv + offset * ShadowConst[0], lightClipPos.z, bias, count);
			shadow += SampleShadowTex(uv + offset * ShadowConst[1], lightClipPos.z, bias, count);
			shadow += SampleShadowTex(uv + offset * ShadowConst[2], lightClipPos.z, bias, count);
			shadow += SampleShadowTex(uv + offset * ShadowConst[3], lightClipPos.z, bias, count);

			UNITY_BRANCH
			if (count == 0 || count == 4)
				return 1 - (shadow / 4.0f) * _SmartShadowIntensity;
			else
			UNITY_BRANCH
			{
				shadow += SampleShadowTex(uv + offset * ShadowConst[4], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[5], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[6], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[7], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[8], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[9], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[10], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[11], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[12], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[13], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[14], lightClipPos.z, bias, count);
				shadow += SampleShadowTex(uv + offset * ShadowConst[15], lightClipPos.z, bias, count);
				return 1 - (shadow / 16.0f) * _SmartShadowIntensity;
			}
			*/

			
		#if defined(ENABLE_CLOUD)
			return (1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity)) * CloudShadowColor(worldPos);
		#else
			return (1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity));
		#endif
		//

			//float shadow = SampleShadowTex(uv + offset * ShadowConst[0], lightClipPos.z, bias, count);
			//shadow += SampleShadowTex(uv + offset * ShadowConst[1], lightClipPos.z, bias, count);
			//shadow += SampleShadowTex(uv + offset * ShadowConst[2], lightClipPos.z, bias, count);
			//shadow += SampleShadowTex(uv + offset * ShadowConst[3], lightClipPos.z, bias, count);
			//return 1 - (shadow / 4.0f) * _SmartShadowIntensity;
		#else
			return 1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity);
		#endif
		
	#else
		return 1;
	#endif
	}
	float GetSmartShadow(float3 lightDir, float3 normal, float4 worldPos, float intensity,half noise,half noiseInt)
	{
	#if defined(_SMARTSOFTSHADOW_ON)
		worldPos.xyz = _LoopBlockSize.w ? float3(worldPos.x % _LoopBlockSize.x, worldPos.y % _LoopBlockSize.y, worldPos.z % _LoopBlockSize.z) : worldPos.xyz;
		float bias = GetShadowBias(lightDir, normal, _Parameter0.w);
		float4 lightClipPos = mul(_LightProjection, worldPos);
		float2 uv = lightClipPos.xy * 0.5f + 0.5f;
		uv += noise * noiseInt;

		UNITY_BRANCH
		if (any(step(0.5, abs(uv.xyxy - 0.5))) || lightClipPos.z < 0) return 1;

		float2 offset = _LightDepthTex_TexelSize.xy * _Parameter0.z;
		
		#if defined(_SMARTSOFTSHADOW_ON)
		    float count = 0;

		#if defined(ENABLE_CLOUD)
			return (1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity)) * CloudShadowColor(worldPos);
		#else
			return (1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity));
		#endif
		#else
			return 1 - (SampleShadow(uv, lightClipPos.z, bias) * intensity);
		#endif
		
	#else
		return 1;
	#endif
	}

#endif

#endif
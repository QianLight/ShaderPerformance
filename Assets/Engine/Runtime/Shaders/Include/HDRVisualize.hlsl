#ifndef PBS_DEBUG_INCLUDE
#define PBS_DEBUG_INCLUDE

float4 _HDRVisualizeColors[8];
float _HDRVisualizeGraph[256];
float _HDRVisualizeAlpha;

inline int VisualizeTemplateIndex(float2 screenPos)
{
	if (screenPos.x < 128 && screenPos.y > _ScreenParams.y - 16)
	{
		return (int)screenPos.x / 16;
	}
	else
	{
		return -1;
	}
}

inline float3 GetVisualizeColor(float e, int templateIndex)
{
	if (e < 0)
	{
		return float3(0, 0, 0);
	}

	if (e > 8)
	{
		return float3(0, 1, 1);
	}

	if (templateIndex >= 0)
	{
		return _HDRVisualizeColors[templateIndex].rgb;
	}

	return _HDRVisualizeColors[e].rgb;
}

inline float GetVisualizeMask(int index, int templateIndex)
{
	if (templateIndex >= 0)
	{
		return _HDRVisualizeGraph[index];
	}
	return _HDRVisualizeGraph[index];
}

inline float GetVisualizeAlpha(int templateIndex)
{
	return templateIndex >= 0 ? 1 : _HDRVisualizeAlpha;
}

inline float3 VisualizeFloat(float value, float2 screenPos)
{
	int templateIndex = VisualizeTemplateIndex(screenPos);
	float e = log2(value);
	int el = floor(e);
	int er = el + 1;
	int index = ((int)screenPos.x) % 16 + (15 - ((int)screenPos.y) % 16) * 16;
	int graphMask = GetVisualizeMask(index, templateIndex);
	float ml = graphMask & (1 << (templateIndex >= 0 ? templateIndex : el));
	float mr = graphMask & (1 << (templateIndex >= 0 ? templateIndex : er));
	float alpha = GetVisualizeAlpha(templateIndex);
	float3 cl = GetVisualizeColor(el, templateIndex) * lerp(1, ml * 0.5 + 0.5, alpha);
	float3 cr = GetVisualizeColor(er, templateIndex) * lerp(1, mr * 0.5 + 0.5, alpha);
	float3 c = ((int)screenPos.x) % 16 / 16.0 >= frac(e) ? cl : cr;
	return c;
}

inline float3 VisualizeFloatWithVectorMask(float4 color, float4 mask, float2 screenPos)
{
	float value = dot(color, mask);
	return VisualizeFloat(value, screenPos);
}

// 传入UnityEngine.Rendering.ColorWriteMask，在Shader内解析。
inline float3 VisualizeFloatWithFloatMask(float4 color, float mask, float2 screenPos)
{
	int m = (int)mask;
	float4 vectorMask = float4(
		((m & (1 << 3)) > 0),
		((m & (1 << 2)) > 0),
		((m & (1 << 1)) > 0),
		((m & (1 << 0)) > 0)
		);
	return VisualizeFloatWithVectorMask(color, vectorMask, screenPos);
}

#endif // PBS_DEBUG_INCLUDE
#ifndef UI_COMMON_INCLUDE
#define UI_COMMON_INCLUDE

#include "../Include/Common.hlsl"
#include "../Colors.hlsl"

TEX2D_SAMPLER(_MainTex);
FLOAT4 _ClipRect;
FLOAT4 _TextureSampleAdd;
FLOAT _FADEIN_X = 0.0f;
FLOAT _FADEIN_Y = 0.0f;

CBUFFER_START(UnityPerMaterial)
FLOAT4 _Color;
CBUFFER_END

// #ifdef _ADDITIVE02
//  FLOAT _ColorInt;
// #endif


inline FLOAT UIClip(in FLOAT2 position, in FLOAT4 clipRect)
{
	/*FLOAT2 inside = step(clipRect.xy, position.xy) * step(position.xy, clipRect.zw);
	return inside.x * inside.y;*/
	FLOAT halfWidth = (clipRect.w - clipRect.y) * 0.5;
	FLOAT mid = (clipRect.w + clipRect.y) * 0.5;
	FLOAT yy = saturate((halfWidth - abs(mid - position.y)) / (_FADEIN_Y + 0.1f));

	halfWidth = (clipRect.z - clipRect.x) * 0.5;
	mid = (clipRect.z + clipRect.x) * 0.5;
	FLOAT xx = saturate((halfWidth - abs(mid - position.x))/ (_FADEIN_X + 0.1f));
	return (length(clipRect.xy-clipRect.zw) < 1e-10 || isinf(clipRect.x) || isinf(clipRect.y) || isinf(clipRect.z) || isinf(clipRect.w) ) ? 1.0 : yy * xx + step(20000, halfWidth);
}


// inline FLOAT4 Linear2Gamma(in FLOAT4 color)
// {
// 	color = max(color, FLOAT4(0.h, 0.h, 0.h, 0.h));
// 	return max(1.055h * pow(color, 0.416666667h) - 0.055h, 0.h);
// }
//
// inline FLOAT4 Gamma2Linear(in FLOAT4 color)
// {
// 	return color * (color * (color * 0.305306011h + 0.682171111h) + 0.012522878h);
// }
// 
//#define invlerp(a,b,x) (((x) - (a)) / ((b) - (a)))

#ifndef COLOR_TO_GREY_SWITCH
#define COLOR_TO_GREY_SWITCH(color,vertexColor) ColorToGreySwitch(color,vertexColor)
inline void ColorToGreySwitch(inout FLOAT4 color,FLOAT4 vertexColor)
{
	//使用vertexColor.b作为置灰标记，置灰时vertexColor.g已包含压暗信息
	FLOAT grey = Luminance(color.rgb);
	FLOAT b = (vertexColor.b * 255 - 1) / 254;
	color.rgb = vertexColor.b == 0 ? (grey * vertexColor.g) : (color.rgb * FLOAT3(vertexColor.r, vertexColor.g, b));


	// grey = vertexColor.a > 0.999 ? grey * 0.5 : grey;
	// // grey *= 0.6;
	// FLOAT isToGrey = 1 - sign(vertexColor.r + vertexColor.g + vertexColor.b);
	//
	// /* Add By:		Takeshi
	//  * Date:		2021/10/15
	//  * Apply:		修复顶点色B通道颜色不能达到纯黑的BUG
	//  * Dependent:   配合 张欣颖 2021年9月29日 在src\client\CFClient\CFEngine\UI\CFUI\Core\Graphic.cs中的修改 */
	// vertexColor.b = vertexColor.b < 6.0 /255 && vertexColor.r == 0 && vertexColor.g == 0? 0 : vertexColor.b;
	// /* End Add */
	// color.rgb *= vertexColor.rgb;
	// color = lerp(color, FLOAT4(grey,grey,grey,color.a), isToGrey);
}
#endif

#endif //UI_COMMON_INCLUDE
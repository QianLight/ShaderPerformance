#ifndef UI_PIXEL_INCLUDE
#define UI_PIXEL_INCLUDE 

/* 在引用该头文件时，默认引用依赖，可以使用这个宏，断绝该文件的依赖 */
#ifndef UI_PIXEL_DEPEND_OFF
#include "UICommon.hlsl" 
//#include "Debug.hlsl" 
#endif

half _IsInUICamera;
/* Add By:	Takeshi
 * Date:	2021/10/14
 * Apply:	调整了 UI Shader 框架，增加了可覆盖的 Color 声明方法
 *			如果 Feature 要编辑 color，重新定义 INITIALIZE_COLOR 覆盖此方法即可
 */
#ifndef INITIALIZE_COLOR
#define INITIALIZE_COLOR(Interpolants,color) InitializeColor(Interpolants,color)
inline void InitializeColor(UIInterpolantsVSToPS Interpolants, out FLOAT4 color)
{
	color = SAMPLE_TEX2D(_MainTex, Interpolants.uv0) + _TextureSampleAdd;
}
#endif
/* End Add */

FLOAT4 uiFrag(UIInterpolantsVSToPS Interpolants/* ,in FLOAT4 SvPosition : SV_Position*/) : SV_Target
{
	/* By:	Takeshi
	 * Date:		2021/10/13;
	 * Apply:		UI Gamma 矫正后这个变体不需要了,color 直接乘 2，肯定是不对的,估计是以前的折中做法
	 *				如果shader"UI_Scene""ImageSequenceAdditive"受影响，再做调整	
	 *				调整了 UI Shader 框架，
	 *				把 Feature 移出公用的的头文件,今后 Feature 不要写在框架公共头文件里
	 
		//FLOAT4 color = (SAMPLE_TEX2D(_MainTex, Interpolants.uv0)+ _TextureSampleAdd);
		//#ifdef _ADDITIVE
		//	color.rgb *= 2.0f; 
		//#endif	
		// #ifdef _ADDITIVE02
		// color.rgb *= _ColorInt;
		// float circle=1-saturate (length ( Interpolants.uv0.xy*2-1));
		// 	  circle*=circle;
		// color.a *= Interpolants.color.a*circle;
		// #endif
	 * End */

	/* By:Takeshi; UI Base Color 初始化，可重写 */
    FLOAT4 color;
	INITIALIZE_COLOR(Interpolants, color);
	color.a *= Interpolants.color.a;
	
	#ifdef UNITY_UI_CLIP_RECT
		color.a *= UIClip(Interpolants.worldPosition.xy, _ClipRect);
	#endif

	/* UI 功能: 彩色和黑白转换，默认开启此功能，如需关闭，则必须在 INITIALIZE_COLOR 或其他地方重新实现 */
	#ifndef UI_PIXEL_COLOR_TO_GREY_SWITCH_OFF
	COLOR_TO_GREY_SWITCH(color, Interpolants.color);
	#endif

	#ifdef UNITY_UI_ALPHACLIP
	    clip (color.a - 0.01);
	#endif

	/* By:Takeshi; 在Gamme校色后的管线里，UI 使用sRGB工作流 */
	#ifndef GAMMA_FIX_OFF
	color.rgb = lerp(color.rgb,LinearToSRGB(color.rgb),_IsInUICamera);
	#endif

	/* By:Takeshi; 如果需要Alpha透明度混合，设置Blend为 One OneMinusSrcAlpha */
	color.rgb *= color.a;
	
	return color;
}
#endif
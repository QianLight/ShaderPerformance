#ifndef URP_SHADER_LEVEL_HLSL
#define URP_SHADER_LEVEL_HLSL

//����
#if defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_VERY_HIGH)
	#define _NORMALMAP
	#ifdef ROLE_SHADER_LEVEL
		#define FOG_SCATTER_OFF
		#ifndef _ROLE_SOFT_SHADOW
			#define _ROLE_SOFT_SHADOW
		#endif
    #endif

//����
#elif defined(_SHADER_LEVEL_MEDIUM)
	#define _NORMALMAP
	#define FOG_SCATTER_OFF

	// Role
	#ifdef ROLE_SHADER_LEVEL
		#undef _ROLE_HEIGHT_GRADIENT
	#endif

//����
#else
	#define FOG_SCATTER_OFF
	#define _ENVIRONMENTREFLECTIONS_OFF

	#ifdef ROLE_SHADER_LEVEL
		#ifndef SHADOW_RECEIVE_OFF
			#define SHADOW_RECEIVE_OFF
		#endif
		#ifdef _ROLE_HEIGHT_GRADIENT
			#undef _ROLE_HEIGHT_GRADIENT
		#endif
		#ifdef _SM_DARK_RIM
			#undef _SM_DARK_RIM
		#endif
		#ifdef _SM_RIM
			#undef _SM_RIM
		#endif
		#ifndef _ROLE_SHADOW_RECEIVE_OFF
			#define _ROLE_SHADOW_RECEIVE_OFF
		#endif
		#ifndef _PBS_NO_IBL
			#define _PBS_NO_IBL
		#endif
		#ifdef _SMARTSOFTSHADOW_ON
			#undef _SMARTSOFTSHADOW_ON
		#endif
		#ifdef _MAIN_LIGHT_SHADOWS
			#undef _MAIN_LIGHT_SHADOWS
		#endif
		#ifndef _DISABLE_RAMP_TEXTURE
			#define _DISABLE_RAMP_TEXTURE
		#endif
		#ifndef _ROLE_SHADOW_OFF
			#define _ROLE_SHADOW_OFF
		#endif
	#else
		#define _NORMALMAP
	#endif

#endif

#endif

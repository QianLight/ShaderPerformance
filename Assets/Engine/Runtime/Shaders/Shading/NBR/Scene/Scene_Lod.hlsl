
#ifndef SCENE_LOD_INCLUDE
#define SCENE_LOD_INCLUDE


//High
#if defined(_LOD0)
	#define _ADD_LIGHT
	#define _SHADOW_MAP
	#define _CSM3
#elif defined(_LOD1)
	#define _SHADOW_MAP
	#define _CSM3
#elif defined(_LOD2)
	#define _SHADOW_MAP
	#define _CSM3
#elif defined(_LOD_FAR)
	#define _SHADOW_MAP
	#define _SIMPLE_SHADOW
#endif

//Mid
#if defined(_LOD_M0)
	#define _ADD_LIGHT
	#define _SIMPLE_ADD_LIGHT
	#define _SHADOW_MAP
	#define _CSM3
#elif defined(_LOD_M1)
	#define _SHADOW_MAP
	#define _CSM3
#elif defined(_LOD_M2)
	#define _SHADOW_MAP
	#define _SIMPLE_SHADOW
	#define _PBS_NO_IBL
#elif defined(_LOD_MFAR)
	#define _SHADOW_MAP
	#define _SIMPLE_SHADOW
	#define _PBS_NO_IBL
#endif

//Low
#if defined(_LOD_L0)
	#define _SHADOW_MAP
	#define _CSM3
	#define _PBS_NO_IBL
	#define _VERTEX_FOG
#elif defined(_LOD_L1)
	#define _SHADOW_MAP
	#define _CSM3
	#define _PBS_NO_IBL
	#define _VERTEX_FOG
#elif defined(_LOD_L2)
	#define _PBS_NO_IBL
	#define _VERTEX_FOG
#elif defined(_LOD_LFAR)
	#define _PBS_NO_IBL
	#define _VERTEX_FOG
#endif

#ifndef _CUSTOM_LOD
	//High
	#if defined(_LOD0)
		#define _EXTRA_SHADOW
	#elif defined(_LOD1)
		#define _ADD_LIGHT
	#endif

	//Mid
	#if defined(_LOD_M0)
		#define _EXTRA_SHADOW
		#define _SIMPLE_EXTRA_SHADOW
	#endif

	//Low
	#if defined(_LOD_L0)
		#define _EXTRA_SHADOW
		#define _SIMPLE_EXTRA_SHADOW
	#endif

#else//_CUSTOM_LOD
////////////////////////////////Tree////////////////////////////////
	

	#ifdef _CLOUD03
		//High
		#if defined(_LOD0)
			#define _SOFT_PARTICLE
		#endif

		//Mid
		#if defined(_LOD_M0)
		#endif

		//Low
		#if defined(_LOD_L0)
		#endif
	#endif//_CLOUD03

	#ifdef _SIMPLE_WIND
		//High
		#if defined(_LOD0)
			#define _ADD_LIGHT
			#define _SHADOW_MAP
			#define _CSM3
			#define _EXTRA_SHADOW
			#define _CUSTOM_VERTEX_OFFSET
		#elif defined(_LOD1)
			#define _ADD_LIGHT
			#define _SHADOW_MAP
			#define _CSM3
			#define _CUSTOM_VERTEX_OFFSET
		#elif defined(_LOD2)
			#define _SHADOW_MAP
			#define _CSM3
		#elif defined(_LOD_FAR)
			#define _SHADOW_MAP
			#define _SIMPLE_SHADOW
		#endif

		//Mid
		#if defined(_LOD_M0)
			#define _ADD_LIGHT
			#define _SIMPLE_ADD_LIGHT
			#define _SHADOW_MAP
			#define _CSM3
			#define _EXTRA_SHADOW
			#define _SIMPLE_EXTRA_SHADOW
			#define _CUSTOM_VERTEX_OFFSET
		#elif defined(_LOD_M1)
			#define _SHADOW_MAP
			#define _CSM3
		#elif defined(_LOD_M2)
			#define _SHADOW_MAP
			#define _SIMPLE_SHADOW
			#define _PBS_NO_IBL
		#elif defined(_LOD_MFAR)
			#define _SHADOW_MAP
			#define _SIMPLE_SHADOW
			#define _PBS_NO_IBL
		#endif

		//Low
		#if defined(_LOD_L0)
			#define _SHADOW_MAP
			#define _CSM3
			#define _EXTRA_SHADOW
			#define _SIMPLE_EXTRA_SHADOW
			#define _PBS_NO_IBL
			#define _VERTEX_FOG
		#elif defined(_LOD_L1)
			#define _SHADOW_MAP
			#define _CSM3
			#define _PBS_NO_IBL
		#define _VERTEX_FOG
			#elif defined(_LOD_L2)
			#define _PBS_NO_IBL
			#define _VERTEX_FOG
		#elif defined(_LOD_LFAR)
			#define _PBS_NO_IBL
			#define _VERTEX_FOG
		#endif
	#endif//_SIMPLE_WIND

#endif//_CUSTOM_LOD
#endif//SCENE_LOD_INCLUDE
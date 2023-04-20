
#if defined(_DEBUG_APP)&&!defined(SHADER_API_MOBILE)
#define _ENABLE_DEBUG
#endif

#if defined(SHADER_API_D3D11)||defined(SHADER_API_D3D12)||defined(SHADER_API_VULKAN)||defined(SHADER_API_METAL)
#define _SUPPORT_TEXARR_CMP
#endif


#if defined(_ADD_LIGHT)||defined(_SUPPORT_TEXARR_CMP)||defined(_ENABLE_DEBUG)
#define _SM_4
#endif


//Macro
//_NO_COLOR_EFFECT
//_NO_LIGHTMAP
//_PBS_NO_IBL
//_NO_NORMAL_MAP
//_NO_METALLIC
//_NO_AO
//_NO_EMISSIVE
//_NO_ADDLIGHT
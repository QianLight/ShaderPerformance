#ifndef PBS_DEBUG_INCLUDE
#define PBS_DEBUG_INCLUDE

#ifdef _ENABLE_DEBUG
//DEBUG_START
#define Debug_None 0
//vertex
#define DebugVertexDataEnd (Debug_None)
//pixel
#define Debug_F_Vertex_uv (DebugVertexDataEnd+1)
#define Debug_F_Vertex_uv2 (Debug_F_Vertex_uv+1)
#define Debug_F_Vertex_uv3 (Debug_F_Vertex_uv2+1)
#define Debug_F_Vertex_VertexNormal (Debug_F_Vertex_uv3+1)
#define Debug_F_Vertex_VertexTangent (Debug_F_Vertex_VertexNormal+1)
#define Debug_F_Vertex_VertexColor (Debug_F_Vertex_VertexTangent+1)
#define Debug_F_Vertex_VertexColorA (Debug_F_Vertex_VertexColor+1)
#define EndDebugVertex (Debug_F_Vertex_VertexColorA)
//vs 2 ps
#define Debug_F_Frag_Depth01 (EndDebugVertex+1)
#define Debug_F_Frag_Depth01Encode (Debug_F_Frag_Depth01+1)
#define Debug_F_Frag_LightmapUV (Debug_F_Frag_Depth01Encode+1)
#define Debug_F_Frag_SvPosition (Debug_F_Frag_LightmapUV+1)
#define Debug_F_Frag_ScreenPosition (Debug_F_Frag_SvPosition+1)
#define Debug_F_Frag_WorldPosition (Debug_F_Frag_ScreenPosition+1)
#define Debug_F_Frag_CamRelative (Debug_F_Frag_WorldPosition+1)
#define Debug_F_Frag_CameraVector (Debug_F_Frag_CamRelative+1)
#define Debug_F_Frag_Ambient (Debug_F_Frag_CameraVector+1)
#define Debug_F_Frag_CustomData (Debug_F_Frag_Ambient+1)
#define Debug_F_Frag_CustomData1 (Debug_F_Frag_CustomData+1)
#define EndDebugFrag (Debug_F_Frag_CustomData1)
//material
#define Debug_F_Material_BaseColor (EndDebugFrag+1)
#define Debug_F_Material_BaseColorAlpha (Debug_F_Material_BaseColor+1)
#define Debug_F_Material_BlendTex (Debug_F_Material_BaseColorAlpha+1)
#define Debug_F_Material_Metallic (Debug_F_Material_BlendTex+1)
#define Debug_F_Material_Roughness (Debug_F_Material_Metallic+1)
#define Debug_F_Material_PerceptualRoughness (Debug_F_Material_Roughness+1)
#define Debug_F_Material_WorldNormal (Debug_F_Material_PerceptualRoughness+1)
#define Debug_F_Material_TangentSpaceNormal (Debug_F_Material_WorldNormal+1)
#define Debug_F_Material_WorldTangent (Debug_F_Material_TangentSpaceNormal+1)
#define Debug_F_Material_WorldBinormal (Debug_F_Material_WorldTangent+1)
#define Debug_F_Material_NdotV (Debug_F_Material_WorldBinormal+1)
#define Debug_F_Material_DiffuseColor (Debug_F_Material_NdotV+1)
#define Debug_F_Material_SpecularColor (Debug_F_Material_DiffuseColor+1)
#define Debug_F_Material_Emissive (Debug_F_Material_SpecularColor+1)
#define Debug_F_Material_SrcAO (Debug_F_Material_Emissive+1)
#define Debug_F_Material_AO (Debug_F_Material_SrcAO+1)
#define Debug_F_Material_CustomParam (Debug_F_Material_AO+1)
#define EndDebugMaterial (Debug_F_Material_CustomParam)
//lighting
#define Debug_F_GI_Lightmap (EndDebugMaterial+1)
#define Debug_F_GI_DiffuseGI (Debug_F_GI_Lightmap+1)
#define Debug_F_GI_AddGI (Debug_F_GI_DiffuseGI+1)
#define Debug_F_GI_AddShadow (Debug_F_GI_AddGI+1)
#define Debug_F_GI_SH (Debug_F_GI_AddShadow+1)
#define Debug_F_GI_Ambient (Debug_F_GI_SH+1)
#define EndDebugGI (Debug_F_GI_Ambient)

#define Debug_F_Lighting_DirectDiffuse (EndDebugGI+1)
#define Debug_F_Lighting_DirectSpecular (Debug_F_Lighting_DirectDiffuse+1)
#define Debug_F_Lighting_Specular (Debug_F_Lighting_DirectSpecular+1)
#define Debug_F_Lighting_DiffuseSupplement (Debug_F_Lighting_Specular+1)
#define Debug_F_Lighting_SpecularSupplement (Debug_F_Lighting_DiffuseSupplement+1)
#define Debug_F_Lighting_SupplementMask (Debug_F_Lighting_SpecularSupplement+1)
#define Debug_F_Lighting_CombineColor (Debug_F_Lighting_SupplementMask+1)
#define Debug_F_Lighting_Output (Debug_F_Lighting_CombineColor+1)
#define Debug_F_Lighting_PostCustomLighting (Debug_F_Lighting_Output+1)
#define EndDebugLighting (Debug_F_Lighting_PostCustomLighting)

#define Debug_F_LC0_NdotL (EndDebugLighting+1)
#define Debug_F_LC0_FixNdotL (Debug_F_LC0_NdotL+1)
#define Debug_F_LC0_H (Debug_F_LC0_FixNdotL+1)
#define Debug_F_LC0_NdotH (Debug_F_LC0_H+1)
#define Debug_F_LC0_LdotH (Debug_F_LC0_NdotH+1)

#define Debug_F_LC1_NdotL (Debug_F_LC0_LdotH+1)
#define Debug_F_LC1_FixNdotL (Debug_F_LC1_NdotL+1)
#define Debug_F_LC1_H (Debug_F_LC1_FixNdotL+1)
#define Debug_F_LC1_NdotH (Debug_F_LC1_H+1)
#define Debug_F_LC1_LdotH (Debug_F_LC1_NdotH+1)
#define EndDebugLC (Debug_F_LC1_LdotH)

#define Debug_F_Shadow_ShadowMapIndex (EndDebugLC+1)
#define Debug_F_Shadow_ShadowMapIndex0 (Debug_F_Shadow_ShadowMapIndex+1)
#define Debug_F_Shadow_ShadowMapIndex1 (Debug_F_Shadow_ShadowMapIndex0+1)
#define Debug_F_Shadow_ShadowMapIndex2 (Debug_F_Shadow_ShadowMapIndex1+1)
#define Debug_F_Shadow_ShadowCoord0 (Debug_F_Shadow_ShadowMapIndex2+1)
#define Debug_F_Shadow_ShadowCoord1 (Debug_F_Shadow_ShadowCoord0+1)
#define Debug_F_Shadow_ShadowCoord2 (Debug_F_Shadow_ShadowCoord1+1)
#define Debug_F_Shadow_ShadowCoordExtra (Debug_F_Shadow_ShadowCoord2+1)
#define Debug_F_Shadow_SceneShadow (Debug_F_Shadow_ShadowCoordExtra+1)
#define Debug_F_Shadow_SimpleShadow (Debug_F_Shadow_SceneShadow+1)
#define Debug_F_Shadow_ExtraShadow (Debug_F_Shadow_SimpleShadow+1)
#define Debug_F_Shadow_ExtraShadow1 (Debug_F_Shadow_ExtraShadow+1)
#define Debug_F_Shadow_SelfShadow (Debug_F_Shadow_ExtraShadow1+1)
#define Debug_F_Shadow_SrcShadow (Debug_F_Shadow_SelfShadow+1)
#define Debug_F_Shadow_FadeShadow (Debug_F_Shadow_SrcShadow+1)
#define Debug_F_Shadow_DynamicShadow (Debug_F_Shadow_FadeShadow+1)
#define Debug_F_Shadow_ShadowColor (Debug_F_Shadow_DynamicShadow+1)
#define Debug_F_Shadow_LightmapShadow (Debug_F_Shadow_ShadowColor+1)
#define EndDebugShadow (Debug_F_Shadow_LightmapShadow)

#define Debug_F_IBL_CubeMipmap (EndDebugShadow+1)
#define Debug_F_IBL_CubeMipmapColor (Debug_F_IBL_CubeMipmap+1)
#define Debug_F_IBL_ReflectionVector (Debug_F_IBL_CubeMipmapColor+1)
#define Debug_F_IBL_IndirectSpecular (Debug_F_IBL_ReflectionVector+1)
#define Debug_F_IBL_IBLFresnel (Debug_F_IBL_IndirectSpecular+1)
#define Debug_F_IBL_IBLScale (Debug_F_IBL_IBLFresnel+1)
#define Debug_F_IBL_IBL (Debug_F_IBL_IBLScale+1)
#define EndDebugIBL (Debug_F_IBL_IBL)

#define Debug_F_AddLight_Diffuse (EndDebugIBL+1)
#define Debug_F_AddLight_Specular (Debug_F_AddLight_Diffuse+1)
#define Debug_F_AddLight_AddIndex (Debug_F_AddLight_Specular+1)
#define Debug_F_AddLight_DynamicDiffuse (Debug_F_AddLight_AddIndex+1)
#define Debug_F_AddLight_DynamicSpecular (Debug_F_AddLight_DynamicDiffuse+1)
#define Debug_F_AddLight_AddColor (Debug_F_AddLight_DynamicSpecular+1)
#define Debug_F_AddLight_Softness (Debug_F_AddLight_AddColor+1)
#define Debug_F_AddLight_ToonAO (Debug_F_AddLight_Softness+1)
#define Debug_F_AddLight_Ramp (Debug_F_AddLight_ToonAO+1)
#define Debug_F_AddLight_Atten (Debug_F_AddLight_Ramp+1)
#define Debug_F_AddLight_LightColor (Debug_F_AddLight_Atten+1)
#define Debug_F_AddLight_Result (Debug_F_AddLight_LightColor+1)
#define Debug_F_AddLight_Ambient (Debug_F_AddLight_Result+1)
#define EndDebugAddLight (Debug_F_AddLight_Ambient)

#define Debug_F_Water_Subsurface (EndDebugAddLight+1)
#define Debug_F_Water_SceneZRefract (Debug_F_Water_Subsurface+1)
#define Debug_F_Water_Alpha (Debug_F_Water_SceneZRefract+1)
#define Debug_F_Water_SkyCol (Debug_F_Water_Alpha+1)
#define Debug_F_Water_Foam (Debug_F_Water_SkyCol+1)
#define Debug_F_Water_SDF (Debug_F_Water_Foam+1)
#define EndDebugWater (Debug_F_Water_SDF)

#define Debug_F_Role_KajiyaShift (EndDebugWater+1)
#define Debug_F_Role_Ramp (Debug_F_Role_KajiyaShift+1)
#define Debug_F_Role_Rim (Debug_F_Role_Ramp+1)
#define Debug_F_Role_DarkRim (Debug_F_Role_Rim+1)
#define EndDebugRole (Debug_F_Role_DarkRim)


//DEBUG_END


FLOAT4 DebugOutputColor(FLOAT4 OutColor,FFragData FragData,FMaterialData MaterialData, FCustomData CustomData,FLOAT2 SvPosition)
{
	FLOAT4 debugColor = OutColor;
	int debugMode = _DebugMode;
	UNITY_BRANCH
	if(_GlobalDebugMode>0)
	{
		debugMode = _GlobalDebugMode;
	}
	UNITY_BRANCH
	if (debugMode!=0)
	{
		debugColor = FLOAT4(0,0,0,0);
		//////////////////////////////vertex////////////////////////////////
		FLOAT mask = CalcDebugMask(debugMode,Debug_F_Vertex_uv);
		debugColor += mask*FLOAT4(GET_FRAG_UV, 0, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Vertex_uv2);
		debugColor += mask*FLOAT4(GET_FRAG_UV2, 0, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Vertex_VertexNormal);
		debugColor += mask*FLOAT4(GET_VERTEX_NORMAL, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Vertex_VertexTangent);
		debugColor += mask*FLOAT4(GET_VERTEX_TANGENT, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Vertex_VertexColor);
		debugColor += mask*FLOAT4(FragData.VertexColor.xyz, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Vertex_VertexColorA);
		debugColor += mask*FLOAT4(FragData.VertexColor.www, 1);		

		//////////////////////////////vs2ps////////////////////////////////
		mask = CalcDebugMask(debugMode,Debug_F_Frag_Depth01);
		debugColor += mask*FLOAT4(FragData.depth01.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Frag_Depth01Encode);
		FLOAT d = DecodeFloatRGB(EncodeFloatRGB(FragData.depth01));
		debugColor += mask*FLOAT4(d.xxx, 1);
		

		mask = CalcDebugMask(debugMode,Debug_F_Frag_LightmapUV);
		debugColor += mask*FLOAT4(GET_FRAG_LIGTHMAP_UV,0,1);

		mask = CalcDebugMask(debugMode,Debug_F_Frag_SvPosition);
		debugColor += mask*FragData.SvPosition;
		
		mask = CalcDebugMask(debugMode,Debug_F_Frag_ScreenPosition);
		debugColor += mask*FragData.ScreenPosition;

		mask = CalcDebugMask(debugMode,Debug_F_Frag_WorldPosition);
		debugColor += mask*FLOAT4(FragData.WorldPosition.xyz, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_Frag_CamRelative);
		debugColor += mask*FLOAT4(FragData.WorldPosition_CamRelative, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Frag_CameraVector);
		debugColor += mask*FLOAT4(FragData.CameraVector, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Frag_Ambient);
		debugColor += mask*FLOAT4(FragData.Ambient.xyz, 1);

#ifdef _CUSTOM_VERTEX_PARAM
		mask = CalcDebugMask(debugMode,Debug_F_Frag_CustomData);
		debugColor += mask*FLOAT4(FragData.CustomData.xyz, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Frag_CustomData1);
		debugColor += mask*FLOAT4(FragData.CustomData1.xyz, 1);
#endif//_CUSTOM_VERTEX_PARAM

		//////////////////////////////Material////////////////////////////////
		mask = CalcDebugMask(debugMode,Debug_F_Material_BaseColor);
		debugColor += mask*MaterialData.BaseColor;

		mask = CalcDebugMask(debugMode,Debug_F_Material_BaseColorAlpha);
		debugColor += mask*FLOAT4(MaterialData.BaseColor.aaa,1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_BlendTex);
		debugColor += mask*MaterialData.BlendMask;

		mask = CalcDebugMask(debugMode,Debug_F_Material_Metallic);
		debugColor += mask*FLOAT4(MaterialData.Metallic.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_Roughness);
		debugColor += mask*FLOAT4(MaterialData.Roughness.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_PerceptualRoughness);
		debugColor += mask*FLOAT4(MaterialData.PerceptualRoughness.xxx, 1);		

		mask = CalcDebugMask(debugMode,Debug_F_Material_WorldNormal);
		debugColor += mask*FLOAT4(MaterialData.WorldNormal, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_TangentSpaceNormal);
		debugColor += mask*FLOAT4(MaterialData.TangentSpaceNormal, 1);

#ifdef _ANISOTROPY
		mask = CalcDebugMask(debugMode,Debug_F_Material_WorldTangent);
		debugColor += mask*FLOAT4(MaterialData.WorldTangent, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_WorldBinormal);
		debugColor += mask*FLOAT4(MaterialData.WorldBinormal, 1);		
#endif

		mask = CalcDebugMask(debugMode,Debug_F_Material_NdotV);
		debugColor += mask*FLOAT4(MaterialData.NdotV.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_DiffuseColor);
		debugColor += mask*FLOAT4(MaterialData.DiffuseColor, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_SpecularColor);
		debugColor += mask*FLOAT4(MaterialData.SpecularColor, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_Emissive);
		debugColor += mask*FLOAT4(MaterialData.Emissive, 1);
		
		mask = CalcDebugMask(debugMode, Debug_F_Material_SrcAO);
		debugColor += mask * FLOAT4(MaterialData.SrcAO.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_AO);
		debugColor += mask*FLOAT4(MaterialData.AO.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Material_CustomParam);
		debugColor += mask*FLOAT4(MaterialData.CustomParam);
		
		//////////////////////////////GI////////////////////////////////
		mask = CalcDebugMask(debugMode,Debug_F_GI_Lightmap);
		debugColor += mask*FLOAT4(CustomData.GI, 1);

		mask = CalcDebugMask(debugMode,Debug_F_GI_DiffuseGI);
		debugColor += mask*FLOAT4(CustomData.DiffuseGI, 1);		

		mask = CalcDebugMask(debugMode,Debug_F_GI_AddGI);
		debugColor += mask*FLOAT4(CustomData.AddGI, 1);

		mask = CalcDebugMask(debugMode, Debug_F_GI_AddShadow);
		debugColor += mask * FLOAT4(CustomData.AddShadow.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_GI_SH);
		debugColor += mask*FLOAT4(CustomData.SH, 1);

		mask = CalcDebugMask(debugMode,Debug_F_GI_Ambient);
		debugColor += mask*FLOAT4(CustomData.Ambient, 1);

		//////////////////////////////Lighting////////////////////////////////
		mask = CalcDebugMask(debugMode,Debug_F_Lighting_DirectDiffuse);
		debugColor += mask*FLOAT4(CustomData.DirectDiffuse, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_Lighting_DirectSpecular);
		debugColor += mask*FLOAT4(CustomData.DirectSpecular, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_Specular);
		debugColor += mask*FLOAT4(CustomData.Specular, 1);		

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_DiffuseSupplement);
		debugColor += mask*FLOAT4(CustomData.DirectDiffuseSupplement, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_SpecularSupplement);
		debugColor += mask*FLOAT4(CustomData.DirectSpecularSupplement, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_SupplementMask);
		debugColor += mask*FLOAT4(CustomData.SupplementMask.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_CombineColor);
		debugColor += mask*FLOAT4(CustomData.CombineColor, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_Output);
		debugColor += mask*FLOAT4(CustomData.LightOutput, 1);		

		mask = CalcDebugMask(debugMode,Debug_F_Lighting_PostCustomLighting);
		debugColor += mask*FLOAT4(CustomData.PostCustomLighting, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Diffuse);
		debugColor += mask*FLOAT4(CustomData.AddDiffuse, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Specular);
		debugColor += mask*FLOAT4(CustomData.AddSpecular, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_AddIndex);
		FLOAT indexColor =(CustomData.AddIndex.x + CustomData.AddIndex.y+CustomData.AddIndex.z+CustomData.AddIndex.w);
		debugColor += mask*FLOAT4(indexColor.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_AddColor);
		debugColor += mask*FLOAT4(CustomData.AddColor, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Softness);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.softness.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_ToonAO);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.toonAO.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Ramp);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.ramp.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Atten);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.atten.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_LightColor);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.lightColor, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Ambient);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.ambient, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_Result);
		debugColor += mask*FLOAT4(CustomData.addLightDebug.result, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_DynamicDiffuse);
		debugColor += mask*FLOAT4(CustomData.AddDynamicDiffuse, 1);

		mask = CalcDebugMask(debugMode,Debug_F_AddLight_DynamicSpecular);
		debugColor += mask*FLOAT4(CustomData.AddDynamicSpecular, 1);

		//////////////////////////////Light Param////////////////////////////////
		mask = CalcDebugMask(debugMode, Debug_F_LC0_NdotL);
		debugColor += mask * FLOAT4(CustomData.LC0.NdotL.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC0_FixNdotL);
		debugColor += mask * FLOAT4(CustomData.LC0.FixNdotL.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC0_H);
#ifdef _UNREAL_MODE
		debugColor += mask * FLOAT4(CustomData.LC0.H.xxx, 1);
#else
		debugColor += mask * FLOAT4(CustomData.LC0.H, 1);
#endif		

		mask = CalcDebugMask(debugMode, Debug_F_LC0_NdotH);
		debugColor += mask * FLOAT4((CustomData.LC0.NdotH * CustomData.LC0.NdotH).xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC0_LdotH);
		debugColor += mask * FLOAT4(CustomData.LC0.LdotH.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC1_NdotL);
		debugColor += mask * FLOAT4(CustomData.LC1.NdotL.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC1_FixNdotL);
		debugColor += mask * FLOAT4(CustomData.LC1.FixNdotL.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC1_H);
#ifdef _UNREAL_MODE
		debugColor += mask * FLOAT4(CustomData.LC1.H.xxx, 1);
#else
		debugColor += mask * FLOAT4(CustomData.LC1.H, 1);
#endif

		mask = CalcDebugMask(debugMode, Debug_F_LC1_NdotH);
		debugColor += mask * FLOAT4(CustomData.LC1.NdotH.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_LC1_LdotH);
		debugColor += mask * FLOAT4(CustomData.LC1.LdotH.xxx, 1);

		//////////////////////////////Shadow////////////////////////////////
		FLOAT4 shadowIndexColor[4] = 
		{
			FLOAT4(0, 0, 1, 1),//blue
			FLOAT4(1, 0, 0, 1),//red
			FLOAT4(1, 1, 1, 1),//white
			FLOAT4(0, 0, 0, 1),//black
		};
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowMapIndex);
		debugColor += mask*shadowIndexColor[CustomData.ShadowMapIndex];

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowMapIndex0);
		debugColor += mask*((CustomData.ShadowMapIndex==0)?FLOAT4(0, 0, 1, 1):OutColor);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowMapIndex1);
		debugColor += mask*((CustomData.ShadowMapIndex==1)?FLOAT4(1, 0, 0, 1):OutColor);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowMapIndex2);
		debugColor += mask*((CustomData.ShadowMapIndex==2)?FLOAT4(1, 1, 1, 1):OutColor);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowCoord0);
		debugColor += mask*FLOAT4(FragData.ShadowCoord[0],1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowCoord1);
		debugColor += mask*FLOAT4(FragData.ShadowCoord[1],1);
#ifdef _CSM3
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowCoord2);
		debugColor += mask*FLOAT4(FragData.ShadowCoord[2],1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowCoordExtra);
		debugColor += mask*FLOAT4(FragData.ShadowCoord[3],1);
#else
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowCoordExtra);
		debugColor += mask*FLOAT4(FragData.ShadowCoord[2],1);
#endif
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_SceneShadow);
		debugColor += mask*FLOAT4(CustomData.SceneShadow.xxx, 1);

		mask = CalcDebugMask(debugMode, Debug_F_Shadow_SimpleShadow);
		debugColor += mask * FLOAT4(CustomData.SimpleShadow.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ExtraShadow);
		debugColor += mask*FLOAT4(CustomData.ExtraShadow.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ExtraShadow1);
		debugColor += mask*FLOAT4(CustomData.ExtraShadow1.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_SelfShadow);
		debugColor += mask*FLOAT4(CustomData.SelfShadow.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_SrcShadow);
		debugColor += mask*FLOAT4(CustomData.Shadow.yyy, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_FadeShadow);
		debugColor += mask*FLOAT4(CustomData.Shadow.xxx, 1);
		
		mask = CalcDebugMask(debugMode,Debug_F_Shadow_DynamicShadow);
		debugColor += mask*FLOAT4(CustomData.Shadow.zzz, 1);		

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_LightmapShadow);
		debugColor += mask*FLOAT4(CustomData.LightmapShadow.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Shadow_ShadowColor);
		debugColor += mask*FLOAT4(CustomData.ShadowColor, 1);

		//////////////////////////////IBL////////////////////////////////

		mask = CalcDebugMask(debugMode,Debug_F_IBL_ReflectionVector);
		debugColor += mask*FLOAT4(CustomData.ReflectionVector, 1);


		mask = CalcDebugMask(debugMode,Debug_F_IBL_IndirectSpecular);
		debugColor += mask*FLOAT4(CustomData.IndirectSpecular, 1);

		mask = CalcDebugMask(debugMode,Debug_F_IBL_IBLFresnel);
		debugColor += mask*FLOAT4(CustomData.IBLFresnel.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_IBL_IBLScale);
		debugColor += mask*FLOAT4(CustomData.IBLScale, 1);

		mask = CalcDebugMask(debugMode,Debug_F_IBL_IBL);
		debugColor += mask*FLOAT4(CustomData.IBL, 1);

		if(debugMode == Debug_F_IBL_CubeMipmapColor)
		{
			uint mipmap = (uint)CustomData.CubeMipmap;
			FLOAT4 mipmapColor[10] = 
			{
				FLOAT4(0, 0, 0, 1),//black
				FLOAT4(1, 0, 0, 1),//red
				FLOAT4(1, 0.5, 0, 1),//orange
				FLOAT4(1, 1, 0, 1),//yellow
				FLOAT4(0, 1, 0, 1),//green
				FLOAT4(0, 1, 1, 1),//cyan
				FLOAT4(0, 0, 1, 1),//blue
				FLOAT4(1, 0, 1, 1),//magenta
				FLOAT4(0.5, 0.5, 0.5, 1),//gray
				FLOAT4(1, 1, 1, 1),//white
			};
			debugColor = mipmapColor[mipmap];
		}
		mask = CalcDebugMask(debugMode, Debug_F_IBL_CubeMipmap);
		debugColor += mask * FLOAT4((CustomData.CubeMipmap * 0.1).xxx, 1);


		//////////////////////////////Misc////////////////////////////////
		mask = CalcDebugMask(debugMode,Debug_F_Water_Subsurface);
		debugColor += mask*FLOAT4(CustomData.Subsurface, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Water_SceneZRefract);
		debugColor += mask*FLOAT4(CustomData.SceneZRefract.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Water_Alpha);
		debugColor += mask*FLOAT4(CustomData.WaterAlpha, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Water_SkyCol);
		debugColor += mask*FLOAT4(CustomData.SkyCol, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Water_Foam);
		debugColor += mask*FLOAT4(CustomData.Foam, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Water_SDF);
		debugColor += mask*FLOAT4(CustomData.SDF.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Role_KajiyaShift);
		debugColor += mask*FLOAT4(CustomData.KajiyaShift.xxx, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Role_Ramp);
		debugColor += mask*FLOAT4(CustomData.Ramp, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Role_Rim);
		debugColor += mask*FLOAT4(CustomData.Rim, 1);

		mask = CalcDebugMask(debugMode,Debug_F_Role_DarkRim);
		debugColor += mask*FLOAT4(CustomData.DarkRim.xxx, 1);
		


		debugColor = SplitScreen(OutColor,debugColor,SvPosition);	
	}
	return debugColor;	
}

#endif//_ENABLE_DEBUG

#endif //PBS_DEBUG_INCLUDE
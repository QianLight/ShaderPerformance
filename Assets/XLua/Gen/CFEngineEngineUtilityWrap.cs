#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class CFEngineEngineUtilityWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFEngine.EngineUtility);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 78, 7, 6);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetRender", _m_GetRender_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetSceneRoot", _m_SetSceneRoot_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetAddScene", _m_SetAddScene_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetUIParent", _m_SetUIParent_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BeginLoading", _m_BeginLoading_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EndLoading", _m_EndLoading_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegistCameraAvoidBlockHandle", _m_RegistCameraAvoidBlockHandle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DoCameraAvoidBlockHandle", _m_DoCameraAvoidBlockHandle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetMainCamera", _m_SetMainCamera_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetMainCamera", _m_GetMainCamera_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DisableSkyBox", _m_DisableSkyBox_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EnableSkyBox", _m_EnableSkyBox_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Instantiate", _m_Instantiate_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Destroy", _m_Destroy_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "XHash", _m_XHash_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "XHashLowerRelpaceDot", _m_XHashLowerRelpaceDot_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PointToSegmentClosestPoint", _m_PointToSegmentClosestPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PointToLineClosestPoint", _m_PointToLineClosestPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsLineCircelCross", _m_IsLineCircelCross_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SqrDist3DSegmentToSegment", _m_SqrDist3DSegmentToSegment_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsLineSegmentCross", _m_IsLineSegmentCross_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CrossPointInSegment", _m_CrossPointInSegment_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BetweenLineAndCircle", _m_BetweenLineAndCircle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsLineSegmentCrossQuadrangle", _m_IsLineSegmentCrossQuadrangle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsLineSegmentCrossTriangle", _m_IsLineSegmentCrossTriangle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Expand", _m_Expand_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Point2PlaneDistance", _m_Point2PlaneDistance_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShaderRecover", _m_ShaderRecover_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSkinMesh", _m_GetSkinMesh_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ClearSkinMesh", _m_ClearSkinMesh_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterFollowPointCallBack", _m_RegisterFollowPointCallBack_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DealFollowPointCallBack", _m_DealFollowPointCallBack_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MemCpy", _m_MemCpy_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "HackArraySizeCall", _m_HackArraySizeCall_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadAnim", _m_LoadAnim_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadPrefab", _m_LoadPrefab_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadTex", _m_LoadTex_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadAsset", _m_LoadAsset_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LightQuadTreeTest", _m_LightQuadTreeTest_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IntersectsParallel3D", _m_IntersectsParallel3D_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "QuadTreeTest", _m_QuadTreeTest_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IntersectsAABB", _m_IntersectsAABB_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ContainPoint", _m_ContainPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RectContain", _m_RectContain_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RectAABBOverlaps", _m_RectAABBOverlaps_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RectOverlaps", _m_RectOverlaps_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetProjAABB", _m_GetProjAABB_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Point2Ray", _m_Point2Ray_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FastTest2dRayBox", _m_FastTest2dRayBox_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FastTestRayAABB", _m_FastTestRayAABB_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Ray2D", _m_Ray2D_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FastTestPointPlane", _m_FastTestPointPlane_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Dot", _m_Dot_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DistSqrXZ", _m_DistSqrXZ_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DistSqr", _m_DistSqr_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MinDist2AABB", _m_MinDist2AABB_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BoxIntersectFrustum", _m_BoxIntersectFrustum_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "VectorDist2", _m_VectorDist2_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Reflect", _m_Reflect_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Lerp", _m_Lerp_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsSphereInBox", _m_IsSphereInBox_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ScreenPercent", _m_ScreenPercent_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CompareVector", _m_CompareVector_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CompareQuation", _m_CompareQuation_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetFileName", _m_GetFileName_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsZero", _m_IsZero_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsOne", _m_IsOne_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetIAudio", _m_GetIAudio_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ReturnIAudio", _m_ReturnIAudio_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SavePNG", _m_SavePNG_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EnableGC", _m_EnableGC_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DisableGC", _m_DisableGC_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetRaycastCommandHelper", _m_SetRaycastCommandHelper_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CalcLod", _m_CalcLod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MaskToIndex", _m_MaskToIndex_xlua_st_);
            
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Far_Far_Away", CFEngine.EngineUtility.Far_Far_Away);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "UI_Far_Far_Away", CFEngine.EngineUtility.UI_Far_Far_Away);
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "RaycastCommandHelper", _g_get_RaycastCommandHelper);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "IsBuildingGame", _g_get_IsBuildingGame);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "AutoAssetPostprocessor", _g_get_AutoAssetPostprocessor);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "tmpSkinRender", _g_get_tmpSkinRender);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "tmpObject", _g_get_tmpObject);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "getAudio", _g_get_getAudio);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "returnAudio", _g_get_returnAudio);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "IsBuildingGame", _s_set_IsBuildingGame);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "AutoAssetPostprocessor", _s_set_AutoAssetPostprocessor);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "tmpSkinRender", _s_set_tmpSkinRender);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "tmpObject", _s_set_tmpObject);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "getAudio", _s_set_getAudio);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "returnAudio", _s_set_returnAudio);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFEngine.EngineUtility gen_ret = new CFEngine.EngineUtility();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetRender_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 1, typeof(UnityEngine.GameObject));
                    
                        System.Collections.Generic.List<UnityEngine.Renderer> gen_ret = CFEngine.EngineUtility.GetRender( _go );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetSceneRoot_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _sceneRoot = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    
                    CFEngine.EngineUtility.SetSceneRoot( _sceneRoot );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetAddScene_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    bool _bState = LuaAPI.lua_toboolean(L, 1);
                    
                    CFEngine.EngineUtility.SetAddScene( _bState );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetUIParent_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _holder = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    
                    CFEngine.EngineUtility.SetUIParent( _holder );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginLoading_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.BeginLoading(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EndLoading_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.EndLoading(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegistCameraAvoidBlockHandle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Action<bool> _callBack = translator.GetDelegate<System.Action<bool>>(L, 1);
                    
                    CFEngine.EngineUtility.RegistCameraAvoidBlockHandle( _callBack );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DoCameraAvoidBlockHandle_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    bool _b = LuaAPI.lua_toboolean(L, 1);
                    
                    CFEngine.EngineUtility.DoCameraAvoidBlockHandle( _b );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetMainCamera_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Camera _cam = (UnityEngine.Camera)translator.GetObject(L, 1, typeof(UnityEngine.Camera));
                    
                    CFEngine.EngineUtility.SetMainCamera( _cam );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetMainCamera_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        UnityEngine.Camera gen_ret = CFEngine.EngineUtility.GetMainCamera(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DisableSkyBox_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.DisableSkyBox(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EnableSkyBox_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.EnableSkyBox(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Instantiate_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Object>(L, 1)) 
                {
                    UnityEngine.Object _prefab = (UnityEngine.Object)translator.GetObject(L, 1, typeof(UnityEngine.Object));
                    
                        UnityEngine.Object gen_ret = CFEngine.EngineUtility.Instantiate( _prefab );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Object>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)) 
                {
                    UnityEngine.Object _original = (UnityEngine.Object)translator.GetObject(L, 1, typeof(UnityEngine.Object));
                    UnityEngine.Transform _holder = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        UnityEngine.Object gen_ret = CFEngine.EngineUtility.Instantiate( _original, _holder );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Object>(L, 1)) 
                {
                    UnityEngine.Object _original = (UnityEngine.Object)translator.GetObject(L, 1, typeof(UnityEngine.Object));
                    
                        UnityEngine.Object gen_ret = CFEngine.EngineUtility.Instantiate( _original );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.Instantiate!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Destroy_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Object _obj = (UnityEngine.Object)translator.GetObject(L, 1, typeof(UnityEngine.Object));
                    
                    CFEngine.EngineUtility.Destroy( _obj );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_XHash_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 1);
                    
                        uint gen_ret = CFEngine.EngineUtility.XHash( _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_XHashLowerRelpaceDot_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _str = LuaAPI.lua_tostring(L, 2);
                    
                        uint gen_ret = CFEngine.EngineUtility.XHashLowerRelpaceDot( _hash, _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PointToSegmentClosestPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _point;translator.Get(L, 1, out _point);
                    UnityEngine.Vector3 _segmentP0;translator.Get(L, 2, out _segmentP0);
                    UnityEngine.Vector3 _segmentP1;translator.Get(L, 3, out _segmentP1);
                    
                        UnityEngine.Vector3 gen_ret = CFEngine.EngineUtility.PointToSegmentClosestPoint( _point, _segmentP0, _segmentP1 );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PointToLineClosestPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _point;translator.Get(L, 1, out _point);
                    UnityEngine.Vector3 _segmentP0;translator.Get(L, 2, out _segmentP0);
                    UnityEngine.Vector3 _segmentP1;translator.Get(L, 3, out _segmentP1);
                    
                        UnityEngine.Vector3 gen_ret = CFEngine.EngineUtility.PointToLineClosestPoint( _point, _segmentP0, _segmentP1 );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLineCircelCross_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _from;translator.Get(L, 1, out _from);
                    UnityEngine.Vector3 _to;translator.Get(L, 2, out _to);
                    UnityEngine.Vector3 _center;translator.Get(L, 3, out _center);
                    float _radius = (float)LuaAPI.lua_tonumber(L, 4);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsLineCircelCross( _from, _to, _center, _radius );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SqrDist3DSegmentToSegment_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _s1p0;translator.Get(L, 1, out _s1p0);
                    UnityEngine.Vector3 _s1p1;translator.Get(L, 2, out _s1p1);
                    UnityEngine.Vector3 _s2p0;translator.Get(L, 3, out _s2p0);
                    UnityEngine.Vector3 _s2p1;translator.Get(L, 4, out _s2p1);
                    
                        float gen_ret = CFEngine.EngineUtility.SqrDist3DSegmentToSegment( _s1p0, _s1p1, _s2p0, _s2p1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLineSegmentCross_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _p1;translator.Get(L, 1, out _p1);
                    UnityEngine.Vector3 _p2;translator.Get(L, 2, out _p2);
                    UnityEngine.Vector3 _q1;translator.Get(L, 3, out _q1);
                    UnityEngine.Vector3 _q2;translator.Get(L, 4, out _q2);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsLineSegmentCross( _p1, _p2, _q1, _q2 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CrossPointInSegment_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _point;translator.Get(L, 1, out _point);
                    UnityEngine.Vector3 _segmentBegin;translator.Get(L, 2, out _segmentBegin);
                    UnityEngine.Vector3 _segmentEnd;translator.Get(L, 3, out _segmentEnd);
                    
                        bool gen_ret = CFEngine.EngineUtility.CrossPointInSegment( _point, _segmentBegin, _segmentEnd );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BetweenLineAndCircle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _circleCenter;translator.Get(L, 1, out _circleCenter);
                    float _circleRadius = (float)LuaAPI.lua_tonumber(L, 2);
                    UnityEngine.Vector3 _point1;translator.Get(L, 3, out _point1);
                    UnityEngine.Vector3 _point2;translator.Get(L, 4, out _point2);
                    UnityEngine.Vector3 _intersection1;
                    UnityEngine.Vector3 _intersection2;
                    
                        int gen_ret = CFEngine.EngineUtility.BetweenLineAndCircle( _circleCenter, _circleRadius, _point1, _point2, out _intersection1, out _intersection2 );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _intersection1);
                        
                    translator.PushUnityEngineVector3(L, _intersection2);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLineSegmentCrossQuadrangle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _p1;translator.Get(L, 1, out _p1);
                    UnityEngine.Vector3 _p2;translator.Get(L, 2, out _p2);
                    UnityEngine.Vector3 _p3;translator.Get(L, 3, out _p3);
                    UnityEngine.Vector3 _p4;translator.Get(L, 4, out _p4);
                    UnityEngine.Vector3 _normal;translator.Get(L, 5, out _normal);
                    UnityEngine.Vector3 _p;translator.Get(L, 6, out _p);
                    UnityEngine.Vector3 _q;translator.Get(L, 7, out _q);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsLineSegmentCrossQuadrangle( _p1, _p2, _p3, _p4, _normal, _p, _q );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLineSegmentCrossTriangle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _a;translator.Get(L, 1, out _a);
                    UnityEngine.Vector3 _b;translator.Get(L, 2, out _b);
                    UnityEngine.Vector3 _c;translator.Get(L, 3, out _c);
                    UnityEngine.Vector3 _p;translator.Get(L, 4, out _p);
                    UnityEngine.Vector3 _q;translator.Get(L, 5, out _q);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsLineSegmentCrossTriangle( _a, _b, _c, _p, _q );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Expand_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector4 _src;translator.Get(L, 1, out _src);
                    float _minx = (float)LuaAPI.lua_tonumber(L, 2);
                    float _miny = (float)LuaAPI.lua_tonumber(L, 3);
                    float _maxx = (float)LuaAPI.lua_tonumber(L, 4);
                    float _maxy = (float)LuaAPI.lua_tonumber(L, 5);
                    
                        bool gen_ret = CFEngine.EngineUtility.Expand( ref _src, _minx, _miny, _maxx, _maxy );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector4(L, _src);
                        translator.UpdateUnityEngineVector4(L, 1, _src);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Point2PlaneDistance_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Vector3 _pos;translator.Get(L, 1, out _pos);
                    UnityEngine.Vector3 _normal;translator.Get(L, 2, out _normal);
                    float _dist = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = CFEngine.EngineUtility.Point2PlaneDistance( ref _pos, ref _normal, _dist );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _pos);
                        translator.UpdateUnityEngineVector3(L, 1, _pos);
                        
                    translator.PushUnityEngineVector3(L, _normal);
                        translator.UpdateUnityEngineVector3(L, 2, _normal);
                        
                    
                    
                    
                    return 3;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Vector3 _pos;translator.Get(L, 1, out _pos);
                    UnityEngine.Vector3 _normal;translator.Get(L, 2, out _normal);
                    float _dist = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = CFEngine.EngineUtility.Point2PlaneDistance( ref _pos, _normal, _dist );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _pos);
                        translator.UpdateUnityEngineVector3(L, 1, _pos);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.Point2PlaneDistance!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShaderRecover_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.GameObject _obj = (UnityEngine.GameObject)translator.GetObject(L, 1, typeof(UnityEngine.GameObject));
                    
                    CFEngine.EngineUtility.ShaderRecover( _obj );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkinMesh_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 1, typeof(UnityEngine.GameObject));
                    
                        System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer> gen_ret = CFEngine.EngineUtility.GetSkinMesh( _go );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearSkinMesh_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.ClearSkinMesh(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterFollowPointCallBack_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Action<UnityEngine.Transform> _callBack = translator.GetDelegate<System.Action<UnityEngine.Transform>>(L, 1);
                    
                    CFEngine.EngineUtility.RegisterFollowPointCallBack( _callBack );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DealFollowPointCallBack_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    
                    CFEngine.EngineUtility.DealFollowPointCallBack( _tf );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MemCpy_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector3[]>(L, 1)&& translator.Assignable<UnityEngine.Vector3[]>(L, 2)) 
                {
                    UnityEngine.Vector3[] _src = (UnityEngine.Vector3[])translator.GetObject(L, 1, typeof(UnityEngine.Vector3[]));
                    UnityEngine.Vector3[] _des = (UnityEngine.Vector3[])translator.GetObject(L, 2, typeof(UnityEngine.Vector3[]));
                    
                    CFEngine.EngineUtility.MemCpy( _src, _des );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<int[]>(L, 1)&& translator.Assignable<int[]>(L, 2)) 
                {
                    int[] _src = (int[])translator.GetObject(L, 1, typeof(int[]));
                    int[] _des = (int[])translator.GetObject(L, 2, typeof(int[]));
                    
                    CFEngine.EngineUtility.MemCpy( _src, _des );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.MemCpy!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HackArraySizeCall_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    float[] _array = (float[])translator.GetObject(L, 1, typeof(float[]));
                    int _hacksize = LuaAPI.xlua_tointeger(L, 2);
                    System.Action<float[], object> _func = translator.GetDelegate<System.Action<float[], object>>(L, 3);
                    object _param = translator.GetObject(L, 4, typeof(object));
                    
                    CFEngine.EngineUtility.HackArraySizeCall( _array, _hacksize, _func, _param );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadAnim_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    bool _bImmediateRelease = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.AnimationClip gen_ret = CFEngine.EngineUtility.LoadAnim( _path, ref _ah, _flag, _bImmediateRelease );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    
                        UnityEngine.AnimationClip gen_ret = CFEngine.EngineUtility.LoadAnim( _path, ref _ah, _flag );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    
                        UnityEngine.AnimationClip gen_ret = CFEngine.EngineUtility.LoadAnim( _path, ref _ah );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.LoadAnim!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadPrefab_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 6&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& translator.Assignable<CFEngine.ResLoadCb>(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    bool _useObjectPool = LuaAPI.lua_toboolean(L, 4);
                    CFEngine.ResLoadCb _prefabCb = translator.GetDelegate<CFEngine.ResLoadCb>(L, 5);
                    bool _bImmediateRelease = LuaAPI.lua_toboolean(L, 6);
                    
                        UnityEngine.GameObject gen_ret = CFEngine.EngineUtility.LoadPrefab( _path, ref _ah, _flag, _useObjectPool, _prefabCb, _bImmediateRelease );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 5&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& translator.Assignable<CFEngine.ResLoadCb>(L, 5)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    bool _useObjectPool = LuaAPI.lua_toboolean(L, 4);
                    CFEngine.ResLoadCb _prefabCb = translator.GetDelegate<CFEngine.ResLoadCb>(L, 5);
                    
                        UnityEngine.GameObject gen_ret = CFEngine.EngineUtility.LoadPrefab( _path, ref _ah, _flag, _useObjectPool, _prefabCb );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    bool _useObjectPool = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.GameObject gen_ret = CFEngine.EngineUtility.LoadPrefab( _path, ref _ah, _flag, _useObjectPool );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    
                        UnityEngine.GameObject gen_ret = CFEngine.EngineUtility.LoadPrefab( _path, ref _ah, _flag );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 2)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    
                        UnityEngine.GameObject gen_ret = CFEngine.EngineUtility.LoadPrefab( _path, ref _ah );
                        translator.Push(L, gen_ret);
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.LoadPrefab!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadTex_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    string _ext = LuaAPI.lua_tostring(L, 2);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 3, typeof(CFEngine.AssetHandler));
                    bool _bImmediateRelease = LuaAPI.lua_toboolean(L, 4);
                    
                    CFEngine.EngineUtility.LoadTex( _path, _ext, ref _ah, _bImmediateRelease );
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.AssetHandler>(L, 3)) 
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    string _ext = LuaAPI.lua_tostring(L, 2);
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 3, typeof(CFEngine.AssetHandler));
                    
                    CFEngine.EngineUtility.LoadTex( _path, _ext, ref _ah );
                    translator.Push(L, _ah);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.LoadTex!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadAsset_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<CFEngine.AssetHandler>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 1, typeof(CFEngine.AssetHandler));
                    string _ext = LuaAPI.lua_tostring(L, 2);
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    bool _bImmediateRelease = LuaAPI.lua_toboolean(L, 4);
                    
                    CFEngine.EngineUtility.LoadAsset( _ah, _ext, _flag, _bImmediateRelease );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<CFEngine.AssetHandler>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 1, typeof(CFEngine.AssetHandler));
                    string _ext = LuaAPI.lua_tostring(L, 2);
                    uint _flag = LuaAPI.xlua_touint(L, 3);
                    
                    CFEngine.EngineUtility.LoadAsset( _ah, _ext, _flag );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<CFEngine.AssetHandler>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    CFEngine.AssetHandler _ah = (CFEngine.AssetHandler)translator.GetObject(L, 1, typeof(CFEngine.AssetHandler));
                    string _ext = LuaAPI.lua_tostring(L, 2);
                    
                    CFEngine.EngineUtility.LoadAsset( _ah, _ext );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.LoadAsset!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LightQuadTreeTest_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    bool _calcObjectBoundLS = LuaAPI.lua_toboolean(L, 1);
                    CFEngine.AABB _objectBoundLS;translator.Get(L, 2, out _objectBoundLS);
                    CFEngine.AABB _objectBoundWS;translator.Get(L, 3, out _objectBoundWS);
                    UnityEngine.Matrix4x4 _cullMatrix;translator.Get(L, 4, out _cullMatrix);
                    UnityEngine.Rect _boundRectLS;translator.Get(L, 5, out _boundRectLS);
                    CFEngine.AABB _frustumBoundLS;translator.Get(L, 6, out _frustumBoundLS);
                    bool _testObj = LuaAPI.lua_toboolean(L, 7);
                    
                        bool gen_ret = CFEngine.EngineUtility.LightQuadTreeTest( _calcObjectBoundLS, ref _objectBoundLS, ref _objectBoundWS, ref _cullMatrix, ref _boundRectLS, ref _frustumBoundLS, _testObj );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _objectBoundLS);
                        translator.Update(L, 2, _objectBoundLS);
                        
                    translator.Push(L, _objectBoundWS);
                        translator.Update(L, 3, _objectBoundWS);
                        
                    translator.Push(L, _cullMatrix);
                        translator.Update(L, 4, _cullMatrix);
                        
                    translator.Push(L, _boundRectLS);
                        translator.Update(L, 5, _boundRectLS);
                        
                    translator.Push(L, _frustumBoundLS);
                        translator.Update(L, 6, _frustumBoundLS);
                        
                    
                    
                    
                    return 6;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IntersectsParallel3D_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.Parallel3D _parallel3D;translator.Get(L, 1, out _parallel3D);
                    CFEngine.AABB _objectBoundWS;translator.Get(L, 2, out _objectBoundWS);
                    UnityEngine.Vector3 _dir;translator.Get(L, 3, out _dir);
                    float _invSin = (float)LuaAPI.lua_tonumber(L, 4);
                    bool _testYmax = LuaAPI.lua_toboolean(L, 5);
                    
                        bool gen_ret = CFEngine.EngineUtility.IntersectsParallel3D( ref _parallel3D, ref _objectBoundWS, ref _dir, _invSin, _testYmax );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _parallel3D);
                        translator.Update(L, 1, _parallel3D);
                        
                    translator.Push(L, _objectBoundWS);
                        translator.Update(L, 2, _objectBoundWS);
                        
                    translator.PushUnityEngineVector3(L, _dir);
                        translator.UpdateUnityEngineVector3(L, 3, _dir);
                        
                    
                    
                    
                    return 4;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_QuadTreeTest_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.AABB _aabb;translator.Get(L, 1, out _aabb);
                    UnityEngine.Plane[] _planes = (UnityEngine.Plane[])translator.GetObject(L, 2, typeof(UnityEngine.Plane[]));
                    
                        bool gen_ret = CFEngine.EngineUtility.QuadTreeTest( ref _aabb, _planes );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _aabb);
                        translator.Update(L, 1, _aabb);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IntersectsAABB_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<CFEngine.AABB>(L, 1)&& translator.Assignable<CFEngine.AABB>(L, 2)) 
                {
                    CFEngine.AABB _aabb0;translator.Get(L, 1, out _aabb0);
                    CFEngine.AABB _aabb1;translator.Get(L, 2, out _aabb1);
                    
                        bool gen_ret = CFEngine.EngineUtility.IntersectsAABB( ref _aabb0, ref _aabb1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _aabb0);
                        translator.Update(L, 1, _aabb0);
                        
                    translator.Push(L, _aabb1);
                        translator.Update(L, 2, _aabb1);
                        
                    
                    
                    
                    return 3;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& translator.Assignable<UnityEngine.Vector3>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)) 
                {
                    UnityEngine.Vector3 _min0;translator.Get(L, 1, out _min0);
                    UnityEngine.Vector3 _max0;translator.Get(L, 2, out _max0);
                    UnityEngine.Vector3 _min1;translator.Get(L, 3, out _min1);
                    UnityEngine.Vector3 _max1;translator.Get(L, 4, out _max1);
                    
                        bool gen_ret = CFEngine.EngineUtility.IntersectsAABB( ref _min0, ref _max0, ref _min1, ref _max1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _min0);
                        translator.UpdateUnityEngineVector3(L, 1, _min0);
                        
                    translator.PushUnityEngineVector3(L, _max0);
                        translator.UpdateUnityEngineVector3(L, 2, _max0);
                        
                    translator.PushUnityEngineVector3(L, _min1);
                        translator.UpdateUnityEngineVector3(L, 3, _min1);
                        
                    translator.PushUnityEngineVector3(L, _max1);
                        translator.UpdateUnityEngineVector3(L, 4, _max1);
                        
                    
                    
                    
                    return 5;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.IntersectsAABB!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ContainPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.AABB _aabb;translator.Get(L, 1, out _aabb);
                    UnityEngine.Vector3 _point;translator.Get(L, 2, out _point);
                    
                        bool gen_ret = CFEngine.EngineUtility.ContainPoint( ref _aabb, ref _point );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _aabb);
                        translator.Update(L, 1, _aabb);
                        
                    translator.PushUnityEngineVector3(L, _point);
                        translator.UpdateUnityEngineVector3(L, 2, _point);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RectContain_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& translator.Assignable<UnityEngine.Vector3>(L, 5)) 
                {
                    float _minx = (float)LuaAPI.lua_tonumber(L, 1);
                    float _miny = (float)LuaAPI.lua_tonumber(L, 2);
                    float _maxx = (float)LuaAPI.lua_tonumber(L, 3);
                    float _maxy = (float)LuaAPI.lua_tonumber(L, 4);
                    UnityEngine.Vector3 _point;translator.Get(L, 5, out _point);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectContain( _minx, _miny, _maxx, _maxy, ref _point );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _point);
                        translator.UpdateUnityEngineVector3(L, 5, _point);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& translator.Assignable<UnityEngine.Vector2>(L, 5)) 
                {
                    float _minx = (float)LuaAPI.lua_tonumber(L, 1);
                    float _miny = (float)LuaAPI.lua_tonumber(L, 2);
                    float _maxx = (float)LuaAPI.lua_tonumber(L, 3);
                    float _maxy = (float)LuaAPI.lua_tonumber(L, 4);
                    UnityEngine.Vector2 _point;translator.Get(L, 5, out _point);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectContain( _minx, _miny, _maxx, _maxy, ref _point );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector2(L, _point);
                        translator.UpdateUnityEngineVector2(L, 5, _point);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.RectContain!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RectAABBOverlaps_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Rect>(L, 1)&& translator.Assignable<CFEngine.AABB>(L, 2)) 
                {
                    UnityEngine.Rect _boundRect;translator.Get(L, 1, out _boundRect);
                    CFEngine.AABB _objectBound;translator.Get(L, 2, out _objectBound);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectAABBOverlaps( ref _boundRect, ref _objectBound );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _boundRect);
                        translator.Update(L, 1, _boundRect);
                        
                    translator.Push(L, _objectBound);
                        translator.Update(L, 2, _objectBound);
                        
                    
                    
                    
                    return 3;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector4>(L, 1)&& translator.Assignable<CFEngine.AABB>(L, 2)) 
                {
                    UnityEngine.Vector4 _boundRect;translator.Get(L, 1, out _boundRect);
                    CFEngine.AABB _objectBound;translator.Get(L, 2, out _objectBound);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectAABBOverlaps( ref _boundRect, ref _objectBound );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector4(L, _boundRect);
                        translator.UpdateUnityEngineVector4(L, 1, _boundRect);
                        
                    translator.Push(L, _objectBound);
                        translator.Update(L, 2, _objectBound);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.RectAABBOverlaps!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RectOverlaps_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 8&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 7)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 8)) 
                {
                    float _minx = (float)LuaAPI.lua_tonumber(L, 1);
                    float _miny = (float)LuaAPI.lua_tonumber(L, 2);
                    float _maxx = (float)LuaAPI.lua_tonumber(L, 3);
                    float _maxy = (float)LuaAPI.lua_tonumber(L, 4);
                    float _minx1 = (float)LuaAPI.lua_tonumber(L, 5);
                    float _miny1 = (float)LuaAPI.lua_tonumber(L, 6);
                    float _maxx1 = (float)LuaAPI.lua_tonumber(L, 7);
                    float _maxy1 = (float)LuaAPI.lua_tonumber(L, 8);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectOverlaps( _minx, _miny, _maxx, _maxy, _minx1, _miny1, _maxx1, _maxy1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector4>(L, 1)&& translator.Assignable<UnityEngine.Vector4>(L, 2)) 
                {
                    UnityEngine.Vector4 _boundRect;translator.Get(L, 1, out _boundRect);
                    UnityEngine.Vector4 _objectBound;translator.Get(L, 2, out _objectBound);
                    
                        bool gen_ret = CFEngine.EngineUtility.RectOverlaps( ref _boundRect, ref _objectBound );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector4(L, _boundRect);
                        translator.UpdateUnityEngineVector4(L, 1, _boundRect);
                        
                    translator.PushUnityEngineVector4(L, _objectBound);
                        translator.UpdateUnityEngineVector4(L, 2, _objectBound);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.RectOverlaps!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetProjAABB_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector4 _proj;translator.Get(L, 1, out _proj);
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    UnityEngine.Vector3 _vec;translator.Get(L, 3, out _vec);
                    UnityEngine.Vector3 _tmp;translator.Get(L, 4, out _tmp);
                    float _offsetX = (float)LuaAPI.lua_tonumber(L, 5);
                    float _offsetZ = (float)LuaAPI.lua_tonumber(L, 6);
                    
                    CFEngine.EngineUtility.GetProjAABB( ref _proj, ref _pos, ref _vec, ref _tmp, _offsetX, _offsetZ );
                    translator.PushUnityEngineVector4(L, _proj);
                        translator.UpdateUnityEngineVector4(L, 1, _proj);
                        
                    translator.PushUnityEngineVector3(L, _pos);
                        translator.UpdateUnityEngineVector3(L, 2, _pos);
                        
                    translator.PushUnityEngineVector3(L, _vec);
                        translator.UpdateUnityEngineVector3(L, 3, _vec);
                        
                    translator.PushUnityEngineVector3(L, _tmp);
                        translator.UpdateUnityEngineVector3(L, 4, _tmp);
                        
                    
                    
                    
                    return 4;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Point2Ray_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    float _x = (float)LuaAPI.lua_tonumber(L, 1);
                    float _y = (float)LuaAPI.lua_tonumber(L, 2);
                    UnityEngine.Vector3 _normalDist;translator.Get(L, 3, out _normalDist);
                    
                        float gen_ret = CFEngine.EngineUtility.Point2Ray( _x, _y, ref _normalDist );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _normalDist);
                        translator.UpdateUnityEngineVector3(L, 3, _normalDist);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FastTest2dRayBox_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _normalDist;translator.Get(L, 1, out _normalDist);
                    float _xMin = (float)LuaAPI.lua_tonumber(L, 2);
                    float _yMin = (float)LuaAPI.lua_tonumber(L, 3);
                    float _xMax = (float)LuaAPI.lua_tonumber(L, 4);
                    float _yMax = (float)LuaAPI.lua_tonumber(L, 5);
                    
                        bool gen_ret = CFEngine.EngineUtility.FastTest2dRayBox( ref _normalDist, _xMin, _yMin, _xMax, _yMax );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _normalDist);
                        translator.UpdateUnityEngineVector3(L, 1, _normalDist);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FastTestRayAABB_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& translator.Assignable<CFEngine.AABB>(L, 3)) 
                {
                    UnityEngine.Vector3 _normal;translator.Get(L, 1, out _normal);
                    float _dist = (float)LuaAPI.lua_tonumber(L, 2);
                    CFEngine.AABB _aabb;translator.Get(L, 3, out _aabb);
                    
                        bool gen_ret = CFEngine.EngineUtility.FastTestRayAABB( ref _normal, _dist, ref _aabb );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _normal);
                        translator.UpdateUnityEngineVector3(L, 1, _normal);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 3, _aabb);
                        
                    
                    
                    
                    return 3;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& translator.Assignable<CFEngine.AABB>(L, 3)) 
                {
                    UnityEngine.Vector3 _start;translator.Get(L, 1, out _start);
                    UnityEngine.Vector3 _dir;translator.Get(L, 2, out _dir);
                    CFEngine.AABB _aabb;translator.Get(L, 3, out _aabb);
                    
                        bool gen_ret = CFEngine.EngineUtility.FastTestRayAABB( ref _start, ref _dir, ref _aabb );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _start);
                        translator.UpdateUnityEngineVector3(L, 1, _start);
                        
                    translator.PushUnityEngineVector3(L, _dir);
                        translator.UpdateUnityEngineVector3(L, 2, _dir);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 3, _aabb);
                        
                    
                    
                    
                    return 4;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& translator.Assignable<CFEngine.Ray2D>(L, 3)&& translator.Assignable<CFEngine.AABB>(L, 4)) 
                {
                    UnityEngine.Vector3 _start;translator.Get(L, 1, out _start);
                    UnityEngine.Vector3 _end;translator.Get(L, 2, out _end);
                    CFEngine.Ray2D _ray2D;translator.Get(L, 3, out _ray2D);
                    CFEngine.AABB _aabb;translator.Get(L, 4, out _aabb);
                    CFEngine.ERayAABB _rayAABB;
                    
                        bool gen_ret = CFEngine.EngineUtility.FastTestRayAABB( ref _start, ref _end, ref _ray2D, ref _aabb, out _rayAABB );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _start);
                        translator.UpdateUnityEngineVector3(L, 1, _start);
                        
                    translator.PushUnityEngineVector3(L, _end);
                        translator.UpdateUnityEngineVector3(L, 2, _end);
                        
                    translator.Push(L, _ray2D);
                        translator.Update(L, 3, _ray2D);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 4, _aabb);
                        
                    translator.Push(L, _rayAABB);
                        
                    
                    
                    
                    return 6;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.FastTestRayAABB!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Ray2D_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    float _x = (float)LuaAPI.lua_tonumber(L, 1);
                    float _y = (float)LuaAPI.lua_tonumber(L, 2);
                    float _x0 = (float)LuaAPI.lua_tonumber(L, 3);
                    float _y0 = (float)LuaAPI.lua_tonumber(L, 4);
                    UnityEngine.Vector3 _ray2D;translator.Get(L, 5, out _ray2D);
                    
                    CFEngine.EngineUtility.Ray2D( _x, _y, _x0, _y0, ref _ray2D );
                    translator.PushUnityEngineVector3(L, _ray2D);
                        translator.UpdateUnityEngineVector3(L, 5, _ray2D);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FastTestPointPlane_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _start;translator.Get(L, 1, out _start);
                    UnityEngine.Vector3 _end;translator.Get(L, 2, out _end);
                    CFEngine.AABB _aabb;translator.Get(L, 3, out _aabb);
                    
                        bool gen_ret = CFEngine.EngineUtility.FastTestPointPlane( ref _start, ref _end, ref _aabb );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _start);
                        translator.UpdateUnityEngineVector3(L, 1, _start);
                        
                    translator.PushUnityEngineVector3(L, _end);
                        translator.UpdateUnityEngineVector3(L, 2, _end);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 3, _aabb);
                        
                    
                    
                    
                    return 4;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Dot_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 6&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)) 
                {
                    float _vx = (float)LuaAPI.lua_tonumber(L, 1);
                    float _vy = (float)LuaAPI.lua_tonumber(L, 2);
                    float _vz = (float)LuaAPI.lua_tonumber(L, 3);
                    float _x = (float)LuaAPI.lua_tonumber(L, 4);
                    float _y = (float)LuaAPI.lua_tonumber(L, 5);
                    float _z = (float)LuaAPI.lua_tonumber(L, 6);
                    
                        float gen_ret = CFEngine.EngineUtility.Dot( _vx, _vy, _vz, _x, _y, _z );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 1, out _v);
                    float _x = (float)LuaAPI.lua_tonumber(L, 2);
                    float _y = (float)LuaAPI.lua_tonumber(L, 3);
                    float _z = (float)LuaAPI.lua_tonumber(L, 4);
                    
                        float gen_ret = CFEngine.EngineUtility.Dot( ref _v, _x, _y, _z );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 1, _v);
                        
                    
                    
                    
                    return 2;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    
                        float gen_ret = CFEngine.EngineUtility.Dot( ref _v0, ref _v1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.Dot!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DistSqrXZ_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    
                        float gen_ret = CFEngine.EngineUtility.DistSqrXZ( ref _v0, ref _v1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DistSqr_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    
                        float gen_ret = CFEngine.EngineUtility.DistSqr( ref _v0, ref _v1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    
                    
                    
                    return 3;
                }
                if(gen_param_count == 2&& translator.Assignable<CFEngine.AABB>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    CFEngine.AABB _aabb;translator.Get(L, 1, out _aabb);
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    
                        float gen_ret = CFEngine.EngineUtility.DistSqr( ref _aabb, ref _v );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.Push(L, _aabb);
                        translator.Update(L, 1, _aabb);
                        
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 2, _v);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.DistSqr!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MinDist2AABB_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 1, out _v);
                    CFEngine.AABB _aabb;translator.Get(L, 2, out _aabb);
                    
                        float gen_ret = CFEngine.EngineUtility.MinDist2AABB( ref _v, ref _aabb );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 1, _v);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 2, _aabb);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BoxIntersectFrustum_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.AABB _aabb;translator.Get(L, 1, out _aabb);
                    UnityEngine.Plane[] _planes = (UnityEngine.Plane[])translator.GetObject(L, 2, typeof(UnityEngine.Plane[]));
                    
                        bool gen_ret = CFEngine.EngineUtility.BoxIntersectFrustum( ref _aabb, _planes );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _aabb);
                        translator.Update(L, 1, _aabb);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_VectorDist2_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    
                        float gen_ret = CFEngine.EngineUtility.VectorDist2( ref _v0, ref _v1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reflect_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _dir;translator.Get(L, 1, out _dir);
                    UnityEngine.Vector3 _normal;translator.Get(L, 2, out _normal);
                    
                        UnityEngine.Vector3 gen_ret = CFEngine.EngineUtility.Reflect( ref _dir, ref _normal );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _dir);
                        translator.UpdateUnityEngineVector3(L, 1, _dir);
                        
                    translator.PushUnityEngineVector3(L, _normal);
                        translator.UpdateUnityEngineVector3(L, 2, _normal);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Lerp_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    float _v0 = (float)LuaAPI.lua_tonumber(L, 1);
                    float _v1 = (float)LuaAPI.lua_tonumber(L, 2);
                    float _t = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = CFEngine.EngineUtility.Lerp( _v0, _v1, _t );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector3>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& translator.Assignable<UnityEngine.Vector3>(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    UnityEngine.Vector3 _v;translator.Get(L, 3, out _v);
                    float _t = (float)LuaAPI.lua_tonumber(L, 4);
                    
                    CFEngine.EngineUtility.Lerp( ref _v0, ref _v1, ref _v, _t );
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 3, _v);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.Lerp!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsSphereInBox_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector4 _cube;translator.Get(L, 1, out _cube);
                    UnityEngine.Vector4 _sphere;translator.Get(L, 2, out _sphere);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsSphereInBox( ref _cube, ref _sphere );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector4(L, _cube);
                        translator.UpdateUnityEngineVector4(L, 1, _cube);
                        
                    translator.PushUnityEngineVector4(L, _sphere);
                        translator.UpdateUnityEngineVector4(L, 2, _sphere);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ScreenPercent_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Camera _camera = (UnityEngine.Camera)translator.GetObject(L, 1, typeof(UnityEngine.Camera));
                    UnityEngine.Bounds _b;translator.Get(L, 2, out _b);
                    float _percent;
                    
                        bool gen_ret = CFEngine.EngineUtility.ScreenPercent( _camera, _b, out _percent );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    LuaAPI.lua_pushnumber(L, _percent);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CompareVector_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _v0;translator.Get(L, 1, out _v0);
                    UnityEngine.Vector3 _v1;translator.Get(L, 2, out _v1);
                    
                        bool gen_ret = CFEngine.EngineUtility.CompareVector( ref _v0, ref _v1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v0);
                        translator.UpdateUnityEngineVector3(L, 1, _v0);
                        
                    translator.PushUnityEngineVector3(L, _v1);
                        translator.UpdateUnityEngineVector3(L, 2, _v1);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CompareQuation_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Quaternion _q0;translator.Get(L, 1, out _q0);
                    UnityEngine.Quaternion _q1;translator.Get(L, 2, out _q1);
                    
                        bool gen_ret = CFEngine.EngineUtility.CompareQuation( ref _q0, ref _q1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineQuaternion(L, _q0);
                        translator.UpdateUnityEngineQuaternion(L, 1, _q0);
                        
                    translator.PushUnityEngineQuaternion(L, _q1);
                        translator.UpdateUnityEngineQuaternion(L, 2, _q1);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetFileName_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _path = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = CFEngine.EngineUtility.GetFileName( _path );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsZero_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    float _v = (float)LuaAPI.lua_tonumber(L, 1);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsZero( _v );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Vector3>(L, 1)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 1, out _v);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsZero( ref _v );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 1, _v);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.IsZero!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsOne_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 1, out _v);
                    
                        bool gen_ret = CFEngine.EngineUtility.IsOne( ref _v );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 1, _v);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetIAudio_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        CFEngine.IAuido gen_ret = CFEngine.EngineUtility.GetIAudio(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ReturnIAudio_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.IAuido _audio = (CFEngine.IAuido)translator.GetObject(L, 1, typeof(CFEngine.IAuido));
                    
                    CFEngine.EngineUtility.ReturnIAudio( _audio );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SavePNG_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    Unity.Collections.NativeArray<byte> _data;translator.Get(L, 1, out _data);
                    int _width = LuaAPI.xlua_tointeger(L, 2);
                    int _height = LuaAPI.xlua_tointeger(L, 3);
                    string _path = LuaAPI.lua_tostring(L, 4);
                    
                    CFEngine.EngineUtility.SavePNG( ref _data, _width, _height, _path );
                    translator.Push(L, _data);
                        translator.Update(L, 1, _data);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EnableGC_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)) 
                {
                    bool _autoDis = LuaAPI.lua_toboolean(L, 1);
                    
                    CFEngine.EngineUtility.EnableGC( _autoDis );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 0) 
                {
                    
                    CFEngine.EngineUtility.EnableGC(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFEngine.EngineUtility.EnableGC!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DisableGC_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFEngine.EngineUtility.DisableGC(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetRaycastCommandHelper_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.EngineUtility.PhysicsRaycastDelegate _action = translator.GetDelegate<CFEngine.EngineUtility.PhysicsRaycastDelegate>(L, 1);
                    
                    CFEngine.EngineUtility.SetRaycastCommandHelper( _action );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CalcLod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.LodSize _near;translator.Get(L, 1, out _near);
                    CFEngine.LodSize _far;translator.Get(L, 2, out _far);
                    CFEngine.AABB _aabb;translator.Get(L, 3, out _aabb);
                    CFEngine.LodDist _ld;translator.Get(L, 4, out _ld);
                    
                    CFEngine.EngineUtility.CalcLod( ref _near, ref _far, ref _aabb, ref _ld );
                    translator.Push(L, _near);
                        translator.Update(L, 1, _near);
                        
                    translator.Push(L, _far);
                        translator.Update(L, 2, _far);
                        
                    translator.Push(L, _aabb);
                        translator.Update(L, 3, _aabb);
                        
                    translator.Push(L, _ld);
                        translator.Update(L, 4, _ld);
                        
                    
                    
                    
                    return 4;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MaskToIndex_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _mask = LuaAPI.xlua_touint(L, 1);
                    
                        int gen_ret = CFEngine.EngineUtility.MaskToIndex( _mask );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_RaycastCommandHelper(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFEngine.EngineUtility.RaycastCommandHelper);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsBuildingGame(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushboolean(L, CFEngine.EngineUtility.IsBuildingGame);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_AutoAssetPostprocessor(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushboolean(L, CFEngine.EngineUtility.AutoAssetPostprocessor);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_tmpSkinRender(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFEngine.EngineUtility.tmpSkinRender);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_tmpObject(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFEngine.EngineUtility.tmpObject);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_getAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFEngine.EngineUtility.getAudio);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_returnAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFEngine.EngineUtility.returnAudio);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IsBuildingGame(RealStatePtr L)
        {
		    try {
                
			    CFEngine.EngineUtility.IsBuildingGame = LuaAPI.lua_toboolean(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_AutoAssetPostprocessor(RealStatePtr L)
        {
		    try {
                
			    CFEngine.EngineUtility.AutoAssetPostprocessor = LuaAPI.lua_toboolean(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_tmpSkinRender(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFEngine.EngineUtility.tmpSkinRender = (System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<UnityEngine.SkinnedMeshRenderer>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_tmpObject(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFEngine.EngineUtility.tmpObject = (System.Collections.Generic.List<object>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<object>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_getAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFEngine.EngineUtility.getAudio = translator.GetDelegate<CFEngine.GetAudio>(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_returnAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFEngine.EngineUtility.returnAudio = translator.GetDelegate<CFEngine.ReturnAudio>(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

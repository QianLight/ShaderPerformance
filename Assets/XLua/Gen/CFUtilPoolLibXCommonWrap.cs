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
    public class CFUtilPoolLibXCommonWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFUtilPoolLib.XCommon);
			Utils.BeginObjectRegister(type, L, translator, 0, 54, 3, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "XHash", _m_XHash);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "XHashLowerRelpaceDot", _m_XHashLowerRelpaceDot);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Combine", _m_Combine);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsEqual", _m_IsEqual);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsLess", _m_IsLess);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Clamp", _m_Clamp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsGreater", _m_IsGreater);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsEqualLess", _m_IsEqualLess);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsEqualGreater", _m_IsEqualGreater);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetToken", _m_GetToken);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ProcessValueDamp", _m_ProcessValueDamp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ProcessValueEvenPace", _m_ProcessValueEvenPace);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsRectCycleCross", _m_IsRectCycleCross);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Intersection", _m_Intersection);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsLineSegmentCross", _m_IsLineSegmentCross);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SqrDist3DSegmentToSegment", _m_SqrDist3DSegmentToSegment);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Horizontal", _m_Horizontal);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AngleNormalize", _m_AngleNormalize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "HorizontalRotateVetor2", _m_HorizontalRotateVetor2);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "HorizontalRotateVetor3", _m_HorizontalRotateVetor3);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "TicksToSeconds", _m_TicksToSeconds);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SecondsToTicks", _m_SecondsToTicks);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AngleToFloat", _m_AngleToFloat);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AngleWithSign", _m_AngleWithSign);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "FloatToAngle", _m_FloatToAngle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "VectorToQuaternion", _m_VectorToQuaternion);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "FloatToQuaternion", _m_FloatToQuaternion);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RotateToGround", _m_RotateToGround);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Clockwise", _m_Clockwise);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsInRect", _m_IsInRect);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RandomPercentage", _m_RandomPercentage);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RandomFloat", _m_RandomFloat);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RandomInt", _m_RandomInt);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsInteger", _m_IsInteger);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetFloatingValue", _m_GetFloatingValue);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetSmoothFactor", _m_GetSmoothFactor);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetJumpForce", _m_GetJumpForce);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SecondsToString", _m_SecondsToString);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SecondsToStringWithHour", _m_SecondsToStringWithHour);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "FindChildRecursively", _m_FindChildRecursively);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanStringCombine", _m_CleanStringCombine);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetSharedStringBuilder", _m_GetSharedStringBuilder);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetString", _m_GetString);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AppendString", _m_AppendString);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "StringCombine", _m_StringCombine);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CombineAdd", _m_CombineAdd);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CombineSetHeigh", _m_CombineSetHeigh);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CombineGetHeigh", _m_CombineGetHeigh);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CombineSetLow", _m_CombineSetLow);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CombineGetLow", _m_CombineGetLow);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "EnableParticleRenderer", _m_EnableParticleRenderer);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "EnableParticle", _m_EnableParticle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Init", _m_Init);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Uninit", _m_Uninit);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "New_id", _g_get_New_id);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UniqueToken", _g_get_UniqueToken);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "FrameStep", _g_get_FrameStep);
            
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 12, 3, 2);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "InitFModBus", _m_InitFModBus_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PointToSegmentRecentPoint", _m_PointToSegmentRecentPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Line2Plane", _m_Line2Plane_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Expand", _m_Expand_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Format", _m_Format_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FormatToInt", _m_FormatToInt_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ConvertNumWithAdd", _m_ConvertNumWithAdd_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Instantiate", _m_Instantiate_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetCapacityValue", _m_GetCapacityValue_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ReplaceReturn", _m_ReplaceReturn_xlua_st_);
            
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "Far_Far_Away", CFUtilPoolLib.XCommon.Far_Far_Away);
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "XEps", _g_get_XEps);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "tmpParticle", _g_get_tmpParticle);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "tmpMeshRender", _g_get_tmpMeshRender);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "tmpParticle", _s_set_tmpParticle);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "tmpMeshRender", _s_set_tmpMeshRender);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFUtilPoolLib.XCommon gen_ret = new CFUtilPoolLib.XCommon();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_InitFModBus_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        CFUtilPoolLib.IXFmodBus gen_ret = CFUtilPoolLib.XCommon.InitFModBus(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_XHash(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    uint _hashA = LuaAPI.xlua_touint(L, 2);
                    uint _hashB = LuaAPI.xlua_touint(L, 3);
                    
                        uint gen_ret = gen_to_be_invoked.XHash( _hashA, _hashB );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _str = LuaAPI.lua_tostring(L, 2);
                    
                        uint gen_ret = gen_to_be_invoked.XHash( _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<System.Text.StringBuilder>(L, 2)) 
                {
                    System.Text.StringBuilder _str = (System.Text.StringBuilder)translator.GetObject(L, 2, typeof(System.Text.StringBuilder));
                    
                        uint gen_ret = gen_to_be_invoked.XHash( _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 2);
                    string _str = LuaAPI.lua_tostring(L, 3);
                    
                        uint gen_ret = gen_to_be_invoked.XHash( _hash, _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.XHash!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_XHashLowerRelpaceDot(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _hash = LuaAPI.xlua_touint(L, 2);
                    string _str = LuaAPI.lua_tostring(L, 3);
                    
                        uint gen_ret = gen_to_be_invoked.XHashLowerRelpaceDot( _hash, _str );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Combine(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    uint _a = LuaAPI.xlua_touint(L, 2);
                    uint _b = LuaAPI.xlua_touint(L, 3);
                    
                        ulong gen_ret = gen_to_be_invoked.Combine( _a, _b );
                        LuaAPI.lua_pushuint64(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    byte _heigh = (byte)LuaAPI.xlua_tointeger(L, 2);
                    byte _low = (byte)LuaAPI.xlua_tointeger(L, 3);
                    
                        ushort gen_ret = gen_to_be_invoked.Combine( _heigh, _low );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.Combine!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsEqual(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _a = (float)LuaAPI.lua_tonumber(L, 2);
                    float _b = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        bool gen_ret = gen_to_be_invoked.IsEqual( _a, _b );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLess(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _a = (float)LuaAPI.lua_tonumber(L, 2);
                    float _b = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        bool gen_ret = gen_to_be_invoked.IsLess( _a, _b );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Clamp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    int _value = LuaAPI.xlua_tointeger(L, 2);
                    int _min = LuaAPI.xlua_tointeger(L, 3);
                    int _max = LuaAPI.xlua_tointeger(L, 4);
                    
                        int gen_ret = gen_to_be_invoked.Clamp( _value, _min, _max );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    double _value = LuaAPI.lua_tonumber(L, 2);
                    double _min = LuaAPI.lua_tonumber(L, 3);
                    double _max = LuaAPI.lua_tonumber(L, 4);
                    
                        double gen_ret = gen_to_be_invoked.Clamp( _value, _min, _max );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.Clamp!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsGreater(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _a = (float)LuaAPI.lua_tonumber(L, 2);
                    float _b = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        bool gen_ret = gen_to_be_invoked.IsGreater( _a, _b );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsEqualLess(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _a = (float)LuaAPI.lua_tonumber(L, 2);
                    float _b = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        bool gen_ret = gen_to_be_invoked.IsEqualLess( _a, _b );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsEqualGreater(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _a = (float)LuaAPI.lua_tonumber(L, 2);
                    float _b = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        bool gen_ret = gen_to_be_invoked.IsEqualGreater( _a, _b );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetToken(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        uint gen_ret = gen_to_be_invoked.GetToken(  );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ProcessValueDamp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _values = (float)LuaAPI.lua_tonumber(L, 2);
                    float _target = (float)LuaAPI.lua_tonumber(L, 3);
                    float _factor = (float)LuaAPI.lua_tonumber(L, 4);
                    float _deltaT = (float)LuaAPI.lua_tonumber(L, 5);
                    
                    gen_to_be_invoked.ProcessValueDamp( ref _values, _target, ref _factor, _deltaT );
                    LuaAPI.lua_pushnumber(L, _values);
                        
                    LuaAPI.lua_pushnumber(L, _factor);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ProcessValueEvenPace(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _value = (float)LuaAPI.lua_tonumber(L, 2);
                    float _target = (float)LuaAPI.lua_tonumber(L, 3);
                    float _speed = (float)LuaAPI.lua_tonumber(L, 4);
                    float _deltaT = (float)LuaAPI.lua_tonumber(L, 5);
                    
                    gen_to_be_invoked.ProcessValueEvenPace( ref _value, _target, _speed, _deltaT );
                    LuaAPI.lua_pushnumber(L, _value);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsRectCycleCross(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _rectw = (float)LuaAPI.lua_tonumber(L, 2);
                    float _recth = (float)LuaAPI.lua_tonumber(L, 3);
                    UnityEngine.Vector3 _c;translator.Get(L, 4, out _c);
                    float _r = (float)LuaAPI.lua_tonumber(L, 5);
                    
                        bool gen_ret = gen_to_be_invoked.IsRectCycleCross( _rectw, _recth, _c, _r );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Intersection(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector2 _begin;translator.Get(L, 2, out _begin);
                    UnityEngine.Vector2 _end;translator.Get(L, 3, out _end);
                    UnityEngine.Vector2 _center;translator.Get(L, 4, out _center);
                    float _radius = (float)LuaAPI.lua_tonumber(L, 5);
                    float _t;
                    
                        bool gen_ret = gen_to_be_invoked.Intersection( _begin, _end, _center, _radius, out _t );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    LuaAPI.lua_pushnumber(L, _t);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsLineSegmentCross(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _p1;translator.Get(L, 2, out _p1);
                    UnityEngine.Vector3 _p2;translator.Get(L, 3, out _p2);
                    UnityEngine.Vector3 _q1;translator.Get(L, 4, out _q1);
                    UnityEngine.Vector3 _q2;translator.Get(L, 5, out _q2);
                    
                        bool gen_ret = gen_to_be_invoked.IsLineSegmentCross( _p1, _p2, _q1, _q2 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SqrDist3DSegmentToSegment(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _s1p0;translator.Get(L, 2, out _s1p0);
                    UnityEngine.Vector3 _s1p1;translator.Get(L, 3, out _s1p1);
                    UnityEngine.Vector3 _s2p0;translator.Get(L, 4, out _s2p0);
                    UnityEngine.Vector3 _s2p1;translator.Get(L, 5, out _s2p1);
                    
                        float gen_ret = gen_to_be_invoked.SqrDist3DSegmentToSegment( _s1p0, _s1p1, _s2p0, _s2p1 );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PointToSegmentRecentPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _point;translator.Get(L, 1, out _point);
                    UnityEngine.Vector3 _segmentP0;translator.Get(L, 2, out _segmentP0);
                    UnityEngine.Vector3 _segmentP1;translator.Get(L, 3, out _segmentP1);
                    
                        UnityEngine.Vector3 gen_ret = CFUtilPoolLib.XCommon.PointToSegmentRecentPoint( _point, _segmentP0, _segmentP1 );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Line2Plane_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _start;translator.Get(L, 1, out _start);
                    UnityEngine.Vector3 _dir;translator.Get(L, 2, out _dir);
                    UnityEngine.Vector3 _normal;translator.Get(L, 3, out _normal);
                    float _dist = (float)LuaAPI.lua_tonumber(L, 4);
                    UnityEngine.Vector3 _pos;translator.Get(L, 5, out _pos);
                    
                        float gen_ret = CFUtilPoolLib.XCommon.Line2Plane( ref _start, ref _dir, ref _normal, _dist, ref _pos );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    translator.PushUnityEngineVector3(L, _start);
                        translator.UpdateUnityEngineVector3(L, 1, _start);
                        
                    translator.PushUnityEngineVector3(L, _dir);
                        translator.UpdateUnityEngineVector3(L, 2, _dir);
                        
                    translator.PushUnityEngineVector3(L, _normal);
                        translator.UpdateUnityEngineVector3(L, 3, _normal);
                        
                    translator.PushUnityEngineVector3(L, _pos);
                        translator.UpdateUnityEngineVector3(L, 5, _pos);
                        
                    
                    
                    
                    return 5;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Horizontal(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    
                        UnityEngine.Vector3 gen_ret = gen_to_be_invoked.Horizontal( _v );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    
                    gen_to_be_invoked.Horizontal( ref _v );
                    translator.PushUnityEngineVector3(L, _v);
                        translator.UpdateUnityEngineVector3(L, 2, _v);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.Horizontal!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AngleNormalize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _basic = (float)LuaAPI.lua_tonumber(L, 2);
                    float _degree = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = gen_to_be_invoked.AngleNormalize( _basic, _degree );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HorizontalRotateVetor2(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector2>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Vector2 _v;translator.Get(L, 2, out _v);
                    float _degree = (float)LuaAPI.lua_tonumber(L, 3);
                    bool _normalized = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.Vector2 gen_ret = gen_to_be_invoked.HorizontalRotateVetor2( _v, _degree, _normalized );
                        translator.PushUnityEngineVector2(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector2>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Vector2 _v;translator.Get(L, 2, out _v);
                    float _degree = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        UnityEngine.Vector2 gen_ret = gen_to_be_invoked.HorizontalRotateVetor2( _v, _degree );
                        translator.PushUnityEngineVector2(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.HorizontalRotateVetor2!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HorizontalRotateVetor3(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    float _degree = (float)LuaAPI.lua_tonumber(L, 3);
                    bool _normalized = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.Vector3 gen_ret = gen_to_be_invoked.HorizontalRotateVetor3( _v, _degree, _normalized );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    float _degree = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        UnityEngine.Vector3 gen_ret = gen_to_be_invoked.HorizontalRotateVetor3( _v, _degree );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.HorizontalRotateVetor3!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TicksToSeconds(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    long _tick = LuaAPI.lua_toint64(L, 2);
                    
                        float gen_ret = gen_to_be_invoked.TicksToSeconds( _tick );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SecondsToTicks(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _time = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        long gen_ret = gen_to_be_invoked.SecondsToTicks( _time );
                        LuaAPI.lua_pushint64(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AngleToFloat(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _dir;translator.Get(L, 2, out _dir);
                    
                        float gen_ret = gen_to_be_invoked.AngleToFloat( _dir );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AngleWithSign(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _from;translator.Get(L, 2, out _from);
                    UnityEngine.Vector3 _to;translator.Get(L, 3, out _to);
                    
                        float gen_ret = gen_to_be_invoked.AngleWithSign( _from, _to );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FloatToAngle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _angle = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        UnityEngine.Vector3 gen_ret = gen_to_be_invoked.FloatToAngle( _angle );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_VectorToQuaternion(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _v;translator.Get(L, 2, out _v);
                    
                        UnityEngine.Quaternion gen_ret = gen_to_be_invoked.VectorToQuaternion( _v );
                        translator.PushUnityEngineQuaternion(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FloatToQuaternion(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _angle = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        UnityEngine.Quaternion gen_ret = gen_to_be_invoked.FloatToQuaternion( _angle );
                        translator.PushUnityEngineQuaternion(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RotateToGround(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    UnityEngine.Vector3 _forward;translator.Get(L, 3, out _forward);
                    
                        UnityEngine.Quaternion gen_ret = gen_to_be_invoked.RotateToGround( _pos, _forward );
                        translator.PushUnityEngineQuaternion(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Clockwise(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& translator.Assignable<UnityEngine.Vector3>(L, 3)) 
                {
                    UnityEngine.Vector3 _fiduciary;translator.Get(L, 2, out _fiduciary);
                    UnityEngine.Vector3 _relativity;translator.Get(L, 3, out _relativity);
                    
                        bool gen_ret = gen_to_be_invoked.Clockwise( _fiduciary, _relativity );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Vector2>(L, 2)&& translator.Assignable<UnityEngine.Vector2>(L, 3)) 
                {
                    UnityEngine.Vector2 _fiduciary;translator.Get(L, 2, out _fiduciary);
                    UnityEngine.Vector2 _relativity;translator.Get(L, 3, out _relativity);
                    
                        bool gen_ret = gen_to_be_invoked.Clockwise( _fiduciary, _relativity );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.Clockwise!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsInRect(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _point;translator.Get(L, 2, out _point);
                    UnityEngine.Rect _rect;translator.Get(L, 3, out _rect);
                    UnityEngine.Vector3 _center;translator.Get(L, 4, out _center);
                    UnityEngine.Quaternion _rotation;translator.Get(L, 5, out _rotation);
                    
                        bool gen_ret = gen_to_be_invoked.IsInRect( _point, _rect, _center, _rotation );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RandomPercentage(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1) 
                {
                    
                        float gen_ret = gen_to_be_invoked.RandomPercentage(  );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    float _min = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        float gen_ret = gen_to_be_invoked.RandomPercentage( _min );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.RandomPercentage!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RandomFloat(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    float _max = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        float gen_ret = gen_to_be_invoked.RandomFloat( _max );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    float _min = (float)LuaAPI.lua_tonumber(L, 2);
                    float _max = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = gen_to_be_invoked.RandomFloat( _min, _max );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.RandomFloat!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RandomInt(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1) 
                {
                    
                        int gen_ret = gen_to_be_invoked.RandomInt(  );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    int _max = LuaAPI.xlua_tointeger(L, 2);
                    
                        int gen_ret = gen_to_be_invoked.RandomInt( _max );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    int _min = LuaAPI.xlua_tointeger(L, 2);
                    int _max = LuaAPI.xlua_tointeger(L, 3);
                    
                        int gen_ret = gen_to_be_invoked.RandomInt( _min, _max );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.RandomInt!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsInteger(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _n = (float)LuaAPI.lua_tonumber(L, 2);
                    
                        bool gen_ret = gen_to_be_invoked.IsInteger( _n );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetFloatingValue(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _value = (float)LuaAPI.lua_tonumber(L, 2);
                    float _floating = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = gen_to_be_invoked.GetFloatingValue( _value, _floating );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSmoothFactor(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _distance = (float)LuaAPI.lua_tonumber(L, 2);
                    float _timespan = (float)LuaAPI.lua_tonumber(L, 3);
                    float _nearenough = (float)LuaAPI.lua_tonumber(L, 4);
                    
                        float gen_ret = gen_to_be_invoked.GetSmoothFactor( _distance, _timespan, _nearenough );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetJumpForce(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _airTime = (float)LuaAPI.lua_tonumber(L, 2);
                    float _g = (float)LuaAPI.lua_tonumber(L, 3);
                    
                        float gen_ret = gen_to_be_invoked.GetJumpForce( _airTime, _g );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SecondsToString(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _time = LuaAPI.xlua_tointeger(L, 2);
                    
                        string gen_ret = gen_to_be_invoked.SecondsToString( _time );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SecondsToStringWithHour(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    ulong _time = LuaAPI.lua_touint64(L, 2);
                    
                        string gen_ret = gen_to_be_invoked.SecondsToStringWithHour( _time );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
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
                    
                        bool gen_ret = CFUtilPoolLib.XCommon.Expand( ref _src, _minx, _miny, _maxx, _maxy );
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
        static int _m_FindChildRecursively(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _name = LuaAPI.lua_tostring(L, 3);
                    
                        UnityEngine.Transform gen_ret = gen_to_be_invoked.FindChildRecursively( _t, _name );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanStringCombine(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CleanStringCombine(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSharedStringBuilder(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Text.StringBuilder gen_ret = gen_to_be_invoked.GetSharedStringBuilder(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetString(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        string gen_ret = gen_to_be_invoked.GetString(  );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AppendString(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s = LuaAPI.lua_tostring(L, 2);
                    
                        CFUtilPoolLib.XCommon gen_ret = gen_to_be_invoked.AppendString( _s );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    
                        CFUtilPoolLib.XCommon gen_ret = gen_to_be_invoked.AppendString( _s0, _s1 );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    
                        CFUtilPoolLib.XCommon gen_ret = gen_to_be_invoked.AppendString( _s0, _s1, _s2 );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.AppendString!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_StringCombine(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1, _s2 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 5&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    string _s3 = LuaAPI.lua_tostring(L, 5);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1, _s2, _s3 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    string _s3 = LuaAPI.lua_tostring(L, 5);
                    string _s4 = LuaAPI.lua_tostring(L, 6);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1, _s2, _s3, _s4 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 7&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 7) || LuaAPI.lua_type(L, 7) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    string _s3 = LuaAPI.lua_tostring(L, 5);
                    string _s4 = LuaAPI.lua_tostring(L, 6);
                    string _s5 = LuaAPI.lua_tostring(L, 7);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1, _s2, _s3, _s4, _s5 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 8&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 7) || LuaAPI.lua_type(L, 7) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 8) || LuaAPI.lua_type(L, 8) == LuaTypes.LUA_TSTRING)) 
                {
                    string _s0 = LuaAPI.lua_tostring(L, 2);
                    string _s1 = LuaAPI.lua_tostring(L, 3);
                    string _s2 = LuaAPI.lua_tostring(L, 4);
                    string _s3 = LuaAPI.lua_tostring(L, 5);
                    string _s4 = LuaAPI.lua_tostring(L, 6);
                    string _s5 = LuaAPI.lua_tostring(L, 7);
                    string _s6 = LuaAPI.lua_tostring(L, 8);
                    
                        string gen_ret = gen_to_be_invoked.StringCombine( _s0, _s1, _s2, _s3, _s4, _s5, _s6 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XCommon.StringCombine!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CombineAdd(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _value = LuaAPI.xlua_touint(L, 2);
                    int _heigh = LuaAPI.xlua_tointeger(L, 3);
                    int _low = LuaAPI.xlua_tointeger(L, 4);
                    
                        uint gen_ret = gen_to_be_invoked.CombineAdd( _value, _heigh, _low );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CombineSetHeigh(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _value = LuaAPI.xlua_touint(L, 2);
                    uint _heigh = LuaAPI.xlua_touint(L, 3);
                    
                    gen_to_be_invoked.CombineSetHeigh( ref _value, _heigh );
                    LuaAPI.xlua_pushuint(L, _value);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CombineGetHeigh(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _value = LuaAPI.xlua_touint(L, 2);
                    
                        ushort gen_ret = gen_to_be_invoked.CombineGetHeigh( _value );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CombineSetLow(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _value = LuaAPI.xlua_touint(L, 2);
                    uint _low = LuaAPI.xlua_touint(L, 3);
                    
                    gen_to_be_invoked.CombineSetLow( ref _value, _low );
                    LuaAPI.xlua_pushuint(L, _value);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CombineGetLow(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _value = LuaAPI.xlua_touint(L, 2);
                    
                        ushort gen_ret = gen_to_be_invoked.CombineGetLow( _value );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EnableParticleRenderer(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
                    bool _enable = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.EnableParticleRenderer( _go, _enable );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EnableParticle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
                    bool _enable = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.EnableParticle( _go, _enable );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Format_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    object[] _param = translator.GetParams<object>(L, 2);
                    
                        string gen_ret = CFUtilPoolLib.XCommon.Format( _value, _param );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FormatToInt_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _value = LuaAPI.lua_tostring(L, 1);
                    double _value2 = LuaAPI.lua_tonumber(L, 2);
                    
                        string gen_ret = CFUtilPoolLib.XCommon.FormatToInt( _value, _value2 );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ConvertNumWithAdd_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _num = LuaAPI.xlua_tointeger(L, 1);
                    bool _isShowZero = LuaAPI.lua_toboolean(L, 2);
                    
                        string gen_ret = CFUtilPoolLib.XCommon.ConvertNumWithAdd( _num, _isShowZero );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
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
            
            
            
                
                {
                    UnityEngine.Object _original = (UnityEngine.Object)translator.GetObject(L, 1, typeof(UnityEngine.Object));
                    
                        UnityEngine.Object gen_ret = CFUtilPoolLib.XCommon.Instantiate( _original );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetCapacityValue_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    long _capacity = LuaAPI.lua_toint64(L, 1);
                    
                        string gen_ret = CFUtilPoolLib.XCommon.GetCapacityValue( _capacity );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ReplaceReturn_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = CFUtilPoolLib.XCommon.ReplaceReturn( _str );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Init(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        bool gen_ret = gen_to_be_invoked.Init(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Uninit(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Uninit(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_XEps(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.lua_pushnumber(L, CFUtilPoolLib.XCommon.XEps);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_New_id(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.New_id);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UniqueToken(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.UniqueToken);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_FrameStep(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFUtilPoolLib.XCommon gen_to_be_invoked = (CFUtilPoolLib.XCommon)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.FrameStep);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_tmpParticle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFUtilPoolLib.XCommon.tmpParticle);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_tmpMeshRender(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFUtilPoolLib.XCommon.tmpMeshRender);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_tmpParticle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFUtilPoolLib.XCommon.tmpParticle = (System.Collections.Generic.List<UnityEngine.ParticleSystem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<UnityEngine.ParticleSystem>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_tmpMeshRender(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    CFUtilPoolLib.XCommon.tmpMeshRender = (System.Collections.Generic.List<UnityEngine.MeshRenderer>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<UnityEngine.MeshRenderer>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

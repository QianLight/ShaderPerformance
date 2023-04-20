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
    public class UnityEngineCFEventSystemsUIBehaviourWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFEventSystems.UIBehaviour);
			Utils.BeginObjectRegister(type, L, translator, 0, 6, 5, 4);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsActive", _m_IsActive);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DelayCallTransformParentChanged", _m_DelayCallTransformParentChanged);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetGrey", _m_SetGrey);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetDark", _m_SetDark);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "IsDestroyed", _m_IsDestroyed);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerClickEvent", _m_RegisterPointerClickEvent);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "RT", _g_get_RT);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ID", _g_get_ID);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Data", _g_get_Data);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_shieldFlag", _g_get_m_shieldFlag);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_shieldCode", _g_get_m_shieldCode);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "ID", _s_set_ID);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Data", _s_set_Data);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_shieldFlag", _s_set_m_shieldFlag);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_shieldCode", _s_set_m_shieldCode);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 2, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "Get", _m_Get_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "UnityEngine.CFEventSystems.UIBehaviour does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsActive(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        bool gen_ret = gen_to_be_invoked.IsActive(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DelayCallTransformParentChanged(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.DelayCallTransformParentChanged(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetGrey(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _grey = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.SetGrey( _grey );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetDark(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _dark = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.SetDark( _dark );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsDestroyed(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        bool gen_ret = gen_to_be_invoked.IsDestroyed(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterPointerClickEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.UIBehaviour.VoidBehaviourDelegate _ubd = translator.GetDelegate<UnityEngine.CFEventSystems.UIBehaviour.VoidBehaviourDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterPointerClickEvent( _ubd );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Get_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.Transform gen_ret = UnityEngine.CFEventSystems.UIBehaviour.Get( _t, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_RT(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.RT);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ID(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushint64(L, gen_to_be_invoked.ID);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Data(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.Data);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_shieldFlag(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_shieldFlag);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_shieldCode(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.m_shieldCode);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ID(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ID = LuaAPI.lua_toint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Data(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Data = translator.GetObject(L, 2, typeof(object));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_shieldFlag(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                UnityEngine.CFEventSystems.ShieldFlag gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_shieldFlag = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_shieldCode(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFEventSystems.UIBehaviour gen_to_be_invoked = (UnityEngine.CFEventSystems.UIBehaviour)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_shieldCode = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

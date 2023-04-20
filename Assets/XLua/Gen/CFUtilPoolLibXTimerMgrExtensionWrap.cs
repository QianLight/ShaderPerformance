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
    public class CFUtilPoolLibXTimerMgrExtensionWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFUtilPoolLib.XTimerMgrExtension);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 3, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "SetTimer", _m_SetTimer_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "KillTimer", _m_KillTimer_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "CFUtilPoolLib.XTimerMgrExtension does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetTimer_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Action<object>>(L, 2)&& translator.Assignable<object>(L, 3)) 
                {
                    float _interval = (float)LuaAPI.lua_tonumber(L, 1);
                    System.Action<object> _handler = translator.GetDelegate<System.Action<object>>(L, 2);
                    object _param = translator.GetObject(L, 3, typeof(object));
                    
                        uint gen_ret = CFUtilPoolLib.XTimerMgrExtension.SetTimer( _interval, _handler, _param );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Action<object>>(L, 2)&& translator.Assignable<System.Action>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    float _interval = (float)LuaAPI.lua_tonumber(L, 1);
                    System.Action<object> _handler = translator.GetDelegate<System.Action<object>>(L, 2);
                    System.Action _exception = translator.GetDelegate<System.Action>(L, 3);
                    object _param = translator.GetObject(L, 4, typeof(object));
                    
                        uint gen_ret = CFUtilPoolLib.XTimerMgrExtension.SetTimer( _interval, _handler, _exception, _param );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFUtilPoolLib.XTimerMgrExtension.SetTimer!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_KillTimer_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _token = LuaAPI.xlua_touint(L, 1);
                    
                    CFUtilPoolLib.XTimerMgrExtension.KillTimer( _token );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}

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
    public class UnityEngineCFUICFToggleGroupWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFToggleGroup);
			Utils.BeginObjectRegister(type, L, translator, 0, 8, 1, 1);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "NotifyToggleOn", _m_NotifyToggleOn);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UnregisterToggle", _m_UnregisterToggle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterToggle", _m_RegisterToggle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AnyTogglesOn", _m_AnyTogglesOn);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ActiveToggles", _m_ActiveToggles);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetAllTogglesOff", _m_SetAllTogglesOff);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetToggleOn", _m_SetToggleOn);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetToggleUIOn", _m_SetToggleUIOn);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "allowSwitchOff", _g_get_allowSwitchOff);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "allowSwitchOff", _s_set_allowSwitchOff);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "UnityEngine.CFUI.CFToggleGroup does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_NotifyToggleOn(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFToggle _toggle = (UnityEngine.CFUI.CFToggle)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFToggle));
                    
                    gen_to_be_invoked.NotifyToggleOn( _toggle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UnregisterToggle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFToggle _toggle = (UnityEngine.CFUI.CFToggle)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFToggle));
                    
                    gen_to_be_invoked.UnregisterToggle( _toggle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterToggle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFToggle _toggle = (UnityEngine.CFUI.CFToggle)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFToggle));
                    
                    gen_to_be_invoked.RegisterToggle( _toggle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AnyTogglesOn(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        bool gen_ret = gen_to_be_invoked.AnyTogglesOn(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ActiveToggles(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Collections.Generic.IEnumerable<UnityEngine.CFUI.CFToggle> gen_ret = gen_to_be_invoked.ActiveToggles(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetAllTogglesOff(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.SetAllTogglesOff(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetToggleOn(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetToggleOn( _index );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetToggleUIOn(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    bool _ison = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.SetToggleUIOn( _index, _ison );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_allowSwitchOff(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.allowSwitchOff);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_allowSwitchOff(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFToggleGroup gen_to_be_invoked = (UnityEngine.CFUI.CFToggleGroup)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.allowSwitchOff = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

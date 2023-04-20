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
    public class UnityEngineCFUIXSRControlNormalWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.XSRControlNormal);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 2, 2);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Destroy", _m_Destroy);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "_temp", _g_get__temp);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "_tempParent", _g_get__tempParent);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "_temp", _s_set__temp);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "_tempParent", _s_set__tempParent);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 2 && translator.Assignable<UnityEngine.Transform>(L, 2))
				{
					UnityEngine.Transform _temp = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
					
					UnityEngine.CFUI.XSRControlNormal gen_ret = new UnityEngine.CFUI.XSRControlNormal(_temp);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				if(LuaAPI.lua_gettop(L) == 3 && translator.Assignable<UnityEngine.Transform>(L, 2) && translator.Assignable<UnityEngine.Transform>(L, 3))
				{
					UnityEngine.Transform _temp = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
					UnityEngine.Transform _tempParent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
					
					UnityEngine.CFUI.XSRControlNormal gen_ret = new UnityEngine.CFUI.XSRControlNormal(_temp, _tempParent);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XSRControlNormal constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Destroy(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XSRControlNormal gen_to_be_invoked = (UnityEngine.CFUI.XSRControlNormal)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Destroy(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get__temp(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XSRControlNormal gen_to_be_invoked = (UnityEngine.CFUI.XSRControlNormal)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked._temp);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get__tempParent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XSRControlNormal gen_to_be_invoked = (UnityEngine.CFUI.XSRControlNormal)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked._tempParent);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set__temp(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XSRControlNormal gen_to_be_invoked = (UnityEngine.CFUI.XSRControlNormal)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked._temp = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set__tempParent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XSRControlNormal gen_to_be_invoked = (UnityEngine.CFUI.XSRControlNormal)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked._tempParent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

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
    public class UnityEngineCFUICFLoopScrollRectControlWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFLoopScrollRectControl);
			Utils.BeginObjectRegister(type, L, translator, 0, 11, 1, 1);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetTotalSize", _m_GetTotalSize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetSkin", _m_GetSkin);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ReturnSkin", _m_ReturnSkin);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ProvideSkin", _m_ProvideSkin);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Destroy", _m_Destroy);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Rebuild", _m_Rebuild);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Clean", _m_Clean);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Register", _m_Register);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetItemCount", _m_SetItemCount);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetSkinSize", _m_GetSkinSize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanBuffer", _m_CleanBuffer);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "WrapContent", _g_get_WrapContent);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "WrapContent", _s_set_WrapContent);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "UnityEngine.CFUI.CFLoopScrollRectControl does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetTotalSize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        int gen_ret = gen_to_be_invoked.GetTotalSize(  );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkin(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                        UnityEngine.CFUI.ISkin gen_ret = gen_to_be_invoked.GetSkin( _index );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ReturnSkin(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.ISkin _go = (UnityEngine.CFUI.ISkin)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.ISkin));
                    
                    gen_to_be_invoked.ReturnSkin( _go );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ProvideSkin(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.ISkin _skin = (UnityEngine.CFUI.ISkin)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.ISkin));
                    int _index = LuaAPI.xlua_tointeger(L, 3);
                    
                    gen_to_be_invoked.ProvideSkin( _skin, _index );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Destroy(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Destroy(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Rebuild(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _refill = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Rebuild( _refill );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.Rebuild(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.CFLoopScrollRectControl.Rebuild!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Clean(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Clean(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Register(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.ISRWrapper _wrap = (UnityEngine.CFUI.ISRWrapper)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.ISRWrapper));
                    
                    gen_to_be_invoked.Register( _wrap );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemCount(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _value = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetItemCount( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkinSize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        float gen_ret = gen_to_be_invoked.GetSkinSize(  );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanBuffer(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CleanBuffer(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_WrapContent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.WrapContent);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_WrapContent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFLoopScrollRectControl gen_to_be_invoked = (UnityEngine.CFUI.CFLoopScrollRectControl)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.WrapContent = (UnityEngine.CFUI.ISRWrapper)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.ISRWrapper));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

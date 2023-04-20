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
    public class UnityEngineCFUIEmpty4RaycastWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.Empty4Raycast);
			Utils.BeginObjectRegister(type, L, translator, 0, 6, 1, 1);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanUp", _m_CleanUp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerClick", _m_OnPointerClick);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPoinnter", _m_RegisterPoinnter);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerClickEvent", _m_RegisterPointerClickEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerDown", _m_RegisterPointerDown);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerDown", _m_OnPointerDown);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "mRaycastPass", _g_get_mRaycastPass);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "mRaycastPass", _s_set_mRaycastPass);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 1, 1);
			
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "onClickFx", _g_get_onClickFx);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "onClickFx", _s_set_onClickFx);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "UnityEngine.CFUI.Empty4Raycast does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanUp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CleanUp(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnPointerClick(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerClick( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterPoinnter(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.Empty4Raycast.VoidCastPointer _pointer = translator.GetDelegate<UnityEngine.CFUI.Empty4Raycast.VoidCastPointer>(L, 2);
                    
                    gen_to_be_invoked.RegisterPoinnter( _pointer );
                    
                    
                    
                    return 0;
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
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.Empty4Raycast.VoidCastDelegate _click = translator.GetDelegate<UnityEngine.CFUI.Empty4Raycast.VoidCastDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterPointerClickEvent( _click );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterPointerDown(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.Empty4Raycast.VoidCastDelegate _pointerDown = translator.GetDelegate<UnityEngine.CFUI.Empty4Raycast.VoidCastDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterPointerDown( _pointerDown );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnPointerDown(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerDown( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onClickFx(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, UnityEngine.CFUI.Empty4Raycast.onClickFx);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mRaycastPass(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.mRaycastPass);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onClickFx(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    UnityEngine.CFUI.Empty4Raycast.onClickFx = translator.GetDelegate<UnityEngine.CFUI.CFButton.ButtonClickFxDelegate>(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mRaycastPass(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.Empty4Raycast gen_to_be_invoked = (UnityEngine.CFUI.Empty4Raycast)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mRaycastPass = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

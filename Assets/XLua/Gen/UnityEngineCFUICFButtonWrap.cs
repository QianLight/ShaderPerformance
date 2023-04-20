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
    public class UnityEngineCFUICFButtonWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFButton);
			Utils.BeginObjectRegister(type, L, translator, 0, 13, 6, 6);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanUp", _m_CleanUp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerClick", _m_OnPointerClick);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerClickEvent", _m_RegisterPointerClickEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerDownEvent", _m_RegisterPointerDownEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterPointerUpEvent", _m_RegisterPointerUpEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterOnDragEvent", _m_RegisterOnDragEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerDown", _m_OnPointerDown);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerUp", _m_OnPointerUp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetEnable", _m_SetEnable);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetVisible", _m_SetVisible);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CheckShieldFlag", _m_CheckShieldFlag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetRaycastEnable", _m_SetRaycastEnable);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Update", _m_Update);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "onClick", _g_get_onClick);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "triggerTime", _g_get_triggerTime);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_useAnimation", _g_get_m_useAnimation);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "disableAnimWhileDisable", _g_get_disableAnimWhileDisable);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ClickAudio", _g_get_ClickAudio);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ClickAudioEnable", _g_get_ClickAudioEnable);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "onClick", _s_set_onClick);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "triggerTime", _s_set_triggerTime);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_useAnimation", _s_set_m_useAnimation);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "disableAnimWhileDisable", _s_set_disableAnimWhileDisable);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ClickAudio", _s_set_ClickAudio);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ClickAudioEnable", _s_set_ClickAudioEnable);
            
			
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
            return LuaAPI.luaL_error(L, "UnityEngine.CFUI.CFButton does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanUp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
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
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
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
        static int _m_RegisterPointerClickEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
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
        static int _m_RegisterPointerDownEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFButton.VoidButtonDelegate _onClick = translator.GetDelegate<UnityEngine.CFUI.CFButton.VoidButtonDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterPointerDownEvent( _onClick );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterPointerUpEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFButton.VoidButtonDelegate _onClick = translator.GetDelegate<UnityEngine.CFUI.CFButton.VoidButtonDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterPointerUpEvent( _onClick );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterOnDragEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CFButton.VoidButtonDelegate _onDrag = translator.GetDelegate<UnityEngine.CFUI.CFButton.VoidButtonDelegate>(L, 2);
                    
                    gen_to_be_invoked.RegisterOnDragEvent( _onDrag );
                    
                    
                    
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
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
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
        static int _m_OnPointerUp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerUp( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetEnable(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _bEnable = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.SetEnable( _bEnable );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetVisible(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _active = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.SetVisible( _active );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckShieldFlag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CheckShieldFlag(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetRaycastEnable(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _bEnabled = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.SetRaycastEnable( _bEnabled );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Update(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Update(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onClick(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.onClick);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_triggerTime(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.triggerTime);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onClickFx(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, UnityEngine.CFUI.CFButton.onClickFx);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_useAnimation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.m_useAnimation);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_disableAnimWhileDisable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.disableAnimWhileDisable);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ClickAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.ClickAudio);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ClickAudioEnable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.ClickAudioEnable);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onClick(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.onClick = (UnityEngine.CFUI.CFButton.ButtonClickedEvent)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFButton.ButtonClickedEvent));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_triggerTime(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.triggerTime = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onClickFx(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    UnityEngine.CFUI.CFButton.onClickFx = translator.GetDelegate<UnityEngine.CFUI.CFButton.ButtonClickFxDelegate>(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_useAnimation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_useAnimation = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_disableAnimWhileDisable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.disableAnimWhileDisable = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ClickAudio(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ClickAudio = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ClickAudioEnable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFButton gen_to_be_invoked = (UnityEngine.CFUI.CFButton)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ClickAudioEnable = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

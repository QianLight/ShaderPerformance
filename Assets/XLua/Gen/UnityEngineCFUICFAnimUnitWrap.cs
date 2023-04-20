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
    public class UnityEngineCFUICFAnimUnitWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFAnimUnit);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 18, 17);
			
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_CanvasGroup", _g_get_m_CanvasGroup);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Target", _g_get_Target);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isActive", _g_get_isActive);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_Curve", _g_get_m_Curve);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_WhenFinished", _g_get_m_WhenFinished);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_Type", _g_get_m_Type);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_LoopType", _g_get_m_LoopType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_From", _g_get_m_From);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_To", _g_get_m_To);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_From_C", _g_get_m_From_C);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_To_C", _g_get_m_To_C);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_Duration", _g_get_m_Duration);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_Delay", _g_get_m_Delay);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IncludeChildren", _g_get_IncludeChildren);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "DoNotInitStartValue", _g_get_DoNotInitStartValue);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_IsPlaying", _g_get_m_IsPlaying);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "time", _g_get_time);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Forward", _g_get_Forward);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "Target", _s_set_Target);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "isActive", _s_set_isActive);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_Curve", _s_set_m_Curve);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_WhenFinished", _s_set_m_WhenFinished);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_Type", _s_set_m_Type);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_LoopType", _s_set_m_LoopType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_From", _s_set_m_From);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_To", _s_set_m_To);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_From_C", _s_set_m_From_C);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_To_C", _s_set_m_To_C);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_Duration", _s_set_m_Duration);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_Delay", _s_set_m_Delay);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IncludeChildren", _s_set_IncludeChildren);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "DoNotInitStartValue", _s_set_DoNotInitStartValue);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_IsPlaying", _s_set_m_IsPlaying);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "time", _s_set_time);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Forward", _s_set_Forward);
            
			
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
				if(LuaAPI.lua_gettop(L) == 2 && translator.Assignable<UnityEngine.CFUI.CFAnimUnit>(L, 2))
				{
					UnityEngine.CFUI.CFAnimUnit _bCopy = (UnityEngine.CFUI.CFAnimUnit)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFAnimUnit));
					
					UnityEngine.CFUI.CFAnimUnit gen_ret = new UnityEngine.CFUI.CFAnimUnit(_bCopy);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.CFUI.CFAnimUnit gen_ret = new UnityEngine.CFUI.CFAnimUnit();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.CFAnimUnit constructor!");
            
        }
        
		
        
		
        
        
        
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_CanvasGroup(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_CanvasGroup);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Target(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.Target);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isActive(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isActive);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_Curve(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_Curve);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_WhenFinished(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_WhenFinished);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_Type(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_Type);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_LoopType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_LoopType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_From(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineVector3(L, gen_to_be_invoked.m_From);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_To(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineVector3(L, gen_to_be_invoked.m_To);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_From_C(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineColor(L, gen_to_be_invoked.m_From_C);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_To_C(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineColor(L, gen_to_be_invoked.m_To_C);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_Duration(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.m_Duration);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_Delay(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.m_Delay);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IncludeChildren(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IncludeChildren);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_DoNotInitStartValue(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.DoNotInitStartValue);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_IsPlaying(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.m_IsPlaying);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_time(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.time);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Forward(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Forward);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Target(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Target = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_isActive(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.isActive = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_Curve(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_Curve = (UnityEngine.AnimationCurve)translator.GetObject(L, 2, typeof(UnityEngine.AnimationCurve));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_WhenFinished(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFAnimPlayFinish gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_WhenFinished = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_Type(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFAnimType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_Type = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_LoopType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFAnimLoopType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_LoopType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_From(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.Vector3 gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_From = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_To(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.Vector3 gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_To = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_From_C(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.Color gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_From_C = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_To_C(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.Color gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_To_C = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_Duration(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_Duration = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_Delay(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_Delay = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IncludeChildren(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IncludeChildren = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_DoNotInitStartValue(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.DoNotInitStartValue = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_IsPlaying(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_IsPlaying = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_time(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.time = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Forward(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.CFAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Forward = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

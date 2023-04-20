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
    public class UnityEngineCFUIXAnimUnitWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.XAnimUnit);
			Utils.BeginObjectRegister(type, L, translator, 0, 4, 16, 16);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Setup", _m_Setup);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Begin", _m_Begin);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ResetFunc", _m_ResetFunc);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_TargetType", _g_get_m_TargetType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Target", _g_get_Target);
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
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "func", _g_get_func);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "playForward", _g_get_playForward);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_TargetType", _s_set_m_TargetType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Target", _s_set_Target);
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
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "func", _s_set_func);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "playForward", _s_set_playForward);
            
			
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
				if(LuaAPI.lua_gettop(L) == 2 && translator.Assignable<UnityEngine.CFUI.XAnimUnit>(L, 2))
				{
					UnityEngine.CFUI.XAnimUnit _bCopy = (UnityEngine.CFUI.XAnimUnit)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XAnimUnit));
					
					UnityEngine.CFUI.XAnimUnit gen_ret = new UnityEngine.CFUI.XAnimUnit(_bCopy);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.CFUI.XAnimUnit gen_ret = new UnityEngine.CFUI.XAnimUnit();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XAnimUnit constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Setup(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Setup(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reset(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _forward = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Reset( _forward );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Begin(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _forward = LuaAPI.lua_toboolean(L, 2);
                    
                        bool gen_ret = gen_to_be_invoked.Begin( _forward );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ResetFunc(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ResetFunc(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_TargetType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_TargetType);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.Target);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.DoNotInitStartValue);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_func(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.func);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_playForward(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.playForward);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_TargetType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.XAnimTargetType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_TargetType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Target(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Target = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.XAnimPlayFinish gen_value;translator.Get(L, 2, out gen_value);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.XAnimType gen_value;translator.Get(L, 2, out gen_value);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.XAnimLoopType gen_value;translator.Get(L, 2, out gen_value);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.DoNotInitStartValue = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_func(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.func = (UnityEngine.CFUI.IAnimFun)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IAnimFun));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_playForward(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimUnit gen_to_be_invoked = (UnityEngine.CFUI.XAnimUnit)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.playForward = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

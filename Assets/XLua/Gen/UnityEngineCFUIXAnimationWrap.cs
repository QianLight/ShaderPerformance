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
    public class UnityEngineCFUIXAnimationWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.XAnimation);
			Utils.BeginObjectRegister(type, L, translator, 0, 13, 6, 6);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanUp", _m_CleanUp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Setup", _m_Setup);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PlayForward", _m_PlayForward);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetDelay", _m_SetDelay);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Stop", _m_Stop);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterAnimationFinish", _m_RegisterAnimationFinish);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CheckFinish", _m_CheckFinish);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RecalculateMasking", _m_RecalculateMasking);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SpeedPlay", _m_SpeedPlay);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CopyPlayRate", _m_CopyPlayRate);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetTotalTime", _m_GetTotalTime);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Find", _m_Find);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "PlayType", _g_get_PlayType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IgnoreTimeScale", _g_get_IgnoreTimeScale);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "AnimList", _g_get_AnimList);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "aniGroup", _g_get_aniGroup);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "maskUpdate", _g_get_maskUpdate);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "beEndStateWhileDisable", _g_get_beEndStateWhileDisable);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "PlayType", _s_set_PlayType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IgnoreTimeScale", _s_set_IgnoreTimeScale);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "AnimList", _s_set_AnimList);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "aniGroup", _s_set_aniGroup);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "maskUpdate", _s_set_maskUpdate);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "beEndStateWhileDisable", _s_set_beEndStateWhileDisable);
            
			
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
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.CFUI.XAnimation gen_ret = new UnityEngine.CFUI.XAnimation();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XAnimation constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanUp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CleanUp(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Setup(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Setup(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayForward(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _forward = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.PlayForward( _forward );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.PlayForward(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XAnimation.PlayForward!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetDelay(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _delay = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.SetDelay( _delay );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Stop(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    bool _reset = LuaAPI.lua_toboolean(L, 2);
                    bool _forward = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.Stop( _reset, _forward );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _reset = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Stop( _reset );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.Stop(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XAnimation.Stop!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reset(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _forward = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Reset( _forward );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.Reset(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XAnimation.Reset!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterAnimationFinish(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action _e = translator.GetDelegate<System.Action>(L, 2);
                    
                    gen_to_be_invoked.RegisterAnimationFinish( _e );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckFinish(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        bool gen_ret = gen_to_be_invoked.CheckFinish(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecalculateMasking(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RecalculateMasking(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SpeedPlay(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _rate = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    gen_to_be_invoked.SpeedPlay( _rate );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CopyPlayRate(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Collections.Generic.List<UnityEngine.CFUI.XAnimUnit> _targetList = (System.Collections.Generic.List<UnityEngine.CFUI.XAnimUnit>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<UnityEngine.CFUI.XAnimUnit>));
                    
                    gen_to_be_invoked.CopyPlayRate( _targetList );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetTotalTime(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        float gen_ret = gen_to_be_invoked.GetTotalTime(  );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Find(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _name = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.XAnimUnit gen_ret = gen_to_be_invoked.Find( _name );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_PlayType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.PlayType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IgnoreTimeScale(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IgnoreTimeScale);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_AnimList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.AnimList);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_aniGroup(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushuint(L, gen_to_be_invoked.aniGroup);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_maskUpdate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.maskUpdate);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_beEndStateWhileDisable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.beEndStateWhileDisable);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_PlayType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.XAnimPlayType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.PlayType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IgnoreTimeScale(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IgnoreTimeScale = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_AnimList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.AnimList = (System.Collections.Generic.List<UnityEngine.CFUI.XAnimUnit>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<UnityEngine.CFUI.XAnimUnit>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_aniGroup(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.aniGroup = LuaAPI.xlua_touint(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_maskUpdate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.maskUpdate = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_beEndStateWhileDisable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XAnimation gen_to_be_invoked = (UnityEngine.CFUI.XAnimation)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.beEndStateWhileDisable = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

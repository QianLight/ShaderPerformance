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
    public class UnityEngineCFUICFWrapContentWithSkinWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFWrapContentWithSkin);
			Utils.BeginObjectRegister(type, L, translator, 0, 13, 7, 5);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Register", _m_Register);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetSkins", _m_GetSkins);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetItemCount", _m_SetItemCount);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Rebuild", _m_Rebuild);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CalTotalCount", _m_CalTotalCount);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RefreshCells", _m_RefreshCells);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RefreshCellsInView", _m_RefreshCellsInView);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RefillCells", _m_RefillCells);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ScrollToCell", _m_ScrollToCell);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateSkinByIndex", _m_UpdateSkinByIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Clean", _m_Clean);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CleanUp", _m_CleanUp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ClearSkins", _m_ClearSkins);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsInited", _g_get_IsInited);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Control", _g_get_Control);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_controlType", _g_get_m_controlType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "_temp", _g_get__temp);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "_tempParent", _g_get__tempParent);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "autoRefreshWhenStart", _g_get_autoRefreshWhenStart);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "atOnce", _g_get_atOnce);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_controlType", _s_set_m_controlType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "_temp", _s_set__temp);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "_tempParent", _s_set__tempParent);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "autoRefreshWhenStart", _s_set_autoRefreshWhenStart);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "atOnce", _s_set_atOnce);
            
			
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
					
					UnityEngine.CFUI.CFWrapContentWithSkin gen_ret = new UnityEngine.CFUI.CFWrapContentWithSkin();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.CFWrapContentWithSkin constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Register(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.ISRControl _control = (UnityEngine.CFUI.ISRControl)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.ISRControl));
                    
                    gen_to_be_invoked.Register( _control );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkins(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Collections.Generic.List<UnityEngine.CFUI.ISkin> gen_ret = gen_to_be_invoked.GetSkins(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
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
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
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
        static int _m_Rebuild(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    bool _fill = LuaAPI.lua_toboolean(L, 2);
                    int _startIndex = LuaAPI.xlua_tointeger(L, 3);
                    
                    gen_to_be_invoked.Rebuild( _fill, _startIndex );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _fill = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Rebuild( _fill );
                    
                    
                    
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
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.CFWrapContentWithSkin.Rebuild!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CalTotalCount(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CalTotalCount(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RefreshCells(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    int _startIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.RefreshCells( _startIndex );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.RefreshCells(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.CFWrapContentWithSkin.RefreshCells!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RefreshCellsInView(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RefreshCellsInView(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RefillCells(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RefillCells(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ScrollToCell(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _startindex = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.ScrollToCell( _startindex );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateSkinByIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.UpdateSkinByIndex( _index );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Clean(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Clean(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanUp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CleanUp(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearSkins(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ClearSkins(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsInited(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsInited);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Control(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.Control);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_controlType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_controlType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get__temp(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked._tempParent);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_autoRefreshWhenStart(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.autoRefreshWhenStart);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_atOnce(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.atOnce);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_controlType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.ScrollRectControlType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.m_controlType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set__temp(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
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
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked._tempParent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_autoRefreshWhenStart(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.autoRefreshWhenStart = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_atOnce(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFWrapContentWithSkin gen_to_be_invoked = (UnityEngine.CFUI.CFWrapContentWithSkin)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.atOnce = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

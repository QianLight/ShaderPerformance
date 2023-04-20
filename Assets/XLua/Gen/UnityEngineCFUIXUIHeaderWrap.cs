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
    public class UnityEngineCFUIXUIHeaderWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.XUIHeader);
			Utils.BeginObjectRegister(type, L, translator, 0, 27, 20, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetHandle", _m_SetHandle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnInitialize", _m_SetOnInitialize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnShow", _m_SetOnShow);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnRender", _m_SetOnRender);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnFlushData", _m_SetOnFlushData);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnRecycle", _m_SetOnRecycle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnHide", _m_SetOnHide);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnFlushRedPoint", _m_SetOnFlushRedPoint);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetOnHandlerNotification", _m_SetOnHandlerNotification);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Create", _m_Create);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetLocalPositionAndScale", _m_SetLocalPositionAndScale);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Setup", _m_Setup);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetContext", _m_SetContext);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ClearContext", _m_ClearContext);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BeginLoad", _m_BeginLoad);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PreEnable", _m_PreEnable);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PreLoad", _m_PreLoad);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Close", _m_Close);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ToggleGameObject", _m_ToggleGameObject);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Render", _m_Render);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Flush", _m_Flush);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "FlushRedPoint", _m_FlushRedPoint);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PreLoaded", _m_PreLoaded);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetParent", _m_SetParent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetSiblingIndex", _m_SetSiblingIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Clean", _m_Clean);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Recycle", _m_Recycle);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "HandleList", _g_get_HandleList);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnInitialize", _g_get_OnInitialize);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnShow", _g_get_OnShow);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnRender", _g_get_OnRender);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnFlushRedPoint", _g_get_OnFlushRedPoint);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnFlushData", _g_get_OnFlushData);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnHide", _g_get_OnHide);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnRecycle", _g_get_OnRecycle);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "OnHandlerNotification", _g_get_OnHandlerNotification);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Context", _g_get_Context);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "hashCode", _g_get_hashCode);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "gameObject", _g_get_gameObject);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "transform", _g_get_transform);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "holder", _g_get_holder);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "rectTransform", _g_get_rectTransform);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "enable", _g_get_enable);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsShow", _g_get_IsShow);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "uiBase", _g_get_uiBase);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Override", _g_get_Override);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "filename", _g_get_filename);
            
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 3, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "ExecuteMethod", _m_ExecuteMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ReturnHeader", _m_ReturnHeader_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.CFUI.XUIHeader gen_ret = new UnityEngine.CFUI.XUIHeader();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XUIHeader constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetHandle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint[] _handle = translator.GetParams<uint>(L, 2);
                    
                    gen_to_be_invoked.SetHandle( _handle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ExecuteMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.CFUI.XUIFuncs _method = (UnityEngine.CFUI.XUIFuncs)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.XUIFuncs));
                    
                    UnityEngine.CFUI.XUIHeader.ExecuteMethod( _method );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnInitialize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction _value = translator.GetDelegate<UnityEngine.Events.UnityAction>(L, 2);
                    
                    gen_to_be_invoked.SetOnInitialize( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnShow(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction _value = translator.GetDelegate<UnityEngine.Events.UnityAction>(L, 2);
                    
                    gen_to_be_invoked.SetOnShow( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnRender(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction<bool> _value = translator.GetDelegate<UnityEngine.Events.UnityAction<bool>>(L, 2);
                    
                    gen_to_be_invoked.SetOnRender( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnFlushData(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction<object> _value = translator.GetDelegate<UnityEngine.Events.UnityAction<object>>(L, 2);
                    
                    gen_to_be_invoked.SetOnFlushData( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnRecycle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction _value = translator.GetDelegate<UnityEngine.Events.UnityAction>(L, 2);
                    
                    gen_to_be_invoked.SetOnRecycle( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnHide(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction _value = translator.GetDelegate<UnityEngine.Events.UnityAction>(L, 2);
                    
                    gen_to_be_invoked.SetOnHide( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnFlushRedPoint(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction _value = translator.GetDelegate<UnityEngine.Events.UnityAction>(L, 2);
                    
                    gen_to_be_invoked.SetOnFlushRedPoint( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetOnHandlerNotification(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Events.UnityAction<object> _value = translator.GetDelegate<UnityEngine.Events.UnityAction<object>>(L, 2);
                    
                    gen_to_be_invoked.SetOnHandlerNotification( _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ReturnHeader_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.CFUI.XUIHeader _header = (UnityEngine.CFUI.XUIHeader)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.XUIHeader));
                    
                    UnityEngine.CFUI.XUIHeader.ReturnHeader( _header );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Create(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    uint _hashCode = LuaAPI.xlua_touint(L, 2);
                    UnityEngine.CFUI.IUIDisplay _uiBase = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 3, typeof(UnityEngine.CFUI.IUIDisplay));
                    
                    gen_to_be_invoked.Create( _hashCode, _uiBase );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetLocalPositionAndScale(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    UnityEngine.Vector3 _scale;translator.Get(L, 3, out _scale);
                    
                    gen_to_be_invoked.SetLocalPositionAndScale( _pos, _scale );
                    
                    
                    
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
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _filename = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.Setup( _filename );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetContext(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.XDisplayContext _context = (UnityEngine.CFUI.XDisplayContext)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XDisplayContext));
                    
                    gen_to_be_invoked.SetContext( _context );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearContext(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ClearContext(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginLoad(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _mm = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.BeginLoad( _mm );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PreEnable(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.PreEnable(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PreLoad(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<CFEngine.AssetHandler>(L, 2)) 
                {
                    CFEngine.AssetHandler _asset = (CFEngine.AssetHandler)translator.GetObject(L, 2, typeof(CFEngine.AssetHandler));
                    
                    gen_to_be_invoked.PreLoad( _asset );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.GameObject>(L, 2)) 
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
                    
                    gen_to_be_invoked.PreLoad( _go );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XUIHeader.PreLoad!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Close(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Close(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToggleGameObject(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _active = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.ToggleGameObject( _active );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Render(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    bool _bShow = LuaAPI.lua_toboolean(L, 2);
                    bool _self = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.Render( _bShow, _self );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _bShow = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.Render( _bShow );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XUIHeader.Render!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Flush(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.CFUI.XDisplayContext>(L, 2)) 
                {
                    UnityEngine.CFUI.XDisplayContext _context = (UnityEngine.CFUI.XDisplayContext)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XDisplayContext));
                    
                    gen_to_be_invoked.Flush( _context );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.Flush(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.XUIHeader.Flush!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FlushRedPoint(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.FlushRedPoint(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PreLoaded(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.PreLoaded(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetParent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                    gen_to_be_invoked.SetParent( _parent );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetSiblingIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetSiblingIndex( _index );
                    
                    
                    
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
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Clean(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Recycle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Recycle(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_HandleList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.HandleList);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnInitialize(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnInitialize);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnShow(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnShow);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnRender(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnRender);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnFlushRedPoint(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnFlushRedPoint);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnFlushData(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnFlushData);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnHide(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnHide);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnRecycle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnRecycle);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_OnHandlerNotification(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.OnHandlerNotification);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Context(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.Context);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_hashCode(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.hashCode);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_gameObject(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.gameObject);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_transform(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.transform);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_holder(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.holder);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_rectTransform(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.rectTransform);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_enable(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.enable);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsShow(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsShow);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_uiBase(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.uiBase);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Override(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.Override);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_filename(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.XUIHeader gen_to_be_invoked = (UnityEngine.CFUI.XUIHeader)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.filename);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
		
		
		
		
    }
}

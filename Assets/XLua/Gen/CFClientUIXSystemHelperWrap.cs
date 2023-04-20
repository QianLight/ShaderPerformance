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
    public class CFClientUIXSystemHelperWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFClient.UI.XSystemHelper);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 13, 1, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckJoinGuild", _m_CheckJoinGuild_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckPrefabOpended", _m_CheckPrefabOpended_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "HasSystemRedPoint", _m_HasSystemRedPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetSysRedPoint", _m_SetSysRedPoint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "UpdateRedPointState", _m_UpdateRedPointState_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "AddVerifier", _m_AddVerifier_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "AddVerifierByPrefabID", _m_AddVerifierByPrefabID_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "AddVerifierBySystemID", _m_AddVerifierBySystemID_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RemoveVerifier", _m_RemoveVerifier_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckSystemOpened", _m_CheckSystemOpened_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "JumpToOtherSystem", _m_JumpToOtherSystem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckShield", _m_CheckShield_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "PlayerData", _g_get_PlayerData);
            
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFClient.UI.XSystemHelper gen_ret = new CFClient.UI.XSystemHelper();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSystemHelper constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckJoinGuild_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.CheckJoinGuild(  );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckPrefabOpended_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    bool _showtips = LuaAPI.lua_toboolean(L, 2);
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.CheckPrefabOpended( _prefabID, _showtips );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HasSystemRedPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _sys = LuaAPI.xlua_touint(L, 1);
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.HasSystemRedPoint( _sys );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetSysRedPoint_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    uint _sys = LuaAPI.xlua_touint(L, 1);
                    bool _state = LuaAPI.lua_toboolean(L, 2);
                    bool _refreshImm = LuaAPI.lua_toboolean(L, 3);
                    
                    CFClient.UI.XSystemHelper.SetSysRedPoint( _sys, _state, _refreshImm );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    uint _sys = LuaAPI.xlua_touint(L, 1);
                    bool _state = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XSystemHelper.SetSysRedPoint( _sys, _state );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSystemHelper.SetSysRedPoint!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateRedPointState_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _sys = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XSystemHelper.UpdateRedPointState( _sys );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddVerifier_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.IXVerifier _verifier = (CFClient.UI.IXVerifier)translator.GetObject(L, 1, typeof(CFClient.UI.IXVerifier));
                    
                    CFClient.UI.XSystemHelper.AddVerifier( _verifier );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddVerifierByPrefabID_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<uint, uint>>(L, 4)) 
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    bool _conver = LuaAPI.lua_toboolean(L, 3);
                    System.Action<uint, uint> _message = translator.GetDelegate<System.Action<uint, uint>>(L, 4);
                    
                    CFClient.UI.XSystemHelper.AddVerifierByPrefabID( _prefabID, _verifier, _conver, _message );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    bool _conver = LuaAPI.lua_toboolean(L, 3);
                    
                    CFClient.UI.XSystemHelper.AddVerifierByPrefabID( _prefabID, _verifier, _conver );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)) 
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    
                    CFClient.UI.XSystemHelper.AddVerifierByPrefabID( _prefabID, _verifier );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSystemHelper.AddVerifierByPrefabID!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddVerifierBySystemID_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<uint, uint>>(L, 4)) 
                {
                    uint _systemid = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    bool _conver = LuaAPI.lua_toboolean(L, 3);
                    System.Action<uint, uint> _message = translator.GetDelegate<System.Action<uint, uint>>(L, 4);
                    
                    CFClient.UI.XSystemHelper.AddVerifierBySystemID( _systemid, _verifier, _conver, _message );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    uint _systemid = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    bool _conver = LuaAPI.lua_toboolean(L, 3);
                    
                    CFClient.UI.XSystemHelper.AddVerifierBySystemID( _systemid, _verifier, _conver );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Func<uint, uint>>(L, 2)) 
                {
                    uint _systemid = LuaAPI.xlua_touint(L, 1);
                    System.Func<uint, uint> _verifier = translator.GetDelegate<System.Func<uint, uint>>(L, 2);
                    
                    CFClient.UI.XSystemHelper.AddVerifierBySystemID( _systemid, _verifier );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSystemHelper.AddVerifierBySystemID!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RemoveVerifier_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XSystemHelper.RemoveVerifier( _prefabID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckSystemOpened_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    uint _systemID = LuaAPI.xlua_touint(L, 1);
                    bool _showTips = LuaAPI.lua_toboolean(L, 2);
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.CheckSystemOpened( _systemID, _showTips );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _systemID = LuaAPI.xlua_touint(L, 1);
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.CheckSystemOpened( _systemID );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSystemHelper.CheckSystemOpened!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_JumpToOtherSystem_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _tableString = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XSystemHelper.JumpToOtherSystem( _tableString );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckShield_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _code = LuaAPI.xlua_touint(L, 1);
                    
                        bool gen_ret = CFClient.UI.XSystemHelper.CheckShield( _code );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_PlayerData(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, CFClient.UI.XSystemHelper.PlayerData);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
		
		
		
		
    }
}

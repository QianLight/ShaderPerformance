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
    public class UnityEngineCFUIUIUtilsWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.UIUtils);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 12, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "PlayUISfx", _m_PlayUISfx_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlaySfx", _m_PlaySfx_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DestroyUISfx", _m_DestroyUISfx_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlaySfx_Aux", _m_PlaySfx_Aux_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DestroySfx_Aux", _m_DestroySfx_Aux_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateObject", _m_CreateObject_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "TimeFormatString", _m_TimeFormatString_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "TimeFormatStringWithoutSecond", _m_TimeFormatStringWithoutSecond_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "TimeFormatStringWithoutDay", _m_TimeFormatStringWithoutDay_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetUniversalTime", _m_GetUniversalTime_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlaySpriteAnimation", _m_PlaySpriteAnimation_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					UnityEngine.CFUI.UIUtils gen_ret = new UnityEngine.CFUI.UIUtils();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayUISfx_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.LoadSFXFinish>(L, 5)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    string _childPath = LuaAPI.lua_tostring(L, 4);
                    CFEngine.LoadSFXFinish _cb = translator.GetDelegate<CFEngine.LoadSFXFinish>(L, 5);
                    
                    UnityEngine.CFUI.UIUtils.PlayUISfx( ref _sfx, _name, _parent, _childPath, _cb );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    string _childPath = LuaAPI.lua_tostring(L, 4);
                    
                    UnityEngine.CFUI.UIUtils.PlayUISfx( ref _sfx, _name, _parent, _childPath );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                    UnityEngine.CFUI.UIUtils.PlayUISfx( ref _sfx, _name, _parent );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Vector3>(L, 5)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _offset;translator.Get(L, 4, out _offset);
                    UnityEngine.Vector3 _scale;translator.Get(L, 5, out _scale);
                    string _childPath = LuaAPI.lua_tostring(L, 6);
                    
                    UnityEngine.CFUI.UIUtils.PlayUISfx( ref _sfx, _name, _parent, _offset, _scale, _childPath );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 5&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Vector3>(L, 5)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _offset;translator.Get(L, 4, out _offset);
                    UnityEngine.Vector3 _scale;translator.Get(L, 5, out _scale);
                    
                    UnityEngine.CFUI.UIUtils.PlayUISfx( ref _sfx, _name, _parent, _offset, _scale );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.PlayUISfx!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlaySfx_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& translator.Assignable<CFEngine.LoadSFXFinish>(L, 5)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    string _childPath = LuaAPI.lua_tostring(L, 4);
                    CFEngine.LoadSFXFinish _cb = translator.GetDelegate<CFEngine.LoadSFXFinish>(L, 5);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx( ref _sfx, _name, _parent, _childPath, _cb );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    string _childPath = LuaAPI.lua_tostring(L, 4);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx( ref _sfx, _name, _parent, _childPath );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx( ref _sfx, _name, _parent );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.PlaySfx!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DestroyUISfx_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<CFEngine.SFX>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    bool _immediate = LuaAPI.lua_toboolean(L, 2);
                    
                    UnityEngine.CFUI.UIUtils.DestroyUISfx( ref _sfx, _immediate );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<CFEngine.SFX>(L, 1)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    
                    UnityEngine.CFUI.UIUtils.DestroyUISfx( ref _sfx );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.DestroyUISfx!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlaySfx_Aux_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    float _duration = (float)LuaAPI.lua_tonumber(L, 4);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _duration );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Vector3>(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Vector3 _pos;translator.Get(L, 3, out _pos);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 4);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _pos, _duration );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Vector3>(L, 3)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Vector3 _pos;translator.Get(L, 3, out _pos);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _pos );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Quaternion>(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _pos;translator.Get(L, 4, out _pos);
                    UnityEngine.Quaternion _rot;translator.Get(L, 5, out _rot);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 6);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _pos, _rot, _duration );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 8&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Quaternion>(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 7)&& translator.Assignable<CFEngine.LoadSFXFinish>(L, 8)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _pos;translator.Get(L, 4, out _pos);
                    UnityEngine.Quaternion _rot;translator.Get(L, 5, out _rot);
                    uint _sync = LuaAPI.xlua_touint(L, 6);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 7);
                    CFEngine.LoadSFXFinish _cb = translator.GetDelegate<CFEngine.LoadSFXFinish>(L, 8);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _pos, _rot, _sync, _duration, _cb );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 7&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Quaternion>(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 7)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _pos;translator.Get(L, 4, out _pos);
                    UnityEngine.Quaternion _rot;translator.Get(L, 5, out _rot);
                    uint _sync = LuaAPI.xlua_touint(L, 6);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 7);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _pos, _rot, _sync, _duration );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Quaternion>(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _pos;translator.Get(L, 4, out _pos);
                    UnityEngine.Quaternion _rot;translator.Get(L, 5, out _rot);
                    uint _sync = LuaAPI.xlua_touint(L, 6);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _pos, _rot, _sync );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 5&& translator.Assignable<CFEngine.SFX>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& translator.Assignable<UnityEngine.Vector3>(L, 4)&& translator.Assignable<UnityEngine.Quaternion>(L, 5)) 
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    UnityEngine.Vector3 _pos;translator.Get(L, 4, out _pos);
                    UnityEngine.Quaternion _rot;translator.Get(L, 5, out _rot);
                    
                    UnityEngine.CFUI.UIUtils.PlaySfx_Aux( ref _sfx, _name, _parent, _pos, _rot );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.PlaySfx_Aux!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DestroySfx_Aux_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    
                    UnityEngine.CFUI.UIUtils.DestroySfx_Aux( ref _sfx );
                    translator.Push(L, _sfx);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateObject_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    string _url = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    int _layer = LuaAPI.xlua_tointeger(L, 3);
                    string _name = LuaAPI.lua_tostring(L, 4);
                    
                        CFEngine.XGameObject gen_ret = UnityEngine.CFUI.UIUtils.CreateObject( _url, _parent, _layer, _name );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    string _url = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    int _layer = LuaAPI.xlua_tointeger(L, 3);
                    
                        CFEngine.XGameObject gen_ret = UnityEngine.CFUI.UIUtils.CreateObject( _url, _parent, _layer );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.CreateObject!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TimeFormatString_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1) || LuaAPI.lua_isuint64(L, 1))) 
                {
                    ulong _second = LuaAPI.lua_touint64(L, 1);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _second );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    uint _lowCount = LuaAPI.xlua_touint(L, 2);
                    uint _upCount = LuaAPI.xlua_touint(L, 3);
                    uint _minUnit = LuaAPI.xlua_touint(L, 4);
                    bool _isCarry = LuaAPI.lua_toboolean(L, 5);
                    bool _needPadLeft = LuaAPI.lua_toboolean(L, 6);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond, _lowCount, _upCount, _minUnit, _isCarry, _needPadLeft );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    uint _lowCount = LuaAPI.xlua_touint(L, 2);
                    uint _upCount = LuaAPI.xlua_touint(L, 3);
                    uint _minUnit = LuaAPI.xlua_touint(L, 4);
                    bool _isCarry = LuaAPI.lua_toboolean(L, 5);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond, _lowCount, _upCount, _minUnit, _isCarry );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    uint _lowCount = LuaAPI.xlua_touint(L, 2);
                    uint _upCount = LuaAPI.xlua_touint(L, 3);
                    uint _minUnit = LuaAPI.xlua_touint(L, 4);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond, _lowCount, _upCount, _minUnit );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    uint _lowCount = LuaAPI.xlua_touint(L, 2);
                    uint _upCount = LuaAPI.xlua_touint(L, 3);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond, _lowCount, _upCount );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    uint _lowCount = LuaAPI.xlua_touint(L, 2);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond, _lowCount );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _totalSecond = LuaAPI.xlua_touint(L, 1);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatString( _totalSecond );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.TimeFormatString!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TimeFormatStringWithoutSecond_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    ulong _second = LuaAPI.lua_touint64(L, 1);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatStringWithoutSecond( _second );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TimeFormatStringWithoutDay_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    ulong _second = LuaAPI.lua_touint64(L, 1);
                    
                        string gen_ret = UnityEngine.CFUI.UIUtils.TimeFormatStringWithoutDay( _second );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetUniversalTime_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    ulong _time = LuaAPI.lua_touint64(L, 1);
                    
                        ulong gen_ret = UnityEngine.CFUI.UIUtils.GetUniversalTime( _time );
                        LuaAPI.lua_pushuint64(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlaySpriteAnimation_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Transform _root = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    float _delay = (float)LuaAPI.lua_tonumber(L, 2);
                    uint _group = LuaAPI.xlua_touint(L, 3);
                    bool _rebuildMask = LuaAPI.lua_toboolean(L, 4);
                    
                    UnityEngine.CFUI.UIUtils.PlaySpriteAnimation( _root, _delay, _group, _rebuildMask );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Transform _root = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    float _delay = (float)LuaAPI.lua_tonumber(L, 2);
                    uint _group = LuaAPI.xlua_touint(L, 3);
                    
                    UnityEngine.CFUI.UIUtils.PlaySpriteAnimation( _root, _delay, _group );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    UnityEngine.Transform _root = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    float _delay = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    UnityEngine.CFUI.UIUtils.PlaySpriteAnimation( _root, _delay );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.Transform>(L, 1)) 
                {
                    UnityEngine.Transform _root = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    
                    UnityEngine.CFUI.UIUtils.PlaySpriteAnimation( _root );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to UnityEngine.CFUI.UIUtils.PlaySpriteAnimation!");
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}

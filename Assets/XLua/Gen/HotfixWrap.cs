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
    public class HotfixWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(Hotfix);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 40, 1, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetDocument", _m_GetDocument_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetDocumentMember", _m_SetDocumentMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetDocumentMember", _m_GetDocumentMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetGetDocumentLongMember", _m_GetGetDocumentLongMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetDocumentStaticMember", _m_GetDocumentStaticMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CallDocumentMethod", _m_CallDocumentMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CallDocumentLongMethod", _m_CallDocumentLongMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CallDocumentStaticMethod", _m_CallDocumentStaticMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSingle", _m_GetSingle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetGSDKManager", _m_GetGSDKManager_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetIRuGameAdvancedInjectionManager", _m_GetIRuGameAdvancedInjectionManager_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "JsonDecode", _m_JsonDecode_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSingleMember", _m_GetSingleMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSingleLongMember", _m_GetSingleLongMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetSingleMember", _m_SetSingleMember_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CallSingleMethod", _m_CallSingleMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CallSingleLongMethod", _m_CallSingleLongMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetEnumType", _m_GetEnumType_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetStringTable", _m_GetStringTable_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetObjectString", _m_GetObjectString_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LuaWait", _m_LuaWait_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LuaLoop", _m_LuaLoop_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RemoveTimer", _m_RemoveTimer_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SendLuaPtc", _m_SendLuaPtc_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SendLuaRPC", _m_SendLuaRPC_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateAttributeTagSkin", _m_CreateAttributeTagSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecycleInstanceTagSkin", _m_RecycleInstanceTagSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateOpItemSkinGroup", _m_CreateOpItemSkinGroup_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetText", _m_GetText_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetButton", _m_GetButton_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetImage", _m_GetImage_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetRawImage", _m_GetRawImage_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetEmpty4DragRaycast", _m_GetEmpty4DragRaycast_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetEmpty4Raycast", _m_GetEmpty4Raycast_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetCFSlider", _m_GetCFSlider_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetAnimation", _m_GetAnimation_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetInput", _m_GetInput_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetWrapContent", _m_GetWrapContent_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSpectorEffect", _m_GetSpectorEffect_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "luaExtion", _g_get_luaExtion);
            
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					Hotfix gen_ret = new Hotfix();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to Hotfix constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDocument_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    
                        object gen_ret = Hotfix.GetDocument( _doc );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetDocumentMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    object _value = translator.GetObject(L, 3, typeof(object));
                    bool _isPublic = LuaAPI.lua_toboolean(L, 4);
                    bool _isField = LuaAPI.lua_toboolean(L, 5);
                    
                    Hotfix.SetDocumentMember( _doc, _key, _value, _isPublic, _isField );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDocumentMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    
                        object gen_ret = Hotfix.GetDocumentMember( _doc, _key, _isPublic, _isField );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetGetDocumentLongMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    
                        string gen_ret = Hotfix.GetGetDocumentLongMember( _doc, _key, _isPublic, _isField );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetDocumentStaticMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    
                        object gen_ret = Hotfix.GetDocumentStaticMember( _doc, _key, _isPublic, _isField );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CallDocumentMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 2);
                    string _method = LuaAPI.lua_tostring(L, 3);
                    object[] _args = translator.GetParams<object>(L, 4);
                    
                        object gen_ret = Hotfix.CallDocumentMethod( _doc, _isPublic, _method, _args );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CallDocumentLongMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 2);
                    string _method = LuaAPI.lua_tostring(L, 3);
                    object[] _args = translator.GetParams<object>(L, 4);
                    
                        string gen_ret = Hotfix.CallDocumentLongMethod( _doc, _isPublic, _method, _args );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CallDocumentStaticMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _doc = LuaAPI.lua_tostring(L, 1);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 2);
                    string _method = LuaAPI.lua_tostring(L, 3);
                    object[] _args = translator.GetParams<object>(L, 4);
                    
                        object gen_ret = Hotfix.CallDocumentStaticMethod( _doc, _isPublic, _method, _args );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSingle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    
                        object gen_ret = Hotfix.GetSingle( _className );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetGSDKManager_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        object gen_ret = Hotfix.GetGSDKManager(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetIRuGameAdvancedInjectionManager_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        object gen_ret = Hotfix.GetIRuGameAdvancedInjectionManager(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_JsonDecode_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _jsonStr = LuaAPI.lua_tostring(L, 1);
                    
                        object gen_ret = Hotfix.JsonDecode( _jsonStr );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSingleMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 5);
                    
                        object gen_ret = Hotfix.GetSingleMember( _className, _key, _isPublic, _isField, _isStatic );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSingleLongMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 5);
                    
                        string gen_ret = Hotfix.GetSingleLongMember( _className, _key, _isPublic, _isField, _isStatic );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetSingleMember_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    string _key = LuaAPI.lua_tostring(L, 2);
                    object _value = translator.GetObject(L, 3, typeof(object));
                    bool _isPublic = LuaAPI.lua_toboolean(L, 4);
                    bool _isField = LuaAPI.lua_toboolean(L, 5);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 6);
                    
                    Hotfix.SetSingleMember( _className, _key, _value, _isPublic, _isField, _isStatic );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CallSingleMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 2);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 3);
                    string _methodName = LuaAPI.lua_tostring(L, 4);
                    object[] _args = translator.GetParams<object>(L, 5);
                    
                        object gen_ret = Hotfix.CallSingleMethod( _className, _isPublic, _isStatic, _methodName, _args );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CallSingleLongMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _className = LuaAPI.lua_tostring(L, 1);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 2);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 3);
                    string _methodName = LuaAPI.lua_tostring(L, 4);
                    object[] _args = translator.GetParams<object>(L, 5);
                    
                        string gen_ret = Hotfix.CallSingleLongMethod( _className, _isPublic, _isStatic, _methodName, _args );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetEnumType_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _classname = LuaAPI.lua_tostring(L, 1);
                    string _value = LuaAPI.lua_tostring(L, 2);
                    
                        object gen_ret = Hotfix.GetEnumType( _classname, _value );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetStringTable_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _key = LuaAPI.lua_tostring(L, 1);
                    object[] _args = translator.GetParams<object>(L, 2);
                    
                        string gen_ret = Hotfix.GetStringTable( _key, _args );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetObjectString_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<object>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    object _o = translator.GetObject(L, 1, typeof(object));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    
                        string gen_ret = Hotfix.GetObjectString( _o, _name );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<object>(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    object _o = translator.GetObject(L, 1, typeof(object));
                    string _name = LuaAPI.lua_tostring(L, 2);
                    bool _isPublic = LuaAPI.lua_toboolean(L, 3);
                    bool _isField = LuaAPI.lua_toboolean(L, 4);
                    
                        string gen_ret = Hotfix.GetObjectString( _o, _name, _isPublic, _isField );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Hotfix.GetObjectString!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LuaWait_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _delay = LuaAPI.xlua_tointeger(L, 1);
                    XLua.LuaFunction _cb = (XLua.LuaFunction)translator.GetObject(L, 2, typeof(XLua.LuaFunction));
                    
                        uint gen_ret = Hotfix.LuaWait( _delay, _cb );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LuaLoop_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _delay = LuaAPI.xlua_tointeger(L, 1);
                    int _loop = LuaAPI.xlua_tointeger(L, 2);
                    XLua.LuaFunction _cb = (XLua.LuaFunction)translator.GetObject(L, 3, typeof(XLua.LuaFunction));
                    
                        uint gen_ret = Hotfix.LuaLoop( _delay, _loop, _cb );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RemoveTimer_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _seq = LuaAPI.xlua_touint(L, 1);
                    
                    Hotfix.RemoveTimer( _seq );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SendLuaPtc_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _type = LuaAPI.xlua_touint(L, 1);
                    System.IO.MemoryStream _stream = (System.IO.MemoryStream)translator.GetObject(L, 2, typeof(System.IO.MemoryStream));
                    int _len = LuaAPI.xlua_tointeger(L, 3);
                    
                        bool gen_ret = Hotfix.SendLuaPtc( _type, _stream, _len );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SendLuaRPC_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _type = LuaAPI.xlua_touint(L, 1);
                    System.IO.MemoryStream _stream = (System.IO.MemoryStream)translator.GetObject(L, 2, typeof(System.IO.MemoryStream));
                    int _len = LuaAPI.xlua_tointeger(L, 3);
                    CFUtilPoolLib.LuaRpcRespond _onRes = translator.GetDelegate<CFUtilPoolLib.LuaRpcRespond>(L, 4);
                    
                    Hotfix.SendLuaRPC( _type, _stream, _len, _onRes );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateAttributeTagSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.Transform _trans = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _active = LuaAPI.lua_toboolean(L, 3);
                    
                        CFClient.UI.AttributeTagSkin gen_ret = Hotfix.CreateAttributeTagSkin( _prefabID, _trans, _active );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleInstanceTagSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<CFClient.UI.AttributeTagSkin>(L, 1)) 
                {
                    CFClient.UI.AttributeTagSkin _skin = (CFClient.UI.AttributeTagSkin)translator.GetObject(L, 1, typeof(CFClient.UI.AttributeTagSkin));
                    
                    Hotfix.RecycleInstanceTagSkin( _skin );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<CFClient.UI.OpItemSkinGroup>(L, 1)) 
                {
                    CFClient.UI.OpItemSkinGroup _skin = (CFClient.UI.OpItemSkinGroup)translator.GetObject(L, 1, typeof(CFClient.UI.OpItemSkinGroup));
                    
                    Hotfix.RecycleInstanceTagSkin( _skin );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<CFClient.UI.OpItemSkinGroup>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    CFClient.UI.OpItemSkinGroup _skin = (CFClient.UI.OpItemSkinGroup)translator.GetObject(L, 1, typeof(CFClient.UI.OpItemSkinGroup));
                    bool _recycle = LuaAPI.lua_toboolean(L, 2);
                    
                    Hotfix.RecycleInstanceTagSkin( _skin, _recycle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to Hotfix.RecycleInstanceTagSkin!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateOpItemSkinGroup_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.Transform _trans = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _active = LuaAPI.lua_toboolean(L, 3);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = Hotfix.CreateOpItemSkinGroup( _prefabID, _trans, _active );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetText_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFText gen_ret = Hotfix.GetText( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetButton_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFButton gen_ret = Hotfix.GetButton( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetImage_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFImage gen_ret = Hotfix.GetImage( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetRawImage_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFRawImage gen_ret = Hotfix.GetRawImage( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetEmpty4DragRaycast_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.Empty4DragRaycast gen_ret = Hotfix.GetEmpty4DragRaycast( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetEmpty4Raycast_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.Empty4Raycast gen_ret = Hotfix.GetEmpty4Raycast( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetCFSlider_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFSlider gen_ret = Hotfix.GetCFSlider( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAnimation_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFAnimation gen_ret = Hotfix.GetAnimation( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetInput_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFInput gen_ret = Hotfix.GetInput( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetWrapContent_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFWrapContent gen_ret = Hotfix.GetWrapContent( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSpectorEffect_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    string _path = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.CFUI.CFSpectorEffect gen_ret = Hotfix.GetSpectorEffect( _tf, _path );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_luaExtion(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.PushAny(L, Hotfix.luaExtion);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
		
		
		
		
    }
}

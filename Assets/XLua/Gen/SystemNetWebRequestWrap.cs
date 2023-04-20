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
    public class SystemNetWebRequestWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(System.Net.WebRequest);
			Utils.BeginObjectRegister(type, L, translator, 0, 9, 14, 13);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetRequestStream", _m_GetRequestStream);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetResponse", _m_GetResponse);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BeginGetResponse", _m_BeginGetResponse);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "EndGetResponse", _m_EndGetResponse);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "BeginGetRequestStream", _m_BeginGetRequestStream);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "EndGetRequestStream", _m_EndGetRequestStream);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetRequestStreamAsync", _m_GetRequestStreamAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetResponseAsync", _m_GetResponseAsync);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Abort", _m_Abort);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "CachePolicy", _g_get_CachePolicy);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Method", _g_get_Method);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "RequestUri", _g_get_RequestUri);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ConnectionGroupName", _g_get_ConnectionGroupName);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Headers", _g_get_Headers);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ContentLength", _g_get_ContentLength);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ContentType", _g_get_ContentType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Credentials", _g_get_Credentials);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "UseDefaultCredentials", _g_get_UseDefaultCredentials);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Proxy", _g_get_Proxy);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "PreAuthenticate", _g_get_PreAuthenticate);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Timeout", _g_get_Timeout);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "AuthenticationLevel", _g_get_AuthenticationLevel);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ImpersonationLevel", _g_get_ImpersonationLevel);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "CachePolicy", _s_set_CachePolicy);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Method", _s_set_Method);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ConnectionGroupName", _s_set_ConnectionGroupName);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Headers", _s_set_Headers);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ContentLength", _s_set_ContentLength);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ContentType", _s_set_ContentType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Credentials", _s_set_Credentials);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "UseDefaultCredentials", _s_set_UseDefaultCredentials);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Proxy", _s_set_Proxy);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "PreAuthenticate", _s_set_PreAuthenticate);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Timeout", _s_set_Timeout);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "AuthenticationLevel", _s_set_AuthenticationLevel);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ImpersonationLevel", _s_set_ImpersonationLevel);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 6, 2, 2);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "Create", _m_Create_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateDefault", _m_CreateDefault_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateHttp", _m_CreateHttp_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterPrefix", _m_RegisterPrefix_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSystemWebProxy", _m_GetSystemWebProxy_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "DefaultCachePolicy", _g_get_DefaultCachePolicy);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "DefaultWebProxy", _g_get_DefaultWebProxy);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "DefaultCachePolicy", _s_set_DefaultCachePolicy);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "DefaultWebProxy", _s_set_DefaultWebProxy);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "System.Net.WebRequest does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Create_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _requestUriString = LuaAPI.lua_tostring(L, 1);
                    
                        System.Net.WebRequest gen_ret = System.Net.WebRequest.Create( _requestUriString );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<System.Uri>(L, 1)) 
                {
                    System.Uri _requestUri = (System.Uri)translator.GetObject(L, 1, typeof(System.Uri));
                    
                        System.Net.WebRequest gen_ret = System.Net.WebRequest.Create( _requestUri );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to System.Net.WebRequest.Create!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateDefault_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Uri _requestUri = (System.Uri)translator.GetObject(L, 1, typeof(System.Uri));
                    
                        System.Net.WebRequest gen_ret = System.Net.WebRequest.CreateDefault( _requestUri );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateHttp_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _requestUriString = LuaAPI.lua_tostring(L, 1);
                    
                        System.Net.HttpWebRequest gen_ret = System.Net.WebRequest.CreateHttp( _requestUriString );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<System.Uri>(L, 1)) 
                {
                    System.Uri _requestUri = (System.Uri)translator.GetObject(L, 1, typeof(System.Uri));
                    
                        System.Net.HttpWebRequest gen_ret = System.Net.WebRequest.CreateHttp( _requestUri );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to System.Net.WebRequest.CreateHttp!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterPrefix_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _prefix = LuaAPI.lua_tostring(L, 1);
                    System.Net.IWebRequestCreate _creator = (System.Net.IWebRequestCreate)translator.GetObject(L, 2, typeof(System.Net.IWebRequestCreate));
                    
                        bool gen_ret = System.Net.WebRequest.RegisterPrefix( _prefix, _creator );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetRequestStream(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.IO.Stream gen_ret = gen_to_be_invoked.GetRequestStream(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetResponse(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Net.WebResponse gen_ret = gen_to_be_invoked.GetResponse(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginGetResponse(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.AsyncCallback _callback = translator.GetDelegate<System.AsyncCallback>(L, 2);
                    object _state = translator.GetObject(L, 3, typeof(object));
                    
                        System.IAsyncResult gen_ret = gen_to_be_invoked.BeginGetResponse( _callback, _state );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EndGetResponse(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.IAsyncResult _asyncResult = (System.IAsyncResult)translator.GetObject(L, 2, typeof(System.IAsyncResult));
                    
                        System.Net.WebResponse gen_ret = gen_to_be_invoked.EndGetResponse( _asyncResult );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginGetRequestStream(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.AsyncCallback _callback = translator.GetDelegate<System.AsyncCallback>(L, 2);
                    object _state = translator.GetObject(L, 3, typeof(object));
                    
                        System.IAsyncResult gen_ret = gen_to_be_invoked.BeginGetRequestStream( _callback, _state );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EndGetRequestStream(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.IAsyncResult _asyncResult = (System.IAsyncResult)translator.GetObject(L, 2, typeof(System.IAsyncResult));
                    
                        System.IO.Stream gen_ret = gen_to_be_invoked.EndGetRequestStream( _asyncResult );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetRequestStreamAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Threading.Tasks.Task<System.IO.Stream> gen_ret = gen_to_be_invoked.GetRequestStreamAsync(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetResponseAsync(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        System.Threading.Tasks.Task<System.Net.WebResponse> gen_ret = gen_to_be_invoked.GetResponseAsync(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Abort(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Abort(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSystemWebProxy_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        System.Net.IWebProxy gen_ret = System.Net.WebRequest.GetSystemWebProxy(  );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_DefaultCachePolicy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, System.Net.WebRequest.DefaultCachePolicy);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_CachePolicy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.CachePolicy);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Method(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.Method);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_RequestUri(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.RequestUri);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ConnectionGroupName(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.ConnectionGroupName);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Headers(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.Headers);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ContentLength(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushint64(L, gen_to_be_invoked.ContentLength);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ContentType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.ContentType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Credentials(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.Credentials);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UseDefaultCredentials(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.UseDefaultCredentials);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Proxy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.PushAny(L, gen_to_be_invoked.Proxy);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_PreAuthenticate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.PreAuthenticate);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Timeout(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Timeout);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_AuthenticationLevel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.AuthenticationLevel);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ImpersonationLevel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ImpersonationLevel);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_DefaultWebProxy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.PushAny(L, System.Net.WebRequest.DefaultWebProxy);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_DefaultCachePolicy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    System.Net.WebRequest.DefaultCachePolicy = (System.Net.Cache.RequestCachePolicy)translator.GetObject(L, 1, typeof(System.Net.Cache.RequestCachePolicy));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_CachePolicy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.CachePolicy = (System.Net.Cache.RequestCachePolicy)translator.GetObject(L, 2, typeof(System.Net.Cache.RequestCachePolicy));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Method(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Method = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ConnectionGroupName(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ConnectionGroupName = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Headers(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Headers = (System.Net.WebHeaderCollection)translator.GetObject(L, 2, typeof(System.Net.WebHeaderCollection));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ContentLength(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ContentLength = LuaAPI.lua_toint64(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ContentType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ContentType = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Credentials(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Credentials = (System.Net.ICredentials)translator.GetObject(L, 2, typeof(System.Net.ICredentials));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UseDefaultCredentials(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.UseDefaultCredentials = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Proxy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Proxy = (System.Net.IWebProxy)translator.GetObject(L, 2, typeof(System.Net.IWebProxy));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_PreAuthenticate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.PreAuthenticate = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Timeout(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Timeout = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_AuthenticationLevel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                System.Net.Security.AuthenticationLevel gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.AuthenticationLevel = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ImpersonationLevel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                System.Net.WebRequest gen_to_be_invoked = (System.Net.WebRequest)translator.FastGetCSObj(L, 1);
                System.Security.Principal.TokenImpersonationLevel gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.ImpersonationLevel = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_DefaultWebProxy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    System.Net.WebRequest.DefaultWebProxy = (System.Net.IWebProxy)translator.GetObject(L, 1, typeof(System.Net.IWebProxy));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

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
    public class CFClientUIXSkinParamWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFClient.UI.XSkinParam);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 23, 23);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "asynch", _g_get_asynch);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "forceNum", _g_get_forceNum);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "MaxShowNum", _g_get_MaxShowNum);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "forceShowNum", _g_get_forceShowNum);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "MaxItemCount", _g_get_MaxItemCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "RedPoint", _g_get_RedPoint);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IconRaycast", _g_get_IconRaycast);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ClickCallback", _g_get_ClickCallback);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "HideBg", _g_get_HideBg);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "HideQuality", _g_get_HideQuality);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showChip", _g_get_showChip);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showTip", _g_get_showTip);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showGetWay", _g_get_showGetWay);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "receivedIcon", _g_get_receivedIcon);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "acquireType", _g_get_acquireType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "acquireTxt", _g_get_acquireTxt);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "hasOptionalToggle", _g_get_hasOptionalToggle);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isBlack", _g_get_isBlack);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "offset", _g_get_offset);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showProgrammeIcon", _g_get_showProgrammeIcon);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "fromView", _g_get_fromView);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "equiper", _g_get_equiper);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "numberString", _g_get_numberString);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "asynch", _s_set_asynch);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "forceNum", _s_set_forceNum);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "MaxShowNum", _s_set_MaxShowNum);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "forceShowNum", _s_set_forceShowNum);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "MaxItemCount", _s_set_MaxItemCount);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "RedPoint", _s_set_RedPoint);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "IconRaycast", _s_set_IconRaycast);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "ClickCallback", _s_set_ClickCallback);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "HideBg", _s_set_HideBg);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "HideQuality", _s_set_HideQuality);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showChip", _s_set_showChip);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showTip", _s_set_showTip);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showGetWay", _s_set_showGetWay);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "receivedIcon", _s_set_receivedIcon);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "acquireType", _s_set_acquireType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "acquireTxt", _s_set_acquireTxt);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "hasOptionalToggle", _s_set_hasOptionalToggle);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "isBlack", _s_set_isBlack);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "offset", _s_set_offset);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showProgrammeIcon", _s_set_showProgrammeIcon);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "fromView", _s_set_fromView);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "equiper", _s_set_equiper);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "numberString", _s_set_numberString);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 2, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSkinParam", _m_GetSkinParam_xlua_st_);
            
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFClient.UI.XSkinParam gen_ret = new CFClient.UI.XSkinParam();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XSkinParam constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reset(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Reset(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkinParam_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        CFClient.UI.XSkinParam gen_ret = CFClient.UI.XSkinParam.GetSkinParam(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_asynch(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.asynch);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_forceNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.forceNum);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_MaxShowNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.MaxShowNum);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_forceShowNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.forceShowNum);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_MaxItemCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.MaxItemCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_RedPoint(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.RedPoint);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IconRaycast(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IconRaycast);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ClickCallback(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ClickCallback);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_HideBg(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.HideBg);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_HideQuality(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.HideQuality);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showChip(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showChip);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showTip(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showTip);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showGetWay(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showGetWay);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_receivedIcon(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.receivedIcon);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_acquireType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushuint(L, gen_to_be_invoked.acquireType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_acquireTxt(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.acquireTxt);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_hasOptionalToggle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.hasOptionalToggle);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isBlack(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isBlack);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_offset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.offset);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showProgrammeIcon(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showProgrammeIcon);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_fromView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushuint(L, gen_to_be_invoked.fromView);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_equiper(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushuint(L, gen_to_be_invoked.equiper);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_numberString(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.numberString);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_asynch(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.asynch = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_forceNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.forceNum = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_MaxShowNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.MaxShowNum = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_forceShowNum(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.forceShowNum = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_MaxItemCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.MaxItemCount = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_RedPoint(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.RedPoint = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_IconRaycast(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.IconRaycast = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ClickCallback(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.ClickCallback = translator.GetDelegate<UnityEngine.CFEventSystems.UIBehaviour.VoidBehaviourDelegate>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_HideBg(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.HideBg = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_HideQuality(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.HideQuality = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showChip(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showChip = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showTip(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showTip = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showGetWay(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showGetWay = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_receivedIcon(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.receivedIcon = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_acquireType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.acquireType = LuaAPI.xlua_touint(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_acquireTxt(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.acquireTxt = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_hasOptionalToggle(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.hasOptionalToggle = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_isBlack(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.isBlack = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_offset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.offset = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showProgrammeIcon(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showProgrammeIcon = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_fromView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.fromView = LuaAPI.xlua_touint(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_equiper(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.equiper = LuaAPI.xlua_touint(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_numberString(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.XSkinParam gen_to_be_invoked = (CFClient.UI.XSkinParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.numberString = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

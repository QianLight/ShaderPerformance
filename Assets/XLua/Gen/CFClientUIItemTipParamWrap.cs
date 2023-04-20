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
    public class CFClientUIItemTipParamWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFClient.UI.ItemTipParam);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 11, 11);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "skinParam", _g_get_skinParam);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mShowButtons", _g_get_mShowButtons);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mshowGetWayBtn", _g_get_mshowGetWayBtn);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mHightLightSuit", _g_get_mHightLightSuit);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showChangeBtn", _g_get_showChangeBtn);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "showUnloadBtn", _g_get_showUnloadBtn);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "raycastPass", _g_get_raycastPass);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "forceShowGetway", _g_get_forceShowGetway);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "bindPartner", _g_get_bindPartner);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "openFromView", _g_get_openFromView);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "equipsList", _g_get_equipsList);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "skinParam", _s_set_skinParam);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mShowButtons", _s_set_mShowButtons);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mshowGetWayBtn", _s_set_mshowGetWayBtn);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mHightLightSuit", _s_set_mHightLightSuit);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showChangeBtn", _s_set_showChangeBtn);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "showUnloadBtn", _s_set_showUnloadBtn);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "raycastPass", _s_set_raycastPass);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "forceShowGetway", _s_set_forceShowGetway);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "bindPartner", _s_set_bindPartner);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "openFromView", _s_set_openFromView);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "equipsList", _s_set_equipsList);
            
			
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
					
					CFClient.UI.ItemTipParam gen_ret = new CFClient.UI.ItemTipParam();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.ItemTipParam constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSkinParam_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        CFClient.UI.ItemTipParam gen_ret = CFClient.UI.ItemTipParam.GetSkinParam(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
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
            
            
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Reset(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_skinParam(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.skinParam);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mShowButtons(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.mShowButtons);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mshowGetWayBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.mshowGetWayBtn);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mHightLightSuit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.mHightLightSuit);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showChangeBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showChangeBtn);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_showUnloadBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.showUnloadBtn);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_raycastPass(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.raycastPass);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_forceShowGetway(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.forceShowGetway);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_bindPartner(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.bindPartner);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_openFromView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushuint(L, gen_to_be_invoked.openFromView);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_equipsList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.equipsList);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_skinParam(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.skinParam = (CFClient.UI.XSkinParam)translator.GetObject(L, 2, typeof(CFClient.UI.XSkinParam));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mShowButtons(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mShowButtons = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mshowGetWayBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mshowGetWayBtn = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mHightLightSuit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mHightLightSuit = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showChangeBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showChangeBtn = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_showUnloadBtn(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.showUnloadBtn = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_raycastPass(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.raycastPass = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_forceShowGetway(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.forceShowGetway = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_bindPartner(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.bindPartner = (CFClient.XPartnerData)translator.GetObject(L, 2, typeof(CFClient.XPartnerData));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_openFromView(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.openFromView = LuaAPI.xlua_touint(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_equipsList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                CFClient.UI.ItemTipParam gen_to_be_invoked = (CFClient.UI.ItemTipParam)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.equipsList = (System.Collections.Generic.List<CFClient.OPEquipmentData>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.OPEquipmentData>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

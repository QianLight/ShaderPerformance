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
    public class CFClientUIXItemHelperWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFClient.UI.XItemHelper);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 30, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "MakeXItem", _m_MakeXItem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MakeItemObject", _m_MakeItemObject_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItems", _m_GetItems_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecycleXItem", _m_RecycleXItem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ClearItems", _m_ClearItems_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowRewardPreview", _m_ShowRewardPreview_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ConvertXItemSkin", _m_ConvertXItemSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemIcon", _m_SetItemIcon_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemIconAndName", _m_SetItemIconAndName_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemIconAndCount", _m_SetItemIconAndCount_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemIconNameCount", _m_SetItemIconNameCount_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemQualityBack", _m_GetItemQualityBack_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemQualityCircleBack", _m_GetItemQualityCircleBack_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemBest", _m_GetItemBest_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemQualityColorStr", _m_GetItemQualityColorStr_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetNameWithTypeColor", _m_GetNameWithTypeColor_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemQualityColor", _m_GetItemQualityColor_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemTypeStr", _m_GetItemTypeStr_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckLevel", _m_CheckLevel_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FilterSceneType", _m_FilterSceneType_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetItemCount", _m_GetItemCount_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemCountString", _m_SetItemCountString_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "TryCheckFullItem", _m_TryCheckFullItem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "HasItem", _m_HasItem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ConvertMapItem2XItemList", _m_ConvertMapItem2XItemList_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ConverSeqListRef2XItemList", _m_ConverSeqListRef2XItemList_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EntryConditions", _m_EntryConditions_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SortReward", _m_SortReward_xlua_st_);
            
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "mBuilder", CFClient.UI.XItemHelper.mBuilder);
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFClient.UI.XItemHelper gen_ret = new CFClient.UI.XItemHelper();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MakeXItem_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    uint _itemID = LuaAPI.xlua_touint(L, 1);
                    uint _itemCount = LuaAPI.xlua_touint(L, 2);
                    
                        CFClient.XItem gen_ret = CFClient.UI.XItemHelper.MakeXItem( _itemID, _itemCount );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _itemID = LuaAPI.xlua_touint(L, 1);
                    
                        CFClient.XItem gen_ret = CFClient.UI.XItemHelper.MakeXItem( _itemID );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<KKSG.Item>(L, 1)) 
                {
                    KKSG.Item _data = (KKSG.Item)translator.GetObject(L, 1, typeof(KKSG.Item));
                    
                        CFClient.XItem gen_ret = CFClient.UI.XItemHelper.MakeXItem( _data );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.MakeXItem!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MakeItemObject_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    uint _itemId = LuaAPI.xlua_touint(L, 2);
                    uint _itemCount = LuaAPI.xlua_touint(L, 3);
                    bool _force = LuaAPI.lua_toboolean(L, 4);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.MakeItemObject( _parent, _itemId, _itemCount, _force );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    uint _itemId = LuaAPI.xlua_touint(L, 2);
                    uint _itemCount = LuaAPI.xlua_touint(L, 3);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.MakeItemObject( _parent, _itemId, _itemCount );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Transform>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    uint _itemId = LuaAPI.xlua_touint(L, 2);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.MakeItemObject( _parent, _itemId );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.MakeItemObject!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItems_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 8&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 8)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    bool _redpoint = LuaAPI.lua_toboolean(L, 4);
                    bool _showTip = LuaAPI.lua_toboolean(L, 5);
                    bool _received = LuaAPI.lua_toboolean(L, 6);
                    bool _isBlack = LuaAPI.lua_toboolean(L, 7);
                    bool _showGetway = LuaAPI.lua_toboolean(L, 8);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx, _redpoint, _showTip, _received, _isBlack, _showGetway );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 7&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    bool _redpoint = LuaAPI.lua_toboolean(L, 4);
                    bool _showTip = LuaAPI.lua_toboolean(L, 5);
                    bool _received = LuaAPI.lua_toboolean(L, 6);
                    bool _isBlack = LuaAPI.lua_toboolean(L, 7);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx, _redpoint, _showTip, _received, _isBlack );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 6&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    bool _redpoint = LuaAPI.lua_toboolean(L, 4);
                    bool _showTip = LuaAPI.lua_toboolean(L, 5);
                    bool _received = LuaAPI.lua_toboolean(L, 6);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx, _redpoint, _showTip, _received );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 5&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    bool _redpoint = LuaAPI.lua_toboolean(L, 4);
                    bool _showTip = LuaAPI.lua_toboolean(L, 5);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx, _redpoint, _showTip );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    bool _redpoint = LuaAPI.lua_toboolean(L, 4);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx, _redpoint );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    bool _showfx = LuaAPI.lua_toboolean(L, 3);
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t, _showfx );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<UnityEngine.Transform>(L, 2)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    UnityEngine.Transform _t = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.OpItemSkinGroup gen_ret = CFClient.UI.XItemHelper.GetItems( _item, _t );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.GetItems!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleXItem_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.RecycleXItem( _items );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearItems_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Collections.Generic.List<CFClient.UI.OpItemSkinGroup> _items = (System.Collections.Generic.List<CFClient.UI.OpItemSkinGroup>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.UI.OpItemSkinGroup>));
                    
                    CFClient.UI.XItemHelper.ClearItems( ref _items );
                    translator.Push(L, _items);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowRewardPreview_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _list = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.ShowRewardPreview( _list );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    
                    CFClient.UI.XItemHelper.ShowRewardPreview( _items, _pos );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.ShowRewardPreview!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ConvertXItemSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    uint _skin = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XItemHelper.ConvertXItemSkin( _item, ref _skin );
                    LuaAPI.xlua_pushuint(L, _skin);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemIcon_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    uint _itemid = LuaAPI.xlua_touint(L, 2);
                    bool _little = LuaAPI.lua_toboolean(L, 3);
                    bool _force = LuaAPI.lua_toboolean(L, 4);
                    
                    CFClient.UI.XItemHelper.SetItemIcon( _ico, _itemid, _little, _force );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    uint _itemid = LuaAPI.xlua_touint(L, 2);
                    bool _little = LuaAPI.lua_toboolean(L, 3);
                    
                    CFClient.UI.XItemHelper.SetItemIcon( _ico, _itemid, _little );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    uint _itemid = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XItemHelper.SetItemIcon( _ico, _itemid );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.SetItemIcon!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemIconAndName_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 6&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    string _color = LuaAPI.lua_tostring(L, 4);
                    bool _little = LuaAPI.lua_toboolean(L, 5);
                    bool _force = LuaAPI.lua_toboolean(L, 6);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndName( _ico, _text, _itemid, _color, _little, _force );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    string _color = LuaAPI.lua_tostring(L, 4);
                    bool _little = LuaAPI.lua_toboolean(L, 5);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndName( _ico, _text, _itemid, _color, _little );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    string _color = LuaAPI.lua_tostring(L, 4);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndName( _ico, _text, _itemid, _color );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.SetItemIconAndName!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemIconAndCount_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 7&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    bool _little = LuaAPI.lua_toboolean(L, 6);
                    bool _force = LuaAPI.lua_toboolean(L, 7);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndCount( _ico, _text, _itemid, _count, _color, _little, _force );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 6&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    bool _little = LuaAPI.lua_toboolean(L, 6);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndCount( _ico, _text, _itemid, _count, _color, _little );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    
                    CFClient.UI.XItemHelper.SetItemIconAndCount( _ico, _text, _itemid, _count, _color );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.SetItemIconAndCount!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemIconNameCount_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 7&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    bool _little = LuaAPI.lua_toboolean(L, 6);
                    bool _force = LuaAPI.lua_toboolean(L, 7);
                    
                    CFClient.UI.XItemHelper.SetItemIconNameCount( _ico, _text, _itemid, _count, _color, _little, _force );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 6&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 6)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    bool _little = LuaAPI.lua_toboolean(L, 6);
                    
                    CFClient.UI.XItemHelper.SetItemIconNameCount( _ico, _text, _itemid, _count, _color, _little );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& translator.Assignable<UnityEngine.CFUI.CFImage>(L, 1)&& translator.Assignable<UnityEngine.CFUI.CFText>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4) || LuaAPI.lua_isuint64(L, 4))&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)) 
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    UnityEngine.CFUI.CFText _text = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 3);
                    ulong _count = LuaAPI.lua_touint64(L, 4);
                    string _color = LuaAPI.lua_tostring(L, 5);
                    
                    CFClient.UI.XItemHelper.SetItemIconNameCount( _ico, _text, _itemid, _count, _color );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.SetItemIconNameCount!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemQualityBack_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    string _spriteName;
                    string _atlas;
                    string _effect;
                    
                    CFClient.UI.XItemHelper.GetItemQualityBack( _quality, out _spriteName, out _atlas, out _effect );
                    LuaAPI.lua_pushstring(L, _spriteName);
                        
                    LuaAPI.lua_pushstring(L, _atlas);
                        
                    LuaAPI.lua_pushstring(L, _effect);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemQualityCircleBack_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    string _spriteName;
                    string _atlas;
                    string _effect;
                    
                    CFClient.UI.XItemHelper.GetItemQualityCircleBack( _quality, out _spriteName, out _atlas, out _effect );
                    LuaAPI.lua_pushstring(L, _spriteName);
                        
                    LuaAPI.lua_pushstring(L, _atlas);
                        
                    LuaAPI.lua_pushstring(L, _effect);
                        
                    
                    
                    
                    return 3;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemBest_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    string _spriteName;
                    string _atlas;
                    
                    CFClient.UI.XItemHelper.GetItemBest( _quality, out _spriteName, out _atlas );
                    LuaAPI.lua_pushstring(L, _spriteName);
                        
                    LuaAPI.lua_pushstring(L, _atlas);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemQualityColorStr_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    uint _type = LuaAPI.xlua_touint(L, 2);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetItemQualityColorStr( _quality, _type );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetItemQualityColorStr( _quality );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    string _str = LuaAPI.lua_tostring(L, 2);
                    uint _type = LuaAPI.xlua_touint(L, 3);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetItemQualityColorStr( _quality, _str, _type );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    string _str = LuaAPI.lua_tostring(L, 2);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetItemQualityColorStr( _quality, _str );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.GetItemQualityColorStr!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetNameWithTypeColor_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    bool _withTypeColor = LuaAPI.lua_toboolean(L, 2);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetNameWithTypeColor( _item, _withTypeColor );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemQualityColor_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    uint _type = LuaAPI.xlua_touint(L, 2);
                    
                        UnityEngine.Color gen_ret = CFClient.UI.XItemHelper.GetItemQualityColor( _quality, _type );
                        translator.PushUnityEngineColor(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _quality = LuaAPI.xlua_touint(L, 1);
                    
                        UnityEngine.Color gen_ret = CFClient.UI.XItemHelper.GetItemQualityColor( _quality );
                        translator.PushUnityEngineColor(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.GetItemQualityColor!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemTypeStr_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _type = LuaAPI.xlua_tointeger(L, 1);
                    
                        string gen_ret = CFClient.UI.XItemHelper.GetItemTypeStr( _type );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckLevel_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFUtilPoolLib.ItemList.RowData _data = (CFUtilPoolLib.ItemList.RowData)translator.GetObject(L, 1, typeof(CFUtilPoolLib.ItemList.RowData));
                    string _str = LuaAPI.lua_tostring(L, 2);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.CheckLevel( _data, ref _str );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    LuaAPI.lua_pushstring(L, _str);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FilterSceneType_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int[] _sceneList = (int[])translator.GetObject(L, 1, typeof(int[]));
                    int _sceneType = LuaAPI.xlua_tointeger(L, 2);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.FilterSceneType( _sceneList, _sceneType );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemCount_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _itemid = LuaAPI.xlua_touint(L, 1);
                    
                        ulong gen_ret = CFClient.UI.XItemHelper.GetItemCount( _itemid );
                        LuaAPI.lua_pushuint64(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemCountString_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.CFUI.CFText _label = (UnityEngine.CFUI.CFText)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFText));
                    uint _itemid = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XItemHelper.SetItemCountString( ref _label, _itemid );
                    translator.Push(L, _label);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TryCheckFullItem_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _itemid = LuaAPI.xlua_touint(L, 1);
                    uint _count = LuaAPI.xlua_touint(L, 2);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.TryCheckFullItem( _itemid, _count );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HasItem_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _itemid = LuaAPI.xlua_touint(L, 1);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.HasItem( _itemid );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ConvertMapItem2XItemList_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<Google.Protobuf.Collections.RepeatedField<KKSG.Item>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    Google.Protobuf.Collections.RepeatedField<KKSG.Item> _items = (Google.Protobuf.Collections.RepeatedField<KKSG.Item>)translator.GetObject(L, 1, typeof(Google.Protobuf.Collections.RepeatedField<KKSG.Item>));
                    System.Collections.Generic.List<CFClient.XItem> _itemList = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    uint _status = LuaAPI.xlua_touint(L, 3);
                    
                    CFClient.UI.XItemHelper.ConvertMapItem2XItemList( _items, ref _itemList, _status );
                    translator.Push(L, _itemList);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<Google.Protobuf.Collections.RepeatedField<KKSG.Item>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)) 
                {
                    Google.Protobuf.Collections.RepeatedField<KKSG.Item> _items = (Google.Protobuf.Collections.RepeatedField<KKSG.Item>)translator.GetObject(L, 1, typeof(Google.Protobuf.Collections.RepeatedField<KKSG.Item>));
                    System.Collections.Generic.List<CFClient.XItem> _itemList = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.ConvertMapItem2XItemList( _items, ref _itemList );
                    translator.Push(L, _itemList);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief> _items = (Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>)translator.GetObject(L, 1, typeof(Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>));
                    System.Collections.Generic.List<CFClient.XItem> _itemList = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    uint _status = LuaAPI.xlua_touint(L, 3);
                    
                    CFClient.UI.XItemHelper.ConvertMapItem2XItemList( _items, ref _itemList, _status );
                    translator.Push(L, _itemList);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)) 
                {
                    Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief> _items = (Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>)translator.GetObject(L, 1, typeof(Google.Protobuf.Collections.RepeatedField<KKSG.ItemBrief>));
                    System.Collections.Generic.List<CFClient.XItem> _itemList = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.ConvertMapItem2XItemList( _items, ref _itemList );
                    translator.Push(L, _itemList);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.ConvertMapItem2XItemList!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ConverSeqListRef2XItemList_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& translator.Assignable<CFUtilPoolLib.SeqListRef<uint>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    CFUtilPoolLib.SeqListRef<uint> _reward;translator.Get(L, 1, out _reward);
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    uint _status = LuaAPI.xlua_touint(L, 3);
                    
                    CFClient.UI.XItemHelper.ConverSeqListRef2XItemList( _reward, ref _items, _status );
                    translator.Push(L, _items);
                        
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<CFUtilPoolLib.SeqListRef<uint>>(L, 1)&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 2)) 
                {
                    CFUtilPoolLib.SeqListRef<uint> _reward;translator.Get(L, 1, out _reward);
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.ConverSeqListRef2XItemList( _reward, ref _items );
                    translator.Push(L, _items);
                        
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.ConverSeqListRef2XItemList!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EntryConditions_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<CFUtilPoolLib.SeqListRef<uint>>(L, 1)&& translator.Assignable<CFUtilPoolLib.SeqListRef<uint>>(L, 2)) 
                {
                    CFUtilPoolLib.SeqListRef<uint> _seqListRef0;translator.Get(L, 1, out _seqListRef0);
                    CFUtilPoolLib.SeqListRef<uint> _seqListRef1;translator.Get(L, 2, out _seqListRef1);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.EntryConditions( _seqListRef0, _seqListRef1 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 1&& translator.Assignable<CFUtilPoolLib.SeqListRef<uint>>(L, 1)) 
                {
                    CFUtilPoolLib.SeqListRef<uint> _seqListRef0;translator.Get(L, 1, out _seqListRef0);
                    
                        bool gen_ret = CFClient.UI.XItemHelper.EntryConditions( _seqListRef0 );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XItemHelper.EntryConditions!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SortReward_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XItemHelper.SortReward( _items );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}

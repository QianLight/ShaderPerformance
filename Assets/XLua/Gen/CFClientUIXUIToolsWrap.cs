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
    public class CFClientUIXUIToolsWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(CFClient.UI.XUITools);
			Utils.BeginObjectRegister(type, L, translator, 0, 2, 0, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SerializeCurve", _m_SerializeCurve);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UnSerializeCurve", _m_UnSerializeCurve);
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 95, 2, 2);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetVideoURL", _m_GetVideoURL_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSpecificView", _m_GetSpecificView_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSpecificViewInCache", _m_GetSpecificViewInCache_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsSpecificActive", _m_IsSpecificActive_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DeactivateSpecificView", _m_DeactivateSpecificView_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ActivateSpecificView", _m_ActivateSpecificView_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowTeamPrepareRoomView", _m_ShowTeamPrepareRoomView_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Back2MainView", _m_Back2MainView_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ResetState2HallSystem", _m_ResetState2HallSystem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "InformCache", _m_InformCache_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "FlushInCache", _m_FlushInCache_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Inform", _m_Inform_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Flush", _m_Flush_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Create", _m_Create_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateByGameObject", _m_CreateByGameObject_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateCommonSkin", _m_CreateCommonSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateCommonHandler", _m_CreateCommonHandler_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateGameObj", _m_CreateGameObj_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecycleGameObj", _m_RecycleGameObj_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowDialogEx", _m_ShowDialogEx_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowNoticeDialogEx", _m_ShowNoticeDialogEx_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowEquipHint", _m_ShowEquipHint_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowSuggestTag", _m_ShowSuggestTag_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowBossTag", _m_ShowBossTag_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowDesc", _m_ShowDesc_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CheckExitTeam", _m_CheckExitTeam_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToTeamMain", _m_ToTeamMain_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ToTeamMainAndCreateTeam", _m_ToTeamMainAndCreateTeam_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateTeam", _m_CreateTeam_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "OnInvitedToTeam", _m_OnInvitedToTeam_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetTeamStatus", _m_SetTeamStatus_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowOPToolTip", _m_ShowOPToolTip_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowOPToolTipOfStore", _m_ShowOPToolTipOfStore_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BackToTeam", _m_BackToTeam_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowSystemTipsByErrCode", _m_ShowSystemTipsByErrCode_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowSystemTipsByKey", _m_ShowSystemTipsByKey_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowSystemTips", _m_ShowSystemTips_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowAchivementTips", _m_ShowAchivementTips_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowCommonTimeOutTips", _m_ShowCommonTimeOutTips_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowCommonRewardDlg", _m_ShowCommonRewardDlg_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowCommonPartnerInReward", _m_ShowCommonPartnerInReward_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateFlagSkin", _m_CreateFlagSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Show3DHead", _m_Show3DHead_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Stop3DHead", _m_Stop3DHead_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowPartnerScoreChange", _m_ShowPartnerScoreChange_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlayVideo", _m_PlayVideo_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlayUISound", _m_PlayUISound_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowInfo", _m_ShowInfo_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "AdjustFloatWindow", _m_AdjustFloatWindow_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetItemIconFromLua", _m_SetItemIconFromLua_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateAvatarSkin", _m_CreateAvatarSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateAvatarSkinCommon", _m_CreateAvatarSkinCommon_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetAvatarSkinCommon", _m_SetAvatarSkinCommon_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecycleAvatarSkin", _m_RecycleAvatarSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowCardOfPos", _m_ShowCardOfPos_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowCard", _m_ShowCard_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowNotice", _m_ShowNotice_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ShowNoticePunish", _m_ShowNoticePunish_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreatePartnerData", _m_CreatePartnerData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecycleParnterData", _m_RecycleParnterData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreatePartnerSkin", _m_CreatePartnerSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RecyclePartnerSkin", _m_RecyclePartnerSkin_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlayUISFX", _m_PlayUISFX_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlaySFX", _m_PlaySFX_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "DestoryUISFX", _m_DestoryUISFX_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CreateGradeIcon", _m_CreateGradeIcon_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetActive", _m_SetActive_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetEnable", _m_SetEnable_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "Freeze", _m_Freeze_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "UnFreeze", _m_UnFreeze_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "WorldPositionToLocalPointInRectangle", _m_WorldPositionToLocalPointInRectangle_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "OpenTalk", _m_OpenTalk_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "OnTalkOption", _m_OnTalkOption_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "TryGetItemRecord", _m_TryGetItemRecord_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CleanItemData", _m_CleanItemData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CleanAllItemData", _m_CleanAllItemData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "CleanActiveViewItemData", _m_CleanActiveViewItemData_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "OpenLastItemTipWhenBack", _m_OpenLastItemTipWhenBack_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "OpenLastViewedItem", _m_OpenLastViewedItem_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "HideUIBySkill", _m_HideUIBySkill_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ClearHideUIFlag", _m_ClearHideUIFlag_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "HideUIBySkillResume", _m_HideUIBySkillResume_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetPartnerTexWithPRS", _m_SetPartnerTexWithPRS_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetPartnerFirstName", _m_GetPartnerFirstName_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetPartnerLastName", _m_GetPartnerLastName_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetTimeWithFormat", _m_GetTimeWithFormat_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetTodayDate", _m_GetTodayDate_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetCurWeekDay", _m_GetCurWeekDay_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetActiveSignTimes", _m_GetActiveSignTimes_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RemoveSystemEmoji", _m_RemoveSystemEmoji_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSeqUintListValue", _m_GetSeqUintListValue_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetSeqintListValue", _m_GetSeqintListValue_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "PlayTimeline", _m_PlayTimeline_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetSelectedPartner", _m_SetSelectedPartner_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "SceneBlur", _g_get_SceneBlur);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "UIBlur", _g_get_UIBlur);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "SceneBlur", _s_set_SceneBlur);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "UIBlur", _s_set_UIBlur);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					CFClient.UI.XUITools gen_ret = new CFClient.UI.XUITools();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetVideoURL_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _videoName = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = CFClient.UI.XUITools.GetVideoURL( _videoName );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSpecificView_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.GetSpecificView( _id );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSpecificViewInCache_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.GetSpecificViewInCache( _id );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsSpecificActive_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                        bool gen_ret = CFClient.UI.XUITools.IsSpecificActive( _id );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DeactivateSpecificView_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.DeactivateSpecificView( _id );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ActivateSpecificView_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.ActivateSpecificView( _id );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.XDisplayContext>(L, 2)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.XDisplayContext _context = (UnityEngine.CFUI.XDisplayContext)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XDisplayContext));
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.ActivateSpecificView( _id, _context );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ActivateSpecificView!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowTeamPrepareRoomView_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    bool _active = LuaAPI.lua_toboolean(L, 1);
                    
                    CFClient.UI.XUITools.ShowTeamPrepareRoomView( _active );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Back2MainView_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.Back2MainView(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ResetState2HallSystem_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.ResetState2HallSystem(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_InformCache_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.InformCache( _id );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    
                    CFClient.UI.XUITools.InformCache( _id, _val );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Collections.Generic.List<object>>(L, 2)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    System.Collections.Generic.List<object> _param = (System.Collections.Generic.List<object>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<object>));
                    
                    CFClient.UI.XUITools.InformCache( _id, _param );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    
                    CFClient.UI.XUITools.InformCache( _id, _val, _val1 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    
                    CFClient.UI.XUITools.InformCache( _id, _val, _val1, _val2 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    object _val3 = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.InformCache( _id, _val, _val1, _val2, _val3 );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.InformCache!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_FlushInCache_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.XDisplayContext _context = (UnityEngine.CFUI.XDisplayContext)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XDisplayContext));
                    
                    CFClient.UI.XUITools.FlushInCache( _id, _context );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Inform_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.Inform( _id );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<UnityEngine.CFUI.IUIDisplay>(L, 1)) 
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    
                    CFClient.UI.XUITools.Inform( _display );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _id, _val );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<System.Collections.Generic.List<object>>(L, 2)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    System.Collections.Generic.List<object> _param = (System.Collections.Generic.List<object>)translator.GetObject(L, 2, typeof(System.Collections.Generic.List<object>));
                    
                    CFClient.UI.XUITools.Inform( _id, _param );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.CFUI.IUIDisplay>(L, 1)&& translator.Assignable<object>(L, 2)) 
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    object _val = translator.GetObject(L, 2, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _display, _val );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _id, _val, _val1 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<UnityEngine.CFUI.IUIDisplay>(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)) 
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _display, _val, _val1 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _id, _val, _val1, _val2 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.CFUI.IUIDisplay>(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _display, _val, _val1, _val2 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    object _val3 = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _id, _val, _val1, _val2, _val3 );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& translator.Assignable<UnityEngine.CFUI.IUIDisplay>(L, 1)&& translator.Assignable<object>(L, 2)&& translator.Assignable<object>(L, 3)&& translator.Assignable<object>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    object _val = translator.GetObject(L, 2, typeof(object));
                    object _val1 = translator.GetObject(L, 3, typeof(object));
                    object _val2 = translator.GetObject(L, 4, typeof(object));
                    object _val3 = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.Inform( _display, _val, _val1, _val2, _val3 );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.Inform!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Flush_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.CFUI.IUIDisplay _display = (UnityEngine.CFUI.IUIDisplay)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.IUIDisplay));
                    UnityEngine.CFUI.XDisplayContext _context = (UnityEngine.CFUI.XDisplayContext)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.XDisplayContext));
                    
                    CFClient.UI.XUITools.Flush( _display, _context );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Create_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    bool _visible = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.CFUI.IUIDisplay gen_ret = CFClient.UI.XUITools.Create( _id, _container, _parent, _visible );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                        UnityEngine.CFUI.IUIDisplay gen_ret = CFClient.UI.XUITools.Create( _id, _container, _parent );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<System.Type>(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    bool _visible = LuaAPI.lua_toboolean(L, 4);
                    
                        CFClient.UI.XUIDisplay gen_ret = CFClient.UI.XUITools.Create( _type, _container, _parent, _visible );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<System.Type>(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.XUIDisplay gen_ret = CFClient.UI.XUITools.Create( _type, _container, _parent );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 4&& translator.Assignable<System.Type>(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.GameObject>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 3, typeof(UnityEngine.GameObject));
                    bool _visible = LuaAPI.lua_toboolean(L, 4);
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.Create( _type, _container, _go, _visible );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& translator.Assignable<System.Type>(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.GameObject>(L, 3)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 3, typeof(UnityEngine.GameObject));
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.Create( _type, _container, _go );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.Create!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateByGameObject_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.GameObject>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 3, typeof(UnityEngine.GameObject));
                    bool _visible = LuaAPI.lua_toboolean(L, 4);
                    
                        UnityEngine.CFUI.IUIDisplay gen_ret = CFClient.UI.XUITools.CreateByGameObject( _id, _container, _go, _visible );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.GameObject>(L, 3)) 
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 3, typeof(UnityEngine.GameObject));
                    
                        UnityEngine.CFUI.IUIDisplay gen_ret = CFClient.UI.XUITools.CreateByGameObject( _id, _container, _go );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.CreateByGameObject!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateCommonSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _type = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.XSkin gen_ret = CFClient.UI.XUITools.CreateCommonSkin( _type, _parent );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateCommonHandler_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    bool _visible = LuaAPI.lua_toboolean(L, 4);
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.CreateCommonHandler( _hash, _container, _parent, _visible );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.CFUI.IUIContainer>(L, 2)&& translator.Assignable<UnityEngine.Transform>(L, 3)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.CFUI.IUIContainer _container = (UnityEngine.CFUI.IUIContainer)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.IUIContainer));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 3, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.XUIBase gen_ret = CFClient.UI.XUITools.CreateCommonHandler( _hash, _container, _parent );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.CreateCommonHandler!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateGameObj_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.XSkinCreator gen_ret = CFClient.UI.XUITools.CreateGameObj( _name, _parent );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleGameObj_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.XSkinCreator _obj = (CFClient.UI.XSkinCreator)translator.GetObject(L, 1, typeof(CFClient.UI.XSkinCreator));
                    
                    CFClient.UI.XUITools.RecycleGameObj( _obj );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowDialogEx_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 2)&& translator.Assignable<object>(L, 3)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 2);
                    object _value = translator.GetObject(L, 3, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _sHandler, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 2)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 2);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _sHandler );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 3);
                    object _value = translator.GetObject(L, 4, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandler, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 3);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandler );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    bool _noClose = LuaAPI.lua_toboolean(L, 3);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 4);
                    object _value = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _noClose, _sHandler, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    bool _noClose = LuaAPI.lua_toboolean(L, 3);
                    System.Action<object> _sHandler = translator.GetDelegate<System.Action<object>>(L, 4);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _noClose, _sHandler );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<object>(L, 4)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 2);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    object _value = translator.GetObject(L, 4, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _sHandle, _eHandle, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 2);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _sHandle, _eHandle );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    object _value = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandle, _eHandle, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandle, _eHandle );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    string _tipContent = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    object _value = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _tipContent, _sHandle, _eHandle, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    string _tipContent = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _tipContent, _sHandle, _eHandle );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 6&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<bool>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _onToggle = translator.GetDelegate<System.Action<bool>>(L, 2);
                    System.Action<object> _onCheck = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _onCancel = translator.GetDelegate<System.Action<object>>(L, 4);
                    bool _single = LuaAPI.lua_toboolean(L, 5);
                    string _tipContent = LuaAPI.lua_tostring(L, 6);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _onToggle, _onCheck, _onCancel, _single, _tipContent );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 6&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<System.Action<object>>(L, 5)&& translator.Assignable<object>(L, 6)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    System.Action<object> _cHandle = translator.GetDelegate<System.Action<object>>(L, 5);
                    object _value = translator.GetObject(L, 6, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandle, _eHandle, _cHandle, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<System.Action<object>>(L, 5)) 
                {
                    uint _hash = LuaAPI.xlua_touint(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 4);
                    System.Action<object> _cHandle = translator.GetDelegate<System.Action<object>>(L, 5);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _hash, _content, _sHandle, _eHandle, _cHandle );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 8&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<bool>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 8)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _onToggle = translator.GetDelegate<System.Action<bool>>(L, 2);
                    System.Action<object> _onCheck = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _onCancel = translator.GetDelegate<System.Action<object>>(L, 4);
                    bool _single = LuaAPI.lua_toboolean(L, 5);
                    string _tipContent = LuaAPI.lua_tostring(L, 6);
                    bool _toggleOn = LuaAPI.lua_toboolean(L, 7);
                    uint _showTodayTipBySystemID = LuaAPI.xlua_touint(L, 8);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _onToggle, _onCheck, _onCancel, _single, _tipContent, _toggleOn, _showTodayTipBySystemID );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 7&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<bool>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 7)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _onToggle = translator.GetDelegate<System.Action<bool>>(L, 2);
                    System.Action<object> _onCheck = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _onCancel = translator.GetDelegate<System.Action<object>>(L, 4);
                    bool _single = LuaAPI.lua_toboolean(L, 5);
                    string _tipContent = LuaAPI.lua_tostring(L, 6);
                    bool _toggleOn = LuaAPI.lua_toboolean(L, 7);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _onToggle, _onCheck, _onCancel, _single, _tipContent, _toggleOn );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 6&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<bool>>(L, 2)&& translator.Assignable<System.Action<object>>(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 5)&& (LuaAPI.lua_isnil(L, 6) || LuaAPI.lua_type(L, 6) == LuaTypes.LUA_TSTRING)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _onToggle = translator.GetDelegate<System.Action<bool>>(L, 2);
                    System.Action<object> _onCheck = translator.GetDelegate<System.Action<object>>(L, 3);
                    System.Action<object> _onCancel = translator.GetDelegate<System.Action<object>>(L, 4);
                    bool _single = LuaAPI.lua_toboolean(L, 5);
                    string _tipContent = LuaAPI.lua_tostring(L, 6);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _onToggle, _onCheck, _onCancel, _single, _tipContent );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 8&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 6)&& translator.Assignable<System.Action<object>>(L, 7)&& translator.Assignable<object>(L, 8)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    string _okTip = LuaAPI.lua_tostring(L, 2);
                    string _cancleTip = LuaAPI.lua_tostring(L, 3);
                    string _okTitle = LuaAPI.lua_tostring(L, 4);
                    string _cancleTitle = LuaAPI.lua_tostring(L, 5);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 6);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 7);
                    object _value = translator.GetObject(L, 8, typeof(object));
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _okTip, _cancleTip, _okTitle, _cancleTitle, _sHandle, _eHandle, _value );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 7&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 5) || LuaAPI.lua_type(L, 5) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<object>>(L, 6)&& translator.Assignable<System.Action<object>>(L, 7)) 
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    string _okTip = LuaAPI.lua_tostring(L, 2);
                    string _cancleTip = LuaAPI.lua_tostring(L, 3);
                    string _okTitle = LuaAPI.lua_tostring(L, 4);
                    string _cancleTitle = LuaAPI.lua_tostring(L, 5);
                    System.Action<object> _sHandle = translator.GetDelegate<System.Action<object>>(L, 6);
                    System.Action<object> _eHandle = translator.GetDelegate<System.Action<object>>(L, 7);
                    
                    CFClient.UI.XUITools.ShowDialogEx( _content, _okTip, _cancleTip, _okTitle, _cancleTitle, _sHandle, _eHandle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowDialogEx!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowNoticeDialogEx_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _content = LuaAPI.lua_tostring(L, 1);
                    System.Action<object> _onCheck = translator.GetDelegate<System.Action<object>>(L, 2);
                    System.Action<object> _onCancel = translator.GetDelegate<System.Action<object>>(L, 3);
                    bool _single = LuaAPI.lua_toboolean(L, 4);
                    string _Titletext = LuaAPI.lua_tostring(L, 5);
                    
                    CFClient.UI.XUITools.ShowNoticeDialogEx( _content, _onCheck, _onCancel, _single, _Titletext );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowEquipHint_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _tip = LuaAPI.lua_tostring(L, 1);
                    uint _itemId = LuaAPI.xlua_touint(L, 2);
                    uint _count = LuaAPI.xlua_touint(L, 3);
                    System.Action<object> _certaionCallBack = translator.GetDelegate<System.Action<object>>(L, 4);
                    
                    CFClient.UI.XUITools.ShowEquipHint( _tip, _itemId, _count, _certaionCallBack );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowSuggestTag_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _tagID = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.ShowSuggestTag( _tagID );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    uint _tagID = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    
                    CFClient.UI.XUITools.ShowSuggestTag( _tagID, _pos );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowSuggestTag!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowBossTag_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>>(L, 1)) 
                {
                    System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData> _bossTags = (System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>));
                    
                    CFClient.UI.XUITools.ShowBossTag( _bossTags );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>>(L, 1)&& translator.Assignable<UnityEngine.Vector3>(L, 2)) 
                {
                    System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData> _bossTags = (System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFUtilPoolLib.BossTag.RowData>));
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    
                    CFClient.UI.XUITools.ShowBossTag( _bossTags, _pos );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowBossTag!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowDesc_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _title = LuaAPI.lua_tostring(L, 1);
                    string _content = LuaAPI.lua_tostring(L, 2);
                    UnityEngine.Vector3 _pos;translator.Get(L, 3, out _pos);
                    
                    CFClient.UI.XUITools.ShowDesc( _title, _content, _pos );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CheckExitTeam_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Action _action = translator.GetDelegate<System.Action>(L, 1);
                    
                        bool gen_ret = CFClient.UI.XUITools.CheckExitTeam( _action );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToTeamMain_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _sceneID = LuaAPI.xlua_tointeger(L, 1);
                    uint _typeID = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.ToTeamMain( _sceneID, _typeID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToTeamMainAndCreateTeam_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _sceneID = LuaAPI.xlua_tointeger(L, 1);
                    uint _typeID = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.ToTeamMainAndCreateTeam( _sceneID, _typeID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateTeam_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _typeID = LuaAPI.xlua_touint(L, 1);
                    uint _sceneID = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.CreateTeam( _typeID, _sceneID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnInvitedToTeam_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _inviterName = LuaAPI.lua_tostring(L, 1);
                    uint _sceneID = LuaAPI.xlua_touint(L, 2);
                    uint _teamID = LuaAPI.xlua_touint(L, 3);
                    ulong _id = LuaAPI.lua_touint64(L, 4);
                    
                    CFClient.UI.XUITools.OnInvitedToTeam( _inviterName, _sceneID, _teamID, _id );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetTeamStatus_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    bool _active = LuaAPI.lua_toboolean(L, 1);
                    uint _typeID = LuaAPI.xlua_touint(L, 2);
                    uint _sceneID = LuaAPI.xlua_touint(L, 3);
                    bool _create = LuaAPI.lua_toboolean(L, 4);
                    
                    CFClient.UI.XUITools.SetTeamStatus( _active, _typeID, _sceneID, _create );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    bool _active = LuaAPI.lua_toboolean(L, 1);
                    uint _typeID = LuaAPI.xlua_touint(L, 2);
                    uint _sceneID = LuaAPI.xlua_touint(L, 3);
                    
                    CFClient.UI.XUITools.SetTeamStatus( _active, _typeID, _sceneID );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    bool _active = LuaAPI.lua_toboolean(L, 1);
                    uint _typeID = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.SetTeamStatus( _active, _typeID );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)) 
                {
                    bool _active = LuaAPI.lua_toboolean(L, 1);
                    
                    CFClient.UI.XUITools.SetTeamStatus( _active );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.SetTeamStatus!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowOPToolTip_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _itemId = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.ShowOPToolTip( _itemId );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<CFClient.UI.ItemTipParam>(L, 2)&& translator.Assignable<CFClient.XItem>(L, 3)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    CFClient.UI.ItemTipParam _param = (CFClient.UI.ItemTipParam)translator.GetObject(L, 2, typeof(CFClient.UI.ItemTipParam));
                    CFClient.XItem _comparaItem = (CFClient.XItem)translator.GetObject(L, 3, typeof(CFClient.XItem));
                    
                    CFClient.UI.XUITools.ShowOPToolTip( _item, _param, _comparaItem );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<CFClient.XItem>(L, 1)&& translator.Assignable<CFClient.UI.ItemTipParam>(L, 2)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    CFClient.UI.ItemTipParam _param = (CFClient.UI.ItemTipParam)translator.GetObject(L, 2, typeof(CFClient.UI.ItemTipParam));
                    
                    CFClient.UI.XUITools.ShowOPToolTip( _item, _param );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<CFClient.XItem>(L, 1)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    
                    CFClient.UI.XUITools.ShowOPToolTip( _item );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowOPToolTip!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowOPToolTipOfStore_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& translator.Assignable<CFClient.XItem>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& translator.Assignable<CFClient.XItem>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 5)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    bool _mShowBtn = LuaAPI.lua_toboolean(L, 2);
                    CFClient.XItem _comparaItem = (CFClient.XItem)translator.GetObject(L, 3, typeof(CFClient.XItem));
                    bool _showGetWayBtn = LuaAPI.lua_toboolean(L, 4);
                    uint _prefabID = LuaAPI.xlua_touint(L, 5);
                    
                    CFClient.UI.XUITools.ShowOPToolTipOfStore( _item, _mShowBtn, _comparaItem, _showGetWayBtn, _prefabID );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& translator.Assignable<CFClient.XItem>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& translator.Assignable<CFClient.XItem>(L, 3)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 4)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    bool _mShowBtn = LuaAPI.lua_toboolean(L, 2);
                    CFClient.XItem _comparaItem = (CFClient.XItem)translator.GetObject(L, 3, typeof(CFClient.XItem));
                    bool _showGetWayBtn = LuaAPI.lua_toboolean(L, 4);
                    
                    CFClient.UI.XUITools.ShowOPToolTipOfStore( _item, _mShowBtn, _comparaItem, _showGetWayBtn );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& translator.Assignable<CFClient.XItem>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& translator.Assignable<CFClient.XItem>(L, 3)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    bool _mShowBtn = LuaAPI.lua_toboolean(L, 2);
                    CFClient.XItem _comparaItem = (CFClient.XItem)translator.GetObject(L, 3, typeof(CFClient.XItem));
                    
                    CFClient.UI.XUITools.ShowOPToolTipOfStore( _item, _mShowBtn, _comparaItem );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<CFClient.XItem>(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    bool _mShowBtn = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.ShowOPToolTipOfStore( _item, _mShowBtn );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<CFClient.XItem>(L, 1)) 
                {
                    CFClient.XItem _item = (CFClient.XItem)translator.GetObject(L, 1, typeof(CFClient.XItem));
                    
                    CFClient.UI.XUITools.ShowOPToolTipOfStore( _item );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowOPToolTipOfStore!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BackToTeam_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.BackToTeam(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowSystemTipsByErrCode_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _errorStr = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowSystemTipsByErrCode( _errorStr );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowSystemTipsByKey_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    string _key = LuaAPI.lua_tostring(L, 1);
                    bool _defaultDuration = LuaAPI.lua_toboolean(L, 2);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 3);
                    
                    CFClient.UI.XUITools.ShowSystemTipsByKey( _key, _defaultDuration, _duration );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    string _key = LuaAPI.lua_tostring(L, 1);
                    bool _defaultDuration = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.ShowSystemTipsByKey( _key, _defaultDuration );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _key = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowSystemTipsByKey( _key );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowSystemTipsByKey!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowSystemTips_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _text = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowSystemTips( _text );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    string _text = LuaAPI.lua_tostring(L, 1);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    CFClient.UI.XUITools.ShowSystemTips( _text, _duration );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    string _text = LuaAPI.lua_tostring(L, 1);
                    float _fromY = (float)LuaAPI.lua_tonumber(L, 2);
                    float _toY = (float)LuaAPI.lua_tonumber(L, 3);
                    
                    CFClient.UI.XUITools.ShowSystemTips( _text, _fromY, _toY );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowSystemTips!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowAchivementTips_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    string _text = LuaAPI.lua_tostring(L, 1);
                    float _duration = (float)LuaAPI.lua_tonumber(L, 2);
                    
                    CFClient.UI.XUITools.ShowAchivementTips( _text, _duration );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _text = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowAchivementTips( _text );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowAchivementTips!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowCommonTimeOutTips_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.ShowCommonTimeOutTips(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowCommonRewardDlg_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)&& translator.Assignable<CFClient.UI.CFCommonRewardView.CloseCallBack>(L, 2)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    CFClient.UI.CFCommonRewardView.CloseCallBack _callback = translator.GetDelegate<CFClient.UI.CFCommonRewardView.CloseCallBack>(L, 2);
                    
                    CFClient.UI.XUITools.ShowCommonRewardDlg( _items, _callback );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XUITools.ShowCommonRewardDlg( _items );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowCommonRewardDlg!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowCommonPartnerInReward_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)&& translator.Assignable<System.Action>(L, 2)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    System.Action _onComplate = translator.GetDelegate<System.Action>(L, 2);
                    
                    CFClient.UI.XUITools.ShowCommonPartnerInReward( _items, _onComplate );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& translator.Assignable<System.Collections.Generic.List<CFClient.XItem>>(L, 1)) 
                {
                    System.Collections.Generic.List<CFClient.XItem> _items = (System.Collections.Generic.List<CFClient.XItem>)translator.GetObject(L, 1, typeof(System.Collections.Generic.List<CFClient.XItem>));
                    
                    CFClient.UI.XUITools.ShowCommonPartnerInReward( _items );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowCommonPartnerInReward!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateFlagSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _holder = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.ISkinItem gen_ret = CFClient.UI.XUITools.CreateFlagSkin( _name, _holder );
                        translator.PushAny(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Show3DHead_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _id = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.Show3DHead( _id );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Stop3DHead_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.Stop3DHead(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowPartnerScoreChange_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    bool _show = LuaAPI.lua_toboolean(L, 1);
                    uint _oldScore = LuaAPI.xlua_touint(L, 2);
                    uint _newScore = LuaAPI.xlua_touint(L, 3);
                    
                    CFClient.UI.XUITools.ShowPartnerScoreChange( _show, _oldScore, _newScore );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    bool _show = LuaAPI.lua_toboolean(L, 1);
                    uint _oldScore = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.ShowPartnerScoreChange( _show, _oldScore );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)) 
                {
                    bool _show = LuaAPI.lua_toboolean(L, 1);
                    
                    CFClient.UI.XUITools.ShowPartnerScoreChange( _show );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowPartnerScoreChange!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayVideo_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)&& translator.Assignable<object>(L, 5)) 
                {
                    string _videoOriginalName = LuaAPI.lua_tostring(L, 1);
                    string _videoName = LuaAPI.lua_tostring(L, 2);
                    bool _fullname = LuaAPI.lua_toboolean(L, 3);
                    System.Action<object> _endCall = translator.GetDelegate<System.Action<object>>(L, 4);
                    object _param = translator.GetObject(L, 5, typeof(object));
                    
                    CFClient.UI.XUITools.PlayVideo( _videoOriginalName, _videoName, _fullname, _endCall, _param );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action<object>>(L, 4)) 
                {
                    string _videoOriginalName = LuaAPI.lua_tostring(L, 1);
                    string _videoName = LuaAPI.lua_tostring(L, 2);
                    bool _fullname = LuaAPI.lua_toboolean(L, 3);
                    System.Action<object> _endCall = translator.GetDelegate<System.Action<object>>(L, 4);
                    
                    CFClient.UI.XUITools.PlayVideo( _videoOriginalName, _videoName, _fullname, _endCall );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    string _videoOriginalName = LuaAPI.lua_tostring(L, 1);
                    string _videoName = LuaAPI.lua_tostring(L, 2);
                    bool _fullname = LuaAPI.lua_toboolean(L, 3);
                    
                    CFClient.UI.XUITools.PlayVideo( _videoOriginalName, _videoName, _fullname );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _videoOriginalName = LuaAPI.lua_tostring(L, 1);
                    string _videoName = LuaAPI.lua_tostring(L, 2);
                    
                    CFClient.UI.XUITools.PlayVideo( _videoOriginalName, _videoName );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.PlayVideo!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayUISound_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    bool _stopall = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.PlayUISound( _name, _stopall );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)) 
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.PlayUISound( _name );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.PlayUISound!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowInfo_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowInfo( _str );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AdjustFloatWindow_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    UnityEngine.CFUI.CFSimpleLoopScrollRect _scroll = (UnityEngine.CFUI.CFSimpleLoopScrollRect)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFSimpleLoopScrollRect));
                    
                        float gen_ret = CFClient.UI.XUITools.AdjustFloatWindow( _tf, _scroll );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetItemIconFromLua_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.CFUI.CFImage _ico = (UnityEngine.CFUI.CFImage)translator.GetObject(L, 1, typeof(UnityEngine.CFUI.CFImage));
                    uint _itemid = LuaAPI.xlua_touint(L, 2);
                    
                    CFClient.UI.XUITools.SetItemIconFromLua( _ico, _itemid );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateAvatarSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _sceneId = LuaAPI.xlua_touint(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    ulong _rId = LuaAPI.lua_touint64(L, 3);
                    uint _avatarId = LuaAPI.xlua_touint(L, 4);
                    uint _frameId = LuaAPI.xlua_touint(L, 5);
                    bool _isself = LuaAPI.lua_toboolean(L, 6);
                    int _level = LuaAPI.xlua_tointeger(L, 7);
                    bool _registerCallback = LuaAPI.lua_toboolean(L, 8);
                    bool _isRed = LuaAPI.lua_toboolean(L, 9);
                    
                        CFClient.UI.XAvatarImageSkinWithFrame gen_ret = CFClient.UI.XUITools.CreateAvatarSkin( _sceneId, _parent, _rId, _avatarId, _frameId, _isself, _level, _registerCallback, _isRed );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateAvatarSkinCommon_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    ulong _rId = LuaAPI.lua_touint64(L, 2);
                    uint _avatarId = LuaAPI.xlua_touint(L, 3);
                    uint _frameId = LuaAPI.xlua_touint(L, 4);
                    bool _isself = LuaAPI.lua_toboolean(L, 5);
                    int _level = LuaAPI.xlua_tointeger(L, 6);
                    bool _registerCallback = LuaAPI.lua_toboolean(L, 7);
                    bool _isRed = LuaAPI.lua_toboolean(L, 8);
                    
                        CFClient.UI.XAvatarImageSkinWithFrame gen_ret = CFClient.UI.XUITools.CreateAvatarSkinCommon( _parent, _rId, _avatarId, _frameId, _isself, _level, _registerCallback, _isRed );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetAvatarSkinCommon_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.XAvatarImageSkinWithFrame _skin = (CFClient.UI.XAvatarImageSkinWithFrame)translator.GetObject(L, 1, typeof(CFClient.UI.XAvatarImageSkinWithFrame));
                    ulong _rId = LuaAPI.lua_touint64(L, 2);
                    uint _avatarId = LuaAPI.xlua_touint(L, 3);
                    uint _frameId = LuaAPI.xlua_touint(L, 4);
                    bool _isself = LuaAPI.lua_toboolean(L, 5);
                    int _level = LuaAPI.xlua_tointeger(L, 6);
                    bool _registerCallback = LuaAPI.lua_toboolean(L, 7);
                    bool _isRed = LuaAPI.lua_toboolean(L, 8);
                    
                        CFClient.UI.XAvatarImageSkinWithFrame gen_ret = CFClient.UI.XUITools.SetAvatarSkinCommon( _skin, _rId, _avatarId, _frameId, _isself, _level, _registerCallback, _isRed );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleAvatarSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.XAvatarImageSkinWithFrame _skin = (CFClient.UI.XAvatarImageSkinWithFrame)translator.GetObject(L, 1, typeof(CFClient.UI.XAvatarImageSkinWithFrame));
                    bool _recycle = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.RecycleAvatarSkin( _skin, _recycle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowCardOfPos_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1) || LuaAPI.lua_isuint64(L, 1))&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)&& translator.Assignable<System.Action>(L, 4)) 
                {
                    ulong _roleId = LuaAPI.lua_touint64(L, 1);
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    int _global = LuaAPI.xlua_tointeger(L, 3);
                    System.Action _action = translator.GetDelegate<System.Action>(L, 4);
                    
                    CFClient.UI.XUITools.ShowCardOfPos( _roleId, _pos, _global, _action );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 3&& (LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1) || LuaAPI.lua_isuint64(L, 1))&& translator.Assignable<UnityEngine.Vector3>(L, 2)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 3)) 
                {
                    ulong _roleId = LuaAPI.lua_touint64(L, 1);
                    UnityEngine.Vector3 _pos;translator.Get(L, 2, out _pos);
                    int _global = LuaAPI.xlua_tointeger(L, 3);
                    
                    CFClient.UI.XUITools.ShowCardOfPos( _roleId, _pos, _global );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.ShowCardOfPos!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowCard_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    ulong _roleId = LuaAPI.lua_touint64(L, 1);
                    
                    CFClient.UI.XUITools.ShowCard( _roleId );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowNotice_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _codeStr = LuaAPI.lua_tostring(L, 1);
                    
                    CFClient.UI.XUITools.ShowNotice( _codeStr );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ShowNoticePunish_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _seconds = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.ShowNoticePunish( _seconds );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreatePartnerData_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _partnerId = LuaAPI.xlua_touint(L, 1);
                    uint _level = LuaAPI.xlua_touint(L, 2);
                    uint _star = LuaAPI.xlua_touint(L, 3);
                    
                        CFClient.XPartnerData gen_ret = CFClient.UI.XUITools.CreatePartnerData( _partnerId, _level, _star );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleParnterData_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.XPartnerData _data = (CFClient.XPartnerData)translator.GetObject(L, 1, typeof(CFClient.XPartnerData));
                    
                    CFClient.UI.XUITools.RecycleParnterData( _data );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreatePartnerSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.XPartnerData _data = (CFClient.XPartnerData)translator.GetObject(L, 1, typeof(CFClient.XPartnerData));
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFClient.UI.OPPartnerSmallSkin gen_ret = CFClient.UI.XUITools.CreatePartnerSkin( _data, _parent );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecyclePartnerSkin_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.OPPartnerSmallSkin _skin = (CFClient.UI.OPPartnerSmallSkin)translator.GetObject(L, 1, typeof(CFClient.UI.OPPartnerSmallSkin));
                    bool _recycle = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.RecyclePartnerSkin( _skin, _recycle );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayUISFX_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _tr = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFEngine.SFX gen_ret = CFClient.UI.XUITools.PlayUISFX( _name, _tr );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlaySFX_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 2)) 
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _tr = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        CFEngine.SFX gen_ret = CFClient.UI.XUITools.PlaySFX( _name, _tr );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 1) || LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TSTRING)&& translator.Assignable<UnityEngine.Transform>(L, 2)&& translator.Assignable<CFEngine.LoadSFXFinish>(L, 3)) 
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    UnityEngine.Transform _tr = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    CFEngine.LoadSFXFinish _cb = translator.GetDelegate<CFEngine.LoadSFXFinish>(L, 3);
                    
                        CFEngine.SFX gen_ret = CFClient.UI.XUITools.PlaySFX( _name, _tr, _cb );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.PlaySFX!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DestoryUISFX_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFEngine.SFX _sfx = (CFEngine.SFX)translator.GetObject(L, 1, typeof(CFEngine.SFX));
                    
                    CFClient.UI.XUITools.DestoryUISFX( _sfx );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateGradeIcon_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    float _point = (float)LuaAPI.lua_tonumber(L, 1);
                    UnityEngine.Transform _parent = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                    CFClient.UI.XUITools.CreateGradeIcon( _point, _parent );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetActive_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.GameObject _go = (UnityEngine.GameObject)translator.GetObject(L, 1, typeof(UnityEngine.GameObject));
                    bool _value = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.SetActive( _go, _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetEnable_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Behaviour _c = (UnityEngine.Behaviour)translator.GetObject(L, 1, typeof(UnityEngine.Behaviour));
                    bool _value = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.SetEnable( _c, _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Freeze_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    float _time = (float)LuaAPI.lua_tonumber(L, 1);
                    
                    CFClient.UI.XUITools.Freeze( _time );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 0) 
                {
                    
                    CFClient.UI.XUITools.Freeze(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.Freeze!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UnFreeze_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.UnFreeze(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_WorldPositionToLocalPointInRectangle_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Vector3 _worldPosition;translator.Get(L, 1, out _worldPosition);
                    UnityEngine.Transform _targetUITransform = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                        UnityEngine.Vector3 gen_ret = CFClient.UI.XUITools.WorldPositionToLocalPointInRectangle( _worldPosition, _targetUITransform );
                        translator.PushUnityEngineVector3(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OpenTalk_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.XUITools.TalkParam _param;translator.Get(L, 1, out _param);
                    
                    CFClient.UI.XUITools.OpenTalk( _param );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnTalkOption_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& translator.Assignable<System.Action>(L, 1)) 
                {
                    System.Action _action = translator.GetDelegate<System.Action>(L, 1);
                    
                    CFClient.UI.XUITools.OnTalkOption( _action );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 0) 
                {
                    
                    CFClient.UI.XUITools.OnTalkOption(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.OnTalkOption!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_TryGetItemRecord_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    CFClient.UI.XUITools.ItemRecord _record = (CFClient.UI.XUITools.ItemRecord)translator.GetObject(L, 2, typeof(CFClient.UI.XUITools.ItemRecord));
                    
                        bool gen_ret = CFClient.UI.XUITools.TryGetItemRecord( _prefabID, ref _record );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    translator.Push(L, _record);
                        
                    
                    
                    
                    return 2;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanItemData_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.CleanItemData( _prefabID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanAllItemData_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.CleanAllItemData(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CleanActiveViewItemData_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.CleanActiveViewItemData(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OpenLastItemTipWhenBack_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    ulong _guid = LuaAPI.lua_touint64(L, 2);
                    ulong _itemid = LuaAPI.lua_touint64(L, 3);
                    bool _showGetWay = LuaAPI.lua_toboolean(L, 4);
                    
                    CFClient.UI.XUITools.OpenLastItemTipWhenBack( _prefabID, _guid, _itemid, _showGetWay );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OpenLastViewedItem_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _prefabID = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.OpenLastViewedItem( _prefabID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HideUIBySkill_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFClient.UI.XUITools.HideUIBySkillFlag _flag;translator.Get(L, 1, out _flag);
                    
                        int gen_ret = CFClient.UI.XUITools.HideUIBySkill( _flag );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearHideUIFlag_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                    CFClient.UI.XUITools.ClearHideUIFlag(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_HideUIBySkillResume_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _keyindex = LuaAPI.xlua_tointeger(L, 1);
                    
                    CFClient.UI.XUITools.HideUIBySkillResume( _keyindex );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetPartnerTexWithPRS_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    UnityEngine.Transform _tf = (UnityEngine.Transform)translator.GetObject(L, 1, typeof(UnityEngine.Transform));
                    uint _partnerId = LuaAPI.xlua_touint(L, 2);
                    CFClient.UI.TextureType _type;translator.Get(L, 3, out _type);
                    
                    CFClient.UI.XUITools.SetPartnerTexWithPRS( _tf, _partnerId, _type );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetPartnerFirstName_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _partnerID = LuaAPI.xlua_touint(L, 1);
                    
                        string gen_ret = CFClient.UI.XUITools.GetPartnerFirstName( _partnerID );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetPartnerLastName_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _partnerID = LuaAPI.xlua_touint(L, 1);
                    
                        string gen_ret = CFClient.UI.XUITools.GetPartnerLastName( _partnerID );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetTimeWithFormat_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    uint _time = LuaAPI.xlua_touint(L, 1);
                    
                        string gen_ret = CFClient.UI.XUITools.GetTimeWithFormat( _time );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetTodayDate_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                        string gen_ret = CFClient.UI.XUITools.GetTodayDate(  );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetCurWeekDay_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    
                        int gen_ret = CFClient.UI.XUITools.GetCurWeekDay(  );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetActiveSignTimes_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _RefreshWeekDay = LuaAPI.xlua_tointeger(L, 1);
                    
                        int gen_ret = CFClient.UI.XUITools.GetActiveSignTimes( _RefreshWeekDay );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RemoveSystemEmoji_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 1);
                    
                        string gen_ret = CFClient.UI.XUITools.RemoveSystemEmoji( _str );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSeqUintListValue_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFUtilPoolLib.SeqListRef<uint> _seq;translator.Get(L, 1, out _seq);
                    int _x = LuaAPI.xlua_tointeger(L, 2);
                    int _y = LuaAPI.xlua_tointeger(L, 3);
                    
                        uint gen_ret = CFClient.UI.XUITools.GetSeqUintListValue( _seq, _x, _y );
                        LuaAPI.xlua_pushuint(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetSeqintListValue_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    CFUtilPoolLib.SeqListRef<int> _seq;translator.Get(L, 1, out _seq);
                    int _x = LuaAPI.xlua_tointeger(L, 2);
                    int _y = LuaAPI.xlua_tointeger(L, 3);
                    
                        int gen_ret = CFClient.UI.XUITools.GetSeqintListValue( _seq, _x, _y );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PlayTimeline_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _name = LuaAPI.lua_tostring(L, 1);
                    System.Action _action = translator.GetDelegate<System.Action>(L, 2);
                    
                    CFClient.UI.XUITools.PlayTimeline( _name, _action );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetSelectedPartner_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    uint _partnerID = LuaAPI.xlua_touint(L, 1);
                    bool _openPartnerView = LuaAPI.lua_toboolean(L, 2);
                    
                    CFClient.UI.XUITools.SetSelectedPartner( _partnerID, _openPartnerView );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 1)) 
                {
                    uint _partnerID = LuaAPI.xlua_touint(L, 1);
                    
                    CFClient.UI.XUITools.SetSelectedPartner( _partnerID );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to CFClient.UI.XUITools.SetSelectedPartner!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SerializeCurve(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFClient.UI.XUITools gen_to_be_invoked = (CFClient.UI.XUITools)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.AnimationCurve _curve = (UnityEngine.AnimationCurve)translator.GetObject(L, 2, typeof(UnityEngine.AnimationCurve));
                    
                        string gen_ret = gen_to_be_invoked.SerializeCurve( _curve );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UnSerializeCurve(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                CFClient.UI.XUITools gen_to_be_invoked = (CFClient.UI.XUITools)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _curveStr = LuaAPI.lua_tostring(L, 2);
                    
                        UnityEngine.AnimationCurve gen_ret = gen_to_be_invoked.UnSerializeCurve( _curveStr );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_SceneBlur(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.xlua_pushinteger(L, CFClient.UI.XUITools.SceneBlur);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_UIBlur(RealStatePtr L)
        {
		    try {
            
			    LuaAPI.xlua_pushinteger(L, CFClient.UI.XUITools.UIBlur);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_SceneBlur(RealStatePtr L)
        {
		    try {
                
			    CFClient.UI.XUITools.SceneBlur = (byte)LuaAPI.xlua_tointeger(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_UIBlur(RealStatePtr L)
        {
		    try {
                
			    CFClient.UI.XUITools.UIBlur = (byte)LuaAPI.xlua_tointeger(L, 1);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

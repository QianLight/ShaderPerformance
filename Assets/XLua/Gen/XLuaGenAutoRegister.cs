#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Collections.Generic;
using System.Reflection;


namespace XLua.CSObjectWrap
{
    public class XLua_Gen_Initer_Register__
	{
        
        
        static void wrapInit0(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(Hotfix), HotfixWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngineObjectExtention), UnityEngineObjectExtentionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(object), SystemObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(System.IO.MemoryStream), SystemIOMemoryStreamWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Object), UnityEngineObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector2), UnityEngineVector2Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector3), UnityEngineVector3Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Vector4), UnityEngineVector4Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Quaternion), UnityEngineQuaternionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Color), UnityEngineColorWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Ray), UnityEngineRayWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Bounds), UnityEngineBoundsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Time), UnityEngineTimeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.GameObject), UnityEngineGameObjectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Behaviour), UnityEngineBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Transform), UnityEngineTransformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.TextAsset), UnityEngineTextAssetWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Keyframe), UnityEngineKeyframeWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.AnimationCurve), UnityEngineAnimationCurveWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.AnimationClip), UnityEngineAnimationClipWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.MonoBehaviour), UnityEngineMonoBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.ParticleSystem), UnityEngineParticleSystemWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Animator), UnityEngineAnimatorWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(System.Net.WebRequest), SystemNetWebRequestWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Mathf), UnityEngineMathfWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(System.Collections.Generic.List<int>), SystemCollectionsGenericList_1_SystemInt32_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Debug), UnityEngineDebugWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Application), UnityEngineApplicationWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.AnimatorStateInfo), UnityEngineAnimatorStateInfoWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.RectTransform), UnityEngineRectTransformWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CanvasGroup), UnityEngineCanvasGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.Events.UnityEvent<UnityEngine.Vector2>), UnityEngineEventsUnityEvent_1_UnityEngineVector2_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFImage), UnityEngineCFUICFImageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFText), UnityEngineCFUICFTextWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFInput), UnityEngineCFUICFInputWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFRawImage), UnityEngineCFUICFRawImageWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.Empty4Raycast), UnityEngineCFUIEmpty4RaycastWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFToggle), UnityEngineCFUICFToggleWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFButton), UnityEngineCFUICFButtonWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFSlider), UnityEngineCFUICFSliderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.Empty4DragRaycast), UnityEngineCFUIEmpty4DragRaycastWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFEventSystems.UIBehaviour), UnityEngineCFEventSystemsUIBehaviourWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFEventSystems.CFEventSystem), UnityEngineCFEventSystemsCFEventSystemWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XAnimation), UnityEngineCFUIXAnimationWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XAnimationGroup), UnityEngineCFUIXAnimationGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XTextGroup), UnityEngineCFUIXTextGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XDisplayContext), UnityEngineCFUIXDisplayContextWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.UIUtils), UnityEngineCFUIUIUtilsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFToggleGroup), UnityEngineCFUICFToggleGroupWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFSequenceFrameEx), UnityEngineCFUICFSequenceFrameExWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.LongPressButton), UnityEngineCFUILongPressButtonWrap.__Register);
        
        }
        
        static void wrapInit1(LuaEnv luaenv, ObjectTranslator translator)
        {
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.SeqListRef<uint>), CFUtilPoolLibSeqListRef_1_SystemUInt32_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.SeqListRef<float>), CFUtilPoolLibSeqListRef_1_SystemSingle_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.SeqRef<float>), CFUtilPoolLibSeqRef_1_SystemSingle_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.SeqRef<uint>), CFUtilPoolLibSeqRef_1_SystemUInt32_Wrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.XTimerMgrExtension), CFUtilPoolLibXTimerMgrExtensionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFUtilPoolLib.XCommon), CFUtilPoolLibXCommonWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFWrapContent), UnityEngineCFUICFWrapContentWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFWrapContentWithSkin), UnityEngineCFUICFWrapContentWithSkinWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFLoopScrollRectControl), UnityEngineCFUICFLoopScrollRectControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFScrollRect), UnityEngineCFUICFScrollRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFLoopScrollRect), UnityEngineCFUICFLoopScrollRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFSimpleLoopScrollRect), UnityEngineCFUICFSimpleLoopScrollRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XSRControlNormal), UnityEngineCFUIXSRControlNormalWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFLoopInsideScrollRect), UnityEngineCFUICFLoopInsideScrollRectWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFAnimUnit), UnityEngineCFUICFAnimUnitWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFAnimation), UnityEngineCFUICFAnimationWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XAnimUnit), UnityEngineCFUIXAnimUnitWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFUIDummy), UnityEngineCFUICFUIDummyWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFUIPool), UnityEngineCFUICFUIPoolWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XUIHeader), UnityEngineCFUIXUIHeaderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.XSfxGroupPlay), UnityEngineCFUIXSfxGroupPlayWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFEngine.EngineUtility), CFEngineEngineUtilityWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(UnityEngine.CFUI.CFLayoutRebuilder), UnityEngineCFUICFLayoutRebuilderWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.CFTitanHandler), CFClientUICFTitanHandlerWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XItemCommControl), CFClientUIXItemCommControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XTabElementBase), CFClientUIXTabElementBaseWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XCommonTabSystemControl), CFClientUIXCommonTabSystemControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XCommonTabCommonControl), CFClientUIXCommonTabCommonControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XCopyCenterTabControl), CFClientUIXCopyCenterTabControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XFleetBossCenterTabControl), CFClientUIXFleetBossCenterTabControlWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XUITools), CFClientUIXUIToolsWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XSystemHelper), CFClientUIXSystemHelperWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.XStringDefineProxy), CFClientXStringDefineProxyWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.XGlobalConfigExtension), CFClientXGlobalConfigExtensionWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XItemHelper), CFClientUIXItemHelperWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.XSkinParam), CFClientUIXSkinParamWrap.__Register);
        
        
            translator.DelayWrapLoader(typeof(CFClient.UI.ItemTipParam), CFClientUIItemTipParamWrap.__Register);
        
        
        
        }
        
        static void Init(LuaEnv luaenv, ObjectTranslator translator)
        {
            
            wrapInit0(luaenv, translator);
            
            wrapInit1(luaenv, translator);
            
            
            translator.AddInterfaceBridgeCreator(typeof(System.Collections.IEnumerator), SystemCollectionsIEnumeratorBridge.__Create);
            
        }
        
	    static XLua_Gen_Initer_Register__()
        {
		    XLua.LuaEnv.AddIniter(Init);
		}
		
		
	}
	
}
namespace XLua
{
	public partial class ObjectTranslator
	{
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ s_gen_reg_dumb_obj = new XLua.CSObjectWrap.XLua_Gen_Initer_Register__();
		static XLua.CSObjectWrap.XLua_Gen_Initer_Register__ gen_reg_dumb_obj {get{return s_gen_reg_dumb_obj;}}
	}
	
	internal partial class InternalGlobals
    {
	    
		delegate bool __GEN_DELEGATE0( UnityEngine.Object o);
		
	    static InternalGlobals()
		{
		    extensionMethodMap = new Dictionary<Type, IEnumerable<MethodInfo>>()
			{
			    
				{typeof(UnityEngine.Object), new List<MethodInfo>(){
				
				  new __GEN_DELEGATE0(UnityEngineObjectExtention.IsNull)
#if UNITY_WSA && !UNITY_EDITOR
                                      .GetMethodInfo(),
#else
                                      .Method,
#endif
				
				}},
				
			};
			
			genTryArrayGetPtr = StaticLuaCallbacks.__tryArrayGet;
            genTryArraySetPtr = StaticLuaCallbacks.__tryArraySet;
		}
	}
}

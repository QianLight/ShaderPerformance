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
    public class UnityEngineCFUICFInputWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(UnityEngine.CFUI.CFInput);
			Utils.BeginObjectRegister(type, L, translator, 0, 22, 41, 30);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetTextWithoutNotify", _m_SetTextWithoutNotify);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RegisterBeyondLines", _m_RegisterBeyondLines);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "MoveTextEnd", _m_MoveTextEnd);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "MoveTextStart", _m_MoveTextStart);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnBeginDrag", _m_OnBeginDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnDrag", _m_OnDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnEndDrag", _m_OnEndDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerDown", _m_OnPointerDown);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ProcessEvent", _m_ProcessEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnUpdateSelected", _m_OnUpdateSelected);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ForceLabelUpdate", _m_ForceLabelUpdate);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Rebuild", _m_Rebuild);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LayoutComplete", _m_LayoutComplete);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GraphicUpdateComplete", _m_GraphicUpdateComplete);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ActivateInputField", _m_ActivateInputField);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnSelect", _m_OnSelect);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnPointerClick", _m_OnPointerClick);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DeactivateInputField", _m_DeactivateInputField);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnDeselect", _m_OnDeselect);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnSubmit", _m_OnSubmit);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CalculateLayoutInputHorizontal", _m_CalculateLayoutInputHorizontal);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CalculateLayoutInputVertical", _m_CalculateLayoutInputVertical);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "shouldHideMobileInput", _g_get_shouldHideMobileInput);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "shouldActivateOnSelect", _g_get_shouldActivateOnSelect);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "text", _g_get_text);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "isFocused", _g_get_isFocused);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "caretBlinkRate", _g_get_caretBlinkRate);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "caretWidth", _g_get_caretWidth);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "textComponent", _g_get_textComponent);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "placeholder", _g_get_placeholder);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "caretColor", _g_get_caretColor);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "customCaretColor", _g_get_customCaretColor);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "selectionColor", _g_get_selectionColor);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "onEndEdit", _g_get_onEndEdit);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "onValueChanged", _g_get_onValueChanged);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "onValidateInput", _g_get_onValidateInput);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "onOverrideInput", _g_get_onOverrideInput);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "characterLimit", _g_get_characterLimit);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "contentType", _g_get_contentType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "lineType", _g_get_lineType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "inputType", _g_get_inputType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "touchScreenKeyboard", _g_get_touchScreenKeyboard);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "keyboardType", _g_get_keyboardType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "characterValidation", _g_get_characterValidation);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "readOnly", _g_get_readOnly);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "multiLine", _g_get_multiLine);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "asteriskChar", _g_get_asteriskChar);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "wasCanceled", _g_get_wasCanceled);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "caretPosition", _g_get_caretPosition);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "selectionAnchorPosition", _g_get_selectionAnchorPosition);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "selectionFocusPosition", _g_get_selectionFocusPosition);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "minWidth", _g_get_minWidth);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "preferredWidth", _g_get_preferredWidth);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "flexibleWidth", _g_get_flexibleWidth);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "minHeight", _g_get_minHeight);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "preferredHeight", _g_get_preferredHeight);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "flexibleHeight", _g_get_flexibleHeight);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "layoutPriority", _g_get_layoutPriority);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_Keyboard", _g_get_m_Keyboard);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "LinesLimit", _g_get_LinesLimit);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_checkLimitOnceWhenCopy", _g_get_m_checkLimitOnceWhenCopy);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_replaceEmoji", _g_get_m_replaceEmoji);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "m_onValidateInputCheckOnce", _g_get_m_onValidateInputCheckOnce);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "shouldHideMobileInput", _s_set_shouldHideMobileInput);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "shouldActivateOnSelect", _s_set_shouldActivateOnSelect);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "text", _s_set_text);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "caretBlinkRate", _s_set_caretBlinkRate);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "caretWidth", _s_set_caretWidth);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "textComponent", _s_set_textComponent);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "placeholder", _s_set_placeholder);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "caretColor", _s_set_caretColor);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "customCaretColor", _s_set_customCaretColor);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "selectionColor", _s_set_selectionColor);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "onEndEdit", _s_set_onEndEdit);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "onValueChanged", _s_set_onValueChanged);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "onValidateInput", _s_set_onValidateInput);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "onOverrideInput", _s_set_onOverrideInput);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "characterLimit", _s_set_characterLimit);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "contentType", _s_set_contentType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "lineType", _s_set_lineType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "inputType", _s_set_inputType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "keyboardType", _s_set_keyboardType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "characterValidation", _s_set_characterValidation);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "readOnly", _s_set_readOnly);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "asteriskChar", _s_set_asteriskChar);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "caretPosition", _s_set_caretPosition);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "selectionAnchorPosition", _s_set_selectionAnchorPosition);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "selectionFocusPosition", _s_set_selectionFocusPosition);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_Keyboard", _s_set_m_Keyboard);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "LinesLimit", _s_set_LinesLimit);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_checkLimitOnceWhenCopy", _s_set_m_checkLimitOnceWhenCopy);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_replaceEmoji", _s_set_m_replaceEmoji);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "m_onValidateInputCheckOnce", _s_set_m_onValidateInputCheckOnce);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "UnityEngine.CFUI.CFInput does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetTextWithoutNotify(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _input = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.SetTextWithoutNotify( _input );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterBeyondLines(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action _act = translator.GetDelegate<System.Action>(L, 2);
                    int _lineMax = LuaAPI.xlua_tointeger(L, 3);
                    
                    gen_to_be_invoked.RegisterBeyondLines( _act, _lineMax );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MoveTextEnd(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _shift = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.MoveTextEnd( _shift );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MoveTextStart(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    bool _shift = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.MoveTextStart( _shift );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnBeginDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnBeginDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnEndDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnEndDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnPointerDown(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerDown( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ProcessEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Event _e = (UnityEngine.Event)translator.GetObject(L, 2, typeof(UnityEngine.Event));
                    
                    gen_to_be_invoked.ProcessEvent( _e );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnUpdateSelected(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.BaseEventData _eventData = (UnityEngine.CFEventSystems.BaseEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.BaseEventData));
                    
                    gen_to_be_invoked.OnUpdateSelected( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ForceLabelUpdate(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ForceLabelUpdate(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Rebuild(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFUI.CanvasUpdate _update;translator.Get(L, 2, out _update);
                    
                    gen_to_be_invoked.Rebuild( _update );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LayoutComplete(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.LayoutComplete(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GraphicUpdateComplete(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.GraphicUpdateComplete(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ActivateInputField(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ActivateInputField(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnSelect(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.BaseEventData _eventData = (UnityEngine.CFEventSystems.BaseEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.BaseEventData));
                    
                    gen_to_be_invoked.OnSelect( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnPointerClick(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.PointerEventData _eventData = (UnityEngine.CFEventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnPointerClick( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DeactivateInputField(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.DeactivateInputField(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnDeselect(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.BaseEventData _eventData = (UnityEngine.CFEventSystems.BaseEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.BaseEventData));
                    
                    gen_to_be_invoked.OnDeselect( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnSubmit(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.CFEventSystems.BaseEventData _eventData = (UnityEngine.CFEventSystems.BaseEventData)translator.GetObject(L, 2, typeof(UnityEngine.CFEventSystems.BaseEventData));
                    
                    gen_to_be_invoked.OnSubmit( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CalculateLayoutInputHorizontal(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CalculateLayoutInputHorizontal(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CalculateLayoutInputVertical(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.CalculateLayoutInputVertical(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_shouldHideMobileInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.shouldHideMobileInput);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_shouldActivateOnSelect(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.shouldActivateOnSelect);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_text(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.text);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_isFocused(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.isFocused);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_caretBlinkRate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.caretBlinkRate);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_caretWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.caretWidth);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_textComponent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.textComponent);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_placeholder(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.placeholder);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_caretColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineColor(L, gen_to_be_invoked.caretColor);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_customCaretColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.customCaretColor);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_selectionColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.PushUnityEngineColor(L, gen_to_be_invoked.selectionColor);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onEndEdit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.onEndEdit);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onValueChanged(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.onValueChanged);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onValidateInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.onValidateInput);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_onOverrideInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.onOverrideInput);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_characterLimit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.characterLimit);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_contentType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.contentType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_lineType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.lineType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_inputType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.inputType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_touchScreenKeyboard(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.touchScreenKeyboard);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_keyboardType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.keyboardType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_characterValidation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.characterValidation);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_readOnly(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.readOnly);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_multiLine(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.multiLine);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_asteriskChar(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.asteriskChar);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_wasCanceled(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.wasCanceled);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_caretPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.caretPosition);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_selectionAnchorPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.selectionAnchorPosition);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_selectionFocusPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.selectionFocusPosition);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_minWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.minWidth);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_preferredWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.preferredWidth);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_flexibleWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.flexibleWidth);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_minHeight(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.minHeight);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_preferredHeight(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.preferredHeight);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_flexibleHeight(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.flexibleHeight);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_layoutPriority(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.layoutPriority);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_Keyboard(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.m_Keyboard);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_LinesLimit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.LinesLimit);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_checkLimitOnceWhenCopy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.m_checkLimitOnceWhenCopy);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_replaceEmoji(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.m_replaceEmoji);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_m_onValidateInputCheckOnce(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.m_onValidateInputCheckOnce);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_shouldHideMobileInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.shouldHideMobileInput = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_shouldActivateOnSelect(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.shouldActivateOnSelect = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_text(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.text = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_caretBlinkRate(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.caretBlinkRate = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_caretWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.caretWidth = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_textComponent(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.textComponent = (UnityEngine.CFUI.CFText)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFText));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_placeholder(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.placeholder = (UnityEngine.CFUI.Graphic)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.Graphic));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_caretColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.Color gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.caretColor = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_customCaretColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.customCaretColor = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_selectionColor(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.Color gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.selectionColor = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onEndEdit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.onEndEdit = (UnityEngine.CFUI.CFInput.SubmitEvent)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFInput.SubmitEvent));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onValueChanged(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.onValueChanged = (UnityEngine.CFUI.CFInput.OnChangeEvent)translator.GetObject(L, 2, typeof(UnityEngine.CFUI.CFInput.OnChangeEvent));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onValidateInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.onValidateInput = translator.GetDelegate<UnityEngine.CFUI.CFInput.OnValidateInput>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_onOverrideInput(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.onOverrideInput = translator.GetDelegate<UnityEngine.CFUI.CFInput.OnOverrideInput>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_characterLimit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.characterLimit = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_contentType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFInput.ContentType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.contentType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_lineType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFInput.LineType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.lineType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_inputType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFInput.InputType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.inputType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_keyboardType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.TouchScreenKeyboardType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.keyboardType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_characterValidation(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                UnityEngine.CFUI.CFInput.CharacterValidation gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.characterValidation = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_readOnly(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.readOnly = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_asteriskChar(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.asteriskChar = (char)LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_caretPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.caretPosition = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_selectionAnchorPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.selectionAnchorPosition = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_selectionFocusPosition(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.selectionFocusPosition = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_Keyboard(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_Keyboard = (UnityEngine.TouchScreenKeyboard)translator.GetObject(L, 2, typeof(UnityEngine.TouchScreenKeyboard));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_LinesLimit(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.LinesLimit = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_checkLimitOnceWhenCopy(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_checkLimitOnceWhenCopy = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_replaceEmoji(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_replaceEmoji = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_m_onValidateInputCheckOnce(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                UnityEngine.CFUI.CFInput gen_to_be_invoked = (UnityEngine.CFUI.CFInput)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.m_onValidateInputCheckOnce = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}

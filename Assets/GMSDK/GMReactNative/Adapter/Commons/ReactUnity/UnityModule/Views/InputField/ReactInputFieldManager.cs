/*
 * @Author: hexiaonuo
 * @Date: 2021-10-27
 * @Description: input component
 * @FilePath: ReactUnity/UnityModule/Views/InputField/ReactInputFieldManager.cs
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GSDK.RNU
{
    public class ReactInputFieldManager : BaseViewManager
    {
        public static string viewName = "RNUInputField";
        
        public override string GetName()
        {
            return viewName;
        }
        
        protected override BaseView createViewInstance()
        {
            return new ReactInputField(viewName);
        }
        
         
        
        public override ReactSimpleShadowNode CreateShadowNodeInstance(int tag)
        {
            return new ReactInputFieldShadowNode(tag);
        }
        // Update text settings
        // text placeHolder
        public override void UpdateProperties(BaseView viewToUpdate, Dictionary<string, object> props)
        {
            ReactInputField textField = (ReactInputField)viewToUpdate;

            // 添加 text placeholder 字体属性的更新
            TextGenerationSettings textSettings = (TextGenerationSettings) props["textGenerationSettings"];
            props.Remove("textGenerationSettings");
            textField.UpdateBySettings(textSettings);
            base.UpdateProperties(viewToUpdate, props);
        }
        
        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable{
                {"onChangeText", new Hashtable{
                    {sRegistration, "onChangeText"}
                }},
                {"onEditEndText", new Hashtable{
                    {sRegistration, "onEditEndText"}
                }},
            };
        }
        
        
        [ReactProp(name = "isMaskable")]
        public void SetMaskable(ReactInputField view, bool isMaskable)
        {
            view.SetMaskable(isMaskable);
        }
        
        [ReactProp(name = "value")]
        public void SetTextContent(ReactInputField view, string content)
        {
            view.SetText(content);
        }
        
        [ReactProp(name = "placeholder")]
        public void SetPlaceHolderContent(ReactInputField view, string content)
        {
            view.SetPlaceHolderText(content);
        }

        [ReactProp(name = "characterLimit")]
        public void SetCharacterLimit(ReactInputField view, int limit)
        {
            view.SetCharacterLimit(limit);
        }
        
        [ReactProp(name = "multiline", defaultBoolean = false)]
        public void SetMultiline(ReactInputField view, bool multiline)
        {
            view.SetMultiline(multiline);
        }
        
        [ReactProp(name = "overflow")]
        public override void setOverflow(BaseView view, string overflow)
        {
            throw new Exception("TextInput do not support set overflow !!!");
        }
        
        
        [ReactProp(name = "overflowMask")]
        public override void setOverflowMask(BaseView view, string overflow)
        {
            throw new Exception("TextInput do not support set overflow Mask !!!");
        }
        
        [ReactProp(name = "disabled", defaultBoolean = false)]
        public void SetDisable(BaseView view, bool disable)
        {
            ReactInputField input = (ReactInputField) view;
            input.SetDisable(disable);
        }

        [ReactCommand]
        public void focus(BaseView view)
        {
            EventSystem.current.SetSelectedGameObject(view.GetGameObject());
        }
        
        [ReactCommand]
        public void blur(BaseView _)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        [ReactCommand]
        public void isFocused(BaseView view, Promise promise)
        {
            // TODO promise的resovle/reject需要间隔至少一帧。
            StaticCommonScript.StaticStartCoroutine(GetFocusedAndReturn(view, promise));
        }

        private IEnumerator GetFocusedAndReturn(BaseView view, Promise promise)
        {
            yield return null;
            promise.Resolve(EventSystem.current.currentSelectedGameObject == view.GetGameObject());
        }

    }
}
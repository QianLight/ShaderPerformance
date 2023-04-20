/*
 * @Author: hexiaonuo
 * @Date: 2021-10-27
 * @Description: input component
 * @FilePath: ReactUnity/UnityModule/Views/InputField/ReactinputField.cs
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GSDK.RNU
{
    public class ReactInputField : SimpleBaseView
    {
        private GameObject realGameObject;

        private GameObject text;
        private GameObject placeHolder;
        
        public static string textName = "Text";
        public static string placeHolderName = "PlaceHolder";

        private string placeText = "";
        
        // 事件触发屏蔽标识位
        private bool eventTrigger = true;
        
        // ---------
        // SimpleBaseView
        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public override GameObject GetContentObject()
        {
            throw new Exception("TextInput do not support child node !!!");
        }

        // text 设置需要屏蔽监听事件
        public void SetText(string content)
        {
            if (realGameObject == null || realGameObject.GetComponent<InputField>() == null)
            {
                Util.Log("InputField component is null, set text failed, return");
                return;
            }

            eventTrigger = false;
            
            if (placeHolder != null && placeHolder.GetComponent<Text>() != null)
            {
                placeHolder.GetComponent<Text>().text = "";
            }
            realGameObject.GetComponent<InputField>().text = content;

            eventTrigger = true;
        }

        public void SetPlaceHolderText(string content)
        {                
            if (placeHolder == null || placeHolder.GetComponent<Text>() == null)
            {
                Util.Log("input placeHolder is null, set place text failed, return");
                return;
            }

            placeHolder.GetComponent<Text>().text = content;
            placeText = content;
        }
        
        
        
        
        // ----------
        // ReactInputField
        public ReactInputField(string name)
        {
            realGameObject = new GameObject(name, typeof(InputField), typeof(Image));
            
            realGameObject.AddComponent<LoadScript>();
            
            placeHolder = new GameObject(placeHolderName, typeof(Text));
            placeHolder.transform.SetParent(realGameObject.transform, false);

            text = new GameObject(textName,typeof(Text));
            text.transform.SetParent(realGameObject.transform, false);
            
            RectTransform placeHolderRect = placeHolder.GetComponent<RectTransform>();
            placeHolderRect.anchorMin = new Vector2(0, 0);
            placeHolderRect.anchorMax = new Vector2(1, 1);
            placeHolderRect.pivot = new Vector2(0.5f, 0.5f);
            placeHolderRect.offsetMin = new Vector2(0, 0);
            placeHolderRect.offsetMax = new Vector2(0, 0);
            
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(0,0);
            textRect.offsetMax = new Vector2(0, 0);


            FillInputField();
            
        }

        public void FillInputField()
        {
            InputField input = realGameObject.GetComponent<InputField>();
            if (input == null)
            {
                Util.Log("inputField is null, return");
                return;
            }

            input.onValidateInput += (str, index, addChar) =>
            {
                var uniCodeCategory = char.GetUnicodeCategory(addChar);
                switch (uniCodeCategory)
                {
                    case UnicodeCategory.OtherSymbol:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.ModifierSymbol:
                    case UnicodeCategory.NonSpacingMark:
                    {
                        return char.MinValue;
                    }
                    default:
                    {
                        return addChar;
                    }
                }
            };
            input.targetGraphic = realGameObject.GetComponent<Image>();
            input.textComponent = text.GetComponent<Text>();
            input.onValueChanged.AddListener(ValueChange);
            // input.onEndEdit.AddListener(ValueEnd);

            Image img = realGameObject.GetComponent<Image>();
            if (img == null)
            {
                Util.Log("image is null, return");
                return;
            }
            // 默认设置为 透明 背景
            img.color = new Color(255, 255, 255, 0);
        }

        public void UpdatePlaceHolder(string content)
        {
            if (placeHolder != null && placeHolder.GetComponent<Text>() != null)
            {
                if (content == null || content == "")
                {
                    placeHolder.GetComponent<Text>().text = placeText;
                }
                else
                {
                    placeHolder.GetComponent<Text>().text = "";
                }
            }
        }
        public void ValueEnd(string content)
        {
            if (!eventTrigger)
            {
                return;
            }
            UpdatePlaceHolder(content);
            
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add("onEditEndText");
            args.Add(new Hashtable
            {
                {"value", content}
            });

            Util.Log("onvalue editEndText..........{0}", content);
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
        public void ValueChange(string content)
        {
            if (!eventTrigger)
            {
                return;
            }
            UpdatePlaceHolder(content);

            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add("onChangeText");
            args.Add(new Hashtable
            {
                {"value", content}
            });

            Util.Log("onvalue Changed..........{0}", content);
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }

        // update
        public void UpdateBySettings(TextGenerationSettings textSettings)
        {
            if (text == null || text.GetComponent<Text>() == null)
            {
                Util.Log("text is null, update setting failed, return");
                return;
            }
            Text tc = text.GetComponent<Text>();
            tc.fontSize = textSettings.fontSize;
            tc.color = textSettings.color;
            tc.alignment = textSettings.textAnchor;
            tc.lineSpacing = textSettings.lineSpacing;
            tc.font = textSettings.font;
            tc.fontStyle = textSettings.fontStyle;
            
            if (placeHolder == null || placeHolder.GetComponent<Text>() == null)
            {
                Util.Log("placeHolder is null, update setting not container");
            }
            else
            {
                Text pc = placeHolder.GetComponent<Text>();
                pc.fontSize = textSettings.fontSize;
                pc.color = textSettings.color;
                pc.alignment = textSettings.textAnchor;
                pc.lineSpacing = textSettings.lineSpacing;
                pc.font = textSettings.font;
                pc.fontStyle = textSettings.fontStyle;
            }
        }
        
       // prop 
       public void SetMaskable(bool isMaskable)
       {
           if (text == null || text.GetComponent<Text>() == null)
           {
               Util.Log("text is null, SetMaskable failed, return");
               return;
           }
           text.GetComponent<Text>().maskable = isMaskable;

           if (placeHolder == null || placeHolder.GetComponent<Text>() == null)
           {
               Util.Log("placeHolder is null, SetMaskable failed, return");
               return;
           }
           placeHolder.GetComponent<Text>().maskable = isMaskable;

       }

       public void SetCharacterLimit(int limit)
       {
           if (realGameObject == null || realGameObject.GetComponent<InputField>() == null)
           {
               Util.Log("get input component is null, return");
               return;
           }

           limit = limit < 0 ? 0 : limit;
           realGameObject.GetComponent<InputField>().characterLimit = limit;
       }

       public void SetMultiline(bool multiline)
       {
           if (realGameObject == null || realGameObject.GetComponent<InputField>() == null)
           {
               Util.Log("get input component is null, return");
               return;
           }

           var lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
           realGameObject.GetComponent<InputField>().lineType = lineType;
       }

       public void SetDisable(bool disable)
       {
           InputField input = realGameObject.GetComponent<InputField>();
           if (input != null)
           {
               input.interactable = !disable;
           }
       }
       
       public override void Destroy()
        {
            Object.Destroy(text);
            Object.Destroy(placeHolder);
            base.Destroy();
        }
    }
}
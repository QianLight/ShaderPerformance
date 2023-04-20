using System;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU {
    public class ReactText: SimpleBaseView {
        private GameObject realGameObject;

        public ReactText(string name, bool isRich)
        {
            realGameObject = new GameObject(name);

            Text tc = realGameObject.AddComponent<Text>();

            // 不可以修改，如果要修改要和 settings 一起修改。
            tc.raycastTarget = false;
            tc.maskable = true;
            tc.supportRichText = isRich;
            tc.verticalOverflow = VerticalWrapMode.Overflow;
            tc.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        public override GameObject GetGameObject() {
            return realGameObject;
        }
        
        public override GameObject GetContentObject()
        {
            throw new Exception("Text do not support child node !!!");
        }

        public void UpdateBySettings(TextGenerationSettings settings) {
            Text tc = realGameObject.GetComponent<Text>();

            if (tc.fontSize != settings.fontSize) {
                tc.fontSize = settings.fontSize;
            }

            if (tc.color != settings.color) {
                tc.color = settings.color;
            }

            if (tc.alignment != settings.textAnchor) {
                tc.alignment = settings.textAnchor;
            }

            if (tc.lineSpacing != settings.lineSpacing) {
                tc.lineSpacing = settings.lineSpacing;
            }

            if (tc.font != settings.font) {
                tc.font = settings.font;
            }

            if (tc.fontStyle != settings.fontStyle) {
                tc.fontStyle = settings.fontStyle;
            }

            if (tc.resizeTextForBestFit != settings.resizeTextForBestFit)
            {
                tc.resizeTextForBestFit = settings.resizeTextForBestFit;
            }
            
            if (tc.resizeTextMaxSize != settings.resizeTextMaxSize)
            {
                tc.resizeTextMaxSize = settings.resizeTextMaxSize;
            }
            
            if (tc.resizeTextMinSize != settings.resizeTextMinSize)
            {
                tc.resizeTextMinSize = settings.resizeTextMinSize;
            }
        }

        public void SetText(string txt) {
            Text tc = realGameObject.GetComponent<Text>();
            tc.text = txt;
        }

        public void SetMaskable(bool isMaskable)
        {
            Text tc = realGameObject.GetComponent<Text>();
            tc.maskable = isMaskable;
        }
        
        public void setMaterial(string material)
        {                
            Text text = realGameObject.GetComponent<Text>();
            
            if (text == null)
            {
                Util.Log("text is null, setMaterial failed, return");
                return;
            }

            text.material = RNUMainCore.LoadMaterial(material);
        }
        
    }
}

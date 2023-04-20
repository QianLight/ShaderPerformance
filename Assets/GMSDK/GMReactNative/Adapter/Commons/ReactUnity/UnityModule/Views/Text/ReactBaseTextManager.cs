using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU
{
    public abstract class ReactBaseTextManager : BaseViewManager
    {
        
        [ReactProp(name = "accessible")]
        public void setAccessible(ReactText view, bool accessible)
        {
            return;
        }

        [ReactProp(name = "allowFontScaling")]
        public void setAllowFontScaling(ReactText view, bool allowFontScaling)
        {
            return;
        }

        [ReactProp(name = "ellipsizeMode")]
        public void setEllipsizeMode(ReactText view, string ellipsizeMode)
        {
            //TODO
        }

        [ReactProp(name = "isMaskable")]
        public void SetMaskable(ReactText view, bool isMaskable)
        {
            view.SetMaskable(isMaskable);
        }

        // text shadow参数，textShadowColor textShadowOffset textShadowOpacity
        [ReactProp(name = "textShadowColor", defaultLong = 0)]
        public void setTextShadowColor(BaseView view, long textShadowColor)
        {

            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {
                i = panel.AddComponent<Shadow>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = UnityUtils.GetColor(textShadowColor);
        }

        [ReactProp(name = "textShadowOffset")]
        public void setTextShadowOffset(BaseView view, Dictionary<string, object> textShadowOffset)
        {
            if (!textShadowOffset.ContainsKey("width") || !textShadowOffset.ContainsKey("height"))
            {
                Util.Log("Missing width or height, return");
                return;
            }
            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {
                i = panel.AddComponent<Shadow>();
            }
            float a = Convert.ToSingle(textShadowOffset["width"]);
            float b = Convert.ToSingle(textShadowOffset["height"]);
            i.effectDistance = new Vector2(a, -b);
        }

        [ReactProp(name = "textShadowOpacity", defaultFloat = 1.0F)]
        public void setTextShadowOpacity(BaseView view, float textShadowOpacity)
        {
            GameObject panel = view.GetGameObject();

            Shadow i = panel.GetComponent<Shadow>();
            if (i == null)
            {
                i = panel.AddComponent<Shadow>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = new Color(i.effectColor[0], i.effectColor[1], i.effectColor[2], textShadowOpacity);
        }
        //shadow end

        // text outline参数，textOutlineColor textOutlineOffset textOutlineOpacity
        [ReactProp(name = "textOutlineColor", defaultLong = 0)]
        public void setTextOutlineColor(BaseView view, long textOutlineColor)
        {
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {
                i = panel.AddComponent<Outline>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = UnityUtils.GetColor(textOutlineColor);
        }

        [ReactProp(name = "textOutlineOffset")]
        public void setTextOutlineOffset(BaseView view, Dictionary<string, object> textOutlineOffset)
        {
            if (!textOutlineOffset.ContainsKey("width") || !textOutlineOffset.ContainsKey("height"))
            {
                Util.Log("Missing width or height, return");
                return;
            }
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {
                i = panel.AddComponent<Outline>();
            }
            float a = Convert.ToSingle(textOutlineOffset["width"]);
            float b = Convert.ToSingle(textOutlineOffset["height"]);
            i.effectDistance = new Vector2(a, b);
        }

        [ReactProp(name = "textOutlineOpacity", defaultFloat = 1.0F)]
        public void setTextOutlineOpacity(BaseView view, float textOutlineOpacity)
        {
            GameObject panel = view.GetGameObject();

            Outline i = panel.GetComponent<Outline>();
            if (i == null)
            {
                i = panel.AddComponent<Outline>();
                i.effectDistance = new Vector2(0, 0);
            }
            i.effectColor = new Color(i.effectColor[0], i.effectColor[1], i.effectColor[2], textOutlineOpacity);
        }
        //outline end
        
        [ReactProp(name = "material")]
        public override void setMaterial(BaseView view,  string material)
        {
            ReactText text = (ReactText) view;
            text.setMaterial(material);
        }

        
        [ReactProp(name = "overflow")]
        public override void setOverflow(BaseView view, string overflow)
        {
            // todo 属性没有传进来
            throw new Exception("Text do not support set overflow !!!");
        }
        
        
        [ReactProp(name = "overflowMask")]
        public override void setOverflowMask(BaseView view, string overflow)
        {
            // todo 属性没有传进来
            throw new Exception("Text do not support set overflow Mask !!!");
        }
        
        [ReactProp(name = "backgroundColor", defaultInt = 0)]
        public override void setBackgroundColor(BaseView view, long backgroundColor)
        {
            //no-op
        }


        public override void UpdateProperties(BaseView viewToUpdate, Dictionary<string, object> props)
        {
            ReactText textToUpdate = (ReactText)viewToUpdate;
            
            TextGenerationSettings textSettings = (TextGenerationSettings)props["textGenerationSettings"];
            props.Remove("textGenerationSettings");
            textToUpdate.UpdateBySettings(textSettings);
            base.UpdateProperties(viewToUpdate, props);
        }
    }
}
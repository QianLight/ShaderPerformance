using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU
{
    public class ReactToggle:SimpleBaseView
    {
        public static string sOnValueChanged = "onValueChange";
        
        private GameObject realGameObject;
        private GameObject background;
        private GameObject checkmark;
        private Toggle toggle;
        private GameObject label;
        private bool hasListener;
        
        public ReactToggle(string name)
        {
            realGameObject = new GameObject(name);
            background = new GameObject("Background");
            background.transform.SetParent(realGameObject.transform, false);
            var bgTransform = background.AddComponent<RectTransform>();
            bgTransform.anchorMin = new Vector2(0.0f, 1f);
            bgTransform.anchorMax = new Vector2(0.0f, 1f);
            bgTransform.anchoredPosition = new Vector2(10f, -10f);
            bgTransform.sizeDelta = new Vector2(20f, 20f);
            
            checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform,false);
            var checkTransform = checkmark.AddComponent<RectTransform>();
            checkTransform.anchorMin = new Vector2(0.5f, 0.5f);
            checkTransform.anchorMax = new Vector2(0.5f, 0.5f);
            checkTransform.anchoredPosition = Vector2.zero;
            checkTransform.sizeDelta = new Vector2(20f, 20f);

            label = new GameObject("Label");
            label.transform.SetParent(realGameObject.transform,false);
            var labelTransform = label.AddComponent<RectTransform>();
            labelTransform.anchorMin = new Vector2(0.0f, 0.0f);
            labelTransform.anchorMax = new Vector2(1f, 1f);
            labelTransform.offsetMin = new Vector2(23f, 1f);
            labelTransform.offsetMax = new Vector2(-5f, -2f);            
            
            realGameObject.name = name;
            toggle = realGameObject.AddComponent<Toggle>();
            toggle.isOn = true;
 
            Text lbl = label.AddComponent<Text>();
            lbl.text = "Toggle";
            lbl.color = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);;
            lbl.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            Util.Log("ReactToggle constructor");
        }
        

        public void SetSelectedSource(string uri)
        {
            if (uri == "")
            {
                return;
            }
            
            Image image =checkmark.GetComponent<Image>();
            if (image == null)
            {
                image = checkmark.AddComponent<Image>();
            }
            
            toggle.graphic = (Graphic) image;
            StaticCommonScript.LoadTexture(uri, texture =>
            {   if(!image.IsDestroyed())
                    image.sprite = texture == null ? null : Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            });
            
        }
        
        public void SetUnselectedSource(string uri)
        {
            if (uri == "")
            {
                return;
            }

            Image image =background.GetComponent<Image>();
            if (image == null)
            {
                image = background.AddComponent<Image>();
            }

            toggle.targetGraphic = (Graphic) image;
            StaticCommonScript.LoadTexture(uri, texture =>
            {
                if(!image.IsDestroyed())
                {
                    image.sprite = texture == null
                        ? null
                        : Sprite.Create((Texture2D) texture, new Rect(0, 0, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f));
                    image.type = Image.Type.Sliced;
                }
                
            });
        }

        public void SetIsOn(bool isOn)
        {
            toggle.isOn = isOn;
        }

        public void SetLabel(Dictionary<string,object> props)
        {
            Text lbl = label.GetComponent<Text>();
            if (props.ContainsKey("text") && lbl.text != Convert.ToString(props["text"]))
            {
                lbl.text = Convert.ToString(props["text"]);    
            }
            if (props.ContainsKey("color") && lbl.color != UnityUtils.GetColor(Convert.ToInt64(props["color"])))
            {
                lbl.color = UnityUtils.GetColor(Convert.ToInt64(props["color"]));
            }
            if (props.ContainsKey("fontSize") && lbl.fontSize != Convert.ToInt32(props["fontSize"]))
            {
                lbl.fontSize = Convert.ToInt32(props["fontSize"]);    
            }
        }

        public void SetValueChangedListener(bool hasListener)
        {
            this.hasListener = hasListener;
            AddValueChangedRectEvent();
        }

        private void AddValueChangedRectEvent()
        {
            toggle.onValueChanged.RemoveAllListeners();
            if (hasListener)
            {
                toggle.onValueChanged.AddListener(OnValueChanged);    
            }
            
        }

        private void OnValueChanged(bool value)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(sOnValueChanged);
            args.Add(new Hashtable {
                {"value",value},
            });
            
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
        
        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public override void Destroy()
        {
            UnityEngine.Object.Destroy(toggle);
            UnityEngine.Object.Destroy(label);
            UnityEngine.Object.Destroy(background);
            UnityEngine.Object.Destroy(checkmark);

            base.Destroy();
        }
    }
}
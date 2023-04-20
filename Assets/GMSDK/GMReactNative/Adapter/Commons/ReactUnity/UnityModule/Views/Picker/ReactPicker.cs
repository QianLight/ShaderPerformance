using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU
{
    public class ReactPicker : SimpleBaseView
    {

        private GameObject realGameObject;

        public GameObject hand;

        private Dropdown dropdown;

        private Dropdown.OptionData Items;

        private Image arrow;

        private GameObject contentObject;

        private GameObject itemObject;

        public ReactPicker(string name)
        {
            DefaultControls.Resources uiResources = new DefaultControls.Resources();
            realGameObject = DefaultControls.CreateDropdown(uiResources);
            realGameObject.name = name;
            dropdown = realGameObject.GetComponent<Dropdown>();
            dropdown.ClearOptions();

            GameObject viewPortObject = dropdown.template.Find("Viewport").gameObject;
            contentObject = viewPortObject.transform.Find("Content").gameObject;
            itemObject = contentObject.transform.Find("Item").gameObject;

            //设置滚动条
            GameObject scrollbarObject = dropdown.template.Find("Scrollbar").gameObject;
            RectTransform scrollbar = scrollbarObject.GetComponent<RectTransform>();
            scrollbar.sizeDelta = new Vector2(4, float.NaN);//i dont know why

            GameObject arrowObject = dropdown.transform.Find("Arrow").gameObject;
            arrow = arrowObject.GetComponent<Image>();

            //设置onValueChange监听
            dropdown.onValueChanged.AddListener(delegate { ValueChangeEvent(dropdown); });
        }

        public void SetItems(ArrayList tempOptions)
        {
            dropdown.ClearOptions();

            List<string> options = new List<string>((string[])tempOptions.ToArray(typeof(string)));

            dropdown.AddOptions(options);
        }

        public void SetValue(int value)
        {
            dropdown.value = value;
        }

        public void SetEnabled(bool enabled)
        {
            dropdown.enabled = enabled;
        }

        public void SetArrowImage(string uri)
        {
            if (uri == "")
            {
                return;
            }
            if (arrow == null)
            {
                Util.LogError("arrow is null, return");
                return;
            }

            StaticCommonScript.LoadTexture(uri, texture =>
               {
                   if (!arrow.IsDestroyed())
                   {
                       arrow.sprite = texture == null ? null : Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                   }
               });
        }

        public void SetCaptionImage(string uri)
        {
            if (uri == "")
            {
                return;
            }

            Transform content = null, item = null;
            GameObject checkedMark = null;
            Transform viewport = dropdown.template.Find("Viewport");
            if (viewport != null) content = viewport.Find("Content");
            if (content != null) item = content.Find("Item");
            if (item != null) checkedMark = item.Find("Item Checkmark").gameObject;


            if (checkedMark == null)
            {
                Util.LogError("checkedMark is null, return");
                return;
            }

            Image checkedImage = checkedMark.GetComponent<Image>();

            if (checkedImage == null)
            {
                Util.Log("checkedImage is null, return");
                return;
            }

            StaticCommonScript.LoadTexture(uri, texture =>
               {
                   if (!checkedImage.IsDestroyed())
                   {
                       checkedImage.sprite = texture == null ? null : Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                   }
               });
        }

        public void SetItemTextColor(long textColor)
        {


            Transform content = null, item = null;
            GameObject checkedLabel = null;
            Transform viewport = dropdown.template.Find("Viewport");
            if (viewport != null) content = viewport.Find("Content");
            if (content != null) item = content.Find("Item");
            if (item != null) checkedLabel = item.Find("Item Label").gameObject;

            Text itemText = checkedLabel.GetComponent<Text>();

            if (itemText == null)
            {
                Util.Log("itemText is null, return");
                return;
            }
            itemText.color = UnityUtils.GetColor(textColor);
        }

        public void SetCaptionTextSize(int fontSize)
        {
            GameObject labelObject = dropdown.transform.Find("Label").gameObject;
            Text labelText = labelObject.GetComponent<Text>();
            labelText.fontSize = fontSize;
        }

        public void SetListHeight(int height)
        {
            RectTransform templateRectTransform = dropdown.template;
            Rect templateRect = templateRectTransform.rect;
            templateRectTransform.sizeDelta = new Vector2(templateRectTransform.sizeDelta.x, height);
        }

        public void SetItemHeight(int height)
        {
            RectTransform contentRectTransform = contentObject.GetComponent<RectTransform>();
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, 100);

            RectTransform itemRectTransform = itemObject.GetComponent<RectTransform>();
            itemRectTransform.sizeDelta = new Vector2(itemRectTransform.sizeDelta.x, 100);
        }

        public void SetCheckedSize(int size)
        {
            GameObject checkMarkObject = itemObject.transform.Find("Item Checkmark").gameObject;
            RectTransform checkMarkTransform = checkMarkObject.GetComponent<RectTransform>();
            checkMarkTransform.offsetMin = new Vector2(size / 2 + 5, 0);
            checkMarkTransform.sizeDelta = new Vector2(size, size);

            GameObject itemLabelObject = itemObject.transform.Find("Item Label").gameObject;
            RectTransform itemLabelTransform = itemLabelObject.GetComponent<RectTransform>();
            itemLabelTransform.offsetMin = new Vector2(size + 5, 0);
        }

        public void SetArrowSize(int size)
        {
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1.0f, 0.5f);
            arrowRect.anchorMax = new Vector2(1.0f, 0.5f);
            arrowRect.pivot = new Vector2(0.5f, 0.5f);
            arrowRect.offsetMin = new Vector2(size * -2 / 3, 0);
            arrowRect.offsetMax = new Vector2(0, 0);
            arrowRect.sizeDelta = new Vector2(size, size);
        }

        public void SetItemTextSize(int size)
        {
            GameObject itemLabelObject = itemObject.transform.Find("Item Label").gameObject;
            Text itemLabelText = itemLabelObject.GetComponent<Text>();
            itemLabelText.fontSize = size;
        }

        public void SetMode(string mode)
        {
            if (mode != "dropdown")
            {
                Util.Log("RU Picker only support dropdown, SetMode invalidly, return");
                return;
            }
        }


        public void SetDisable(bool disable)
        {
            if (dropdown != null)
            {
                dropdown.interactable = !disable;
            }
        }

        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public void ValueChangeEvent(Dropdown m_dropdown)
        {
            int newValue = m_dropdown.value;

            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add("onValueChange");
            args.Add(new Hashtable{{
                "value",newValue
            }});

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
    }
}

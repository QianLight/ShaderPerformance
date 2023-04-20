using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU
{
    public class ReactSlider : SimpleBaseView
    {
        public static string sOnValueChange = "onValueChange";
        public enum SliderDirection
        {
            LeftToRight = 0,
            RightToLeft = 1,
            BottomToTop = 2,
            TopToBottom = 3
        }
        
        private GameObject realGameObject;
        private Slider slider;
        private GameObject fillObject; // 进度条object
        private GameObject handleObject; // 滑动块object
        private GameObject fillChildObject; // 进度条子组件oject
        
        private bool listenOnValueChanged = false;
        
        public ReactSlider(string name)
        {
            realGameObject = new GameObject(name);
            slider = realGameObject.AddComponent<Slider>();

            fillObject = new GameObject("Fill Area");
            fillObject.transform.SetParent(realGameObject.transform, false);
            var fillTransform = fillObject.AddComponent<RectTransform>();
            fillTransform.sizeDelta = new Vector2(0f, 0.0f);
            fillTransform.anchorMin = new Vector2(0.0f, 0f);
            fillTransform.anchorMax = new Vector2(1f, 1f);
            fillTransform.anchoredPosition = new Vector2(0f, 0.0f);

            handleObject = new GameObject("Handle Slide Area");
            handleObject.transform.SetParent(realGameObject.transform, false);
            var hTransform = handleObject.AddComponent<RectTransform>();
            hTransform.sizeDelta = new Vector2(0.0f, 0.0f);
            hTransform.anchorMin = new Vector2(0.0f, 0.0f);
            hTransform.anchorMax = new Vector2(1f, 1f);

            fillChildObject = new GameObject("Fill Child");
            fillChildObject.transform.SetParent(fillObject.transform, false);
            var fillChildTransform = fillChildObject.AddComponent<RectTransform>();
            fillChildTransform.sizeDelta = new Vector2(0f, 0.0f);
            // 添加mask用于控制fill进度条区域的展示
            fillChildObject.AddComponent<Mask>(); 
            var f = fillChildObject.AddComponent<Image>();
            f.color = new Color(0, 0, 0, 0.01f); // 设为0.01f，是因为设置为0的话会导致渲染目标不显示（透明度都为0）
            slider.fillRect = f.rectTransform;
        }

        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public override void Add(BaseView child)
        {
            base.Add(child);
            GameObject goChild = child.GetGameObject();

            if (goChild.name == ReactSliderHandlerManager.viewName)
            {
                goChild.transform.SetParent(handleObject.transform, false);
                var image = goChild.GetComponent<Image>();
                if (image != null)
                {
                    slider.handleRect = image.rectTransform;
                }
            }

            if (goChild.name == ReactSliderFillManager.viewName)
            {
                goChild.transform.SetParent(fillChildObject.transform, false);
            }
        }
        
        public void SetValue(float value)
        {
            slider.value = value;
        }

        public void SetMaxValue(float value)
        {
            slider.maxValue = value;
        }

        public void SetMinValue(float value)
        {
            slider.minValue = value;
        }

        public void SetDirection(SliderDirection value)
        {
            if (value == SliderDirection.LeftToRight)
            {
                slider.direction = Slider.Direction.LeftToRight;
            } else if (value == SliderDirection.RightToLeft)
            {
                slider.direction = Slider.Direction.RightToLeft;
            } else if (value == SliderDirection.BottomToTop)
            {
                slider.direction = Slider.Direction.BottomToTop;
            } else if (value == SliderDirection.TopToBottom)
            {
                slider.direction = Slider.Direction.TopToBottom;
            }
        }
        
        public void SetListener(bool listener)
        {
            listenOnValueChanged = listener;
            AddValueChangedRectEvent();
        }

        private void AddValueChangedRectEvent()
        {
            slider.onValueChanged.RemoveAllListeners();
            if (listenOnValueChanged)
            {
                slider.onValueChanged.AddListener(OnValueChanged);
            }
        }

        private void OnValueChanged(float value)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(sOnValueChange);
            args.Add(new Hashtable
            {
                {
                    "value", value
                }
            });

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
    }
}
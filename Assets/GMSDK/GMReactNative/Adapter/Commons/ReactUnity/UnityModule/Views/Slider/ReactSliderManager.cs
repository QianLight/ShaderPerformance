using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class ReactSliderManager : BaseViewManager
    {
        public static string viewName = "RNUSlider";

        override public string GetName()
        {
            return viewName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactSlider(GetName());
        }

        [ReactProp(name = "maximumValue", defaultLong = 1)]
        public void SetMaxValue(ReactSlider view, long value)
        {
            view.SetMaxValue(value);
        }

        [ReactProp(name = "minimumValue", defaultLong = 0)]
        public void SetMinValue(ReactSlider view, long value)
        {
            view.SetMinValue(value);
        }

        [ReactProp(name = "value", defaultLong = 0)]
        public void InitValue(ReactSlider view, long value)
        {
            view.SetValue(value);
        }

        [ReactCommand]
        public void setValue(ReactSlider view, long value)
        {
            view.SetValue(value);
        }

        [ReactProp(name = "sliderDirection", defaultInt = 0)]
        public void SetDirection(ReactSlider view, int value)
        {
            view.SetDirection((ReactSlider.SliderDirection) value);
        }

        [ReactProp(name = "listenOnValueChanged", defaultBoolean = false)]
        public void SetListener(ReactSlider view, bool listen)
        {
            Debug.Log("listenlistenlistenlistenlisten" + listen.ToString());
            view.SetListener(listen);
        }

        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {
                    ReactSlider.sOnValueChange, new Hashtable
                    {
                        {sRegistration, ReactSlider.sOnValueChange}
                    }
                },
            };
        }
    }
}
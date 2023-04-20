using System.Collections;
using UnityEngine;

namespace GSDK.RNU {
    public class ReactSliderFillManager: BaseViewManager
    {
        public static string viewName = "RNUSliderFill";

        override public string GetName() {
            return viewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactSliderFill(GetName());
        }
        
        [ReactProp(name = "sliderDirection", defaultInt = 0)]
        public void SetDirection(ReactSliderFill view, int value)
        {
            view.SetDirection((ReactSlider.SliderDirection)value);
        }
    }
}
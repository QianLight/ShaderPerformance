using System.Collections;
using UnityEngine;

namespace GSDK.RNU {
    public class ReactSliderHandlerManager: BaseViewManager
    {
        public static string viewName = "RNUSliderHandler";

        override public string GetName() {
            return viewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactSliderHandler(GetName());
        }
        
    }
}
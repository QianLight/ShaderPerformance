using System.Collections;
using UnityEngine;

namespace GSDK.RNU {
    public class ReactViewManager: BaseViewManager
    {
        public static string ReactViewName = "RNUView";

        override public string GetName() {
            return ReactViewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactView(GetName());
        }
        
        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {sPressInEventname, new Hashtable{{sRegistration, sPressInEventname}}},
                {sPressUpEventname, new Hashtable{{sRegistration, sPressUpEventname}}},
                {sLongPressEventname, new Hashtable{{sRegistration, sLongPressEventname}}},
                {sPressEventname, new Hashtable{{sRegistration, sPressEventname}}},
                {sDoubleClickEventname, new Hashtable{{sRegistration, sDoubleClickEventname}}}
            };
        }
        
        [ReactProp(name = "delayLongPress")]
        public void SetLongPressTime(ReactView view, float time)
        {
            view.SetLongPressTime(time);
        }
        
        [ReactProp(name = "doubleClickTime")]
        public void SetDoubleClickTime(ReactView view, float time)
        {
            view.SetDoubleClickTime(time);
        }
    }
}
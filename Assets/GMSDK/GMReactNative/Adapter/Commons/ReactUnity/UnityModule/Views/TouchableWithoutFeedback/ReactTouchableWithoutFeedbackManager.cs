/*
 * @Author: hexiaonuo
 * @Date: 2021-11-08
 * @Description: ReactTouchableWithoutFeedback
 * @FilePath: ReactUnity/UnityModule/views/TouchableWithoutFeedback/ReactTouchableWithoutFeedback.cs
 */

using System.Collections;

namespace GSDK.RNU
{
    public class ReactTouchableWithoutFeedbackManager: BaseViewManager
    {
        public static string viewName = "RNUTouchableWithoutFeedback";

        public override string GetName()
        {
            return viewName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactTouchableWithoutFeedback(GetName());
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
        public void SetLongPressTime(ReactTouchableWithoutFeedback view, float time)
        {
            view.SetLongPressTime(time);
        }
        
        [ReactProp(name = "doubleClickTime")]
        public void SetDoubleClickTime(ReactTouchableWithoutFeedback view, float time)
        {
            view.SetDoubleClickTime(time);
        }
    }
}
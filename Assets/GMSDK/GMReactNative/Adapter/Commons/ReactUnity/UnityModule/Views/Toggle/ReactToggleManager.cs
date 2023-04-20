using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    public class ReactToggleManager:BaseViewManager
    {
        
        public static string viewName = "RNUToggle";
        public override string GetName()
        {
            return viewName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactToggle(GetName());
        }
        
        [ReactProp(name = "isOn", defaultBoolean = false)]
        public void SetIsOn(ReactToggle view, bool value)
        {
            view.SetIsOn(value);
        }
        
        [ReactProp(name = "label")]
        public void SetLabel(ReactToggle view, Dictionary<string, object> label)
        {
            view.SetLabel(label);
        }

        [ReactProp(name = "selectedImageSource", defaultString = "")]
        public void SetSelectedImageSource(ReactToggle view, string uri)
        {
            view.SetSelectedSource(uri);
        }
        
        [ReactProp(name = "unselectedImageSource",defaultString = "")]
        public void SetUnselectedImageSource(ReactToggle view, string uri)
        {
            view.SetUnselectedSource(uri);
        }
        
        [ReactProp(name = "listenOnValueChanged", defaultBoolean = false)]
        public void SetListener(ReactToggle view, bool listen)
        {
            view.SetValueChangedListener(listen);
        }

        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {
                    ReactToggle.sOnValueChanged, new Hashtable
                    {
                        {sRegistration, ReactToggle.sOnValueChanged}
                    }
                },
            };
        }
        
        
    }
}
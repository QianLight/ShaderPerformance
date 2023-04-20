using System;
using System.Collections;

namespace GSDK.RNU {
    public class ReactButtonManager: BaseViewManager {
        override public string GetName() {
            return "RNUButton";
        }

        protected override BaseView createViewInstance() {
            return new ReactButton(GetName());
        }

        
        public override Hashtable GetExportedCustomDirectEventTypeConstants() {
            return new Hashtable{
                {"onClick", new Hashtable{
                    {sRegistration, "onClick"}
                }},
            };
        }
        
        [ReactProp(name = "disabled", defaultBoolean = false)]
        public void SetDisable(BaseView view, bool disable)
        {
            ReactButton button = (ReactButton) view;
            button.SetDisable(disable);
        }
        
    }
}
using System.Collections;
using UnityEngine;

namespace GSDK.RNU {
    public class ReactImageManager: BaseViewManager {
        public override string GetName() {
            return "RNUImage";
        }

        protected override BaseView createViewInstance() {
            return new ReactImage(GetName());
        }

        [ReactProp(name = "source")]
        public void setSource(ReactImage view,  string uri)
        {
            // JObject source = (JObject) val;
            // string uri = (string)source["uri"];
            view.SetNetworkUri(uri);
        }
        
        
        
        [ReactProp(name = "material")]
        public override void setMaterial(BaseView view,  string material)
        {
            ReactImage image = (ReactImage) view;
            image.setMaterial(material);
        }

        [ReactProp(name = "listenOnLoad")]
        public void setIsListenOnload(BaseView view, bool listen)
        {
            ReactImage image = (ReactImage) view;
            image.setIsListenOnload(listen);
        }

        [ReactProp(name = "listenOnLoadEnd")]
        public void setIsListenOnloadEnd(BaseView view, bool listen)
        {
            ReactImage image = (ReactImage) view;
            image.setIsListenOnloadEnd(listen);
        }

        [ReactProp(name = "listenOnProgress")]
        public void setIsListenOnProgress(BaseView view, bool listen)
        {
            ReactImage image = (ReactImage) view;
            image.setIsListenOnProgress(listen);
        }

        [ReactProp(name = "listenOnError")]
        public void setIsListenOnError(BaseView view, bool listen)
        {
            ReactImage image = (ReactImage) view;
            image.setIsListenOnError(listen);
        }

        [ReactProp(name = "listenOnLoadStart")]
        public void setIsListenOnLoadStart(BaseView view, bool listen)
        {
            ReactImage image = (ReactImage) view;
            image.setIsListenOnLoadStart(listen);
        }

        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {sPressInEventname, new Hashtable {{sRegistration, sPressInEventname}}},
                {sPressUpEventname, new Hashtable {{sRegistration, sPressUpEventname}}},
                {sLongPressEventname, new Hashtable {{sRegistration, sLongPressEventname}}},
                {sPressEventname, new Hashtable {{sRegistration, sPressEventname}}},
                {sDoubleClickEventname, new Hashtable {{sRegistration, sDoubleClickEventname}}},
                {ReactImage.sOnLoadStartEventName, new Hashtable {{sRegistration, ReactImage.sOnLoadStartEventName}}},
                {ReactImage.sOnProgressEventName, new Hashtable {{sRegistration, ReactImage.sOnProgressEventName}}},
                {ReactImage.sOnLoadEndEventName, new Hashtable {{sRegistration, ReactImage.sOnLoadEndEventName}}},
                {ReactImage.sOnLoadEventName, new Hashtable {{sRegistration, ReactImage.sOnLoadEventName}}},
                {ReactImage.sOnErrorEventName, new Hashtable {{sRegistration, ReactImage.sOnErrorEventName}}}
            };
        }
        
        [ReactProp(name = "delayLongPress")]
        public void SetLongPressTime(ReactImage view, float time)
        {
            view.SetLongPressTime(time);
        }
        
        [ReactProp(name = "doubleClickTime")]
        public void SetDoubleClickTime(ReactImage view, float time)
        {
            view.SetDoubleClickTime(time);
        }
    }
}
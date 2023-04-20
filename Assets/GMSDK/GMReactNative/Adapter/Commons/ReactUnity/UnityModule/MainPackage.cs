using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class MainPackage
    {
        public List<ViewManager> CreateViewManagers()
        {
            List<ViewManager>  modules  = new List<ViewManager>();
            modules.Add(new ReactViewManager());
            modules.Add(new ReactImageManager());
            modules.Add(new ReactTextManager());
            modules.Add(new ReactRawTextManager());
            modules.Add(new ReactButtonManager());
            modules.Add(new ReactScrollViewManager());
            modules.Add(new ReactScrollViewContentManager());
            modules.Add(new ReactInputFieldManager());
            modules.Add(new ReactPrefabTmpManager());
            modules.Add(new ReactTouchableWithoutFeedbackManager());
            modules.Add(new ExperimentalReactRichTextManager());
            modules.Add(new ReactPickerManager());
            modules.Add(new ReactViewShotManager());
            modules.Add(new ReactVideoManager());
            modules.Add(new ReactSliderManager());
            modules.Add(new ReactSliderFillManager());
            modules.Add(new ReactSliderHandlerManager());
            modules.Add(new ReactToggleManager());
            return modules;
        }

        public List<BaseUnityModule> CreateNativeModules(RNUMainCore rnuContext)
        {
            List<BaseUnityModule> modules = new List<BaseUnityModule>();
            modules.Add(new Common());
            modules.Add(new TimerModule());
            modules.Add(new DeviceInfoModule());
            modules.Add(new ExceptionsManagerModule(rnuContext));
            modules.Add(new AsyncStorage());


            if (rnuContext.IsDebugMode())
            {
                modules.Add(new WebSocketModule());
                // logbox
                modules.Add(new LogBoxModule(rnuContext));
                // Networking for WebSocketJSExecutor
                modules.Add(new NetworkingModule(rnuContext));
                modules.Add(new SourceCodeModule(rnuContext));
                modules.Add(new DevSettingsModule(rnuContext));
            }
            
            return modules;
        }

    }
}

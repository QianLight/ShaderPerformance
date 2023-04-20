using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public interface ViewManager: BaseUnityModule
    {
        void UpdateProperties(BaseView viewToUpdate, Dictionary<string, object> props);

        BaseView CreateView();

        Dictionary<string, MethodInfo> GetPropSetters();

        Hashtable GetFlexProps();

        Hashtable GetExportedCustomDirectEventTypeConstants();
        
        ReactSimpleShadowNode CreateShadowNode(int tag);

        void UpdateLayout(BaseView viewToUpdate, int x, int y, int width, int height);

        void ReceiveCommand(BaseView view, string commandId, ArrayList args, Promise promise);
    };

}
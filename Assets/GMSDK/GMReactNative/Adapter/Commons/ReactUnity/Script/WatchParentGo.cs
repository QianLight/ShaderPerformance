using System;
using UnityEngine;

namespace GSDK.RNU
{
    public class WatchParentGo: MonoBehaviour
    {
        private static readonly string sParentGoStateMessage = "ParentState";
        private void OnEnable()
        {
            RNUMainCore.SendUnityEventToJs(sParentGoStateMessage, "enable");
        }

        private void OnDisable()
        {
            RNUMainCore.SendUnityEventToJs(sParentGoStateMessage, "disable");
        }

        private void OnDestroy()
        {
            RNUMainCore.SendUnityEventToJs(sParentGoStateMessage, "destroy");
        }
        
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class ReactViewShotManager: BaseViewManager
    {
        public static string ReactViewName = "RNUViewShot";

        override public string GetName() {
            return ReactViewName;
        }

        protected override BaseView createViewInstance() {
            return new ReactViewShot(GetName());
        }
        
               
        [ReactProp(name = "cameraDepth")]
        public void SetCameraDepth(ReactViewShot view, float depth)
        {
            view.SetCameraDepth(depth);
        }
        
        [ReactProp(name = "cameraPosition")]
        public void SetCameraPosition(ReactViewShot view, Dictionary<string, object> position)
        {
            view.SetCameraPosition(position);
        }
        
        [ReactProp(name = "cameraRotation")]
        public void SetCameraRotation(ReactViewShot view, Dictionary<string, object> rotation)
        {
            view.SetCameraRotation(rotation);
        }
        
        [ReactCommand]
        public void capture(ReactViewShot view, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(view.CaptureCamera(promise));
        }
    }
}
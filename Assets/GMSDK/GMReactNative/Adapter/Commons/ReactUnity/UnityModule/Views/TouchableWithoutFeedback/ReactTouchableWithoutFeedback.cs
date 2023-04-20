/*
 * @Author: hexiaonuo
 * @Date: 2021-11-08
 * @Description: ReactTouchableWithoutFeedback
 * @FilePath: ReactUnity/UnityModule/views/TouchableWithoutFeedback/ReactTouchableWithoutFeedback.cs
 */

using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GSDK.RNU
{
    public class ReactTouchableWithoutFeedback : SimpleBaseView
    {
        private GameObject realGameObject;

        public ReactTouchableWithoutFeedback(string name)
        {
            realGameObject = new GameObject(name);
            IPointerEvent gLongPress = realGameObject.GetComponent<IPointerEvent>();
            if (gLongPress == null)
            {
                gLongPress = realGameObject.AddComponent<IPointerEvent>();
            }

            if (gLongPress == null)
            {
                Util.Log("AddComponent IPointerEvent failed, return");
                return;
            }
            gLongPress.mPressIn = (position) => { OnEvent(BaseViewManager.sPressInEventname, position); };
            gLongPress.mPressUp = (position) => { OnEvent(BaseViewManager.sPressUpEventname, position); };
            gLongPress.mLongPress = (position) => { OnEvent(BaseViewManager.sLongPressEventname, position); };
            gLongPress.mDoubleClick = (position) => { OnEvent(BaseViewManager.sDoubleClickEventname, position); };
            gLongPress.mPress = (position) => { OnEvent(BaseViewManager.sPressEventname, position); };
        }


        public override GameObject GetGameObject()
        {
            return realGameObject;
        }

        public void OnEvent(string eventName, Vector2 position)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(eventName);
            args.Add(new Hashtable
            {
                {"pageX", position.x},
                {"pageY", position.y}
            });

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }

        public void SetLongPressTime(float time)
        {
            IPointerEvent gLongPress = realGameObject.GetComponent<IPointerEvent>();
            if (gLongPress == null)
            {
                gLongPress = realGameObject.AddComponent<IPointerEvent>();
            }

            if (gLongPress == null)
            {
                Util.Log("AddComponent IPointerEvent failed, return");
                return;
            }
            gLongPress.SetLongPressTime(time);
        }

        public void SetDoubleClickTime(float time)
        {
            IPointerEvent gLongPress = realGameObject.GetComponent<IPointerEvent>();
            if (gLongPress == null)
            {
                gLongPress = realGameObject.AddComponent<IPointerEvent>();
            }

            if (gLongPress == null)
            {
                Util.Log("AddComponent IPointerEvent failed, return");
                return;
            }
            gLongPress.SetDoubleClickTime(time);
        }
    }

}
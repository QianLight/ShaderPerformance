using UnityEngine;

namespace GSDK.RNU {
    public class ReactView: SimpleBaseView {
        private GameObject realGameObject;

        public ReactView(string name) {
            realGameObject = new GameObject(name);
        }
        
        public override GameObject GetGameObject() {
            return realGameObject;
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

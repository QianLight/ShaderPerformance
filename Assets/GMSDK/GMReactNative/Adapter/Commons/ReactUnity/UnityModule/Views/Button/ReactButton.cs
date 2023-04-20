using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GSDK.RNU {
    public class ReactButton: SimpleBaseView {

        private GameObject realGameObject;

        private UnityAction btnAction;

        public ReactButton(string name) {
            realGameObject = new GameObject(name);
            Button btn = realGameObject.AddComponent<Button>();

            // 添加默认Image 否则Button 没有raycastTarget
            Image i = realGameObject.AddComponent<Image>();
            i.color = new Color32(0, 0, 0, 0);
            i.raycastTarget = true;
            
            btnAction += BtnCommonCall;
            btn.onClick.AddListener(btnAction);
        }


        public override GameObject GetGameObject() {
            return realGameObject;
        }

        public void BtnCommonCall() {
            // ArrayList event = new ArrayList();
            // event.Add("YkApp");
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add("onClick");
            args.Add(new Hashtable());

            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }

        public void SetDisable(bool disable)
        {
            Button btn = realGameObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = !disable;
            }
        }
        
    }
}

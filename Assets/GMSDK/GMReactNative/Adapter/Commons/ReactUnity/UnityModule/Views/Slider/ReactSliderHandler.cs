using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU {
    public class ReactSliderHandler: SimpleBaseView {
        private GameObject realGameObject;
        private int direction;
        public ReactSliderHandler(string name) {
            realGameObject = new GameObject(name);
            var h = realGameObject.AddComponent<Image>();
            h.color = new Color(0, 0, 0, 0);
        }
        
        public override GameObject GetGameObject() {
            return realGameObject;
        }

        public override void SetLayout(int x, int y, int width, int height)
        {
            GameObject panel = GetGameObject();
            
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = panel.AddComponent<RectTransform>();
            }
            rectTransform.sizeDelta = new Vector2(width, height);
        }
    }
}

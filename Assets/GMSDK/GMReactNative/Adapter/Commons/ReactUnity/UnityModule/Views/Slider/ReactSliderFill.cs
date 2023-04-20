using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU {
    public class ReactSliderFill: SimpleBaseView {
        private GameObject realGameObject;
        private ReactSlider.SliderDirection direction;
        public ReactSliderFill(string name) {
            realGameObject = new GameObject(name);
        }
        
        public override GameObject GetGameObject() {
            return realGameObject;
        }

        public override void SetLayout(int x, int y, int width, int height)
        {
            base.SetLayout(x, y, width, height);
            GameObject panel = GetGameObject();
            
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (direction == ReactSlider.SliderDirection.RightToLeft || direction == ReactSlider.SliderDirection.BottomToTop )
            {
                // 方向为从右至左，或者从下至上时，需要重新设置fill的锚点位置
                var anchoredPosition = rectTransform.anchoredPosition;
                rectTransform.anchorMin = new Vector2(1.0F, 0.0F);
                rectTransform.anchorMax = new Vector2(1.0F, 0.0F);
                rectTransform.anchoredPosition =
                    new Vector2(-1 * anchoredPosition.x, -1 * anchoredPosition.y);
            }
        }

        public void SetDirection(ReactSlider.SliderDirection value)
        {
            direction = value;
        }
    }
}

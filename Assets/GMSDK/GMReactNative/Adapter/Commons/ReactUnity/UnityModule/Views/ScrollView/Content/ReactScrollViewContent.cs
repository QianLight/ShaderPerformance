using System;
using System.Collections;
using UnityEngine;

namespace GSDK.RNU
{
    public class ReactScrollViewContent: SimpleBaseView
    {
        private GameObject realGameObject;

        // 首次创建 view 的时候强制顶端对齐，以后的刷新更新不需要更改位置
        private bool isFirstCreate = true;

        private RectTransform rectTransform;

        private Vector2 rect = Vector2.zero;
        private Vector2 position = Vector2.zero;

        // 默认每帧移动 100 px
        private int animationThreshold = 100;
        

        public ReactScrollViewContent(string name) {
            realGameObject = new GameObject(name, typeof(RectTransform));
            rectTransform = realGameObject.GetComponent<RectTransform>();
        }

        public override GameObject GetGameObject() {
            return realGameObject;
        }


        public void SetAnimationThreshold(int value)
        {
            animationThreshold = value;
        }

        /*
         * 重载 SetLayout
         * content 需要与父节点 viewPort 的顶端对齐
         * 不足的部分，下方留空白
         * 超出的部分，下方被遮挡
         */
        public override void SetLayout(int x, int y, int width, int height)
        {
            // save position and rect;
            {
                position.x = x;
                position.y = y;
                rect.x = width;
                rect.y = height;
            }
            
            if (rectTransform == null)
            {
                Util.Log("rectTransform is null, return");
                return;
            }
            rectTransform.anchorMin = new Vector2(0.0F, 1.0F);
            rectTransform.anchorMax = new Vector2(0.0F, 1.0F);
            if (isFirstCreate)
            {
                rectTransform.anchoredPosition = new Vector2(0.0F, 0.0F);
                isFirstCreate = false;
            }
            else
            {
                // todo 下一帧回调
                // OnContentSizeChangeEvent(width, height);
                StaticCommonScript.StaticStartCoroutine(OnContentSizeChangeEvent(width, height));
            }
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.pivot = new Vector2(0.0F, 1.0F);
        }
        private IEnumerator OnContentSizeChangeEvent(int width, int height)
        {
            yield return null;
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(ReactScrollViewContentManager.sContentSizeChange);
            args.Add(new Hashtable
            {
                {"contentWidth", width},
                {"contentHeight", height}
            });
            
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }

        public Vector2 GetRect()
        {
            return rect;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        /*
         * 手动重制 content 位置
         * params x，y 重制后坐标
         * params animated 是否平滑过渡
         */
        public void ReSetLayout(float x, float y, bool animated, bool isHorizontal)
        {
            {
                position.x = x;
                position.y = y;
            }

            if (rectTransform == null)
            {
                Util.Log("rectTransform is null, return");
                return;
            }

            if (!animated)
            {
                rectTransform.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                int flag = 1;
                if (!isHorizontal)
                {
                    if (rectTransform.anchoredPosition.y < y)
                    {
                        flag = -1;
                    }

                    StaticCommonScript.StaticStartCoroutine(AnimationScrollVer(x, y, flag));
                }
                else
                {
                    if (rectTransform.anchoredPosition.x < x)
                    {
                        flag = -1;
                    }
                    StaticCommonScript.StaticStartCoroutine(AnimationScrollHor(x, y, flag));

                }
            }
        }

        private IEnumerator AnimationScrollVer(float x, float y, int flag)
        {
            int frameCount = (int) (Math.Abs(rectTransform.anchoredPosition.y - y) / animationThreshold);
            yield return null;
            for (int i = 0; i < frameCount; ++ i)
            {
                rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y - flag * animationThreshold);
                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(x, y);

        }

        private IEnumerator AnimationScrollHor(float x, float y, int flag)
        {
            int frameCount = (int) (Math.Abs(rectTransform.anchoredPosition.x - x) / animationThreshold);
            yield return null;

            for (int i = 0; i < frameCount; ++i)
            {
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x - flag * animationThreshold, y);
                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}
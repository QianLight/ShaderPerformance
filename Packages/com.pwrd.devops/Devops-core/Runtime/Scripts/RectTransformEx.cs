using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Core
{
    public static class RectTransformEx
    {
        public enum eAnchorType
        {
            Top,
            RightTop,
            Right,
            RightBottom,
            Bottom,
            LeftBottom,
            Left,
            LeftTop,
            Center,
            Unknow,
        }
        public static void ResetAnchor(this RectTransform self, Vector2 anchorCenter)
        {
            Vector2 oldAnchorCenter = (self.anchorMin + self.anchorMax) / 2;
            Vector2 oldAnchoredPos = self.anchoredPosition;
            Vector2 objSize = new Vector2(self.rect.width, self.rect.height);

            self.anchorMin = anchorCenter;
            self.anchorMax = anchorCenter;
            self.sizeDelta = objSize;

            RectTransform rectTransPanent = self.parent as RectTransform;
            Vector2 deltaAnchor = anchorCenter - oldAnchorCenter;
            Vector2 deltaMove = new Vector2(deltaAnchor.x * rectTransPanent.rect.width, deltaAnchor.y * rectTransPanent.rect.height);
            self.anchoredPosition = oldAnchoredPos - deltaMove;
        }

        public static eAnchorType GetAnchorType(this RectTransform self)
        {
            if (self.anchorMin != self.anchorMax)
                return eAnchorType.Unknow;
            if (Mathf.Abs(self.anchorMin.x - 0.5f) < 0.001f && Mathf.Abs(self.anchorMin.y - 0.5f) < 0.001f)
                return eAnchorType.Center;
            if (self.anchorMin.x < 0.001f)
            {
                if (self.anchorMin.y < 0.001f)
                {
                    return eAnchorType.LeftBottom;
                }
                else if (self.anchorMin.y > 0.999f)
                {
                    return eAnchorType.LeftTop;
                }
                else
                {
                    return eAnchorType.Right;
                }
            }
            else if (self.anchorMin.x > 0.999f)
            {
                if (self.anchorMin.y < 0.001f)
                {
                    return eAnchorType.RightBottom;
                }
                else if (self.anchorMin.y > 0.999f)
                {
                    return eAnchorType.RightTop;
                }
                else
                {
                    return eAnchorType.Right;
                }
            }
            else
            {
                if (self.anchorMin.y < 0.001f)
                {
                    return eAnchorType.Left;
                }
                else if (self.anchorMin.y > 0.999f)
                {
                    return eAnchorType.Right;
                }
                else
                {
                    return eAnchorType.Unknow;
                }
            }
        }

        public static bool IsNearSide_horizontal(this RectTransform self, out float distance, out eAnchorType anchorType)
        {
            anchorType = self.GetAnchorType();
            if(anchorType == eAnchorType.Left || anchorType == eAnchorType.LeftTop || anchorType == eAnchorType.LeftBottom)
            {
                distance = Mathf.Abs(self.anchoredPosition.x);
                return true;
            }
            else if(anchorType == eAnchorType.Right || anchorType == eAnchorType.RightTop || anchorType == eAnchorType.RightBottom)
            {
                distance = Mathf.Abs(self.anchoredPosition.x);
                return true;
            }
            else
            {
                distance = 0;
                return false;
            }
        }

        public static bool IsNearSide_vertical(this RectTransform self, float distance, out eAnchorType anchorType)
        {
            anchorType = self.GetAnchorType();
            if (anchorType == eAnchorType.Top || anchorType == eAnchorType.LeftTop || anchorType == eAnchorType.RightTop)
            {
                distance = self.anchoredPosition.y;
                return true;
            }
            else if (anchorType == eAnchorType.Bottom || anchorType == eAnchorType.LeftBottom || anchorType == eAnchorType.RightBottom)
            {
                distance = self.anchoredPosition.y;
                return true;
            }
            else
            {
                distance = 0;
                return false;
            }
        }
    }
}
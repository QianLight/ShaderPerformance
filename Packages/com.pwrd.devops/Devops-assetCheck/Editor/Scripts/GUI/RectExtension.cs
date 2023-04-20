using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectExtension
{
    public static Rect RightRect(this Rect self, float width)
    {
        return new Rect(self.xMax, self.yMin, width, self.height);
    }

    public static Rect[] SplitHorizontal(this Rect self, int nCount)
    {
        if (nCount == 0)
            return null;
        Rect[] rects = new Rect[nCount];
        float w = self.width / nCount;
        for(int i = 0; i < rects.Length; i++)
        {
            rects[i] = new Rect() {xMin = w * i, xMax = w * (i+1), yMin = self.yMin, yMax = self.yMax};
        }
        return rects;
    }

    public static Rect[] SplitTwoHorizontalPercent(this Rect self, float fSplitPercent)
    {
        fSplitPercent = Mathf.Clamp01(fSplitPercent);
        Rect[] rects = new Rect[2];
        rects[0] = new Rect()
        {
            xMin = self.xMin,
            yMin = self.yMin,
            xMax = self.xMin + self.width * fSplitPercent,
            yMax = self.yMax,
        };
        rects[1] = new Rect()
        {
            xMin = self.xMin + self.width * fSplitPercent,
            yMin = self.yMin,
            xMax = self.xMax,
            yMax = self.yMax,
        };
        return rects;
    }

    public static Rect[] SplitTwoHorizontal(this Rect self, float fSplit)
    {
        Rect[] rects = new Rect[2];
        rects[0] = new Rect()
        {
            xMin = self.xMin,
            yMin = self.yMin,
            xMax = self.xMin + fSplit,
            yMax = self.yMax,
        };
        rects[1] = new Rect()
        {
            xMin = self.xMin + fSplit,
            yMin = self.yMin,
            xMax = self.xMax,
            yMax = self.yMax,
        };
        return rects;
    }

    public static Rect GetPart(this Rect self, float xBeginPer, float xEndPer, float yBeginPer, float yEndPer)
    {
        return new Rect()
        {
            xMin = self.width * xBeginPer + self.xMin,
            xMax = self.width * xEndPer + self.xMin,
            yMin = self.height * yBeginPer + self.yMin,
            yMax = self.height * yEndPer + self.yMin
        };
    }

    public static Rect GetLeftPart(this Rect self, float width, bool inRect = true)
    {
        float w = inRect ? Mathf.Min(width, self.width) : width;
        return new Rect() {xMin = self.xMin, xMax = self.xMin + w,  yMin = self.yMin, yMax = self.yMax};
    }

    public static Rect GetRightPart(this Rect self, float width, bool inRect = true)
    {
        float w = inRect ? Mathf.Min(width, self.width) : width;
        return new Rect() { xMin = self.xMax - w, xMax = self.xMax, yMin = self.yMin, yMax = self.yMax };
    }

    public static Rect GetHorizontalPart(this Rect self, float begin, float end)
    {
        return new Rect() { xMin = begin, xMax = end, yMin = self.yMin, yMax = self.yMax };
    }

    public static Rect GetTopPart(this Rect self, float height, bool inRect = true)
    {
        float h = inRect ? Mathf.Min(height, self.height) : height;
        return new Rect() { xMin = self.xMin, xMax = self.xMax, yMin = self.yMin, yMax = self.yMin + h };
    }

    public static Rect GetBottomPart(this Rect self, float height, bool inRect = true)
    {
        float h = inRect ? Mathf.Min(height, self.height) : height;
        return new Rect() { xMin = self.xMin, xMax = self.xMax, yMin = self.yMax - h, yMax = self.yMax };
    }

    public static Rect Add(this Rect self, Rect other)
    {
        return new Rect()
        {
            xMin = Mathf.Min(self.xMin, other.xMin),
            xMax = Mathf.Max(self.xMax, other.xMax),
            yMin = Mathf.Min(self.yMin, other.yMin),
            yMax = Mathf.Max(self.yMax, other.yMax)
        };
    }

    public static Rect GetCenterRect(this Rect self, float width, float height)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;
        return new Rect()
        {
            xMin = self.center.x - halfWidth,
            xMax = self.center.x + halfWidth,
            yMin = self.center.y - halfHeight,
            yMax = self.center.y + halfHeight,
        };
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BlueprintSlider : BluePrintWidget
    {
        protected BluePrintNode node;

        public BlueprintSlider(BluePrintNode parent)
        {
            node = parent;
            Root = parent.Root;
        }

        public float Draw(float value)
        {
            adjustedBounds = Bounds.Scale(Scale);

            return GUI.HorizontalSlider(adjustedBounds, value, 0, 100);
        }

        protected override bool OnMouseLeftDown(Event e)
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BlueprinButton : BluePrintWidget
    {
        protected BluePrintWidget node;

        public string ImageName;

        public delegate void ButtonClickCb(object o);
        public object Param;

        ButtonClickCb OnButtonClicked = null;

        public BlueprinButton(BluePrintWidget parent)
        {
            node = parent;
            Root = parent.Root;

            ImageName = "BluePrint/MonsterIndicateGray";
        }

        public override void Draw()
        {
            adjustedBounds = Bounds.Scale(Scale);

            DrawTool.DrawIcon(adjustedBounds, ImageName);
        }

        public void RegisterClickEvent(ButtonClickCb cb)
        {
            OnButtonClicked = cb;
        }

        protected override bool OnMouseLeftDown(Event e)
        {
            if (OnButtonClicked != null)
                OnButtonClicked(Param);
            return true;
        }

    }
}

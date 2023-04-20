using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    class BlueprintImageToggle : BluePrintWidget
    {
        protected BluePrintWidget node;
        public object Param = null;

        public string OnImageName = "";
        public string OffImageName= "";

        private bool State;

        public delegate void ToggleClickCb(bool state, object o);
        ToggleClickCb OnToggle = null;

        public BlueprintImageToggle(BluePrintWidget parent)
        {
            node = parent;
            Root = parent.Root;
        }

        public override void Draw()
        {
            adjustedBounds = Bounds.Scale(Scale);

            if(State)
                DrawTool.DrawIcon(adjustedBounds, OnImageName);
            else
                DrawTool.DrawIcon(adjustedBounds, OffImageName);
        }

        protected override bool OnMouseLeftDown(Event e)
        {
            bool oldState = State;
            State = !State;

            if (OnToggle != null) OnToggle(oldState, Param);
            return true;
        }

        public void RegisterClickEvent(ToggleClickCb cb)
        {
            OnToggle = cb;
        }
    }
}

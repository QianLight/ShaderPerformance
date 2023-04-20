using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BluePrintWidget
    {
        public Rect Bounds;
        protected Rect adjustedBounds;

        public BluePrintGraph Root { get; set; }

        public float Scale { get { return Root.Scale; } }

        protected List<BluePrintWidget> Children = new List<BluePrintWidget>();

        public float xMin { get { return adjustedBounds.xMin; } }
        public float xMax { get { return adjustedBounds.xMax; } }
        public float yMin { get { return adjustedBounds.yMin; } }
        public float yMax { get { return adjustedBounds.yMax; } }

        public bool IsMouseOver { get; set; }
        public bool IsSelected { get; set; }
        public bool Enabled { get; set; }
        public bool IsEditing { get; set; }

        public virtual void Draw() { }

        public virtual void PostDraw() { }

        public void AddChild(BluePrintWidget widget)
        {
            Children.Add(widget);
        }

        public void RemoveChild(BluePrintWidget widget)
        {
            Children.Remove(widget);
        }

        protected virtual bool CheckOver(Vector2 position)
        {
            if (xMin > position.x) return false;
            if (xMax < position.x) return false;
            if (yMin > position.y) return false;
            if (yMax < position.y) return false;

            return true;
        }

        public bool OnMouseEvent(Event e, bool already)
        {
            bool flag = false;
            for (int i = 0; i < Children.Count; ++i)
            {
                flag |= Children[i].OnMouseEvent(Event.current, flag);
            }

            if (flag) return true;

            bool isOver = CheckOver(e.mousePosition);

            if (isOver ^ IsMouseOver)
            {
                if (IsMouseOver) OnMouseExit(e);
                else OnMouseEnter(e);
            }
            IsMouseOver = isOver;

            bool handled = false;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (!IsMouseOver/* || allready*/)
                        handled = OnMouseDownOutSide(e);
                    else if (e.button == 0)
                        handled = OnMouseLeftDown(e);
                    else if (e.button == 1)
                        handled = OnMouseRightDown(e);
                    break;
                case EventType.MouseUp:
                    if(IsMouseOver) handled = OnMouseUp(e);
                    break;
                case EventType.MouseMove:
                    handled = OnMouseMove(e);
                    break;
                case EventType.MouseDrag:
                    if(e.button == 0)
                        handled = OnMouseDragging(e);
                    break;
            }

            return handled;
        }

        protected virtual bool OnMouseDownOutSide(Event e) { return false; }
        protected virtual bool OnMouseEnter(Event e) { return false; }

        protected virtual bool OnMouseExit(Event e) { return false; }
        protected virtual bool OnMouseLeftDown(Event e) { return false; }

        protected virtual bool OnMouseDoubleClick(Event e) { return false; }
        protected virtual bool OnMouseRightDown(Event e) { return false; }
        protected virtual bool OnMouseUp(Event e)
        {
            Root.ResetCachedPinData();
            return true;
        }
        protected virtual bool OnMouseMove(Event e) { return false; }

        protected bool OnMouseDragging(Event e)
        {
            if (IsSelected && Root.ConnectStartPin == null && !Root.InMultiselect)
            {
                OnMouseDrag(e);
                return true;
            }

            return false;
        }
        protected virtual bool OnMouseDrag(Event e)
        {
            Bounds = Bounds.Move(e.delta / Scale);
            return true;
        }
    }
}

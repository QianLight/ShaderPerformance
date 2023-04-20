using System;
using UnityEngine;


namespace UnityEditor.Timeline
{
    
    class TimelineKeyFrameContext
    {
        private bool press_e;
        private Vector2 start;
        private Rect rect;

        public Action cbReset;
        public Action<Rect> cbRect;
        public Action<float> cbDrag;

        public void EventProcess()
        {
            var e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.E)
                    {
                        if (!press_e)
                            Debug.Log("key frame record triger");
                        press_e = true;
                    }
                    break;
                case EventType.KeyUp:
                    if (press_e)
                        rect = Rect.zero;
                    cbReset?.Invoke();
                    press_e = false;
                    break;
                case EventType.MouseDown:
                    if (press_e)
                    {
                        start = e.mousePosition;
                    }
                    break;
                case EventType.MouseDrag:
                    if (press_e && rect != Rect.zero)
                    {
                        cbDrag?.Invoke(e.delta.x);
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (press_e)
                    {
                        Vector2 p2 = e.mousePosition;
                        rect = new Rect();
                        rect.xMin = Mathf.Min(start.x, p2.x);
                        rect.xMax = Mathf.Max(start.x, p2.x);
                        rect.yMin = Mathf.Min(start.y, p2.y);
                        rect.yMax = Mathf.Max(start.y, p2.y);
                        cbRect?.Invoke(rect);
                    }
                    break;
            }
        }
    }

}

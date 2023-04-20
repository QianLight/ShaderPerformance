using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using PositionType = UnityEngine.UIElements.Position;

namespace CFEngine.Editor
{
    static class BoardPreferenceHelper
    {
        public enum Board
        {
            blackboard,
            componentBoard
        }

        const string rectPreferenceFormat = "mateffect-{0}-rect";
        const string visiblePreferenceFormat = "mateffect-{0}-visible";

        public static bool IsVisible (Board board, bool defaultState)
        {
            return EditorPrefs.GetBool (string.Format (visiblePreferenceFormat, board), defaultState);
        }

        public static void SetVisible (Board board, bool value)
        {
            EditorPrefs.SetBool (string.Format (visiblePreferenceFormat, board), value);
        }

        public static Rect LoadPosition (Board board, Rect defaultPosition)
        {
            string str = EditorPrefs.GetString (string.Format (rectPreferenceFormat, board));

            Rect blackBoardPosition = defaultPosition;
            if (!string.IsNullOrEmpty (str))
            {
                var rectValues = str.Split (',');

                if (rectValues.Length == 4)
                {
                    float x, y, width, height;
                    if (float.TryParse (rectValues[0], NumberStyles.Float, CultureInfo.InvariantCulture, out x) &&
                        float.TryParse (rectValues[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                        float.TryParse (rectValues[2], NumberStyles.Float, CultureInfo.InvariantCulture, out width) &&
                        float.TryParse (rectValues[3], NumberStyles.Float, CultureInfo.InvariantCulture, out height))
                    {
                        if (height < 50)
                            height = 50;
                        blackBoardPosition = new Rect (x, y, width, height);
                    }
                }
            }

            return blackBoardPosition;
        }

        public static void SavePosition (Board board, Rect r)
        {
            EditorPrefs.SetString (string.Format (rectPreferenceFormat, board), string.Format (CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r.x, r.y, r.width, r.height));
        }

        public static readonly Vector2 sizeMargin = Vector2.one * 30;

        public static bool ValidatePosition (GraphElement element, MatEffectView view, Rect defaultPosition)
        {
            Rect viewrect = view.contentRect;
            Rect rect = element.GetPosition ();
            bool changed = false;

            if (!viewrect.Contains (rect.position))
            {
                Vector2 newPosition = defaultPosition.position;
                if (!viewrect.Contains (defaultPosition.position))
                {
                    newPosition = sizeMargin;
                }

                rect.position = newPosition;

                changed = true;
            }

            Vector2 maxSizeInView = viewrect.max - rect.position - sizeMargin;
            float newWidth = Mathf.Max (element.resolvedStyle.minWidth.value, Mathf.Min (rect.width, maxSizeInView.x));
            float newHeight = Mathf.Max (element.resolvedStyle.minHeight.value, Mathf.Min (rect.height, maxSizeInView.y));

            if (Mathf.Abs (newWidth - rect.width) > 1)
            {
                rect.width = newWidth;
                changed = true;
            }

            if (Mathf.Abs (newHeight - rect.height) > 1)
            {
                rect.height = newHeight;
                changed = true;
            }

            if (changed)
            {
                element.SetPosition (rect);
            }

            return false;
        }
    }

    class MatEffectBoard : GraphElement, IGraphMovable, IGraphResizable
    {

        MatEffectView view;
        MatEffectGraph graph;
        ToolbarButton save;
        public MatEffectBoard (MatEffectView view, MatEffectGraph graph)
        {
            this.view = view;
            this.graph = graph;
            var tpl = CanvasView.LoadUXML ("MatEffectBoard");

            tpl.CloneTree (contentContainer);

            styleSheets.Add (CanvasView.LoadStyle ("MatEffectBoard"));

            save = this.Query<ToolbarButton> ("save");
            if (save != null)
            {
                save.clickable.clicked += () =>
                {
                    graph.Save();
                };
            }

            this.AddManipulator (new Dragger { clampToParentEdges = true });

            capabilities |= Capabilities.Movable;

            RegisterCallback<MouseDownEvent> (OnMouseClick, TrickleDown.TrickleDown);

            style.position = PositionType.Absolute;

            SetPosition (BoardPreferenceHelper.LoadPosition (BoardPreferenceHelper.Board.componentBoard, defaultRect));
        }

        VisualElement m_ComponentContainerParent;

        public void ValidatePosition ()
        {
            // BoardPreferenceHelper.ValidatePosition(this, m_View, defaultRect);
        }

        static readonly Rect defaultRect = new Rect (200, 100, 300, 300);

        public override Rect GetPosition ()
        {
            return new Rect (resolvedStyle.left, resolvedStyle.top, resolvedStyle.width, resolvedStyle.height);
        }

        public override void SetPosition (Rect newPos)
        {
            style.left = newPos.xMin;
            style.top = newPos.yMin;
            style.width = newPos.width;
            style.height = newPos.height;
        }

        void OnMouseClick (MouseDownEvent e)
        {
            // m_View.SetBoardToFront(this);
        }
        public void OnMoved ()
        {
            BoardPreferenceHelper.SavePosition (BoardPreferenceHelper.Board.componentBoard, GetPosition ());
        }
        public void OnStartResize () { }
        public void OnResized ()
        {
            BoardPreferenceHelper.SavePosition (BoardPreferenceHelper.Board.componentBoard, GetPosition ());
        }
    }
}
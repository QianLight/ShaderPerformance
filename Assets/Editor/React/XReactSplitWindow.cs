#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;

namespace XEditor {

    public class XBaseReactSplitWindow
    {
        //float toolHeight = 30;
        Vector2 posLeft;
        Vector2 posRight;
        GUIStyle styleLeftView;
        GUIStyle styleRightView;
        float splitterPos = 500f;
        Vector2 dragStartPos;
        bool dragging;
        float splitterWidth = 2;

        //float minLeftX = 400f;
        //float maxLeftX = 800f;

        GUIStyle slider ;
        GUIStyle boxStyle;

        EditorWindow Parent;

        public virtual void Init(EditorWindow parent)
        {
            Parent = parent;
        }

        public virtual void InitUI()
        {
            if (styleLeftView == null)
                styleLeftView = new GUIStyle();
            if (styleRightView == null)
                styleRightView = new GUIStyle(GUI.skin.scrollView);

            if (slider == null)
            {
                slider = new GUIStyle(GUI.skin.box);
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle();
                boxStyle.border = new RectOffset();
            }
        }

        public virtual void OnGUI()
        {
            
            GUILayout.BeginHorizontal();

            InitUI();
            //////////////////////////////////////////////  Left ///////////////////
            {

                // Left view
                posLeft = GUILayout.BeginScrollView(posLeft,
                    GUILayout.Width(splitterPos),
                    //GUILayout.MaxWidth(splitterPos),
                    //GUILayout.MinWidth(200f),
                    GUILayout.ExpandWidth(false));

                DrawLeft();

                GUILayout.EndScrollView();

            }

            //////////////////////////////////////////////  Splitter ///////////////////            
            GUILayout.Box("",                 GUILayout.Width(splitterWidth),
                GUILayout.MaxWidth(splitterWidth),
                GUILayout.MinWidth(splitterWidth),
                GUILayout.ExpandHeight(true));

            //////////////////////////////////////////////  Right ///////////////////
            {
                // Right view
                posRight = GUILayout.BeginScrollView(posRight,
                    GUILayout.MaxWidth(Parent.position.width - splitterPos - splitterWidth),
                    GUILayout.MinWidth(200f),
                    GUILayout.ExpandWidth(true));   

                DrawRight();

                GUILayout.EndScrollView();
            }


            GUILayout.EndHorizontal();

            // Splitter events
            //if (Event.current != null)
            //{
            //    switch (Event.current.rawType)
            //    {
            //        case EventType.MouseDown:
            //            if (splitterRect.Contains(Event.current.mousePosition))
            //            {
            //                Debug.Log("Start dragging");
            //                dragging = true;
            //            }
            //            break;
            //        case EventType.MouseDrag:
            //            if (dragging)
            //            {
                            

            //                if (Event.current.mousePosition.x <= maxLeftX && Event.current.mousePosition.x >= minLeftX)
            //                    splitterPos = Event.current.mousePosition.x;

            //                Debug.Log("moving splitter "+ splitterPos);

            //                //splitterPos += Event.current.delta.x;
            //                Repaint();
            //            }
            //            break;
            //        case EventType.MouseUp:
            //            if (dragging)
            //            {
            //                Debug.Log("Done dragging");
            //                dragging = false;
            //            }
            //            break;
            //    }
            //}
        }

        protected virtual void Repaint()
        {
        }

        protected virtual void DrawLeft()
        {

        }

        protected virtual void DrawRight()
        {

        }
    }
}

#endif
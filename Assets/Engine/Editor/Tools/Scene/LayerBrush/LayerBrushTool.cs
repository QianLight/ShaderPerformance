using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public class LayerBrushTool : CommonToolTemplate
    {
        BrushMode curMode = null;
        Vector2 m_Scroll = Vector2.zero;
        private bool selectMode = false;


        public static int s_LayeredBrushEditorHash = "LayerBrushTool".GetHashCode ();
        public override void OnInit ()
        {
            base.OnInit ();
            Init ();
        }

        public override void OnUninit ()
        {
            base.OnUninit ();
            if (curMode != null)
                curMode.OnDisable ();
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
            GUIUtility.GetControlID(s_LayeredBrushEditorHash, FocusType.Keyboard);
        }
        private void Init ()
        {
            curMode = new BrushModeTexture ();
            curMode.OnEnable ();
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }
        public override void DrawGUI (ref Rect rect)
        {
            GUILayout.Space (8);

            m_Scroll = EditorGUILayout.BeginScrollView (m_Scroll);

            BrushSettingsGUI ();

            EditorGUILayout.Space ();

            EditorGUILayout.EndScrollView ();
        }

        void BrushSettingsGUI ()
        {
            if (curMode == null)
            {
                Init ();
            }
         //   EditorGUILayout.HelpBox ("Press T to Select Objects", MessageType.Info, true);
            if (selectMode)
            {
                EditorGUILayout.HelpBox ("Select Objects", MessageType.Warning, true);
            }
            curMode.PreDraw ();

            if (!curMode.m_SupportsLayered)
            {
                return;
            }

            // Brush preset selector
            using (new GUILayout.VerticalScope ("box"))
            {
                LayeredBrushLayout.Header (LayeredBrushGUI.TempContent ("Brush Settings"));
                curMode.DrawBrushSettings ();
            }
        }
        private RaycastHit MainRay ()
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
            RaycastHit hit;
            Physics.Raycast (mouseRay, out hit, float.MaxValue, BrushMode.LayerEditMask);
            return hit;
        }
        void OnSceneGUI (SceneView sceneView)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.C)
                {
                    selectMode = true;
                    SceneTool.DoRepaint ();
                }
            }
            if (e.type == EventType.KeyUp)
            {
                if (e.keyCode == KeyCode.C)
                {
                    selectMode = false;
                  //  SceneTool.DoRepaint();
                }
                if (e.keyCode == KeyCode.A)
                {
                    BrushMode.isEditing = false;
                }
            }
         
            if (curMode == null || selectMode)
            {
                return;
            }
           
            curMode.DrawGizmos (e);
            if (BrushMode.isEditing)
            {
                int controlID = GUIUtility.GetControlID(s_LayeredBrushEditorHash, FocusType.Passive);
                var eType = e.GetTypeForControl(controlID);
                switch (eType)
                {
                    case EventType.Layout:
                        {
                            HandleUtility.AddDefaultControl(controlID);
                        }
                        break;
                    // case EventType.MouseMove:
                    //     {

                    //     }
                    //     break;
                    case EventType.MouseDown:
                    case EventType.MouseDrag:
                        {
                            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != controlID)
                            {
                                return;
                            }
                            // Don't do anything on MouseDrag if we don't own the hotControl.
                            if (e.GetTypeForControl(controlID) == EventType.MouseDrag && GUIUtility.hotControl != controlID)
                            {
                                return;
                            }
                            // If user is ALT-dragging, we want to return to main routine
                            if (Event.current.alt)
                            {
                                return;
                            }
                            if (e.button != 0)
                            {
                                return;
                            }

                            //有些机器上这个值一直是0，姑且视为unity bug
                            if (HandleUtility.nearestControl != controlID && HandleUtility.nearestControl != 0)
                            {
                                return;
                            }
                            RaycastHit hit = MainRay();
                            if (hit.transform != null)
                            {
                                if (e.type == EventType.MouseDown)
                                {
                                    GUIUtility.hotControl = controlID;
                                    OnPrePaint(hit);
                                }
                                curMode.OnPaint(hit);
                            }
                        }
                        break;
                    case EventType.MouseUp:
                        {
                            if (GUIUtility.hotControl != controlID)
                            {
                                return;
                            }
                            OnEndPaint();
                        }
                        break;
                }
            }
            else
            {
                GUIUtility.GetControlID(s_LayeredBrushEditorHash, FocusType.Keyboard);
            }       
        }

        private void OnPrePaint (RaycastHit hit)
        {
            if (curMode != null)
            {
                curMode.OnPrePaint (hit);
            }
        }

        private void OnEndPaint ()
        {
            if (curMode != null)
            {
                curMode.OnEndPaint ();
            }
        }
        void OnSelectionChanged ()
        {
            if (curMode != null)
            {
                curMode.OnSelectionChanged ();
            }
            SceneTool.DoRepaint ();
        }

       

        public override void Update ()
        {
            if (curMode != null)
            {
                curMode.Update ();
            }
        }
    }
}
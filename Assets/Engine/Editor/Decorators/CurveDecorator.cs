using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [CFDecorator (typeof (CFTextureCurveAttribute))]
    public sealed class CurveDecorator : AttributeDecorator<CFTextureCurveAttribute, TextureCurveParam>
    {
        private static CurveEditor curveEditor;
        private static Material s_MaterialGrid;
        private static GUIStyle s_PreLabel;
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.AnimationCurve;
        }
        // protected override bool EndOverride (ref byte mask)
        // {
        //     base.EndOverride (ref mask);
        //     if (overrideParam != null)
        //     {
        //         if (valueChange)
        //         {
        //             overrideParam.value = editValue;
        //         }
        //         return valueChange;
        //     }
        //     else
        //     {
        //         if (valueChange)
        //         {
        //             sp.colorValue = editValue;
        //         }
        //         return true;
        //     }
        // }
        protected override void InitEditValue ()
        {
            base.InitEditValue ();
            if (curveEditor == null)
            {
                curveEditor = new CurveEditor ();
            }
        }
        private static void DrawBackgroundTexture (ref Rect rect, int pass)
        {
            if (s_MaterialGrid == null)
                s_MaterialGrid = new Material (Shader.Find ("Hidden/PostProcessing/Editor/CurveBackground")) { hideFlags = HideFlags.HideAndDontSave };

            float scale = EditorGUIUtility.pixelsPerPoint;

            var oldRt = RenderTexture.active;
            var rt = RenderTexture.GetTemporary (Mathf.CeilToInt (rect.width * scale), Mathf.CeilToInt (rect.height * scale), 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            s_MaterialGrid.SetFloat ("_DisabledState", GUI.enabled ? 1f : 0.5f);

            Graphics.Blit (null, rt, s_MaterialGrid, pass);
            RenderTexture.active = oldRt;

            GUI.DrawTexture (rect, rt);
            RenderTexture.ReleaseTemporary (rt);
        }
        private static void DrawBackground (ref Rect rect, ref Rect innerRect, int background)
        {
            if (Event.current.type == EventType.Repaint)
            {
                // Background
                EditorGUI.DrawRect (rect, new Color (0.15f, 0.15f, 0.15f, 1f));
                if (background >= 0)
                {
                    DrawBackgroundTexture (ref innerRect, background);
                }

                // Bounds
                Handles.color = Color.white * (GUI.enabled ? 1f : 0.5f);
                Handles.DrawSolidRectangleWithOutline (innerRect, Color.clear, new Color (0.8f, 0.8f, 0.8f, 0.5f));

                // Grid setup
                Handles.color = new Color (1f, 1f, 1f, 0.05f);
                int hLines = (int) Mathf.Sqrt (innerRect.width);
                int vLines = (int) (hLines / (innerRect.width / innerRect.height));

                // Vertical grid
                int gridOffset = Mathf.FloorToInt (innerRect.width / hLines);
                int gridPadding = ((int) (innerRect.width) % hLines) / 2;

                for (int i = 1; i < hLines; i++)
                {
                    var offset = i * Vector2.right * gridOffset;
                    offset.x += gridPadding;
                    Handles.DrawLine (innerRect.position + offset, new Vector2 (innerRect.x, innerRect.yMax - 1) + offset);
                }

                // Horizontal grid
                gridOffset = Mathf.FloorToInt (innerRect.height / vLines);
                gridPadding = ((int) (innerRect.height) % vLines) / 2;

                for (int i = 1; i < vLines; i++)
                {
                    var offset = i * Vector2.up * gridOffset;
                    offset.y += gridPadding;
                    Handles.DrawLine (innerRect.position + offset, new Vector2 (innerRect.xMax - 1, innerRect.y) + offset);
                }
            }
        }

        private static void DrawForground (ref Rect rect, ref Rect innerRect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                // // Borders
                // Handles.color = Color.black;
                // Handles.DrawLine (new Vector2 (rect.x, rect.y - 20f), new Vector2 (rect.xMax, rect.y - 20f));
                // Handles.DrawLine (new Vector2 (rect.x, rect.y - 21f), new Vector2 (rect.x, rect.yMax));
                // Handles.DrawLine (new Vector2 (rect.x, rect.yMax), new Vector2 (rect.xMax, rect.yMax));
                // Handles.DrawLine (new Vector2 (rect.xMax, rect.yMax), new Vector2 (rect.xMax, rect.y - 20f));

                bool editable = curveEditor.IsEdit ();
                string editableString = editable ? string.Empty : "(Not Overriding)\n";

                // Selection info
                var selection = curveEditor.GetSelection ();
                var infoRect = innerRect;
                infoRect.x += 5f;
                infoRect.width = 100f;
                infoRect.height = 30f;

                if (s_PreLabel == null)
                    s_PreLabel = new GUIStyle ("ShurikenLabel");
                s_PreLabel.normal.textColor = color;
                if (selection.curve != null && selection.keyframeIndex > -1)
                {
                    var key = selection.keyframe.Value;
                    GUI.Label (infoRect, $"{key.time:F3}\n{key.value:F3}", s_PreLabel);
                }
                else
                {
                    GUI.Label (infoRect, editableString, s_PreLabel);
                }
            }
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            var property = spo as SerializedParameterOverride;
            bool isEdit = false;
            if (overrideParam != null)
            {
                isEdit = maskX;
            }
            else
            {
                isEdit = property.overrideState.boolValue;
            }
            EditorGUI.BeginChangeCheck ();
            var overrideRect = EditorUtilities.DrawOverrideCheckbox (ref isEdit);
            if (EditorGUI.EndChangeCheck ())
            {
                if (overrideParam != null)
                {
                    overrideToggleChange = true;
                    valueChange = true;
                    maskX = isEdit;
                    overrideParam.overrideState = isEdit;
                }
                else
                {
                    property.overrideState.boolValue = isEdit;
                }
                editParam.SetDirty ();
            }
            overrideRect.x += 20;
            overrideRect.width = 100;
            GUI.Label (overrideRect, title);
            var rect = GUILayoutUtility.GetAspectRect (1.5f);
            if (isEdit && overrideParam == null)
            {
                overrideRect.x = rect.width;
                overrideRect.y -= 2;
                overrideRect.width = 20;
                overrideRect.height = 18;
                if (GUI.Button (overrideRect, "R"))
                {
                    editParam.InitCurve (attr.keyCount, attr.curveType, true);
                }
            }

            rect.x -= 17;
            rect.y += 21;
            rect.width += 17;
            rect.height -= 21;
            var innerRect = new RectOffset (10, 10, 10, 10).Remove (rect);

            DrawBackground (ref rect, ref innerRect, attr.background);
            using (new GUI.ClipScope (innerRect))
            {
                var state = CurveEditor.CurveState.defaultState;
                state.color = attr.color;
                state.minPointCount = attr.keyCount;
                state.onlyShowHandlesOnSelection = true;
                state.zeroKeyConstantValue = 0.5f;
                state.loopInBounds = attr.loop;
                curveEditor.Set (editParam.value, ref state);
                if (curveEditor.OnGUI (new Rect (0, 0, innerRect.width - 1, innerRect.height - 1)))
                {
                    // Repaint ();
                    valueChange = true;
                    GUI.changed = true;
                    editParam.SetDirty ();
                }
            }
            DrawForground (ref rect, ref innerRect, attr.color);

        }

        public override void InnerOnPreGUI (float width)
        {

        }
        public override void InnerOnPostGUI ()
        {

        }
        public override void InnerResetValue () { }
    }
}
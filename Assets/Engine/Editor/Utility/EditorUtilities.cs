using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace CFEngine.Editor
{
    public static class EditorUtilities
    {
        static EnvSetting s_ClipboardContent;

        public static bool isTargetingConsoles
        {
            get
            {
                var t = EditorUserBuildSettings.activeBuildTarget;
                return t == BuildTarget.PS4 ||
                    t == BuildTarget.XboxOne ||
                    t == BuildTarget.Switch;
            }
        }

        public static bool isTargetingMobiles
        {
            get
            {
                var t = EditorUserBuildSettings.activeBuildTarget;
                return t == BuildTarget.Android ||
                    t == BuildTarget.iOS ||
                    t == BuildTarget.tvOS;
            }
        }

        public static bool isTargetingConsolesOrMobiles
        {
            get { return isTargetingConsoles || isTargetingMobiles; }
        }

        public static GUIContent GetContent (string textAndTooltip)
        {
            return RuntimeUtilities.GetContent (textAndTooltip);
        }

        public static void DrawFixMeBox (string text, Action action)
        {
            Assert.IsNotNull (action);

            EditorGUILayout.HelpBox (text, MessageType.Warning);

            GUILayout.Space (-32);
            using (new EditorGUILayout.HorizontalScope ())
            {
                GUILayout.FlexibleSpace ();

                if (GUILayout.Button ("Fix", GUILayout.Width (60)))
                    action ();

                GUILayout.Space (8);
            }
            GUILayout.Space (11);
        }

        public static void DrawSplitter ()
        {
            var rect = GUILayoutUtility.GetRect (1f, 1f);

            // Splitter rect should be full-width
            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect (rect, !EditorGUIUtility.isProSkin ?
                new Color (0.6f, 0.6f, 0.6f, 1.333f) :
                new Color (0.12f, 0.12f, 0.12f, 1.333f));
        }

        public static void DrawOverrideCheckbox (Rect rect, SerializedProperty property)
        {
            var oldColor = GUI.color;
            GUI.color = new Color (0.6f, 0.6f, 0.6f, 0.75f);
            property.boolValue = GUI.Toggle (rect, property.boolValue, GetContent ("|Override."), Styling.smallTickbox);
            GUI.color = oldColor;
        }

        public static Rect DrawOverrideCheckbox (ref bool toggle)
        {
            var rect = GUILayoutUtility.GetRect (17f, 17f, GUILayout.ExpandWidth (false));
            rect.yMin += 4f;
            var oldColor = GUI.color;
            GUI.color = new Color (0.6f, 0.6f, 0.6f, 0.75f);
            toggle = GUI.Toggle (rect, toggle, GetContent ("|Override."), Styling.smallTickbox);
            GUI.color = oldColor;

            return rect;
        }
        public static void DrawHeaderLabel (string title)
        {
            EditorGUILayout.LabelField (title, Styling.labelHeader);
        }

        public static void DrawRect (string title)
        {
            var backgroundRect = GUILayoutUtility.GetRect (1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
            EditorGUI.DrawRect (backgroundRect, new Color (backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            EditorGUI.LabelField (labelRect, GetContent (title), EditorStyles.boldLabel);
        }

        public static bool DrawHeader (string title, SerializedProperty group, EnvSetting target, Action resetAction, Action moveUpAction, Action moveDownAction, Action removeAction)
        {
            Assert.IsNotNull (group);
            Assert.IsNotNull (target);

            var backgroundRect = GUILayoutUtility.GetRect (1f, 17f);

            var labelRect = backgroundRect;
            labelRect.xMin += 16f;
            labelRect.xMax -= 20f;

            var toggleRect = backgroundRect;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;

            var menuIcon = EditorGUIUtility.isProSkin ?
                Styling.paneOptionsIconDark :
                Styling.paneOptionsIconLight;

            var menuRect = new Rect (labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            float backgroundTint = EditorGUIUtility.isProSkin ? 0.1f : 1f;
            EditorGUI.DrawRect (backgroundRect, new Color (backgroundTint, backgroundTint, backgroundTint, 0.2f));

            // Title
            using (new EditorGUI.DisabledScope (!target.active))
            EditorGUI.LabelField (labelRect, GetContent (title), EditorStyles.boldLabel);

            // Active checkbox
            //activeField.serializedObject.Update();
            target.active.value = GUI.Toggle (toggleRect, target.active.value, GUIContent.none, Styling.smallTickbox);
            //activeField.serializedObject.ApplyModifiedProperties();

            // Dropdown menu icon
            GUI.DrawTexture (menuRect, menuIcon);

            // Handle events
            var e = Event.current;

            if (e.type == EventType.MouseDown)
            {
                if (menuRect.Contains (e.mousePosition))
                {
                    ShowHeaderContextMenu (new Vector2 (menuRect.x, menuRect.yMax), target, resetAction, moveUpAction, moveDownAction, removeAction);
                    e.Use ();
                }
                else if (labelRect.Contains (e.mousePosition))
                {
                    if (e.button == 0)
                        group.isExpanded = !group.isExpanded;
                    else
                        ShowHeaderContextMenu (e.mousePosition, target, resetAction, moveUpAction, moveDownAction, removeAction);

                    e.Use ();
                }
            }

            return group.isExpanded;
        }

        static void ShowHeaderContextMenu (Vector2 position, EnvSetting target, Action resetAction, Action moveUpAction, Action moveDownAction, Action removeAction)
        {
            Assert.IsNotNull (resetAction);
            Assert.IsNotNull (removeAction);

            var menu = new GenericMenu ();
            menu.AddItem (GetContent ("Reset"), false, () => resetAction ());
            menu.AddItem (GetContent ("MoveUp"), false, () => moveUpAction ());
            menu.AddItem (GetContent ("MoveDown"), false, () => moveDownAction ());
            menu.AddItem (GetContent ("Remove"), false, () => removeAction ());
            menu.AddSeparator (string.Empty);
            menu.AddItem (GetContent ("Copy Settings"), false, () => CopySettings (target));

            if (CanPaste (target))
                menu.AddItem (GetContent ("Paste Settings"), false, () => PasteSettings (target));
            else
                menu.AddDisabledItem (GetContent ("Paste Settings"));

            menu.DropDown (new Rect (position, Vector2.zero));
        }

        static void CopySettings (EnvSetting target)
        {
            Assert.IsNotNull (target);

            if (s_ClipboardContent != null)
            {
                EngineUtility.Destroy (s_ClipboardContent);
                s_ClipboardContent = null;
            }

            s_ClipboardContent = (EnvSetting) ScriptableObject.CreateInstance (target.GetType ());
            EditorUtility.CopySerializedIfDifferent (target, s_ClipboardContent);
        }

        static void PasteSettings (EnvSetting target)
        {
            Assert.IsNotNull (target);
            Assert.IsNotNull (s_ClipboardContent);
            Assert.AreEqual (s_ClipboardContent.GetType (), target.GetType ());

            Undo.RecordObject (target, "Paste Settings");
            EditorUtility.CopySerializedIfDifferent (s_ClipboardContent, target);
        }

        static bool CanPaste (EnvSetting target)
        {
            return s_ClipboardContent != null &&
                s_ClipboardContent.GetType () == target.GetType ();
        }
    }
}
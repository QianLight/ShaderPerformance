using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine.Editor
{
    public class EnvEffectBaseEditor
    {
        internal EnvSetting target { get; private set; }
        internal SerializedObject serializedObject { get; private set; }

        internal SerializedProperty baseProperty;
        //internal SerializedProperty activeProperty;
        UnityEngineEditor m_Inspector;

        // static HelpDropdown helpWindow;
        // static string[] enableType = new string[] { "enable", "disable" };
        internal EnvEffectBaseEditor () { }

        // public void Repaint ()
        // {
        //     m_Inspector.Repaint ();
        // }

        internal void Init (EnvSetting target, UnityEngineEditor inspector)
        {
            this.target = target;
            m_Inspector = inspector;
            serializedObject = new SerializedObject (target);
            //activeProperty = serializedObject.FindProperty("active");
            OnEnable ();
        }

        public virtual void OnEnable () { }

        public virtual void OnDisable () { }

        internal void OnInternalInspectorGUI ()
        {
            serializedObject.Update ();
            EditorGUI.BeginChangeCheck ();
            // TopRowFields ();
            OnInspectorGUI ();
            if (EditorGUI.EndChangeCheck ())
            {
                //DebugLog.AddErrorLog("value changed");
                var context = EngineContext.instance;
                if (EngineContext.IsRunning && context != null)
                {
                    var effectType = target.GetEnvType();
                    var env = context.envModifys[(int)effectType];
                    if (env != null && env.modify != null)
                    {
                        env.modify.DirtySetting();
                    }
                }

                if (!Application.isPlaying)
                    SceneAssets.SceneModify();
            }
            EditorGUILayout.Space ();
            serializedObject.ApplyModifiedProperties ();
        }

        public virtual void OnInspectorGUI () { }

        internal void OnInternalSceneGUI ()
        {
            OnSceneGUI ();
        }
        // public virtual void OnDrawGizmos () { }

        public virtual void OnSceneGUI () { }
        public virtual string GetDisplayTitle ()
        {
            return ObjectNames.NicifyVariableName (target.GetType ().Name);
        }

        public static void InitProperty (SerializedParameterOverride property)
        {
            foreach (var attr in property.attributes)
            {
                // Use the first decorator we found
                if (property.decorator == null)
                {
                    property.decorator = AttributeDecorator.GetDecorator (attr.GetType ());
                    property.decoratorAttr = attr;
                    if (property.decorator != null)
                        return;
                }
            }
        }

        public static void PropertyField (SerializedParameterOverride property)
        {
            if (property != null)
            {
                var title = EditorUtilities.GetContent (property.displayName);
                PropertyField (property, title);
            }
        }

        private static void DrawDecorator (SerializedParameterOverride property, GUIContent title,
            AttributeDecorator decorator, bool customOverride)
        {
            if (decorator != null)
            {
                Attribute attribute = property.decoratorAttr;
                RuntimeParamOverride param = AttributeDecorator.DebugRuntimeParam (property);
                if (decorator.OnGUI (property, title, attribute, param))
                {
                    if (!customOverride)
                    {
                        if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                        {
                            decorator.ResetValue (property, attribute);
                        }
                    }

                    GUI.color = Color.white;
                }
            }
            else
            {
                if (property.value.hasVisibleChildren &&
                    property.value.propertyType != SerializedPropertyType.Vector2 &&
                    property.value.propertyType != SerializedPropertyType.Vector3 &&
                    property.value.propertyType != SerializedPropertyType.Vector4)
                {
                    GUILayout.Space (12f);
                    EditorGUILayout.PropertyField (property.value, title, true);
                }
                else
                {
                    EditorGUILayout.PropertyField (property.value, title, true);
                }
            }

        }

        protected static void PropertyField (SerializedParameterOverride property, GUIContent title)
        {
            // Check for DisplayNameAttribute first
            var displayNameAttr = property.GetAttribute<CFDisplayNameAttribute> ();
            if (displayNameAttr != null)
                title.text = displayNameAttr.displayName;
            bool customOverride = property.GetAttribute<CFCustomOverrideAttribute> () != null;

            AttributeDecorator decorator = property.decorator;
            using (new EditorGUILayout.HorizontalScope ())
            {

                if (customOverride)
                {
                    DrawDecorator (property, title, decorator, customOverride);
                }
                else
                {
                    // Override checkbox
                    var overrideRect = GUILayoutUtility.GetRect (17f, 17f, GUILayout.ExpandWidth (false));
                    overrideRect.yMin += 4f;
                    EditorUtilities.DrawOverrideCheckbox (overrideRect, property.overrideState);
                    using (new EditorGUI.DisabledScope (!property.overrideState.boolValue))
                    {
                        DrawDecorator (property, title, decorator, customOverride);
                    }
                }
            }
        }
    }
}
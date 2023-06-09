﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace CFEngine.Editor
{
    /// <summary>
    /// Factory for preconfigured VisualElement instances.
    /// </summary>
    public static class ControlElementFactory
    {
        public static VisualElement CreateControl (FieldInfo fieldInfo, NodeView view, string label = null)
        {
            // This mess is similar to what Unity is doing for ShaderGraph. It's not great.
            // But the automatic alternatives depend on SerializableObject which is a performance bottleneck.
            // Ref: https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/master/com.unity.shadergraph/Editor/Drawing/Controls/DefaultControl.cs

            Type type = fieldInfo.FieldType;

            // Builtin unity type editors
            if (type == typeof (bool))
                return BuildVal<Toggle, bool> (view, fieldInfo, label);

            if (type == typeof (int))
                return BuildVal<IntegerField, int> (view, fieldInfo, label);

            if (type == typeof (float))
                return BuildVal<FloatField, float> (view, fieldInfo, label);

            if (type == typeof (string))
                return BuildVal<TextField, string> (view, fieldInfo, label);

            if (type == typeof (Rect))
                return BuildVal<RectField, Rect> (view, fieldInfo, label);

            if (type == typeof (Color))
                return BuildVal<ColorField, Color> (view, fieldInfo, label);

            if (type == typeof (Vector2))
                return BuildVal<Vector2Field, Vector2> (view, fieldInfo, label);

            if (type == typeof (Vector3))
                return BuildVal<Vector3Field, Vector3> (view, fieldInfo, label);

            if (type == typeof (Vector4))
                return BuildVal<Vector4Field, Vector4> (view, fieldInfo, label);

            if (type == typeof (Gradient))
                return BuildVal<GradientField, Gradient> (view, fieldInfo, label);

            if (type == typeof (AnimationCurve))
                return BuildVal<CurveField, AnimationCurve> (view, fieldInfo, label);

            if (type == typeof (uint))
            {
                var attr = fieldInfo.GetCustomAttribute (typeof (MaskAttribute), false) as MaskAttribute;
                if (attr != null)
                {
                    var masks = attr.choise;
                    var defaultIndex = (uint) fieldInfo.GetValue (view.target);
                    var field = new MaskField (label, masks, (int)defaultIndex);

                    field.RegisterValueChangedCallback ((change) =>
                    {
                        fieldInfo.SetValue (view.target, (uint)field.value);
                        view.OnPropertyChange ();
                    });

                    return field;
                }
                else
                {
                    return BuildVal<IntegerField, int> (view, fieldInfo, label);
                }
            }

            if (type == typeof (LayerMask))
            {
                var value = ((LayerMask) fieldInfo.GetValue (view.target)).value;
                var field = new LayerMaskField (label, value);

                field.RegisterValueChangedCallback ((change) =>
                {
                    fieldInfo.SetValue (view.target, (LayerMask) change.newValue);
                    view.OnPropertyChange ();
                });

                return field;
                // return BuildVal<LayerMaskField, int>(view, fieldInfo, label);
            }

            // Implementation (rather than just using EnumField) comes from:
            // https://github.com/Unity-Technologies/UnityCsReference/blob/1e8347ec4cbda9e8a4929e42a20f39df9bbab9d9/Editor/Mono/UIElements/Controls/PropertyField.cs#L306-L323

            if (typeof (Enum).IsAssignableFrom (type))
            {
                var choices = new List<string> (type.GetEnumNames ());
                var defaultIndex = (int) fieldInfo.GetValue (view.target);

                // if (type.IsDefined(typeof(FlagsAttribute), false))
                // {
                //     var field = new EnumFlagsField(label, (Enum)fieldInfo.GetValue(view.target));

                //     field.RegisterValueChangedCallback((change) =>
                //     {
                //         fieldInfo.SetValue(view.target, change.newValue);
                //         view.OnPropertyChange();
                //     });

                //     return field;
                // }
                // else
                {
                    if (defaultIndex >= choices.Count)
                        defaultIndex = choices.Count - 1;
                    var field = new PopupField<string> (label, choices, defaultIndex);

                    field.RegisterValueChangedCallback ((change) =>
                    {
                        fieldInfo.SetValue (view.target, field.index);
                        view.OnPropertyChange ();
                    });

                    return field;
                }
            }

            // Specialized construct so I can set .objectType on the ObjectField
            if (typeof (Object).IsAssignableFrom (type))
            {
                var field = BuildRef<ObjectField, Object> (view, fieldInfo, label) as ObjectField;
                field.objectType = type;
                return field;
            }

            // TODO: EnumFlags/Masks (we have MaskField - how do we detect mask types?)

            // TODO: Specialized common types. Transform, Rotation, Texture2D, etc.

            // TODO: Custom plugin types

            return null;
        }

        /// <summary>
        /// Generic factory for instantiating and configuring builtin Unity controls for value types
        /// 
        /// The control will be created and bound to the given NodeView and its associated target node. 
        /// </summary>
        static VisualElement BuildVal<TField, TType> (NodeView view, FieldInfo fieldInfo, string label)
        where TField : BaseField<TType>, new ()
        {
            try
            {
                var field = new TField ();
                field.label = label;
                field.SetValueWithoutNotify ((TType) fieldInfo.GetValue (view.target));
                field.RegisterValueChangedCallback ((change) =>
                {
                    fieldInfo.SetValue (view.target, change.newValue);
                    view.OnPropertyChange ();
                });

                return field;
            }
            catch (InvalidCastException e)
            {
                Debug.LogError (
                    $"Failed to build control for {view.target.name}:{fieldInfo.Name} of type {fieldInfo.FieldType}: {e}"
                );

                return null;
            }
        }

        /// <summary>
        /// Generic factory for instantiating and configuring builtin Unity controls for reference types
        /// 
        /// The control will be created and bound to the given NodeView and its associated target node. 
        /// </summary>
        static VisualElement BuildRef<TField, TType> (NodeView view, FieldInfo fieldInfo, string label)
        where TField : BaseField<TType>, new ()
        where TType : class
        {
            try
            {
                var field = new TField ();
                field.label = label;
                field.SetValueWithoutNotify (fieldInfo.GetValue (view.target) as TType);
                field.RegisterValueChangedCallback ((change) =>
                {
                    fieldInfo.SetValue (view.target, change.newValue);
                    view.OnPropertyChange ();
                });

                return field;
            }
            catch (InvalidCastException e)
            {
                Debug.LogError (
                    $"Failed to build control for {view.target.name}:{fieldInfo.Name} of type {fieldInfo.FieldType}: {e}"
                );

                return null;
            }
        }
    }
}
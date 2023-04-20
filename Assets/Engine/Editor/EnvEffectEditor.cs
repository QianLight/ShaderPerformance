using System;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class EnvEffectEditor<T> : EnvEffectBaseEditor
    where T : EnvSetting
    {
        protected SerializedProperty FindProperty<TValue> (Expression<Func<T, TValue>> expr)
        {
            return serializedObject.FindProperty (RuntimeUtilities.GetFieldPath (expr));
        }

        public SerializedParameterOverride FindParameterOverride<TValue> (Expression<Func<T, TValue>> expr)
        {
            var property = serializedObject.FindProperty (RuntimeUtilities.GetFieldPath (expr));
            var attributes = RuntimeUtilities.GetMemberAttributes (expr);
            var spo = new SerializedParameterOverride (property, attributes);
            InitProperty (spo);
            return spo;
        }
        public ClassSerializedParameterOverride FindClassParameterOverride<TValue> (Expression<Func<T, TValue>> expr, ParamOverride param)
        {
            var property = serializedObject.FindProperty (RuntimeUtilities.GetFieldPath (expr));
            var attributes = RuntimeUtilities.GetMemberAttributes (expr);
            var cspo = new ClassSerializedParameterOverride (property, attributes, param);
            InitProperty (cspo);
            return cspo;
        }

        protected GameObject GetGameObject ()
        {
            return Selection.activeGameObject;
        }
    }
}
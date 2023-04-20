using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CFEngine.Editor
{
    public class DefaultEnvEffectEditor : EnvEffectBaseEditor
    {
        List<SerializedParameterOverride> m_Parameters;

        public override void OnEnable ()
        {
            m_Parameters = new List<SerializedParameterOverride> ();

            var fields = target.GetType ()
                .GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where (t => t.FieldType.IsSubclassOf (typeof (ParamOverride)))
                .Where (t =>
                    (t.IsPublic && t.GetCustomAttributes (typeof (NonSerializedAttribute), false).Length == 0) ||
                    (t.GetCustomAttributes (typeof (UnityEngine.SerializeField), false).Length > 0)
                )
                .ToList ();

            foreach (var field in fields)
            {
                var property = serializedObject.FindProperty (field.Name);
                var attributes = field.GetCustomAttributes (false).Cast<Attribute> ().ToArray ();
                SerializedParameterOverride parameter = null;
                if (field.GetType () == typeof (ResParam))
                {
                    parameter = new ClassSerializedParameterOverride (property, attributes,field.GetValue(target) as ParamOverride);
                }
                else
                    parameter = new SerializedParameterOverride (property, attributes);
                m_Parameters.Add (parameter);
            }
        }

        public override void OnInspectorGUI ()
        {
            for (int i = 0; i < m_Parameters.Count; ++i)
            {
                PropertyField (m_Parameters[i]);
            }
        }
    }
}
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    public class OverrideParameter
    {
        public Vector4 value0;
        public Vector4 value1;
        public byte byteMask;
        public bool maskReadOnly;

        public void SetMask (byte mask, bool add)
        {
            if (add)
            {
                byteMask |= mask;
            }
            else
            {
                byteMask &= (byte) (~(mask));
            }
        }
    }

    public class SerializedParameter
    {
        public SerializedProperty value { get; protected set; }
        public Attribute[] attributes { get; protected set; }

        public SerializedProperty baseProperty;

        public AttributeDecorator decorator;
        public Attribute decoratorAttr;
        public string displayName
        {
            get { return baseProperty.displayName; }
        }
        public SerializedParameter ()
        {

        }
        public SerializedParameter (SerializedProperty property, Attribute[] attributes)
        {
            baseProperty = property.Copy ();

            // var localCopy = baseProperty.Copy();
            // localCopy.Next(true);
            value = baseProperty.Copy ();

            this.attributes = attributes;
        }

        public T GetAttribute<T> ()
        where T : Attribute
        {
            return (T) attributes.FirstOrDefault (x => x is T);
        }
    }

    public class SerializedParameterOverride : SerializedParameter
    {
        public SerializedProperty overrideState { get; private set; }

        public SerializedParameterOverride (SerializedProperty property, Attribute[] attributes) : base ()
        {
            if (property == null)
            {
                Debug.LogError($"new SerializedParameterOverride fail, property is null.");
            }

            baseProperty = property.Copy ();

            var localCopy = baseProperty.Copy ();
            localCopy.Next (true);
            overrideState = localCopy.Copy ();
            localCopy.Next (false);
            value = localCopy.Copy ();

            this.attributes = attributes;
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
    }

    public delegate void ButtonCallback ();

    public class ClassSerializedParameterOverride : SerializedParameterOverride
    {
        public ParamOverride param { get; private set; }
        // public ButtonCallback onButton { get; set; }
        // public ButtonCallback onButton2 { get; set; }
        // public ButtonCallback onButton3 { get; set; }
        public ClassSerializedParameterOverride (SerializedProperty property, Attribute[] attributes, ParamOverride p) : base (property, attributes)
        {
            param = p;
        }
    }
}
#endif
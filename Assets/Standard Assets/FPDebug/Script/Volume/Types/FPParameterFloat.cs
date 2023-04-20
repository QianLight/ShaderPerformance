using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class FPParameterFloat : IFPParameter
{
    public override bool OnGUI()
    {
#if UNITY_EDITOR
        FloatValue = EditorGUILayout.FloatField(ParaName, FloatValue);
        if (floatValue != FloatValue)
        {
            floatValue = FloatValue;
            ValueString = floatValue.ToString();
            return true;
        }
#endif
        return false;
    }
    public override void SetValue(string valueStr)
    {
        FloatValue = floatValue = float.Parse(valueStr);
        ValueString = valueStr;
    }
    public override string Object2String(object obj)
    {
        float floatValue = (float)obj;
        return floatValue.ToString();
    }
    public override object Value()
    {
        return FloatValue;
    }
    private float floatValue;
    public float FloatValue;
}

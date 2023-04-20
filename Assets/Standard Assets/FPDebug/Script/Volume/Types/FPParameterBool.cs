using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPParameterBool : IFPParameter
{
    public override object Value()
    {
        return BoolValue;
    }
    public override bool OnGUI()
    {
#if UNITY_EDITOR
        BoolValue = UnityEditor.EditorGUILayout.Toggle(ParaName, BoolValue);
        if (boolValue != BoolValue)
        {
            boolValue = BoolValue;
            ValueString = BoolValue.ToString();
            return true;
        }
#endif
        return false;
    }
    public override void SetValue(string valueStr)
    {
        BoolValue = boolValue = bool.Parse(valueStr);
        ValueString = valueStr;
    }
    public override string Object2String(object obj)
    {
        bool boolValue = (bool)obj;
        return boolValue.ToString();
    }
    private bool boolValue;
    public bool BoolValue;
}

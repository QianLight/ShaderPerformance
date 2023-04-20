using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPParameterInt : IFPParameter
{
    public override object Value()
    {
        return IntValue;
    }
    public override bool OnGUI()
    {
#if UNITY_EDITOR
        IntValue = UnityEditor.EditorGUILayout.IntField(ParaName, IntValue);
        if (intValue != IntValue)
        {
            intValue = IntValue;
            ValueString = IntValue.ToString();
            return true;
        }
#endif
        return false;
    }
    public override void SetValue(string valueStr)
    {
        IntValue = intValue = int.Parse(valueStr);
        ValueString = valueStr;
    }
    public override string Object2String(object obj)
    {
        int intValue = (int)obj;
        return intValue.ToString();
    }
    private int intValue;
    public int IntValue;
}

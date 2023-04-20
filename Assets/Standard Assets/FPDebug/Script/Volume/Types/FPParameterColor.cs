using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FPParameterColor : IFPParameter
{
    public override object Value()
    {
        return ColorValue;
    }
    public override bool OnGUI()
    {
#if UNITY_EDITOR
        ColorValue = UnityEditor.EditorGUILayout.ColorField(ParaName, ColorValue);
        if (colorValue != ColorValue)
        {
            colorValue = ColorValue;
            ValueString = Color2String(colorValue);
            return true;
        }
#endif
        return false;
    }
    public override void SetValue(string valueStr)
    {
        ColorValue = colorValue = String2Color(valueStr);
        ValueString = valueStr;
    }
    public override string Object2String(object obj)
    {
        Color colorValue = (Color)obj;
        return Color2String(colorValue);
    }
    public Color String2Color(string str)
    {
        string[] sp = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        return new Color(float.Parse(sp[0]), float.Parse(sp[1]), float.Parse(sp[2]), float.Parse(sp[3]));
    }
    public string Color2String(Color color)
    {
        return string.Concat(color.r.ToString(), ",", color.g.ToString(), ",", color.b.ToString(), ",", color.a.ToString());
    }
    private Color colorValue;
    public Color ColorValue;
}

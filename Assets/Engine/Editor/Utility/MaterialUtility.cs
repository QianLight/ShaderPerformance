using CFEngine;
using System;
using UnityEngine;

public static class MaterialUtility
{
    public static float GetFloat(Material mat, string desc)
    {
        ParsePropertyDesc(desc, out string property, out int component, out bool isVector);

        if (isVector)
        {
            return mat.GetVector(property)[component];
        }
        else
        {
            return mat.GetFloat(desc);
        }
    }

    public static bool GetBool(Material mat, string desc, Func<float, bool> condition = null)
    {
        float fvalue = GetFloat(mat, desc);
        return condition != null ? condition(fvalue) : fvalue > 0;
    }

    public static void SetFloat(Material mat, string desc, float value)
    {
        ParsePropertyDesc(desc, out string property, out int component, out bool isFloat4);
        if (isFloat4)
        {
            if (ShaderUtility.GetPropertyInfo(mat.shader, property, out var pInfo))
            {
                if (pInfo.type == UnityEditor.ShaderUtil.ShaderPropertyType.Color)
                {
                    Vector4 temp = mat.GetVector(property);
                    temp[component] = value;
                    mat.SetVector(property, temp);
                }
                else if (pInfo.type == UnityEditor.ShaderUtil.ShaderPropertyType.Vector)
                {
                    Color temp = mat.GetColor(property);
                    temp[component] = value;
                    mat.SetColor(property, temp);
                }
                else
                {
                    Debug.LogError($"Set float4's component fail, type is not float4, mat = {mat}, property = {property}, component = {component}, type = {pInfo.type}");
                }
            }
            else
            {
                Debug.LogError($"Set float4's component fail, property not exist, mat = {mat}, property = {property}");
            }
        }
        else
        {
            mat.SetFloat(property, value);
        }
    }

    public static void SetBool(Material mat, string desc, bool value, Func<bool, float> setter = null)
    {
        float fvalue = setter == null ? (value ? 1 : 0) : setter(value);
        SetFloat(mat, desc, fvalue);
    }

    private static void ParsePropertyDesc(string desc, out string property, out int component, out bool isFloat4)
    {
        int dotIndex = desc.IndexOf('.');
        if (dotIndex > 0)
        {
            string[] temp = desc.Split('.');
            char componentName = temp[1][0];
            if (GetComponentIndex(componentName, out component))
            {
                isFloat4 = true;
                property = temp[0];
            }
            else
            {
                isFloat4 = false;
                property = desc;
            }
        }
        else
        {
            property = desc;
            component = -1;
            isFloat4 = false;
        }
    }

    public static bool GetComponentIndex(char componentName, out int component)
    {
        if (componentName == 'x')
        {
            component = 0;
            return true;
        }
        else if (componentName == 'y')
        {
            component = 1;
            return true;
        }
        else if (componentName == 'z')
        {
            component = 2;
            return true;
        }
        else if (componentName == 'w')
        {
            component = 3;
            return true;
        }

        component = default;
        return false;
    }
}

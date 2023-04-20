using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class FPVolumeHelp
{
    public static void InitTypes()
    {
        if (init)
        {
            //-----------------volume的参数类型在这里注册-----------------------------
            RegisterTypes(FPParameterType.ClampedFloatParameter, typeof(FPParameterFloat));
            RegisterTypes(FPParameterType.MinFloatParameter, typeof(FPParameterFloat));
            RegisterTypes(FPParameterType.ColorParameter, typeof(FPParameterColor));
            RegisterTypes(FPParameterType.ClampedIntParameter, typeof(FPParameterInt));
            RegisterTypes(FPParameterType.BoolParameter, typeof(FPParameterBool));
        }
        init = false;
    }
    private static bool init = true;
    private static Dictionary<string, Type> staticTypesList = new Dictionary<string, Type>();
    public static void RegisterTypes(string s, Type t)
    {
        staticTypesList.Add(s, t);
    }
    public static bool CheckType(string typeStr)
    {
        InitTypes();
        if (staticTypesList.ContainsKey(typeStr))
        {
            return true;
        }
        return false;
    }

    public static VolumeComponent GetVolumeComponent(GameObject obj, string componentName)
    {
        Volume ppm = obj.GetComponent<Volume>();
        if (ppm != null)
        {
            VolumeProfile vpf = ppm.profileRef;
            if (vpf != null)
            {
                foreach (var item in vpf.components)
                {
                    if (item != null && item.name == componentName)
                    {
                        return item;
                    }
                }
            }
        }
        return null;
    }
    private static Dictionary<string, IFPParameter> convertsTypesList = new Dictionary<string, IFPParameter>();
    private static void typesConvertGet()
    {
        InitTypes();
        foreach (KeyValuePair<string, Type> kv in staticTypesList)
        {
            IFPParameter para = Activator.CreateInstance(kv.Value) as IFPParameter;
            convertsTypesList[kv.Key] = para;
        }
    }
    private static void typesConvertClear()
    {
        convertsTypesList.Clear();
    }
    public static IFPParameter GetParameterConvert(string typeName, string paraName, string valueStr)
    {
        InitTypes();
        Type type = null;
        if (staticTypesList.TryGetValue(typeName, out type))
        {
            IFPParameter para = Activator.CreateInstance(type) as IFPParameter;
            para.ParaName = paraName;
            para.SetValue(valueStr);
            return para;
        }
        return null;
    }
    public static PVP GetVolumeParameter(VolumeComponent com)
    {
        if (com != null)
        {
            typesConvertGet();
            FieldInfo[] fields = com.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            PVP pvp = new PVP();
            pvp.PostName = com.name;
            List<VP> list = new List<VP>();
            foreach (FieldInfo fi in fields)
            {
                string typeName = fi.FieldType.ToString();
                if (FPVolumeHelp.CheckType(typeName))
                {
                    //Debug.LogWarning(typeName + "," + fi.Name);
                    VP vp = getVolumeParameterValue(com, fi, typeName);
                    list.Add(vp);
                }
            }
            pvp.Parameters = list;
            typesConvertClear();
            return pvp;
        }
        return null;
    }
    private static VP getVolumeParameterValue(VolumeComponent com, FieldInfo field, string typeName)
    {
        VP vp = new VP();
        object obj = field.GetValue(com);
        Type type = obj.GetType();
        //foreach(FieldInfo i in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        //{
        //    Debug.LogError(i.Name + "," + i.FieldType);
        //}
        FieldInfo overrideStateField = type.GetField(FPParameterType.OverrideStateName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        vp.OverrideState = (bool)overrideStateField.GetValue(obj);
        vp.ParaName = field.Name;
        vp.TypeName = typeName;
        FieldInfo valuefield = type.GetField(FPParameterType.ValueFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        vp.ValueString = convertsTypesList[typeName].Object2String(valuefield.GetValue(obj));
        return vp;
    }

    public static void SetVolumeParameter(VolumeComponent com, string para, bool enable, string value)
    {
        FieldInfo[] fields = com.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo operationField = null;
        foreach (FieldInfo fi in fields)
        {
            if (fi.Name == para)
            {
                operationField = fi;
            }
        }
        
        if (operationField != null)
        {
            string typeName = operationField.FieldType.ToString();
            IFPParameter convert = GetParameterConvert(typeName, para, value);

            object obj = operationField.GetValue(com);
            Type type = obj.GetType();
            FieldInfo overrideStateField = type.GetField(FPParameterType.OverrideStateName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            overrideStateField.SetValue(obj, enable);
            FieldInfo valuefield = type.GetField(FPParameterType.ValueFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            valuefield.SetValue(obj, convert.Value());
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

public sealed class FPParameterItem
{
    public FPParameterItem(FPDebugObjectItem _Target, bool _OverrideState, string _PostName, string _TypeName, string _ParaName, string _ValueString)
    {
        Target = _Target;
        OverrideState = overrideState = _OverrideState;
        PostName = _PostName;
        TypeName = _TypeName;
        ParaName = _ParaName;
        parameterHandle = FPVolumeHelp.GetParameterConvert(TypeName, ParaName, _ValueString);
    }

    public void DrawGUI()
    {
        EditorGUILayout.BeginHorizontal();
        OverrideState = EditorGUILayout.Toggle(OverrideState, GUILayout.Width(30));
        bool parameterChange = parameterHandle.OnGUI();
        EditorGUILayout.EndHorizontal();
        if (overrideState != OverrideState || parameterChange)
        {
            overrideState = OverrideState;
            OnValueChange(overrideState, parameterHandle.ValueString);
        }
    }

    public void OnValueChange(bool enable, string value)
    {
        Target.ParameterHandleAction(PostName, ParaName, enable, value);
    }

    public FieldInfo Field;
    public string TypeName, PostName, ParaName;
    public VolumeComponent Component;
    public FPDebugObjectItem Target;

    public bool OverrideState;
    private bool overrideState;
    private IFPParameter parameterHandle = null;
}

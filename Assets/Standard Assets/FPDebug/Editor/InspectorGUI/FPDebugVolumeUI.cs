using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;

public sealed class FPDebugVolumeUI : FPDebugUIBase
{
    private int tabIndex = 0;
    private string[] tabString = new string[] { "标准", "专业" };
    private List<FPParameterView> views = null;
    FPDebugObjectItem obj;
    public FPDebugVolumeUI(FPDebugObjectItem _obj)
    {
        obj = _obj;
        OnEnable();
    }
    public void OnDisable()
    {
        if(views != null)
        {
            views.Clear();
            views = null;
        }
    }

    public void OnEnable()
    {
        if (obj.VL != null && obj.VL.Count > 0)
        {
            views = new List<FPParameterView>();
            for (int i = 0; i < obj.VL.Count; i++)
            {
                VL vl = obj.VL[i];
                views.Add(new FPParameterView(obj, vl.Name, vl.Enable));
            }
        }
        tabIndex = 0;
    }

    public bool OnInspectorGUI()
    {
        if (views == null)
            return false;
        tabIndex = GUILayout.Toolbar(tabIndex, tabString);
        bool pro = tabIndex == 1;
        for (int i = 0; i < views.Count; i++)
        {
            FPParameterView rf = views[i];
            if (pro)
            {
                if (FPEditorGUIUtility.ToggleGroup(rf.Name, ref rf.Show, ref rf.Active))
                {
                    rf.ShowGUI(pro);
                }
            }
            else
            {
                bool noShow = false;
                FPEditorGUIUtility.ToggleGroup(rf.Name, ref noShow, ref rf.Active);
                rf.ShowGUI(pro);
            }
        }
        return true;
    }
}

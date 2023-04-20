using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class FPParameterView
{
    public FPParameterView(FPDebugObjectItem _Target, VolumeComponent com)
    {
        Target = _Target;
        Com = com;
        Active = com.active;
        Name = com.name;
    }
    public FPParameterView(FPDebugObjectItem _Target, string _Name, bool _Active)
    {
        Target = _Target;
        Active = _Active;
        Name = _Name;
    }

    private VolumeComponent Com;
    public string Name = null;
    public bool Show = false;
    public bool Active = false;
    public FPParameterItem[] Para = null;
    private bool load = true;
    private bool active = false;
    public FPDebugObjectItem Target;
    public void ShowGUI(bool pro)
    {
        if(pro)
        {
            if (Para == null)
            {
                if (load)
                {
                    load = false;
                    ClientMessage.GetPostPara(Target.RemoteID, Name, OnGetPostPara);
                }
                GUILayout.Label("正在加载中...");
            }
            else
            {
                for (int i = 0; i < Para.Length; i++)
                {
                    Para[i].DrawGUI();
                }
            }
        }

        if(active != Active)
        {
            active = Active;
            Target.PostHandleAction(Name, active);
        }
    }
    private void OnGetPostPara(PVP pvp)
    {
        List<FPParameterItem> list = new List<FPParameterItem>();
        foreach(VP vp in pvp.Parameters)
        {
            FPParameterItem fpp = new FPParameterItem(Target, vp.OverrideState, pvp.PostName, vp.TypeName, vp.ParaName, vp.ValueString);
            list.Add(fpp);
        }
        Para = list.ToArray();
        //Para = GetVolumeParameter(Target, Com, Com.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
    }
    //public static FPParameterItem[] GetVolumeParameter(FPDebugObjectItem target, VolumeComponent com, FieldInfo[] fields)
    //{
    //    List<FPParameterItem> list = new List<FPParameterItem>();
    //    foreach (FieldInfo v in fields)
    //    {
    //        string typeName = v.FieldType.ToString();
    //        string valueStr = null;
    //        if (FPVolumeParameterHelp.CheckType(typeName, ref valueStr))
    //        {
    //            Debug.LogWarning(typeName + "," + v.Name);
    //            FPParameterItem fpp = new FPParameterItem(target, com, v, typeName, valueStr);
    //            list.Add(fpp);
    //        }
    //    }
    //    return list.ToArray();
    //}
}

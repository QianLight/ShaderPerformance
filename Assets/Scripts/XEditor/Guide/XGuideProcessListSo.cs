#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using System;
using CFUtilPoolLib;
[Serializable]
public class XGuideProcessSO
{
    [SerializeField]
    public int step;

    [SerializeField]
    public string module = string.Empty;

    [SerializeField]
    public List<XTriDataSo> others = new List<XTriDataSo>();

    public void Sort()
    {
        others.Sort(XTriDataSo.Compare);
    }

    public void CopyTo(XGuideProcessSO so)
    {
        step = so.step;
        module = so.module;
        others.Clear();
        for (int i = 0; i < so.others.Count; i++)
        {
            XTriDataSo other = new XTriDataSo();
            other.func = so.others[i].func;
            other.func_key = so.others[i].func_key;
            other.func_value = so.others[i].func_value;
            others.Add(other);
        }
    }
    public bool EditorToData( ref XGuideProcess data)
    {
        data.step = step;
        data.process = new List<Tri>();
        for (int i = 0; i < others.Count; i++)
        {
            if (others[i].func == XGuideID.Guide_Cmd_None)
            {
                return false;
            }
            Tri tri = new Tri();
            others[i].Copy(ref tri);
            data.process.Add(tri);
        }
        return true;
    }
}

[Serializable]
public class XGuideProcessListSo:ScriptableObject
{
    [SerializeField]
    public List<XGuideProcessSO> processSo = new List<XGuideProcessSO>();




    public void EditorToData()
    {
        XGuideScript script = new XGuideScript();
        XGuideProcess proces = null;
        for(int i = 0;i < processSo.Count; i++)
        {
            proces = new XGuideProcess();

            if (processSo[i].EditorToData(ref proces))
            {
                script.table.Add(proces);
            }
        }
        DataIO.SerializeData<XGuideScript>(string.Format("Assets/BundleRes/Table/Guide/{0}.bytes", name), script);
    }
}

#endif
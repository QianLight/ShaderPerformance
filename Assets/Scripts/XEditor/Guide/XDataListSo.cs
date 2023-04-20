#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;
using CFUtilPoolLib;
[Serializable]
public class XDataListSo 
{
    [SerializeField]
    public List<XDataSO> dataSo = new List<XDataSO>();

    public void EditorToData()
    {
        XGuideList script = new XGuideList();
        XGuideData proces = null;
        for (int i = 0; i < dataSo.Count; i++)
        {
            proces = new XGuideData();
            if (dataSo[i].EditorToData(ref proces))
            {
                script.table.Add(proces);
            }
        }
        DataIO.SerializeData<XGuideList>(string.Format("Assets/BundleRes/Guide/{0}.bytes", "GuideTable"), script);
    }
}

[Serializable]
public class XDataSO
{
    [SerializeField]
    public int id;
    [SerializeField]
    public string module;
    //[SerializeField]
    //public bool use = true;
    [SerializeField]
    public string script = string.Empty;
    //[SerializeField]
    //public UnityEngine.Object scriptObject = null;
    [SerializeField]
    public List<XTriDataSo> others = new List<XTriDataSo>();

    public void Sort()
    {
        others.Sort(XTriDataSo.Compare);
    }

    public void CopyTo( XDataSO so)
    {
        id = so.id;
        module = so.module;
        script = so.script;
        others.Clear();
        for(int i = 0;i < so.others.Count; i++)
        {
            XTriDataSo other = new XTriDataSo();
            other.func = so.others[i].func;
            other.func_key = so.others[i].func_key;
            other.func_value = so.others[i].func_value;
            others.Add(other);
        }
    }

    public bool EditorToData(ref XGuideData data)
    {
        data.module = id;
        data.script = script;
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
        if (string.IsNullOrEmpty(script))
        {
            XDebug.singleton.AddErrorLog("not found script :" + module);
            return false;
        }
        return true;
    }
}


#endif
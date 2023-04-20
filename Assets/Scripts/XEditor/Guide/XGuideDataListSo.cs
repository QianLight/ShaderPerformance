#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using System;
using CFUtilPoolLib;

[Serializable]
public class XGuideDataSO 
{
    [SerializeField]
    public int id;
    [SerializeField]
    public string module;
    [SerializeField]
    public bool use = true;
    [SerializeField]
    public string script = string.Empty;
    [SerializeField]
    public UnityEngine.Object scriptObject = null;
    [SerializeField]
    public List<XTriDataSo> others = new List<XTriDataSo>();

    public void Sort()
    {
        others.Sort(XTriDataSo.Compare);
    }

    public bool EditorToData( ref XGuideData data)
    {
        data.module = id;
        if(scriptObject == null)
        {
            XDebug.singleton.AddErrorLog("not found script :" + module);
            return false;
        }
        data.script = scriptObject.name;
        data.process = new List<Tri>();
        
        for( int i =0; i < others.Count; i++)
        {
            
            if(others[i].func == XGuideID.Guide_Cmd_None)
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
public class XTriDataSo
{
    [SerializeField]
    public int func = 0;
    [SerializeField]
    public int func_key = 0;
    [SerializeField]
    public string func_value = string.Empty;

    public void Copy( ref Tri tri  )
    {
        tri.cmd = func;
        tri.id = func_key;
        tri.param = func_value;

    }

    public static int Compare( XTriDataSo f, XTriDataSo l)
    {
        return f.func - l.func;
    }
}

[Serializable]
public class XGuideDataListSo : ScriptableObject
{
    [SerializeField]
    public List<XGuideDataSO> dataSo = new List<XGuideDataSO>();

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
        DataIO.SerializeData<XGuideList>(string.Format("Assets/BundleRes/Table/Guide/{0}.bytes", name), script);
    }
}


#endif
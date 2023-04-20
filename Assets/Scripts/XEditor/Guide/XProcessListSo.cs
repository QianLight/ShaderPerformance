#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using System;
using CFUtilPoolLib;

[Serializable]
public class XProcessListSo
{
    [SerializeField]
    public List<XGuideProcessSO> processSo = new List<XGuideProcessSO>();

    public string name = string.Empty;

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
        DataIO.SerializeData<XGuideScript>(string.Format("Assets/BundleRes/Guide/{0}.bytes", name), script);
    }
}

#endif
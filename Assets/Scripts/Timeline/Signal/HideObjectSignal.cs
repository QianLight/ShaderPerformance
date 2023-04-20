using CFEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;
using CFEngine.Editor;
using System;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("隐藏物件")]
#endif
public class HideObjectSignal : DirectorSignalEmmiter
{

    [SerializeField]
    public List<HideObjectInfo> m_hideObjs;

    [HideInInspector]
    public bool m_executed = false;


    public void PlayAnimation()
    {
        if (m_hideObjs == null) return;

        m_executed = true;

        for (int i = 0; i < m_hideObjs.Count; ++i)
        {
            //if (EngineContext.IsRunning)
            //{
            //    SceneDynamicObjectSystem.PlayAnimation(m_hideObjs[i].m_name, m_hideObjs[i].m_visible);
            //}
            //else
            {
                //#if UNITY_EDITOR
                if (m_hideObjs[i].m_go != null)
                {
                    ActiveObject ao = m_hideObjs[i].m_go.GetComponent<ActiveObject>();
                    if (ao != null && ao.animationTargetGroup != null) ao.animationTargetGroup.gameObject.SetActive(m_hideObjs[i].m_visible);
                    else
                    {
                        m_hideObjs[i].m_go.SetActive(m_hideObjs[i].m_visible);
                    }
                }
                else
                {
                    string path = m_hideObjs[i].m_name;
                    GameObject go = GameObject.Find("SA_" + path);
                    if (go != null)
                    {
                        ActiveObject ao = go.GetComponent<ActiveObject>();
                        if (ao != null && ao.animationTargetGroup != null) ao.animationTargetGroup.gameObject.SetActive(m_hideObjs[i].m_visible);
                        else
                        {
                            go.SetActive(m_hideObjs[i].m_visible);
                        }
                    }
                    else
                    {
                        go = GameObject.Find(path);
                        if (go != null) go.SetActive(m_hideObjs[i].m_visible);
                    }
                }
                //#endif
            }
        }
    }
}


[Serializable]
public class HideObjectInfo
{
    public GameObject m_go;
    public string m_name;
    public bool m_visible;
}
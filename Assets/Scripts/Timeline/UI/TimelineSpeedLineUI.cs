using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;

public class TimelineSpeedLineUI : TimelineBaseUI<TimelineSpeedLineUI, SpeedLineSignal>
{
    private List<uint> timerList = null;
    private SpeedLineSignal newSign = null;
    private GameObject[] List = null;


    protected override string prefab { get { return "SpeedLineTimeline"; } }


    protected override void OnCreated()
    {
        bool first = false;
        if(List == null)
        {
            timerList = new List<uint>();
            List = new GameObject[go.transform.childCount];
            first = true;
        }
        for (int i = 0; i < List.Length; i++)
        {
            List[i] = go.transform.Find(string.Format("Type{0}", i + 1)).gameObject;
            if(first)
                List[i].SetActive(i + 1 == newSign.m_Type);
            else
            {
                if(i + 1 == newSign.m_Type)
                {
                    List[i].SetActive(true);
                }
            }
        }
    }

    private void Stop(object o)
    {
        if(List != null)
        {
            int id = (int)o;
            List[id - 1].SetActive(false);
        }
    }


    protected override void OnDestroy()
    {
        List = null;
        if(timerList != null && timerList.Count != 0)
        {
            for(int i = 0; i < timerList.Count; i++)
            {
                XTimerMgr.singleton.KillTimer(timerList[i]);
            }
        }
        timerList = null;
        base.OnDestroy();
    }
    

    public override void Show(SpeedLineSignal signal)
    {
        newSign = signal;
        base.Show(signal);
        timerList.Add(XTimerMgr.singleton.SetTimer(signal.m_EndTime, Stop, signal.m_Type));
    }
}
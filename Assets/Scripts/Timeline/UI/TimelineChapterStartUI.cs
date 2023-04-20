using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;
using UnityEngine.Playables;

public class TimelineChapterStartUI : TimelineBaseUI<TimelineChapterStartUI, UISign>
{
    protected override string prefab { get { return "StoryTitle"; } }

    private CFText m_title;
    private CFText m_content;
    private bool inited = false;
    public string m_param;
    public float m_duration = 3;
    private CFAnimation m_animation;

    public override void Show(UISign sign)
    {
        base.Show(sign);
        inited = false;
        InitUI();
    }

    private void InitUI()
    {
        if (inited) return;
        inited = true;

        PlayableDirector director = RTimeline.singleton.Director;
        //director.playableGraph.GetRootPlayable(0).SetSpeed(0);
        RTimeline.singleton.SetSpeed(0);
        m_title = go.transform.Find("ChapterT").GetComponent<CFText>();
        m_content = go.transform.Find("NameT").GetComponent<CFText>();
        m_animation = go.transform.GetComponent<CFAnimation>();
        //ChapterStartData data = (ChapterStartData)RTimeline.singleton.Arg; 
        string title = string.Empty;
        string content = string.Empty;
        if (m_param != null)
        {
            string[] strs = m_param.Split('|');
            if (strs.Length >= 2)
            {
                title = strs[0];
                content = strs[1];
            }
        }
        if (m_title != null)
        {
            m_title.text = title;
        }
        if (m_content != null)
        {
            m_content.text = content;
        }

        if (m_animation != null)
        {
            m_animation.RegisterAnimationFinish(OnPlayFinish);
            List<CFAnimUnit> animationList = m_animation.AnimList;
            for (int i = 0; i < animationList.Count; ++i)
            {
                if(animationList[i].Target != null && animationList[i].Target.name.Contains("StoryTitle"))
                {
                    animationList[i].m_Delay = m_duration;
                    break;
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        m_animation.RegisterAnimationFinish(null);
        base.OnDestroy();
    }

    private void OnPlayFinish(CFAnimation param)
    {
        go.SetActive(false);
        PlayableDirector director = RTimeline.singleton.Director;
        double speed = director.playableGraph.GetRootPlayable(0).GetSpeed();
        if (speed == 1.0) return;
        //director.playableGraph.GetRootPlayable(0).SetSpeed(1.0);
        RTimeline.singleton.SetSpeed(1.0f);
    }
}

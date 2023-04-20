using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.CFUI;

class TimelineBlackFrameUI : TimelineBaseUI<TimelineBlackFrameUI, UISignal>
{
    protected override string prefab { get { return "DialogBlack"; } }
    private CFAnimation m_animation;

    public override void Show(UISignal signal)
    {
        base.Show(signal);

        //Debug.LogError("show = " + signal.uiSign + "  " + signal.m_arg);
        m_animation = GetComponent<CFAnimation>("Black");
        int arg = 0;
        int.TryParse(signal.m_arg, out arg);
        if (signal.uiSign == UISign.BlackIn)
        {
            if(arg > 0)
            {
                m_animation.PlayAll();
            }
        }
        else
        {
            if(arg > 0)
            {
                m_animation.PlayReverse();
            }
        }
    }
}


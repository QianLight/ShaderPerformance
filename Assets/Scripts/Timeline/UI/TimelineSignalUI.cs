using CFUtilPoolLib;

public class TimelineSignalUI : TimelineBaseUI<TimelineSignalUI, UISign>
{
    public static void ShowSign(UISignal uiSignal)
    {
        UISign signal = uiSignal.uiSign;
        switch (signal)
        {
            case UISign.TEAM_OPEN1:
            case UISign.TEAM_OPEN2:
                TimelinePvpUI.singleton.Show(signal);
                break;
            //去掉显示抽卡通缉令效果   
            //case UISign.CARD_WANTED:
            //case UISign.CARD_PROPERTY:
            //case UISign.CARD_SSR:
            //    TimelineCardsUI.singleton.Show(signal);
            //    break;
            case UISign.DRAMA_MAP:
                TimelineDramaMapUI.singleton.PlayUIEffect(signal);
                break;
            case UISign.CONTROL_UI:
                RTimeline.singleton.ControlUICallback(uiSignal.m_arg);
                break;
            case UISign.CHAPTER_START:
                TimelineChapterStartUI.singleton.m_param = uiSignal.m_arg;
                TimelineChapterStartUI.singleton.m_duration = uiSignal.m_duration;
                TimelineChapterStartUI.singleton.Show(signal);
                break;
            case UISign.BOUNTY_SHOW:
                TimelineBountyUI.singleton.Show(signal);
                break;
            case UISign.BlackIn:
            case UISign.BlackOut:
                TimelineBlackFrameUI.singleton.Show(uiSignal);
                break;
            default:
                XDebug.singleton.AddWarningLog("unkown signal: " + signal);
                break;
        }
    }

}
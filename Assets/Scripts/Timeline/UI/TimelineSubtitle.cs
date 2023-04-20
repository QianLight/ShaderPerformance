using UnityEngine;
using UnityEngine.CFUI;

public class TimelineSubtitle : TimelineBaseUI<TimelineSubtitle, UISubtitleAsset>
{
    private CFText mText;
    private UISubtitleAsset m_currentPlayingAsset;
    public UISubtitleAsset CurrentPlayingAsset
    {
        get
        {
            return m_currentPlayingAsset;
        }
        set
        {
            m_currentPlayingAsset = value;
        }
    }

    //private Empty4Raycast cast;

    protected override string prefab { get { return "Subtitle"; } }

    private Canvas m_canvas;


    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (mText == null)
        {
            mText = CFText.Get<CFText>(tf, "T");
            mText.text = string.Empty;
            //cast = Empty4Raycast.Get<Empty4Raycast>(tf, "cast");
            //cast.RegisterPointerClickEvent(OnClick);
        }

        if (m_canvas == null && tf != null)
        {
            m_canvas = tf.GetComponent<Canvas>();
        }

        if (m_canvas != null)
        {
            m_canvas.sortingOrder = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.Subtitle_Layer).Param);
        }
    }

    private void OnClick(Empty4Raycast c)
    {
        Clear();
        RTimeline.singleton.Director?.Resume();
    }

    public override void Show(UISubtitleAsset asset)
    {
        base.Show(asset);
        if (mText != null)
        {
            mText.text = asset.subTitle ;
        }
    }
    

    public void Clear()
    {
        if (mText != null)
        {
            mText.text = string.Empty;
        }
    }

}

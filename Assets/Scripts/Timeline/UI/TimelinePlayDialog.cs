using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;
using System.Text.RegularExpressions;

public class TimelinePlayDialog : TimelineBaseUI<TimelinePlayDialog, UIPlayDialogAsset>
{
    private TimelineClip m_currentClip;
    private CFText mTitle;
    private CFText mContent;
    private Canvas m_canvas;
    private UIPlayDialogAsset dialogAsset;

    protected override string prefab { get { return "ShipDispatchPlayView"; } }

    protected override void OnCreated()
    {
        base.OnCreated();
        if (m_canvas != null)
        {
            m_canvas.sortingOrder = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.CutsceneDialog_Layer).Param);
        }
    }

    public override void Show(UIPlayDialogAsset asset)
    {
        base.Show(asset);
        OnInit(asset);
        ShowDialogArea(true);
    }

    public void OnInit(UIPlayDialogAsset asset)
    {
        if (go == null)
        {
            base.Show(asset);
        }

        Transform tf = go.transform;

        if (m_canvas == null && tf != null)
        {
            m_canvas = tf.GetComponent<Canvas>();
        }
        dialogAsset = asset;
        OnCreated();

        if (mTitle == null)
        {
            mTitle = CFText.Get<CFText>(tf, "panel_describe/panel_title/text_title");
        }

        if (mContent == null)
        {
            mContent = CFText.Get<CFText>(tf, "panel_describe/panel_content/text_content");
        }

        if (RTimeline.singleton.Director != null && RTimeline.singleton.Director.time <= 0)
        {
            ShowDialogArea(false);
        }
    }

    public void ShowDialogArea(bool bShow)
    {
        if (mTitle != null)
        {
            mTitle.SetText(dialogAsset.title);
        }
        if (mContent != null)
        {
            mContent.SetText(dialogAsset.content);
        }
        if (m_canvas != null)
        {
            m_canvas.gameObject.SetActive(bShow);
        }
    }
}

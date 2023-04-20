using CFEngine;
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;
using UnityEngine.Playables;

public class TimelineFadeUI : TimelineBaseUI<TimelineFadeUI, UIFadeAsset>
{
    private CFRawImage m_bg;
    private CFText text;

    private string m_defaultPath = "UIBackground/ui_Bg/tex_cutscene_fade";

    protected override string prefab { get { return "FadeUI"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (text == null)
        {
            text = CFText.Get<CFText>(tf, "T");
        }

        if (m_bg == null)
        {
            m_bg = CFRawImage.Get<CFRawImage>(tf, "CFImage");
        }
    }

    public override void Show(UIFadeAsset arg)
    {
        base.Show(arg);
        if (!string.IsNullOrEmpty(arg.content))
        {
            text.text = arg.content;
        }

        if (m_bg != null)
        {
            string path = string.Empty;
            if (string.IsNullOrEmpty(arg.m_bgTexturePath))
            {
                path = m_defaultPath;
            }
            else
            {
                path = arg.m_bgTexturePath;
            }
            m_bg.m_TexPath = string.Empty;
#if UNITY_EDITOR
            m_bg.SetTexturePath(path, SourceFlag.Sync);
#else
            m_bg.SetTexturePath(path, SourceFlag.Sync);
#endif
        }
    }
}


public class MaskFadeUI : TimelineBaseUI<MaskFadeUI, MaskFadeAsset>
{
    protected override string prefab { get { return "MaskFadeUI"; } }
}

public class TBCFadeUI : TimelineBaseUI<TBCFadeUI, TBCFadeAsset>
{
    protected override string prefab { get { return "TBCFadeUI"; } }

    private CFButton btn;

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (btn == null)
        {
            btn = CFButton.Get<CFButton>(tf, "root");
            btn.RegisterPointerClickEvent(OnBtnClick);
        }
    }

    private void OnBtnClick(UIBehaviour btn)
    {
        Clean();
        var director = RTimeline.singleton.Director;
        var graph = director.playableGraph;
        if (graph.IsValid())
        {
            RTimeline.singleton.SetSpeed(1);
            //graph.GetRootPlayable(0).SetSpeed(1);
        }

        if (AnimEnv.currentAE != null)
        {
            AnimEnv.currentAE.UnInit();
            AnimEnv.currentAE = null;
        }
    }

}
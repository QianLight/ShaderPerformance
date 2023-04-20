using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFUI;
using CFEngine;

public class TimelineUI : TimelineBaseUI<TimelineUI, UIPlayerAsset>
{
    private CFText mTitle, mWanted;
    private CFRawImage mSign, mSign2;
    private CFText mNick, mEnglish, mName;
    private CFImage bg_img;
    private AssetHandler ah;
    private UIPlayerAsset m_asset;
    private Canvas m_canvas;
    protected override string prefab { get { return "TimelineUI"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if(m_canvas == null)
        {
            Transform transition = tf.Find("Transition");
            if(transition != null)
            {
                m_canvas = transition.GetComponent<Canvas>();
            }
        }
        if (mTitle == null)
        {
            mTitle = CFText.Get<CFText>(tf, "Sign/Titleof/Title");
            mTitle.text = string.Empty;
            mWanted = CFText.Get<CFText>(tf, "Sign/Wantedof/Wanted");
            mWanted.text = string.Empty;
            mSign = CFRawImage.Get<CFRawImage>(tf, "Sign/Image");
            mSign2 = CFRawImage.Get<CFRawImage>(tf, "Sign/Image2");
        }
        if (mNick == null)
        {
            mNick = CFText.Get<CFText>(tf, "Intro/Text/Nick/Nickname");
            mNick.text = string.Empty;
            mEnglish = CFText.Get<CFText>(tf, "Intro/Text/Name/Englishname");
            mEnglish.text = string.Empty;
            mName = CFText.Get<CFText>(tf, "Intro/Text/Name");
            mName.text = string.Empty;
        }
        if (bg_img == null)
        {
            bg_img = go.transform.Find("BGImage").GetComponent<CFImage>();
            CleanBG();
        }
    }

    protected override void ControlLayer()
    {
        base.ControlLayer();
        if(m_asset != null && m_canvas != null)
        {
            int layer = 0;
            if(m_asset.m_layer == UIPlayerAsset.UIPlayerAssetLayer.BelowCutsceneDialog)
            {
                layer = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.TimelineUI_Layer1).Param);
            }
            else
            {
                layer = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.TimelineUI_Layer2).Param);
            }
            m_canvas.sortingOrder = layer;
        }
    }

    public override void Show(UIPlayerAsset upa)
    {
        m_asset = upa;
        base.Show(upa);
        ShowSign(upa.title, upa.wanted, upa.icon);
        ShowName(upa.mName, upa.mEnglish, upa.mNick);
    }

    public void CleanBG()
    {
        if (bg_img != null)
        {
            bg_img.SetAlpha(0);
        }
    }

    private void ShowSign(string title, string wanted, string sign)
    {
        if (mTitle != null)
        {
            mTitle.text = title;
        }
        if (mWanted)
        {
            mWanted.text = wanted;
        }
        bool hasSign = !string.IsNullOrEmpty(sign);
        if (mSign != null)
            mSign.gameObject.SetActive(hasSign);
        if (mSign2 != null)
            mSign2.gameObject.SetActive(hasSign);
        if (hasSign && mSign != null && mSign2 != null)
        {
            LoadMgr.singleton.Destroy(ref ah);
            EngineUtility.LoadTex(sign, ResObject.ResExt_PNG, ref ah);
            var tex = ah.obj as Texture;
            mSign.texture = tex;
            mSign2.texture = tex;
        }
    }

    private void ShowName(string name, string english, string nick)
    {
        if (mNick)
        {
            this.mNick.text = nick;
        }
        if (mName)
        {
            mName.text = name;
        }
        if (mEnglish)
        {
            mEnglish.text = english;
        }
    }

    public void FadeOut()
    {
        if (bg_img != null)
        {
            bg_img.SetAlpha(1);
        }
    }

    protected override void OnDestroy() 
    { 
        LoadMgr.singleton.Destroy(ref ah); 
    }
}
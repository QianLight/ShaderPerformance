using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFUI;
using CFEngine;

public class TimelineBoss : TimelineBaseUI<TimelineBoss, UIBossShowAsset>
{
    private CFText mName, mEnglish, mText;
    private CFText mTedian, mYingdui;
    private CFRawImage mSign;
    private AssetHandler ah;
    protected override string prefab { get { return "TimelineBossUI"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (mSign == null)
        {
            mSign = CFRawImage.Get<CFRawImage>(tf, "Intro/SignLight");
        }
        if (mName == null)
        {
            mEnglish = CFText.Get<CFText>(tf, "Intro/Englishname");
            mEnglish.text = string.Empty;
            mName = CFText.Get<CFText>(tf, "Intro/Nickname");
            mName.text = string.Empty;
            mText = CFText.Get<CFText>(tf, "Intro/Text");
            mText.text = string.Empty;
            mYingdui = CFText.Get<CFText>(tf, "Intro/Yingdui/Text");
            mYingdui.text = string.Empty;
            mTedian = CFText.Get<CFText>(tf, "Intro/Tedian/Text");
            mTedian.text = string.Empty;
        }
    }

    public override void Show(UIBossShowAsset a)
    {
        base.Show(a);
        Refresh(a.uiName, a.uiEnglish, a.uiText, a.uiYingdui, a.uiTedian, a.icon);
    }


    private void Refresh(string name, string english, string text, string yingdui, string tedian, string sign)
    {
        if (mName)
        {
            mName.text = name;
        }
        if (mEnglish)
        {
            mEnglish.text = english;
        }
        if (mText)
        {
            mText.text = text;
        }
        if (mYingdui)
        {
            mYingdui.text = yingdui;
        }
        if (mTedian)
        {
            mTedian.text = tedian;
        }
        if (mSign && !string.IsNullOrEmpty((sign)))
        {
            LoadMgr.singleton.Destroy(ref ah);
            EngineUtility.LoadTex(sign, ResObject.ResExt_PNG, ref ah);
            var tex = ah.obj as Texture;
            mSign.texture = tex;
            mSign.gameObject.SetActive(true);
        }
    }

    protected override void OnDestroy() { LoadMgr.singleton.Destroy(ref ah); }

}

using UnityEngine;
using UnityEngine.CFUI;

public class TimelineTip : TimelineBaseUI<TimelineTip, UITipAsset>
{
    private CFText mText;

    protected override string prefab { get { return "TimelineUI_result"; } }


    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (mText == null)
        {
            mText = CFText.Get<CFText>(tf, "Text");
            mText.text = string.Empty;
        }
    }

    public override void Show(UITipAsset asset)
    {
        base.Show(asset);
        if (mText != null)
        {
            mText.text = asset.uiText;
        }
    }

}

using CFEngine;
using CFUtilPoolLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;

public class TimelineBountyUI : TimelineBaseUI<TimelineBountyUI, UISign>
{
    private GameObject bountyGo;
    private AssetHandler bountyAsset;
    private System.Action callback;
    private Canvas mCan;

    public override void Show(UISign sign)
    {
        if (mCan == null)
            mCan = TimelineUIMgr.canvas;
        TimelineUIMgr.Regist(this);

        if (sign == UISign.BOUNTY_SHOW)
        {
            if (bountyGo == null)
            {
                bountyGo = EngineUtility.LoadPrefab("ui/opsystemprefab/task/taskbountyshow", ref bountyAsset,0,true,null,true);
                bountyGo.transform.SetParent(mCan.transform);
                bountyGo.transform.localPosition = Vector3.zero;
                bountyGo.transform.localScale = Vector3.one;
                RectTransform rt = bountyGo.transform as RectTransform;
                rt.anchoredPosition = Vector3.zero;
                rt.localEulerAngles = Vector3.zero;
                rt.localScale = Vector3.one;
                rt.offsetMax = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                bountyGo.SetActive(true);

                CFText playerName = bountyGo.transform.Find("bounty/TName").GetComponent<CFText>();
                CFText bounty = bountyGo.transform.Find("bounty/Bounty/T").GetComponent<CFText>();
                CFButton close = bountyGo.transform.Find("Btn_close").GetComponent<CFButton>();
                close.RegisterPointerClickEvent(OnCloseClick);
                CFRawImage cardImage = bountyGo.transform.Find("bounty/portrait/p").GetComponent<CFRawImage>();

                List<object> param = (List<object>)RTimeline.singleton.Arg;
                if (param != null)
                {
                    if (param.Count > 1) bounty.SetText((string)param[1]);
                    if (param.Count > 2) playerName.SetText((string)param[2]);

                    if (param.Count > 3) cardImage.SetTexturePath((string)param[3],SourceFlag.Sync);
                    if (param.Count > 4) callback = (System.Action)param[4];
                }
//                 if(Application.isPlaying)
//                     RTimeline.singleton.PauseTimeline(null, true);
            }
        }
    }

    private void OnCloseClick(UIBehaviour btn)
    {
        callback();
    }

    protected override void OnDestroy()
    {
        if (bountyGo != null)
            bountyGo.SetActive(false);

        LoadMgr.singleton.Destroy(ref bountyAsset);
        bountyGo = null;
        base.OnDestroy();
    }
}
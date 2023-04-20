using CFEngine;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.CFUI;

public class TimelineCardsUI : TimelineBaseUI<TimelineCardsUI, UISign>
{
    private GameObject mCardPos;
    private AssetHandler cardPosAsset;
    private GameObject mCard;
    private AssetHandler cardAsset;
    private GameObject mSSR;
    private AssetHandler ssrAsset;
    private Canvas mCan;
    private SFX mSfx;
    IXFmod UIFModBus;

    public override void Show(UISign sign)
    {
        if (mCan == null)
            mCan = TimelineUIMgr.canvas;
        TimelineUIMgr.Regist(this);
        if (EngineContext.IsRunning)
        {
            GameObject root = GameObject.Find(@"GamePoint");
            
            List<object> param = (List<object>)RTimeline.singleton.Arg;
            TimelineArgType type = (TimelineArgType)param[0];
            if (sign == UISign.CARD_WANTED)
            {
                CreateCardPos();
                if (type == TimelineArgType.CARD_WAVE)
                {
                    mCard = (GameObject)param[1];
                }
                if (mCard != null)
                {
                    Transform pos = mCardPos.transform.Find("Card/Pos");
                    mCard.transform.SetParent(pos);
                    RectTransform rt = mCard.transform as RectTransform;
                    rt.anchoredPosition = Vector3.zero;
                    rt.localEulerAngles = Vector3.zero;
                    rt.localScale = Vector3.one;
                    rt.offsetMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;
                    if (!mCard.activeInHierarchy)
                        mCard.SetActive(true);
                    int quality = System.Convert.ToInt32(param[3]);
                                        
                    if (quality > 2)
                    {
                        UIFModBus = param[4] as IXFmod;
                        UIFModBus.StartEvent("event:/UI/Card_stamp");
                    }
                    if(quality == 4)
                    {
                        IXFmod MusicFMod = param[5] as IXFmod;
                        Action<bool> callback = param[6] as Action<bool>;
                        if (callback != null)
                            callback(true);
                        MusicFMod.StartEvent("event:/Music/Card_SSR",AudioChannel.Motion);

                    }
                    string fx = quality <= 2 ? "UI_LotteryCard_ui_normal" : (quality == 3 ? "UI_LotteryCard_ui_sr" : "UI_LotteryCard_ui_ssr");
                    mSfx = SFXMgr.singleton.Create(fx);
                    if (mSfx != null)
                    {
                        Transform parent = pos.Find("fx");
                        mSfx.SetParent(parent, string.Empty, true);
                        mSfx.flag.SetFlag(SFX.Flag_Follow, true);
                        mSfx.RefreshUI(parent);
                        mSfx.SetUISortingLayer();
                        mSfx.Play();
                    }
                    SFXMgr.singleton.RefreshSFXLayer();
                }

            }
            //else if (sign == UISign.CARD_PROPERTY)
            //{
            //    if (type == TimelineArgType.CARD_WAVE && System.Convert.ToInt32(param[3]) == 4)
            //    {
            //        CreateAttr();
            //        BindAttr(System.Convert.ToInt32(param[4]));
            //        //UIFModBus.StartEvent("event:/UI/Card_SSR");
            //    }

            //}
            else if (sign == UISign.CARD_SSR)
            {
                CreateSSR();
                if (type == TimelineArgType.CARD_SSR)
                {
                    BindSSR((string)param[1], (string)param[2]);
                }

            }
        }
        else
        {
            if (sign == UISign.CARD_WANTED)
            {
                CreateCardPos();
                CreateCard();
                if (mCard != null)
                {
                    mCard.transform.SetParent(mCardPos.transform.Find("Card/Pos"));
                    RectTransform rt = mCard.transform as RectTransform;
                    rt.anchoredPosition = Vector3.zero;
                    rt.localScale = Vector3.one;
                    rt.offsetMax = Vector2.zero;
                    rt.offsetMin = Vector2.zero;
                    if (!mCard.activeInHierarchy)
                        mCard.SetActive(true);
                }
            }
            else if (sign == UISign.CARD_SSR)
            {
                CreateSSR();
            }
        }
    }

    private void CreateCardPos()
    {
        if (mCardPos == null)
        {
            mCardPos = EngineUtility.LoadPrefab("ui/opsystemprefab/lottery/timeline_card", ref cardPosAsset,0,true,null,true);
        }
            //mCardPos = XResourceLoaderMgr.singleton.CreateFromPrefab("UI/OPsystemprefab/lottery/Timeline_Card") as GameObject;
        mCardPos.transform.SetParent(mCan.transform);
        mCardPos.transform.localPosition = Vector3.zero;
        mCardPos.transform.localScale = Vector3.one;
        mCardPos.SetActive(true);
    }
    private void CreateSSR()
    {
        if (mSSR == null)
            mSSR = EngineUtility.LoadPrefab("ui/opsystemprefab/lottery/lotteryssrcutscene", ref ssrAsset,0,true,null,true); 
        mSSR.transform.SetParent(mCan.transform);
        RectTransform rt = mSSR.transform as RectTransform;
        rt.anchoredPosition = Vector3.zero;
        rt.localScale = Vector3.one;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        mSSR.SetActive(true);
    }
    private void CreateCard()
    {
        if (mCard == null)
            mCard = EngineUtility.LoadPrefab("ui/opsystemprefab/lottery/lotterycard_0", ref cardAsset,0,true,null,true); 
    }
    private void BindSSR(string name, string engName)
    {
        CFText nameTxt = mSSR.transform.Find("SSR/bg/Name").GetComponent<CFText>();
        nameTxt.SetText(name);
        CFText engNameTxt = mSSR.transform.Find("SSR/EnName").GetComponent<CFText>();
        engNameTxt.SetText(engName);
    }

    protected override void OnDestroy()
    {
        if (UIFModBus != null)
        {
            UIFModBus.Stop();
            UIFModBus = null;
        }
            
        SFXMgr.singleton.Destroy(ref mSfx, true);
        if (mCard != null)
        {
            if (EngineContext.IsRunning)
            {
                mCard.gameObject.SetActive(false);
                mCard.transform.SetParent(null);
                mCard = null;
            }
            else
            {
                mCard.SetActive(false);
                LoadMgr.singleton.Destroy(ref cardAsset);
                mCard = null;
            }
        }

        if (mCardPos != null)
        {
            mCardPos.SetActive(false);
            LoadMgr.singleton.Destroy(ref cardPosAsset);
            mCardPos = null;
        }

        if (mSSR != null)
        {
            mSSR.SetActive(false);
            LoadMgr.singleton.Destroy(ref ssrAsset);
            mSSR = null;
        }

        base.OnDestroy();
    }

}
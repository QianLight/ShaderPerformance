using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;

public class TimelinePictureUI : TimelineBaseUI<TimelinePictureUI, CustomPictureAsset>
{
    protected override string prefab { get { return "CustomPicture"; } }

    private Transform m_bigRoot;
    private Transform m_smallRoot;
    private CFRawImage m_bigPicBg;
    private CFRawImage m_bigPic1;
    private CFRawImage m_bigPic2;
    private CFImage m_smallPic;

    private EPictureEffect m_curPictureEffect = EPictureEffect.None;
    private AssetHandler m_effectHandler1;
    private AssetHandler m_effectHandler2;
    private string m_effectPath1 = "Timeline/Materials/OldFilm";
    private string m_effectPath2 = "Timeline/Materials/OldFilm2";

    private CFAnimation m_animation;
    private Animator m_animator;
    private CustomPictureAsset m_asset;
    private RectTransform m_root;

    private TimelineClip m_currentClip;
    private TimelineClip CurrentClip
    {
        get
        {
            return m_currentClip;
        }
        set
        {
            m_currentClip = value;
        }
    }

    public override void Show(CustomPictureAsset asset)
    {
        m_asset = asset;
        if (go == null)
        {
            base.Show(asset);
        }
        if (go != null) go.SetActive(true);

        Transform tf = go.transform;
        if (m_bigRoot == null)
        {
            m_bigRoot = tf.Find("bigBg");
        }

        m_root = tf as RectTransform;

        if (m_smallRoot == null)
        {
            m_smallRoot = tf.Find("smallBg");
        }

        if (m_animation == null)
        {
            m_animation = CFAnimation.Get<CFAnimation>(tf, "bigBg");
        }

        if (m_bigPicBg == null)
        {
            m_bigPicBg = CFRawImage.Get<CFRawImage>(tf, "bigBg");
        }

        if (m_animator == null && go != null)
        {
            m_animator = go.transform.GetComponent<Animator>();
        }

        if (m_bigPic1 == null)
        {
            m_bigPic1 = CFRawImage.Get<CFRawImage>(tf, "bigBg/pic1");
        }

        if (m_bigPic2 == null)
        {
            m_bigPic2 = CFRawImage.Get<CFRawImage>(tf, "bigBg/pic2");
        }


        if (m_smallPic == null)
        {
            m_smallPic = CFImage.Get<CFImage>(tf, "smallBg/pic");
        }

        m_bigRoot.gameObject.SetActive(asset.m_pictureType == EPictureType.BigPicture);
        m_smallRoot.gameObject.SetActive(asset.m_pictureType == EPictureType.SmallPicture);
        if (asset.m_customPictureInfos == null) return;
        int count = asset.m_customPictureInfos.Count;
        CustomPictureInfo info = null;
        if (asset.m_pictureType == EPictureType.BigPicture)
        {
            m_bigPicBg.enabled = asset.m_useBigBg;
            if (count == 0)
            {
                m_bigPic1.enabled = false;
                m_bigPic2.enabled = false;
            }
            else if (count == 1)
            {
                info = asset.m_customPictureInfos[0];
                SetPicInfo(m_bigPic1, info);
                SetPicEffect(m_bigPic1, info, m_effectPath1, m_effectHandler1);
                m_bigPic2.enabled = false;
            }
            else
            {
                info = asset.m_customPictureInfos[0];
                SetPicInfo(m_bigPic1, info);
                SetPicEffect(m_bigPic1, info, m_effectPath1, m_effectHandler1);

                info = asset.m_customPictureInfos[1];
                SetPicInfo(m_bigPic2, info);
                SetPicEffect(m_bigPic2, info, m_effectPath2, m_effectHandler2);
            }
        }
        else
        {
            if (count == 1)
            {
                info = asset.m_customPictureInfos[0];
                SetPicInfoImage(m_smallPic, info);
            }
        }
    }

    public void PlayShake()
    {
        //if (m_animation != null && m_asset.m_useShake)
        //{
        //    m_animation.PlayAll();
        //}

        if (m_animator != null && m_asset.m_useShake)
        {
            m_animator.enabled = true;
        }
    }

    public void StopShake()
    {
        //if (m_animation != null)
        //{
        //    m_animation.StopAll();
        //}

        if (m_animator != null)
        {
            m_animator.enabled = false;
        }

        if(go != null)
        {
            m_root.anchorMin = Vector2.zero;
            m_root.anchorMax = Vector2.one;
            m_root.anchoredPosition = Vector2.zero;
        }
    }

    private void SetPicEffect(CFRawImage pic, CustomPictureInfo info, string effectPath, AssetHandler assetHandler)
    {
        if (info.m_pictureEffect == EPictureEffect.OldFilm)
        {
            LoadMgr.GetAssetHandler(ref assetHandler, effectPath, ResObject.ResExt_Mat);
            LoadMgr.loadContext.flag = LoadMgr.LoadForceImmediate;
            LoadMgr.singleton.LoadAsset<Material>(assetHandler, ResObject.ResExt_Mat, true);
            pic.material = assetHandler.obj as Material;
            pic.material.SetTexture("_MainTex", pic.mainTexture);
        }
        else if (info.m_pictureEffect == EPictureEffect.None)
        {
            pic.material = pic.defaultMaterial;
        }
    }

    private void SetPicInfoImage(CFImage pic, CustomPictureInfo info)
    {
        int index = info.m_path.LastIndexOf("/");
        string imageName = info.m_path;
        if (index >= 0)
        {
            imageName = info.m_path.Substring(index + 1);
        }
        pic.Bind(imageName, "ui_item_story");
    }


    private CustomPictureInfo SetPicInfo(CFRawImage pic, CustomPictureInfo info)
    {
        pic.m_TexPath = string.Empty;
#if UNITY_EDITOR
        pic.SetTexturePath(info.m_path, SourceFlag.Native | SourceFlag.Sync );
#else
        pic.SetTexturePath(info.m_path, SourceFlag.Native | SourceFlag.Sync );
#endif
        // pic.SetNativeSize();
        SetPicPosition(pic, info.m_startPosition);
        SetPicScale(pic, info.m_startScale);
        SetAlpha(pic, info.m_startAlpha);
        pic.enabled = true;
        return info;
    }

    public void SetPicPosition(int index, Vector2 position)
    {
        if (index == 0)
        {
            SetPicPosition(m_bigPic1, position);
        }
        else if (index == 1)
        {
            SetPicPosition(m_bigPic2, position);
        }
        else
        {
            SetPicPositionImage(m_smallPic, position);
        }
    }

    public void SetPicPositionImage(CFImage pic, Vector2 position)
    {
        pic.rectTransform.anchoredPosition = position;
        pic.rectTransform.anchoredPosition = position;
    }

    public void SetPicPosition(CFRawImage pic, Vector2 position)
    {
        pic.rectTransform.anchoredPosition = position;
        pic.rectTransform.anchoredPosition = position;
    }

    public void SetPicScale(int index, float scale)
    {
        if (index == 0)
        {
            SetPicScale(m_bigPic1, scale);
        }
        else if (index == 1)
        {
            SetPicScale(m_bigPic2, scale);
        }
        else
        {
            SetPicScaleImage(m_smallPic, scale);
        }
    }

    public void SetPicScaleImage(CFImage pic, float scale)
    {
        pic.rectTransform.localScale = scale * Vector2.one;
        pic.rectTransform.localScale = scale * Vector2.one;
    }

    public void SetPicScale(CFRawImage pic, float scale)
    {
        pic.rectTransform.localScale = scale * Vector2.one;
        pic.rectTransform.localScale = scale * Vector2.one;
    }

    public void SetAlpha(int index, float alpha)
    {
        if (index == 0)
        {
            m_bigPic1.SetAlpha(alpha);
        }
        else if (index == 1)
        {
            m_bigPic2.SetAlpha(alpha);
        }
        else
        {
            m_smallPic.SetAlpha(alpha);
        }
    }

    public void SetAlpha(CFRawImage pic, float alpha)
    {
        pic.SetAlpha(alpha);
    }

    public void Hide()
    {
        if (go != null) go.SetActive(false);
    }

    public void SetCurrentPlayingClip(TimelineClip c)
    {
        m_currentClip = c;
    }

    public TimelineClip GetCurrentPlayingClip()
    {
        return m_currentClip;
    }

    protected override void OnDestroy()
    {
        if (m_effectHandler1 != null)
        {
            LoadMgr.singleton.Destroy(ref m_effectHandler1);
            m_effectHandler1 = null;
        }
        if (m_effectHandler2 != null)
        {
            LoadMgr.singleton.Destroy(ref m_effectHandler2);
            m_effectHandler2 = null;
        }
        base.OnDestroy();
    }
}

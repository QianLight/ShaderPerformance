using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFUI;


public class TimelineDramaMapUI : TimelineBaseUI<TimelineDramaMapUI, UIDramaMapAsset>
{
    private CFImage head, head1, head2, head3, head4, head5, head6;
    private CFRawImage tex, tex2;
    private SFX m_sfx;
    private RectTransform m_cloudRoot;
    private const int HEAD_COUNT = 7;   
    private SFX[] m_headSfx = new SFX[HEAD_COUNT];

    protected override string prefab { get { return "TimelineUI_Map"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (tex == null)
        {
            tex = CFRawImage.Get<CFRawImage>(tf, "root/BG/bg");
            tex2 = CFRawImage.Get<CFRawImage>(tf, "root/BG/bg2");
            head = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon0/Tpl/icon");
            head1 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon1/Tpl/icon");
            head2 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon2/Tpl/icon");
            head3 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon3/Tpl/icon");
            head4 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon4/Tpl/icon");
            head5 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon5/Tpl/icon");
            head6 = CFImage.Get<CFImage>(tf, "root/HeadPos/HeadIcon6/Tpl/icon");
            m_cloudRoot = go.transform.Find("sfx") as RectTransform;
        }
    }


    public override void Show(UIDramaMapAsset arg)
    {
        base.Show(arg);

        if (!string.IsNullOrEmpty(arg.sprite))
        {
            head.Bind(arg.sprite, arg.atlas);
        }
        if (!string.IsNullOrEmpty(arg.sp1))
        {
            head1.Bind(arg.sp1, arg.atlas1);
        }
        if (!string.IsNullOrEmpty(arg.sp2))
        {
            head2.Bind(arg.sp2, arg.atlas2);
        }
        if (!string.IsNullOrEmpty(arg.sp3))
        {
            head3.Bind(arg.sp3, arg.atlas3);
        }
        if (!string.IsNullOrEmpty(arg.sp4))
        {
            head4.Bind(arg.sp4, arg.atlas4);
        }
        if (!string.IsNullOrEmpty(arg.sp5))
        {
            head5.Bind(arg.sp5, arg.atlas5);
        }
        if (!string.IsNullOrEmpty(arg.sp6))
        {
            head6.Bind(arg.sp6, arg.atlas6);
        }
        if (!string.IsNullOrEmpty(arg.rawTex))
        {
            tex.SetTexturePath(arg.rawTex);
        }
        if (!string.IsNullOrEmpty(arg.rawTex2))
        {
            tex2.SetTexturePath(arg.rawTex2);
        }
        PlayHeadEffect(0);
    }

    private void PlayHeadEffect(int index)
    {
        m_headSfx[index] = SFXMgr.singleton.Create("UI_emoji_here");
        if (m_headSfx[index] != null)
        {
            m_headSfx[index].SetParent(head.transform.parent, string.Empty, true);
            m_headSfx[index].flag.SetFlag(SFX.Flag_Follow, true);
            m_headSfx[index].RefreshUI(m_cloudRoot);
            m_headSfx[index].SetUISortingLayer();
            m_headSfx[index].Play();
        }
        SFXMgr.singleton.RefreshSFXLayer();
    }

    public void PlayUIEffect(UISign sign)
    {
        m_sfx = SFXMgr.singleton.Create("UI_Transition_Cloud");
        if (m_sfx != null)
        {
            m_sfx.SetParent(m_cloudRoot, string.Empty, true);
            m_sfx.flag.SetFlag(SFX.Flag_Follow, true);
            m_sfx.RefreshUI(m_cloudRoot);
            m_sfx.SetUISortingLayer();
            m_sfx.Play();
        }
        SFXMgr.singleton.RefreshSFXLayer();
    }

    protected override void OnDestroy()
    {
        tex.SetTexturePath("");
        //if (tex.texture)
        //{
        //    XResourceLoaderMgr.singleton.UnSafeDestroyShareResource(arg.rawTex, ".png", tex.texture);
        //    tex.texture = null;
        //}
        tex2.SetTexturePath("");
        //if (tex2.texture)
        //{
        //    XResourceLoaderMgr.singleton.UnSafeDestroyShareResource(arg.rawTex, ".png", tex2.texture);
        //    tex2.texture = null;
        //}
        SFXMgr.singleton.Destroy(ref m_sfx, true);
        for (int i = 0; i < HEAD_COUNT; ++i)
        {
            if (m_headSfx[i] != null)
            {
                SFXMgr.singleton.Destroy(ref m_headSfx[i], true);
            }
        }
        base.OnDestroy();
    }
}

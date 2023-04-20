using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFUI;
using CFEngine;

public class TimelineEmotionUI : TimelineBaseUI<TimelineEmotionUI, UIEmotionAsset>
{
    private CFRawImage mBg, mIcon;
    private Transform mEmotion;
    private GameObject fx;
    private AssetHandler asset;
    private ParticleSystem[] ps;
    private string m_strIco;
    private AssetHandler ah;
    private IXFmodBus UIFModBus;
    protected override string prefab { get { return "CutsceneStrongUI"; } }

    protected override void OnCreated()
    {
        Transform tf = go.transform;
        if (mIcon == null)
        {
            mIcon = CFRawImage.Get<CFRawImage>(tf, "root/Mask/Head");
            mEmotion = tf.Find("root/Symbol");
        }
        if (EngineContext.IsRunning)
        {
            GameObject root = GameObject.Find(@"GamePoint");
            UIFModBus = root.GetComponent("XFmodBus") as IXFmodBus;
        }

    }

    public override void Show(UIEmotionAsset e)
    {
        base.Show(e);
        if (mIcon && !string.IsNullOrEmpty(e.head))
        {
            m_strIco = e.head;
            LoadMgr.singleton.Destroy(ref ah);
            EngineUtility.LoadTex(e.head, ResObject.ResExt_PNG, ref ah);
            var tex = ah.obj as Texture;
            mIcon.texture = tex;
        }
        if (mEmotion && !string.IsNullOrEmpty(e.emotion))
        {
            fx = EngineUtility.LoadPrefab(e.emotion, ref asset,0,true,null,true);
            fx.transform.parent = mEmotion;
            fx.transform.localPosition = e.pos;
            fx.transform.localScale = e.scale;
            fx.transform.localRotation = Quaternion.Euler(e.rot);
            ps = fx.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].Play();
            }
        }
        if (EngineContext.IsRunning)
        {            
            UIFModBus.StartEvent("event:/UI/Shoot_Frame/Expression");
        }

    }
    

    public void Update(float t)
    {
        if (ps != null)
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].Simulate(t);
            }
    }

    public override void Clean()
    {
        UnloadRefAssets();
        base.Clean();
    }

    protected override void OnDestroy()
    {
        UnloadRefAssets();
        base.OnDestroy();
    }

    private void UnloadRefAssets()
    {
        LoadMgr.singleton.Destroy(ref asset);
        if (fx)
        {
            fx = null;
            ps = null;
        }
        if (!string.IsNullOrEmpty(m_strIco))
        {
            //XResourceLoaderMgr.singleton.UnSafeDestroyShareResource(m_strIco, ".png", mIcon.texture);
            m_strIco = string.Empty;
        }
        LoadMgr.singleton.Destroy(ref ah);
    }
    
}

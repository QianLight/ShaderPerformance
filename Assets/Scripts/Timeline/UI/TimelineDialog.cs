using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;
using System.Text.RegularExpressions;
using CFClient.Utility;

public enum TextState
{
    Text_Showing,
    Text_Full_Show,
    Text_Clip_End,
    Text_None,
}
public class TimelineDialog : TimelineBaseUI<TimelineDialog, UIDialogAsset>
{
    private TimelineClip m_currentClip;
    private CFTextEffect mTextEffect;
    private CFText mSpeaker;
    private CFButton mTextBg;
    private CFRawImage m_head;
    private CFAnimation m_headAnimation;
    private CFAnimation m_boxAnimation;
    private RectTransform m_line;
    private GameObject m_namebg;

    private string m_lastSpeaker;
    private SFX m_sfx;

    private CFButton mAutoButton;
    private GameObject autoTrue;
    private GameObject autoFalse;
    private Transform m_black;
    private Transform m_dialog;
    private CFImage mNext;

    private bool bAuto = false;
    private TextState state;

    private float m_printSpeed = 0.05f;

    public bool IsAuto { get { return bAuto || !Application.isPlaying; } }
    public bool ShowAuto = true;

    public bool CanJumpToNext = false;
    public bool AlreadyHaveJump = false;
    private Canvas m_canvas;
    private Empty4Raycast cast;

    private Regex m_Reg = new Regex("(?<=<color).*?(?=</color>)", RegexOptions.None);
    private const int textCountLimit = 25;
    private const int strWidth = 30;
    private const string insertTex = "{*}";
    private MatchCollection m_Matches;

    private XTextGenerator tg;
    private TextGenerationSettings settings;

    protected override string prefab { get { return "CutsceneDialog"; } }

    protected override void OnCreated()
    {
        base.OnCreated();
        if (m_canvas != null)
        {
            m_canvas.sortingOrder = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.CutsceneDialog_Layer).Param);
        }
    }

    public override void Update(float time)
    {
        if (this.go == null) return;
        base.Update(time);
        CheckAdaptive();
    }

    public void OnInit(UIDialogAsset asset)
    {
        if (go == null)
        {
            base.Show(asset);
        }

        CalAdaptive();
        AdjustAdaptive(this.go);

        Transform tf = go.transform;
        m_black = tf.Find("DialogBlack");
        m_dialog = tf.Find("DialogCutscene");

        if (m_canvas == null && tf != null)
        {
            m_canvas = tf.GetComponent<Canvas>();
        }

        OnCreated();

        if (mTextEffect == null)
        {
            mTextEffect = CFTextEffect.Get<CFTextEffect>(tf, "DialogCutscene/line/Text");
            mTextEffect.SetText(string.Empty, 1.0f);
            mTextEffect.ShowAllText();
            mTextEffect.RegisterFinishCallback(() =>
            {
                ShowNext();
                CanJumpToNext = true;
                //×Ô¶¯»¯²âÊÔ
#if AUTOTESTING
        AutoTestingInterface.CallFunction(AutoTestingInterface.TimelineDialog);
#endif

            });

            tg = mTextEffect.CoreText.cachedTextGeneratorForLayout;
            settings = mTextEffect.CoreText.GetGenerationSettings(Vector2.zero);

        }

        if (cast == null)
        {
            cast = Empty4Raycast.Get<Empty4Raycast>(tf, "cast");
            cast.RegisterPointerClickEvent(OnTextBgClick);
        }

        if (mSpeaker == null)
        {
            mSpeaker = CFText.Get<CFText>(tf, "DialogCutscene/Name");
            mSpeaker.text = string.Empty;
        }

        if (m_head == null)
        {
            m_boxAnimation = CFAnimation.Get<CFAnimation>(tf, "DialogCutscene/Next/Next2");
        }

        if (mAutoButton == null)
        {
            mAutoButton = CFButton.Get<CFButton>(tf, "Auto");
            mAutoButton.RegisterPointerClickEvent(OnAutoButtonClick);
        }

        if (mNext == null)
        {
            mNext = CFImage.Get<CFImage>(tf, "DialogCutscene/Next");
        }

        if (autoTrue == null)
        {
            autoTrue = tf.Find("Auto/1").gameObject;
        }

        if (autoFalse == null)
        {
            autoFalse = tf.Find("Auto/0").gameObject;
        }

        if (m_line == null)
        {
            m_line = tf.Find("DialogCutscene/line").GetComponent<RectTransform>();
        }

        if (m_namebg == null)
        {
            m_namebg = tf.Find("DialogCutscene/Name_bg").gameObject;
        }

        if (RTimeline.singleton.Director != null && RTimeline.singleton.Director.time <= 0)
        {
            SetAuto(false);
            ShowDialogArea(false);
        }
    }

    private void PrintFinishCallback()
    {
        ShowNext();
    }

    public void SetCurrentPlayingClip(TimelineClip c)
    {
        m_currentClip = c;
    }

    public TimelineClip GetCurrentPlayingClip()
    {
        return m_currentClip;
    }

    public void ShowDialogArea(bool bShow)
    {
        if (m_dialog != null)
        {
            m_dialog.gameObject.SetActive(bShow);
        }

        if (bShow)
        {
            m_line?.gameObject.SetActive(true);
            m_namebg?.gameObject.SetActive(true);
        }

        if (!bShow) state = TextState.Text_None;
    }

    private void SetAuto(bool b)
    {
        bAuto = b;
        autoTrue.SetActive(b);
        autoFalse.SetActive(!b);
    }

    private void OnAutoButtonClick(UIBehaviour ub)
    {
        SetAuto(!bAuto);

        if (bAuto)
        {
            arg.GetNativeBehaviour().GoOn();
        }
    }

    private void GoNext()
    {
        if (!bAuto && CanJumpToNext)
        {
            arg.GetNativeBehaviour().GoOn();
        }
    }

    public void SetState(TextState s)
    {
        state = s;
    }

    private void OnTextBgClick(UIBehaviour ub)
    {
        state = TextState.Text_Clip_End;
        GoNext();
    }

    public void HideNext()
    {
        if (mNext != null)
        {
            mNext.gameObject.SetActive(false);
        }
    }

    public void ShowNext()
    {
        if (mNext != null)
        {
            mNext.gameObject.SetActive(true);
        }
    }

    public bool GetNextBtnActive()
    {
        return mNext.gameObject.activeInHierarchy;
    }

    public override void Show(UIDialogAsset asset)
    {
        if (go == null)
        {
            OnInit(asset);
        }

        ControlAuto(asset);
        ControlSkip(asset);

        if (asset.m_isEmpty)
        {
            ShowDialogArea(false);
            return;
        }

        ShowDialogArea(true);

        if (!EngineContext.IsRunning)
        {
            if (TimelineGlobalConfig.Instance.m_keyValues == null)
            {
                TimelineGlobalConfig.Instance.ReadConfig();
            }
            m_printSpeed = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["printspeed"]);
        }
        else
        {
            if (RTimeline.singleton.m_timelineConfig != null)
            {
                m_printSpeed = RTimeline.singleton.m_timelineConfig.m_printSpeed;// CFClient.XGlobalConfig.singleton.printspeed;
            }
            else
            {
                m_printSpeed = 0.02f;// CFClient.XGlobalConfig.singleton.printspeed;
            }
        }

        if (mTextEffect != null)
        {
            string content = asset.content != null ? asset.content : string.Empty;

            m_Matches = m_Reg.Matches(content);

            int matchCount = 0;
            int count = 0;
            int wordCont = 0;
            int insertCount = 0;
            string changeStr = content;
            bool maxLine = false;

            for (int i = 0; i < content.Length; ++i)
            {
                if (content[i] == '<' && m_Matches != null && matchCount < m_Matches.Count)
                {
                    string tmpStr = m_Matches[matchCount].Value.Substring(9);
                    count += tmpStr.Length;
                    wordCont += tmpStr.Length;

                    string color = content.Substring(i, 15);
                    string colorEnd = "</color>";
                    if (count >= textCountLimit)
                    {
                        int abs = tmpStr.Length - (count - textCountLimit);
                        changeStr = changeStr.Insert(i + 14 + (count - textCountLimit) + insertCount * insertTex.Length, colorEnd + insertTex + color);
                        count = abs;
                        ++insertCount;
                        maxLine = true;
                        i += color.Length + colorEnd.Length;
                    }
                    i += m_Matches[matchCount].Length + 6 + 8 - 1;
                    matchCount++;
                }
                else
                {
                    wordCont++;
                    count++;
                    if (count >= textCountLimit)
                    {
                        changeStr = changeStr.Insert(i + insertCount * insertTex.Length, insertTex);
                        count = 0;
                        ++insertCount;
                        maxLine = true;
                    }
                }
            }
            changeStr = changeStr.Replace(insertTex, "\n");
            content = changeStr;


            if (Application.isPlaying)
            {
                mTextEffect.SetText(content, m_printSpeed);
                state = TextState.Text_Showing;
            }
            else
            {
                mTextEffect.SetText(content, m_printSpeed);
                mTextEffect.ShowAllText();
                state = TextState.Text_Full_Show;
            }

            Canvas.ForceUpdateCanvases();
            float width = (tg.GetPreferredWidth(content, settings) / mTextEffect.CoreText.pixelsPerUnit);
            m_line.sizeDelta = new UnityEngine.Vector2(width + 50, 24);
            mTextEffect.CoreText.rectTransform.sizeDelta = new UnityEngine.Vector2(width, 60);

        }

        if (mSpeaker != null)
        {
            mSpeaker.text = asset.speaker;
        }

        if (m_head != null)
        {
            if (!string.IsNullOrEmpty(asset.m_head))
            {
                m_head.m_TexPath = string.Empty;
#if UNITY_EDITOR
                m_head.m_TexPath = "UIBackground/ui_cutscene_head/" + asset.m_head;
                m_head.SetTexturePath("UIBackground/ui_cutscene_head/" + asset.m_head, SourceFlag.Native | SourceFlag.Sync);
#else
                m_head.SetTexturePath("UIBackground/ui_cutscene_head/" + asset.m_head,SourceFlag.Native );
#endif
            }
            // m_head.SetNativeSize();

            if (!string.IsNullOrEmpty(asset.m_head) && !asset.m_head.Equals(m_lastSpeaker))
            {
                m_headAnimation.PlayAll();
            }
            m_lastSpeaker = asset.m_head;
            if (string.IsNullOrEmpty(asset.m_face))
            {
                //PlayHeadEffect("UI_emoji_happy_Loop_d", m_head.transform);
            }
        }

        m_boxAnimation?.PlayAll();
    }

    public void ControlAuto(UIDialogAsset asset)
    {
        if (mAutoButton != null)
        {
            bool showAutoButton = RTimeline.singleton.Data.showAutoButton;
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                var dir = RTimeline.singleton.Director;
                if (dir)
                {
                    var data = dir.GetComponent<OrignalTimelineData>();
                    if (data) showAutoButton = data.showAutoButton;
                }
            }
#endif
            if (showAutoButton || asset.m_autoEnable)
            {
                mAutoButton.gameObject.SetActive(true);
            }
            else
            {
                mAutoButton.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            SetAuto(bAuto);
        }
#endif
    }

    public void ControlSkip(UIDialogAsset asset)
    {
        RTimeline.singleton.ControlSkipCallback(asset.m_skipEnable);
    }

    public void HideDialogButBg()
    {
        if (mTextEffect != null)
            mTextEffect.CoreText.text = string.Empty;

        mSpeaker.SetText(string.Empty);
        m_line?.gameObject.SetActive(false);
        m_namebg?.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        m_currentClip = null;
        autoTrue = null;
        autoFalse = null;
        m_canvas = null;
        mTextEffect = null;
        cast = null;
        mSpeaker = null;
        m_head = null;
        mAutoButton = null;
        mNext = null;
        m_line = null;
        m_namebg = null;
        CanJumpToNext = false;
        AlreadyHaveJump = false;
        bAuto = false;

        base.OnDestroy();
    }

    private void PlayHeadEffect(string effectName, Transform parent)
    {
        if (m_sfx == null)
        {
            m_sfx = SFXMgr.singleton.Create(effectName);
        }
        if (m_sfx != null)
        {
            m_sfx.SetParent(parent, string.Empty, true);
            m_sfx.flag.SetFlag(SFX.Flag_Follow, true);
            m_sfx.RefreshUI(parent);
            m_sfx.SetUISortingLayer();
            m_sfx.Play();
        }
        SFXMgr.singleton.RefreshSFXLayer();
    }
}

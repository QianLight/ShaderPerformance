using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFUtilPoolLib;
using CFEngine;
using System;
using System.Collections.Generic;
using LipSync;
using FMODUnity;
using UnityEditor;
using CFClient;

/// <summary>
/// 开三个新的频道Music/Vocal/SFX
/// Music，不打断，用于控制bgm的逻辑（特殊情况跳过要执行最后一个块的逻辑）
/// Vocal，打断，用于口型
/// SFX，不打断，用于播放特效声音
/// </summary>

public class FmodPlayableBehaviour : DirectBaseBehaviour
{
    private XRuntimeFmod m_fmod;
    private PlayableDirector m_director;
    private XRuntimeFmod m_bgmFmod;

    #region Facial

    private FacialExpressionCurve curve;
    public bool isPlaying = false;
    private int fmodRate = 48000;
    private FMOD.ChannelGroup master;
    private FMOD.DSP mixerHead;
    private const int MAX_BLEND_VALUE_COUNT = 6;
    private const float rampRate = 0.2f;

    [HideInInspector]
    private ERecognizerLanguage m_language;
    private LipSyncRuntimeRecognizer m_runtimeRecognizer;

    private string recognizeResult;
    private string[] m_currentVowels;
    private Dictionary<string, int> vowelToIndexDict = new Dictionary<string, int>();
    private float[] targetBlendValues = new float[MAX_BLEND_VALUE_COUNT];
    private float[] currentBlendValues = new float[MAX_BLEND_VALUE_COUNT];
    private int[] propertyIndexs = new int[MAX_BLEND_VALUE_COUNT];

    #region 录制
    private AnimationClip recordClip;
    private AnimationCurve curveA;
    private AnimationCurve curveE;
    private AnimationCurve curveI;
    private AnimationCurve curveO;
    private AnimationCurve curveU;
    #endregion

    #endregion

    private FmodPlayableAsset curveAsset
    {
        get
        {
            if (asset != null)
            {
                var fmodAsset = asset as FmodPlayableAsset;
                if (!EngineContext.IsRunning)
                {
                    if (TimelineGlobalConfig.Instance.m_keyValues == null)
                    {
                        TimelineGlobalConfig.Instance.ReadConfig();
                    }
                    if (fmodAsset.windowSize == 0)
                        fmodAsset.windowSize = int.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncWindowSize"]);
                    if (fmodAsset.amplitudeThreshold == 0)
                        fmodAsset.amplitudeThreshold = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncThreshold"]);
                    if (fmodAsset.facialSpeed == 0)
                        fmodAsset.facialSpeed = float.Parse(TimelineGlobalConfig.Instance.m_keyValues["LipsyncMoveTowardsSpeed"]);
                }
                else
                {
                    if (fmodAsset.windowSize == 0)
                        fmodAsset.windowSize = RTimeline.singleton.m_timelineConfig.m_lipWindowSize;
                    if (fmodAsset.amplitudeThreshold == 0)
                        fmodAsset.amplitudeThreshold = RTimeline.singleton.m_timelineConfig.m_lipAmplitudeThreshold;
                    if (fmodAsset.facialSpeed == 0)
                        fmodAsset.facialSpeed = RTimeline.singleton.m_timelineConfig.m_lipFacialSpeed;
                }
                return fmodAsset;
            }
            return null;
        }
    }

    private bool isPlayState
    {
        get { return RTimeline.singleton.Director?.state == PlayState.Playing; }
    }

    public override void Reset()
    {
        base.Reset();

        curve.A = 0;
        curve.E = 0;
        curve.I = 0;
        curve.O = 0;
        curve.U = 0;

        //_runtimeFmod = null;
#if UNITY_EDITOR
        //m_fmod = null;
#endif
    }

    public override void OnGraphStart(Playable playable)
    {
        if (m_fmod == null)
        {
            m_director = (playable.GetGraph().GetResolver() as PlayableDirector);
            m_fmod = RTimeline.singleton.GetFmod(m_director);
        }
        //UnityEditor.Timeline.TimelineStateListener.Instance.RegisterFmodBehaviour(this);
    }

    public override void OnGraphStop(Playable playable)
    {
        if (!Application.isPlaying)
        {
            FmodPlayableTrack track = curveAsset.m_trackAsset as FmodPlayableTrack;
            if (track != null && m_fmod != null && m_fmod.IsPlaying(track.m_audioChannel))
            {
                if (m_director != null && (m_director.time < curveAsset.m_timelineClip.start || m_director.time > curveAsset.m_timelineClip.end))
                {
                    return;
                }
                m_fmod.SetPause(track.m_audioChannel, true);
                curveAsset.m_isPause = true;
            }
        }

        if (RTimeline.singleton.Evaluting) return;
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            FmodPlayableUtils.Destroy();
        }
        else
#endif
        {
            if (curveAsset != null)
            {
                curveAsset.isExecuted = false;
            }
        }
    }

    /// <summary>
    /// 如果音频放在第0帧，则会出现执行两次的情况，后面优化一下，目前解决方法是，音频至少放在第1帧
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (m_director == null)
        {
            m_director = (playable.GetGraph().GetResolver() as PlayableDirector);
        }


        if (RTimeline.singleton.Evaluting) return;
        if (isPlayState)
        {
            if (m_fmod != null && curveAsset != null /*&& !fpa.isExecuted */&& !string.IsNullOrEmpty(curveAsset.clip))
            {
                curveAsset.isExecuted = true;
                AudioChannel channel = AudioChannel.Action;
                FmodPlayableTrack track = curveAsset.m_trackAsset as FmodPlayableTrack;
                if (track != null)
                {
                    channel = track.m_audioChannel;
                }

                if (!Application.isPlaying)
                {
                    if (curveAsset.m_isPause)
                    {
                        m_fmod.SetPause(track.m_audioChannel, false);
                        curveAsset.m_isPause = false;
                    }
                    else
                    {
                        m_fmod.StartEvent(curveAsset.clip, channel, true);
                    }
                }
                else
                {
                    bool stopPre = false;
                    if (channel == AudioChannel.Action || channel == AudioChannel.Vocal) stopPre = true; //action频道是老的口型频道，vocal频道是新加的，都是停止方式

                    if (channel == AudioChannel.SFX) //音效频道，由于不是stopPre，需加入到播放列表中去，timeline结束之后统一停止
                    {
                        RTimeline.singleton.m_playedEvents.Add(curveAsset.clip);
                    }

                    if (channel == AudioChannel.LogicMusic)
                    {
                        if(m_bgmFmod == null)
                        {
                            m_bgmFmod = XAudioMgr.singleton.m_sceneBgmFmod as XRuntimeFmod;
                        }
                        if(m_bgmFmod != null)
                        {
                            bool isValid = m_bgmFmod.IsValid(AudioChannel.Music);

                            m_bgmFmod.StartEvent(curveAsset.clip, channel, stopPre);  //Action和Vocal频道是口型频道，使用打断方式
                        }
                        else
                        {
                            m_fmod.StartEvent(curveAsset.clip, channel, stopPre); //编辑器下运行时
                        }
                        XDebug.singleton.AddGreenLog("play logicMusic event=" + curveAsset.clip);
                        if (curveAsset == RTimeline.singleton.m_lastMusicEvent)
                        {
                            RTimeline.singleton.m_lastMusicEventExecuted = true;
                        }
                    }
                    else
                    {
                        m_fmod.StartEvent(curveAsset.clip, channel, stopPre);
                    }
                }
                AddFacialToDic(channel);
                InitializeRecognizer();
#if UNITY_EDITOR
                if (this.curveAsset.m_isRecord && recordClip == null)
                {
                    recordClip = new AnimationClip();
                    recordClip.name = "RecordClip";
                    curveA = new AnimationCurve();
                    curveE = new AnimationCurve();
                    curveI = new AnimationCurve();
                    curveO = new AnimationCurve();
                    curveU = new AnimationCurve();
                }
#endif
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
#if UNITY_EDITOR
        if (curveAsset.m_isRecord && recordClip != null)
        {
            recordClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), "A", curveA);
            recordClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), "E", curveE);
            recordClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), "I", curveI);
            recordClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), "O", curveO);
            recordClip.SetCurve(string.Empty, typeof(FacialExpressionCurve), "U", curveU);

            AssetDatabase.CreateAsset(recordClip, "Assets/BundleRes/Animation/FacialMouth/" + recordClip.name + ".anim");
        }
#endif

    }

    public void AddFacialToDic(AudioChannel channel)
    {
        if (m_fmod == null) return;
        if (channel == AudioChannel.Action || channel == AudioChannel.Vocal)
        {
            Guid guid = m_fmod.GetEventGUID(channel);
            if (guid == Guid.Empty) return;
            this.isPlaying = true;
            RTimeline.singleton.AddFacialToDic(guid, this);
        }
    }

    public void UpdateFrame()
    {
        if (curveAsset == null)
            return;

        if (curveAsset.facialCurve != null && isPlayState)
        {
            curve = curveAsset.facialCurve;
            if (curve != null && m_runtimeRecognizer != null)
            {
                recognizeResult = m_runtimeRecognizer.RecognizeByAudioSource(XFmodBus.m_fftDsp, fmodRate);
                RecordingFmod(recognizeResult);
                //Debug.Log(recognizeResult);
            }
        }
        else if (curveAsset.facialCurve == null && !string.IsNullOrEmpty(curveAsset.curvePath))
        {
            GameObject tmp = GameObject.Find(curveAsset.curvePath);
            GameObject tmp1 = GameObject.Find(curveAsset.curvePath1);

            if (tmp != null)
                curveAsset.facialCurve = GameObject.Find(curveAsset.curvePath).GetComponent<FacialExpressionCurve>();
            else if (tmp1)
            {
                curveAsset.facialCurve = GameObject.Find(curveAsset.curvePath1).GetComponent<FacialExpressionCurve>();
            }
            else
            {
                curveAsset.curvePath = String.Empty;
                curveAsset.curvePath1 = String.Empty;
                curveAsset.facialCurve = null;
            }
        }
    }


    public void InitializeRecognizer()
    {
        m_language = ERecognizerLanguage.Japanese;

        switch (m_language)
        {
            case ERecognizerLanguage.Japanese:
                m_currentVowels = LipSync.LipSync.vowelsJP;
                break;
            case ERecognizerLanguage.Chinese:
                m_currentVowels = LipSync.LipSync.vowelsCN;
                break;
        }
        for (int i = 0; i < m_currentVowels.Length; ++i)
        {
            vowelToIndexDict[m_currentVowels[i]] = i;
            currentBlendValues[i] = 0;
        }

        if (curveAsset != null)
        {
            if (curveAsset.windowSize == 0)
            {
                Debug.LogError("timeline " + RTimeline.singleton.m_timelineName + "==" + curveAsset.clip);
                curveAsset.windowSize = 256;
            }
            m_runtimeRecognizer = new LipSyncRuntimeRecognizer(m_language, curveAsset.windowSize, curveAsset.amplitudeThreshold);
        }

        InitDsp();
    }

    public void Release()
    {
        //m_FFTDsp.release();
        //m_runtimeRecognizer = null;
    }

    void InitDsp()
    {
        if (RuntimeManager.Instance != null)
        {
            //RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out m_FFTDsp);
            XFmodBus.m_fftDsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)FMOD.DSP_FFT_WINDOW.HANNING);
            XFmodBus.m_fftDsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, curveAsset.windowSize);

            //RuntimeManager.CoreSystem.getMasterChannelGroup(out master);
            //var m_Result = master.addDSP((int)FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, RTimeline.singleton.FFTDsp);
            //var m_Result = master.addDSP(-2, RTimeline.singleton.FFTDsp);
            //mixerHead.setMeteringEnabled(true, true);

            //int num = -1;
            //master.getNumDSPs(out num);
            //FMOD.DSP dsp1;
            //var res1 = master.getDSP(-1, out dsp1);
            //int hashCode1 = dsp1.GetHashCode();
            //Debug.LogError("dsp1 hashcode=" + res1 + "  " + hashCode1);

            //FMOD.DSP dsp2;
            //var res2 = master.getDSP(-2, out dsp2);
            //int hashCode2 = dsp2.GetHashCode();
            //Debug.LogError("dsp2 hashcode=" + res2 + "  " + hashCode2);

            //FMOD.DSP dsp3;
            //var res3 = master.getDSP(-3, out dsp3);
            //int hashCode3 = dsp3.GetHashCode();
            //Debug.LogError("dsp3 hashcode=" + res3 + "  " + hashCode3);

            FMOD.SPEAKERMODE mode;
            int raw;
            RuntimeManager.CoreSystem.getSoftwareFormat(out fmodRate, out mode, out raw);
        }
    }

    public override bool Equals(object obj)
    {
        if (this.curveAsset == null)
            return true;

        FmodPlayableBehaviour other = obj as FmodPlayableBehaviour;
        if (this.curveAsset.facialCurve == null || other.curveAsset.facialCurve == null)
            return true;

        if (this.curveAsset.facialCurve.transform == other.curveAsset.facialCurve.transform)
            return true;
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public void RecordingFmod(string word)
    {
        curve = curveAsset.facialCurve;
        if (curve == null)
            return;

        //if (!RTimeline.singleton.isPlaying && Time.frameCount % 1 != 0)
        //    return;
        //else if (Application.isPlaying && Time.frameCount % 1 != 0)
        //    return;

        if (!RTimeline.singleton.isPlaying && Time.frameCount % 2 != 0)
            return;

        for (int i = 0; i < targetBlendValues.Length; ++i)
        {
            targetBlendValues[i] = 0.0f;
        }

        if (word != null)
        {
            targetBlendValues[vowelToIndexDict[word]] = 1.0f;
        }

        for (int k = 0; k < currentBlendValues.Length; ++k)
        {
            if (propertyIndexs[k] != -1)
            {
                float changeSpeed = Time.deltaTime * curveAsset.facialSpeed;

                //if (RTimeline.singleton.isPlaying)
                //{
                //    targetBlendValues[k] *= 1.5f;
                //}
                if (targetBlendValues[k] != 0)
                {
                    float absVal = Mathf.Abs(currentBlendValues[k] - targetBlendValues[k]);
                    if (absVal < 0.2f)
                        changeSpeed = 0;
                    else
                    {
                        changeSpeed += 0.3f;
                    }
                }

                currentBlendValues[k] = Mathf.Lerp(currentBlendValues[k], targetBlendValues[k], changeSpeed);

                if (vowelToIndexDict.ContainsValue(k))
                {
                    float setValue = currentBlendValues[k];

                    if (setValue < 0.1f)
                        setValue = 0;

                    if (setValue > 0)
                    {
                        float rate = 1 - setValue;
                        float tmp = 1;

                        if (setValue > 0.8)
                        {
                            rate -= rampRate;
                            tmp = Mathf.Pow(0.6f, rate);
                        }
                        else if (setValue < 0.2)
                        {
                            rate += rampRate;
                            tmp = Mathf.Pow(0.6f, rate);
                        }
                        setValue = tmp * setValue;
                    }

                    switch (k)
                    {
                        case 0:
                            curve.A = setValue;
                            if (curveAsset.m_isRecord && curveA != null)
                            {
                                curveA.AddKey((float)RTimeline.singleton.Director.time, setValue);
                            }
                            break;
                        case 1:
                            curve.I = setValue;
                            if (curveAsset.m_isRecord && curveI != null)
                            {
                                curveI.AddKey((float)RTimeline.singleton.Director.time, setValue);
                            }
                            break;
                        case 2:
                            curve.U = setValue;
                            if (curveAsset.m_isRecord && curveA != null)
                            {
                                curveU.AddKey((float)RTimeline.singleton.Director.time, setValue);
                            }
                            break;
                        case 3:
                            curve.E = setValue;
                            if (curveAsset.m_isRecord && curveE != null)
                            {
                                curveE.AddKey((float)RTimeline.singleton.Director.time, setValue);
                            }
                            break;
                        case 4:
                            curve.O = setValue;
                            if (curveAsset.m_isRecord && curveO != null)
                            {
                                curveO.AddKey((float)RTimeline.singleton.Director.time, setValue);
                            }
                            break;
                    }
                }
            }
        }
    }
}
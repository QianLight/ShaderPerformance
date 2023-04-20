using UnityEngine;
using CFUtilPoolLib;
using FMODUnity;
using System.Collections.Generic;

/// <summary>
/// fmodbus，全局应该只有一个，如果有多个脚本的情况下，通过m_uniqueID确保只有m_ID=0号的脚本执行添加和释放DSP
/// </summary>
public class XFmodBus : MonoBehaviour, IXFmodBus
{
    FMOD.Studio.VCA mainVCA;
    FMOD.Studio.VCA bgmVCA;
    FMOD.Studio.VCA sfxVCA;
    FMOD.Studio.VCA voiceVCA;
    FMODUnity.StudioEventEmitter e;


    public static FMOD.DSP m_testDSP;
    public static FMOD.DSP m_fftDsp;
    public static FMOD.DSP m_faderDsp;
    public static FMOD.DSP m_faderAllDsp;
    private FMOD.Studio.EventInstance m_toneEmpty;
    private FMOD.ChannelGroup m_masterGroup;
    private FMOD.ChannelGroup m_toneGroup;
    private bool m_addedDSP = false;
    private bool m_created = false;
    private static int m_uniqueID = 0;
    private int m_ID;

    private void Start()
    {
        m_ID = m_uniqueID++;
        CreateDSP();
    }

    private void CreateDSP()
    {
        if (m_ID != 0) return;
        if (m_created) return;
        m_created = true;
        m_addedDSP = false;
        var res = RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out m_fftDsp);
        res = RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FADER, out m_faderDsp);
        res = RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FADER, out m_faderAllDsp);
        m_toneEmpty = RuntimeManager.CreateInstance("event:/Tone/ToneEmpty");
        res = m_toneEmpty.start();
    }

    private void ReleaseDSP()
    {
        if (m_ID != 0) return;
        if (m_addedDSP)
        {
            var result = m_masterGroup.removeDSP(m_faderAllDsp);
            result = m_toneGroup.removeDSP(m_faderDsp);
            result = m_toneGroup.removeDSP(m_fftDsp);
            m_addedDSP = false;
        }
        if(m_created)
        {
            var result = m_faderAllDsp.release();
            result = m_faderDsp.release();
            result = m_fftDsp.release();
            m_created = false;
        }
    }

    private void AddDSP()
    {
        if (m_ID != 0) return;
        if (m_addedDSP) return;
        m_addedDSP = true;
        RuntimeManager.CoreSystem.getMasterChannelGroup(out m_masterGroup);
        var result = m_masterGroup.addDSP(-1, m_faderAllDsp);

        result = m_toneEmpty.getChannelGroup(out FMOD.ChannelGroup g1);
        result = g1.getParentGroup(out FMOD.ChannelGroup parentGroup);
        result = parentGroup.getParentGroup(out FMOD.ChannelGroup parentGroup2);
        m_toneGroup = parentGroup2;
        result = m_toneGroup.addDSP(-1, m_faderDsp);
        result = m_toneGroup.addDSP(-2, m_fftDsp);
    }

    private void Update()
    {
        if (m_ID != 0) return;
        if (!m_addedDSP)
        {
            FMOD.Studio.PLAYBACK_STATE state;
            m_toneEmpty.getPlaybackState(out state);
            if (state != FMOD.Studio.PLAYBACK_STATE.STARTING)
            {
                AddDSP();
            }
        }
    }

    /// <summary>
    /// 关闭游戏的时候要销毁，否则会有一个报错信息"connectionsremaining==0" failed.
    /// </summary>
    public void OnDestroy()
    {
        if (m_ID != 0) return;
        ReleaseDSP();
    }

    private float x = -40;
    private float y = -10;
    /// <summary>
    /// x=-40, y=-10
    /// </summary>
    /// <param name="value"></param>
    private float GetFaderValue(float value)
    {
        if (value >= x)
        {
            return y * 1.0f * value / x;
        }
        else
        {
            return (y + 70) * 1.0f * value / (x + 70) + y - (y + 70) * 1.0f * x / (x + 70);
        }
    }

    public void MuteBus(string busName, bool mute)
    {
        FMOD.Studio.Bus bus;
        FMODUnity.RuntimeManager.StudioSystem.getBus(busName, out bus);
        if (bus.isValid()) bus.setMute(mute);
    }

    public void PauseBus(string busName, bool pause)
    {
        FMOD.Studio.Bus bus;
        FMODUnity.RuntimeManager.StudioSystem.getBus(busName, out bus);
        if (bus.isValid()) bus.setPaused(pause);
    }

    public void SetBusVolume(string strBus, float volume)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            FMOD.Studio.Bus bus;
            FMODUnity.RuntimeManager.StudioSystem.getBus(strBus, out bus);
            if (bus.isValid()) bus.setVolume(volume);
        }
    }

    public void SetMainVolume(float volume)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            float faderValue = GetFaderValue(volume);
            var res = m_faderAllDsp.setParameterFloat((int)FMOD.DSP_FADER.GAIN, faderValue);
            // Debug.LogError(res + "   faderValue=" + faderValue);
        }
    }

    /// <summary>
    /// 最初的实现方式，可以用ChannelGroup和VCA的方式进行音量的控制
    /// </summary>
    /// <param name="volume"></param>
    //public void SetMainVolume(float volume)
    //{
    //    if (FMODUnity.RuntimeManager.Instance != null)
    //    {
    //        if (!mainVCA.isValid())
    //            FMODUnity.RuntimeManager.StudioSystem.getVCA("vca:/Master", out mainVCA);
    //        if (mainVCA.isValid()) mainVCA.setVolume(volume);
    //    }
    //}

    public void SetBGMVolume(float volume)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            if (!bgmVCA.isValid())
                FMODUnity.RuntimeManager.StudioSystem.getVCA("vca:/BGM", out bgmVCA);

            if (bgmVCA.isValid()) bgmVCA.setVolume(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            if (!sfxVCA.isValid())
                FMODUnity.RuntimeManager.StudioSystem.getVCA("vca:/SFX", out sfxVCA);

            if (sfxVCA.isValid()) sfxVCA.setVolume(volume);
        }
    }
    public void SetVoiceVolume(float volume)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            //if (!voiceVCA.isValid())
            //    FMODUnity.RuntimeManager.StudioSystem.getVCA("vca:/VO", out voiceVCA);
            //if (voiceVCA.isValid()) voiceVCA.setVolume(volume);
            float faderValue = GetFaderValue(volume);
            var res = m_faderDsp.setParameterFloat((int)FMOD.DSP_FADER.GAIN, faderValue);
            //Debug.LogError(res + "   faderValue=" + faderValue);
        }

    }
    public void PlayOneShot(string key, Vector3 pos)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            FMODUnity.RuntimeManager.PlayOneShot(key, pos);
        }
    }

    public void StartEvent(string key)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            if (e == null)
                e = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();

            e.Event = key;
            e.CacheEventInstance();
            e.Play();
        }
    }

    public void StopEvent()
    {
        if (e == null) return;

        e.Stop();
    }

    public bool Deprecated
    {
        get;
        set;
    }

    public System.Object GetRuntimeFMOD()
    {
        return XRuntimeFmod.GetFMOD();
    }
    public void ReturnRuntimeFMOD(System.Object obj)
    {
        XRuntimeFmod.ReturnFMOD(obj as XRuntimeFmod);
    }

    public System.Object GetXDialogueFmod()
    {
        return XDialogueFmod.GetDialogueFmod();
    }

    public void ReturnXDialogueFmod(System.Object obj)
    {
        XDialogueFmod.ReturnDialogueFMOD(obj as XDialogueFmod);
    }

    public Component AddFMODComponent(GameObject go)
    {
        return go.AddComponent<XFmod>();
    }

    public void SetGameMusicPause(bool pauseStatus)
    {
        FMODUnity.RuntimeManager.Instance.OnApplicationPauseForAllPlatform(pauseStatus);
    }

    public void StopAllSoundByChannel(AudioChannel channel)
    {
        if (FMODUnity.RuntimeManager.Instance != null)
        {
            XRuntimeFmod.StopAllSoundByChannel(channel);
        }
    }

    public void OpenFmodFunction()
    {
        FMODUnity.RuntimeManager.OpenFmodFunction();
    }

    public void CloseFmodFunction()
    {
        FMODUnity.RuntimeManager.CloseFmodFunction();
    }

    public int GetEventLength(string eventName)
    {
        FMOD.Studio.EventDescription eventDescription = RuntimeManager.GetEventDescription(eventName);
        int length = 0;
        var result = eventDescription.getLength(out length);
        return length;
    }

    public void MasterMute(bool mute)
    {
        RuntimeManager.CoreSystem.getMasterChannelGroup(out m_masterGroup);
        m_masterGroup.setMute(mute);
    }

    public void UnloadInternalBanks()
    {
        RuntimeManager.Instance.UnloadBanks();
        ReleaseDSP();
    }

    public void LoadExternalBanks(List<string> banks)
    {
        for (int i = 0; i < banks.Count; ++i)
        {
            RuntimeManager.LoadBank(banks[i]);
        }
        CreateDSP();
    }

    public bool HasEvent(string eventPath)
    {
        return RuntimeManager.HasEvent(eventPath);
    }
}

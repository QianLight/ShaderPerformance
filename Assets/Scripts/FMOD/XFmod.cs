using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CFEngine;
using CFUtilPoolLib;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
public class XFmod :
#if UNITY_EDITOR
    EngineAudio,
#else
    MonoBehaviour,
#endif
    IXFmod, IAuido
{
    private FMODUnity.StudioEventEmitter _emitter = null;
    private FMODUnity.StudioEventEmitter _emitter2 = null;
    private FMODUnity.StudioEventEmitter _emitter3 = null;
    private FMODUnity.StudioEventEmitter _emitter4 = null;
    private FMODUnity.StudioEventEmitter _emitter5 = null;
    private FMODUnity.StudioEventEmitter _emitter6 = null;

    private Vector3 _3dPos = Vector3.zero;
    private string eventName = "";
    public void SetName(string name)
    {
        eventName = "event:/" + name;
    }
    public void Play(GameObject go)
    {
        Init(go, null);
        StartEvent(eventName);
    }
    public void Play(ref Vector3 pos)
    {
        _3dPos = pos;
        StartEvent(eventName);
    }
    public void Destroy()
    {
        if (_emitter != null)
        {
            _emitter.Stop();
            _emitter = null;
        }

        if (_emitter2 != null)
        {
            _emitter2.Stop();
            _emitter2 = null;
        }

        if (_emitter3 != null)
        {
            _emitter3.Stop();
            _emitter3 = null;
        }

        if (_emitter4 != null)
        {
            _emitter4.Stop();
            _emitter4 = null;
        }

        if (_emitter5 != null)
        {
            _emitter5.Stop();
            _emitter5 = null;
        }

        if (_emitter6 != null)
        {
            _emitter6.Stop();
            _emitter6 = null;
        }
        eventName = "";
    }

    public bool IsPlaying(AudioChannel channel)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);

        return e != null && e.IsPlaying();
    }

    public void SetSpeed(float speed, AudioChannel channel = AudioChannel.Action)
    {
        StudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return;
        if (e.EventInstance.isValid())
        {
            FMOD.RESULT res = e.EventInstance.setPitch(speed);
            // Debug.Log (res);
        }
        else
        {
            DebugLog.AddWarningLog("event instance is not valid");
        }
    }

    public void SetVolume(float volume, AudioChannel channel = AudioChannel.Action)
    {
        StudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return;
        if (e.EventInstance.isValid())
        {
            FMOD.RESULT res = e.EventInstance.setVolume(volume);
            // Debug.Log (res);
        }
        else
        {
            DebugLog.AddWarningLog("event instance is not valid");
        }
    }

    public float GetVolume(AudioChannel channel = AudioChannel.Action)
    {
        StudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return 0;
        float volume = 0;
        if (e.EventInstance.isValid())
        {
            e.EventInstance.getVolume(out volume);
            return volume;
        }

        return 0;
    }

    public void SetSpeedAndVolume(AudioChannel channel, float speed, float volume, string eventName = "")
    {
        SetSpeed(speed, channel);
        volume = GetVolumeBySpeed(speed); //根据speed重写计算volume值
        SetVolume(volume, channel);
    }

    /// <summary>
    /// 参考：XAudioMgr的GetVolumeBySpeed方法
    /// </summary>
    /// <param name="speed"></param>
    /// <returns></returns>
    public float GetVolumeBySpeed(float speed)
    {
        if (speed <= 0.25f) return 0;
        else if (speed <= 0.5f) return 0.5f;
        else return 1.0f;
    }

    public void StartEvent(string key, AudioChannel channel = AudioChannel.Action, bool stopPre = true, string para = "", float value = 0)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return;

        if (stopPre) e.Stop();

        if (!string.IsNullOrEmpty(key))
        {
            e.Event = key;
        }

        e.CacheEventInstance();

        SetParamValueBeforePlay(channel, para, value);

        //if (_3dPos != Vector3.zero)
        //{
        //    e.Update3DAttributes(_3dPos);
        //    _3dPos = Vector3.zero;
        //}

        e.Play();
    }

    public void Play(AudioChannel channel = AudioChannel.Action)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);

        if (e != null)
            e.Play();
    }

    public void Stop(AudioChannel channel = AudioChannel.Action, CFUtilPoolLib.STOP_MODE stopMode = CFUtilPoolLib.STOP_MODE.ALLOWFADEOUT)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);

        if (e != null)
            e.Stop();
    }

    public void SetPause(AudioChannel channel, bool isPause)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            e.SetPause(isPause);
        }
    }

    public void PlayOneShot(string key, Vector3 pos)
    {
        RuntimeManager.PlayOneShot(key, pos);
    }

    public void Update3DAttributes(Vector3 vec, AudioChannel channel = AudioChannel.Action)
    {
        _3dPos = vec;
    }

    public void SetParamValueBeforePlay(AudioChannel channel, string param, float value)
    {
        if (!string.IsNullOrEmpty(param))
        {
            FMODUnity.StudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                FMODUnity.ParamRef fmodParam = new ParamRef();
                fmodParam.Name = param;
                fmodParam.Value = value;

                if (e.Params == null)
                {
                    e.Params = new ParamRef[1];
                    e.Params[0].Name = param;
                    e.Params[0].Value = value;
                }
            }
        }
    }

    public void SetParam(AudioChannel channel, string param, float value)
    {
        if (!string.IsNullOrEmpty(param))
        {
            FMODUnity.StudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                e.SetParameter(param, value);
            }
        }
    }

    public void GetParam(AudioChannel channel, string param, out float value)
    {
        value = 0;
        if (!string.IsNullOrEmpty(param))
        {
            StudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                e.GetParameter(param, out value);
            }
        }
    }
    private FMODUnity.StudioEventEmitter AddEmitter()
    {
        FMODUnity.StudioEventEmitter e;
        if (!gameObject.TryGetComponent(out e))
        {
            e = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
        }
        return e;
    }
    public FMODUnity.StudioEventEmitter GetEmitter(AudioChannel channel)
    {
#if !DISABLE_FMODE
        switch (channel)
        {
            case AudioChannel.Action:
                {
                    if (_emitter == null)
                    {
                        _emitter = AddEmitter();
                        _emitter.StopEvent = EmitterGameEvent.ObjectDestroy;
                    }

                    return _emitter;
                }
            case AudioChannel.Motion:
                {
                    if (_emitter2 == null)
                    {
                        _emitter2 = AddEmitter();
                        _emitter2.StopEvent = EmitterGameEvent.ObjectDestroy;
                    }

                    return _emitter2;
                }
            case AudioChannel.Skill:
                {
                    if (_emitter3 == null)
                    {
                        _emitter3 = AddEmitter();
                        _emitter3.StopEvent = EmitterGameEvent.ObjectDestroy;
                    }

                    return _emitter3;
                }
            case AudioChannel.Behit:
                {
                    if (_emitter4 == null)
                    {
                        _emitter4 = AddEmitter();
                        _emitter4.StopEvent = EmitterGameEvent.ObjectDestroy;
                    }

                    return _emitter4;
                }
            case AudioChannel.SkillCombine:
                {
                    if (_emitter5 == null)
                    {
                        _emitter5 = AddEmitter();
                        //_emitter5.StopEvent = EmitterGameEvent.ObjectDestroy;
                    }

                    return _emitter5;
                }
            case AudioChannel.BatiBehit:
                {
                    if (_emitter6 == null)
                    {
                        _emitter6 = AddEmitter();
                    }

                    return _emitter6;
                }
        }

#endif
        return null;

    }
    public void Init(GameObject go, Rigidbody rigidbody) { }

    public bool IsValid(AudioChannel channel)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            return e.EventInstance.isValid();
        }
        return false;
    }
#if UNITY_EDITOR
    public override void StartAudio(string name)
    {
        StartEvent(name);
    }
    public override void StopAudio()
    {
        Stop();
    }
    public override void SetPos()
    {
        Update3DAttributes(this.transform.position);
    }
    public override bool IsValid()
    {
        return GetEmitter(AudioChannel.Action) != null;
    }

    public static EngineAudio AddXFmodComponent(GameObject go)
    {
        XFmod fmod;
        if (!go.TryGetComponent(out fmod))
        {
            fmod = go.AddComponent<XFmod>();
        }
        return fmod;
    }
#endif
    public void PlayLevelAudio(string key, Vector3 pos, bool stopPre = false)
    {
        // do nothing.
    }

    public void StopLevelAudio(string key, Vector3 pos)
    {
        // do nothing.
    }

    public void StopLevelAllAudio()
    {
        // do nothing.
    }

    public void SetFmodID(int id)
    {
        // do nothing.
    }

    public void PauseAudio(bool pause)
    {
        FMODUnity.StudioEventEmitter e = GetEmitter(AudioChannel.Action);
        if (e != null)
        {
            e.SetPause(pause);
        }
    }

    public void SetPauseChannels(List<AudioChannel> channels, bool isPause)
    {
        // do nothing...
    }

    public void PauseLevelAudio(string key, Vector3 pos, bool pause)
    {

    }

    public void PauseAllLevelAudio(bool pause)
    {

    }

    public void PlayMp4Audio(string eventName)
    {
    }

    public void StopMp4Audio()
    {
    }

    public void PauseMp4Audio(bool isPause)
    {
    }

    public bool Deprecated
    {
        get;
        set;
    }
}

public class XRuntimeFmod : IXFmod, ICsFmod, IAuido
{
    private static Queue<RuntimeStudioEventEmitter> emitterQueue = new Queue<RuntimeStudioEventEmitter>();
    private static Queue<XRuntimeFmod> fmodQueue = new Queue<XRuntimeFmod>();
    private static List<XRuntimeFmod> m_currentUsingFmodList = new List<XRuntimeFmod>();

    private RuntimeStudioEventEmitter _emitter1 = null;
    private RuntimeStudioEventEmitter _emitter2 = null;
    private RuntimeStudioEventEmitter _emitter3 = null;
    private RuntimeStudioEventEmitter _emitter4 = null;
    private RuntimeStudioEventEmitter _emitter5 = null;
    private RuntimeStudioEventEmitter _emitter6 = null;
    private RuntimeStudioEventEmitter _emitter7 = null;
    private RuntimeStudioEventEmitter _emitter8 = null;
    private RuntimeStudioEventEmitter _emitter9 = null;
    private RuntimeStudioEventEmitter _emitter10 = null;


    private static XRuntimeFmod m_levelFmod = new XRuntimeFmod();
    private static Dictionary<string, EventInstance> m_levelEventDict = new Dictionary<string, EventInstance>();

    public int m_fmodID = 0;
    private Vector3 _3dPos = Vector3.zero;
    public GameObject cachedGo;
    public Rigidbody cachedRigidBody;

    private string eventName = "";
    public static void Clear()
    {
        emitterQueue.Clear();
        fmodQueue.Clear();
    }
    public static XRuntimeFmod GetFMOD()
    {
        XRuntimeFmod fmod = null;
        if (fmodQueue.Count > 0)
        {
            fmod = fmodQueue.Dequeue();
        }
        else
        {
            fmod = new XRuntimeFmod();
        }
        m_currentUsingFmodList.Add(fmod);
        return fmod;
    }

    public static void ReturnFMOD(XRuntimeFmod fmod)
    {
        if (fmod == null)
        {
            //Debug.LogError("fmod is null");
            return;
        }
        fmod.Destroy();
        fmodQueue.Enqueue(fmod);
        m_currentUsingFmodList.Remove(fmod);
    }

    public static void StopAllSoundByChannel(AudioChannel channel)
    {
        for (int i = 0; i < m_currentUsingFmodList.Count; ++i)
        {
            XRuntimeFmod fmod = m_currentUsingFmodList[i];
            if (fmod == null) continue;
            fmod.Stop(channel);
        }
    }

    private static RuntimeStudioEventEmitter GetEmitter()
    {
        if (emitterQueue.Count > 0)
        {
            RuntimeStudioEventEmitter e = emitterQueue.Dequeue();
            e.Reset();
            return e;
        }
        return new RuntimeStudioEventEmitter();
    }

    private static void ReturnEmitter(RuntimeStudioEventEmitter e)
    {
        emitterQueue.Enqueue(e);
    }

    public void SetName(string name)
    {
        eventName = name; //在场景中直接配置形如event:/Music/Battle事件全路径
    }
    public void Play(GameObject go)
    {
        Init(go, null);
        StartEvent(eventName);
        //Debug.LogError("sfx play=" + eventName);
    }
    public void Play(ref Vector3 pos)
    {
        _3dPos = pos;
        StartEvent(eventName);
    }

    public void PlayLevelAudio(string key, Vector3 pos, bool stopPre = false)
    {
        string id = key + ":" + pos.x + ":" + pos.y + ":" + pos.z;
        EventInstance instance;
        if (m_levelEventDict.ContainsKey(id))
        {
            instance = m_levelEventDict[id];
            if (instance.isValid())
            {
                if (stopPre)
                {
                    instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
                instance.release();
                instance.clearHandle();
            }
            m_levelEventDict.Remove(id);
        }
        instance = RuntimeManager.CreateInstance(key);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        instance.start();
        m_levelEventDict.Add(id, instance);
    }

    public void StopLevelAudio(string key, Vector3 pos)
    {
        string id = key + ":" + pos.x + ":" + pos.y + ":" + pos.z;
        EventInstance instance;
        if (m_levelEventDict.ContainsKey(id))
        {
            instance = m_levelEventDict[id];
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
                instance.clearHandle();
            }
            m_levelEventDict.Remove(id);
        }
    }

    public void StopLevelAllAudio()
    {
        foreach (var item in m_levelEventDict)
        {
            if (item.Value.isValid())
            {
                item.Value.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                item.Value.release();
                item.Value.clearHandle();
            }
        }
        m_levelEventDict.Clear();
    }

    public void PauseLevelAudio(string key, Vector3 pos, bool pause)
    {
        string id = key + ":" + pos.x + ":" + pos.y + ":" + pos.z;
        EventInstance instance;
        if (m_levelEventDict.ContainsKey(id))
        {
            instance = m_levelEventDict[id];
            if (instance.isValid())
            {
                instance.setPaused(pause);
            }
        }
    }

    public void PauseAllLevelAudio(bool pause)
    {
        foreach (var item in m_levelEventDict)
        {
            if (item.Value.isValid())
            {
                item.Value.setPaused(pause);
            }
        }
    }

    public void Init(GameObject go, Rigidbody rigidbody)
    {
        cachedGo = go;
        cachedRigidBody = rigidbody;
    }

    public void Destroy()
    {
        DestroyEmitter(ref _emitter1);
        DestroyEmitter(ref _emitter2);
        DestroyEmitter(ref _emitter3);
        DestroyEmitter(ref _emitter4);
        DestroyEmitter(ref _emitter5);
        DestroyEmitter(ref _emitter6);
        DestroyEmitter(ref _emitter7);
        DestroyEmitter(ref _emitter8);
        DestroyEmitter(ref _emitter9);
        DestroyEmitter(ref _emitter10);

        eventName = "";

        cachedGo = null;
        cachedRigidBody = null;
        m_fmodID = 0;
    }

    /// <summary>
    /// 一定要ref传递，否则_emitterN依然持有对象，造成停止事件异常的问题
    /// </summary>
    /// <param name="emitter"></param>
    public void DestroyEmitter(ref RuntimeStudioEventEmitter emitter)
    {
        if (emitter != null)
        {
            emitter.Stop();
            ReturnEmitter(emitter);
            emitter = null;
        }
    }

    // public void Release ()
    // {
    //     Destroy ();
    //     ReturnFMOD (this);
    // }
    public bool IsPlaying(AudioChannel channel)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);

        return e != null && e.IsPlaying();
    }

    /// <summary>
    /// 设置某个channel的播放速度，先调用播放StartEvent
    /// </summary>
    /// <param name="speed">小于1减速 1正常速度 大于1加速播放</param>
    /// <param name="channel"></param>
    public void SetSpeed(float speed, AudioChannel channel = AudioChannel.Action)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null || !e.EventInstance.isValid())
            return;
        if (speed > 1) speed = 1; //无加速需求，只控制减速
        FMOD.RESULT res = e.EventInstance.setPitch(speed);
    }

    /// <summary>
    /// 设置某个channel的播放声音大小，先调用播放StartEvent
    /// </summary>
    /// <param name="volume">0静音 1正常 可以大于1，但是不能小于0</param>
    /// <param name="channel"></param>

    public void SetVolume(float volume, AudioChannel channel = AudioChannel.Action)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null || !e.EventInstance.isValid())
            return;
        e.EventInstance.setVolume(volume);
    }

    public float GetVolume(AudioChannel channel = AudioChannel.Action)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null || !e.EventInstance.isValid())
            return 0;
        float volume = 1;
        e.EventInstance.getVolume(out volume);
        return volume;
    }

    public void SetSpeedAndVolume(AudioChannel channel, float speed, float volume, string eventName = "")
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null) return;
        if (!string.IsNullOrEmpty(eventName) && !e.Event.Contains(eventName)) return;

        SetSpeed(speed, channel);
        SetVolume(volume, channel);
    }

    public void StartEvent(string key, AudioChannel channel = AudioChannel.Action, bool stopPre = false, string para = "", float value = 0)
    {
        //Debug.LogError("key = " + key + " channel=" + channel);


        bool isOpen = RuntimeManager.GetFmodFunction();
        if (!isOpen) return;

        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return;
        if (stopPre) e.Stop();

        if (!string.IsNullOrEmpty(key))
        {
            e.Event = key;
        }

        e.CacheEventInstance();

        SetParamValueBeforePlay(channel, para, value);

        if (_3dPos != Vector3.zero)
        {
            e.Update3DAttributes(_3dPos);
            _3dPos = Vector3.zero;
        }

        e.Play(cachedGo, cachedRigidBody);
    }

    public Guid GetEventGUID(AudioChannel channel)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e.EventInstance.isValid())
        {
            EventDescription description;
            Guid guid;
            e.EventInstance.getDescription(out description);
            description.getID(out guid);
            return guid;
        }
        return Guid.Empty;
    }

    public void Play(AudioChannel channel = AudioChannel.Action)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);

        if (e != null && !e.IsPlaying())
            e.Play(cachedGo, cachedRigidBody);
    }

    public void Stop(AudioChannel channel = AudioChannel.Action, CFUtilPoolLib.STOP_MODE stopMode = CFUtilPoolLib.STOP_MODE.IMMEDIATE)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);

        if (e != null)
        {
            FMOD.Studio.STOP_MODE mode = (FMOD.Studio.STOP_MODE)stopMode;
            e.Stop(mode);
        }
    }

    public void Stop(AudioChannel channel, List<string> events, CFUtilPoolLib.STOP_MODE stopMode = CFUtilPoolLib.STOP_MODE.IMMEDIATE)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            FMOD.Studio.STOP_MODE mode = (FMOD.Studio.STOP_MODE)stopMode;
            FMOD.Studio.EventInstance[] array;

            for (int i = 0; i < events.Count; ++i)
            {
                FMOD.Studio.EventDescription eventDescription = RuntimeManager.GetEventDescription(events[i]);
                eventDescription.getInstanceList(out array);
                if (array != null)
                {
                    for (int j = 0; j < array.Length; ++j)
                    {
                        if (array[j].isValid())
                        {
                            array[j].stop(mode);
                            array[j].release();
                            array[j].clearHandle();
                        }
                    }
                }
            }
        }
    }

    public void SetPause(AudioChannel channel, bool isPause)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            e.SetPause(isPause);
        }
    }

    public void SetPauseChannels(List<AudioChannel> channels, bool isPause)
    {
        if (channels == null) return;
        for (int i = 0; i < channels.Count; ++i)
        {
            SetPause(channels[i], isPause);
        }
    }

    /// <summary>
    /// IAudio中的接口，为了实现SFX中的音乐暂停，sfx.cs中都是使用Action频道播放的事件
    /// </summary>
    /// <param name="pause"></param>
    public void PauseAudio(bool pause)
    {
        RuntimeStudioEventEmitter e = GetEmitter(AudioChannel.Action);
        if (e != null)
        {
            e.SetPause(pause);
        }
    }

    public FMOD.Studio.PLAYBACK_STATE GetEventState(AudioChannel channel)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            return e.GetEventState();
        }
        return FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    public void PlayOneShot(string key, Vector3 pos)
    {
#if !DISABLE_FMODE
        RuntimeManager.PlayOneShot(key, pos);
#endif
    }

    public void Update3DAttributes(Vector3 vec, AudioChannel channel = AudioChannel.Action)
    {
        _3dPos = vec;
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e == null)
            return;
        e.Update3DAttributes(vec);
    }

    public void SetParamValueBeforePlay(AudioChannel channel, string param, float value)
    {
        if (!string.IsNullOrEmpty(param))
        {
            RuntimeStudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                FMODUnity.ParamRef fmodParam = new ParamRef();
                fmodParam.Name = param;
                fmodParam.Value = value;

                if (e.Params == null)
                {
                    e.Params = new List<ParamRef>();
                    e.Params.Add(fmodParam);
                }
            }

        }
    }

    public void SetParam(AudioChannel channel, string param, float value)
    {
        if (!string.IsNullOrEmpty(param))
        {
            RuntimeStudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                e.SetParameter(param, value);
            }
        }
    }

    public void GetParam(AudioChannel channel, string param, out float value)
    {
        value = 0;
        if (!string.IsNullOrEmpty(param))
        {
            RuntimeStudioEventEmitter e = GetEmitter(channel);
            if (e != null)
            {
                e.GetParameter(param, out value);
            }
        }
    }

    public void PlayMp4Audio(string eventName)
    {
        StartEvent(eventName, AudioChannel.Action, true);
    }

    public void StopMp4Audio()
    {
        Stop(AudioChannel.Action);
    }

    public void PauseMp4Audio(bool isPause)
    {
        SetPause(AudioChannel.Action, isPause);
    }


    private RuntimeStudioEventEmitter GetEmitter(AudioChannel channel)
    {
#if !DISABLE_FMODE
        switch (channel)
        {
            case AudioChannel.Action:
                {
                    if (_emitter1 == null)
                    {
                        _emitter1 = GetEmitter();
                    }
                    return _emitter1;
                }
            case AudioChannel.Motion:
                {
                    if (_emitter2 == null)
                    {
                        _emitter2 = GetEmitter();
                    }
                    return _emitter2;
                }
            case AudioChannel.Skill:
                {
                    if (_emitter3 == null)
                    {
                        _emitter3 = GetEmitter();
                    }
                    return _emitter3;
                }
            case AudioChannel.Behit:
                {
                    if (_emitter4 == null)
                    {
                        _emitter4 = GetEmitter();
                    }
                    return _emitter4;
                }
            case AudioChannel.SkillCombine:
                {
                    if (_emitter5 == null)
                    {
                        _emitter5 = GetEmitter();
                    }
                    return _emitter5;
                }
            case AudioChannel.BatiBehit:
                {
                    if (_emitter6 == null)
                    {
                        _emitter6 = GetEmitter();
                    }
                    return _emitter6;
                }
            case AudioChannel.Music:
                {
                    if (_emitter7 == null)
                    {
                        _emitter7 = GetEmitter();
                    }
                    return _emitter7;
                }
            case AudioChannel.Vocal:
                {
                    if (_emitter8 == null)
                    {
                        _emitter8 = GetEmitter();
                    }

                    return _emitter8;
                }
            case AudioChannel.SFX:
                {
                    if (_emitter9 == null)
                    {
                        _emitter9 = GetEmitter();
                    }
                    return _emitter9;
                }
            case AudioChannel.LogicMusic:
                {
                    if (_emitter10 == null)
                    {
                        _emitter10 = GetEmitter();
                    }
                    return _emitter10;
                }
        }
#endif
        return null;

    }

    public bool IsValid(AudioChannel channel)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e != null)
        {
            return e.EventInstance.isValid();
        }
        return false;
    }

    public void RegisterEventPlayFinishCallback(FMOD.Studio.EVENT_CALLBACK callback, AudioChannel channel)
    {
        RuntimeStudioEventEmitter e = GetEmitter(channel);
        if (e != null && e.EventInstance.isValid())
        {
            e.EventInstance.setCallback(callback);
        }
    }

    public void SetFmodID(int id)
    {
        m_fmodID = id;
    }

    public bool Deprecated
    {
        get;
        set;
    }
}

public class XDialogueFmod : IXDialogueFmod
{
    private static Queue<XDialogueFmod> dialogueFmod = new Queue<XDialogueFmod>();
    private FMOD.Studio.EVENT_CALLBACK dialogueCallback;
    private string eventName;
    private EventInstance dialogueInstance;

    public static XDialogueFmod GetDialogueFmod()
    {
        if (dialogueFmod.Count > 0)
        {
            return dialogueFmod.Dequeue();
        }
        return new XDialogueFmod();
    }

    public static void ReturnDialogueFMOD(XDialogueFmod fmod)
    {
        dialogueFmod.Enqueue(fmod);
    }

    /// <summary>
    /// event:/Tone/Dialogue
    /// </summary>
    /// <param name="eventName"></param>
    public void Init(string eventName)
    {
        this.eventName = eventName;
    }

    /// <summary>
    /// audioName=abc
    /// </summary>
    /// <param name="audioName"></param>
    /// <param name="stopPre"></param>
    public void PlayDialogue(string audioName, bool stopPre = false)
    {
        //if (dialogueCallback == null)
        //{
        //    dialogueCallback = new FMOD.Studio.EVENT_CALLBACK(DialogueEventCallback);
        //}

        //if (stopPre)
        //{
        //    dialogueInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        //}

        //dialogueInstance = FMODUnity.RuntimeManager.CreateInstance(eventName);
        //GCHandle stringHandle = GCHandle.Alloc(audioName, GCHandleType.Pinned);
        //dialogueInstance.setUserData(GCHandle.ToIntPtr(stringHandle));

        //dialogueInstance.setCallback(dialogueCallback);
        //dialogueInstance.start();
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, FMOD.Studio.EventInstance instance, IntPtr parameterPtr)
    {
        // Retrieve the user data
        IntPtr stringPtr;
        instance.getUserData(out stringPtr);

        // Get the string object
        GCHandle stringHandle = GCHandle.FromIntPtr(stringPtr);
        String key = stringHandle.Target as String;

        switch (type)
        {
            case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE | FMOD.MODE.NONBLOCKING;
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                    FMOD.Studio.SOUND_INFO dialogueSoundInfo;
                    var keyResult = FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(key, out dialogueSoundInfo);
                    if (keyResult != FMOD.RESULT.OK)
                    {
                        break;
                    }
                    FMOD.Sound dialogueSound;
                    var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode, ref dialogueSoundInfo.exinfo, out dialogueSound);
                    if (soundResult == FMOD.RESULT.OK)
                    {
                        parameter.sound = dialogueSound.handle;
                        parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
                        Marshal.StructureToPtr(parameter, parameterPtr, false);
                    }
                }
                break;
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                {
                    var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                    var sound = new FMOD.Sound();
                    sound.handle = parameter.sound;
                    sound.release();
                }
                break;
            case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                // Now the event has been destroyed, unpin the string memory so it can be garbage collected
                stringHandle.Free();
                break;
        }
        return FMOD.RESULT.OK;
    }

    public void TestPlay()
    {
        PlayDialogue("abc");
    }
    public bool Deprecated
    {
        get;
        set;
    }
}
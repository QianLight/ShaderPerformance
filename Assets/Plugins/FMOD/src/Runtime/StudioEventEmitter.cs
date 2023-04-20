using System;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;

namespace FMODUnity
{

    public class StudioEventEmitterManager : XSingleton<StudioEventEmitterManager>, IStudioEventEmitter
    {
        public static Dictionary<uint, StudioEventEmitter> m_dict;
        public bool Deprecated
        {
            get;
            set;
        }

        public void PlayEvent(string eventName)
        {

        }

        public void StopEvent(string eventName)
        {
            if (m_dict == null) return;
            uint hashID = XCommon.singleton.XHash(eventName);
            StudioEventEmitter emitter;
            if (m_dict.TryGetValue(hashID, out emitter))
            {
                emitter.Stop();
            }
        }

        /// <summary>
        /// 增加StudioEventEmitter
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="emitter"></param>
        public void AddStudioEventEmitter(string eventName, StudioEventEmitter emitter)
        {
            if (m_dict == null) m_dict = new Dictionary<uint, StudioEventEmitter>();
            uint hashID = XCommon.singleton.XHash(eventName);
            if (m_dict.ContainsKey(hashID))
            {
                m_dict[hashID] = emitter;
            }
            else
            {
                m_dict.Add(hashID, emitter);
            }
        }

        /// <summary>
        /// 删除StudioEventEmitter
        /// </summary>
        /// <param name="eventName"></param>
        public void RemoveStudioEventEmitter(string eventName)
        {
            if (m_dict == null) return;
            uint hashID = XCommon.singleton.XHash(eventName);
            if (m_dict.ContainsKey(hashID))
            {
                m_dict.Remove(hashID);
            }
        }
    }


    [AddComponentMenu("FMOD Studio/FMOD Studio Event Emitter")]
    public class StudioEventEmitter : EventHandler
    {
        [EventRef]
        public string Event = "";
        public EmitterGameEvent PlayEvent = EmitterGameEvent.None;
        public EmitterGameEvent StopEvent = EmitterGameEvent.None;
        public bool AllowFadeout = true;
        public bool TriggerOnce = false;
        public bool Preload = false;
        public ParamRef[] Params = new ParamRef[0];
        public bool OverrideAttenuation = false;
        public float OverrideMinDistance = -1.0f;
        public float OverrideMaxDistance = -1.0f;

        protected FMOD.Studio.EventDescription eventDescription;
        public FMOD.Studio.EventDescription EventDescription { get { return eventDescription; } }

        protected FMOD.Studio.EventInstance instance;
        public FMOD.Studio.EventInstance EventInstance { get { return instance; } }

        private bool hasTriggered = false;
        private bool isQuitting = false;
        private bool isOneshot = false;
        private List<ParamRef> cachedParams = new List<ParamRef>();

        private const string SnapshotString = "snapshot";

        public bool IsActive { get; private set; }

        public float MaxDistance
        {
            get
            {
                if (OverrideAttenuation)
                {
                    return OverrideMaxDistance;
                }

                if (!eventDescription.isValid())
                {
                    Lookup();
                }

                float maxDistance;
                eventDescription.getMaximumDistance(out maxDistance);
                return maxDistance;
            }
        }

        void Start()
        {
            RuntimeUtils.EnforceLibraryOrder();
            if (Preload)
            {
                Lookup();
                eventDescription.loadSampleData();
                RuntimeManager.StudioSystem.update();
                FMOD.Studio.LOADING_STATE loadingState;
                eventDescription.getSampleLoadingState(out loadingState);
                while (loadingState == FMOD.Studio.LOADING_STATE.LOADING)
                {
#if WINDOWS_UWP
                    System.Threading.Tasks.Task.Delay(1).Wait();
#else
                    System.Threading.Thread.Sleep(1);
#endif
                    eventDescription.getSampleLoadingState(out loadingState);
                }
            }
            HandleGameEvent(EmitterGameEvent.ObjectStart);

            //主要为了给场景中水特效停止声音使用
            if (!string.IsNullOrEmpty(Event))
            {
                StudioEventEmitterManager.singleton.AddStudioEventEmitter(Event, this);
            }
        }

        void OnApplicationQuit()
        {
            isQuitting = true;
        }

        void OnDestroy()
        {
            if (!string.IsNullOrEmpty(Event))
            {
                StudioEventEmitterManager.singleton.RemoveStudioEventEmitter(Event);
            }

            if (!isQuitting)
            {
                HandleGameEvent(EmitterGameEvent.ObjectDestroy);

                if (instance.isValid())
                {
                    RuntimeManager.DetachInstanceFromGameObject(instance);
                    if (eventDescription.isValid() && isOneshot)
                    {
                        instance.release();
                        instance.clearHandle();
                    }
                }

                RuntimeManager.DeregisterActiveEmitter(this);

                if (Preload)
                {
                    eventDescription.unloadSampleData();
                }
            }
        }

        protected override void HandleGameEvent(EmitterGameEvent gameEvent)
        {
            if (PlayEvent == gameEvent)
            {
                Play();
            }
            if (StopEvent == gameEvent)
            {
                Stop();
            }
        }

        void Lookup()
        {
            eventDescription = RuntimeManager.GetEventDescription(Event);

            if (eventDescription.isValid())
            {
                for (int i = 0; i < Params.Length; i++)
                {
                    FMOD.Studio.PARAMETER_DESCRIPTION param;
                    eventDescription.getParameterDescriptionByName(Params[i].Name, out param);
                    Params[i].ID = param.id;
                }
            }
        }

        public void Play()
        {
            if (TriggerOnce && hasTriggered)
            {
                return;
            }

            if (string.IsNullOrEmpty(Event))
            {
                return;
            }

            cachedParams.Clear();

            if (!eventDescription.isValid())
            {
                Lookup();
            }

            if (!Event.StartsWith(SnapshotString, StringComparison.CurrentCultureIgnoreCase))
            {
                eventDescription.isOneshot(out isOneshot);
            }

            bool is3D;
            eventDescription.is3D(out is3D);

            IsActive = true;

            if (is3D && !isOneshot && Settings.Instance.StopEventsOutsideMaxDistance)
            {
                RuntimeManager.RegisterActiveEmitter(this);
                RuntimeManager.UpdateActiveEmitter(this, true);
            }
            else
            {
                PlayInstance();
            }
        }

        public void PlayInstance()
        {
            if (!instance.isValid())
            {
                instance.clearHandle();
            }

            // Let previous oneshot instances play out
            if (isOneshot && instance.isValid())
            {
                instance.release();
                instance.clearHandle();
            }

            bool is3D;
            eventDescription.is3D(out is3D);

            if (!instance.isValid())
            {
                eventDescription.createInstance(out instance);

                // Only want to update if we need to set 3D attributes
                if (is3D)
                {
                    var transform = GetComponent<Transform>();
#if UNITY_PHYSICS_EXIST || !UNITY_2019_1_OR_NEWER
                    if (GetComponent<Rigidbody>())
                    {
                        Rigidbody rigidBody = GetComponent<Rigidbody>();
                        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject, rigidBody));
                        RuntimeManager.AttachInstanceToGameObject(instance, transform, rigidBody);
                    }
                    else
#endif
#if UNITY_PHYSICS2D_EXIST || !UNITY_2019_1_OR_NEWER
                    if (GetComponent<Rigidbody2D>())
                    {
                        var rigidBody2D = GetComponent<Rigidbody2D>();
                        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject, rigidBody2D));
                        RuntimeManager.AttachInstanceToGameObject(instance, transform, rigidBody2D);
                    }
                    else
#endif
                    {
                        instance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                        RuntimeManager.AttachInstanceToGameObject(instance, transform);
                    }
                }
            }

            foreach (var param in Params)
            {
                instance.setParameterByID(param.ID, param.Value);
            }

            foreach (var cachedParam in cachedParams)
            {
                instance.setParameterByID(cachedParam.ID, cachedParam.Value);
            }

            if (is3D && OverrideAttenuation)
            {
                instance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, OverrideMinDistance);
                instance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, OverrideMaxDistance);
            }

            instance.start();

            hasTriggered = true;
        }

        public void SetPause(bool isPause)
        {
            if (instance.isValid())
            {
                instance.setPaused(isPause);
            }
        }

        public void Stop()
        {
            RuntimeManager.DeregisterActiveEmitter(this);
            IsActive = false;
            cachedParams.Clear();
            StopInstance();
        }

        public void StopInstance()
        {
            if (TriggerOnce && hasTriggered)
            {
                RuntimeManager.DeregisterActiveEmitter(this);
            }

            if (instance.isValid())
            {
                instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
                instance.clearHandle();
            }
        }

        public void SetParameter(string name, float value)
        {
            if (instance.isValid())
            {
                instance.setParameterByName(name, value);
            }
        }

        public void GetParameter(string name, out float value)
        {
            value = 0;
            if (instance.isValid())
            {
                instance.getParameterByName(name, out value);
            }
        }

        public void SetParameter(string name, float value, bool ignoreseekspeed = false)
        {
            if (Settings.Instance.StopEventsOutsideMaxDistance && IsActive)
            {
                ParamRef cachedParam = cachedParams.Find(x => x.Name == name);

                if (cachedParam == null)
                {
                    FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
                    eventDescription.getParameterDescriptionByName(name, out paramDesc);

                    cachedParam = new ParamRef();
                    cachedParam.ID = paramDesc.id;
                    cachedParam.Name = paramDesc.name;
                    cachedParams.Add(cachedParam);
                }

                cachedParam.Value = value;
            }

            if (instance.isValid())
            {
                instance.setParameterByName(name, value, ignoreseekspeed);
            }
        }



        public void SetParameter(FMOD.Studio.PARAMETER_ID id, float value, bool ignoreseekspeed = false)
        {
            if (Settings.Instance.StopEventsOutsideMaxDistance && IsActive)
            {
                ParamRef cachedParam = cachedParams.Find(x => x.ID.Equals(id));

                if (cachedParam == null)
                {
                    FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
                    eventDescription.getParameterDescriptionByID(id, out paramDesc);

                    cachedParam = new ParamRef();
                    cachedParam.ID = paramDesc.id;
                    cachedParam.Name = paramDesc.name;
                    cachedParams.Add(cachedParam);
                }

                cachedParam.Value = value;
            }

            if (instance.isValid())
            {
                instance.setParameterByID(id, value, ignoreseekspeed);
            }
        }

        public bool IsPlaying()
        {
            if (instance.isValid())
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                instance.getPlaybackState(out playbackState);
                return (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
            }
            return false;
        }

        public void CacheEventInstance()
        {
            if (String.IsNullOrEmpty(Event))
            {
                return;
            }

            Lookup();
        }
    }

    public class RuntimeStudioEventEmitter
    {
        public String Event = "";

        public List<ParamRef> Params = null;
        public Vector3 Pos = Vector3.zero;

        private FMOD.Studio.EventDescription eventDescription;
        private FMOD.Studio.EventInstance instance;
        // private bool hasTriggered = false;
        public static bool isQuitting = false;
        public FMOD.Studio.EventInstance EventInstance { get { return instance; } }

        public void Reset()
        {
            Event = "";
            Params = null;
            Pos = Vector3.zero;

            eventDescription.clearHandle();
            instance.clearHandle();
            // hasTriggered = false;
            isQuitting = false;
        }

        public void Set3DPos(GameObject go, Rigidbody rigidbody)
        {
            if (instance.isValid())
            {
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(go, rigidbody));
                //if(go.transform.position == CFEngine.EngineUtility.Far_Far_Away)
                //{
                //    Debug.Log(go.name + " position is = " + go.transform.position + " the sound is far far away!");
                //}
                //RuntimeManager.AttachInstanceToGameObject(instance, go.transform, rigidbody);
            }
        }

        public void Start()
        {
            RuntimeUtils.EnforceLibraryOrder();
        }

        void Lookup()
        {
            eventDescription = RuntimeManager.GetEventDescription(Event);
        }

        public void Play(GameObject go, Rigidbody rigidbody)
        {
            if (String.IsNullOrEmpty(Event))
            {
                return;
            }

            if (!eventDescription.isValid())
            {
                Lookup();
            }

            bool isOneshot = false;
            //if (!Event.StartsWith("snapshot", StringComparison.CurrentCultureIgnoreCase))
            if(!CFEngine.SimpleTools.StartsWith(Event, "snapshot"))
            {
                eventDescription.isOneshot(out isOneshot);
            }
            bool is3D;
            eventDescription.is3D(out is3D);

            if (!instance.isValid())
            {
                instance.clearHandle();
            }

            // Let previous oneshot instances play out
            if (instance.isValid())
            {
                instance.release();
                instance.clearHandle();
            }

            if (!instance.isValid())
            {
                eventDescription.createInstance(out instance);
                //FMOD.Studio.EventInstance[] array;
                //eventDescription.getInstanceList(out array);
                //if(array != null && Event.Contains("Music/Sky_Island/BGM"))
                //{
                //    for (int i = 0; i < array.Length; ++i)
                //    {
                //        Debug.LogError("instance " + i + "  hashcode=" + array[i].GetHashCode());
                //    }
                //    Debug.LogError("arrary count =" + array.Length);
                //}

                //Debug.LogError("instance hashcode=" + instance.GetHashCode());

                // Only want to update if we need to set 3D attributes
                if (is3D && go != null)
                {
                    Set3DPos(go, rigidbody);
                }
                else if (is3D && Pos != Vector3.zero)
                {
                    FMOD.ATTRIBUTES_3D attr = Pos.To3DAttributes();
                    instance.set3DAttributes(attr);
                }

                if (Params != null)
                {
                    for (int i = 0; i < Params.Count; ++i)
                    {
                        instance.setParameterByName(Params[i].Name, Params[i].Value);
                    }
                }
            }

            FMOD.RESULT result = instance.start();

            // hasTriggered = true;

        }

        public void Stop(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
        {
            if (instance.isValid())
            {
                bool isPause = false;
                instance.getPaused(out isPause);

                if (isPause) //如果是pause状态，设置为非pause状态，然后才能释放，需要连fmod profiler查看
                {
                    instance.setPaused(false);
                }

                //instance.stop(AllowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.stop(stopMode);
                instance.release();
                instance.clearHandle();
            }
        }

        public void SetPause(bool isPause)
        {
            if (instance.isValid())
            {
                instance.setPaused(isPause);
                //Debug.LogError("setpause instancehashcode=" + instance.GetHashCode());
                FMOD.Studio.EventInstance[] array;
                eventDescription.getInstanceList(out array);
                if (array != null)
                {
                    for (int i = 0; i < array.Length; ++i)
                    {
                        if (array[i].isValid()) array[i].setPaused(isPause);
                        //Debug.LogError("pauseevent=" + Event);
                    }
                }
            }
        }

        public FMOD.Studio.PLAYBACK_STATE GetEventState()
        {
            if (instance.isValid())
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                instance.getPlaybackState(out playbackState);
                return playbackState;
            }
            return FMOD.Studio.PLAYBACK_STATE.STOPPED;
        }

        public bool IsPlaying()
        {
            if (instance.isValid())
            {
                FMOD.Studio.PLAYBACK_STATE playbackState;
                instance.getPlaybackState(out playbackState);
                //Debug.LogError("event state = " + playbackState +"  "+ (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED));
                return (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);
            }
            //Debug.LogError("event is in Valid");
            return false;
        }

        public void Update3DAttributes(Vector3 pos)
        {
            Pos = pos;
            FMOD.ATTRIBUTES_3D attr = Pos.To3DAttributes();
            if (instance.isValid())
            {
                instance.set3DAttributes(attr);
            }
        }

        public void CacheEventInstance()
        {
            if (String.IsNullOrEmpty(Event))
            {
                return;
            }

            Lookup();
        }

        public void SetParameter(string name, float value)
        {
            if (instance.isValid())
            {
                instance.setParameterByName(name, value);
            }
        }

        public void GetParameter(string name, out float value)
        {
            value = 0;
            if (instance.isValid())
            {
                instance.getParameterByName(name, out value);
            }
        }
    }
}
using System.Runtime.InteropServices;
using UnityEngine.CFUI;
using UnityEngine.CFEventSystems;
using System;
using System.Collections;
using CFEngine;
// #if AVPRO
// using RenderHeads.Media.AVProVideo;
// #else
using UnityEngine.Video;
// #endif
using UnityEngine;
using System.IO;

public class XMediaController : UIBehaviour, IMediaController
{
    [SerializeField]
    public bool fullscreen = false;

    [SerializeField]
    public bool referenceRosolution = false;

    [SerializeField]
    public bool scaleAndCrol = false;
    /*
    #if AVPRO
        [NonSerialized]
        private Action<IMediaController> m_complete = null;

        [NonSerialized]
        private MediaPlayer m_mediaPlayer = null;
        private float m_audioVolueme = 1f;

        private DisplayUGUI m_mediaDisplay = null;

        protected override void Awake()
        {
            base.Awake();
            if (m_mediaPlayer == null)
            {
                m_mediaPlayer =gameObject.GetComponent<MediaPlayer>();
                if (m_mediaPlayer == null) m_mediaPlayer = gameObject.AddComponent<MediaPlayer>();
            }

            if(m_mediaDisplay == null)
            {
                m_mediaDisplay = gameObject.GetComponent<DisplayUGUI>();
                if (m_mediaDisplay == null) m_mediaDisplay = gameObject.AddComponent<DisplayUGUI>();

                m_mediaDisplay.ScaleMode  = ScaleMode.ScaleAndCrop;
            }
            m_mediaDisplay.Player = m_mediaPlayer;
            m_mediaPlayer.Events.AddListener(OnMediaHandler);
        }
        protected override void Start()
        {
            base.Start();
            if (m_mediaPlayer != null) return;
            m_audioVolueme = m_mediaPlayer.AudioVolume;
        }

        public void Play(string url, bool isLoop = false, Action<IMediaController> call = null)
        {
            Debug.Log("UnityEngine persistentDataPath:"+ Application.persistentDataPath );
            Debug.Log("UnityEngine streamingAssetsPath" + Application.streamingAssetsPath);
             m_complete = call;
            if(m_mediaPlayer == null)
            {
                Debug.LogError("not config MediaPlayer");
                OnComplete();
                return;
            }

            MediaPathType type = MediaPathType.AbsolutePathOrURL;
            bool absoluteflag = url.ToLower().Contains("http:") || url.ToLower().Contains("https:") || url.Contains("OuterPackage") || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor; 
            if(!absoluteflag){
                int index = url.IndexOf("bundleres");
                if(index > 0){
                    url = url.Substring(index);
                }  
                type =  MediaPathType.RelativeToStreamingAssetsFolder;

            }
            Debug.Log("UnityEngine url: "+ url +  "  " + type );
            m_mediaPlayer.OpenMedia(type, url, true);
            m_mediaPlayer.Loop = isLoop;
            EngineContext.TimelineIsPlaying = true;
            m_mediaDisplay.color = Color.black;
        }

        public void SetPause()
        {
            if (m_mediaPlayer.Control.IsPlaying())
            {
                m_mediaPlayer.Pause();
            }
        }

        public void Play()
        {
            if(m_mediaPlayer != null && m_mediaPlayer.Control.IsPaused())
            {
                m_mediaPlayer.Control.Play();
            }
        }

        public void Stop()
        {
            EngineContext.TimelineIsPlaying = false;
            if (m_mediaPlayer != null && m_mediaPlayer.Control != null)
            {
                m_mediaPlayer.Control.Stop();
                m_mediaPlayer.Control.CloseMedia();
            }
        }

        private void OnMediaHandler(MediaPlayer media , MediaPlayerEvent.EventType type ,ErrorCode code )
        {
            if(code != ErrorCode.None)
            {
                Debug.LogError("Media Player ErrorCode: "  + media.MediaPath);
                return;
            }
            switch (type)
            {
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    m_mediaDisplay.color = Color.white;
                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    m_mediaDisplay.color = Color.white;
                    break;
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    Stop();
                    OnComplete();
                    return;
            }
        }

        private void OnComplete()
        {

            if (m_complete != null)
            {
                m_complete.Invoke(this);
            }
        }


        protected override void OnDestroy()
        {
            m_complete = null;
            base.OnDestroy();
        }

        public void SetDirectAudioVolume(float volume, ushort trackIndex = 0)
        {
            if(m_mediaPlayer != null)
            {
                m_mediaPlayer.AudioVolume = volume;
            }
        }
    //#if AVPRO
    #else
    */
    private VideoPlayer _videoPlayer;
    private CFRawImage _videoTarget;
    private RenderTexture _videoRender;
    //private AudioSource _audioSource;

    private bool _loop = false;
    private Action<IMediaController> _OnComplated;
    private Action<IMediaController> _ErrorCall;

    private uint step = 0;
    private string m_originalVideoName = string.Empty;
    private IAuido m_audio;


    protected override void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        if (_videoPlayer == null)
        {
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            _videoPlayer.playOnAwake = false;
            _videoPlayer.isLooping = false;
            _videoPlayer.skipOnDrop = true; //AudioSampleProvider buffer overflow
            _videoPlayer.waitForFirstFrame = true;
        }

        _videoPlayer.errorReceived += _ErrorReceived;
        _videoPlayer.prepareCompleted += _PrepareCompleted;
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoTarget = GetComponent<CFRawImage>();
        if (_videoTarget == null) _videoTarget = gameObject.AddComponent<CFRawImage>();
        if (fullscreen)
        {
            _videoRender = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.sRGB);
        }
        else
        {
            RectTransform rt = _videoTarget.GetComponent<RectTransform>();
            if (referenceRosolution)
            {

                int height = 750;
                int width = 16 * height / 9;
                rt.sizeDelta = new Vector2(16 * height / 9, height);
                _videoRender = new RenderTexture(width, height, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.sRGB);

            }
            else
            {
                _videoRender = new RenderTexture((int)rt.sizeDelta.x, (int)rt.sizeDelta.y, 0, RenderTextureFormat.RGB111110Float, RenderTextureReadWrite.sRGB);
            }
        }

        _videoRender.autoGenerateMips = true;
        _videoRender.Create();
        _videoTarget.texture = _videoRender;
        _videoPlayer.targetTexture = _videoRender;

        if (m_audio == null)
        {
            m_audio = EngineUtility.GetIAudio();
        }
    }

    public void SetDirectAudioVolume(float volume, ushort trackIndex = 0)
    {
        if (_videoPlayer != null) _videoPlayer.SetDirectAudioVolume(trackIndex, volume);
    }

    protected override void OnDestroy()
    {
        _ErrorCall = null;
        _OnComplated = null;
        if (_videoRender != null)
        {
            _videoRender.Release();
            _videoRender = null;
        }

        if (_videoPlayer != null)
        {
            //_videoPlayer.loopPointReached -= _EndReachedCall;
            _videoPlayer.errorReceived -= _ErrorReceived;
            _videoPlayer.prepareCompleted -= _PrepareCompleted;
        }
        //_audioSource = null;
        _videoPlayer = null;
        _videoTarget = null;

        ReturnMp4Audio();
        base.OnDestroy();
    }

    public void ClearOutRenderTexture(RenderTexture renderTexture)
    {
        if (renderTexture == null) return;
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = rt;

    }

    private void _PrepareCompleted(VideoPlayer player)
    {
        CFEngine.DebugLog.AddGreenLog("_PrepareCompleted:" + player.url);
        _videoPlayer.isLooping = _loop;
        _videoPlayer.Play();
        step++;

        if (!string.IsNullOrEmpty(m_originalVideoName))
        {
            string eventName = m_originalVideoName;
            int index = m_originalVideoName.LastIndexOf(".");
            if (index >= 0)
            {
                eventName = m_originalVideoName.Substring(0, index);
            }
            eventName = "event:/MP4/" + eventName;
            PlayMp4Audio(eventName);
        }
    }

    private void SetVideoRenderTarget()
    {
        if (_videoTarget != null && _videoTarget.texture != _videoRender)
        {
            _videoTarget.texture = _videoRender;
            _videoPlayer.targetTexture = _videoRender;
        }
    }

    public void Play(string url, bool isLoop = false, Action<IMediaController> call = null, Action<IMediaController> error = null)
    {

        if (string.IsNullOrEmpty(url)) return;
        if (!CFUICallbackRegister.MP4)
        {
            if (!isLoop)
            {
                call?.Invoke(this);
            }
            return;
        }
        if (_videoPlayer.isPlaying) return;
        step = 0;

        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = url;
        _loop = isLoop;
        _ErrorCall = error;
        _OnComplated = call;
        SetVideoRenderTarget();
        ClearOutRenderTexture(_videoRender);
        Play();
        EngineContext.TimelineIsPlaying = true;

        if (this.gameObject.activeInHierarchy)
            StartCoroutine(Timer());

    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(0.5f);
        if (_videoTarget != null)
            _videoTarget.color = Color.white;
    }

    void Update()
    {

        if (!_loop && step > 1 && _videoPlayer != null && _videoPlayer.enabled)
        {
            if (_videoPlayer.isPlaying) return;
            _EndReachedCall(_videoPlayer);
        }
    }

    public bool IsRunning
    {
        get
        {
            return _videoPlayer != null && _videoPlayer.isPrepared && _videoPlayer.isPlaying;
        }
    }

    private void Play()
    {
        try
        {
            _videoPlayer.enabled = true;
            _videoTarget.enabled = true;
            _videoPlayer.Prepare();
            step++;
        }
        catch (Exception e)
        {
            CFEngine.DebugLog.AddGreenLog(e.ToString());
            Stop();

        }
    }

    public void SetPause()
    {
        CFEngine.DebugLog.AddGreenLog(" VideoPlayer SetPause:");
        if (_videoPlayer == null || !_videoPlayer.isPrepared || !_videoPlayer.isPlaying) return;
        _videoPlayer.Pause();
        step--;
        PauseMp4Audio(true);
    }

    public void SetContinue()
    {
        CFEngine.DebugLog.AddGreenLog(" VideoPlayer SetContinue:");
        if (_videoPlayer == null || !_videoPlayer.isPrepared || _videoPlayer.isPlaying) return;
        _videoPlayer.enabled = true;
        _videoTarget.enabled = true;
        _videoPlayer.Play();
        step++;
        PauseMp4Audio(false);
    }

    public void Stop()
    {
        step = 0;
        EngineContext.TimelineIsPlaying = false;
        ClearOutRenderTexture(_videoRender);
        if (!CFUICallbackRegister.MP4) return;
        if (_videoPlayer.isPlaying || _videoPlayer.isPrepared)
        {
            _videoPlayer.Stop();
        }
        _videoPlayer.enabled = false;
        _videoTarget.enabled = false;
        StopMp4Audio();
    }

    private void _ErrorReceived(VideoPlayer source, string message)
    {

        if (_videoPlayer != null)
        {
            _videoPlayer.Stop();
            _videoPlayer.enabled = false;
        }
        if (_ErrorCall != null)
        {
            _ErrorCall.Invoke(this);
            _ErrorCall = null;
        }
    }



    private void _EndReachedCall(VideoPlayer player)
    {
        EngineContext.TimelineIsPlaying = false;
        CFEngine.DebugLog.AddGreenLog("_EndReachedCall");
        if (!_loop)
        {
            Stop();
            if (_OnComplated != null)
            {
                _OnComplated.Invoke(this);
                _OnComplated = null;
            }
        }
    }

    protected override void OnDisable()
    {
        Reset2Black();
        StopMp4Audio();
        base.OnDisable();
    }

    public void Reset2Black()
    {
        _videoTarget.color = new Color(0.03f, 0.03f, 0.03f, 1f);
    }

    public void SetOriginalVideoName(string originalVideoName)
    {
        m_originalVideoName = originalVideoName;
    }

    private void PlayMp4Audio(string eventName)
    {
        if (m_audio != null)
        {
            m_audio.PlayMp4Audio(eventName);
            CFEngine.DebugLog.AddGreenLog("XMediaControllerPlayMp4Audio event=" + eventName);
        }
    }

    private void StopMp4Audio()
    {
        if (m_audio != null)
        {
            m_audio.StopMp4Audio();
            CFEngine.DebugLog.AddGreenLog("XMediaControllerStopMp4Audio");
        }
    }

    private void PauseMp4Audio(bool isPause)
    {
        if (m_audio != null)
        {
            m_audio.PauseMp4Audio(isPause);
            CFEngine.DebugLog.AddGreenLog("XMediaControllerPauseMp4Audio");
        }
    }

    private void ReturnMp4Audio()
    {
        if (m_audio != null)
        {
            m_audio.StopMp4Audio();
            EngineUtility.returnAudio(m_audio);
            m_audio = null;
            CFEngine.DebugLog.AddGreenLog("XMediaControllerReturnMp4Audio");
        }
    }
}
// #endif

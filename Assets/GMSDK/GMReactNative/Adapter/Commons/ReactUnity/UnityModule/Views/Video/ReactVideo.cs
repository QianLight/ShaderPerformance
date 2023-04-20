using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GSDK.RNU {
    public class ReactVideo: SimpleBaseView 
    {
        private GameObject realGameObject;

        private VideoPlayer videoPlayer;
        private AudioSource audioSource;

        private RawImage rawImage;
        private VideoEventHelper eventHelper;

        private RenderTexture rt;

        private int freshRate = 10;
        private int freshIndx = 0;

        public ReactVideo(string name)
        {
            realGameObject = new GameObject(name);

            videoPlayer = realGameObject.AddComponent<VideoPlayer>();
            audioSource = realGameObject.AddComponent<AudioSource>();
            rawImage = realGameObject.AddComponent<RawImage>();
            
            // rt = new RenderTexture(1024, 1024, 0);
            // SetVideoPlayerTargetTexture();

            InitVideoPlayer();

            InitVideoEvent();
            
            InitEventHelper();
        }

        public override GameObject GetGameObject() {
            return realGameObject;
        }

        public void SetFreshRate(int rate)
        {
            freshRate = rate;
        }

        private void InitVideoPlayer()
        {
            SetVideoPlayerType();
            SetVideoPlayerOnAwake();
            SetVideoPlayerRenderMode();
            // SetVideoPlayerWaitFirstFrame();
            // SetVideoPlayerTargetTexture();
            SetVideoPlayerControTracks();
            SetVideoPlayerAudio();
        }

        private void InitVideoEvent()
        {
            if (videoPlayer == null)
            {
                return;
            }
            videoPlayer.started += source =>
            {
                OnEvent(ReactVideoManager.sStarEventName);
            };
            videoPlayer.errorReceived += (source, message) =>
            {
                OnEvent(ReactVideoManager.sErrorEventName, message);
            };
            videoPlayer.seekCompleted += source =>
            {
                OnEvent(ReactVideoManager.sSeekedEventName);
            };
            videoPlayer.loopPointReached += source =>
            {
                OnEvent(ReactVideoManager.sLoopPointEventName);
            };
        }

        private void InitEventHelper()
        {
            eventHelper = realGameObject.AddComponent<VideoEventHelper>();
            eventHelper.InitEventHelper(this);
        }
        
        /**
         * 资源类型，0 Video Clip；1 URL
         * 因为 Video Clip 需要额外读取视频资源文件并播放，且内部参数不一致
         * 暂时只支持 URL 模式
        **/
        private void SetVideoPlayerType(int type = 1)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.source = (VideoSource)type;
        }
        
        /**
         * 设置视频播放的 音频
         */
        private void SetVideoPlayerAudio()
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }

            if (audioSource == null)
            {
                Util.Log("audioSource is null, return");
                return;
            }
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        
        /**
         * 设置渲染模式，当前仅支持 RenderTexture，其他复杂模式需要材质、相机等
         * renderTexture 在初始化时创建
         */
        private void SetVideoPlayerTargetTexture()
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.targetTexture = rt;
            rawImage.texture = rt;
        }
        
        
        /**
         * 播放前添加该配置，告诉 Unity 播放视频，音频才会跟上
         */
        private void SetVideoPlayerControTracks(int value = 1)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.controlledAudioTrackCount = (ushort)value;
        }

        private void SetVideoPlayerRenderMode(int RTType = 2)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.renderMode = (VideoRenderMode)RTType;
        }
        
        
        /**
         * 关闭加载完资源后直接播放，因为 RU 设置 url 要晚一步
         */
        private void SetVideoPlayerOnAwake(bool isPlayOnAwake = false)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.playOnAwake = isPlayOnAwake;
            audioSource.playOnAwake = isPlayOnAwake;
        }

        /**
         * 是否等第一帧加载后开始播放，仅对 playOnAwake 为 true 时生效，默认开启
         */
        private void SetVideoPlayerWaitFirstFrame(bool isWait = true)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.waitForFirstFrame = isWait;
        }
        
        public bool IsPlayingStatus()
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return false;
            }
            return videoPlayer.isPlaying;
        }
        
        /**
         * URL 地址，可以是本地地址，或者网络 url 地址
        **/
        public void SetVideoPlayerUrl(string url)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.url = url;
            Util.Log("videoPlayer url is: " + videoPlayer.url);

        }

        /**
         * 是否循环播放，默认否
         */
        public void SetVideoPlayerLoop(bool isLoop)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.isLooping = isLoop;
            if (audioSource == null)
            {
                return;
            }
            audioSource.loop = isLoop;
        }

        /**
         * 设置播放速度，范围在 0-10，默认为 1
         */
        public void SetVideoPlayerSpeed(double speed)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }

            if (videoPlayer.canSetPlaybackSpeed)
            {
                videoPlayer.playbackSpeed = (float) speed;
            }
        }


        /**
         * 设置播放的缩放模式，
         * 0 NoScaling，无缩放模式
         * 1 FitVertically，高适配模式
         * 2 FitHorizontally，宽适配模式
         * 3 FitInside，内部适配，宽的一边黑色区域填充
         * 4 FitOutside，外部适配
         * 5 Stretch，拉伸填充整个屏幕，默认模式
         */
        public void SetVideoPlayerARatio(int type)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            
            videoPlayer.aspectRatio = (VideoAspectRatio) type;
        }

        /**
         * 设置视频的播放声音相关
         * 0 None，无声音
         * 1 AudioSource，视频原声音
         * 2 Direct，视频声音放到平台硬件播放
         */
        public void SetVideoPlayerOutputMode(int mode)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.audioOutputMode = (VideoAudioOutputMode) mode;
        }


        /**
         * 设置视频音量，范围在 0-1 之间
         */
        public void SetVolum(double volume)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.SetDirectAudioVolume(0, (float)volume);
            if (audioSource == null)
            {
                return;
            }
            audioSource.volume = (float)volume;
        }

        public void SetMute(bool isMuted)
        {
            if (videoPlayer == null)
            {
                Util.Log("videoPlayer is null, return");
                return;
            }
            videoPlayer.SetDirectAudioMute(0, isMuted);
            audioSource.mute = isMuted;
        }


        public void VideoPlay()
        {
            if (!videoPlayer.isPrepared)
            {            
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += data =>
                {
                    videoPlayer.Play();
                };
            }
            else
            {
                videoPlayer.Play();
            }
        }

        public void VideoPause()
        {
            videoPlayer.Pause();
        }

        public void VideoStop()
        {
            videoPlayer.Stop();
        }

        public void VideoSeek(double time)
        {
            if (videoPlayer.canSetTime)
            {
                videoPlayer.time = (float)time;
            }
        }


        public void PlayProgress()
        {
            if (!videoPlayer.isPlaying)
            {
                return;
            }

            freshIndx = (freshIndx + 1) % freshRate;
            if (freshIndx != 0)
            {
                // Util.Log("-------freshIndex:{0}----", freshIndx);
                return;
            }

            float total = videoPlayer.frameCount / videoPlayer.frameRate;
            // float current = videoPlayer.frame / videoPlayer.frameRate;
            // Util.Log("----------videoPlayer.time:{0}----", videoPlayer.time);
            float current = (float)videoPlayer.time;

            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(ReactVideoManager.sOnProgress);
            args.Add(new Hashtable
            {
                {"currentTime", current},
                {"playableDuration", total}
            });

            // Util.Log("current Time {0}, total Time {1}", current, total);
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }
        public void OnEvent(string eventName, string value = null)
        {
            ArrayList args = new ArrayList();
            args.Add(GetId());
            args.Add(eventName);
            if (!String.IsNullOrEmpty(value))
            {
                args.Add(new Hashtable
                {
                    {"value", value}
                });
            }

            Util.Log("OnEvent eventName {0}", eventName);
            RNUMainCore.CallJSFunction("RCTEventEmitter", "receiveEvent", args);
        }


        public override void SetLayout(int x, int y, int width, int height)
        {
            base.SetLayout(x, y, width, height);

            if (rt == null)
            {
                rt = new RenderTexture(width * 2, height * 2, 0);
                SetVideoPlayerTargetTexture();
            }
            else if (rt.width != width*2 || rt.height != height*2) 
            {
                rt.Release();
                rt = new RenderTexture(width * 2, height * 2, 0);
                SetVideoPlayerTargetTexture();
            }
        }


        public override void Destroy()
        {
            base.Destroy();
            if (rt != null)
            {
                rt.Release();
                GameObject.Destroy(rt);
                rt = null;
            }
        }
    }
}

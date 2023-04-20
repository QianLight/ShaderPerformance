using System.Collections;
using UnityEngine;

namespace GSDK.RNU {
    public class ReactVideoManager: BaseViewManager
    {
        public static string sReactVideoName = "RNUVideo";

        
        public static string sStarEventName = "onPlay"; 
        public static string sErrorEventName = "onError"; 
        public static string sSeekedEventName = "onSeek"; 
        public static string sLoopPointEventName = "onEnd";

        public static string sOnProgress = "onProgress";
        
        override public string GetName() {
            return sReactVideoName;
        }

        protected override BaseView createViewInstance() {
            return new ReactVideo(GetName());
        }
        
        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable
            {
                {sOnProgress, new Hashtable{{sRegistration, sOnProgress}}},
                {sStarEventName, new Hashtable{{sRegistration, sStarEventName}}},
                {sErrorEventName, new Hashtable{{sRegistration, sErrorEventName}}},
                {sSeekedEventName, new Hashtable{{sRegistration, sSeekedEventName}}},
                {sLoopPointEventName, new Hashtable{{sRegistration, sLoopPointEventName}}}

            };
        }
        
        [ReactProp(name = "rate")]
        public void setVideoSpeed(ReactVideo video, float speed)
        {
            if (speed < 0 || speed > 10)
            {
                Util.Log("rate {0} is not in the right range, return", speed);
                return;
            }
            video.SetVideoPlayerSpeed(speed);
        }
        
        [ReactProp(name = "volume")]
        public void setVideoVolume(ReactVideo video, float volume)
        {
            if (volume < 0 || volume > 1)
            {
                Util.Log("volume {0} is not in the right range, return", volume);
                return;
            }
            video.SetVolum(volume);
        }
        
        
        [ReactProp(name = "muted")]
        public void setVideoMuted(ReactVideo video, bool isMuted)
        {
            video.SetMute(isMuted);
        }
        
        [ReactProp(name = "repeat")]
        public void setVideoRepeat(ReactVideo video, bool loop)
        {
            video.SetVideoPlayerLoop(loop);
        }
        
        [ReactProp(name = "outPut")]
        public void setVideoOutPut(ReactVideo video, int outPut)
        {
            if (outPut < 0 || outPut > 2)
            {
                Util.Log("outPut {0} is not in the right range, return", outPut);
                return;
            }
            video.SetVideoPlayerOutputMode(outPut);
        }
        
        [ReactProp(name = "resizeMode")]
        public void setVideoARatio(ReactVideo video, int type)
        {
            if (type < 0 || type > 5)
            {
                Util.Log("resizeMode {0} is not in the right range, return", type);
                return;
            }
            video.SetVideoPlayerARatio(type);
        }
        
        
        [ReactProp(name = "source")]
        public void setVideoSource(ReactVideo video, string source)
        {
            video.SetVideoPlayerUrl(source);
        }
        
        [ReactProp(name = "freshRate")]
        public void setFreshRate(ReactVideo video, int value)
        {
            if (value < 1 || value > 30)
            {
                Util.Log("freshRate {0} is not in the right range, return", value);
                return;
            }
            video.SetFreshRate(value);
        }
        

        [ReactCommand]
        public void seek(ReactVideo video, double time)
        {
            video.VideoSeek(time);
        }
        

        [ReactCommand]
        public void play(ReactVideo video)
        {
            video.VideoPlay();
        }
        
        [ReactCommand]
        public void pause(ReactVideo video)
        {
            video.VideoPause();
        }
        
        [ReactCommand]
        public void stop(ReactVideo video)
        {
            video.VideoStop();
        }

        [ReactCommand]
        public void isPlaying(ReactVideo video, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(GetPlayingAndReturn(video, promise));
        }
        private IEnumerator GetPlayingAndReturn(ReactVideo video, Promise promise)
        {
            yield return null;
            promise.Resolve(video.IsPlayingStatus());
        }
        
    }
}
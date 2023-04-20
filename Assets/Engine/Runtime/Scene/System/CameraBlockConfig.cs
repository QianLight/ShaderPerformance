using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class CameraBlockConfig
    {
        [Header("进入延迟时间")]
        public float enterDelayTime = 25.0f / 60.0f;
        [Header("淡入持续时间")]
        public float enterLerpTime = 35.0f / 60.0f;
        [Header("推出延迟时间")]
        public float leaveDelayTime = 7.0f / 60.0f;
        [Header("透明度")]
        public float minTransparency = 0.7f;

        public CameraBlockConfig(float enterDelayTime, float enterLerpTime, float leaveDelayTime, float minTransparency)
        {
            this.enterDelayTime = enterDelayTime;
            this.enterLerpTime = enterLerpTime;
            this.leaveDelayTime = leaveDelayTime;
            this.minTransparency = minTransparency;
        }

        public CameraBlockConfig()
        {
        }
    }
}
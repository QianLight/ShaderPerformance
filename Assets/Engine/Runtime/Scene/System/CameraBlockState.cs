using System;
using UnityEngine;

namespace CFEngine
{

    public class CameraBlockState
    {
        public enum OcclusionStage
        {
            NoOcclusion,
            EnterDelay,
            EnterLerp,
            Last,
            LeaveDelay,
        }



        public OcclusionStage stage;

        private float mOcclusiveeTimer = 0f;
        private float mTransparency = 1f;
        private CameraBlockConfig mConfig;
        private Action<float, OcclusionStage> mApplyCallback;

        public void StartOccluding(CameraBlockConfig config)
        {
            mConfig = config;
            mOcclusiveeTimer = Time.realtimeSinceStartup;
            stage = OcclusionStage.EnterDelay;
        }

        public void EndOccluding()
        {
            if (stage == OcclusionStage.EnterDelay)
            {
                mOcclusiveeTimer = Time.realtimeSinceStartup;
                stage = OcclusionStage.NoOcclusion;
            }
            else
            {
                mOcclusiveeTimer = Time.realtimeSinceStartup;
                stage = OcclusionStage.LeaveDelay;
            }
        }

        public bool Update()
        {
            float currentTime = Time.realtimeSinceStartup;
            float lastTime = currentTime - mOcclusiveeTimer;
            switch (stage)
            {
                case OcclusionStage.NoOcclusion:
                    return false; //返回
                case OcclusionStage.EnterDelay:
                    if (lastTime > mConfig.enterDelayTime)
                    {
                        stage = OcclusionStage.EnterLerp;
                        mOcclusiveeTimer = currentTime;
                    }

                    mTransparency = 1f;
                    break;
                case OcclusionStage.EnterLerp:
                    if (lastTime > mConfig.enterLerpTime)
                    {
                        stage = OcclusionStage.Last;
                        mOcclusiveeTimer = currentTime;
                    }

                    // float percent = lastTime / mConfig.enterLerpTime;
                    // percent *= percent;
                    // mTransparency = Mathf.Lerp(1f, mConfig.minTransparency, percent);
                    mTransparency = 1 - EaseInOutSine(lastTime, mConfig.enterLerpTime, 0, 1 - mConfig.minTransparency);
                    mTransparency = Mathf.Max(mTransparency, mConfig.minTransparency);
                    break;
                case OcclusionStage.Last:
                    mTransparency = mConfig.minTransparency;
                    break;
                case OcclusionStage.LeaveDelay:
                    if (lastTime > mConfig.leaveDelayTime)
                    {
                        
                        mTransparency = 1.0f;
                        stage = OcclusionStage.NoOcclusion;
                        mOcclusiveeTimer = currentTime;
                    }

                    break;
            }

            mApplyCallback?.Invoke(mTransparency,stage);

            return stage != OcclusionStage.NoOcclusion;
        }

        private float EaseInOutSine(float currentTime, float duration, float startValue, float finalValue)
        {
            if ((currentTime /= (duration * 0.5f)) < 1)
            {
                return finalValue * 0.5f * (Mathf.Sin(Mathf.PI * currentTime * 0.5f)) + startValue;
            }

            return -finalValue * 0.5f * (Mathf.Cos(Mathf.PI * --currentTime * 0.5f) - 2) + startValue;
        }

        public void Reset()
        {
            mConfig = null;
            stage = OcclusionStage.NoOcclusion;
            mOcclusiveeTimer = 0f;
            mTransparency = 1f;
            mApplyCallback = null;
        }

        public void Init(Action<float, OcclusionStage> applyCallback)
        {
            mApplyCallback = applyCallback;
        }
    }
}
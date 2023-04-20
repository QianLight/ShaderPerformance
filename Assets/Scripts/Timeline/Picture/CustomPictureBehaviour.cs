using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif

public class CustomPictureBehaviour : DirectBaseBehaviour
{
    private CustomPictureAsset PicAsset
    {
        get { return asset as CustomPictureAsset; }
    }

    private TimelineClip SelfClip
    {
        get { return PicAsset.m_clip; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        TimelinePictureUI.singleton.SetCurrentPlayingClip(SelfClip);
        TimelinePictureUI.singleton.Show(PicAsset);
        TimelinePictureUI.singleton.PlayShake();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        TimelineClip currentPlayingClip = TimelinePictureUI.singleton.GetCurrentPlayingClip();
        if (SelfClip == currentPlayingClip)
        {
            TimelinePictureUI.singleton.Hide();
            TimelinePictureUI.singleton.SetCurrentPlayingClip(null);
        }
        TimelinePictureUI.singleton.StopShake();
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.ProcessFrame(playable, info, playerData);

        if (PicAsset.m_customPictureInfos == null || PicAsset.m_customPictureInfos.Count == 0) return;
        int count = PicAsset.m_customPictureInfos.Count;

        CustomPictureInfo picInfo = null;
        if (PicAsset.m_pictureType == EPictureType.BigPicture)
        {
            if (count == 1)
            {
                picInfo = PicAsset.m_customPictureInfos[0];
                TweenAlpha(playable, picInfo, 0);
                TweenPositionAndScale(playable, picInfo, 0);
            }
            else if (count == 2)
            {
                picInfo = PicAsset.m_customPictureInfos[0];
                TweenAlpha(playable, picInfo, 0);
                TweenPositionAndScale(playable, picInfo, 0);

                picInfo = PicAsset.m_customPictureInfos[1];
                TweenAlpha(playable, picInfo, 1);
                TweenPositionAndScale(playable, picInfo, 1);
            }
        }
        else
        {
            if (count == 1)
            {
                picInfo = PicAsset.m_customPictureInfos[0];
                TweenAlpha(playable, picInfo, 0);
                TweenPositionAndScale(playable, picInfo, 2);
            }
        }
    }

    private void TweenAlpha(Playable playable, CustomPictureInfo info, int index)
    {
        double now = RTimeline.singleton.Director.time;
        double s1 = PicAsset.m_clip.start;
        double e1 = PicAsset.m_clip.start + info.m_startToMiddleAlphaDuration;

        double s2 = PicAsset.m_clip.end - info.m_middleToEndAlphaDuration;
        double e2 = PicAsset.m_clip.end;

        if (now >= s1 && now <= e1)
        {
            TweenAlpha((float)(now - s1), info.m_startToMiddleAlphaDuration, info.m_startAlpha, info.m_middleAlpha, index);
        }
        else if (now > e1 && now < s2)
        {
            TimelinePictureUI.singleton.SetAlpha(index, info.m_middleAlpha);
        }
        else
        {
            TweenAlpha((float)(now - s2), info.m_middleToEndAlphaDuration, info.m_middleAlpha, info.m_endAlpha, index);
        }
    }

    private void TweenPositionAndScale(Playable playable, CustomPictureInfo info, int index)
    {
        double now = playable.GetTime();
        float normalisedTime = (float)(now / playable.GetDuration());

        float tweenProgress = EvaluateCurrentCurve(info.m_positionTweenType, normalisedTime);
        Vector2 lerpValue = Vector2.Lerp(info.m_startPosition, info.m_endPosition, tweenProgress);
        TimelinePictureUI.singleton.SetPicPosition(index, lerpValue);

        tweenProgress = EvaluateCurrentCurve(info.m_scaleTweenType, normalisedTime);
        float lerpValue2 = Mathf.Lerp(info.m_startScale, info.m_endScale, tweenProgress);
        TimelinePictureUI.singleton.SetPicScale(index, lerpValue2);
    }

    /// <summary>
    /// alpha从0变到指定的值
    /// </summary>
    private void TweenAlpha(float gap, float duration, float startAlpha, float endAlpha, int index)
    {
        if (duration <= 0) duration = 1;
        double percent = gap / duration;
        float alpha = Mathf.Lerp(startAlpha, endAlpha, (float)percent);
        TimelinePictureUI.singleton.SetAlpha(index, alpha);
    }


    public float EvaluateCurrentCurve(CustomPictureAsset.TweenType tweenType, float time)
    {
        if (tweenType == CustomPictureAsset.TweenType.Custom && !IsCustomCurveNormalised())
        {
            Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
            return 0f;
        }

        switch (tweenType)
        {
            case CustomPictureAsset.TweenType.Linear:
                return PicAsset.m_LinearCurve.Evaluate(time);
            case CustomPictureAsset.TweenType.Deceleration:
                return PicAsset.m_DecelerationCurve.Evaluate(time);
            case CustomPictureAsset.TweenType.Harmonic:
                return PicAsset.m_HarmonicCurve.Evaluate(time);
            default:
                return PicAsset.customCurve.Evaluate(time);
        }
    }

    bool IsCustomCurveNormalised()
    {
        if (!Mathf.Approximately(PicAsset.customCurve[0].time, 0f))
            return false;

        if (!Mathf.Approximately(PicAsset.customCurve[0].value, 0f))
            return false;

        if (!Mathf.Approximately(PicAsset.customCurve[PicAsset.customCurve.length - 1].time, 1f))
            return false;

        return Mathf.Approximately(PicAsset.customCurve[PicAsset.customCurve.length - 1].value, 1f);
    }
}


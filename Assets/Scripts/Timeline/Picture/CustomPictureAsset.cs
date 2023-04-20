using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public enum EPictureType
{
    BigPicture,
    SmallPicture
}

public enum EPictureEffect
{
    None,
    OldFilm,
}

[Serializable]
public class CustomPictureInfo
{
    public EPictureEffect m_pictureEffect;                          //图片的效果
    public Vector2 m_startPosition;                                 //初始位置
    public Vector2 m_endPosition;                                   //终止位置
    public CustomPictureAsset.TweenType m_positionTweenType;        //动画类型
    public float m_positionDuration;                                //位置动画时长

    public float m_startScale;                                    //初始缩放
    public float m_endScale;                                      //终止缩放
    public CustomPictureAsset.TweenType m_scaleTweenType;           //动画类型
    public float m_scaleDuration;                                   //缩放动画时长

    public string m_path;                                           //图片路径
    public float m_startAlpha;                                      //大图的初始透明度
    public float m_middleAlpha;                                     //大图的初始透明度
    public float m_endAlpha;                                        //大图的初始透明度
    public float m_startToMiddleAlphaDuration;                      //第一阶段alpha动画时长
    public float m_middleToEndAlphaDuration;                        //第二阶段alpha动画时长
}

public class CustomPictureAsset : DirectBasePlayable<CustomPictureBehaviour>, ITimelineClipAsset
{
    public enum TweenType
    {
        Linear,
        Deceleration,
        Harmonic,
        Custom,
    }

    public EPictureType m_pictureType;
    public bool m_useBigBg = true;
    public bool m_useShake = false;
    public List<CustomPictureInfo> m_customPictureInfos;

    [System.NonSerialized]
    public TimelineClip m_clip;

    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    [HideInInspector]
    public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [HideInInspector]
    public AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [HideInInspector]
    public AnimationCurve m_DecelerationCurve = new AnimationCurve
    (
        new Keyframe(0f, 0f, -k_RightAngleInRads, k_RightAngleInRads),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    [HideInInspector]
    public AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    const float k_RightAngleInRads = Mathf.PI * 0.5f;


    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<CustomPictureBehaviour>.Create(graph, behaviour);
    }
}


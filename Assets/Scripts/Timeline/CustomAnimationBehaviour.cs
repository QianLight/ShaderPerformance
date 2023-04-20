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

public class CustomAnimationBehaviour : DirectBaseBehaviour
{
    public AnimationClip clip;
    public bool m_facialIdle = false;
    private Animator m_animator = null;
    private AnimatorOverrideController m_overrideController = null;
    private FacialExpressionCurve m_facialCurve = null;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (m_animator == null)
        {
            CustomAnimationAsset ca = asset as CustomAnimationAsset;
            if (ca != null && ca.trackAsset != null)
            {
                var go = RTimeline.singleton.Director.GetGenericBinding(ca.trackAsset) as GameObject;

                if (go != null)
                {
                    m_animator = go.GetComponent<Animator>();
                }
                else
                {
                    m_animator = RTimeline.singleton.Director.GetGenericBinding(ca.trackAsset) as Animator;
                }
            }
        }


        if (m_animator != null)
        {
#if UNITY_EDITOR
            bool IsRecording = EditorStateHelper.IsKFrame();
            //bool IsRecording = AnimationMode.InAnimationMode();
            if (!IsRecording && Application.isPlaying)
#endif
            {
                //m_animator.enabled = false;
                InitRuntimeAnimatorController();
                SetAnimationIdleClip();
                SetAnimationFacialIdleClip();
                //m_animator.enabled = true;
            }
        }
    }

    /// <summary>
    /// 重写运行时的AnimatorController
    /// </summary>
    public void InitRuntimeAnimatorController()
    {
        if (m_animator == null) return;
        if (m_overrideController != null) return;

        if (m_animator.runtimeAnimatorController is AnimatorOverrideController)
        {
            m_overrideController = m_animator.runtimeAnimatorController as AnimatorOverrideController;
        }
        else
        {
            if (m_animator.runtimeAnimatorController != null)
            {
                RuntimeAnimatorController myController = m_animator.runtimeAnimatorController;
                AnimatorOverrideController myOverrideController = myController as AnimatorOverrideController;
                if (myOverrideController != null)
                    myController = myOverrideController.runtimeAnimatorController;

                m_overrideController = new AnimatorOverrideController(myController);
                m_animator.runtimeAnimatorController = m_overrideController;
            }
        }
    }

    /// <summary>
    /// 当为表情轨道时，非表情轨道则直接返回if (!m_facialIdle) return;
    /// 当速度为0时，即暂停的时候，走idle状态
    /// 当速度不为0时，即正常播放时，取消idle状态
    /// </summary>
    private void SetAnimationFacialIdleClip()
    {
        if (m_overrideController == null) return;
        if (!m_facialIdle) return;
        PlayableDirector director = RTimeline.singleton.Director;
        if (!director.playableGraph.IsValid()) return;
        double speed = director.playableGraph.GetRootPlayable(0).GetSpeed();
        bool isPause = speed == 0;
        if (isPause)
        {
            if (m_overrideController["TimelineIdle"] != clip) //assign only when clip is different
            {
                m_overrideController["TimelineIdle"] = clip;
            }
            m_animator.SetBool("TimelineIdle", true);
            if (m_animator.gameObject != null && m_facialCurve == null)
            {
                m_facialCurve = m_animator.gameObject.GetComponent<FacialExpressionCurve>();
            }
            if (m_facialCurve != null)
            {
                m_facialCurve.SetToZero(false);
            }
        }
        else
        {
            SetFacialIdleToNull(); //当速度不为0时，即正常播放时，取消idle状态
        }
    }

    /// <summary>
    /// 当clip片段走到最后触发OnBehaviourPause函数
    /// 当速度恢复正常了，即不正常了，则将idle的状态清楚掉
    /// </summary>
    /// <param name="playable"></param>
    /// <param name="info"></param>
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        base.OnBehaviourPause(playable, info);
        PlayableDirector director = RTimeline.singleton.Director;
        if (director == null || !director.playableGraph.IsValid()) return;
        double speed = director.playableGraph.GetRootPlayable(0).GetSpeed();
        bool isPause = speed == 0;
        if (!isPause)
        {
            SetFacialIdleToNull();
        }
    }

    private void SetFacialIdleToNull()
    {
        if (m_overrideController != null && m_overrideController["TimelineIdle"] != null)
        {
            m_overrideController["TimelineIdle"] = null;
        }
        if (m_animator != null && m_animator.gameObject != null && m_facialCurve == null)
        {
            m_facialCurve = m_animator.gameObject.GetComponent<FacialExpressionCurve>();
        }
        if (m_facialCurve != null)
        {
            m_animator.SetBool("TimelineIdle", false);
            m_facialCurve.SetAllWeightToZero();
        }
    }


    private void SetAnimationIdleClip()
    {
        if (m_overrideController == null) return;
        if (m_facialIdle) return;
        if (m_overrideController["Idle"] != clip) //assign only when clip is different
        {
            m_overrideController["Idle"] = clip;
        }
    }
}


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
using CFEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif

class RenderEffectBehaviour : DirectBaseBehaviour
{
    private GameObject m_go;
    private EffectInst m_effectInstance;

#if UNITY_EDITOR
    private EffectPreviewContext m_effectPreviewContext;
#endif

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        RenderEffectAsset effectAsset = asset as RenderEffectAsset;
        uint effectID = effectAsset.m_effectID;
        m_go = RTimeline.singleton.Director.GetGenericBinding(effectAsset.trackAsset) as GameObject;
        if (m_go == null)
        {
            Animator actor = RTimeline.singleton.Director.GetGenericBinding(effectAsset.trackAsset) as Animator;
            if (actor != null) m_go = actor.gameObject;
        }

        if (EngineContext.IsRunning)
        {
            m_effectInstance = RTimeline.singleton.BeginEffect(m_go, effectAsset.m_effectID);
        }
        else
        {
#if UNITY_EDITOR
            if (m_effectPreviewContext == null)
            {
                m_effectPreviewContext = new EffectPreviewContext();
            }
            EffectConfig.InitEffect(m_effectPreviewContext, m_go, EffectConfig.instance);
            EffectConfig.PostInit(m_effectPreviewContext, string.Empty);
            m_effectInstance = RenderEffectSystem.BeginEffect(m_effectPreviewContext, (short)effectAsset.m_effectID);
#endif
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);

        if (EngineContext.IsRunning)
        {
            if (m_effectInstance != null && m_go != null)
            {
                RTimeline.singleton.EndEffect(m_go, m_effectInstance);
                m_effectInstance = null;
            }
        }
        else
        {
#if UNITY_EDITOR
            if (m_effectInstance != null)
            {
                RenderEffectSystem.EndEffect(m_effectInstance);
                m_effectInstance = null;
            }
#endif
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        base.PrepareFrame(playable, info);
#if UNITY_EDITOR
        if (m_effectPreviewContext != null)
        {
            m_effectPreviewContext.Update();
        }
#endif
    }
}


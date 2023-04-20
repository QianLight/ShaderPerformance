using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System;
using System.Collections.Generic;
using UnityEngine.Timeline;
using CFEngine;

public enum FacialClipType
{
    idle = 0,
    A,
    E,
    I,
    O,
    U,

    eyebrow_sad,
    eyebrow_happy,
    eyebrow_angry,

    eye_Squint,
    eye_Earnest,
    eye_Stare,

    mouth_smile,
    mouth_laugh,
    mouth_sad,
    mouth_angry,

    blink

    //surprise,
    //getangry,
    //normal,
    //teether,
    //afraid,
    //nervous,
    //sad,
    //Max,
}

[Serializable]
public class FacialAnimationClip
{
    public FacialClipType m_clipType;
    public AnimationClip m_clip;
    public bool m_fold;
}

[ExecuteInEditMode]
public class FacialExpression : MonoBehaviour
{
    public string m_roleDir = string.Empty;
    public string m_roleName = string.Empty;

    public List<FacialAnimationClip> m_clips;
    public bool activeFlush = false;

    public FacialExpressionCurve m_curve;
    private PlayableGraph m_graph;
    private AnimationLayerMixerPlayable m_mixerPlayable;
    private Animator m_animator;
    private List<AnimationClipPlayable> m_animationClipPlayables;
    private bool m_inited = false;
    public bool m_enableGraph = true;
    private List<AnimationTrack> ch_tracks;
    //public string m_trackName = string.Empty;

    public static string[] m_clipNames = new string[] {
                                "Idle动作",
                                "元音A",
                                "元音E",
                                "元音I",
                                "元音O",
                                "元音U",

                                "眉毛哀伤",
                                "眉毛开心",
                                "眉毛愤怒",

                                "眼睛眯眼",
                                "眼睛认真",
                                "眼睛瞪大",

                                "嘴巴微笑",
                                "嘴巴大笑",
                                "嘴巴哀伤",
                                "嘴巴愤怒",

                                "眨眼",

                                //"愤怒",
                                //"大怒",
                                //"大笑",
                                //"开心",
                                //"伤心",
                                //"笑",
                                //"微笑",
                                //"悲伤",
                                //"严肃",
                                //"惊讶",
                                //"生气",
                                //"眨眼",
                                //"咬牙裂齿",
                                //"害怕",
                                //"紧张",
                                //"难过"

    };


    private void OnEnable()
    {
        Init();
    }

    private Vector3 m_position;
    public void Init()
    {
        if (m_clips == null || m_clips.Count == 0) return;

        if (m_graph.IsValid())
        {
            m_graph.Stop();
            m_graph.Destroy();
        }

        m_graph = PlayableGraph.Create();
        if (m_graph.IsValid())
        {
            int len = m_clips.Count;
            m_mixerPlayable = AnimationLayerMixerPlayable.Create(m_graph, len);
            for (int i = 0; i < len; ++i)
            {
                if (m_clips[i].m_clip == null) continue;
                m_mixerPlayable.SetLayerAdditive((uint)i, i == 0 ? false : true);
                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(m_graph, m_clips[i].m_clip);
                m_graph.Connect(clipPlayable, 0, m_mixerPlayable, i);
                m_mixerPlayable.SetInputWeight(i, i == 0 ? 1 : 0);
            }

            m_animator = GetComponent<Animator>();
            AnimationPlayableOutput animOutput = AnimationPlayableOutput.Create(m_graph, "AnimationOutput", m_animator);
            animOutput.SetSourcePlayable(m_mixerPlayable);
            m_graph.Play();
        }

        ExtractAnimationTrack();
    }

    private void ExtractAnimationTrack()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            ch_tracks = new List<AnimationTrack>();
            if (RTimeline.singleton.Director != null)
            {
                foreach (var pb in RTimeline.singleton.Director.playableAsset.outputs)
                {
                    switch (pb.sourceObject)
                    {
                        case AnimationTrack _:
                            var atrack = pb.sourceObject as AnimationTrack;
                            ch_tracks.Add(atrack);
                            break;
                    }
                }
            }
        }
#endif
    }

    void LateUpdate()
    {
        if (!m_enableGraph) return;

        if (!m_inited)
        {
            Init();
            m_inited = true;
        }

        if (m_mixerPlayable.IsValid())
        {
            if (m_curve != null && m_curve.m_weights != null)
            {
                for (int i = 0; i < m_clips.Count; ++i)
                {
                    if (m_clips[i] == null) continue;
                    int index = (int)m_clips[i].m_clipType;
                    if (index < m_curve.m_weights.Count)
                    {
                        float weight = m_curve.m_weights[index];
                        if (m_clips[i].m_clipType == FacialClipType.idle) weight = 1;
                        m_mixerPlayable.SetInputWeight(i, weight);
                    }
                }
            }
        }

        SampleAnimationWhenDragCursor();
    }

    private void SampleAnimationWhenDragCursor()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            if (activeFlush && ch_tracks != null && RTimeline.singleton.Director.state == PlayState.Paused)
            {
                for (int i = 0; i < ch_tracks.Count; i++)
                {
                    var obj = RTimeline.singleton.Director.GetGenericBinding(ch_tracks[i]);
                    if (obj != null && obj.name == this.transform.name)
                    {
                        float time = (float)RTimeline.singleton.Director.time;
                        if (ch_tracks[i].infiniteClip != null)
                            ch_tracks[i].infiniteClip.SampleAnimation(this.gameObject, time);
                        else
                        {
                            var clips = ch_tracks[i].GetClipList();
                            for (int j = 0; j < clips.Count; j++)
                            {
                                if (clips[j].start <= time && clips[j].end >= time)
                                {
                                    float sampleTime = time - (float)clips[j].start;
                                    if (clips[j].animationClip.isLooping && sampleTime > clips[j].animationClip.averageDuration)
                                    {
                                        sampleTime = sampleTime % clips[j].animationClip.averageDuration;
                                    }
                                    clips[j].animationClip.SampleAnimation(this.gameObject, sampleTime);
                                }
                            }
                        }
                    }
                }
            }
        }

#endif
    }

    public void DisableGraph()
    {
        m_enableGraph = false;

#if UNITY_EDITOR
        if (!EngineContext.IsRunning && ch_tracks != null)
        {
            ch_tracks.Clear();
        }
#endif

        if (m_graph.IsValid())
        {
            m_graph.Stop();
            m_graph.Destroy();
        }
    }

    public void EnableGraph()
    {
        m_enableGraph = true;
        m_inited = false;
        Init();
    }
}



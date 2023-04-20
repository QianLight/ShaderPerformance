#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace XEditor
{

    public class XReactPlable
    {
        PlayableGraph _playableGraph;
        AnimationPlayableOutput _playableOutput;
        AnimationLayerMixerPlayable _mixer;
        AnimationClipPlayable _clipPlayable;
        AnimationClipPlayable _clipPlayable2;
        Animator m_Ator;

        private AvatarMask m_NoneAvatarMask;
        private bool m_bCreated = false;
        private bool _bIsAdditive = false;

        public bool IsGraphCreated { get { return m_bCreated; } }

        public void Init(Animator ator)
        {
            m_Ator = ator;
            m_NoneAvatarMask = new AvatarMask();
        }

        public void OnDisable()
        {
            if (m_bCreated)
            {
                _playableGraph.Destroy();
                m_bCreated = false;
            }
        }

        public void CreatReactMixLayer()
        {
            if (m_bCreated) return;
            m_bCreated = true;

            _playableGraph = PlayableGraph.Create("ReactGraph_T");
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "ReactAnimation_T", m_Ator);
            _mixer = AnimationLayerMixerPlayable.Create(_playableGraph, 2);
            _playableOutput.SetSourcePlayable(_mixer);
        }

        public void Play(AnimationClip clip, AnimationClip clip2 = null, AvatarMask mask = null)
        {
            if (clip == null) return;
            if (!m_bCreated)
                CreatReactMixLayer();

            if (_clipPlayable.IsValid())
            {
                _mixer.DisconnectInput(0);
                _clipPlayable.Destroy();
            }

            if (_clipPlayable2.IsValid())
            {
                _mixer.DisconnectInput(1);
                _clipPlayable2.Destroy();
            }

            _clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip);
            if (clip2 != null)
            {
                _clipPlayable2 = AnimationClipPlayable.Create(_playableGraph, clip2);
            }
            _mixer.ConnectInput(0, _clipPlayable, 0, 1f);

            if (clip2 != null)
            {
                _mixer.ConnectInput(1, _clipPlayable2, 0, 0f);
            }

            if (mask == null)
            {
                SetNoneLayerMask();
            }
            else
            {
                SetLayerMask(mask);
            }

            _playableGraph.Play();
        }

        public void DPlay()
        {
            if (m_bCreated)
            {
                if (_playableGraph.IsPlaying())
                    _playableGraph.Stop();

                _clipPlayable.SetTime(0f);
                if (_clipPlayable2.IsValid())
                    _clipPlayable2.SetTime(0f);
                _playableGraph.Play();
            }
        }

        public void SetMixInputWeight(float weight)
        {
            if (m_bCreated)
            {
                _mixer.SetInputWeight(0, weight);
                if (_mixer.GetInputCount() > 1)
                    _mixer.SetInputWeight(1, 1f - weight);
            }

        }

        public void StopReactAnim()
        {
            if (IsGraphCreated)
            {
                if (_playableGraph.IsPlaying())
                    _playableGraph.Stop();
            }
        }

        public void SetLayerWeight(float weight)
        {
            _playableOutput.SetWeight(weight);
        }

        public void SetLayerMask(AvatarMask mask)
        {
            if (IsGraphCreated)
            {
                _mixer.SetLayerMaskFromAvatarMask(0, mask);
            }
        }

        public void SetNoneLayerMask()
        {
            SetLayerMask(m_NoneAvatarMask);
        }

        public void SetLayerAdditive(bool isAdditive)
        {
            if (IsGraphCreated)
            {
                _mixer.SetLayerAdditive(0, isAdditive);
                _bIsAdditive = isAdditive;
            }
        }

        public void SetSpeed(float speed)
        {
            if (IsGraphCreated)
            {
                _mixer.SetSpeed(speed);
            }
        }
        private float m_AtorWeight = 1f;
        private float m_CrossFadeBeginWeight = 1f;
        private bool m_bCrossFade = false;
        private float m_CrossFadeDurationIn = 0f;
        private float m_CrossFadeDurationOut = 0f;
        private float m_CrossFadeTimer = 0f;
        private float m_CrossFadeTimerOut = 0f;
        private float m_ReactTimer = 0f;
        private float m_speed = 1f;
        public void BeginCrossFade(float durationIn , float durationOut, float reactTimer, float fromWeight = 1f,  bool isIn = true)
        {
            if (durationIn <= 0f) return;
            m_bCrossFade = true;
            m_CrossFadeDurationIn = durationIn;
            m_CrossFadeDurationOut = durationOut;
            m_CrossFadeTimer = 0f;
            m_CrossFadeTimerOut = 0f;
            m_CrossFadeBeginWeight = fromWeight;
            m_AtorWeight = m_CrossFadeTimer;
            m_ReactTimer = reactTimer;
        }
        public void Update(float fDeltaT)
        {
            if (m_bCrossFade)
            {
                float bb = 0;
                if (m_CrossFadeTimer < m_CrossFadeDurationIn) //进入融合
                {
                    float aa = m_CrossFadeBeginWeight / m_CrossFadeDurationIn;
                    bb = aa * m_CrossFadeTimer;
                    bb = Mathf.Clamp01(bb);
                    SetAnimatorWeight(bb);
                }
                else if(m_CrossFadeTimer >= m_CrossFadeDurationOut)
                {
                    float aa = m_CrossFadeBeginWeight / (m_ReactTimer - m_CrossFadeDurationOut);
                    m_CrossFadeTimerOut += fDeltaT;
                    bb = aa * m_CrossFadeTimerOut;
                    bb = Mathf.Clamp01(bb);
                    SetAnimatorWeight(m_CrossFadeBeginWeight - bb);

                }

                m_CrossFadeTimer += fDeltaT;

                if (m_CrossFadeTimer >= m_ReactTimer)
                {
                    m_bCrossFade = false;
                }
            }
        }

        public void SetAnimatorWeight(float weight)
        {
            if (IsGraphCreated)
            {
                _playableOutput.SetWeight(weight);
                _mixer.SetInputWeight(0, weight);
                //_mixer.SetInputWeight(1, 0);
                //_mixer.SetInputWeight(2, 1 - weight);
            }

            m_AtorWeight = weight;
        }
    }
}
#endif
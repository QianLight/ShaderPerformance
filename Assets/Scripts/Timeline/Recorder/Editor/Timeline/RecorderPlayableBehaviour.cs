using UnityEditor.Recorder;
using UnityEngine.Playables;

class RecorderPlayableBehaviour : PlayableBehaviour
{
    PlayState m_PlayState = PlayState.Paused;
    public RecordingSession session { get; set; }
    WaitForEndOfFrameComponent endOfFrameComp;
    bool m_FirstOneSkipped;

    public override void OnGraphStart(Playable playable)
    {
        if (session != null)
        {
            session.SessionCreated();
            m_PlayState = PlayState.Paused;
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        if (session != null && session.isRecording)
        {
            session.EndRecording();
            session.Dispose();
            session = null;
        }
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (session != null && session.isRecording)
        {
            session.PrepareNewFrame();
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (session != null)
        {
            if (endOfFrameComp == null)
            {
                endOfFrameComp = session.recorderGameObject.AddComponent<WaitForEndOfFrameComponent>();
                endOfFrameComp.m_playable = this;
            }
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (session == null)
            return;

        m_PlayState = PlayState.Playing;
        session.BeginRecording();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (session == null)
            return;

        if (session.isRecording && m_PlayState == PlayState.Playing)
        {
            session.Dispose();
            session = null;
        }

        m_PlayState = PlayState.Paused;
    }

    public void FrameEnded()
    {
        if (session != null && session.isRecording)
            session.RecordFrame();
    }
}

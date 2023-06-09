using System.Collections;
using UnityEngine;

namespace UnityEditor.Recorder
{
    /// <summary>
    /// Base component used for requesting a new frame. This class uses coroutines and WaitForEndOfFrame.
    /// This will not accumulate requests. All requests for the same frame will be merged into one request.
    /// Thus, FrameReady will be called once.
    /// This class pauses the project simulation (updates), waiting for the GPU to be ready.
    /// </summary>
    abstract class _FrameRequestComponent : MonoBehaviour
    {
        protected enum State
        {
            WaitingForFirstFrame,
            Running
        }
        
        private float projectTimeScale = 0;

        protected int requestCount = 0;

        protected int frameProducedCount = 0;

        protected State currentState;

        protected virtual void Awake()
        {
            requestCount = frameProducedCount = 0;

            EnterWaitingForFirstFrameState();
        }

        protected virtual void RequestNewFrame()
        {
            if (frameProducedCount == requestCount)
            {
                StartCoroutine(FrameRequest());
                requestCount++;
            }
        }

        protected virtual void OnDestroy()
        {
            // Restore timescale if we exit playmode before we had
            // time to restore it after first frame is rendered.
            if (currentState == State.WaitingForFirstFrame)
                RestoreProjectTimeScale();
        }

        protected abstract void FrameReady();

        IEnumerator FrameRequest()
        {
            yield return new WaitForEndOfFrame();

            FrameReady();

            if (currentState == State.WaitingForFirstFrame)
                EnterRunningState();

            frameProducedCount++;
        }

        void SaveProjectTimeScale()
        {
            projectTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        void RestoreProjectTimeScale()
        {
            if (Time.timeScale == 0)
                Time.timeScale = projectTimeScale;
        }

        void EnterWaitingForFirstFrameState()
        {
            currentState = State.WaitingForFirstFrame;
            SaveProjectTimeScale();
        }

        void EnterRunningState()
        {
            currentState = State.Running;
            RestoreProjectTimeScale();
        }
    }
}

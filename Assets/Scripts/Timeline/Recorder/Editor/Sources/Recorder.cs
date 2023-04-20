using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Recorder
{
    internal enum ERecordingSessionStage
    {
        BeginRecording,
        NewFrameStarting,
        NewFrameReady,
        FrameDone,
        EndRecording,
        SessionCreated
    }

    public abstract class Recorder : ScriptableObject
    {
        static int sm_CaptureFrameRateCount;
        bool m_ModifiedCaptureFR;

        protected internal int RecordedFramesCount { get; internal set; }

        protected List<RecorderInput> m_Inputs;

        void Awake()
        {
            sm_CaptureFrameRateCount = 0;
        }

        protected internal virtual void Reset()
        {
            RecordedFramesCount = 0;
            Recording = false;
        }

        void OnDestroy()
        {
            if (m_ModifiedCaptureFR)
            {
                sm_CaptureFrameRateCount--;
                if (sm_CaptureFrameRateCount == 0)
                {
                    Time.captureFramerate = 0;
                    if (RecorderOptions.VerboseMode)
                        Debug.Log("Recorder resetting 'CaptureFrameRate' to zero");
                }
            }
        }

        internal abstract RecorderSettings settings { get; set; }

        protected internal virtual void SessionCreated(RecordingSession session)
        {
            if (RecorderOptions.VerboseMode)
                Debug.Log(string.Format("Recorder {0} session created", GetType().Name));

            settings.SelfAdjustSettings(); // ignore return value.

            var fixedRate = settings.FrameRatePlayback == FrameRatePlayback.Constant ? (int)settings.FrameRate : 0;
            if (fixedRate > 0)
            {
                if (Time.captureFramerate != 0 && fixedRate != Time.captureFramerate)
                    Debug.LogError(string.Format("Recorder {0} is set to record at a fixed rate and another component has already set a conflicting value for [Time.captureFramerate], new value being applied : {1}!", GetType().Name, fixedRate));
                else if (Time.captureFramerate == 0 && RecorderOptions.VerboseMode)
                    Debug.Log("Frame recorder set fixed frame rate to " + fixedRate);

                Time.captureFramerate = fixedRate;
                sm_CaptureFrameRateCount++;
                m_ModifiedCaptureFR = true;
            }

            m_Inputs = new List<RecorderInput>();
            foreach (var inputSettings in settings.InputsSettings)
            {
                var input = (RecorderInput)Activator.CreateInstance(inputSettings.InputType);
                input.settings = inputSettings;
                m_Inputs.Add(input);
                SignalInputsOfStage(ERecordingSessionStage.SessionCreated, session);
            }
        }

        protected internal virtual bool BeginRecording(RecordingSession session)
        {
            if (Recording)
                throw new Exception("Already recording!");

            if (RecorderOptions.VerboseMode)
                Debug.Log(string.Format("Recorder {0} starting to record", GetType().Name));

            return Recording = true;
        }

        protected internal virtual void EndRecording(RecordingSession session)
        {
            if (!Recording) return;

            Recording = false;
            if (m_ModifiedCaptureFR)
            {
                m_ModifiedCaptureFR = false;
                sm_CaptureFrameRateCount--;
                if (sm_CaptureFrameRateCount == 0)
                {
                    Time.captureFramerate = 0;
                    if (RecorderOptions.VerboseMode)
                        Debug.Log("Recorder resetting 'CaptureFrameRate' to zero");
                }
            }

            foreach (var input in m_Inputs)
            {
                input?.Dispose();
            }

            if (RecorderOptions.VerboseMode)
                Debug.Log(string.Format("{0} recording stopped, total frame count: {1}", GetType().Name, RecordedFramesCount));
        }

        protected internal abstract void RecordFrame(RecordingSession ctx);


        protected internal virtual void PrepareNewFrame(RecordingSession ctx) { }


        protected internal virtual bool SkipFrame(RecordingSession ctx)
        {
            return !Recording
                || ctx.frameIndex % settings.captureEveryNthFrame != 0
                || settings.RecordMode == RecordMode.TimeInterval && ctx.currentFrameStartTS < settings.StartTime
                || settings.RecordMode == RecordMode.FrameInterval && ctx.frameIndex < settings.StartFrame
                || settings.RecordMode == RecordMode.SingleFrame && ctx.frameIndex < settings.StartFrame;
        }

        public bool Recording { get; protected set; }

        internal void SignalInputsOfStage(ERecordingSessionStage stage, RecordingSession session)
        {
            if (m_Inputs == null) return;

            switch (stage)
            {
                case ERecordingSessionStage.SessionCreated:
                    foreach (var input in m_Inputs)
                        input.SessionCreated(session);
                    break;
                case ERecordingSessionStage.BeginRecording:
                    foreach (var input in m_Inputs)
                        input.BeginRecording(session);
                    break;
                case ERecordingSessionStage.NewFrameStarting:
                    foreach (var input in m_Inputs)
                        input.NewFrameStarting(session);
                    break;
                case ERecordingSessionStage.NewFrameReady:
                    foreach (var input in m_Inputs)
                        input.NewFrameReady(session);
                    break;
                case ERecordingSessionStage.FrameDone:
                    foreach (var input in m_Inputs)
                        input.FrameDone(session);
                    break;
                case ERecordingSessionStage.EndRecording:
                    foreach (var input in m_Inputs)
                        input.EndRecording(session);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("stage", stage, null);
            }
        }
    }
}

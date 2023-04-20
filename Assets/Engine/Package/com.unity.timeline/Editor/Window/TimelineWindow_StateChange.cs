using System.Collections.Generic;

namespace UnityEditor.Timeline
{
    public class TimelineStateListener
    {
        private static TimelineStateListener m_instancce;
        public static TimelineStateListener Instance
        {
            get
            {
                if (m_instancce == null) m_instancce = new TimelineStateListener();
                return m_instancce;
            }
        }

        private List<object> m_fmods = new List<object>();
        public void RegisterFmodBehaviour(object obj)
        {
            if(!m_fmods.Contains(obj))
            {
                m_fmods.Add(obj);
            }
        }

        public void ClearFmods()
        {
            m_fmods.Clear();
        }

        public void OnStageChange(bool isPlaying)
        {
            for (int i = 0; i < m_fmods.Count; ++i)
            {
                if (m_fmods[i] == null) continue;
                System.Type curType = m_fmods[i].GetType();
                System.Reflection.MethodInfo methodInfo = curType.GetMethod("OnStateChange");
                methodInfo.Invoke(m_fmods[i], new object[] { isPlaying });
            }
        }

        public void RefreshTimelineWindow()
        {
            if(TimelineWindow.instance != null)
            {
                TimelineWindow.instance.state.Refresh();
            }
        }
    }


    partial class TimelineWindow
    {
        void InitializeStateChange()
        {
            state.OnPlayStateChange += OnPreviewPlayModeChanged;
            state.OnDirtyStampChange += OnStateChange;
            state.OnBeforeSequenceChange += OnBeforeSequenceChange;
            state.OnAfterSequenceChange += OnAfterSequenceChange;

            state.OnRebuildGraphChange += () =>
            {
                // called when the graph is rebuild, since the UI tree isn't necessarily rebuilt.
                if (!state.rebuildGraph)
                {
                    // send callbacks to the tacks
                    if (treeView != null)
                    {
                        var allTrackGuis = treeView.allTrackGuis;
                        if (allTrackGuis != null)
                        {
                            for (int i = 0; i < allTrackGuis.Count; i++)
                                allTrackGuis[i].OnGraphRebuilt();
                        }
                    }
                }
            };

            state.OnTimeChange += () =>
            {
                if (EditorApplication.isPlaying == false)
                {
                    state.UpdateRecordingState();
                    EditorApplication.SetSceneRepaintDirty();
                }

                // the time is sync'd prior to the callback
                state.Evaluate();     // will do the repaint

                InspectorWindow.RepaintAllInspectors();
            };

            state.OnRecordingChange += () =>
            {
                if (!state.recording)
                {
                    TrackAssetRecordingExtensions.ClearRecordingState();
                }
            };
        }
    }
}

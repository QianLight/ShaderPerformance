using CFEngine;
using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class DirectorActivationPlayableBehviour : PlayableBehaviour
    {
        public DirectorActivationTrack track;
        private Transform transformCache = null;
        private Vector3 initPos;
        private Vector3 lastPos;
        private bool initActive = false;

        public void Reset ()
        {
            transformCache = null;
            initPos = EngineUtility.Far_Far_Away;
        }

        public override void OnPlayableDestroy (Playable playable)
        {
            if (transformCache != null && track != null)
            {
                switch (track.postPlaybackState)
                {
                    case ActivationTrack.PostPlaybackState.Active:
                        transformCache.position = lastPos;
                        track.IsActive = true;
                        break;
                    case ActivationTrack.PostPlaybackState.Inactive:
                        transformCache.position = EngineUtility.Far_Far_Away;
                        track.IsActive = false;
                        break;
                    case ActivationTrack.PostPlaybackState.Revert:
                        track.IsActive = initActive;
                        transformCache.position = initPos;
                        break;
                    case ActivationTrack.PostPlaybackState.LeaveAsIs:
                    default:
                        break;
                }
            }
        }

        public override void PrepareFrame (Playable playable, FrameData info)
        {
            if (transformCache == null && track != null && track.BindTransform != null)
            {
                transformCache = track.BindTransform;
                initPos = transformCache.position;
                lastPos = initPos;
                initActive = track.IsActive;
            }

            if (transformCache != null)
            {
                int inputCount = playable.GetInputCount ();
                bool hasInput = false;
                for (int i = 0; i < inputCount; i++)
                {
                    if (playable.GetInputWeight (i) > 0)
                    {
                        hasInput = true;
                        break;
                    }
                }
                if (hasInput)
                {
                    if (!track.IsActive)
                    {
                        transformCache.position = lastPos;
                        track.IsActive = true;
                    }
                }
                else if (track.IsActive)
                {
                    lastPos = transformCache.position;
                    transformCache.position = EngineUtility.Far_Far_Away;
                    track.IsActive = false;
                }
            }

        }
    }
}
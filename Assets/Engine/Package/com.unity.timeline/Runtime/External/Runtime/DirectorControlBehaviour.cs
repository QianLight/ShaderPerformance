using CFEngine;
using UnityEngine.Playables;
namespace UnityEngine.Timeline
{
    public class DirectorControlBehaviour : DirectBaseBehaviour
    {
        private bool active = false;

        private DirectorControlAsset controlAsset
        {
            get { return asset as DirectorControlAsset; }
        }

        public override void Reset ()
        {
            base.Reset ();
            active = false;
        }

        public override void OnBehaviourPlay (Playable playable, FrameData info)
        {
            var ca = controlAsset;
            if (ca != null && ca.sfx != null)
            {
                active = ca.sfx.flag.HasFlag (SFX.Flag_Enable);
                ca.sfx.Play ();
            }
        }

        public override void OnPlayableDestroy (Playable playable)
        {
            var ca = controlAsset;
            if (ca != null && ca.sfx != null && !active)
            {
                switch (ca.postPlayback)
                {
                    case ActivationControlPlayable.PostPlaybackState.Active:
                        ca.sfx.Play ();
                        break;

                    case ActivationControlPlayable.PostPlaybackState.Inactive:
                        // ca.sfx.Stop (EngineContext.instance);
                        break;

                    case ActivationControlPlayable.PostPlaybackState.Revert:
                        // if(active)
                        //     ca.sfx.SetActive (active == 0);
                        break;
                }
            }
        }

        // public override void PrepareFrame (Playable playable, FrameData info)
        // {
        //     var ca = controlAsset;
        //     if (ca != null && ca.sfx != null)
        //     {

        //     }
        // }
    }
}
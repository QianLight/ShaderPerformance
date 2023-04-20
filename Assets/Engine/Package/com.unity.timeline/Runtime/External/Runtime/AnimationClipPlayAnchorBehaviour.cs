using UnityEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class AnimationClipPlayBehaviour : DirectBaseBehaviour
    {
        private GameObject go;
        private Transform trans;
        private AnimationClipPlayAsset animationClipPlayAsset
        {
            get { return asset as AnimationClipPlayAsset; }
        }

        public override void OnBehaviourPlay (Playable playable, FrameData info)
        {
            var acp = animationClipPlayAsset;
            if (acp != null && acp.clip != null && acp.track != null)
            {
                var trans = acp.track.BindTransform;
                if (trans != null)
                {
                    go = trans.gameObject;
                    this.trans = trans;
                }
            }
        }
        public override void OnBehaviourPause (Playable playable, FrameData info)
        {
            go = null;
        }
        public override void PrepareFrame (Playable playable, FrameData info)
        {
            var acp = animationClipPlayAsset;
            if (acp != null && acp.clip != null && go != null)
            {
                double time = playable.GetTime ();
                acp.clip.SampleAnimation (go, (float) time);
                trans.position += acp.OffsetPos;
                trans.rotation *= acp.OffsetRot;
            }
        }
    }

}
#if UNITY_EDITOR
using System.ComponentModel;
#endif
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public partial class DirectorActivationAsset : DirectPlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps { get { return ClipCaps.None; } }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject go)
        {
            return Playable.Create (graph);
        }
    }
}
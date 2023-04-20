using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using CFEngine;

namespace UnityEngine.Timeline
{
    /// <summary>
    /// A PlayableAsset that represents a timeline.
    /// </summary>
    [Serializable]
    public partial class DirectorAsset : PlayableAsset
    {
        public static DirectorAsset directorAsset;
        public static DirectorAsset instance
        {
            get
            {
                if (directorAsset == null)
                    directorAsset = CreateInstance<DirectorAsset>();
                return directorAsset;
            }
        }

        public ScriptPlayable<DirectorPlayable> directorPlayable;
        public double m_Duration;
        public override double duration
        {
            get
            {
                return m_Duration;
            }
        }
        public override IEnumerable<PlayableBinding> outputs
        {
            get
            {
                var tracks = DirectorHelper.singleton.tracks;
                if (tracks != null)
                {
                    for (int i = 0; i < tracks.Length; ++i)
                    {
                        var asset = tracks[i];
                        if (asset != null)
                        {
                            foreach (var output in asset.outputs)
                                yield return output;
                        }

                    }
                }
                yield return ScriptPlayableBinding.Create(name, this, typeof(GameObject));
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var director = directorPlayable.GetBehaviour();
            if (director == null)
            {
                directorPlayable = DirectorPlayable.Create(graph);
                director = directorPlayable.GetBehaviour();
            }
            int count = graph.GetPlayableCount();
            director.Compile(ref graph, directorPlayable, go, count == 1);
            if (directorPlayable.IsValid())
                return directorPlayable;
            return Playable.Null;
        }

        public void Reset()
        {
            m_Duration = 0;
            var behaviour = directorPlayable.GetBehaviour();
            if (behaviour != null)
            {
                behaviour.Clear();
            }
        }

        public void SetSpeed(double speed)
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                var director = DirectorHelper.GetDirector();
                if (director != null)
                {
                    var graph = director.playableGraph;
                    if (graph.IsValid())
                    {
                        Playable playable = graph.GetRootPlayable(0);
                        playable.SetSpeed(speed);
                    }
                }
                return;
            }
#endif
            if (directorPlayable.IsValid())
                directorPlayable.SetSpeed(speed);
        }
    }
}
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class XCineTrack : DirectorTrackAsset
{
    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
        
        //bind
        var director = go.GetComponent<PlayableDirector>();
        Camera mc = Camera.main;
        if (director != null && mc != null)
        {
            director.SetGenericBinding(this, mc.gameObject);
        }

        var mixer = ScriptPlayable<CinemachineMixer>.Create(graph);
        mixer.SetInputCount(inputCount);
        return mixer;
    }



}

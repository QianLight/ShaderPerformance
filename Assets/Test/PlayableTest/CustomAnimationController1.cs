using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


[RequireComponent(typeof(Animator))]
[ExecuteInEditMode]
public class CustomAnimationController1 : MonoBehaviour
{
    private PlayableGraph m_Graph;
    private AnimationPlayableOutput m_Output;
    private AnimationLayerMixerPlayable m_Mixer;
    public AnimationClip idle = null;
    public AnimationClip anim00 = null;
    public AnimationClip anim01 = null;
    public AnimationClip anim02 = null;
    public AnimationClip anim03 = null;
    

    [Range(0, 1)]
    public float kW0 = 0;
    [Range(0, 1)]
    public float kW1 = 0;
    [Range(0, 1)]
    public float kW2 = 0;
    [Range(0, 1)]
    public float kW3 = 0;

    private AnimationClipPlayable ClipPlayable00;
    private AnimationClipPlayable ClipPlayable01;
    private AnimationClipPlayable ClipPlayable02;
    private AnimationClipPlayable ClipPlayable03;
    AnimationLayerMixerPlayable mixerPlayable;
    Animator ar;

    void OnEnable()
    {
        m_Graph = PlayableGraph.Create();// RTimeline.singleton.Director.playableGraph;

        // Add an AnimationPlayableOutput to the graph.

        if (m_Graph.IsValid())
        {
            // Add an AnimationMixerPlayable to the graph.
            mixerPlayable = AnimationLayerMixerPlayable.Create(m_Graph, 5);


            mixerPlayable.SetLayerAdditive(0, false);
            mixerPlayable.SetLayerAdditive(1, true);
            mixerPlayable.SetLayerAdditive(2, true);
            mixerPlayable.SetLayerAdditive(3, true);
            mixerPlayable.SetLayerAdditive(4, true);

            // Add two AnimationClipPlayable to the graph.
            var clipPlayable0 = AnimationClipPlayable.Create(m_Graph, idle);
            var clipPlayable1 = AnimationClipPlayable.Create(m_Graph, anim00);
            var clipPlayable2 = AnimationClipPlayable.Create(m_Graph, anim01);
            var clipPlayable3 = AnimationClipPlayable.Create(m_Graph, anim02);
            var clipPlayable4 = AnimationClipPlayable.Create(m_Graph, anim03);

            // Create the topology, connect the AnimationClipPlayable to the
            // AnimationMixerPlayable.
            m_Graph.Connect(clipPlayable0, 0, mixerPlayable, 0);
            m_Graph.Connect(clipPlayable1, 0, mixerPlayable, 1);
            m_Graph.Connect(clipPlayable2, 0, mixerPlayable, 2);
            m_Graph.Connect(clipPlayable3, 0, mixerPlayable, 3);
            m_Graph.Connect(clipPlayable4, 0, mixerPlayable, 4);

            // Use the AnimationMixerPlayable as the source for the AnimationPlayableOutput.
            ar = GetComponent<Animator>();
            var animOutput = AnimationPlayableOutput.Create(m_Graph, "AnimationOutput", ar);
            animOutput.SetSourcePlayable(mixerPlayable);

            // Set the weight for both inputs of the mixer.
            mixerPlayable.SetInputWeight(0, 1);
            mixerPlayable.SetInputWeight(1, 0);
            mixerPlayable.SetInputWeight(2, 0);
            mixerPlayable.SetInputWeight(3, 0);
            mixerPlayable.SetInputWeight(4, 0);

            // Play the graph.
            m_Graph.Play();
        }
    }


    void LateUpdate()
    {
        if (mixerPlayable.IsValid())
        {
            //ar.enabled=true;
            mixerPlayable.SetInputWeight(1, kW0);
            mixerPlayable.SetInputWeight(2, kW1);
            mixerPlayable.SetInputWeight(3, kW2);
            mixerPlayable.SetInputWeight(4, kW3);
        }
    }
    
}
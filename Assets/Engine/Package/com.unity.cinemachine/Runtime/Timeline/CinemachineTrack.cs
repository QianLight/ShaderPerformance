#if !UNITY_2019_1_OR_NEWER
#define CINEMACHINE_TIMELINE
#endif
#if CINEMACHINE_TIMELINE

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Cinemachine;

//namespace Cinemachine.Timeline
//{
/// <summary>
/// Timeline track for Cinemachine virtual camera activation
/// </summary>
[Serializable]
[TrackClipType(typeof(CinemachineShot))]
[TrackBindingType(typeof(CinemachineBrain), TrackBindingFlags.None)]
#if UNITY_EDITOR
[TrackColor(0.53f, 0.0f, 0.08f)]
[CSDiscriptor("ÐéÄâ¾µÍ·")]
#endif
public partial class CinemachineTrack : TrackAsset
{
    public static List<CinemachineVirtualCameraBase> vc;

    private static Camera m_MainCamera;
    public static void SetMainCamera(Camera cam)
    {
        m_MainCamera = cam;
    }

    public static Camera GetMainCamera()
    {
        if (ReferenceEquals(m_MainCamera, null))
        {
            return Camera.main;
        }
        return m_MainCamera;
    }
    
    /// <summary>
    /// TrackAsset implementation
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="go"></param>
    /// <param name="inputCount"></param>
    /// <returns></returns>
    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
//#if !UNITY_2019_2_OR_NEWER
        int idx = 0;

        // Hack to set the display name of the clip to match the vcam
        foreach (var c in GetClips())
        {
            CinemachineShot shot = (CinemachineShot)c.asset;
            CinemachineVirtualCameraBase vcam = shot.VirtualCamera.Resolve(graph.GetResolver());
            if (vcam != null)
                c.displayName = vcam.Name;

            if (vc != null && vc.Count > idx)
            {
                shot.vcb = vc[idx++];
            }
        }

        //bind
        var director = go.GetComponent<PlayableDirector>();
        Camera mc = GetMainCamera();
        if (director != null && mc != null)
        {
            director.SetGenericBinding(this, mc.gameObject);
        }

//#endif



        var mixer = ScriptPlayable<CinemachineMixer>.Create(graph);
        mixer.SetInputCount(inputCount);
        return mixer;
    }

#if UNITY_EDITOR
    public override byte GetTrackType ()
    {
        return DirectorHelper.TrackType_Cine;
    }
#endif

}
//}
#endif

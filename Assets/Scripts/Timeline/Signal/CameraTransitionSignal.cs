using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;


#if UNITY_EDITOR
[Serializable]
[CSDiscriptor("抓屏")]
[MarkerAttribute(TrackType.MARKER)]
#endif
public class CameraTransitionSignal : DirectorSignalEmmiter
{

}

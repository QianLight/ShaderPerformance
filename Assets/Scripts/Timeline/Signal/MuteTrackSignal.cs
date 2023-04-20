using CFEngine;
using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("MuteStrack")]
#endif
public class MuteTrackSignal : DirectorSignalEmmiter
{
    //背景音的控制
    public string trackName;
    public bool mute;
}

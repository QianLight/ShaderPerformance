using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;


[Serializable]
#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("跳转", "Add Jump Timeline")]
#endif
public class JumpTimelineSignal : DirectorSignalEmmiter
{
    public string m_timelineName;
}

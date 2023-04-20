using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;


[Serializable]
#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("阴影", "Add SmartShadow")]
#endif
public class SmartShadowSignal : DirectorSignalEmmiter
{
    public bool m_enable;
}

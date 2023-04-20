
using System;
using UnityEngine.Timeline;

[Serializable]
#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("切换镜头光晕屏蔽状态", "Add Switch Lensflare State")]
#endif
public class SwitchLensflareStateSignal: DirectorSignalEmmiter
{
    
}


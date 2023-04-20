using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public class UIEmotionAsset : DirectBasePlayable<UIEmotionBehaviour>, ITimelineClipAsset
{

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string head;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string emotion;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public Vector3 pos;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public Vector3 rot;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public Vector3 scale;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string clip;
    

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

}

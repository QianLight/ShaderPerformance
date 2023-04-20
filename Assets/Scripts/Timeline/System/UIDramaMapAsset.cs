using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public class UIDramaMapAsset : DirectBasePlayable<UIDramaMapBehaviour>, ITimelineClipAsset
{


#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string sprite, sp1, sp2, sp3, sp4, sp5, sp6;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string atlas, atlas1, atlas2, atlas3, atlas4, atlas5, atlas6;



#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string rawTex;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string rawTex2;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string clip;


    public ClipCaps clipCaps { get { return ClipCaps.None; } }
}

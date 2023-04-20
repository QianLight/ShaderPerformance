using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
[System.Serializable]
#endif

public class UISubtitleAsset : DirectBasePlayable<UISubtitleBehaviour>, ITimelineClipAsset
{

#if UNITY_EDITOR
    [Header("字幕")]
#endif
    public string subTitle;
    
#if UNITY_EDITOR
    [Header("ID")]
#endif
    public int idx;
    

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Reset()
    {
        base.Reset();
        subTitle = null;
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        subTitle = DirectorHelper.singleton.ReadStr(reader);
    }

#if UNITY_EDITOR
    public static void SaveAsset(BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        UISubtitleAsset upa = asset as UISubtitleAsset;
        DirectorHelper.SaveStringIndex(bw, upa.subTitle, presave);
    }
#endif
}

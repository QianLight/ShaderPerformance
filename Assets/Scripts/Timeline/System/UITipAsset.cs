using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
[System.Serializable]
#endif

public class UITipAsset : DirectBasePlayable<UITipBehaviour>, ITimelineClipAsset
{
#if UNITY_EDITOR
    [Header("内容")]
#endif
    public string uiText;

#if UNITY_EDITOR
    [Header("ui clip")]
#endif
    public AnimationClip clip;
    AssetHandler animHandle;

    static ResLoadCb cb = LoadFinish;


    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Reset()
    {
        base.Reset();
        uiText = null;
        clip = null;
        LoadMgr.singleton.Destroy(ref animHandle);
    }

    private static void LoadFinish(AssetHandler ah, LoadInstance li)
    {
        var asset = li.loadHolder as UITipAsset;
        if (asset != null)
        {
            asset.clip = ah.obj as AnimationClip;
        }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        LoadAsset<AnimationClip>(reader, 0, ".anim", ref animHandle, this, cb);
        uiText = DirectorHelper.singleton.ReadStr(reader);
    }


#if UNITY_EDITOR
    public static void SaveAsset(BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        UITipAsset upa = asset as UITipAsset;
        DirectorHelper.SaveAsset(bw, upa.clip, ".anim", presave);
        DirectorHelper.SaveStringIndex(bw, upa.uiText, presave);
    }
#endif


}

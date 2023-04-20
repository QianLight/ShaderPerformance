using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
[System.Serializable]
#endif

public class UIBossShowAsset : DirectBasePlayable<UIBossBehaviour>, ITimelineClipAsset
{
#if UNITY_EDITOR
    [FormerlySerializedAs("uiText")] [Header("名字")]
#endif
    public string uiName;

#if UNITY_EDITOR
    [Header("英文名")]
#endif
    public string uiEnglish, uiText;


#if UNITY_EDITOR
    [Header("特点")]
#endif
    public string uiTedian;

#if UNITY_EDITOR
    [Header("应对")]
#endif
    public string uiYingdui;

#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string icon;
    

#if UNITY_EDITOR
    [Header("ui clip")]
#endif
    public AnimationClip clip;
    AssetHandler animHandle;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override void Reset()
    {
        base.Reset();
        clip = null;
        LoadMgr.singleton.Destroy(ref animHandle);
    }
    static ResLoadCb cb = LoadFinish;
    private static void LoadFinish(AssetHandler ah, LoadInstance li)
    {
        var asset = li.loadHolder as UIBossShowAsset;
        if (asset != null)
        {
            asset.clip = ah.obj as AnimationClip;
        }
    }


    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        LoadAsset<AnimationClip>(reader, 0, ".anim", ref animHandle, this, cb);
        uiName = DirectorHelper.singleton.ReadStr(reader);
        uiTedian = DirectorHelper.singleton.ReadStr(reader);
        uiYingdui = DirectorHelper.singleton.ReadStr(reader); 
        icon = DirectorHelper.singleton.ReadStr(reader);
    }


#if UNITY_EDITOR
    public static void SaveAsset(BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        UIBossShowAsset upa = asset as UIBossShowAsset;
        DirectorHelper.SaveAsset(bw, upa.clip, ".anim", presave);
        DirectorHelper.SaveStringIndex(bw, upa.uiName, presave);
        DirectorHelper.SaveStringIndex(bw, upa.uiTedian, presave);
        DirectorHelper.SaveStringIndex(bw, upa.uiYingdui, presave);
        DirectorHelper.SaveStringIndex(bw, upa.icon, presave);
    }
#endif
}

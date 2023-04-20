using CFEngine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using System.IO;
#endif
#if UNITY_EDITOR
[System.Serializable]
#endif
public class UIPlayerAsset : DirectBasePlayable<UIPlayerBehaviour>, ITimelineClipAsset
{
    public enum UIPlayerAssetLayer
    {
        BelowCutsceneDialog,
        OverCutsceneDialog,
    }
#if UNITY_EDITOR
    [Header("层级")]
#endif
    public UIPlayerAssetLayer m_layer = UIPlayerAssetLayer.OverCutsceneDialog;
#if UNITY_EDITOR
    [Multiline(1)]
    [Header("名字")]
#endif
    public string mNick, mEnglish, mName;
#if UNITY_EDITOR
    [Header ("ui clip")]
#endif
    public AnimationClip clip;
    AssetHandler animHandle;
#if UNITY_EDITOR
    [Header ("称号")]
#endif
    public string title;
#if UNITY_EDITOR
    [Header("赏金")]
#endif
    public string wanted;
#if UNITY_EDITOR
    [HideInInspector]
#endif
    public string icon;

    
    static ResLoadCb cb = LoadFinish;

    public ClipCaps clipCaps { get { return ClipCaps.Blending; } }

    public override void Reset ()
    {
        base.Reset ();
        mName = null;
        mNick = null;
        mEnglish = null;
        title = null;
        clip = null;
        LoadMgr.singleton.Destroy (ref animHandle);
    }

    private static void LoadFinish (AssetHandler ah, LoadInstance li)
    {
        var asset = li.loadHolder as UIPlayerAsset;
        if (asset != null)
        {
            asset.clip = ah.obj as AnimationClip;
        }
    }
    public override void Load (CFBinaryReader reader)
    {
        base.Load(reader);
        LoadAsset<AnimationClip>(reader, 0, ".anim", ref animHandle, this, cb);
        mName = DirectorHelper.singleton.ReadStr (reader);
        title = DirectorHelper.singleton.ReadStr (reader);
        mNick = DirectorHelper.singleton.ReadStr(reader);
        mEnglish = DirectorHelper.singleton.ReadStr(reader);
        icon = DirectorHelper.singleton.ReadStr(reader);
        wanted = reader.ReadString();
    }

#if UNITY_EDITOR
    public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave)
    {
        UIPlayerAsset upa = asset as UIPlayerAsset;
        DirectorHelper.SaveAsset (bw, upa.clip, ".anim", presave);
        DirectorHelper.SaveStringIndex (bw, upa.mName, presave);
        DirectorHelper.SaveStringIndex (bw, upa.title, presave);
        DirectorHelper.SaveStringIndex(bw, upa.mNick, presave);
        DirectorHelper.SaveStringIndex(bw, upa.mEnglish, presave);
        DirectorHelper.SaveStringIndex(bw, upa.icon, presave);
        if (!presave)
        {
            bw.Write(upa.wanted);
        }
    }
#endif
}
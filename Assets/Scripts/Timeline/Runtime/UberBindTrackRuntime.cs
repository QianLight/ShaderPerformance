using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using System;
#endif

public enum BindObjectType
{
    Player,
    Prefab,
    Fx,
    Static
}

public partial class UberBindTrackRuntime : DirectorTrackAsset
{
    public BindObjectType bindType = BindObjectType.Prefab;
    public uint id;
    public AssetHandler ah;
    //static ResLoadCb cb = LoadFinish;
    public override void Reset ()
    {
        base.Reset ();
        LoadMgr.singleton.Destroy (ref ah);
    }
    //private static void LoadFinish (AssetHandler ah, LoadInstance li)
    //{
    //    DirectorHelper.singleton.loadResCount--;
    //    var track = li.loadHolder as UberBindTrackRuntime;
    //    if (track != null)
    //    {
    //    }
    //}
    
    public override void Load (CFBinaryReader reader)
    {
        base.Load (reader);
        bindType = (BindObjectType) reader.ReadByte ();
        if (bindType == BindObjectType.Prefab)
        {
            id = reader.ReadUInt32 ();
        }
        string assetPath = DirectorHelper.singleton.ReadStr (reader);
        if (id != 0)
        {

        }
        else if (!string.IsNullOrEmpty(assetPath))
        {
            LoadMgr.GetAssetHandler(ref ah, assetPath, ResObject.ResExt_Prefab);
            LoadMgr.loadContext.Init(null, null, LoadMgr.AsyncLoad);
            LoadMgr.singleton.LoadAsset<GameObject>(ah, ResObject.ResExt_Prefab,true);
        }

    }

}
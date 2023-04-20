using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Timeline;
public class XActiveTrack : XTrack, ICallbackData, ISharedObject
{
    public DirectorActiveBinding bind = DirectorActiveBinding.Null;
    private static AnimInitCallback cb = AnimInitCb;
    public override void Reset ()
    {
        base.Reset();
        bind = DirectorActiveBinding.Null;
    }
    static void AnimInitCb (XGameObject gameObject, XAnimator ator, Animator unityAtor, ICallbackData cbData)
    {
        if (unityAtor != null)
        {
            XActiveTrack data = cbData as XActiveTrack;
            DirectorHelper.singleton.BindObject2Track (data.track, unityAtor, cbData);
        }
    }

    public override void LoadRefAsset ()
    {
        DebugLog.AddWarningLog("to do not implement");
        if (bind.type == PlayerableActiveType.Entity)
        {
            //uint uid = 0;
            //if (uint.TryParse(bind.val, out uid))
            //{
            //    List<XEntity> list = XEntityMgr.singleton.GetAll();
            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        if (list[i].TypeID == uid)
            //        {
            //            //list[i].EngineObject.SetDirectorBind(XTimeline.singleton.Director, pb.sourceObject);
            //            break;
            //        }
            //    }
            //}
        }
        else if (bind.type == PlayerableActiveType.Player)
        {
            XGameObject player = RTimeline.Ixtl.GetPlayerObject ();
            player.Ator.AnimCallback (cb, this);
        }
    }
    public override void UnloadRefAsset () { }

    public static void BindObject (DirectorTrackAsset track, string streamName)
    {
        bool find = false;
        ref DirectorActiveBinding bind = ref RTimeline.singleton.Data.GetActiveBinding (streamName,ref find);
        if (!find) return;
        XActiveTrack data = SharedObjectPool<XActiveTrack>.Get ();
        data.bind = bind;
        data.track = track;
        data.LoadRefAsset ();
    }

    public static void UnBindObject (XActiveTrack data)
    {
        data.UnloadRefAsset ();
        SharedObjectPool<XActiveTrack>.Release (data);
    }  
}
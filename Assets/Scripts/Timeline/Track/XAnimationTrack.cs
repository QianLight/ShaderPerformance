using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using XEditor;
#endif
public class XAnimtionTrack : XTrack, ICallbackData, ISharedObject
{
    public DirectorAnimBinding bind = DirectorAnimBinding.Null;
    public IActor actor = null;
    private static AnimInitCallback cb = AnimInitCb;
    public override void Reset ()
    {
        base.Reset ();
        bind = DirectorAnimBinding.Null;
    }

    private static void AnimInitCb (XGameObject gameObject, XAnimator ator, Animator unityAtor, ICallbackData cbData)
    {
        if (unityAtor != null)
        {
            XAnimtionTrack data = cbData as XAnimtionTrack;
            if (data.bind.type == PlayableAnimType.SyncInner)
            {
                gameObject.Position = data.bind.pos;
                gameObject.Rotation = Quaternion.Euler(data.bind.rot);
                gameObject.LocalScale = data.bind.scale;
            }
            DirectorHelper.singleton.BindObject2Track (data.track, unityAtor, cbData);
        }
    }
    public override void LoadRefAsset()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            //only test in editor
            if (RTimeline.animationTrackOnLoad != null)
                RTimeline.animationTrackOnLoad(ref bind, track, this);
            return;
        }
#endif
        {
            if (bind.type == PlayableAnimType.Entity)
            {
                actor = RTimeline.Ixtl.CreateActor(bind, cb, this);
            }
            else if (bind.type == PlayableAnimType.Player)
            {
                XGameObject player = RTimeline.Ixtl.GetPlayerObject();
                player.Ator.AnimCallback(cb, this);
            }
            else if (bind.type == PlayableAnimType.Dummy)
            {
                //to do create dummy
            }
            else if (bind.type == PlayableAnimType.Inner || bind.type == PlayableAnimType.SyncInner)
            {
                RTimeline.Ixtl.InnerBind((int)bind.val, cb, this);
            }
            else if (bind.type == PlayableAnimType.VCamera)
            {
                var cine = DirectorHelper.singleton.cine;
                if (cine && cine.transform.childCount > bind.val)
                {
                    var ator = cine.transform.GetChild((int)bind.val);
                    DirectorHelper.singleton.BindObject2Track(track, ator, this);
                }
            }
            else
            {
                DebugLog.AddErrorLog2("error bind type:{0}", bind.type.ToString());
                //must bind null,other wise memory leak
                DirectorHelper.singleton.BindObject2Track(track, null, this);
            }
        }
    }

    public override void UnloadRefAsset ()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning) { }
        else
#endif
        {
            if (!string.IsNullOrEmpty (bind.streamName))
            {
                if (bind.type == PlayableAnimType.Entity)
                {
                    RTimeline.Ixtl.DestroyActor (ref actor);
                }
                else if (bind.type == PlayableAnimType.Player) { }
                else if (bind.type == PlayableAnimType.Dummy) { }
            }
        }
    }

    public static void BindObject (DirectorTrackAsset track, string streamName)
    {
        bool find = false;
        ref DirectorAnimBinding bind = ref RTimeline.singleton.Data.GetAnimBinding (streamName, ref find);
        if (!find) return;
        XAnimtionTrack data = SharedObjectPool<XAnimtionTrack>.Get ();
        data.bind = bind;
        data.track = track;
        data.LoadRefAsset ();
    }

    public static void UnBindObject (XAnimtionTrack data)
    {
        data.UnloadRefAsset ();
        SharedObjectPool<XAnimtionTrack>.Release (data);
    }
}
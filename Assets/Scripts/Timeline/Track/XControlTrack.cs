using System;
using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.Timeline;

public class XControlTrack : XTrack, ICallbackData, ISharedObject
{
    Dictionary<string, SFX> pool = new Dictionary<string, SFX> ();

    public DirectorControlBinding bind = DirectorControlBinding.Null;
    public override void Reset ()
    {
        base.Reset ();
        bind = DirectorControlBinding.Null;
    }

    public override void LoadRefAsset ()
    {
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            //only test in editor
            if (RTimeline.controlTrackOnLoad != null)
                RTimeline.controlTrackOnLoad (ref bind, track, this);
            return;
        }
#endif
        if (bind.nodes != null && bind.nodes.Length > 0)
        {
            var clips = DirectorHelper.singleton.clips;
            for (int i = track.clipStart, index = 0; i < track.clipEnd; ++i, ++index)
            {
                if (bind.nodes.Length <= index) break;
                var node = bind.nodes[index];
                SFX sfx = null;
                if (!pool.TryGetValue (node.source, out sfx))
                {
                    sfx = SFXMgr.singleton.Create (node.source);
                    pool.Add (node.source, sfx);
                }

                if (sfx == null)
                {
                    XDebug.singleton.AddErrorLog ("fx config error, source: ", node.source);
                    break;
                }
                sfx.SetPos (ref node.pos);
                if (!EngineUtility.IsZero (ref node.scl)) sfx.SetScale (ref node.scl);
                if (!EngineUtility.IsZero (ref node.rot)) sfx.SetGlobalRot (ref node.rot);

                // var trans = go.transform;
                // trans.position = node.pos;
                // if (node.scl != Vector3.zero) trans.localScale = node.scl;
                // if (node.rot != Vector3.zero) trans.rotation = Quaternion.Euler (node.rot);
                // go.SetActive (false);
                var asset = clips[i].asset as DirectorControlAsset;
                if (asset != null)
                {
                    asset.sfx = sfx;
                }
            }
        }
    }

    public override void UnloadRefAsset ()
    {
        var sfxMgr = SFXMgr.singleton;
        var it = pool.GetEnumerator ();
        while (it.MoveNext ())
        {
            var value = it.Current.Value;
            if (value != null)
            {
                sfxMgr.Destroy (ref value);
            }
            // if (value != null) XResourceLoaderMgr.SafeDestroy (ref value, false);
        }
        pool.Clear ();
    }

    public static void BindObject (DirectorTrackAsset track, string streamName)
    {
        bool find = false;
        ref DirectorControlBinding bind = ref RTimeline.singleton.Data.GetControlBinding (streamName, ref find);
        if (!find) return;
        XControlTrack data = SharedObjectPool<XControlTrack>.Get ();
        data.bind = bind;
        data.track = track;
        DirectorHelper.singleton.BindObject2Track (track, null, data);
        data.LoadRefAsset ();
    }

    public static void UnBindObject (XControlTrack data)
    {
        data.UnloadRefAsset ();
        SharedObjectPool<XControlTrack>.Release (data);
    }
}
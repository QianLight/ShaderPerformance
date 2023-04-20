using CFEngine;
using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class DirectPlayableAsset : PlayableAsset, ISharedObject
    {
        //only valid at runtime
        [NonSerialized]
        public DirectorTrackAsset track;
        [NonSerialized]
        public DirectorClip directorClip;
        public virtual void Reset ()
        {
            track = null;
            directorClip = null;
        }

        public override Playable CreatePlayable (PlayableGraph graph, GameObject go)
        {
            return Playable.Null;
        }

        public virtual void Load (CFBinaryReader reader)
        {

        }
        //public static T GetAsset<T> (AssetHandler ah, ref Vector4Int param)
        //where T : DirectPlayableAsset
        //{
        //    DirectorHelper.singleton.loadResCount--;
        //    if (ah.obj != null)
        //    {
        //        var clips = DirectorHelper.singleton.clips;
        //        if (param.x < clips.Length)
        //        {
        //            var clip = clips[param.x];
        //            var asset = clip.asset as T;
        //            if (asset != null)
        //            {
        //                return asset;
        //            }
        //        }
        //    }
        //    return null;
        //}

        public void LoadAsset<T>(CFBinaryReader reader, int id, string ext, ref AssetHandler ah,
            DirectPlayableAsset asset, ResLoadCb cb)
        where T : UnityEngine.Object
        {
            string resPath = DirectorHelper.singleton.ReadStr(reader);
            if (!string.IsNullOrEmpty(resPath))
            {
                LoadMgr.GetAssetHandler(ref ah, resPath, ext);
                LoadMgr.loadContext.Init(cb, asset, LoadMgr.LoadForceImmediate);
                LoadMgr.loadContext.param.x = directorClip.index;
                LoadMgr.loadContext.param.y = id;
                LoadMgr.singleton.LoadAsset<T>(ah, ext,true);
            }
        }
    }

    public class DirectBasePlayable<TBehaviour> : DirectPlayableAsset
    where TBehaviour : DirectBaseBehaviour, new ()
    {
        protected TBehaviour behaviour;
        public TBehaviour GetBehavior ()
        {
            if (behaviour == null)
            {
                behaviour = new TBehaviour ();
            }
            return behaviour as TBehaviour;
        }

        protected TBehaviour nativeBehaviour;
        public TBehaviour GetNativeBehaviour()
        {
            return nativeBehaviour;
        }

        public override void Reset ()
        {
            base.Reset ();
            if (behaviour != null)
            {
                behaviour.Reset ();
                behaviour = null;
            }
        }
        public override Playable CreatePlayable (PlayableGraph graph, GameObject go)

        {
            var behavior = GetBehavior ();
            //set param
            behavior.asset = this;
            ScriptPlayable<TBehaviour> ret =  ScriptPlayable<TBehaviour>.Create (graph, behavior);
            nativeBehaviour = ret.GetBehaviour();
            return ret;
        }
    }

    public class DirectBaseBehaviour : PlayableBehaviour
    {
        public DirectPlayableAsset asset;
        public PlayableAsset m_asset;

        public virtual void Reset ()
        {
            asset = null;
        }

        protected void UpdateUIClip(Playable playable, AnimationClip clip, GameObject gameObject)
        {
            if (clip != null)
            {
                float len = clip.length;
                double scale = len / playable.GetDuration();
                double time = playable.GetTime();
                if (gameObject != null)
                    clip.SampleAnimation(gameObject, (float)(time * scale));
            }
        }

        protected void UpdateUIClip2(Playable playable, AnimationClip clip, GameObject gameObject)
        {
            if(clip!=null)
            {
                double time = playable.GetTime();
                time = Mathf.Max((float)time, clip.length);
                if (gameObject != null)
                    clip.SampleAnimation(gameObject, (float)(time));
            }
        }
    }
}
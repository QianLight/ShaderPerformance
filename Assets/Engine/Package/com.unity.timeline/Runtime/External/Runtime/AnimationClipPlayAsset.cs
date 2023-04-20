using CFEngine;
using UnityEngine.Playables;
#if UNITY_EDITOR
using System.IO;
#endif

namespace UnityEngine.Timeline
{

    public class AnimationClipPlayAsset : DirectBasePlayable<AnimationClipPlayBehaviour>, ITimelineClipAsset
    {
        public AnimationClip clip;
        public Vector3 OffsetPos = Vector3.zero;
        public Quaternion OffsetRot = Quaternion.identity;
        AssetHandler animHandle;
        static ResLoadCb cb = LoadFinish;
        public ClipCaps clipCaps { get { return ClipCaps.None; } }

        public override void Reset ()
        {
            base.Reset ();
            clip = null;
            LoadMgr.singleton.Destroy (ref animHandle);
        }
        private static void LoadFinish (AssetHandler ah, LoadInstance li)
        {
            var asset = li.loadHolder as AnimationClipPlayAsset;
            if (asset != null)
            {
                asset.clip = ah.obj as AnimationClip;
            }
        }
        public override void Load (CFBinaryReader reader)
        {
            base.Load (reader);
            reader.ReadVector (ref OffsetPos);
            reader.ReadQuaternion (ref OffsetRot);
            LoadAsset<AnimationClip> (reader, 0, ResObject.ResExt_Anim, ref animHandle,this, cb);
        }

#if UNITY_EDITOR
        public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave)
        {
            AnimationClipPlayAsset acp = asset as AnimationClipPlayAsset;
            DirectorHelper.SaveVector (bw, ref acp.OffsetPos);
            DirectorHelper.SaveQuaternion (bw, ref acp.OffsetRot);
            DirectorHelper.SaveAsset (bw, acp.clip, ".anim", presave);
        }
#endif
    }
}
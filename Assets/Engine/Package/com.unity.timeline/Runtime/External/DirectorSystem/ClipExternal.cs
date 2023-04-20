#if UNITY_EDITOR
using System.IO;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    partial class TimelineClip
    {
        public virtual void Save (BinaryWriter bw, bool presave)
        {
            SavePlayableAsset spa = null;
            byte assetType = DirectorHelper.GetAssetType (m_Asset);
            if (assetType < DirectorHelper.savePlayableFun.Length)
            {
                spa = DirectorHelper.savePlayableFun[assetType];
            }
            if (presave)
            {
                if (spa != null && assetType != 255)
                {
                    spa (bw, m_Asset as PlayableAsset, true);
                }
            }
            else
            {
                bw.Write (m_Start);
                bw.Write (m_ClipIn);
                bw.Write (m_Duration);
                bw.Write (m_TimeScale);
                bw.Write (m_EaseInDuration);
                bw.Write (m_EaseOutDuration);
                bw.Write (m_BlendInDuration);
                bw.Write (m_BlendOutDuration);
                bw.Write ((byte) m_PostExtrapolationMode);
                bw.Write ((byte) m_PreExtrapolationMode);
                bw.Write (m_PostExtrapolationTime);
                bw.Write (m_PreExtrapolationTime);
                bw.Write (assetType);
                if (spa != null && assetType != 255)
                {
                    spa (bw, m_Asset as PlayableAsset, false);
                }
            }

        }

        // public virtual void Save (DirectorClip clip)
        // {
        //     clip.m_Start = m_Start;
        //     clip.m_ClipIn = m_ClipIn;
        //     clip.m_Duration = m_Duration;
        //     clip.m_TimeScale = m_TimeScale;
        //     clip.m_EaseInDuration = m_EaseInDuration;
        //     clip.m_EaseOutDuration = m_EaseOutDuration;
        //     clip.m_BlendInDuration = m_BlendInDuration;
        //     clip.m_BlendOutDuration = m_BlendOutDuration;
        //     clip.m_PostExtrapolationMode = m_PostExtrapolationMode;
        //     clip.m_PreExtrapolationMode = m_PreExtrapolationMode;
        //     clip.m_PostExtrapolationTime = m_PostExtrapolationTime;
        //     clip.m_PreExtrapolationTime = m_PreExtrapolationTime;
        //     byte assetType = DirectorHelper.GetAssetType (m_Asset);
        //     DirectPlayableAsset a = DirectorHelper.CreateAsset (assetType);
        //     if (a != null)
        //     {
        //         a.directorClip = clip;
        //     }
        //     m_Asset = a;
        // }
    }
    public partial class RecordTimelineClip : TimelineClip
    {
        public RecordTimelineClip () : base (null)
        {

        }
        public override void Save (BinaryWriter bw, bool presave)
        {
            SavePlayableAsset spa = DirectorHelper.savePlayableFun[DirectorHelper.AssetType_AnimationClip];
            if (presave)
            {
                spa (bw, asset as PlayableAsset, true);
            }
            else
            {
                double start = 0;
                bw.Write (start);
                bw.Write (base.clipIn);
                bw.Write (DirectorHelper.timelineAsset.duration);
                bw.Write (base.timeScale);
                bw.Write (base.easeInDuration);
                bw.Write (base.easeOutDuration);
                bw.Write (base.blendInDuration);
                bw.Write (base.blendOutDuration);
                bw.Write ((byte) base.postExtrapolationMode);
                bw.Write ((byte) base.preExtrapolationMode);
                bw.Write (start);
                bw.Write (start);
                bw.Write (DirectorHelper.AssetType_AnimationClip);
                spa (bw, asset as PlayableAsset, false);
            }

        }

    }
}
#endif
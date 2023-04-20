#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public partial class AnimationPlayableAsset
    {
        [SerializeField]
        public uint flag = 0;

        public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave)
        {
            AnimationPlayableAsset apa = asset as AnimationPlayableAsset;
            if (presave)
            {
                if ((apa.flag & DirectorAnimationPlayableAsset.Flag_Override) == 0)
                {
                    DirectorHelper.SaveAsset (bw, apa.m_Clip, ".anim", true);
                }

            }
            else
            {
                DirectorHelper.SaveVector (bw, ref apa.m_Position);
                DirectorHelper.SaveVector (bw, ref apa.m_EulerAngles);

                bw.Write (apa.m_RemoveStartOffset);

                bw.Write (apa.m_ApplyFootIK);
                bw.Write ((byte) apa.m_Loop);
                bw.Write (apa.flag);
                if ((apa.flag & DirectorAnimationPlayableAsset.Flag_Override) == 0)
                {
                    DirectorHelper.SaveAsset (bw, apa.m_Clip, ".anim", false);
                }

            }
        }
    }

    partial class ActivationPlayableAsset
    {
        public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave)
        {
            //save nothing
        }
    }

    public partial class ControlPlayableAsset
    {
        public static void SaveAsset (BinaryWriter bw, PlayableAsset asset, bool presave) { }
    }
}
#endif
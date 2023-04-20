using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TDTools
{
    [CreateAssetMenu (fileName = "CARTreeAsset", menuName = "CARTreeAsset", order = 999)]
    public class CARTreeAsset : ScriptableObject
    {
        [SerializeField] List<TimelineConfigAsset> m_TimelineConfigAssets = new List<TimelineConfigAsset> ();
        [SerializeField] List<LevelConfigAsset> m_LevelConfigAssets = new List<LevelConfigAsset> ();
        [SerializeField] List<SkillConfigAsset> m_SkillConfigAssets = new List<SkillConfigAsset> ();
        [SerializeField] List<BehitConfigAsset> m_BehitConfigAssets = new List<BehitConfigAsset> ();
        [SerializeField] List<ModelArtAsset> m_ModelArtAssets = new List<ModelArtAsset> ();
        [SerializeField] List<AnimationArtAsset> m_AnimationArtAssets = new List<AnimationArtAsset> ();
        [SerializeField] List<EffectArtAsset> m_EffectArtAssets = new List<EffectArtAsset> ();

        internal List<TimelineConfigAsset> timelineConfigAssets { get { return m_TimelineConfigAssets; } set { m_TimelineConfigAssets = value; } }
        internal List<LevelConfigAsset> levelConfigAssets { get { return m_LevelConfigAssets; } set { m_LevelConfigAssets = value; } }
        internal List<BehitConfigAsset> behitConfigAssets { get { return m_BehitConfigAssets; } set { m_BehitConfigAssets = value; } }
        internal List<SkillConfigAsset> skillConfigAssets { get { return m_SkillConfigAssets; } set { m_SkillConfigAssets = value; } }
        internal List<ModelArtAsset> modelArtAssets { get { return m_ModelArtAssets; } set { m_ModelArtAssets = value; } }
        internal List<AnimationArtAsset> animationArtAssets { get { return m_AnimationArtAssets; } set { m_AnimationArtAssets = value; } }
        internal List<EffectArtAsset> effectArtAssets { get { return m_EffectArtAssets; } set { m_EffectArtAssets = value; } }

        [SerializeField] DateTime lastUpdateDate;
        readonly TimeSpan updateDateGap = new TimeSpan (15, 0, 0, 0);

        public static readonly Dictionary<Enum, string> dict = new Dictionary<Enum, string> ()
        {
            { ConfigAssetEnum.Timeline, "*.playable" },
            { ConfigAssetEnum.Level, "*.cfg" },
            { ConfigAssetEnum.Skill, "*.bytes" },
            { ConfigAssetEnum.Behit, "*.bytes" },
            { ArtAssetEnum.Animation, "*.anim" },
            { ArtAssetEnum.Model, "*.prefab" },
            { ArtAssetEnum.Effect, "*.prefab" },
        };

        void Awake ()
        {
            if (lastUpdateDate == null || NeedToUpdate ())
            {
                RefreshAll (this);
            }
        }

        void RefreshAll (CARTreeAsset asset)
        {
            CARUtility.RefreshArtAssets (asset);
            CARUtility.RefreshConfigAssets (asset);
            CARUtility.RefreshReferences (asset);
            lastUpdateDate = DateTime.Now;
        }

        bool NeedToUpdate ()
        {
            return lastUpdateDate + updateDateGap <= DateTime.Now;
        }
    }
}
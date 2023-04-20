#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class SelfShadowEditorInfo
    {
        public string name = "";
        public SelfShadowConfig config = SelfShadowConfig.defaultConfig;
        [NonSerialized]
        public Transform shadowLight;
        [NonSerialized]
        public float height = 0;

        public string hash = "";
        public string GetHash ()
        {
            if (string.IsNullOrEmpty (hash))
            {
                hash = FolderConfig.Hash ();
            }
            return hash;
        }
    }

    [System.Serializable]
    public class SelfShadowConfigs : BaseAssetConfig
    {
        public List<SelfShadowEditorInfo> configs = new List<SelfShadowEditorInfo> ();

        public override IList GetList () { return configs; }

        public override Type GetListType () { return typeof (List<SelfShadowEditorInfo>); }

        public override void OnAdd () { configs.Add (new SelfShadowEditorInfo ()); }

        public override float GetHeight (int index) { return configs[index].height; }

        public override void SetHeight (int index, float height) { configs[index].height = height; }

        public int Count
        {
            get
            {
                return configs.Count;
            }
        }
    }

    public class SelfShadowEditorConfig : AssetBaseConifg<SelfShadowEditorConfig>
    {
        public SelfShadowEditorInfo defaultConfig = new SelfShadowEditorInfo ()
        {
            name = "default"
        };
        public SelfShadowConfigs configs = new SelfShadowConfigs ();

    }

}
#endif
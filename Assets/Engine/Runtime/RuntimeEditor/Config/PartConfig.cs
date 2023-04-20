#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class PartInfo
    {
        public string[] parts;
        public int[] partsFlags;
    }

    [Serializable]
    public class PrefabPartList : BaseAssetConfig
    {
        public List<PartList> prefabParts = new List<PartList> ();
        public int maxPartCount = 8;

        public override IList GetList () { return prefabParts; }

        public override Type GetListType () { return typeof (List<PartList>); }

        public override void OnAdd ()
        {
            var pl = new PartList ();
            pl.maxPartCount = maxPartCount;
            prefabParts.Add (pl);
        }

        public override float GetHeight (int index) { return prefabParts[index].height; }

        public override void SetHeight (int index, float height) { prefabParts[index].height = height; }
    }

    [Serializable]
    public class PartList : BaseAssetConfig
    {

        public string prefabName = "";
        public List<string> partSuffix = new List<string> ();
        [NonSerialized]

        public float height = 0;

        public int maxPartCount = 8;
        public string hash = "";
        public string GetHash ()
        {
            if (string.IsNullOrEmpty (hash))
            {
                hash = FolderConfig.Hash ();
            }
            return hash;
        }
        public override IList GetList () { return partSuffix; }

        public override Type GetListType () { return typeof (List<string>); }

        public override void OnAdd ()
        {
            if (partSuffix.Count < maxPartCount)
            {
                partSuffix.Add ("");
            }
        }

        public override float GetHeight (int index) { return 21; }

        public override void SetHeight (int index, float height) { }

    }

    public class PartConfig : AssetBaseConifg<PartConfig>
    {
        public PartList parts = new PartList () { maxPartCount = prefabPartOffset };

        public PrefabPartList prefabParts = new PrefabPartList () { maxPartCount = prefabMaxPart };

        [NonSerialized]
        public Dictionary<string, PartInfo> partInfos = new Dictionary<string, PartInfo> ();

        public static int prefabPartOffset = 8;
        public static int prefabMaxPart = 32 - 8 - 1;
        public static uint commonMask = 0x80000000;

        [NonSerialized]
        public string[] partStr;

        private void UpdatePart (string name, PartInfo pi)
        {
            PartList pl = parts;
            int count = pl.partSuffix.Count;
            if (!string.IsNullOrEmpty (name))
            {
                for (int i = 0; i < prefabParts.prefabParts.Count; ++i)
                {
                    var pp = prefabParts.prefabParts[i];
                    if (pp.prefabName == name)
                    {
                        pl = pp;
                        count += pl.partSuffix.Count;
                        break;
                    }
                }
            }
            pi.parts = new string[count];
            pi.partsFlags = new int[count];
            for (int i = 0; i < parts.partSuffix.Count; ++i)
            {
                pi.parts[i] = parts.partSuffix[i];
                pi.partsFlags[i] = 1 << i;
            }
            if (pl != parts)
            {
                int start = parts.partSuffix.Count;
                for (int i = 0; i < pl.partSuffix.Count; ++i)
                {
                    pi.parts[i + start] = pl.partSuffix[i];
                    pi.partsFlags[i + start] = 1 << (i + prefabPartOffset);
                }
            }
        }

        public PartInfo GetPartInfo (string name, bool forceUpdate = false)
        {
            if (name != null)
            {
                name = name.ToLower ();
                if (partInfos.TryGetValue (name, out var pi))
                {
                    if (forceUpdate)
                    {
                        UpdatePart (name, pi);
                    }
                    return pi;
                }
                else
                {
                    pi = new PartInfo ();
                    partInfos.Add (name, pi);
                    UpdatePart (name, pi);
                    return pi;
                }
            }
            return null;

        }

        public void OnPartGUI (string prefabName, ref uint mask, int width = 300)
        {
            var pi = GetPartInfo (prefabName);
            if (pi != null)
            {
                EditorGUILayout.BeginHorizontal ();
                var position = EditorGUILayout.GetControlRect (true, 16, EditorStyles.popup, GUILayout.MaxWidth (width));
                mask = (uint) EditorCommon.DoMaskPopup (ref position, (int) mask,
                    pi.parts, pi.partsFlags);
                EditorGUILayout.EndHorizontal ();
            }
        }
    }
}
#endif
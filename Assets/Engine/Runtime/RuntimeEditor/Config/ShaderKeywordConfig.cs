#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;

namespace CFEngine
{
    [System.Serializable]
    public class KeywordInfo
    {
        public bool idKeyword;
        public string str;
    }

    [System.Serializable]
    public class KeywordGroup : BaseFolderHash
    {
        [NonSerialized]
        public float height = 0;
        public string name;
        public List<KeywordInfo> keywords = new List<KeywordInfo> ();
    }

    [System.Serializable]
    public class ShaderKeywordData : BaseAssetConfig
    {
        public List<KeywordGroup> groups = new List<KeywordGroup> ();
        [NonSerialized]
        public List<string> keyIndex = new List<string> ();

        public override IList GetList () { return groups; }

        public override Type GetListType () { return typeof (List<KeywordGroup>); }

        public override void OnAdd () { groups.Add (new KeywordGroup ()); }

        public override float GetHeight (int index) { return groups[index].height; }

        public override void SetHeight (int index, float height) { groups[index].height = height; }

        public void InitKeyMap ()
        {
            keyIndex.Clear ();
            for (int i = 0; i < groups.Count; ++i)
            {
                var g = groups[i];
                for (int j = 0; j < g.keywords.Count; ++j)
                {
                    var key = g.keywords[j];
                    if (key.idKeyword)
                    {
                        keyIndex.Add (key.str);
                    }
                }
            }
        }

        public bool ContainKeyIndex (string keyword)
        {
            for (int i = 0; i < groups.Count; ++i)
            {
                var group = groups[i];
                for (int j = 0; j < group.keywords.Count; ++j)
                {
                    var kw = group.keywords[j];
                    if (keyword == kw.str)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public void CombineKeyIndex (string keyword, ref uint key)
        {
            int index = 0;
            for (int i = 0; i < groups.Count; ++i)
            {
                var group = groups[i];
                for (int j = 0; j < group.keywords.Count; ++j)
                {
                    var kw = group.keywords[j];
                    if (kw.idKeyword && keyword == kw.str)
                    {
                        if (index < 32)
                        {
                            key |= (uint) (1 << index);
                        }
                        else
                        {
                            DebugLog.AddErrorLog ("too many keywords,more than 32");
                        }
                        return;
                    }
                    index++;
                }
            }
        }
        public uint GetShaderKey (string[] keywords)
        {
            uint key = 0;
            for (int i = 0; i < keywords.Length; ++i)
            {
                var keyword = keywords[i];
                CombineKeyIndex (keyword, ref key);
            }
            return key;

        }

        public uint GetShaderKey (List<string> keywords)
        {
            uint key = 0;
            for (int i = 0; i < keywords.Count; ++i)
            {
                var keyword = keywords[i];
                CombineKeyIndex (keyword, ref key);
            }
            return key;
        }

        public void GetShaderKeyStr (List<string> keywords, ref string key)
        {
            for (int i = 0; i < keyIndex.Count; ++i)
            {
                var keyword = keyIndex[i];
                if (keywords.IndexOf (keyword) >= 0)
                {
                    key += keyword;
                }
            }
        }
    }

    [System.Serializable]
    public class ShaderKeywordConfig : AssetBaseConifg<ShaderKeywordConfig>
    {
        public ShaderKeywordData keywordData = new ShaderKeywordData ();
    }
}
#endif
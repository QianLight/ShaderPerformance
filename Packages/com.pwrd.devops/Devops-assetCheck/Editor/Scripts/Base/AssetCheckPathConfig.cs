using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace AssetCheck
{
    // 这个结构数据这么全是因为有可能重编以后数据就变化了，
    // 所以需要在加载程序集的时候对比所有数据，如果有不同，更新数据
    [Serializable]
    public class ParamInfo
    {
        public string description;
        public string uiType;
        public string uiParam;
        public string paramValue = string.Empty;
        public string paramType;

        // 除了value，其他都一样
        public static bool Similarity(ParamInfo p1, ParamInfo p2)
        {
            return p1.description.Equals(p2.description) && p1.uiType.Equals(p2.uiType) && p1.paramType.Equals(p2.paramType);
        }

        public ParamInfo Clone()
        {
            return (ParamInfo)this.MemberwiseClone();
        }
    }
    [Serializable]
    public class CheckAssetWithParams
    {
        public string assetPath;
        public string tags;
        public List<string> excludePaths = new List<string>();
        public string includeKeyword = string.Empty;
        public List<ParamInfo> ruleParams = new List<ParamInfo>();
    }

    [Serializable]
    public class Content
    {
        public List<CheckAssetWithParams> checkPathsWithParams = new List<CheckAssetWithParams>();
    }

    [Serializable]
    public class RuleCheckAssetPath
    {
        public string ruleName;
        public string ruleClassify;
        public bool isOpen;
        // 主要就是因为JsonUtility的功能太弱了
        public Content content = new Content();
    }

    [Serializable]
    public class AssetCheckPathConfig : ScriptableObject
    {
        public List<RuleCheckAssetPath> ruleCheckAssetPaths = new List<RuleCheckAssetPath>();
    }
}
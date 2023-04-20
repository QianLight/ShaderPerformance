using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    namespace AssetCheckToDevops
    {
        [Serializable]
        public class CheckRule
        {
            public string ruleKey;
            public string ruleName;
            public string category;
            public string description;
            public int weight;
            public int enableState;
            public string content;
        }
        [Serializable]
        public class ToDevopsRules
        {
            public string versionCode;
            public string updateMsg;
            public List<CheckRule> rules = new List<CheckRule>();
        }
    }
    // 参数
    [Serializable]
    public class RuleParamReport
    {
        // 参数名字
        public string paramName;
        // 参数类型
        public string paramType;
        // 参数描述
        public string description;
        // ui类型
        public string guiType;
        // ui类型的参数
        public string uiParam;
    }

    // 规则
    [Serializable]
    public class RuleReport
    {
        // 规则名字
        public string name;
        // 规则描述
        public string description;
        // 详细规则描述
        public string detailedDescription;
        // 分类
        public string classify;
        // 权重
        public int weight;
        // 参数s
        public List<RuleParamReport> ruleParams = new List<RuleParamReport>();
        // 检查的目录
        public List<CheckPath> checkPathsReprot = new List<CheckPath>();
    }

    // 检测目录
    [Serializable]
    public class CheckPath
    {
        // 检测目录或文件
        public string checkPath;
        // 排除目录或文件
        public List<string> excludePath = new List<string>();
        // 参数
        public List<string> ruleParams = new List<string>();
    }

    // 发送给服务器的规则同步，文件夹设置同步
    [Serializable]
    public class ToDevopsRulesAndPathsData
    {
        public List<RuleReport> rulesReport = new List<RuleReport>();
    }

    [Serializable]
    public class AssetPathAndOutput
    {
        public string path;
        public string tags;
        public string output;
    }

    [Serializable]
    public class AssetPathGroupResultData
    {
        public CheckAssetWithParams checkAssetWithParams;
        public List<AssetPathAndOutput> assetsPathAndOutputs = new List<AssetPathAndOutput>();
        // 可能参数不一致,无法解析等
        public string errorMsg;
    }

    [Serializable]
    public class RuleCheckResultData
    {
        public string ruleKey;          // key
        public string ruleClassify;     // 分类
        public string ruleName;         // 描述
        public string description;      // 详细描述
        public long beginTime;
        public long endTime;
        public int checkAssetCount = 0;
        public int passAssetCount = 0;
        public int failedAssetCount = 0;
        // 可能找不到rule等
        public string errorMsg;
        public string outputFile = string.Empty;
        public List<AssetPathGroupResultData> assetPathGroups = new List<AssetPathGroupResultData>();
    }

    // 发送给服务器检测结果
    [Serializable]
    public class ToDevopsCheckResultData
    {
        public List<RuleCheckResultData> checkPathsReprot = new List<RuleCheckResultData>();
    }
}

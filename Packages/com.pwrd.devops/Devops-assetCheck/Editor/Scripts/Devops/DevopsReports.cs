using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using Devops.Core;
using AssetCheck.AssetCheckToDevops;

namespace AssetCheck
{
    public class DevopsReports
    {
        public static ToDevopsRules ConfigToDevopsRules(AssetCheckPathConfig config)
        {
            DevopsCoreInfo devopscoreInfo = EditorDevopsInfoSettings.GetDevopsInfo();
            ToDevopsRules syncRules = new ToDevopsRules();
            syncRules.versionCode = devopscoreInfo.versionId;
            foreach (var ruleCheckAssetPath in config.ruleCheckAssetPaths)
            {
                Type ruleT = Type.GetType(ruleCheckAssetPath.ruleName);
                if (ruleT == null)
                    continue;
                CheckRuleDescription descriptionAttr = ruleT.GetCustomAttribute<CheckRuleDescription>();
                if (descriptionAttr == null)
                    continue;
                CheckRule cr = new CheckRule();
                cr.ruleKey = ruleCheckAssetPath.ruleName;
                cr.ruleName = descriptionAttr.description;
                cr.category = descriptionAttr.classify;
                cr.description = descriptionAttr.detailedDescription;
                cr.weight = descriptionAttr.weight;
                cr.enableState = 1;
                cr.content = JsonUtility.ToJson(ruleCheckAssetPath.content);
                syncRules.rules.Add(cr);
            }
            return syncRules;
        }

        public static AssetCheckPathConfig DevopsRulesToConfig(ToDevopsRules syncRules)
        {
            AssetCheckPathConfig config = BuildConfig.CreateTempAssetCheckPathConfig();
            foreach(var rule in syncRules.rules)
            {
                RuleCheckAssetPath rcap = new RuleCheckAssetPath();
                rcap.ruleName = rule.ruleKey;
                rcap.ruleClassify = rule.category;
                rcap.isOpen = true;
                rcap.content = JsonUtility.FromJson<Content>(rule.content);
                config.ruleCheckAssetPaths.Add(rcap);
            }
            return config;
        }

        [MenuItem("Devops/ToDevopsCheckRulesReport")]
        public static void ToDevopsCheckRulesReport()
        {
            ToDevopsRulesAndPathsData report = new ToDevopsRulesAndPathsData();
            var ruleTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(RuleBase)));
            foreach (var ruleType in ruleTypes)
            {
                RuleReport ruleReprot = new RuleReport();
                report.rulesReport.Add(ruleReprot);
                ruleReprot.name = ruleType.FullName;
                var checkRuleDescription = ruleType.GetCustomAttribute<CheckRuleDescription>();
                if (checkRuleDescription != null)
                {
                    ruleReprot.description = checkRuleDescription.description;
                    ruleReprot.classify = checkRuleDescription.classify;
                }
                FieldInfo[] fieldsInfo = ruleType.GetFields();
                foreach (var fieldInfo in fieldsInfo)
                {
                    PublicParam publicParam = fieldInfo.GetCustomAttribute<PublicParam>();
                    if (publicParam == null)
                        continue;
                    RuleParamReport paramReport = new RuleParamReport();
                    ruleReprot.ruleParams.Add(paramReport);
                    paramReport.paramName = fieldInfo.Name;
                    paramReport.paramType = fieldInfo.FieldType.ToString();
                    paramReport.description = publicParam.description;
                    paramReport.guiType = publicParam.guiType.ToString();
                    paramReport.uiParam = publicParam.uiParam;
                }
            }

            string testReport = JsonUtility.ToJson(report);
            Debug.Log(testReport);
        }
    }

}
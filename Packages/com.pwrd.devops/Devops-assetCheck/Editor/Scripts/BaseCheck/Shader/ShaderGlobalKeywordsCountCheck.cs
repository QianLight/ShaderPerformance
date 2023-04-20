using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Shader", "全局关键字过多", "t:Shader", "")]
    public class ShaderGlobalKeywordsCountCheck : RuleBase
    {
        delegate void GetShaderVariantEntriesFiltered_type(Shader shader, int maxEntries, string[] filterKeywords, ShaderVariantCollection excludeCollection, out int[] passTypes, out string[] keywordLists, out string[] remainingKeywords);

        [PublicParam("全局关键字个数", eGUIType.Input)]
        public int keywordsCount = 10;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null)
                return true;
            int currentGlobalkeywordsCount = 0;
            MethodInfo method = typeof(UnityEditor.ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            string[] globalkeywords = (string[])method.Invoke(null, new System.Object[] { shader });
            currentGlobalkeywordsCount = globalkeywords.Length;
            output = currentGlobalkeywordsCount.ToString();
            return currentGlobalkeywordsCount < keywordsCount;
        }
    }
}
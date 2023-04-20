using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    public enum eGUIType
    {
        Input,
        Enum,
        Bool,
        StringMaskField,
    }

    // 类型描述（分类，描述）
    public class CheckRuleDescription : System.Attribute
    {
        public string classify;
        public string description;
        public string filter;
        public string detailedDescription;
        public int weight;
        public bool runtime;
        public CheckRuleDescription(string classify, string description, string filter, string detailedDescription, int weight = 1, bool runtime = false)
        {
            this.classify = classify;
            this.description = description;
            this.filter = filter;
            this.detailedDescription = detailedDescription;
            this.weight = weight;
            this.runtime = runtime;
        }
    }

    // 公开的参数(描述，ui类型，ui参数）
    public class PublicParam : System.Attribute
    {
        public string description;
        public eGUIType guiType;
        public string uiParam;
        public PublicParam(string description, eGUIType guiType, string uiParam = null)
        {
            this.description = description;
            this.guiType = guiType;
            this.uiParam = uiParam;
        }
    }

    // 检测函数
    public class PublicMethod : System.Attribute
    {
        public bool isAsync = false;
        public PublicMethod(bool async = false)
        {
            isAsync = async;
        }
    }
}
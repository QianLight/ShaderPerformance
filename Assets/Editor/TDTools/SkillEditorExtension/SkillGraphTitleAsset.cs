using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TDTools
{
    public class SkillGraphTitleAsset : ScriptableObject
    {
        public SkillGraphShowDataTitle[] Titles;
        [NonSerialized]
        public Dictionary<string, SkillGraphShowDataTitle> TitleDic;
        public void ReBuild()
        {
            if (TitleDic == null)
                TitleDic = new Dictionary<string, SkillGraphShowDataTitle>();
            TitleDic.Clear();
            foreach (var item in Titles)
            {
                TitleDic[item.TypeName] = item;
            }
        }

    }
}

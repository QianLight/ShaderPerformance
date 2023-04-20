using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "使用了Outline的Text组件", "t:Prefab", "会增加大量顶点，增加了较高的重建开销")]

    public class TextOutlineCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> names = new List<string>();
            Outline[] outlines = gObject.GetComponentsInChildren<Outline>();
            foreach(var outline in outlines)
            {
                names.Add(outline.gameObject.name);
            }
            output = string.Join(",", names);
            return outlines.Length == 0;
        }
    }
}
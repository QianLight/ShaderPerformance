using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Prefab", "检查prefab的GameObject数量", "t:Prefab", "")]
    public class TransformChildCheck : RuleBase
    {
        [PublicParam("最大数量", eGUIType.Input)]
        public int maxcount = 500;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            int transformCount = 0;
            GetChildCount(gObject.transform, ref transformCount);
            output = transformCount.ToString();
            return transformCount < maxcount;
        }

        void GetChildCount(Transform tf, ref int count)
        {
            count += tf.childCount;
            for (int i = 0; i < tf.childCount; i++)
            {
                GetChildCount(tf.GetChild(i), ref count);
            }
        }
    }
}
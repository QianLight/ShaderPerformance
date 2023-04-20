using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// shenmo
namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "粒子层数", "t:Prefab", "查有多少个ParticleSystem组件")]
    public class ParticleLayerCheck : RuleBase
    {
        [PublicParam("层数阈值", eGUIType.Input)]
        public int layerCount = 10;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            ParticleSystem[] pss = gObject.GetComponentsInChildren<ParticleSystem>();
            output = $"{ pss.Length}--标准值:{layerCount}--层数超出占比:{(float)pss.Length/layerCount}";
            return pss.Length < layerCount;
        }
    }
}
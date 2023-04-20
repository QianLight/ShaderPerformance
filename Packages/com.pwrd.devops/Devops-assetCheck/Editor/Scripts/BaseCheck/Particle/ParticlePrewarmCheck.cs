using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查ParticleSystem开启了Prewarm", "t:Prefab", "建议没有特殊需求关闭这个功能，该功能会在第一帧造成一定程度的卡顿")]
    public class ParticlePrewarmCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> prewarmNames = new List<string>();
            ParticleSystem[] pss = gObject.GetComponentsInChildren<ParticleSystem>();
            bool result = true;
            foreach (var ps in pss)
            {
                if (ps.main.prewarm)
                {
                    prewarmNames.Add(ps.gameObject.name);
                    result = false;
                }
            }
            output = string.Join(",", prewarmNames);
            return result;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Animation", "检查Animation曲线值包含scaled", "t:AnimationClip", "")]
    public class AnimationScaleCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                return true;
           
             foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip))
            {
                string tName = curveBinding.propertyName.ToLower();
                if (tName.Contains("scale"))
                {
                    output = "Contains Scale";
                    return false;
                }
            }
            return true;
        }
    }
}


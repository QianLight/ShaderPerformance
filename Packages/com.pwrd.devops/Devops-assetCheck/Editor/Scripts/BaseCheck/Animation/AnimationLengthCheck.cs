using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetCheck
{
    [CheckRuleDescription("Animation", "检查动画长度", "t:animation", "")]
    public class AnimationLengthCheck : RuleBase
    {
        [PublicParam("动画长度(秒)", eGUIType.Input)]
        public float animationLength = 5.0f;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                return true;
            output = clip.length.ToString();
            return clip.length < animationLength;
        }
    }
}
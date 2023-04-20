using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AssetCheck
{
    [CheckRuleDescription("Animation", "检查动画FrameRate", "t:animation", "帧率过高会影响效率")]
    public class AnimationFrameRateCheck : RuleBase
    {
        [PublicParam("帧率", eGUIType.Input)]
        public float frameRate = 0.84f;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                return true;
            output = clip.frameRate.ToString();
            return clip.frameRate < frameRate;
        }
    }
}
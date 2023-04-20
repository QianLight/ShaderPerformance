using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Animation", "检查动画运行时占用内存大小", "t:animation", "")]
    public class AnimationRuntimeMemoryCheck : RuleBase
    {
        [PublicParam("内存占用（k）", eGUIType.Input)]
        public float memorySize = 1024;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                return true;
            float currentMemorySize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(clip) / 1024;
            output = currentMemorySize.ToString() + "k";
            return currentMemorySize < memorySize;
        }
    }
}
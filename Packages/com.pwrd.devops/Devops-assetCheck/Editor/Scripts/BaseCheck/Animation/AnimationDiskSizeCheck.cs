using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Animation", "检查动画磁盘占用空间大小", "t:animation", "检查Fbx文件，文件占用磁盘大小超过阈值的会被筛出")]
    public class AnimationDiskSizeCheck : RuleBase
    {
        [PublicParam("磁盘占用(k)", eGUIType.Input)]
        public long diskSize = 1024;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                return true;
            var fileInfo = new FileInfo(path);
            output = (fileInfo.Length / 1024).ToString();
            return fileInfo.Length / 1024 < diskSize;
        }
    }
}
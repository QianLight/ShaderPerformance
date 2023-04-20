using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Renderer", "检查使用了MotionVector的SkinnedMeshRenderer", "t:Prefab", "开启了以后蒙皮和网格均需要两倍内存")]
    public class SkinnedMeshRendererUseMotionVectorCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> motionVectorsNames = new List<string>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = gObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            bool result = true;
            foreach(var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if(skinnedMeshRenderer.skinnedMotionVectors)
                {
                    motionVectorsNames.Add(skinnedMeshRenderer.gameObject.name);
                    result = false;
                }
            }
            output = string.Join(",", motionVectorsNames);
            return result;
        }
    }
}
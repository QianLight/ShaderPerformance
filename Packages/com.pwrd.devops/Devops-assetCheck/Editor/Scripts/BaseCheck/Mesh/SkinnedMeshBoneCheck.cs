using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "蒙皮网格骨骼数超过阈值", "t:Prefab", "")]

    public class SkinnedMeshBoneCheck : RuleBase
    {
        [PublicParam("骨骼数", eGUIType.Input)]
        public int boneCount = 100;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<KeyValuePair<string, int>> names = new List<KeyValuePair<string, int>>();
            SkinnedMeshRenderer[] skinnedMeshRenderers = gObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            bool result = true;
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.bones.Length > boneCount)
                {
                    names.Add(new KeyValuePair<string, int>(skinnedMeshRenderer.gameObject.name, skinnedMeshRenderer.bones.Length));
                    result = false;
                }
            }
            output = string.Join(",", names);
            return result;
        }
    }
}
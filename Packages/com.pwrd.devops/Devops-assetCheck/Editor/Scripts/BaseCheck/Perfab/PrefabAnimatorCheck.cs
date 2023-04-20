using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Prefab", "勾选了ApplyRootMotion的Animator组件", "t:prefab", "")]
    public class PrefabAnimatorCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gameObject == null)
                return true;
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                return true;
            if(animator.applyRootMotion)
            {
                output = "ApplyRootMotion";
            }
            return !animator.applyRootMotion;

        }
    }

    [CheckRuleDescription("Prefab", "包含TransformHierarchy的Animator组件", "t:prefab", "")]
    public class PrefabAnimatorTransformHierarchyCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gameObject == null)
                return true;
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                return true;

            return !animator.hasTransformHierarchy;

        }
    }

    [CheckRuleDescription("Prefab", "检查prefab中Animator未开启OptimizeGameObjects", "t:prefab", "")]
    public class PrefabAnimatorOptimizeCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gameObject == null)
                return true;
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
                return true;

            return animator.isOptimizable;
        }
    }
}

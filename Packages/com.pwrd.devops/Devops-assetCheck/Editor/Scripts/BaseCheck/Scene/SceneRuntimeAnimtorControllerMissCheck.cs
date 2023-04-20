using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Scene", "场景RuntimeAnimatorController是否为空", "t:scene", "")]
    public class SceneRuntimeAnimtorControllerMissCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            AssetHelper.OpenScene(path);
            Animator[] animators = (Animator[])Resources.FindObjectsOfTypeAll(typeof(Animator));
            bool result = true;
            List<string> names = new List<string>();
            foreach (var animator in animators)
            {
                if (animator.runtimeAnimatorController == null)
                {
                    names.Add(animator.gameObject.name);
                    result = false;
                }
            }
            output = string.Join(",", names);
            AssetHelper.BackLastScene();
            return result;
        }
    }
}

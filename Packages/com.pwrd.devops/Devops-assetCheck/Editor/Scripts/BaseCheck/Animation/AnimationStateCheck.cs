using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Animation", "AnimationState数量过高的AnimatorController", "t:animatorcontroller", "")]
    public class AnimationStateCheck : RuleBase
    {
        [PublicParam("State数量", eGUIType.Input)]
        public int stateCount = 100;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                return true;
            int currentStateCount = 0;
            foreach (var layer in controller.layers)
            {
                currentStateCount += layer.stateMachine.states.Length;
            }
            output = currentStateCount.ToString();
            return currentStateCount < stateCount;
        }
    }
}


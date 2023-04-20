using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AssetCheck
{

    [CheckRuleDescription("Scene", "检查场景层级中的obj组件丢失", "t:Scene", "")]
    public class SceneMissCheck : RuleBase
    {
        List<string> gameObjectNames = new List<string>();
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            AssetHelper.OpenScene(path);
            bool result = true;
            GameObject[] sceneGameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
            foreach(var gameObject in sceneGameObjects)
            {
                var comps = gameObject.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    var tempComp = comps[i];
                    if (tempComp == null)
                    {
                        gameObjectNames.Add(gameObject.name);
                        result = false;
                    }
                }
            }
            output = string.Join(",", gameObjectNames);
            AssetHelper.BackLastScene();
            return result;
        }
    }

}
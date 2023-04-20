using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "开启Collision或Trigger的ParticleSystem", "t:prefab", "")]
    public class PrefabCollisionCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ParticleSystem particle = AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
            if (particle == null)
                return true;

            if (particle.collision.enabled || particle.trigger.enabled)
            {
                output = "开启Collision或Trigger";
                return false;
            }
            //ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(path);

            return true;
        }
    }
}
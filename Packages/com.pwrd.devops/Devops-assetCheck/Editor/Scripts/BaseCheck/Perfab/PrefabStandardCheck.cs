using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Prefab", "该粒子系统中使用了Standard Shader", "t:prefab", "")]
    public class PrefabStandardCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ParticleSystem particle = AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
            if (particle == null)
                return true;
            Renderer renderer = particle.GetComponent<Renderer>();
            foreach(var material in renderer.sharedMaterials)
            {
                if(material != null && material.shader != null && material.shader.name.Contains("Standard"))
                {
                    output = renderer.gameObject.name;
                    return false;
                }
            }
            return true;
          
        }
    }
}

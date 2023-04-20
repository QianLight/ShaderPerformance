using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Material", "使用了Standard Shader的材质", "t:material", "")]
    public class MaterialStandardCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                return true;

            Shader shader = material.shader;
            bool isStandard = shader.name.Equals("Standard");
            if(isStandard)
            {
                output = "Standard";
            }
            return !isStandard;
        }
    }
}


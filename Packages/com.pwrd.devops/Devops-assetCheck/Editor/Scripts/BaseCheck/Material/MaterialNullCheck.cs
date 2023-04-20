using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Material", "包含空纹理采样的材质", "t:material", "")]
    public class MaterialNullCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                return true;

            Shader shader = material.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); ++i)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = ShaderUtil.GetPropertyName(shader, i);
                    Texture tex = material.GetTexture(propertyName);
                    if (tex == null)
                    {
                        output = propertyName;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
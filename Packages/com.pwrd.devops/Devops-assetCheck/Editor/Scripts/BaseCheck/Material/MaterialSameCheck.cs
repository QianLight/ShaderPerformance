using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Material", "包含相同纹理采样的材质", "t:material", "")]
    public class MaterialSameCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                return true;

            List<string> sameTextureNames = new List<string>();
            List<string> texturePaths = new List<string>();
            bool result = true;
            int[] textureNames = material.GetTexturePropertyNameIDs();
            foreach(var textureName in textureNames)
            {
                Texture tex = material.GetTexture(textureName);
                if(tex != null)
                {
                    string texPath = AssetDatabase.GetAssetPath(tex.GetInstanceID());
                    if (texPath == null)
                        continue;
                    if(texturePaths.Contains(texPath))
                    {
                        sameTextureNames.Add(tex.name);
                        result = false;
                    }
                    else
                    {
                        texturePaths.Add(texPath);
                    }
                }
            }
            output = string.Join(",", sameTextureNames);
            return result;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Material", "包含纯色纹理采样的材质", "t:material", "")]
    public class MaterialSolidCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
                return true;

            List<string> solidTextures = new List<string>();
            bool result = true;
            int[] textureNames = material.GetTexturePropertyNameIDs();
            foreach (var textureName in textureNames)
            {
                Texture tex = material.GetTexture(textureName);
                if (tex != null && tex is Texture2D)
                {
                    if(((Texture2D)tex).IsSolidColor())
                    {
                        solidTextures.Add(tex.name);
                        result = false;
                    }
                }
            }
            output = string.Join(",", solidTextures);
            return result;
        }
    }
}


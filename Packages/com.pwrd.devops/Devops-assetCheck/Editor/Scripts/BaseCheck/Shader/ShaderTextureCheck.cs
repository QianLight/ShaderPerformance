using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Shader", "纹理采样数过多的Shader", "t:Shader", "")]
    public class ShaderTextureCheck : RuleBase
    {
        [PublicParam("纹理数量", eGUIType.Input)]
        public int textureCount = 3;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null)
                return true;
            int currentTextureCount = 0;
            Material m = new Material(shader);
            int[] textureNames = m.GetTexturePropertyNameIDs();
            currentTextureCount = textureNames.Length;
            output = currentTextureCount.ToString();
            return currentTextureCount < textureCount;
        }
    }
}


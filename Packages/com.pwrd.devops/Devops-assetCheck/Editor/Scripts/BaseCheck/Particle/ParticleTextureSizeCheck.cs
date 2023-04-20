using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "贴图大小", "t:Prefab", "粒子系统使用贴图应酌情用低一些的")]

    public class ParticleTextureSizeCheck : RuleBase
    {
        [PublicParam("图片大小", eGUIType.Input)]
        public int textureSize = 256;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> textureNames = new List<string>();
            ParticleSystemRenderer[] psrs = gObject.GetComponentsInChildren<ParticleSystemRenderer>();
            bool result = true;
            foreach (var psr in psrs)
            {
                foreach(var material in psr.sharedMaterials)
                {
                    if (material == null)
                        continue;
                    string[] texturePropertyNames = material.GetTexturePropertyNames();
                    foreach(var texturePropertyName in texturePropertyNames)
                    {
                        Texture texture = material.GetTexture(texturePropertyName);
                        if(texture != null)
                        {
                            if(texture.width > textureSize || texture.height > textureSize)
                            {
                                textureNames.Add(texture.name);
                                result = false;
                            }
                        }
                    }
                }
            }
            output = string.Join("，", textureNames) + $"--标准值--{textureSize}";
            return result;
        }
    }
}
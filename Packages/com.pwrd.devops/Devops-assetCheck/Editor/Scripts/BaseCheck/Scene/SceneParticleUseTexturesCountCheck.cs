using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Scene", "场景中特效引用贴图总数数量超过阈值", "t:scene", "")]
    public class SceneParticleUseTexturesCountCheck : RuleBase
    {
        [PublicParam("图片数量", eGUIType.Input)]
        public uint textureCount = 100;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            uint currentSceneTextureCount = 0;
            AssetHelper.OpenScene(path);
            ParticleSystemRenderer[] particleSystemRenderers = (ParticleSystemRenderer[])Resources.FindObjectsOfTypeAll(typeof(ParticleSystemRenderer));
            foreach (var particleSystemRenderer in particleSystemRenderers)
            {
                foreach(var material in particleSystemRenderer.sharedMaterials)
                {
                    if (material == null)
                        continue;
                    string[] texturePropertyNames = material.GetTexturePropertyNames();
                    foreach (var texturePropertyName in texturePropertyNames)
                    {
                        Texture texture = material.GetTexture(texturePropertyName);
                        if (texture != null)
                        {
                            currentSceneTextureCount++;
                        }
                    }
                }
            }
            output = currentSceneTextureCount.ToString();
            AssetHelper.BackLastScene();
            return currentSceneTextureCount < textureCount;
        }
    }
}
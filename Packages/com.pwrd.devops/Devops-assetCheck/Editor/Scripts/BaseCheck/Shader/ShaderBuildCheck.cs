using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;





namespace AssetCheck
{
    [CheckRuleDescription("Shader", "Build后：生成变体数过多的Shader", "t:Shader", "")]
    public class ShaderBuildCheck : RuleBase
    {
        [PublicParam("变体数量", eGUIType.Input)]
        public int shaderVariantCount = 100;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (shader == null)
                return true; 
            int currentShaderVariantCount = 0;
            currentShaderVariantCount = GetAllShaderVariantCount(shader);
            output = currentShaderVariantCount.ToString();
            return currentShaderVariantCount < shaderVariantCount;
        }

        int GetAllShaderVariantCount(Shader shader)
        {
            Assembly asm = Assembly.GetAssembly(typeof(UnityEditor.SceneView));
            System.Type t2 = asm.GetType("UnityEditor.ShaderUtil");
            MethodInfo method = t2.GetMethod("GetVariantCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
 
            var variantCount = method.Invoke(null, new System.Object[] { shader, true });
            return int.Parse(variantCount.ToString());
        }

    }
}

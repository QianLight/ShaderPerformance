using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Texture", "过滤模式为Trilinear的纹理", "t:Texture2D","")]
    public class TextureTrilinearCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null)
                return true;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return true;
            output = importer.filterMode.ToString();
            return importer.filterMode != FilterMode.Trilinear;
        }
    }
}
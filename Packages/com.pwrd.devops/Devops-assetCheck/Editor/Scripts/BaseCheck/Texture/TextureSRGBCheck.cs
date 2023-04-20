using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Texture", "SRGB开关状态检查", "t:Texture2D", "和参数开关状态不符合的资源会被筛出")]
    public class TextureSRGBCheck : RuleBase
    {
        [PublicParam("sRGB开关状态", eGUIType.Bool)]
        public bool bOpenSRGB = false;

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
            output = importer.sRGBTexture.ToString();
            return importer.sRGBTexture == bOpenSRGB;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Texture", "图片AlphaSource检查", "t:Texture2D", "")]
    public class TextureAlphaSourceCheck : RuleBase
    {
        [PublicParam("AlphaSource", eGUIType.Enum)]
        public TextureImporterAlphaSource textureAlphaSource = TextureImporterAlphaSource.None;
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
            output = importer.alphaSource.ToString();
            return textureAlphaSource == importer.alphaSource;
        }
    }
}
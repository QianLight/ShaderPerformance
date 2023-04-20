using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Texture", "图片Type检查", "t:Texture2D", "不是选择的类型的图片，会被筛出")]
    public class TextureTypeCheck : RuleBase
    {
        [PublicParam("TextureType", eGUIType.Enum)]
        public TextureImporterType textureType = TextureImporterType.Default;
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
            output = importer.textureType.ToString();
            return textureType == importer.textureType;
        }
    }
}
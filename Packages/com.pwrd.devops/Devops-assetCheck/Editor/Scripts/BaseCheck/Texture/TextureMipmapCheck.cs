using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Texture", "开启Mipmap选项的Sprite纹理", "t:Texture2D", "")]
    public class TextureMipmapCheck : RuleBase
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
            if (importer.textureType != TextureImporterType.Sprite)
                return true;
            output = importer.mipmapEnabled.ToString();
            return !importer.mipmapEnabled;
        }
    }
}


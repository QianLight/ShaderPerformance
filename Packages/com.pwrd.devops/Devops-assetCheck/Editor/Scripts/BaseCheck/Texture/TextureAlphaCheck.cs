using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Texture", "包含无效透明通道的纹理", "t:Texture2D", "")]
    public class TextureAlphaCheck : RuleBase
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
            if (importer.alphaSource != TextureImporterAlphaSource.FromInput)
                return true;
            if (!importer.DoesSourceTextureHaveAlpha())
                return true;
            if(tex is Texture2D)
            {
                if(((Texture2D)tex).IsAlphaZero() || ((Texture2D)tex).IsAlphaOne())
                {
                    output = tex.name.ToString();
                    return false;
                }
            }
            return true;
        }
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Texture", "开启Read/Write选项的纹理", "t:Texture2D", "")]
    public class TextureReadCheck : RuleBase
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
            if(importer.isReadable)
            {
                output = "开启了Read/Write";
            }
            return !importer.isReadable;
        }
    }

}

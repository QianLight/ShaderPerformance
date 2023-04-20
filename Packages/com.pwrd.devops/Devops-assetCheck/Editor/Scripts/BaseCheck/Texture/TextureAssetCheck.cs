#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Texture", "检查图片大小", "t:Texture2D", "检查图片的宽高")]
    class TextureSizeCheck : RuleBase
    {
        [PublicParam("宽", eGUIType.Input)]
        public int width = 512;
        [PublicParam("高", eGUIType.Input)]
        public int height = 512;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex == null)
                return true;
            int w = tex.width;
            int h = tex.height;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer)
            {
                w = Mathf.Min(w, importer.maxTextureSize);
                h = Mathf.Min(h, importer.maxTextureSize);
            }
            output = $"{w}*{h}";
            return w < width && h < height;
        }
    }
}
#endif
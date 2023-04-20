using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Texture", "使用非压缩格式的纹理", "t:Texture2D", "")]
    public class TextureFormatCheck : RuleBase
    {
        [PublicParam("Standalone压缩格式", eGUIType.Enum)]
        public TextureImporterFormat StandaloneSettingFormater = TextureImporterFormat.ARGB32;
        [PublicParam("IOS压缩格式", eGUIType.Enum)]
        public TextureImporterFormat IOSSettingFormater = TextureImporterFormat.ARGB32;
        [PublicParam("Android压缩格式", eGUIType.Enum)]
        public TextureImporterFormat AndroidSettingFormater = TextureImporterFormat.ARGB32;

        Dictionary<string, string> results = new Dictionary<string, string>();
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
            bool bResult = true;
            var standaloneSetting = importer.GetPlatformTextureSettings("Standalone");
            if (standaloneSetting.format != AndroidSettingFormater)
            {
                results.Add("Standalone", standaloneSetting.format.ToString());
                bResult = false;
            }
            var iphoneSetting = importer.GetPlatformTextureSettings("iPhone");
            if (iphoneSetting.format != IOSSettingFormater)
            {
                results.Add("iPhone", iphoneSetting.format.ToString());
                bResult = false;
            }
            var androidSetting = importer.GetPlatformTextureSettings("Android");
            if (androidSetting.format != AndroidSettingFormater)
            {
                results.Add("Android", androidSetting.format.ToString());
                bResult = false;
            }
            output = string.Join(" , ", results);
            return bResult;
        }
    }
}


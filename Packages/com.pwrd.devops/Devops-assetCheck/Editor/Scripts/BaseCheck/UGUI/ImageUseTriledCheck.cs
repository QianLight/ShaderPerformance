using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "Image控件使用了Triled模式", "t:Prefab", "Texture为Clamp时会增加面片数量")]
    public class ImageUseTriledCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> names = new List<string>();
            Image[] images = gObject.GetComponentsInChildren<Image>();
            bool result = true;
            foreach (var image in images)
            {
                if (image.type == Image.Type.Tiled)
                {
                    names.Add(image.gameObject.name);
                    result = false;
                }
            }
            output = string.Join(",", names);
            return result;
        }
    }

}
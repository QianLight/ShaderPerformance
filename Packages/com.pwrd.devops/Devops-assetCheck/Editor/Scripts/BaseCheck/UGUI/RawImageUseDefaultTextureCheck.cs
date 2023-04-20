using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "使用了默认纹理的RawImage组件", "t:Prefab", "texture为空会生成一个默认的白色图片")]
    public class RawImageUseDefaultTextureCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> names = new List<string>();
            RawImage[] rawImages = gObject.GetComponentsInChildren<RawImage>();
            bool result = true;
            foreach(var rawImage in rawImages)
            {
                if(rawImage.texture == null)
                {
                    names.Add(rawImage.gameObject.name);
                    result = false;
                }
            }
            output = string.Join(",", names);
            return result;
        }
    }

}
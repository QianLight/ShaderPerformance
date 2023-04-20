using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "使用了不可见的RawImage组件", "t:Prefab", "alpha=0，且对应的Canvas Renderer组件没有开启Cull Transparent Mesh 的 RawImage 组件依然会参与渲染，建议进行排除")]
    public class RawImageInvisibilityCheck : RuleBase
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
            foreach (var rawImage in rawImages)
            {
                CanvasRenderer cr = rawImage.gameObject.GetComponent<CanvasRenderer>();
                if (cr == null)
                    continue;
                if (cr.cullTransparentMesh)
                    continue;
                if(rawImage.texture != null && rawImage.texture is Texture2D && (rawImage.texture as Texture2D).IsAlphaZero())
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
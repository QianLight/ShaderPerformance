using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AssetCheck
{
    [CheckRuleDescription("UGUI", "使用了不可见的Image组件", "t:Prefab", "alpha=0，且对应的Canvas Renderer组件没有开启Cull Transparent Mesh 的 Image 组件依然会参与渲染，建议进行排除")]
    public class ImageInvisibilityCheck : RuleBase
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
                CanvasRenderer cr = image.gameObject.GetComponent<CanvasRenderer>();
                if (cr == null)
                    continue;
                if (cr.cullTransparentMesh)
                    continue;
                Sprite sprite = image.sprite;
                if(sprite != null)
                {
                    Rect rect = sprite.rect;
                    if (sprite.texture.IsAlphaZero((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height))
                    {
                        names.Add(image.name);
                        result = false;
                    }
                }
            }
            output = string.Join(",", names);
            return result;
        }
    }

}
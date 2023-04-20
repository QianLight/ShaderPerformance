/*
 * @author yankang.nj
 * 预制件初版，默认子节点
 */
using UnityEngine;

namespace GSDK.RNU {
    public class ReactPrefabTmp: SimpleBaseView {
        private GameObject realGameObject = null;

        public ReactPrefabTmp() {
            // no-op
            realGameObject = new GameObject();
            realGameObject.AddComponent<RectTransform>();
        }

        public override GameObject GetGameObject() {
            return realGameObject;
        }

        public void SetPrefabName(string name)
        {
            RNUMainCore.LoadGameObjectByIDAsync(name, (go) =>
            {
                if (go == null)
                {
                    Util.Log("LoadGameObjectByIDAsync with name {0} get null", name);
                    return;
                }

                if (realGameObject.transform.childCount != 0)
                {
                    var oldChild = realGameObject.transform.GetChild(0);
                    Object.Destroy(oldChild.gameObject);
                }

                var newChildGo = StaticCommonScript.InstantiatePrefab(go);

                var rectTransform = newChildGo.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = newChildGo.AddComponent<RectTransform>();

                    if (rectTransform == null)
                    {
                        // 有可能添加RectTransform失败，这种情况直接返回，不做处理
                        Util.Log("Prefab {0} add RectTransform error!!, please check", name);
                        return;
                    }
                }

                // 保持和父组件大小一致
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                rectTransform.SetParent(realGameObject.transform, false);
            });
        }
    }
}

/*
 * @author yankang.nj
 * 所有组件的基类 view
 * 其中 GetGameObject 所有子类必须实现，返回自己的节点
 * GetGameObjectByIndex，返回查找的子节点，没有则返回为 null
 * GetContentObject，默认获取得失 GetGameObject 返回节点，复杂节点可重写该方法，返回内层节点
 * 
 */

using UnityEngine;

namespace GSDK.RNU {
    public  abstract class SimpleBaseView: BaseView {
        private int id;
        private TransformInfo transformInfo = null;

        public  abstract GameObject GetGameObject();

        public virtual GameObject GetContentObject()
        {
            return GetGameObject();
        }
        
        public GameObject GetGameObjectByIndex(int index)
        {
            GameObject go = GetGameObject();
            if (index > 0 && index < go.GetComponentsInChildren<Transform>(true).Length)
            {
                return go.GetComponentsInChildren<Transform>(true)[index].gameObject;
            }
            return null;
        }
        
        public int GetId() {
            return id;
        }


        
        public void SetId(int id) { 
            this.id = id;
        }

        public virtual void Destroy() {
            GameObject innerObj = GetGameObject();
            Object.Destroy(innerObj);

        }

        public void SetTransformInfo(TransformInfo ti)
        {
            transformInfo = ti;
        }

        public virtual void Add(BaseView child)
        { 
            GameObject my = GetContentObject();
            GameObject goChild = child.GetGameObject();
            
            goChild.transform.SetParent(my.transform, false);
        }

        public virtual void Insert(int index, BaseView child)
        {
            Add(child);
            child.GetGameObject().transform.SetSiblingIndex(index);
        }

        public virtual void Unset(BaseView child)
        {
            GameObject goChild = child.GetGameObject();
            goChild.transform.SetParent(null, false);
        }

        public virtual void SetLayout(int x, int y, int width, int height)
        {
            GameObject panel = GetGameObject();
            
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = panel.AddComponent<RectTransform>();
            }

            float finalWidth = width;
            float finalHeight = height;
            float finalX = x;
            float finalY = y;
            if (transformInfo != null)
            {
                // 在原本布局的基础上 ，添加Translate变换
                finalX = x + transformInfo.Translate.x;
                finalY = y + transformInfo.Translate.y;
                // 注意使用localRotation 而不是rotation。因为rotation会影响子元素
                rectTransform.localRotation = Quaternion.Euler(transformInfo.Rotate);
                // 在原本布局的基础上 ，添加scale变换
                rectTransform.localScale = transformInfo.Scale;
            }


            rectTransform.anchorMin = new Vector2(0.0F, 1.0F);
            rectTransform.anchorMax = new Vector2(0.0F, 1.0F);
            rectTransform.offsetMin = new Vector2(finalX, -(finalY + finalHeight));
            rectTransform.offsetMax = new Vector2(finalX + finalWidth, -finalY);
            rectTransform.pivot = new Vector2(0.5F, 0.5F);
        }
        
    };
}
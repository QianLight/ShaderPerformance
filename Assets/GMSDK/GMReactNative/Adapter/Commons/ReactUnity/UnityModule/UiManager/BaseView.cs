/*
 * @author yankang.nj
 * 
 * ReactUnity基本的View。实现类必须实现：
 * 1. Add， 添加一个BaseView
 * 2. Remove 移除一个BaseView
 * 3. Insert 在任意位置添加BaseView
 * 4. SetLayout 如何布局自己
 *
 * 一般情况，一个组件View表现有两种形式， 外侧的表现由 GetGameObject 定义， 内侧的表现则有GetContentObject定义
 */
using UnityEngine;

namespace GSDK.RNU {
    public interface BaseView {
        int GetId();

        void SetId(int id);

        void Destroy();

        void SetTransformInfo(TransformInfo ti);

        // 操作UI结构的API
        GameObject GetGameObject();
        GameObject GetContentObject();

        GameObject GetGameObjectByIndex(int index);
        
        void Add(BaseView child);

        void SetLayout(int x, int y, int width, int height);

        void Unset(BaseView child);

        void Insert(int index, BaseView child);
    };
}
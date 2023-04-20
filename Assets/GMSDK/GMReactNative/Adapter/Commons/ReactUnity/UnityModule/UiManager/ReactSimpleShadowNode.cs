using System;
using System.Collections.Generic;
using Facebook.Yoga;
using UnityEngine;

namespace GSDK.RNU {
    public abstract class ReactSimpleShadowNode   {
        public YogaNode yogaNode;
        public int tag;

        public string className;

        public ReactSimpleShadowNode parent;
        public List<ReactSimpleShadowNode> children = new List<ReactSimpleShadowNode>();

        private int x;
        private int y;
        private int width;
        private int height;
        

        // 记录transform属性是否改变
        private bool transformChanged = false;

        public ReactSimpleShadowNode(int tag) {
            this.tag = tag;
            this.yogaNode = new YogaNode();
        }

        public void SetClassName(string cn)
        {
            className = cn;
        }

        public virtual void Insert(int index, ReactSimpleShadowNode sChild) {
            children.Insert(index, sChild);
            yogaNode.Insert(index, sChild.yogaNode);

            sChild.parent = this;
        }

        public ReactSimpleShadowNode GetChildAt(int index) {
            return children[index];
        }

        public void RemoveChildAt(int index) {
            ReactSimpleShadowNode child = children[index];

            children.RemoveAt(index);
            yogaNode.RemoveAt(index);

            child.parent = null;
        }

        public void CalculateLayout() {
            yogaNode.CalculateLayout();
        }

        public void Destory() {
            DestroyInner(this.yogaNode);
        }

        private void DestroyInner(YogaNode yn) {
            while (yn.Count > 0)
            {
                int index = yn.Count - 1;
                YogaNode child = yn[index];
                yn.RemoveAt(index);

                DestroyInner(child);
            }

            yn.Destory();
        }

        public void DispatchUpdates(UIViewOperationQueue operationQueue) {
            int yx =(int) Math.Round(yogaNode.LayoutX);
            int yy =(int) Math.Round(yogaNode.LayoutY);
            int yw =(int) Math.Round(yogaNode.LayoutWidth);
            int yh =(int) Math.Round(yogaNode.LayoutHeight);

            if (yx != x
                || yy != y
                || yw != width
                || yh != height
                || transformChanged
            ) {
                // need update
                x = yx;
                y = yy;
                width = yw;
                height = yh;
                transformChanged = false;

                operationQueue.EnqueueUpdateLayout(
                    tag,
                    className,
                    x,
                    y,
                    width,
                    height
                );
            }
        }

        public virtual Dictionary<string, object> UpdateYogaNodePropsAndGetOtherProps(Dictionary<string, object> args) {
            Dictionary<string, object> r = new Dictionary<string, object>();
            YogaHelper.UpdateYogaNodeAndGetOtherProps(args, this.yogaNode, r);

            if (args != null && args.ContainsKey("transform"))
            {
                transformChanged = true;
            }

            return r;
        }

        public virtual bool isVirtual() {
            return false;
        }

        // 在Layout 之前， 处理可能存在的额外的更新行为， 配合ExtraUpdaterShadowNodeQueue 使用
        public virtual void DoExtraUpdate(UIViewOperationQueue operationQueue) {
            // no-op
        }

        public Vector4 GetLayout()
        {
            return new Vector4(x, y, width, height);
        }
    }
}

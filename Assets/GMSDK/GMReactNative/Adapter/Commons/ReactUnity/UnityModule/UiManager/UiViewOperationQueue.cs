/*
 * @Author: yankang.nj@bytedance.com
 * @Date: 2021-08-20 14:26:36
 * @Description: UI 更新队列，UIOperation是更新的最小单元， UIManagerModule将负责把UI的更新 已UIOperation的形式 保存在operationQueue
 * 里面，实际上这里有两个queue，分别是batchedOperations， nonBatchedOperations。 nonBatchedOperations 可以用job或thread 并行执行，主要是
 * 创建元素等。 而batchedOperations 依赖nonBatchedOperations的结果， 必须一次完成。
 *
 * CreateViewOperation/UpdateViewOperation 等使用struct 是考虑到：他们从创建到销毁的时间极短，故struct 比class 更加合适
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSDK.RNU
{


    interface UIOperation
    {
        void execute(UIViewOperationQueue oq);
    }


    public struct CreateViewOperation : UIOperation
    {

        private int tag;
        private string className;
        private Dictionary<string, object> props;
        public CreateViewOperation(int tag, string className, Dictionary<string, object> props)
        {
            this.tag = tag;
            this.className = className;
            this.props = props;
        }

        public void execute(UIViewOperationQueue oq)
        {
            ViewManager vm = oq.viewManagersMap[className];
            BaseView view = vm.CreateView();
            view.SetId(tag);
            oq.allViews.Add(tag, view);

            // 由于rootPanel需要特殊处理，
            if (props.ContainsKey("isRoot") && (bool) props["isRoot"])
            {
                var root = oq.allViews[tag];
                var rootPanel = root.GetGameObject();

                rootPanel.name = (string)props["name"];

                RNUMainCore.SetGumihoRootGo(rootPanel);
                
                // 根节点raycastTarget 设置为true 防止点击穿透
                Image rootImage = rootPanel.AddComponent<Image>();
                rootImage.color = new Color32(0, 0, 0, 0);
                rootImage.raycastTarget = true;

                var parentTran = RNUMainCore.GetPageParentGo().transform;
                rootPanel.transform.SetParent(parentTran, false);

                //TODO 否则debugRoot 有可能在其后
                var debugRoot = GameObject.Find("DebugRoot");
                if (debugRoot != null)
                {
                    debugRoot.transform.SetAsLastSibling();
                }
            }
            else
            {
                vm.UpdateProperties(view, props);
            }
        }
    }

    public struct UpdateViewOperation : UIOperation
    {

        private int tag;
        private Dictionary<string, object> props;
        private string className;
        public UpdateViewOperation(int tag, string className, Dictionary<string, object> props)
        {
            this.tag = tag;
            this.props = props;
            this.className = className;
        }
        public void execute(UIViewOperationQueue oq)
        {
            ViewManager vm = oq.viewManagersMap[className];
            BaseView view = oq.allViews[tag];
            vm.UpdateProperties(view, props);
        }
    }

    public struct SetChildrenOperation : UIOperation
    {
        private int parentTag;
        private List<int> childrenTags;

        public SetChildrenOperation(int parentTag, List<int> childrenTags)
        {
            this.parentTag = parentTag;
            this.childrenTags = childrenTags;
        }

        public void execute(UIViewOperationQueue oq)
        {
            BaseView parent = oq.allViews[parentTag];

            for (int i = 0; i < childrenTags.Count; i++)
            {
                BaseView child = oq.allViews[(int)childrenTags[i]];
                parent.Add(child);
            }
        }
    }

    public struct UpdateLayoutOperation: UIOperation {
        private int tag;
        private string className;
        private int x;
        private int y;
        private int width;
        private int height;

        public UpdateLayoutOperation(int tag, string className, int x, int y, int width, int height) {
            this.tag = tag;
            this.className = className;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void execute(UIViewOperationQueue oq) {
            BaseView view = oq.allViews[tag];
            var vm = oq.viewManagersMap[className];
            vm.UpdateLayout(view, x, y, width, height);
        }
    }

    public struct ManageChildrenOperation : UIOperation
    {
        private ViewAtIndex[] viewsToRemove;
        private ViewAtIndex[] viewsToAdd;
        private ArrayList tagsToDelete;
        private int parentTag;

        public ManageChildrenOperation(int parentTag, ViewAtIndex[] viewsToRemove, ViewAtIndex[] viewsToAdd, ArrayList tagsToDelete)
        {
            this.parentTag = parentTag;
            this.viewsToRemove = viewsToRemove;
            this.viewsToAdd = viewsToAdd;
            this.tagsToDelete = tagsToDelete;
        }

        public void execute(UIViewOperationQueue oq)
        {
            BaseView parent = oq.allViews[parentTag];

            for (int i = viewsToRemove.Length - 1; i >= 0; i--)
            {
                ViewAtIndex toRemove = viewsToRemove[i];
                BaseView toRemoveView = oq.allViews[toRemove.tag];
                parent.Unset(toRemoveView);
            }

            for (int i = 0; i < viewsToAdd.Length; i++)
            {
                ViewAtIndex toAdd = viewsToAdd[i];
                BaseView toAddView = oq.allViews[toAdd.tag];
                parent.Insert(toAdd.index, toAddView);
            }

            for (int i = 0; i < tagsToDelete.Count; i++)
            {
                int tagToDelete = (int) tagsToDelete[i];
                BaseView goToDelete = oq.allViews[tagToDelete];
                parent.Unset(goToDelete);
                goToDelete.Destroy();
                oq.allViews.Remove(tagToDelete);
            }
        }
    }


    public struct ViewCommandOperation : UIOperation
    {
        public int tag;
        private string command;
        private string className;
        private ArrayList commandArgs;
        private Promise promise;
        public ViewCommandOperation(int tag, string className, string command, ArrayList commandArgs, Promise promise = null)
        {
            this.tag = tag;
            this.className = className;
            this.command = command;
            this.commandArgs = commandArgs;
            this.promise = promise;
        }

        public void execute(UIViewOperationQueue oq)
        {
            var view = oq.allViews[tag];
            var vm = oq.viewManagersMap[className];

            vm.ReceiveCommand(view, command, commandArgs, promise);
        }
    }



    public class UIViewOperationQueue
    {
        private List<UIOperation> batchedOperations = new List<UIOperation>();
        private List<UIOperation> nonBatchedOperations = new List<UIOperation>();

        public Dictionary<int, BaseView> allViews = new Dictionary<int, BaseView>();

        public Dictionary<string, ViewManager> viewManagersMap;
        public GameObject canvasObject;

        private bool isGameParentGo = false;
        public UIViewOperationQueue(Dictionary<string, ViewManager> viewManagersMap)
        {
            this.viewManagersMap = viewManagersMap;
        }

        public void HandleSetChildren(SetChildrenOperation so)
        {
            batchedOperations.Add(so);
        }

        public void HandleCreateView(CreateViewOperation cv)
        {
            nonBatchedOperations.Add(cv);
        }

        public void HandleUpdateView(UpdateViewOperation uv)
        {
            batchedOperations.Add(uv);
        }

        public void HandleManageChildren(ManageChildrenOperation mv)
        {
            batchedOperations.Add(mv);
        }

        public void EnqueueUpdateLayout(int tag, string className, int x, int y, int width, int height) {
            batchedOperations.Add(new UpdateLayoutOperation(tag, className, x, y, width, height));
        }

        public void HandleViewCommand(ViewCommandOperation vc)
        {
            // 保证command的调用时机。当组件已经存在的情况下，优先调用。 当组件还未渲染的时候 在nonBatchedOperations 结束之后调用
            if (allViews.ContainsKey(vc.tag))
            { 
                nonBatchedOperations.Insert(0, vc);
            }
            else
            { 
                batchedOperations.Add(vc);
            }
        }

        public void DispatchViewUpdates()
        {
            Util.Log("DispatchViewUpdates: {0}  {1}", nonBatchedOperations.Count, batchedOperations.Count);
            foreach (UIOperation op in nonBatchedOperations)
            {
                op.execute(this);
            }
            nonBatchedOperations = new List<UIOperation>();

            foreach (UIOperation op in batchedOperations)
            {
                op.execute(this);
            }
            batchedOperations = new List<UIOperation>();
            Util.Log("allViews count: {0}", allViews.Count);
        }

        public void DestroyAllUIs() {
            foreach(KeyValuePair<int, BaseView> kv in allViews) {
                BaseView bv = kv.Value;
                bv.Destroy();
            }
            allViews.Clear();
        }
    }


}

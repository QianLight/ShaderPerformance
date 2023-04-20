using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GSDK.RNU
{
    public class UiManagerModule : SimpleUnityModule
    {
        private static Dictionary<string, string> TypeMaps = new Dictionary<string, string>{
            {"System.Int32", "number"},
            {"System.Int64", "number"},
            {"System.String", "String"},
            {"System.Boolean", "boolean"},
            {"System.Single", "number"},
            {"System.Double", "number"},
            {"System.Collections.Generic.Dictionary`2", "object"},
            {"System.Collections.ArrayList", "Array"},
        };
        private Dictionary<int, ReactSimpleShadowNode> allNodes = new Dictionary<int, ReactSimpleShadowNode>();

        private Dictionary<string, ViewManager> viewManagersMap;
        private Hashtable mModuleConstants;

        private UIViewOperationQueue operationQueue;

        private HashSet<int> rootTags = new HashSet<int>();

        private ViewAtIndexOrder viewAtIndexOrder = new ViewAtIndexOrder();

        public UiManagerModule(List<ViewManager> viewManagers)
        {
            viewManagersMap = new Dictionary<string, ViewManager>();
            foreach (ViewManager vm in viewManagers)
            {
                viewManagersMap.Add(vm.GetName(), vm);
            }
            operationQueue = new UIViewOperationQueue(viewManagersMap);
            mModuleConstants = createConstants(viewManagers);
        }

        public Hashtable createConstants(List<ViewManager> viewManagers)
        {
            Hashtable r = new Hashtable();


            foreach (ViewManager viewManager in viewManagers)
            {


                Hashtable item = new Hashtable(viewManager.GetFlexProps());
                Dictionary<string, MethodInfo> propsSetter = viewManager.GetPropSetters();

                foreach (KeyValuePair<string, MethodInfo> kv in propsSetter)
                {
                    // FlexProps 有可能已经存在此属性
                    if (item.ContainsKey(kv.Key))
                    {
                        continue;
                    }
                    MethodInfo m = kv.Value;
                    ParameterInfo[] infos = m.GetParameters();

                    var fullName = infos[1].ParameterType.Namespace + "." + infos[1].ParameterType.Name;
                    var typeStr = TypeMaps[fullName];

                    if (typeStr == null)
                    {
                        Util.LogError("not support type: {0}", fullName);
                        continue;
                    }
                    
                    item.Add(kv.Key, typeStr);
                }

                r.Add(viewManager.GetName(), new Hashtable{
                    {"NativeProps", item},
                    {"directEventTypes", viewManager.GetExportedCustomDirectEventTypeConstants()}
                });
            }
            return r;
        }

        override public string GetName()
        {
            return "UIManager";
        }

        public override Hashtable GetConstants()
        {
            return mModuleConstants;
        }

        public int GetRootPanelTag(string rootName)
        {
            var rootTag = GetNextTag();
            rootTags.Add(rootTag);
            ReactViewLikeShadowNode rootUsn = new ReactViewLikeShadowNode(rootTag);
            rootUsn.SetClassName(ReactViewManager.ReactViewName);
            Rect parentRect = RNUMainCore.GetGameGoParentRect();
            rootUsn.UpdateYogaNodePropsAndGetOtherProps(new Dictionary<string, object>
            {
                {"height", parentRect == Rect.zero ? Screen.height: parentRect.height},
                {"width", parentRect == Rect.zero ? Screen.width: parentRect.width}
            });

            allNodes.Add(rootTag, rootUsn);

            // 创建 Root节点
            operationQueue.HandleCreateView(new CreateViewOperation(rootTag, ReactViewManager.ReactViewName, new Dictionary<string, object>()
            {
                {"name", rootName},
                {"isRoot", true}
            }));
            return rootTag;
        }

        [ReactMethod]
        public void createView(int tag, string className, int rootViewTag, Dictionary<string, object> props)
        {
            ViewManager vm = viewManagersMap[className];
            ReactSimpleShadowNode usn =  vm.CreateShadowNode(tag);
            usn.SetClassName(className);
            Dictionary<string, object>  otherProps = usn.UpdateYogaNodePropsAndGetOtherProps(props);
            allNodes.Add(tag, usn);

            if (!usn.isVirtual()) {
                operationQueue.HandleCreateView(new CreateViewOperation(tag, className, otherProps));
            }
        }

        [ReactMethod]
        public void updateView(int tag, string className, Dictionary<string, object> props)
        {
            if (!allNodes.ContainsKey(tag))
            { 
                // ReactCommand 和 JS-driver动画，有可能修改到已经移除节点的属性。
                Util.Log("updateView {0} has destory!", tag);
                return;
            }
            
            ReactSimpleShadowNode usn = allNodes[tag];

            Dictionary<string, object> otherProps = usn.UpdateYogaNodePropsAndGetOtherProps(props);

            if (!usn.isVirtual()) {
                operationQueue.HandleUpdateView(new UpdateViewOperation(tag, className, otherProps));
            }
        }

        // 参考 react native 的部分实现
        [ReactMethod]
        public void manageChildren(int parentTag, ArrayList moveFrom, ArrayList moveTo, ArrayList addChildTags, ArrayList addAtIndices, ArrayList removeFrom)
        {
            int numToMove = moveFrom == null ? 0 : moveFrom.Count;
            int numToAdd = addChildTags == null ? 0 : addChildTags.Count;
            int numToRemove = removeFrom == null ? 0 : removeFrom.Count;
            // move == remove  + add
            ViewAtIndex[] viewsToAdd = new ViewAtIndex[numToMove + numToAdd];
            ViewAtIndex[] viewsToRemove = new ViewAtIndex[numToMove + numToRemove];

            int[] tagsToDelete = new int[numToRemove];

            ReactSimpleShadowNode parentUSN =  allNodes[parentTag];

            if (numToMove > 0) {
                for (int i = 0; i < numToMove; i++) {
                    int moveFromIndex = (int) moveFrom[i];

                    int tagToMove = parentUSN.GetChildAt(moveFromIndex).tag;
                    viewsToAdd[i] = new ViewAtIndex(tagToMove,(int)moveTo[i]);
                    viewsToRemove[i] = new ViewAtIndex(tagToMove, moveFromIndex);
                }
            }

            if (numToAdd > 0) {
                for (int i = 0; i < numToAdd; i++) {
                    int viewTagToAdd = (int)addChildTags[i];
                    int indexToAddAt = (int)addAtIndices[i];
                    viewsToAdd[numToMove + i] = new ViewAtIndex(viewTagToAdd, indexToAddAt);
                }
            }

            if (numToRemove > 0) {
                for (int i = 0; i < numToRemove; i++) {
                    int indexToRemove = (int)removeFrom[i];
                    int tagToRemove = parentUSN.GetChildAt(indexToRemove).tag;
                    viewsToRemove[numToMove + i] = new ViewAtIndex(tagToRemove, indexToRemove);
                    tagsToDelete[i] = tagToRemove;
                }
            }

            Array.Sort<ViewAtIndex>(viewsToRemove, viewAtIndexOrder);
            Array.Sort<ViewAtIndex>(viewsToAdd, viewAtIndexOrder);

            for (int i = viewsToRemove.Length - 1; i >= 0; i--) {
                ViewAtIndex toRemove = viewsToRemove[i];
                parentUSN.RemoveChildAt(toRemove.index);
            }

            for (int i = 0; i < viewsToAdd.Length; i++) {
                ViewAtIndex toAdd = viewsToAdd[i];
                ReactSimpleShadowNode childUSN = allNodes[toAdd.tag];
                parentUSN.Insert(toAdd.index, childUSN);
            }

            ArrayList realNodeToDelete = new ArrayList();
            foreach (var t in tagsToDelete)
            {
                ReactSimpleShadowNode childToDelete =  allNodes[t];
                // 清理yoga
                childToDelete.Destory();
                
                RemoveShadowNodeRecursive(t, realNodeToDelete);
            }

            operationQueue.HandleManageChildren(
                new ManageChildrenOperation(
                    parentTag,
                    viewsToRemove,
                    viewsToAdd,
                    realNodeToDelete
                )
            );
        }

        private void RemoveShadowNodeRecursive(int tagToDelete, ArrayList realDeleteList)
        {
            ReactSimpleShadowNode usn = allNodes[tagToDelete];
            allNodes.Remove(tagToDelete);

            if (!usn.isVirtual())
            {
                realDeleteList.Add(tagToDelete);
            }

            foreach (var child in usn.children)
            {
                RemoveShadowNodeRecursive(child.tag, realDeleteList);
            }
        }

        [ReactMethod]
        public void setChildren(int parentTag, ArrayList childrenTags)
        {
            List<int> childrenRealNode = new List<int>();
            ReactSimpleShadowNode parentNode = allNodes[parentTag];
            for (int i = 0; i < childrenTags.Count; i++)
            {
                ReactSimpleShadowNode usn = allNodes[(int)childrenTags[i]];

                parentNode.Insert(i, usn);

                if (!usn.isVirtual()) {
                    childrenRealNode.Add(usn.tag);
                }
            }

            operationQueue.HandleSetChildren(new SetChildrenOperation(parentTag, childrenRealNode));
        }

        [ReactMethod]
        public void measure()
        {

        }

        [ReactMethod]
        public void measureInWindow()
        {

        }

        [ReactMethod]
        public void measureLayout()
        {

        }

        [ReactMethod]
        public void showPopupMenu()
        {

        }

        [ReactMethod]
        public void dispatchViewManagerCommand(int tag, string command, ArrayList commandArgs)
        {
            var rsn = allNodes[tag];
            operationQueue.HandleViewCommand(new ViewCommandOperation(tag, rsn.className, command, commandArgs));
        }
        
        [ReactMethod(true)]
        public void dispatchViewManagerCommandAsync(int tag, string command, ArrayList commandArgs, Promise promise)
        {
            var rsn = allNodes[tag];
            operationQueue.HandleViewCommand(new ViewCommandOperation(tag, rsn.className, command, commandArgs, promise));
        }


        public void OnBatchComplete()
        {
            long start;
            ExtraUpdaterShadowNodeQueue.execute(operationQueue);
            foreach (var rootTag in rootTags)
            {
                ReactSimpleShadowNode rootUSN = allNodes[rootTag];

                start = DateTime.Now.Ticks / 10000;
                rootUSN.CalculateLayout();
                Util.Log("yk-duration YogaCalculateLayout: {0}", DateTime.Now.Ticks / 10000 - start);

                ApplyUpdatesRecursive(rootUSN);
            }
            // 应用所有更新
            start = DateTime.Now.Ticks / 10000;
            operationQueue.DispatchViewUpdates();
            Util.Log("yk-duration DispatchViewUpdates unity: {0}", DateTime.Now.Ticks / 10000 - start);
        }

        // TODO 需要测试 先放child 还是先放parent的性能好
        private void ApplyUpdatesRecursive(ReactSimpleShadowNode usn) {
            if (usn.isVirtual()) {
                return;
            }

            usn.DispatchUpdates(operationQueue);

            for(int i = 0; i < usn.children.Count; i ++ ) {
                ReactSimpleShadowNode child = usn.children[i];
                ApplyUpdatesRecursive(child);
            }
        }

        public ReactSimpleShadowNode GetShadowNodeByTag(int tag)
        {
            return allNodes[tag];
        }

        private int GetNextTag()
        {
            var maxKey = 0;
            var allNodesKeys = allNodes.Keys;
            foreach (var key in allNodesKeys)
            {
                if (key > maxKey)
                {
                    maxKey = key;
                }
            }

            return maxKey + 1;
        }

        public override void Destroy() {
            operationQueue.DestroyAllUIs();

            // 清理ExtraUpdaterQueue
            ExtraUpdaterShadowNodeQueue.ClearAll();

            foreach (var rootTag in rootTags)
            {
                // 有可能启动时候失败，导致Destroy，那么此时rootTag 有可能还没有创建
                if (allNodes.ContainsKey(rootTag))
                {
                    // 清理yoga 节点
                    ReactSimpleShadowNode rootUSN = allNodes[rootTag];
                    rootUSN.Destory();
                }
            }

            allNodes.Clear();
        }
    }
}

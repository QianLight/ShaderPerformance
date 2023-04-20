using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFEngine.Editor
{
    public delegate void PreProcess<T> (ref AssetListContext<T> context, T data) where T : BaseAssetConfig;
    public delegate void HeadGUI<T> (ref Rect rect, ref AssetListContext<T> context, T data) where T : BaseAssetConfig;
    public delegate void ElementGUI<T> (ref ListElementContext lec, ref AssetListContext<T> context, T data, int i) where T : BaseAssetConfig;
    public enum EInsertState
    {
        None,
        OnInsert,
        InsertBefore,
        InsertAfter,
    }
    public delegate void OnCopy (ref ListEditContext context);
    public delegate void OnInsertBefore (ref ListEditContext context);
    public delegate void OnInsertAfter (ref ListEditContext context);

    public struct ListEditContext
    {
        //public EInsertState insertState;
        public int selectIndex;
        public IList srcList;
        public int targetIndex;
        public IList targetList;
        public OnCopy onCopy;
        public OnInsertBefore onInsertBefore;
        public OnInsertAfter onInsertAfter;

        public bool canCopy;
        public bool canInsert;
        public bool canSwitch;

        public void Reset ()
        {
            selectIndex = -1;
            srcList = null;
            targetIndex = -1;
            targetList = null;
        }
        public void OnGUI (ref ListElementContext lec, IList list, int index)
        {
            if (selectIndex == -1)
            {
                //no insert
                if (ToolsUtility.Button (ref lec, "S", 20))
                {
                    selectIndex = index;
                    srcList = list;
                    targetIndex = -1;
                    targetList = null;
                }
            }
            else
            {

                if (srcList == list && selectIndex == index)
                {
                    if (ToolsUtility.Button (ref lec, "R", 20))
                    {
                        Reset ();
                    }
                }
                else
                {
                    if (canCopy && ToolsUtility.Button (ref lec, "P", 20))
                    {
                        targetIndex = index;
                        targetList = list;
                        if (onCopy != null)
                        {
                            onCopy (ref this);
                        }
                        Reset ();
                    }
                    if (canSwitch && ToolsUtility.Button (ref lec, "SW", 30))
                    {
                        targetIndex = index;
                        targetList = list;
                        Switch ();
                        Reset ();
                    }
                    if (canInsert)
                    {
                        if (ToolsUtility.Button (ref lec, "B", 20))
                        {
                            targetIndex = index;
                            targetList = list;
                            InsertBefore ();
                            if (onInsertBefore != null)
                            {
                                onInsertBefore (ref this);
                            }
                            Reset ();
                        }
                        if (ToolsUtility.Button (ref lec, "A", 20))
                        {
                            targetIndex = index;
                            targetList = list;
                            InsertAfter ();
                            if (onInsertAfter != null)
                            {
                                onInsertAfter (ref this);
                            }
                            Reset ();
                        }
                    }
                }

            }
        }

        private void Switch (IList lst, int i, int j)
        {
            var obj = lst[j];
            lst[j] = lst[i];
            lst[i] = obj;
        }
        private void Switch ()
        {
            if (srcList == targetList)
            {
                Switch (srcList, selectIndex, targetIndex);
            }
            else
            {
                var obj = targetList[targetIndex];
                targetList[targetIndex] = srcList[selectIndex];
                srcList[selectIndex] = obj;
            }
        }
        private void InsertBefore ()
        {
            if (srcList == targetList)
            {
                if (selectIndex < targetIndex)
                {
                    for (int i = selectIndex; i < targetIndex - 1; ++i)
                    {
                        Switch (srcList, i, i + 1);
                    }
                }
                else if (selectIndex > targetIndex)
                {
                    for (int i = selectIndex; i > targetIndex; --i)
                    {
                        Switch (srcList, i, i - 1);
                    }
                }
            }
            else
            {
                var obj = srcList[selectIndex];
                srcList.RemoveAt (selectIndex);
                targetList.Insert (targetIndex, obj);
            }
        }

        private void InsertAfter ()
        {
            if (srcList == targetList)
            {
                if (selectIndex < targetIndex)
                {
                    for (int i = selectIndex; i < targetIndex; ++i)
                    {
                        Switch (srcList, i, i + 1);
                    }
                }
                else if (selectIndex > targetIndex)
                {
                    for (int i = selectIndex; i > targetIndex + 1; --i)
                    {
                        Switch (srcList, i, i - 1);
                    }
                }
            }
            else
            {
                var obj = srcList[selectIndex];
                srcList.RemoveAt (selectIndex);
                if (targetIndex < targetList.Count - 1)
                {
                    targetList.Insert (targetIndex + 1, obj);
                }
                else
                {
                    targetList.Add (obj);
                }

            }
        }
    }

    public struct AssetListContext<T> where T : BaseAssetConfig
    {
        public string name;
        public PreProcess<T> preProcess;
        public HeadGUI<T> headGUI;
        public ElementGUI<T> elementGUI;
        public bool needDelete;
        public bool needAdd;
        public float defaultHeight;
    }

    public class CommonListEditor<T> : ReorderableList where T : BaseAssetConfig
    {
        public T data;
        public AssetListContext<T> context;
        private ListElementContext listContext;
        private Vector2 scroll = Vector2.zero;
        public CommonListEditor (T data, ref AssetListContext<T> context) : base (data.GetList (), data.GetListType ())
        {
            this.data = data;
            this.context = context;
            drawHeaderCallback = OnHeaderGui;
            drawElementCallback = OnElementGui;
            elementHeightCallback = ElementHeightCb;
            drawElementBackgroundCallback = OnElementBackGui;

            onAddCallback = OnAdd;
            onRemoveCallback = OnRemove;
            onReorderCallback = OnReorder;
            displayRemove = context.needDelete;
            displayAdd = context.needAdd;
            this.context.defaultHeight = elementHeight;
        }
        public void Draw (FolderConfig folder, ref Rect rect)
        {
            if (context.preProcess != null)
            {
                context.preProcess (ref context, data);
            }
            if (folder.Folder (data.name, data.name))
            {
                float height = GetHeight ();
                if (height > 800)
                    height = 800;
                EditorCommon.BeginScroll (ref scroll, this.count, 10, height, rect.width - 20);
                DoLayoutList ();
                EditorCommon.EndScroll ();
            }
        }

        void OnHeaderGui (Rect rect)
        {
            if (context.headGUI != null)
            {
                context.headGUI (ref rect, ref context, data);
            }
        }
        void OnElementGui (Rect rect, int index, bool isActive, bool isFocused)
        {
            if (context.elementGUI != null)
            {
                listContext.rect = rect;
                listContext.draw = true;
                context.elementGUI (ref listContext, ref context, data, index);
            }
        }
        void OnElementBackGui (Rect rect, int index, bool isActive, bool isFocused)
        {
            Handles.color = Color.gray;
            Vector2 pos0 = rect.position + new Vector2 (0, rect.height - 1);
            Vector2 pos1 = rect.position + new Vector2 (rect.width, rect.height - 1);
            Handles.DrawLine (pos0, pos1);
        }

        float ElementHeightCb (int index)
        {
            if (context.elementGUI != null)
            {
                listContext.height = 0;
                listContext.draw = false;
                context.elementGUI (ref listContext, ref context, data, index);
                return listContext.height;
            }

            // if (context.elementHeightCb != null)
            // {
            //     //return context.elementHeightCb (ref context, data, index);
            // }
            return elementHeight;
        }
        void OnAdd (ReorderableList list)
        {
            data.OnAdd ();
        }
        void OnRemove (ReorderableList list)
        {
            if (context.needDelete)
                ReorderableList.defaultBehaviours.DoRemoveButton (list);
            data.OnRemove ();
        }
        void OnReorder (ReorderableList list)
        {
            data.OnReorder ();
        }
    }
}
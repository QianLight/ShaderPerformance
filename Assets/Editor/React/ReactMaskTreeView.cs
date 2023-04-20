#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XEditor
{
    class ReactMaskTreeView : TreeView
    {
        public ReactMaskTreeView(TreeViewState treeViewState)
        : base(treeViewState)
        {
            showBorder = true;

            Reload();
        }

        public static XReactMaskEditor editor;

        protected override TreeViewItem BuildRoot()
        {
            if (editor == null || editor.list == null || editor.list.Count == 0) return null;
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Mask" };
            var allItems = new List<TreeViewItem>();
            for (int i = 0; i < editor.list.Count; ++i)
            {
                var item = editor.list[i];
                allItems.Add(new TreeViewItem { id = item.lineID, depth = item.depth, displayName = item.bone });
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;

        }

        // Custom GUI

        protected override void RowGUI(RowGUIArgs args)
        {
            Event evt = Event.current;
            extraSpaceBeforeIconAndLabel = 18f;

            // GameObject isStatic toggle 
            //var gameObject = GetGameObject(args.item.id);
            //if (gameObject == null)
            //    return;

            Rect toggleRect = args.rowRect;
            toggleRect.x += GetContentIndent(args.item);
            toggleRect.width = 16f;

            // Ensure row is selected before using the toggle (usability)
            if (evt.type == EventType.MouseDown && toggleRect.Contains(evt.mousePosition))
                SelectionClick(args.item, false);

            EditorGUI.BeginChangeCheck();

            var data = editor.list[args.item.id];
            bool isSelectd = EditorGUI.Toggle(toggleRect, data.seleced);
            if (EditorGUI.EndChangeCheck())
            {
                data.seleced = isSelectd;

                if (evt.control)
                {
                    CtrlSelects(data, isSelectd);                    
                }
            }

            // Text
            base.RowGUI(args);
        }


        GameObject GetGameObject(int instanceID)
        {
            return (GameObject)EditorUtility.InstanceIDToObject(instanceID);
        }

        // Selection

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            //Selection.instanceIDs = selectedIds.ToArray();
        }

        void CtrlSelects(XReactMaskEditor.PathWeight pair, bool selectd)
        {
            pair.seleced = selectd;
            if (pair.Childs.Count > 0)
            {
                for (int i = 0; i < pair.Childs.Count; ++i)
                {
                    CtrlSelects(editor.list[pair.Childs[i]], selectd);
                }
            }
        }
    }
}

#endif
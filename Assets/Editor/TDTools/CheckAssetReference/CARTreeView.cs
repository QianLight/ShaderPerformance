using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Assertions;
using XEditor;
using CFClient;
using CFEngine;
using CFUtilPoolLib;

namespace TDTools
{
    internal class CARTreeView : CARTreeViewWithTreeModel<TreeViewElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;
        public bool showControls = true;

        static Texture2D[] s_Icons = 
        {
			EditorGUIUtility.FindTexture ("Folder Icon"),
			EditorGUIUtility.FindTexture ("Prefab Icon"),
			EditorGUIUtility.FindTexture ("d_PrefabModel Icon"),
			EditorGUIUtility.FindTexture ("GameObject Icon"),
            EditorGUIUtility.FindTexture ("SceneViewCamera@2x"),
            EditorGUIUtility.FindTexture ("cs Script Icon")
		};

        enum Columns
        {
            Icon,
            Name,
            OpenButton
        }

        public enum SortOption
        {
            Name
        }

        SortOption[] m_SortOptions = 
        {
            SortOption.Name,
            SortOption.Name,
            SortOption.Name
        };

        public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException ("root");
            if (result == null)
                throw new NullReferenceException ("result");

            result.Clear ();

            if (root.children == null)
                return;
            
            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; --i)
                stack.Push (root.children[i]);
            
            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop ();
                result.Add (current);
                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; --i)
                        stack.Push (current.children[i]);
                }
            }
        }

        public CARTreeView (TreeViewState state, MultiColumnHeader multiColumnHeader, CARTreeModel<TreeViewElement> model) : base (state, multiColumnHeader, model)
        {
            Assert.AreEqual (m_SortOptions.Length, Enum.GetValues (typeof(Columns)).Length, "Ensure number of sort options are in sync with number of Columns enum values.");

            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multiColumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        void OnSortingChanged (MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded (rootItem, GetRows());
        }

        void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1) return;
            if (multiColumnHeader.sortedColumnIndex == -1) return;

            SortByMultipleColumns ();
            TreeToList (root, rows);
            Repaint ();
        }

        void SortByMultipleColumns ()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<CARTreeViewItem<TreeViewElement> >();
            var orderedQuery = InitialOrder (myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; ++i)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending (sortedColumns[i]);
                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy (l => l.data.name, ascending);
                        break;
                }
            }
            rootItem.children = orderedQuery.Cast<TreeViewItem> ().ToList ();
        }

        IOrderedEnumerable<CARTreeViewItem<TreeViewElement>> InitialOrder (IEnumerable<CARTreeViewItem<TreeViewElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending (history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order (l => l.data.name, ascending);
                default:
                    Assert.IsTrue (false, "Unhandled enum");
                    break;
            }
            return myTypes.Order (l => l.data.name, ascending);
        }

        protected override void RowGUI (RowGUIArgs args)
        {
            var item = (CARTreeViewItem<TreeViewElement>) args.item;

            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
            {
                CellGUI (args.GetCellRect (i), item, (Columns)args.GetColumn (i), ref args);
            }
        }

        void CellGUI (Rect cellRect, CARTreeViewItem<TreeViewElement> item, Columns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight (ref cellRect);

            switch (column)
            {
                case Columns.Icon:
                    GUI.DrawTexture (cellRect, s_Icons[GetIconIndex (item)], ScaleMode.ScaleToFit);
                    break;
                case Columns.Name:
                    args.rowRect = cellRect;
                    base.RowGUI (args);
                    break;
                case Columns.OpenButton:
                    if (GUI.Button (cellRect, "Open"))
                    {
                        item.data.Open ();
                    }
                    break;
            }
        }

        int GetIconIndex (CARTreeViewItem<TreeViewElement> item)
        {
            if (!item.data.path.Contains ("."))
                return 0;
            if (item.data.path.EndsWith (".prefab"))
                return 1;
            if (item.data.path.EndsWith (".anim"))
                return 2;
            if (item.data.path.EndsWith (".playable"))
                return 4;
            if (item.data.path.EndsWith (".bytes"))
                return 5;
            if (item.data.path.EndsWith (".cfg"))
                return 5;
            return 3;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState (float treeViewWidth)
        {
            var columns = new []
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent ("Type", "Type of the asset."),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 50,
                    minWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent ("Name", "Name of asset."),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 90,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent ("Open", "Open the asset."),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 60,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                }
            };

            Assert.AreEqual (columns.Length, Enum.GetValues (typeof (Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState (columns);
            return state;
        }
    }
}
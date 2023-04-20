//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEditor;
//using UnityEditor.IMGUI.Controls;
//using UnityEngine;

//namespace CFEngine.Editor
//{
//    [Serializable]
//    public class ResTreeElement : TreeElement
//    {
//        public ResItem ri;
//        public string sizeStr = "";
//        public string totalSizeStr = "";
//        public bool asc = false;

//        public ResTreeElement (ResItem ri, int depth, int id) : base (ri.name, depth, id)
//        {
//            this.ri = ri;
//        }
//    }
//    public class ResMultiColumnHeader : MultiColumnHeader
//    {
//        Mode m_Mode;

//        public enum Mode
//        {
//            LargeHeader,
//            DefaultHeader,
//            MinimumHeaderWithoutSorting
//        }

//        public ResMultiColumnHeader (MultiColumnHeaderState state) : base (state)
//        {
//            mode = Mode.DefaultHeader;
//        }

//        public Mode mode
//        {
//            get
//            {
//                return m_Mode;
//            }
//            set
//            {
//                m_Mode = value;
//                switch (m_Mode)
//                {
//                    case Mode.LargeHeader:
//                        canSort = true;
//                        height = 37f;
//                        break;
//                    case Mode.DefaultHeader:
//                        canSort = true;
//                        height = DefaultGUI.defaultHeight;
//                        break;
//                    case Mode.MinimumHeaderWithoutSorting:
//                        canSort = false;
//                        height = DefaultGUI.minimumHeight;
//                        break;
//                }
//            }
//        }

//        protected override void ColumnHeaderGUI (MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
//        {
//            // Default column header gui
//            base.ColumnHeaderGUI (column, headerRect, columnIndex);

//            // Add additional info for large header
//            if (mode == Mode.LargeHeader)
//            {
//                // Show example overlay stuff on some of the columns
//                if (columnIndex > 1)
//                {
//                    headerRect.xMax -= 3f;
//                    var oldAlignment = EditorStyles.largeLabel.alignment;
//                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
//                    GUI.Label (headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
//                    EditorStyles.largeLabel.alignment = oldAlignment;
//                }
//            }
//        }
//    }
//    public class ResTreeView : TreeViewWithTreeModel<ResTreeElement>
//    {
//        const float kRowHeights = 20f;
//        const float kToggleWidth = 18f;
//        enum ResColumns
//        {
//            Name,
//            Asset,
//            Size,
//            TotalSize,
//            Status,
//        }
//        public enum SortOption
//        {
//            Name,
//            Asset,
//            Size,
//            TotalSize,
//            State,
//        }

//        // Sort options per column
//        SortOption[] m_SortOptions = {
//            SortOption.Name,
//            SortOption.Asset,
//            SortOption.Size,
//            SortOption.TotalSize,
//            SortOption.State,
//        };

//        private int sortColumnIndex = -1;
//        private TreeViewItem<ResTreeElement> sortItem = null;
//        public ResTreeView (TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<ResTreeElement> model) : base (state, multicolumnHeader, model)
//        {
//            // Custom setup
//            rowHeight = kRowHeights;
//            columnIndexForTreeFoldouts = 0;
//            showAlternatingRowBackgrounds = true;
//            showBorder = true;
//            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
//            extraSpaceBeforeIconAndLabel = kToggleWidth;
//            //multicolumnHeader.sortingChanged += OnSortingChanged;

//            Reload ();
//        }

//        public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
//        {
//            if (root == null || result == null)
//                return;

//            result.Clear ();

//            if (root.children == null)
//                return;

//            Stack<TreeViewItem> stack = new Stack<TreeViewItem> ();
//            for (int i = root.children.Count - 1; i >= 0; i--)
//                stack.Push (root.children[i]);

//            while (stack.Count > 0)
//            {
//                TreeViewItem current = stack.Pop ();
//                result.Add (current);

//                if (current.hasChildren && current.children[0] != null)
//                {
//                    for (int i = current.children.Count - 1; i >= 0; i--)
//                    {
//                        stack.Push (current.children[i]);
//                    }
//                }
//            }
//        }
//        // protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
//        // {
//        //     var rows = base.BuildRows (root);
//        //     Repaint ();
//        //     return rows;
//        // }

//        // void OnSortingChanged (MultiColumnHeader multiColumnHeader)
//        // {
//        //     SortIfNeeded (rootItem, GetRows ());
//        // }

//        // void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
//        // {
//        //     if (rows.Count <= 1)
//        //         return;

//        //     if (multiColumnHeader.sortedColumnIndex == -1)
//        //     {
//        //         return; // No column to sort for (just use the order the data are in)
//        //     }

//        //     // Sort the roots of the existing tree items
//        //     // SortByMultipleColumns ();
//        //     // TreeToList (root, rows);
//        //     Repaint ();
//        // }

//        void SortByMultipleColumns ()
//        {
//            var sortedColumns = multiColumnHeader.state.sortedColumns;

//            if (sortedColumns.Length == 0)
//                return;

//            var myTypes = rootItem.children.Cast<TreeViewItem<ResTreeElement>> ();
//            var orderedQuery = InitialOrder (myTypes, sortedColumns);
//            for (int i = 0; i < sortedColumns.Length; i++)
//            {
//                SortOption sortOption = m_SortOptions[sortedColumns[i]];
//                bool ascending = multiColumnHeader.IsSortedAscending (sortedColumns[i]);

//                switch (sortOption)
//                {
//                    case SortOption.Name:
//                        orderedQuery = orderedQuery.ThenBy (l => l.data.name, ascending);
//                        break;
//                    case SortOption.Asset:
//                        orderedQuery = orderedQuery.ThenBy (l => l.data.ri.resType, ascending);
//                        break;
//                    case SortOption.Size:
//                        orderedQuery = orderedQuery.ThenBy (l => l.data.ri.size, ascending);
//                        break;
//                    case SortOption.TotalSize:
//                        orderedQuery = orderedQuery.ThenBy (l => l.data.ri.totalSize, ascending);
//                        break;
//                    case SortOption.State:
//                        orderedQuery = orderedQuery.ThenBy (l => l.data.ri.state, ascending);
//                        break;
//                }
//            }

//            rootItem.children = orderedQuery.Cast<TreeViewItem> ().ToList ();
//        }

//        IOrderedEnumerable<TreeViewItem<ResTreeElement>> InitialOrder (IEnumerable<TreeViewItem<ResTreeElement>> myTypes, int[] history)
//        {
//            SortOption sortOption = m_SortOptions[history[0]];
//            bool ascending = multiColumnHeader.IsSortedAscending (history[0]);
//            switch (sortOption)
//            {
//                case SortOption.Name:
//                    return myTypes.Order (l => l.data.name, ascending);
//                case SortOption.Asset:
//                    return myTypes.Order (l => l.data.ri.resType, ascending);
//                case SortOption.Size:
//                    return myTypes.Order (l => l.data.ri.size, ascending);
//                case SortOption.TotalSize:
//                    return myTypes.Order (l => l.data.ri.totalSize, ascending);
//                case SortOption.State:
//                    return myTypes.Order (l => l.data.ri.state, ascending);
//                default:
//                    // Assert.IsTrue (false, "Unhandled enum");
//                    break;
//            }

//            // default
//            return myTypes.Order (l => l.data.name, ascending);
//        }
//        public override void OnGUI (Rect rect)
//        {
//            base.OnGUI (rect);
//            if (sortColumnIndex >= 0 && sortItem != null)
//            {
//                var item = sortItem.data;
//                item.asc = !item.asc;
//                switch (sortColumnIndex)
//                {
//                    case 0:
//                        {
//                            var child = item.children;
//                            if (item.asc)
//                                child.Sort ((x, y) => x.name.CompareTo (y.name));
//                            else
//                                child.Sort ((x, y) => y.name.CompareTo (x.name));
//                        }
//                        break;
//                    case 1:
//                        {
//                            var child = item.children;
//                            if (item.asc)
//                                child.Sort ((x, y) => (x as ResTreeElement).ri.resType.CompareTo ((y as ResTreeElement).ri.resType));
//                            else
//                                child.Sort ((x, y) => (y as ResTreeElement).ri.resType.CompareTo ((x as ResTreeElement).ri.resType));
//                        }
//                        break;
//                    case 2:
//                        {
//                            var child = item.children;
//                            if (item.asc)
//                                child.Sort ((x, y) => (x as ResTreeElement).ri.size.CompareTo ((y as ResTreeElement).ri.size));
//                            else
//                                child.Sort ((x, y) => (y as ResTreeElement).ri.size.CompareTo ((x as ResTreeElement).ri.size));
//                        }
//                        break;
//                    case 3:
//                        {
//                            var child = item.children;
//                            if (item.asc)
//                                child.Sort ((x, y) => (x as ResTreeElement).ri.totalSize.CompareTo ((y as ResTreeElement).ri.totalSize));
//                            else
//                                child.Sort ((x, y) => (y as ResTreeElement).ri.totalSize.CompareTo ((x as ResTreeElement).ri.totalSize));
//                        }
//                        break;
//                    case 4:
//                        {
//                            var child = item.children;
//                            if (item.asc)
//                                child.Sort ((x, y) => (x as ResTreeElement).ri.state.CompareTo ((y as ResTreeElement).ri.state));
//                            else
//                                child.Sort ((x, y) => (y as ResTreeElement).ri.state.CompareTo ((x as ResTreeElement).ri.state));
//                        }
//                        break;
//                }
//                BuildRows (sortItem);
//                sortItem = null;
//                sortColumnIndex = -1;
//            }
//        }
//        protected override void RowGUI (RowGUIArgs args)
//        {
//            var item = (TreeViewItem<ResTreeElement>) args.item;

//            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
//            {
//                CellGUI (args.GetCellRect (i), item, (ResColumns) args.GetColumn (i), ref args);
//            }
//        }

//        void SortButtonGUI (ref Rect cellRect, TreeViewItem<ResTreeElement> item, int columnIndex)
//        {
//            if (item.children != null && item.children.Count > 0)
//            {
//                cellRect.xMin += cellRect.width + 5;
//                cellRect.width = 20;
//                if (GUI.Button (cellRect, item.data.asc? "A": "D"))
//                {
//                    sortItem = item;
//                    sortColumnIndex = columnIndex;
//                }
//            }
//        }

//        void CellGUI (Rect cellRect, TreeViewItem<ResTreeElement> item, ResColumns column, ref RowGUIArgs args)
//        {
//            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
//            CenterRectUsingSingleLineHeight (ref cellRect);
//            var c = GUI.color;
//            switch (column)
//            {
//                case ResColumns.Name:
//                    {
//                        cellRect.width -= 20;
//                        args.rowRect = cellRect;
//                        base.RowGUI (args);
//                        SortButtonGUI (ref cellRect, item, (int) column);
//                    }
//                    break;
//                case ResColumns.Asset:
//                    {
//                        cellRect.xMin += 5f; // When showing controls make some extra spacing
//                        cellRect.width -= 20;
//                        EditorGUI.ObjectField (cellRect, item.data.ri.res, typeof (UnityEngine.Object), false);
//                        SortButtonGUI (ref cellRect, item, (int) column);
//                    }
//                    break;
//                case ResColumns.Size:
//                    {
//                        cellRect.xMin += 5f;
//                        cellRect.width -= 20;
//                        EditorGUI.LabelField (cellRect, item.data.sizeStr);
//                        SortButtonGUI (ref cellRect, item, (int) column);
//                    }
//                    break;
//                case ResColumns.TotalSize:
//                    {
//                        cellRect.xMin += 5f;
//                        cellRect.width -= 20;
//                        EditorGUI.LabelField (cellRect, item.data.totalSizeStr);
//                        SortButtonGUI (ref cellRect, item, (int) column);
//                    }
//                    break;
//                case ResColumns.Status:
//                    {
//                        cellRect.xMin += 5f;
//                        cellRect.width -= 20;
//                        // if (item.data.ri.red)
//                        // {
//                        //     GUI.color = Color.red;
//                        // }
//                        EditorGUI.LabelField (cellRect, item.data.ri.state);
//                        SortButtonGUI (ref cellRect, item, (int) column);
//                    }
//                    break;

//            }
//            GUI.color = c;
//        }

//        // Rename
//        //--------

//        protected override bool CanRename (TreeViewItem item)
//        {
//            // Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
//            Rect renameRect = GetRenameRect (treeViewRect, 0, item);
//            return renameRect.width > 30;
//        }

//        protected override void RenameEnded (RenameEndedArgs args)
//        {
//            // Set the backend name and reload the tree to reflect the new model
//            if (args.acceptedRename)
//            {
//                var element = treeModel.Find (args.itemID);
//                element.name = args.newName;
//                Reload ();
//            }
//        }

//        protected override Rect GetRenameRect (Rect rowRect, int row, TreeViewItem item)
//        {
//            Rect cellRect = GetCellRectForTreeFoldouts (rowRect);
//            CenterRectUsingSingleLineHeight (ref cellRect);
//            return base.GetRenameRect (cellRect, row, item);
//        }

//        // Misc
//        //--------

//        protected override bool CanMultiSelect (TreeViewItem item)
//        {
//            return true;
//        }

//        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState ()
//        {
//            var columns = new []
//            {
//                new MultiColumnHeaderState.Column
//                {
//                headerContent = new GUIContent ("Name"),
//                headerTextAlignment = TextAlignment.Left,
//                sortedAscending = false,
//                sortingArrowAlignment = TextAlignment.Center,
//                width = 150,
//                minWidth = 60,
//                autoResize = false,
//                allowToggleVisibility = false
//                },
//                new MultiColumnHeaderState.Column
//                {
//                headerContent = new GUIContent ("Asset", "Res."),
//                headerTextAlignment = TextAlignment.Right,
//                sortedAscending = false,
//                sortingArrowAlignment = TextAlignment.Left,
//                width = 95,
//                minWidth = 60,
//                autoResize = true,
//                allowToggleVisibility = false
//                },
//                new MultiColumnHeaderState.Column
//                {
//                headerContent = new GUIContent ("Size"),
//                headerTextAlignment = TextAlignment.Right,
//                sortedAscending = false,
//                sortingArrowAlignment = TextAlignment.Left,
//                width = 70,
//                minWidth = 60,
//                autoResize = true
//                },
//                new MultiColumnHeaderState.Column
//                {
//                headerContent = new GUIContent ("TotalSize", "ChildSize"),
//                headerTextAlignment = TextAlignment.Right,
//                sortedAscending = false,
//                sortingArrowAlignment = TextAlignment.Left,
//                width = 70,
//                minWidth = 60,
//                autoResize = true
//                },
//                new MultiColumnHeaderState.Column
//                {
//                headerContent = new GUIContent ("Status"),
//                headerTextAlignment = TextAlignment.Right,
//                sortedAscending = false,
//                sortingArrowAlignment = TextAlignment.Left,
//                width = 70,
//                minWidth = 60,
//                autoResize = true
//                }
//            };

//            // Assert.AreEqual (columns.Length, Enum.GetValues (typeof (MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

//            var state = new MultiColumnHeaderState (columns);
//            return state;
//        }
//    }

//    static class MultiTreeExtensionMethods
//    {
//        public static IOrderedEnumerable<T> Order<T, TKey> (this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
//        {
//            if (ascending)
//            {
//                return source.OrderBy (selector);
//            }
//            else
//            {
//                return source.OrderByDescending (selector);
//            }
//        }

//        public static IOrderedEnumerable<T> ThenBy<T, TKey> (this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
//        {
//            if (ascending)
//            {
//                return source.ThenBy (selector);
//            }
//            else
//            {
//                return source.ThenByDescending (selector);
//            }
//        }
//    }
//}
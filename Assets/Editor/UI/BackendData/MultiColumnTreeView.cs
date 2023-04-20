using CFEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEditor.TreeViewExamples
{
	internal class MultiColumnTreeView : TreeViewWithTreeModel<MyTreeElement>
	{
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		public bool showControls = true;


		// All columns
		enum MyColumns
		{
			Prefab,
			LabelName,
			LabelValue,
			Button,
		}

		public enum SortOption
		{
			Name,
			LabelName,
			LabelValue,
            Button
        }

		// Sort options per column
		SortOption[] m_SortOptions =
		{
			SortOption.Name,
		SortOption.LabelName,
		SortOption.LabelValue,
		SortOption.Button
		};

		public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();
	
			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		public MultiColumnTreeView (TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<MyTreeElement> model) : base (state, multicolumnHeader, model)
		{
			Assert.AreEqual(m_SortOptions.Length , Enum.GetValues(typeof(MyColumns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

			// Custom setup
			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 2;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			extraSpaceBeforeIconAndLabel = kToggleWidth;
			multicolumnHeader.sortingChanged += OnSortingChanged;
			
			Reload();
		}


		// Note we We only build the visible rows, only the backend has the full tree information. 
		// The treeview only creates info for the row list.
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows (root);
			SortIfNeeded (root, rows);
			return rows;
		}

		void OnSortingChanged (MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded (rootItem, GetRows());
		}

		void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
		{
			if (rows.Count <= 1)
				return;
			
			if (multiColumnHeader.sortedColumnIndex == -1)
			{
				return; // No column to sort for (just use the order the data are in)
			}
			
			// Sort the roots of the existing tree items
			//SortByMultipleColumns ();
			TreeToList(root, rows);
			Repaint();
		}

		void SortByMultipleColumns ()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<TreeViewItem<MyTreeElement> >();
			var orderedQuery = InitialOrder (myTypes, sortedColumns);
			for (int i=1; i<sortedColumns.Length; i++)
			{
				SortOption sortOption = m_SortOptions[sortedColumns[i]];
				bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.data.name, ascending);
						break;
				}
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem> ().ToList ();
		}

		IOrderedEnumerable<TreeViewItem<MyTreeElement>> InitialOrder(IEnumerable<TreeViewItem<MyTreeElement>> myTypes, int[] history)
		{
			SortOption sortOption = m_SortOptions[history[0]];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortOption)
			{
				case SortOption.Name:
					return myTypes.Order(l => l.data.name, ascending);
				default:
					Assert.IsTrue(false, "Unhandled enum");
					break;
			}

			// default
			return myTypes.Order(l => l.data.name, ascending);
		}

		//int GetIcon1Index(TreeViewItem<MyTreeElement> item)
		//{
		//	return (int)(Mathf.Min(0.99f, item.data.floatValue1) * s_TestIcons.Length);
		//}

		//int GetIcon2Index (TreeViewItem<MyTreeElement> item)
		//{
		//	return Mathf.Min(item.data.text.Length, s_TestIcons.Length-1);
		//}

		protected override void RowGUI (RowGUIArgs args)
		{
			var item = (TreeViewItem<MyTreeElement>) args.item;

			for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
			}
		}

		void CellGUI (Rect cellRect, TreeViewItem<MyTreeElement> item, MyColumns column, ref RowGUIArgs args)
		{
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);

			if (item.data.lableValue.Length > 4)
				GUI.contentColor = Color.red;
			else
				GUI.contentColor = Color.white;


			switch (column)
			{
				case MyColumns.LabelName:
					{
						GUI.Label(cellRect, item.data.lableName);
						//GUI.DrawTexture(cellRect, s_TestIcons[GetIcon1Index(item)], ScaleMode.ScaleToFit);
					}
					break;
				case MyColumns.LabelValue:
					{
						GUI.Label(cellRect, item.data.lableValue);
						//GUI.DrawTexture(cellRect, s_TestIcons[GetIcon2Index(item)], ScaleMode.ScaleToFit);
					}
					break;
				case MyColumns.Prefab:

					GUI.Label(cellRect, item.data.shirtName);
					//EditorGUI.ObjectField(cellRect, item.data.ge)
					//EditorGUI.ObjectField(cellRect, item.data.get("go"));
					break;
				case MyColumns.Button:

					if(GUI.Button(cellRect, new GUIContent("Go")))
					{
						OpenPrefab(item.data);
					}
					break;

			}
		}

		private void OpenPrefab(MyTreeElement ele)
		{
            GameObject peart = GameObject.Find("Canvas");
            GameObject go = null;
            GameObject newGo;
            if (peart != null)
                go = GameObject.Find("Canvas/" + ele.shirtName);
			else
                go = GameObject.Find(ele.shirtName);
            if (go == null)
			{
				string fullname = ele.prefabName;
				if (fullname.IndexOf("Assets") >= 0)
				{
					fullname = fullname.Substring(fullname.IndexOf("Assets"));
				}
				go = AssetDatabase.LoadAssetAtPath<GameObject>(fullname);
                if (peart != null)
                {
                    newGo = PrefabUtility.InstantiatePrefab(go, peart.transform) as GameObject;
                }
                else
                {
                    newGo = PrefabUtility.InstantiatePrefab(go) as GameObject;
                }
            }
            else
            {

                newGo = go;

            }
			if (newGo != null)
			{
				Transform node = newGo.transform.Find(ele.lablePath);
				Selection.activeTransform = node;
			}
		}

		// Rename
		//--------

		protected override bool CanStartDrag(CanStartDragArgs args)
		{
			return false;
		}

		protected override bool CanRename(TreeViewItem item)
		{
			// Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
			//Rect renameRect = GetRenameRect (treeViewRect, 0, item);
			//return renameRect.width > 30;
			return false;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
			// Set the backend name and reload the tree to reflect the new model
			if (args.acceptedRename)
			{
				var element = treeModel.Find(args.itemID);
				element.name = args.newName;
				Reload();
			}
		}

		protected override Rect GetRenameRect (Rect rowRect, int row, TreeViewItem item)
		{
			Rect cellRect = GetCellRectForTreeFoldouts (rowRect);
			CenterRectUsingSingleLineHeight(ref cellRect);
			return base.GetRenameRect (cellRect, row, item);
		}

		// Misc
		//--------

		protected override bool CanMultiSelect (TreeViewItem item)
		{
			return true;
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), "Lorem ipsum dolor sit amet, consectetur adipiscing elit. "),
					contextMenuText = "Prefab",
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Right,
					width = 200,
					minWidth = 180,
					maxWidth = 300,
					autoResize = true,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Label"),
					contextMenuText = "LableName",
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Right,
					width = 200,
					minWidth = 150,
					maxWidth = 300,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Value"),
					contextMenuText = "LabelVlaue",
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 400, 
					minWidth = 400,
					autoResize = true,
					allowToggleVisibility = false
				},

				new MultiColumnHeaderState.Column
				{
                    headerContent = new GUIContent("Op"),
                    contextMenuText = "LabelVlaue",
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 100,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false
                }
			};

			Assert.AreEqual(columns.Length, Enum.GetValues(typeof(MyColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

			var state =  new MultiColumnHeaderState(columns);
			return state;
		}
	}

	static class MyExtensionMethods
	{
		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}
			else
			{
				return source.OrderByDescending(selector);
			}
		}

		public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.ThenBy(selector);
			}
			else
			{
				return source.ThenByDescending(selector);
			}
		}
	}
}

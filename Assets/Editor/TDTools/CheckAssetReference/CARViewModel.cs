using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor.IMGUI.Controls;
using XEditor;
using CFClient;
using CFEngine;
using CFUtilPoolLib;


namespace TDTools
{
    internal class CARTreeViewItem<T> : TreeViewItem where T : TreeViewElement
    {
        public T data { get; set; }
        public CARTreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
        {
            this.data = data;
        }
    }

    internal class CARTreeModel<T> where T : TreeViewElement
    {
        IList<T> m_Data;
        T m_Root;
        int m_MaxID;

        public T root { get { return m_Root; } set { m_Root = value; } }
        public event Action modelChanged;
        public int numberOfDataElements
        {
            get { return m_Data.Count; }
        }

        public CARTreeModel (IList<T> data)
        {
            SetData (data);
        }

        public void SetData (IList<T> data)
        {
            Init (data);
        }

        void Init (IList<T> data)
        {
            if (data == null)
                throw new ArgumentNullException ("data", "Input data is null. Ensure input is a non-null list.");
            
            m_Data = data;
            if (m_Data.Count > 0)
                m_Root =  CARUtility.ListToTree (data);

            m_MaxID = m_Data.Max (e => e.id);
        }

        public int GenerateUniqueID ()
        {
            return ++m_MaxID;
        }

        public IList<int> GetAncestors (int id)
        {
            var parents = new List<int> ();
            TreeViewElement T = Find (id);
            if (T != null)
            {
                while (T.parent != null)
                {
                    parents.Add (T.parent.id);
                    T = T.parent;
                }
            }
            return parents;
        }

        public IList<int> GetDescendantsThatHaveChildren (int id)
        {
            T searchFromThis = Find (id);
            if (searchFromThis != null)
            {
                return GetParentsBelowStackBased (searchFromThis);
            }
            return new List<int>();
        }

        IList<int> GetParentsBelowStackBased (TreeViewElement searchFromThis)
        {
            Stack<TreeViewElement> stack = new Stack<TreeViewElement>();
            stack.Push (searchFromThis);

            var parentsBelow = new List<int> ();
            while (stack.Count > 0)
            {
                TreeViewElement current = stack.Pop ();
                if (current.hasChildren)
                {
                    parentsBelow.Add (current.id);
                    foreach (var T in current.children)
                        stack.Push (T);
                }
            }

            return parentsBelow;
        }

        public T Find (int id)
        {
            return m_Data.FirstOrDefault (element => element.id == id);
        }

        public void MoveElements (TreeViewElement parentElement, int insertionIndex, List<TreeViewElement> elements)
        {
            if (insertionIndex < 0)
                throw new ArgumentException ("Invalid input: insertionIndex is -1.");

            if (parentElement == null)
                return;

            if (insertionIndex > 0)
                insertionIndex -= parentElement.children.GetRange(0, insertionIndex).Count(elements.Contains);

            foreach (var draggedItem in elements)
            {
                draggedItem.parent.children.Remove(draggedItem);	// remove from old parent
				draggedItem.parent = parentElement;	
            }

            if (parentElement.children == null)
				parentElement.children = new List<TreeViewElement>();

			// Insert dragged items under new parent
			parentElement.children.InsertRange(insertionIndex, elements);

			CARUtility.UpdateDepthValues (root);
			CARUtility.TreeToList (m_Root, m_Data);

			Changed ();
        }

        public void RemveElements (IList<int> elementIDs)
        {
            IList<T> elements = m_Data.Where (element => elementIDs.Contains (element.id)).ToArray ();
            RemoveElements (elements);
        }

        public void RemoveElements (IList<T> elements)
        {
            foreach (var element in elements)
                if (element == m_Root)
                    throw new ArgumentException ("It is not allowed to remove the root element.");

            var commonAncestors = CARUtility.FindCommonAncestorsWithinList (elements);

            foreach (var element in commonAncestors)
            {
                element.parent.children.Remove (element);
                element.parent = null;
            }

            CARUtility.TreeToList (m_Root, m_Data);
            Changed ();
        }

        void Changed ()
        {
            if (modelChanged != null)
                modelChanged ();
        }
    }

    internal class CARTreeViewWithTreeModel<T> : TreeView where T : TreeViewElement
    {
        CARTreeModel<T> m_TreeModel;
        List<TreeViewItem> m_Rows = new List<TreeViewItem>();
        public event Action treeChanged;

        public CARTreeModel<T> treeModel { get { return m_TreeModel; } }
        public event Action<IList<TreeViewItem>> beforeDroppingDraggedItems;

        public CARTreeViewWithTreeModel (TreeViewState state, CARTreeModel<T> model) : base (state)
        {
            Init (model);
        }

        public CARTreeViewWithTreeModel (TreeViewState state, MultiColumnHeader multiColumnHeader, CARTreeModel<T> model) : base (state, multiColumnHeader)
        {
            Init (model);
        }

        void Init (CARTreeModel<T> model)
        {
            m_TreeModel = model;
            m_TreeModel.modelChanged += ModelChanged;
        }

        void ModelChanged ()
        {
            if (treeChanged != null)
                treeChanged ();
            
            Reload ();
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new CARTreeViewItem<T> (m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
        }

        protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
        {
            if (m_TreeModel.root == null)
                Debug.Log ("Tree model root is null, did you call SetData()?");
            
            m_Rows.Clear ();
            if (!String.IsNullOrEmpty (searchString))
            {
                Search (m_TreeModel.root, searchString, m_Rows);
            }
            else 
            {
                if (m_TreeModel.root.hasChildren)
                    AddChildrenRecursive (m_TreeModel.root, 0, m_Rows);
            }

            SetupParentsAndChildrenFromDepths (root, m_Rows);

            return m_Rows;
        }

        void AddChildrenRecursive (T parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (T child in parent.children)
            {
                var item = new CARTreeViewItem<T> (child.id, depth, child.name, child);
                newRows.Add (item);

                if (child.hasChildren)
                {
                    if (IsExpanded (child.id))
                        AddChildrenRecursive (child, depth + 1, newRows);
                    else item.children = CreateChildListForCollapsedParent ();
                }
            }
        }

        void Search (T searchFromThis, string search, List<TreeViewItem> result)
        {
            if (String.IsNullOrEmpty (search))
                throw new ArgumentException ("Invalid search: cannot be null of empty", "search");

            const int kItemDepth = 0;

            Stack<T> stack = new Stack<T> ();
            foreach (var element in searchFromThis.children)
                stack.Push ((T)element);
            
            while (stack.Count > 0)
            {
                T current = stack.Pop ();
                if (current.name.IndexOf (search, StringComparison.OrdinalIgnoreCase) >= 0)
                    result.Add (new CARTreeViewItem<T>(current.id, kItemDepth, current.name, current));
                
                if (current.children != null && current.children.Count > 0)
                    foreach (var element in current.children)
                        stack.Push ((T) element);
            }

            SortSearchResult (result);
        }

        protected virtual void SortSearchResult (List<TreeViewItem> rows)
        {
            rows.Sort ((x, y) => EditorUtility.NaturalCompare (x.displayName, y.displayName));
        }

        protected override IList<int> GetAncestors (int id)
        {
            return m_TreeModel.GetAncestors (id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren (int id)
        {
            return m_TreeModel.GetDescendantsThatHaveChildren (id);
        }

        const string k_GenericDragID = "GenericDragColumnDragging";

        protected override bool CanStartDrag (CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop (SetupDragAndDropArgs args)
        {
            if (hasSearch) return;

            DragAndDrop.PrepareStartDrag ();
            var draggedRows = GetRows().Where (item => args.draggedItemIDs.Contains (item.id)).ToList();
            DragAndDrop.SetGenericData (k_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] {};
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag (title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args)
		{
			// Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
			var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
			if (draggedRows == null)
				return DragAndDropVisualMode.None;

			// Parent item is null when dragging outside any tree view items.
			switch (args.dragAndDropPosition)
			{
				case DragAndDropPosition.UponItem:
				case DragAndDropPosition.BetweenItems:
					{
						bool validDrag = ValidDrag(args.parentItem, draggedRows);
						if (args.performDrop && validDrag)
						{
							T parentData = ((CARTreeViewItem<T>)args.parentItem).data;
							OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
						}
						return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
					}

				case DragAndDropPosition.OutsideItems:
					{
						if (args.performDrop)
							OnDropDraggedElementsAtIndex(draggedRows, m_TreeModel.root, m_TreeModel.root.children.Count);

						return DragAndDropVisualMode.Move;
					}
				default:
					Debug.Log("Unhandled enum " + args.dragAndDropPosition);
					return DragAndDropVisualMode.None;
			}
		}

		public virtual void OnDropDraggedElementsAtIndex (List<TreeViewItem> draggedRows, T parent, int insertIndex)
		{
			if (beforeDroppingDraggedItems != null)
				beforeDroppingDraggedItems (draggedRows);

			var draggedElements = new List<TreeViewElement> ();
			foreach (var x in draggedRows)
				draggedElements.Add (((CARTreeViewItem<T>) x).data);
		
			var selectedIDs = draggedElements.Select (x => x.id).ToArray();
			m_TreeModel.MoveElements (parent, insertIndex, draggedElements);
			SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
		}

        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
		{
			TreeViewItem currentParent = parent;
			while (currentParent != null)
			{
				if (draggedItems.Contains(currentParent))
					return false;
				currentParent = currentParent.parent;
			}
			return true;
		}
    }
}

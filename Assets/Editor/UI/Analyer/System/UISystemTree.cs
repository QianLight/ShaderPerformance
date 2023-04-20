
using System.Collections.Generic;
using UIAnalyer;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIAnalyer
{
    public class UISystemTree : TreeView
    {
        private UISystemAnalyer m_control;

        public UISystemAnalyer control { get { return m_control; } }
        public UISystemTree(TreeViewState state, UISystemAnalyer control) : base(state)
        {
            m_control = control;
            Reload();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override IList<TreeViewItem> GetRows()
        {
            return base.GetRows();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();
        }

        protected override TreeViewItem BuildRoot()
        {
            return m_control.sourceModel.folder.CreateTreeItem(-1);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return base.CanBeParent(item);
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {
            return base.CanChangeExpandedState(item);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return base.CanRename(item);
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return base.CanStartDrag(args);
        }

        protected override void CommandEventHandling()
        {
            base.CommandEventHandling();
        }

        protected override void ContextClicked()
        {
            base.ContextClicked();
        }

        protected override void ContextClickedItem(int id)
        {
            UISystemTreeItem sourceItem = FindItem(id, rootItem) as UISystemTreeItem;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("删  除"), false, _MenuSystemDeleteCall, sourceItem.Source);
            menu.ShowAsContext();
        }

        protected void _MenuSystemDeleteCall(object o){
            UISystemInfo system = o as UISystemInfo;
            m_control.sourceModel.folder.m_children.Remove(system.hasCode);
            m_control.systemTree.Reload();
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return base.DoesItemMatchSearch(item, search);
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
        }

        protected override void ExpandedStateChanged()
        {
            base.ExpandedStateChanged();
        }

        protected override IList<int> GetAncestors(int id)
        {
            return base.GetAncestors(id);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return base.GetCustomRowHeight(row, item);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return base.GetDescendantsThatHaveChildren(id);
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return base.GetRenameRect(rowRect, row, item);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return base.HandleDragAndDrop(args);
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
        }

        protected override void RefreshCustomRowHeights()
        {
            base.RefreshCustomRowHeights();
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
        }

        protected override void RowGUI(RowGUIArgs args)
        {

            base.RowGUI(args);
        }

        protected override void SearchChanged(string newSearch)
        {
            base.SearchChanged(newSearch);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            UISystemTreeItem treeItem = FindItem(id , rootItem) as UISystemTreeItem;
            m_control.selectionGUI.SetSelection(treeItem.Source);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace UIAnalyer
{
    public class UIPrefabTree : TreeView
    {
        private UIPrefabAnalyer m_control;

        public UIPrefabTree(TreeViewState state, UIPrefabAnalyer control) : base(state)
        {
            m_control = control;
            showBorder = true;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            return m_control.sourceModel.folder.CreateTreeItem(-1);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            UISourceTreeItem sourceItem = args.item as UISourceTreeItem;
            if (args.item.icon == null)
            {
                extraSpaceBeforeIconAndLabel = 32f;
            }
            else
            {
                extraSpaceBeforeIconAndLabel = 0f;
            }

            Color old = GUI.color;
            if (sourceItem.Source.IsSource(UISource.Folder))
            {
                UISourceFolderInfo folderInfo = sourceItem.Source as UISourceFolderInfo;
                if(folderInfo.children.Count == 0){
                    GUI.color =  Color.red;
                }
            }
            base.RowGUI(args);
            GUI.color = old;
        }
   
        //选择单项右键点击
        protected override void ContextClickedItem(int id)
        {
            UISourceTreeItem sourceItem = FindItem(id, rootItem) as UISourceTreeItem;        
            GenericMenu menu = new GenericMenu();
            if(sourceItem.Source.IsSource(UISource.File))
            {
            }else if(sourceItem.Source.IsSource(UISource.Folder)){
                //Folder
            }else if(sourceItem.Source.IsSource(UISource.Prefab)){
                menu.AddItem(new GUIContent("打  开"),false,_MenuSourceOpenPrefabCall, sourceItem);
            }
            menu.ShowAsContext();
        }

        protected void _MenuSourceOpenPrefabCall(object o){
            UISourceTreeItem source = o as UISourceTreeItem;
            OpenAsset(source);
        }
        
        
        private void OpenAsset(UISourceTreeItem item){
                UISourcePrefabInfo prefab = item.Source as UISourcePrefabInfo;
                string fullname = prefab.fullname;
                fullname = fullname.Substring(fullname.IndexOf("Assets"));
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fullname);
                AssetDatabase.OpenAsset(go);
                Selection.activeGameObject = go;
        }
        
        public static Texture2D GetTexture(string Path)
        {
            UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(Path);
            foreach (UnityEngine.Object o in data)
            {
                Texture2D s = (Texture2D)o;
                if (s != null)
                    return s;
            }
            return null;
        }
        /// <summary>
        /// 点击右键
        /// </summary>
        protected override void ContextClicked()
        {
            base.ContextClicked();
            Debug.Log("ContextClicked:");
        }

        private int currentSelectId = 0;
        protected override void SingleClickedItem(int id)
        {
                SetSelection(id);
        }

        private void SetSelection(int id){
            UISourceTreeItem item = FindItem(id, rootItem) as UISourceTreeItem;
            if(item.Source.IsSource(UISource.Folder))
            {
                if(currentSelectId != id){
                    
                    SetExpanded(currentSelectId, false);
                    SetExpanded(id,true);
                    currentSelectId = id;
                }    
            }
            else if(item.Source.IsSource(UISource.Prefab))
            {
                m_control.SetSelection(item);
            }   
        }

        
        protected override void DoubleClickedItem(int id)
        {
                UISourceTreeItem item = FindItem(id, rootItem) as UISourceTreeItem;
                if(item == null) return;
                if(item.Source.IsSource(UISource.File)){
                string fullname = item.Source.fullname;
                fullname = fullname.Substring(fullname.IndexOf("Assets"));
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(fullname,typeof(Texture2D));
                }else if (item.Source.IsSource(UISource.Prefab)){
                    OpenAsset(item);
                }
        }

#region  Drag
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
            Debug.Log("SetupDragAndDrop:" + args.draggedItemIDs);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            Debug.Log("HandleDragAndDrop:" + args.parentItem.displayName);
            return base.HandleDragAndDrop(args);
        
        }
#endregion

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        public override IList<TreeViewItem> GetRows()
        {
            return base.GetRows();
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            Debug.Log("SelectionChanged:" + selectedIds.Count);
        }

        protected override void ExpandedStateChanged()
        {
            base.ExpandedStateChanged();
        }

        protected override void SearchChanged(string newSearch)
        {
            base.SearchChanged(newSearch);
        }

        protected override IList<int> GetAncestors(int id)
        {
            return base.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return base.GetDescendantsThatHaveChildren(id);
        }
#region  Rename
        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            base.RenameEnded(args);
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return base.GetRenameRect(rowRect, row, item);
        }

#endregion

        protected override bool CanBeParent(TreeViewItem item)
        {
            return base.CanBeParent(item);
        }

        protected override bool CanChangeExpandedState(TreeViewItem item)
        {

            return base.CanChangeExpandedState(item);
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return base.DoesItemMatchSearch(item, search);
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();
        }

        protected override void RefreshCustomRowHeights()
        {
            base.RefreshCustomRowHeights();
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return base.GetCustomRowHeight(row, item);
        }

 
        protected override void CommandEventHandling()
        {
            base.CommandEventHandling();
        }
    }
}
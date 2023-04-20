using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.U2D;
using UnityEngine.CFUI;

namespace UIAnalyer
{
    public class UISourceTree : TreeView
    {
        private UISourceAnalyer m_control;

        public UISourceTree(TreeViewState state, UISourceAnalyer control) : base(state)
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
            if (sourceItem.Source.IsSource(UISource.File))
            {
                UISourceFileInfo folderInfo = sourceItem.Source as UISourceFileInfo;
                if (!m_control.sourceModel.ContainsSelection(folderInfo))
                {
                    GUI.color = Color.red;
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
            if (sourceItem.Source.IsSource(UISource.File))
            {
                menu.AddItem(new GUIContent("删除文件"), false, _MenuSourceRemoveCall, sourceItem);
                menu.AddItem(new GUIContent("复制名称"), false, _MenuSourcCopyCall, sourceItem);
                menu.AddItem(new GUIContent("重命名"), false, _RenameCall, sourceItem);
                menu.AddItem(new GUIContent("保存GUID"),false, _SaveGuidToPrefab, id);
                // menu.AddItem(new GUIContent("移动文件"), false, _MenuSourceMoveCall, sourceItem);
            }
            else if (sourceItem.Source.IsSource(UISource.Folder))
            {
              //  menu.AddItem(new GUIContent("批量删除"), false, _MenuSourceRemoveFolderCall, sourceItem);
                menu.AddItem(new GUIContent("显示所有"), false, _MenuSourceSelectCall, id);
                menu.AddItem(new GUIContent("生成Atlas"), false, _MenuSourcConfigSpriteAtlasCall, sourceItem);
            }
            menu.ShowAsContext();
        }

        protected void _SaveGuidToPrefab( object o )
        {
            int id = (int)o;
            UISourceTreeItem ti = FindItem(id, rootItem) as UISourceTreeItem;
            if (ti.Source.IsSource(UISource.File))
            {
                m_control.SetSelection(ti);
            }
            if (ti.Source.IsSource(UISource.Folder))
            {
                if (currentSelectId != id)
                {

                    SetExpanded(currentSelectId, false);
                    SetExpanded(id, true);
                    currentSelectId = id;
                }
            }
        }

        private void _RenameCall(object o)
        {
            UISourceTreeItem treeItem = o as UISourceTreeItem;
            UIRenameEditor.ShowSourceRename(treeItem,  OnApplyRename);
        }


        private void OnApplyRename( string newName, UISourceTreeItem select)
        {
            Debug.Log("Rename:"+ newName);
            string oldPath = select.Source.fullname;
            string newPath = select.Source.fullname.Replace(select.Source.shortName, newName);
            UISourceFolderInfo folder = XDataPool<UISourceFolderInfo>.GetData();
            folder.Setup("root", null);
            folder.Clear();
            UISourceUtils.FindDependences(select.Source as UISourceFileInfo, m_control.sourceType, ref folder);
            foreach (KeyValuePair<string, UISourceInfo> pair in folder.children)
            {
                UISourceFolderInfo child = pair.Value as UISourceFolderInfo;
                foreach (KeyValuePair<string, UISourceInfo> childnode in child.children)
                {
                    UISourcePrefabInfo prefab = childnode.Value as UISourcePrefabInfo;
                    if (m_control.sourceType == UISourceType.Sprite)
                    {
                        CFImage image = child.temp.Find(prefab.usePath).GetComponent<CFImage>();
                        image.m_SpriteName = newName;
                    }
                    else if (m_control.sourceType == UISourceType.Texture)
                    {
                        CFRawImage rawImage = child.temp.Find(prefab.usePath).GetComponent<CFRawImage>();
                        string usePath = newPath.Substring(newPath.IndexOf("UIBackground")) + "/" + newName;
                        rawImage.m_TexPath = usePath.Replace('\\', '/');
                    }
                }
                GameObject newObj =  PrefabUtility.SavePrefabAsset(child.temp.gameObject);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
                Debug.Log("修改成功:" + child.temp);
            }
            File.Move(oldPath, newPath);
            folder.Clear();
            XDataPool<UISourceFolderInfo>.Recycle(folder);
            AssetDatabase.SaveAssets();
            m_control.Refresh();
        }

        private void _MenuSourceSelectCall(object o)
        {
            int selectID = (int)o;
            SetSelection(selectID, true);
        }


        private void _MenuSourcCopyCall(object o)
        {
            UISourceTreeItem treeItem = o as UISourceTreeItem;
            GUIUtility.systemCopyBuffer = treeItem.Source.shortName;
            Debug.Log("copy:" + treeItem.Source.shortName);
        }



        private void _MenuSourceRemoveCall(object o)
        {
            UISourceTreeItem treeItem = o as UISourceTreeItem;
            string fullname = treeItem.Source.fullname;
            fullname = fullname.Substring(fullname.IndexOf("Assets"));
            bool status = AssetDatabase.MoveAssetToTrash(fullname);
            if (status)
            {
                m_control.Refresh();
            }
            Debug.Log("Remove Source:" + treeItem.displayName + ":" + status);
        }

        private void _MenuSourceMoveCall(object o)
        {
            UISourceTreeItem treeItem = o as UISourceTreeItem;
            string fullname = treeItem.Source.fullname;
            fullname = fullname.Substring(fullname.IndexOf("Assets"));

            string newUrl = EditorUtility.OpenFolderPanel("Select UI Source Folder!", Application.dataPath + "/BundleRes/UI", "");
            if (!string.IsNullOrEmpty(newUrl))
            {
                newUrl = newUrl.Substring(newUrl.IndexOf("Assets"));
                if (newUrl.Contains(UISourceModel.m_uisource_url))
                {
                    newUrl = newUrl + "/" + treeItem.Source.shortName + ".png";
                    AssetDatabase.MoveAsset(fullname, newUrl);



                    //设置替换
                }
                else if (newUrl.Contains(UISourceModel.m_uibackground_url))
                {

                }
                AssetDatabase.SaveAssets();
                Debug.Log("GO:" + newUrl + " 未做");
            }
            //待做
        }

        private void _MenuSourceRemoveFolderCall(object o)
        {
            UISourceTreeItem treeItem = o as UISourceTreeItem;
            Debug.Log("Remove Folder:" + treeItem.displayName + "未做");
        }

        private void OpenAsset(UISourceInfo item)
        {
            UISourcePrefabInfo prefab = item as UISourcePrefabInfo;
            string fullname = prefab.fullname;
            fullname = fullname.Substring(fullname.IndexOf("Assets"));
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(fullname);
            GameObject peart = GameObject.Find("Canvas");
            GameObject newGo;
            if (peart != null)
            {
                newGo = PrefabUtility.InstantiatePrefab(go, peart.transform) as GameObject;
            }
            else
            {
                newGo = PrefabUtility.InstantiatePrefab(go) as GameObject;
            }
            Transform node = newGo.transform.Find(prefab.usePath);
            Selection.activeTransform = node;
        }
        private static string atlas_url = "Assets/BundleRes/UI/atlas";

        private void _MenuSourcConfigSpriteAtlasCall(object o)
        {
            UISourceTreeItem sourceItem = o as UISourceTreeItem;
            string atlas = atlas_url + "\\" + sourceItem.Source.shortName + ".spriteatlas";
            SpriteAtlas sa = null;
            if (!File.Exists(atlas))
            {
                sa = new SpriteAtlas();
                SpriteAtlasPackingSettings packset = new SpriteAtlasPackingSettings
                {
                    blockOffset = 1,
                    enableRotation = true,
                    enableTightPacking = false,
                    padding = 4,

                };
                sa.SetPackingSettings(packset);

                SpriteAtlasTextureSettings textureSet = new SpriteAtlasTextureSettings
                {
                    readable = true,
                    generateMipMaps = false,
                    sRGB = true,
                    filterMode = FilterMode.Bilinear
                };
                sa.SetTextureSettings(textureSet);
                AssetDatabase.CreateAsset(sa, atlas);
                string fullname = sourceItem.Source.fullname;
                fullname = fullname.Substring(fullname.IndexOf("Assets"));
                UnityEngine.Object ot = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullname);
                SpriteAtlasExtensions.Add(sa, new UnityEngine.Object[] { ot });
            }
            else
            {
                sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlas);
            }
            Selection.activeObject = sa;
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { sa }, BuildTarget.StandaloneWindows);
            AssetDatabase.SaveAssets();
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

        private void SetSelection(int id, bool select = false)
        {
            UISourceTreeItem item = FindItem(id, rootItem) as UISourceTreeItem;
            if (select || item.Source.IsSource(UISource.File))
            {
                m_control.SetSelection(item);
            }
            if (item.Source.IsSource(UISource.Folder))
            {
                if (currentSelectId != id)
                {

                    SetExpanded(currentSelectId, false);
                    SetExpanded(id, true);
                    currentSelectId = id;
                }
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            UISourceTreeItem item = FindItem(id, rootItem) as UISourceTreeItem;
            if (item == null) return;
            if (item.Source.IsSource(UISource.File))
            {
                string fullname = item.Source.fullname;
                fullname = fullname.Substring(fullname.IndexOf("Assets"));
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(fullname, typeof(Texture2D));
            }
        }

        #region  Drag
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        private List<UnityEngine.Object> m_emptyList = new List<UnityEngine.Object>();
        private string m_draggedItemIDs = "m_draggedItemIDs";
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (args.draggedItemIDs == null) return;


            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            for (int i = 0; i < draggedRows.Count; i++)
            {
                UISourceTreeItem treeItem = draggedRows[i] as UISourceTreeItem;
                if (!treeItem.Source.IsSource(UISource.File)) return;
            }

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = m_emptyList.ToArray();
            DragAndDrop.SetGenericData(m_draggedItemIDs, draggedRows);
            DragAndDrop.StartDrag("SourceInfo");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var draggedRows = DragAndDrop.GetGenericData(m_draggedItemIDs) as List<TreeViewItem>;
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
                            //T parentData = ((TreeViewItem<T>)args.parentItem).data;
                            OnDropDraggedElementsAtIndex(draggedRows, args.parentItem, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                            Debug.Log("Drag  end!");
                        }
                        return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                    }

                case DragAndDropPosition.OutsideItems:
                    {
                        if (args.performDrop)
                        {
                            //OnDropDraggedElementsAtIndex(draggedRows, m_TreeModel.root, m_TreeModel.root.children.Count);
                            Debug.Log("OutsideItems");
                        }
                        return DragAndDropVisualMode.Move;
                    }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        private void OnDropDraggedElementsAtIndex(List<TreeViewItem> rows, TreeViewItem parent, int index)
        {
            UISourceTreeItem sourceItemParent = parent as UISourceTreeItem;
            UISourceFolderInfo folder = XDataPool<UISourceFolderInfo>.GetData();
            folder.Setup("root", null);
            bool change = false;
            for (int i = 0; i < rows.Count; i++)
            {
                folder.Clear();
                UISourceTreeItem select = rows[i] as UISourceTreeItem;

                string oldPath = select.Source.fullname;
                string newPath = sourceItemParent.Source.fullname + "/" + select.Source.shortName + ".png";
                if (MoveFile(oldPath, newPath))
                {
                    UISourceUtils.FindDependences(select.Source as UISourceFileInfo, m_control.sourceType, ref folder);
                    Debug.Log("FindDependences:" + sourceItemParent.Source.fullname + ":" + select.Source.fullname);
                    select.SetParent(sourceItemParent);
                    foreach (KeyValuePair<string, UISourceInfo> pair in folder.children)
                    {
                        UISourceFolderInfo child = pair.Value as UISourceFolderInfo;
                        foreach(KeyValuePair<string,UISourceInfo> childnode in child.children)
                        {
                            UISourcePrefabInfo prefab = childnode.Value as UISourcePrefabInfo;
                            if (m_control.sourceType == UISourceType.Sprite)
                            {
                                CFImage image = child.temp.Find(prefab.usePath).GetComponent<CFImage>();
                                image.m_AtlasName = sourceItemParent.Source.shortName;                            
                            }else if(m_control.sourceType == UISourceType.Texture){
                                CFRawImage rawImage = child.temp.Find(prefab.usePath).GetComponent<CFRawImage>();
                                string full = sourceItemParent.Source.fullname;
                                string usePath = full.Substring(full.IndexOf("UIBackground"))+"/"+select.Source.shortName;
                                Debug.Log(usePath);
                                rawImage.m_TexPath = usePath.Replace('\\','/');
                            }
                        }
                        GameObject newObj = PrefabUtility.SavePrefabAsset(child.temp.gameObject);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newObj));
                        change = true;
                        Debug.Log("修改成功:"+child.temp);
                    }
                               
                }
            }
            folder.Clear();
            XDataPool<UISourceFolderInfo>.Recycle(folder);
            if (change)
            {
                AssetDatabase.SaveAssets();
                m_control.Refresh();
            }
         
        }

        private bool MoveFile(string oldPath, string newPath)
        {
            oldPath = oldPath.Substring(oldPath.IndexOf("Assets"));
            newPath = newPath.Substring(newPath.IndexOf("Assets"));
            string message = AssetDatabase.MoveAsset(oldPath, newPath);
            return string.IsNullOrEmpty(message);
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
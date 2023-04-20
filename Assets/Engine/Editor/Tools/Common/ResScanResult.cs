//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEditor;
//using UnityEditor.IMGUI.Controls;
//using UnityEngine;

//namespace CFEngine.Editor
//{

//    public class ResScanResult : EditorWindow
//    {
//        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
//        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
//        private SearchField m_SearchField;
//        private ResTreeView m_TreeView;
//        private List<ResTreeElement> resTreeData = new List<ResTreeElement> ();
//        private int IDCounter;
//        private static ResScanResult instance;
//        public static ResScanResult Init (ScaneResult scaneResult)
//        {
//            if (instance == null)
//            {
//                instance = EditorWindow.GetWindow (typeof (ResScanResult), false, "") as ResScanResult;
//            }
//            instance.titleContent = new GUIContent ("Res");
//            instance.InitResTree (instance.GenTreeData (scaneResult));
//            instance.Show ();
//            return instance;
//        }
//        private void InitResTree (IList<ResTreeElement> scaneResult)
//        {
//            if (m_TreeView == null)
//            {
//                if (m_TreeViewState == null)
//                    m_TreeViewState = new TreeViewState ();

//                bool firstInit = m_MultiColumnHeaderState == null;
//                var headerState = ResTreeView.CreateDefaultMultiColumnHeaderState ();
//                if (MultiColumnHeaderState.CanOverwriteSerializedFields (m_MultiColumnHeaderState, headerState))
//                    MultiColumnHeaderState.OverwriteSerializedFields (m_MultiColumnHeaderState, headerState);
//                m_MultiColumnHeaderState = headerState;

//                var multiColumnHeader = new ResMultiColumnHeader (headerState);
//                if (firstInit)
//                    multiColumnHeader.ResizeToFit ();

//                var treeModel = new TreeModel<ResTreeElement> (scaneResult);

//                m_TreeView = new ResTreeView (m_TreeViewState, multiColumnHeader, treeModel);

//                m_SearchField = new SearchField ();
//                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;
//            }
//            else
//            {
//                var model = m_TreeView.treeModel;
//                model.SetData (scaneResult);
//                m_TreeView.Reload ();
//            }
//        }

//        private void AddChildrenRecursive (ResTreeElement parent, ResItem ri)
//        {
//            var item = new ResTreeElement (ri, parent.depth + 1, IDCounter++);
//            item.sizeStr = string.Format ("{0} ({1} Bytes)",
//                EditorUtility.FormatBytes (ri.size),
//                ri.size.ToString ());
//            ri.totalSize = ri.size;
//            resTreeData.Add (item);
//            if (ri.childs != null)
//            {
//                for (int i = 0; i < ri.childs.Count; ++i)
//                {
//                    var child = ri.childs[i];
//                    AddChildrenRecursive (item, child);
//                    ri.totalSize += child.totalSize;
//                }
//                item.totalSizeStr = string.Format ("Count {0} {1}({2} Bytes)",
//                    ri.childs.Count,
//                    EditorUtility.FormatBytes (ri.totalSize),
//                    ri.totalSize.ToString ());
//            }

//        }

//        private List<ResTreeElement> GenTreeData (ScaneResult scaneResult)
//        {
//            resTreeData.Clear ();
//            IDCounter = 0;

//            var root = new ResTreeElement (new ResItem () { name = "Assets" }, -1, IDCounter);
//            resTreeData.Add (root);
//            for (int i = 0; i < scaneResult.rootRes.Count; ++i)
//            {
//                AddChildrenRecursive (root, scaneResult.rootRes.res[i]);
//            }

//            return resTreeData;
//        }
//        public void SetData (ScaneResult scaneResult)
//        {
//            InitResTree (GenTreeData (scaneResult));
//            Focus ();
//            Repaint ();
//        }

//        void OnGUI ()
//        {
//            if (m_TreeView != null)
//                m_TreeView.OnGUI (new Rect (20, 30, position.width - 40, position.height - 60));
//        }
//    }
//}
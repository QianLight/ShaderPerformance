using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using XEditor;
using CFClient;
using CFEngine;
using CFUtilPoolLib;

namespace TDTools
{
    class CARWindow : EditorWindow
    {
#region Used for TreeView
        [NonSerialized] bool m_Initialized;
        [SerializeField] TreeViewState m_ArtTreeViewState, m_ConfigTreeViewState;
        [SerializeField] MultiColumnHeaderState m_ArtMultiColumnHeaderState, m_ConfigMultiColumnHeaderState;
        SearchField m_ArtSearchField, m_ConfigSearchField;
        CARTreeView m_ArtTreeView, m_ConfigTreeView;
        CARTreeAsset m_CARTreeAsset;

        public SearchField artSearchField { get { return m_ArtSearchField; } }
        public SearchField configSearchField { get { return m_ConfigSearchField; } }
        public CARTreeView artTreeView { get { return m_ArtTreeView; } }
        public CARTreeView configTreeView { get { return m_ConfigTreeView; } }

        int commitNumber;
        Vector2 scrollPos;
        int selectedID;
        CommandRunner git;
        List<Commit> commits;
#endregion

        Rect artTreeViewRect { get { return new Rect (20f, 30f, position.width / 2f - 30f, (position.height - 60f) * 2f / 3f); } }
        Rect commitRect { get { return new Rect (20f, 35f + (position.height - 60f ) * 2f / 3f, position.width / 2f - 30f, (position.height - 60f ) / 3f - 5f); } }
        Rect configTreeViewRect { get { return new Rect (position.width / 2f + 10f, 30f, position.width / 2f - 30f, position.height - 60f); } }
        Rect artBottomToolBarRect { get { return new Rect (20f, position.height - 25f, position.width / 2f - 30f, 20f); } }
        Rect configBottomToolBarRect { get { return new Rect (position.width / 2f + 10f, position.height - 25f, position.width / 2f - 30f, 20f); } }
        Rect artToolBarRect { get { return new Rect (20f, 10f, position.width / 2f - 30f, 20f); } }
        Rect configToolBarRect { get { return new Rect (position.width / 2f + 10f, 10f, position.width / 2f - 30f, 20f); } }

        [MenuItem("Tools/TDTools/通用工具/美术资源引用配置检测", false, priority = 1)]
        public static CARWindow GetWindow ()
        {
            var window = GetWindow<CARWindow>();
            window.titleContent = new GUIContent ("美术资源引用配置检测");
            window.minSize = new Vector2 (800, 800);
            window.Focus ();
            window.Repaint ();
            return window;
        }

        void OnEnable() 
        {
            InitIfNeeded ();
        }

        void InitIfNeeded ()
        {
            if (!m_Initialized)
            {
                m_Initialized = true;
                commitNumber = 10;
                selectedID = -1;
                if (git == null)
                    git = new CommandRunner ();
                commits = new List<Commit>();

                if (m_ArtTreeViewState == null)
                    m_ArtTreeViewState = new TreeViewState ();
                if (m_ConfigTreeViewState == null)
                    m_ConfigTreeViewState = new TreeViewState ();

                bool artFirstInit = m_ArtMultiColumnHeaderState == null;
                var artHeaderState = CARTreeView.CreateDefaultMultiColumnHeaderState (artTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields (m_ArtMultiColumnHeaderState, artHeaderState))
                    MultiColumnHeaderState.OverwriteSerializedFields (m_ArtMultiColumnHeaderState, artHeaderState);
                m_ArtMultiColumnHeaderState = artHeaderState;

                bool configFirstInit = m_ConfigMultiColumnHeaderState == null;
                var configHeaderState = CARTreeView.CreateDefaultMultiColumnHeaderState (configTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields (m_ConfigMultiColumnHeaderState, configHeaderState))
                    MultiColumnHeaderState.OverwriteSerializedFields (m_ConfigMultiColumnHeaderState, configHeaderState);
                m_ConfigMultiColumnHeaderState = configHeaderState;

                var artMultiColumnHeader = new CARMultiColumnHeader (artHeaderState);
                if (artFirstInit)
                    artMultiColumnHeader.ResizeToFit ();
                
                var configMultiColumnHeader = new CARMultiColumnHeader (configHeaderState);
                if (configFirstInit)
                    configMultiColumnHeader.ResizeToFit ();

                var artTreeModel = new CARTreeModel<TreeViewElement> (GetArtData ());
                m_ArtTreeView = new CARTreeView (m_ArtTreeViewState, artMultiColumnHeader, artTreeModel);

                var configTreeModel = new CARTreeModel<TreeViewElement> (GetConfigData ());
                m_ConfigTreeView = new CARTreeView (m_ConfigTreeViewState, configMultiColumnHeader, configTreeModel);

                m_ArtSearchField = new SearchField ();
                m_ArtSearchField.downOrUpArrowKeyPressed += m_ArtTreeView.SetFocusAndEnsureSelectedItem;
                m_ConfigSearchField = new SearchField ();
                m_ConfigSearchField.downOrUpArrowKeyPressed += m_ConfigTreeView.SetFocusAndEnsureSelectedItem;
            }
        }

        IList<TreeViewElement> GetArtData ()
        {
            if (m_CARTreeAsset == null)
                m_CARTreeAsset = AssetDatabase.LoadAssetAtPath<CARTreeAsset> ("Assets/Editor/TDTools/CheckAssetReference/CARTreeAsset.asset");
            
            List<TreeViewElement> result = new List<TreeViewElement> ();
            result.Add (GenerateRoot ());
            result.AddRange (m_CARTreeAsset.modelArtAssets.Cast<TreeViewElement>());
            result.AddRange (m_CARTreeAsset.animationArtAssets.Cast<TreeViewElement>());
            result.AddRange (m_CARTreeAsset.effectArtAssets.Cast<TreeViewElement>());

            return result;
        }

        IList<TreeViewElement> GetConfigData ()
        {
            if (m_CARTreeAsset == null)
                m_CARTreeAsset = AssetDatabase.LoadAssetAtPath<CARTreeAsset> ("Assets/Editor/TDTools/CheckAssetReference/CARTreeAsset.asset");
            
            List<TreeViewElement> result = new List<TreeViewElement> ();
            result.Add (GenerateRoot ());
            result.AddRange (m_CARTreeAsset.timelineConfigAssets.Cast<TreeViewElement>());
            result.AddRange (m_CARTreeAsset.levelConfigAssets.Cast<TreeViewElement>());
            result.AddRange (m_CARTreeAsset.skillConfigAssets.Cast<TreeViewElement>());
            result.AddRange (m_CARTreeAsset.behitConfigAssets.Cast<TreeViewElement>());

            return result;
        }

        TreeViewElement GenerateRoot ()
        {
            return new TreeViewElement ("root", -1, -1);
        }

        void OnGUI() 
        {
            SearchBar (artTreeView, artSearchField, artToolBarRect);
            SearchBar (configTreeView, configSearchField, configToolBarRect);
            DrawTreeView (artTreeView, artTreeViewRect);
            DrawTreeView (configTreeView, configTreeViewRect);
            DrawCommitArea (commitRect);
            DrawBottomToolBar (artBottomToolBarRect, false);
            DrawBottomToolBar (configBottomToolBarRect, true);
        }

        void SearchBar (CARTreeView treeView,  SearchField searchField, Rect rect)
        {
            treeView.searchString = searchField.OnGUI (rect, treeView.searchString);
        }

        void DrawTreeView (CARTreeView treeView, Rect rect)
        {
            treeView.OnGUI (rect);
        }
        
        void DrawCommitArea (Rect rect)
        {
            GUILayout.BeginArea (rect);
            
            GUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Number of history commits to show: ");
            commitNumber = EditorGUILayout.IntSlider (commitNumber, 1, 20);
            GUILayout.EndHorizontal ();

            if (GetSelectedIDs (artTreeView) != selectedID)
            {
                selectedID = GetSelectedIDs (artTreeView);
                TreeViewElement selection = artTreeView.treeModel.Find (selectedID);
                string fileName = Path.GetFileName (selection.path);
                string workingDirectory =  Application.dataPath + "/" + Path.GetDirectoryName (selection.path);

                commits = git.Run ($"log -n {commitNumber} --pretty=format:\"%h|%an|%cr|%s\" -- {fileName}", workingDirectory);
            } 

            if (selectedID != -1)
            {
                scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
                for (int i = 0; i < commits.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal ();
                    GUILayout.Label (commits[i].id, CARUtility.boldLabelStyle, new GUILayoutOption[] {GUILayout.Width (60)});
                    GUILayout.Label (commits[i].author, new GUILayoutOption[] {GUILayout.Width (100)});
                    GUILayout.Label (commits[i].date, new GUILayoutOption[] {GUILayout.Width (130)});
                    GUILayout.Label (commits[i].message, new GUILayoutOption[] {GUILayout.Width (250)});
                    EditorGUILayout.EndHorizontal ();
                }
                EditorGUILayout.EndScrollView ();
            }
            GUILayout.EndArea ();
        }

        int GetSelectedIDs (CARTreeView treeView)
        {
            return treeView.state.lastClickedID;
        }

        void DrawBottomToolBar (Rect rect, bool isConfig)
        {
            GUILayout.BeginArea (rect);

            using (new EditorGUILayout.HorizontalScope ())
            {
                var style = "miniButton";
                if (GUILayout.Button ("Expand All", style))
                {
                    if (isConfig) 
                        configTreeView.ExpandAll ();
                    else 
                        artTreeView.ExpandAll ();
                }

                if (GUILayout.Button ("Collapse All", style))
                {
                    if (isConfig)
                        configTreeView.CollapseAll ();
                    else
                        artTreeView.CollapseAll ();
                }

                GUILayout.FlexibleSpace ();
                
                GUILayout.Label (String.Empty);

                GUILayout.FlexibleSpace ();

                if (GUILayout.Button ("Set sorting", style))
                {
                    var columnHeader = (CARMultiColumnHeader) artTreeView.multiColumnHeader;
                    if (isConfig)
                        columnHeader = (CARMultiColumnHeader) configTreeView.multiColumnHeader;
                    
                    columnHeader.SetSortingColumns (new int[] {0, 1, 2}, new[] {true, true, true});
                    columnHeader.mode = CARMultiColumnHeader.Mode.DefaultHeader;
                }

                GUILayout.Label ("Header: ", "Label");

                if (GUILayout.Button ("Large", style))
                {
                    var columnHeader = (CARMultiColumnHeader) artTreeView.multiColumnHeader;
                    if (isConfig)
                        columnHeader = (CARMultiColumnHeader) configTreeView.multiColumnHeader;
                    columnHeader.mode = CARMultiColumnHeader.Mode.LargeHeader;
                }
                if (GUILayout.Button ("Default", style))
                {
                    var columnHeader = (CARMultiColumnHeader) artTreeView.multiColumnHeader;
                    if (isConfig)
                        columnHeader = (CARMultiColumnHeader) configTreeView.multiColumnHeader;
                    columnHeader.mode = CARMultiColumnHeader.Mode.DefaultHeader;
                }
                if (GUILayout.Button ("No Sort", style))
                {
                    var columnHeader = (CARMultiColumnHeader) artTreeView.multiColumnHeader;
                    if (isConfig)
                        columnHeader = (CARMultiColumnHeader) configTreeView.multiColumnHeader;
                    columnHeader.mode = CARMultiColumnHeader.Mode.MinimumHeaderWithoutSorting;
                }
                
                GUILayout.Space (10);
            }

            GUILayout.EndArea ();
        }
    }

    internal class CARMultiColumnHeader : MultiColumnHeader
    {
        Mode m_Mode;

        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        public Mode mode 
        {
            get { return m_Mode; }
            set 
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        public CARMultiColumnHeader (MultiColumnHeaderState state) : base (state)
        {
            mode = Mode.DefaultHeader;
        }

        protected override void ColumnHeaderGUI (MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            base.ColumnHeaderGUI (column, headerRect, columnIndex);

            if (mode == Mode.LargeHeader)
            {
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label (headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }

}
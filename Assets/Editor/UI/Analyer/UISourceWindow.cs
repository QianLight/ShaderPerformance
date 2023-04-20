using UnityEditor;
using UnityEngine;
namespace UIAnalyer
{
    public class UISourceWindow : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        public enum Mode
        {
            Source,
            Prefab
        }

        private Mode m_mode;
        private string[] m_modeTabs = new string[2] { "UI Source", "UI Prefab" };


        internal const float kButtonWidth = 150;
        [SerializeField]
        int m_DataSourceIndex;

        [SerializeField]
        internal UISourceAnalyer m_sourceAnalyer;



        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 40;

       // [MenuItem("Tools/UI/UISourceWindow", priority = 2050)]
        static void ShowWindow()
        {
            UISourceWindow instance = GetWindow<UISourceWindow>();
            instance.titleContent = new GUIContent("UISourceWindow");
            instance.Show();
        }

        [SerializeField]
        internal bool multiDataSource = false;
        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            if (menu != null)
                menu.AddItem(new GUIContent("Custom Sources"), multiDataSource, FlipDataSource);
        }
        internal void FlipDataSource()
        {
            multiDataSource = !multiDataSource;
        }

        private void OnEnable()
        {
            // Rect subPos = GetSubWindowArea();
            // if (m_sourceAnalyer == null) m_sourceAnalyer = new UISourceAnalyer();
            // m_sourceAnalyer.OnEnable(subPos, this);
            // m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
            // InitDataSources();
            Selection.selectionChanged = _OnSelectionChange;
        }

        protected void OnDisable(){
                Selection.selectionChanged = null;
        }

        private void _OnSelectionChange(){
            if(Selection.activeObject != null) 
                Debug.Log("Selection.activeObject :" +Selection.activeObject.name);

                if(Selection.activeGameObject != null) 
                Debug.Log("Selection.activeGameObject :" +Selection.activeGameObject.name);
                if(Selection.activeTransform != null) 
                Debug.Log("Selection.activeTransform :" +Selection.activeTransform.name);
                if(Selection.activeContext != null) 
                Debug.Log("Selection.activeContext :" +Selection.activeContext.name);

                if(Selection.activeInstanceID > 0) 
                Debug.Log("Selection.activeInstanceID :" +Selection.activeInstanceID);

        }
        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
        }

        public void OnBeforeSerialize()
        {
        }
        public void OnAfterDeserialize()
        {
        }

        private Rect GetSubWindowArea()
        {
            float padding = k_MenubarPadding;
            if (multiDataSource)
                padding += k_MenubarPadding * 0.5f;
            Rect subPos = new Rect(0, padding, position.width, position.height - padding);
            return subPos;
        }

        private void Update()
        {
            // switch (m_mode)
            // {
            //     case Mode.Prefab:
            //         m_sourceAnalyer.Update();
            //         break;
            //     case Mode.Source:
            //         m_sourceAnalyer.Update();
            //         break;
            // }
        }

        private void OnGUI()
        {
            // ModeToggle();
            // switch (m_mode)
            // {
            //     case Mode.Prefab:
            //         m_sourceAnalyer.OnGUI(GetSubWindowArea());
            //         break;
            //     case Mode.Source:
            //         m_sourceAnalyer.OnGUI(GetSubWindowArea());
            //         break;
            // }
        }

        void ModeToggle()
        {
            // GUILayout.BeginHorizontal();
            // GUILayout.Space(k_ToolbarPadding);

            // if (GUILayout.Button(m_RefreshTexture))
            // {
            //     m_sourceAnalyer.Refresh();
            // }
            // float toolbarWidth = position.width - k_ToolbarPadding * 4 - m_RefreshTexture.width;

            // m_mode = (Mode)GUILayout.Toolbar((int)m_mode, m_modeTabs, "LargeButton", GUILayout.Width(toolbarWidth));
            // GUILayout.FlexibleSpace();
            // GUILayout.EndHorizontal();
            //if (multiDataSource)
            //{
            //    //GUILayout.BeginArea(r);
            //    GUILayout.BeginHorizontal();

            //    using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            //    {
            //        GUILayout.Label("Bundle Data Source:");
            //        GUILayout.FlexibleSpace();
            //        var c = new GUIContent(string.Format("{0} ({1})", AssetBundleModel.Model.DataSource.Name, AssetBundleModel.Model.DataSource.ProviderName), "Select Asset Bundle Set");
            //        if (GUILayout.Button(c, EditorStyles.toolbarPopup))
            //        {
            //            GenericMenu menu = new GenericMenu();

            //            for (int index = 0; index < m_DataSourceList.Count; index++)
            //            {
            //                var ds = m_DataSourceList[index];
            //                if (ds == null)
            //                    continue;

            //                if (index > 0)
            //                    menu.AddSeparator("");

            //                var counter = index;
            //                menu.AddItem(new GUIContent(string.Format("{0} ({1})", ds.Name, ds.ProviderName)), false,
            //                    () =>
            //                    {
            //                        m_DataSourceIndex = counter;
            //                        var thisDataSource = ds;
            //                        AssetBundleModel.Model.DataSource = thisDataSource;
            //                        m_ManageTab.ForceReloadData();
            //                    }
            //                );

            //            }

            //            menu.ShowAsContext();
            //        }

            //        GUILayout.FlexibleSpace();
            //        if (AssetBundleModel.Model.DataSource.IsReadOnly())
            //        {
            //            GUIStyle tbLabel = new GUIStyle(EditorStyles.toolbar);
            //            tbLabel.alignment = TextAnchor.MiddleRight;

            //            GUILayout.Label("Read Only", tbLabel);
            //        }
            //    }

            //    GUILayout.EndHorizontal();
            //    //GUILayout.EndArea();
            //}
        }
    }
}
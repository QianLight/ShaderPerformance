using UnityEditor;
using UnityEngine;
namespace UIAnalyer
{
    public class UIAssetsAnalyerMain : EditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
    {
        public enum Mode
        {
            Source,
            Prefab,
            //System
        }

        private Mode m_mode;
        //    private string[] m_modeTabs = new string[3] { "UI Source", "UI Prefab" ,"UI  System"
            private string[] m_modeTabs = new string[2] { "UI Source", "UI Prefab"};

        private static UIAssetsAnalyerMain s_instance = null;
        internal static UIAssetsAnalyerMain instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = GetWindow<UIAssetsAnalyerMain>();
                return s_instance;
            }
        }

        internal const float kButtonWidth = 150;
        [SerializeField]
        int m_DataSourceIndex;

        [SerializeField]
        internal UISourceAnalyer m_sourceAnalyer;

        //private UISystemAnalyer m_systemAnalyer;
        private UIPrefabAnalyer m_prefabAnalyer; 

        private Texture2D m_RefreshTexture;

        const float k_ToolbarPadding = 15;
        const float k_MenubarPadding = 40;

        [MenuItem("Tools/UI/UI Analyer Browser", priority = 2050)]
        public static void ShowWindow()
        {
            s_instance = null;
            instance.titleContent = new GUIContent("UI Analyer Browser");
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
            UISourceUtils.InitSource();
            Rect subPos = GetSubWindowArea();
            if (m_sourceAnalyer == null) m_sourceAnalyer = new UISourceAnalyer();
            m_sourceAnalyer.OnEnable(subPos, this);

            if(m_prefabAnalyer == null) m_prefabAnalyer = new UIPrefabAnalyer();
            m_prefabAnalyer.OnEnable(subPos , this);

            //if(m_systemAnalyer == null) m_systemAnalyer = new UISystemAnalyer();
            //m_systemAnalyer.OnEnable(subPos, this);

            m_RefreshTexture = EditorGUIUtility.FindTexture("Refresh");
            InitDataSources();
        }
        private void InitDataSources()
        {
            //determine if we are "multi source" or not...
        }
        private void OnDisable()
        {
            if(m_sourceAnalyer != null) m_sourceAnalyer.OnDisable();
            if(m_prefabAnalyer != null) m_prefabAnalyer.OnDisable();
            //if(m_systemAnalyer != null) m_systemAnalyer.OnDisable();
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
            switch (m_mode)
            {
                case Mode.Prefab:
                    m_prefabAnalyer.Update();
                    break;
                case Mode.Source:
                    m_sourceAnalyer.Update();
                    break;
                //case Mode.System:
                //    m_systemAnalyer.Update();
                //break;
            }
        }

        private void OnGUI()
        {
            ModeToggle();
            switch (m_mode)
            {
                case Mode.Prefab:
                    m_prefabAnalyer.OnGUI(GetSubWindowArea());
                    break;
                case Mode.Source:
                    m_sourceAnalyer.OnGUI(GetSubWindowArea());
                    break;
                //case Mode.System:
                //    m_systemAnalyer.OnGUI(GetSubWindowArea());
                //    break;
            }
        }

        void ModeToggle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(k_ToolbarPadding);

            if (GUILayout.Button(m_RefreshTexture))
            {
                if (m_mode == Mode.Prefab)
                    m_prefabAnalyer.Refresh();
                else if (m_mode == Mode.Source)
                    m_sourceAnalyer.Refresh();
                //else if(m_mode == Mode.System)
                //    m_sourceAnalyer.Refresh();
                //else
                //    m_systemAnalyer.Refresh();
            }
            float toolbarWidth = position.width - k_ToolbarPadding * 4 - 40;

            m_mode = (Mode)GUILayout.Toolbar((int)m_mode, m_modeTabs, "LargeButton", GUILayout.Width(toolbarWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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
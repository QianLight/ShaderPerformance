using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIAnalyer
{
    public enum UISourceType
    {
        Sprite,
        Texture
    }
    public class UIPrefabAnalyer
    {
        private UISourceType m_sourceType = UISourceType.Sprite;

        // private UISourceType m_oldSourceType = UISourceType.Sprite;
        private UIPrefabTree m_prefabTree;
        private TreeViewState m_sourceTreeState;

        private TreeViewState m_prefabDepTreeState;
        private UIPrefabSelectionTree m_prefabDepTree;
        private UIPrefabModel m_prefabModel;

        public const float k_SplitterWidth = 3f;
        Rect m_HorizontalSplitterRect, m_VerticalSplitterRectRight, m_VerticalSplitterRectLeft;
        [SerializeField]
        float m_HorizontalSplitterPercent;
        [SerializeField]
        float m_VerticalSplitterPercentRight;
        [SerializeField]
        float m_VerticalSplitterPercentLeft;

        bool m_ResizingHorizontalSplitter = false;
        bool m_ResizingVerticalSplitterRight = false;
        bool m_ResizingVerticalSplitterLeft = false;

        public UISourceType sourceType { get { return m_sourceType; } }
        public UIPrefabModel sourceModel { get { return m_prefabModel; } }

        public UIPrefabAnalyer()
        {
            m_HorizontalSplitterPercent = 0.4f;
            m_VerticalSplitterPercentRight = 0.7f;
            m_VerticalSplitterPercentLeft = 0.85f;
            m_prefabModel = new UIPrefabModel();
        }
        public void OnDisable(){
            if(m_prefabModel != null){
                m_prefabModel.Clear();
            }
        }
        private void OnSelectionChangeed()
        {
            Debug.Log("Select : " + Selection.activeGameObject.name);
        }

        public void SetSelection(UISourceTreeItem source)
        {
            m_prefabModel.SetSeletion(source.Source as UISourcePrefabInfo);
            m_prefabDepTree.Reload();
            m_prefabDepTree.ExpandAll();
            // m_prefabTree.Reload();
            // m_prefabTree.SetExpanded(source.id , true);
        }
        #region  GUI
        public Rect m_Position;
        public EditorWindow m_Parent = null;
        public SearchField m_searchField;
        public void OnEnable(Rect pos, EditorWindow parent)
        {
            m_Parent = parent;
            m_Position = pos;
            m_HorizontalSplitterRect = new Rect(
                (int)(m_Position.x + m_Position.width * m_HorizontalSplitterPercent),
                m_Position.y,
                k_SplitterWidth,
                m_Position.height);
            m_VerticalSplitterRectRight = new Rect(
                m_HorizontalSplitterRect.x,
                (int)(m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight),
                (m_Position.width - m_HorizontalSplitterRect.width) - k_SplitterWidth,
                k_SplitterWidth);
            m_VerticalSplitterRectLeft = new Rect(
                m_Position.x,
                (int)(m_Position.y + m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft),
                (m_HorizontalSplitterRect.width) - k_SplitterWidth,
                k_SplitterWidth);
            m_prefabModel.InitUISourceInfo();
            m_searchField = new SearchField();
            m_sourceTreeState = new TreeViewState();
            m_prefabDepTreeState = new TreeViewState();

            m_prefabTree = new UIPrefabTree(m_sourceTreeState, this);
            m_prefabDepTree = new UIPrefabSelectionTree(m_prefabDepTreeState, this);
        }


        public void Refresh()
        {
            m_prefabModel.InitUISourceInfo();
            m_prefabTree.Reload();
            m_prefabDepTree.Reload();
        }

        private float lineHeght = 20f;
        public void OnGUI(Rect pos)
        {
            m_Position = pos;
            HandleHorizontalResize();
            HandleVerticalResize();
            var sourceSearchRect = new Rect(m_Position.x, m_Position.y, m_HorizontalSplitterRect.x, lineHeght);
            var sourceTreeRect = new Rect(m_Position.x, m_Position.y + lineHeght, m_HorizontalSplitterRect.x, m_VerticalSplitterRectLeft.y - lineHeght * 2);
            m_prefabTree.searchString = m_searchField.OnGUI(sourceSearchRect, m_prefabTree.searchString);
            m_prefabTree.OnGUI(sourceTreeRect);
            float panelLeft = m_HorizontalSplitterRect.x + k_SplitterWidth;
            float panelWidth = m_VerticalSplitterRectRight.width - k_SplitterWidth * 2;
            float searchHeight = 20f;
            float panelTop = m_Position.y + searchHeight;
            float panelHeight = m_VerticalSplitterRectRight.y - panelTop;
            var rightUp = new Rect(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight);
            m_prefabDepTree.OnGUI(rightUp);
            string message = sourceModel.Message;
            if (!string.IsNullOrEmpty(message))
            {
                EditorGUI.HelpBox(new Rect(panelLeft, rightUp.y + rightUp.height, panelWidth, m_VerticalSplitterRectRight.y - rightUp.y - rightUp.height), message, MessageType.Warning);
            }

            var sourceTips = new Rect(m_Position.x , sourceTreeRect.y+sourceTreeRect.height+5, sourceTreeRect.width, m_Position.height-sourceTreeRect.y - sourceTreeRect.height+20);
            EditorGUI.HelpBox(sourceTips, staticMessage,MessageType.Warning);
        }
        private string staticMessage = "1、双击或者右键能显示当前图片或者打开Prefab指定到目标节点 \t\n\t\n 2、点击显示所有引用的 Altas 和  Texture ";



        public void Update()
        {


        }


        private void HandleHorizontalResize()
        {
            m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            m_HorizontalSplitterRect.height = m_Position.height;

            EditorGUIUtility.AddCursorRect(m_HorizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && m_HorizontalSplitterRect.Contains(Event.current.mousePosition))
                m_ResizingHorizontalSplitter = true;

            if (m_ResizingHorizontalSplitter)
            {
                m_HorizontalSplitterPercent = Mathf.Clamp(Event.current.mousePosition.x / m_Position.width, 0.1f, 0.9f);
                m_HorizontalSplitterRect.x = (int)(m_Position.width * m_HorizontalSplitterPercent);
            }

            if (Event.current.type == EventType.MouseUp)
                m_ResizingHorizontalSplitter = false;
        }

        private void HandleVerticalResize()
        {
            m_VerticalSplitterRectRight.x = m_HorizontalSplitterRect.x;
            m_VerticalSplitterRectRight.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight);
            m_VerticalSplitterRectRight.width = m_Position.width - m_HorizontalSplitterRect.x;
            m_VerticalSplitterRectLeft.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft);
            m_VerticalSplitterRectLeft.width = m_VerticalSplitterRectRight.width;


            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRectRight, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VerticalSplitterRectRight.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitterRight = true;

            EditorGUIUtility.AddCursorRect(m_VerticalSplitterRectLeft, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VerticalSplitterRectLeft.Contains(Event.current.mousePosition))
                m_ResizingVerticalSplitterLeft = true;


            if (m_ResizingVerticalSplitterRight)
            {
                m_VerticalSplitterPercentRight = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalSplitterRect.height, 0.2f, 0.98f);
                m_VerticalSplitterRectRight.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentRight);
            }
            else if (m_ResizingVerticalSplitterLeft)
            {
                m_VerticalSplitterPercentLeft = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalSplitterRect.height, 0.25f, 0.98f);
                m_VerticalSplitterRectLeft.y = (int)(m_HorizontalSplitterRect.height * m_VerticalSplitterPercentLeft);
            }


            if (Event.current.type == EventType.MouseUp)
            {
                m_ResizingVerticalSplitterRight = false;
                m_ResizingVerticalSplitterLeft = false;
            }
        }
        #endregion
    }
}
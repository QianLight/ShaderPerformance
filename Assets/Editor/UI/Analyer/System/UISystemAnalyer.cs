using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIAnalyer
{
    public class UISystemAnalyer
    {
        private UISystemTree m_systemTree;
        private TreeViewState m_systemTreeState;

        private UISystemSelection m_selectionGUI;

        // private TreeViewState m_prefabDepTreeState;
        // private UIPrefabSelectionTree m_prefabDepTree;
        private UISystemModel m_systemModel;

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

        public UISystemTree systemTree { get { return m_systemTree; } }
        public UISystemModel sourceModel { get { return m_systemModel; } }

        public UISystemSelection selectionGUI { get { return m_selectionGUI; } }

        public UISystemAnalyer()
        {
            m_HorizontalSplitterPercent = 0.4f;
            m_VerticalSplitterPercentRight = 0.7f;
            m_VerticalSplitterPercentLeft = 0.85f;
            m_systemModel = new UISystemModel();
            m_selectionGUI = new UISystemSelection(this);
        }
        public void OnDisable()
        {
            if (m_systemModel != null)
            {
                m_systemModel.Clear();
            }
        }
        private void OnSelectionChangeed()
        {
            Debug.Log("Select : " + Selection.activeGameObject.name);
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
            m_systemModel.InitUISourceInfo();
            m_searchField = new SearchField();
            m_systemTreeState = new TreeViewState();
            // m_prefabDepTreeState = new TreeViewState();
            m_systemTree = new UISystemTree(m_systemTreeState, this);
            // m_prefabDepTree = new UIPrefabSelectionTree(m_prefabDepTreeState, this);
        }


        public void Refresh()
        {
            m_systemModel.InitUISourceInfo();
            m_systemTree.Reload();
            // m_prefabDepTree.Reload();
        }

        private float lineHeght = 20f;
        public void OnGUI(Rect pos)
        {
            m_Position = pos;
            HandleHorizontalResize();
            HandleVerticalResize();
            var buttonRect = new Rect(m_Position.x, m_Position.y, m_Position.width, lineHeght);
            DrawButtons(buttonRect);
            var sourceSearchRect = new Rect(m_Position.x, m_Position.y + lineHeght, m_HorizontalSplitterRect.x, lineHeght);
            var sourceTreeRect = new Rect(m_Position.x, m_Position.y + lineHeght * 2, m_HorizontalSplitterRect.x, m_VerticalSplitterRectLeft.y - lineHeght * 2);
            m_systemTree.searchString = m_searchField.OnGUI(sourceSearchRect, m_systemTree.searchString);
            m_systemTree.OnGUI(sourceTreeRect);


            float panelLeft = m_HorizontalSplitterRect.x + k_SplitterWidth;
            float panelWidth = m_VerticalSplitterRectRight.width - k_SplitterWidth * 2;
            float panelTop = m_Position.y + lineHeght;
            float panelHeight = m_VerticalSplitterRectRight.y - panelTop;
            var rightUp = new Rect(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight);
            m_selectionGUI.OnGUI(new Rect(
                    panelLeft,
                    panelTop,
                    panelWidth,
                    panelHeight));
            // string message = sourceModel.Message;
            // if (!string.IsNullOrEmpty(message))
            // {
            //     EditorGUI.HelpBox(new Rect(panelLeft, rightUp.y + rightUp.height, panelWidth, m_VerticalSplitterRectRight.y - rightUp.y - rightUp.height), message, MessageType.Warning);
            // }

            var sourceTips = new Rect(m_Position.x, sourceTreeRect.y + sourceTreeRect.height + 5, sourceTreeRect.width, m_Position.height - sourceTreeRect.y - sourceTreeRect.height + 20);
            EditorGUI.HelpBox(sourceTips, staticMessage, MessageType.Warning);
            ExecuteKeyCode();
        }

        private void DrawButtons(Rect position)
        {
            position.width = 50;
            if (GUI.Button(position, "新  建"))
            {
                m_selectionGUI.CreateUISystem();
            }
            position.x += position.width + 10;
            position.width = 50;
            if (GUI.Button(position, "保  存"))
            {
                m_systemModel.SaveSourceInfo();
            }
        }

        private void RowGUISelection()
        {

        }
        private string staticMessage = "辅助阅读SystemDefine用.如果用修改后保存的文档会去掉Excel格式化设置,如果用excel打开，先打开空的excel,数据>文本打开根据默认设置确定";

        private bool m_keyCode_s = false;
        private bool m_keyCode_lctrl = false;
        public void Update()
        {

        }

        private void ExecuteKeyCode()
        {
            if (Event.current == null) return;
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.LeftControl)
                    m_keyCode_lctrl = true;
                else if (Event.current.keyCode == KeyCode.S)
                    m_keyCode_s = true;
                ExecuteCommand(m_keyCode_lctrl && m_keyCode_s);
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode == KeyCode.LeftControl)
                    m_keyCode_lctrl = false;
                else if (Event.current.keyCode == KeyCode.S)
                    m_keyCode_s = false;
                ExecuteCommand(m_keyCode_lctrl && m_keyCode_s);
            }
        }

        private bool m_lastExe = false;
        private void ExecuteCommand(bool exe)
        {
            if (m_lastExe != exe)
            {
                m_lastExe = exe;
                UnityEngine.Debug.Log("ExecuteCommand:" + m_keyCode_lctrl + ":" + m_keyCode_s);
                if (m_lastExe)
                    m_systemModel.SaveSourceInfo();
            }
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
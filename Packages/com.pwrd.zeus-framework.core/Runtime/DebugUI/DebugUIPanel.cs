using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Core
{
    public class DebugUIPanel : MonoBehaviour
    {
        public enum LogAnchor
        {
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
            CenterTop,
            CenterBottom,
            Center
        }

        private class MenuItem
        {
            public int Layer;
            public MenuItem Parent;
            public string Path;
            public List<MenuItem> Children = new List<MenuItem>();
            public string Title;
            public Action OnClick;
            public Rect Rect;
        }

        private static DebugUIPanel m_instance;
        public static DebugUIPanel Instance
        {
            get
            {
                if(m_instance == null)
                {
                    var go = new GameObject("DebugUIPanel");
                    m_instance = go.AddComponent<DebugUIPanel>();
                    m_instance.InitStyle();
                    DontDestroyOnLoad(m_instance.gameObject);
                }
                return m_instance;
            }
        }
        private bool m_showMenu;
        private bool m_isDirty;
        private MenuItem m_currentRootMenu;
        [SerializeField]
        private GUIStyle m_buttonStyle;
        [SerializeField]
        private GUIStyle m_logStyle;
        Dictionary<string, MenuItem> m_pathMenuMap = new Dictionary<string, MenuItem>();
        Dictionary<int, List<MenuItem>> m_layerMenuMap = new Dictionary<int, List<MenuItem>>();
        private const float ButtonHeightFactor = 0.618f;
        private int m_countInARow = 0;
        private int m_buttonWidth = 100;
        private int m_buttonHeight= 100;
        private int m_border = 2;
        private const float FontSizeFactor = 20f / 1920;
        private const float CountInARowFactor = 10f / 1920;
        private const int MaxRowCount = 4;
        private const int RootLayer = 0;
        private Texture2D m_normalButtonBG;
        private Texture2D m_activeButtonBG;
        private Texture2D m_logBG;
        private Font m_currentFont;
        private Action m_onShow;
        private Action m_onHide;

        private void InitStyle()
        {
            var fonts = Font.GetOSInstalledFontNames();
            m_currentFont = Font.CreateDynamicFontFromOSFont(fonts[0], 20);
            if(m_normalButtonBG == null)
            {
                m_normalButtonBG = new Texture2D(1, 1);
                m_normalButtonBG.SetPixel(0, 0, Color.white);
                m_normalButtonBG.Apply();
            }
            if(m_activeButtonBG == null)
            {
                m_activeButtonBG = new Texture2D(1, 1);
                m_activeButtonBG.SetPixel(0, 0, Color.blue);
                m_activeButtonBG.Apply();
            }
            if(m_logBG == null)
            {
                m_logBG = new Texture2D(1, 1);
                m_logBG.SetPixel(0, 0, new Color(0,0,0,0.3f));
                m_logBG.Apply();
            }
            int fontSize;
            if(Screen.width > Screen.height)
            {
                fontSize = (int)(Screen.width * FontSizeFactor);
            }
            else
            {
                fontSize = (int)(Screen.height * FontSizeFactor);
            }
            m_buttonStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset
                {
                    bottom = 2,
                    left = 2,
                    right = 2, 
                    top = 2,
                },
                normal = new GUIStyleState
                {
                    textColor = Color.black,
                    background = m_normalButtonBG,
                },
                active = new GUIStyleState
                {
                    textColor = Color.white,
                    background = m_activeButtonBG,
                },
                wordWrap = true,
                fontSize = fontSize,
                font = m_currentFont,
            };
            m_logStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState
                {
                    textColor = Color.white,
                    background = m_logBG,
                },
                wordWrap = true,
                fontSize = fontSize,
                font = m_currentFont,
            };
        }

        private void RefreshLayoutInfo(int buttonCount)
        {
            var minCountInARow = buttonCount / MaxRowCount;
            m_countInARow = Mathf.Max(minCountInARow, (int)(Screen.width * CountInARowFactor));
            m_buttonWidth = Screen.width / m_countInARow;
            m_buttonHeight = (int)(m_buttonWidth * ButtonHeightFactor);
        }

        private MenuItem FindMenu(string path)
        {
            var r = m_pathMenuMap.TryGetValue(path, out var menu);
            if (r)
            {
                return menu;
            }
            return null;
        }

        public void RegisterCallback(Action onShow, Action onHide)
        {
            m_onShow = onShow;
            m_onHide = onHide;
        }

        private string m_tempLog;
        private Func<string> m_logGetter;
        private const float LogShowOnceDuration = 5;
        private float m_logShowElapse;
        private bool m_showLog;
        private bool m_showLogOnce;
        public void ShowLogOnce(string log)
        {
            m_tempLog = log;
            m_logShowElapse = 0;
            m_showLogOnce = true;
        }

        public void ShowLog(Func<string> logGetter)
        {
            m_logGetter = logGetter;
            m_showLog = true;
        }

        public void HideLog()
        {
            m_showLog = false;
        }

        private LogAnchor m_logAnchor;
        private float m_logScale = DefaultLogScale;
        private const float DefaultLogScale = 0.2f;
        public void SetLogPanelAnchor(LogAnchor anchor)
        {
            m_logAnchor = anchor;
        }

        public void SetLogPanelScale(float scale)
        {
            m_logScale = DefaultLogScale * scale;
        }

        public void AddMenu(string path, Action action)
        {
            var menusTitles = path.Split('/');
            var menuPath = "";
            MenuItem parentMenu = null;
            for (var i = 0; i < menusTitles.Length - 1; i++)
            {
                var menuTitle = menusTitles[i];
                menuPath += menuTitle + "/";
                var menu = FindMenu(menuPath);
                if (menu == null)
                {
                    menu = new MenuItem { Layer = i, Title = menuTitle, Path = menuPath };
                    menu.OnClick = () =>
                    {
                        m_currentRootMenu = menu;
                    };
                    if (null != parentMenu)
                    {
                        menu.Parent = parentMenu;
                        parentMenu.Children.Add(menu);
                    }
                    AddMenuToMap(menu);
                }
                parentMenu = menu;
            }
            var actionMenu = new MenuItem { Title = menusTitles[menusTitles.Length - 1], Parent = parentMenu, Layer = menusTitles.Length - 1, OnClick = action, Path = path };
            if (parentMenu != null)
            {
                parentMenu.Children.Add(actionMenu);
            }
            AddMenuToMap(actionMenu);
            m_isDirty = true;
        }

        private void AddMenuToMap(MenuItem menu)
        {
            if (!m_layerMenuMap.TryGetValue(menu.Layer, out var list))
            {
                m_layerMenuMap[menu.Layer] = list = new List<MenuItem>();
            }
            list.Add(menu);
            m_pathMenuMap.Add(menu.Path, menu);
        }

        public void Show()
        {
            m_currentRootMenu = null;
            m_showMenu = true;
            m_onShow?.Invoke();
        }

        public void Start()
        {

        }

        public void Hide()
        {
            m_showMenu = false;
            m_onHide?.Invoke();
        }

        private int m_touchCount = 0;
        /// <summary>
        /// 点击触发次数
        /// </summary>
        private const int TriggerCount = 3;
        private float m_triggerElapsed = 0f;
        /// <summary>
        /// 每次点击最长间隔
        /// </summary>
        private const float ClearDuration = 1;
        private Vector3? m_touchPosition;
        private void UpdateOpenTrigger()
        {
            if(!Input.GetMouseButtonUp(0))
            {
                return;
            }
            var triggerRect = new Rect(0, 0, Screen.width * 0.1f, Screen.height * 0.1f);
            if(triggerRect.Contains(Input.mousePosition))
            {
                m_touchCount++;
                m_triggerElapsed = 0;
                if(m_touchCount >= TriggerCount)
                {
                    m_touchCount = 0;
                    Show();
                }
            }
        }

        private void Update()
        {
            UpdateOpenTrigger();
            UpdateElapsed(Time.deltaTime);
        }

        private void UpdateElapsed(float elapse)
        {
            m_logShowElapse += elapse;
            if(m_logShowElapse > LogShowOnceDuration)
            {
                m_showLogOnce = false;
            }
            m_triggerElapsed += elapse;
            if(m_touchCount > 0 && m_triggerElapsed > ClearDuration)
            {
                m_touchCount = 0;
            }
        }

        private void OnGUI()
        {
            if (m_showMenu)
            {
                if (m_isDirty)
                {
                    m_isDirty = false;
                }
                if (null == m_currentRootMenu)
                {
                    var rootMenus = m_layerMenuMap[RootLayer];
                    RefreshLayoutInfo(rootMenus.Count);
                    ShowMenus(rootMenus);
                    if (GUI.Button(new Rect(0, 0, m_buttonWidth - m_border, m_buttonHeight - m_border), "关闭", m_buttonStyle))
                    {
                        Hide();
                    }
                }
                else
                {
                    RefreshLayoutInfo(m_currentRootMenu.Children.Count + 1);
                    ShowMenus(m_currentRootMenu.Children);
                    if (GUI.Button(new Rect(0, 0, m_buttonWidth - m_border, m_buttonHeight - m_border), "回到上一级", m_buttonStyle))
                    {
                        m_currentRootMenu = m_currentRootMenu.Parent;
                    }
                }

            }
            if (m_showLog || m_showLogOnce)
            {
                //TopCenter
                //GUI.Box(new Rect(Screen.width / 2 - 100, 0, Screen.width * 0.2f, Screen.height * 0.2f), m_logGetter?.Invoke(), m_logStyle);
                //TopLeft
                var rect = GetLogRect();
                if(m_showLogOnce)
                {
                    GUI.Box(rect, m_tempLog, m_logStyle);
                }
                else
                {
                    GUI.Box(rect, m_logGetter?.Invoke(), m_logStyle);
                }
            }
        }

        private Rect GetLogRect()
        {
            var width = Screen.width * m_logScale;
            var height = Screen.height * m_logScale;
            switch(m_logAnchor)
            {
                case LogAnchor.CenterBottom:
                    return new Rect((Screen.width - width) * 0.5f, Screen.height - height, width, height);
                case LogAnchor.Center:
                    return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
                case LogAnchor.LeftBottom:
                    return new Rect(0, Screen.height - height, width, height);
                case LogAnchor.LeftTop:
                    return new Rect(0, 0, width, height);
                case LogAnchor.RightBottom:
                    return new Rect(Screen.width - width, Screen.height - height, width, height);
                 case LogAnchor.RightTop:
                    return new Rect(Screen.width - width, 0, width, height);
                case LogAnchor.CenterTop:
                default:
                    return new Rect((Screen.width - width) * 0.5f, 0, width, height);
            }
        }

        private void ShowMenus(List<MenuItem> menus)
        {
            for(var i = 0; i < menus.Count; i++)
            {
                var index = i + 1;
                var x = index % m_countInARow;
                var y = index / m_countInARow;
                var menu = menus[i];
                var rect = new Rect(x * m_buttonWidth, y * m_buttonHeight, m_buttonWidth - m_border, m_buttonHeight - m_border);
                if (GUI.Button(rect, menu.Title, m_buttonStyle))
                {
                    menu.Rect = rect;
                    menu.OnClick?.Invoke();
                }
            }
        }
    }
}

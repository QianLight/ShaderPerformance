using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public static class StyleHelp
    {
        public static GUIStyle ForNormalState(this GUIStyle style, Texture2D texture)
        {
            style.normal.background = texture;
            return style;
        }
    }

    public static class RectEx
    {
        public static Vector2 Snap(this Vector2 pos, float snapSize)
        {
            var x = Math.Round(pos.x / snapSize) * snapSize;
            var y = Math.Round(pos.y / snapSize) * snapSize;
            return new Vector2((float)x, (float)y);
        }

        public static Rect Scale(this Rect r, float scale)
        {
            return new Rect(r.x * scale, r.y * scale, r.width * scale, r.height * scale);
        }

        public static Rect SetPos(this Rect r, float x, float y)
        {
            r.x = x;
            r.y = y;
            return r;
        }

        public static Rect Normalize(this Rect r, float scale)
        {
            return new Rect(r.x / scale, r.y / scale, r.width / scale, r.height / scale);
        }

        public static RectOffset Scale(this RectOffset r, float scale)
        {
            return new RectOffset(Mathf.RoundToInt(r.left * scale), Mathf.RoundToInt(r.right * scale),
                Mathf.RoundToInt(r.top * scale), Mathf.RoundToInt(r.bottom * scale));
        }

        public static Rect Move(this Rect source, Vector2 move)
        {
            return new Rect(Math.Max(0, source.x + move.x), Math.Max(0, source.y + move.y), source.width, source.height);
        }

        public static Rect Add(this Rect source, Rect add)
        {
            return new Rect(source.x + add.x, source.y + add.y, source.width + add.width, source.height + add.height);
        }

        public static GUIStyle Scale(this GUIStyle style, float scale)
        {
            var s = new GUIStyle(style);
            s.fontSize = Mathf.RoundToInt(style.fontSize * scale);
            s.fixedHeight = Mathf.RoundToInt(style.fixedHeight * scale);
            s.fixedWidth = Mathf.RoundToInt(style.fixedWidth * scale);
            s.padding = s.padding.Scale(scale);
            s.margin = s.margin.Scale(scale);
            return s;
        }
    }

    public class BlueprintStyles
    {
        public static float Scale
        {
            get { return _scale; }
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    ClearCacheStyle();
                }
            }
        }
        private static float _scale = 1;

        private static GUIStyle _header;
        private static GUIStyle _tag1;
        private static GUIStyle _tag2;
        private static GUIStyle _titleBarStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _pinDefaultValueStyle;
        private static GUIStyle _pinTextStyle;
        // private static GUIStyle _pinMainStyle;
        // private static GUIStyle _pinDataStyle;
        private static GUIStyle _noteTextStyle;
        private static GUIStyle _areaCommentStyle;

        private static GUIStyle _boxHighlighter2;
        private static GUIStyle _boxHighlighter3;
        private static GUIStyle _boxHighlighter4;
        private static GUIStyle _boxHighlighterExecute;
        private static GUIStyle _boxHighlighter6;
        private static GUIStyle _nodeBackgroundBorderless;
        private static GUIStyle _nodeBackground;
        private static GUIStyle _noteBackground;

        private static GUIStyle _tabOn;
        private static GUIStyle _tabOff;
        private static GUIStyle _tabClose;

        private static GUIStyle _toolbar;
        private static GUIStyle _toolbarButton;

        private static void ClearCacheStyle()
        {
            _header = null;
            _tag1 = null;
            _tag2 = null;
            _titleBarStyle = null;
            _headerStyle = null;
            _pinDefaultValueStyle = null;
            _pinTextStyle = null;
            // _pinMainStyle = null;
            // _pinDataStyle = null;

            _boxHighlighter2 = null;
            _boxHighlighter3 = null;
            _boxHighlighter4 = null;
            _boxHighlighterExecute = null;
            _boxHighlighter6 = null;
            _nodeBackgroundBorderless = null;
            _nodeBackground = null;
        }
        public static GUIStyle TabOn
        {
            get
            {
                if (_tabOn == null)
                    _tabOn = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeTabOn), textColor = new Color(0.75f, 0.75f, 0.75f) },
                        padding = (new RectOffset(8, 8, 5, 3)),
                        border = new RectOffset(10, 10, 10, 0),
                        fixedHeight = 19f,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(10),
                        fontStyle = FontStyle.Normal
                    };
                return _tabOn;
            }
        }

        public static GUIStyle TabOff
        {
            get
            {
                if (_tabOff == null)
                    _tabOff = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeTabOff), textColor = new Color(0.75f, 0.75f, 0.75f) },
                        padding = (new RectOffset(8, 8, 5, 3)),
                        border = new RectOffset(10, 10, 10, 0),
                        fixedHeight = 19f,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(10),
                        fontStyle = FontStyle.Normal
                    };
                return _tabOff;
            }
        }

        public static GUIStyle TabClose
        {
            get
            {
                if (_tabClose == null)
                    _tabClose = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BluePrint/none"), textColor = new Color(0.75f, 0.75f, 0.75f) },
                        //padding = (new RectOffset(8, 8, 5, 3)),
                        //border = new RectOffset(10, 10, 10, 0),
                        fixedHeight = 14f ,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(12),
                        fontStyle = FontStyle.Normal
                    };
                return _tabClose;
            }
        }

        public static GUIStyle Tag1
        {
            get
            {
                if (_tag1 == null)
                    _tag1 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeTag1), textColor = new Color(0.75f, 0.75f, 0.75f) },
                        padding = (new RectOffset(8, 8, 5, 3)),
                        border = new RectOffset(10, 10, 10, 0),
                        fixedHeight = 19f * Scale,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(10 * Scale),
                        fontStyle = FontStyle.Normal
                    };
                return _tag1;
            }
        }

        public static GUIStyle Tag2
        {
            get
            {
                if (_tag2 == null)
                    _tag2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeTag1), textColor = Color.white },
                        padding = new RectOffset(7, 7, 5, 3),
                        border = new RectOffset(10, 10, 10, 0),
                        fixedHeight = 19f * Scale,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(12 * Scale),
                        fontStyle = FontStyle.Bold
                    };
                return _tag2;
            }
        }

        public static GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                    _headerStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.white },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(14f * Scale),
                        fontStyle = FontStyle.Bold,

                        alignment = TextAnchor.MiddleLeft
                    };
                return _headerStyle;
            }
        }

        public static GUIStyle PinDefaultValueStyle
        {
            get
            {
                if (_pinDefaultValueStyle == null)
                    _pinDefaultValueStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.white },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(9f * Scale),
                        fontStyle = FontStyle.Normal,

                        alignment = TextAnchor.MiddleLeft
                    };
                return _pinDefaultValueStyle;
            }
        }

        public static GUIStyle AreaCommentStyle
        {
            get
            {
                if (_areaCommentStyle == null)
                    _areaCommentStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.white },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(12f * Scale),
                        fontStyle = FontStyle.Normal,

                        alignment = TextAnchor.UpperLeft,
                    };
                return _areaCommentStyle;
            }
        }

        public static GUIStyle PinTextStyle
        {
            get
            {
                if (_pinTextStyle == null)
                    _pinTextStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.white },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(10f * Scale),
                        fontStyle = FontStyle.Normal,

                        alignment = TextAnchor.MiddleLeft
                    };
                return _pinTextStyle;
            }
        }

        public static GUIStyle NoteTextStyle
        {
            get
            {
                if (_noteTextStyle == null)
                    _noteTextStyle = new GUIStyle()
                    {
                        normal = { textColor = Color.black },//new Color(0.35f, 0.35f, 0.35f) },
                                                             //padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(10f * Scale),
                        fontStyle = FontStyle.Normal,

                        alignment = TextAnchor.MiddleLeft
                    };
                return _noteTextStyle;
            }
        }

        public static GUIStyle BoxHighlighter2
        {
            get
            {
                if (_boxHighlighter2 == null)
                    _boxHighlighter2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeSelectBackground), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(6, 6, 6, 6),
                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter2;
            }
        }

        public static GUIStyle BoxHighlighter3
        {
            get
            {
                if (_boxHighlighter3 == null)
                    _boxHighlighter3 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeHoverBackground), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(6, 6, 6, 6),
                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter3;
            }
        }

        public static GUIStyle BoxHighlighter4
        {
            get
            {
                if (_boxHighlighter4 == null)
                    _boxHighlighter4 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BluePrint/BoxHighlighter4"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(3, 3, 6, 7),
                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter4;
            }
        }

        public static GUIStyle BoxHighlighterExecute
        {
            get
            {
                if (_boxHighlighterExecute == null)
                    _boxHighlighterExecute = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeExcutedBackground), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(6, 6, 6, 6),

                    };

                return _boxHighlighterExecute;
            }
        }

        public static GUIStyle BoxHighlighter6
        {
            get
            {
                if (_boxHighlighter6 == null)
                    _boxHighlighter6 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BluePrint/BoxHighlighter6"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(5, 5, 5, 5),
                        //padding = new RectOffset(7, 7, 7, 7).Scale(Scale)
                    };

                return _boxHighlighter6;
            }
        }

        public static GUIStyle NodeBackgroundBorderless
        {
            get
            {
                if (_nodeBackgroundBorderless == null)
                    _nodeBackgroundBorderless = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BluePrint/Box12Borderless"), textColor = new Color(0.82f, 0.82f, 0.82f) },

                        stretchHeight = true,
                        stretchWidth = true,

                        //border = new RectOffset(20,20,20,20)
                        border = new RectOffset(44, 50, 20, 34).Scale(Scale),
                        //padding = new RectOffset(9,1,19,9)

                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _nodeBackgroundBorderless;
            }
        }

        public static GUIStyle NodeBackground
        {
            get
            {
                if (_nodeBackground == null)
                    _nodeBackground = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNodeBackground), textColor = new Color(0.82f, 0.82f, 0.82f) },

                        stretchHeight = true,
                        stretchWidth = true,

                        border = new RectOffset(14, 14, 14, 14)
                        //border = new RectOffset(44, 50, 20, 34).Scale(Scale),
                        //padding = new RectOffset(9,1,19,9)

                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _nodeBackground;
            }
        }

        public static GUIStyle NoteBackground
        {
            get
            {
                if (_noteBackground == null)
                    _noteBackground = new GUIStyle
                    {
                        normal = { background = GetSkinTexture(BluePrintHelper.strNoteBackground), textColor = new Color(0.82f, 0.82f, 0.82f) },

                        stretchHeight = true,
                        stretchWidth = true,

                        border = new RectOffset(14, 14, 14, 14)
                    };

                return _noteBackground;
            }
        }

        public static GUIStyle TitleBarStyle
        {
            get
            {
                if (_titleBarStyle == null)
                    _titleBarStyle = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BluePrint/Background"), textColor = new Color(0.7f, 0.7f, 0.7f) },
                        padding = new RectOffset(2, 2, 2, 2).Scale(Scale),
                        margin = new RectOffset(0, 0, 0, 0).Scale(Scale),
                        fixedHeight = 45f * Scale,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = (int)(14 * Scale),
                        fontStyle = FontStyle.Bold
                        //fontStyle = FontStyle.Bold
                    };
                return _titleBarStyle;
            }
        }

        static Dictionary<string, string> skinDic = new Dictionary<string, string>();
        public static Texture2D GetSkinTexture(string name)
        {
            if (!skinDic.ContainsKey(name))
                skinDic.Add(name, String.Format("Assets/Editor Default Resources/images/{0}.png", name));
            return AssetDatabase.LoadMainAssetAtPath(skinDic[name]) as Texture2D;
        }

        public static GUIStyle Header()
        {
            if (_header == null)
                _header = new GUIStyle
                {
                    //normal = { background = texture },
                    //padding = new RectOffset(-9, 1, 19, 9),
                    stretchHeight = true,
                    stretchWidth = true,
                    //border = new RectOffset(16, 16, 13, 0).Scale(Scale),
                    border = new RectOffset(5, 5, 5, 13),
                    // fixedHeight = 36,
                    // border = new RectOffset(44, 50, 20, 34),
                    //padding = new RectOffset(7, 7, 7, 7)
                };

            return _header;
        }

        public static GUIStyle Toolbar()
        {
            if (_toolbar == null)
            {
                _toolbar = EditorStyles.toolbar.Scale(1.5f);
            }

            return _toolbar;
        }

        public static GUIStyle ToolbarButton()
        {
            if (_toolbarButton == null)
            {
                _toolbarButton = EditorStyles.toolbarButton.Scale(1.5f);
            }

            return _toolbarButton;
        }
    }
}
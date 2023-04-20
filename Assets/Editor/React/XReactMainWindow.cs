#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Xml.Serialization;
using CFUtilPoolLib;
using System;
using UnityEngine.CFUI;

namespace XEditor {

    public enum ReactTrackType
    {
        Anim = 0,
        FX = 1,
        Audio = 2,
        ShaderEffect = 3,
        BoneShake = 4
    }

    interface IReactTrackDraw
    {
        void OnDrawLeft();

        void OnDrawRight();

        void Add();
    }

    public class BaseTrackDraw : IReactTrackDraw
    {
        protected XReactMainWindow Window;

        public virtual void Init(XReactMainWindow main)
        {
            Window = main;
        }

        public virtual void OnDrawLeft()
        {

        }

        public virtual void OnDrawRight()
        {

        }

        public virtual void Add() { }

        public virtual string Name { get { return ""; } }

        public virtual string GetCount()
        {
            return "";
        }
        public virtual void OnSave() { }
    }
    public class XReactMainWindow : XBaseReactSplitWindow
    {
        public XReactDataSet ReactDataSet { get { return Window.ReactDataSet; } }

        public object CacheNode { get; private set; }
        public  XReactEntranceWindow Window;

        GUIStyle tabStyle;
        GUIStyle addStyle;
        GUIStyle toolStyle;

        public GUIStyle labelStyle;
        public GUIStyle trackStyle;
        public GUIStyle trackDropStyle;

        public GUIStyle LabelStyle;
        public GUIStyle hintLabelStyle;

        public Color trackColor;
        public Color selectdColor;
        public Color trackDropColor;

        int curTrackType;

        public List<BaseTrackDraw> trackDraws = new List<BaseTrackDraw>();

        string[] typeNames;

        public Dictionary<string, string> resMap = new Dictionary<string, string>();

        public bool IsHot { get { return Window.IsHot; } }

        public void Init(XReactEntranceWindow parent)
        {
            base.Init(parent);

            Window = parent;

            trackColor = ReactUtils.GetColor("#615F5F");//#D2D2D2
            selectdColor = ReactUtils.GetColor("#3E7DE7");
            trackDropColor = ReactUtils.GetColor("#D2D2D2");

            AddTrackDraw();

        }


        public void InitGUI()
        {
            if (tabStyle == null)
            {
                tabStyle = new GUIStyle("button");
                tabStyle.fixedHeight = 30;
                tabStyle.fixedWidth = 90;
                tabStyle.normal.textColor = Color.white;
            }

            if (addStyle == null)
            {
                addStyle = new GUIStyle(EditorStyles.toolbarDropDown);
                addStyle.fixedHeight = 25;
                //addStyle.fixedWidth = 50;
                addStyle.alignment = TextAnchor.MiddleLeft;
                addStyle.stretchWidth = true;
            }
            if (toolStyle == null)
            {
                toolStyle = new GUIStyle(GUI.skin.box);
                toolStyle.fixedHeight = 30;
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(EditorStyles.toolbar);
                labelStyle.fixedHeight = 25;
                labelStyle.alignment = TextAnchor.MiddleRight;
                labelStyle.stretchWidth = true;
            }

            if (trackStyle == null)
            {
                trackStyle = new GUIStyle();
                trackStyle.fixedHeight = 40;
                trackStyle.stretchWidth = true;
                trackStyle.normal.textColor = Color.white;
                trackStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (trackDropStyle == null)
            {
                trackDropStyle = new GUIStyle(EditorStyles.popup);
                trackDropStyle.fixedHeight = 30;
                //trackDropStyle.fixedWidth = 40;
                trackDropStyle.stretchWidth = false;
                trackDropStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (hintLabelStyle == null)
            {
                hintLabelStyle = new GUIStyle();
                hintLabelStyle.normal.textColor = Color.red;
                hintLabelStyle.fontStyle = FontStyle.BoldAndItalic;
            }

        }

        void AddTrackDraw()
        {
            trackDraws.Clear();
            trackDraws.Add(new AnimTrackDraw());
            trackDraws.Add(new FXTrackDraw());
            trackDraws.Add(new AudioTrackDraw());
            trackDraws.Add(new ShaderTrackDraw());
            trackDraws.Add(new BoneShakeTrackDraw());

            for (int i = 0; i < trackDraws.Count; ++i)
            {
                trackDraws[i].Init(this);
            }

            typeNames = new string[trackDraws.Count];

        }

        protected override void Repaint()
        {
            Window.Repaint();
        }

        protected override void DrawLeft()
        {
            DrawLeftTool();
        }

        string[] GetTypeNames()
        {
            if (typeNames == null) return null;
            for (int i = 0; i < typeNames.Length; ++i)
            {
                typeNames[i] = trackDraws[i].Name + trackDraws[i].GetCount();
            }
            return typeNames;
        }

        void DrawLeftTool()
        {
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add", addStyle, GUILayout.Width(100)))
                {
                    OnMouseRightClicked(null);
                }
                //GUILayout.FlexibleSpace();
                GUILayout.Space(1);

                if (GUILayout.Button("LoadDummy", GUILayout.Width(100)))
                {
                    var window = EditorWindow.GetWindow<CreateEntityPrefabWindow>(@"CreateEntityPrefab Editor", true);
                    window.position = new Rect(500, 400, 400, 200);
                    window.wantsMouseMove = true;
                    window.Init(Window);
                    window.Show();
                    window.Repaint();
                }

                if (ReactDataSet != null)
                    GUILayout.Label(ReactDataSet.ReactData.Name, labelStyle);
            }

            GUILayout.EndHorizontal();

            ///
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                var names = GetTypeNames();
                curTrackType = GUILayout.Toolbar(curTrackType, names != null ? names : System.Enum.GetNames(typeof(ReactTrackType)), tabStyle);

                GUILayout.Space(10);
            }
            GUILayout.EndHorizontal();

            DrawTracks();
            Window.RefreshHost();
        }

        void DrawTracks()
        {
            GUILayout.Space(30);
            GUILayout.BeginVertical();
            if (curTrackType >= 0 && curTrackType < trackDraws.Count)
            {
                trackDraws[curTrackType].OnDrawLeft();
            }
            GUILayout.EndVertical();
        }

        protected void OnMouseRightClicked(Event e)
        {
            var genericMenu = new GenericMenu();
            //genericMenu.AddItem(new GUIContent("Paste"), false, OnMenuFunc, e);

            //genericMenu.AddItem(new GUIContent("AddAction/a"), false, OnMenuClickFunc, e);
            //genericMenu.AddItem(new GUIContent("AddAction/B"), false, OnMenuClickFunc, e);

            for (int i = 1; i < trackDraws.Count; ++i)
            {
                genericMenu.AddItem(new GUIContent(trackDraws[i].Name), false, OnMenuClickFunc, i);
            }

            genericMenu.ShowAsContext();
        }

        private void OnMenuFunc(object o)
        {

        }

        private void OnMenuClickFunc(object o)
        {
            trackDraws[(int)o].Add();
        }

        protected override void DrawRight()
        {
            DrawTracksRight();
        }

        void DrawTracksRight()
        {
            GUILayout.Space(50);

            ///////////// Time /////////////
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(10);
                GUILayout.Label("React Time");
                GUILayout.FlexibleSpace();
                GUILayout.Label(ReactDataSet.ReactData.Time.ToString("F3") + " (s)");
                GUILayout.Space(50);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                //GUILayout.Label("Ignore Attack Script Fx", ReactCommon.NormalLabelStyle);
                //GUILayout.FlexibleSpace();
                GUILayout.Space(10);
                ReactDataSet.ReactData.IgnoreAttackScriptFx = EditorGUILayout.Toggle("Ignore AttackScript", ReactDataSet.ReactData.IgnoreAttackScriptFx);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Space(20);
            GUILayout.Box("", GUILayout.ExpandWidth(true),
                GUILayout.Height(2),
                GUILayout.ExpandHeight(false));

            GUILayout.Space(20);
            GUILayout.BeginVertical();
            if (curTrackType >= 0 && curTrackType < trackDraws.Count)
            {
                trackDraws[curTrackType].OnDrawRight();
            }
            GUILayout.EndVertical();
        }
        public string GetPath(string name, string ext)
        {
            if (!string.IsNullOrEmpty(name) &&
                resMap.TryGetValue(name + ext, out var path))
            {
                return path;
            }
            return "";
        }
    }
}

#endif
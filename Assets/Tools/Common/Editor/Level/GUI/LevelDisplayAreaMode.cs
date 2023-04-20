using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using CFUtilPoolLib;
using XLevel;

namespace XEditor.Level
{
    class LevelDisplayAreaMode : LevelDisplayBaseMode
    {
        LevelGridBrush m_Brush;
        private int m_SelectBrushAreaID = -1;
        private HashSet<LevelBlock> m_CachedUpdateBlock = new HashSet<LevelBlock>();

        public GUIStyle m_AreaButtonStyle = null;
        public GUIStyleState m_GUIStyleButton1;
        public GUIStyleState m_GUIStyleButton2;
        public LevelAreaSetting m_AreaConfig;

        // cached var
        int texSize = 20;
        Texture2D[] texes = null;

        Texture2D selectAreaTex = null;

        Color[] colorArray = null;

        public LevelDisplayAreaMode(LevelGridTool tool) : base(tool)
        {
            m_AreaConfig = new LevelAreaSetting();
        }

        public override void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(200), GUILayout.Height(texSize + 10) });
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Select an area to brush: ", new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(texSize) });
            if (m_SelectBrushAreaID >= 0)
            {
                GUILayout.Button(selectAreaTex, GUIStyle.none, new GUILayoutOption[] { GUILayout.Width(texSize), GUILayout.Height(texSize) });
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(1);

            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(50) });
            GUILayout.Space(20);

            int areaCount = m_AreaConfig.m_AreaConfig.AreaColor.Length;
            for (int i = 0; i < areaCount; ++i)
            {
                m_AreaButtonStyle.normal = (m_SelectBrushAreaID == i ? m_GUIStyleButton1 : m_GUIStyleButton2);

                if (GUILayout.Button(texes[i], m_AreaButtonStyle, GUILayout.ExpandWidth(false)))
                {
                    OnSelectArea(i);
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnSelectArea(int select)
        {
            if (m_SelectBrushAreaID == select)
            {
                m_SelectBrushAreaID = -1;
                return;
            }
            m_SelectBrushAreaID = select;

            if (selectAreaTex == null)
                selectAreaTex = new Texture2D(texSize, texSize);

            Color32 color = m_AreaConfig.GetAreaColor(m_SelectBrushAreaID);
            for (int j = 0; j < texSize * texSize; ++j)
                colorArray[j] = color;
            selectAreaTex.SetPixels(colorArray);
            selectAreaTex.Apply();
        }

        public override void OnLeaveMode()
        {
            m_Brush.Reset();
            m_SelectBrushAreaID = -1;
        }

        public override void OnEnable()
        {
            m_Brush = new LevelGridBrush();
            m_Brush.SetData(m_tool.m_Data);

            m_GUIStyleButton1 = new GUIStyleState();
            m_GUIStyleButton1.background = EditorGUIUtility.Load("Tools/editor_btn_0.png") as Texture2D;
            m_GUIStyleButton2 = new GUIStyleState();
            m_GUIStyleButton2.background = EditorGUIUtility.Load("Tools/editor_btn_1.png") as Texture2D;

            texes = new Texture2D[m_AreaConfig.m_AreaConfig.AreaColor.Length];
            colorArray = new Color[texSize * texSize];

            for (int i = 0; i < m_AreaConfig.m_AreaConfig.AreaColor.Length; ++i)
            {
                if (texes[i] == null)
                    texes[i] = new Texture2D(texSize, texSize);

                Color32 color = m_AreaConfig.GetAreaColor(i);
                for (int j = 0; j < texSize * texSize; ++j)
                    colorArray[j] = color;

                texes[i].SetPixels(colorArray);
                texes[i].Apply();

                m_AreaButtonStyle = new GUIStyle();
                m_AreaButtonStyle.padding.left = 6;
                m_AreaButtonStyle.padding.top = 6;
                m_AreaButtonStyle.padding.right = 6;
                m_AreaButtonStyle.padding.bottom = 6;
                m_AreaButtonStyle.margin = new RectOffset(5, 5, 5, 5);
                m_AreaButtonStyle.fixedHeight = 30;
                m_AreaButtonStyle.fixedWidth = 30;
            }

            selectAreaTex = new Texture2D(texSize, texSize);
        }

        public override void OnDisable()
        {
            if (m_Brush != null)
            {
                m_Brush.Clear();
                m_Brush = null;
            }
        }

        public override void OnMouseMove(SceneView sceneView)
        {
            if (m_SelectBrushAreaID == -1) return;

            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            object o = HandleUtility.RaySnap(r);

            if (o != null)
            {
                RaycastHit hitInfo = (RaycastHit)o;
                Vector3 MousePoint = hitInfo.point;
                m_Brush.SetViewPos(MousePoint);
            }
        }

        public override bool OnMouseDrag(SceneView sceneView)
        {
            if (m_SelectBrushAreaID == -1) return false;

            Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            object o = HandleUtility.RaySnap(r);

            if (o != null)
            {
                RaycastHit hitInfo = (RaycastHit)o;
                Vector3 MousePoint = hitInfo.point;

                m_CachedUpdateBlock.Clear();

                m_Brush.OnBrush(MousePoint, BrushEvent);

                HashSet<LevelBlock>.Enumerator e = m_CachedUpdateBlock.GetEnumerator();
                while (e.MoveNext())
                {
                    m_tool.m_Drawer.ReDrawBlockColors(e.Current);
                }
            }

            return true;
        }
        private void BrushEvent(QuadTreeElement grid)
        {

        }

    }
}

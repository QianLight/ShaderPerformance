using System.Collections.Generic;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;
using CFEngine;
using CFEngine.Editor;

namespace XEditor
{

    public class PrefabPreviewWindow : EditorWindow
    {
        public enum PreviewType
        {
            Player,
            Boss,
            NpcAndEnemy,
            Setting,
        }

        private GUIContent[] toolnames;
        private PreviewType currType = PreviewType.Player;
        private ToolTemplate tool;
        private List<ToolTemplate> tools = new List<ToolTemplate>();

        #region Gui Style
        public static GUIStyle TabStyle;
        static void InitStyle()
        {
            TabStyle = new GUIStyle("button");
            TabStyle.fixedHeight = 40;
            TabStyle.fixedWidth = 120;
        }
        #endregion

        [MenuItem("Tools/Asset/PrefabPreview")]
        static void AnimExportTool()
        {
            InitStyle();
            if (XEditorUtil.MakeNewScene())
            {
                EditorWindow.GetWindowWithRect(typeof(PrefabPreviewWindow), new Rect(0, 0, 1200, 800), true, "Prefab预览修改工具");
            }
        }

        private void OnEnable()
        {
            toolnames = new GUIContent[]
            {
                    new GUIContent("主角"),
                    new GUIContent("BOSS"),
                    new GUIContent("NPC  小怪"),
                    new GUIContent("设置"),
            };
            //tools.Add(new PlayerPreview(this));
            //tools.Add(new BossPartPreview(this));
            tools.Add(null);
            tools.Add(null);
            SetTool(0);
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            int toolbarIndex = (int)currType;
            toolbarIndex = GUILayout.Toolbar(toolbarIndex, toolnames, TabStyle);
            if (EditorGUI.EndChangeCheck())
            {
                PreviewType newTab = (PreviewType)(toolbarIndex);
                SetTool(newTab);
            }
            if (tool != null)
            {
                tool.OnGUI(this.position);
            }
        }

        void SetTool(PreviewType type)
        {
            if (currType == type && tool != null)
                return;

            currType = type;

            if (tool != null)
                tool.OnDisable();

            tool = tools[(int)currType];

            if (tool != null)
                tool.OnEnable();

            Repaint();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (tool != null)
            {
                tool.OnSceneGUI(sceneView);
            }
        }

        void Update()
        {
            if (tool != null)
            {
                tool.Update();
            }
        }
        public void DrawGizmos()
        {
            if (tool != null)
            {
                tool.DrawGizmos();
            }
        }

        void OnDisable()
        {

        }

        void OnDestroy()
        {
            for (int i = 0; i < tools.Count; ++i)
            {
                if (tools[i] != null)
                {
                    tools[i].OnDestroy();
                }
            }
        }
    }
}
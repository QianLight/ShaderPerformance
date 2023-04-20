using System;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CFEngine.Editor
{
    public class MatEffectWindow : EditorWindow
    {
        MatEffectGraph m_Graph;
        MatEffectView m_Canvas;

        /// <summary>
        /// Load a graph asset in this window for editing
        /// </summary>
        public void Load (MatEffectGraph graph)
        {
            m_Graph = graph;
            m_Graph.Init ();
            m_Canvas = new MatEffectView (this, "MatEffect");
            m_Canvas.Load (graph);

            rootVisualElement.Add (m_Canvas);

            // Box box = new Box ();
            // rootVisualElement.Add (box);
            titleContent = new GUIContent (graph.name);
            Repaint ();
        }

        private void Update ()
        {
            if (m_Canvas != null)
                m_Canvas.Update ();
        }

        // public virtual void OnGUI ()
        // {
        //     GUI.Box (new Rect (5, 5, 100, 100), "test");
        // }

        /// <summary>
        /// Restore an already opened graph after a reload of assemblies
        /// </summary>
        private void OnEnable ()
        {
            if (m_Graph)
            {
                Load (m_Graph);
            }
        }
        private static MatEffectWindow window;
        private static MatEffectGraph matEffectGraph;

        [MenuItem ("XEditor/材质特效编辑器 MatEffectEditor")]
        public static void Open ()
        {
            if (window == null)
                window = CreateInstance<MatEffectWindow> ();
            window.Show ();
            if (matEffectGraph == null)
                matEffectGraph = AssetDatabase.LoadAssetAtPath<MatEffectGraph> (AssetsConfig.instance.MatEffectPath);
            if (matEffectGraph == null)
            {
                matEffectGraph = MatEffectGraph.CreateInstance<MatEffectGraph> ();
                matEffectGraph.name = "MatEffectConfig";
                matEffectGraph = EditorCommon.CreateAsset<MatEffectGraph> (AssetsConfig.instance.MatEffectPath, ".asset", matEffectGraph);
            }
            window.Load (matEffectGraph);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Zeus.Core.Util
{
    public class BundleConfigNodeWindow : EditorWindow
    {
        [MenuItem("Window/BundleConfigWindow")]
        public static void OpenWindow()
        {
            var window = GetWindow<BundleConfigNodeWindow>();
            var json = File.ReadAllText("d:/flow.json");
            window.m_graph = JsonUtility.FromJson<ConfigNodeGraph>(json);
            window.m_graph.Init(true);
        }

        private ConfigNodeGraph m_graph;

        public void OnGUI()
        {
            if(null == m_graph)
            {
                return;
            }

            m_graph.Draw(this);
        }

        private void DrawMenu()
        {

        }
    }
}


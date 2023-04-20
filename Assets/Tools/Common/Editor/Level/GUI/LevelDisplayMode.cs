using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using CFUtilPoolLib;
using CFEngine.Editor;
using CFEngine;
namespace XEditor.Level
{
    class LevelDisplayBaseMode
    {
        public LevelGridTool m_tool;
        public static int layer_mask = (1 << LayerMask.NameToLayer("Terrain") | 1 << LayerMask.NameToLayer("BigGuy") | 1 << LayerMask.NameToLayer("Default"));

        public LevelDisplayBaseMode(LevelGridTool tool)
        {
            m_tool = tool;            
        }

        public virtual void OnEnterMode()
        {
            m_tool.m_Drawer.SmartDrawGrid();
        }
        public virtual void OnGUI() {}

        public virtual void OnLeaveMode() {}

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnUpdate() { }

        public virtual bool OnMouseClick(SceneView sceneView) { return false; }

        public virtual bool OnMouseDoubleClick(SceneView sceneView) { return false; }

        public virtual void OnMouseMove(SceneView sceneView) { }

        public virtual bool OnMouseDrag(SceneView sceneView) { return false; }

        public virtual bool OnKeyDown(SceneView sceneView) { return false; }

        public virtual void OnSceneGUI() { }

        public virtual bool IsCurrentGridSelect(QuadTreeElement grid) { return false; }

        public virtual bool IsCurrentGridDebug(QuadTreeElement grid) { return false; }


    }

    


    
}

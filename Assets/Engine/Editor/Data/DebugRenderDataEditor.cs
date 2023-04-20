using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CanEditMultipleObjects, CustomEditor (typeof (DebugRenderData))]
    public sealed class DebugRenderDataEditor : UnityEngineEditor
    {
        // MaterialProperty[] props = null;
        // PBSShaderGUI gui = null;
        // MaterialEditor materialEditor;
        Renderer r;
        private void OnEnable ()
        {
            var drd = target as DebugRenderData;
            drd.TryGetComponent (out r);
        }

        public override void OnInspectorGUI ()
        {
            var drd = target as DebugRenderData;
            EditorGUILayout.Toggle("IsMeshLoaded", drd.isMeshLoaded);
            if (drd.md != null)
            {
                Material mat = drd.md.GetMat (true);
                EditorGUILayout.ObjectField ("", mat, typeof (Material), false);
                if (GUILayout.Button("Save"))
                {
                    drd.Save(r);
                }
                // if (gui != null)
                // {
                //     gui.OnGUI (materialEditor, props);
                // }

            }
        }
    }
}
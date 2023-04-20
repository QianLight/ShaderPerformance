using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    /// <summary>
    /// A base class for creating editors that decorate Unity's built-in editor types.
    /// </summary>
    public class CustomAssetInspector
    {
        private static Assembly editorAssembly = Assembly.GetAssembly (typeof (UnityEditor.Editor));
        private Type inspectorType;
        private ReflectFun AwakeFun;
        private ReflectFun OnEnableFun;

        private ReflectFun OnInspectorGUIFun;

        private ReflectFun HasModifiedFun;

        private ReflectFun ResetValuesFun;
        private ReflectFun ApplyFun;
        private UnityEditor.Editor editorInstance;
        public CustomAssetInspector (string inspectorName)
        {
            inspectorType = editorAssembly.GetTypes ().Where (t => t.Name == inspectorName).FirstOrDefault ();
            if (inspectorType != null)
            {
                AwakeFun = EditorCommon.GetInternalFunction (inspectorType, "Awake", false, false, true, true, true);
                OnEnableFun = EditorCommon.GetInternalFunction (inspectorType, "OnEnable", false, false, true, false);
                OnInspectorGUIFun = EditorCommon.GetInternalFunction (inspectorType, "OnInspectorGUI", false, false, true, false);
                HasModifiedFun = EditorCommon.GetInternalFunction (inspectorType, "HasModified", false, false, true, false);
                ResetValuesFun = EditorCommon.GetInternalFunction (inspectorType, "ResetValues", false, false, true, false, true);
                ApplyFun = EditorCommon.GetInternalFunction (inspectorType, "Apply", false, false, true, false, true);
            }

        }
        FieldInfo assetEditorField = null;
        public void Init (UnityEngine.Object[] targetObjects)
        {
            if (inspectorType != null)
            {
                editorInstance = UnityEditor.Editor.CreateEditor (targetObjects, inspectorType);
                assetEditorField = inspectorType.BaseType.GetField ("m_AssetEditor", BindingFlags.NonPublic |
                    BindingFlags.Instance);
                if (assetEditorField != null)
                {
                    assetEditorField.SetValue (editorInstance, editorInstance);
                }

            }

            else
                editorInstance = null;
        }

        public void Awake ()
        {
            if (AwakeFun != null && editorInstance != null)
            {

                AwakeFun.Call (editorInstance, null);
            }
        }
        public void OnEnable ()
        {
            if (OnEnableFun != null && editorInstance != null)
            {
                OnEnableFun.Call (editorInstance, null);

            }
        }
        public void OnInspectorGUI ()
        {
            if (OnInspectorGUIFun != null && editorInstance != null)
            {
                OnInspectorGUIFun.Call (editorInstance, null);
            }
        }

        public bool HasModified ()
        {
            if (HasModifiedFun != null && editorInstance != null)
            {
                return (bool) HasModifiedFun.Call (editorInstance, null);
            }
            return false;
        }
        public void ResetValues ()
        {
            if (ResetValuesFun != null && editorInstance != null)
            {
                ResetValuesFun.Call (editorInstance, null);
            }
        }
        public void Apply ()
        {
            if (ApplyFun != null && editorInstance != null)
            {
                ApplyFun.Call (editorInstance, null);
            }
        }
    }
}
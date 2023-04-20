#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Engine.Editor.ShaderGUI
{
    public class DecalShaderGUI : UnityEditor.ShaderGUI
    {
        private MaterialEditor _editor;
        private Object[] _materials;
        private MaterialProperty[] _properties;

        private bool _isDebug;
        private bool _isMixedValue;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);

            _editor = materialEditor;
            _materials = materialEditor.targets;
            _properties = properties;

            InitDebugMode();
            DrawDebugMode();
        }
        
        private void InitDebugMode()
        {
            MaterialProperty materialProperty = FindProperty("_DecalDebug", _properties, false);
            if (materialProperty == null)
            {
                _isDebug = false;
                return;
            }

            _isMixedValue = materialProperty.hasMixedValue;
            _isDebug = materialProperty.floatValue > 0;
        }

        private void DrawDebugMode()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _isMixedValue;
            _isDebug = EditorGUILayout.Toggle("DebugDecal", _isDebug);
            if (EditorGUI.EndChangeCheck())
            {
                SetDebugMode(_isDebug);
                InitDebugMode();
            }
            
            EditorGUI.showMixedValue = false;
        }

        private void SetDebugMode(bool isDebug)
        {
            MaterialProperty materialProperty = FindProperty("_CullMode", _properties, false);
            if (materialProperty != null)
            {
                materialProperty.floatValue = isDebug ? (float) CullMode.Back : (float) CullMode.Front;
            }

            materialProperty = FindProperty("_DecalDebug", _properties, false);
            if (materialProperty != null)
            {
                materialProperty.floatValue = isDebug ? 1 : 0;
            }
                
            foreach (Material material in _materials)
            {
                if (isDebug)
                {
                    material.EnableKeyword("_DECAL_DEBUG");
                }
                else
                {
                    material.DisableKeyword("_DECAL_DEBUG");
                }
            }
        }
    }
}
#endif
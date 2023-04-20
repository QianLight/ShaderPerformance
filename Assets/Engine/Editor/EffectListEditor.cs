using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    public sealed class EffectListEditor
    {
        UnityEngineEditor m_BaseEditor;

        EnvProfile m_Asset;
        SerializedObject m_SerializedObject;
        SerializedProperty m_SettingsProperty;

        Dictionary<Type, Type> m_EditorTypes; // SettingsType => EditorType
        List<EnvEffectBaseEditor> m_Editors;

        private static Dictionary<Type, EnvAttribute> settingsTypes = new Dictionary<Type, EnvAttribute> ();
        private static readonly SavedBool rolePropertiesFolder = new SavedBool($"{nameof(EffectListEditor)}.{nameof(rolePropertiesFolder)}", false);
        private static bool init = false;
        // Called every time Unity recompile scripts in the editor. We need this to keep track of
        // any new custom effect the user might add to the project
        //[UnityEditor.Callbacks.DidReloadScripts]
        static void OnEditorReload ()
        {
            settingsTypes.Clear ();
            var editorTypes = EngineUtility.GetAllAssemblyTypes ()
                .Where (
                    t => t.IsSubclassOf (typeof (EnvSetting)) &&
                    t.IsDefined (typeof (EnvAttribute), false) &&
                    !t.IsAbstract
                );
            var it = editorTypes.GetEnumerator ();
            while (it.MoveNext ())
            {
                var type = it.Current;
                settingsTypes.Add (type, type.GetAttribute<EnvAttribute> ());
            }
        }

        public EffectListEditor (UnityEngineEditor editor)
        {
            Assert.IsNotNull (editor);
            m_BaseEditor = editor;
            if (!init)
            {
                OnEditorReload ();
                init = true;
            }
        }

        public void Init (EnvProfile asset, SerializedObject serializedObject)
        {
            Assert.IsNotNull (asset);
            Assert.IsNotNull (serializedObject);

            m_Asset = asset;
            m_Asset.isDirty = true;
            m_SerializedObject = serializedObject;
            m_SettingsProperty = serializedObject.FindProperty ("settings");
            Assert.IsNotNull (m_SettingsProperty);

            m_EditorTypes = new Dictionary<Type, Type> ();
            m_Editors = new List<EnvEffectBaseEditor> ();

            // Gets the list of all available postfx editors
            var editorTypes = EngineUtility.GetAllAssemblyTypes ()
                .Where (
                    t => t.IsSubclassOf (typeof (EnvEffectBaseEditor)) &&
                    t.IsDefined (typeof (EnvEditorAttribute), false) &&
                    !t.IsAbstract
                );

            // Map them to their corresponding settings type
            foreach (var editorType in editorTypes)
            {
                var attribute = editorType.GetAttribute<EnvEditorAttribute> ();
                m_EditorTypes.Add (attribute.settingsType, editorType);
            }

            // Create editors for existing settings
            for (int i = 0; i < m_Asset.settings.Count; i++)
                CreateEditor (m_Asset.settings[i], m_SettingsProperty.GetArrayElementAtIndex (i));

            // Keep track of undo/redo to redraw the inspector when that happens
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        void OnUndoRedoPerformed ()
        {
            m_Asset.isDirty = true;

            // Dumb hack to make sure the serialized object is up to date on undo (else there'll be
            // a state mismatch when this class is used in a GameObject inspector).
            m_SerializedObject.Update ();
            m_SerializedObject.ApplyModifiedProperties ();

            // Seems like there's an issue with the inspector not repainting after some undo events
            // This will take care of that
            m_BaseEditor.Repaint ();
        }

        void CreateEditor (EnvSetting settings, SerializedProperty property, int index = -1)
        {
            if (settings != null)
            {
                var settingsType = settings.GetType ();
                Type editorType;

                if (!m_EditorTypes.TryGetValue (settingsType, out editorType))
                    editorType = typeof (DefaultEnvEffectEditor);

                var editor = (EnvEffectBaseEditor) Activator.CreateInstance (editorType);
                editor.Init (settings, m_BaseEditor);
                editor.baseProperty = property.Copy ();

                if (index < 0)
                    m_Editors.Add (editor);
                else
                    m_Editors[index] = editor;
            }
        }

        // Clears & recreate all editors - mainly used when the volume has been modified outside of
        // the editor (user scripts, inspector reset etc).
        void RefreshEditors ()
        {
            // Disable all editors first
            foreach (var editor in m_Editors)
                editor.OnDisable ();

            // Remove them
            m_Editors.Clear ();

            // Recreate editors for existing settings, if any
            for (int i = 0; i < m_Asset.settings.Count; i++)
                CreateEditor (m_Asset.settings[i], m_SettingsProperty.GetArrayElementAtIndex (i));
        }

        public void Clear ()
        {
            if (m_Editors == null)
                return; // Hasn't been inited yet

            foreach (var editor in m_Editors)
                editor.OnDisable ();

            m_Editors.Clear ();
            m_EditorTypes.Clear ();

            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }
        
        public enum Operation
        {
            None,
            Active,
            Save,
            Debug,
            Dump,
        }

        public Operation OnHeadGUI(Action<Operation,string> onwillSave = null)
        {
            Operation operation = Operation.None;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Active", GUILayout.MaxWidth(100)))
            {
                EnvProfile.activeProfile = m_Asset;
                onwillSave?.Invoke(Operation.Active, "");
                RenderingManager.instance.SetCurrentEnvEffect (m_Asset, true);
                SceneAssets.SceneModify ();
                operation = Operation.Active;
            }
            if (GUILayout.Button ("Save", GUILayout.MaxWidth (100)))
            {
               
                if (EngineContext.IsRunning)
                {
                    var context = EngineContext.instance;
                    if (!string.IsNullOrEmpty(context.envPath))
                    {
                        //onwillSave?.Invoke(Operation.Save, context.envPath);
                        EnvProfile profile = context.envProfile as EnvProfile;
                        if (profile == null)
                        {
                            profile = AssetDatabase.LoadAssetAtPath<EnvProfile>(context.envPath);
                            context.envProfile = profile;
                            if (profile != null)
                            {
                                profile.Refresh();
                            }
                        }
                        EnvProfile.Save(profile, RenderLayer.envProfile);
                    }
                }
                else
                {
                    onwillSave?.Invoke(Operation.Save,"");
                    CommonAssets.SaveAsset(m_Asset);
                }
                
                operation = Operation.Save;
            }
            if (GUILayout.Button (AttributeDecorator.DebugCurrentParamValue? "Debuging": "Debug", GUILayout.MaxWidth (100)))
            {
                onwillSave?.Invoke(Operation.Debug, "");
                AttributeDecorator.DebugCurrentParamValue = !AttributeDecorator.DebugCurrentParamValue;
                operation = Operation.Debug;
            }
            if (GUILayout.Button ("Dump", GUILayout.MaxWidth (100)))
            {
                onwillSave?.Invoke(Operation.Dump, "");
                RenderingManager.instance.Dump ();
                operation = Operation.Dump;
            }
            EditorGUILayout.EndHorizontal ();
            return operation;
        }

        public void OnGUI ()
        {
            if (m_Asset == null)
                return;

            if (m_Asset.isDirty)
            {
                RefreshEditors ();
                m_Asset.isDirty = false;
            }

            bool isEditable = !UnityEditor.VersionControl.Provider.isActive ||
                AssetDatabase.IsOpenForEdit (m_Asset, StatusQueryOptions.UseCachedIfPossible);

            using (new EditorGUI.DisabledScope (!isEditable))
            {
                EditorGUILayout.LabelField (EditorUtilities.GetContent ("Overrides"), EditorStyles.boldLabel);
                rolePropertiesFolder.Value = EditorGUILayout.Foldout(rolePropertiesFolder.Value, "角色光照参数集合");
                if (rolePropertiesFolder.Value)
                {
                    DrawProperty<LightingEditor, Lighting, ColorParam>(x => x.roleLightColorV2, x => x.roleLightColorV2);
                    DrawProperty<AmbientEditor, Ambient, SHParam>(x => x.roleSHV2, x => x.roleSHV2);
                    DrawProperty<ShadowEditor, Shadow, Vector4Param>(x => x.shadowMisc, x => x.shadowMisc);
                    DrawProperty<ShadowEditor, Shadow, ColorParam>(x => x.roleShadowColor, x => x.roleShadowColor);
                }

                // Override list
                for (int i = 0; i < m_Editors.Count; i++)
                {
                    var editor = m_Editors[i];
                    string title = editor.GetDisplayTitle ();
                    int id = i; // Needed for closure capture below

                    EditorUtilities.DrawSplitter ();
                    bool displayContent = EditorUtilities.DrawHeader (
                        title,
                        editor.baseProperty,
                        editor.target,
                        () => ResetEffectOverride (editor.target.GetType (), id),
                        () => MoveUpOverride (id),
                        () => MoveDownOverride (id),
                        () => RemoveEffectOverride (id)
                    );

                    if (displayContent)
                    {
                        using (new EditorGUI.DisabledScope (!editor.target.active))
                        editor.OnInternalInspectorGUI ();
                    }
                }

                if (m_Editors.Count > 0)
                {
                    EditorUtilities.DrawSplitter ();
                    EditorGUILayout.Space ();
                }
                else
                {
                    EditorGUILayout.HelpBox ("No override set on this profile.", MessageType.Info);
                }

                if (GUILayout.Button ("Add effect...", EditorStyles.miniButton))
                {
                    var menu = new GenericMenu ();
                    foreach (var kvp in settingsTypes)
                    {
                        var type = kvp.Key;
                        var value = kvp.Value;
                        if (value.isVirtualEffect)
                            continue;
                        var title = EditorUtilities.GetContent (value.menuItem);
                        bool exists = m_Asset.HasSettings (type);

                        if (!exists)
                            menu.AddItem (title, false, () => AddEffectOverride (type));
                        else
                            menu.AddDisabledItem (title);
                    }

                    menu.ShowAsContext ();
                }
                if (GUILayout.Button ("Add All", EditorStyles.miniButton))
                {
                    foreach (var kvp in settingsTypes)
                    {
                        var type = kvp.Key;
                        var value = kvp.Value;
                        if (value.isVirtualEffect)
                            continue;
                        bool exists = m_Asset.HasSettings (type);

                        if (!exists)
                        {
                            AddEffectOverride (type);
                        }
                    }
                }

                EditorGUILayout.Space ();
            }
        }

        void DrawProperty<TEditor, TEffect, TValue>(Expression<Func<TEffect, TValue>> expr, Func<TEffect, TValue> param)
            where TEditor : EnvEffectEditor<TEffect>
            where TEffect : EnvSetting
            where TValue : ParamOverride
        {
            foreach (EnvEffectBaseEditor editor in m_Editors)
            {
                if (editor.GetType() == typeof(TEditor))
                {
                    editor.serializedObject.Update();
                    TEditor targetEditor = editor as TEditor;
                    TEffect effect = targetEditor.target as TEffect;
                    var property = targetEditor.FindClassParameterOverride(expr, param(effect));
                    if (property != null)
                    {
                        EnvEffectBaseEditor.PropertyField(property);
                    }
                    editor.serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
        }

        void AddEffectOverride (Type type)
        {
            m_SerializedObject.Update ();

            var effect = CreateNewEffect (type);
            Undo.RegisterCreatedObjectUndo (effect, "Add Effect Override");

            // Store this new effect as a subasset so we can reference it safely afterwards
            AssetDatabase.AddObjectToAsset (effect, m_Asset);

            // Grow the list first, then add - that's how serialized lists work in Unity
            m_SettingsProperty.arraySize++;
            var effectProp = m_SettingsProperty.GetArrayElementAtIndex (m_SettingsProperty.arraySize - 1);
            effectProp.objectReferenceValue = effect;

            // Force save / refresh
            // EditorUtility.SetDirty(m_Asset);
            // AssetDatabase.SaveAssets();

            // Create & store the internal editor object for this effect
            CreateEditor (effect, effectProp);

            m_SerializedObject.ApplyModifiedProperties ();
        }
        void MoveUpOverride (int id)
        {
            if (id > 0)
            {
                var setting = m_Asset.settings[id - 1];
                m_Asset.settings[id - 1] = m_Asset.settings[id];
                m_Asset.settings[id] = setting;

                var editor = m_Editors[id - 1];
                m_Editors[id - 1] = m_Editors[id];
                m_Editors[id] = editor;
            }
        }
        void MoveDownOverride (int id)
        {
            if (id < m_Asset.settings.Count - 1)
            {
                var setting = m_Asset.settings[id];
                m_Asset.settings[id] = m_Asset.settings[id + 1];
                m_Asset.settings[id + 1] = setting;

                var editor = m_Editors[id];
                m_Editors[id] = m_Editors[id + 1];
                m_Editors[id + 1] = editor;
            }
        }

        void RemoveEffectOverride (int id)
        {
            // Huh. Hack to keep foldout state on the next element...
            bool nextFoldoutState = false;
            if (id < m_Editors.Count - 1)
                nextFoldoutState = m_Editors[id + 1].baseProperty.isExpanded;

            // Remove from the cached editors list
            m_Editors[id].OnDisable ();
            m_Editors.RemoveAt (id);

            m_SerializedObject.Update ();

            var property = m_SettingsProperty.GetArrayElementAtIndex (id);
            var effect = property.objectReferenceValue;

            // Unassign it (should be null already but serialization does funky things
            property.objectReferenceValue = null;

            // ...and remove the array index itself from the list
            m_SettingsProperty.DeleteArrayElementAtIndex (id);

            // Finally refresh editor reference to the serialized settings list
            for (int i = 0; i < m_Editors.Count; i++)
                m_Editors[i].baseProperty = m_SettingsProperty.GetArrayElementAtIndex (i).Copy ();

            if (id < m_Editors.Count)
                m_Editors[id].baseProperty.isExpanded = nextFoldoutState;

            m_SerializedObject.ApplyModifiedProperties ();

            // Destroy the setting object after ApplyModifiedProperties(). If we do it before, redo
            // actions will be in the wrong order and the reference to the setting object in the
            // list will be lost.
            Undo.DestroyObjectImmediate (effect);

            // Force save / refresh
            EditorUtility.SetDirty (m_Asset);
            AssetDatabase.SaveAssets ();
        }

        // Reset is done by deleting and removing the object from the list and adding a new one in
        // the place as it was before
        void ResetEffectOverride (Type type, int id)
        {
            // Remove from the cached editors list
            m_Editors[id].OnDisable ();
            m_Editors[id] = null;

            m_SerializedObject.Update ();

            var property = m_SettingsProperty.GetArrayElementAtIndex (id);
            var prevSettings = property.objectReferenceValue;

            // Unassign it but down remove it from the array to keep the index available
            property.objectReferenceValue = null;

            // Create a new object
            var newEffect = CreateNewEffect (type);
            Undo.RegisterCreatedObjectUndo (newEffect, "Reset Effect Override");

            // Store this new effect as a subasset so we can reference it safely afterwards
            AssetDatabase.AddObjectToAsset (newEffect, m_Asset);

            // Put it in the reserved space
            property.objectReferenceValue = newEffect;

            // Create & store the internal editor object for this effect
            CreateEditor (newEffect, property, id);

            m_SerializedObject.ApplyModifiedProperties ();

            // Same as RemoveEffectOverride, destroy at the end so it's recreated first on Undo to
            // make sure the GUID exists before undoing the list state
            Undo.DestroyObjectImmediate (prevSettings);

            // Force save / refresh
            EditorUtility.SetDirty (m_Asset);
            AssetDatabase.SaveAssets ();
        }

        EnvSetting CreateNewEffect (Type type)
        {
            var effect = (EnvSetting) ScriptableObject.CreateInstance (type);
            effect.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            effect.name = type.Name;
            return effect;
        }

        public void OnSceneGUI ()
        {
            if (m_Editors == null)
                return;
            for (int i = 0; i < m_Editors.Count; i++)
            {
                var editor = m_Editors[i];
                editor.OnInternalSceneGUI ();
            }
        }
    }
}
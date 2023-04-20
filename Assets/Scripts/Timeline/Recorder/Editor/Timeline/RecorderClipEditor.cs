using System;
using System.Globalization;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Timeline;

[CustomEditor(typeof(RecorderClip), true)]
class RecorderClipEditor : Editor
{
    RecorderEditor m_Editor;
    TimelineAsset m_Timeline;
    RecorderSelector m_RecorderSelector;

    public void OnEnable()
    {
        m_RecorderSelector = null;
    }
    
    public override void OnInspectorGUI()
    {
        try
        {
            if (target == null) return;

            if (m_Editor != null && m_Editor.target == null)
            {
                UnityHelpers.Destroy(m_Editor);
                m_Editor = null;
                m_RecorderSelector = null;
            }

            if (m_RecorderSelector == null)
            {
                m_RecorderSelector = new RecorderSelector();
                m_RecorderSelector.OnSelectionChanged += OnRecorderSelected;
                m_RecorderSelector.Init(((RecorderClip)target).settings);
            }

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                var clip = (RecorderClip)target;
                if (m_Timeline == null)
                    m_Timeline = clip.FindTimelineAsset();

                if (m_Timeline != null)
                {
                    EditorGUILayout.LabelField("Frame Rate");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Playback", FrameRatePlayback.Constant.ToString());
                    EditorGUILayout.LabelField("Target (Timeline FPS)", m_Timeline.editorSettings.fps.ToString(CultureInfo.InvariantCulture));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Separator();
                }

                EditorGUILayout.BeginHorizontal();

                if (clip.needsDuplication)
                {
                    if (clip.settings != null)
                    {
                        clip.settings = Instantiate(clip.settings);
                        AssetDatabase.AddObjectToAsset(clip.settings, clip);
                    }
                    clip.needsDuplication = false;
                }

                m_RecorderSelector.OnGui();
                EditorGUILayout.EndHorizontal();
                if (clip.settings)
                {
                    EditorGUILayout.LabelField("Save Directory");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(clip.settings.BuildAbsolutePath());
                    EditorGUI.indentLevel--;
                }
                if (m_Editor != null)
                {
                    EditorGUILayout.Separator();
                    var prevValue = RecorderEditor.FromRecorderWindow;
                    RecorderEditor.FromRecorderWindow = false;
                    m_Editor.OnInspectorGUI();
                    RecorderEditor.FromRecorderWindow = prevValue;
                    serializedObject.Update();
                }
            }
        }
        catch (ExitGUIException)
        {
        }
        catch (Exception ex)
        {
            EditorGUILayout.HelpBox("An exception was raised while editing the settings. This can be indicative of corrupted settings.", MessageType.Warning);

            if (GUILayout.Button("Reset settings to default"))
                ResetSettings();

            Debug.LogException(ex);
        }
    }

    void ResetSettings()
    {
        UnityHelpers.Destroy(m_Editor);
        m_Editor = null;
        m_RecorderSelector = null;
        UnityHelpers.Destroy(((RecorderClip)target).settings, true);
    }

    void OnRecorderSelected(Type selectedRecorder)
    {
        var clip = (RecorderClip)target;

        if (m_Editor != null)
        {
            UnityHelpers.Destroy(m_Editor);
            m_Editor = null;
        }

        if (selectedRecorder == null) return;

        if (clip.settings != null && RecordersInventory.GetRecorderInfo(selectedRecorder).settingsType != clip.settings.GetType())
        {
            UnityHelpers.Destroy(clip.settings, true);
            clip.settings = null;
        }

        if (clip.settings == null)
        {
            clip.settings = RecordersInventory.CreateDefaultRecorderSettings(selectedRecorder);
            AssetDatabase.CreateAsset(clip.settings, OriginalSetting.recdPat);
        }

        m_Editor = (RecorderEditor)CreateEditor(clip.settings);
        AssetDatabase.Refresh();
    }
}

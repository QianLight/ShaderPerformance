using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace CFEngine.Editor
{
    [CustomEditor(typeof(LightRender))]
    public class LightRenderEditor : UnityEditor.Editor
    {
        SerializedProperty priority;
        SerializedProperty index;
        SerializedProperty delay;
        SerializedProperty fadeInLength;
        SerializedProperty loopLength;
        SerializedProperty loopTimes;
        SerializedProperty fadeOutLength;
        SerializedProperty softness;
        SerializedProperty coverage;
        SerializedProperty fadeIn;
        SerializedProperty loop;
        SerializedProperty fadeOut;

        private void OnEnable()
        {
            priority = serializedObject.FindProperty(nameof(LightRender.priority));
            index = serializedObject.FindProperty(nameof(LightRender.index));
            delay = serializedObject.FindProperty(nameof(LightRender.delay));
            fadeInLength = serializedObject.FindProperty(nameof(LightRender.fadeInLength));
            loopLength = serializedObject.FindProperty(nameof(LightRender.loopLength));
            loopTimes = serializedObject.FindProperty(nameof(LightRender.loopTimes));
            fadeOutLength = serializedObject.FindProperty(nameof(LightRender.fadeOutLength));
            softness = serializedObject.FindProperty(nameof(LightRender.softness));
            coverage = serializedObject.FindProperty(nameof(LightRender.coverage));
            fadeIn = serializedObject.FindProperty(nameof(LightRender.fadeIn));
            loop = serializedObject.FindProperty(nameof(LightRender.loop));
            fadeOut = serializedObject.FindProperty(nameof(LightRender.fadeOut));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(priority);
            EditorGUILayout.PropertyField(index);
            EditorGUILayout.PropertyField(delay);
            EditorGUILayout.PropertyField(fadeInLength);
            EditorGUILayout.PropertyField(loopLength);
            EditorGUILayout.PropertyField(loopTimes);
            EditorGUILayout.PropertyField(fadeOutLength);
            EditorGUILayout.PropertyField(softness);
            EditorGUILayout.PropertyField(coverage);

            GUILayout.Space(10);

            GUILayout.Box("以下曲线会自动处理成线性以保证预览一致", GUILayout.ExpandWidth(true));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(fadeIn);
            EditorGUILayout.PropertyField(loop);
            EditorGUILayout.PropertyField(fadeOut);
            bool packDirty = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            LightRender lightRender = target as LightRender;

            if (packDirty)
            {
                ProcessCurve(lightRender.fadeIn.intensity);
                ProcessCurve(lightRender.fadeIn.range);
                ProcessCurve(lightRender.loop.intensity);
                ProcessCurve(lightRender.loop.range);
                ProcessCurve(lightRender.fadeOut.intensity);
                ProcessCurve(lightRender.fadeOut.range);
                EditorUtility.SetDirty(lightRender);
            }

            using (new GUILayout.VerticalScope("box"))
            {
                float length = lightRender.delay + lightRender.fadeInLength + lightRender.loopLength * lightRender.loopTimes + lightRender.fadeOutLength;
               // lightRender.previewProgress.Value = EditorGUILayout.Slider($"预览", lightRender.previewProgress.Value, 0, length);
            }

            if (GUILayout.Button("重新保存所有包含该组件的Prefab"))
            {
                if (EditorUtility.DisplayDialog("重新保存所有包含该组件的Prefab", "确定要重新保存所有包含该组件的Prefab吗？\n至少需要几十秒", "确定"))
                {
                    ResaveEffects();
                }
            }
        }

        private static void ProcessCurve(CFloat cfloat)
        {
            if (cfloat.useCurve)
            {
                var curve = cfloat.curve;
                for (int i = 0; i < curve.length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
            }
        }

        public static void ResaveEffects()
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/BundleRes/Effects/Prefabs" });
            List<LightRender> temp = new List<LightRender>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!prefab)
                {
                    Debug.LogError(path);
                    continue;
                }
                SFXWrapper wrapper = prefab.GetComponent<SFXWrapper>();
                if (wrapper)
                {
                    SFXWrapperEditor editor = CreateEditor(wrapper) as SFXWrapperEditor;
                    if (editor)
                    {
                        prefab.GetComponentsInChildren(temp);
                        if (temp.Count > 0)
                        {
                            wrapper.Refresh();
                            editor.Save(wrapper);
                            Debug.Log($"Resave LightRender : {path}");
                        }
                    }
                    temp.Clear();
                }
            }
        }
    }
}

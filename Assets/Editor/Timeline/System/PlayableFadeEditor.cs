using UnityEditor;
using UnityEngine;

namespace XEditor
{
    [CustomEditor(typeof(UIFadeAsset))]
    public class PlayableFadeEditor : Editor
    {
        UIFadeAsset asset;

        SerializedObject so;
        SerializedProperty m_key;
        SerializedProperty m_content;
        SerializedProperty m_bgTexturePath;

        private Object clip1, clip2;
        private bool hasOneClip;

        private void OnEnable()
        {
            asset = target as UIFadeAsset;
            if (asset == null) return;
            so = new SerializedObject(asset);
            m_key = so.FindProperty("key");
            m_content = so.FindProperty("content");
            m_bgTexturePath = so.FindProperty("m_bgTexturePath");

            //if (!string.IsNullOrEmpty(asset.clip1))
            //{
            //    clip1 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BundleRes/" + asset.clip1 + ".anim");
            //}
            //if (!string.IsNullOrEmpty(asset.clip2))
            //{
            //    clip2 = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BundleRes/" + asset.clip2 + ".anim");
            //}
        }


        public override void OnInspectorGUI()
        {
            if (so == null) return;
            //EditorGUILayout.PropertyField(m_key, true);
            EditorGUILayout.PropertyField(m_content, true);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            //hasOneClip = EditorGUILayout.Toggle("HasOneClip", hasOneClip);

            //EditorGUILayout.Space();
            //clip1 = EditorGUILayout.ObjectField(hasOneClip ? "clip" : "Fade In", clip1, typeof(AnimationClip), false);
            //var tmp = AssetDatabase.GetAssetPath(clip1);
            //if (!string.IsNullOrEmpty(tmp)) asset.clip1 = XEditorUtil.GetRelativePath(tmp);
            //EditorGUILayout.LabelField(asset.clip1);

            //if (!hasOneClip)
            //{
            //    EditorGUILayout.Space();
            //    clip2 = EditorGUILayout.ObjectField("Fade Out", clip2, typeof(AnimationClip), false);
            //    tmp = AssetDatabase.GetAssetPath(clip2);
            //    if (!string.IsNullOrEmpty(tmp)) asset.clip2 = XEditorUtil.GetRelativePath(tmp);
            //    EditorGUILayout.LabelField(asset.clip2);
            //}

            EditorGUILayout.PropertyField(m_bgTexturePath, true);

            EditorGUILayout.EndVertical();
            so.ApplyModifiedProperties();
        }
    }


    [CustomEditor(typeof(MaskFadeAsset))]
    public class PlayableMaskFadeEditor : Editor
    {
        MaskFadeAsset asset;

        private Object clip;

        private void OnEnable()
        {
            asset = target as MaskFadeAsset;
            if (!string.IsNullOrEmpty(asset.clip))
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BundleRes/" + asset.clip + ".anim");
            }
        }

        public override void OnInspectorGUI()
        {
            clip = EditorGUILayout.ObjectField("Fade Clip", clip, typeof(AnimationClip), false);
            if (clip)
            {
                var tmp = AssetDatabase.GetAssetPath(clip);
                if (!string.IsNullOrEmpty(tmp))
                {
                    tmp = XEditorUtil.GetRelativePath(tmp);
                    asset.clip = tmp;
                }
            }
            if (!string.IsNullOrEmpty(asset.clip))
            {
                EditorGUILayout.LabelField(asset.clip);
            }
        }
    }

    [CustomEditor(typeof(TBCFadeAsset))]
    public class PlayablTBCFadeEditor : Editor
    {
        TBCFadeAsset asset;

        private Object clip;

        private void OnEnable()
        {
            asset = target as TBCFadeAsset;
            if (!string.IsNullOrEmpty(asset.clip))
            {
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BundleRes/" + asset.clip + ".anim");
            }
        }

        public override void OnInspectorGUI()
        {
            clip = EditorGUILayout.ObjectField("Fade Clip", clip, typeof(AnimationClip), false);
            if (clip)
            {
                var tmp = AssetDatabase.GetAssetPath(clip);
                if (!string.IsNullOrEmpty(tmp))
                {
                    tmp = XEditorUtil.GetRelativePath(tmp);
                    asset.clip = tmp;
                }
            }
            if (!string.IsNullOrEmpty(asset.clip))
            {
                EditorGUILayout.LabelField(asset.clip);
            }
        }
    }
}
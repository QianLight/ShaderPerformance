using UnityEditor;
using UnityEngine;

namespace XEditor
{

    [CustomEditor(typeof(UIEmotionAsset))]
    public class UIEmotionAssetEditor : Editor
    {
        Texture tex2;
        GameObject emotion;
        UIEmotionAsset asset;
        Object clip;

        private void OnEnable()
        {
            asset = target as UIEmotionAsset;
            if (asset)
            {
                string p = "Assets/BundleRes/" + asset.clip + ".anim";
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();
            string prefix = "Assets/BundleRes/";
            if (tex2 == null && !string.IsNullOrEmpty(asset.head))
            {
                string pat = prefix + asset.head + ".png";
                tex2 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("头像", XEditorUtil.boldLableStyle);
            tex2 = EditorGUILayout.ObjectField(tex2,
                typeof(Texture),
                false,
                GUILayout.Width(164),
                GUILayout.Height(160)) as Texture;
            string path = AssetDatabase.GetAssetPath(tex2);
            path = path.Replace(prefix, "");
            path = path.Replace(".png", "");
            asset.head = path;
            EditorGUILayout.LabelField(asset.head);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("表情", XEditorUtil.boldLableStyle);
            emotion = (GameObject)EditorGUILayout.ObjectField(emotion, typeof(GameObject), false);

            if (emotion != null)
            {
                path = AssetDatabase.GetAssetPath(emotion);
                path = path.Replace(prefix, "");
                path = path.Replace(".prefab", "");
                asset.emotion = path;
            }
            asset.emotion = EditorGUILayout.TextField("emotion", asset.emotion);

            EditorGUILayout.Space();
            asset.pos = EditorGUILayout.Vector3Field("pos: ", asset.pos);
            asset.rot = EditorGUILayout.Vector3Field("rot: ", asset.rot);
            asset.scale = EditorGUILayout.Vector3Field("scale: ", asset.scale);


            EditorGUILayout.Space();
            clip = EditorGUILayout.ObjectField("UI Clip", clip, typeof(AnimationClip), false);
            var t = AssetDatabase.GetAssetPath(clip);
            asset.clip = XEditorUtil.GetRelativePath(t);
            EditorGUILayout.LabelField(asset.clip);
            EditorGUILayout.EndVertical();
        }

    }
}
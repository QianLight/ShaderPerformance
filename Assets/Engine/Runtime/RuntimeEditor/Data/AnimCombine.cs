#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class AnimCombine : ScriptableObject
    {
        public float fps = 30;
        public List<AnimationClip> clips = new List<AnimationClip>();
        public void Combine()
        {
            string fullpath = AssetDatabase.GetAssetPath(this);
            fullpath = fullpath.Replace(".asset", ".anim");
            AnimationClip combineClip = null;
            if (File.Exists(fullpath))
            {
                combineClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fullpath);
            }
            else
            {
                combineClip = new AnimationClip();
            }
            combineClip.name = this.name;
            combineClip.frameRate = fps;
            combineClip.ClearCurves();
            for (int i = 0; i < clips.Count; ++i)
            {
                var clip = clips[i];
                EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(clip);
                for (int j = 0; j < curveBinding.Length; ++j)
                {
                    var binding = curveBinding[j];
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                    AnimationUtility.SetEditorCurve(combineClip, binding, curve);
                };
            }
            EditorCommon.CreateAsset<AnimationClip>(fullpath, ".anim", combineClip);           
        }

        internal class CreateAnimCombine : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                //Create asset
                AssetDatabase.CreateAsset(CreateInstance<AnimCombine>(), pathName);
            }
        }

        [MenuItem(@"Assets/Tool/Anim_Combine")]
        static void AnimCombineCreator()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateAnimCombine>(),
                "AnimCombine.asset", null, null);
        }
    }

    [CustomEditor(typeof(AnimCombine))]
    public class AnimCombineEditor : UnityEngineEditor
    {
        SerializedProperty fps;
        SerializedProperty clips;
        private void OnEnable()
        {
            fps = serializedObject.FindProperty("fps");
            clips = serializedObject.FindProperty("clips");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            var ac = target as AnimCombine;
            EditorGUILayout.PropertyField(fps);
            EditorGUILayout.PropertyField(clips);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Combine", GUILayout.Width(100)))
            {
                ac.Combine();
            }
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
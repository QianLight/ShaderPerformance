#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [Serializable]
    public class AnimControl
    {
        public bool folder;
        public AnimationClip clip;
        public Avatar mask;
        [Range (0, 1)]
        public float weight;
    }
    public enum ERotAxis
    {
        EX,
        EY,
        EZ,
    }
    [DisallowMultipleComponent, ExecuteInEditMode]
    public sealed class DummyCharacter : MonoBehaviour
    {
        public List<AnimControl> clips = new List<AnimControl> ();
        public TextAsset rotCurve;
        public ERotAxis rotAxis = ERotAxis.EZ;
        public Transform bip001;
        private Animator ator;

        private PlayableGraph pg;
        private AnimationPlayableOutput output;
        private AnimationClipPlayable playClip;
        private AnimationCurve rots = new AnimationCurve();
        private float time = 0;


        public void Play (AnimationClip preview)
        {
            if (preview != null)
            {
                if (ator == null)
                {
                    this.gameObject.TryGetComponent (out ator);
                }
                if (ator != null)
                {
                    if (!pg.IsValid ())
                    {
                        pg = PlayableGraph.Create (this.name);
                        output = AnimationPlayableOutput.Create (pg, "output", ator);

                    }
                }
                if (pg.IsValid ())
                {
                    if (playClip.IsValid ())
                    {
                        playClip.Destroy ();
                    }

                    playClip = AnimationClipPlayable.Create (pg, preview);
                    output.SetSourcePlayable (playClip);
                    pg.Play ();
                }
            }
            rots.keys = null;
            if (rotCurve != null)
            {
                string[] curve = rotCurve.text.Replace("\r\n", "\t").Replace("\n", "\t").Split('\t');
                for (int i = 0; i < curve.Length; i += 5)
                {
                    if (i + 5 < curve.Length)
                    {
                        float t = float.Parse(curve[i]);
                        float rot = float.Parse(curve[i + 4]);
                        rots.AddKey(t, rot);
                    }
                }
            }
            time = 0;
        }

        public void Stop ()
        {
            if (pg.IsValid ())
            {
                pg.Stop ();
            }
        }
        private void Update ()
        {
            if (pg.IsValid () && output.IsOutputValid () && !Application.isPlaying)
            {
                output.SetWeight(1);
            }
            if (bip001 != null)
            {
                float rot = rots.Evaluate(time);
                bip001.rotation = Quaternion.LookRotation(Quaternion.Euler(0, rot, 0) * Vector3.forward, Vector3.up);
   
            }
            time += Time.deltaTime;
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (DummyCharacter))]
    public class DummyCharacterEditor : UnityEngineEditor
    {
        SerializedProperty clips;
        SerializedProperty rotCurve;
        SerializedProperty rotAxis;
        SerializedProperty bip001;
        private void OnEnable ()
        {
            clips = serializedObject.FindProperty ("clips");
            rotCurve = serializedObject.FindProperty("rotCurve");
            rotAxis = serializedObject.FindProperty("rotAxis");
            bip001 = serializedObject.FindProperty("bip001");
        }
        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            DummyCharacter dc = target as DummyCharacter;
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Stop", GUILayout.MaxWidth (80)))
            {
                dc.Stop ();
            }
            if (GUILayout.Button ("Add", GUILayout.MaxWidth (80)))
            {
                clips.InsertArrayElementAtIndex (0);
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(rotAxis);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(rotCurve);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(bip001);
            EditorGUILayout.EndHorizontal();
            int removeIndex = -1;
            for (int i = 0; i < clips.arraySize; ++i)
            {
                var ac = clips.GetArrayElementAtIndex (i);

                SerializedProperty folder = ac.FindPropertyRelative ("folder");
                SerializedProperty clip = ac.FindPropertyRelative ("clip");
                var preview = clip.objectReferenceValue as AnimationClip;
                var text = string.Format ("{0}.{1}", i.ToString (), preview != null?preview.name: "empty");
                EditorGUILayout.BeginHorizontal ();
                folder.boolValue = EditorGUILayout.Foldout (folder.boolValue, text);
                if (GUILayout.Button ("Play", GUILayout.MaxWidth (60)))
                {
                    dc.Play (preview);
                }
                if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal ();
                if (folder.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.PropertyField (clip);
                    EditorGUILayout.EndHorizontal ();
                    // EditorGUILayout.BeginHorizontal ();
                    // SerializedProperty mask = property.FindPropertyRelative ("mask");
                    // EditorGUILayout.PropertyField (mask);
                    // EditorGUILayout.EndHorizontal ();
                    // EditorGUILayout.BeginHorizontal ();
                    // SerializedProperty weight = property.FindPropertyRelative ("weight");
                    // EditorGUILayout.PropertyField (weight);
                    // EditorGUILayout.EndHorizontal ();
                    EditorGUI.indentLevel--;
                }

                // EditorGUILayout.PropertyField (ac);
            }
            if (removeIndex >= 0)
            {
                clips.DeleteArrayElementAtIndex (removeIndex);
            }
            serializedObject.ApplyModifiedProperties ();
        }
    }
}
#endif
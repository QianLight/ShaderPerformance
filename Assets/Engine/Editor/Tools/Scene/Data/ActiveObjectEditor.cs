using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine.Editor
{
	[CustomEditor (typeof (ActiveObject))]
	public class ActiveObjectEditor : UnityEngineEditor
	{
		SerializedProperty exString;
		SerializedProperty animationTargetGroup;
		void OnEnable ()
		{
			exString = serializedObject.FindProperty ("exString");
			animationTargetGroup = serializedObject.FindProperty ("animationTargetGroup");
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();
			ActiveObject ao = target as ActiveObject;
#if ANIMATION_OBJECT_V0
			if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
			{
				ao.Save ();
			}
			EditorGUILayout.ObjectField ("", ao.profile, typeof (SceneAnimationData), false);
#endif
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Hide", GUILayout.MaxWidth (80)))
			{
                ao.SetVisible(false);
			}
			if (GUILayout.Button ("Show", GUILayout.MaxWidth (80)))
			{
                ao.SetVisible(true);

            }
            EditorGUILayout.EndHorizontal ();
			EditorGUILayout.PropertyField (exString);
			if (EditorCommon.BeginFolderGroup ("ObjectSetting", ref ao.objGroup))
			{
				EditorGUILayout.PropertyField (animationTargetGroup);
				EditorCommon.EndGroup ();
			}
			serializedObject.ApplyModifiedProperties ();
		}
	}
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine.Editor
{
	[CustomEditor (typeof (LoopMove))]
	public class LoopMoveEditor : UnityEngineEditor
	{
		SerializedProperty exString;
		SerializedProperty duration;
		SerializedProperty autoPlay;
		SerializedProperty moveSpeed;
		SerializedProperty angle;
		SerializedProperty animationTargetGroup;
		SerializedProperty animationMatTargetGroup;
		SerializedProperty startPoint;
		SerializedProperty endPoint;
		SerializedProperty uvMoveX;
		SerializedProperty uvMoveY;
		void OnEnable ()
		{
			exString = serializedObject.FindProperty ("exString");
			duration = serializedObject.FindProperty ("duration");
			autoPlay = serializedObject.FindProperty ("autoPlay");
			moveSpeed = serializedObject.FindProperty ("moveSpeed");
			angle = serializedObject.FindProperty ("angle");
			animationTargetGroup = serializedObject.FindProperty ("animationTargetGroup");
			startPoint = serializedObject.FindProperty ("startPoint");
			endPoint = serializedObject.FindProperty ("endPoint");

			animationMatTargetGroup = serializedObject.FindProperty ("animationMatTargetGroup");
			uvMoveX = serializedObject.FindProperty ("uvMoveX");
			uvMoveY = serializedObject.FindProperty ("uvMoveY");
		}

		public override void OnInspectorGUI ()
		{
			serializedObject.Update ();
			LoopMove lm = target as LoopMove;
			if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
			{
				lm.Save ();
			}
			EditorGUILayout.ObjectField ("", lm.profile, typeof (SceneAnimationData), false);
			if (Application.isPlaying)
				lm.isPlay = EditorGUILayout.Toggle ("IsPlay", lm.isPlay);
			EditorGUILayout.PropertyField (exString);
			EditorGUILayout.PropertyField (duration);
			EditorGUILayout.PropertyField (autoPlay);

			if (EditorCommon.BeginFolderGroup ("ObjectSetting", ref lm.objGroup))
			{
				EditorGUILayout.PropertyField (moveSpeed);
				EditorGUILayout.PropertyField (angle);
				EditorGUILayout.PropertyField (startPoint);
				EditorGUILayout.PropertyField (endPoint);
				EditorGUILayout.PropertyField (animationTargetGroup);
				EditorCommon.EndGroup ();
			}
			if (EditorCommon.BeginFolderGroup ("MatSetting", ref lm.matGroup))
			{
				EditorGUILayout.PropertyField (uvMoveX);
				EditorGUILayout.PropertyField (uvMoveY);
				EditorGUILayout.PropertyField (animationMatTargetGroup);
				EditorCommon.EndGroup ();
			}

			serializedObject.ApplyModifiedProperties ();
		}
	}
}
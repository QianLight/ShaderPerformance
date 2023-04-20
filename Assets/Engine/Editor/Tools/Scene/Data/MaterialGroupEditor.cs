using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine.Editor
{
	[CanEditMultipleObjects, CustomEditor (typeof (MaterialGroup))]
	public class MaterialGroupEditor : UnityEngineEditor
	{
		private List<MeshRenderObject> tmpMergeList = new List<MeshRenderObject> ();

		private void MergeMesh (MergedMeshRef mm)
		{
			if (mm.targetMergeGroupIndex == -1)
			{
				tmpMergeList.AddRange (mm.mergeObjets);
				tmpMergeList.AddRange (mm.smallMergeObjets);
				if (tmpMergeList.Count > 1)
				{
					mm.mergeMesh = MeshAssets.Merge (tmpMergeList, mm.blockID, EngineContext.sceneName);
				}

				tmpMergeList.Clear ();
			}

		}
		public override void OnInspectorGUI ()
		{
			MaterialGroup mg = target as MaterialGroup;
			int mergeIndex = -2;
			GUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField (string.Format ("MeshCount:{0}", mg.meshCount), EditorStyles.boldLabel);
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Merge", GUILayout.MaxWidth (80)))
			{
				mergeIndex = -1;
			}
			bool drawAABB = mg == MaterialGroup.currentMG && -1 == MaterialGroup.blockID;
			bool newDrawAABB = EditorGUILayout.ToggleLeft ("DebugAll", drawAABB);
			if (drawAABB != newDrawAABB)
			{
				if (drawAABB)
				{
					MaterialGroup.currentMG = null;
					MaterialGroup.blockID = -1;
				}
				else
				{
					MaterialGroup.currentMG = mg;
					MaterialGroup.blockID = -1;
				}
			}
			GUILayout.EndHorizontal ();
			int localBlockID = -1;
			for (int i = 0; i < mg.mergeMesh.Count; ++i)
			{
				var mm = mg.mergeMesh[i];
				if (mm.blockID != localBlockID)
				{
					localBlockID = mm.blockID;
					GUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField (string.Format ("=============Block:{0}=============", localBlockID.ToString ()),
						EditorStyles.boldLabel);
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Merge", GUILayout.MaxWidth (80)))
					{
						mergeIndex = i;
					}
					drawAABB = mg == MaterialGroup.currentMG && localBlockID == MaterialGroup.blockID;
					newDrawAABB = EditorGUILayout.ToggleLeft ("Debug", drawAABB);
					if (drawAABB != newDrawAABB)
					{
						if (drawAABB)
						{
							MaterialGroup.currentMG = null;
							MaterialGroup.blockID = -1;
						}
						else
						{
							MaterialGroup.currentMG = mg;
							MaterialGroup.blockID = localBlockID;
						}
					}
					GUILayout.EndHorizontal ();
				}
				EditorGUI.indentLevel++;
				for (int j = 0; j < mm.mergeObjets.Count; ++j)
				{
					GUILayout.BeginHorizontal ();
					EditorGUILayout.ObjectField (mm.mergeObjets[j], typeof (MeshRenderObject), true);
					GUILayout.EndHorizontal ();
				}
				EditorGUI.indentLevel++;
				EditorGUILayout.LabelField ("SmallObjects");
				for (int j = 0; j < mm.smallMergeObjets.Count; ++j)
				{
					GUILayout.BeginHorizontal ();
					EditorGUILayout.ObjectField (mm.smallMergeObjets[j], typeof (MeshRenderObject), true);
					GUILayout.EndHorizontal ();
				}
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;

			}
			if (mergeIndex == -1)
			{
				for (int i = 0; i < mg.mergeMesh.Count; ++i)
				{
					var mm = mg.mergeMesh[i];
					EditorUtility.DisplayProgressBar ("Combine Meshs",
						string.Format ("Block:{0}", mm.blockID), (float) i / mg.mergeMesh.Count);

					MergeMesh (mm);
				}
				EditorUtility.ClearProgressBar ();
			}
			else if (mergeIndex >= 0)
			{
				var mm = mg.mergeMesh[mergeIndex];
				MergeMesh (mm);

			}
		}
	}
}
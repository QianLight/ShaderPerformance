using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
	internal class CreateNestPrefabAsset : EndNameEditAction
	{
		public override void Action (int instanceId, string pathName, string resourceFile)
		{
			//Create asset
			AssetDatabase.CreateAsset (CreateInstance<NestPrefab> (), pathName);
		}
	}
	public class NestPrefab : PrefabData
	{

		public GameObject prefabRef;
		public GameObject runtime;
		public GameObject editorPrefab;
		public CFEngine.PrefabRes res;
		public bool fadeEffect;

		private static void FindRefPrefab (Transform src, Transform tran, List<EditorMeshInfo> eMeshInfo)
		{
            //for (int i = tran.childCount - 1; i >= 0; --i)
            for (int i = 0; i < tran.childCount; i++)
            {
                var t = tran.GetChild (i);
				var srct = src.GetChild (i);
				var prefab = PrefabUtility.GetCorrespondingObjectFromSource (srct.gameObject) as GameObject;
				if (prefab != null)
				{
					var type = PrefabUtility.GetPrefabAssetType (prefab);
					if (type == PrefabAssetType.Regular)
					{
						string prefabPath = AssetsPath.GetAssetFullPath (prefab, out var ext);
						DebugLog.AddEngineLog2 (prefabPath);
						if (srct.TryGetComponent (out SFXWrapper sfx))
						{
							eMeshInfo.Add (new EditorMeshInfo ()
							{
								meshPath = prefab.name.ToLower (),
									isSkin = false,
									isSfx = true,
									partMask = 0,
									pos = t.localPosition,
									rot = t.localRotation,
									scale = t.localScale,
							});
						}
						else
						{
							DebugLog.AddErrorLog ("only sfx can be sub asset");
						}
						GameObject.DestroyImmediate (t.gameObject);
						return;
					}
				}
				else
				{
					if (t.TryGetComponent (out MeshFilter mf))
					{
						string meshPath = AssetsPath.GetAssetFullPath (mf.sharedMesh, out var ext);
						DebugLog.AddEngineLog2 (meshPath);
						Material mat = null;
						if (t.TryGetComponent (out MeshRenderer mr))
						{
							mat = mr.sharedMaterial;
							//mr.sharedMaterial = null;
						}
						eMeshInfo.Add (new EditorMeshInfo ()
						{
							meshPath = meshPath,
								isSkin = false,
								partMask = 0,
								matPath = mat != null?AssetDatabase.GetAssetPath (mat): "",
						});
						mf.sharedMesh = null;
					}
				}
			}
		}

		public void MakePrefab (bool quiet = false)
		{
			if (prefabRef == null)
			{
				return;
			}
			string targetPath = string.Format ("{0}/Runtime/Prefab/{1}.prefab",
				AssetsConfig.instance.ResourcePath, prefabRef.name.ToLower ());
            string prefabPath = string.Format("{0}/Prefabs/{1}.prefab",
                AssetsConfig.instance.ResourcePath, prefabRef.name);
            if (quiet || EditorUtility.DisplayDialog ("MakePrefab", "Is Make Prefab：" + targetPath, "OK", "Cancel"))
			{
				List<EditorMeshInfo> eMeshInfo = new List<EditorMeshInfo> ();
				var go = GameObject.Instantiate (prefabRef) as GameObject;
				go.name = prefabRef.name;
				FindRefPrefab (prefabRef.transform, go.transform, eMeshInfo);
				if (go.TryGetComponent (out Animator ator))
				{
					UnityEngine.Object.DestroyImmediate (ator);
				}
				var t = go.transform;
				t.position = Vector3.zero;
				t.rotation = Quaternion.identity;
				t.localScale = Vector3.one;
				runtime = PrefabUtility.SaveAsPrefabAsset (go, targetPath);
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                res = PrefabConfigTool.SaveEditorPrefabRes (go.name, this, eMeshInfo, fadeEffect);
				GameObject.DestroyImmediate (go);

			}
			EditorCommon.SaveAsset (this);

		}

		private void CombineMesh (Transform tran, Dictionary<Material, List<CombineInstance>> combineInstance)
		{
            //for (int i = tran.childCount - 1; i >= 0; --i)
            for (int i = 0; i < tran.childCount; i++)
            {
				var t = tran.GetChild (i);
				if (t.TryGetComponent (out MeshFilter mf))
				{
					var mesh = mf.sharedMesh;
					if (t.TryGetComponent (out MeshRenderer mr))
					{
						var mat = mr.sharedMaterial;
						if (mat != null)
						{
							List<CombineInstance> lst;
							if (!combineInstance.TryGetValue (mat, out lst))
							{
								lst = new List<CombineInstance> ();
								combineInstance.Add (mat, lst);
							}
							lst.Add (new CombineInstance ()
							{
								mesh = mesh,
									transform = t.localToWorldMatrix
							});
						}
					}

				}
				CombineMesh (t, combineInstance);
			}

		}

		private Mesh SaveCombineMesh (List<CombineInstance> combineInstance, int index)
		{
			string targetPath = AssetDatabase.GetAssetPath (editorPrefab);
			targetPath = targetPath.Replace (".prefab", "_" + index.ToString () + ".asset");
			if (System.IO.File.Exists (targetPath))
			{
				AssetDatabase.DeleteAsset (targetPath);
			}

			var m = new Mesh ();
			m.CombineMeshes (combineInstance.ToArray (), true, true, false);
			m.uv2 = null;
			m.uv4 = null;
			m.uv4 = null;
			m.colors = null;
			MeshUtility.SetMeshCompression (m, ModelImporterMeshCompression.Low);
			MeshUtility.Optimize (m);
			m.UploadMeshData (true);
			m = CommonAssets.CreateAsset<Mesh> (targetPath, ".asset", m);
			return m;
		}
		public void CombineMesh ()
		{
			if (editorPrefab == null)
			{
				return;
			}

			Dictionary<Material, List<CombineInstance>> combineInstance = new Dictionary<Material, List<CombineInstance>> ();

			CombineMesh (editorPrefab.transform, combineInstance);

			if (combineInstance.Count > 0)
			{
				var prefab = new GameObject (editorPrefab.name);
				var it = combineInstance.GetEnumerator ();
				int index = 0;
				while (it.MoveNext ())
				{
					var current = it.Current;
					var m = SaveCombineMesh (current.Value, index);
					var sub = new GameObject (m.name);
					var mr = sub.AddComponent<MeshRenderer> ();
					var mf = sub.AddComponent<MeshFilter> ();
					mf.mesh = m;
					mr.sharedMaterial = current.Key;
					sub.transform.parent = prefab.transform;
					index++;
				}
				string targetPath = string.Format ("{0}/Prefabs/{1}.prefab",
					AssetsConfig.instance.ResourcePath, editorPrefab.name);
				prefabRef = PrefabUtility.SaveAsPrefabAsset (prefab, targetPath);
				GameObject.DestroyImmediate (prefab);
			}

			EditorCommon.SaveAsset (this);

		}

		[MenuItem (@"Assets/Tool/Fbx_NestPrefabConfig")]
		private static void Fbx_NestPrefabConfig ()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists (0, CreateInstance<CreateNestPrefabAsset> (),
				"NestPrefab.asset", null, null);
		}

		internal static void MakeNestPrefab (GameObject prefab)
		{
			string npPath = string.Format ("Assets/Creatures/NestPrefab/{0}.asset", prefab.name);
			var np = AssetDatabase.LoadAssetAtPath<NestPrefab> (npPath);
			if (np == null)
			{
				np = ScriptableObject.CreateInstance<NestPrefab> ();
				np = EditorCommon.CreateAsset<NestPrefab> (npPath, ".asset", np);
			}
			np.prefabRef = prefab;
			np.MakePrefab (true);
		}

		[MenuItem (@"Assets/Tool/Fbx_MakeNestPrefab")]
		private static void Fbx_MakeNestPrefab ()
		{
			CommonAssets.enumPrefab.cb = (prefab, path, context) =>
			{
				MakeNestPrefab (prefab);
			};
			CommonAssets.EnumAsset<GameObject> (CommonAssets.enumPrefab, "MakeNestPrefab");
		}
		public static GameObject FindPrefab (GameObject go)
		{
			if (go == null)
				return null;
			GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource (go) as GameObject;
			if (prefab == null)
				return null;
			string path = AssetDatabase.GetAssetPath (prefab);
			if (path.StartsWith (string.Format ("{0}/Prefab", AssetsConfig.instance.ResourcePath)))
			{
				return AssetDatabase.LoadAssetAtPath<GameObject>(path);
			}
			var parent = go.transform.parent;
			if (parent != null)
				return FindPrefab (parent.gameObject);
			else
				return null;
		}

		[MenuItem ("GameObject/Prefab/MakeNestPrefab", false, 100)]
		public static void MakeNestPrefab ()
		{
			var prefab = FindPrefab (Selection.activeGameObject);
			if (prefab != null)
			{
				NestPrefab.MakeNestPrefab (prefab);
			}
		}
	}

	[CanEditMultipleObjects, CustomEditor (typeof (NestPrefab))]
	public class NestPrefabEditor : UnityEngineEditor
	{

		public override void OnInspectorGUI ()
		{
			NestPrefab np = target as NestPrefab;
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Save", GUILayout.MaxWidth (80)))
			{
				EditorCommon.SaveAsset (np);
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("EditorPrefab", GUILayout.MaxWidth (100));
			np.prefabRef = EditorGUILayout.ObjectField (np.prefabRef, typeof (GameObject), true) as GameObject;
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Runtime", GUILayout.MaxWidth (100));
			np.runtime = EditorGUILayout.ObjectField (np.runtime, typeof (GameObject), true) as GameObject;
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Res", GUILayout.MaxWidth (100));
			np.res = EditorGUILayout.ObjectField (np.res, typeof (CFEngine.PrefabRes), true) as CFEngine.PrefabRes;
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("MakePrefab", GUILayout.MaxWidth (120)))
			{
				np.MakePrefab ();
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			np.fadeEffect = EditorGUILayout.Toggle ("FadeEffect", np.fadeEffect, GUILayout.MaxWidth (300));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("EditorPrefab2", GUILayout.MaxWidth (100));
			np.editorPrefab = EditorGUILayout.ObjectField (np.editorPrefab, typeof (GameObject), true) as GameObject;
			if (GUILayout.Button ("CombineMesh", GUILayout.MaxWidth (120)))
			{
				np.CombineMesh ();
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUI.indentLevel--;
		}
	}
}
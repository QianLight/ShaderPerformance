using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine.Editor
{
	public enum PrefabType
	{
		Prefab,
		SFXPrefab,
	}

	[Serializable]
	public class PrefabRes
	{
		public GameObject src;
		public GameObject des;
		public BandposeData bd;
	}

	[Serializable]
	public class PrefabPathConfig : BaseFolderHash
	{
		public string srcPath;
		public List<PrefabRes> configs = new List<PrefabRes> ();
		[System.NonSerialized]
		public HashSet<string> targetPaths = new HashSet<string> ();
		[System.NonSerialized]
		public Vector2 scroll = Vector2.zero;
		public void Clear ()
		{
			configs.Clear ();
			targetPaths.Clear ();
		}

	}

	public class EditorPrefabData : AssetBaseConifg<EditorPrefabData>
	{
		public List<PrefabPathConfig> prefabFolder = new List<PrefabPathConfig> ();
		public string[] ignoreName = new string[]
		{
			"empty",
			"test"
		};
		public static string[] prefabPath = new string[]
		{
			"Prefab",
			"SFX"
		};
	}
}
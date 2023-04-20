using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine.Editor
{
	[Serializable]
	public class PartOutlineConfig
	{
		public string partName;
		public bool isOverride = false;
        [NonSerialized]
        public Material mat;
	}

	[Serializable]
	public class OutlineData
	{
		public Color outlineColor = Color.black;
		public float outlineWidth = 0.1f;
		public float outlineMinWidth = 0.5f;
		public float outlineMaxWidth = 2f;
	}

	[Serializable]
	public class PrefabOutlineConfig
	{
		public GameObject prefab;
		public OutlineData outline = new OutlineData ();

		[NonSerialized]
		public bool edit = false;
		[NonSerialized]
		public GameObject go;
		public List<PartOutlineConfig> partOutlineConfig = new List<PartOutlineConfig> ();
	}

	public class OutlineConfigData : ScriptableObject
	{
		public List<PrefabOutlineConfig> configs = new List<PrefabOutlineConfig> ();
		public bool configFolder = false;
		[NonSerialized]
		public PrefabOutlineConfig testGo;
		[NonSerialized]
		public float currentScale;

		public float offsetY = 0;
		public float dist = 0;
		// public float minDist = 1;
		// public float minScale = 1;
		// public float maxDist = 5;
		// public float maxScale = 1;

		// public float disapperDist = 10;
	}
}
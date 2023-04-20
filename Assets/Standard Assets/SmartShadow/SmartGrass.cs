using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using System.IO;
using UnityEngine.Rendering;
using UnityEditor;

[ExecuteInEditMode]
#endif
public class SmartGrass : MonoBehaviour
{
#if UNITY_EDITOR
	public class StaticLightBake
	{
		public StaticLightBake(UnityEngine.Rendering.Universal.ScriptableRendererFeature feature)
		{
			mFeature = feature as UnityEngine.Experiemntal.Rendering.Universal.ShadowBakeFeature;
		}
		private Transform mLight;
		private Camera mCamera;
		private string lightMapName = "LightGrassMap";
		private string groundcolorName = "GroundColor";
		private UnityEngine.Experiemntal.Rendering.Universal.ShadowBakeFeature mFeature = null;
		private bool lightReady = false;
		private int maxSize = 4096;
		private string mBakedLightName;

		public void SetBakedLightName(string bakedLightName)
		{
			mBakedLightName = bakedLightName;
		}

		public void SetLight(List<Transform> fromObj, List<Transform> toObj)
		{
			lightReady = false;
			if (fromObj != null && fromObj.Count > 0)
			{
				GameObject camObj = new GameObject("LightBakeCamera");
				camObj.layer = SmartShadow.BakeLayer;
				mCamera = camObj.AddComponent<Camera>();
				mCamera.cullingMask = (int)Mathf.Pow(2, SmartShadow.BakeLayer);
				mCamera.orthographic = true;
				mCamera.backgroundColor = Color.black;
				mCamera.clearFlags = CameraClearFlags.Color;
				mCamera.enabled = false;

				mLight = mCamera.transform;
				mLight.rotation = Quaternion.Euler(90, 0, 0);
				for (int i = 0; i < fromObj.Count; i++)
				{
					FetchRoot(fromObj[i], true);
				}
				if (toObj != null && toObj.Count > 0)
				{
					for (int i = 0; i < toObj.Count; i++)
					{
						FetchRoot(toObj[i], false);
					}
				}

				SmartShadow.SetCorners(boundsPointsList, mLight, mCamera);
				lightReady = true;
			}
		}

		string GetPath()
		{
			string path = mLight.gameObject.scene.path;
			if (!string.IsNullOrEmpty(path))
			{
				string folder = string.Concat(path.Substring(0, path.Length - 6 - mLight.gameObject.scene.name.Length) + "/SmartShadow/");
				if (!System.IO.Directory.Exists(folder))
				{
					System.IO.Directory.CreateDirectory(folder);
					AssetDatabase.Refresh();
				}
				return folder;
			}
			return "Assets/";
		}
		public void BakeLightmapTex(bool useTexArray, int maxTexSize, float pixPM, ref StaticLightBakeResult result)
		{
			if (!lightReady)
				return;
			string folderStr = GetPath();

			if (result == null)
				result = new StaticLightBakeResult();
			result.CanUse = true;
			result.LightProjecionMatrix = SmartShadow.GetLightProjectMatrix(mCamera);
			int height = SmartShadow.GetPOTSize(useTexArray, mCamera.orthographicSize * pixPM, 0);
			int width = SmartShadow.GetPOTSize(useTexArray, mCamera.aspect * height, 0);

			SmartShadow.RenderTexSize size = new SmartShadow.RenderTexSize(width, height);
			if (!useTexArray)
			{
				size.SetMaxSize(maxTexSize);
			}
			RenderTextureDescriptor ds = new RenderTextureDescriptor(size.Width, size.Height, SmartShadow.RTFormat, 32, 0);
			RenderTexture rt = new RenderTexture(ds);
			rt.antiAliasing = 4;
			rt.filterMode = FilterMode.Bilinear;
			rt.name = "rt";
			mFeature.settings.RT = rt;
			mCamera.Render();
			mFeature.settings.RT = null;


			for (int i = 0; i < renderItemList.Count; i++)
			{
				SmartShadow.RenderItem item = renderItemList[i];
				item.Obj.layer = item.LastLayer;
			}
			renderItemList.Clear();

			string pngName = folderStr + mLight.gameObject.scene.name + "_" + mBakedLightName + "_" + lightMapName + ".png";

			result._lightmaptex = new Texture2D(size.Width, size.Height, TextureFormat.RGB24, true, true);
			result._lightmaptex.filterMode = FilterMode.Point;
			result._lightmaptex.name = "depthTex";
			SmartShadow.TextureToTexture2D(ref rt, ref result._lightmaptex, 0, 0);
			byte[] bs = result._lightmaptex.EncodeToPNG();
			System.IO.File.WriteAllBytes(pngName, bs);
			AssetDatabase.Refresh();
			result._lightmaptex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngName);
			SmartShadow.SetTextureFormat(result._lightmaptex, maxSize);

			rt.Release();

			GameObject.DestroyImmediate(mCamera);
			DestoryMe destory = mLight.gameObject.AddComponent<DestoryMe>();
			destory.Destory = 2;
			EditorUtility.ClearProgressBar();
		}
		public void BakeGroundColorTex(bool useTexArray, int maxTexSize, float pixPM, ref StaticLightBakeResult result)
		{
			if (!lightReady)
				return;
			string folderStr = GetPath();
			if (result == null)
				result = new StaticLightBakeResult();
			result.CanUse = true;
			result.LightProjecionMatrix = SmartShadow.GetLightProjectMatrix(mCamera);
			int height = SmartShadow.GetPOTSize(useTexArray, mCamera.orthographicSize * pixPM, 0);
			int width = SmartShadow.GetPOTSize(useTexArray, mCamera.aspect * height, 0);

			SmartShadow.RenderTexSize size = new SmartShadow.RenderTexSize(width, height);
			if (!useTexArray)
			{
				size.SetMaxSize(maxTexSize);
			}
			RenderTextureDescriptor ds = new RenderTextureDescriptor(size.Width, size.Height, SmartShadow.RTFormat, 32, 0);
			RenderTexture rt = new RenderTexture(ds);
			rt.antiAliasing = 4;
			rt.filterMode = FilterMode.Bilinear;
			rt.name = "rt";
			mFeature.settings.RT = rt;
			mCamera.Render();
			mFeature.settings.RT = null;


			for (int i = 0; i < renderItemList.Count; i++)
			{
				SmartShadow.RenderItem item = renderItemList[i];
				item.Obj.layer = item.LastLayer;
			}
			renderItemList.Clear();

			string tgaName = folderStr + mLight.gameObject.scene.name + "_" + mBakedLightName + "_" + groundcolorName + ".tga";
			string pngName = folderStr + mLight.gameObject.scene.name + "_" + mBakedLightName + "_" + groundcolorName + ".png";

			result._groundcolor = new Texture2D(size.Width, size.Height, TextureFormat.RGBA32, true, true);
			result._groundcolor.filterMode = FilterMode.Bilinear;
			result._groundcolor.name = "depthTex";
			SmartShadow.TextureToTexture2D(ref rt, ref result._groundcolor, 0, 0);
			byte[] bs = result._groundcolor.EncodeToTGA();
			System.IO.File.WriteAllBytes(tgaName, bs);
			AssetDatabase.Refresh();
			result._groundcolor = AssetDatabase.LoadAssetAtPath<Texture2D>(tgaName);
			SmartShadow.SetTextureFormat(result._groundcolor, maxSize);

			if (File.Exists(pngName))
				AssetDatabase.DeleteAsset(pngName);

			rt.Release();

			GameObject.DestroyImmediate(mCamera);
			DestoryMe destory = mLight.gameObject.AddComponent<DestoryMe>();
			destory.Destory = 2;
			EditorUtility.ClearProgressBar();
		}



		public void FetchRoot(Transform root, bool isFromObj)
		{
			if (root != null && root.gameObject.activeSelf)
			{
				foreach (Transform obj in root)
				{
					if (obj.gameObject.activeSelf)
					{
						FetchRoot(obj, isFromObj);
					}
				}
				PushBounds(root, isFromObj);
			}
		}

		public void PushBounds(Transform obj, bool isFromObj)
		{
			Renderer render = obj.gameObject.GetComponent<Renderer>();
			if (render != null)
			{
				Material mat = render.sharedMaterial;
				if (mat != null && (mat.shader.name == "URP/Scene/Uber" || mat.shader.name == "URP/Scene/Layer" || mat.shader.name == "URP/Scene/UberGrass"))
				{
					if (isFromObj)
					{
						renderItemList.Add(new SmartShadow.RenderItem(obj.gameObject, obj.gameObject.layer));
						obj.gameObject.layer = SmartShadow.BakeLayer;
					}
					boundsPointsList.AddRange(SmartShadow.GetBoundsPoints(mLight, render.bounds));
				}
			}
		}

		private List<SmartShadow.RenderItem> renderItemList = new List<SmartShadow.RenderItem>();
		private List<Vector3> boundsPointsList = new List<Vector3>();
	}
	public UnityEngine.Rendering.Universal.ScriptableRendererFeature OpenBakeFeature(bool open)
	{
		UnityEngine.Rendering.Universal.ScriptableRendererFeature feature = null;
		UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
		if (urpAsset != null)
		{
			ScriptableRendererData[] data = urpAsset.GetCurrentRendererData();
			if (data.Length > 0)
			{
				ScriptableRendererData RendererData = data[0];
				foreach (UnityEngine.Rendering.Universal.ScriptableRendererFeature tmp in RendererData.rendererFeatures)
				{
					if (tmp.name == URPFeatureName)
					{
						feature = tmp;
						tmp.SetActive(open);
						break;
					}
				}
				RendererData.SetDirty();
			}
		}
		return feature;
	}
	private StaticLightBake mapAABB = null;
	public string URPFeatureName = "GrassLightBake";
#endif

	[System.Serializable]
	public class StaticLightBakeResult
	{
		public bool CanUse = false;
		public Texture2D _lightmaptex;
		public Texture2D _groundcolor;
		public Matrix4x4 LightProjecionMatrix;
		public Vector4 _normaldir;
		public Vector4 _grasslightcontrol;

	}

	public int PixPM = 20;
	public int MaxTextureSize = 1024;
#if UNITY_EDITOR
	public List<Transform> FromObjects;
#endif
	public List<Transform> ToObjects;
	private int _CustomLightmap = Shader.PropertyToID("_CustomLightmap");
	private int _CustomGroundColor = Shader.PropertyToID("_CustomGroundColor");
	private int _BakeLightmapProjection = Shader.PropertyToID("_BakeLightmapProjection");
	private int _GrassNormalDir = Shader.PropertyToID("_GrassNormalDir");
	private int _GrassLightDirction = Shader.PropertyToID("_SSSlightcontrol");

	[Range(0, 1.0f)]
	public float ShadowStrength = 0.9f;
	private Transform GrasssetnormalTf;
	private Transform GrassLightDirction;
	public StaticLightBakeResult BakeInfo;

	private string lightName;

#if UNITY_EDITOR
	public void BakeLightmap()
	{
		UnityEngine.Rendering.Universal.ScriptableRendererFeature feature = OpenBakeFeature(true);
		mapAABB = new StaticLightBake(feature);
		mapAABB.SetBakedLightName(lightName);
		mapAABB.SetLight(FromObjects, ToObjects);
		mapAABB.BakeLightmapTex(false, MaxTextureSize, PixPM, ref BakeInfo);
		SetParameter(true);
		feature.SetActive(false);
	}

	public void BakeGroundColor()
	{
		UnityEngine.Rendering.Universal.ScriptableRendererFeature feature = OpenBakeFeature(true);
		mapAABB = new StaticLightBake(feature);
		mapAABB.SetBakedLightName(lightName);
		mapAABB.SetLight(FromObjects, ToObjects);
		mapAABB.BakeGroundColorTex(false, MaxTextureSize, PixPM, ref BakeInfo);
		SetParameter(true);
		feature.SetActive(false);
	}

	public static void BakeGrassLight(GameObject light, List<Transform> from, List<Transform> to, int PixPM, int MaxTextureSize)
	{
		SmartGrass grass = light.GetComponent<SmartGrass>();
		if (grass == null)
		{
			grass = light.AddComponent<SmartGrass>();
		}
		grass.PixPM = PixPM;
		grass.MaxTextureSize = MaxTextureSize;
		grass.FromObjects = from;
		grass.ToObjects = to;
		grass.lightName = light.name;
		grass.ProcessInvalidData();
		grass.SetKeywordEnabled(BakeType._LIGHTMAPUNENABLE, true);
		grass.BakeLightmap();
		grass.SetKeywordEnabled(BakeType._LIGHTMAPUNENABLE, false);
		grass.SetKeywordEnabled(BakeType._GROUNDCOLORUNENABLE, true);
		grass.BakeGroundColor();
		grass.SetKeywordEnabled(BakeType._GROUNDCOLORUNENABLE, false);
	}

	private void ProcessInvalidData()
	{
		void RemoveNullElement(List<Transform> transforms)
		{
			for (int i = 0; i < transforms.Count; i++)
			{
				if (!transforms[i])
				{
					transforms.RemoveAt(i--);
				}
			}
		}

		RemoveNullElement(FromObjects);
		RemoveNullElement(ToObjects);
	}
#endif

	void SetParameter(bool enable)
	{
		if (BakeInfo != null && BakeInfo.CanUse)
		{
			Shader.SetGlobalTexture(_CustomGroundColor, BakeInfo._groundcolor);
			Shader.SetGlobalTexture(_CustomLightmap, BakeInfo._lightmaptex);
			Shader.SetGlobalMatrix(_BakeLightmapProjection, BakeInfo.LightProjecionMatrix);
			Shader.SetGlobalVector(_GrassNormalDir, BakeInfo._normaldir);
			Shader.SetGlobalVector(_GrassLightDirction, BakeInfo._grasslightcontrol);

			// if (ToObjects != null && ToObjects.Count > 0)
			// {
			// 	foreach (Transform o in ToObjects)
			// 	{
			// 		SetRenderParameter(o, enable);
			// 	}
			// }
		}
	}

	void SetRenderParameter(Transform trans, bool enable)
	{
		if (trans == null)
			return;
		Renderer render = trans.gameObject.GetComponent<Renderer>();
		if (render != null)
		{
			Material mat = render.sharedMaterial;
			if (mat != null && mat.shader.name == "URP/Scene/UberGrass")
			{
				mat.SetTexture(_CustomGroundColor, BakeInfo._groundcolor);
				mat.SetTexture(_CustomLightmap, BakeInfo._lightmaptex);
				mat.SetMatrix(_BakeLightmapProjection, BakeInfo.LightProjecionMatrix);
				mat.SetVector(_GrassNormalDir, BakeInfo._normaldir);
				mat.SetVector(_GrassLightDirction, BakeInfo._grasslightcontrol);
			}
		}
		foreach (Transform o in trans)
		{
			SetRenderParameter(o, enable);
		}
	}

	/*
		void Start()
		{
			SetParameter(true);
		}
	*/
	void OnEnable()
	{
		firstUpdate = true;
		SetParameter(true);
	}

	private bool firstUpdate = true;

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || firstUpdate)
        {
            firstUpdate = false;
            SetGrassNormal();
            SetGrassLightdirction();
        }
		SetParameter(true);
#else
		if (firstUpdate)
		{
			firstUpdate = false;
			SetParameter(true);
		}
#endif

	}

	void OnDisable()
	{
		SetParameter(false);
	}
#if UNITY_EDITOR
	public void SetKeywordEnabled(BakeType RT, bool enable)
	{
		if (FromObjects != null && FromObjects.Count > 0)
		{
			for (int i = 0; i < FromObjects.Count; i++)
			{
				Renderer[] _renderers = FromObjects[i].GetComponentsInChildren<Renderer>();
				if (_renderers.Length > 0)
				{
					for (int x = 0; x < _renderers.Length; x++)
					{
						SetEnable(_renderers[x], RT, enable);
					}
				}
			}
		}
	}

	private void SetEnable(Renderer _renderer, BakeType RT, bool enable)
	{
		string KEYWORD = RT.ToString();

		if (enable)
		{
			//	_renderer.sharedMaterial.EnableKeyword("_DEBUG_ON");
			_renderer.sharedMaterial.EnableKeyword(KEYWORD);
		}
		else
		{
			//_renderer.sharedMaterial.DisableKeyword("_DEBUG_ON");
			_renderer.sharedMaterial.DisableKeyword(KEYWORD);
		}
	}

	private void SetGrassNormal()
	{
		if (GrasssetnormalTf == null)
		{
			if (!this.transform.Find("grassnormal"))
			{
				GrasssetnormalTf = new GameObject("grassnormal").transform;
				GrasssetnormalTf.SetParent(this.transform);
				GrasssetnormalTf.position = Vector3.zero;

			}
			else
			{
				GrasssetnormalTf = this.transform.Find("grassnormal");
			}

		}
		BakeInfo._normaldir.x = GrasssetnormalTf.forward.x;
		BakeInfo._normaldir.y = GrasssetnormalTf.forward.y;
		BakeInfo._normaldir.z = GrasssetnormalTf.forward.z;
	}
	private void SetGrassLightdirction()
	{
		if (GrassLightDirction == null)
		{
			if (!this.transform.Find("grasslightdirction"))
			{
				GrassLightDirction = new GameObject("grasslightdirction").transform;
				GrassLightDirction.SetParent(this.transform);
				GrassLightDirction.position = Vector3.zero;
				//		GrassLightDirction.rotation = this.transform.rotation;
				GrassLightDirction.forward = this.transform.forward;
			}
			else
			{
				GrassLightDirction = this.transform.Find("grasslightdirction");
			}
		}
		BakeInfo._grasslightcontrol.x = GrassLightDirction.forward.x;
		BakeInfo._grasslightcontrol.y = GrassLightDirction.forward.y;
		BakeInfo._grasslightcontrol.z = GrassLightDirction.forward.z;

	}



	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (GrasssetnormalTf)
		{
			Gizmos.DrawSphere(GrasssetnormalTf.position, 1f);
			Vector3 despostion = GrasssetnormalTf.forward;
			Gizmos.DrawRay(GrasssetnormalTf.position, despostion * 50f);
		}
		if (GrassLightDirction)
		{
			Gizmos.DrawSphere(GrassLightDirction.position, 1f);
			Vector3 despostion1 = GrassLightDirction.forward;
			Gizmos.DrawRay(GrassLightDirction.position, despostion1 * 50f);
		}
	}

#endif
}
public enum BakeType
{
	_NONE,
	_LIGHTMAPUNENABLE,
	_GROUNDCOLORUNENABLE
}



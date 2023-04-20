using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


#if UNITY_EDITOR
using UnityEngine.Rendering;
using UnityEditor;
[ExecuteInEditMode]
#endif
public class SmartShadow : MonoBehaviour
{
	public const TextureFormat TexFormat = TextureFormat.ASTC_6x6;
#if UNITY_EDITOR
	public const int BakeLayer = 2;
	public const TextureImporterFormat TexImporterFormat = TextureImporterFormat.ASTC_6x6;
	public const RenderTextureFormat RTFormat = RenderTextureFormat.ARGB32;
#endif

	public static SmartShadow Instance;

	public static TextureFormat Tex2DFormat()
	{
#if UNITY_IOS
          return TextureFormat.ASTC_5x5;
#endif
#if UNITY_ANDROID
		return TextureFormat.ASTC_5x5;
#endif
#if UNITY_STANDALONE
		return TextureFormat.ARGB32;
#endif
	}

#if UNITY_EDITOR
	public class RenderItem
	{
		public RenderItem(GameObject obj, int lastLayer)
		{
			Obj = obj;
			LastLayer = lastLayer;
		}
		public GameObject Obj;
		public int LastLayer;
	}
	public class RenderTexSize
    {
		public int Width;
		public int Height;
		public RenderTexSize(int width, int height)
        {
			Width = width;
			Height = height;
		}
		public void SetMaxSize(int size)
        {
			if(Width > Height)
            {
				if(size < Width)
                {
					float wdh = Height * 1.0f / Width;
					Width = size;
					Height = (int)(Width * wdh);
				}
            }
			else
            {
				if (size < Height)
				{
					float wdh = Width * 1.0f / Height;
					Height = size;
					Width = (int)(Height * wdh);
				}
			}
        }
    }
	public class StaticShadowBake
	{
		public StaticShadowBake(UnityEngine.Rendering.Universal.ScriptableRendererFeature feature)
        {
			mFeature = feature as UnityEngine.Experiemntal.Rendering.Universal.ShadowBakeFeature;
		}
		private Transform mLight;
		private Camera mCamera;
		private string lightMapName = "lightMap";
		private UnityEngine.Experiemntal.Rendering.Universal.ShadowBakeFeature mFeature = null;
		private bool lightReady = false;
		private int maxSize = 4096;
		public void SetLight(Transform light, List<Transform> fromObj, List<Transform> fromObjectsExclude, List<Transform> toObj, bool toObjsAddBounds)
		{
			lightReady = false;
			lightMapName = light.gameObject.scene.name + "_" + light.name + "_bakeshadow";
			if (light != null && fromObj != null && fromObj.Count > 0)
			{
				GameObject camObj = new GameObject("LightShadowCamera");
				camObj.layer = BakeLayer;
				mCamera = camObj.AddComponent<Camera>();
				mCamera.cullingMask = (int)Mathf.Pow(2, BakeLayer);
				mCamera.orthographic = true;
				mCamera.backgroundColor = Color.black;
				mCamera.clearFlags = CameraClearFlags.Color;
				mCamera.enabled = false;

				mLight = mCamera.transform;
				mLight.rotation = light.rotation;
				for (int i = 0; i < fromObj.Count; i++)
				{
					FetchRoot(fromObj[i], fromObjectsExclude, true);
				}
				if (toObjsAddBounds && toObj != null && toObj.Count > 0)
				{
					for (int i = 0; i < toObj.Count; i++)
					{
						FetchRoot(toObj[i], fromObjectsExclude, false);
					}
				}
				SetCorners(boundsPointsList, mLight, mCamera);
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
		public void BakeTex(bool useTexArray, int maxTexSize, float pixPM, int splitSize, Material ResolveMat, ref StaticShadowBakeResult result)
		{
			if (!lightReady)
				return;
			string folderStr = GetPath();
			result = new StaticShadowBakeResult();
			result.CanUse = true;
			result.LightProjecionMatrix = GetLightProjectMatrix(mCamera);
			int height = GetPOTSize(useTexArray, mCamera.orthographicSize * pixPM, splitSize);
			int width = GetPOTSize(useTexArray, mCamera.aspect * height, splitSize);

			RenderTexSize size = new RenderTexSize(width, height);
			if (!useTexArray)
			{
				size.SetMaxSize(maxTexSize);
			}
			RenderTextureDescriptor ds = new RenderTextureDescriptor(size.Width, size.Height, RTFormat, 32, 0);
			RenderTexture rt = new RenderTexture(ds);
			//rt.antiAliasing = 1;
			rt.antiAliasing = 4;
			rt.filterMode = FilterMode.Bilinear;
			rt.name = "rt";
			mFeature.settings.RT = rt;
			//mCamera.targetTexture = rt;
			mCamera.Render();
			mFeature.settings.RT = null;
			//mCamera.targetTexture = null;

			for (int i = 0; i < renderItemList.Count; i++)
			{
				RenderItem item = renderItemList[i];
				item.Obj.layer = item.LastLayer;
			}
			renderItemList.Clear();

			if (ResolveMat != null && isNeedPostResolve)
			{
				
				RenderTexture colorRT = new RenderTexture(ds);
				colorRT.name = "colorRT";
				colorRT.filterMode = FilterMode.Bilinear;
				//ResolveMat.SetTexture("_MyDepthTex", rt);
				Graphics.Blit(rt, colorRT, ResolveMat, 0);
				Graphics.Blit(colorRT, rt);
				//AssetDatabase.CreateAsset(colorRT, string.Format("{0}depth2.asset", folderStr));
				colorRT.Release();
				
				//RenderTexture temp = RenderTexture.GetTemporary(width, width, 0, rt.format);
				//Graphics.Blit(rt, temp, ResolveMat);
				//Graphics.Blit(temp, rt);
				//RenderTexture.ReleaseTemporary(temp);
			}
			//AssetDatabase.CreateAsset(rt, string.Format("{0}depth1.asset", folderStr));
			if (useTexArray)
			{
				result.XCount = size.Width / splitSize;
				result.YCount = size.Height / splitSize;
				result.Count = result.XCount * result.YCount;
				//Debug.LogError(result.XCount + "," + result.YCount + "," + result.Count + "," + width + "," + height);

				List<Texture2D> texList = new List<Texture2D>();
				for (int j = 0; j < result.YCount; j++)
				{
					for (int i = 0; i < result.XCount; i++)
					{
						int tmpIndex = i + j * result.XCount;
						string pngName = string.Format("{0}depthTexturePNG_{1}.png", folderStr, tmpIndex);
						Texture2D t2d = new Texture2D(splitSize, splitSize, TextureFormat.RGB24, false, true);
						t2d.name = "depthTexture2Dcell_" + tmpIndex;

						EditorUtility.DisplayProgressBar("生成小纹理", pngName, (float)(tmpIndex / (result.Count - 1)));
						TextureToTexture2D(ref rt, ref t2d, i * t2d.width, j * t2d.height);

						byte[] bs = t2d.EncodeToPNG();
						System.IO.File.WriteAllBytes(pngName, bs);
						AssetDatabase.Refresh();
						t2d = AssetDatabase.LoadAssetAtPath<Texture2D>(pngName);
						SetTextureFormat(t2d, maxSize);
						texList.Add(t2d);
					}
				}

				//查找无像素数据的空白块
				result.Map = new float[result.Count];
				int goodCount = 0, badCount = 0;
				int mapIndex = 0;
				for (int i = 0; i < texList.Count; i++)
				{
					Texture2D t2d = texList[i];
					var mip0Data = t2d.GetPixelData<Color32>(0);
					bool blackBlock = true;
					//Debug.LogError("检查空图片中:" + (float)(i / (texList.Count - 1)));
					//EditorUtility.DisplayProgressBar("检查空图片中", t2d.name, (float)(i / (texList.Count - 1)));
					for (int j = 0; j < mip0Data.Length; j++)
					{
						Color32 c = mip0Data[j];
						if (c.g > 0)
						{
							blackBlock = false;
							break;
						}
					}
					if (blackBlock)
					{
						result.Map[mapIndex] = -1;
						texList.RemoveAt(i);
						i--;
						badCount++;

					}
					else
					{
						result.Map[mapIndex] = goodCount;
						goodCount++;
					}
					mapIndex++;

				}

				result.TexArray = new Texture2DArray(splitSize, splitSize, texList.Count, Tex2DFormat(), false, true);
				result.TexArray.wrapMode = TextureWrapMode.Clamp;
				for (int i = 0; i < texList.Count; i++)
				{
					Texture2D t2d = texList[i];
					//EditorUtility.DisplayProgressBar("生成纹理数组中", t2d.name, (float)(i / (texList.Count - 1)));
					//Debug.LogError("生成纹理数组中:" + (float)(i / (texList.Count - 1)));
					Graphics.CopyTexture(t2d, 0, 0, result.TexArray, i, 0);
				}
				AssetDatabase.CreateAsset(result.TexArray, string.Format("{0}{1}_texArray.asset", folderStr, lightMapName));

				//for (int i = 0; i < result.Map.Length; i++)
				{
					//string pngName = string.Format("{0}depthTexturePNG_{1}.png", folderStr, i);
					//EditorUtility.DisplayProgressBar("删除小纹理", pngName, (float)(i / (texList.Count - 1)));
					//AssetDatabase.DeleteAsset(pngName);
				}

				//Texture2D alltex = new Texture2D(width, height);
				//alltex.name = "depthTexture2D";
				//TextureToTexture2D(ref rt, ref alltex, 0, 0);
				//AssetDatabase.CreateAsset(alltex, "Assets/Shadow/depthTexture2D.asset");
			}
			else
			{
				string pngName = string.Format("{0}{1}.png", folderStr, lightMapName);
				result.XCount = 1;
				result.YCount = 1;
				result.Count = 1;
				result.Map = new float[result.Count];

				result.Tex = new Texture2D(size.Width, size.Height, TextureFormat.RGB24, true, true);
				result.Tex.filterMode = FilterMode.Point;
				result.Tex.name = "depthTex";
				TextureToTexture2D(ref rt, ref result.Tex, 0, 0);
				byte[] bs = result.Tex.EncodeToPNG();
				System.IO.File.WriteAllBytes(pngName, bs);
				AssetDatabase.Refresh();
				result.Tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngName);
				SetTextureFormat(result.Tex, maxSize);
				//AssetDatabase.CreateAsset(rt, string.Format("{0}xxxxxxxx.asset", folderStr));
			}
			rt.Release();

			GameObject.DestroyImmediate(mCamera);
			DestoryMe destory = mLight.gameObject.AddComponent<DestoryMe>();
			destory.Destory = 2;
			EditorUtility.ClearProgressBar();
		}

		public void FetchRoot(Transform root, List<Transform> fromObjectsExclude, bool isFromObj)
		{
			if(fromObjectsExclude != null && fromObjectsExclude.Count > 0)
            {
				if(fromObjectsExclude.Contains(root))
                {
					return;
                }
            }
			if(root != null && root.gameObject.activeSelf)
            {
				foreach (Transform obj in root)
				{
					if (obj.gameObject.activeSelf)
					{
						FetchRoot(obj, fromObjectsExclude, isFromObj);
					}
				}
				PushBounds(root, fromObjectsExclude, isFromObj);
			}
		}

		public void PushBounds(Transform obj, List<Transform> fromObjectsExclude, bool isFromObj)
		{
			Renderer render = obj.gameObject.GetComponent<Renderer>();
			if (render != null && (render.shadowCastingMode == ShadowCastingMode.On))
			{
				if (isFromObj)
				{
					renderItemList.Add(new RenderItem(obj.gameObject, obj.gameObject.layer));
					obj.gameObject.layer = BakeLayer;
				}
				boundsPointsList.AddRange(GetBoundsPoints(mLight, render.bounds));
			}
		}
		private List<RenderItem> renderItemList = new List<RenderItem>();
		private List<Vector3> boundsPointsList = new List<Vector3>();
	}
	public static void SetCorners(List<Vector3> boundsPointsList, Transform mLight, Camera mCamera)
	{
		Vector3[] nearCorners = new Vector3[4];
		Vector3[] farCorners = new Vector3[4];

		Vector3 tmpV3;
		float minX = 0;
		float maxX = 0;

		float minY = 0;
		float maxY = 0;

		float minZ = 0;
		float maxZ = 0;
		for (int i = 0; i < boundsPointsList.Count; i++)
		{
			tmpV3 = boundsPointsList[i];
			if (i == 0)
			{
				minX = maxX = tmpV3.x;
				minY = maxY = tmpV3.y;
				minZ = maxZ = tmpV3.z;
				continue;
			}
			if (minX > tmpV3.x)
				minX = tmpV3.x;
			if (maxX < tmpV3.x)
				maxX = tmpV3.x;

			if (minY > tmpV3.y)
				minY = tmpV3.y;
			if (maxY < tmpV3.y)
				maxY = tmpV3.y;

			if (minZ > tmpV3.z)
				minZ = tmpV3.z;
			if (maxZ < tmpV3.z)
				maxZ = tmpV3.z;
		}
		boundsPointsList.Clear();

		nearCorners[0] = new Vector3(minX, minY, minZ);
		nearCorners[1] = new Vector3(maxX, minY, minZ);
		nearCorners[2] = new Vector3(maxX, maxY, minZ);
		nearCorners[3] = new Vector3(minX, maxY, minZ);

		farCorners[0] = new Vector3(minX, minY, maxZ);
		farCorners[1] = new Vector3(maxX, minY, maxZ);
		farCorners[2] = new Vector3(maxX, maxY, maxZ);
		farCorners[3] = new Vector3(minX, maxY, maxZ);

		Vector3 pos = nearCorners[0] + (nearCorners[2] - nearCorners[0]) * 0.5f;
		mLight.position = mLight.TransformPoint(pos);
		//dirLightCameraSplits[k].transform.rotation = dirLight.transform.rotation;

		mCamera.nearClipPlane = 0;//从0开始
		mCamera.farClipPlane = farCorners[0].z - nearCorners[0].z;

		mCamera.aspect = Vector3.Magnitude(nearCorners[0] - nearCorners[1]) / Vector3.Magnitude(nearCorners[1] - nearCorners[2]);
		mCamera.orthographicSize = Vector3.Magnitude(nearCorners[1] - nearCorners[2]) * 0.5f;
	}

	public static Vector3[] GetBoundsPoints(Transform mLight, Bounds bounds)
	{
		Vector3[] tmpBoundsPoints = new Vector3[8];
		Vector3 center = bounds.center;
		Vector3 ext = bounds.extents;

		float deltaX = Mathf.Abs(ext.x);
		float deltaY = Mathf.Abs(ext.y);
		float deltaZ = Mathf.Abs(ext.z);

		tmpBoundsPoints[0] = mLight.InverseTransformPoint(center + new Vector3(-deltaX, deltaY, -deltaZ));        // 上前左（相对于中心点）
		tmpBoundsPoints[1] = mLight.InverseTransformPoint(center + new Vector3(deltaX, deltaY, -deltaZ));         // 上前右
		tmpBoundsPoints[2] = mLight.InverseTransformPoint(center + new Vector3(deltaX, deltaY, deltaZ));          // 上后右
		tmpBoundsPoints[3] = mLight.InverseTransformPoint(center + new Vector3(-deltaX, deltaY, deltaZ));         // 上后左

		tmpBoundsPoints[4] = mLight.InverseTransformPoint(center + new Vector3(-deltaX, -deltaY, -deltaZ));       // 下前左
		tmpBoundsPoints[5] = mLight.InverseTransformPoint(center + new Vector3(deltaX, -deltaY, -deltaZ));        // 下前右
		tmpBoundsPoints[6] = mLight.InverseTransformPoint(center + new Vector3(deltaX, -deltaY, deltaZ));         // 下后右
		tmpBoundsPoints[7] = mLight.InverseTransformPoint(center + new Vector3(-deltaX, -deltaY, deltaZ));
		return tmpBoundsPoints;
	}

	public static int GetPOTSize(bool useArray, float allSize, int size)
	{
		if (useArray)
		{
			int cout = Mathf.CeilToInt(allSize / size);
			return cout * size;
		}
		else
		{
			int[] sizeValue = new int[] { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };
			for (int i = 0; i < sizeValue.Length; i++)
			{
				int currentValue = sizeValue[i];
				int nextIndex = i + 1;
				if (nextIndex >= sizeValue.Length)
				{
					return currentValue;
				}
				if (Mathf.Abs(allSize - currentValue) < Mathf.Abs(allSize - sizeValue[nextIndex]))
				{
					return currentValue;
				}
			}
			return sizeValue[0];
		}
	}

	public static Matrix4x4 GetLightProjectMatrix(Camera cam)
	{
		Matrix4x4 worldToView = cam.worldToCameraMatrix;
		Matrix4x4 projection = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
		return projection * worldToView;
	}

	public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
	{
		float x = 2.0F * near / right - left;
		float y = 2.0F * near / top - bottom;
		float a = right + left / right - left;
		float b = top + bottom / top - bottom;
		float c = -far + near / far - near;
		float d = -2.0F * far * near / far - near;
		float e = -1.0F;
		Matrix4x4 m = default(Matrix4x4);
		m[0, 0] = x;
		m[0, 1] = 0;
		m[0, 2] = a;
		m[0, 3] = 0;
		m[1, 0] = 0;
		m[1, 1] = y;
		m[1, 2] = b;
		m[1, 3] = 0;
		m[2, 0] = 0;
		m[2, 1] = 0;
		m[2, 2] = c;
		m[2, 3] = d;
		m[3, 0] = 0;
		m[3, 1] = 0;
		m[3, 2] = e;
		m[3, 3] = 0;
		return m;
	}

	public static void SetTextureFormat(Texture2D tex, int maxSize)
	{
		if (tex == null) return;
		string path = AssetDatabase.GetAssetPath(tex);
		TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;

		if (tImporter == null) return;

		tImporter.isReadable = true;
		tImporter.textureCompression = TextureImporterCompression.Compressed;

		TextureImporterPlatformSettings pcSetting = new TextureImporterPlatformSettings();
		pcSetting.overridden = true;
		pcSetting.format = TextureImporterFormat.DXT5;
		pcSetting.compressionQuality = 50;
		pcSetting.maxTextureSize = maxSize;
		pcSetting.name = "Standalone";
		TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings();
		androidSetting.overridden = true;
		androidSetting.name = "Android";
		androidSetting.format = TextureImporterFormat.ASTC_5x5;
		//androidSetting.format = TextureImporterFormat.RGBA32 ;
		androidSetting.compressionQuality = 50;
		androidSetting.maxTextureSize = maxSize;
		TextureImporterPlatformSettings iosSetting = new TextureImporterPlatformSettings();
		iosSetting.overridden = true;
		iosSetting.name = "iPhone";
		iosSetting.format = TextureImporterFormat.ASTC_5x5;
		//iosSetting.format = TextureImporterFormat.RGBA32;
		iosSetting.compressionQuality = 50;
		iosSetting.maxTextureSize = maxSize;
		tImporter.SetPlatformTextureSettings(pcSetting);
		tImporter.SetPlatformTextureSettings(androidSetting);
		tImporter.SetPlatformTextureSettings(iosSetting);
		tImporter.sRGBTexture = false;
		tImporter.filterMode = FilterMode.Bilinear;
		tImporter.SaveAndReimport();
	}

	public static void TextureToTexture2D(ref RenderTexture texture, ref Texture2D t2d, int xOffset, int yOffset)
	{
		RenderTexture currentRT = RenderTexture.active;

		RenderTexture.active = texture;
		//Debug.LogError(xOffset + "," + yOffset + ":" + t2d.width + "," + t2d.height + "," + texture.width + "," + texture.height);
		t2d.ReadPixels(new Rect(xOffset, yOffset, t2d.width, t2d.height), 0, 0);
		t2d.Apply();
		RenderTexture.active = currentRT;
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
	private StaticShadowBake mapAABB = null;
	private static bool isNeedPostResolve = false; //新树流程暂时不需要阴影模糊处理
	public Material ResolveMat;
	public string URPFeatureName = "ShadowBake";
#endif

	[System.Serializable]
	public class StaticShadowBakeResult
	{
		public bool CanUse = false;
		public Texture2DArray TexArray;
		public Texture2D Tex;
		public int XCount;
		public int YCount;
		public int Count;
		public float[] Map;
		public Matrix4x4 LightProjecionMatrix;
	}

	public bool UseTextureArray = false;
	public int PixPM = 20;
	public int SplitSize = 32;
	public int MaxTextureSize = 1024;
#if UNITY_EDITOR
	[Range(0, 1)]
	public float AlphaTestValue = 0.3f;
	public List<Transform> FromObjects;
	public List<Transform> FromObjectsExclude;
	public bool AddToObjectsBounds = true;
#endif
	
	public List<Transform> ToObjects;
	public bool SetGlobal = true;
	public bool SoftShadow = true;
	//public bool ShadowPro = false;
	[Range(0, 5)]
	public float RScale = 0.3f;
	public float Bias = 0.005f;
	[Range(0, 1.0f)]
	public float ShadowStrength = 0.9f;
	public const string ShadowUseArrayKey = "_TEXARRAY_ON";
	public const string SoftShadowKey = "_SMARTSOFTSHADOW_ON";
	public const string ShadowProKey = "_SHADOWPRO_ON";
	public const string ShadowAlohaTestKey = "_DepthOutputAlpha";

	public StaticShadowBakeResult BakeInfo;

#if UNITY_EDITOR
	public bool BakeGrassLight = false;
	public int MaxGrassLightSize = 512;
	public List<Transform> FromTerrain;
#endif

#if UNITY_EDITOR
	public void Bake()
	{
		float lastBias = QualitySettings.lodBias;
		QualitySettings.lodBias = 10;
		try
        {
			UnityEngine.Rendering.Universal.ScriptableRendererFeature feature = OpenBakeFeature(true);
			mapAABB = new StaticShadowBake(feature);
			mapAABB.SetLight(transform, FromObjects, FromObjectsExclude, ToObjects, AddToObjectsBounds);
			Shader.SetGlobalFloat(ShadowAlohaTestKey, AlphaTestValue);
			mapAABB.BakeTex(UseTextureArray, MaxTextureSize, PixPM, SplitSize, ResolveMat, ref BakeInfo);
			SetParameter(true);
			feature.SetActive(false);

			if (BakeGrassLight)
			{
				SmartGrass.BakeGrassLight(gameObject, FromTerrain, ToObjects, PixPM, MaxGrassLightSize);
			}
		}
		catch(System.Exception ex)
        {

        }
        finally
        {
			QualitySettings.lodBias = lastBias;
		}
	}
#endif

	void SetParameter(bool enable)
	{
		if (BakeInfo != null && BakeInfo.CanUse)
		{
			//if(ToObjects != null && ToObjects.Count > 0)
   //         {
			//	foreach (Transform o in ToObjects)
			//	{
			//		SetRenderParameter(o, enable);
			//	}
			//}
			//Debug.Log("SmartShadow.SetParameter:" + enable);
			SetGlobalParameter(enable);
		}
	}

	void SetGlobalParameter(bool enable)
    {
		if(SetGlobal)
        {
			if (UseTextureArray)
			{
				Shader.EnableKeyword(ShadowUseArrayKey);
				Shader.SetGlobalTexture("_TexArr", BakeInfo.TexArray);
				if (BakeInfo.Map != null && BakeInfo.Map.Length > 0)
				{
					Shader.SetGlobalFloatArray("_Map", BakeInfo.Map);
				}
			}
			else
			{
				Shader.DisableKeyword(ShadowUseArrayKey);
				Shader.SetGlobalTexture("_LightDepthTex", BakeInfo.Tex);
			}

			Shader.SetGlobalVector("_Parameter0", new Vector4(BakeInfo.XCount, ShadowStrength, RScale, Bias));
			Shader.SetGlobalMatrix("_LightProjection", enable ? BakeInfo.LightProjecionMatrix : Matrix4x4.identity);

			if (SoftShadow && enable)
				Shader.EnableKeyword(SoftShadowKey);
			else
				Shader.DisableKeyword(SoftShadowKey);
			//if (ShadowPro)
				//Shader.EnableKeyword(ShadowProKey);
			//else
				//Shader.DisableKeyword(ShadowProKey);
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
			if (mat != null)
			{
				if (UseTextureArray)
				{
					mat.EnableKeyword(ShadowUseArrayKey);
					mat.SetTexture("_TexArr", BakeInfo.TexArray);
					if (BakeInfo.Map != null && BakeInfo.Map.Length > 0)
					{
						mat.SetFloatArray("_Map", BakeInfo.Map);
					}
				}
				else
				{
					mat.DisableKeyword(ShadowUseArrayKey);
					mat.SetTexture("_LightDepthTex", BakeInfo.Tex);
				}
				
				mat.SetVector("_Parameter0", new Vector4(BakeInfo.XCount, ShadowStrength, RScale, Bias));
				mat.SetMatrix("_LightProjection", BakeInfo.LightProjecionMatrix);
				if(SoftShadow && enable)
					mat.EnableKeyword(SoftShadowKey);
				else
					mat.DisableKeyword(SoftShadowKey);
				//if(ShadowPro)
					//mat.EnableKeyword(ShadowProKey);
				//else
					//mat.DisableKeyword(ShadowProKey);
			}
		}
		foreach (Transform o in trans)
		{
			SetRenderParameter(o, enable);
		}
	}

	//void Start()
	//{
		//SetParameter(true);
	//}
	void OnAwake()
    {
#if UNITY_EDITOR
		if (ResolveMat == null)
		{
			ResolveMat = Resources.Load<Material>("CopyAndResolve");
		}
#endif
	}
    void OnEnable()
    {
		firstUpdate = true;
		Instance = this;
    }
    private bool firstUpdate = true;

	void Update()
	{
		if(firstUpdate)
        {
			firstUpdate = false;
			SetParameter(true);
		}
	}

	void OnDisable()
    {
		firstUpdate = true;
		SetParameter(false);
    }

    private void OnDestroy()
    {
		Instance = null;
    }

    public void SetEnable(bool enable)
	{
		this.enabled = enable;
	}
}

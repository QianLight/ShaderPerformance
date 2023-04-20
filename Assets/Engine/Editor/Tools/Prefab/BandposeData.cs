using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.Editor
{

    [System.Serializable]
	public class PartMaterial
	{
		public string matName;
		public Material material;
		public bool sharedMat;
		public uint id;
	}

	[System.Serializable]
	public class SfxData
	{
		public string path;
		public string attachName;
		public GameObject sfx;
		public Vector3 offset;
		public Quaternion rotation;
		public Vector3 scale = new Vector3 ();
		[System.NonSerialized]
		public Transform attach;
	}
	public class TransInfo
	{
		public bool enable;
		public bool active = true;
		public MeshMatPair mmp;
		public Material overrideMat;
		public Transform parent;
		public Transform t;
		public Vector3 pos;
		public Quaternion rot;
		public Vector3 scale;
	}

	[System.Serializable]
	public class MeshMatPair
	{
		public Material mat;
		public Mesh m;
		public int renderIndex = -1;
		public uint partMask = 0xffffffff;
		public bool isShadow = false;
		public string path;
	}
	
	[System.Serializable]
	public class ModelPartConfig : System.IComparable<ModelPartConfig>
	{
		public string partName;
		public FlagMask flags = new FlagMask{ flag = Flag_Outline | Flag_HeightGradient };
		public const uint Flag_Outline = 1 << 0;
		public const uint Flag_HeightGradient = 1 << 1;
		public const uint Flag_DeleteColor = 1 << 2;

		public static readonly ModelPartConfig Default = new ModelPartConfig();

		public int CompareTo(ModelPartConfig other)
		{
			return string.Compare(partName, other.partName, StringComparison.Ordinal);
		}
	}

	[System.Serializable]
	public class PrefabPartConfig : System.IComparable<PrefabPartConfig>
	{
		public string partName;
		public bool partEnable;
		public bool partActive;

		public int CompareTo (PrefabPartConfig other)
		{
			return partName.CompareTo (other.partName);
		}
	}

	[System.Serializable]
	public class PrefabExportConfig
	{
		public List<bool> addSfxIndex = new List<bool> ();
		public List<bool> partEnable = new List<bool> ();
		public List<bool> partActive = new List<bool> ();
		public List<Material> partMaterial = new List<Material> ();
		public string prefabName = "";
		public GameObject prefabRef;
		public GameObject runTimePrefabRef;
		public CFEngine.PrefabRes res;
		public bool folder = false;
		public int source = 0; // 0: artist  1: play design
		public bool dirty = true;
		public string editorPrefabMd5;
		public List<string> errors = new List<string>();

		public List<PrefabPartConfig> partConfigs = new List<PrefabPartConfig> ();
		public bool bUseStringIndex = false;

		public List<SfxData> sfxData = new List<SfxData> ();

		public PrefabExportConfig (BandposeData bd)
		{
			for (int i = 0; i < bd.exportMesh.Length; ++i)
			{
				partConfigs.Add (new PrefabPartConfig ()
				{
					partName = bd.exportMesh[i].m.name,
						partEnable = true,
						partActive = true,
				});
				partMaterial.Add (null);
			}

			bUseStringIndex = true;
		}
		public PrefabPartConfig FindPartConfig (string name)
		{
			for (int i = 0; i < partConfigs.Count; ++i)
				if (partConfigs[i].partName == name) return partConfigs[i];
			return null;
		}

	}

	class ModifyMeshColorJob : IJobProcessor
	{
		private Mesh mesh = null;
		private Color[] vc = null;
		private Vector2[] uv3 = null;
		private Vector4[] tangents = null;
		private Vector3[] normals = null;
		private int vertexCountPreJob = 1000;
		private int jobCount = 1;
		private Color[] newvc = null;

		public void Init (Mesh m)
		{
			mesh = m;
			if (mesh != null)
			{
				vc = mesh.colors;
				if (vc != null)
				{
					tangents = mesh.tangents;
					normals = mesh.normals;
					uv3 = mesh.uv3.Length == vc.Length?mesh.uv3 : null;
					jobCount = vc.Length / vertexCountPreJob;
					if (vc.Length % vertexCountPreJob > 0)
					{
						jobCount++;
					}
					newvc = new Color[vc.Length];
				}
			}
		}

		public void Prepare (int jobID)
		{

		}

		public void Execute (int jobID)
		{
			if (vc != null)
			{
				for (int j = jobID * vertexCountPreJob, i = 0; j < vc.Length && i < vertexCountPreJob; ++j, ++i)
				{
					ref var t = ref tangents[j];
					ref var n = ref normals[j];
					ref var color = ref vc[j];

					Vector3 tangent = Vector3.Normalize (new Vector3 (t.x, t.y, t.z));
					Vector3 normal = Vector3.Normalize (n);
					Vector3 binormal = Vector3.Cross (normal, tangent) * t.w;
					Matrix4x4 matr = new Matrix4x4 ();
					matr.m00 = tangent.x;
					matr.m01 = tangent.y;
					matr.m02 = tangent.z;
					matr.m03 = 0;
					matr.m10 = binormal.x;
					matr.m11 = binormal.y;
					matr.m12 = binormal.z;
					matr.m13 = 0;
					matr.m20 = normal.x;
					matr.m21 = normal.y;
					matr.m22 = normal.z;
					matr.m23 = 0;
					matr.m30 = 0;
					matr.m31 = 0;
					matr.m32 = 0;
					matr.m33 = 1;
					float a = color.a;
					Vector4 c = color * new Color (2, 2, 2, 1) - new Color (1, 1, 1, 0);
					c = matr * c;
					if (uv3 != null)
					{
						ref var outlineControl = ref uv3[j];
						c.w = outlineControl.x;
					}
					else
					{
						c.w = a;
					}
					newvc[j] = c;
				}
			}
		}

		public void Collection (int jobID)
		{
			// mesh.SetColors (colors);
			// mesh.uv3 = null;
		}

		public int GetJobCount ()
		{
			return jobCount;
		}
	}

	[System.Serializable]
	public class PartProcess
    {
		public string name;
		public bool folder;
		public FlagMask flag;
		public static uint Flag_UnCompress = 0x00000001;
		public static uint Flag_KeepColor = 0x00000002;
		public static uint Flag_KeepUV2 = 0x00000004;
	}
	public class PrefabData : ScriptableObject
	{

	}

	public class BandposeData : PrefabData, IJobContext
	{
		public int version = 1;
		public bool partsFolder = false;
		public GameObject editorPrefab;
		public List<PartMaterial> parts = new List<PartMaterial> ();

		[System.NonSerialized]
		public static HashSet<Material> mats = new HashSet<Material> ();
		public GameObject fbxRef;
		public bool needUV2 = false;
		public bool needOutline = false;
		public bool renameMesh = false;
		public bool sceneMesh = false;
		public bool removeAnimator = false;
		public bool testFade = false;
		public bool weightedNormal = false;
		public RuntimeAnimatorController controller;
		public bool resExportFolder = true;
		public MeshMatPair[] exportMesh;
		//public List<string> uncompressPart = new List<string> ();
		public List<PartProcess> partsProcess = new List<PartProcess>();
		public Avatar avatar;

		public HeightGradient gradient = new HeightGradient();

		public string partTag = "";

		public bool sfxFolder = false;
		public List<SfxData> sfxData = new List<SfxData> ();
		public bool exportFolder = false;
		public List<PrefabExportConfig> exportConfig = new List<PrefabExportConfig> ();
		public Vector3 meshRot = Vector3.zero;
		public List<string> facemapParts = new List<string>();
		public List<string> cloakParts = new List<string>();
		public List<ModelPartConfig> modelPartConfigs = new List<ModelPartConfig>();
		
        private ModifyMeshColorJob meshJob = new ModifyMeshColorJob ();

		public bool EditorPrefabFolder = false;
		public bool ArtistFolder = false;
		public bool DesignerFolder = false;
		public bool PlayFolder = false;

		private void CreateParts (Material mat)
		{
			if (mat != null)
			{
				mats.Add (mat);
			}
		}

		public void RefreshMaterial (bool createmat = true)
		{
			if (fbxRef != null)
			{
				string assetPath;
				string dir = AssetsPath.GetAssetDir (fbxRef, out assetPath);
				string matFolderName = "Material_" + fbxRef.name;
				string matPath = string.Format ("{0}/{1}", dir, matFolderName);
				if (!AssetDatabase.IsValidFolder (matPath))
					AssetDatabase.CreateFolder (dir, matFolderName);
				mats.Clear ();
				parts.Clear ();
				ModelImporter modelImporter = AssetImporter.GetAtPath (assetPath) as ModelImporter;
				if (modelImporter != null)
				{
					modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
					modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
					MaterialShaderAssets.ExtractMaterialsFromAsset (modelImporter, matPath, createmat, CreateParts);
					AssetDatabase.ImportAsset (assetPath, ImportAssetOptions.ForceUpdate);

					var renders = EditorCommon.GetRenderers (fbxRef);
					for (int i = 0; i < renders.Count; ++i)
					{
						var r = renders[i];
						Material mat = r.sharedMaterial; 
						if (mat)
						{
							Color gradientColor = gradient.enable ? gradient.color : Color.white;
							mat.SetColor("_HeightGradientColor", gradientColor);
							EditorUtility.SetDirty(mat);
						}
						parts.Add (new PartMaterial ()
						{
							matName = r.name,
								material = r.sharedMaterial,
						});
					}
				}
			}
		}

		private List<MeshRenderPair> meshList = new List<MeshRenderPair> ();

		void MeshPreExport(Mesh m, Renderer render, ref FBXAssets.ExportContext context)
		{
            string name = m.name.ToLower();
            var pp = partsProcess.Find((x) => x.name == name);
			if (pp != null)
			{
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_KeepUV2,
					pp.flag.HasFlag(PartProcess.Flag_KeepUV2));
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_KeepColor,
					pp.flag.HasFlag(PartProcess.Flag_KeepColor));
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_UnCompress,
					pp.flag.HasFlag(PartProcess.Flag_UnCompress));
			}
			else
            {
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_KeepUV2,
					context.flag.HasFlag(PartProcess.Flag_KeepUV2));
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_KeepColor,
					context.flag.HasFlag(PartProcess.Flag_KeepColor));
                context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_UnCompress,
					context.flag.HasFlag(PartProcess.Flag_UnCompress));
            }
			context.flagOverride.SetFlag(FBXAssets.ExportContext.Flag_KeepUV3,
					context.flag.HasFlag(FBXAssets.ExportContext.Flag_KeepUV3));
        }
		
		bool MeshExport (Mesh m, Renderer render, bool post, BandposeData bandpose)
		{
			if (post)
			{
				meshList.Add (new MeshRenderPair ()
				{
					r = render,
						m = m,
				});
				return false;
			}
			else
			{
				colors.Clear ();
				ModifyMesh (m, bandpose);
				return true;
			}
		}

		public void Export (string partName = "")
		{
			if (fbxRef != null)
			{
				if (fbxRef.transform.childCount == 0)
					meshRot = new Vector3 (-90, 0, 0);

				if (sceneMesh)
				{
					var renders = EditorCommon.GetRenderers (fbxRef);
					exportMesh = new MeshMatPair[renders.Count];
					for (int i = 0; i < renders.Count; ++i)
					{
						var render = renders[i];
						string meshName = string.Format ("{0}/{1}_{2}.asset", LoadMgr.singleton.editorResPath, fbxRef.name, i.ToString ());
						var mm = new MeshMatPair ();
						mm.renderIndex = render.transform.GetSiblingIndex ();
						mm.mat = render.sharedMaterial;
						mm.m = AssetDatabase.LoadAssetAtPath<Mesh> (meshName);
						exportMesh[i] = mm;
					}
				}
				else
				{

					string assetPath;
                    meshList.Clear();
                    var ec = new FBXAssets.ExportContext();
					string dir = AssetsPath.GetAssetDir(fbxRef, out assetPath);
					ec.exportDir = dir;
					ec.baseName = fbxRef.name;
                    ec.exportCb = MeshExport;
					ec.preExportCb = MeshPreExport;
					ec.partFliter = partName;
					ec.flag.SetFlag(FBXAssets.ExportContext.Flag_KeepUV2, needUV2);
                    ec.flag.SetFlag(FBXAssets.ExportContext.Flag_KeepColor, needOutline);
                    ec.flag.SetFlag(FBXAssets.ExportContext.Flag_KeepUV3, true);
					ec.flag.SetFlag(FBXAssets.ExportContext.Flag_KeepUV4, true);
					ec.renameMesh = renameMesh;
					ec.bandpose = this;
                    FBXAssets.ExportMesh(fbxRef, null, ref ec, EditorMessageType.Dialog);

					// meshList.Sort ((x, y) => x.sortIndex.CompareTo (y.sortIndex));
					var pi = PartConfig.instance.GetPartInfo (partTag);
					if (string.IsNullOrEmpty (partName))
					{
						exportMesh = new MeshMatPair[meshList.Count];
						var t = fbxRef.transform;
						for (int j = 0; j < meshList.Count; ++j)
						{
							var mr = meshList[j];
							var mm = new MeshMatPair ();
							// mm.bd = mr.bd;
							mm.mat = mr.r.sharedMaterial;
							mm.m = mr.m;
							mm.partMask = PartConfig.commonMask;
							string n = mm.m.name.ToLower ();
							mm.isShadow = n.Contains("_sd_") || n.EndsWith("_sd");
							for (int i = 0; i < pi.parts.Length; ++i)
							{
								var s = pi.parts[i];
								if (n.EndsWith (s))
								{
									mm.partMask = (uint) pi.partsFlags[i];
								}
							}
							var r = mr.r.transform;
							if (r.IsChildOf (t))
							{
								mm.renderIndex = r.GetSiblingIndex ();
							}
							else
							{
								mm.path = EditorCommon.GetSceneObjectPath (r, false);
							}

							if (mm.isShadow)
							{
								mm.partMask |= RendererInstance.Part_Shadow;
							}
							
							exportMesh[j] = mm;
						}
						string name = fbxRef.name;
						if (name.ToLower ().EndsWith ("_bandpose"))
						{
							int index = name.LastIndexOf ("_");
							if (index >= 0)
								name = name.Substring (0, index);
						}
						name += "_avatar";
						avatar = FBXAssets.ExportAvatar (fbxRef, dir, name);
						FBXAssets.ResetAttribute ();

						ResizeExistingPrefabConfig ();
					}
				}

				SetAllPrefabDirty (true);
				EditorCommon.SaveAsset (this);
			}
		}

		public bool IsExportCorrectly()
        {
			if (exportMesh == null || exportMesh.Length == 0) return false;

			for (int i = 0; i < exportMesh.Length; ++i)
				if (exportMesh[i].m == null) return false;

			return true;

		}
		public void UpdateMask ()
		{
			var pi = PartConfig.instance.GetPartInfo (partTag);
			if (exportMesh != null)
			{
				for (int j = 0; j < exportMesh.Length; ++j)
				{
					var mm = exportMesh[j];
					mm.partMask = PartConfig.commonMask;
					if (mm.m != null)
					{
						string n = mm.m.name.ToLower ();
						mm.isShadow = n.Contains("_sd_") || n.EndsWith("_sd");
						for (int i = 0; i < pi.parts.Length; ++i)
						{
							var s = pi.parts[i];
							if (n.EndsWith (s))
							{
								mm.partMask = (uint) pi.partsFlags[i];
							}
						}
					}
					
					if (mm.isShadow)
					{
						mm.partMask |= RendererInstance.Part_Shadow;
					}
				}
				EditorCommon.SaveAsset (this);
			}

		}
		private void ResizeExistingPrefabConfig ()
		{
			for (int i = 0; i < exportConfig.Count; ++i)
			{
				var OldPrefabConfig = exportConfig[i];

				_ResizePartConfig (ref OldPrefabConfig);
			}
		}

		public void _ResizePartConfig (ref PrefabExportConfig prefabConfig)
		{
			for (int i = prefabConfig.partConfigs.Count - 1; i >= 0; i--)
			{
				if (!IsValidExportMesh (prefabConfig.partConfigs[i].partName))
					prefabConfig.partConfigs.RemoveAt (i);
			}

			for (int i = 0; i < exportMesh.Length; ++i)
			{
				if (prefabConfig.FindPartConfig (exportMesh[i].m.name) != null) continue;

				prefabConfig.partConfigs.Add (new PrefabPartConfig ()
				{
					partName = exportMesh[i].m.name,
						partEnable = false,
						partActive = false,
				});

				prefabConfig.partMaterial.Add (null);

			}
		}

		private bool IsValidExportMesh (string name)
		{
			for (int i = 0; i < exportMesh.Length; ++i)
				if (exportMesh[i].m.name == name) return true;
			return false;
		}

		private List<Color> colors = new List<Color> ();

        // Raw data:
        //      Legacy:
        //          uv1     : main uv(xy)
        //          uv2     : 石化均匀uv
        //          uv3     : outline width(x)
        //          uv4     : 披风flag(x)
        //          uv5     : dirty uv(xy)

        // Porcessed data:
        //      Legacy:
        //          color   : outline dir, outline width
        //          
        //      New:
        //          color   : scaled outline dir(rgb) 上下渐变factor(a)
        //          uv1     : main uv(xy), dirty uv
        //          uv2     : 石化uv(xy), 披风flag(z)

        public void ModifyMesh(Mesh mesh, BandposeData bandpose)
		{
			if (mesh != null)
			{
				#region Fetching mesh data.
				int vertexCount = mesh.vertexCount;
				var positions = mesh.vertices;
				var tangents = mesh.tangents;
				var normals = mesh.normals;
				var triangles = mesh.triangles;

				Color[] rawColors = mesh.HasVertexAttribute(VertexAttribute.Color) ? mesh.colors : null;
				Vector2[] mainUVs = mesh.HasVertexAttribute(VertexAttribute.TexCoord0) ? mesh.uv : null;
				Vector2[] stoneUVs = mesh.HasVertexAttribute(VertexAttribute.TexCoord1) ? mesh.uv2 : null;
				Vector2[] rawUv3s = mesh.HasVertexAttribute(VertexAttribute.TexCoord2) ? mesh.uv3 : null;
				Vector2[] rawUV4s = mesh.HasVertexAttribute(VertexAttribute.TexCoord3) ? mesh.uv4 : null;
				Vector2[] dirtyUVs = mesh.HasVertexAttribute(VertexAttribute.TexCoord4) ? mesh.uv5 : null;

				string meshName = mesh.name;
				bool facemap = bandpose.facemapParts.Contains(meshName);
				// bool cloakmap = bandpose.cloakParts.Contains(meshName);
				
				#endregion
				
				#region Recombine vertex data.

				#region COLOR : Outline vector + gradient factor

				if (needOutline && FindModelPartConfig(meshName, out ModelPartConfig mpc))
				{
					if (mpc.flags.HasFlag(ModelPartConfig.Flag_DeleteColor))
					{
						mesh.colors = null;
					}
					else
					{
						Vector3[] outlineVecs = mpc != null && !mpc.flags.HasFlag(ModelPartConfig.Flag_Outline)
							? null 
							: weightedNormal
								? CalculateOutlineWeightedVectors(mesh, rawUv3s, rawColors) 
								: CalculateOutlineVectors(vertexCount, positions, triangles, tangents, normals, rawUv3s, rawColors);

						float[] gradientFactors = CalculateGradients(mpc, vertexCount, positions);

						if (outlineVecs != null || gradientFactors != null)
						{
							Color[] newColors = new Color[vertexCount];
							for (int index = 0; index < vertexCount; ++index)
							{
								ref Color color = ref newColors[index];

								if (outlineVecs != null)
								{
									ref Vector3 outlineVec = ref outlineVecs[index];
									color.r = outlineVec.x;
									color.g = outlineVec.y;
									color.b = outlineVec.z;
								}
								else if (rawColors != null && rawColors.Length > 0)
								{
									ref Color rawColor = ref rawColors[index];
									color.r = rawColor.r;
									color.g = rawColor.g;
									color.b = rawColor.b;
								}
								else
								{
									color.r = 1;
									color.g = 1;
									color.b = 1;
								}

								float gradientFactor = gradientFactors == null ? 0 : gradientFactors[index];
								color.a = gradientFactor;
							}

							mesh.SetColors(newColors);
						}

					}
				}
				#endregion

				#region TEXCOORD0 : Main uv + dirty uv
				if (dirtyUVs != null)
                {
					Vector4[] outUV1Array = new Vector4[vertexCount];
                    for (int i = 0; i < vertexCount; i++)
                    {
						ref Vector2 mainUV = ref mainUVs[i];
                        ref Vector2 dirtyUV = ref dirtyUVs[i];
						ref Vector4 outUV1 = ref outUV1Array[i];
						outUV1.x = mainUV.x;
						outUV1.y = mainUV.y;
						outUV1.z = dirtyUV.x;
						outUV1.w = dirtyUV.y;
					}
					mesh.SetUVs(0, outUV1Array);
				}
				#endregion

				#region TEXCOORD1 : Stone uv + cloak flag
				
				if (rawUV4s != null)
				{
					if (facemap)
					{
						Vector4[] outUV2Array = new Vector4[vertexCount];
						for (int i = 0; i < vertexCount; i++)
						{
							ref Vector2 stoneUV = ref stoneUVs[i];
							ref Vector2 facemapUV = ref rawUV4s[i];
							ref Vector4 outUV2 = ref outUV2Array[i];
							outUV2.x = stoneUV.x;
							outUV2.y = stoneUV.y;
							outUV2.z = facemapUV.x;
							outUV2.w = facemapUV.y;
						}
						mesh.SetUVs(1, outUV2Array);
					}
					else
					{
						// cloak
						Vector3[] outUV2Array = new Vector3[vertexCount];
						for (int i = 0; i < vertexCount; i++)
						{
							ref Vector2 stoneUV = ref stoneUVs[i];
							ref float cloakFlag = ref rawUV4s[i].x;
							ref Vector3 outUV2 = ref outUV2Array[i];
							outUV2.x = stoneUV.x;
							outUV2.y = stoneUV.y;
							outUV2.z = cloakFlag;
						}
						mesh.SetUVs(1, outUV2Array);
					}
				}
                #endregion

                #region Clear unused uvs
				
				if( mesh.name.Contains("_sd_"))
				{
					for (int i = 0; i < 8; i++)
					{
						mesh.SetUVs(i, (Vector4[])null);
					}
					mesh.colors = null;
					mesh.tangents = null;
					mesh.normals = null;
				}
				else
				{
					for (int i = 2; i < 8; i++)
					{
						mesh.SetUVs(i, (Vector4[])null);
					}
				}
                #endregion

                #endregion
            }
        }

        private float[] CalculateGradients(ModelPartConfig mpc, int vertexCount, Vector3[] positions)
        {
	        if (mpc != null && !mpc.flags.HasFlag(ModelPartConfig.Flag_HeightGradient))
	        {
		        return null;
	        }
	        
	        float[] gradientFactors = null;
	        if (gradient.enable)
	        {
		        gradientFactors = new float[vertexCount];
		        for (int index = 0; index < vertexCount; index++)
		        {
			        float t = Mathf.InverseLerp(gradient.topHeight, gradient.bottomHeight, positions[index].z);
			        t = Mathf.Clamp01(t);
			        t = Mathf.Pow(t, gradient.fade);
			        gradientFactors[index] = t;
		        }
	        }

	        return gradientFactors;
        }

        public static Vector3[] CalculateOutlineVectors(int vertexCount, Vector3[] positions, int[] triangles,
	        Vector4[] tangents, Vector3[] normals, Vector2[] rawUv3s, Color[] rawColors)
        {
	        #region Calculate outline direction from triangle.

	        #region Collect position-index map.

	        // Group vertex indexes by vertex position.
	        // Note that vertices that is very close will not be collected into one group.
	        Dictionary<Vector3, List<int>> positionIndexMap = new Dictionary<Vector3, List<int>>();
	        for (int index = 0; index < vertexCount; index++)
	        {
		        Vector3 position = positions[index];
		        if (!positionIndexMap.TryGetValue(position, out List<int> indexList))
		        {
			        indexList = new List<int>();
			        positionIndexMap[position] = indexList;
		        }

		        indexList.Add(index);
	        }

	        #endregion

	        #region Calculate and collect triangle-normal map.

	        // Calculate normal for each triangle.
	        Vector3[] triangleNormalMap = new Vector3[triangles.Length / 3];
	        for (int triangleIndex = 0; triangleIndex < triangles.Length / 3; triangleIndex++)
	        {
		        Vector3 v0 = positions[triangles[triangleIndex * 3 + 0]];
		        Vector3 v1 = positions[triangles[triangleIndex * 3 + 1]];
		        Vector3 v2 = positions[triangles[triangleIndex * 3 + 2]];
		        Vector3 normal = Vector3.Cross((v1 - v0).normalized, (v2 - v0).normalized);
		        triangleNormalMap[triangleIndex] = normal;
	        }

	        #endregion

	        #region Collect vertexIndex-triangleIndex map as list.

	        // Collect triangle index for each vertex index, so we can collect position in next step.
	        List<int>[] indexMap = new List<int>[vertexCount];
	        for (int triangleIndex = 0; triangleIndex < triangles.Length; triangleIndex++)
	        {
		        int vertexIndex = triangles[triangleIndex];
		        ref List<int> vertexTriangles = ref indexMap[vertexIndex];
		        if (vertexTriangles == null)
			        vertexTriangles = new List<int>();
		        vertexTriangles.Add(triangleIndex / 3);
	        }

	        #endregion

	        #region Calculate position-vertexNormalOS map.

	        // Calculate combined object space normal for position.
	        Dictionary<Vector3, Vector3> positionCombinedNormalOSMap = new Dictionary<Vector3, Vector3>();
	        HashSet<int> triangleSet = new HashSet<int>();
	        foreach (KeyValuePair<Vector3, List<int>> pair in positionIndexMap)
	        {
		        Vector3 position = pair.Key;
		        List<int> positionIndexList = pair.Value;
		        Vector3 sum = default;
		        triangleSet.Clear();
		        foreach (int vertexIndex in positionIndexList)
		        {
			        foreach (int vertexTriangleIndex in indexMap[vertexIndex])
			        {
				        if (triangleSet.Add(vertexTriangleIndex))
				        {
					        sum += triangleNormalMap[vertexTriangleIndex];
				        }
			        }
		        }

		        Vector3 normal = sum.normalized;
		        positionCombinedNormalOSMap.Add(position, normal);
	        }

	        #endregion

	        #endregion

	        #region Transform object space outlineVector to tangent space for each vertex index.

	        // This step can not be combined with last step, becouse 
	        // different vertex has different normal, tangent and binormal.
	        Vector3[] outlineVecs = new Vector3[vertexCount];
	        for (int index = 0; index < vertexCount; index++)
	        {
		        ref var p = ref positions[index];
		        ref var t = ref tangents[index];
		        ref var n = ref normals[index];

		        float scale;
		        if (rawUv3s != null)
			        scale = rawUv3s[index].x;
		        else if (rawColors != null)
			        scale = rawColors[index].a;
		        else
			        scale = 1;
		        scale = Mathf.Clamp01(scale);

		        Vector3 tangent = Vector3.Normalize(new Vector3(t.x, t.y, t.z));
		        Vector3 normal = Vector3.Normalize(n);
		        Vector3 binormal = Vector3.Cross(normal, tangent) * t.w;

		        Vector3 outlineDirOS = positionCombinedNormalOSMap[p];
		        Vector3 outlineVecOS = outlineDirOS;
		        Vector3 outlineVecTS = new Vector3(
			        Vector3.Dot(tangent, outlineVecOS),
			        Vector3.Dot(binormal, outlineVecOS),
			        Vector3.Dot(normal, outlineVecOS)
		        );

		        outlineVecs[index] = outlineVecTS.normalized * scale;
	        }

	        #endregion

	        return outlineVecs;
        }
        
		public static Vector3[] CalculateOutlineWeightedVectors(Mesh mesh, 
	         Vector2[] rawUv3s, Color[] rawColors)
		{

			int vertexCount = mesh.vertexCount;
			Vector3[] positions = mesh.vertices;
			Vector4[] tangents = mesh.tangents;
			Vector3[] normals = mesh.normals;
			
			NormalSmoothModifier.ModifyNormals(mesh,0.01f,0.5f,0,out Vector3[] outlineVecs);
			
	        // This step can not be combined with last step, becouse 
	        // different vertex has different normal, tangent and binormal.
	        for (int index = 0; index < vertexCount; index++)
	        {
		        ref var t = ref tangents[index];
		        ref var n = ref normals[index];

		        float scale;
		        if (rawUv3s != null)
			        scale = rawUv3s[index].x;
		        else if (rawColors != null)
			        scale = rawColors[index].a;
		        else
			        scale = 1;
		        scale = Mathf.Clamp01(scale);

		        Vector3 tangent = Vector3.Normalize(new Vector3(t.x, t.y, t.z));
		        Vector3 normal = Vector3.Normalize(n);
		        Vector3 binormal = Vector3.Cross(normal, tangent) * t.w;

		        // Vector3 outlineDirOS = positionCombinedNormalOSMap[p];
		        Vector3 outlineDirOS = outlineVecs[index];
		        Vector3 outlineVecOS = outlineDirOS;
		        Vector3 outlineVecTS = new Vector3(
			        Vector3.Dot(tangent, outlineVecOS),
			        Vector3.Dot(binormal, outlineVecOS),
			        Vector3.Dot(normal, outlineVecOS)
		        );

		        outlineVecs[index] = outlineVecTS.normalized * scale;
	        }
	        return outlineVecs;
        }

        // public void ModifyMesh (GameObject go)
		// {
		// 	if (needOutline)
		// 	{
		// 		var render = EditorCommon.GetRenderers (go);

		// 		for (int i = 0; i < render.Count; ++i)
		// 		{
		// 			colors.Clear ();
		// 			var r = render[i];
		// 			Mesh mesh = null;
		// 			if (r is SkinnedMeshRenderer)
		// 			{
		// 				var smr = r as SkinnedMeshRenderer;
		// 				mesh = smr.sharedMesh;
		// 			}
		// 			else if (r is MeshRenderer)
		// 			{
		// 				MeshFilter mf;
		// 				if (r.gameObject.TryGetComponent (out mf))
		// 				{
		// 					mesh = mf.sharedMesh;
		// 				}
		// 			}
		// 			ModifyMesh (mesh);

		// 		}
		// 	}
		// }

		//public void MakeExportConfig (GameObject prefab)
		//{
		//	bool find = false;
		//	string prefabName = prefab.name;
		//	if (controller == null)
		//	{
		//		Animator animator;
		//		if (prefab.TryGetComponent (out animator))
		//		{
		//			controller = animator.runtimeAnimatorController;
		//		}
		//	}
		//	for (int i = 0; i < exportConfig.Count; ++i)
		//	{
		//		if (exportConfig[i].prefabName == prefabName)
		//		{
		//			exportConfig[i].prefabRef = prefab;
		//			find = true;
		//			break;
		//		}
		//	}
		//	if (!find)
		//	{
		//		var ec = new PrefabExportConfig ()
		//		{
		//			prefabName = prefabName,
		//				prefabRef = prefab
		//		};
		//		exportConfig.Add (ec);
		//		for (int i = 0; i < parts.Count; ++i)
		//		{
		//			ec.partEnable.Add (true);
		//		}
		//	}

		//	EditorCommon.SaveAsset (this);
		//}
		private void LoadRefSkinMesh (Transform t, SkinnedMeshRenderer smr, BandposeData bd)
		{
			if (bd.fbxRef != null)
			{
				var refRenders = EditorCommon.GetRenderers (bd.fbxRef);

				for (int i = 0; i < refRenders.Count; ++i)
				{
					var r = refRenders[i] as SkinnedMeshRenderer;
					if (r != null && r.name == smr.name)
					{
						//find ref mesh
						var bones = r.bones;
						if (bones.Length == smr.bones.Length)
						{
							var newBones = new Transform[bones.Length];
							for (int j = 0; j < bones.Length; ++j)
							{
								string path = EditorCommon.GetSceneObjectPath (bones[j], false);
								newBones[j] = t.Find (path);
							}
							smr.bones = newBones;
							smr.sharedMesh = null;
							if (bd.exportMesh != null)
							{
								for (int j = 0; j < bd.exportMesh.Length; ++j)
								{
									var em = bd.exportMesh[j];
									if (em.m != null && em.m.name == smr.name)
									{
										smr.sharedMesh = em.m;
										break;
									}
								}
							}

							DebugLog.AddEngineLog2 ("replace bones:{0} fbx:{1}", smr.name, bd.fbxRef.name);
						}
						else
						{
							DebugLog.AddErrorLog2 ("bone count not same:{0} fbx:{1}", smr.name, bd.fbxRef.name);
						}

						break;
					}
				}
			}
		}

		public void SetAllPrefabDirty (bool dirty)
		{
			for (int i = 0; i < exportConfig.Count; ++i)
			{
				exportConfig[i].dirty = dirty;
			}
		}
		public void MakePrefab (int index = 0, int source = -1, bool makePrefabs = true, bool quiet = false, bool batch = false)
		{
			if (exportMesh != null && exportMesh.Length > 0)
			{
				if (index >= 0 && index < exportConfig.Count)
				{
					var ec = exportConfig[index];
					_MakePrefab (ec, makePrefabs, quiet);
				}
				else
				{
					bool flag = true;
					for (int i = 0; i < exportConfig.Count; ++i)
					{
						if (source >= 0)
						{
							if (exportConfig[i].source == source)
								flag &= _MakePrefab (exportConfig[i], makePrefabs, quiet);
						}
						else
						{
							flag &= _MakePrefab (exportConfig[i], makePrefabs, quiet);
						}
					}

					if (batch)
					{
						if (flag)
							Debug.Log($"Prefab生成成功 ({name})");
						else
							Debug.LogError($"Prefab生成失败，看红字 ({name})");
					}
					else
					{
						if (flag)
							EditorUtility.DisplayDialog ("Make All Prefab", $"Prefab生成成功 ({name})", "OK");
						else
							EditorUtility.DisplayDialog ("Make All Prefab", $"Prefab生成失败，看红字 ({name})", "OK");
					}
				}
				EditorCommon.SaveAsset (this);
			}

		}

		public static string GetMD5HashFromFile(string fileName)
		{
		    try
		    {
		        FileStream file = new FileStream(fileName, FileMode.Open);
		        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		        byte[] retVal = md5.ComputeHash(file);
		        file.Close();
		
		        StringBuilder sb = new StringBuilder();
		        for (int i = 0; i < retVal.Length; i++)
		        {
		            sb.Append(retVal[i].ToString("x2"));
		        }
		        return sb.ToString();
		    }
		    catch (Exception ex)
		    {
		        throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
		    }
		}

		private static void CopyComponents<T>(GameObject from, GameObject to, List<string> result) where T : Component
        {
			T[] srcs = from.GetComponentsInChildren<T>(true);
            foreach (T src in srcs)
            {
				string path = EditorCommon.GetSceneObjectPath(src.transform, false);
				UnityEditorInternal.ComponentUtility.CopyComponent(src);
				Transform toTransform = to.transform.Find(path);
                if (toTransform)
                {
                    if (!toTransform.GetComponent<T>())
                    {
						UnityEditorInternal.ComponentUtility.PasteComponentAsNew(toTransform.gameObject);
                    }
                }
                else
                {
					result.Add($"找不到目标节点\"{path}\"");
					continue;
                }
			}
		}
		
		private static T FindMatchedComponent<T>(T source, GameObject target) where T : Component
        {
			string path = EditorCommon.GetSceneObjectPath(source.transform, false);
			Transform transform = target.transform.Find(path);
			if (transform == null)
			{
				Debug.LogError("找不到目标节点 :" + path);
			}
			return transform ? transform.GetComponent<T>() : null;
        }

		private static void FindMatchedComponents<T>(IList<T> src, IList<T> dst, GameObject go) where T : Component
		{
			for (int i = 0; i < src.Count; i++)
				dst[i] = FindMatchedComponent(src[i], go);
			for (int i = 0; i < dst.Count; i++)
				if (!dst[i] || !dst[i].transform.IsChildOf(go.transform))
					dst.RemoveAt(i--);
		}
		
		private void LogDynamicBoneError(PrefabExportConfig ec)
        {
			if (ec.errors.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string error in ec.errors)
				{
					stringBuilder.AppendLine(error);
				}
				string errorContent = stringBuilder.ToString();
				EditorUtility.DisplayDialog("导出完成了但是有些问题，具体看Log。", errorContent, "OK");
				Debug.LogError(errorContent);
			}
		}

		private bool _MakePrefab (PrefabExportConfig ec, bool makePrefabs, bool quiet)
		{
			// if (index < exportConfig.Count && exportMesh != null && exportMesh.Length > 0)
			// {
			// 	var ec = exportConfig[index];

			if (!ec.bUseStringIndex) ConvertPrefabSaveData (ec);

			AssetDatabase.SaveAssets();
			string md5 = editorPrefab ? GetMD5HashFromFile(AssetDatabase.GetAssetPath(editorPrefab)) : string.Empty;
			ec.dirty |= md5 != ec.editorPrefabMd5;

			if (!ec.dirty)
			{
				LogDynamicBoneError(ec);
				return true;
			}
			string name = ec.prefabName;
			if (string.IsNullOrEmpty (name))
			{
				DebugLog.AddErrorLog ("prefab name empty!");
				return false;
			}
			string targetPath = ec.prefabRef ? AssetDatabase.GetAssetPath(ec.prefabRef) : string.Format ("{0}/Prefabs/{1}.prefab", AssetsConfig.instance.ResourcePath, name);
			if (quiet || EditorUtility.DisplayDialog ("MakePrefab", "Is Make Prefab：" + targetPath, "OK", "Cancel"))
			{
				var go = Instantiate(fbxRef);
				go.name = name;

				MeshRenderer mr;
				if (go.TryGetComponent (out mr))
				{
					var newGo = new GameObject (go.name);
					go.transform.parent = newGo.transform;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.Euler (meshRot);
					go.transform.localScale = Vector3.one;
					go = newGo;
				}
				else
				{

					if (go.TryGetComponent (out Animator ator))
					{
						if (removeAnimator)
						{
                            DestroyImmediate(ator);
						}
						else
						{
							ator.runtimeAnimatorController = controller;
						}

					}
				}

                Transform t = go.transform;

				var parts = new List<TransInfo> ();
				for (int i = 0; i < exportMesh.Length; ++i)
				{
					var mm = exportMesh[i];
					PrefabPartConfig ppc = ec.FindPartConfig (mm.m.name);

					var isEnable = ppc.partEnable;
					var isActive = ppc.partActive;
					var mat = ec.partMaterial[i];
					if (mm.renderIndex >= 0)
					{
						if (mm.renderIndex < t.childCount)
						{
							var child = t.GetChild (mm.renderIndex);
							TransInfo ti = new TransInfo ()
							{
								mmp = mm,
									overrideMat = mat,
									parent = t,
									t = child,
									pos = child.localPosition,
									rot = child.localRotation,
									scale = child.localScale,
									enable = isEnable,
									active = isActive,
							};
							parts.Add (ti);
						}
						else
						{
							DebugLog.AddErrorLog2 ("not find partIndex:{0} {1}", mm.renderIndex.ToString (), this.name);
						}

					}
					else if (!string.IsNullOrEmpty (mm.path))
					{
						var child = t.Find (mm.path);
						if (child != null)
						{
							TransInfo ti = new TransInfo ()
							{
								mmp = mm,
									parent = child.parent,
									t = child,
									pos = child.localPosition,
									rot = child.localRotation,
									scale = child.localScale,
									enable = isEnable,
									active = isActive,
							};
							parts.Add (ti);
						}
						else
						{
							DebugLog.AddErrorLog2 ("not find partPath:{0} {1}", mm.path, this.name);
						}
					}
					else
					{
						DebugLog.AddErrorLog2 ("not find part:{0} {1}", i.ToString (), this.name);
					}
				}

				var renders = EditorCommon.GetRenderers (go);
				for (int i = 0; i < renders.Count; ++i)
				{
					renders[i].transform.parent = null;
				}
				int realIndex = 0;
				List<EditorMeshInfo> eMeshInfo = new List<EditorMeshInfo> ();
				for (int i = 0; i < parts.Count; ++i)
				{
					var part = parts[i];
					if (part.enable)
					{
						part.t.parent = part.parent;
						part.t.localPosition = part.pos;
						part.t.localRotation = part.rot;
						part.t.localScale = part.scale;
						MeshFilter mf;
						Material mat = null;
						if (part.t.TryGetComponent (out SkinnedMeshRenderer smr))
						{
							smr.sharedMesh = part.mmp.m;
							if (!smr.sharedMesh)
								Debug.LogError($"sharedMesh is null, Renderer = {smr}");
							smr.sharedMaterial = part.overrideMat != null?part.overrideMat : part.mmp.mat;
							mat = smr.sharedMaterial;
							smr.enabled = part.active;
						}
						else if (part.t.TryGetComponent (out mf))
						{
							mf.sharedMesh = part.mmp.m;
							if (!mf.sharedMesh)
								Debug.LogError($"sharedMesh is null, Renderer = {mf}");
							if (part.t.TryGetComponent (out MeshRenderer r))
							{
								r.sharedMaterial = part.overrideMat != null?part.overrideMat : part.mmp.mat;
								mat = r.sharedMaterial;
								r.enabled = part.active;
							}

						}
						if (part.parent == t)
							part.t.SetSiblingIndex (realIndex++);
                        string meshPath = AssetsPath.GetAssetFullPath(part.mmp.m, out var ext);
                        //DebugLog.AddEngineLog2 (meshPath);
                        uint matHash = 0;
                        if (mat != null)
                        {
                            MaterialContext context = new MaterialContext();
                            MaterialShaderAssets.CalcMatHash(mat, ref context);
                            matHash = context.matHash;
                        }

                        FlagMask flag = new FlagMask();
                        flag.SetFlag(RendererInstance.Flag_IsShadow, part.mmp.isShadow);
                        EditorMeshInfo emi = new EditorMeshInfo()
                        {
	                        meshPath = meshPath,
	                        isSkin = smr != null,
	                        partMask = part.mmp.partMask,
	                        matPath = mat != null ? AssetDatabase.GetAssetPath(mat) : "",
	                        active = part.active,
	                        matHash = matHash,
	                        flag = flag,
                        };
                        eMeshInfo.Add(emi);
                    }
                    else
					{
						GameObject.DestroyImmediate (part.t.gameObject);
					}

				}

				for (int i = 0; i < ec.sfxData.Count; ++i)
				{
					//if (i < ec.addSfxIndex.Count && ec.addSfxIndex[i])
					{
						var sfx = ec.sfxData[i];
						var attach = t;
						if (!string.IsNullOrEmpty (sfx.path))
						{
							attach = go.transform.Find (sfx.path);
						}

						if (attach != null)
						{
							var sfxTrans = new GameObject ("sfx").transform;
							sfxTrans.parent = attach;
							sfxTrans.localPosition = sfx.offset;
							sfxTrans.localRotation = sfx.rotation;
							sfxTrans.localScale = sfx.scale;
							sfx.attach = sfxTrans;
						}

					}
				}

				#region editor prefabs

				if (editorPrefab)
                {
					bool modified = false;

					#region EditorPrefabFlager
					
					List<EditorPrefabFlager> flagers = EditorPrefabFlager.Get(editorPrefab, EditorPrefabFlagEnum.ForceCopyGameObject);
					foreach (EditorPrefabFlager flager in flagers)
					{
						string path = EditorCommon.GetSceneObjectPath(flager.transform, false);
						string parentPath = path.Substring(0, path.LastIndexOf('/'));
						Transform parentTransform = go.transform.Find(parentPath);
						if (parentTransform)
						{
							GameObject copy = Instantiate(flager.gameObject);
							copy.transform.parent = parentTransform;
							copy.transform.localPosition = flager.transform.localPosition;
							copy.transform.localRotation = flager.transform.localRotation;
							copy.transform.localScale = flager.transform.localScale;
							DestroyImmediate(copy.GetComponent<EditorPrefabFlager>());
							copy.name = flager.gameObject.name;
							modified = true;
						}
						else
						{
							string errorContent = $"标记需要创建的GameObject找不到对应的父节点，路径={path}";
							EditorUtility.DisplayDialog("拷贝节点出错", errorContent, "OK");
							Debug.LogError(errorContent);
						}
					}

                    #endregion

                    #region dynamic bone

                    CFDynamicBone[] srcs = editorPrefab.GetComponentsInChildren<CFDynamicBone>(true);
                    ec.errors.Clear();
					CopyComponents<CFDynamicBoneManager>(editorPrefab, go, ec.errors);
					CopyComponents<CFDynamicBone>(editorPrefab, go, ec.errors);
					CopyComponents<CFDynamicBoneColliderBase>(editorPrefab, go, ec.errors);
					modified |= ec.errors.Count > 0;

                    foreach (CFDynamicBone src in srcs)
                    {
                        CFDynamicBone dst = FindMatchedComponent(src, go);

                        if (dst == null)
                        {
	                        continue;
                        }
                        if (src.root)
                        {
                            dst.root = FindMatchedComponent(src.root, go);
                        }
                        else
                        {
							ec.errors.Add("骨骼根节点(Root)丢失");
						}
						if (src.entityRoot)
                        {
							dst.entityRoot = FindMatchedComponent(src.entityRoot, go);
						}
                        else
                        {
							ec.errors.Add("角色根节点(Entity Root)丢失");
						}
						for (int iExclude = 0; iExclude < src.exclusions.Count; iExclude++)
                        {
                            if (src.exclusions[iExclude])
                            {
                                dst.exclusions[iExclude] = FindMatchedComponent(src.exclusions[iExclude], go);
                            }
                        }
                        for (int iCollider = 0; iCollider < src.colliders.Count; iCollider++)
                        {
                            if (src.colliders[iCollider])
                            {
                                dst.colliders[iCollider] = FindMatchedComponent(src.colliders[iCollider], go);
                            }
                        }
                    }

					#endregion

					#region RenderBinding
					{
						RenderBinding srcRb = editorPrefab.GetComponent<RenderBinding>();
						if (srcRb)
						{
							modified = true;
							UnityEditorInternal.ComponentUtility.CopyComponent(srcRb);
							RenderBinding toRb = go.GetComponent<RenderBinding>();
							if (toRb)
								UnityEditorInternal.ComponentUtility.PasteComponentValues(toRb);
							else
								UnityEditorInternal.ComponentUtility.PasteComponentAsNew(go);
							toRb = go.GetComponent<RenderBinding>();
							for (int iFace = 0; iFace < toRb.faces.Count; iFace++)
							{
								FaceBinding srcFace = srcRb.faces[iFace];
								FaceBinding toFace = toRb.faces[iFace];
								if (toFace.bone)
								{
									toFace.bone = FindMatchedComponent(srcFace.bone, go);
								}

								if (toFace.renderers != null)
								{
									for (int iRenderer = 0; iRenderer < toFace.renderers.Count; iRenderer++)
									{
										Renderer srcRenderer = srcFace.renderers[iRenderer];
										toFace.renderers[iRenderer] = FindMatchedComponent(srcRenderer, go);
									}	
								}
							}
						}
					}

					#endregion

					#region Bounds

					{
						SkinnedMeshRenderer[] srcSmrs = editorPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
						for (int i = 0; i < srcSmrs.Length; i++)
						{
							SkinnedMeshRenderer srcSmr = srcSmrs[i];
							string path = EditorCommon.GetSceneObjectPath(srcSmr.transform, false);
							Transform dstSmrTs = go.transform.Find(path);
							if (dstSmrTs && dstSmrTs.TryGetComponent(out SkinnedMeshRenderer dstSmr))
							{
								Transform srcBone = srcSmr.rootBone; 
								Transform dstBone = dstSmr.rootBone;
								dstSmr.shadowCastingMode = srcSmr.shadowCastingMode;
								bool boneEqual = (bool)srcBone && (bool)dstBone;
								string srcBonePath = srcBone ? EditorCommon.GetSceneObjectPath(srcBone, false) : string.Empty;
								string dstBonePath = dstBone ? EditorCommon.GetSceneObjectPath(dstBone, false) : string.Empty;
								boneEqual &= srcBonePath == dstBonePath;
								
								if (!boneEqual)
								{
									dstSmr.rootBone = string.IsNullOrEmpty(srcBonePath) ? null : go.transform.Find(srcBonePath);
									dstSmr.localBounds = srcSmr.localBounds;
									modified = true;
								}
							}
						}
					}

					#endregion

					#region CameraBlockGroup
					{
						CameraBlockGroup srcCbg = editorPrefab.GetComponent<CameraBlockGroup>();
						if (srcCbg)
						{
							modified = true;
							CameraBlockGroup toCbg = go.GetComponent<CameraBlockGroup>();
							UnityEditorInternal.ComponentUtility.CopyComponent(srcCbg);
							if (toCbg)
								UnityEditorInternal.ComponentUtility.PasteComponentValues(toCbg);
							else
								UnityEditorInternal.ComponentUtility.PasteComponentAsNew(go);
							toCbg = go.GetComponent<CameraBlockGroup>();

							CopyComponents<Collider>(editorPrefab, go, ec.errors);
							
							FindMatchedComponents(srcCbg.colliders, toCbg.colliders, go);
							FindMatchedComponents(srcCbg.renderers, toCbg.renderers, go);
							
							EditorUtility.SetDirty(editorPrefab);
						}
					}
					#endregion
					
					if (modified)
					{
						EditorUtility.SetDirty(this);
					}
					
					LogDynamicBoneError(ec);
				}
                else
                {
					ec.errors.Clear();
                }

				#endregion

				if (makePrefabs)
				{
					string runtimePrefabPath = ec.runTimePrefabRef ? AssetDatabase.GetAssetPath(ec.runTimePrefabRef) : null;
					ec.runTimePrefabRef = PrefabConfigTool.MakeRunTimePrefab (go, runtimePrefabPath);
				}
				for (int i = 0; i < ec.sfxData.Count; ++i)
				{
					//if (i < ec.addSfxIndex.Count && ec.addSfxIndex[i])
					{
						var sfx = ec.sfxData[i];

						if (sfx.sfx != null && sfx.attach != null)
						{
							var sfxGo = PrefabUtility.InstantiatePrefab (sfx.sfx) as GameObject;
							sfxGo.name = sfx.sfx.name;
							sfxGo.transform.parent = sfx.attach;
							sfxGo.transform.localPosition = Vector3.zero;
							sfxGo.transform.localRotation = Quaternion.identity;
							sfxGo.transform.localScale = Vector3.one;

							eMeshInfo.Add (new EditorMeshInfo ()
							{
								meshPath = sfx.sfx.name,
									isSfx = true,
									pos = sfx.offset,
									rot = sfx.rotation,
									scale = sfx.scale
							});
						}
					}
				}

				if (makePrefabs)
				{
					ec.prefabRef = PrefabUtility.SaveAsPrefabAsset (go, targetPath);
					DebugLog.AddEngineLog2 ("Make EditorPrefab:{0}", targetPath);
				}

				ec.res = PrefabConfigTool.SaveEditorPrefabRes (go.name, this, eMeshInfo, testFade);
				GameObject.DestroyImmediate (go);

				ec.dirty = false;
				ec.editorPrefabMd5 = md5;
				return true;
			}

			return false;
			// }
		}

		private void ConvertPrefabSaveData (PrefabExportConfig ec)
		{
			ec.partConfigs.Clear ();

			for (int i = 0; i < exportMesh.Length; ++i)
			{
				ec.partConfigs.Add (new PrefabPartConfig ()
				{
					partName = exportMesh[i].m.name,
						partEnable = i < ec.partEnable.Count ? ec.partEnable[i] : true,
						partActive = i < ec.partActive.Count ? ec.partActive[i] : true,
				});
			}
			ec.bUseStringIndex = true;

		}

		public void Execute ()
		{
			for (int i = 0; i < exportConfig.Count; ++i)
			{
				var ec = exportConfig[i];
				if (ec.prefabRef != null)
				{
					DebugData dd;
					if (!ec.prefabRef.TryGetComponent (out dd))
					{
						dd = ec.prefabRef.AddComponent<DebugData> ();
					}
					dd.bandPoseConfig = this;
				}
			}
		}

		// [MenuItem (@"Assets/Tool/Fbx_ConfigExe")]
		// private static void Fbx_ConfigExe ()
		// {
		// 	CommonAssets.enumSO.cb = (so, path, context) =>
		// 	{
		// 		if (so is BandposeData)
		// 		{
		// 			var bd = so as BandposeData;
		// 			bd.Execute ();
		// 		}
		// 	};
		// 	CommonAssets.EnumAsset<ScriptableObject> (CommonAssets.enumSO, "ConfigExe");

		// }

		public void StartJobs (ThreadManager threadManager)
		{
			// ThreadManager.isSingleThread = true;
			threadManager.AddJobGroup (
				meshJob,
				meshJob.GetJobCount (), 0.5f);
			// running = true;
			threadManager.StartJobs ();
		}
		public void UpdateJobs (bool finish)
		{

		}
		public void ResetJobs ()
		{
			// running = false;
		}
		public void Release ()
		{
			ResetJobs ();
		}
		
		public bool FindModelPartConfig(string name, out ModelPartConfig config)
		{
			foreach (ModelPartConfig mpc in modelPartConfigs)
			{
				if (string.Equals(mpc.partName, name, StringComparison.CurrentCultureIgnoreCase))
				{
					config = mpc;
					return true;
				}
			}

			SkinnedMeshRenderer[] skinnedMeshRenderers = fbxRef.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
			{
				if (string.Equals(smr.sharedMesh.name, name, StringComparison.CurrentCultureIgnoreCase))
				{
					config = null;
					return false;
				}
			}

			config = ModelPartConfig.Default;
			return true;
		}
	}

}
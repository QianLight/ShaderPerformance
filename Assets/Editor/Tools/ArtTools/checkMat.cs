
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using  System.IO;
public class checkMat : EditorWindow {
	// private Material[] mats;
	// private string path;
	private string varietasPath="Assets";
	private List<MatPrefabs> ListMatPrefabs=new List<MatPrefabs>();
	private List<shaderMat> ListShaMat=new List<shaderMat>();
	private Vector2 scroll=new Vector2();
	private FileInfo[] files;
	private string DirectionPath="Assets/Effects";
	private string filtrateStr="";


	 public enum objectType
	{
		prefab,
		Material,
	}
	private objectType _OType=objectType.Material;


	class shaderMat
	{
		public Shader shader;
		public List<MatPrefabs> ListMatPrefabs;
		public bool IsFadeout;

		public bool IsFiltrated;
	}

	// class MatPrefab
	// {
	// 	public Material material;
	// 	public GameObject prefab;
	// }

	class MatPrefabs
	{
		public Material material;
		public List<GameObject> prefabs;
		public bool IsShowPrefab;
		public bool IsFiltrated;
	}

	class shaderParamType
	{
		public float CullMode;
		public float ZWriteMode;
		public float ZTestMode;
		public float BlendMode;
		public string matName;
	}	
	

	[MenuItem("ArtTools/checkMat")]
	private static void ShowWindow() {
		var window = GetWindow<checkMat>();
		window.titleContent = new GUIContent("checkMat");
		window.Show();
	}
 
	private void OnGUI() 
	{

		

		DirectionPath = EditorGUILayout.TextField("CheckDirectionPath",DirectionPath);
		string DPath=DirectionPath.Replace('\\', '/'); 
		GUIStyle EnumGUIStyle=new GUIStyle();
		_OType = (objectType)EditorGUILayout.EnumPopup("Search Type",_OType);
		varietasPath = EditorGUILayout.TextField("SaveVarietasPath",varietasPath);
		if(GUILayout.Button("Search Materials"))
		{
			ListShaMat.Clear();
			ListMatPrefabs.Clear();
			// path = "Assets/Effects";  //路径
			if (Directory.Exists(DPath))
			{  
				DirectoryInfo direction = new DirectoryInfo(DPath);  
				if(_OType==objectType.Material)
				{
					files = direction.GetFiles("*.mat",SearchOption.AllDirectories);  
				}
				else if((_OType==objectType.prefab))
				{
					files = direction.GetFiles("*.prefab",SearchOption.AllDirectories);  
				}
				

				foreach(FileInfo FI in files)
				{
					string mp =FI.FullName;
					mp = mp.Substring(mp.IndexOf("Assets"));
					mp = mp.Replace('\\', '/');

					if(_OType==objectType.Material)
					{
						Material m0 =AssetDatabase.LoadAssetAtPath(mp,typeof(Material)) as Material;
						MatPrefabs matPrefabs=new MatPrefabs();
						matPrefabs.material=m0;
						matPrefabs.prefabs=new List<GameObject>();
						matPrefabs.IsShowPrefab=false;
						ListMatPrefabs.Add(matPrefabs);
						//ListMat.Add(m0);
					}
					else if((_OType==objectType.prefab))
					{
						GameObject p0=AssetDatabase.LoadAssetAtPath(mp,typeof(GameObject)) as GameObject;
						Renderer[] tempRendererArray=p0.GetComponentsInChildren<Renderer>();
						foreach(Renderer R in tempRendererArray)
						{
							
							if(R)
							{
								bool NotEquals=true;
								foreach(MatPrefabs mp1 in ListMatPrefabs)
								{
									if(NotEquals)
									{
										if(R.sharedMaterial)
										{
											if(R.sharedMaterial.Equals(mp1.material))
											{
												NotEquals=false;

												bool NotEqualsPref=true;
												if(NotEqualsPref)
												{
													foreach(GameObject g in mp1.prefabs)
													{
														if(g.Equals(p0))
															{
 																NotEqualsPref=false;
															}

													}
												}
												
												if(NotEqualsPref)
													mp1.prefabs.Add(p0);
	
											}
										}
										
									}
								}
								if(NotEquals)
								{
									if(R.sharedMaterial)
									{
										//ListMatPrefabs.Add(R.sharedMaterial);
										MatPrefabs matPrefabs=new MatPrefabs();
										matPrefabs.material=R.sharedMaterial;
										matPrefabs.prefabs=new List<GameObject>();
										matPrefabs.prefabs.Add(p0);
										matPrefabs.IsShowPrefab=false;
										ListMatPrefabs.Add(matPrefabs);
									}

										
								}
							}
							
								
						}
						// foreach(Material m1 in tempMatArray)
						// {
						// 	ListMat.Add(m1);
						// }
					}

					//Material m =AssetDatabase.LoadAssetAtPath(mp,typeof(Material)) as Material;
					
				}
				// foreach (Material m in ListMat)
				// {
				// 	if(m)
				// 	{
				// 		if(ListShaMat.Count==0)
				// 		{

				// 			shaderMat sM= new shaderMat();
				// 			sM.shader=m.shader;
				// 			sM.ListMaterial=new List<Material>();
				// 			sM.ListMaterial.Add(m);
				// 			sM.IsFadeout=false;
				// 			ListShaMat.Add(sM);
						
							
				// 		}
				// 		else
				// 		{
				// 			bool isAdd=false;
				// 			foreach(shaderMat shaderM in ListShaMat)
				// 			{
				// 				if(m.shader.Equals(shaderM.shader))
				// 				{
				// 					shaderM.ListMaterial.Add(m);
				// 					isAdd=true;
				// 				}
				// 			}
				// 			if(!isAdd)
				// 			{
				// 				shaderMat sM= new shaderMat();
				// 				sM.shader=m.shader;
				// 				sM.ListMaterial=new List<Material>();
				// 				sM.ListMaterial.Add(m);
				// 				sM.IsFadeout=false;
				// 				ListShaMat.Add(sM);
						
				// 			}

				// 		}
				// 	}


				// }

				foreach (MatPrefabs MatPs in ListMatPrefabs)
				{
					if(MatPs!=null)
					{
						if(ListMatPrefabs.Count==0)
						{

							// shaderMat sM= new shaderMat();
							// sM.shader=MatPs.material.shader;
							// sM.ListMatPrefabs=new List<MatPrefabs>();
							// MatPrefabs matPre= new MatPrefabs();
							// matPre.material=MatPs.material;
							// //matPre.prefabs=new List<GameObject>();
							// matPre.prefabs=(MatPs.prefabs);
							// sM.ListMatPrefabs.Add(matPre);
							// sM.IsFadeout=false;
							// ListShaMat.Add(sM);


							shaderMat sM= new shaderMat();
							sM.shader=MatPs.material.shader;
							sM.ListMatPrefabs=new List<MatPrefabs>();
							MatPrefabs matPre= new MatPrefabs();
							matPre.prefabs=(MatPs.prefabs);
							matPre.material=MatPs.material;
							sM.ListMatPrefabs.Add(matPre);
							sM.IsFadeout=false;
							ListShaMat.Add(sM);
						}
						else
						{
							bool isAdd=false;
							foreach(shaderMat shaderM in ListShaMat)
							{
								if(MatPs.material.shader.Equals(shaderM.shader))
								{
									MatPrefabs matPre= new MatPrefabs();
									matPre.material=MatPs.material;
									matPre.prefabs=new List<GameObject>();
									matPre.prefabs=(MatPs.prefabs);
									shaderM.ListMatPrefabs.Add(matPre);
									isAdd=true; 
								}
							}
							if(!isAdd)
							{
								shaderMat sM= new shaderMat();
								sM.shader=MatPs.material.shader;
								sM.ListMatPrefabs=new List<MatPrefabs>();
								MatPrefabs matPre= new MatPrefabs();
								matPre.prefabs=(MatPs.prefabs);
								matPre.material=MatPs.material;
								sM.ListMatPrefabs.Add(matPre);
								sM.IsFadeout=false;
								ListShaMat.Add(sM);
						
							}
							else
							{

							}

						}
					}


				}
				

		
			}
			
		}
		int MatCount=0;
		if(files!=null)
		{
			MatCount=ListMatPrefabs.Count;
		}

		string shaderCountStr= "shader count:"+ (ListShaMat.Count.ToString()+"      Material Count:"+MatCount.ToString());

		GUILayout.BeginHorizontal();
		GUILayout.Label(shaderCountStr);
		filtrateStr=EditorGUILayout.TextField("材质名:",filtrateStr);
		GUILayout.EndHorizontal();


		 scroll =GUILayout.BeginScrollView(scroll);

		foreach(shaderMat sM in ListShaMat)
		{
			List<MatPrefabs> ListMP =new List<MatPrefabs>();
			for(int i=0;i<sM.ListMatPrefabs.Count;i++)
			{
				if(sM.ListMatPrefabs[i].material.name.Contains(filtrateStr))
				{
					sM.IsFiltrated=true;
					sM.ListMatPrefabs[i].IsFiltrated=true;
				}
				else
				{
					sM.IsFiltrated=false;
					sM.ListMatPrefabs[i].IsFiltrated=false;
				}
			}

		}



		foreach(shaderMat sM in ListShaMat)
		{
			GUILayout.Space(10.0f);
			GUIStyle ShaderGUIStyle=new GUIStyle();
			ShaderGUIStyle.normal.textColor=new Color(0.0f,0.0f,0.0f);
			GUIStyle MaterialGUIStyle=new GUIStyle();
			MaterialGUIStyle.normal.textColor=new Color(1.0f,1.0f,1.0f);
			//GUILayout.Label(sM.shader.name + "  count:"+ (sM.ListMaterial.Count.ToString()),ShaderGUIStyle);
			if(sM.IsFiltrated)
			{
				Shader s=EditorGUILayout.ObjectField(sM.shader,typeof(Shader),true) as Shader;
				EditorGUILayout.BeginHorizontal();
				sM.IsFadeout = EditorGUILayout.Foldout(sM.IsFadeout,  "Materials:"+ (sM.ListMatPrefabs.Count.ToString()));
			
			
			
				if(GUILayout.Button("calculate varietas",GUILayout.Width(120)))
				{
					if(Directory.Exists(varietasPath))
					{
						List<shaderParamType> shaParam=new List<shaderParamType>();
						foreach(MatPrefabs matPs in sM.ListMatPrefabs)
						{
							if(shaParam.Count==0)
							{
								shaderParamType myshaderParam = new shaderParamType ();
								myshaderParam.CullMode=matPs.material.GetFloat("_CullMode");
								myshaderParam.ZWriteMode=matPs.material.GetFloat("_DepthMode");
								myshaderParam.ZTestMode=matPs.material.GetFloat("_ZTest");
								myshaderParam.BlendMode=matPs.material.GetFloat("_BlendMode");
								myshaderParam.matName=matPs.material.name;
								shaParam.Add(myshaderParam);
								Material material=new Material(sM.shader);
								material.SetFloat("_CullMode",myshaderParam.CullMode);
								material.SetFloat("_DepthMode",myshaderParam.ZWriteMode);
								material.SetFloat("_ZTest",myshaderParam.ZTestMode);
								material.SetFloat("_BlendMode",myshaderParam.BlendMode);
								AssetDatabase.CreateAsset(material,varietasPath+"/"+myshaderParam.matName+".mat");
							}
							else
							{
								bool EqualsParam =false;
								foreach(shaderParamType spt in shaParam)
								{
									if(!EqualsParam)
									{
										EqualsParam=(matPs.material.GetFloat("_CullMode")==spt.CullMode);
										EqualsParam=EqualsParam&&(matPs.material.GetFloat("_DepthMode")==spt.ZWriteMode);
										EqualsParam=EqualsParam&&(matPs.material.GetFloat("_ZTest")==spt.ZTestMode);
										EqualsParam=EqualsParam&&(matPs.material.GetFloat("_BlendMode")==spt.BlendMode);
									}
								}
								if(!EqualsParam)
									{
										shaderParamType myshaderParam = new shaderParamType ();
										myshaderParam.CullMode=matPs.material.GetFloat("_CullMode");
										myshaderParam.ZWriteMode=matPs.material.GetFloat("_DepthMode");
										myshaderParam.ZTestMode=matPs.material.GetFloat("_ZTest");
										myshaderParam.BlendMode=matPs.material.GetFloat("_BlendMode");
										myshaderParam.matName=matPs.material.name;
										shaParam.Add(myshaderParam);
										Material material=new Material(sM.shader);
										material.SetFloat("_CullMode",myshaderParam.CullMode);
										material.SetFloat("_DepthMode",myshaderParam.ZWriteMode);
										material.SetFloat("_ZTest",myshaderParam.ZTestMode);
										material.SetFloat("_BlendMode",myshaderParam.BlendMode);
										AssetDatabase.CreateAsset(material,varietasPath+"/"+myshaderParam.matName+".mat");

									}

							}

						}
					}
				}
				EditorGUILayout.EndHorizontal(); 


			}
			

			if(sM.IsFadeout)
			{
				foreach(MatPrefabs matPs in sM.ListMatPrefabs)
				{
					if(matPs.IsFiltrated)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(50);
						Material m0=EditorGUILayout.ObjectField(matPs.material,typeof(Material),true) as Material;
						EditorGUILayout.EndHorizontal();
						if(_OType==objectType.prefab)
						{ 
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(50);
							matPs.IsShowPrefab = EditorGUILayout.Foldout(matPs.IsShowPrefab,  "Prefabs:"+ (matPs.prefabs.Count.ToString()));
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginVertical(); 
							
							if(matPs.IsShowPrefab)
							{
								foreach(GameObject prefabs in matPs.prefabs )
								{
									EditorGUILayout.BeginHorizontal();
									GUILayout.Space(100);
									GameObject GO0=EditorGUILayout.ObjectField(prefabs,typeof(GameObject),true) as GameObject;
									EditorGUILayout.EndHorizontal();
								}
								GUILayout.Space(10); 
							}
							
							EditorGUILayout.EndVertical();
							
						}
					}
					//GUILayout.Label(material.name,MaterialGUIStyle);
					
					
					

				}
			}
			
		}
		GUILayout.EndScrollView();
		// foreach(Material material in ListMat)
		// 				{
		// 					//GUILayout.Label(material.name,MaterialGUIStyle);
		// 					Material m0=EditorGUILayout.ObjectField(material,typeof(Material)) as Material;

		// 				}

	}
} 


	

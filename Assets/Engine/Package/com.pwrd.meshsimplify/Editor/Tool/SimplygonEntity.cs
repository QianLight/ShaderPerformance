using System;
using System.Collections.Generic;
using System.Linq;
using Simplygon;
using UnityEngine;

namespace Athena.MeshSimplify
{
    public class SimplygonEntity
    {
        private ISimplygon m_simplygon;
        private spScene m_scene;
        private Dictionary<string, spGeometryData> m_geomMap = new Dictionary<string, spGeometryData>();
        private Dictionary<string, int> m_matMap = new Dictionary<string, int>();
        private Dictionary<string, spShadingFilterNode> m_shaderNodeMap = new Dictionary<string, spShadingFilterNode>();
        private string m_tempFolderPath;

        private Action<string> logCallback;
        
        private const string UVzero = "UVzero";
        private const string UVone = "UVone";

        public bool InitSDK()
        {
            m_simplygon = SimplygonLoader.InitSimplygon(out EErrorCodes simplygonErrorCode, out string simplygonErrorMessage);
            if (simplygonErrorCode != EErrorCodes.NoError)
            {
	            SimplygonTool.DisplayError(simplygonErrorCode, simplygonErrorMessage);
	            return false;
            }
            Log("simplygon init success");
            return true;
        }

        ~SimplygonEntity()
        {
	        DeInitSDK();
        }
        
        public void Release()
        {
	        if (m_scene != null)
	        {
		        m_scene.Clear();
		        m_scene.Dispose();
	        }
	        m_scene = null;
            m_geomMap.Clear();
            m_matMap.Clear();
            m_shaderNodeMap.Clear();
        }
        
        public void DeInitSDK()
        {
            Release();
            if (m_simplygon != null)
            {
                m_simplygon.Dispose();
                m_simplygon = null;
            }
            global::Simplygon.Simplygon.DeinitializeSimplygonThread();
            Log("simplygon dispose success");
        }

        

        public void SetTempFolderPath(string tempFolderPath)
        {
	        m_tempFolderPath = tempFolderPath;
        }
        
        public void AddMeshNode(string name, float[] matrixArr, int vertex_count, float[] vertex_coordinates, int triangle_count, int[] corner_ids, float[] texture_coordinates, float[] texture_coordinates2, float[] normals)
        {
            if (m_simplygon == null)
            {
	            Debug.LogError("please init simplygon before it");
	            return;
            }
            CheckSceneInit();
            
            int corner_count = triangle_count * 3;
            
            //Create the Geometry.  All geometrydata will be loaded into this object
            var geometry = m_simplygon.CreateGeometryData();
            
            // Set vertex- and triangle-counts for the Geometry. 
	        // NOTE: The number of vertices and triangles has to be set before vertex- and triangle-data is loaded into the GeometryData.
            geometry.SetVertexCount((uint)vertex_count);
            geometry.SetTriangleCount((uint)triangle_count);
            
            // Array with vertex-coordinates. Will contain 3 real-values for each vertex in the geometry.
	        spRealArray coords = geometry.GetCoords();
	        // add vertex-coordinates to the Geometry. Each tuple contains the 3 coordinates for each vertex. x, y and z values.
	        for (int i = 0; i < vertex_count; ++i)
	        {
		        coords.SetTuple(i, new float[]{ vertex_coordinates[i * 3], vertex_coordinates[i * 3 + 1], vertex_coordinates[i * 3 + 2]});
	        }
	        
	        // Array with triangle-data. Will contain 3 ids for each corner of each triangle, so the triangles know what vertices to use.
	        spRidArray vertex_ids = geometry.GetVertexIds();
	        // Add triangles to the Geometry. Each triangle-corner contains the id for the vertex that corner uses.
	        // SetTuple can also be used, but since the TupleSize of the vertex_ids array is 1, it would make no difference.
	        // (This since the vertex_ids array is corner-based, not triangle-based.)
	        for (int i = 0; i < corner_count; ++i)
	        {
		        vertex_ids.SetItem(i, corner_ids[i]);
	        }
	        // Must add texture channel before adding data to it. 
	        geometry.AddTexCoords(0);
	        spRealArray texcoords = geometry.GetTexCoords(0);
	        texcoords.SetAlternativeName(UVzero);
            for (int i = 0; i < corner_count; i++)
			{
				texcoords.SetTuple(i, new float[]{ texture_coordinates[i * 2], texture_coordinates[i * 2 + 1]});
			}
	
			geometry.AddTexCoords(1);
			spRealArray texcoords1 = geometry.GetTexCoords(1);
			texcoords1.SetAlternativeName(UVone);
			for (int i = 0; i < corner_count; ++i)
			{
				texcoords1.SetTuple(i, new float[]{ texture_coordinates2[i * 2], texture_coordinates2[i * 2 + 1]});
			}
   
			geometry.AddNormals();
			spRealArray normals_arr = geometry.GetNormals();
			for (int i = 0; i < corner_count; ++i)
			{
				normals_arr.SetTuple(i, new float[]{ normals[i * 3], normals[i * 3 + 1], normals[i * 3 + 2]});
			}
   
			//create a SceneMesh node with the geometry
			spSceneMesh sceneMesh = m_simplygon.CreateSceneMesh();
			sceneMesh.SetGeometry(geometry);
			sceneMesh.SetName(name);
			m_scene.GetRootNode().AddChild(sceneMesh);
			m_geomMap[name] = geometry;
   
			geometry.Transform(ConvertToMatrix(matrixArr));
			printMatrix(ConvertToMatrix(matrixArr));
			Log("AddMeshNode end");
			
			//OutputScene(scene, "E:\\MyTempFolder\\AddMeshNode_" + name + ".obj");
        }

        public void AddTextureMaterial(string name, string texturePath, string lightmapPath)
        {
            //BindMeshAndMaterial("aMeshNode", name);
			Log("AddTextureMaterial start texturepath:" + texturePath);
			
			var materialTable = m_scene.GetMaterialTable();
			var textureTable = m_scene.GetTextureTable();

			var importer = m_simplygon.CreateImageDataImporter();
			importer.SetImportFilePath(texturePath);
			importer.RunImport();
			spImageData imgData = importer.GetImage();

			spTexture texture = m_simplygon.CreateTexture();
			texture.SetName(name);
			texture.SetImageData(imgData);
			textureTable.AddTexture(texture);

			var lightMapPathStr = lightmapPath;
			lightMapPathStr = lightMapPathStr.Replace("\\", "/");
			int iPos = lightMapPathStr.LastIndexOf('/') + 1;
			string filename = lightMapPathStr.Substring(iPos, lightMapPathStr.Length - iPos);

			var name2 = filename.Substring(0, filename.LastIndexOf("."));
			//LogInfo("AddTextureMaterial origin filename:" + filename + " pos:" + to_string(iPos) + " name2:" + name2);
			var importer2 = m_simplygon.CreateImageDataImporter();
			importer2.SetImportFilePath(lightmapPath);
			importer2.RunImport();
			spImageData imgData2 = importer2.GetImage();

			spTexture texture2 = m_simplygon.CreateTexture();
			texture2.SetName(name2);
			texture2.SetImageData(imgData2);
			textureTable.AddTexture(texture2);

			spShadingTextureNode shaderNode = m_simplygon.CreateShadingTextureNode();
			shaderNode.SetTextureName(name);
			shaderNode.SetTexCoordLevel(1);
			// shaderNode.SetTexCoordName(UVzero);
			shaderNode.SetTexCoordName(UVone);

			spShadingTextureNode shaderNode2 = m_simplygon.CreateShadingTextureNode();
			shaderNode2.SetTextureName(name2);
			shaderNode2.SetTexCoordName(UVone);

			spShadingColorNode colorNode = m_simplygon.CreateShadingColorNode();
			colorNode.SetColor(0, 0, 1, 255);
			spShadingMultiplyNode multi1 = m_simplygon.CreateShadingMultiplyNode();
			multi1.SetInput(0, colorNode);
			multi1.SetInput(1, shaderNode2);

			spShadingMultiplyNode multi = m_simplygon.CreateShadingMultiplyNode();
			multi.SetInput(0, shaderNode);
			multi.SetInput(1, shaderNode2);

			spMaterial mat = m_simplygon.CreateMaterial();

			mat.AddMaterialChannel(TextureChannel.Albedo.ToString());
			mat.AddMaterialChannel(TextureChannel.LightMap.ToString());
			mat.SetShadingNetwork(TextureChannel.Albedo.ToString(), shaderNode);
			mat.SetShadingNetwork(TextureChannel.LightMap.ToString(), shaderNode2);
			int id = materialTable.AddMaterial(mat);

			m_matMap[name] = id;

			// OutputImage(texture, "E:\\MyTempFolder\\" + name + "_" + channel + "_AddTextureMaterial.png");
			Log("AddTextureMaterial success");

			//BindMeshAndMaterial("aMeshNode", name);
        }
        
        public void AddTextureMaterialByChannel(string matName, string texturePath, TextureChannel channel)
        {
			Log("AddTextureMaterial start texturepath:" + texturePath);
			
			var materialTable = m_scene.GetMaterialTable();
			var textureTable = m_scene.GetTextureTable();

			var importer = m_simplygon.CreateImageDataImporter();
			importer.SetImportFilePath(texturePath);
			importer.RunImport();
			spImageData imgData = importer.GetImage();

			var texPathStr = texturePath;
			texPathStr = texPathStr.Replace("\\", "/");
			int iPos = texPathStr.LastIndexOf('/') + 1;
			string filename = texPathStr.Substring(iPos, texPathStr.Length - iPos);
			var name = filename.Substring(0, filename.LastIndexOf("."));

			spTexture texture = m_simplygon.CreateTexture();
			texture.SetName(name);
			texture.SetImageData(imgData);
			textureTable.AddTexture(texture);

			spShadingTextureNode shaderNode = m_simplygon.CreateShadingTextureNode();
			shaderNode.SetTextureName(name);
			// shaderNode.SetTexCoordName(UVzero);
			shaderNode.SetTexCoordName(UVone);
			
			spShadingColorNode colorNode = m_simplygon.CreateShadingColorNode();
			colorNode.SetColor(1, 1, 1, 1);
			spShadingMultiplyNode multi = m_simplygon.CreateShadingMultiplyNode();
			multi.SetInput(0, colorNode);
			multi.SetInput(1, shaderNode);

			var channelName = channel.ToString();
			if (m_matMap.ContainsKey(matName))
			{
				spMaterial mat = materialTable.GetMaterial(m_matMap[matName]);
				mat.AddMaterialChannel(channelName);
				mat.SetShadingNetwork(channelName, multi);
			}
			else
			{
				spMaterial mat = m_simplygon.CreateMaterial();
				mat.AddMaterialChannel(channelName);
				mat.SetShadingNetwork(channelName, multi);
				int id = materialTable.AddMaterial(mat);

				m_matMap[name] = id;
			}

			Log("AddTextureMaterial success");
        }

        public void AddTextureMaterial(string matName, string texturePath)
        {
            Log("AddSingleTextureMaterial start texturepath:" + texturePath);

            var materialTable = m_scene.GetMaterialTable();
			var textureTable = m_scene.GetTextureTable();

			var importer = m_simplygon.CreateImageDataImporter();
			importer.SetImportFilePath(texturePath);
			importer.RunImport();
			spImageData imgData = importer.GetImage();

			var pathStr = texturePath;
			pathStr = pathStr.Replace("\\", "/");
			int iPos = pathStr.LastIndexOf('/') + 1;
			string filename = pathStr.Substring(iPos, pathStr.Length - iPos);
			var name = filename.Substring(0, filename.LastIndexOf("."));

			spTexture texture = m_simplygon.CreateTexture();
			texture.SetName(name);
			texture.SetImageData(imgData);
			textureTable.AddTexture(texture);

			spShadingTextureNode shaderNode = m_simplygon.CreateShadingTextureNode();
			shaderNode.SetTextureName(name);
			shaderNode.SetTexCoordName(UVone);

			spMaterial mat = m_simplygon.CreateMaterial();
			mat.AddMaterialChannel(TextureChannel.Albedo.ToString());
			mat.SetShadingNetwork(TextureChannel.Albedo.ToString(), shaderNode);
			int id = materialTable.AddMaterial(mat);

			m_matMap[matName] = id;

			Log("AddSingleTextureMaterial success texturepath:" + texturePath);
        }

        public void BindMeshAndMaterial(string meshName, string matName)
        {
            Log("BindMeshAndMaterial start");

			m_geomMap.TryGetValue(meshName, out spGeometryData geom);
			if (geom == null)
			{
				Log("BindMeshAndMaterial faile this is no mesh named:" + meshName);
				return;
			}

			m_matMap.TryGetValue(matName, out int matID);
			if (matID == -1)
			{
				Log("BindMeshAndMaterial faile this is no mat named:" + matName);
				return;
			}
			geom.AddMaterialIds();
			spRidArray materialIdsArray = geom.GetMaterialIds();
			for (int i = 0; i < geom.GetTriangleCount(); i++)
			{
				materialIdsArray.SetItem(i, matID);
			}
			Log("BindMeshAndMaterial success");

			// RenderScene(m_scene, m_scene.GetMaterialTable().GetMaterial(matID));
        }
        
        public void RunReductionProcessing(float triangleRatio)
        {
            RunReductionProcessing(triangleRatio, -1, false);
        }

        public void RunReductionByScreenSize(int screenSize)
        {
            RunReductionProcessing(1, screenSize, false);
        }

        public void RunReductionProcessingUseOcclusion(float triangleRatio, float pitchAngle, float yawAngle, float coverage)
        {
            RunReductionProcessing(triangleRatio, -1, true, pitchAngle, yawAngle, coverage);
        }

        public void RunReductionByScreenSizeUseOcclusion(int screenSize, float pitchAngle, float yawAngle, float coverage)
        {
            RunReductionProcessing(1, screenSize, true, pitchAngle, yawAngle, coverage);
        }

        public void RunAggregate(int width, int height, TextureChannel textureChannel = TextureChannel.Albedo)
        {
            Log("RunAggregate  start ");
			// string writeTo = "RunAggregate";
			uint tileSize = 1;

			// Get the scene
			spScene aggregatedScene = m_scene;

			// Create the spAggregationProcessor
			spAggregationProcessor aggregationProcessor = m_simplygon.CreateAggregationProcessor();
			// Set the input scene
			aggregationProcessor.SetScene(aggregatedScene);

			spAggregationSettings aggregatorSettings = aggregationProcessor.GetAggregationSettings();


			//Enable the subdivision
			aggregatorSettings.SetSubdivideGeometryBasedOnUVTiles(false);
			aggregatorSettings.SetSubdivisionTileSize(tileSize);

			// Set the BaseAtlasOnOriginalTexCoords to true so that the new texture
			// coords will be based on the original. This way the four identical
			// SimplygonMan mesh instances will still share texture coords, and
			// when packing the texture charts into the new atlas, only rotations
			// multiples of 90 degrees are allowed.
			//aggregatorSettings.SetBaseAtlasOnOriginalTexCoords(false);

			// Get the mapping image settings, a mapping image is needed to
			// cast the new textures.
			spMappingImageSettings mappingImageSettings = aggregationProcessor.GetMappingImageSettings();
			mappingImageSettings.SetGenerateMappingImage(true);
			/*mappingImageSettings.SetGutterSpace(2);
			mappingImageSettings.SetWidth(width);
			mappingImageSettings.SetHeight(height);*/
			mappingImageSettings.SetUseFullRetexturing(true); //replace old UVs

			spMappingImageOutputMaterialSettings materialSettings = mappingImageSettings.GetOutputMaterialSettings(0);
			materialSettings.SetGutterSpace(2);
			materialSettings.SetTextureWidth((uint)width);
			materialSettings.SetTextureHeight((uint)height);

			// If BaseAtlasOnOriginalTexCoords is enabled and
			// if charts are overlapping in the original texture coords, they will be separated if
			// SeparateOverlappingCharts is set to true.

			spChartAggregatorSettings chartAggregatorSettings = mappingImageSettings.GetChartAggregatorSettings();

			chartAggregatorSettings.SetSeparateOverlappingCharts(true);
			Log("RunAggregate RunProcessing  start ");
			// Run the process
			aggregationProcessor.RunProcessing();
			Log("RunAggregate RunProcessing  end ");

			List<string> channelList = new List<string>();
			channelList.Add(TextureChannel.Albedo.ToString());
			if ((textureChannel & TextureChannel.Normal) != 0) channelList.Add(TextureChannel.Normal.ToString());
			if ((textureChannel & TextureChannel.Metallic) != 0) channelList.Add(TextureChannel.Metallic.ToString());
			if ((textureChannel & TextureChannel.LightMap) != 0) channelList.Add(TextureChannel.LightMap.ToString());

			var e = m_scene == aggregatedScene;
			Log("RunAggregate RunProcessing  start ");
			foreach (var ch in channelList) {
				spColorCaster caster = m_simplygon.CreateColorCaster();
				caster.SetSourceMaterials(aggregatedScene.GetMaterialTable());
				caster.SetSourceTextures(aggregatedScene.GetTextureTable());
				var setting = caster.GetColorCasterSettings();
				setting.SetMaterialChannel(ch);
				//setting.SetOutputChannels(4);
				string path = m_tempFolderPath + ch + ".png";
				Log("RunAggregate diffuseCaster path: " + path);
				caster.SetOutputFilePath(path);
				caster.SetMappingImage(aggregationProcessor.GetMappingImage());

				caster.RunProcessing();
			}
			Log("RunAggregate RunProcessing  start ");


			//Clear the copied materials from the lod, and later populate it with newly generated materials
			spTextureTable lodTexTable = aggregatedScene.GetTextureTable();
			for (int i = 0; i < lodTexTable.GetTexturesCount(); i++)
			{
				var item = lodTexTable.GetTexture(i);
				var i_name = item.GetName();
				Debug.Log(i_name);
			}
			lodTexTable.Clear();
			spMaterialTable lodMatTable = aggregatedScene.GetMaterialTable();
			lodMatTable.Clear();

			//Create a new material that we cast the new textures onto
			spMaterial lodMaterial = m_simplygon.CreateMaterial();
			lodMaterial.SetName("output_material");
			foreach (var ch in channelList) {
				lodMaterial.AddMaterialChannel(ch);
			}

			int id = lodMatTable.AddMaterial(lodMaterial);
			lodMatTable.SetName("CombinedMaterials");
			//Setup new texture in LOD scene

			foreach (var ch in channelList) {
				string path = m_tempFolderPath + ch + ".png";
				AddSimplygonTexture(lodMaterial, lodTexTable, ch, path);
			}

			Log("RunAggregate  end ");
        }

        public void ExportScene(string outputPath)
        {
            var scene = m_simplygon.CreateScene();
			scene = m_scene.NewCopy();

			/*spTextureTable lodTexTable = scene->GetTextureTable();
			lodTexTable->Clear();
			spMaterialTable lodMatTable = scene->GetMaterialTable();
			lodMatTable->Clear();*/

			OutputScene(scene, outputPath);
        }

        public void CreateNewScene()
        {
	        Release();
	        
            m_scene = m_simplygon.CreateScene();
        }
        
        public void SetLogCallback(Action<string> callback)
        {
	        logCallback = callback;
        }

        private void Log(string info)
        {
	        logCallback?.Invoke(info);
        }
        
        private void CheckSceneInit()
        {
            if (m_scene == null)
            {
                m_scene = m_simplygon.CreateScene();
            }
        }

        private void AddSimplygonTexture(spMaterial material, spTextureTable textureTable, string chanel, string outputFileName)
		{
			var importer = m_simplygon.CreateImageDataImporter();
			importer.SetImportFilePath(outputFileName);
			importer.RunImport();
			spImageData imageData = importer.GetImage();
			spTexture tex = m_simplygon.CreateTexture();
			tex.SetImageData(imageData);
			tex.SetName(chanel);
			var id = textureTable.AddTexture(tex);

			//test output 
			// OutputImage(tex, (chanel + "_reImport.png"));

			var node = m_simplygon.CreateShadingTextureNode();
			node.SetTextureName(chanel);
			material.AddMaterialChannel(chanel);
			material.SetShadingNetwork(chanel, node);
		}
        
        private void OutputImage(spTexture tex, string path)
		{
			Log("OutputImage start target:" + path);

			var importer = m_simplygon.CreateImageDataExporter();
			importer.SetExportFilePath(path);
			importer.SetImage(tex.GetImageData());
			importer.RunExport();

			Log("OutputImage success ");
		}

        private spMatrix4x4 ConvertToMatrix(float[] matrixArr)
		{
			/*
			1	1	1	1
			2	2	2	2
			3	3	3	3
			4	4	4	4
			*/
			var matrix = m_simplygon.CreateMatrix4x4();
			for (uint x = 0; x < 4; x++)
			{
				for (uint y = 0; y < 4; y++)
				{
					matrix.SetElement(x, y, matrixArr[x + y * 4]);
				}
			}
			//matrix.SetToTranslationTransform(0, 1, 0);
			return matrix;
		}
        
        private  void printMatrix(spMatrix4x4 matrix)
		{
			for (uint y = 0; y < 4; y++)
			{
				for (uint x = 0; x < 4; x++)
				{
					Debug.Log(matrix.GetElement(x, y));
				}
			}
		}

        public void RunReductionProcessing(ReductionSetting setting)
        {
	        Log("RunReductionProcessing start");

	        //Create a copy of the original scene on which we will run the reduction
	        spScene lodScene = m_scene; // .NewCopy();
	        // Create the reduction-processor, and set which scene to reduce
	        spReductionProcessor reductionProcessor = m_simplygon.CreateReductionProcessor();
	        reductionProcessor.SetScene(lodScene);
	        
	        spReductionSettings reductionSettings = reductionProcessor.GetReductionSettings();
	        reductionSettings.SetKeepSymmetry(true); //Try, when possible to reduce symmetrically
	        reductionSettings.SetUseAutomaticSymmetryDetection(true); //var-detect the symmetry plane, if one exists. Can, if required, be set manually instead.
	        reductionSettings.SetUseHighQualityNormalCalculation(true); //Drastically increases the quality of the LODs normals, at the cost of extra processing time.
	        reductionSettings.SetReductionHeuristics(EReductionHeuristics.Consistent); //Choose between "fast" and "consistent" processing. Fast will look as good, but may cause inconsistent 
	        //triangle counts when comparing MaxDeviation targets to the corresponding percentage targets.
	        
	        reductionSettings.SetReductionTargets(EStopCondition.Any, true, false, setting.enableMaxDeviation, setting.enableScreenSize);
	        reductionSettings.SetReductionTargetTriangleRatio(setting.triangleRatio);
	        reductionSettings.SetLockGeometricBorder(setting.lockGeometricBorder);
            if (setting.enableScreenSize) reductionSettings.SetReductionTargetOnScreenSize(setting.screenSize);
            if (setting.enableMaxDeviation) reductionSettings.SetReductionTargetMaxDeviation(setting.maxDeviation);
            reductionSettings.SetGeometryImportance(setting.geometryImportance);
            
	        using (spNormalCalculationSettings normalSettings = reductionProcessor.GetNormalCalculationSettings())
	        {
		        normalSettings.SetReplaceNormals(false); //If true, this will turn off normal handling in the reducer and recalculate them all afterwards instead.
		        normalSettings.SetRepairInvalidNormals(true);
	        }
	        
	        reductionProcessor.RunProcessing();
	        
	        Log("RunReductionProcessing end");
        }

        private void RunReductionProcessing(float triangleRatio, int screenSize, bool useOcclusion, float pitchAngle = 0, float yawAngle = 0, float coverage = 360)
        {
	        Log("RunReductionProcessing start");

	        //Create a copy of the original scene on which we will run the reduction
	        spScene lodScene = m_scene; // .NewCopy();
	        // Create the reduction-processor, and set which scene to reduce
	        spReductionProcessor reductionProcessor = m_simplygon.CreateReductionProcessor();
	        reductionProcessor.SetScene(lodScene);

	        if (useOcclusion)
	        {
		        var visibilitySettings = reductionProcessor.GetVisibilitySettings();
		        spSceneCamera sceneCamera = m_simplygon.CreateSceneCamera();
		        sceneCamera.SetCustomSphereCameraPath(
			        3, //Fidelity
			        pitchAngle, //Pitch angle
			        yawAngle, //Yaw angle
			        coverage //Coverage (degrees)
		        );
		        if (sceneCamera.ValidateCamera()) //If the camera is setup correctly
		        {
			        lodScene.GetRootNode().AddChild(sceneCamera);
			        spSelectionSet selectionSet = m_simplygon.CreateSelectionSet();
			        selectionSet.AddItem(sceneCamera.GetNodeGUID());
			        int cameraSelectionSetID = lodScene.GetSelectionSetTable().AddItem(selectionSet);
			        visibilitySettings.SetCameraSelectionSetID(cameraSelectionSetID);
		        }

		        visibilitySettings.SetUseVisibilityWeightsInReducer(true);
		        visibilitySettings.SetUseVisibilityWeightsInTexcoordGenerator(true);
		        visibilitySettings.SetCullOccludedGeometry(true);
	        }

	        ///////////////////////////////////////////////////////////////////////////////////////////////
	        // SETTINGS - Most of these are set to the same value by default, but are set anyway for clarity

	        // The reduction settings object contains settings pertaining to the actual decimation
	        spReductionSettings reductionSettings = reductionProcessor.GetReductionSettings();
	        reductionSettings.SetKeepSymmetry(true); //Try, when possible to reduce symmetrically
	        reductionSettings.SetUseAutomaticSymmetryDetection(true); //var-detect the symmetry plane, if one exists. Can, if required, be set manually instead.
	        reductionSettings.SetUseHighQualityNormalCalculation(true); //Drastically increases the quality of the LODs normals, at the cost of extra processing time.
	        reductionSettings.SetReductionHeuristics(EReductionHeuristics.Consistent); //Choose between "fast" and "consistent" processing. Fast will look as good, but may cause inconsistent 
	        //triangle counts when comparing MaxDeviation targets to the corresponding percentage targets.

	        // The reducer uses importance weights for all features to decide where and how to reduce.
	        // These are advanced settings and should only be changed if you have some specific reduction requirement
	        /*reductionSettings.SetShadingImportance(2.f); //This would make the shading twice as important to the reducer as the other features.*/

	        // The actual reduction triangle target are controlled by these settings

	        reductionSettings.SetReductionTargets(EStopCondition.Any, true, false, true, screenSize > 0);
	        reductionSettings.SetReductionTargetTriangleRatio(triangleRatio);

	        //reductionSettings.SetReductionTargetStopCondition(EStopCondition::Any);//The reduction stops when any of the targets below is reached
	        //if (screenSize > 0)
	        //{
	        //	reductionSettings.SetReductionTargets(SG_REDUCTIONTARGET_ONSCREENSIZE);//Selects which targets should be considered when reducing
	        //	reductionSettings.SetOnScreenSize(screenSize); //Targets when the LOD is optimized for the selected on screen pixel size
	        //}
	        //else
	        //{
	        //	reductionSettings.SetReductionTargets(SG_REDUCTIONTARGET_TRIANGLERATIO);//Selects which targets should be considered when reducing
	        //	reductionSettings.SetTriangleRatio(triangleRatio); //Targets at 50% of the original triangle count
	        //}
	        //reductionSettings.SetTriangleCount(10); //Targets when only 10 triangle remains

	        //reductionSettings.SetReductionTargetMaxDeviation(REAL_MAX); //Targets when an error of the specified size has been reached. As set here it never happens.


	        // The repair settings object contains settings to fix the geometries
	        //spRepairSettings repairSettings = reductionProcessor.GetRepairSettings();
	        //repairSettings.SetTjuncDist(0.0f); //Removes t-junctions with distance 0.0f
	        //repairSettings.SetWeldDist(0.0f); //Welds overlapping vertices

	        // The normal calculation settings deal with the normal-specific reduction settings
	        using (spNormalCalculationSettings normalSettings = reductionProcessor.GetNormalCalculationSettings())
	        {
		        normalSettings.SetReplaceNormals(false); //If true, this will turn off normal handling in the reducer and recalculate them all afterwards instead.
		        //If false, the reducer will try to preserve the original normals as well as possible
		        /*normalSettings.SetHardEdgeAngleInRadians( 3.14159f*60.0f/180.0f ); //If the normals are recalculated, this sets the hard-edge angle.*/
	        }

	        //END SETTINGS 
	        ///////////////////////////////////////////////////////////////////////////////////////////////

	        // Add progress observer
	        // Run the actual processing. After this, the set geometry will have been reduced according to the settings
	        reductionProcessor.RunProcessing();

	        // For this reduction, the LOD will use the same material set as the original, and hence no further processing is required

	        // Generate the output filenames
	        /*std::string outputGeomFilename = "E:\\workspace\\Unity\\AutoLODTest\\Assets\\HLOD\\Reduction.obj";
	        OutputScene(lodScene, outputGeomFilename.c_str());*/

	        Log("RunReductionProcessing end");
        }

        private void OutputScene(spScene scene, string path)
		{
			Log(("OutputScene start target:" + path));
			spWavefrontExporter objexp = m_simplygon.CreateWavefrontExporter();
			objexp.SetExportFilePath(path);
			objexp.SetScene(scene);
			objexp.Run();
			Log("OutputScene success ");
		}
    }
}
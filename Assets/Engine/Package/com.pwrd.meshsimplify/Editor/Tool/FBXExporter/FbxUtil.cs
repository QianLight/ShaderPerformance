using System.Collections;
using System.Collections.Generic;
using Athena.MeshSimplify;
using Autodesk.Fbx;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.FBX
{
    public class FbxEntity
    {
        public UnityEngine.Vector3 unityTranslate;
        public FbxDouble3 fbxRotate;
        public UnityEngine.Vector3 unityScale;
        public Matrix4x4 matrix;

        internal ModelExporter.MeshInfo meshInfo;
        public Dictionary<TextureChannel, string> textures;

        public FbxEntity(Mesh mesh)
        {
            meshInfo = new ModelExporter.MeshInfo(mesh, null);
            textures = new Dictionary<TextureChannel, string>();
        }

        public void AddTex(TextureChannel channel, string path)
        {
            if (textures.ContainsKey(channel))
            {
                textures[channel] = path;
            }
            else
            {
                textures.Add(channel, path);
            }
        }
    }

    public class FbxBuilder
    {
        const string Title =
            "Created by FBX Exporter from Unity Technologies";

        const string Subject =
            "";

        const string Keywords =
            "Nodes Meshes Materials Textures Cameras Lights Skins Animation";

        const string Comments =
            @"";

        internal const string PACKAGE_UI_NAME = "FBX Exporter";

        const string ProgressBarTitle = "FBX Export";

        internal const float UnitScaleFactor = 100f;

        private List<FbxEntity> entities;

        public FbxBuilder()
        {
            entities = new List<FbxEntity>();
        }

        public void Add(FbxEntity entity)
        {
            entities.Add(entity);
        }

        static bool exportCancelled = false;

        static bool ExportProgressCallback(float percentage, string status)
        {
            // Convert from percentage to [0,1].
            // Then convert from that to [0.5,1] because the first half of
            // the progress bar was for creating the scene.
            var progress01 = 0.5f * (1f + (percentage / 100.0f));

            bool cancel =
                EditorUtility.DisplayCancelableProgressBar(ProgressBarTitle, "Exporting Scene...", progress01);

            if (cancel)
            {
                exportCancelled = true;
            }

            // Unity says "true" for "cancel"; FBX wants "true" for "continue"
            return !cancel;
        }

        internal static string GetVersionFromReadme()
        {
            return "0.0.1";
        }

        internal static FbxVector4 ConvertToFbxVector4(Vector3 leftHandedVector, float unitScale = 1f)
        {
            // negating the x component of the vector converts it from left to right handed coordinates
            return unitScale * new FbxVector4(
                leftHandedVector[0],
                leftHandedVector[1],
                leftHandedVector[2]);
        }

        bool ExportTexture(FbxEntity meshInfo, FbxNode fbxNode, FbxSurfacePhong fbxMaterial, string channel)
        {
            // Find the corresponding property on the fbx material.
            var fbxMaterialProperty = fbxMaterial.FindProperty(channel);
            if (fbxMaterialProperty == null || !fbxMaterialProperty.IsValid())
            {
                Debug.Log("property not found");
                return false;
            }

            var textureName = fbxNode.GetName() + "_Tex";
            var fbxTexture = FbxFileTexture.Create(fbxMaterial, textureName);
            if (FbxSurfaceMaterial.sDiffuse == channel)
            {
                if (meshInfo.textures.ContainsKey(TextureChannel.Albedo))
                    fbxTexture.SetFileName(meshInfo.textures[TextureChannel.Albedo]);
            }
            else if (FbxSurfaceMaterial.sNormalMap == channel)
            {
                if (meshInfo.textures.ContainsKey(TextureChannel.Normal))
                    fbxTexture.SetFileName(meshInfo.textures[TextureChannel.Normal]);
            }
            else if (FbxSurfaceMaterial.sSpecular == channel)
            {
                if (meshInfo.textures.ContainsKey(TextureChannel.Metallic))
                    fbxTexture.SetFileName(meshInfo.textures[TextureChannel.Metallic]);
            }

            else if (FbxSurfaceMaterial.sAmbient == channel)
            {
                if (meshInfo.textures.ContainsKey(TextureChannel.LightMap))
                    fbxTexture.SetFileName(meshInfo.textures[TextureChannel.LightMap]);
            }

            fbxTexture.SetTextureUse(FbxTexture.ETextureUse.eStandard);
            fbxTexture.SetMappingType(FbxTexture.EMappingType.eUV);
            fbxTexture.ConnectDstProperty(fbxMaterialProperty);
            return true;
        }

        bool ExportMesh(FbxEntity meshEntity, FbxNode fbxNode)
        {
            // create the mesh structure.
            var fbxScene = fbxNode.GetScene();
            FbxMesh fbxMesh = FbxMesh.Create(fbxScene, "Scene");

            {
                var vertices = meshEntity.meshInfo.Vertices;
                fbxMesh.InitControlPoints(vertices.Length);
                for (int v = 0, n = meshEntity.meshInfo.Vertices.Length; v < n; v++)
                {
                    fbxMesh.SetControlPointAt(ConvertToFbxVector4(vertices[v], UnitScaleFactor), v);
                }
            }

            var unmergedPolygons = new List<int>();
            var mesh = meshEntity.meshInfo.mesh;
            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                var topology = mesh.GetTopology(s);
                var indices = mesh.GetIndices(s);

                int polySize;
                int[] vertOrder;

                switch (topology)
                {
                    case MeshTopology.Triangles:
                        polySize = 3;
                        vertOrder = new int[] { 0, 1, 2 };
                        break;
                    case MeshTopology.Quads:
                        polySize = 4;
                        vertOrder = new int[] { 0, 1, 2, 3 };
                        break;
                    case MeshTopology.Lines:
                        throw new System.NotImplementedException();
                    case MeshTopology.Points:
                        throw new System.NotImplementedException();
                    case MeshTopology.LineStrip:
                        throw new System.NotImplementedException();
                    default:
                        throw new System.NotImplementedException();
                }

                for (int f = 0; f < indices.Length / polySize; f++)
                {
                    fbxMesh.BeginPolygon();

                    foreach (int val in vertOrder)
                    {
                        int polyVert = indices[polySize * f + val];

                        // Save the polygon order (without merging vertices) so we
                        // properly export UVs, normals, binormals, etc.
                        unmergedPolygons.Add(polyVert);

                        //polyVert = ControlPointToIndex [meshInfo.Vertices [polyVert]];
                        fbxMesh.AddPolygon(polyVert);
                    }

                    fbxMesh.EndPolygon();
                }
            }

            //如果后续需要用shared material 或者 shared texture的话，需要用map记录
            var fbxMaterial = FbxSurfacePhong.Create(fbxScene, fbxNode.GetName() + "_mat");
            ExportTexture(meshEntity, fbxNode, fbxMaterial, FbxSurfaceMaterial.sDiffuse);
            ExportTexture(meshEntity, fbxNode, fbxMaterial, FbxSurfaceMaterial.sNormalMap);
            ExportTexture(meshEntity, fbxNode, fbxMaterial, FbxSurfaceMaterial.sSpecular);
            ExportTexture(meshEntity, fbxNode, fbxMaterial, FbxSurfaceMaterial.sAmbient);

            fbxNode.AddMaterial(fbxMaterial);

            FbxLayer fbxLayer = fbxMesh.GetLayer(0 /* default layer */);
            if (fbxLayer == null)
            {
                fbxMesh.CreateLayer();
                fbxLayer = fbxMesh.GetLayer(0 /* default layer */);
            }

            //上层是通过Camera.Render的方式将UV展开，这样就可以不用考虑多材质了，因为多材质已经合并到了一起
            using (var fbxLayerElement = FbxLayerElementMaterial.Create(fbxMesh, "Material"))
            {
                // if there is only one material then set everything to that material
                fbxLayerElement.SetMappingMode(FbxLayerElement.EMappingMode.eAllSame);
                fbxLayerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eIndexToDirect);

                FbxLayerElementArray fbxElementArray = fbxLayerElement.GetIndexArray();
                fbxElementArray.Add(0);

                fbxLayer.SetMaterials(fbxLayerElement);
            }

            if (meshEntity.meshInfo.HasValidNormals())
            {
                using (var fbxLayerElement = FbxLayerElementNormal.Create(fbxMesh, "Normals"))
                {
                    fbxLayerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                    fbxLayerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

                    FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray();
                    for (int n = 0; n < meshEntity.meshInfo.Normals.Length; n++)
                    {
                        fbxElementArray.Add(ConvertToFbxVector4(meshEntity.meshInfo.Normals[n]));
                    }

                    fbxLayer.SetNormals(fbxLayerElement);
                }
            }

            /// Set the tangents on Layer 0.
            if (meshEntity.meshInfo.HasValidBinormals())
            {
                using (var fbxLayerElement = FbxLayerElementTangent.Create(fbxMesh, "Tangents"))
                {
                    fbxLayerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                    fbxLayerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

                    // Add one normal per each vertex face index (3 per triangle)
                    FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray();

                    for (int n = 0; n < meshEntity.meshInfo.Binormals.Length; n++)
                    {
                        fbxElementArray.Add(ConvertToFbxVector4(
                            meshEntity.meshInfo.Binormals[n]));
                    }

                    fbxLayer.SetTangents(fbxLayerElement);
                }
            }

            /// Set the tangents on Layer 0.
            if (meshEntity.meshInfo.HasValidTangents()) {
                using (var fbxLayerElement = FbxLayerElementTangent.Create (fbxMesh, "Tangents")) {
                    fbxLayerElement.SetMappingMode (FbxLayerElement.EMappingMode.eByControlPoint);
                    fbxLayerElement.SetReferenceMode (FbxLayerElement.EReferenceMode.eDirect);

                    // Add one normal per each vertex face index (3 per triangle)
                    FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray ();

                    for (int n = 0; n < meshEntity.meshInfo.Tangents.Length; n++) {
                        fbxElementArray.Add (ConvertToFbxVector4 (
                            new Vector3 (
                                meshEntity.meshInfo.Tangents [n] [0],
                                meshEntity.meshInfo.Tangents [n] [1],
                                meshEntity.meshInfo.Tangents [n] [2]
                            )));
                    }
                    fbxLayer.SetTangents (fbxLayerElement);
                }
            }

            //我们暂时只导出uv2，如果要导出其他uv，请参考FBXExporter 398行
            using (var fbxLayerElement = FbxLayerElementUV.Create(fbxMesh, "UVSet"))
            {
                // fbxLayerElement.SetMappingMode (FbxLayerElement.EMappingMode.eByControlPoint);
                // fbxLayerElement.SetReferenceMode (FbxLayerElement.EReferenceMode.eDirect);
                //
                // // set texture coordinates per vertex
                // FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray ();
                //
                // // (Uni-31596) only copy unique UVs into this array, and index appropriately
                // for (int n = 0; n < mesh.uv2.Length; n++) {
                //     fbxElementArray.Add (new FbxVector2 (mesh.uv2 [n] [0],
                //         mesh.uv2 [n] [1]));
                // }
                //
                // // // For each face index, point to a texture uv
                // // FbxLayerElementArray fbxIndexArray = fbxLayerElement.GetIndexArray ();
                // // fbxIndexArray.SetCount (unmergedTriangles.Length);
                // //
                // // for(int j = 0; j < unmergedTriangles.Length; j++){
                // //     fbxIndexArray.SetAt (j, unmergedTriangles [j]);
                // // }
                // fbxLayer.SetUVs (fbxLayerElement, FbxLayerElement.EType.eTextureDiffuse);
                fbxLayerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                fbxLayerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

                // set texture coordinates per vertex
                FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray();

                Vector2[] uv2 = meshEntity.meshInfo.mesh.uv2;
                if (uv2 == null || uv2.Length == 0)
                {
                    uv2 = meshEntity.meshInfo.mesh.uv;
                }
                // (Uni-31596) only copy unique UVs into this array, and index appropriately
                for (int n = 0; n < uv2.Length; n++)
                {
                    fbxElementArray.Add(new FbxVector2(uv2[n][0],
                        uv2[n][1]));
                }

                // For each face index, point to a texture uv
                // FbxLayerElementArray fbxIndexArray = fbxLayerElement.GetIndexArray();
                // fbxIndexArray.SetCount(unmergedPolygons.Count);
                //
                // for (int j = 0; j < unmergedPolygons.Count; j++)
                // {
                //     fbxIndexArray.SetAt(j, unmergedPolygons[j]);
                // }

                fbxLayer.SetUVs(fbxLayerElement, FbxLayerElement.EType.eTextureDiffuse);
            }

            if (meshEntity.meshInfo.HasValidVertexColors())
            {
                using (var fbxLayerElement = FbxLayerElementVertexColor.Create(fbxMesh, "VertexColors"))
                {
                    fbxLayerElement.SetMappingMode(FbxLayerElement.EMappingMode.eByControlPoint);
                    fbxLayerElement.SetReferenceMode(FbxLayerElement.EReferenceMode.eDirect);

                    // set texture coordinates per vertex
                    FbxLayerElementArray fbxElementArray = fbxLayerElement.GetDirectArray();

                    // (Uni-31596) only copy unique UVs into this array, and index appropriately
                    for (int n = 0; n < meshEntity.meshInfo.VertexColors.Length; n++)
                    {
                        // Converting to Color from Color32, as Color32 stores the colors
                        // as ints between 0-255, while FbxColor and Color
                        // use doubles between 0-1
                        Color color = meshEntity.meshInfo.VertexColors[n];
                        fbxElementArray.Add(new FbxColor(color.r,
                            color.g,
                            color.b,
                            color.a));
                    }

                    // For each face index, point to a texture uv
                    // FbxLayerElementArray fbxIndexArray = fbxLayerElement.GetIndexArray ();
                    // fbxIndexArray.SetCount (unmergedTriangles.Length);
                    //
                    // for (int i = 0; i < unmergedTriangles.Length; i++) {
                    //     fbxIndexArray.SetAt (i, unmergedTriangles [i]);
                    // }
                    fbxLayer.SetVertexColors(fbxLayerElement);
                }
            }

            // set the fbxNode containing the mesh
            fbxNode.SetNodeAttribute(fbxMesh);
            fbxNode.SetShadingMode(FbxNode.EShadingMode.eWireFrame);
            return true;
        }

        private void GetTRSFromMatrix(Matrix4x4 unityMatrix, out FbxVector4 translation, out FbxVector4 rotation, out FbxVector4 scale){
            // FBX is transposed relative to Unity: transpose as we convert.
            FbxMatrix matrix = new FbxMatrix ();
            matrix.SetColumn (0, new FbxVector4 (unityMatrix.GetRow (0).x, unityMatrix.GetRow (0).y, unityMatrix.GetRow (0).z, unityMatrix.GetRow (0).w));
            matrix.SetColumn (1, new FbxVector4 (unityMatrix.GetRow (1).x, unityMatrix.GetRow (1).y, unityMatrix.GetRow (1).z, unityMatrix.GetRow (1).w));
            matrix.SetColumn (2, new FbxVector4 (unityMatrix.GetRow (2).x, unityMatrix.GetRow (2).y, unityMatrix.GetRow (2).z, unityMatrix.GetRow (2).w));
            matrix.SetColumn (3, new FbxVector4 (unityMatrix.GetRow (3).x, unityMatrix.GetRow (3).y, unityMatrix.GetRow (3).z, unityMatrix.GetRow (3).w));

            // FBX wants translation, rotation (in euler angles) and scale.
            // We assume there's no real shear, just rounding error.
            FbxVector4 shear;
            double sign;
            matrix.GetElements (out translation, out rotation, out shear, out scale, out sign);
        }
        
        private void ExportAll(FbxScene fbxScene, FbxNode root)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                FbxNode fbxNode = FbxNode.Create(fbxScene, fbxScene.GetNodeCount().ToString());

                // fbxNode.LclTranslation.Set(new FbxDouble3(entities[i].unityTranslate.x * UnitScaleFactor,
                //     entities[i].unityTranslate.y * UnitScaleFactor,
                //     entities[i].unityTranslate.z * UnitScaleFactor));
                // fbxNode.LclRotation.Set(entities[i].fbxRotate);
                // fbxNode.LclScaling.Set(new FbxDouble3(entities[i].unityScale.x, entities[i].unityScale.y,
                //     entities[i].unityScale.z));
                
                FbxVector4 translation, rotation, scale;
                GetTRSFromMatrix (entities[i].matrix, out translation, out rotation, out scale);

                // Export bones with zero rotation, using a pivot instead to set the rotation
                // so that the bones are easier to animate and the rotation shows up as the "joint orientation" in Maya.
                fbxNode.LclTranslation.Set (new FbxDouble3(translation.X*UnitScaleFactor, translation.Y*UnitScaleFactor, translation.Z*UnitScaleFactor));
                fbxNode.LclRotation.Set (new FbxDouble3(0,0,0));
                fbxNode.LclScaling.Set (new FbxDouble3 (scale.X, scale.Y, scale.Z));

                // TODO (UNI-34294): add detailed comment about why we export rotation as pre-rotation (日：这段代码不得不加)
                fbxNode.SetRotationActive (true);
                fbxNode.SetPivotState (FbxNode.EPivotSet.eSourcePivot, FbxNode.EPivotState.ePivotReference);
                fbxNode.SetPreRotation (FbxNode.EPivotSet.eSourcePivot, new FbxVector4 (rotation.X, rotation.Y, rotation.Z));

                ExportMesh(entities[i], fbxNode);

                root.AddChild(fbxNode);
            }
        }

        public int Export(string outPath)
        {
            bool status = false;
            using (var fbxManager = FbxManager.Create())
            {
                // Configure fbx IO settings.
                fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));

                // Create the exporter
                var fbxExporter = FbxExporter.Create(fbxManager, "Exporter");

                // Initialize the exporter.
                // fileFormat must be binary if we are embedding textures
                int fileFormat = -1;
                //fileFormat = fbxManager.GetIOPluginRegistry().FindWriterIDByDescription("FBX ascii (*.fbx)");
                status = fbxExporter.Initialize(outPath, fileFormat, fbxManager.GetIOSettings());
                // Check that initialization of the fbxExporter was successful
                if (!status)
                    return 0;

                // Set compatibility to 2014
                fbxExporter.SetFileExportVersion("FBX201400");

                // Set the progress callback.
                //fbxExporter.SetProgressCallback(ExportProgressCallback);

                // Create a scene
                var fbxScene = FbxScene.Create(fbxManager, "Scene");

                // set up the scene info
                FbxDocumentInfo fbxSceneInfo = FbxDocumentInfo.Create(fbxManager, "SceneInfo");
                fbxSceneInfo.mTitle = Title;
                fbxSceneInfo.mSubject = Subject;
                fbxSceneInfo.mAuthor = "Unity Technologies";
                fbxSceneInfo.mRevision = "1.0";
                fbxSceneInfo.mKeywords = Keywords;
                fbxSceneInfo.mComment = Comments;
                fbxSceneInfo.Original_ApplicationName.Set(string.Format("Unity {0}", PACKAGE_UI_NAME));
                // set last saved to be the same as original, as this is a new file.
                fbxSceneInfo.LastSaved_ApplicationName.Set(fbxSceneInfo.Original_ApplicationName.Get());

                var version = GetVersionFromReadme();
                if (version != null)
                {
                    fbxSceneInfo.Original_ApplicationVersion.Set(version);
                    fbxSceneInfo.LastSaved_ApplicationVersion.Set(fbxSceneInfo.Original_ApplicationVersion.Get());
                }

                fbxScene.SetSceneInfo(fbxSceneInfo);

                // Set up the axes (Y up, Z forward, X to the right) and units (centimeters)
                // Exporting in centimeters as this is the default unit for FBX files, and easiest
                // to work with when importing into Maya or Max
                var fbxSettings = fbxScene.GetGlobalSettings();
                fbxSettings.SetSystemUnit(FbxSystemUnit.cm);

                // The Unity axis system has Y up, Z forward, X to the right (left handed system with odd parity).
                // DirectX has the same axis system, so use this constant.
                var unityAxisSystem = FbxAxisSystem.DirectX;
                fbxSettings.SetAxisSystem(unityAxisSystem);

                // export set of object
                FbxNode fbxRootNode = fbxScene.GetRootNode();
                ExportAll(fbxScene, fbxRootNode);

                // The Maya axis system has Y up, Z forward, X to the left (right handed system with odd parity).
                // We need to export right-handed for Maya because ConvertScene (used by Maya and Max importers) can't switch handedness:
                // https://forums.autodesk.com/t5/fbx-forum/get-confused-with-fbxaxissystem-convertscene/td-p/4265472
                // This needs to be done last so that everything is converted properly.
                FbxAxisSystem.MayaYUp.DeepConvertScene(fbxScene);

                // Export the scene to the file.
                status = fbxExporter.Export(fbxScene);

                // cleanup
                fbxScene.Destroy();
                fbxExporter.Destroy();
            }

            if (exportCancelled)
            {
                Debug.LogWarning("Export Cancelled");
                return 0;
            }

            return 0;
        }
    }

    public static class FbxUtil
    {
    }
}
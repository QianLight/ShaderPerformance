using System;
using System.Runtime.InteropServices;
using Athena.MeshSimplify;

namespace com.pwrd.hlod.editor
{
    internal static class SimplygonBridge
    {
        private static Athena.MeshSimplify.SimplygonEntity Entity = new Athena.MeshSimplify.SimplygonEntity();
        
        public static bool InitSDK()
        {
            return Entity.InitSDK();
        }
        
        public static void SetLogCallback(Action<string> callback)
        {
            Entity.SetLogCallback(callback);
        }
        
        public static void SetTempFolderPath(string tempFolderPath)
        {
            Entity.SetTempFolderPath(tempFolderPath);
        }

        public static void Release()
        {
            Entity.Release();
        }
        
        public static void DeInitSDK()
        {
            Entity.DeInitSDK();
        }
        
        public static void AddMeshNode(string name, float[] matrixArr, int vertex_count, float[] vertex_coordinates,
            int triangle_count, int[] corner_ids, float[] texture_coordinates, float[] texture_coordinates2,
            float[] normals)
        {
            Entity.AddMeshNode(name, matrixArr, vertex_count, vertex_coordinates, triangle_count, corner_ids, texture_coordinates, texture_coordinates2, normals);
        }
        
        public static void AddTextureMaterial(string name, string texturePath, string lightmapPath)
        {
            Entity.AddTextureMaterial(name, texturePath, lightmapPath);
        }
        
        public static void AddTextureMaterial(string name, string texturePath)
        {
            Entity.AddTextureMaterial(name, texturePath);
        }
        
        public static void AddTextureMaterialByChannel(string name, string texturePath, TextureChannel channel)
        {
            Entity.AddTextureMaterialByChannel(name, texturePath, channel);
        }
        
        public static void BindMeshAndMaterial(string meshName, string matName)
        {
            Entity.BindMeshAndMaterial(meshName, matName);
        }
        
        public static void RunReductionProcessing(float triangleRatio)
        {
            Entity.RunReductionProcessing(triangleRatio);
        }
        
        public static void RunReductionProcessing(ReductionSetting setting)
        {
            Entity.RunReductionProcessing(setting);
        }
        
        public static void RunReductionByScreenSize(int screenSize)
        {
            Entity.RunReductionByScreenSize(screenSize);
        }
        public static void RunReductionProcessingUseOcclusion(float triangleRatio, float pitchAngle, float yawAngle, float coverage)
        {
            Entity.RunReductionProcessingUseOcclusion(triangleRatio, pitchAngle, yawAngle, coverage);
        }
        
        public static void RunReductionByScreenSizeUseOcclusion(int screenSize, float pitchAngle, float yawAngle, float coverage)
        {
            Entity.RunReductionByScreenSizeUseOcclusion(screenSize, pitchAngle, yawAngle, coverage);
        }
        
        public static void RunAggregate(int width, int height, TextureChannel textureChannel)
        {
            Entity.RunAggregate(width, height, textureChannel);
        }
        
        public static void ExportScene(string outputPath)
        {
            Entity.ExportScene(outputPath);
        }
        
        public static void CreateNewScene()
        {
            Entity.CreateNewScene();
        }
        
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool LoadLibrary(string name);
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFEngine
{
    public class ScatterEditor
    {
        public static void SaveDefaultScatter()
        {
            SaveScatterRes("Assets/Engine/Runtime/Res", "Default", out SavedScatterInfo info);
            PostProcessResources resource =
                AssetDatabase.LoadAssetAtPath<PostProcessResources>("Assets/Engine/Runtime/Res/EngineResources.asset");
            resource.defaultScatterInfo.lut = info.lut;
            resource.defaultScatterInfo.param = info.param;
            EditorUtility.SetDirty(resource);
            AssetDatabase.SaveAssets();
        }

        public static void SaveCurrentProfileScatter(string path)
        {
            Skybox skybox = RenderLayer.envProfile.Get<Skybox>();
            if (!skybox)
            {
                Debug.LogError("RenderLayer还没添加Skybox组件。");
                return;
            }

            string name = "";
            if (string.IsNullOrEmpty(path))
            {
                Scene scene = SceneManager.GetActiveScene();
                path = Path.GetDirectoryName(scene.path);
                name = scene.name;
            }
            else
            {
                AssetsPath.GetFileName(path, out name);
                path = Path.GetDirectoryName(path);
            }

            if (SaveScatterRes(path, name, out var info))
            {
                skybox.scatterParams.overrideState = true;
                skybox.scatterParams.value = info.param;

                skybox.scatterTex.overrideState = true;
                skybox.scatterTex.asset = info.lut;
                skybox.scatterTex.value = info.lut.name;
            }
        }

        public static bool SaveScatterRes(string directory, string name, out SavedScatterInfo info)
        {
            info = new SavedScatterInfo();

            name = $"{name}_ScatterTex";
            string path = $"{directory}/{name}.exr".Replace('\\', '/');

            // Save scatter params
            Vector4 param;
            param = Shader.GetGlobalVector("_ScatteringSunDirection");
            param.w = Shader.GetGlobalFloat("_ScatteringExposure");
            info.param = param;

            // Save scatter texture
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            RenderTexture rt = Shader.GetGlobalTexture("_PrecomputeScatterTex") as RenderTexture;
            if (!rt)
            {
                EditorUtility.DisplayDialog("Error", "保存大气散射图失败，请先正确搭建Skybox环境再保存。", "OK");
                return false;
            }

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false, true);
            texture.name = name;
            texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
            Color[] pixels = texture.GetPixels();
            texture.SetPixels(pixels);
            texture.Apply();
            byte[] bytes = texture.EncodeToEXR();
            Object.DestroyImmediate(texture);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            RenderTexture.active = active;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.sRGBTexture = false;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 0;

            TextureImporterPlatformSettings pc = importer.GetPlatformTextureSettings("PC");
            pc.overridden = true;
            pc.format = TextureImporterFormat.RGBAHalf;
            pc.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            importer.SetPlatformTextureSettings(pc);

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.format = TextureImporterFormat.RGBAHalf;
            android.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            importer.SetPlatformTextureSettings(android);

            TextureImporterPlatformSettings iPhone = importer.GetPlatformTextureSettings("iPhone");
            iPhone.overridden = true;
            iPhone.format = TextureImporterFormat.RGBAHalf;
            iPhone.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            importer.SetPlatformTextureSettings(iPhone);

            importer.SaveAndReimport();

            info.lut = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            return true;
        }
    }
}
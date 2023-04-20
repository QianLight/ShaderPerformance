using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
#endif
using UnityEngine;


namespace CFEngine
{
    public class LightmapCombineManager
    {
        private static bool isUseLightmapCombine = false;

        private static LightmapCombineManager _instance;

        public static LightmapCombineManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LightmapCombineManager();
                }

                return _instance;
            }
        }
#if UNITY_EDITOR

        private Material blurMat;
        private readonly int _BlurOffset = Shader.PropertyToID("_BlurOffset");

        public void BlurTex(Texture2D targetTex, int iteration, float blurOffset)
        {
            if (targetTex == null)
            {
                Debug.Log("targetTex is Null");
                return;
            }

            if (blurMat == null)
            {
                Shader tempBlurShader = Shader.Find("Hidden/PostProcessing/RTBlur2");
                if (tempBlurShader == null)
                {
                    Debug.Log("未找到相关Shader: Hidden/PostProcessing/RTBlur2");
                    return;
                }
                blurMat = new Material(tempBlurShader);
            }

            int width = targetTex.width;
            int height = targetTex.height;
            string texturePath = AssetDatabase.GetAssetPath(targetTex);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear;
            if (textureImporter != null)
            {
                readWrite = textureImporter.sRGBTexture ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear;
            }

            RenderTexture tempRT1 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, readWrite); // 要根据原来的贴图确定是sRGB还是Linear
            RenderTexture tempRT2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, readWrite);
            Texture2D blurTex = new Texture2D(width, height, TextureFormat.RGBAFloat, true, targetTex.mipmapCount > 1);

            CommandBuffer cmd = CommandBufferPool.Get();

            cmd.Blit(targetTex, tempRT1, blurMat, 1);
            for (int i = 0; i < iteration; i++)
            {
                blurMat.SetVector(_BlurOffset, new Vector4(blurOffset / width, 0, 0, 0));
                cmd.Blit(tempRT1, tempRT2, blurMat, 0);
                blurMat.SetVector(_BlurOffset, new Vector4(0, blurOffset / height, 0, 0));
                cmd.Blit(tempRT2, tempRT1, blurMat, 0);
            }

            Graphics.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = tempRT1;

            blurTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);
            // string writePath = texturePath.Replace(targetTex.name, targetTex.name + "_Blur");
            string writePath = texturePath;
            byte[] bytes = blurTex.EncodeToPNG();
            File.WriteAllBytes(writePath, bytes);
            AssetDatabase.Refresh();
        }

        private void GenerateCombineLightmap()
        {
            LightmapData[] lightmapDatas = LightmapSettings.lightmaps;
            Texture2D combineLightmap = null;
            for (int i = 0; i < lightmapDatas.Length; i++)
            {
                combineLightmap = GenerateSignleCombineLightmap("", lightmapDatas[i].lightmapColor, lightmapDatas[i].shadowMask);
                if (combineLightmap == null)
                {
                    Debug.Log("lightmap转换失败： " + lightmapDatas[i].lightmapColor.name);
                    continue;
                }

                lightmapDatas[i].lightmapColor = combineLightmap;
            }

            LightmapSettings.lightmaps = lightmapDatas;
            AssetDatabase.Refresh();
        }

        public Texture2D GenerateSignleCombineLightmap(string generatePath, Texture2D lightmap, Texture2D shadowMask = null)
        {
            if (lightmap == null)
            {
                return null;
            }

            Texture2D newLightmap = CopyTexAndSetReadable(lightmap);
            Texture2D newShadowMask = null;
            if (newLightmap == null)
            {
                return null;
            }

            Texture2D tempTex = new Texture2D(newLightmap.width, newLightmap.height, TextureFormat.RGBAFloat, true, true);
            tempTex.wrapMode = newLightmap.wrapMode;
            tempTex.wrapModeU = newLightmap.wrapModeU;
            tempTex.wrapModeV = newLightmap.wrapModeV;
            tempTex.wrapModeW = newLightmap.wrapModeW;
            tempTex.filterMode = newLightmap.filterMode;
            tempTex.anisoLevel = newLightmap.anisoLevel;
            tempTex.mipMapBias = newLightmap.mipMapBias;

            Color[] lightmapColors = newLightmap.GetPixels();
            Color[] finalColors = new Color[lightmapColors.Length];
            if (shadowMask != null)
            {
                newShadowMask = CopyTexAndSetReadable(shadowMask);
                if (newShadowMask != null)
                {
                    Color[] shadowmaskColors = newShadowMask.GetPixels();
                    if (lightmapColors.Length != shadowmaskColors.Length)
                    {
                        Debug.Log("lightmap 和 shadowmask 贴图尺寸不统一 " + lightmap.name + ", " + shadowMask.name);
                    }

                    int len = Mathf.Min(lightmapColors.Length, shadowmaskColors.Length);
                    for (int i = 0; i < len; i++)
                    {
                        lightmapColors[i].a = GammaToLinearSpace(shadowmaskColors[i].r);
                    }
                }
            }

            tempTex.SetPixels(lightmapColors);

            if (string.IsNullOrEmpty(generatePath))
            {
                string orginPath = AssetDatabase.GetAssetPath(lightmap);
                string tempName = lightmap.name.Replace("Lightmap", "lmap");
                generatePath = orginPath.Replace(lightmap.name, "Lightmap_Combine-" + tempName);
                generatePath = generatePath.Replace(".exr", ".tga");
            }

            string writePath = generatePath.Replace("Assets", "");
            writePath = Application.dataPath + "/" + writePath;
            if (File.Exists(writePath))
            {
                File.Delete(writePath);
            }

            byte[] lightmapBytes = tempTex.EncodeToTGA();
            File.WriteAllBytes(writePath, lightmapBytes);
            AssetDatabase.ImportAsset(generatePath);
            Texture2D resultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(generatePath);

            HandleCombineLightmapSetting(generatePath, lightmap);

            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(newLightmap));
            if (newShadowMask != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(newShadowMask));
            }

            return resultTex;
        }

        private bool HandleCombineLightmapSetting(string targetPath, Texture2D lightmap)
        {
            if (string.IsNullOrEmpty(targetPath) || lightmap == null)
            {
                return false;
            }

            string lightmapPath = AssetDatabase.GetAssetPath(lightmap);
            TextureImporter lightmapTexImporter = AssetImporter.GetAtPath(lightmapPath) as TextureImporter;
            if (lightmapTexImporter == null)
            {
                Debug.LogError("请检查光照贴图是否有误: " + lightmapPath);
                return false;
            }

            AssetImporter tt = AssetImporter.GetAtPath(targetPath);
            TextureImporter combineLightmapTexImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (combineLightmapTexImporter == null)
            {
                Debug.LogError("设置复合光照贴图失败，请检查贴图类型是否为 Texture: " + targetPath);
                return false;
            }

            TextureImporterSettings lightmapImportSettings = new TextureImporterSettings();
            lightmapTexImporter.ReadTextureSettings(lightmapImportSettings);
            lightmapImportSettings.textureType = TextureImporterType.Default;
            lightmapImportSettings.alphaSource = TextureImporterAlphaSource.FromInput;
            combineLightmapTexImporter.SetTextureSettings(lightmapImportSettings);

            //The options for the platform string are "Standalone", "Web", "iPhone", "Android", "WebGL", "Windows Store Apps", "PS4", "PSM", "XboxOne", "Nintendo 3DS" and "tvOS".
            TextureImporterPlatformSettings lightmapImportSettingsPC = lightmapTexImporter.GetPlatformTextureSettings("Standalone");
            TextureImporterPlatformSettings lightmapImportSettingsAndroid = lightmapTexImporter.GetPlatformTextureSettings("Android");
            TextureImporterPlatformSettings lightmapImportSettingsiPhone = lightmapTexImporter.GetPlatformTextureSettings("iPhone");

            combineLightmapTexImporter.SetPlatformTextureSettings(lightmapImportSettingsPC);
            combineLightmapTexImporter.SetPlatformTextureSettings(lightmapImportSettingsAndroid);
            combineLightmapTexImporter.SetPlatformTextureSettings(lightmapImportSettingsiPhone);

            combineLightmapTexImporter.SaveAndReimport();

            return true;
        }

        private Texture2D CopyTexAndSetReadable(Texture2D targetTex)
        {
            if (targetTex == null)
            {
                return null;
            }

            string orginPath = AssetDatabase.GetAssetPath(targetTex);
            string tempName = targetTex.name.Replace("Lightmap", "lmap");
            string targetPath = orginPath.Replace(targetTex.name, "~~temp-" + tempName);
            AssetDatabase.CopyAsset(orginPath, targetPath);
            Texture2D copyLightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
            TextureImporter copyLightmapTexImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            if (copyLightmapTexImporter != null)
            {
                copyLightmapTexImporter.isReadable = true;
                copyLightmapTexImporter.SaveAndReimport();
                return copyLightmap;
            }
            else
            {
                Debug.LogError("获取 TextureImporter 失败，请检查资源类型：" + orginPath);
                AssetDatabase.DeleteAsset(targetPath);
                return null;
            }
        }

        /// <summary>
        /// 合并lightmap和shadowmask，并去除引用
        /// </summary>
        /// <param name="lightmapVolumns"></param>
        public void ConvertLightmap(LightmapVolumn[] lightmapVolumns)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (lightmapVolumns == null)
            {
                Debug.LogError(scene.name + " 传入的 lightmapVolumns 为Null");
                return;
            }

            BrightAOParamSetting brightAOParamSetting;
            GameObject editorObj = GameObject.Find("EditorScene");
            if (editorObj != null)
            {
                if (!editorObj.TryGetComponent<BrightAOParamSetting>(out brightAOParamSetting))
                {
                    brightAOParamSetting = editorObj.AddComponent<BrightAOParamSetting>();
                    brightAOParamSetting.InitParam();
                }
            }

            LightmapVolumn tempLightmapVolume;
            LigthmapRes tempLightmapRes;
            string lightmapCombinePath;
            MeshRenderObject tempMeshRenderObject;
            LigthmapRenderData tempLigthmapRenderData;
            int lightmapCount, lightmapIndex;
            for (int i = 0; i < lightmapVolumns.Length; i++)
            {
                tempLightmapVolume = lightmapVolumns[i];
                // if (!tempLightmapVolume.gameObject.activeSelf) //只转换当前显示的，测试用
                // {
                //     continue;
                // }

                lightmapCount = tempLightmapVolume.res.Length;
                for (int j = 0; j < lightmapCount; j++)
                {
                    tempLightmapRes = tempLightmapVolume.res[j];
                    if (tempLightmapRes == null || tempLightmapRes.color == null || tempLightmapRes.shadowMask == null)
                    {
                        continue;
                    }

                    if ((tempLightmapRes.color.width != tempLightmapRes.shadowMask.width) || (tempLightmapRes.color.height != tempLightmapRes.shadowMask.height))
                    {
                        Debug.Log("## ConvertLightmap失败 ##: " + scene.name + "场景贴图尺寸大小不统一");
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("ConvertLightmap", tempLightmapRes.color.name, i / lightmapVolumns.Length * 1.0f);
                    string combineName = string.Format("{0}_Lightmap_Combine_{1}-{2}.tga", scene.name, tempLightmapVolume.name, j.ToString());
                    lightmapCombinePath = AssetDatabase.GetAssetPath(tempLightmapRes.color);
                    string[] tempStrs = lightmapCombinePath.Split('/');
                    lightmapCombinePath = lightmapCombinePath.Replace(tempStrs[tempStrs.Length - 1], combineName);
                    tempLightmapRes.colorCombineShadowMask = GenerateSignleCombineLightmap(lightmapCombinePath, tempLightmapRes.color, tempLightmapRes.shadowMask);
                    tempLightmapRes.color = null;
                    tempLightmapRes.shadowMask = null;
                    tempLightmapRes.combine = tempLightmapRes.combine ? tempLightmapRes.colorCombineShadowMask : null;
                }

                for (int j = 0; j < tempLightmapVolume.renders.Length; j++)
                {
                    tempLigthmapRenderData = tempLightmapVolume.renders[j];
                    if (tempLigthmapRenderData == null)
                    {
                        continue;
                    }

                    if (tempLigthmapRenderData.render == null)
                    {
                        continue;
                    }

                    tempMeshRenderObject = tempLigthmapRenderData.render.GetComponent<MeshRenderObject>();
                    if (tempMeshRenderObject == null || tempMeshRenderObject.lightmapComponent == null)
                    {
                        continue;
                    }

                    lightmapIndex = tempMeshRenderObject.lightmapComponent.lightMapIndex;
                    if (lightmapIndex > 0 && lightmapIndex < lightmapCount && tempLightmapVolume.res[lightmapIndex] != null)
                    {
                        tempMeshRenderObject.SetLightmapRes(null, null, null, tempLightmapVolume.res[lightmapIndex].colorCombineShadowMask);
                    }
                }
            }

            LightmapVolumn.RenderLightmaps(UnityEngine.Object.FindObjectsOfType<LightmapVolumn>(true));
            SceneAssets.SceneModify(true);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 添加shadowmask和lightmap引用，去除混合光照贴图引用
        /// </summary>
        /// <param name="lightmapVolumns"></param>
        public void RecoverLightmap(LightmapVolumn[] lightmapVolumns)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (lightmapVolumns == null)
            {
                Debug.LogError(scene.name + " 传入的 lightmapVolumns 为Null");
                return;
            }

            LightmapVolumn tempLightmapVolume;
            LigthmapRes tempLightmapRes;
            string lightmapCombinePath, lightmapPath, shadowmaskPath, tempPath;
            Texture2D lightmap, shadowmask;
            MeshRenderObject tempMeshRenderObject;
            LigthmapRenderData tempLigthmapRenderData;
            int lightmapCount, lightmapIndex;
            for (int i = 0; i < lightmapVolumns.Length; i++)
            {
                tempLightmapVolume = lightmapVolumns[i];
                // if (!tempLightmapVolume.gameObject.activeSelf) //只转换当前显示的，测试用
                // {
                //     continue;
                // }

                lightmapCount = tempLightmapVolume.res.Length;
                for (int j = 0; j < lightmapCount; j++)
                {
                    tempLightmapRes = tempLightmapVolume.res[j];
                    if (tempLightmapRes == null || tempLightmapRes.colorCombineShadowMask == null)
                    {
                        continue;
                    }

                    lightmapCombinePath = AssetDatabase.GetAssetPath(tempLightmapRes.colorCombineShadowMask);
                    tempPath = lightmapCombinePath.Replace("Lightmap_Combine", "Lightmap");
                    lightmapPath = tempPath.Replace(".tga", ".exr");
                    shadowmaskPath = tempPath.Replace(".tga", ".png");

                    lightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapPath);
                    shadowmask = AssetDatabase.LoadAssetAtPath<Texture2D>(shadowmaskPath);

                    tempLightmapRes.colorCombineShadowMask = null;
                    tempLightmapRes.color = lightmap;
                    tempLightmapRes.shadowMask = shadowmask;
                    tempLightmapRes.combine = tempLightmapRes.combine ? tempLightmapRes.color : null;
                }

                for (int j = 0; j < tempLightmapVolume.renders.Length; j++)
                {
                    tempLigthmapRenderData = tempLightmapVolume.renders[j];
                    if (tempLigthmapRenderData == null)
                    {
                        continue;
                    }

                    if (tempLigthmapRenderData.render == null)
                    {
                        continue;
                    }

                    tempMeshRenderObject = tempLigthmapRenderData.render.GetComponent<MeshRenderObject>();
                    if (tempMeshRenderObject == null || tempMeshRenderObject.lightmapComponent == null)
                    {
                        continue;
                    }

                    lightmapIndex = tempMeshRenderObject.lightmapComponent.lightMapIndex;
                    if (lightmapIndex > 0 && lightmapIndex < lightmapCount && tempLightmapVolume.res[lightmapIndex] != null)
                    {
                        tempMeshRenderObject.SetLightmapRes(tempLightmapVolume.res[lightmapIndex].color, tempLightmapVolume.res[lightmapIndex].shadowMask, null, null);
                    }
                }
            }

            LightmapVolumn.RenderLightmaps(UnityEngine.Object.FindObjectsOfType<LightmapVolumn>(true));
            SceneAssets.SceneModify(true);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        public void FormatLightmap(LightmapVolumn[] lightmapVolumns, string sceneName)
        {
            if (lightmapVolumns == null)
            {
                Debug.LogError(sceneName + " 传入的 lightmapVolumns 为Null");
                return;
            }

            LightmapVolumn tempLightmapVolume;
            LigthmapRes tempLightmapRes;
            string orginPath;
            string newName;
            for (int i = 0; i < lightmapVolumns.Length; i++)
            {
                tempLightmapVolume = lightmapVolumns[i];
                // if (!tempLightmapVolume.gameObject.activeSelf)
                // {
                //     continue;
                // }
                for (int j = 0; j < tempLightmapVolume.res.Length; j++)
                {
                    tempLightmapRes = tempLightmapVolume.res[j];
                    if (tempLightmapRes == null || tempLightmapRes.color == null)
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("RenameLightmap", tempLightmapRes.color.name, i / lightmapVolumns.Length * 1.0f);
                    newName = string.Format("{0}_Lightmap_{1}-{2}.exr", sceneName, tempLightmapVolume.name, j.ToString());
                    orginPath = AssetDatabase.GetAssetPath(tempLightmapRes.color);
                    AssetDatabase.RenameAsset(orginPath, newName);

                    newName = string.Format("{0}_Lightmap_{1}-{2}.tga", sceneName, tempLightmapVolume.name, j.ToString());
                    orginPath = AssetDatabase.GetAssetPath(tempLightmapRes.shadowMask);
                    AssetDatabase.RenameAsset(orginPath, newName);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
#endif
        private float LinearToGammaSpace(float value)
        {
            if (value <= 0.0F)
                return 0.0F;
            else if (value <= 0.0031308F)
                return 12.92F * value;
            else if (value < 1.0F)
                return 1.055F * Mathf.Pow(value, 0.4166667F) - 0.055F;
            else if (value == 1.0F)
                return 1.0f;
            else
                return Mathf.Pow(value, 0.45454545454545F);
        }

        private float GammaToLinearSpace(float value)
        {
            if (value <= 0.04045F)
                return value / 12.92F;
            else if (value < 1.0F)
                return Mathf.Pow((value + 0.055F) / 1.055F, 2.4F);
            else if (value == 1.0F)
                return 1.0f;
            else
                return Mathf.Pow(value, 2.2F);
        }

        public void SetUseCombineLightmap(bool isUse)
        {
            isUseLightmapCombine = isUse;
        }

        public bool CheckIsUseCombineLightmap()
        {
            return false;
            // if (EngineContext.IsRunning)
            // {
            //     // return WorldSystem.miscConfig.useLightmapCombine;
            //     return isUseLightmapCombine;
            // }
            //
            // return isUseLightmapCombine;
        }
    }
}
﻿using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace CFEngine.Editor
{
    internal class TextureAssets
    {
        internal static TexCompressConfig GetTexureType (string path)
        {
            TexCompressConfig config = null;
            string fileName;
            if (AssetsPath.GetFileName (path, out fileName))
            {
                int priority = 1000;
                for (int i = 0; i < AssetsConfig.instance.texConfig.texConfigs.Count; ++i)
                {
                    var tcc = AssetsConfig.instance.texConfig.texConfigs[i];
                    if (tcc.vaild)
                    {
                        bool find = tcc.type == TexFilterType.Or?false : true;
                        for (int j = 0; j < tcc.compressFilters.Count; ++j)
                        {
                            var cf = tcc.compressFilters[j];
                            string str = cf.str;
                            if (CommonAssets.StrFilter (str, path, fileName))
                            {
                                if (tcc.type == TexFilterType.Or)
                                {
                                    find = true;
                                    break;
                                }

                            }
                            else if (tcc.type == TexFilterType.And)
                            {
                                find = false;
                                break;
                            }
                        }
                        if (find)
                        {
                            if (tcc.priority < priority)
                            {
                                config = tcc;
                                priority = tcc.priority;
                            }
                        }
                    }

                }
                return config;
            }
            return null;
        }

     //   static TextureImporterPlatformSettings iosSetting = new TextureImporterPlatformSettings ();
    //    static TextureImporterPlatformSettings androidSetting = new TextureImporterPlatformSettings ();
    //    static TextureImporterPlatformSettings standaloneSetting = new TextureImporterPlatformSettings ();
        internal static void SetTextureConfig (string path, TextureImporter textureImporter)
        {
            // uint flag = 0;
            // if (!string.IsNullOrEmpty (textureImporter.userData))
            // {
            //     uint.TryParse (textureImporter.userData, out flag);
            // }
            // if (EditorCommon.HasFlag (flag, TexFlag.IgnoreImport))
            // {
            //     return;
            // }
            var config = TextureAssets.GetTexureType (path);
            if (config == null)
                return;
            bool unResize = AssetsConfig.instance.texConfig.unResizePath.Contains(path);
            SetTextureConfig (config, textureImporter, unResize);
        }

        private static bool IsDirty (TexCompressConfig config, TextureImporter textureImporter)
        {
            if (textureImporter.isReadable != config.isReadable ||
                textureImporter.textureType != config.importType ||
                textureImporter.textureShape != config.importShape ||
                textureImporter.sRGBTexture != config.sRGB ||
                textureImporter.mipmapEnabled != config.mipMap ||
                textureImporter.filterMode != config.filterMode ||
                textureImporter.anisoLevel != config.anisoLevel ||
                textureImporter.mipMapBias != config.minBias ||
                (config.isOverride && textureImporter.wrapMode != config.wrapMode))
            {
                return true;
            }
            bool hasAlpha = textureImporter.DoesSourceTextureHaveAlpha();
            if (hasAlpha)
            {
                if (textureImporter.alphaSource != TextureImporterAlphaSource.FromInput)
                    return true;
            }
            else
            {
                if (textureImporter.alphaSource != TextureImporterAlphaSource.None)
                    return true;
            }

            var iso = textureImporter.GetPlatformTextureSettings("iPhone");
            if (!iso.overridden ||
                iso.maxTextureSize != config.iosSetting.maxTextureSize.GetSize() ||
                iso.resizeAlgorithm != TextureResizeAlgorithm.Bilinear ||
                iso.textureCompression != TextureImporterCompression.Compressed ||
                iso.compressionQuality != 80 ||
                iso.crunchedCompression != true ||
                iso.allowsAlphaSplitting != false ||
                iso.format != (hasAlpha ? config.iosSetting.alphaFormat : config.iosSetting.format))
                return true;

            var android = textureImporter.GetPlatformTextureSettings("Android");
            int compressionQualityInt = 100;
            if (config.androidSetting.texCompressorQuality == TextureCompressionQuality.Best)
            {
                compressionQualityInt = 100;
            }else if (config.androidSetting.texCompressorQuality == TextureCompressionQuality.Normal)
            {
                compressionQualityInt = 80;
            }
            else
            {
                compressionQualityInt = 50;
            }
            if (!android.overridden ||
                android.maxTextureSize != config.androidSetting.maxTextureSize.GetSize() ||
                android.resizeAlgorithm != TextureResizeAlgorithm.Bilinear ||
                android.textureCompression != TextureImporterCompression.Compressed ||
                android.compressionQuality != compressionQualityInt ||
                android.crunchedCompression != true ||
                android.allowsAlphaSplitting != false ||
                android.androidETC2FallbackOverride != AndroidETC2FallbackOverride.Quality32BitDownscaled ||
                android.format != (hasAlpha ? config.androidSetting.alphaFormat : config.androidSetting.format))
                return true;

            var standalone = textureImporter.GetPlatformTextureSettings("Standalone");
            if (!standalone.overridden ||
                standalone.maxTextureSize != config.standaloneSetting.maxTextureSize.GetSize() ||
                standalone.resizeAlgorithm != TextureResizeAlgorithm.Bilinear ||
                standalone.textureCompression != TextureImporterCompression.Compressed ||
                standalone.compressionQuality != 80 ||
                standalone.crunchedCompression != true ||
                standalone.allowsAlphaSplitting != false ||
                standalone.format != (hasAlpha ? config.standaloneSetting.alphaFormat : config.standaloneSetting.format))
                return true;
            return false;
        }

        internal static void SetTextureConfig (TexCompressConfig config, TextureImporter textureImporter, bool unResize)
        {
            if (!IsDirty(config, textureImporter))
                return;
            bool hasAlpha = textureImporter.DoesSourceTextureHaveAlpha ();
            if (config.attrOverride)
            {
                textureImporter.isReadable = config.isReadable;
                textureImporter.textureType = config.importType;
                textureImporter.textureShape = config.importShape;
                textureImporter.sRGBTexture = config.sRGB;
                textureImporter.mipmapEnabled = config.mipMap;
                textureImporter.mipMapBias = config.minBias;
                textureImporter.filterMode = config.filterMode;
                if (config.isOverride)
                    textureImporter.wrapMode = config.wrapMode;
                textureImporter.anisoLevel = config.anisoLevel;
                if (hasAlpha)
                {
                    textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                }
                else
                {
                    textureImporter.alphaSource = TextureImporterAlphaSource.None;
                }
            }
            
            TextureImporterPlatformSettings iso = textureImporter.GetPlatformTextureSettings("iPhone");
            iso.name = "iPhone";
            iso.overridden = true;
            iso.maxTextureSize = unResize ? iso.maxTextureSize : config.iosSetting.maxTextureSize.GetSize();
            iso.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            iso.textureCompression = TextureImporterCompression.Compressed;
            iso.compressionQuality = 80;
            iso.crunchedCompression = true;
            iso.allowsAlphaSplitting = false;
            iso.format = hasAlpha ? config.iosSetting.alphaFormat : config.iosSetting.format;

            TextureImporterPlatformSettings android = textureImporter.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = unResize ? android.maxTextureSize : config.androidSetting.maxTextureSize.GetSize();
            //android. = config.androidSetting.texCompressorQuality;
            android.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            android.textureCompression = TextureImporterCompression.Compressed;
            if (config.androidSetting.texCompressorQuality == TextureCompressionQuality.Best)
            {
                android.compressionQuality = 100;
            }else if (config.androidSetting.texCompressorQuality == TextureCompressionQuality.Normal)
            {
                android.compressionQuality = 80;
            }
            else
            {
                android.compressionQuality = 50;
            }
            android.crunchedCompression = true;
            android.allowsAlphaSplitting = false;
            android.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32BitDownscaled;
            android.format = hasAlpha ? config.androidSetting.alphaFormat : config.androidSetting.format;

            TextureImporterPlatformSettings standalone = textureImporter.GetPlatformTextureSettings("Standalone");
            standalone.name = "Standalone";
            standalone.overridden = true;
            standalone.maxTextureSize = unResize ? standalone.maxTextureSize : config.standaloneSetting.maxTextureSize.GetSize ();
            standalone.resizeAlgorithm = TextureResizeAlgorithm.Bilinear;
            standalone.textureCompression = TextureImporterCompression.Compressed;
            standalone.compressionQuality = 80;
            standalone.crunchedCompression = true;
            standalone.allowsAlphaSplitting = false;
            standalone.format = hasAlpha ? config.standaloneSetting.alphaFormat : config.standaloneSetting.format;

            textureImporter.SetPlatformTextureSettings (iso);
            textureImporter.SetPlatformTextureSettings (android);
            textureImporter.SetPlatformTextureSettings (standalone);
        }

        internal static void SetSpriteAtlasConfig (string path)
        {
            SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas> (path);
            if (sa != null)
            {
                SerializedProperty spMipmap = CommonAssets.GetSerializeProperty (sa, "m_EditorData.textureSettings.generateMipMaps");
                SerializedObject so = spMipmap.serializedObject;
                SerializedProperty spReadable = CommonAssets.GetSerializeProperty (so, "m_EditorData.textureSettings.readable");
                spMipmap.boolValue = false;
                spReadable.boolValue = false;

                var iosSettings = sa.GetPlatformSettings ("iPhone");
                var androidSettings = sa.GetPlatformSettings ("Android");
                var standaloneSettings = sa.GetPlatformSettings ("Standalone");
                iosSettings.overridden = true;
                iosSettings.maxTextureSize = 1024;
                iosSettings.format = TextureImporterFormat.ASTC_6x6;
                androidSettings.overridden = true;
                androidSettings.maxTextureSize = 1024;
                androidSettings.format = TextureImporterFormat.ASTC_6x6;
                standaloneSettings.overridden = true;
                standaloneSettings.maxTextureSize = 1024;
                standaloneSettings.format = TextureImporterFormat.RGBA32;

                sa.SetPlatformSettings (iosSettings);
                sa.SetPlatformSettings (androidSettings);
                sa.SetPlatformSettings (standaloneSettings);
                so.ApplyModifiedProperties ();
            }
        }
        private static ReflectFun GetRuntimeMemorySizeFun;
        internal static long GetTexSize (Texture tex)
        {
            if (GetRuntimeMemorySizeFun == null)
            {
                Type textureUtilType = Assembly.GetAssembly (typeof (UnityEditor.Editor)).
                GetTypes ().Where (t => t.Name == "TextureUtil").FirstOrDefault ();
                if (textureUtilType != null)
                {
                    GetRuntimeMemorySizeFun = EditorCommon.GetInternalFunction (
                        textureUtilType,
                        "GetRuntimeMemorySizeLong", true, false, false, false);
                }
            }
            if (GetRuntimeMemorySizeFun != null)
            {
                return (long) GetRuntimeMemorySizeFun.Call (null, new object[] { tex });
            }
            return 0;
        }

        internal static void PackAtlas (RenderTexture rt, Texture2D tex, int offsetX, int offsetY, int sizeX, int sizeY)
        {
            CommandBuffer cb = new CommandBuffer () { name = "Temp CB" };
            cb.SetRenderTarget (rt);
            Material mat = new Material (AssetsConfig.instance.TextureBake);
            mat.mainTexture = tex;

            Rect viewPort = new Rect ();
            viewPort.xMin = (float) offsetX / rt.width;
            viewPort.xMax = viewPort.xMin + (float) sizeX / rt.width;
            viewPort.xMin = viewPort.xMin * 2 - 1.0f;
            viewPort.xMax = viewPort.xMax * 2 - 1.0f;

            viewPort.yMin = (float) (rt.height - offsetY - sizeY) / rt.height;
            viewPort.yMax = (float) (rt.height - offsetY) / rt.height;

            viewPort.yMin = viewPort.yMin * 2 - 1.0f;
            viewPort.yMax = viewPort.yMax * 2 - 1.0f;

            Mesh mesh = RuntimeUtilities.GetScreenMesh (viewPort);
            cb.DrawMesh (mesh, Matrix4x4.identity, mat, 0, 1);
            Graphics.ExecuteCommandBuffer (cb);
            cb.Release ();
            UnityEngine.Object.DestroyImmediate (mesh);
            UnityEngine.Object.DestroyImmediate (mat);
        }

        static float InterpolationCalculate (float x)
        {
            const float A = -1;
            float absX = Mathf.Abs (x);
            float x2 = x * x;
            float x3 = absX * x2;

            if (absX <= 1)
            {
                return 1 - (A + 3) * x2 + (A + 2) * x3;
            }
            else if (absX <= 2)
            {
                return -4 * A + 8 * A * absX - 5 * A * x2 + A * x3;
            }

            return 0;
        }

        internal static void PackAtlas (Texture2D packTex, Texture2D tex, int offsetX, int offsetY, int sizeX, int sizeY)
        {
            int xMin = offsetX;
            int xMax = offsetX + sizeX;

            int yMin = offsetY;
            int yMax = offsetY + sizeY;

            float scaleW = (float) (sizeX - 1) / tex.width;
            float scaleH = (float) (sizeY - 1) / tex.height;

            for (int y = yMin; y < yMax; ++y)
            {
                for (int x = xMin; x < xMax; ++x)
                {
                    float srcCol = Mathf.Min (tex.width - 1, (x - xMin) / scaleW - 0.5f);
                    float srcRow = Mathf.Min (tex.width - 1, (y - yMin) / scaleH - 0.5f);
                    int intCol = Mathf.FloorToInt (srcCol);
                    int intRow = Mathf.FloorToInt (srcRow);
                    float u = srcCol - intCol;
                    float v = srcRow - intRow;

                    Color c = Color.black;
                    for (int row = -1; row <= 2; row += 1)
                    {
                        for (int col = -1; col <= 2; col += 1)
                        {
                            int colX = intCol + col;
                            int rowY = intRow + row;
                            if (colX >= 0 && colX < tex.width && rowY >= 0 && rowY < tex.height)
                            {
                                float f1 = InterpolationCalculate (row - v);
                                float f2 = InterpolationCalculate (col - u);
                                Color currentCol = tex.GetPixel (colX, rowY) * f1 * f2;
                                c += currentCol;
                            }
                        }
                    }
                    packTex.SetPixel (x, y, c);
                }
            }
            //1 padding
            // for (int x = xMin; x < xMax; ++x) {
            //     packTex.SetPixel (x, 0, tex.GetPixel (x, yMax - 1));
            // }
            // for (int y = yMin; y < yMax; ++y) {
            //     packTex.SetPixel (xMax, y, tex.GetPixel (0, y));
            // }
        }
        internal static void GaussBlur (Texture2D tex, string dir)
        {
            // CommandBuffer cb = new CommandBuffer () { name = "Temp CB" };
            // Material mat = new Material (Shader.Find ("Hidden/PostProcessing/BgBlur"));
            // RenderTexture blurRT = new RenderTexture (tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            // {
            //     name = "_BgBlursampleRT",
            //     hideFlags = HideFlags.DontSave,
            //     filterMode = FilterMode.Bilinear,
            //     wrapMode = TextureWrapMode.Clamp,
            //     anisoLevel = 0,
            //     autoGenerateMips = false,
            //     useMipMap = false
            // };
            // blurRT.Create ();
            // cb.SetGlobalTexture ("_MainTex", tex);
            // cb.SetRenderTarget (blurRT);
            // cb.DrawMesh (MeshAssets.fullscreenTriangle, Matrix4x4.identity, mat, 0, 0);
            // MaterialPropertyBlock mpb = new MaterialPropertyBlock ();
            // float widthMod = 1.0f / (1.0f * (1 << 2));
            // mpb.SetFloat ("_BgBlur_DownSampleNum", 3 * widthMod);

            // RenderTexture bgBlurTmpRt = new RenderTexture (tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            // {
            //     name = "bgBlurTmpRt",
            //     hideFlags = HideFlags.DontSave,
            //     filterMode = FilterMode.Bilinear,
            //     wrapMode = TextureWrapMode.Clamp,
            //     anisoLevel = 0,
            //     autoGenerateMips = false,
            //     useMipMap = false
            // };
            // bgBlurTmpRt.Create ();

            // //for (int i = 0; i < 3; i++)
            // //{
            // //    float iterationOffs = (i * 1.0f);
            // //    mpb.SetFloat("_BgBlur_DownSampleNum", 3 * widthMod + iterationOffs);
            // //    cb.SetGlobalTexture("_MainTex", blurRT);
            // //    cb.SetRenderTarget(bgBlurTmpRt);
            // //    cb.DrawMesh(MeshAssets.fullscreenTriangle, Matrix4x4.identity, mat, 0, 1, mpb);
            // //    cb.SetGlobalTexture("_MainTex", bgBlurTmpRt);
            // //    cb.SetRenderTarget(blurRT);
            // //    cb.DrawMesh(MeshAssets.fullscreenTriangle, Matrix4x4.identity, mat, 0, 2, mpb);
            // //}

            // Graphics.ExecuteCommandBuffer (cb);

            // CommonAssets.CreateAsset<Texture2D> (dir, SceneAssets.scene_foldername, ".png", blurRT);
            // cb.Release ();
            // bgBlurTmpRt.Release ();
            // blurRT.Release ();
        }

        internal static void BlitTex2RT (Texture2D tex, RenderTexture rt, int slice = -1)
        {
            CommandBuffer cb = new CommandBuffer () { name = "Temp CB" };
            if (slice >= 0)
            {
                cb.SetRenderTarget (rt, 0, CubemapFace.Unknown, slice);
            }
            cb.Blit (tex, rt);
            Graphics.ExecuteCommandBuffer (cb);
            cb.Release ();
        }

        internal static void BlitRT2Tex (RenderTexture rt, Texture2D tex, bool genMipMap = true)
        {
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels (new Rect (0, 0, rt.width, rt.height), 0, 0);
            tex.Apply (genMipMap);
            RenderTexture.active = prev;
        }

        static CommandBuffer drawCB;
        internal static void BeginDrawRT ()
        {
            drawCB = new CommandBuffer () { name = "Temp CB" };
        }

        internal static void DrawRT (RenderTexture rt, Material mat, int pass = 0)
        {
            if (drawCB != null)
            {
                Mesh mesh = RuntimeUtilities.fullscreenTriangle;
                drawCB.SetRenderTarget (rt);
                drawCB.DrawMesh (mesh, Matrix4x4.identity, mat, 0, pass);
                Graphics.ExecuteCommandBuffer (drawCB);
                drawCB.Clear ();
            }
        }
        internal static void BlitRT (RenderTexture src, RenderTexture des)
        {
            if (drawCB != null)
            {
                Mesh mesh = RuntimeUtilities.fullscreenTriangle;
                drawCB.Blit (src, des);
                Graphics.ExecuteCommandBuffer (drawCB);
                drawCB.Clear ();
            }
        }
        internal static void EndDrawRT ()
        {
            if (drawCB != null)
            {
                drawCB.Release ();
                drawCB = null;
            }
        }

        [MenuItem (@"Assets/Tool/Tex_ReflectionProbe")]
        private static void Tex_ReflectionProbe ()
        {
            CommonAssets.enumTexCube.cb = (cube, texImporter, path, context) =>
            {
                GameObject go = new GameObject (cube.name);
                var rp = go.AddComponent<ReflectionProbe> ();
                rp.mode = ReflectionProbeMode.Custom;
                rp.customBakedTexture = cube;
                go.AddComponent<ReflectProbeParam> ();
                return false;
            };
            CommonAssets.EnumAsset<Cubemap> (CommonAssets.enumTexCube, "ReflectionProbe");
        }

        [MenuItem (@"Assets/Tool/Tex_CreateIBLConfig")]
        private static void Tex_CreateIBLConfig ()
        {
            CommonAssets.enumTexCube.cb = (cube, texImporter, path, context) =>
            {
                int index = path.LastIndexOf (".");
                path = path.Substring (0, index);
                path += ".asset";
                if (!System.IO.File.Exists (path))
                {
                    IBLConfig config = IBLConfig.CreateInstance<IBLConfig> ();
                    config.name = cube.name;
                    EditorCommon.CreateAsset<IBLConfig> (path, ".asset", config);
                }
                return false;
            };
            CommonAssets.EnumAsset<Cubemap> (CommonAssets.enumTexCube, "CreateIBLConfig");
        }

        [MenuItem(@"Assets/Tool/Tex_ReImport")]
        private static void Tex_ReImport()
        {
            CommonAssets.enumTex2D.cb = (tex, texImporter, path, context) =>
            {
                SetTextureConfig(path, texImporter);
                return false;
            };
            CommonAssets.EnumAsset<Texture2D>(CommonAssets.enumTex2D, "ReImport");
        }
        // NOTE: Changing offsets below requires updating all instances of #SSSS_CONSTANTS
        // TODO: This needs to be defined in a single place and shared between C++ and shaders!
        const int SSSS_SUBSURFACE_COLOR_OFFSET = 0;
        const int SSSS_TRANSMISSION_OFFSET = (SSSS_SUBSURFACE_COLOR_OFFSET + 1);
        const int SSSS_BOUNDARY_COLOR_BLEED_OFFSET = (SSSS_TRANSMISSION_OFFSET + 1);
        const int SSSS_DUAL_SPECULAR_OFFSET = (SSSS_BOUNDARY_COLOR_BLEED_OFFSET + 1);
        const int SSSS_KERNEL0_OFFSET = (SSSS_DUAL_SPECULAR_OFFSET + 1);
        const int SSSS_KERNEL0_SIZE = 13;
        const int SSSS_KERNEL1_OFFSET = (SSSS_KERNEL0_OFFSET + SSSS_KERNEL0_SIZE);
        const int SSSS_KERNEL1_SIZE = 9;
        const int SSSS_KERNEL2_OFFSET = (SSSS_KERNEL1_OFFSET + SSSS_KERNEL1_SIZE);
        const int SSSS_KERNEL2_SIZE = 6;
        const int SSSS_KERNEL_TOTAL_SIZE = (SSSS_KERNEL0_SIZE + SSSS_KERNEL1_SIZE + SSSS_KERNEL2_SIZE);
        const int SSSS_TRANSMISSION_PROFILE_OFFSET = (SSSS_KERNEL0_OFFSET + SSSS_KERNEL_TOTAL_SIZE);
        const int SSSS_TRANSMISSION_PROFILE_SIZE = 32;
        const float SSSS_MAX_TRANSMISSION_PROFILE_DISTANCE = 5.0f; // See MaxTransmissionProfileDistance in ComputeTransmissionProfile(), SeparableSSS.cpp
        const float SSSS_MAX_DUAL_SPECULAR_ROUGHNESS = 2.0f;

        static float GetNextSmallerPositiveFloat (float x)
        {
            unsafe
            {
                uint bx = * (uint * ) & x;

                // float are ordered like int, at least for the positive part
                uint ax = bx - 1;

                return *(float * ) & ax;
            }
        }

        //     internal static void CreateSSSProfile ()
        //     {
        //         const bool b16Bit = true;

        // 	// Each row of the texture contains SSS parameters, followed by 3 precomputed kernels. Texture must be wide enough to fit all data.
        // 	const uint Width = SSSS_TRANSMISSION_PROFILE_OFFSET + SSSS_TRANSMISSION_PROFILE_SIZE;

        // 	// at minimum 64 lines (less reallocations)
        // 	FPooledRenderTargetDesc Desc(FPooledRenderTargetDesc::Create2DDesc(FIntPoint(Width, FMath::Max(Height, (uint32)64)), PF_B8G8R8A8, FClearValueBinding::None, 0, TexCreate_None, false));
        // 	if (b16Bit)
        // 	{
        // 		Desc.Format = PF_A16B16G16R16;
        // 	}

        // 	GetRendererModule().RenderTargetPoolFindFreeElement(RHICmdList, Desc, GSSProfiles, TEXT("SSProfiles"));

        // 	// Write the contents of the texture.
        // 	uint32 DestStride;
        // 	uint8* DestBuffer = (uint8*)RHICmdList.LockTexture2D((FTexture2DRHIRef&)GSSProfiles->GetRenderTargetItem().ShaderResourceTexture, 0, RLM_WriteOnly, DestStride, false);

        // 	Color[] TextureRow = new Color[Width];
        // 	// FMemory::Memzero(TextureRow);

        // 	const float FloatScale = GetNextSmallerPositiveFloat((float)0x10000);
        // 	// check((int32)GetNextSmallerPositiveFloat(0x10000) == 0xffff);
        // const uint Height = 1;
        // 	for (uint y = 0; y < Height; ++y)
        // 	{
        // 		FSubsurfaceProfileStruct Data = SubsurfaceProfileEntries[y].Settings;

        // 		// bias to avoid div by 0 and a jump to a different value
        // 		// this basically means we don't want subsurface scattering
        // 		// 0.0001f turned out to be too small to fix the issue (for a small KernelSize)
        // 		const float Bias = 0.009f;

        // 		Data.SubsurfaceColor = Data.SubsurfaceColor.GetClamped();
        // 		Data.FalloffColor = Data.FalloffColor.GetClamped(Bias);

        // 		// to allow blending of the Subsurface with fullres in the shader
        // 		TextureRow[SSSS_SUBSURFACE_COLOR_OFFSET] = Data.SubsurfaceColor;
        // 		TextureRow[SSSS_SUBSURFACE_COLOR_OFFSET].A = 0; // unused

        // 		TextureRow[SSSS_BOUNDARY_COLOR_BLEED_OFFSET] = Data.BoundaryColorBleed;

        // 		float MaterialRoughnessToAverage = Data.Roughness0 * (1.0f - Data.LobeMix) + Data.Roughness1 * Data.LobeMix;
        // 		float AverageToRoughness0 = Data.Roughness0 / MaterialRoughnessToAverage;
        // 		float AverageToRoughness1 = Data.Roughness1 / MaterialRoughnessToAverage;

        // 		TextureRow[SSSS_DUAL_SPECULAR_OFFSET].R = FMath::Clamp(AverageToRoughness0 / SSSS_MAX_DUAL_SPECULAR_ROUGHNESS, 0.0f, 1.0f);
        // 		TextureRow[SSSS_DUAL_SPECULAR_OFFSET].G = FMath::Clamp(AverageToRoughness1 / SSSS_MAX_DUAL_SPECULAR_ROUGHNESS, 0.0f, 1.0f);
        // 		TextureRow[SSSS_DUAL_SPECULAR_OFFSET].B = Data.LobeMix;
        // 		TextureRow[SSSS_DUAL_SPECULAR_OFFSET].A = FMath::Clamp(MaterialRoughnessToAverage / SSSS_MAX_DUAL_SPECULAR_ROUGHNESS, 0.0f, 1.0f);

        // 		//X:ExtinctionScale, Y:Normal Scale, Z:ScatteringDistribution, W:OneOverIOR
        // 		TextureRow[SSSS_TRANSMISSION_OFFSET].R = Data.ExtinctionScale;
        // 		TextureRow[SSSS_TRANSMISSION_OFFSET].G = Data.NormalScale;
        // 		TextureRow[SSSS_TRANSMISSION_OFFSET].B = Data.ScatteringDistribution;
        // 		TextureRow[SSSS_TRANSMISSION_OFFSET].A = 1.0f / Data.IOR;

        // 		ComputeMirroredSSSKernel(&TextureRow[SSSS_KERNEL0_OFFSET], SSSS_KERNEL0_SIZE, Data.SubsurfaceColor, Data.FalloffColor);
        // 		ComputeMirroredSSSKernel(&TextureRow[SSSS_KERNEL1_OFFSET], SSSS_KERNEL1_SIZE, Data.SubsurfaceColor, Data.FalloffColor);
        // 		ComputeMirroredSSSKernel(&TextureRow[SSSS_KERNEL2_OFFSET], SSSS_KERNEL2_SIZE, Data.SubsurfaceColor, Data.FalloffColor);

        // 		ComputeTransmissionProfile(&TextureRow[SSSS_TRANSMISSION_PROFILE_OFFSET], SSSS_TRANSMISSION_PROFILE_SIZE, Data.SubsurfaceColor, Data.FalloffColor, Data.ExtinctionScale);

        // 		// could be lower than 1 (but higher than 0) to range compress for better quality (for 8 bit)
        // 		const float TableMaxRGB = 1.0f;
        // 		const float TableMaxA = 3.0f;
        // 		const FLinearColor TableColorScale = FLinearColor(
        // 			1.0f / TableMaxRGB,
        // 			1.0f / TableMaxRGB,
        // 			1.0f / TableMaxRGB,
        // 			1.0f / TableMaxA);

        // 		const float CustomParameterMaxRGB = 1.0f;
        // 		const float CustomParameterMaxA = 1.0f;
        // 		const FLinearColor CustomParameterColorScale = FLinearColor(
        // 			1.0f / CustomParameterMaxRGB,
        // 			1.0f / CustomParameterMaxRGB,
        // 			1.0f / CustomParameterMaxRGB,
        // 			1.0f / CustomParameterMaxA);

        // 		// each kernel is normalized to be 1 per channel (center + one_side_samples * 2)
        // 		for (int32 Pos = 0; Pos < Width; ++Pos)
        // 		{
        // 			FVector4 C = TextureRow[Pos];

        // 			// Remap custom parameter and kernel values into 0..1
        // 			if (Pos >= SSSS_KERNEL0_OFFSET && Pos < SSSS_KERNEL0_OFFSET + SSSS_KERNEL_TOTAL_SIZE)
        // 			{
        // 				C *= TableColorScale;
        // 				// requires 16bit (could be made with 8 bit e.g. using sample0.w as 8bit scale applied to all samples (more multiplications in the shader))
        // 				C.W *= Data.ScatterRadius / SUBSURFACE_RADIUS_SCALE;
        // 			}
        // 			else
        // 			{
        // 				C *= CustomParameterColorScale;
        // 			}

        // 			if (b16Bit)
        // 			{
        // 				// scale from 0..1 to 0..0xffff
        // 				// scale with 0x10000 and round down to evenly distribute, avoid 0x10000

        // 				uint16* Dest = (uint16*)(DestBuffer + DestStride * y);

        // 				Dest[Pos * 4 + 0] = (uint16)(C.X * FloatScale);
        // 				Dest[Pos * 4 + 1] = (uint16)(C.Y * FloatScale);
        // 				Dest[Pos * 4 + 2] = (uint16)(C.Z * FloatScale);
        // 				Dest[Pos * 4 + 3] = (uint16)(C.W * FloatScale);
        // 			}
        // 			else
        // 			{
        // 				FColor* Dest = (FColor*)(DestBuffer + DestStride * y);

        // 				Dest[Pos] = FColor(FMath::Quantize8UnsignedByte(C.X), FMath::Quantize8UnsignedByte(C.Y), FMath::Quantize8UnsignedByte(C.Z), FMath::Quantize8UnsignedByte(C.W));
        // 			}
        // 		}
        // 	}

        // 	RHICmdList.UnlockTexture2D((FTexture2DRHIRef&)GSSProfiles->GetRenderTargetItem().ShaderResourceTexture, 0, false);
        //     }
        // [MenuItem ("Assets/Tool/Tex_RenameAlphaTex")]
        // static void RenameAlphaTex ()
        // {
        //     CommonAssets.enumMat.cb = (mat, path) =>
        //     {
        //         Color mainColor = Color.white;
        //         Vector4 uvST = new Vector4 (1, 1, 0, 0);
        //         Texture mainTex;
        //         Texture bumpTex;
        //         BlendMode blendMode;
        //         MaterialShaderAssets.GetDefaultMatProperty (mat, out mainTex, out bumpTex, out mainColor, out uvST, out blendMode);
        //         if (blendMode != BlendMode.Opaque && mainTex != null)
        //         {
        //         if (!mainTex.name.EndsWith ("_a"))
        //         {
        //         string texPath = AssetDatabase.GetAssetPath (mainTex);
        //         string ext = Path.GetExtension (texPath);
        //         // string dir = Path.GetDirectoryName (texPath);
        //         string newPath = string.Format ("{0}_a{1}", mainTex.name, ext);
        //         //File.Move (texPath, newPath);
        //         AssetDatabase.RenameAsset (texPath, newPath);
        //             }
        //         }
        //     };
        //     CommonAssets.EnumAsset<Material> (CommonAssets.enumMat, "Reanane");
        //     AssetDatabase.Refresh ();
        //     AssetDatabase.SaveAssets ();
        // }

    }
}
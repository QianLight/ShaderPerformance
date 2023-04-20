using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

namespace AssetCheck
{
    enum eCheckStep
    {
        None,
        WaitProfiler,
        CheckFiles,
        Interval,
        CheckFileOverDraw,
        End
    }
    public class RuntimeRenderCheckBox : MonoBehaviour
    {
        public RenderPipelineAsset overdrawRenderPipelineAsset;
        const int CheckFrameCount = 50;
#if UNITY_2020_2_OR_NEWER
        ProfilerRecorder setPassCallsRecorder;
        ProfilerRecorder drawCallsRecorder;
        ProfilerRecorder verticesRecorder;
#endif
        // ----------------------------------------------------
        AssetCheckRenderInfoConfig assetCheckRenderInfoConfig;
        AssetCheckRenderResultConfig assetCheckRenderResultConfig;
        int assetFileIndex = 0;
        eCheckStep currentCheckStep = eCheckStep.None;
        eCheckStep lastCheckStep = eCheckStep.None;
        long currentStepFrame = 0;
        GameObject gObject = null;
        ParticleSystem[] particleSystems;
        string currentFileName = string.Empty;
        ProfilerRenderData initProfilerRenderData = new ProfilerRenderData();
        List<ProfilerRenderData> currentFileProfilerRenderDatas = new List<ProfilerRenderData>();
        Dictionary<string, List<ProfilerRenderData>> dicProfilerRenderDatas = new Dictionary<string, List<ProfilerRenderData>>();
        
        Camera mainCamera;
        RenderTexture cameraRenderTarget;
        Texture2D screenShot;
        void Start()
        {
            mainCamera = Camera.main;
            mainCamera.clearFlags = CameraClearFlags.Color;
            mainCamera.backgroundColor = Color.black;
            mainCamera.allowHDR = false;
            mainCamera.allowMSAA = false;
            mainCamera.cullingMask = -1;

            cameraRenderTarget = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            screenShot = new Texture2D(512, 512, TextureFormat.ARGB32, false);
            mainCamera.targetTexture = cameraRenderTarget;
            RenderTexture.active = cameraRenderTarget;

            string filePath = $"{RuntimeDefines.CheckPathRuntimeConfigPath}/{RuntimeDefines.CheckPathRuntimeRenderConfig}";
            if(!File.Exists(filePath))
            {
                ApplicationEnd();
                return;
            }
            assetCheckRenderInfoConfig = JsonUtility.FromJson<AssetCheckRenderInfoConfig>(File.ReadAllText(filePath));
            File.Delete(filePath);
            assetCheckRenderResultConfig = new AssetCheckRenderResultConfig();
            assetCheckRenderResultConfig.exportCSV = assetCheckRenderInfoConfig.exportCSV;
            assetCheckRenderResultConfig.checkEndCloseApp = assetCheckRenderInfoConfig.checkEndCloseApp;
            assetCheckRenderResultConfig.assetCheckPathConfig = assetCheckRenderInfoConfig.assetCheckPathConfig ?? string.Empty;
        }

        private void OnEnable()
        {
#if UNITY_2020_2_OR_NEWER
            setPassCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
#endif
            RenderPipelineManager.endCameraRendering += OnEnCameraRendering;
        }

        private void OnDisable()
        {
#if UNITY_2020_2_OR_NEWER
            setPassCallsRecorder.Dispose();
            drawCallsRecorder.Dispose();
            verticesRecorder.Dispose();
#endif
            RenderPipelineManager.endCameraRendering -= OnEnCameraRendering;
            if (OverDrawSRPHelper.IsUseUniversalRenderPipeline())
            {
                OverDrawSRPHelper.ResetRenderPipeline();
            }
            else
            {
                mainCamera.ResetReplacementShader();
            }
        }

        void Update()
        {
            if (currentCheckStep == eCheckStep.None)
            {
                SetStep(eCheckStep.WaitProfiler);
            }
            else if (currentCheckStep == eCheckStep.WaitProfiler)
            {
                UpdateWaitProfilerInitData();
            }
            else if (currentCheckStep == eCheckStep.CheckFiles)
            {
                UpdateCheckFiles();
            }
            else if(currentCheckStep == eCheckStep.Interval)
            {
                if (currentStepFrame >= 10)
                    BackToLastStep();
            }
            else if(currentCheckStep == eCheckStep.CheckFileOverDraw)
            {
            }
            else if(currentCheckStep == eCheckStep.End)
            {
                ApplicationEnd();
            }
            else { }
            ++currentStepFrame;
            UpdateProfilerRecorder();
        }

        void OnEnCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            OnPostRender();
        }

        void OnPostRender()
        {
            if(currentCheckStep == eCheckStep.CheckFileOverDraw)
            {
                UpdateOverDrawCheck();
            }
        }

        void SetStep(eCheckStep step)
        {
            lastCheckStep = currentCheckStep;
            currentCheckStep = step;
        }

        void BackToLastStep()
        {
            SetStep(lastCheckStep);
        }

        string statsText;
        private void OnGUI()
        {
            GUI.TextArea(new Rect(10, 30, 250, 200), statsText);
        }

        void UpdateProfilerRecorder()
        {
            var sb = new StringBuilder(500);
            sb.AppendLine($"File:{currentFileName}");
            if(currentCheckStep == eCheckStep.CheckFiles
                || currentCheckStep == eCheckStep.CheckFileOverDraw)
            {
                sb.AppendLine($"Frame:{currentStepFrame}/{CheckFrameCount}");
                if(currentCheckStep == eCheckStep.CheckFiles)
                {
                    sb.AppendLine($"Draw Calls Count: {_GetDrawcalls()}");
                    sb.AppendLine($"Set Pass Calls Count: {_GetSetPassCall()}");
                    sb.AppendLine($"Vertices Count: {_GetVertices()}");
                }
            }
            statsText = sb.ToString();
        }

        void UpdateWaitProfilerInitData()
        {
            initProfilerRenderData.SetPassCall = _GetSetPassCall();
            initProfilerRenderData.DrawCalls = _GetDrawcalls();
            initProfilerRenderData.Vertices = _GetVertices();
            if(currentStepFrame > 50)
            {
                SetStep(eCheckStep.CheckFiles);
            }
        }

        void UpdateCheckFiles()
        {
            if (gObject == null)
            {
                while(!GetNewFile())
                {
                    if(IsFileListEnd())
                    {
                        BeginOverDrawStep();
                        break;
                    }
                }
            }
            if(gObject != null)
            {
                _UpdateParticleSystems();
                if (currentStepFrame >= CheckFrameCount)
                {
                    SingleFileEnd();
                }
                else
                {
                    ProfilerRenderData data = new ProfilerRenderData();
                    data.SetPassCall = _GetSetPassCall();
                    data.DrawCalls = _GetDrawcalls();
                    data.Vertices = _GetVertices();
                    currentFileProfilerRenderDatas.Add(data - initProfilerRenderData);
                }
            }
        }

        void UpdateOverDrawCheck()
        {
            if (gObject == null)
            {
                while (!GetNewFile())
                {
                    if (IsFileListEnd())
                    {
                        SetStep(eCheckStep.End);
                        break;
                    }
                }
            }
            if (gObject != null)
            {
                _UpdateParticleSystems();
                if (currentStepFrame >= CheckFrameCount)
                {
                    SingleFileEnd();
                }
                else
                {
                    _GetOverDraw(out float overDraw, out float fillRate);
                    currentFileProfilerRenderDatas[(int)currentStepFrame].OverDraw = overDraw;
                    currentFileProfilerRenderDatas[(int)currentStepFrame].fillRate = fillRate;
                }
            }
        }

        void _UpdateParticleSystems()
        {
            foreach(var particleSystem in particleSystems)
            {
                particleSystem.time = currentStepFrame * 0.2f;
            }
        }

        void BeginOverDrawStep()
        {
            assetFileIndex = 0;
            SetStep(eCheckStep.CheckFileOverDraw);
            if(OverDrawSRPHelper.IsUseUniversalRenderPipeline())
            {
                OverDrawSRPHelper.SetRenderPipeline(overdrawRenderPipelineAsset);
            }
            else
            {
                mainCamera.SetReplacementShader(Shader.Find("Camera/OverDraw"), "");
            }
            SetStep(eCheckStep.Interval);
        }

        bool GetNewFile()
        {
            if (assetCheckRenderInfoConfig.assetList.Count <= assetFileIndex)
                return false;
            currentFileName = assetCheckRenderInfoConfig.assetList[assetFileIndex];
            string resouceFileName;
            if(assetCheckRenderInfoConfig.copyAssetList[assetFileIndex].Equals(string.Empty))
            {
                resouceFileName = currentFileName;
            }
            else
            {
                resouceFileName = assetCheckRenderInfoConfig.copyAssetList[assetFileIndex];
            }
            ++assetFileIndex;
            string fileName = resouceFileName.Replace("\\", "/");
            if (!fileName.StartsWith("Assets/Resources"))
                return false;
            if (!fileName.EndsWith(".prefab"))
                return false;
            string resourceFileWithSuffix = fileName.Substring(("Assets/Resources").Length + 1);
            string resourceFileName = resourceFileWithSuffix.Substring(0, resourceFileWithSuffix.Length - 7);
            Object obj = Resources.Load<GameObject>(resourceFileName);
            if (obj == null)
                return false;
            gObject = GameObject.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
            particleSystems = gObject.GetComponentsInChildren<ParticleSystem>();
            currentStepFrame = 0;
            if(dicProfilerRenderDatas.ContainsKey(currentFileName))
            {
                currentFileProfilerRenderDatas = dicProfilerRenderDatas[currentFileName];
            }
            else
            {
                currentFileProfilerRenderDatas = new List<ProfilerRenderData>();
                dicProfilerRenderDatas.Add(currentFileName, currentFileProfilerRenderDatas);
            }
            return true;
        }

        void SingleFileEnd()
        {
            GameObject.Destroy(gObject);
            gObject = null;
            currentStepFrame = 0;
            currentFileProfilerRenderDatas = null;
            SetStep(eCheckStep.Interval);
        }

        bool IsFileListEnd()
        {
            return assetFileIndex >= assetCheckRenderInfoConfig.assetList.Count;
        }

        void ApplicationEnd()
        {
            if (OverDrawSRPHelper.IsUseUniversalRenderPipeline())
            {
                OverDrawSRPHelper.ResetRenderPipeline();
            }
            else
            {
                mainCamera.ResetReplacementShader();
            }
            string jsonData = string.Empty;
            if (assetCheckRenderResultConfig != null)
            {
                assetCheckRenderResultConfig.filesProfilerRenderDatas = new List<SingleFileProfilerRenderDatas>();
                foreach(var profilerRenderDatas in dicProfilerRenderDatas)
                {
                    var singleFileData = new SingleFileProfilerRenderDatas();
                    singleFileData.fileName = profilerRenderDatas.Key;
                    singleFileData.profilerRenderDatas = profilerRenderDatas.Value.ToArray();
                    assetCheckRenderResultConfig.filesProfilerRenderDatas.Add(singleFileData);
                }
                jsonData = JsonUtility.ToJson(assetCheckRenderResultConfig);
                File.WriteAllText(RuntimeDefines.CheckPathRuntimeRenderResult, jsonData);
            }

            if(Directory.Exists(RuntimeDefines.ResourceTempPath))
            {
                Directory.Delete(RuntimeDefines.ResourceTempPath, true);
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        long _GetDrawcalls()
        {
#if UNITY_2020_2_OR_NEWER
            return drawCallsRecorder.LastValue;
#else
#if UNITY_EDITOR
            return UnityEditor.UnityStats.drawCalls;
#else
            return 0;
#endif
#endif
        }

        long _GetSetPassCall()
        {
#if UNITY_2020_2_OR_NEWER
            return setPassCallsRecorder.LastValue;
#else
#if UNITY_EDITOR
            return UnityEditor.UnityStats.setPassCalls;
#else
            return 0;
#endif
#endif
        }

        long _GetVertices()
        {
#if UNITY_2020_2_OR_NEWER
            return verticesRecorder.LastValue;
#else
#if UNITY_EDITOR
            return UnityEditor.UnityStats.vertices;
#else
            return 0;
#endif
#endif
        }

        void _GetOverDraw(out float overDraw, out float fillRate)
        {
            screenShot.ReadPixels(new Rect(0, 0, cameraRenderTarget.width, cameraRenderTarget.height), 0, 0);
            int pixDrawCount = 0;
            int pixDrawTimes = 0;
            int pixCount = 0;
            GetTexturePixCount(screenShot, out pixDrawCount, out pixDrawTimes, out pixCount);
            overDraw = (float)pixDrawTimes / pixCount;
            fillRate = (float)pixDrawCount / pixCount;
        }

        void GetTexturePixCount(Texture2D texture, out int pixDrawCount, out int pixDrawTimes, out int pixCount)
        {
            var pixels = texture.GetPixels();
            pixCount = pixels.Length;
            pixDrawCount = 0;
            pixDrawTimes = 0;
            foreach (var pixel in pixels)
            {
                if(!_IsEmptyColor(pixel))
                {
                    ++pixDrawCount;
                }
                pixDrawTimes += _PixTimes(pixel);
            }
        }

        bool _IsEmptyColor(Color c)
        {
            return Mathf.Approximately(c.r, 0f) && Mathf.Approximately(c.g, 0f) && Mathf.Approximately(c.b, 0f);
        }

        int _PixTimes(Color c)
        {
            return Convert.ToInt32(c.b / 0.02);
        }
    }
}
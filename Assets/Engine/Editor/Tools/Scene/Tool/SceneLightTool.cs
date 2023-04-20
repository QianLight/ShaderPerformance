using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{

    class LightIntersectJob : IJobProcessor
    {
        public ChunkLightData cld;
        public Color[] lightIndexData;
        public int jobLineCount = 1;
        public int xChunkCount = 1;
        public void Prepare(int jobID)
        {
            int lenght = cld.maxXGrid * cld.maxZGrid;
            if (lightIndexData == null || lenght != lightIndexData.Length)
            {
                lightIndexData = new Color[lenght];
            }
        }

        public void Execute(int jobID)
        {
            int chunkLightGridCount = (int)(EngineContext.ChunkSize / EngineContext.LightGridSize);
            int endLine = jobID * jobLineCount + jobLineCount;
            for (int z = jobID * jobLineCount; z < endLine; z++)
            {
                for (int x = 0; x < cld.maxXGrid; x++)
                {
                    Vector2 min = new Vector2(x * EngineContext.LightGridSize, z * EngineContext.LightGridSize);
                    Vector2 max = new Vector2(min.x + EngineContext.LightGridSize, min.y + EngineContext.LightGridSize);
                    int lightCount = 0;
                    Color c = new Color(0, 0, 0, 0);
                    int xx = x / chunkLightGridCount;
                    int zz = z / chunkLightGridCount;
                    int lightChunkIndex = xx + zz * xChunkCount;
                    var chunkLightIndex = cld.chunkLightIndex[lightChunkIndex];
                    for (int i = 0; i < chunkLightIndex.lightIndex.Count; ++i)
                    {
                        var lightIndex = chunkLightIndex.lightIndex[i];
                        var l = cld.lights[lightIndex];
                        if (RuntimeUtilities.TestCircleRect(l.pos.x, l.pos.z, l.range, ref min, ref max))
                        {
                            c[lightCount] = (i + 1) / 255.0f;
                            lightCount++;
                        }
                        if (lightCount >= 3)
                            break;
                    }
                    int index = x + z * cld.maxXGrid;
                    lightIndexData[index] = c;
                }
            }
        }
        public void Collection(int jobID)
        {

        }

        public int GetJobCount()
        {
            return cld.maxZGrid / jobLineCount;
        }
    }
    class SceneLightContext : IJobContext
    {
        private bool running = false;
        LightIntersectJob lightIntersectJob = new LightIntersectJob();

        public void Init(ChunkLightData lightData, int xChunkCount)
        {
            lightIntersectJob.cld = lightData;
            lightIntersectJob.xChunkCount = xChunkCount;
        }
        public void StartJobs(ThreadManager threadManager)
        {
            // ThreadManager.isSingleThread = true;
            threadManager.AddJobGroup(
                lightIntersectJob,
                lightIntersectJob.GetJobCount(), 0.5f);
            running = true;
            threadManager.StartJobs();
        }
        public void UpdateJobs(bool finish)
        {
            if (running)
            {
                if (finish)
                {
                    if (lightIntersectJob.lightIndexData != null)
                    {
                        Texture2D chunkTex = new Texture2D(
                            lightIntersectJob.cld.maxXGrid,
                            lightIntersectJob.cld.maxZGrid,
                            TextureFormat.ARGB32, false, true)
                        {
                            name = "Chunk_LightTex",
                            hideFlags = HideFlags.DontSave,
                            anisoLevel = 0,
                            wrapMode = TextureWrapMode.Clamp,
                            filterMode = FilterMode.Bilinear
                        };
                        chunkTex.SetPixels(lightIntersectJob.lightIndexData);
                        chunkTex.Apply();
                        lightIntersectJob.cld.lightIndexTex = CommonAssets.CreateAsset<Texture2D>(lightIntersectJob.cld.dir, "Chunk_LightTex", ".tga", chunkTex);
                        UnityEngine.Object.DestroyImmediate(chunkTex);
                        CommonAssets.SaveAsset(lightIntersectJob.cld);

                    }
                    ResetJobs();
                }
            }
        }

        public void ResetJobs()
        {
            running = false;
        }
        public void Release()
        {
            ResetJobs();
        }
    }
    public partial class SceneEditTool : CommonToolTemplate
    {
        enum OpLighType
        {
            OpNone,
            OpBakeLights,
        }

        private SceneLightContext sceneLightContext = new SceneLightContext();
        private OpLighType opLighType = OpLighType.OpNone;
        private List<LightprobeArea> lightProbesArea = new List<LightprobeArea>();

        void BakeLights()
        {
            // Transform t = commonContext.editorSceneGos[(int)EditorSceneObjectType.Light].transform;
            // LightGroup lightGroup = t.GetComponent<LightGroup>();
            // if (lightGroup == null)
            // {
            //     lightGroup = t.gameObject.AddComponent<LightGroup>();
            // }
            // lightGroup.CollectLights(EngineContext.instance, ref sceneContext);
            // InitThread();
            // sceneLightContext.Init(lightGroup.cld, widthCount);
            // sceneLightContext.StartJobs(threadManager);
        }

        private void UpdateLights()
        {
            switch (opLighType)
            {
                case OpLighType.OpBakeLights:
                    BakeLights();
                    break;

            }
            opLighType = OpLighType.OpNone;
        }
        private void OnLightGUI()
        {
            EditorCommon.BeginGroup("Lights");
            // if (GUILayout.Button("Bake Lights", GUILayout.MaxWidth(160)))
            // {
            //     opLighType = OpLighType.OpBakeLights;
            // }

            EditorCommon.EndGroup(false);
        }
    }
}
using System.IO;
using UnityEngine;

namespace CFEngine.Editor
{
    public class LightSystem : SceneResProcess
    {
        public override void Init(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init(ref sceneContext, ssContext);
            serialize = SerializeCb;
        }
        private static void Save(SceneSerializeContext ssContext, Transform trans, int objID, Light light)
        {
            var lightData = new LightData();
            lightData.Save(light);
            lightData.objID = objID;
            ssContext.sd.lightDatas.Add(lightData);
        }

        ////////////////////////////PreSerialize////////////////////////////
        /// 
        ////////////////////////////Serialize////////////////////////////
        private static void SavePointLight(SceneSerializeContext ssContext, Light light, Transform trans)
        {
            if (light.enabled && light.type == LightType.Point &&
                trans.TryGetComponent<SceneLightRender>(out var lr))
            {
                for (int z = 0; z < ssContext.heightCount; ++z)
                {
                    for (int x = 0; x < ssContext.widthCount; ++x)
                    {
                        Vector2 min = new Vector2(x * EngineContext.ChunkSize, z * EngineContext.ChunkSize);
                        Vector2 max = new Vector2(min.x + EngineContext.ChunkSize, min.y + EngineContext.ChunkSize);

                        Vector3 pos = trans.position;
                        if (RuntimeUtilities.TestCircleRect(pos.x,
                                pos.z,
                                light.range, ref min, ref max))
                        {
                            int chunkIndex = x + z * ssContext.widthCount;
                            var chunk = ssContext.sd.GetChunk(chunkIndex, true);
                            if (chunk != null)
                            {
                                chunk.pointLights.Add(lr.GetLigthInfo());
                            }
                        }
                    }
                }
            }
        }
        protected static void SerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<Light>(out var light))
            {
                LightGroup.SortLight(light, ssContext.objIDMap);
                var goi = StaticObjectSystem.GreatePrefabInstance(ssContext, trans, -1);
                Save(ssContext, trans, goi.ID, light);
                SavePointLight(ssContext, light, trans);

            }
            EnumFolder(trans, ssContext, ssContext.serialize);
        }

        ////////////////////////////Save////////////////////////////
        public static void SaveChunk(BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            int pointCount = chunk.pointLights.Count;
            bw.Write(pointCount);
            for (short j = 0; j < pointCount; ++j)
            {
                var point = chunk.pointLights[j];
                EditorCommon.WriteVector(bw, point.posCoverage);
                EditorCommon.WriteVector(bw, point.color);
                EditorCommon.WriteVector(bw, point.param);
            }
            DebugLog.DebugStream(bw, "ChunkLight");
        }
    }
}
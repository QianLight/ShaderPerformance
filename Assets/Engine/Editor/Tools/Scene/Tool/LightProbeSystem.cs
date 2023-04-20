using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public partial class LightProbeSystem : SceneResProcess
    {
        public static LightProbeSystem system;
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            serialize = SerializeCb;
            system = this;
        }
        private static void Save (SceneSerializeContext ssContext, Transform trans, int objID, Collider collider)
        {
            var colliderData = new ColliderData ();
            colliderData.Save (collider);
            colliderData.objID = objID;
            ssContext.sd.colliderDatas.Add (colliderData);
        }

        public static void Save (SceneSerializeContext ssContext, Transform trans, int objID)
        {
            if (trans.TryGetComponent<Collider> (out var collider))
            {
                Save (ssContext, trans, objID, collider);
            }
        }

        ////////////////////////////PreSerialize////////////////////////////

        ////////////////////////////Serialize////////////////////////////
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<LightprobeArea> (out var lpa))
            {
                var chunk = ssContext.sd.chunks[lpa.chunkID];
                chunk.lpo.Copy (lpa.lpo);
            }
            else
            {
                EnumFolder (trans, ssContext, ssContext.serialize);

            }
        }

        ////////////////////////////Save////////////////////////////
        public static void SaveChunk (BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        { 
            var lpo = chunk.lpo;
            if (lpo != null && lpo.probPostions.Count > 0)
            {
                bw.Write (true);
                int minChunkX = 10000;
                int maxChunkX = 0;
                int minChunkZ = 10000;
                int maxChunkZ = 0;
                for (int j = 0; j < lpo.probPostions.Count; ++j)
                {
                    var lpi = lpo.probPostions[j];
                    if (lpi.x < minChunkX) { minChunkX = lpi.x; }
                    if (lpi.x > maxChunkX) { maxChunkX = lpi.x; }
                    if (lpi.z < minChunkZ) { minChunkZ = lpi.z; }
                    if (lpi.z > maxChunkZ) { maxChunkZ = lpi.z; }
                }
                Vector4Int area = new Vector4Int (minChunkX, maxChunkX, minChunkZ, maxChunkZ);
                EditorCommon.WriteVector (bw, area);
                bw.Write ((short) lpo.probPostions.Count);
                List<SHData> shData = new List<SHData> ();
                for (int j = 0; j < lpo.probPostions.Count; ++j)
                {
                    var lpi = lpo.probPostions[j];
                    bw.Write (lpi.x);
                    bw.Write (lpi.z);
                    bw.Write ((byte) lpi.shData.Length);
                    for (int k = 0; k < lpi.shData.Length; ++k)
                    {
                        ref var sd = ref lpi.shData[k];
                        shData.Add (sd);
                    }
                }
                bw.Write ((short) shData.Count);
                for (int j = 0; j < shData.Count; ++j)
                {
                    var sd = shData[j];

                    bw.Write (sd.y);
                    Vector4 shAr = Vector4.zero;
                    Vector4 shAg = Vector4.zero;
                    Vector4 shAb = Vector4.zero;
                    Vector4 shBr = Vector4.zero;
                    Vector4 shBg = Vector4.zero;
                    Vector4 shBb = Vector4.zero;
                    Vector4 shC = Vector4.zero;
#if !UNITY_ANDROID
                    EnviromentSHBakerHelper.PrepareCoefs (ref sd.sh,
                        ref shAr, ref shAg, ref shAb,
                        ref shBr, ref shBg, ref shBb,
                        ref shC);
#endif
                    EditorCommon.WriteVector (bw, shAr);
                    EditorCommon.WriteVector (bw, shAg);
                    EditorCommon.WriteVector (bw, shAb);

                    EditorCommon.WriteVector (bw, shBr);
                    EditorCommon.WriteVector (bw, shBg);
                    EditorCommon.WriteVector (bw, shBb);
                    EditorCommon.WriteVector (bw, shC);
                }
            }
            else
            {
                bw.Write (false);
            }
        }
    }
}
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
    public class AudioSystem : SceneResProcess
    {
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            serialize = SerializeCb;
        }

        ////////////////////////////PreSerialize////////////////////////////

        ////////////////////////////Serialize////////////////////////////
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent (out AudioObject ao))
            {
                SceneAssets.SortSceneObjectName (ao, "Audio", ssContext.objIDMap);
                var pos = trans.position;
                var sao = new SceneAudioObject ();
                sao.pos = pos;
                sao.eventName = ao.eventName;
                sao.range = ao.range;
                int chunkID = -1;
                if (ao.range > ssContext.chunkWidth * 2)
                {
                    chunkID = -1; //global object
                }
                else
                {
                    chunkID = SceneQuadTree.FindChunkIndex (ref pos,
                        ssContext.chunkWidth, ssContext.widthCount, ssContext.heightCount, out var x, out var z);
                }
                var chunk = ssContext.sd.GetChunk (chunkID);
                chunk.audios.Add (sao);
            }
            EnumFolder (trans, ssContext, ssContext.serialize);
        }

        ////////////////////////////Save////////////////////////////
        public static void SaveChunk (BinaryWriter bw, SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            var audios = chunk.audios;
            bw.Write ((ushort) audios.Count);
            for (int j = 0; j < audios.Count; ++j)
            {
                var ao = audios[j];
                ssc.SaveStringIndex (bw, ao.eventName);
                EditorCommon.WriteVector (bw, ao.pos);
                bw.Write (ao.range);
            }
        }

        public override void PreSaveChunk(ref SceneContext sceneContext, BaseSceneContext bsc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            var ssc = bsc as SceneSaveContext;
            var audios = chunk.audios;
            for (int j = 0; j < audios.Count; ++j)
            {
                var ao = audios[j];
                ssc.AddResName(ao.eventName);
            }
        }
    }
}
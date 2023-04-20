using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CFEngine.Editor
{
    public delegate void PreSaveExternalDataCb (SceneSaveContext ssc);
    public delegate void SaveHeadExternalDataCb (BinaryWriter binaryWriter, SceneSaveContext ssc);
    public delegate void SaveChunkExternalDataCb (BinaryWriter binaryWriter, int i);

    public partial class SceneSerialize
    {
        public static PreSaveExternalDataCb preSaveExternalDataCb;
        public static SaveHeadExternalDataCb saveHeadExternalDataCb;
        public static SaveChunkExternalDataCb saveChunkExternalDataCb;
        internal static void GetSceneChunkCount(SceneConfig sceneConfig, out int chunkWidth, out int chunkHeight, out int widthCount, out int heightCount)
        {
            chunkWidth = EngineContext.ChunkSize;
            chunkHeight = EngineContext.ChunkSize;
            widthCount = sceneConfig.widthCount;
            heightCount = sceneConfig.heightCount;
        }
        public static void PrepareSceneFolder(ref SceneContext context)
        {
            if (context.valid)
            {
                string config = "Config";
                string subDir = context.dir + "/" + config;
                if (!AssetDatabase.IsValidFolder(subDir))
                    AssetDatabase.CreateFolder(context.dir, config);
                string terrain = "Terrain";
                if (!AssetDatabase.IsValidFolder(context.terrainDir))
                    AssetDatabase.CreateFolder(context.dir, terrain);
            }
        }
        public static SceneConfig CreateSceneConfig(ref SceneContext context, int widthCount, int heightCount, string tagname)
        {
            if (context.valid)
            {
                string config = "Config";
                string subDir = context.dir + "/" + config;
                if (!AssetDatabase.IsValidFolder(subDir))
                    AssetDatabase.CreateFolder(context.dir, config);
                string terrain = "Terrain";
                if (!AssetDatabase.IsValidFolder (context.terrainDir))
                    AssetDatabase.CreateFolder (context.dir, terrain);

                SceneConfig sc = ScriptableObject.CreateInstance<SceneConfig> ();
                sc.widthCount = widthCount;
                sc.heightCount = heightCount;
                if (!string.IsNullOrEmpty (tagname))
                    sc.sceneEditorTag.Add (tagname);
                sc.chunks = new System.Collections.Generic.List<SceneConfig.TerrainChunk> ();
                SceneConfig newSi = CommonAssets.CreateAsset<SceneConfig> (subDir,
                    context.name + SceneContext.SceneConfigSuffix, ".asset", sc);
                return newSi;
            }
            else
            {
                EditorUtility.DisplayDialog ("Error", "Save Scene First.", "OK");
                return null;
            }
        }

        public static string GetConfigName (ref SceneContext context, string name)
        {
            if (string.IsNullOrEmpty (name))
                return string.IsNullOrEmpty (context.suffix) ? context.name : context.suffix;
            else
                return name;
        }

        public static void LoadEditorChunkData (ref SceneContext context, string name, bool create, out EditorChunkData ecd)
        {
            string path = string.Format ("{0}/{1}.asset",
                context.configDir,
                GetConfigName (ref context, name));

            ecd = AssetDatabase.LoadAssetAtPath<EditorChunkData> (path);
            if (create && ecd == null)
            {
                ecd = ScriptableObject.CreateInstance<EditorChunkData> ();
                ecd = CommonAssets.CreateAsset<EditorChunkData> (context.configDir,
                    GetConfigName (ref context, name), ".asset", ecd);
            }
        }

        public static void SaveEditorChunkData (ref SceneContext context, EditorChunkData ecd)
        {
            CommonAssets.CreateAsset<EditorChunkData> (context.configDir, GetConfigName (ref context, ""), ".asset", ecd);
        }
        public static void LoadSceneData<T> (ref SceneContext context, bool create, out T sd, string suffix = "") where T : ScriptableObject
        {
            string sdName = string.IsNullOrEmpty (suffix) ?
                string.Format ("{0}_{1}", context.name, context.suffix) :
                string.Format ("{0}_{1}_{2}", context.name, suffix, context.suffix);
            string path = string.Format ("{0}/{1}.asset",
                context.configDir,
                sdName);

            sd = AssetDatabase.LoadAssetAtPath<T> (path);
            if (create && sd == null)
            {
                sd = ScriptableObject.CreateInstance<T> ();
                sd.name = sdName;
                sd = CommonAssets.CreateAsset<T> (context.configDir,
                    sdName, ".asset", sd);
            }
            else if (sd != null)
            {
                sd.name = sdName;
            }
        }

        public static void LoadSceneData (ref SceneContext context, bool create, out SceneData sd)
        {
            string sdName = string.Format ("{0}_{1}", context.name, context.suffix);
            string path = string.Format ("{0}/{1}.asset",
                context.configDir,
                sdName);

            sd = AssetDatabase.LoadAssetAtPath<SceneData> (path);
            if (create && sd == null)
            {
                sd = ScriptableObject.CreateInstance<SceneData> ();
                sd.name = sdName;
                sd = CommonAssets.CreateAsset<SceneData> (context.configDir,
                    sdName, ".asset", sd);
            }
        }

        public static void LoadLightmapVolumnData (ref SceneContext context, bool create, out LightmapVolumnData lvd)
        {
            string name = string.Format ("{0}_Lightmap", context.name);
            string path = string.Format ("{0}/{1}.asset",
                context.configDir,
                name);

            lvd = AssetDatabase.LoadAssetAtPath<LightmapVolumnData> (path);
            if (create && lvd == null)
            {
                lvd = ScriptableObject.CreateInstance<LightmapVolumnData> ();
                lvd.name = name;
                lvd = CommonAssets.CreateAsset<LightmapVolumnData> (context.configDir,
                    name, ".asset", lvd);
            }
        }
        public static void SaveLightmapVolumnData (ref SceneContext context, LightmapVolumnData lvd)
        {
            lvd.name = context.name + "_Lightmap";
            CommonAssets.CreateAsset<LightmapVolumnData> (context.configDir, context.name + "_Lightmap", ".asset", lvd);
        }
 

        public class ReDirectRes
        {
            public string physicDir = "";
            public int logicPathType = 0;
            public static int LogicPath_Common = 0;
            public static int LogicPath_SceneRes = 1;
        }     

        #region PreSave
        private static void PreSave (ref SceneContext sceneContext, SceneSaveContext ssc, ESceneType sceneType)
        {
            var configs = SceneResConfig.instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null && config.sceneType == sceneType)
                {
                    config.process.PreSave (ref sceneContext, ssc);
                }
            }
        }
        private static void PreSaveChunk (ref SceneContext sceneContext, SceneSaveContext ssc,
            ChunkData chunk, ChunkSaveData saveChunk, int index, ESceneType sceneType)
        {
            var configs = SceneResConfig.instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null && config.sceneType == sceneType)
                {
                    config.process.PreSaveChunk (ref sceneContext, ssc, chunk, saveChunk, index);
                }
            }
        }
        private static void PreSaveExternalData (SceneSaveContext ssc)
        {
            EditorCommon.CallInternalFunction (typeof (SceneSaveExternal), "Init", true, false, false, null, null);
            if (preSaveExternalDataCb != null)
            {
                preSaveExternalDataCb (ssc);
            }
        }
        #endregion
        #region save head
        private static void SaveStringIndex (BinaryWriter bw, SceneSaveContext ssc, string str)
        {
            ssc.resAsset.SaveStringIndex (bw, str);
        }
        private static void SaveHeadData (BinaryWriter bw,
            SceneSaveContext ssc)
        {
            bw.Write (EngineContext.VersionLatest);
            bw.Write (RenderContext.ResVersionLatest);
            bw.Write (ssc.sd.saveFlag.flag);
            bw.Write (ssc.widthCount * EngineContext.ChunkSize);
            bw.Write (ssc.heightCount * EngineContext.ChunkSize);

            if (EngineContext.UseUrp) return;

            bw.Write(ssc.chunkWidth);
            bw.Write(ssc.chunkHeight);
            bw.Write(ssc.widthCount);
            bw.Write(ssc.heightCount);
            bw.Write(ssc.ChunkStreamOffset.Length / 2);            
            ssc.chunkStreamPos = (int)bw.BaseStream.Length;
            for (int i = 0; i < ssc.ChunkStreamOffset.Length; ++i)
            {
                bw.Write (ssc.ChunkStreamOffset[i]);
            }
            //pvs
            bw.Write(ssc.sd.pvsCellSize);
            bw.Write(ssc.sd.pvsMaskCount);
            bw.Write(ssc.sd.pvsChunkData.Count);
            int pvsXCount = ssc.widthCount * 4;
            ssc.chunkPvsStreamPos = (int)bw.BaseStream.Length;
            for (int i = 0; i < ssc.sd.pvsChunkData.Count; ++i)
            {
                var pcd = ssc.sd.pvsChunkData[i];
                bw.Write((short)pcd.x);
                bw.Write((short)pcd.z);
                int blockIndex = pcd.x + pcd.z * pvsXCount;
                //DebugLog.AddErrorLog2("block index:{0} x:{1} z:{2}",
                //    blockIndex.ToString(),pcd.x.ToString(), pcd.z.ToString());
                bw.Write(pcd.startOffset);
                bw.Write(pcd.length);
            }
            DebugLog.DebugStream (bw, "Head");
        }
        private static void SaveHeadString (BinaryWriter bw, SceneSaveContext ssc)
        {
            ssc.resAsset.SaveHeadString (bw);
            DebugLog.DebugStream (bw, "String");
        }

        private static void SaveHeadMaterial (BinaryWriter bw, SceneSaveContext ssc)
        {
            ushort materialGroupCount = (ushort) ssc.matSaveData.matInfo.Count;
            bw.Write (materialGroupCount);
            for (ushort j = 0; j < materialGroupCount; ++j)
            {
                var mi = ssc.matSaveData.matInfo[j];
                var context = mi.context;
                bw.Write(context.flag);
                bw.Write(context.matHash);
                byte resCount = (byte)context.textureValue.Count;
                bw.Write (resCount);
                for (int i = 0; i < resCount; ++i)
                {
                    var stpv = context.textureValue[i];
                    byte index = (byte) stpv.shaderID;
                    bw.Write (index);
                    if (stpv.shaderID >= ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        SaveStringIndex (bw, ssc, stpv.shaderKeyName);
                    }
                    bw.Write (stpv.texType);
                    SaveStringIndex (bw, ssc, stpv.path);

                }
                byte shaderPropertyCount = (byte) context.shaderIDs.Count;
                bw.Write (shaderPropertyCount);
                for (int i = 0; i < shaderPropertyCount; ++i)
                {
                    var spv = context.shaderIDs[i];
                    byte index = (byte) spv.shaderID;
                    bw.Write (index);
                    if (index >= ShaderManager._ShaderKeyEffectKey.Length)
                    {
                        SaveStringIndex (bw, ssc, spv.shaderKeyName);
                    }
                    bw.Write (spv.paramType);
                    EditorCommon.WriteVector (bw, spv.value);
                }
            }
            DebugLog.DebugStream (bw, "MaterialHead");
        }
        private static void SaveObjectHead (BinaryWriter bw, SceneSaveContext ssc)
        {
            MultiLayerSystem.SaveChunk(bw, ssc, ssc.sd.global, ssc.saveSD.global, -1);
            StaticObjectSystem.SaveChunkGroupObject(bw, ssc, ssc.sd.global, ssc.saveSD.global, -1);
            StaticObjectSystem.SaveChunkSceneObject(bw, ssc, ssc.sd.global, ssc.saveSD.global, true);
            DebugLog.DebugStream(bw, "SceneObjectHead");
            AudioSystem.SaveChunk (bw, ssc, ssc.sd.global, ssc.saveSD.global, -1);
            DebugLog.DebugStream (bw, "AudioObjectHead");
        }
        private static void SaveHeadExternalData (BinaryWriter bw, SceneSaveContext ssc)
        {
            if (saveHeadExternalDataCb != null)
            {
                saveHeadExternalDataCb (bw, ssc);
            }
            DebugLog.DebugStream (bw, "ExternalHead");

        }
        #endregion

        #region save
        private static void SaveChunkRes (BinaryWriter bw,
            SceneSaveContext ssc, ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            bw.Write (saveChunk.hasCollider);
            LightProbeSystem.SaveChunk (bw, ssc, chunk, saveChunk, i);
            DebugLog.DebugStream (bw, "ChunkLightProbe");
        }

        private static void SaveChunkObject (BinaryWriter bw, SceneSaveContext ssc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            StaticObjectSystem.SaveChunkGroupObject (bw, ssc, chunk, saveChunk, i);
            DebugLog.DebugStream (bw, "ChunkGroupObject");
            StaticObjectSystem.SaveChunkSceneObject (bw, ssc, chunk, saveChunk);
            DebugLog.DebugStream (bw, "ChunkSceneObject");
            AudioSystem.SaveChunk (bw, ssc, chunk, saveChunk, i);
            DebugLog.DebugStream (bw, "ChunkAudioObject");
        }

        private static void SaveChunkQuadTree (BinaryWriter bw, SceneSaveContext ssc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {
            byte sqbCount = (byte) saveChunk.sceneQuadBlocks.Count;
            bw.Write (sqbCount);
            if (sqbCount > 0)
            {
                int sceneObjectGroupCount = saveChunk.sceneObjectIndex != null ? saveChunk.sceneObjectIndex.Count : 0;
                bw.Write (sceneObjectGroupCount);
                for (int j = 0; j < sceneObjectGroupCount; ++j)
                {
                    bw.Write (saveChunk.sceneObjectIndex[j]);
                }
                DebugLog.DebugStream (bw, "ChunkQuadIndex");
                var it = saveChunk.sceneQuadBlocks.GetEnumerator ();
                while (it.MoveNext ())
                {
                    var kvp = it.Current;
                    var sqb = kvp.Value;
                    byte index = (byte) kvp.Key;
                    bw.Write (index);
                    EditorCommon.SaveAABB (bw, ref sqb.aabb);
                    bw.Write (sqb.sceneObjectGroupIndex);
                }
            }
            DebugLog.DebugStream (bw, "ChunkQuadTree");
        }

        private static void SaveChunkExternalData (BinaryWriter bw, int i)
        {
            if (saveChunkExternalDataCb != null)
            {
                saveChunkExternalDataCb (bw, i);
            }
            DebugLog.DebugStream (bw, "ChunkExternal");
        }
        #endregion
        #region postsave
        private static void PostSaveHeadData (BinaryWriter bw,
            SceneSaveContext ssc)
        {
            bw.Seek (ssc.chunkStreamPos, SeekOrigin.Begin);
            for (int i = 0; i < ssc.ChunkStreamOffset.Length; ++i)
            {
                bw.Write (ssc.ChunkStreamOffset[i]);
            }
            bw.Seek(ssc.chunkPvsStreamPos, SeekOrigin.Begin);
            for (int i = 0; i < ssc.sd.pvsChunkData.Count; ++i)
            {
                var pcd = ssc.sd.pvsChunkData[i];
                bw.Write((short)pcd.x);
                bw.Write((short)pcd.z);
                bw.Write(pcd.startOffset);
                bw.Write(pcd.length);
            }
        }
        #endregion
        private static void PrepareFolder (ref SceneContext sceneContext)
        {
            string targetSceneDir = string.Format ("{0}Scene/{1}",
                LoadMgr.singleton.BundlePath,
                sceneContext.name);
            EditorCommon.CreateDir (targetSceneDir);

            using (FileStream fs = new FileStream (string.Format ("{0}Scene/{1}/SceneRes.txt",
                LoadMgr.singleton.BundlePath,
                sceneContext.name), FileMode.Create)) { }
        }
        public static void SaveScene2 (SceneConfig sceneConfig, ref SceneContext sceneContext, bool saveEditorScene = true, bool outputlog = true)
        {
            try
            {
                if (sceneConfig != null && sceneConfig.chunks != null)
                {
                    SceneResConfig.instance.Init (ref sceneContext, null);
                    PrepareFolder (ref sceneContext);
                    SceneData sd;
                    LoadSceneData (ref sceneContext, false, out sd);
                    if (sd != null)
                    {
                        SceneSaveContext ssc = new SceneSaveContext ();
                        GetSceneChunkCount (sceneConfig, out ssc.chunkWidth, out ssc.chunkHeight, out ssc.widthCount, out ssc.heightCount);
                        ssc.sd = sd;
                        ssc.sceneConfig = sceneConfig;
                        ssc.sceneContext = sceneContext;
                        ssc.ChunkStreamOffset = new int[sceneConfig.chunks.Count * 2];                      
                        ssc.matSaveData.resAsset = ssc.resAsset;
                        ResParam.addRes = ssc.resAsset.AddResReDirct;
                        for (int i = 0; i < sceneConfig.chunks.Count; ++i)
                        {
                            ssc.ChunkStreamOffset[i] = -1;
                            ssc.saveSD.chunks.Add (new ChunkSaveData ());
                        }
                        int cellSize = ssc.sd.pvsCellSize;
                        if (cellSize <= 0)
                        {
                            cellSize = 2;
                            ssc.sd.pvsCellSize = cellSize;
                        }

                        int pvsBlockSize = EngineContext.Lod1ChunkSize / cellSize;
                        PreSave(ref sceneContext, ssc, ESceneType.Scene);
                        PreSaveChunk(ref sceneContext, ssc, ssc.sd.global, ssc.saveSD.global, -1, ESceneType.Scene);
                        var chunks = ssc.sd.chunks;
                        for (int i = 0; i < chunks.Count; ++i)
                        {
                            var chunk = ssc.sd.chunks[i];
                            var saveChunk = ssc.saveSD.chunks[i];
                            InitQuadTree(ssc, saveChunk, i);
                            PreSaveChunk(ref sceneContext, ssc, chunk, saveChunk, i, ESceneType.Scene);
                            PostInitQuadTree(ssc, saveChunk, i);
                        }
                        // ssc.matSaveData.Sort ();
                        PreSaveExternalData(ssc);

                        int headLength = 0;
                        string path = string.Format ("{0}Scene/{1}/{1}.bytes",
                            LoadMgr.singleton.BundlePath,
                            sceneContext.name);
                        using (FileStream fs = new FileStream (path, FileMode.Create))
                        {
                            BinaryWriter bw = new BinaryWriter (fs);
                            DebugLog.DebugStream (bw, "Scene Head", DebugSream.Clear);
                            int headstart = (int) bw.BaseStream.Position;
                            SaveHeadData (bw, ssc);

                            if (EngineContext.UseUrp)
                            {
                                EnvSystem.SaveHead(bw, ssc);
                                headLength = (int)bw.BaseStream.Position;
                            }
                            else
                            {
                                //SaveHeadString(bw, ssc);
                                //TerrainSystem.SaveHead(bw, ssc);
                                //SaveHeadMaterial(bw, ssc);
                                //EnvSystem.SaveHead(bw, ssc);
                                //AnimationSystem.SaveHead(bw, ssc);
                                //SaveObjectHead(bw, ssc);
                                //SaveHeadExternalData(bw, ssc);
                                //if (outputlog)
                                //    DebugLog.DebugStream(bw, "", DebugSream.Output);
                                //headLength = (int)bw.BaseStream.Position;
                                //for (int i = 0; i < chunks.Count; ++i)
                                //{
                                //    var chunk = ssc.sd.chunks[i];
                                //    var saveChunk = ssc.saveSD.chunks[i];
                                //    int start = (int)bw.BaseStream.Position;
                                //    ssc.ChunkStreamOffset[i * 2] = start;
                                //    DebugLog.DebugStream(bw, string.Format("Chunk {0}", i.ToString()), DebugSream.Clear);
                                //    TerrainSystem.SaveChunk(bw, ssc, chunk, saveChunk, i);
                                //    SaveChunkRes(bw, ssc, chunk, saveChunk, i);
                                //    InstanceSystem.SaveChunk(bw, ssc, chunk, saveChunk, i);
                                //    SaveChunkObject(bw, ssc, chunk, saveChunk, i);
                                //    SaveChunkQuadTree(bw, ssc, chunk, saveChunk, i);
                                //    LightSystem.SaveChunk(bw, ssc, chunk, saveChunk, i);
                                //    SaveChunkExternalData(bw, i);
                                //    ssc.ChunkStreamOffset[i * 2 + 1] = (int)bw.BaseStream.Position - start;
                                //    if (outputlog)
                                //        DebugLog.DebugStream(bw, "", DebugSream.Output);
                                //}
                                //var pvsChunks = ssc.sd.pvsChunkData;
                                //for (int i = 0; i < pvsChunks.Count; ++i)
                                //{
                                //    var pvsChunk = pvsChunks[i];
                                //    pvsChunk.startOffset = (int)bw.BaseStream.Position;
                                //    DebugLog.DebugStream(bw, string.Format("PVS Chunk {0}", i.ToString()), DebugSream.Clear);
                                //    PVSSystem.SaveChunk(bw, pvsChunk, pvsBlockSize, ssc.sd.pvsMaskCount);
                                //    pvsChunk.length = (int)bw.BaseStream.Position - pvsChunk.startOffset;
                                //    if (outputlog)
                                //        DebugLog.DebugStream(bw, "", DebugSream.Output);
                                //}
                                //PostSaveHeadData(bw, ssc);
                            }
                        }

                        AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
                        ResParam.addRes = null;
                        if (saveEditorScene)
                            SaveEditorScene (ref sceneContext, ssc, headLength);
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog (e.StackTrace);
            }
        }

        private static void GetSfxObject(List<string> sfxList,ChunkData cd)
        {
            for (int i = 0; i < cd.sceneObjects.Count; ++i)
            {
                var so = cd.sceneObjects[i];
                if (so.flag.HasFlag(SceneObject.IsSfx))
                {
                    sfxList.Add(so.resName);
                }
            }
        }
        private static void SaveEditorScene (ref SceneContext sceneContext, SceneSaveContext ssc, int headLength)
        {
            LoadSceneData<DynamicSceneData>(ref sceneContext, false, out var dsd, "ds");
            string path = string.Format ("{0}Scene/{1}/{1}.scenebytes",
                LoadMgr.singleton.BundlePath, sceneContext.name);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(EngineContext.EditorVersionLatest);
                bw.Write(headLength);
                bw.Write(ssc.sd.chunks.Count);
                bw.Write(ssc.sceneConfig.bundles.Count);

                bw.Write(ssc.resAsset.editorResReDirect.Count);
                var it = ssc.resAsset.editorResReDirect.GetEnumerator();
                while (it.MoveNext())
                {
                    var kvp = it.Current;
                    bw.Write(kvp.Key);
                    var redirectRes = kvp.Value;
                    bw.Write(redirectRes.physicDir);
                    bw.Write(redirectRes.logicPathType);
                }

                bw.Write(ssc.resPath.Count);
                var resIt = ssc.resPath.GetEnumerator();
                while (resIt.MoveNext())
                {
                    var kvp = resIt.Current;
                    bw.Write(kvp.Key);
                    bw.Write(kvp.Value.Count);
                    var strIt = kvp.Value.GetEnumerator();
                    while (strIt.MoveNext())
                    {
                        var kvp2 = strIt.Current;
                        bw.Write(kvp2.Key);
                        bw.Write(kvp2.Value);
                    }
                }

                var sfxList = new List<string>();
                if (dsd != null)
                {
                    bw.Write(dsd.dynamicScenes.Count);
                    for (int i = 0; i < dsd.dynamicScenes.Count; ++i)
                    {
                        var ds = dsd.dynamicScenes[i];
                        bw.Write(ds.dynamicSceneName);
                        bw.Write(ds.dynamicObjects.Count);
                        for (int j = 0; j < ds.dynamicObjects.Count; ++j)
                        {
                            var sdo = ds.dynamicObjects[j];
                            bw.Write(sdo.hash);
                            bw.Write(sdo.name);
                            for (int k = 0; k < sdo.sfxList.Count; ++k)
                            {
                                var ro = sdo.sfxList[k];
                                sfxList.Add(ro.sfxName);
                            }
                        }
                    }
                }
                else
                {
                    bw.Write(0);
                }

                var chunkData = ssc.sd.global;
                GetSfxObject(sfxList, chunkData);
                for (int i = 0; i < ssc.sd.chunks.Count; ++i)
                {
                    GetSfxObject(sfxList, ssc.sd.chunks[i]);
                }
                bw.Write(sfxList.Count);
                foreach (var sfxName in sfxList)
                {
                    bw.Write(sfxName);
                }
                string envProfilePath = string.Format("{0}/{1}_Profiles.asset", sceneContext.configDir, sceneContext.name);
                bw.Write(envProfilePath);
                var envOjbects = ssc.sd.envObjects;
                bw.Write(envOjbects.Count);
                foreach(var eo in envOjbects)
                {
                    var profilePath = eo.profile != null ? AssetDatabase.GetAssetPath(eo.profile) : "";
                    bw.Write(profilePath);
                }
                bw.Write(Ambient.shColorDebug.Count);
                foreach (var debugColor in Ambient.shColorDebug)
                {
                    bw.Write(debugColor.Key);
                    bw.Write((int)debugColor.Value.ambientMode);
                    EditorCommon.WriteColor3(bw, ref debugColor.Value.flatColor);
                    EditorCommon.WriteColor3(bw, ref debugColor.Value.skyColor);
                    EditorCommon.WriteColor3(bw, ref debugColor.Value.equatorColor);
                    EditorCommon.WriteColor3(bw, ref debugColor.Value.groundColor);
                    bw.Write(debugColor.Value.skyIntensity);
                    string cubePath = debugColor.Value.skyCube != null ? AssetDatabase.GetAssetPath(debugColor.Value.skyCube) : "";
                    bw.Write(cubePath);
                }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void SaveDynamicScene (SceneConfig sceneConfig, ref SceneContext sceneContext)
        {
            try
            {
                if (sceneConfig != null && sceneConfig.chunks != null)
                {
                    PrepareFolder (ref sceneContext);
                    LoadSceneData<DynamicSceneData> (ref sceneContext, false, out var dsd, "ds");
                    if (dsd != null && dsd.dynamicScenes.Count > 0)
                    {
                        string path = string.Format ("{0}Scene/{1}/{1}_ds.bytes",
                            LoadMgr.singleton.BundlePath,
                            sceneContext.name);
                        using (FileStream fs = new FileStream (path, FileMode.Create))
                        {
                            BinaryWriter bw = new BinaryWriter (fs);
                            bw.Write (EngineContext.DynamicSceneVersion3);
                            byte dynamicSceneCount = (byte) dsd.dynamicScenes.Count;
                            bw.Write (dynamicSceneCount);
                            for (int i = 0; i < dynamicSceneCount; ++i)
                            {
                                bw.Write (dsd.dynamicScenes[i].dynamicSceneName);
                                int start = (int) bw.BaseStream.Position;
                                int offset = 0;
                                bw.Write (offset);
                                var ds = dsd.dynamicScenes[i];
                                bw.Write ((ushort) ds.dynamicObjects.Count);
                                for (int j = 0; j < ds.dynamicObjects.Count; ++j)
                                {
                                    var sdo = ds.dynamicObjects[j];
                                    bw.Write (sdo.flag.flag);
                                    bw.Write (sdo.hash);
                                    bw.Write ((byte) sdo.sfxList.Count);
                                    for (int k = 0; k < sdo.sfxList.Count; ++k)
                                    {
                                        var ro = sdo.sfxList[k];
                                        bw.Write (ro.sfxName);
                                        EditorCommon.WriteVector (bw, ro.pos);
                                        EditorCommon.WriteQuaternion (bw, ro.rot);
                                        EditorCommon.WriteVector (bw, ro.scale);
                                        EditorCommon.SaveAABB (bw, ref ro.aabb);
                                    }
                                    if ((sdo.flag.flag & 0xff) != 0)
                                    {
                                        //has trigger
                                        EditorCommon.WriteVector (bw, sdo.pos0);
                                        EditorCommon.WriteVector (bw, sdo.pos1);
                                        bw.Write (sdo.exString);

                                        bw.Write(sdo.permanentFx);
                                        bw.Write(sdo.autoY);                                       
                                        EditorCommon.WriteVector(bw, sdo.rotation);

                                        bw.Write(sdo.buffIDStr);
                                    }
                                }

                                int end = (int) bw.BaseStream.Position;
                                bw.Seek (start, SeekOrigin.Begin);
                                bw.Write (end);
                                bw.Seek (end, SeekOrigin.Begin);
                            }
                        }
                        AssetDatabase.ImportAsset (path, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog (e.StackTrace);
            }
        }
        public static void FastSaveScene (SceneAsset sceneAsset, bool saveScene, List<SceneLoadInfo> sceneName)
        {
            string scenePath = AssetDatabase.GetAssetPath (sceneAsset);
            string sceneDir = Path.GetDirectoryName (scenePath);
            string sceneConfigPath = string.Format ("{0}/Config/{1}{2}.asset",
                sceneDir, sceneAsset.name, SceneContext.SceneConfigSuffix);
            SceneConfig sc = AssetDatabase.LoadAssetAtPath<SceneConfig> (sceneConfigPath);
            if (sc != null)
            {
                if (saveScene)
                {
                    SceneContext context = new SceneContext ();
                    SceneAssets.GetSceneContext (ref context, sceneAsset.name, scenePath);
                    SaveScene2 (sc, ref context, true, false);
                    SaveDynamicScene (sc, ref context);
                    SceneEditTool.SaveScenetoBundleRes();
                    
                }
                if (sceneName != null)
                {
                    sceneName.Add (new SceneLoadInfo ()
                    {
                        name = sceneAsset.name.ToLower (),
                            count = 1
                    });
                }

                //if (EngineContext.UseUrp)
                //{
                //    EditorSceneManager.OpenScene(scenePath);

                //    SceneEditTool.SaveScene_Urp();
                //}
            }
        }

        public static List<SceneLoadInfo> FastSaveSceneLoadList (SceneList sceneListConfig, bool saveScene = true)
        {

            if (sceneListConfig != null && sceneListConfig.sceneList != null)
            {
                List<SceneLoadInfo> sceneName = new List<SceneLoadInfo> ();
                for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                {
                    var scene = sceneListConfig.sceneList[i];
                    if (scene.sceneAsset != null && !scene.notBuild)
                    {
                        EditorUtility.DisplayProgressBar("save scene", scene.sceneAsset.name, (float)i / sceneListConfig.sceneList.Count);
                        FastSaveScene (scene.sceneAsset, saveScene, sceneName);
                    }
                }
                EditorUtility.ClearProgressBar();
                string sceneListPath = string.Format ("{0}/Config/SceneLoadList.bytes",
                    AssetsConfig.instance.ResourcePath);
                using (FileStream fs = new FileStream (sceneListPath, FileMode.Create))
                {
                    BinaryWriter bw = new BinaryWriter (fs);
                    int maxChunkCount = 0;
                    int maxSplatCount = 0;
                    bw.Write (sceneName.Count);
                    for (int i = 0; i < sceneName.Count; ++i)
                    {
                        var SceneLoadInfo = sceneName[i];
                        bw.Write (SceneLoadInfo.name);
                        int headLength = 0;
                        string path = string.Format ("{0}Scene/{1}/{1}.scenebytes", LoadMgr.singleton.BundlePath, SceneLoadInfo.name);
                        if (File.Exists (path))
                        {
                            try
                            {
                                using (FileStream otherfs = new FileStream (path, FileMode.Open))
                                {
                                    BinaryReader br = new BinaryReader (otherfs);
                                    br.ReadInt32 ();
                                    headLength = br.ReadInt32 ();
                                     
                                    int chunkCount = br.ReadInt32 ();
                                    if (chunkCount > maxChunkCount)
                                    {
                                        maxChunkCount = chunkCount;
                                    }
                                    int splatCount = br.ReadInt32 ();
                                    if (splatCount > maxSplatCount)
                                    {
                                        maxSplatCount = splatCount;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                DebugLog.AddErrorLog (e.Message);
                            }

                        }
                        DebugLog.AddEngineLog2 ("scene {0} head length:{1}",
                            SceneLoadInfo.name, headLength.ToString ());
                        bw.Write (headLength);
                        string dsScenePath = string.Format("{0}Scene/{1}/{1}_ds.bytes", 
                            LoadMgr.singleton.BundlePath, 
                            SceneLoadInfo.name);
                        bw.Write(File.Exists(dsScenePath));
                        // DebugLog.AddLog2 ("Scene:{0} HeadLength:{1}====================================", 
                        // SceneLoadInfo.name, headLength.ToString ());
                    }
                    bw.Write (maxChunkCount);
                    bw.Write (maxSplatCount);
                }
                AssetDatabase.ImportAsset (sceneListPath, ImportAssetOptions.ForceUpdate);
                return sceneName;
            }
            return null;
        }

        //[MenuItem("Assets/Tool/Scene/MoveRes")]
        //public static void MoveRes()
        //{
        //    string rootDir = "Assets/BundleRes/EditorAssetRes/Scene";
        //    string targetDir = "Assets/BundleRes/Scene";
        //    var di = new DirectoryInfo(rootDir);
        //    var dir = di.GetDirectories();
        //    for (int i = 0; i < dir.Length; ++i)
        //    {
        //        var subDi = dir[i];
        //        var files = subDi.GetFiles();
        //        foreach(var file in files)
        //        {
        //            try
        //            {
        //                if(file.Extension!=".meta")
        //                {
        //                    string srcPath = string.Format("{0}/{1}/{2}", rootDir, subDi.Name, file.Name);
        //                    string targetPath = string.Format("{0}/{1}/{2}", targetDir, subDi.Name, file.Name);
        //                    File.Copy(srcPath, targetPath, true);
        //                }
        //            }
        //            catch(Exception e)
        //            {
        //                DebugLog.AddErrorLog(e.Message);
        //            }
        //        }
               
        //    }
        //}
    }

    public partial class BuildScene : PreBuildPreProcess
    {

        public override string Name { get { return "Scene"; } }
        public override int Priority
        {
            get
            {
                return 0;
            }
        }

        public override void PreProcess()
        {
            string sceneListPath = string.Format("{0}/SceneList.asset",
                AssetsConfig.instance.EngineResPath);
            SceneList sceneListConfig = AssetDatabase.LoadAssetAtPath<SceneList>(sceneListPath);
            if (sceneListConfig != null)
            {
                SceneSerialize.FastSaveSceneLoadList(sceneListConfig, false);
//#if !UNITY_ANDROID
                string path = "Assets/StreamingAssets/Bundles/assets/bundleres/Scene";


                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists)
                {
                    di.Delete(true);
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                for (int i = 0; i < sceneListConfig.sceneList.Count; ++i)
                {
                    var scene = sceneListConfig.sceneList[i];
                    if (scene != null && !scene.notBuild && scene.sceneAsset != null)
                    {
                        string scenename = scene.sceneAsset.name.ToLower();
                        if (scenename != "test")
                        {
                            //string relative = string.Format("scene/{0}/{0}.bytes", scenename);
                            //CopyFile(relative, relative);

                            //relative = string.Format("scene/{0}/{0}_ds.bytes", scenename);
                            //CopyFile(relative, relative);
                        }
                    }
                }
//#endif
            }

        }
    }
}
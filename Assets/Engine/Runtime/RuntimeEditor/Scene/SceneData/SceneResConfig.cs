#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{

    public enum ESceneType
    {
        Scene,
        DynamicScene
    }
    public abstract class SceneResProcess
    {
        public SceneResData resData;

        protected EditorCommon.EnumTransform preSerialize;
        protected EditorCommon.EnumTransform serialize;

        public virtual bool HasGUI { get { return false; } }
        public virtual void Init(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            if (resData.hasNode && !string.IsNullOrEmpty (resData.resName) &&
                ssContext != null)
            {
                var t = ssContext.root.Find (resData.resName);
                if (t == null)
                {
                    t = new GameObject (resData.resName).transform;
                    t.parent = ssContext.root;
                }
                if(resData.hasSceneTag)
                {
                    resData.workspace = t.Find(sceneContext.suffix);
                    if (resData.workspace == null)
                    {
                        resData.workspace = new GameObject(sceneContext.suffix).transform;
                        resData.workspace.parent = t;
                    }
                }
                else
                {
                    resData.workspace = t;
                }

                ssContext.sceneResMap[resData.resName] = t;
            }
        }
        public virtual void UnInit(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
        }

        public virtual void InitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {

        }

        public virtual void UnInitGUI(ref SceneContext sceneContext, SceneConfig sceneConfig)
        {

        }
        public virtual void PreSerialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            if (preSerialize != null)
            {
                string path = GetPath (ref sceneContext);
                ssContext.preSerialize = preSerialize;
                EditorCommon.EnumTargetObject (path, preSerialize, ssContext);
            }
        }

        public virtual void Serialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            if (serialize != null)
            {
                string path = GetPath (ref sceneContext);
                ssContext.serialize = serialize;
                EditorCommon.EnumTargetObject (path, serialize, ssContext);
            }
        }
        public virtual void PreSaveChunk (ref SceneContext sceneContext, BaseSceneContext bsc,
            ChunkData chunk, ChunkSaveData saveChunk, int i)
        {

        }
        public virtual void PreSave (ref SceneContext sceneContext, BaseSceneContext ssc)
        {

        }
        public virtual void OnGUI (ref SceneContext sceneContext, object param, ref Rect rect)
        {

        }

        public virtual void OnDrawGizmos(ref SceneContext sceneContext, object param)
        {

        }
        public virtual void OnSceneGUI(ref SceneContext sceneContext, object param)
        {

        }
        public virtual void Update (ref SceneContext sceneContext, object param)
        {

        }

        protected string GetPath (ref SceneContext sceneContext, string path)
        {
            if (sceneContext.suffix != SceneContext.MainTagName)
            {
                return string.Format ("{0}/{1}/{2}", SceneResConfig.SceneRoot,
                    path, sceneContext.suffix);
            }
            return string.Format ("{0}/{1}", SceneResConfig.SceneRoot,
                path);
        }

        protected string GetPath (ref SceneContext sceneContext)
        {
            return GetPath (ref sceneContext, resData.resName);
        }

        public static void SavePrefabInfo (SceneSerializeContext ssContext,
            GameObject prefab, out int prefabID)
        {
            if (!ssContext.prefabMap.TryGetValue (prefab, out prefabID))
            {
                var pfd = new PrefabFbxData ();
                pfd.prefab = prefab;
                pfd.SetID ();
                ssContext.sd.prefabs.Add (pfd);
                prefabID = pfd.ID;
                ssContext.prefabMap[prefab] = pfd.ID;
            }
        }

        public static GameObjectInstanceData GreatePrefabInstance (
            SceneSerializeContext ssContext,
            Transform trans,
            int prefabId)
        {
            GameObjectInstanceData goi = new GameObjectInstanceData ();
            goi.name = trans.name;
            goi.pos = trans.position;
            goi.rotate = trans.rotation;
            goi.scale = trans.localScale;

            goi.tag = trans.gameObject.tag;
            goi.layer = trans.gameObject.layer;
            goi.parentID = ssContext.lastFolderID;
            goi.prefabID = prefabId;
            goi.flag.SetFlag (SceneObject.GameObjectActive, trans.gameObject.activeSelf);
            goi.flag.SetFlag (SceneObject.GameObjectActiveInHierarchy, trans.gameObject.activeInHierarchy);
            goi.SetID ();
            ssContext.sd.gameObjects.Add (goi);
            return goi;
        }
        protected static void EnumFolder (Transform trans, SceneSerializeContext ssContext,
            EditorCommon.EnumTransform cb)
        {

            string path = EditorCommon.GetSceneObjectPath (trans);
            GameObjectGroupData group = new GameObjectGroupData ();
            group.name = trans.name;
            group.scenePath = path;
            group.visible = trans.gameObject.activeSelf;
            group.SetID ();
            ssContext.folderIDStack.Push(group.ID);
            if (ssContext.sd != null)
                ssContext.sd.groups.Add(group);
            else if (ssContext.dsd != null)
                ssContext.dsd.groups.Add(group);
            ssContext.lastFolderID = group.ID;
            EditorCommon.EnumChildObject (trans, ssContext, cb);
            ssContext.folderIDStack.Pop();
            if (ssContext.folderIDStack.Count > 0)
            {
                ssContext.lastFolderID = ssContext.folderIDStack.Peek();
            }
            else
            {
                ssContext.lastFolderID = -1;
            }
        }

        protected void EnumTarget (ref SceneContext sceneContext, object param, string resname,
            EditorCommon.EnumTransform cb)
        {
            if (cb != null)
            {
                string path = GetPath (ref sceneContext, resname);
                EditorCommon.EnumTargetObject (path, cb, param);
            }
        }
    }

    [System.Serializable]
    public class SceneResData
    {
        public string resName = "";
        public string systemName = "";
        public bool hasNode = false;//create real gameobject in scene
        public bool hasSceneTag = true;//create sub tag
        public ESceneType sceneType = ESceneType.Scene;

        [System.NonSerialized]
        public SceneResProcess process;

        [System.NonSerialized]
        public Transform workspace;
    }

    public class SceneResConfig : AssetBaseConifg<SceneResConfig>
    {
        public List<SceneResData> configs = new List<SceneResData> ();

        //public LodSize lodNearSize = new LodSize () { size = 5, dist = 32, fade = 64 };
        //public LodSize lodFarSize = new LodSize () { size = 20, dist = 64, fade = 128 };
        //public float lodHeight = 5;

        #region lightmap
        public string bakeObjFolderName = "StaticPrefabs";
        public string bakeTerrainFolderName = "MeshTerrain";
        #endregion
        public static string SceneRoot = "EditorScene";

        public void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            if (ssContext != null)
            {
                var root = GameObject.Find (SceneRoot);
                if (root == null)
                {
                    root = new GameObject (SceneRoot);
                }
                ssContext.root = root.transform;
            }
            var configs = instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process == null && !string.IsNullOrEmpty (config.systemName))
                {
                    var type = EngineUtility.GetAssemblyTypeByName (typeof (SceneResProcess), config.systemName);
                    if (type != null)
                    {
                        config.process = Activator.CreateInstance (type) as SceneResProcess;
                    }
                }
                if (config.process != null)
                {
                    config.process.resData = config;
                    if (ssContext != null)
                        config.process.Init (ref sceneContext, ssContext);
                }
            }
        }

        public void Uninit(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            var configs = instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null)
                {
                    if (ssContext != null)
                        config.process.UnInit(ref sceneContext, ssContext);
                }
            }
        }
        public void InitGUI(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            var configs = instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null)
                {
                    if (ssContext != null)
                        config.process.InitGUI(ref sceneContext, ssContext.sceneConfig);
                }
            }
        }

        public void UninitGUI(ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            var configs = instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null)
                {
                    if (ssContext != null)
                        config.process.UnInitGUI(ref sceneContext, ssContext.sceneConfig);
                }
            }
        }
    }
}

#endif
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
    public class ColliderSystem : SceneResProcess
    {
        private Dictionary<int, GameObject> chunkCollider = new Dictionary<int, GameObject> ();
        public static ColliderSystem system;
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
            if (trans.TryGetComponent<Collider> (out var collider))
            {
                Vector3 scale = trans.localScale;
                scale.x = Mathf.Abs (scale.x);
                scale.y = Mathf.Abs (scale.y);
                scale.z = Mathf.Abs (scale.z);
                trans.localScale = scale;
                string path = "BoxCollider";
                if (collider is SphereCollider)
                {
                    path = "SphereCollider";
                }
                else if (collider is CapsuleCollider)
                {
                    path = "CapsuleCollider";
                }
                else if (collider is MeshCollider)
                {
                    path = "MeshCollider";
                }
                SceneAssets.SortSceneObjectName (collider, path, ssContext.objIDMap);
                var goi = StaticObjectSystem.GreatePrefabInstance (ssContext, trans, -1);
                Save (ssContext, trans, goi.ID, collider);

                if (trans.gameObject.activeInHierarchy &&
                    trans.gameObject.layer != DefaultGameObjectLayer.TerrainLayer)
                {
                    var pos = trans.position;
                    var process = ssContext.resProcess as ColliderSystem;
                    int index = SceneQuadTree.FindChunkIndex (ref pos,
                        ssContext.chunkWidth, ssContext.widthCount, ssContext.heightCount, out var x, out var z);
                    var chunk = ssContext.sd.GetChunk (index, true);
                    if (chunk != null)
                    {
                        if (!process.chunkCollider.TryGetValue (index, out var go))
                        {
                            go = new GameObject (string.Format ("Collider_{0}", index.ToString ()));
                            process.chunkCollider.Add (index, go);
                        }
                        var copy = ColliderData.Copy (collider);
                        if (copy != null)
                        {
                            var t = copy.transform;
                            t.parent = go.transform;
                            t.position = pos;
                            t.rotation = trans.rotation;
                            t.localScale = scale;
                            t.tag = trans.gameObject.tag;
                            t.gameObject.layer = trans.gameObject.layer;
                            chunk.hasCollider = true;
                            if (copy is MeshCollider)
                            {
                                chunk.colliderMesh.Add (string.Format("{0}_0",(copy as MeshCollider).sharedMesh.name));
                            }
                        }
                    }

                }
            }
            else
            {
                EnumFolder (trans, ssContext, ssContext.serialize);

            }
        }

        public override void Serialize (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            chunkCollider.Clear ();
            base.Serialize (ref sceneContext, ssContext);
            var it = chunkCollider.GetEnumerator ();
            while (it.MoveNext ())
            {
                var v = it.Current.Value;
                string path = string.Format ("{0}/Scene/{1}/{2}.prefab",
                    AssetsConfig.instance.ResourcePath, sceneContext.name, v.name);
                PrefabUtility.SaveAsPrefabAsset (v, path);
                GameObject.DestroyImmediate (v);
            }
            chunkCollider.Clear ();
        }
        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            var ssc = bsc as SceneSaveContext;
            for (int i = 0; i < ssc.sd.chunks.Count; ++i)
            {
                var chunk = ssc.sd.chunks[i];
                var saveChunk = ssc.saveSD.chunks[i];
                if (chunk.hasCollider)
                {
                    string colliderPath = string.Format ("{0}/Scene/{1}/Collider_{2}.prefab",
                        AssetsConfig.instance.ResourcePath, sceneContext.name, i.ToString ());
                    if (File.Exists (colliderPath))
                    {
                        for (int j = 0; j < chunk.colliderMesh.Count; ++j)
                        {
                            var resName = chunk.colliderMesh[j];
                            ssc.resAsset.AddResReDirct (
                                LoadMgr.singleton.editorResPath,
                                string.Format ("{0}.asset", resName),
                                ReDirectRes.LogicPath_Common);
                        }
                    }
                    saveChunk.hasCollider = true;
                }
            }
        }
    }
}
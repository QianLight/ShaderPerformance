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
    public class SFXSystem : SceneResProcess
    {
        public static Dictionary<string, GameObject> animSfx = new Dictionary<string, GameObject>();
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
            serialize = SerializeCb;
        }
        ////////////////////////////PreSerialize////////////////////////////
        protected static void PreSerializeCb(Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<SFXWrapper>(out var sfx))
            {
                sfx.areaMask = 0xffffffff;
            }
            else
            {
                EnumFolder(trans, ssContext, ssContext.preSerialize);
            }
        }

        ////////////////////////////Serialize////////////////////////////
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
            {
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource (trans.gameObject) as GameObject;
                if (prefab != null)
                {
                    if (trans.TryGetComponent<SFXWrapper>(out var sfx))
                    {
                        SceneAssets.SortSceneObjectName(trans, prefab.name, ssContext.objIDMap);
                        SavePrefabInfo(ssContext, prefab, out var prefabId);
                        var goi = StaticObjectSystem.GreatePrefabInstance(ssContext, trans, prefabId);
                        SceneObjectData so = new SceneObjectData();
                        so.resName = prefab.name.ToLower();
                        so.pos = trans.position;
                        so.rotate = trans.rotation;
                        so.scale = trans.lossyScale;
                        so.localScale = trans.localScale;
                        so.exString = sfx.exString;
                        so.flag.SetFlag(sfx.flag.flag);
                        so.flag.SetFlag(SceneObject.HasAnim, !string.IsNullOrEmpty(sfx.exString));
                        so.flag.SetFlag(SceneObject.ExString, !string.IsNullOrEmpty(sfx.exString));
                        so.flag.SetFlag(SceneObject.GameObjectActiveInHierarchy, trans.gameObject.activeInHierarchy);
                        so.flag.SetFlag(SceneObject.GameObjectActive, trans.gameObject.activeSelf);
                        so.flag.SetFlag(SceneObject.RenderEnable, true);
                        so.flag.SetFlag(SceneObject.IsSfx, true);
                        so.SetID();
                        so.gameObjectID = goi.ID;
                        so.areaMask = sfx.areaMask;
                        var chunkID = SceneQuadTree.FindChunkIndex(ref so.pos,
                            ssContext.chunkWidth, ssContext.widthCount, ssContext.heightCount, out var x, out var z);
                        var chunk = ssContext.sd.GetChunk(chunkID);
                        chunk.sceneObjects.Add(so);
                    }
                    else
                    {
                        DebugLog.AddErrorLog2("not a sfx prefab:{0}", trans.name);
                    }
                }
            }
            else
            {
                EnumFolder (trans, ssContext, ssContext.serialize);
            }
        }

        ////////////////////////////Public////////////////////////////
        static void InitSfx(Transform trans, object param)
        {
            if (trans.TryGetComponent<SFXWrapper>(out var sfx))
            {
                string key = sfx.exString;
                if (!string.IsNullOrEmpty(key))
                {
                    animSfx[key] = sfx.gameObject;
                }
            }
            else
            {
                EditorCommon.EnumChildObject(trans, null, InitSfx);
            }
        }

        static void InitAnimation(Transform trans, object param)
        {
            if (trans.TryGetComponent<ActiveObject>(out var anim))
            {
                string key = anim.exString;
                if (!string.IsNullOrEmpty(key))
                {
                    animSfx[key] = anim.gameObject;
                }
            }
            else
            {
                EnumFolder(trans, null, InitAnimation);
            }
        }

        public static void InitHideObject()
        {
            animSfx.Clear();
            InitEditorSfx();
            InitEditorAnimation();
        }


        public static void InitEditorSfx(string path = "EditorScene/Effects/MainScene")
        {
            EditorCommon.EnumTargetObject(path, InitSfx, null);
        }

        public static void InitEditorAnimation(string path = "EditorScene/Animation/MainScene")
        {
            EditorCommon.EnumTargetObject(path, InitAnimation, null);
        }

        public static void PlayAnimation(string key, bool play)
        {
            if (animSfx.TryGetValue(key, out var sfx))
            {
                sfx.SetActive(play);
            }
            else
            {
                DebugLog.AddErrorLog2("sfx animation not find:{0}", key);
            }
        }
    }
}
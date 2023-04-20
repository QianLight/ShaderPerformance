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
    public class DynamicObjectSystem : SceneResProcess
    {

        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            serialize = SerializeCb;
            // preSerialize = PreSerializeCb;
        }
        ////////////////////////////PreSerialize////////////////////////////
        // protected static void PreSerializeCb (Transform trans, object param)
        // {
        //     SceneSerializeContext ssContext = param as SceneSerializeContext;
        //     if (trans.TryGetComponent<XWall> (out var wall))
        //     {
        //         if (wall is XSpawnWall ||
        //             wall is XTerminalWall ||
        //             wall is XDummyWall ||
        //             wall is XTransferWall ||
        //             wall is XCircleWall)
        //         {
        //             ssc.AddRes ("", so.matId, AssetDatabase.GetAssetPath (so.mat));
        //             wall.hashStr = string.Format ("{0}{1}", trans.parent.name, trans.name);
        //         }
        //     }
        // }

        ////////////////////////////Serialize////////////////////////////
        protected static void SerializeCb (Transform trans, object param)
        {
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent<XWall> (out var wall))
            {
                SceneDynamicObject sdo = null;
                if (wall is XSpawnWall)
                {
                    var exString = (wall as XSpawnWall).exString;
                    if (string.IsNullOrEmpty (exString))
                    {
                        DebugLog.AddErrorLog2 ("exString null:{0}", trans.name);
                    }
                    else
                    {
                        sdo = new SceneDynamicObject()
                        {
                            permanentFx = false,
                            autoY = false,
                            rotation=Vector3.zero,
                            buffIDStr = ""
                    };
                        sdo.editorType = (int) DynamicObjectType.Spawn;
                        sdo.exString = exString;
                        sdo.flag.SetFlag (SceneDynamicObject.WallTrigger | SceneDynamicObject.ExStringTrigger);
                        if ((wall as XSpawnWall).TriggerType == XSpawnWall.etrigger_type.once)
                            sdo.flag.SetFlag (SceneDynamicObject.TriggerOnce, true);
                    }
                }
                else if (wall is XTerminalWall)
                {
                    var exString = (wall as XTerminalWall).exString;
                    if (string.IsNullOrEmpty (exString))
                    {
                        DebugLog.AddErrorLog2 ("exString null:{0}", trans.name);
                    }
                    else
                    {
                        sdo = new SceneDynamicObject ();
                        sdo.editorType = (int) DynamicObjectType.Terminal;
                        sdo.exString = exString;
                        sdo.flag.SetFlag (SceneDynamicObject.WallTrigger);
                        sdo.permanentFx = false;
                        sdo.buffIDStr = "";
                        sdo.autoY = false;
                        sdo.rotation = Vector3.zero;
                    }

                }
                else if (wall is XDummyWall)
                {
                    sdo = new SceneDynamicObject ()
                    {
                        exString = "",
                        buffIDStr=""
                    };

                    if (trans.TryGetComponent<BoxCollider>(out var box))
                    {
                        sdo.editorType = (int)DynamicObjectType.Dummy;
                        sdo.flag.SetFlag(SceneDynamicObject.BlockTrigger | SceneDynamicObject.DynamicBlock);
                    }
                    else if (trans.TryGetComponent<SphereCollider>(out var sphere))
                    {
                        sdo.editorType = (int)DynamicObjectType.Circle;
                        sdo.flag.SetFlag(SceneDynamicObject.SphereTrigger | SceneDynamicObject.DynamicBlock);
                    }
                        
                    var dw = wall as XDummyWall;
                    if (dw.playerFlag.Forward)
                        sdo.flag.SetFlag (SceneDynamicObject.ForwardPass, true);
                    if (dw.playerFlag.Backward)
                        sdo.flag.SetFlag (SceneDynamicObject.BackPass, true);
                    if (dw.HideFx)
                        sdo.flag.SetFlag(SceneDynamicObject.HideFx, true);
                    sdo.permanentFx = dw.permanentFx;
                    sdo.autoY = dw.autoY;
                    sdo.rotation = dw.transform.eulerAngles;
                    if(!string.IsNullOrEmpty(dw.exString))
                        sdo.exString = dw.exString;
                    if (!string.IsNullOrEmpty(dw.buffIDList))
                        sdo.buffIDStr = dw.buffIDList;
                }
                else if (wall is XTransferWall)
                {
                    XTransferWall tw = (wall as XTransferWall);
                    if (tw.sceneID >= 0 &&
                        tw.targetScene == XTransferWall.transfer_type.other_scene)
                    {
                        var exString = tw.sceneID.ToString ();
                        if (string.IsNullOrEmpty (exString))
                        {
                            DebugLog.AddErrorLog2 ("exString null:{0}", trans.name);
                        }
                        else
                        {
                            sdo = new SceneDynamicObject ();
                            sdo.editorType = (int) DynamicObjectType.Transfer;
                            sdo.exString = exString;
                            sdo.flag.SetFlag (SceneDynamicObject.WallTrigger | SceneDynamicObject.SceneTransferTrigger);
                            sdo.buffIDStr = "";
                            sdo.permanentFx = false;
                            sdo.autoY = false;
                            sdo.rotation = Vector3.zero;
                        }
                    }

                }
                else if (wall is XCircleWall)
                {
                    sdo = new SceneDynamicObject()
                    {
                        exString = "",
                        permanentFx = false,
                        autoY = false,
                        rotation = Vector3.zero,
                        buffIDStr = ""
                };
                    sdo.editorType = (int) DynamicObjectType.Circle;
                    sdo.flag.SetFlag (SceneDynamicObject.SphereTrigger | SceneDynamicObject.DynamicBlock);
                }

                if (sdo != null)
                {
                    wall.hashStr = string.Format ("{0}{1}", trans.parent.name, trans.name);
                    Bounds aabb = new Bounds (trans.position, Vector3.one);
                    if (trans.TryGetComponent<BoxCollider> (out var box))
                    {
                        sdo.center = box.center;
                        sdo.size = box.size;
                        Vector3 half = Vector3.right * box.size.x  * 0.5f;
                        float h = box.size.y * 0.5f * trans.localScale.y;
                        sdo.pos0 = box.center - half;
                        sdo.pos1 = box.center + half;

                        sdo.pos0 = trans.localToWorldMatrix * sdo.pos0;
                        sdo.pos0 += trans.position;
                        sdo.pos0.y = trans.position.y - h;
                        sdo.pos1 = trans.localToWorldMatrix * sdo.pos1;
                        sdo.pos1 += trans.position;
                        sdo.pos1.y = trans.position.y + h;
                    }
                    else if (trans.TryGetComponent<SphereCollider> (out var sphere))
                    {
                        sdo.center = sphere.center;
                        sdo.size = Vector3.one * sphere.radius;
                        sdo.pos0 = sphere.transform.position;
                        sdo.pos1 = sdo.size;
                    }
                    else
                    {
                        sdo.buffIDStr = "";
                        sdo.exString = "";
                        sdo.flag.Reset ();
                    }
                    for (int i = 0; i < trans.childCount; ++i)
                    {
                        Transform child = trans.GetChild (i);
                        GameObject sfx = PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject) as GameObject;
                        if (sfx != null)
                        {
                            if (i == 0)
                            {
                                aabb.center = child.position;
                            }
                            else
                            {
                                aabb.Encapsulate (child.position);
                            }
                            //DebugLog.AddErrorLog(sfx.name);
                            sdo.sfxList.Add (
                                new RenderObject ()
                                {
                                    prefab = sfx,
                                        sfxName = sfx.name.ToLower (),
                                        pos = child.position,
                                        rot = child.rotation,
                                        scale = child.lossyScale,
                                        aabb = AABB.Create (PrefabAssets.GetBound (child))

                                });
                        }
                        else
                        {
                            Debug.LogErrorFormat ("effect is not prefab:{0}", child.name);
                        }
                    }
                    sdo.name = trans.name;
                    sdo.dynamicSceneId = (byte) ssContext.dynamicSceneID;
                    var hashStr = ssContext.dynamicSceneName + trans.name;
                    sdo.hash = EngineUtility.XHashLowerRelpaceDot (0, hashStr);
                    var go = trans.gameObject;
                    if (go.activeInHierarchy &&
                        go.activeSelf)
                        sdo.flag.SetFlag (SceneDynamicObject.IsActive, true);

                    sdo.parentID = ssContext.lastFolderID;
                    ssContext.dsd.dynamicScenes[ssContext.dynamicSceneID].dynamicObjects.Add (sdo);
                }
            }
            else
            {
                if (trans.parent == ssContext.resProcess.resData.workspace)
                {

                    ssContext.dynamicSceneID = ssContext.dsd.dynamicScenes.FindIndex ((x) => x.dynamicSceneName == trans.name);
                    if (ssContext.dynamicSceneID < 0)
                    {
                        ssContext.dynamicSceneID = ssContext.dsd.dynamicScenes.Count;
                        ssContext.dsd.dynamicScenes.Add (new DynamicScene ()
                        {
                            dynamicSceneName = trans.name,
                                parentID = ssContext.lastFolderID
                        });
                    }
                    ssContext.dynamicSceneName = trans.name;
                }
                EnumFolder (trans, ssContext, ssContext.serialize);
            }
        }

        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
            //var dssc = bsc as DynamicSceneSaveContext;
            //for (int i = 0; i < dssc.dsd.dynamicScenes.Count; ++i)
            //{
            //    var ds = dssc.dsd.dynamicScenes[i];
            //    for (int j = 0; j < ds.dynamicObjects.Count; ++j)
            //    {
            //        var sdo = ds.dynamicObjects[j];
            //        for (int k = 0; k < sdo.sfxList.Count; ++k)
            //        {
            //            var ro = sdo.sfxList[k];
            //        }
            //    }
            //}
        }

        ////////////////////////////Save////////////////////////////
        public static void SaveEditorRes (SceneSaveContext ssc, DynamicSceneSaveContext dssc)
        {
            // for (int i = 0; i < dssc.dsd.dynamicScenes.Count; ++i)
            // {
            //     var ds = dssc.dsd.dynamicScenes[i];
            //     for (int j = 0; j < ds.dynamicObjects.Count; ++j)
            //     {
            //         var sdo = ds.dynamicObjects[j];
            //         ssc.AddRes ("dynamicObject", sdo.name, sdo.name);
            //     }
            // }
        }
    }
}
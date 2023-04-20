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
    public class AnimationSystem : SceneResProcess
    {
        public override void Init (ref SceneContext sceneContext, SceneSerializeContext ssContext)
        {
            base.Init (ref sceneContext, ssContext);
            preSerialize = PreSerializeCb;
        }

        ////////////////////////////PreSerialize////////////////////////////

        ////////////////////////////Serialize////////////////////////////
        protected static void PreSerializeCb (Transform trans, object param)
        {
#if ANIMATION_OBJECT_V0
            SceneSerializeContext ssContext = param as SceneSerializeContext;
            if (trans.TryGetComponent (out SceneAnimation sa))
            {
                if (string.IsNullOrEmpty (sa.exString))
                {
                    SceneAssets.SortSceneObjectName (sa, "SceneAnimation", ssContext.objIDMap);
                }
                else
                {
                    sa.name = string.Format ("SA_{0}", sa.exString);
                }
                SceneAnimationObject sao = sa.Serialize ();
                if (sa != null)
                {
                    ssContext.sd.animationDatas.Add (sao);
                }
            }
            else
            {

                EnumFolder (trans, ssContext, ssContext.preSerialize);
            }
#endif
        }
        ////////////////////////////PreSave////////////////////////////
        public override void PreSave (ref SceneContext sceneContext, BaseSceneContext bsc)
        {
#if ANIMATION_OBJECT_V0
            var ssc = bsc as SceneSaveContext;
            for (int i = 0; i < ssc.sd.animationDatas.Count; ++i)
            {
                var ad = ssc.sd.animationDatas[i];
                if (ad.profile != null)
                {
                    uint hash = EngineUtility.XHashLowerRelpaceDot (0, ad.profile.exString);
                    ssc.AddRes (AnimationObject.animType, hash, ad.profile.exString);
                }
            }
#endif
        }

        ////////////////////////////Save////////////////////////////

        public static void SaveHead (BinaryWriter bw, SceneSaveContext ssc)
        {
            short animCount = (short) ssc.sd.animationDatas.Count;
            bw.Write (animCount);
            for (int i = 0; i < animCount; ++i)
            {
                var ad = ssc.sd.animationDatas[i];
                ad.Save (bw);
            }
            DebugLog.DebugStream (bw, "AnimationHead");
        }
    }
}
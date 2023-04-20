//#define MRO_ON
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]

    public class ActiveObject : SceneAnimation
    {
        public bool objGroup = true;
        public Transform animationTargetGroup;
        [System.NonSerialized]
        public List<ObjectCache> objects;// = new List<ObjectCache> ();
        
        protected override void OnEnable ()
        {
            base.OnEnable();
            objects = ListPool<ObjectCache>.Get();
            GetObjects (animationTargetGroup, objects);
        }

        protected override void OnDisable()
        {
            ListPool<ObjectCache>.Release(objects);
            base.OnDisable();
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected ()
        {
            var c = Gizmos.color;
            Gizmos.color = Color.green;
            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                if (obj.r != null)
                {
                    Gizmos.DrawWireCube (obj.r.bounds.center, obj.r.bounds.size);
                }

            }
            Gizmos.color = c;
        }
        
        #if ANIMATION_OBJECT_V0
        public void Save ()
        {
            if (!string.IsNullOrEmpty (exString))
            {
                var aod = ActiveObjectData.CreateInstance<ActiveObjectData> ();
                aod.exString = exString;
                aod.animationGroupPath = EditorCommon.GetSceneObjectPath (animationTargetGroup);

                string lmdName = string.Format ("SceneAnimation_{0}", exString);
                aod.name = lmdName;
                SceneContext sceneContext = new SceneContext ();
                SceneAssets.GetCurrentSceneContext (ref sceneContext);
                aod = EditorCommon.CreateAsset<ActiveObjectData> (sceneContext.configDir,
                    lmdName, ".asset", aod);
                profile = aod;
            }
        }

        public override SceneAnimationObject Serialize ()
        {
            if (profile is ActiveObjectData)
            {
                var aod = profile as ActiveObjectData;
                GetObjects (animationTargetGroup, objects);
                if (objects.Count > 0)
                {
                    var sao = new SceneAnimationObject ()
                    {
                        profile = profile,
                    };
                    #if MRO_ON
                    for (int i = 0; i < objects.Count; ++i)
                    {
                        var obj = objects[i];
                        obj.sceneAnim.SetAnim(profile.exString, 0);
                    }        
                    #endif
                    return sao;
                }

            }
            return null;
        }
        #endif
        
    #endif

        public override void Play(bool state, float speed)
        {
            base.Play(state, speed);
            SetVisible(state);
        }

        public void SetVisible(bool flag)
        {
            Debug.Log(animationTargetGroup.gameObject.name+" SetVisible:"+flag);
            animationTargetGroup.gameObject.SetActive(flag);
            for (int i = 0; i < objects.Count; ++i)
            {
                var oc = objects[i];
                oc.r.enabled = flag;
                // oc.r.gameObject.SetActive(flag);
            }
        }
    }
}


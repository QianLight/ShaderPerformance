//#define MRO_ON
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class LoopMove : SceneAnimation
    {
        public string ExString => exString;
        public bool SetPlayState(bool state) => isPlay = state;
        
        [Range (-100, 100)]
        public float moveSpeed = 0;
        [Range (0, 360)]
        public float angle = 0;
        public Transform startPoint = null;
        public Transform endPoint = null;

        public bool objGroup = true;
        public Transform animationTargetGroup;
        public Transform animationMatTargetGroup;
        [System.NonSerialized]
        public List<ObjectCache> objects = new List<ObjectCache> ();
        [System.NonSerialized]
        public List<ObjectCache> matObjects = new List<ObjectCache> ();
        public bool matGroup = true;
        public bool matObjFolder = true;
        [Range (-30, 30)]
        public float uvMoveX = 0;
        [Range (-30, 30)]
        public float uvMoveY = 0;
        public float drawWidth = 10;
        private Vector3 normal;
        private Vector3 offsetDist;
        
        void Start ()
        {
            GetObjects (animationTargetGroup, objects);
            GetObjects (animationMatTargetGroup, matObjects);
        }

        void Update ()
        {
            var context = EngineContext.instance;
            if (context != null)
            {
                context.renderflag.SetFlag(EngineContext.RFlag_DisableCalcCaluceShadow, false);
            }
            if (Application.isPlaying)
            {
                var x = Mathf.Sin(Mathf.Deg2Rad * angle);
                var y = Mathf.Cos(Mathf.Deg2Rad * angle);
                normal = new Vector3(x, 0, y);
                if (isPlay)
                {
                    if (Mathf.Abs(moveSpeed) > 0.0001f && startPoint != null && endPoint != null)
                    {
                        if (context != null)
                        {
                            context.renderflag.SetFlag(EngineContext.RFlag_DisableCalcCaluceShadow, true);
                        }
                        float dist = Vector3.Dot(normal, endPoint.position);
                        float dist2 = Vector3.Dot(normal, startPoint.position);
                        float delta = Mathf.Abs(dist2 - dist);
                        var moveDelta = moveSpeed * normal * Time.deltaTime;
                        offsetDist += moveDelta;
                        float d = Vector3.Magnitude(offsetDist);
                        if (d >= delta)
                        {
                            offsetDist = Vector3.zero;
                        }
                        Shader.SetGlobalVector(ShaderManager._ShadowMoveOffset, -offsetDist);
                        for (int i = 0; i < objects.Count; ++i)
                        {
                            var obj = objects[i];
                            if (obj.t != null)
                            {
                                obj.t.position += moveDelta;
                                if ((Vector3.Dot(normal, obj.t.position) - dist) > 0)
                                {
                                    obj.t.position -= normal * delta;
                                }
                            }

                        }
                        Vector2 uvOffset = new Vector2(uvMoveX, uvMoveY) * Time.deltaTime;
                        for (int i = 0; i < matObjects.Count; ++i)
                        {
                            var obj = matObjects[i];
                            #if MRO_ON
                            if (obj.sceneAnim != null)
                            {
                                obj.sceneAnim.SetUVOffset(ref uvOffset);
                            }
                            #endif
                        }
                    }
                }
            }
            else
            {
                Shader.SetGlobalVector(ShaderManager._ShadowMoveOffset, Vector4.zero);
            }
            
        }
    #if UNITY_EDITOR
        private void OnDrawGizmosSelected ()
        {
            var c = Gizmos.color;
            if (startPoint != null)
            {
                Gizmos.color = Color.green;
                RuntimeUtilities.DrawPlane (startPoint.position, normal, Vector3.up, drawWidth, drawWidth * 0.5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine (startPoint.position, startPoint.position + normal * 5);
            }
            if (endPoint != null)
            {
                Gizmos.color = Color.red;
                RuntimeUtilities.DrawPlane (endPoint.position, normal, Vector3.up, drawWidth, drawWidth * 0.5f);
            }
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

        public void Save ()
        {
            #if ANIMATION_OBJECT_V0
            if (!string.IsNullOrEmpty (exString) && startPoint != null && endPoint != null)
            {
                var lmd = LoopMoveData.CreateInstance<LoopMoveData> ();
                lmd.exString = exString;
                lmd.lma.duration = duration;
                lmd.lma.play = autoPlay;
                lmd.lma.moveSpeed = moveSpeed;
                lmd.lma.angle = angle;
                lmd.lma.start = startPoint.position;
                lmd.lma.end = endPoint.position;

                lmd.startPoint = EditorCommon.GetSceneObjectPath (startPoint);
                lmd.endPoint = EditorCommon.GetSceneObjectPath (endPoint);
                lmd.animationGroupPath = EditorCommon.GetSceneObjectPath (animationTargetGroup);
                lmd.animationMatGroupPath = EditorCommon.GetSceneObjectPath (animationMatTargetGroup);

                lmd.lma.uvMove = new Vector2 (uvMoveX, uvMoveY);

                string lmdName = string.Format ("SceneAnimation_{0}", exString);
                lmd.name = lmdName;
                SceneContext sceneContext = new SceneContext ();
                SceneAssets.GetCurrentSceneContext (ref sceneContext);
                lmd = EditorCommon.CreateAsset<LoopMoveData> (sceneContext.configDir,
                    lmdName, ".asset", lmd);
                profile = lmd;
            }
            #endif
        }

        public override SceneAnimationObject Serialize ()
        {
            #if ANIMATION_OBJECT_V0
            if (profile is LoopMoveData lmd)
            {
                GetObjects(animationTargetGroup, objects);
                GetObjects(animationMatTargetGroup, matObjects);
                if (objects.Count > 0 || matObjects.Count > 0)
                {
                    var sao = new SceneAnimationObject()
                    {
                        profile = profile,
                    };
                    for (int i = 0; i < objects.Count; ++i)
                    {
                        var obj = objects[i];
                        #if MRO_ON
                        obj.sceneAnim.SetAnim(profile.exString, 0);
                        #endif
                    }
                    for (int i = 0; i < matObjects.Count; ++i)
                    {
                        var obj = matObjects[i];
                        #if MRO_ON
                        obj.sceneAnim.SetAnim(profile.exString, SceneObject.UVAnimation);
                        #endif
                    }
                    return sao;
                }
            }
            #endif
            return null;
        }
    #endif
    }
}


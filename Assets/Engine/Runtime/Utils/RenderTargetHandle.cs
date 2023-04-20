using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif
namespace CFEngine
{
    public struct RenderTargetHandle
    {
        public int id { set; get; }
        public RenderTargetIdentifier rtID;

        public static RenderTargetHandle CameraTarget = new RenderTargetHandle (-1);

#if UNITY_EDITOR
        public bool autoRelease;
        public static Dictionary<int, string> targets = new Dictionary<int, string> ();
        public static HashSet<int> frameTargets = new HashSet<int> ();
#endif
        public RenderTargetHandle (string shaderProperty)
        {
            id = Shader.PropertyToID (shaderProperty);
            rtID = new RenderTargetIdentifier (id);

#if UNITY_EDITOR
            autoRelease = false;
            if (!targets.ContainsKey (id))
                targets[id] = shaderProperty;
#endif
        }
        public RenderTargetHandle (int shaderProperty)
        {
            id = shaderProperty;
            rtID = new RenderTargetIdentifier (id);
#if UNITY_EDITOR
            autoRelease = false;
#endif
        }

        public override bool Equals (object obj)
        {
            if (ReferenceEquals (null, obj)) return false;
            return obj is RenderTargetHandle && id == ((RenderTargetHandle) obj).id;
        }

        public override int GetHashCode ()
        {
            return id;
        }

        public static bool operator == (RenderTargetHandle c1, RenderTargetHandle c2)
        {
            return c1.id == c2.id;
        }

        public static bool operator != (RenderTargetHandle c1, RenderTargetHandle c2)
        {
            return c1.id != c2.id;
        }
    }
}
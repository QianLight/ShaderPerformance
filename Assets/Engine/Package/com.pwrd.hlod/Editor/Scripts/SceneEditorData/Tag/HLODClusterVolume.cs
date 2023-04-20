using System;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    
    [ExecuteInEditMode]
    public class HLODClusterVolume : MonoBehaviour
    {
        private Color purple = new Color(0.5f, 0.2f, 0.5f);

        public void Awake()
        {
            this.gameObject.tag = "EditorOnly";
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = purple;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        public bool Contains(Renderer renderer)
        {
            var bounds = renderer.bounds;
            var min = bounds.min;
            var max = bounds.max;

            min = transform.worldToLocalMatrix * new Vector4(min.x, min.y, min.z, 1);
            max = transform.worldToLocalMatrix * new Vector4(max.x, max.y, max.z, 1);

            if (min.x < -0.5f || min.x > 0.5f || min.y < -0.5f || min.y > 0.5f || min.z < -0.5f || min.z > 0.5f ||
                max.x < -0.5f || max.x > 0.5f || max.y < -0.5f || max.y > 0.5f || max.z < -0.5f || max.z > 0.5f)
                return false;
            return true;
        }
        
        
        public static void AddByMenu()
        {
        
        }
    }
}
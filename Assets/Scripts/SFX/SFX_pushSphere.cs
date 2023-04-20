using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CFEngine
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class SFX_pushSphere : MonoBehaviour
    {
#if UNITY_EDITOR

        public Mesh mesh = null;
        public bool DrawGizmos = true;
#endif    
        public float SphereScale = 1.0f;
        private Vector4 ObstructSphere = new Vector4 ();
        private Transform t;
        private static readonly int ObstructSphereID = Shader.PropertyToID ("_ObstructSphere");
        // Start is called before the first frame update
        void Start ()
        {
            t = transform;
        }
#if UNITY_EDITOR
        private void OnDrawGizmos ()
        {
            if (DrawGizmos)
            {

                Gizmos.color = new Vector4 (1, 1, 0, 0.5f); //为随后绘制的gizmos设置颜色。
                Gizmos.DrawWireSphere (transform.position, SphereScale); //使用center和radius参数，绘制一个线框球体。
                Gizmos.DrawMesh (mesh, transform.position, transform.rotation, new Vector3 (SphereScale * 2, SphereScale * 2, SphereScale * 2));

            }
        }
#endif
        // Update is called once per frame
        void Update ()
        {
            if (t != null)
            {
                ObstructSphere.x = t.position.x;
                ObstructSphere.y = t.position.y;
                ObstructSphere.z = t.position.z;
                ObstructSphere.w = SphereScale;
                Shader.SetGlobalVector (ObstructSphereID, ObstructSphere);
            }
        }

    }
}
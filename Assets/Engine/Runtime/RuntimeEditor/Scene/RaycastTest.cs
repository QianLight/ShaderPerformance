#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    [ExecuteInEditMode]
    public class RaycastTest : MonoBehaviour
    {
        private Transform t;
        private void Update()
        {
            if (t == null)
            {
                t = this.transform;
            }
            var context = EngineContext.instance;
            if (t != null && context != null)
            {
                ref Vector3 cameraPos = ref context.cameraPos;
                context.cameraPlane = context.CameraTransCache.right;
                context.cameraPlaneDist = -EngineUtility.Dot(ref cameraPos, ref context.cameraPlane);

                var cameraUp = context.CameraTransCache.up;
                var camera2Target = context.lookAtPos - context.cameraPos;
                EngineUtility.Ray2D(camera2Target.x, camera2Target.z, cameraPos.x, cameraPos.z, ref context.camera2Target.normalXZ);
                EngineUtility.Ray2D(camera2Target.z, camera2Target.y, cameraPos.z, cameraPos.y, ref context.camera2Target.normalYZ);
                EngineUtility.Ray2D(camera2Target.x, camera2Target.y, cameraPos.x, cameraPos.y, ref context.camera2Target.normalXY);

                TestRay(context, t);
            }
        }

        private void TestRay(EngineContext context, Transform t)
        {
            if (t.TryGetComponent(out Renderer r))
            {
                AABB aabb = AABB.Create(r.bounds);
                if (EngineUtility.FastTestRayAABB(ref context.cameraPlane, context.cameraPlaneDist, ref aabb) &&
                    EngineUtility.FastTestRayAABB(ref context.cameraPos, ref context.lookAtPos, ref context.camera2Target, ref aabb, out var rayAABB))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawWireCube(aabb.center, aabb.size);
            }
            for (int i = 0; i < t.childCount; ++i)
            {
                TestRay(context, t.GetChild(i));
            }
        }
    }
}

#endif
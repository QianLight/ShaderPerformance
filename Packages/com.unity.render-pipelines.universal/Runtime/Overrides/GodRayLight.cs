
#if UNITY_EDITOR
using System;
namespace UnityEngine.Rendering.Universal
{
    public class GodRayLight : MonoBehaviour
    {
        public float LineLength = 500.0f;

        private void OnDrawGizmos()
        {
            Vector3 toPos = transform.forward * LineLength + transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, toPos);
            Gizmos.DrawSphere(transform.position, 1);
            Gizmos.DrawSphere(toPos, 2);
        }
    }
}
#endif

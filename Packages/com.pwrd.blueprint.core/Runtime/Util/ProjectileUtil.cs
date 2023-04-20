using UnityEngine;

namespace Blueprint.Util
{

    public static class ProjectileUtil
    {
        /// <summary>
        /// copy from ue GamePlayStatics.cpp
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="tossSpeed"></param>
        /// <param name="favorHighArc"></param>
        /// <param name="gravityZ"></param>
        /// <returns></returns>
        public static Vector3 SuggestProjectSpeed(Vector3 startPos, Vector3 endPos, float tossSpeed, bool favorHighArc, float gravityZ)
        {
            Vector3 flightDelta = endPos - startPos;
            Vector3 XZ = new Vector2(flightDelta.x, flightDelta.z);
            Vector3 dirXZ = XZ.normalized;
            float deltaXZ = XZ.magnitude;

            float deltaY = flightDelta.y;

            float tossSpeedSq = tossSpeed * tossSpeed;
            gravityZ = -gravityZ;

            // v^4 - g * (g*x^2 + 2*y*v^2)
            float insideTheSqrt = tossSpeedSq * tossSpeedSq - gravityZ * ((gravityZ * deltaXZ * deltaXZ) + (2f * deltaY * tossSpeedSq));

            if (insideTheSqrt < 0)
            {
                return Vector3.zero;
            }

            float sqrtPart = Mathf.Sqrt(insideTheSqrt);

            float tanSolutionAngleA = (tossSpeedSq + sqrtPart) / (gravityZ * deltaXZ);
            float tanSolutionAngleB = (tossSpeedSq - sqrtPart) / (gravityZ * deltaXZ);

            float magXZSq_A = tossSpeedSq / (tanSolutionAngleA * tanSolutionAngleA + 1f);
            float magXZSq_B = tossSpeedSq / (tanSolutionAngleB * tanSolutionAngleB + 1f);

            float favoredMagXZSq = favorHighArc ? Mathf.Min(magXZSq_A, magXZSq_B) : Mathf.Max(magXZSq_A, magXZSq_B);
            float ySign = favorHighArc ?
                            (magXZSq_A < magXZSq_B) ? Mathf.Sign(tanSolutionAngleB) : Mathf.Sign(tanSolutionAngleA):
                            (magXZSq_A > magXZSq_B) ? Mathf.Sign(tanSolutionAngleB) : Mathf.Sign(tanSolutionAngleA);

            float magXZ = Mathf.Sqrt(favoredMagXZSq);
            float magY = Mathf.Sqrt(tossSpeedSq - favoredMagXZSq);
            return (dirXZ * magXZ) + (Vector3.up * magY * ySign);
        }
    }

}
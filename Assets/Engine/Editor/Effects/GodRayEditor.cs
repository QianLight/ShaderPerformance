using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (GodRay))]
    public sealed class GodRayEditor : EnvEffectEditor<GodRay>
    {
        SerializedParameterOverride godRayParam0;
        SerializedParameterOverride godRayParam1;
        SerializedParameterOverride color;
        public override void OnEnable ()
        {
            GodRay godRay = target as GodRay;
            godRayParam0 = FindParameterOverride (x => x.godRayParam0);
            godRayParam1 = FindParameterOverride (x => x.godRayParam1);
            color = FindParameterOverride (x => x.color);
        }

        public override void OnInspectorGUI ()
        {
            GodRay godRay = target as GodRay;
            EditorUtilities.DrawHeaderLabel ("GodRay");
            PropertyField (godRayParam0);
            PropertyField (godRayParam1);
            PropertyField (color);
            EditorUtilities.DrawRect ("Debug");
            GodRay.debug.Value = EditorGUILayout.Toggle("debug", GodRay.debug.Value);
            if (GodRay.debug.Value)
                EditorGUILayout.Toggle ("IsGodrayOn", GodRayModify.hasEffect);
        }

        public override void OnSceneGUI ()
        {
            GodRay godRay = target as GodRay;
            EngineContext context = EngineContext.instance;
            if (context != null && GodRay.debug.Value)
            {
                Color c = Handles.color;
                Handles.color = Color.yellow;
                Handles.Label (GodRayModify.debugSunPos + Vector3.up * 1, "Sun");
                Handles.SphereHandleCap (102, GodRayModify.debugSunPos, Quaternion.identity, 1, EventType.Repaint);
                var look = context.CameraTransCache.forward;
                var far = context.cameraPos + look * 10;

                Handles.DrawLine (context.cameraPos, far);
                var right = context.CameraTransCache.right;
                var up = context.CameraTransCache.up;

                float angle = Mathf.Deg2Rad * 70;
                float radius = Mathf.Tan (angle) * 10; 
                Handles.DrawLine (context.cameraPos, far + up * radius);
                Handles.DrawLine (context.cameraPos, far - up * radius);
                Handles.DrawLine (context.cameraPos, far + right * radius);
                Handles.DrawLine (context.cameraPos, far - right * radius);
                Handles.DrawWireDisc (far, look, radius);
                if (GodRayModify.hasEffect)
                    Handles.color = Color.green;
                else
                    Handles.color = Color.red;
                Handles.DrawDottedLine (GodRayModify.debugSunPos, context.cameraPos, 5);

                Handles.color = c;

            }

        }
    }
}
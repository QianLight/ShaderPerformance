using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (SceneMisc))]
    public sealed class SceneMiscEditor : EnvEffectEditor<SceneMisc>
    {
        SerializedParameterOverride cameraParam0;
        SerializedParameterOverride windParam0;

        SerializedParameterOverride windParam1;
        ClassSerializedParameterOverride windMap;
        SerializedParameterOverride sceneColor;

        public override void OnEnable ()
        {
            SceneMisc sceneMisc = target as SceneMisc;
            cameraParam0 = FindParameterOverride (x => x.cameraParam0);
            windParam0 = FindParameterOverride (x => x.windParam0);
            windParam1 = FindParameterOverride (x => x.windParam1);
            windMap = FindClassParameterOverride (x => x.ambientWindMap, sceneMisc.ambientWindMap);
            sceneColor = FindParameterOverride (x => x.sceneColor);

        }

        public override void OnInspectorGUI ()
        {
            SceneMisc sceneMisc = target as SceneMisc;
            EditorUtilities.DrawHeaderLabel ("SceneMisc");
            EditorGUI.BeginChangeCheck ();
            PropertyField (cameraParam0);
            if (EditorGUI.EndChangeCheck ())
            {
                EngineContext.instance.logicflag.SetFlag (EngineContext.Flag_CameraFovDirty, true);
            }
            PropertyField (windParam0);
            PropertyField (windParam1);
            PropertyField (windMap);
            PropertyField (sceneColor);

            // PropertyField (finalColor);
            //if (!EngineContext.IsRunning)
            //{
            //    EditorUtilities.DrawRect("Debug");
            //}
        }
        public override void OnSceneGUI ()
        {
            SceneMisc sceneMisc = target as SceneMisc;
            if (sceneMisc != null)
            {
                Color temp = Handles.color;
                Handles.color = Color.blue;
                ref Vector4 windParam = ref sceneMisc.windParam0.value;
                Quaternion rotY = Quaternion.Euler (0, windParam.x, 0);
                RuntimeUtilities.DrawScreenArrow (ref rotY, 4, -3, windParam.y, "windDir");
                Handles.color = temp;
            }
        }

    }
}
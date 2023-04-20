using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (MotionBlur))]
    public sealed class MotionBlurEditor : EnvEffectEditor<MotionBlur>
    {
        SerializedParameterOverride param;
        public override void OnEnable ()
        {
            param = FindParameterOverride (x => x.param);
        }
        public override void OnInspectorGUI ()
        {
            EditorUtilities.DrawHeaderLabel ("MotionBlur");
            PropertyField (param);
            EditorUtilities.DrawRect("Debug");
            if (GUILayout.Button(EditorUtilities.GetContent("Test"), EditorStyles.miniButton, GUILayout.Width(80)))
            {
                EnvHelp.EnableEffect(EngineContext.instance, (int)EnvSettingType.PPMotionBlur, true);
            }
            if (GUILayout.Button(EditorUtilities.GetContent("Reset"),
                EditorStyles.miniButton, GUILayout.Width(80)))
            {
                EnvHelp.EnableEffect(EngineContext.instance, (int)EnvSettingType.PPMotionBlur, false);
            }
        }
    }
}
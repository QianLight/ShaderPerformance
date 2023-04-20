using UnityEditor;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (DepthOfField))]
    public sealed class DepthOfFieldEditor : EnvEffectEditor<DepthOfField>
    {
        SerializedParameterOverride param;
        SerializedParameterOverride _QualitySet;
        public override void OnEnable ()
        {
            param = FindParameterOverride (x => x.param);
            _QualitySet = FindParameterOverride(x => x._QualitySet);
        }

        public override void OnInspectorGUI ()
        {
            PropertyField (param);
            PropertyField(_QualitySet);
            DepthOfField.showFocusPlane.Value = EditorGUILayout.Toggle("showFocusPlane", DepthOfField.showFocusPlane.Value);
        }
    }
}
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor(typeof(RadialBlur))]
    public sealed class RadialBlurEditor : EnvEffectEditor<RadialBlur>
    {
        SerializedParameterOverride param0;
        SerializedParameterOverride param1;
        SerializedParameterOverride param2;

        public override void OnEnable()
        {
            param0 = FindParameterOverride(x => x.param0);
            param1 = FindParameterOverride(x => x.param1);
            param2 = FindParameterOverride(x => x.param2);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("RadialBlur");
            PropertyField(param0);
            PropertyField(param1);
            PropertyField(param2);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);
                string text = RadialBlur.preview.Value ? "Disable Preview" : "Enable Preview";
                if (GUILayout.Button(text))
                {
                    RadialBlur.preview.Value = !RadialBlur.preview.Value;
                }
                GUILayout.Space(20);
            }
        }
    }
}
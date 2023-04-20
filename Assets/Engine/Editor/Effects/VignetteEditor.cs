using UnityEngine;
using UnityEditor;

namespace CFEngine.Editor
{
    [EnvEditor(typeof(Vignette))]
    public sealed class VignetteEditor : EnvEffectEditor<Vignette>
    {
        SerializedParameterOverride m_Color;

        SerializedParameterOverride vignetteParam0;
        SerializedParameterOverride vignetteParam1;

        public override void OnEnable()
        {
            m_Color = FindParameterOverride(x => x.color);

            vignetteParam0 = FindParameterOverride(x => x.vignetteParam0);
            vignetteParam1 = FindParameterOverride(x => x.vignetteParam1);
        }

        public override void OnInspectorGUI()
        {
            PropertyField(m_Color);

            PropertyField(vignetteParam0);
            PropertyField(vignetteParam1);
        }
    }
}

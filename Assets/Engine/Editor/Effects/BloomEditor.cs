using UnityEditor;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (Bloom))]
    public sealed class BloomEditor : EnvEffectEditor<Bloom>
    {
        SerializedParameterOverride bloomParam;
        SerializedParameterOverride _QualitySet;
        SerializedParameterOverride color;

        public override void OnEnable ()
        {
            bloomParam = FindParameterOverride (x => x.bloomParam);
            _QualitySet = FindParameterOverride (x => x._QualitySet);
            color = FindParameterOverride (x => x.color);
        }

        public override void OnInspectorGUI ()
        {
            EditorUtilities.DrawHeaderLabel ("Bloom");
            PropertyField (bloomParam);
             PropertyField (_QualitySet);
            PropertyField (color);
        }
    }
}
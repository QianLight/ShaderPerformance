using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (Ambient))]
    public sealed class AmbientEditor : EnvEffectEditor<Ambient>
    {
        ClassSerializedParameterOverride sceneSH;
        ClassSerializedParameterOverride roleSH;
        ClassSerializedParameterOverride roleSHV2;
        SerializedParameterOverride ambientParam;
        ClassSerializedParameterOverride skyBoxMat;
        ClassSerializedParameterOverride envCube;
        SerializedParameterOverride ambientParam1;
        SerializedParameterOverride ambientParam2;
        SerializedParameterOverride ambientParam3;
        public override void OnEnable ()
        {
            Ambient ambient = target as Ambient;
            sceneSH = FindClassParameterOverride (x => x.sceneSH, ambient.sceneSH);
            roleSH = FindClassParameterOverride (x => x.roleSH, ambient.roleSH);
            roleSHV2 = FindClassParameterOverride (x => x.roleSHV2, ambient.roleSHV2);
            ambientParam = FindParameterOverride (x => x.ambientParam);
            skyBoxMat = FindClassParameterOverride (x => x.skyBoxMat, ambient.skyBoxMat);
            envCube = FindClassParameterOverride (x => x.envCube, ambient.envCube);
            ambientParam1 = FindParameterOverride (x => x.ambientParam1);
            ambientParam2 = FindParameterOverride (x => x.ambientParam2);
            ambientParam3 = FindParameterOverride(x => x.ambientParam3);
        }

        public override void OnInspectorGUI ()
        {
            EditorUtilities.DrawHeaderLabel("Ambient");
            PropertyField(sceneSH);
            PropertyField(roleSH);
            PropertyField(roleSHV2);
            PropertyField(ambientParam);            
            PropertyField(skyBoxMat);
            PropertyField(envCube);
            PropertyField(ambientParam1);
            PropertyField(ambientParam2);
            PropertyField(ambientParam3);
            EditorUtilities.DrawRect("Debug");
            Ambient ambient = target as Ambient;
            if (GUILayout.Button("LoadCubeParam", GUILayout.Width(200)))
            {
                var config = AmbientModify.GetBLConfig(ambient);
                if (config != null)
                {
                    ambient.ambientParam2.value.x = config.HDR;
                    ambient.ambientParam2.value.y = config.gamma ? 1 : 0;
                    ambientParam2.value.vector4Value = ambient.ambientParam2.value;
                }
            }
            if (GUILayout.Button("SaveCubeParam", GUILayout.Width(200)))
            {
                var config = AmbientModify.GetBLConfig(ambient, true);
                config.HDR = ambient.ambientParam2.value.x;
                config.gamma = ambient.ambientParam2.value.y > 0.5f;
                CommonAssets.SaveAsset(config);
            }
        }
    }
}
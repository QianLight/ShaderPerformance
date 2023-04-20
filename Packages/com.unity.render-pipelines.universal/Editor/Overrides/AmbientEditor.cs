using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeParameterDrawer(typeof(SHParameter))]
    public sealed class SHParameterDrawer : VolumeParameterDrawer
    {
        public override bool OnGUI(SerializedDataParameter parameter, GUIContent title)
        {
            SerializedProperty value = parameter.value;
            SerializedProperty ambientMode = value.FindPropertyRelative(nameof(SHInfo.ambientMode));
            SerializedProperty flatColor = value.FindPropertyRelative(nameof(SHInfo.flatColor));
            SerializedProperty skyColor = value.FindPropertyRelative(nameof(SHInfo.skyColor));
            SerializedProperty equatorColor = value.FindPropertyRelative(nameof(SHInfo.equatorColor));
            SerializedProperty groundColor = value.FindPropertyRelative(nameof(SHInfo.groundColor));
            SerializedProperty skyCube = value.FindPropertyRelative(nameof(SHInfo.skyCube));

            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(title);
            AmbientType mode = (AmbientType) ambientMode.enumValueIndex;
            ambientMode.enumValueIndex = (int) (AmbientType) EditorGUILayout.EnumPopup(ambientMode.displayName, mode);

            // EditorGUI.BeginChangeCheck();
            switch (mode)
            {
                case AmbientType.Flat:
                    flatColor.colorValue = EditorGUILayout.ColorField(new GUIContent(flatColor.displayName),
                        flatColor.colorValue, true,
                        false, true);
                    break;
                case AmbientType.Trilight:
                    skyColor.colorValue = EditorGUILayout.ColorField(new GUIContent(flatColor.displayName),
                        flatColor.colorValue, true,
                        false, true);
                    equatorColor.colorValue = EditorGUILayout.ColorField(new GUIContent(equatorColor.displayName),
                        equatorColor.colorValue, true,
                        false, true);
                    groundColor.colorValue = EditorGUILayout.ColorField(new GUIContent(groundColor.displayName),
                        groundColor.colorValue, true,
                        false, true);
                    break;
                case AmbientType.SkyBox:
                    skyCube.objectReferenceValue = EditorGUILayout.ObjectField(skyCube.displayName,
                        skyCube.objectReferenceValue, typeof(Cubemap), false) as Cubemap;
                    break;
                default:
                    break;
            }

            // if (EditorGUI.EndChangeCheck())
            {
                SHParameter lightParameter = parameter.GetObjectRef<SHParameter>();

                if (mode != AmbientType.SkyBox || !skyCube.objectReferenceValue)
                {
                    SHInfo info = lightParameter.value;
                    EnviromentSHBakerHelper.instance.BakeSkyBoxSphericalHarmonics(ref info);
                    lightParameter.value = info;
                }
            }

            GUILayout.EndVertical();

            return true;
        }
    }

    [VolumeComponentEditor(typeof(Ambient))]
    sealed class AmbientEditor : VolumeComponentEditor
    {
        private SerializedDataParameter sceneSH;
        private SerializedDataParameter roleSH;
        private SerializedDataParameter lightmapIntensity;
        private SerializedDataParameter lightmapDefault;
        private SerializedDataParameter ambientLightScale;
        private SerializedDataParameter ambientDarkScale;
        private SerializedDataParameter envHdr;
        private SerializedDataParameter envGamma;
        private SerializedDataParameter ambietnMax;
        private SerializedDataParameter roleAmbietnMax;
        private SerializedDataParameter contrastLight;
        private SerializedDataParameter contrastIntensity;
        private SerializedDataParameter roleShadowColor;

        private static readonly SavedBool sceneFolder =
            new SavedBool($"{nameof(AmbientEditor)}.{nameof(sceneFolder)}", false);

        private static readonly SavedBool roleFolder =
            new SavedBool($"{nameof(AmbientEditor)}.{nameof(roleFolder)}", false);

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Ambient>(serializedObject);
            sceneSH = Unpack(o.Find(x => x.sceneSH));
            roleSH = Unpack(o.Find(x => x.roleSH));
            lightmapIntensity = Unpack(o.Find(x => x.lightmapIntensity));
            lightmapDefault = Unpack(o.Find(x => x.lightmapDefault));
            ambientLightScale = Unpack(o.Find(x => x.ambientLightScale));
            ambientDarkScale = Unpack(o.Find(x => x.ambientDarkScale));
            envHdr = Unpack(o.Find(x => x.envHdr));
            envGamma = Unpack(o.Find(x => x.envGamma));
            ambietnMax = Unpack(o.Find(x => x.ambietnMax));
            roleAmbietnMax = Unpack(o.Find(x => x.roleAmbietnMax));
            contrastLight = Unpack(o.Find(x => x.contrastLight));
            contrastIntensity = Unpack(o.Find(x => x.contrastIntensity));
            roleShadowColor = Unpack(o.Find(x => x.roleShadowColor));
        }

        public override void OnInspectorGUI()
        {
            if (developMode.Value)
            {
                PropertyField(sceneSH);
                PropertyField(roleSH);
                PropertyField(lightmapIntensity);
                PropertyField(lightmapDefault);
                PropertyField(ambientLightScale);
                PropertyField(ambientDarkScale);
                PropertyField(envHdr);
                PropertyField(envGamma);
                PropertyField(ambietnMax);
                PropertyField(roleAmbietnMax);
                PropertyField(contrastLight);
                PropertyField(contrastIntensity);
                PropertyField(roleShadowColor);
            }
            else
            {
                sceneFolder.value = EditorGUILayout.Foldout(sceneFolder.value, "Scene");
                if (sceneFolder.value)
                {
                }

                roleFolder.value = EditorGUILayout.Foldout(roleFolder.value, "Role");
                if (roleFolder.value)
                {
                    PropertyField(roleSH);
                    PropertyField(roleShadowColor);
                }
            }
        }
    }
}
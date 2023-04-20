using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeParameterDrawer(typeof(FogParameter))]
    sealed class FogRangeParameterDrawer : VolumeParameterDrawer
    {
        public override bool OnGUI(SerializedDataParameter parameter, GUIContent title)
        {
            SerializedProperty value = parameter.value;

            if (value.propertyType != SerializedPropertyType.Generic)
                return false;

            FogParameter o = parameter.GetObjectRef<FogParameter>();
            SerializedProperty start = value.FindPropertyRelative(nameof(FogData.start));
            SerializedProperty end = value.FindPropertyRelative(nameof(FogData.end));
            SerializedProperty intensityMin = value.FindPropertyRelative(nameof(FogData.intensityMin));
            SerializedProperty intensityMax = value.FindPropertyRelative(nameof(FogData.intensityMax));
            SerializedProperty fallOff = value.FindPropertyRelative(nameof(FogData.fallOff));

            using (new GUILayout.VerticalScope())
            {
                GUIContent label = new GUIContent(o.title + "渐变参数");
                if (parameter.overrideState.boolValue)
                {
                    string key = $"{nameof(FogEditor)}.popup.{o.title}";
                    bool fold = EditorPrefs.GetBool(key, false);
                    EditorGUI.BeginChangeCheck();
                    fold = EditorGUILayout.Foldout(fold, label);
                    if (EditorGUI.EndChangeCheck())
                        EditorPrefs.SetBool(key, fold);

                    if (fold)
                    {
                        start.floatValue = EditorGUILayout.FloatField("淡入距离", start.floatValue);
                        end.floatValue = EditorGUILayout.FloatField("淡出距离", end.floatValue);
                        intensityMin.floatValue = EditorGUILayout.Slider("最低强度", intensityMin.floatValue, 0, 1);
                        intensityMax.floatValue = EditorGUILayout.Slider("最高强度", intensityMax.floatValue, 0, 1);
                        fallOff.floatValue = Mathf.Max(0, EditorGUILayout.Slider("淡出速度", fallOff.floatValue, 0.01f, 10));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(label);
                }
            }

            return true;
        }
    }

    [VolumeComponentEditor(typeof(Fog))]
    sealed class FogEditor : VolumeComponentEditor
    {
        private SerializedDataParameter fogEnable;
        private SerializedDataParameter fogIntensity;
        private SerializedDataParameter startColor;
        private SerializedDataParameter endColor;
        // private SerializedDataParameter bottomColor;
        private SerializedDataParameter topColor;
        private SerializedDataParameter shaftOffset;

        private SerializedDataParameter baseFogEnable;
        private SerializedDataParameter baseDistance;
        private SerializedDataParameter baseHeight;

        private SerializedDataParameter noiseEnable;
        private SerializedDataParameter noise3d;
        private SerializedDataParameter noiseDistance;
        private SerializedDataParameter noiseHeight;
        private SerializedDataParameter noiseScale;
        private SerializedDataParameter noiseSpeed;
        private SerializedDataParameter noiseDensity;

        private SerializedDataParameter scatterEnable;
        // private SerializedDataParameter scatterColor;
        private SerializedDataParameter scatterScale;

        private static readonly SavedBool baseFogFoldout =
            new SavedBool($"{nameof(FogEditor)}.{nameof(baseFogFoldout)}", false);

        private static readonly SavedBool noiseFogFoldout =
            new SavedBool($"{nameof(FogEditor)}.{nameof(noiseFogFoldout)}", false);

        private static readonly SavedBool scatterFoldout =
            new SavedBool($"{nameof(FogEditor)}.{nameof(scatterFoldout)}", false);

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Fog>(serializedObject);
            fogEnable = Unpack(o.Find(x => x.fogEnable));
            fogIntensity = Unpack(o.Find(x => x.fogIntensity));
            startColor = Unpack(o.Find(x => x.startColor));
            endColor = Unpack(o.Find(x => x.endColor));
            // bottomColor = Unpack(o.Find(x => x.bottomColor));
            topColor = Unpack(o.Find(x => x.topColor));
            shaftOffset = Unpack(o.Find(x => x.shaftOffset));
            baseFogEnable = Unpack(o.Find(x => x.baseFogEnable));
            baseDistance = Unpack(o.Find(x => x.baseDistance));
            baseHeight = Unpack(o.Find(x => x.baseHeight));
            noiseEnable = Unpack(o.Find(x => x.noiseEnable));
            noise3d = Unpack(o.Find(x => x.noise3d));
            noiseDistance = Unpack(o.Find(x => x.noiseDistance));
            noiseHeight = Unpack(o.Find(x => x.noiseHeight));
            noiseScale = Unpack(o.Find(x => x.noiseScale));
            noiseSpeed = Unpack(o.Find(x => x.noiseSpeed));
            noiseDensity = Unpack(o.Find(x => x.noiseDensity));
            scatterEnable = Unpack(o.Find(x => x.scatterEnable));
            // scatterColor = Unpack(o.Find(x => x.scatterColor));
            scatterScale = Unpack(o.Find(x => x.scatterScale));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Fog", EditorStyles.miniLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                PropertyField(fogEnable, new GUIContent("启用雾效"));
                PropertyField(fogIntensity, new GUIContent("雾效强度"));
                PropertyField(startColor, new GUIContent("近处颜色"));
                PropertyField(endColor, new GUIContent("远处颜色"));
                EditorGUILayout.Space();
                // PropertyField(bottomColor, new GUIContent("底部雾效"));
                PropertyField(topColor, new GUIContent("阳光透射"));
                PropertyField(shaftOffset, new GUIContent("透射衰减速度"));

                using (new GUILayout.VerticalScope("box"))
                {
                    baseFogFoldout.value = EditorGUILayout.Foldout(baseFogFoldout.value, "基础雾效");
                    if (baseFogFoldout.value)
                    {
                        PropertyField(baseFogEnable, new GUIContent("启用基本雾效"));
                        PropertyField(baseDistance);
                        PropertyField(baseHeight);
                    }
                }

                using (new GUILayout.VerticalScope("box"))
                {
                    noiseFogFoldout.value = EditorGUILayout.Foldout(noiseFogFoldout.value, "噪声雾效");
                    if (noiseFogFoldout.value)
                    {
                        PropertyField(noiseEnable, new GUIContent("启用噪声雾效"));
                        if (noiseEnable.value.boolValue)
                        {
                            PropertyField(noise3d, new GUIContent("噪声贴图"));
                            PropertyField(noiseDensity, new GUIContent("噪声密度"));
                            PropertyField(noiseScale, new GUIContent("噪声平铺"));
                            PropertyField(noiseSpeed, new GUIContent("噪声速度"));
                            PropertyField(noiseDistance);
                            PropertyField(noiseHeight);
                        }
                    }
                }

                using (new GUILayout.VerticalScope("box"))
                {
                    scatterFoldout.value = EditorGUILayout.Foldout(scatterFoldout.value, "大气散射");
                    if (scatterFoldout.value)
                    {
                        PropertyField(scatterEnable, new GUIContent("启用大气散射"));
                        if (scatterEnable.value.boolValue)
                        {
                            // PropertyField(scatterColor, new GUIContent("大气散射颜色"));
                            PropertyField(scatterScale, new GUIContent("大气散射强度"));
                        }
                    }
                }
            }
        }
    }
}
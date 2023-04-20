using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeParameterDrawer(typeof(LightParameter))]
    public sealed class LightParameterDrawer : VolumeParameterDrawer
    {
        public override bool OnGUI(SerializedDataParameter parameter, GUIContent title)
        {
            LightParameter lightParameter = parameter.GetObjectRef<LightParameter>();

            GUILayout.BeginVertical();

            if (VolumeEditor.currentProfile)
            {
                GUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                Light oldLight = lightParameter.light;
                Light newLight =
                    EditorGUILayout.ObjectField(parameter.displayName, oldLight, typeof(Light), true) as Light;
                LightParameterEntity entity;
                if (EditorGUI.EndChangeCheck() && newLight != oldLight)
                {
                    if (oldLight && oldLight.TryGetComponent(out entity))
                    {
                        Object.DestroyImmediate(entity);
                    }

                    lightParameter.light = newLight;
                }

                if (lightParameter.light && !lightParameter.light.TryGetComponent(out entity))
                {
                    entity = lightParameter.light.gameObject.AddComponent<LightParameterEntity>();
                    entity.profile = VolumeEditor.currentProfile;
                    entity.type = lightParameter.type;
                }

                if (lightParameter.light || FindLight(lightParameter))
                {
                }
                else if (GUILayout.Button("New", GUILayout.Width(40)))
                {
                    string name = lightParameter.type.ToString();
                    GameObject lgo = new GameObject(name, typeof(Light), typeof(LightParameterEntity));
                    LightParameterEntity e = lgo.GetComponent<LightParameterEntity>();
                    e.profile = VolumeEditor.currentProfile;
                    e.type = lightParameter.type;
                    var light = lgo.GetComponent<Light>();
                    light.type = LightType.Directional;
                    lightParameter.light = light;
                    lgo.transform.SetParent(VolumeEditor.currentVolume.transform, false);
                }

                if (!Application.isPlaying && !lightParameter.light)
                {
                    lightParameter.value = LightInfo.CreateDefault();
                }

                GUILayout.EndHorizontal();
            }

            if (!VolumeEditor.currentProfile || VolumeComponentEditor.developMode.Value)
            {
                SerializedProperty value = parameter.value;
                SerializedProperty color = value.FindPropertyRelative(nameof(LightInfo.color));
                // SerializedProperty horizontal = value.FindPropertyRelative(nameof(LightInfo.horizontal));
                // SerializedProperty vertical = value.FindPropertyRelative(nameof(LightInfo.vertical));

                EditorGUILayout.PropertyField(color);
                // EditorGUILayout.PropertyField(horizontal);
                // EditorGUILayout.PropertyField(vertical);
            }

            GUILayout.EndVertical();

            return true;
        }

        private bool FindLight(LightParameter parameter)
        {
            LightParameterEntity[] entities = Object.FindObjectsOfType<LightParameterEntity>();
            foreach (LightParameterEntity entity in entities)
            {
                if (entity.profile == VolumeEditor.currentProfile && entity.type == parameter.type)
                {
                    parameter.light = entity.GetComponent<Light>();
                    return true;
                }
            }

            return false;
        }
    }

    [VolumeComponentEditor(typeof(Lighting))]
    sealed class LightingEditor : VolumeComponentEditor
    {
        private SerializedDataParameter mainLight;
        private SerializedDataParameter addLight;
        private SerializedDataParameter roleLightColor;
        private SerializedDataParameter minRoleHAngle;
        private SerializedDataParameter maxRoleHAngle;
        private SerializedDataParameter roleFaceHAngle;
        private SerializedDataParameter addRoleLight;
        private SerializedDataParameter roleLightRotOffset;
        private SerializedDataParameter leftRightControl;
        private SerializedDataParameter upDownControl;
        private SerializedDataParameter waterLight;
        private SerializedDataParameter shadowRimIntensity;
        private SerializedDataParameter planarShadow;
        private SerializedDataParameter planarShadowColor;
        private SerializedDataParameter planarShadowFalloff;
        private SerializedDataParameter planarShadowDir;
        private SerializedDataParameter selfShadow;
        private SerializedDataParameter Roleshadow_MaxDistance;
        private SerializedDataParameter Roleshadow_NormalBias;
        private SerializedDataParameter roleHeightSHAlpha;
        private SerializedDataParameter roleHeightSHFadeout;
        private SerializedDataParameter customFaceShadowDir;
        private SerializedDataParameter faceShadowDir;
        private SerializedDataParameter windDirection;
        private SerializedDataParameter windFrenquency;
        private SerializedDataParameter windSpeed;
        private SerializedDataParameter cameraSpaceLighting;
        private SerializedDataParameter usingDefaultLight;

        private static readonly SavedBool sceneFolder =
            new SavedBool($"{nameof(LightingEditor)}.{nameof(sceneFolder)}", false);

        private static readonly SavedBool roleFolder =
            new SavedBool($"{nameof(LightingEditor)}.{nameof(roleFolder)}", false);
        private static readonly SavedBool roleselfshadow =
            new SavedBool($"{nameof(LightingEditor)}.{nameof(roleselfshadow)}", false);

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Lighting>(serializedObject);
            mainLight = Unpack(o.Find(x => x.mainLight));
            addLight = Unpack(o.Find(x => x.addLight));
            roleLightColor = Unpack(o.Find(x => x.roleLightColor));
            minRoleHAngle = Unpack(o.Find(x => x.minRoleHAngle));
            maxRoleHAngle = Unpack(o.Find(x => x.maxRoleHAngle));
            roleFaceHAngle = Unpack(o.Find(x => x.roleFaceHAngle));
            addRoleLight = Unpack(o.Find(x => x.addRoleLight));
            roleLightRotOffset = Unpack(o.Find(x => x.roleLightRotOffset));
            leftRightControl = Unpack(o.Find(x => x.leftRightControl));
            upDownControl = Unpack(o.Find(x => x.upDownControl));
            waterLight = Unpack(o.Find(x => x.waterLight));
            shadowRimIntensity = Unpack(o.Find(x => x.shadowRimIntensity));
            planarShadow = Unpack(o.Find(x => x.planarShadow));
            planarShadowColor = Unpack(o.Find(x => x.planarShadowColor));
            planarShadowFalloff = Unpack(o.Find(x => x.planarShadowFalloff));
            planarShadowDir = Unpack(o.Find(x => x.planarShadowDir));
            selfShadow = Unpack(o.Find(x => x.selfShadow));
            windDirection = Unpack(o.Find(x => x.windDirection));
            windFrenquency = Unpack(o.Find(x => x.windFrenquency));
            windSpeed = Unpack(o.Find(x => x.windSpeed));
            cameraSpaceLighting = Unpack(o.Find(x => x.cameraSpaceLighting));
            usingDefaultLight = Unpack(o.Find(x => x.usingDefaultLight));

            Roleshadow_MaxDistance = Unpack(o.Find(x => x.Roleshadow_MaxDistance));
            Roleshadow_NormalBias = Unpack(o.Find(x => x.Roleshadow_NormalBias));
            roleHeightSHAlpha = Unpack(o.Find(x => x.roleHeightSHAlpha));
            roleHeightSHFadeout = Unpack(o.Find(x => x.roleHeightSHFadeout));
            customFaceShadowDir = Unpack(o.Find(x => x.customFaceShadowDir));
            faceShadowDir = Unpack(o.Find(x => x.faceShadowDir));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Lighting", EditorStyles.miniLabel);
            if (developMode.Value)
            {
                PropertyField(mainLight);
                PropertyField(addLight);
                PropertyField(roleLightColor);
                PropertyField(minRoleHAngle);
                PropertyField(maxRoleHAngle);
                PropertyField(roleFaceHAngle);
                PropertyField(addRoleLight);
                PropertyField(roleLightRotOffset);
                PropertyField(leftRightControl);
                PropertyField(upDownControl);
                PropertyField(waterLight);
                PropertyField(shadowRimIntensity);
                PropertyField(planarShadow);
                PropertyField(planarShadowColor);
                PropertyField(planarShadowFalloff);
                PropertyField(planarShadowDir);
                PropertyField(selfShadow);
                PropertyField(Roleshadow_MaxDistance);
                PropertyField(Roleshadow_NormalBias);
                PropertyField(roleHeightSHAlpha);
                PropertyField(roleHeightSHFadeout);
                PropertyField(customFaceShadowDir);
                PropertyField(faceShadowDir);

                if (GUILayout.Button("刷新所有HeightGradient参数"))
                {
                    string[] guids = AssetDatabase.FindAssets("t:UnityEngine.Rendering.VolumeProfile");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
                        if (profile.TryGet(out Lighting lighting))
                        {
                            lighting.roleHeightSHAlpha.value = 0.5f;
                            lighting.roleHeightSHFadeout.value = 1.5f;
                            lighting.roleHeightSHFadeout.overrideState = false;
                            lighting.roleHeightSHFadeout.overrideState = false;
                            EditorUtility.SetDirty(profile);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("刷默认值 (2021年8月31日)"))
                {
                    string[] guids = AssetDatabase.FindAssets("t:UnityEngine.Rendering.VolumeProfile");
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
                        if (profile.TryGet(out Lighting lighting))
                        {
                            lighting.minRoleHAngle.value = -20f;
                            lighting.maxRoleHAngle.value = 10f;
                            lighting.shadowRimIntensity.value = 0.3f;
                            lighting.minRoleHAngle.overrideState = false;
                            lighting.maxRoleHAngle.overrideState = false;
                            lighting.shadowRimIntensity.overrideState = false;
                            EditorUtility.SetDirty(profile);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                roleFolder.value = EditorGUILayout.Foldout(roleFolder.value, "Role");
                if (roleFolder.value)
                {
                    PropertyField(cameraSpaceLighting);
                    PropertyField(usingDefaultLight);
                    PropertyField(roleLightColor);
                    PropertyField(minRoleHAngle);
                    PropertyField(maxRoleHAngle);
                    PropertyField(roleFaceHAngle);
                    // TODO: Confirm if we need role add lighting.
                    // PropertyField(addRoleLight);
                    PropertyField(roleLightRotOffset);
                    // TODO: Depth face shadow.
                    // PropertyField(leftRightControl);
                    // PropertyField(upDownControl);
                    PropertyField(shadowRimIntensity);
                    
                    PropertyField(roleHeightSHAlpha);
                    PropertyField(roleHeightSHFadeout);

                    PropertyField(customFaceShadowDir);
                    if (customFaceShadowDir.value.boolValue)
                    {
                        PropertyField(faceShadowDir);
                    }
                    roleselfshadow.value = EditorGUILayout.Foldout(roleselfshadow.value, "RoleShadow");
                    if (roleselfshadow.value)
                    {
                        PropertyField(selfShadow);
                        PropertyField(Roleshadow_MaxDistance);
                        PropertyField(Roleshadow_NormalBias);
                    }
                    EditorGUILayout.Space(15);
                    // Planar Shadow
                    EditorGUILayout.BeginVertical("box");
                    PropertyField(planarShadow);
                    EditorGUI.indentLevel++;
                    if (planarShadow.value.boolValue)
                    {
                        PropertyField(planarShadowColor);
                        PropertyField(planarShadowFalloff);
                        PropertyField(planarShadowDir);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }

                sceneFolder.value = EditorGUILayout.Foldout(sceneFolder.value, "Scene");
                if (sceneFolder.value)
                {
                    PropertyField(mainLight);
                    // PropertyField(addLight);
                    PropertyField(waterLight);

                    PropertyField(windDirection);
                    PropertyField(windFrenquency);
                    PropertyField(windSpeed);
                }
            }
        }
    }
}
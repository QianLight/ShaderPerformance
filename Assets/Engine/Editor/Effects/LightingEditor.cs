using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor(typeof(Lighting))]
    public sealed class LightingEditor : EnvEffectEditor<Lighting>
    {
        ClassSerializedParameterOverride mainLightInfo;
        ClassSerializedParameterOverride addLightInfo;
        SerializedParameterOverride roleLightColor;
        SerializedParameterOverride roleLightColorV2;
        SerializedParameterOverride lightParam;
        SerializedParameterOverride lightParam1;
        SerializedParameterOverride roleLightDirParam;
        SerializedParameterOverride rimLightColor;

        ClassSerializedParameterOverride waterLightInfo;
        SavedBool debugDynamicLight;

        static Material matDrawLightIndex;
        public override void OnEnable()
        {
            Lighting lighting = target as Lighting;
            mainLightInfo = FindClassParameterOverride(x => x.mainLightInfo, lighting.mainLightInfo);
            addLightInfo = FindClassParameterOverride(x => x.addLightInfo, lighting.addLightInfo);
            roleLightColor = FindParameterOverride(x => x.roleLightColor);
            roleLightColorV2 = FindParameterOverride(x => x.roleLightColorV2);
            lightParam = FindParameterOverride(x => x.lightParam);
            lightParam1 = FindParameterOverride(x => x.lightParam1);
            roleLightDirParam = FindParameterOverride(x => x.roleLightDirParam);
            rimLightColor = FindParameterOverride(x => x.rimLightColor);
            waterLightInfo = FindClassParameterOverride(x => x.waterLightInfo, lighting.waterLightInfo);
            Lighting.drawMainLightHandlerMode = Lighting.DrawMainLightHandlerMode.LegacyRole;
            debugDynamicLight = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(debugDynamicLight)}", false);



        }
        public override void OnDisable()
        {
            Lighting.drawMainLightHandlerMode = Lighting.DrawMainLightHandlerMode.Disable;
        }
        public override void OnInspectorGUI()
        {
            Lighting lighting = target as Lighting;
            EditorUtilities.DrawHeaderLabel("Lighting");
            PropertyField(mainLightInfo);
            PropertyField(addLightInfo);
            PropertyField(roleLightColor);
            PropertyField(roleLightColorV2);
            PropertyField(lightParam);
            PropertyField(lightParam1);
            PropertyField(roleLightDirParam);
            PropertyField(rimLightColor);
            PropertyField(waterLightInfo);

            EditorUtilities.DrawRect("Debug");
            if(LightingModify.showLightGizmos!=null)
                LightingModify.showLightGizmos.Value = EditorGUILayout.Toggle("showLightGizmos", LightingModify.showLightGizmos.Value);
            if(GUILayout.Button("PrepareLight",GUILayout.MaxWidth(120)))
            {
                lighting.CreateLights(true);
            }
            if (EngineContext.IsRunning)
                LightingModify.forceUpdateVoxelLight = EditorGUILayout.Toggle("forceUpdateVoxelLight", LightingModify.forceUpdateVoxelLight);
            debugDynamicLight.Value = EditorGUILayout.Toggle("debugDynamicLight", debugDynamicLight.Value);
            if (debugDynamicLight.Value)
            {
                var context = EngineContext.instance;
                ref var lc = ref context.staticLightContext;
                if (lc.rt != null)
                    RuntimeUtilities.DrawInstpectorTex(
                       lc.rt,
                        AssetsConfig.instance.LightIndexPreview,
                        ref matDrawLightIndex);
            }
            if (GUILayout.Button("ClearDynamicLight"))
            {
                // if (EngineContext.instance != null)
                //     VoxelLightingSystem.Reset(EngineContext.instance);
            }

            LightingModify.updateFps.Value = EditorGUILayout.Slider("动态点光源更新频率", LightingModify.updateFps.Value, 1, 60);
        }

        public override void OnSceneGUI()
        {
            Lighting lighting = target as Lighting;
            if (lighting != null)
            {
                EngineContext context = EngineContext.instance;
                Color temp = Handles.color;
                if (SceneView.lastActiveSceneView != null &&
                    SceneView.lastActiveSceneView.camera != null &&
                    LightingModify.showLightGizmos != null &&
                    LightingModify.showLightGizmos.Value &&
                    context != null)
                {

                    Transform t = SceneView.lastActiveSceneView.camera.transform;
                    Vector3 pos = t.position + t.forward * 10;
                    if (Lighting.drawMainLightHandlerMode == Lighting.DrawMainLightHandlerMode.LegacyRole)
                    {
                        Color roleColor = lighting.roleLightColor;
                        var lightDir = -EngineContext.roleLightDir;
                        RuntimeUtilities.DrawLightHandle(t, ref pos, ref lightDir, ref roleColor, -4, 3, lighting.mainLightInfo, "MainLight");
                    }
                    else if (Lighting.drawMainLightHandlerMode == Lighting.DrawMainLightHandlerMode.RoleRenderV2)
                    {
                        Color roleColor = lighting.roleLightColorV2;
                        var lightDir = -EngineContext.roleLightDir;
                        RuntimeUtilities.DrawLightHandle(t, ref pos, ref lightDir, ref roleColor, -4, 3, lighting.mainLightInfo, "MainLight");
                    }

                    RuntimeUtilities.DrawLightHandle(t, ref pos, -4, -3, lighting.addLightInfo, "AddLight");

                    RuntimeUtilities.DrawLightHandle(t, ref pos, 4, 3, lighting.waterLightInfo, "WaterLight");
                }

                Handles.color = temp;
            }

        }
    }
}
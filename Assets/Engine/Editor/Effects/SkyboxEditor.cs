using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor(typeof(Skybox))]
    public class SkyboxEditor : EnvEffectEditor<Skybox>
    {
        private ClassSerializedParameterOverride skyBoxMat;
        private SerializedParameterOverride scatterTex;
        private SerializedParameterOverride scatterParams;
        private SerializedParameterOverride skyboxParams0;
        private SerializedParameterOverride skyboxParams1;
        private SerializedParameterOverride skyboxParams2;
        private SerializedParameterOverride sssColor;
        private SerializedParameterOverride tintColor;
        private SerializedParameterOverride sunColor;

        private static readonly SavedBool ScatterFolder = new SavedBool("SkyboxEditor.ScatterFolder");
        private static readonly SavedBool OthersFolder = new SavedBool("SkyboxEditor.Others");

        public override void OnEnable()
        {
            Skybox skybox = target as Skybox;
            skyBoxMat = FindClassParameterOverride(x => x.skyBoxMat, skybox.skyBoxMat);
            scatterTex = FindClassParameterOverride(x => x.scatterTex, skybox.scatterTex);
            scatterParams = FindParameterOverride(x => x.scatterParams);
            skyboxParams0 = FindParameterOverride(x => x.skyboxParams0);
            skyboxParams1 = FindParameterOverride(x => x.skyboxParams1);
            skyboxParams2 = FindParameterOverride(x => x.skyboxParams2);
            sssColor = FindParameterOverride(x => x.sssColor);
            tintColor = FindParameterOverride(x => x.tintColor);
            sunColor = FindParameterOverride(x => x.sunColor);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("Skybox");
            PropertyField(skyBoxMat);

            if (IsScatteredSkybox())
            {
                DrawScattedSkyboxParams();
            }
            else if (IsStaticSkybox())
            {
                PropertyField(sunColor);
                PropertyField(skyboxParams0);
                PropertyField(skyboxParams1);
                PropertyField(skyboxParams2);
                PropertyField(sssColor);
                PropertyField(tintColor);
            }
        }

        private bool IsStaticSkybox()
        {
            return IsSkybox("Custom/Scene/SkyboxStatic");
        }

        private bool IsScatteredSkybox()
        {
            return IsSkybox("Custom/Scene/ScatteredSkybox");
        }

        private bool IsSkybox(string shaderName)
        {
            ResParam skyboxResParam = skyBoxMat.param as ResParam;
            Material material = skyboxResParam.asset as Material;
            bool isScatteredSkybox = material && material.shader && material.shader.name == shaderName;
            return isScatteredSkybox;
        }

        private void DrawScattedSkyboxParams()
        {
            GUILayout.Space(10);
            ScatterFolder.Value = EditorGUILayout.Foldout(ScatterFolder.Value, "Scatter");
            if (ScatterFolder.Value)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    using (new EditorGUI.IndentLevelScope())
                        GUILayout.Label("无法编辑：在点击[Save]按钮时自动填值。");
                    PropertyField(scatterTex);
                    PropertyField(scatterParams);
                }
            }

            OthersFolder.Value = EditorGUILayout.Foldout(OthersFolder.Value, "Others");
            if (OthersFolder.Value)
            {
                PropertyField(skyboxParams0);
            }
        }
    }
}

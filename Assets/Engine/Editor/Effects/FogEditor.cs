using System;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor(typeof(Fog))]
    public sealed class FogEditor : EnvEffectEditor<Fog>
    {
        private SerializedParameterOverride noiseScaleOffset;
        private SerializedParameterOverride noiseParams;
        private SerializedParameterOverride startColor;
        private SerializedParameterOverride endColor;
        private SerializedParameterOverride scatterParams;
        private SerializedParameterOverride scatterColor;

        private SerializedParameterOverride baseDistance;
        private SerializedParameterOverride baseHeight;
        private SerializedParameterOverride noiseDistance;
        private SerializedParameterOverride noiseHeight;
        private SerializedParameterOverride _QualitySet;

        public override void OnEnable()
        {
            Fog fog = target as Fog;
            baseDistance = FindClassParameterOverride(x => x.baseDistance, fog.baseDistance);
            baseHeight = FindClassParameterOverride(x => x.baseHeight, fog.baseHeight);
            noiseDistance = FindClassParameterOverride(x => x.noiseDistance, fog.noiseDistance);
            noiseHeight = FindClassParameterOverride(x => x.noiseHeight, fog.noiseHeight);

            scatterParams = FindParameterOverride(x => x.scatterParams);
            scatterColor = FindParameterOverride(x => x.scatterColor);
            noiseScaleOffset = FindParameterOverride(x => x.noiseScaleOffset);
            noiseParams = FindParameterOverride(x => x.noiseParams);
            startColor = FindParameterOverride(x => x.startColor);
            endColor = FindParameterOverride(x => x.endColor);
            _QualitySet = FindParameterOverride(x => x._QualitySet);
        }

        public override void OnInspectorGUI()
        {
            EditorUtilities.DrawHeaderLabel("Fog");

            serializedObject.Update();
            PropertyField(_QualitySet);
            Group("Color", DrawColors);
            Group("Scatter", DrawScatter);
            Group("Base", DrawBase);
            Group("Noise", DrawFogNoiseParams);
        
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBase()
        {
            PropertyField(baseDistance);
            GUILayout.Space(8);
            PropertyField(baseHeight);
            GUILayout.Space(8);
        }

        private void DrawScatter()
        {
            PropertyField(scatterParams);
            PropertyField(scatterColor);
        }

        private void DrawColors()
        {

            PropertyField(startColor);
            PropertyField(endColor);
        }

        private void DrawFogNoiseParams()
        {
            PropertyField(noiseParams);
            PropertyField(noiseScaleOffset);
            PropertyField(noiseDistance);
            GUILayout.Space(8);
            PropertyField(noiseHeight);
            GUILayout.Space(8);
        }

        private void Group(string name, Action gui)
        {
            GUILayout.Space(4);
            GUILayout.Box(GUIContent.none, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUILayout.Label(name);
            gui?.Invoke();
        }
    }
}
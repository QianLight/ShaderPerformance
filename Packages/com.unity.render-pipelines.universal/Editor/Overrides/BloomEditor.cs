using System.Linq;
using System.Runtime.Remoting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(Bloom))]
    sealed class BloomEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_Mode;
        SerializedDataParameter m_Threshold;
        SerializedDataParameter m_Intensity;
        SerializedDataParameter m_Scatter;
        SerializedDataParameter m_Clamp;
        SerializedDataParameter m_Tint;
        SerializedDataParameter m_HighQualityFiltering;
        SerializedDataParameter m_SkipIterations;
        SerializedDataParameter m_DirtTexture;
        SerializedDataParameter m_DirtIntensity;
        SerializedDataParameter m_DirectThreshold;
        SerializedDataParameter m_Balance;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Bloom>(serializedObject);

            m_DirectThreshold = Unpack(o.Find(x => x.directThreshold));
            m_Mode = Unpack(o.Find(x => x.mode));
            m_Threshold = Unpack(o.Find(x => x.threshold));
            m_Intensity = Unpack(o.Find(x => x.intensity));
            m_Scatter = Unpack(o.Find(x => x.scatter));
            m_Clamp = Unpack(o.Find(x => x.clamp));
            m_Tint = Unpack(o.Find(x => x.tint));
            m_HighQualityFiltering = Unpack(o.Find(x => x.highQualityFiltering));
            m_SkipIterations = Unpack(o.Find(x => x.skipIterations));
            m_DirtTexture = Unpack(o.Find(x => x.dirtTexture));
            m_DirtIntensity = Unpack(o.Find(x => x.dirtIntensity));
            m_Balance = Unpack(o.Find(x => x.balance));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Bloom", EditorStyles.miniLabel);

            PropertyField(m_Mode);
            var mode = (BloomMode) m_Mode.value.enumValueIndex; 
            if (mode == BloomMode.Default || mode == BloomMode.Minus)
            {
                PropertyField(m_Threshold);
                PropertyField(m_Intensity);
                PropertyField(m_Scatter);
                PropertyField(m_Tint);
                PropertyField(m_Clamp);
                PropertyField(m_HighQualityFiltering);

                if (m_HighQualityFiltering.overrideState.boolValue && m_HighQualityFiltering.value.boolValue && CoreEditorUtils.buildTargets.Contains(GraphicsDeviceType.OpenGLES2))
                    EditorGUILayout.HelpBox("High Quality Bloom isn't supported on GLES2 platforms.", MessageType.Warning);

                PropertyField(m_SkipIterations);

                EditorGUILayout.LabelField("Lens Dirt", EditorStyles.miniLabel);

                PropertyField(m_DirtTexture);
                PropertyField(m_DirtIntensity);
            }
            else if (mode == BloomMode.Legacy)
            {
                PropertyField(m_DirectThreshold);
                PropertyField(m_Intensity);
                PropertyField(m_Balance);
            }
        }
    }
}

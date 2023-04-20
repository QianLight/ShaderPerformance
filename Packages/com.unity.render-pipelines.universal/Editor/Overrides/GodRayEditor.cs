using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(GodRay))]
    sealed class GodRayEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_color;
        SerializedDataParameter m_Intensity;
        SerializedDataParameter m_DownSample;
        SerializedDataParameter m_LightDir;
        SerializedDataParameter m_LightPosition;
        SerializedDataParameter m_Power;
        SerializedDataParameter m_MaxPower;
        SerializedDataParameter m_Radius;
        SerializedDataParameter m_Offset;
        SerializedDataParameter m_BlurTimes;
        SerializedDataParameter m_Threshold;
        SerializedDataParameter m_LinearDistance;
        SerializedDataParameter m_Bias;
        SerializedDataParameter m_UseNoise;
        UnityEngine.Transform lightTransform;
        public override void OnEnable()
        {
            var o = new PropertyFetcher<GodRay>(serializedObject);

            StringParameter strPara = ((GodRay)serializedObject.targetObject).LightTransform;
            UnityEngine.Transform obj = strPara.GetObjectByName();
            if (obj != null)
            {
                lightTransform = obj;
            }

            m_Intensity = Unpack(o.Find(x => x.Intensity));
            m_color = Unpack(o.Find(x => x.color));
            m_LightDir = Unpack(o.Find(x => x.LightDir));
            m_LightPosition = Unpack(o.Find(x => x.LightPosition));
            m_Power = Unpack(o.Find(x => x.Power));
            m_MaxPower = Unpack(o.Find(x => x.MaxPower));
            m_Radius = Unpack(o.Find(x => x.Radius));
            m_Offset = Unpack(o.Find(x => x.Offset));
            m_Threshold = Unpack(o.Find(x => x.Threshold));
            m_LinearDistance = Unpack(o.Find(x => x.LinearDistance));
            m_Bias = Unpack(o.Find(x => x.Bias));
            m_DownSample = Unpack(o.Find(x => x.DownSample));
            m_BlurTimes = Unpack(o.Find(x => x.BlurTimes));
            m_UseNoise = Unpack(o.Find(x => x.UseNoise));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Use Scatter Light Dir", EditorStyles.miniLabel);
            lightTransform = EditorGUILayout.ObjectField(lightTransform, typeof(UnityEngine.Transform), true) as UnityEngine.Transform;
            if (lightTransform != null)
            {
                GodRayLight lightView = lightTransform.GetComponent<GodRayLight>();
                if(lightView == null)
                {
                    lightTransform.gameObject.AddComponent<GodRayLight>();
                }
                m_LightDir.value.vector3Value = lightTransform.forward;
                m_LightPosition.value.vector3Value = lightTransform.position;

                StringParameter strPara = ((GodRay)serializedObject.targetObject).LightTransform;
                strPara.value = strPara.GetRootName(lightTransform);
            }
            PropertyField(m_LightDir);
            PropertyField(m_LightPosition);
            PropertyField(m_color);
            PropertyField(m_Threshold);
            PropertyField(m_LinearDistance);
            PropertyField(m_Intensity);
            PropertyField(m_Bias);
            PropertyField(m_DownSample);
            PropertyField(m_Power);
            PropertyField(m_MaxPower);
            PropertyField(m_Radius);
            PropertyField(m_Offset);
            PropertyField(m_BlurTimes);
            PropertyField(m_UseNoise);
        }
        public override void OnDisable()
        {
            
        }
    }
}

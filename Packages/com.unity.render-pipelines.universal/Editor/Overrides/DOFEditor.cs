using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering.Universal;

namespace Overrides
{
    [VolumeComponentEditor(typeof(DOF))]
    public sealed class DOFEditor : VolumeComponentEditor
    {
        private SerializedDataParameter m_EasyMode;
        private SerializedDataParameter m_FocusDistance;
        private SerializedDataParameter m_BokehRangeNear;
        private SerializedDataParameter m_FocusRangeFar;
        private SerializedDataParameter m_BlurRadius;
        private SerializedDataParameter m_Intensity;

        private DOF _target;
        public override void OnEnable()
        {
            // base.OnEnable();
            _target = target as DOF;
            var o = new PropertyFetcher<DOF>(serializedObject);

            m_EasyMode = Unpack(o.Find(x => x.EasyMode));
            m_FocusDistance = Unpack(o.Find(x => x.FocusDistance));
            m_BokehRangeNear = Unpack(o.Find(x => x.BokehRangeNear));
            m_FocusRangeFar = Unpack(o.Find(x => x.FocusRangeFar));
            m_BlurRadius = Unpack(o.Find(x => x.BlurRadius));
            m_Intensity = Unpack(o.Find(x => x.Intensity));
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            serializedObject.UpdateIfRequiredOrScript();
            PropertyField(m_EasyMode);
            PropertyField(m_FocusDistance);
            PropertyField(m_BokehRangeNear);
            PropertyField(m_FocusRangeFar);
            PropertyField(m_BlurRadius);
            PropertyField(m_Intensity);
            _target.Update();
        }
        
    }
}
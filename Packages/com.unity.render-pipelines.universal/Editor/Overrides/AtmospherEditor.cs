using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(Atmospher))]
    sealed class AtmospherEditor : VolumeComponentEditor
    {
        private SerializedDataParameter fogScatteringScale;
        private SerializedDataParameter molecularDensity;
        private SerializedDataParameter wavelengthR;
        private SerializedDataParameter wavelengthG;
        private SerializedDataParameter wavelengthB;
        private SerializedDataParameter rayleigh;
        private SerializedDataParameter mie;
        private SerializedDataParameter sescattering;
        private SerializedDataParameter exposure;
        private SerializedDataParameter rayleighColor;
        private SerializedDataParameter mieColor;
        private SerializedDataParameter scatteringTexWidth;
        private SerializedDataParameter fogScatterColor;

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Atmospher>(serializedObject);
            fogScatterColor = Unpack(o.Find(x => x.fogScatterColor));
            fogScatteringScale = Unpack(o.Find(x => x.fogScatteringScale));
            molecularDensity = Unpack(o.Find(x => x.molecularDensity));
            wavelengthR = Unpack(o.Find(x => x.wavelengthR));
            wavelengthG = Unpack(o.Find(x => x.wavelengthG));
            wavelengthB = Unpack(o.Find(x => x.wavelengthB));
            rayleigh = Unpack(o.Find(x => x.rayleigh));
            mie = Unpack(o.Find(x => x.mie));
            sescattering = Unpack(o.Find(x => x.scattering));
            exposure = Unpack(o.Find(x => x.exposure));
            rayleighColor = Unpack(o.Find(x => x.rayleighColor));
            mieColor = Unpack(o.Find(x => x.mieColor));
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("大气散射", EditorStyles.miniLabel);

            EditorGUI.BeginChangeCheck();
            PropertyField(fogScatterColor, new GUIContent("雾效散射颜色"));
            PropertyField(exposure, new GUIContent("曝光"));
            PropertyField(sescattering, new GUIContent("散射强度"));
            PropertyField(rayleigh, new GUIContent("瑞利（天空）散射", "空气衰减,值越大则天空越接近指定颜色(值过大会覆盖天空原有颜色)"));
            PropertyField(rayleighColor, new GUIContent("瑞利散射颜色", "一般趋向于天空本身的颜色"));
            PropertyField(mie, new GUIContent("米氏（云雾穿透）散射","同时影响雾效，值越大则视角中接近太阳处（包括天空中）越接近指定颜色"));
            PropertyField(mieColor, new GUIContent("米氏散射颜色", "一般趋向于太阳发出的颜色"));
            PropertyField(fogScatteringScale, new GUIContent("雾效散射系数"));
            
            EditorGUILayout.LabelField("高级设置");
            PropertyField(molecularDensity, new GUIContent("分子密度"));
            PropertyField(wavelengthR, new GUIContent("红色波长"));
            PropertyField(wavelengthG,new GUIContent("绿色波长"));
            PropertyField(wavelengthB,new GUIContent("蓝色波长"));

            if (EditorGUI.EndChangeCheck())
            {
                (target as Atmospher).Apply();
            }
        }
    }
}
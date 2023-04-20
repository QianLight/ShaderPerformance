using UnityEngine;
using UnityEngine.Rendering.Universal;
using Skybox = UnityEngine.Rendering.Universal.Skybox;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(Skybox))]
    sealed class SkyboxEditor : VolumeComponentEditor
    {
        private SerializedDataParameter mode;
        private SerializedDataParameter customSkybox;

        private SerializedDataParameter customBackground;
        private SerializedDataParameter cloudDarkColor;        
        private SerializedDataParameter cloudLightColor;
        private SerializedDataParameter baseTex;
        private SerializedDataParameter maskTexture;
        ////闪电
        //private SerializedDataParameter lightingRotX;
        //private SerializedDataParameter lightingRotY;
        //private SerializedDataParameter lightingPTSize;
        //private SerializedDataParameter flashFrequency;
        //private SerializedDataParameter lightingColor;

        private SerializedDataParameter borderColor;

        private SerializedDataParameter borderEmissionRange;

       // private SerializedDataParameter borderRange;
        private SerializedDataParameter scatterFade;
        private SerializedDataParameter sunScatterFalloff;
        private SerializedDataParameter alphaFade;
        private SerializedDataParameter fogFade;
        private SerializedDataParameter sssDirFade;
        private SerializedDataParameter fogStart;
        private SerializedDataParameter fogEnd;
        private SerializedDataParameter scatterStart;
        private SerializedDataParameter scatterEnd;
        private SerializedDataParameter sunScale;
        private SerializedDataParameter rotateSpeed;
        private SerializedDataParameter sunColor;
        private SerializedDataParameter tintColor;
       // private SerializedDataParameter sssColor;

        private SerializedDataParameter sunflareFalloff;
        private SerializedDataParameter sunflareColor;

        private SerializedDataParameter skyLowerColor;
        private SerializedDataParameter skyUpperColor;
        private SerializedDataParameter skyHorizon;
        private SerializedDataParameter skyHorizonTilt;
        private SerializedDataParameter skyFinalExposure;
        public override void OnEnable()
        {
            // Fog params.
            PropertyFetcher<Skybox> o = new PropertyFetcher<Skybox>(serializedObject);
            mode = Unpack(o.Find(x => x.mode));
            customSkybox = Unpack(o.Find(x => x.customSkybox));
            baseTex = Unpack(o.Find(x => x.baseTex));
            customBackground = Unpack(o.Find(x => customBackground));
            cloudDarkColor = Unpack(o.Find(x => x.cloudDarkColor));
          //  borderRange = Unpack(o.Find(x => x.borderRange));
            cloudLightColor = Unpack(o.Find(x => x.cloudLightColor));            
            maskTexture = Unpack(o.Find(x => x.maskTexture));
            scatterFade = Unpack(o.Find(x => x.scatterFade));
            alphaFade = Unpack(o.Find(x => x.alphaFade));
            fogFade = Unpack(o.Find(x => x.fogFade));
            sssDirFade = Unpack(o.Find(x => x.sssDirFade));
            fogStart = Unpack(o.Find(x => x.fogStart));
            fogEnd = Unpack(o.Find(x => x.fogEnd));
            scatterStart = Unpack(o.Find(x => x.scatterStart));
            scatterEnd = Unpack(o.Find(x => x.scatterEnd));
            sunScale = Unpack(o.Find(x => x.sunScale));
            rotateSpeed = Unpack(o.Find(x => x.rotateSpeed));
            sunColor = Unpack(o.Find(x => x.sunColor));
            tintColor = Unpack(o.Find(x => x.tintColor));
           // sssColor = Unpack(o.Find(x => x.sssColor));
            borderColor = Unpack(o.Find(x => x.borderColor));
            borderEmissionRange =  Unpack(o.Find(x => x.borderEmissionRange));
            maskTexture = Unpack(o.Find(x => x.maskTexture));
            ////闪电
            //lightingRotX = Unpack(o.Find(x => x.lightingRotX));
            //lightingRotY = Unpack(o.Find(x => x.lightingRotY));
            //lightingPTSize = Unpack(o.Find(x => x.lightingPTSize));
            //flashFrequency = Unpack(o.Find(x => x.flashFrequency));
            //lightingColor = Unpack(o.Find(x => x.lightingColor));
            sunScatterFalloff = Unpack(o.Find(x => x.sunScatterFalloff));
            sunflareFalloff = Unpack(o.Find(x => x.sunflareFalloff));
            sunflareColor = Unpack(o.Find(x => x.sunflareColor));

            skyLowerColor = Unpack(o.Find(x => x.skyLowerColor));
            skyUpperColor = Unpack(o.Find(x => x.skyUpperColor));
            skyHorizon = Unpack(o.Find(x => x.skyHorizon));
            skyHorizonTilt = Unpack(o.Find(x => x.skyHorizonTilt));
            skyFinalExposure = Unpack(o.Find(x => x.skyFinalExposure));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(mode, new GUIContent("天空盒类型"));
            switch ((SkyboxMode) mode.value.enumValueIndex)
            {
                case SkyboxMode.Atmospher:
                    EditorGUILayout.LabelField("背景");
                    PropertyField(skyFinalExposure, new GUIContent("天空盒整体曝光"));
                    PropertyField(customBackground, new GUIContent("程序生成背景色"));
                    PropertyField(skyLowerColor, new GUIContent("地平线色"));
                    PropertyField(skyUpperColor, new GUIContent("穹顶颜色"));
                    PropertyField(skyHorizon, new GUIContent("地平线高"));
                    PropertyField(skyHorizonTilt, new GUIContent("纬度偏转"));
                    EditorGUILayout.LabelField("云");
                    PropertyField(baseTex, new GUIContent("云层贴图"));
                   // PropertyField(tintColor, new GUIContent("云层颜色"));
                    PropertyField(cloudDarkColor, new GUIContent("云层暗部颜色"));
                    PropertyField(cloudLightColor, new GUIContent("云层亮部颜色")); 
                    PropertyField(rotateSpeed, new GUIContent("旋转速度"));
                   // PropertyField(sssColor, new GUIContent("透光颜色"));
                    PropertyField(maskTexture, new GUIContent("边缘Mask"));
                    PropertyField(borderColor, new GUIContent("边缘颜色"));
                    PropertyField(borderEmissionRange, new GUIContent("边缘发光范围"));
                 //   PropertyField(borderRange, new GUIContent("云边缘收缩"));
                    EditorGUILayout.LabelField("雾效");
                    PropertyField(fogStart, new GUIContent("雾效最低高度"));
                    PropertyField(fogEnd, new GUIContent("雾效最高高度"));
                    
                    EditorGUILayout.LabelField("大气");
                    PropertyField(scatterStart, new GUIContent("最低大气高度"));
                    PropertyField(scatterEnd, new GUIContent("最高大气高度"));

                    EditorGUILayout.LabelField("太阳");
                    PropertyField(sunScale, new GUIContent("太阳尺寸"));
                    PropertyField(sunColor, new GUIContent("太阳颜色"));
                    PropertyField(sunflareFalloff, new GUIContent("日晕衰减速度"));
                    PropertyField(sunflareColor, new GUIContent("日晕颜色"));
                    //EditorGUILayout.LabelField("闪电");
                    //PropertyField(lightingRotX, new GUIContent("X旋转"));
                    //PropertyField(lightingRotY, new GUIContent("Y旋转"));
                    //PropertyField(lightingPTSize, new GUIContent("闪光点大小"));
                    //PropertyField(lightingColor, new GUIContent("闪电颜色"));
                    //PropertyField(flashFrequency, new GUIContent("闪电频率"));
                    
                    EditorGUILayout.LabelField("高级 - 过渡效果调整");
                    PropertyField(sunScatterFalloff, new GUIContent("阳光散射衰减"));
                    PropertyField(scatterFade, new GUIContent("大气"));
                    PropertyField(alphaFade, new GUIContent("云层透明度"));
                    PropertyField(fogFade, new GUIContent("雾效"));
                    PropertyField(sssDirFade, new GUIContent("云层透光范围"));
                    break;
                case SkyboxMode.Custom:
                    PropertyField(customSkybox, new GUIContent("自定义天空盒材质"));
                    break;
                default:
                    break;
            }
        }
    }
}
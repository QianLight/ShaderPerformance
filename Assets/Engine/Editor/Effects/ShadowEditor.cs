using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (Shadow))]
    public sealed class ShadowEditor : EnvEffectEditor<Shadow>
    {
        SerializedParameterOverride shadowMode;
        SerializedParameterOverride shadowParam0;
        SerializedParameterOverride shadowParam1;
        SerializedParameterOverride shadowMisc;
        SerializedParameterOverride color;
        SerializedParameterOverride roleShadowColor;
        SerializedParameterOverride shadowMisc1;
        SerializedParameterOverride shadowMisc2;
        ClassSerializedParameterOverride cloudMap;

        private SavedInt shadowMapDebug;

        static Material mat;
        static Material matExtra;
        static Material matSelf;
        static Material matTmp;
        static Material matTerrain;
        private bool GetMat (ref Material instMat, Shader src)
        {
            if (instMat == null && src != null)
            {
                instMat = new Material (src);
            }
            return instMat != null;
        }
        public override void OnEnable ()
        {
            Shadow shadow = target as Shadow;
            shadowMode = FindParameterOverride (x => x.shadowMode);
            shadowParam0 = FindParameterOverride (x => x.shadowParam0);
            shadowParam1 = FindParameterOverride (x => x.shadowParam1);
            shadowMisc = FindParameterOverride (x => x.shadowMisc);
            color = FindParameterOverride (x => x.color);
            roleShadowColor = FindParameterOverride (x => x.roleShadowColor);
            shadowMisc1 = FindParameterOverride (x => x.shadowMisc1);
            shadowMisc2 = FindParameterOverride (x => x.shadowMisc2);
            cloudMap = FindClassParameterOverride (x => x.cloudMap, shadow.cloudMap);


            shadowMapDebug = new SavedInt($"{EngineContext.sceneNameLower}.{nameof(shadowMapDebug)}", 0);
        }

        private void DrawDebugToggle(SavedBool toggle,string name)
        {
            if (toggle != null)
                toggle.Value = EditorGUILayout.Toggle(name, toggle.Value);

        }
        public override void OnInspectorGUI ()
        {
            PropertyField (shadowMode);
            PropertyField (shadowParam0);
            PropertyField (shadowParam1);
            PropertyField (shadowMisc);
            PropertyField (color);
            PropertyField (roleShadowColor);
            EditorGUI.BeginChangeCheck ();
            PropertyField (shadowMisc1);
            if (EditorGUI.EndChangeCheck ())
            {
                ShadowModify.shadowDirty = true;
            }
            PropertyField (shadowMisc2);

            PropertyField (cloudMap);

            if (!EngineContext.IsRunning)
            {
                EditorUtilities.DrawRect ("Debug");
                if (ShadowModify.drawOctTree != null)
                    ShadowModify.drawOctTree.Value = EditorGUILayout.Toggle("drawOctTree", ShadowModify.drawOctTree.Value);
            }
            else
            {
                EngineContext.instance.shadowMapIndex = EditorGUILayout.IntSlider("shadowIndex", EngineContext.instance.shadowMapIndex, -1, 1);
            }
            DrawDebugToggle(ShadowModify.drawCSM0, "drawCSM0");
            DrawDebugToggle(ShadowModify.drawCSM1, "drawCSM1");
            DrawDebugToggle(ShadowModify.drawCSM2, "drawCSM2");
            DrawDebugToggle(ShadowModify.drawExtra, "drawExtra");
            DrawDebugToggle(ShadowModify.drawExtra2, "drawExtra2");
            DrawDebugToggle(ShadowModify.drawCullingFrustum, "drawCullingFrustum");
            DrawDebugToggle(ShadowModify.drawCullingObject, "drawCullingObject");

            EditorGUI.BeginChangeCheck();
            ShadowMapDebug sdp = (ShadowMapDebug)shadowMapDebug.Value;
            sdp = (ShadowMapDebug)EditorGUILayout.EnumPopup("ShadowDebug", sdp);
            if (EditorGUI.EndChangeCheck())
            {
                shadowMapDebug.Value = (int)sdp;
            }
            var rc = RenderingManager.instance.GetContext ();
            if (rc != null)
            {
                switch (sdp)
                {
                    case ShadowMapDebug.Layer0:
                    case ShadowMapDebug.Layer1:
                    case ShadowMapDebug.Layer2:
                        {
                            RuntimeUtilities.DrawInstpectorTex (
                                null,
                                AssetsConfig.instance.DrawShadowMap,
                                ref mat);
                            if (mat != null)
                            {
                                mat.SetTexture ("_ShadowMap", rc.rts[RenderContext.SceneShadowRT]);
                                mat.SetInt ("_Slice", (int)sdp - 1);
                            }
                        }
                        break;
                    case ShadowMapDebug.ExtraShaow:
                    case ShadowMapDebug.ExtraShaow1:
                        {
                            int offset = sdp - ShadowMapDebug.ExtraShaow;
                            RuntimeUtilities.DrawInstpectorTex (
                                null,
                                AssetsConfig.instance.DrawShadowMapExtra,
                                ref matExtra);
                            if (matExtra != null)
                            {
                                matExtra.SetTexture ("_SimpleShadowMapTex", rc.rts[RenderContext.EXTShadowRT + offset]);
                            }

                        }
                        break;
                    case ShadowMapDebug.SelfShadow:
                        {
                            EditorGUI.BeginChangeCheck();
                            int index = EditorGUILayout.IntSlider("SelfShadow", ShadowModify.selfShadowMapIndex.Value, -1, 2);
                            if(EditorGUI.EndChangeCheck())
                            {
                                ShadowModify.selfShadowMapIndex.Value = index;
                            }
                            var context = EngineContext.instance;
                            if (index >= 0 &&
                                index < context.selfShadowInfo.Length)
                            {
                                var rt = context.selfShadowInfo[index].rt;
                                if (rt != null)
                                    RuntimeUtilities.DrawInstpectorTex (
                                        rt,
                                        AssetsConfig.instance.DrawShadowMapExtra,
                                        ref matSelf);
                                if (matSelf != null)
                                    matSelf.SetTexture ("_SimpleShadowMapTex", rt);
                            }
                        }
                        break;                   
                    case ShadowMapDebug.DepthShadow:
                        {
                            EditorGUI.BeginChangeCheck();
                            int index = EditorGUILayout.IntSlider("SelfShadow", ShadowModify.selfShadowMapIndex.Value, -1, 2);
                            if (EditorGUI.EndChangeCheck())
                            {
                                ShadowModify.selfShadowMapIndex.Value = index;
                            }
                            var context = EngineContext.instance;
                            if (index >= 0 &&
                                index < context.selfShadowInfo.Length)
                            {
                                var rt = context.selfShadowInfo[index].rtdepth;
                                if (rt != null)
                                    RuntimeUtilities.DrawInstpectorTex(
                                        rt,
                                        AssetsConfig.instance.DrawShadowMapExtra,
                                        ref matSelf);
                                if (matSelf != null)
                                    matSelf.SetTexture("_SimpleShadowMapTex", rt);
                            }
                        }
                        break;
                    case ShadowMapDebug.TmpShadow:
                        {
                            //PropertyField(tmpShadowMapIndex);  
                            //int index = tmpShadowMapIndex.value.intValue;
                            //if(index>=0)
                            //{
                            //   RuntimeUtilities.DrawInstpectorTex(
                            //       rc.rts[RenderContext.SceneTmpShadowRT0 + index],
                            //       AssetsConfig.instance.DrawShadowMapExtra,
                            //       ref matTmp);
                            //    if (matTmp != null)
                            //        matTmp.SetTexture("_SimpleShadowMapTex", rc.rts[RenderContext.SceneTmpShadowRT0 + index]);

                            //}
                        }
                        break;
                    case ShadowMapDebug.EditorTerrainShadow:
                        {
                            if (ShadowModify.debugShadowTerrainRT != null)
                            {
                                RuntimeUtilities.DrawInstpectorTex (
                                    ShadowModify.debugShadowTerrainRT,
                                    AssetsConfig.instance.DrawShadowMapExtra,
                                    ref matTerrain);
                            }
                        }
                        break;
                }
            }

        }
    }
}
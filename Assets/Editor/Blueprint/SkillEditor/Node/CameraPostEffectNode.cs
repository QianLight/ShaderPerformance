using System.Collections.Generic;
using BluePrint;
using CFEngine;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public enum EPostffects
    {
        MotionBlur = 0,
        RadialBlur = 1,
    }
    public class CameraPostEffectNode : TimeTriggerNode<EcsData.XCameraPostEffectData>
    {
        private GameObject FxObject;

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";

            if (GetRoot.NeedInitRes)
            {
                if (nodeEditorData != null && !string.IsNullOrEmpty(nodeEditorData.CustomData1))
                {
                    FxObject = AssetDatabase.LoadAssetAtPath(nodeEditorData.CustomData1, typeof(GameObject)) as GameObject;
                }
                else
                    FxObject = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.FxPath + ".prefab", typeof(GameObject)) as GameObject;
            }
        }

        private void ResetValue (EPostffects effectType)
        {
            switch (effectType)
            {
                case EPostffects.MotionBlur:
                    {
                        HosterData.Data0 = 1;
                        HosterData.Data1 = 0.2f;
                        HosterData.Data2 = 0; //blur by camera
                    }
                    break;
                case EPostffects.RadialBlur:
                    {
                        HosterData.Data0 = 0;
                        HosterData.Data1 = 0.06f;
                        HosterData.Data2 = 0.5f;
                        HosterData.Data3 = 0.5f;
                    }
                    break;
            }
        }
        public override void DrawDataInspector ()
        {
            base.DrawDataInspector ();

            HosterData.PlayerTrigger = EditorGUITool.Toggle ("PlayerTrigger", HosterData.PlayerTrigger);
            HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime, false, false, float.MaxValue);
            HosterData.AssistEffect = EditorGUITool.Toggle("AssistEffect", HosterData.AssistEffect);
            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
            FxObject = EditorGUITool.ObjectField ("Fx", FxObject, typeof (GameObject), false) as GameObject;
            if (FxObject != null)
            {
                HosterData.FxPath = FxObject.name.ToLower ();
                if (nodeEditorData != null)
                {
                    nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath (FxObject);
                }
                // HosterData.FxPath = AssetDatabase.GetAssetPath (FxObject).Replace (ResourecePath, "");
                // HosterData.FxPath = HosterData.FxPath.Remove (HosterData.FxPath.LastIndexOf ('.'));
            }
            else HosterData.FxPath = "";

            //if (string.IsNullOrEmpty (HosterData.FxPath))
            //{
            //    EPostffects effectType = EPostffects.MotionBlur;
            //    if (HosterData.EffectType == (int) EnvSettingType.PPMotionBlur)
            //    {
            //        effectType = EPostffects.MotionBlur;
            //    }
            //    else if (HosterData.EffectType == (int) EnvSettingType.PPRadialBlur)
            //    {
            //        effectType = EPostffects.RadialBlur;
            //    }
            //    EditorGUI.BeginChangeCheck ();
            //    effectType = (EPostffects) Popup ("EffectType", (int) effectType, GetRoot.Translate<EPostffects> ());
            //    if (EditorGUI.EndChangeCheck ())
            //    {
            //        ResetValue (effectType);
            //    }
            //    switch (effectType)
            //    {
            //        case EPostffects.MotionBlur:
            //            {
            //                HosterData.EffectType = (int) EnvSettingType.PPMotionBlur;
            //                bool blurByCamera = HosterData.Data2 > 0.5f;
            //                EditorGUI.BeginChangeCheck ();
            //                blurByCamera = EditorGUILayout.Toggle ("BlurByCamera", blurByCamera);
            //                if (EditorGUI.EndChangeCheck ())
            //                {
            //                    if (blurByCamera)
            //                    {
            //                        HosterData.Data1 = 50;
            //                    }
            //                    else
            //                    {
            //                        HosterData.Data0 = 1;
            //                        HosterData.Data1 = 0.2f;
            //                    }
            //                }
            //                HosterData.Data2 = blurByCamera ? 1 : 0;

            //                if (blurByCamera)
            //                {
            //                    HosterData.Data1 = EditorGUILayout.Slider ("Intensity", HosterData.Data1, 0, 100f);
            //                }
            //                else
            //                {
            //                    HosterData.Data0 = EditorGUILayout.Slider ("BlurDiverge", HosterData.Data0, 0, 3);
            //                    HosterData.Data1 = EditorGUILayout.Slider ("Intensity", HosterData.Data1, 0, 1f);
            //                }

            //            }
            //            break;
            //        case EPostffects.RadialBlur:
            //            {
            //                HosterData.EffectType = (int) EnvSettingType.PPRadialBlur;
            //                HosterData.Data0 = EditorGUILayout.Slider ("CleanRadius", HosterData.Data0, 0, 2);
            //                HosterData.Data1 = EditorGUILayout.Slider ("BlurRadius", HosterData.Data1, -0.1f, 0.1f);
            //                HosterData.Data2 = EditorGUILayout.Slider ("CenterX", HosterData.Data2, 0, 1);
            //                HosterData.Data3 = EditorGUILayout.Slider ("CenterY", HosterData.Data3, 0, 1);
            //            }
            //            break;
            //    }
            //}
        }

    }
}
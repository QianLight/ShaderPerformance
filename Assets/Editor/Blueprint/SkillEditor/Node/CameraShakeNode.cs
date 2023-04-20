using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using Cinemachine;

namespace EditorNode
{
    public class CameraShakeNode : TimeTriggerNode<XCameraShakeData>
    {
        private NoiseSettings noise;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";

            if (GetRoot.NeedInitRes)
            {
                noise = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.Path + ".asset", typeof(NoiseSettings)) as NoiseSettings;
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            noise = EditorGUITool.ObjectField("Shake", noise, typeof(NoiseSettings), false) as NoiseSettings;
            if (noise != null)
            {
                HosterData.Path = AssetDatabase.GetAssetPath(noise).Replace(ResourecePath, "");
                HosterData.Path = HosterData.Path.Remove(HosterData.Path.LastIndexOf('.'));
                if (HosterData.Path.EndsWith("_s"))
                {
                    noise = null;
                    HosterData.Path = "";
                }
            }
            else HosterData.Path = "";

            HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime);
            HosterData.Frequency = EditorGUITool.FloatField(  "Frequency", HosterData.Frequency);
            HosterData.Amplitude = EditorGUITool.FloatField(  "Amplitude", HosterData.Amplitude);
            HosterData.AttackTime = TimeFrameField("AttackTime", HosterData.AttackTime);
            HosterData.DecayTime = TimeFrameField("DecayTime", HosterData.DecayTime);
            HosterData.ImpactRadius = EditorGUITool.FloatField("ImpactRadius", HosterData.ImpactRadius);
            HosterData.PlayerTrigger = EditorGUITool.Toggle("PlayerTrigger", HosterData.PlayerTrigger);
            HosterData.CanSkipWhenCastAoYi = EditorGUITool.Toggle("CanSkipWhenCastAoYi", HosterData.CanSkipWhenCastAoYi);
            HosterData.StopAtEnd = EditorGUITool.Toggle("StopAtEnd", HosterData.StopAtEnd);
        }

        public override void ScanPolicy(CFEngine.OrderResList result, CFEngine.ResItem item)
        {
            if (!string.IsNullOrEmpty(HosterData.Path))
                result.Add(item, System.IO.Path.GetFileName(HosterData.Path) + ".asset", ResourecePath + HosterData.Path + ".asset");
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.LifeTime < HosterData.AttackTime + HosterData.DecayTime)
            {
                LogError("Node_" + GetHosterData<XBaseData>().Index + "�����뵭��ʱ�䳬����ʱ�䣡����");
            }

            return true;
        }
    }
}
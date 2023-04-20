// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class SFXWarningZone : MonoBehaviour
// {
//     // [HideInInspector]public WarningType warningType;
//     [HideInInspector]public float warningScale;
//     [HideInInspector]public float warningWeight;
//     [HideInInspector]public float warningLength;
//     [HideInInspector]public float warningDuration;
//     [HideInInspector]public float angle = 90;
//
//     [SerializeField]private ParticleSystem[] area;
//     // public ParticleSystem area_add;
//     [SerializeField]private ParticleSystem spread;
//     [SerializeField]private ParticleSystem flash;
//     [SerializeField]private ParticleSystem glow;
//     // [SerializeField]private bool isSquare = false;
//
//     private ParticleSystemRenderer[] areaRender;
//     // private ParticleSystemRenderer areaAddRender;
//     private ParticleSystemRenderer spreadRender;
//     private ParticleSystemRenderer flashRender;
//     private ParticleSystemRenderer glowRender;
//     private MaterialPropertyBlock _mpb;
//     private static readonly int Angle = Shader.PropertyToID("_Angle");
//
//     // public enum WarningType
//     // {
//     //     Circle,
//     //     Rect,
//     //     Sector
//     // }
//
//     private void OnEnable()
//     {
//         _mpb = new MaterialPropertyBlock();
//         areaRender = new ParticleSystemRenderer[area.Length];
//         for (int i = 0; i < area.Length; i++)
//         {
//             areaRender[i] = area[i].GetComponent<ParticleSystemRenderer>();
//         }
//         // areaRender = area.GetComponent<ParticleSystemRenderer>();
//         // areaAddRender = area_add.GetComponent<ParticleSystemRenderer>();
//         spreadRender = spread.GetComponent<ParticleSystemRenderer>();
//         flashRender = flash.GetComponent<ParticleSystemRenderer>();
//         glowRender = glow.GetComponent<ParticleSystemRenderer>();
//     }
//
//     public void Init(float scale, float duration, float angle = 360)
//     {
//         warningScale = scale;
//         warningDuration = duration;
//         // Random rd = new Random();
//         this.angle = angle/*Random.Range(0, 360)*/;
//         _mpb.SetFloat(Angle, this.angle);
//         SetArc();
//         SetCommon();
//     }
//
//     // public void Init(float scale, float duration, float weight, float length)
//     // {
//     //     warningWeight = weight;
//     //     warningLength = length;
//     //     warningDuration = duration;
//     // }
//     //
//     public void SetCommon()
//     {
//         // transform.localScale = new Vector3(warningScale, warningScale, warningScale);
//         for (int i = 0; i < area.Length; i++)
//         {
//             var areaData = area[i].customData.GetVector(ParticleSystemCustomData.Custom2,1);
//             areaData.mode = ParticleSystemCurveMode.Constant;
//             areaData.constant = warningScale;
//             area[i].customData.SetVector(ParticleSystemCustomData.Custom2, 1, areaData);
//         }
//         
//
//         // var spreadData = spread.customData.GetVector(ParticleSystemCustomData.Custom2,1);
//         // spreadData.mode = ParticleSystemCurveMode.Curve;
//         // spreadData.curve = new AnimationCurve(new Keyframe(0, 0.125f), new Keyframe(1, warningScale));
//         // spread.customData.SetVector(ParticleSystemCustomData.Custom2, 1, areaData);
//         var mainSpread = spread.main;
//         mainSpread.startLifetime = new ParticleSystem.MinMaxCurve(warningDuration);
//
//         var mainFlash = flash.main;
//         mainFlash.startDelay = new ParticleSystem.MinMaxCurve(warningDuration);
//         
//         Debug.Log($"Set Warning: scale:{warningScale} duration:{warningDuration} angle:{angle}");
//     }
//
//     public void SetArc()
//     {
//         for (int i = 0; i < areaRender.Length; i++)
//         {
//             areaRender[i].SetPropertyBlock(_mpb);
//         }
//         // areaAddRender.SetPropertyBlock(_mpb);
//         spreadRender.SetPropertyBlock(_mpb);
//         flashRender.SetPropertyBlock(_mpb);
//         glowRender.SetPropertyBlock(_mpb);
//     }
// }

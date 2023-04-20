#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    // public enum CustomDataType
    // {
    //     Value,
    //     Curve
    // }

    // // public delegate bool CustomDataGui (SFXControl sfx, SFXCustomData customData);
    // [Serializable]
    // public class SFXCustomData
    // {
    //     public CustomDataType dataType = CustomDataType.Value;
    //     public float value = 0;
    //     public AnimationCurve curve = AnimationCurve.Linear (0, 0, 1, 0);
    //     public float scale = 1.0f;

    //     public void Sample (float t, ref float result)
    //     {
    //         if (dataType == CustomDataType.Value)
    //         {
    //             result = value;
    //         }
    //         else
    //         {
    //             result = curve.Evaluate (t) * scale;
    //         }
    //     }
    // }

    [Serializable]
    public class SFXCustomDataGroup
    {
        public bool enable;
        public SFXCustomData[] customData = new SFXCustomData[ParticleData.CustomDataChannel]
        {
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
            new SFXCustomData () { curve = AnimationCurve.Linear (0, 0, 1, 0), scale = 1.0f },
        };
    }
    public abstract class SFXProcessData
    {
        public abstract SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind);
    }

   

    public partial class SFXData : IScquenceTarget
    {
        public Transform t;
        protected Material material;
        protected MaterialPropertyBlock mpb;
        protected bool folder;
        public virtual string CompType { get { return ""; } }

        public virtual Component Comp { get { return t; } }
        public virtual void OnUpdate(float time, float deltaTime, bool restart, bool lockTime)
        {

        }

        public virtual void Refresh(Transform t, Component comp, SFXData parent, float time,
            out float duration)
        {
            duration = -1;
            this.t = t;
        }

        public virtual void Refresh (float time, out float duration)
        {
            duration = -1;
        }

        public virtual void Reset ()
        {

        }

        public virtual void OnGUI (float width)
        {
            if (t != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal ();
                folder = EditorGUILayout.Foldout (folder, t.name);
                EditorGUILayout.ObjectField ("", t, typeof (Transform), true);
                EditorGUILayout.EndHorizontal ();
                EditorGUI.indentLevel--;
            }
        }
    }
    public class SFXParticleDataProcessor : SFXProcessData
    {
        public override SFXData Process(Transform t, SFXData parent, out float duration, out bool processChind)
        {
            duration = 0;
            processChind = true;
            if (t.TryGetComponent<ParticleSystemRenderer>(out var r))
            {
               SFXData sfxData = new ParticleData ();
                sfxData.Refresh (t, r, parent, 0, out var d);
                return sfxData;
            }
            return null;
        }
    }
    public partial class ParticleData : SFXData
    {

        public SFXCustomDataGroup customDataGroup = new SFXCustomDataGroup ();
        public bool isSubEmit = false;
        internal const int CustomDataChannel = 6;
        private Renderer r;
        private ParticleSystem ps;
        static int _Param1 = Shader.PropertyToID ("_Param1");
        static string[] customDateStr = new string[CustomDataChannel]
        {
            "U0",
            "V0",
            "U1",
            "V1",
            "AlphaClip",
            "TurbulenceScale",
        };

        public override Component Comp { get { return ps; } }
        public override void OnUpdate (float time, float deltaTime, bool restart, bool lockTime)
        {
            if (!lockTime)
            {
                if (ps != null)
                {
                    if (!isSubEmit)
                    {
                        ps.Simulate (deltaTime, ps.subEmitters.enabled, restart);
                    }
                    // if (r != null && mpb != null)
                    // {
                    //     if (ps.customData.enabled)
                    //     {
                    //         Vector4 param0 = Vector4.zero;
                    //         Sample (0, ref param0.x);
                    //         Sample (1, ref param0.y);
                    //         Sample (2, ref param0.z);
                    //         Sample (3, ref param0.w);
                    //         mpb.SetVector (ShaderManager._Param, param0);
                    //         Vector4 param1 = Vector4.zero;
                    //         Sample (4, ref param1.x);
                    //         Sample (5, ref param1.y);
                    //         mpb.SetVector (_Param1, param1);
                    //         r.SetPropertyBlock (mpb);
                    //     }

                    // }
                }
            }
        }
        public override void OnGUI (float width)
        {
            base.OnGUI (width);
            if (t != null)
            {
                EditorGUI.indentLevel++;
                if (folder)
                {
                    EditorCommon.BeginGroup ("", true, width - 5);
                    EditorGUILayout.BeginHorizontal ();
                    if (GUILayout.Button ("ConvertMat", GUILayout.MaxWidth (80)))
                    {

                    }
                    EditorGUILayout.EndHorizontal ();
                    if (customDataGroup.enable)
                    {
                        for (int i = 0; i < customDataGroup.customData.Length; ++i)
                        {
                            var cd = customDataGroup.customData[i];
                            EditorGUILayout.BeginHorizontal ();
                            EditorGUILayout.LabelField ("Property", string.Format ("{0}:{1}", customDateStr[i], cd.dataType.ToString ()));
                            EditorGUILayout.EndHorizontal ();
                            EditorGUI.indentLevel++;
                            if (cd.dataType == CustomDataType.Value)
                            {
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.FloatField ("Value", cd.value);
                                EditorGUILayout.EndHorizontal ();
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.CurveField ("Curve", cd.curve);
                                EditorGUILayout.EndHorizontal ();
                                EditorGUILayout.BeginHorizontal ();
                                EditorGUILayout.FloatField ("Scale", cd.scale);
                                EditorGUILayout.EndHorizontal ();
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.Space ();

                    }
                    EditorCommon.EndGroup ();
                }
                EditorGUI.indentLevel--;
            }
        }
        private void Sample (int index, ref float v)
        {
            ref var cd = ref customDataGroup.customData[index];
            cd.Sample (ps.time, ref v);
        }
        public override void Refresh (Transform t, Component comp, SFXData parent, float time,
            out float duration)
        {
            base.Refresh (t, comp, parent, time, out duration);
            Bind (comp as Renderer, parent, time, out duration);
        }

        private void Bind (Renderer r, SFXData parent, float time, out float duration)
        {
            this.r = r;
            this.material = r.sharedMaterial;
            duration = -1;
            if (parent is ParticleData)
            {
                var pd = (parent as ParticleData);
                if (pd.ps != null)
                {
                    isSubEmit = pd.ps.subEmitters.enabled;
                }
            }

            t.TryGetComponent<ParticleSystem> (out ps);
            if (ps != null)
            {
                duration = ps.main.duration;
                bool hasSubEmit = ps.subEmitters.enabled;
                ps.Simulate (time, hasSubEmit, true);
                CopyFromParticle ();
            }
            if (r != null)
            {
                if (mpb == null)
                {
                    mpb = new MaterialPropertyBlock ();
                }
                r.GetPropertyBlock (mpb);
            }
        }

        public override void Refresh (float time, out float duration)
        {
            duration = -1;
            if (t != null)
            {
                if (t.TryGetComponent<Renderer> (out var r))
                {
                    Bind (r, null, time, out duration);
                }
            }
        }
        public static void CopyFromParticle (SFXCustomData[] customData, ref ParticleSystem.CustomDataModule cdm, ParticleSystemCustomData dataId, int index, int compIndex)
        {
            ref var cd = ref customData[index];
            var curve = cdm.GetVector (dataId, compIndex);
            if (curve.mode == ParticleSystemCurveMode.Constant)
            {
                cd.dataType = CustomDataType.Value;
                cd.value = curve.constant;
            }
            else if (curve.mode == ParticleSystemCurveMode.Curve)
            {
                cd.dataType = CustomDataType.Curve;
                cd.curve = curve.curve;
                cd.scale = curve.curveMultiplier;
            }
        }
        public static void CopyFromParticle (SFXCustomData[] customData, ref ParticleSystem.CustomDataModule cdm)
        {
            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom1, 0, 0);
            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom1, 1, 1);
            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom1, 4, 2);
            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom1, 5, 3);

            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom2, 2, 0);
            CopyFromParticle (customData, ref cdm, ParticleSystemCustomData.Custom2, 3, 1);

        }
        public void CopyFromParticle ()
        {
            if (ps != null)
            {
                var psCustomData = ps.customData;
                customDataGroup.enable = psCustomData.enabled;
                if (customDataGroup.enable)
                {
                    var customData = customDataGroup.customData;
                    CopyFromParticle (customData, ref psCustomData);
                }
            }
        }
    }

}

#endif
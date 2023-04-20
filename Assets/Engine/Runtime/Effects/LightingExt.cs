using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
#endif
namespace CFEngine
{
    [Serializable]
    public struct LightInfo
    {
        public Vector4 lightDir;
        public Color lightColor;
#if UNITY_EDITOR
        [NonSerialized]
        public LightWrapper lightWrapper;
#endif

        public static LightInfo DefaultAsset = new LightInfo ()
        {
            lightDir = Quaternion.Euler (90, 0, 0) * -Vector3.forward,
            lightColor = Color.black,
#if UNITY_EDITOR
            lightWrapper = new LightWrapper (),
#endif
        };
        public static LightInfo CreateEmpty ()
        {
            return new LightInfo ()
            {
                lightDir = Quaternion.Euler (90, 0, 0) * -Vector3.forward,
                    lightColor = Color.black,
#if UNITY_EDITOR
                    lightWrapper = new LightWrapper (),
#endif
            };
        }

        public void Copy (ref LightInfo src)
        {
            lightDir = src.lightDir;
            lightColor = src.lightColor;
        }
    }

#if UNITY_EDITOR
    public class LightWrapper
    {
        public Light light;
        // public TransformRotationGUIWrapper lightRot;
        public RotGUI rotGui;
    }
#endif
    [Serializable]
    public sealed class LightingParam : ParamOverride<LightInfo, LightingParam>
    {
        [NonSerialized]
        public AnimContext<Color> anim0;
        [NonSerialized]
        public AnimContext<float> anim1;

#if UNITY_EDITOR
        //public AnimContextColor curve0 = new AnimContextColor() { mask = MaskX };
        //public AnimContextFloat curve1 = new AnimContextFloat() { mask = MaskY };
#endif
        protected override void InnerInterp (ParamOverride<LightInfo, LightingParam> from, ParamOverride<LightInfo, LightingParam> to, float t)
        {
            // Vector4 lightColorIntensity0 = new Vector4 (
            //     from.value.lightColor.r * from.value.lightDir.w,
            //     from.value.lightColor.g * from.value.lightDir.w,
            //     from.value.lightColor.b * from.value.lightDir.w, 1);
            // Vector4 lightColorIntensity1 = new Vector4 (
            //     to.value.lightColor.r * to.value.lightDir.w,
            //     to.value.lightColor.g * to.value.lightDir.w,
            //     to.value.lightColor.b * to.value.lightDir.w, 1);

            value.lightColor = Vector4.Lerp (from.value.lightColor, to.value.lightColor, t);

            value.lightDir = from.value.lightDir + (to.value.lightDir - from.value.lightDir) * t;
        }
        public override void SetValue (ParamOverride parameter, bool shallowCopy)
        {
            LightingParam src = parameter as LightingParam;
            value.lightColor = src.value.lightColor;
            value.lightDir = src.value.lightDir;
        }
        protected override void InnerSetValue (ParamOverride<LightInfo, LightingParam> po, byte mask)
        {
            if ((mask & MaskX) > 0)
                value.lightColor = po.value.lightColor;
            if ((mask & MaskY) > 0)
                value.lightDir.w = po.value.lightDir.w;
            if ((mask & MaskZ) > 0)
                value.lightDir = po.value.lightDir;
        }

        protected override void InnerLoadFromData (IGetEnvValue valueHandler, EnvParam envParam)
        {
#if UNITY_EDITOR
            if (EngineContext.IsRunning)
#endif
            {
                int dataOffset = envParam.dataOffset;
                if ((envParam.valueMask & MaskX) > 0)
                {
                    value.lightColor.r = valueHandler.GetValue (dataOffset++);
                    value.lightColor.g = valueHandler.GetValue (dataOffset++);
                    value.lightColor.b = valueHandler.GetValue (dataOffset++);
                }
                if ((envParam.valueMask & MaskY) > 0)
                {
                    value.lightDir.w = valueHandler.GetValue (dataOffset++);
                }
                if ((envParam.valueMask & MaskZ) > 0)
                {
                    value.lightDir.x = valueHandler.GetValue (dataOffset++);
                    value.lightDir.y = valueHandler.GetValue (dataOffset++);
                    value.lightDir.z = valueHandler.GetValue (dataOffset++);
                }
            }
        }

        public void Load (CFBinaryReader reader)
        {
            value.lightDir = reader.ReadVector4 ();
            value.lightColor = reader.ReadVector4 ();
        }

        public override void BindAnimData (AnimEnvParam aep, ref AnimPack animPack)
        {
            int indexOffset = aep.curveIndexStart;
            if (animPack.cCurve != null)
            {
                if ((aep.valueMask & MaskX) > 0)
                {
                    AnimPack.BindCurve (ref animPack, ref indexOffset, ref anim0);
                }
            }
            if (animPack.fCurve != null)
            {
                if ((aep.valueMask & MaskY) > 0)
                {
                    AnimPack.BindCurve (ref animPack, ref indexOffset, ref anim1);
                }
            }

        }

        public override void SetAnimValue(float t, AnimEnvParam aep, ref AnimPack animPack)
        {
            base.SetAnimValue(t, aep, ref animPack);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                var envParam = aep.envParam;
                if (envParam != null)
                {
                    var p = envParam.param as LightingParam;
                    if ((envParam.valueMask & MaskX) > 0)
                    {
                        //p.curve0.Evaluate(t, ref value.lightColor);                       
                        //p.curve0.envParam = envParam;
                        p.value.lightColor = value.lightColor;
                    }
                    if ((envParam.valueMask & MaskY) > 0)
                    {
                        //p.curve1.Evaluate(t, ref value.lightDir.w);
                        //p.curve1.envParam = envParam;
                        p.value.lightDir.w = value.lightDir.w;
                    }
                }
            }
            else
#endif
            {
                if ((aep.valueMask & MaskX) > 0)
                {
                    anim0.Evaluate (t, animPack.cCurve, ref value.lightColor);
                }
                if ((aep.valueMask & MaskY) > 0)
                {
                    anim1.Evaluate (t, animPack.fCurve, ref value.lightDir.w);
                }
            }
        }

#if UNITY_EDITOR
        public override void Create(IGetEnvValue valueHandler, EnvParam envParam)
        {
            base.Create(valueHandler, envParam);
            if (EngineContext.IsRunning)
            {
                InnerLoadFromData(valueHandler, envParam);
            }
            LightingParam lp = envParam.param as LightingParam;
            lp.value.lightWrapper = new LightWrapper ();
            var runtime = envParam.runtimeParam != null ? envParam.runtimeParam.runtime : null;
            var light = RuntimeUtilities.CreateLight (string.Format ("_AreaDummyLight_{0}", runtime != null?runtime.name: "empty"), ref lp.value, 0, true, envParam.owner);
            light.transform.parent = envParam.owner;
            lp.value.lightWrapper.light = light;
            lp.value.lightWrapper.rotGui = new RotGUI ();
            lp.value.lightWrapper.rotGui.OnInit (light.transform);
            light.color = lp.value.lightColor;
            light.intensity = lp.value.lightDir.w;
            light.transform.rotation = Quaternion.LookRotation (-lp.value.lightDir);

        }

        public override void BeforeCopyParam (ParamOverride runtime)
        {
            var runtimeLight = runtime as LightingParam;
            runtimeLight.value.lightWrapper = value.lightWrapper;
            RuntimeUtilities.SyncLight (ref value);
        }

        public override void OnGizmo (EnvParam envParam)
        {
            var runtime = envParam.runtimeParam != null ? envParam.runtimeParam.runtime : null;
            var src = runtime as LightingParam;
            if ((envParam.valueMask & MaskZ) > 0 && src != null)
            {
                EngineContext context = EngineContext.instance;
                Color temp = Handles.color;
                if (SceneView.lastActiveSceneView != null &&
                    SceneView.lastActiveSceneView.camera != null &&
                    context != null)
                {
                    var wrapper = value.lightWrapper;
                    if (wrapper != null && wrapper.light != null)
                    {
                        Transform t = SceneView.lastActiveSceneView.camera.transform;
                        Vector3 centerPos = t.position + t.forward * 10;
                        Vector3 pos = centerPos + t.right * -4 + t.up * 3;
                        Color c = src.value.lightColor;
                        if ((envParam.valueMask & MaskX) > 0)
                        {
                            c = value.lightColor;
                        }
                        float intensity = src.value.lightDir.w;
                        if ((envParam.valueMask & MaskY) > 0)
                        {
                            intensity = value.lightDir.w;
                        }
                        RuntimeUtilities.DrawLightHandle (wrapper.light.transform,
                            ref pos,
                            intensity,
                            c, "MainLight");
                    }
                }

                Handles.color = temp;
            }
        }

        public void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, value.lightDir);
            EditorCommon.WriteVector (bw, value.lightColor);
        }
        
        public override void SaveAnim(AnimEnvParam aep, ref AnimPack animPack)
        {
            aep.curveIndexStart = animPack.index.Count;
            if ((aep.valueMask & MaskX) > 0)
            {
                int start = animPack.c.Count;
                int end = animPack.c.Count;
                animPack.index.Add(start);
               // animPack.c.AddRange(curve0.data);
                animPack.index.Add(end);
                
                start = animPack.f.Count;
                end = animPack.f.Count;
                animPack.index.Add(start);
                //animPack.f.AddRange(curve1.data);
                animPack.index.Add(end);
            }
        }

        public override void Serialize(EnvParam envParam, IGetEnvValue valueHandler)
        {
            envParam.dataOffset = valueHandler.GetCount();

            if ((envParam.valueMask & MaskX) > 0)
            {
                valueHandler.SetValue(value.lightColor.r);
                valueHandler.SetValue(value.lightColor.g);
                valueHandler.SetValue(value.lightColor.b);
            }
            if ((envParam.valueMask & MaskY) > 0)
            {
                valueHandler.SetValue(value.lightDir.w);
            }
            if ((envParam.valueMask & MaskZ) > 0)
            {
                valueHandler.SetValue(value.lightDir.x);
                valueHandler.SetValue(value.lightDir.y);
                valueHandler.SetValue(value.lightDir.z);
            }
        }
        public override void SetCurveValue(float t, EnvParam envParam)
        {
            if ((envParam.valueChangeMask & MaskX) > 0)
            {
                //curve0.SetKeyValue(t, ref value.lightColor);
            }
            else if ((envParam.valueChangeMask & MaskX) > 0)
            {
               // curve1.SetKeyValue(t, ref value.lightDir.w);
            }
        }
        public override void GetEditingCurve(List<AnimContextBase> editAnim, EnvParam envParam)
        {
            if ((envParam.valueMask & MaskX) > 0 &&
                (envParam.animMask & MaskX) > 0)
            {
                //editAnim.Add(curve0);
            }
            if ((envParam.valueMask & MaskY) > 0 &&
                (envParam.animMask & MaskY) > 0)
            {
                //editAnim.Add(curve1);
            }
        }
#endif
    }
}
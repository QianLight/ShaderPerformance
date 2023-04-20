using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
namespace CFEngine
{
#if UNITY_EDITOR
    public delegate void CurveCb(ref EditorCurveBinding curebinding,AnimationCurve curve);
#endif
    [Serializable]
    public class AnimHelper
    {
        [NonSerialized]
        public uint mask;
        [NonSerialized]
        public EditorFlagMask flag;
#if UNITY_EDITOR
        [NonSerialized]
        public SerializedProperty valueSp;
        [NonSerialized]
        public SerializedProperty animSp;
        public void Reset ()
        {
            valueSp = null;
            animSp = null;
        }

        public bool DrawGUI (SerializedProperty sp, string name)
        {
            if (valueSp == null)
            {
                valueSp = sp.FindPropertyRelative ("value");
            }
            if (animSp == null)
            {
                animSp = sp.FindPropertyRelative ("anim");
            }

            bool useAnim = false;
            if (flag != null)
            {
                useAnim = flag.HasFlag (mask);
                EditorGUILayout.BeginHorizontal ();
                EditorGUI.BeginChangeCheck ();
                EditorGUILayout.LabelField (name, EditorStyles.boldLabel);
                useAnim = EditorGUILayout.Toggle ("", useAnim);
                if (EditorGUI.EndChangeCheck ())
                {
                    flag.SetFlag (mask, useAnim);
                }
                EditorGUILayout.EndHorizontal ();
            }

            return useAnim;
        }
#endif
        public void InitCreate (uint mask, EditorFlagMask flag)
        {
            this.flag = flag;
            this.mask = mask;
        }


#if UNITY_EDITOR
        public static void EnumAnimCurve(AnimationClip clip, CurveCb cb)
        {
            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings(clip);
            for (int i = 0; i < curveBinding.Length; ++i)
            {
                ref var binding = ref curveBinding[i];
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve.keys.Length > 1)
                {
                    cb(ref binding, curve);
                }
            }
        }
#endif
    }

    [Serializable]
    public class FloatAnim
    {
        public float value;
        public AnimationCurve anim;
        public AnimHelper animHelper = new AnimHelper ();
#if UNITY_EDITOR
        [NonSerialized]
        public float min;
        [NonSerialized]
        public float max;
        [NonSerialized]
        public float current;
        public static void InitCreate (ref FloatAnim fa, float min, float max, float defaultValue)
        {
            if (fa == null)
            {
                fa = new FloatAnim () { value = defaultValue };
            }
            fa.min = min;
            fa.max = max;
        }
        public void DrawGUI (SerializedProperty sp, string name)
        {
            bool useAnim = animHelper.DrawGUI (sp, name);
            EditorGUI.indentLevel++;
            if (useAnim)
            {
                EditorGUILayout.PropertyField (animHelper.animSp);
            }
            else
            {
                EditorGUI.BeginChangeCheck ();
                value = EditorGUILayout.Slider ("Value", value, min, max);
                if (EditorGUI.EndChangeCheck ())
                {
                    animHelper.valueSp.floatValue = value;
                }
            }
            EditorGUI.indentLevel--;
        }
#endif   
        public float Evaluate (float time)
        {
            bool isAnim = false;
            return Evaluate (time, ref isAnim);
        }

        public float Evaluate (float time, ref bool isAnim)
        {
            float v = value;
            if (animHelper.flag != null && animHelper.flag.HasFlag (animHelper.mask))
            {
                if (anim != null)
                {
                    v = anim.Evaluate (time);
                    isAnim = true;
                }
            }
            else
            {
                v = value;
            }
#if UNITY_EDITOR
            current = v;
#endif   
            return v;
        }
    }

    [Serializable]
    public class ColorAnim
    {
        public Color value;
        public Gradient anim;
        public AnimHelper animHelper = new AnimHelper ();
#if UNITY_EDITOR
        [NonSerialized]
        public Color current;
        public static void InitCreate (ref ColorAnim fa, Color defaultValue)
        {
            if (fa == null)
            {
                fa = new ColorAnim () { value = defaultValue };
            }
        }
        public void DrawGUI (SerializedProperty sp, string name)
        {
            bool useAnim = animHelper.DrawGUI (sp, name);
            EditorGUI.indentLevel++;
            if (useAnim)
            {
                EditorGUILayout.PropertyField (animHelper.animSp);
            }
            else
            {
                EditorGUILayout.PropertyField (animHelper.valueSp);
            }
            EditorGUI.indentLevel--;
        }
#endif   
        public Color Evaluate (float time)
        {
            Color v = value;
            if (animHelper.flag != null && animHelper.flag.HasFlag (animHelper.mask))
            {
                if (anim != null)
                {
                    v = anim.Evaluate (time);
                }
            }
            else
            {
                v = value;
            }
#if UNITY_EDITOR
            current = v;
#endif   
            return v;
        }
    }

    public static unsafe partial class AnimCurveUtility
    {
        public static Vector4 tmp;
        public static void EvaluateAt (this AnimCurveShare curvePool, ref CurveContext context, float time, ref Vector4 v)
        {
            // CurveData * ptr = (CurveData * ) curvePool.curves.GetUnsafePtr ();
            // ref CurveData curve = ref ptr[context.curveIndex];
            CurveData curve = curvePool.curves.Get (context.curveIndex); // ptr[context.curveIndex];
            curvePool.Evaluate (ref curve, ref context, time, ref v);
        }
        public static void EvaluateAt (this AnimCurveShare curvePool, ref CurveContext context, float time, ref Vector3 v)
        {
            // CurveData * ptr = (CurveData * ) curvePool.curves.GetUnsafePtr ();
            // ref CurveData curve = ref ptr[context.curveIndex];
            CurveData curve = curvePool.curves.Get (context.curveIndex);
            curvePool.Evaluate (ref curve, ref context, time, ref tmp);
            v.x = tmp.x;
            v.y = tmp.y;
            v.z = tmp.z;
        }
        public static void EvaluateAt (this AnimCurveShare curvePool, ref CurveContext context, float time, ref Vector2 v)
        {
            // CurveData * ptr = (CurveData * ) curvePool.curves.GetUnsafePtr ();
            // ref CurveData curve = ref ptr[context.curveIndex];
            CurveData curve = curvePool.curves.Get (context.curveIndex);
            curvePool.Evaluate (ref curve, ref context, time, ref tmp);
            v.x = tmp.x;
            v.y = tmp.y;
        }
        public static void EvaluateAt (this AnimCurveShare curvePool, ref CurveContext context, float time, ref float v)
        {
            // CurveData * ptr = (CurveData * ) curvePool.curves.GetUnsafePtr ();
            // ref CurveData curve = ref ptr[context.curveIndex];
            CurveData curve = curvePool.curves.Get (context.curveIndex);
            curvePool.Evaluate (ref curve, ref context, time, ref tmp);
            v = tmp.x;
        }
#if UNITY_EDITOR
        public class AnimClipGroup
        {
            public int stride = 3;
            public int index = -1;
            public List<KeyFrameValue> keys = new List<KeyFrameValue> ();
            public void AddKeyFrames (AnimationCurve curve)
            {
                for (int j = 0; j < curve.length; ++j)
                {
                    var key = curve[j];
                    int time = (int) (key.time * 100);
                    KeyFrameValue kfv = keys.Find ((x) => x.intTime == time);
                    if (kfv == null)
                    {
                        kfv = new KeyFrameValue ()
                        {
                        intTime = time,
                        key = key,

                        };
                        keys.Add (kfv);
                    }
                }
            }

        }
        public class AnimPRSGroup
        {
            public string path;
            public AnimClipGroup pos;
            public AnimClipGroup rot;
            public AnimClipGroup scale;
        }
        static AnimationCurve[] v3 = new AnimationCurve[3];
        static AnimationCurve[] v4 = new AnimationCurve[4];
        private static void ReSampleAnimationPos (GameObject go, Transform root, AnimationClip clip, List<KeyFrameValue> keys)
        {
            for (int j = 0; j < keys.Count; ++j)
            {
                var kfv = keys[j];
                clip.SampleAnimation (go, kfv.key.time);
                kfv.v = root.position;
            }
        }
        private static void ReSampleAnimationRot (GameObject go, Transform root, AnimationClip clip, List<KeyFrameValue> keys)
        {
            for (int j = 0; j < keys.Count; ++j)
            {
                var kfv = keys[j];
                clip.SampleAnimation (go, kfv.key.time);
                Quaternion q = root.rotation;
                kfv.v.x = q.x;
                kfv.v.y = q.y;
                kfv.v.z = q.z;
                kfv.v.w = q.w;
            }
        }
        private static void ReSampleAnimationScale (GameObject go, Transform root, AnimationClip clip, List<KeyFrameValue> keys)
        {
            for (int j = 0; j < keys.Count; ++j)
            {
                var kfv = keys[j];
                clip.SampleAnimation (go, kfv.key.time);
                kfv.v = root.localScale;
            }
        }
        static string[] rotBindingNamePrefix = new string[] { ".x", ".y", ".z" };
        static string[] rotBindingName = new string[] { "localEulerAnglesBaked.x", "localEulerAnglesBaked.y", "localEulerAnglesBaked.z" };
        public static List<AnimPRSGroup> AddPRSAnim (this AnimCurveShare curvePool, AnimationClip clip,
            out int posIndex, out int rotIndex, out int scaleIndex)
        {
            posIndex = -1;
            rotIndex = -1;
            scaleIndex = -1;
            List<AnimPRSGroup> acg = new List<AnimPRSGroup> ();
            EditorCurveBinding[] curveBinding = AnimationUtility.GetCurveBindings (clip);
            for (int i = 0; i < curveBinding.Length; ++i)
            {
                var binding = curveBinding[i];
                string path = binding.path;
                var ag = acg.Find (x => x.path == path);
                if (ag == null)
                {
                    ag = new AnimPRSGroup ();
                    ag.path = path;
                    acg.Add (ag);
                }

                string name = binding.propertyName;
                if (name.StartsWith ("m_LocalPosition"))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);
                    if (ag.pos == null)
                        ag.pos = new AnimClipGroup ();
                    ag.pos.AddKeyFrames (curve);
                }
                else if (name.StartsWith ("m_LocalRotation"))
                {
                    for (int j = 0; j < rotBindingNamePrefix.Length; ++j)
                    {
                        if (name.EndsWith (rotBindingNamePrefix[j]))
                        {
                            if (ag.rot == null)
                                ag.rot = new AnimClipGroup ();
                            binding.propertyName = rotBindingName[j];
                            AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);
                            if (curve != null)
                            {
                                ag.rot.AddKeyFrames (curve);
                            }
                        }

                    }
                }
                else if (name.StartsWith ("m_LocalScale"))
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve (clip, binding);
                    if (ag.scale == null)
                        ag.scale = new AnimClipGroup ();
                    ag.scale.AddKeyFrames (curve);
                }
            }

            acg.Reverse ();

            GameObject tmpGo = new GameObject ("Tmp");
            GameObject tmpRoot = new GameObject ("Root");
            tmpRoot.transform.parent = tmpGo.transform;
            for (int i = 0; i < acg.Count; ++i)
            {
                var ag = acg[i];
                CurveContext context = new CurveContext ();
                if (ag.pos != null)
                {
                    ReSampleAnimationPos (tmpGo, tmpRoot.transform, clip, ag.pos.keys);
                    curvePool.AddCurve (ag.pos.keys, ag.pos.stride, ref context);
                    // var curves = GetCurveArray (ag.pos);
                    // curvePool.AddCurve (curves, ref context);
                    ag.pos.index = context.curveIndex;
                    if (ag.path.StartsWith ("Root"))
                    {
                        posIndex = context.curveIndex;
                    }
                }
                if (ag.rot != null)
                {
                    // var curves = GetCurveArray (ag.rot);
                    // curvePool.AddCurve (curves, ref context);
                    ReSampleAnimationRot (tmpGo, tmpRoot.transform, clip, ag.rot.keys);
                    ag.rot.stride = 4;
                    curvePool.AddCurve (ag.rot.keys, ag.rot.stride, ref context);
                    ag.rot.index = context.curveIndex;
                    if (ag.path.StartsWith ("Root"))
                    {
                        rotIndex = context.curveIndex;
                    }
                }
                if (ag.scale != null)
                {
                    // var curves = GetCurveArray (ag.scale);
                    // curvePool.AddCurve (curves, ref context);
                    ReSampleAnimationScale (tmpGo, tmpRoot.transform, clip, ag.scale.keys);
                    curvePool.AddCurve (ag.scale.keys, ag.scale.stride, ref context);
                    ag.scale.index = context.curveIndex;
                    if (ag.path.StartsWith ("Root"))
                    {
                        scaleIndex = context.curveIndex;
                    }
                }
            }
            GameObject.DestroyImmediate (tmpRoot);
            GameObject.DestroyImmediate (tmpGo);
            return acg;
        }
#endif
    }
}
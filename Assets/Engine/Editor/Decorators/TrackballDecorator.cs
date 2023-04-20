using System;
using UnityEngine;
using UnityEditor;

namespace CFEngine.Editor
{
    [CFDecorator(typeof(CFTrackballAttribute))]
    public sealed class TrackballDecorator : AttributeDecorator<CFTrackballAttribute, Vector4Param>
    {
        Vector4 editValue = Vector4.zero;
        public static TrackballUIDrawer trackballUIDrawer = new TrackballUIDrawer();
        static Func<Vector4, Vector3>[] customFun = new Func<Vector4, Vector3>[]
        {
            GetLiftValue,
            GetWheelValue
        };

        public static Vector3 GetLiftValue(Vector4 x) => new Vector3(x.x + x.w, x.y + x.w, x.z + x.w);

        public static Vector3 GetWheelValue(Vector4 v)
        {
            float w = v.w * (Mathf.Sign(v.w) < 0f ? 1f : 4f);
            return new Vector3(
                Mathf.Max(v.x + w, 0f),
                Mathf.Max(v.y + w, 0f),
                Mathf.Max(v.z + w, 0f)
            );
        }
        
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Vector4;
        }

        protected override bool EndOverride (ref byte mask)
        {
            if (overrideToggleChange)
            {
                SetMask (ref mask, ParamOverride.MaskX, maskX);
                SetMask (ref mask, ParamOverride.MaskY, maskX);
                SetMask (ref mask, ParamOverride.MaskZ, maskX);
                SetMask (ref mask, ParamOverride.MaskW, maskX);
                if (!maskX)
                {
                    overrideParam.value = profileParam.value;
                }
            }

            if (overrideParam != null)
            {
                if (valueChange)
                {
                    overrideParam.value = editValue;
                }
                return valueChange;
            }
            else
            {
                if (valueChange)
                {
                    sp.vector4Value = editValue;
                }
                return true;
            }
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            Vector4 srcValue = sp.vector4Value;
            editValue = srcValue;

            MaskToggle(ref maskX);
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue = srcValue;
                }
            }
            var fun = customFun[attr.mode];
            bool overrideState = true;
            EditorGUILayout.BeginVertical ();
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxWidth (160)))
            {
                EditorGUI.BeginChangeCheck ();
                trackballUIDrawer.OnGUI(ref editValue, ref overrideState, title, fun);
                if (EditorGUI.EndChangeCheck ())
                {
                    valueChange = true;
                }
            }
            EditorGUILayout.EndVertical ();
        }



        public override void InnerResetValue ()
        {
            sp.vector4Value = attr.defaultValue;
        }
    }
}

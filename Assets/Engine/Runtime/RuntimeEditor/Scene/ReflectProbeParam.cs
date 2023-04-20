#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[DisallowMultipleComponent, ExecuteInEditMode]
public class ReflectProbeParam : MonoBehaviour
{
    [System.NonSerialized]
    public ReflectionProbe rp;
    // Start is called before the first frame update
    void Start ()
    {

    }

    // Update is called once per frame
    void Update ()
    {
        if (rp == null)
        {
            rp = GetComponent<ReflectionProbe> ();
        }
    }
}

[CanEditMultipleObjects, CustomEditor (typeof (ReflectProbeParam))]
public class ReflectProbeParamEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI ()
    {
        ReflectProbeParam rpp = target as ReflectProbeParam;
        if (rpp.rp != null)
        {
            Vector4 param = rpp.rp.textureHDRDecodeValues;
            EditorGUILayout.FloatField ("HDR", param.x);
            EditorGUILayout.FloatField ("Gamma", param.y);
        }

    }
}
#endif
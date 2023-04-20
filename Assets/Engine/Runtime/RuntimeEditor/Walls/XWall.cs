using CFEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public abstract class XWall : MonoBehaviour
{
    public string hashStr;
    // Use this for initialization

    public bool sideLimit = false;

    void Awake ()
    {
        BoxCollider _box = GetComponent<BoxCollider> ();
        if (_box != null)
        {
            _box.enabled = false;

        }
    }

    private void CollisionDetected (Vector3 pos, Vector3 last)
    {
        // if (XCommon.singleton.IsLineSegmentCross(last, pos, _left, _right))
        // {
        //     Vector3 dir = pos - last;

        //     _forward_collision = Vector3.Dot(dir, transform.forward) > 0;
        //     OnTriggered();
        // }
    }

    protected abstract void OnTriggered ();

    private Transform t = null;
    private BoxCollider box = null;
    private SphereCollider sphere = null;

    private void OnDrawGizmos ()
    {
        var c = Gizmos.color;
        Gizmos.color = Color.green;
        if (t == null)
        {
            t = this.transform;
        }
        // if (useNewData)
        // {
        //     if (isSphere)
        //     {
        //         var pos0 = t.position;
        //         var pos1 = pos0;
        //         pos1.x = size;
        //         SceneDynamicObject.DrawGizmo (true, ref pos0, ref pos1, ref pos0, this.name);
        //     }
        //     else
        //     {
        //         Vector3 half = t.right * size * 0.5f;
        //         float h = height * 0.5f;

        //         var pos0 = t.position - half;
        //         var pos1 = t.position + half;

        //         pos0.y = t.position.y - h;
        //         pos1.y = t.position.y + h;
        //         Vector3 delta = pos1 - pos0;
        //         var normal = Vector3.Cross (delta, Vector3.up).normalized;
        //         SceneDynamicObject.DrawGizmo (false, ref pos0, ref pos1, ref normal, this.name);
        //     }

        // }
        // else
        {
            if (box == null && sphere == null)
            {
                t.TryGetComponent (out box);
                t.TryGetComponent (out sphere);
            }
            if (box != null)
            {
                Vector3 half = Vector3.right * box.size.x * box.transform.localScale.x * 0.5f;
                float h = box.size.y * box.transform.localScale.y * 0.5f;
                var pos0 = box.center - half;
                var pos1 = box.center + half;

                pos0 = t.localToWorldMatrix * pos0;
                pos0 += t.position;
                pos0.y = box.transform.position.y - h;
                pos1 = t.localToWorldMatrix * pos1;
                pos1 += t.position;
                pos1.y = t.position.y + h;
                Vector3 delta = pos1 - pos0;
                var normal = Vector3.Cross (delta, Vector3.up).normalized;
                SceneDynamicObject.DrawGizmo (false, ref pos0, ref pos1, ref normal, this.name);
            }
            else if (sphere != null)
            {
                var pos0 = t.position;
                var pos1 = Vector3.one * sphere.radius;
                SceneDynamicObject.DrawGizmo (true, ref pos0, ref pos1, ref pos0, this.name);
            }
        }
        Gizmos.color = c;
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects, CustomEditor (typeof (XWall))]
public class XWallEditor : UnityEditor.Editor
{
    // SerializedProperty size;
    // SerializedProperty height;
    // SerializedProperty isSphere;
    // SerializedProperty useNewData;
    // SerializedProperty useTrigger;
    public virtual void OnEnable ()
    {
        // size = serializedObject.FindProperty ("size");
        // height = serializedObject.FindProperty ("height");
        // isSphere = serializedObject.FindProperty ("isSphere");
        // useNewData = serializedObject.FindProperty ("useNewData");
        // useTrigger = serializedObject.FindProperty ("useTrigger");
    }
    public override void OnInspectorGUI ()
    {
        serializedObject.Update ();
        var wall = target as XWall;
        EditorGUILayout.LabelField (wall.hashStr);


        serializedObject.ApplyModifiedProperties ();
    }
}
#endif
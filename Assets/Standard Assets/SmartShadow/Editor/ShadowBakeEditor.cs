using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmartShadow))]
public class ShadowBakeEditor : Editor
{
    private SmartShadow targetObj;
    private void OnEnable()
    {
        targetObj = target as SmartShadow;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Bake shadow"))
        {
            targetObj.Bake();
        }
        base.OnInspectorGUI();
    }

    void getRndPos()
    {
        print(0.75f, 1.0f, 0, 90);
        print(0.75f, 1.0f, 90, 180);
        print(0.75f, 1.0f, 180, 270);
        print(0.75f, 1.0f, 270, 360);

        print(0.5f, 0.75f, 0, 90);
        print(0.5f, 0.75f, 90, 180);
        print(0.5f, 0.75f, 180, 270);
        print(0.5f, 0.75f, 270, 360);

        print(0.25f, 0.5f, 0, 90);
        print(0.25f, 0.5f, 90, 180);
        print(0.25f, 0.5f, 180, 270);
        print(0.25f, 0.5f, 270, 360);

        print(0.0f, 0.25f, 0, 90);
        print(0.0f, 0.25f, 90, 180);
        print(0.0f, 0.25f, 180, 270);
        print(0.0f, 0.25f, 270, 360);
    }
    void print(float rMin, float rMax, float angleMin, float angleMax)
    {
        Vector2 pos = getPos(Random.Range(rMin, rMax), Random.Range(angleMin, angleMax));
        Debug.Log("float2(" + pos.x + "f," + pos.y + "f),");
    }
    Vector2 getPos(float r, float angle)
    {
        return new Vector2(r * Mathf.Cos(angle * 3.14159f / 180f), r * Mathf.Sin(angle * 3.14159f / 180f));
    }
}

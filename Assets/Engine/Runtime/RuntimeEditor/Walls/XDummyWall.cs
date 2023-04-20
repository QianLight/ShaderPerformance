using CFEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
// 

[Serializable]
public struct PassFlag
{
    [SerializeField]
    public bool Forward;
    [SerializeField]
    public bool Backward;
}

public class XDummyWall : XWall
{
    public PassFlag monsterFlag;
    public PassFlag playerFlag;

    public bool HideFx = false;

    public bool permanentFx = false;
    public bool autoY = false;
    public bool showRange = false;
    public Color gizmosColor = Color.gray;
    public string exString;
    public string buffIDList;
    public string typeList;

    private Mesh gizmosMesh;

    protected override void OnTriggered()
    {
    }

    private Mesh CreateSquareMesh(float x,float y)
    {
        int vertCount = 24;
        Vector3[] verts = new Vector3[vertCount];

        verts[0] = new Vector3(-x, -y, 0.01f);
        verts[1] = new Vector3(-x, y, 0.01f);
        verts[2] = new Vector3(x, y, 0.01f);
        verts[3] = new Vector3(x, -y, 0.01f);

        verts[4] = new Vector3(x, -y, -0.01f);
        verts[5] = new Vector3(x, y, -0.01f);
        verts[6] = new Vector3(-x, y, -0.01f);
        verts[7] = new Vector3(-x, -y, -0.01f);

        verts[8] = verts[4];
        verts[9] = verts[5];
        verts[10] = verts[1];
        verts[11] = verts[0];

        verts[12] = verts[3];
        verts[13] = verts[2];
        verts[14] = verts[6];
        verts[15] = verts[7];

        verts[16] = verts[5];
        verts[17] = verts[1];
        verts[18] = verts[2];
        verts[19] = verts[6];

        verts[20] = verts[4];
        verts[21] = verts[0];
        verts[22] = verts[3];
        verts[23] = verts[7];

        int triCount = 36;
        int[] triIndex = new int[triCount];
        for(int i=0,j=0;i<triCount;i+=6,j+=4)
        {
            triIndex[i] = j;
            triIndex[i + 1] = j + 1;
            triIndex[i + 2] = j + 3;

            triIndex[i + 3] = j + 1;
            triIndex[i + 4] = j + 2;
            triIndex[i + 5] = j + 3;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triIndex;
        mesh.RecalculateNormals();
        return mesh;
    }


    private Mesh CreateCircleMesh(float radius, Color color, float innerRadius)
    {
        float curAngle = Mathf.PI;
        float deltaAngle = 2 * curAngle / 50;
        int vertCount = 2 * (50 + 1);
        Vector3[] verts = new Vector3[vertCount];
        for (var i = 0; i < vertCount; i += 2, curAngle -= deltaAngle)
        {
            float cos = Mathf.Cos(curAngle);
            float sin = Mathf.Sin(curAngle);
            verts[i] = new Vector3(cos * innerRadius, sin * innerRadius, 0);
            verts[i + 1] = new Vector3(cos * radius, sin * radius, 0);
        }
        int[] triangles = new int[3 * (vertCount - 2)];
        for (int i = 0, j = 0; i < triangles.Length; i += 6, j += 2)
        {
            triangles[i] = j + 1;
            triangles[i + 1] = j + 2;
            triangles[i + 2] = j;
            triangles[i + 3] = j + 1;
            triangles[i + 4] = j + 3;
            triangles[i + 5] = j + 2;
        }
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnDrawGizmos()
    {
        if (showRange)
        {
            if (TryGetComponent<SphereCollider>(out var sphere))
            {
                Ray ray = new Ray(transform.position + new Vector3(0, sphere.radius / 2, 0), Vector3.down);
                int layerMask = ~(1 << DefaultGameObjectLayer.InVisiblityLayer
                                   | 1 << 26 // GameObjectLayerHelper.CullLOD0
                                   | 1 << 27 // GameObjectLayerHelper.CullLOD1
                                   | 1 << 28 // GameObjectLayerHelper.CullLOD2
                                   | 1 << 14 // GameObjectLayerHelper.CameraBlock
                    );
                if (Physics.Raycast(ray, out var hit, 20, layerMask))
                {
                    var pos = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                    var deltaY = transform.position.y - hit.point.y;
                    var radius = Mathf.Sqrt(sphere.radius * sphere.radius - deltaY * deltaY);
                    gizmosMesh = CreateCircleMesh(radius, Color.green, 0);
                    var c = Gizmos.color;
                    Gizmos.color = gizmosColor;
                    Gizmos.DrawMesh(gizmosMesh, pos,Quaternion.Euler(90,0,0));
                    Gizmos.color = c;
                }
            }
            if(TryGetComponent<BoxCollider>(out var box))
            {
                var squareMesh = CreateSquareMesh(box.size.x / 2, box.size.y / 2);
                var c = Gizmos.color;
                Gizmos.color = gizmosColor;
                Gizmos.DrawMesh(squareMesh, transform.position,transform.rotation);
                Gizmos.color = c;
            }
        }
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects, CustomEditor(typeof(XDummyWall))]
public class XDummyWallEditor : XWallEditor
{
    SerializedProperty m_MonsterFlagForward;
    SerializedProperty m_MonsterFlagBackward;

    SerializedProperty m_PlayerFlagForward;
    SerializedProperty m_PlayerFlagBackward;

    SerializedProperty m_HideFx;

    SerializedProperty permanentFx;
    SerializedProperty autoY;

    SerializedProperty showRange;

    SerializedProperty color;
    SerializedProperty exString;

    SerializedProperty buffIDList;
    SerializedProperty typeList;
    

    public static GUILayoutOption[] ContentLayout = new GUILayoutOption[] { GUILayout.Width(150f) };

    public override void OnEnable()
    {
        base.OnEnable();
        m_MonsterFlagForward = serializedObject.FindProperty("monsterFlag.Forward");
        m_MonsterFlagBackward = serializedObject.FindProperty("monsterFlag.Backward");

        m_PlayerFlagForward = serializedObject.FindProperty("playerFlag.Forward");
        m_PlayerFlagBackward = serializedObject.FindProperty("playerFlag.Backward");

        m_HideFx = serializedObject.FindProperty("HideFx");

        permanentFx = serializedObject.FindProperty("permanentFx");
        autoY = serializedObject.FindProperty("autoY");

        showRange = serializedObject.FindProperty("showRange");
        color = serializedObject.FindProperty("gizmosColor");
        exString = serializedObject.FindProperty("exString");

        buffIDList = serializedObject.FindProperty("buffIDList");
        typeList = serializedObject.FindProperty("typeList");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        var wall = target as XDummyWall;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(" ", ContentLayout);
        EditorGUILayout.LabelField("正面可过", ContentLayout);
        EditorGUILayout.LabelField("反面可过", ContentLayout);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("怪物", ContentLayout);
        m_MonsterFlagForward.boolValue = EditorGUILayout.Toggle(m_MonsterFlagForward.boolValue, ContentLayout);
        m_MonsterFlagBackward.boolValue = EditorGUILayout.Toggle(m_MonsterFlagBackward.boolValue, ContentLayout);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("玩家", ContentLayout);
        m_PlayerFlagForward.boolValue = EditorGUILayout.Toggle(m_PlayerFlagForward.boolValue, ContentLayout);
        m_PlayerFlagBackward.boolValue = EditorGUILayout.Toggle(m_PlayerFlagBackward.boolValue, ContentLayout);
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.PropertyField(m_MonsterFlagForward);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("隐藏特效", ContentLayout);
        m_HideFx.boolValue = EditorGUILayout.Toggle(m_HideFx.boolValue, ContentLayout);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("常驻特效", ContentLayout);
        permanentFx.boolValue = EditorGUILayout.Toggle(permanentFx.boolValue, ContentLayout);
        EditorGUILayout.EndHorizontal();

        if(permanentFx.boolValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("自动贴地", ContentLayout);
            autoY.boolValue = EditorGUILayout.Toggle(autoY.boolValue, ContentLayout);
            EditorGUILayout.EndHorizontal();
        }

        showRange.boolValue = EditorGUILayout.Toggle("显示行走范围",showRange.boolValue,ContentLayout);

        color.colorValue = EditorGUILayout.ColorField("画墙颜色",color.colorValue);

        EditorGUILayout.PropertyField(exString);

        EditorGUILayout.PropertyField(buffIDList);
        EditorGUILayout.PropertyField(typeList);
        

        if(EngineContext.IsRunning)
        {
            if(GUILayout.Button("DynamicAdd",GUILayout.MaxWidth(100)))
            {
                if (wall.TryGetComponent<BoxCollider>(out var box))
                {
                    var trans = wall.transform;
                    var center = box.center;
                    var size = box.size;
                    Vector3 half = Vector3.right * box.size.x * trans.localScale.x * 0.5f;
                    float h = box.size.y * trans.localScale.y * 0.5f;
                    var pos0 = box.center - half;
                    var pos1 = box.center + half;

                    pos0 = trans.localToWorldMatrix * pos0;
                    pos0 += trans.position;
                    pos0.y = trans.position.y - h;
                    pos1 = trans.localToWorldMatrix * pos1;
                    pos1 += trans.position;
                    pos1.y = trans.position.y + h;
                    uint passFlag = 0;
                    if (m_PlayerFlagForward.boolValue)
                        passFlag |= SceneDynamicObject.ForwardPass;
                    if (m_PlayerFlagBackward.boolValue)
                        passFlag |= SceneDynamicObject.BackPass;

                    SceneDynamicObjectSystem.AddDynamicBlock(wall.name, wall.enabled, pos0, pos1, passFlag, "");
                }

             
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    [CanEditMultipleObjects]
    public class ProjectionMeshTool : MonoBehaviour
    {
        
    }

    [CustomEditor(typeof(ProjectionMeshTool), true)]
    [CanEditMultipleObjects]
    public class ProjectionMeshToolEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI()
        {
            ProjectionMeshTool projectionMeshbox = target as ProjectionMeshTool;
            if (GUILayout.Button("Show Box", GUILayout.MaxWidth(120)))
            {
                if(projectionMeshbox.tag != "ProjectionMeshBox")
                    Debug.LogError("Tag Error. It's not a ProjectionMeshBox.");
                else
                {
                    Clear();
                    CreatPlane(projectionMeshbox.gameObject, "bottom", new Vector3(0, -0.5f, 0), new Vector3(0, 90, 0), true);
                    CreatPlane(projectionMeshbox.gameObject, "right", new Vector3(0, 0, 0.5f), new Vector3(90, 0, 0), false);
                    CreatPlane(projectionMeshbox.gameObject, "left", new Vector3(0, 0, -0.5f), new Vector3(90, 0, 0), false);
                    CreatPlane(projectionMeshbox.gameObject, "forward", new Vector3(0.5f, 0, 0), new Vector3(0, 0, 90), false);
                    CreatPlane(projectionMeshbox.gameObject, "back", new Vector3(-0.5f, 0, 0), new Vector3(0, 0, 90), false);
                }
            }
            if (GUILayout.Button("Clear", GUILayout.MaxWidth(120)))
            {
                if (projectionMeshbox.tag != "ProjectionMeshBox")
                    Debug.LogError("Tag Error. It's not a ProjectionMeshBox.");
                else
                    Clear();
            }
        }

        public void CreatPlane(GameObject parent, string name, Vector3 pos, Vector3 rot, bool isBottom)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = pos;
            GameObject plane1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane1.transform.parent = go.transform;
            plane1.transform.localScale = Vector3.one * 0.1f;
            plane1.transform.localPosition = Vector3.zero;
            plane1.transform.localEulerAngles = -rot;
            GameObject plane2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane2.transform.parent = go.transform;
            plane2.transform.localScale = Vector3.one * 0.1f;
            plane2.transform.localPosition = Vector3.zero;
            if (isBottom)
            {
                rot.x = 180;
                plane2.transform.localEulerAngles = rot;
            }
            else
                plane2.transform.localEulerAngles = rot;
        }

        public void Clear()
        {
            ProjectionMeshTool projectionMeshbox = target as ProjectionMeshTool;
            for (int i = projectionMeshbox.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(projectionMeshbox.transform.GetChild(i).gameObject);
        }
    }
}
#endif
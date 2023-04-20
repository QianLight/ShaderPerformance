using UnityEditor;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public class HLODDebugDrawer : MonoBehaviour
    {
        private void Awake()
        {
            this.tag = "EditorOnly";
        }

        private void OnDestroy()
        {
            HLODMessageCenter.SendMessage(HLODMesssages.SCENE_EDITOR_DATA_DESTORYED);
        }

        private void OnDrawGizmos()
        {
            if (!HLODProvider.Instance.data.debug)
                return;
            Bounds bounds = new Bounds();
            bool notset = true;
            foreach (var obj in Selection.objects)
            {
                var go = obj as GameObject;
                if (go != null)
                {
                    var renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (notset)
                        {
                            bounds = r.bounds;
                            notset = false;
                        }
                        else
                        {
                            bounds.Encapsulate(r.bounds);
                        }
                    }
                }
            }

            var color = Gizmos.color;
            Gizmos.color = Color.green;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = color;
        }

        private void OnGUI()
        {
            if (!HLODProvider.Instance.data.debug)
                return;
            Bounds bounds = new Bounds();
            bool notset = true;
            foreach (var obj in Selection.objects)
            {
                var go = obj as GameObject;
                if (go != null)
                {
                    var renderers = go.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        if (notset)
                        {
                            bounds = r.bounds;
                            notset = false;
                        }
                        else
                        {
                            bounds.Encapsulate(r.bounds);
                        }
                    }
                }
            }

            var b = bounds;
            if (Vector3.Distance(Vector3.zero, b.size) > 0 && HLODProvider.Instance.data.debug)
            {
                GUILayout.Label("center:" + b.center + "size:" + b.size);
                HLODDebug.Log("center:" + b.center + "size:" + b.size);
            }
        }
    }
}
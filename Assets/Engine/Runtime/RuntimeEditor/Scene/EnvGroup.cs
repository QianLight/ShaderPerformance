#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class EnvGroup : MonoBehaviour
    {
        public List<EnvArea> areas = new List<EnvArea> ();
        public Vector2 debugScroll = Vector2.zero;

        [System.NonSerialized]
        public static EnvArea currentArea;
        [System.NonSerialized]
        public static EnvArea lastArea;
        public void CollectAreas ()
        {
            EditorCommon.EnumTransform funEnvArea = null;
            funEnvArea = (trans, param) =>
            {
                if (trans.gameObject.activeSelf && trans.gameObject.activeInHierarchy)
                {
                    EnvArea area = trans.GetComponent<EnvArea> ();
                    if (area != null)
                    {
                        var eg = param as EnvGroup;
                        eg.areas.Add (area);
                    }
                    EditorCommon.EnumChildObject (trans, param, funEnvArea);
                }

            };
            areas.Clear ();
            EditorCommon.EnumChildObject (transform, this, (trans, param) => { funEnvArea (trans, param); });
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (EnvGroup))]
    public class EnvGroupEditor : UnityEngineEditor
    {
        enum OpType
        {
            None,
            Refresh,
        }
        public override void OnInspectorGUI ()
        {
            EnvGroup eg = target as EnvGroup;
            OpType opType = OpType.None;
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                opType = OpType.Refresh;
            }
            int count = eg.areas.Count > 10 ? 10 : eg.areas.Count;
            eg.debugScroll = GUILayout.BeginScrollView (eg.debugScroll, GUILayout.MinHeight (count * 20 + 10));
            for (int i = 0; i < eg.areas.Count; ++i)
            {
                GUILayout.BeginHorizontal ();
                var area = eg.areas[i];
                EditorGUILayout.ObjectField (area, typeof (EnvArea), true);
                GUILayout.EndHorizontal ();
            }
            GUILayout.EndScrollView ();

            switch (opType)
            {
                case OpType.Refresh:
                    {
                        eg.CollectAreas ();
                    }
                    break;
            }
            opType = OpType.None;
        }
    }
}
#endif
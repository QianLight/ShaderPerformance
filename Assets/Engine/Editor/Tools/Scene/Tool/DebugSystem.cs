using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityObject = UnityEngine.Object;

namespace CFEngine.Editor
{
    public class DebugSystem : SceneResProcess
    {
        enum OpType
        {
            None,
            OpFindTree,
        }
        private OpType opType = OpType.None;
        private static EditorCommon.EnumTransform findTreeCb = FindTreeCb;
        private static HashSet<Mesh> treeMeshs = new HashSet<Mesh> ();
        public override bool HasGUI { get { return true; } }

        public override void OnGUI (ref SceneContext sceneContext, object param, ref Rect rect)
        {
            //EditorCommon.BeginGroup ("Tree");
            //if (GUILayout.Button ("Find trees", GUILayout.MaxWidth (80)))
            //{
            //    opType = OpType.OpFindTree;
            //}
            //EditorCommon.EndGroup ();
        }

        public override void Update (ref SceneContext sceneContext, object param)
        {

            switch (opType)
            {
                case OpType.OpFindTree:
                    FindTrees (ref sceneContext);
                    break;
            }
            opType = OpType.None;
        }

        protected static void FindTreeCb (Transform trans, object param)
        {
            if (trans.TryGetComponent<MeshRenderer> (out var mr))
            {
                var mat = mr.sharedMaterial;
                if (mat != null && mat.shader != null &&
                    mat.shader.name == "Custom/Scene/TreeLeaf")
                {
                    if (trans.TryGetComponent<MeshFilter> (out var mf))
                    {
                        if (mf.sharedMesh != null)
                            treeMeshs.Add (mf.sharedMesh);
                    }
                }
            }
            else
            {
                EditorCommon.EnumChildObject (trans, param, findTreeCb);
            }
        }

        private void FindTrees (ref SceneContext sceneContext)
        {
            treeMeshs.Clear ();
            EnumTarget (ref sceneContext, null, "StaticPrefabs", findTreeCb);
            DebugLog.AddEngineLog2 ("tree types:{0}", treeMeshs.Count.ToString ());
            foreach (var tm in treeMeshs)
            {
                DebugLog.AddEngineLog2 ("tree :{0}", tm.name);
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine.Editor
{
    enum OpPrefabTest
    {
        OpNone,
        OpRefresh,
        OpTest
    }
    public class PrefabTestTool : EditorWindow, IPrefabLoad
    {
        private PrefabRes pc;
        private Transform testGo;
        private Animator ator;
        private PrefabAsset pa;
        private XGameObject xgo;
        // private AnimatorOverrideController controller = null;
        private RenderComponent renderComponent;
        private OpPrefabTest opType = OpPrefabTest.OpNone;
        private static PrefabTestTool instance;
        public static void Init (PrefabRes pc)
        {
            if (instance == null)
            {
                instance = EditorWindow.GetWindow (typeof (PrefabTestTool), false, "") as PrefabTestTool;
            }
            instance.pc = pc;
            instance.titleContent = new GUIContent (pc.des != null?pc.des.name: "Empty");
            instance.Init ();
            instance.Show ();
        }
        void Init ()
        {
            if (pc != null)
            {
                if (!EngineContext.IsRunning)
                    PrefabConfig.singleton.Init ();
                FindOrCreateGo ();
            }
        }
        public int Layer
        {
            get
            {
                return -1;
            }
        }
        public uint RenderLayerMask
        {
            get
            {
                return uint.MaxValue;
            }
        }
        public bool LoadCb (XGameObject xgo)
        {
            testGo = xgo.Find ("");
            xgo.SetDebugName (string.Format ("Preview_{0}", pc.des.name));
            return true;
        }
        // public ref PrefabInstance GetPrefabInstance (int index)
        // {
        //     return ref pi;
        // }
        private Material CreateMat (Renderer name)
        {
            return null;
        }
        void FindOrCreateGo ()
        {

            if (pc.des != null)
            {
                string prefabName = string.Format ("Preview_{0}", pc.des.name);
                var go = GameObject.Find (prefabName);
                if (go != null)
                {
                    GameObject.DestroyImmediate (go);
                }
                testGo = null;
                ref var cc = ref GameObjectCreateContext.createContext;
                cc.Reset ();
                cc.name = pc.des.name;
                cc.immediate = true;
                cc.cb = LoadCb;
                xgo = XGameObject.CreateXGameObject (ref cc);
                xgo.EndLoad(ref cc);
            }
        }
        void RefreshData () { }

        void Update ()
        {
            if (pc != null)
            {

                switch (opType)
                {
                    case OpPrefabTest.OpRefresh:
                        RefreshData ();
                        break;
                    case OpPrefabTest.OpTest:
                        FindOrCreateGo ();
                        break;
                }
                opType = OpPrefabTest.OpNone;
            }
        }

        void OnGUI ()
        {
            if (pc != null)
            {
                EditorGUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (120)))
                {
                    opType = OpPrefabTest.OpRefresh;
                }
                if (GUILayout.Button ("Test", GUILayout.MaxWidth (80)))
                {
                    opType = OpPrefabTest.OpTest;
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("src", GUILayout.MaxWidth (40));
                EditorGUILayout.ObjectField ("", pc.src, typeof (GameObject), false, GUILayout.MaxWidth (200));
                EditorGUILayout.LabelField ("des", GUILayout.MaxWidth (40));
                EditorGUILayout.ObjectField ("", pc.des, typeof (GameObject), false, GUILayout.MaxWidth (200));
                EditorGUILayout.LabelField ("Preview", GUILayout.MaxWidth (60));
                EditorGUILayout.ObjectField ("", testGo, typeof (Transform), false, GUILayout.MaxWidth (200));
                EditorGUILayout.EndHorizontal ();
            }
        }
    }
}
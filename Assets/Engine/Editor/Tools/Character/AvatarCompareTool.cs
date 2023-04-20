using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class AvatarCompareTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,
            OpCompare,
            OpCompareBond,
        }

        public class BoneBind
        {
            public string name;
            public List<string> bones = new List<string> ();
        }

        public class BonePair
        {
            public bool same;
            public string name0;
            public string name1;
            public string bone0;
            public string bone1;
            public string sameBone;
        }
        private GameObject fbx0;
        private List<BoneBind> bone0 = new List<BoneBind> ();
        private GameObject fbx1;
        private List<BoneBind> bone1 = new List<BoneBind> ();
        private BoneBind compareRender0 = null;
        private BoneBind compareRender1 = null;
        private List<BonePair> bonePair = new List<BonePair> ();
        private bool onlyShowDifferent = true;
        private OpType opType = OpType.OpNone;
        private Vector2 prefabsScroll = Vector2.zero;
        private StringBuilder sb = new StringBuilder ();
        private StringBuilder sb0 = new StringBuilder ();
        private StringBuilder sb1 = new StringBuilder ();
        public override void OnInit ()
        {
            base.OnInit ();

        }

        public override void OnUninit ()
        {
            base.OnUninit ();
        }

        private void BoneGUI (List<BoneBind> bone, bool needCompare)
        {
            for (int i = 0; i < bone.Count; ++i)
            {
                var bb = bone[i];
                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (bb.name, GUILayout.MaxWidth (200));
                if (needCompare)
                {
                    if (GUILayout.Button ("Compare", GUILayout.MaxWidth (80)))
                    {
                        compareRender1 = bb;
                        opType = OpType.OpCompareBond;
                    }
                }

                EditorGUILayout.EndHorizontal ();
            }
        }

        public override void DrawGUI (ref Rect rect)
        {
            EditorGUILayout.BeginHorizontal ();

            EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();
            fbx0 = EditorGUILayout.ObjectField ("fbx0", fbx0, typeof (GameObject), false, GUILayout.MaxWidth (300)) as GameObject;
            EditorGUILayout.EndHorizontal ();
            BoneGUI (bone0, false);
            EditorGUILayout.EndVertical ();

            EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();
            fbx1 = EditorGUILayout.ObjectField ("fbx1", fbx1, typeof (GameObject), false, GUILayout.MaxWidth (300)) as GameObject;
            if (GUILayout.Button ("Compare", GUILayout.MaxWidth (80)))
            {
                opType = OpType.OpCompare;
            }
            EditorGUILayout.EndHorizontal ();
            BoneGUI (bone1, true);
            EditorGUILayout.EndVertical ();

            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.BeginVertical ();
            EditorGUILayout.BeginHorizontal ();
            onlyShowDifferent = EditorGUILayout.Toggle ("OnlyShowDifferent", onlyShowDifferent);
            EditorGUILayout.EndHorizontal ();

            if (bonePair.Count > 0)
            {
                EditorCommon.BeginScroll (ref prefabsScroll, bonePair.Count, 20);
                for (int i = 0; i < bonePair.Count; ++i)
                {
                    var bp = bonePair[i];
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (string.Format ("{0}.{1}", i.ToString (), bp.same? "T": "F"), GUILayout.MaxWidth (60));

                    if (bp.same)
                    {

                        EditorGUILayout.TextField (bp.name0, GUILayout.MaxWidth (200));
                        if (!onlyShowDifferent)
                            EditorGUILayout.TextField (bp.sameBone, GUILayout.MaxWidth (600));
                    }
                    else
                    {
                        EditorGUILayout.TextField (bp.sameBone, GUILayout.MaxWidth (800));
                    }
                    EditorGUILayout.EndHorizontal ();

                    if (!bp.same)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.TextField (bp.name0, GUILayout.MaxWidth (200));
                        EditorGUILayout.TextField (bp.bone0, GUILayout.MaxWidth (600));
                        EditorGUILayout.EndHorizontal ();
                        EditorGUILayout.BeginHorizontal ();
                        EditorGUILayout.TextField (bp.name1, GUILayout.MaxWidth (200));
                        EditorGUILayout.TextField (bp.bone1, GUILayout.MaxWidth (600));
                        EditorGUILayout.EndHorizontal ();
                        EditorGUI.indentLevel--;
                    }

                }
                EditorCommon.EndScroll ();

            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.EndHorizontal ();
        }

        public override void Update ()
        {

            switch (opType)
            {
                case OpType.OpCompare:
                    CompareAvatar ();
                    break;
                case OpType.OpCompareBond:
                    CompareBones ();
                    break;
            }
            opType = OpType.OpNone;
        }

        private void RefreshAvatarBone (GameObject go, List<BoneBind> bone)
        {
            bone.Clear ();
            if (go != null)
            {
                var skins = EditorCommon.GetRenderers (go);
                for (int i = 0; i < skins.Count; ++i)
                {
                    var smr = skins[i] as SkinnedMeshRenderer;
                    if (smr != null)
                    {
                        var bb = new BoneBind ();
                        bb.name = smr.name;
                        for (int j = 0; j < smr.bones.Length; ++j)
                        {
                            bb.bones.Add (EditorCommon.GetSceneObjectPath (smr.bones[j], false));
                        }
                        bone.Add (bb);
                    }
                }
                bone.Sort ((x, y) => x.name.CompareTo (y.name));
            }

        }
        private void CompareAvatar ()
        {
            RefreshAvatarBone (fbx0, bone0);
            RefreshAvatarBone (fbx1, bone1);
        }

        private void CompareBones ()
        {
            compareRender0 = null;
            bonePair.Clear ();
            if (compareRender1 != null)
            {
                for (int i = 0; i < bone0.Count; ++i)
                {
                    var bb = bone0[i];
                    if (bb.name == compareRender1.name)
                    {
                        compareRender0 = bb;
                        break;
                    }
                }

                int count = compareRender0 != null?compareRender0.bones.Count : 0;
                if (compareRender1.bones.Count > count)
                {
                    count = compareRender1.bones.Count;
                }
                for (int i = 0; i < count; ++i)
                {
                    var bp = new BonePair ();
                    bonePair.Add (bp);
                    string bone0 = (compareRender0 != null && i < compareRender0.bones.Count) ? compareRender0.bones[i] : "";
                    string bone1 = i < compareRender1.bones.Count?compareRender1.bones[i]: "";
                    bp.same = bone0 == bone1;
                    int index = bone0.LastIndexOf ("/");
                    if (index >= 0)
                    {
                        bp.name0 = bone0.Substring (index + 1);
                    }
                    else
                    {
                        bp.name0 = bone0;
                    }
                    index = bone1.LastIndexOf ("/");
                    if (index >= 0)
                    {
                        bp.name1 = bone1.Substring (index + 1);
                    }
                    else
                    {
                        bp.name1 = bone1;
                    }
                    if (bp.same)
                    {
                        bp.sameBone = bone0;
                    }
                    else
                    {
                        sb.Clear ();
                        sb0.Clear ();
                        sb1.Clear ();
                        int c = bone0.Length;
                        if (c < bone1.Length)
                            c = bone1.Length;
                        for (int j = 0; j < c; ++j)
                        {
                            var s0 = j < bone0.Length?bone0[j]: ' ';
                            var s1 = j < bone1.Length?bone1[j]: ' ';
                            if (s0 == s1)
                            {
                                sb.Append (s0);
                            }
                            else
                            {
                                sb0.Append (s0);
                                sb1.Append (s1);
                            }
                        }
                        bp.sameBone = sb.ToString ();
                        bp.bone0 = sb0.ToString ();
                        bp.bone1 = sb1.ToString ();

                    }

                }

            }
        }
    }
}
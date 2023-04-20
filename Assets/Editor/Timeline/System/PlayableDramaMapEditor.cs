using UnityEditor;
using UnityEngine;
using System.IO;

namespace XEditor
{
    [CustomEditor(typeof(UIDramaMapAsset))]
    public class PlayableDramaMapEditor : Editor
    {
        UIDramaMapAsset asset;
        Texture head, head1, head2, head3, head4, head5, head6;
        Texture2D tex, tex2;
        AnimationClip clip;

        private void OnEnable()
        {
            asset = target as UIDramaMapAsset;
            if (asset)
            {
                if (head == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas + "/" + asset.sprite + ".png";
                    head = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (head2 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas2 + "/" + asset.sp2 + ".png";
                    head2 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }

                if (head1 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas1 + "/" + asset.sp1 + ".png";
                    head1 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (head3 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas3 + "/" + asset.sp3+ ".png";
                    head3 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (head4 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas4 + "/" + asset.sp4 + ".png";
                    head4 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (head5 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas5 + "/" + asset.sp5 + ".png";
                    head5 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (head6 == null)
                {
                    string pat = "Assets/BundleRes/UI/UISource/" + asset.atlas6 + "/" + asset.sp6 + ".png";
                    head6 = AssetDatabase.LoadAssetAtPath<Texture>(pat);
                }
                if (asset.clip != null)
                {
                    clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BundleRes/" + asset.clip + ".anim");
                }
                if (!string.IsNullOrEmpty(asset.rawTex))
                {
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BundleRes/" + asset.rawTex + ".png");
                }
                if (!string.IsNullOrEmpty(asset.rawTex2))
                {
                    tex2 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BundleRes/" + asset.rawTex2 + ".png");
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            head = (Texture)EditorGUILayout.ObjectField("Head0", head, typeof(Texture), false);
            if (head)
            {
                asset.sprite = head.name;
                var p = AssetDatabase.GetAssetPath(head);
                FileInfo file = new FileInfo(p);
                asset.atlas = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sprite);
                EditorGUILayout.LabelField("atlas: " + asset.atlas);
            }

            head1 = (Texture)EditorGUILayout.ObjectField("Head1", head1, typeof(Texture), false);
            if (head1)
            {
                asset.sp1 = head1.name;
                var p = AssetDatabase.GetAssetPath(head1);
                FileInfo file = new FileInfo(p);
                asset.atlas1 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp1);
                EditorGUILayout.LabelField("atlas: " + asset.atlas1);
            }

            head2 = (Texture)EditorGUILayout.ObjectField("Head2", head2, typeof(Texture), false);
            if (head2)
            {
                asset.sp2 = head2.name;
                var p = AssetDatabase.GetAssetPath(head2);
                FileInfo file = new FileInfo(p);
                asset.atlas2 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp2);
                EditorGUILayout.LabelField("atlas: " + asset.atlas2);
            }

            head3 = (Texture)EditorGUILayout.ObjectField("Head3", head3, typeof(Texture), false);
            if (head3)
            {
                asset.sp3 = head3.name;
                var p = AssetDatabase.GetAssetPath(head3);
                FileInfo file = new FileInfo(p);
                asset.atlas3 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp3);
                EditorGUILayout.LabelField("atlas: " + asset.atlas3);
            }

            head4 = (Texture)EditorGUILayout.ObjectField("Head4", head4, typeof(Texture), false);
            if (head4)
            {
                asset.sp4 = head4.name;
                var p = AssetDatabase.GetAssetPath(head4);
                FileInfo file = new FileInfo(p);
                asset.atlas4 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp4);
                EditorGUILayout.LabelField("atlas: " + asset.atlas4);
            }


            head5 = (Texture)EditorGUILayout.ObjectField("Head5", head5, typeof(Texture), false);
            if (head5)
            {
                asset.sp5 = head5.name;
                var p = AssetDatabase.GetAssetPath(head5);
                FileInfo file = new FileInfo(p);
                asset.atlas5 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp5);
                EditorGUILayout.LabelField("atlas: " + asset.atlas5);
            }

            head6 = (Texture)EditorGUILayout.ObjectField("Head6", head6, typeof(Texture), false);
            if (head6)
            {
                asset.sp6 = head6.name;
                var p = AssetDatabase.GetAssetPath(head6);
                FileInfo file = new FileInfo(p);
                asset.atlas6 = file.Directory.Name;
                EditorGUILayout.LabelField("sprite: " + asset.sp6);
                EditorGUILayout.LabelField("atlas: " + asset.atlas6);
            }

            

            tex = (Texture2D)EditorGUILayout.ObjectField("BG", tex, typeof(Texture2D), false);
            if (tex)
            {
                var p = AssetDatabase.GetAssetPath(tex);
                asset.rawTex = XEditorUtil.GetRelativePath(p);
            }
            EditorGUILayout.LabelField(asset.rawTex);

            tex2 = (Texture2D)EditorGUILayout.ObjectField("BG2", tex2, typeof(Texture2D), false);
            if (tex2)
            {
                var p = AssetDatabase.GetAssetPath(tex2);
                asset.rawTex2 = XEditorUtil.GetRelativePath(p);
            }
            EditorGUILayout.LabelField(asset.rawTex2);

            clip = (AnimationClip)EditorGUILayout.ObjectField("ui Clip", clip, typeof(AnimationClip), false);
            if (clip)
            {
                var p = AssetDatabase.GetAssetPath(clip);
                asset.clip = XEditorUtil.GetRelativePath(p);
            }
            EditorGUILayout.LabelField(asset.clip);
        }

    }

}
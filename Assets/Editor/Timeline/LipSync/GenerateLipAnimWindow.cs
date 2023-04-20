﻿using System.Collections.Generic;
using System.IO;
using LipSync;
using System.Text;
using UnityEditor;
using UnityEngine;


namespace XEditor
{
    public class GenerateLipAnimWindow : EditorWindow
    {
        internal const string menu = @"XEditor/Timeline/LipAnimationGenerateWindow";

        [MenuItem(menu)]
        static void LipAnimExportTool()
        {
            var win = GetWindowWithRect(typeof(GenerateLipAnimWindow), new Rect(0, 0, 400, 360));
            win.titleContent = new GUIContent("generate lip animation", OriginalSetting.Icon);
            win.Show();
        }

        private GameObject boneGo, avatarGo;
        private string bonePath;
        private float duration;
        private int vowel, frames;
        private float slider;
        private AnimationClip sclip;

        string[] blendProperity = { "blendShape1.MTH_A", "blendShape1.MTH_I", "blendShape1.MTH_U", "blendShape1.MTH_E", "blendShape1.MTH_E", "blendShape1.MTH_V" };


        private void OnEnable()
        {
            duration = 5.0f;
        }

        void OnGUI()
        {
            if (EditorApplication.isCompiling) Close();
            GUILayout.Space(4);
            GUILayout.Label("口型曲线生成工具", XEditorUtil.titleLableStyle);
            GUILayout.Space(12);

            boneGo = EditorGUILayout.ObjectField("Bone", boneGo, typeof(GameObject), true) as GameObject;
            if (boneGo != null)
            {
                if (boneGo.GetComponent<SkinnedMeshRenderer>() == null)
                {
                    EditorGUILayout.HelpBox("select bone is invalid, not found SkinedmeshRender attached", MessageType.Error);
                }
                else
                    bonePath = GetRootFullPath(boneGo.transform);
            }
            if (string.IsNullOrEmpty(bonePath))
            {
                bonePath = EditorGUILayout.TextField("bonePath", bonePath);
            }
            else
            {
                EditorGUILayout.LabelField(bonePath);
            }

            EditorGUILayout.LabelField("duration: " + duration);

            EditorGUILayout.LabelField(FmodLipSync.recdPat);
            if (!File.Exists(FmodLipSync.recdPat))
            {
                EditorGUILayout.HelpBox("record file not found, generate at first", MessageType.Error);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Generate"))
            {
                Generate();
            }

            GUILayout.Space(8);
            sclip = EditorGUILayout.ObjectField("Clip", sclip, typeof(AnimationClip), true) as AnimationClip;
            if (sclip)
            {
                avatarGo = EditorGUILayout.ObjectField("Avatar", avatarGo, typeof(GameObject), true) as GameObject;
                slider = GUILayout.HorizontalSlider(slider, 0, 1);
                if (avatarGo) sclip.SampleAnimation(avatarGo, slider * sclip.length);
            }
        }

        private void Generate()
        {
            string tempPath = LipSync.FmodLipSync.recdPat;
            if (File.Exists(tempPath))
            {
                var list = Read(tempPath);
                GenerateAnim(list, duration);
            }
            else if (boneGo == null)
            {
                EditorUtility.DisplayDialog("tip", "bone is not setted", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("tip", "not found " + LipSync.FmodLipSync.recdPat, "ok");
            }
        }

        private List<float[]> Read(string path)
        {
            List<float[]> list = new List<float[]>();
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                StreamReader reader = new StreamReader(fs, Encoding.Unicode);
                string line = reader.ReadLine();
                duration = float.Parse(line);
                string vowels = reader.ReadLine();
                blendProperity = vowels.Split('\t');
                while ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split('\t');
                    int cnt = a.Length - 1;
                    vowel = cnt;
                    float[] f = new float[cnt];
                    for (int i = 0; i < cnt; i++)
                    {
                        f[i] = float.Parse(a[i + 1]);
                    }
                    list.Add(f);
                }
                frames = list.Count;
            }
            return list;
        }

        private void GenerateAnim(List<float[]> values, float duration)
        {
            AnimationClip clip = new AnimationClip();
            clip.legacy = false;
            for (int i = 0; i < vowel; i++)
            {
                AnimationCurve curve = new AnimationCurve();
                for (int j = 0; j < frames; j++)
                {
                    float t = j / (float)frames * duration;
                    float v = values[j][i];

                    // optimus for removing repeated frame
                    if (j > 0 && j < frames - 2) if (values[j - 1][i] == v && v == values[j + 1][i])
                            continue;

                    Keyframe frame = new Keyframe(t, v, 0, 0);
                    curve.AddKey(frame);
                }
                clip.SetCurve(bonePath, typeof(SkinnedMeshRenderer), "blendShape." + blendProperity[i], curve);
            }

            string tempPath = EditorUtility.SaveFilePanel("save", OriginalSetting.resFolder, "recd_.anim", "anim");
            tempPath = tempPath.Substring(Application.dataPath.Length - "Assets".Length);
            AssetDatabase.CreateAsset(clip, tempPath);
            sclip = clip;
            AssetDatabase.Refresh();
        }


        string GetRootFullPath(Transform tf)
        {
            string path = tf.name;
            while (tf.parent != null && tf.name != "root")
            {
                tf = tf.parent;
                if (tf.parent == null) break;
                path = tf.name + "/" + path;
            }
            return path;
        }
    }
}
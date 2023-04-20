using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using CFUtilPoolLib;
using XLevel;

namespace LevelEditor
{
    class LevelWallEditor:EditorWindow
    {
        private XDummyWall wall1;
        private XDummyWall wall2;
        private bool auto;
        private int align1;
        private int align2;
        private float angle;
        private bool wall1Select;
        private string[] AlignmemtPoint = new string[] { "左下角", "左上角", "右下角", "右上角" };

        public static LevelWallEditor InitWallEditor()
        {
            LevelWallEditor editor = GetWindowWithRect<LevelWallEditor>(new Rect(0, 0, 400, 300));
            editor.titleContent = new GUIContent("WallEditor");
            editor.Show();
            return editor;
        }

        public void InitData(XDummyWall wall1, XDummyWall wall2)
        {
            this.wall1 = wall1;
            this.wall2 = wall2;
        }

        private void OnGUI()
        {
            if (wall1 == null || wall2 == null)
                return;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("基准墙",GUILayout.Width(75f));
            auto = EditorGUILayout.ToggleLeft("自动选择对齐点", auto, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            wall1Select = EditorGUILayout.ToggleLeft(wall1.transform.name, wall1Select);
            wall1Select = !EditorGUILayout.ToggleLeft(wall2.transform.name, !wall1Select);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(string.Format("{0}的对齐点", wall1.transform.name));
            EditorGUILayout.BeginHorizontal();
            for(var i=0;i<AlignmemtPoint.Length;i++)
            {
                align1 = EditorGUILayout.ToggleLeft(AlignmemtPoint[i], align1 == i) ? i : align1;
                if(i==1)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(string.Format("{0}的对齐点", wall2.transform.name));
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < AlignmemtPoint.Length; i++)
            {
                align2 = EditorGUILayout.ToggleLeft(AlignmemtPoint[i], align2 == i) ? i : align2;
                if (i == 1)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("墙的夹角",GUILayout.Width(75f));
            angle = EditorGUILayout.FloatField(angle, GUILayout.Width(75f));
            if (GUILayout.Button("Apply", GUILayout.Width(75f)))
            {
                if (auto)
                    CalculateAlign();
                CombineWall();
                ShowNotification(new GUIContent("应用成功！"), 3);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CalculateAlign()
        {
            wall1.TryGetComponent<BoxCollider>(out var box1);
            wall2.TryGetComponent<BoxCollider>(out var box2);
            float len1 = box1.size.x * wall1.transform.localScale.x / 2;
            float len2 = box2.size.x * wall2.transform.localScale.x / 2;
            float hei1 = box1.size.y * wall1.transform.localScale.y / 2;
            float hei2 = box2.size.y * wall2.transform.localScale.y / 2;
            float min = float.MaxValue;
            int idx1 = 0, idx2 = 0;
            for(var i=0;i<4;i++)
            {
                Vector3 point1 = new Vector3(i < 2 ? -len1 : len1, (i % 2 == 0) ? -hei1 : hei1, 0) + wall1.transform.position;
                for(var j=0;j<4;j++)
                {
                    Vector3 point2 = new Vector3(j < 2 ? -len2 : len2, (j % 2 == 0) ? -hei2 : hei2, 0) + wall2.transform.position;
                    float dis = Vector3.Distance(point1, point2);
                    if(dis<=min)
                    {
                        idx1 = i;
                        idx2 = j;
                        min = dis;
                    }
                }
            }
            align1 = idx1;
            align2 = idx2;
        }

        private void CombineWall()
        {
            XDummyWall wall_1 = wall1Select ? wall1 : wall2;
            XDummyWall wall_2 = wall1Select ? wall2 : wall1;
            int align_1 = wall1Select ? align1 : align2;
            int align_2 = wall1Select ? align2 : align1;
            wall_1.TryGetComponent<BoxCollider>(out var box1);
            wall_2.TryGetComponent<BoxCollider>(out var box2);
            float len1 = box1.size.x * wall_1.transform.localScale.x / 2;
            float len2 = box2.size.x * wall_2.transform.localScale.x / 2;
            float hei1 = box1.size.y * wall_1.transform.localScale.y / 2;
            float hei2 = box2.size.y * wall_2.transform.localScale.y / 2;
            Vector3 point1 = new Vector3(align_1 < 2 ? -len1 : len1, (align_1 % 2 == 0) ? -hei1 : hei1, 0);
            Vector3 point2 = new Vector3((align_2 < 2 ? -len2 : len2) * Mathf.Cos(Mathf.Deg2Rad*angle),
                (align_2 % 2 == 0) ? -hei2 : hei2, 
                (align_2 < 2 ? -len2 : len2) * Mathf.Sin(Mathf.Deg2Rad * angle));
            Vector3 finalPos = wall_1.transform.localToWorldMatrix.MultiplyPoint(point1 - point2);          
            wall_2.transform.rotation = Quaternion.Euler(new Vector3(0, -angle, 0) + wall_1.transform.eulerAngles);
            wall_2.transform.position = finalPos;
        }
    }
}

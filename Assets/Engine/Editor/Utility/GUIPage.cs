using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GUIPage<T>
{
    public string title;
    public SavedInt page;
    public SavedInt lineCount;
    public SavedVector2 viewPosition;
    public Action<IList<T>, T, int> gui;

    public GUIPage(string saveName, string title, Action<IList<T>, T, int> gui)
    {
        this.title = title;
        this.gui = gui;

        page = new SavedInt($"{saveName}.{nameof(page)}");
        lineCount = new SavedInt($"{saveName}.{nameof(lineCount)}", 20);
        viewPosition = new SavedVector2($"{saveName}.{nameof(viewPosition)}");
    }

    public void Draw(IList<T> data, params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(options);

        if (!string.IsNullOrEmpty(title))
            GUILayout.Box(title, GUILayout.ExpandWidth(true));

        if (data == null || data.Count == 0)
        {
            EditorGUILayout.LabelField($"<Color=FFFFFF40>暂无数据</Color>",
                new GUIStyle() {alignment = TextAnchor.MiddleCenter, richText = true, fontSize = 28}, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
        }
        else if (gui == null)
        {
            GUILayout.Box("项目绘制回调丢失", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        }
        else
        {
            viewPosition.Value = GUILayout.BeginScrollView(viewPosition.Value);
            for (int i = page.Value * lineCount.Value; i >= 0 && i < Mathf.Min((page.Value + 1) * lineCount.Value,  data.Count); i++)
            {
                T material = data[i];
                gui?.Invoke(data, material, i);
            }
            GUILayout.EndScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(page.Value <= 0);
            if (GUILayout.Button("上一页", GUILayout.MaxWidth(80)))
            {
                page.Value--;
            }
            EditorGUI.EndDisabledGroup();

            int maxPageCount = Mathf.Max(0, data.Count % lineCount.Value == 0 ? data.Count / lineCount.Value - 1 : data.Count / lineCount.Value);
            page.Value = EditorGUILayout.IntSlider(page.Value, 0, maxPageCount);

            GUILayout.Label(maxPageCount.ToString(), GUILayout.Width(30));

            EditorGUI.BeginDisabledGroup(page.Value >= maxPageCount);
            if (GUILayout.Button("下一页", GUILayout.MaxWidth(80)))
            {
                page.Value++;
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Label("显示行数", GUILayout.Width(54));
            lineCount.Value = EditorGUILayout.IntField(lineCount.Value, GUILayout.Width(25));
            lineCount.Value = Mathf.Clamp(lineCount.Value, 5, 50);

            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}

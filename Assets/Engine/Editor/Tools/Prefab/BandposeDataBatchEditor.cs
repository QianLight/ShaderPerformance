using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class BandposeDataBatchEditor : EditorWindow
    {
        public const string NAME = "bandpose批量处理";
        public static readonly SavedInt selectedCount = new SavedInt($"{nameof(BandposeDataBatchEditor)}.{nameof(selectedCount)}");
        public static readonly SavedVector2 viewPos = new SavedVector2($"{nameof(BandposeDataBatchEditor)}.{nameof(viewPos)}");
        public static List<BandposeData> list;

        private static HeightGradient heightGradient = new HeightGradient();
        private static SavedBool refreshHeightGradient =
            new SavedBool($"{nameof(BandposeDataBatchEditor)}.{nameof(refreshHeightGradient)}");
            
        public static void Open()
        {
            BandposeDataBatchEditor window = GetWindow<BandposeDataBatchEditor>();
            window.titleContent = new GUIContent(NAME);
            window.Show();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += Repaint;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
        }

        private void OnGUI()
        {
            if (list == null)
            {
                list = new List<BandposeData>(selectedCount.Value);
                for (int i = 0; i < selectedCount.Value; i++)
                {
                    string guid = EditorPrefs.GetString($"{nameof(BandposeDataBatchEditor)}.{nameof(list)}[{i}]");
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    list.Add(AssetDatabase.LoadAssetAtPath<BandposeData>(path));
                }
            }

            EditorGUI.BeginChangeCheck();

            var temp = Selection.objects;
            List<BandposeData> selections = new List<BandposeData>();
            for (int i = 0; i < temp.Length; i++)
            {
                var s = temp[i] as BandposeData;
                if (s)
                    selections.Add(s);
            }

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(selections.Count == 0);
            if (GUILayout.Button("添加选中", GUILayout.MaxWidth(100)))
            {
                foreach (var selection in selections)
                {
                    int index = list.IndexOf(selection);
                    if (index < 0)
                        list.Add(selection);
                }
            }
            if (GUILayout.Button("移除选中", GUILayout.MaxWidth(100)))
            {
                foreach (var selection in selections)
                {
                    int index = list.IndexOf(selection);
                    if (index >= 0)
                        list.RemoveAt(index);
                }
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("清空所有", GUILayout.MaxWidth(100)))
            {
                list.Clear();
            }
            GUILayout.EndHorizontal();

            viewPos.Value = GUILayout.BeginScrollView(viewPos.Value, "box");
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), GUILayout.Width(50));
                EditorGUILayout.ObjectField(list[i], typeof(BandposeData), false);
                if (GUILayout.Button("-", GUILayout.Width(23)))
                {
                    list.RemoveAt(i--);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
            
            refreshHeightGradient.Value = EditorGUILayout.Toggle("刷高度渐变", refreshHeightGradient.Value);
            if (refreshHeightGradient.Value)
            {
                BandposeDataEditor.DrawHeightGradient(heightGradient);
            }
            EditorGUI.BeginDisabledGroup(list.Count == 0);
            if (GUILayout.Button($"onekey ({list.Count})")
                && EditorUtility.DisplayDialog("批量onekey", $"要现在处理{list.Count}个bandpose的onekey吗？", "确定", "取消"))
            {
                foreach (BandposeData bandpose in list)
                {
                    if (refreshHeightGradient.Value)
                    {
                        heightGradient.CopyTo(bandpose.gradient);
                        EditorUtility.SetDirty(bandpose);
                        bandpose.SetAllPrefabDirty(true);
                    }
                    BandposeDataEditor.Onekey(bandpose, true);
                }
                EditorUtility.DisplayDialog($"bandpose批量处理工具", $"处理{list.Count}个资源完成，看Console视图检查有没有出错。", "OK");
            }

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                selectedCount.Value = list.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    string path = AssetDatabase.GetAssetPath(list[i]);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    EditorPrefs.SetString($"{nameof(BandposeDataBatchEditor)}.{nameof(list)}[{i}]", guid);
                }
            }
        }
    }
}

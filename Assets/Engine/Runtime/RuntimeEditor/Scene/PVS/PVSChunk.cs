#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [Serializable]
    public class PVSMidCell
    {
        public int x;
        public int z;
        public HashSet<int> yIndex = new HashSet<int>();
    }

    [Serializable]
    public class PVSResultCell
    {
        public Vector3Int cellindex;
        public List<PVSCollider> result;
    }
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class PVSChunk : MonoBehaviour
    {
        public bool drawVisibleObject = false;
        public int drawCellIndex = -1;
        public int x;
        public int z;
        public float xOffset;
        public float zOffset;
        public List<PVSResultCell> cells = new List<PVSResultCell>();
        private Dictionary<int, PVSMidCell> cellMap = new Dictionary<int, PVSMidCell>();
        public bool debug;
        [Range(0, 512)]
        public int sampleNum = 0;
        public static int gizmosCellSize = 0;
        public void Clear()
        {
            cells.Clear();
            cellMap.Clear();
        }

        public void Add(float cellSize, int index, ref Vector3 pos, int x, int z)
        {
            int y = Mathf.CeilToInt(pos.y / cellSize);
            if (cellMap.TryGetValue(index, out var midCell))
            {
                if (!midCell.yIndex.Contains(y))
                    midCell.yIndex.Add(y);
            }
            else
            {               
                midCell = new PVSMidCell()
                {
                    x = x,
                    z = z,
                };
                midCell.yIndex.Add(y);
                //Vector3 cellPos = new Vector3(x * cellSize + halfCellSize,
                //    pos.y,
                //    z * cellSize + halfCellSize);
                //midCell.cellAABB.Init(cellPos, new Vector3(cellSize, cellSize, cellSize));
                cellMap.Add(index, midCell);
            }
        }

        public void PostProcess()
        {
            cells.Clear();
            foreach (var cell in cellMap.Values)
            {
                foreach(var y in cell.yIndex)
                {
                    cells.Add(
                        new PVSResultCell()
                        {
                            cellindex = new Vector3Int(cell.x, y, cell.z)
                        });
                }
            }
            cellMap.Clear();
        }
        private void DrawCell(PVSResultCell cell, ref Vector3 size, float cellSize, float halfCellSize, bool wire = true)
        {
            Vector3 cellPos = new Vector3();
            cellPos.x = cell.cellindex.x * cellSize + halfCellSize + xOffset;
            cellPos.y = cell.cellindex.y * cellSize + halfCellSize;
            cellPos.z = cell.cellindex.z * cellSize + halfCellSize + zOffset;
            Gizmos.DrawWireCube(cellPos, size);
            if (!wire)
            {
                Gizmos.DrawCube(cellPos, size);
            }            
        }

        public void OnDrawGizmosSelected()
        {
            if (gizmosCellSize > 0)
            {
                Vector3 size = new Vector3(gizmosCellSize, gizmosCellSize*2f, gizmosCellSize);

                float halfCellSize = gizmosCellSize / 2;
                if (drawCellIndex >= 0 && drawCellIndex < cells.Count)
                {
                    var cell = cells[drawCellIndex];
                    Gizmos.color = new Color(1, 1, 0, 0.4f);
                    DrawCell(cell, ref size, gizmosCellSize, halfCellSize, false);
                    if (drawVisibleObject && cell.result != null)
                    {
                        foreach (var r in cell.result)
                        {
                            r.DrawGizmos();
                        }
                    }
                }
                else
                {
                    Gizmos.color = Color.blue;

                    foreach (var cell in cells)
                    {
                        DrawCell(cell, ref size, gizmosCellSize, halfCellSize);
                    }
                }
            }
        }
    }

    [CanEditMultipleObjects, CustomEditor(typeof(PVSChunk))]
    public class PVSChunkEditor : UnityEngineEditor
    {
        SerializedProperty drawCellIndex;
        SerializedProperty drawVisibleObject;
        SerializedProperty debug;
        SerializedProperty sampleNum;
        Vector2 scroll = Vector2.zero;

        void OnEnable()
        {
            debug = serializedObject.FindProperty("debug");
            drawCellIndex = serializedObject.FindProperty("drawCellIndex");
            drawVisibleObject = serializedObject.FindProperty("drawVisibleObject");
            sampleNum = serializedObject.FindProperty("sampleNum");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PVSChunk pc = target as PVSChunk;
            EditorGUILayout.PropertyField(debug);
            EditorGUILayout.PropertyField(sampleNum);
            EditorGUILayout.FloatField("xOffset", pc.xOffset);
            EditorGUILayout.FloatField("zOffset", pc.zOffset);
            EditorGUILayout.LabelField(string.Format("CellCount:{0}", pc.cells.Count.ToString()));
            EditorGUILayout.PropertyField(drawCellIndex);
            if (drawCellIndex.intValue >= 0 && drawCellIndex.intValue < pc.cells.Count)
            {
                var cell = pc.cells[drawCellIndex.intValue];
                EditorGUILayout.Vector3IntField("Index", cell.cellindex);
                if (cell.result != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.Format("VisibleCount:{0}", cell.result.Count.ToString()));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(drawVisibleObject);
                    EditorCommon.BeginScroll(ref scroll, cell.result.Count, 20);                    
                    foreach(var result in cell.result)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField("", result, typeof(PVSCollider), true);
                        EditorGUILayout.EndHorizontal();
                    }                    
                    EditorCommon.EndScroll();
                    EditorGUI.indentLevel--;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
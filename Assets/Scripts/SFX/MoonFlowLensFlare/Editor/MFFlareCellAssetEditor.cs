using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MFFlareAssetCell))]
public class MFFlareCellAssetEditor : Editor
{
    private MFFlareAssetCell _targetAssetCell;
    private MFFlareCellAssetEditor _ins;
    private List<bool> _tablelist;

    private void Awake()
    {
        _targetAssetCell = target as MFFlareAssetCell;
        _tablelist = new List<bool>();
    }

    private Rect GetFlareRect(int index)
    {
        return new Rect(
            (float)index % _targetAssetCell.modelCell.x / _targetAssetCell.modelCell.x,
            (float)(_targetAssetCell.modelCell.y - index / _targetAssetCell.modelCell.y - 1)/_targetAssetCell.modelCell.y,
            (float) 1 / _targetAssetCell.modelCell.x,
            (float) 1 / _targetAssetCell.modelCell.y);
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Save"))
        {
            AssetDatabase.SaveAssets();
        }
        EditorGUI.BeginChangeCheck();
        
        _targetAssetCell.fadeWithScale = EditorGUILayout.Toggle("Fade With Scale", _targetAssetCell.fadeWithScale);
        _targetAssetCell.fadeWithAlpha = EditorGUILayout.Toggle("Fade With Alpha", _targetAssetCell.fadeWithAlpha);
        PaintSplitType();
        PaintTable();

        int cellCount = _targetAssetCell.modelCell.x * _targetAssetCell.modelCell.y;

        for (int i = 0; i < _tablelist.Count; i++)
        {
            if (_tablelist[i])
            {
                _targetAssetCell.spriteBlocks.Add(new MFFlareSpriteData()
                {
                    useLightColor = 0,
                    useRotation = false,
                    index = i,
                    block = GetFlareRect(i),
                    scale = 1,
                    offset = 0,
                    color = Color.white
                });
            }
        }
        
        for (int i = 0; i < _targetAssetCell.spriteBlocks.Count;)
        {
            EditorGUILayout.Space(5);
            MFFlareSpriteData data = _targetAssetCell.spriteBlocks[i];
            Rect t = EditorGUILayout.BeginHorizontal(); 
            EditorGUILayout.LabelField(" ",new[] {GUILayout.Height(60), GUILayout.Width(60)});
            EditorGUILayout.BeginVertical();
            data.index = Mathf.Clamp(data.index, 0, cellCount - 1);
            data.block = GetFlareRect(data.index);
            GUI.DrawTextureWithTexCoords(new Rect(t.position + new Vector2(0,30 * (1- data.block.height/data.block.width)),new Vector2(60,60 * data.block.height / data.block.width)), _targetAssetCell.flareSprite, data.block);
            data.index = EditorGUILayout.IntSlider("Index", data.index, 0, cellCount - 1);
            data.useRotation = EditorGUILayout.Toggle("Rotation", data.useRotation);
            data.useLightColor = EditorGUILayout.Slider("LightColor", data.useLightColor, 0, 1);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            data.offset = EditorGUILayout.Slider("Offset",data.offset, -1.5f, 1f);
            data.color = EditorGUILayout.ColorField("Color", data.color);
            data.scale = EditorGUILayout.FloatField("Scale", data.scale);

            if (GUILayout.Button("Remove"))
            {
                _targetAssetCell.spriteBlocks.RemoveAt(i);
            }
            else
            {
                _targetAssetCell.spriteBlocks[i] = data;
                i++;
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_targetAssetCell);
        }
        Undo.RecordObject(_targetAssetCell, "Change Flare Asset Data");
    }

    public void PaintSplitType()
    {
        
        _targetAssetCell.flareSprite = (Texture2D)EditorGUILayout.ObjectField("Texture", _targetAssetCell.flareSprite, typeof(Texture2D),true);
        _targetAssetCell.modelCell = EditorGUILayout.Vector2IntField("Cell num", _targetAssetCell.modelCell);
    }
    public void PaintTable()
    {
        if(_tablelist!=null)_tablelist.Clear();
        if(_targetAssetCell.spriteBlocks == null)_targetAssetCell.spriteBlocks = new List<MFFlareSpriteData>();
        EditorGUILayout.BeginVertical();
        for (int i = 0; i < _targetAssetCell.modelCell.y; i++)
        {
            Rect r = (Rect)EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < _targetAssetCell.modelCell.x; j++)
            {
                _tablelist.Add(GUILayout.Button((i*_targetAssetCell.modelCell.x + j + 1).ToString("00"), new[] {GUILayout.Height(60), GUILayout.Width(60)}));
            }
            EditorGUILayout.EndHorizontal();
            if (_targetAssetCell.flareSprite)
            {
                for (int j = 0; j < _targetAssetCell.modelCell.x; j++)
                {
                    GUI.DrawTextureWithTexCoords(new Rect(r.position.x + j * 63, r.position.y, 60,60), _targetAssetCell.flareSprite, GetFlareRect(i * _targetAssetCell.modelCell.x + j));
                }
            }
        }
        EditorGUILayout.EndVertical();
    }
}

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace TDTools.ExplorationEventEditor {



    [CustomEditor(typeof(ExplorationEventHelper))]
    public class ExplorationEventHelperCustomEditor : Editor {

        ExplorationEventHelper _owner;

        readonly int[] TRIGGER_OPTIONS = { 0, 1, 2, 3 };
        readonly string[] TRIGGER_TEXT = {"区域靠近激活", "登岛登船激活", "船岛出生激活", "前置依赖激活"};


        void OnEnable() {

            try {
                _owner = (ExplorationEventHelper)target;
            } catch { 
            }
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField($"表格第{_owner.Row}行");
            _owner.Comment = EditorGUILayout.TextField("注释", _owner.Comment);
            _owner.ID = EditorGUILayout.IntField("事件ID:", _owner.ID);
            _owner.AreaID = EditorGUILayout.IntField("海域ID:", _owner.AreaID);
            _owner.Radius = EditorGUILayout.FloatField("触发范围", _owner.Radius);
            _owner.EntityBinded = EditorGUILayout.TextField("绑定船ID", _owner.EntityBinded);
            _owner.Island = EditorGUILayout.TextField("绑定岛屿",_owner.Island);
            _owner.Trigger = (ExplorationEventHelper.TriggerType)EditorGUILayout.IntPopup("触发规则", (int)_owner.Trigger, TRIGGER_TEXT, TRIGGER_OPTIONS);
            _owner.Script = EditorGUILayout.TextField("脚本", _owner.Script);
            _owner.Effect = EditorGUILayout.TextField("效果", _owner.Effect);


            if (_owner.UnsupportedColumn.Count > 0) {
                EditorGUILayout.Separator();
                List<string> keys = new List<string>();
                foreach (var pair in _owner.UnsupportedColumn) {
                    keys.Add(pair.Key);
                }
                EditorGUILayout.LabelField($"编辑器不支持的表头");
                foreach (var key in keys) {
                    _owner.UnsupportedColumn[key] = EditorGUILayout.TextField(key, _owner.UnsupportedColumn[key]);
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("以下参数决定事件在存入表时是否会覆盖已有的某一行");
            EditorGUILayout.LabelField("在新建,读取或保存入表格时应该会自动设置这些参数");
            EditorGUILayout.LabelField("只有在复制现有事件时需要手动设定");
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("表内行数代表哪一行会被覆盖,从0行开始，不计表头和注释");
            _owner.Row = EditorGUILayout.IntField("表内行数", _owner.Row);
            EditorGUILayout.LabelField("如果新行被勾选，则无视上一个数值，不会覆盖任何数据，必定新起一行");
            _owner.IsNew = EditorGUILayout.Toggle("是否新行", _owner.IsNew);
        }
    }
}

#endif
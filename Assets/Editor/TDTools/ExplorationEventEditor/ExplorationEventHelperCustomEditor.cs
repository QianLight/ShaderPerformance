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
        readonly string[] TRIGGER_TEXT = {"���򿿽�����", "�ǵ��Ǵ�����", "������������", "ǰ����������"};


        void OnEnable() {

            try {
                _owner = (ExplorationEventHelper)target;
            } catch { 
            }
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField($"����{_owner.Row}��");
            _owner.Comment = EditorGUILayout.TextField("ע��", _owner.Comment);
            _owner.ID = EditorGUILayout.IntField("�¼�ID:", _owner.ID);
            _owner.AreaID = EditorGUILayout.IntField("����ID:", _owner.AreaID);
            _owner.Radius = EditorGUILayout.FloatField("������Χ", _owner.Radius);
            _owner.EntityBinded = EditorGUILayout.TextField("�󶨴�ID", _owner.EntityBinded);
            _owner.Island = EditorGUILayout.TextField("�󶨵���",_owner.Island);
            _owner.Trigger = (ExplorationEventHelper.TriggerType)EditorGUILayout.IntPopup("��������", (int)_owner.Trigger, TRIGGER_TEXT, TRIGGER_OPTIONS);
            _owner.Script = EditorGUILayout.TextField("�ű�", _owner.Script);
            _owner.Effect = EditorGUILayout.TextField("Ч��", _owner.Effect);


            if (_owner.UnsupportedColumn.Count > 0) {
                EditorGUILayout.Separator();
                List<string> keys = new List<string>();
                foreach (var pair in _owner.UnsupportedColumn) {
                    keys.Add(pair.Key);
                }
                EditorGUILayout.LabelField($"�༭����֧�ֵı�ͷ");
                foreach (var key in keys) {
                    _owner.UnsupportedColumn[key] = EditorGUILayout.TextField(key, _owner.UnsupportedColumn[key]);
                }
            }

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("���²��������¼��ڴ����ʱ�Ƿ�Ḳ�����е�ĳһ��");
            EditorGUILayout.LabelField("���½�,��ȡ�򱣴�����ʱӦ�û��Զ�������Щ����");
            EditorGUILayout.LabelField("ֻ���ڸ��������¼�ʱ��Ҫ�ֶ��趨");
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("��������������һ�лᱻ����,��0�п�ʼ�����Ʊ�ͷ��ע��");
            _owner.Row = EditorGUILayout.IntField("��������", _owner.Row);
            EditorGUILayout.LabelField("������б���ѡ����������һ����ֵ�����Ḳ���κ����ݣ��ض�����һ��");
            _owner.IsNew = EditorGUILayout.Toggle("�Ƿ�����", _owner.IsNew);
        }
    }
}

#endif
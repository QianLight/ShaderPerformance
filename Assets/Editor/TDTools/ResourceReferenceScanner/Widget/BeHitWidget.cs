using CFEngine;
using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class BeHitWidget : InspectorElement {
        public BeHitWidget(ResourceReferenceInspector owner, Vector2 position, string path) : base(owner, position, path) {
            InspectorNodeType = NodeType.BeHit;
        }

        public override void BuildContent() {
            base.BuildContent();
            string BeHitName = ID.Substring(ID.LastIndexOf('/') + 1);
            TitleLabel.text = $"�ܻ�: {BeHitName}";
            Label labelRow = new Label($"�ܻ�Ŀǰ��û��ʲô������ʾ��");
            Container.Add(labelRow);
            labelRow = new Label($"�Ҽ��˵����ܻ��༭���д�");
            Container.Add(labelRow);
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("���ܻ��༭���д�"), false, () => {
                var editor = EditorWindow.GetWindow<BehitEditor>();
                editor.OpenFile($"{Application.dataPath}/{ID}.bytes");
            });
            return menu;
        }
    }
}
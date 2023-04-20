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
            TitleLabel.text = $"受击: {BeHitName}";
            Label labelRow = new Label($"受击目前并没有什么可以显示的");
            Container.Add(labelRow);
            labelRow = new Label($"右键菜单从受击编辑器中打开");
            Container.Add(labelRow);
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("在受击编辑器中打开"), false, () => {
                var editor = EditorWindow.GetWindow<BehitEditor>();
                editor.OpenFile($"{Application.dataPath}/{ID}.bytes");
            });
            return menu;
        }
    }
}
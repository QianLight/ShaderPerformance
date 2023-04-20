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
    public class AssetWidget : InspectorElement {
        public AssetWidget(ResourceReferenceInspector owner, Vector2 position, string assetPath) : base(owner, position, assetPath) {
            InspectorNodeType = NodeType.Asset;
        }

        public override void BuildContent() {
            base.BuildContent();
            TitleLabel.text = ID;
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"Assets/{ID}");
            if (obj == null)
                return;
            var _gameObjectEditor = Editor.CreateEditor(obj);

            if (_gameObjectEditor == null)
                return;

            IMGUIContainer preview = new IMGUIContainer();
            preview.onGUIHandler += () => {
                GUIStyle style = new GUIStyle(GUI.skin.window);
                _gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(512, 512), style);
            };
            Container.Add(preview);
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt) {
            var menu = base.ContextClick(evt);
            menu.AddItem(new GUIContent("在Unity中定位到文件位置"), false, () => {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"Assets/{ID}");
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            });

            menu.AddItem(new GUIContent("在文件浏览器中定位这个文件"), false, () => {
                string filePath = $@"{Application.dataPath}/{ID}".Replace('/', '\\');
                if (!File.Exists(filePath)) {
                    Debug.Log("文件不存在");
                    return;
                }

                string argument = "/select, \"" + filePath + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            });
            return menu;
        }
    }
}
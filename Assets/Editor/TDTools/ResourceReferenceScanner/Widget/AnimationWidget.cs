using CFEngine;
using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class AnimationWidget : AssetWidget {
        [SerializeField]
        string prefabPath;
        string animationName;

        public AnimationWidget(ResourceReferenceInspector owner, Vector2 position, string animationPath) : base(owner, position, animationPath) {
            InspectorNodeType = NodeType.Animation;
        }

        public override void BuildContent() {
            string[] s = ID.Split('=');
            prefabPath = s[1];
            animationName = s[0].Substring(s[0].LastIndexOf("/") + 1, s[0].LastIndexOf(".") - s[0].LastIndexOf("/") - 1);
            AnimationClip anim = AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/{s[0]}");
            TitleLabel.text = s[0];
            object[] para = new object[1];
            para[0] = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>($"Assets/{prefabPath}");

            if (anim == null)
                return;
            var _gameObjectEditor = Editor.CreateEditor(anim, Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.AnimationClipEditor", true));
            _gameObjectEditor.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_gameObjectEditor, null);
            var avatarPreview = _gameObjectEditor.GetPrivateField("m_AvatarPreview");
            avatarPreview.GetType().GetMethod("SetPreview", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(avatarPreview, para);

            if (_gameObjectEditor == null)
                return;



            IMGUIContainer preview = new IMGUIContainer();
            preview.onGUIHandler += () => {
                GUIStyle style = new GUIStyle(GUI.skin.window);
                try {
                    _gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(512, 512), style);
                    preview.MarkDirtyRepaint();
                } catch {
                }
            };
            Container.Add(preview);
        }

        protected override GenericMenu ContextClick(ContextClickEvent evt)
        {
            var menu = base.ContextClick(evt);
#if USE_GM
            menu.AddItem(new GUIContent($"GMÖ¸Áî: idleanim {animationName}"), false, () => {
                CFClient.CFCommand.singleton.ProcessClientCommand($"idleanim {animationName}");
            });

            menu.AddItem(new GUIContent($"GMÖ¸Áî: sceneanim {animationName}"), false, () => {
                CFClient.CFCommand.singleton.ProcessClientCommand($"sceneanim {animationName}");
            });
#endif
            return menu;
        }
    }
}
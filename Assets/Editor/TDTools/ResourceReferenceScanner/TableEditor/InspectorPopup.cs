using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace TDTools.ResourceScanner {
    public class InspectorPopup : BaseInspectorField {
        string _id;

        public Func<bool> Verify;

        PopupField<string> _popup;
        List<string> _values;

        public InspectorPopup(InspectorElement owner, string titleText, string currnet, List<string> options, List<string> values, string tooltip) :base(currnet) {
            _id = currnet;
            _values = values;

            Root = new VisualElement();
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.justifyContent = Justify.SpaceBetween;
            SetBorderColor(Color.gray);
            Label title = new Label($"{titleText}:\t");
            int index = 0;
            for (int i = 0; i < values.Count; i++) {
                if (values[i].Equals(currnet)) {
                    index = i;
                    break;
                }
            }
            _popup = new PopupField<string>(options, index);
            title.style.unityTextAlign = TextAnchor.MiddleLeft;

            VisualElement ve = new VisualElement();
            ve.style.flexDirection = FlexDirection.Row;

            Root.Add(title);
            ve.Add(_popup);

            if (Verify != null) {
                Image image = new Image();
                image.image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Tools/AmplifyShaderEditor/Plugins/EditorResources/UI/Buttons/Checkmark.png");
                image.style.flexGrow = 0;
                image.style.flexShrink = 0;
                image.style.height = 12;
                image.style.width = 12;
                image.style.marginTop = 4;
                image.style.marginBottom = 4;
                image.tintColor = Color.green;
                ve.Add(image);
            }

            Root.Add(ve);

            Root.RegisterCallback<MouseEnterEvent>(e => {
                ContextToolTip.Instance.SetContent(new Label(tooltip));
                SetBorderColor(Color.yellow);
                owner.CurrnetMouseOver = _id;
                owner.CurrentColumn = titleText;
                ContextToolTip.Instance.Show();
            });

            Root.RegisterCallback<MouseLeaveEvent>(e => {
                SetBorderColor(Color.gray);
                owner.CurrnetMouseOver = "";
                ContextToolTip.Instance.Hide();
            });
        }

        void SetBorderColor(Color color) {
            Root.style.borderTopWidth = 1f;
            Root.style.borderTopColor = color;
            Root.style.borderBottomWidth = 1f;
            Root.style.borderBottomColor = color;
        }

        public override string GetValue() {
            return _values[_popup.index];
        }
    }
}
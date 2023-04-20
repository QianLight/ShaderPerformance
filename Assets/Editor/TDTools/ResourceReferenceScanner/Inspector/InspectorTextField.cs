using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class InspectorTextfield : BaseInspectorField {
        string _id;

        readonly Func<string, string> Verify;
        readonly Func<string, List<string>> Hint;

        private TextField _textField;

        public InspectorTextfield(InspectorElement owner, string titleText, string valueText, string tooltip, Func<string, string> verify = null, Func<string, List<string>> hint = null) :base(valueText) {
            _id = valueText;

            Verify = verify;
            Hint = hint;

            Root = new VisualElement();
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.justifyContent = Justify.SpaceBetween;
            SetBorderColor(Color.gray);
            Label title = new Label($"{titleText}:\t");
            _textField = new TextField();
            _textField.value = valueText;

            title.style.unityTextAlign = TextAnchor.MiddleLeft;
            _textField.style.unityTextAlign = TextAnchor.MiddleLeft;

            VisualElement ve = new VisualElement();
            ve.style.flexDirection = FlexDirection.Row;
            ve.style.alignItems = Align.Center;

            Root.Add(title);
            ve.Add(_textField);

            Image image = new Image();
            if (Verify != null) {
                image.image = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Tools/AmplifyShaderEditor/Plugins/EditorResources/UI/Buttons/Checkmark.png");
                image.style.flexGrow = 0;
                image.style.flexShrink = 0;
                image.style.height = 12;
                image.style.width = 12;
                string error = Verify(_textField.value);
                if (error == "") {
                    image.tintColor = Color.green;
                } else {
                    image.tintColor = Color.red;
                }
                ve.Add(image);
            }

            void SetHint(string newValue) {
                VisualElement hintVE = new VisualElement();
                bool shouldShow = false;
                if (Verify != null) {
                    string error = Verify(newValue);
                    if (error == "") {
                        image.tintColor = Color.green;
                    } else {
                        shouldShow = true;
                        var errorLabel = new Label(error);
                        errorLabel.style.color = Color.red;
                        hintVE.Add(errorLabel);
                        image.tintColor = Color.red;
                    }
                }

                if (Hint != null && newValue != "") {
                    var hints = Hint(newValue);
                    if (hints != null) {
                        shouldShow = true;
                        for (int i = 0; i < hints.Count; i++) {
                            hintVE.Add(new Label(hints[i]));
                        }
                    }
                }

                if (shouldShow)
                    InputHint.Instance.SetContent(hintVE, _textField.worldBound.position + new Vector2(15 + _textField.resolvedStyle.width, 15));
            }

            _textField.RegisterCallback<FocusOutEvent>(obj => {
                InputHint.Instance.Hide();
            });

            _textField.RegisterCallback<FocusInEvent>(obj => {
                SetHint(_textField.value);
            });

            _textField.RegisterValueChangedCallback(obj => {
                SetHint(obj.newValue);
            });

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
            return _textField.value;
        }
    }
}
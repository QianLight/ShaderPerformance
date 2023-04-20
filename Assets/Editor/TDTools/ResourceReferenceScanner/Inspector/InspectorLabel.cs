using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class InspectorLabel {
        public VisualElement Root;
        ResourceReferenceInspector _inspectorWindow;
        InspectorElement _owner;
        string _id;

        public InspectorLabel(InspectorElement owner, string titleText, string valueText, string tooltip) {
            _inspectorWindow = ResourceReferenceInspector.Instance;
            _owner = owner;
            _id = valueText;

            Root = new VisualElement();
            Root.style.flexDirection = FlexDirection.Row;
            Root.style.justifyContent = Justify.SpaceBetween;
            SetBorderColor(Color.gray);
            Label title = new Label($"{titleText}:\t");
            Label value = new Label(valueText);

            title.style.unityTextAlign = TextAnchor.MiddleLeft;
            value.style.unityTextAlign = TextAnchor.MiddleLeft;

            Root.Add(title);
            Root.Add(value);

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
    }
}
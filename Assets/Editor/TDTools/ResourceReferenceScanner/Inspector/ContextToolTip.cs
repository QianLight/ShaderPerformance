using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {

    public class ContextToolTip {
        public VisualElement Root;
        public static ContextToolTip Instance;
        public VisualElement FixedRoot;


        public ContextToolTip(VisualElement owner) {
            Root = new VisualElement();
            Root.style.position = Position.Absolute;
            Root.style.display = DisplayStyle.None;
            Root.style.flexGrow = 1;
            Root.style.flexShrink = 1;
            Root.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            SetBorderColor(Color.black);
            owner.RegisterCallback<MouseMoveEvent>(OnMouseMove);

            owner.Add(Root);
            Instance = this;


            FixedRoot = new VisualElement();
            FixedRoot.style.position = Position.Absolute;
            FixedRoot.style.flexGrow = 1;
            FixedRoot.style.flexShrink = 1;
            FixedRoot.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            FixedRoot.transform.position = new Vector3(15, 15);
            owner.Add(FixedRoot);
        }

        public void SetBorderColor(Color color) {
            Root.style.borderBottomColor = color;
            Root.style.borderTopColor = color;
            Root.style.borderLeftColor = color;
            Root.style.borderRightColor = color;
            Root.style.borderTopWidth = 1f;
            Root.style.borderRightWidth = 1f;
            Root.style.borderBottomWidth = 1f;
            Root.style.borderLeftWidth = 1f;
        }

        public void Show() {
            Root.style.display = DisplayStyle.Flex;
        }

        public void Hide() {
            Root.style.display = DisplayStyle.None;
        }

        public void SetContent(VisualElement content) {
            Root.Clear();
            Root.Add(content);
        }

        void OnMouseMove(MouseMoveEvent e) {
            Root.transform.position = e.localMousePosition + new Vector2(15f, 15f);
        }

        public void SetFixed(VisualElement content) {
            FixedRoot.Clear();
            FixedRoot.Add(content);
        }

        public void ClearFixed() {
            FixedRoot.Clear();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class InputHint{
        public static InputHint Instance;
        public VisualElement Root;

        VisualElement _owner;

        public InputHint(VisualElement owner) {
            Root = new VisualElement();
            Root.style.position = Position.Absolute;
            Root.style.display = DisplayStyle.None;
            Root.style.flexGrow = 1;
            Root.style.flexShrink = 1;
            Root.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            SetBorderColor(Color.black);
            _owner = owner;
            owner.Add(Root);
            Instance = this;
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

        public void SetContent(VisualElement contents, Vector2 position) {
            Root.Clear();
            Root.Add(contents);
            Root.transform.position = position - _owner.worldBound.position;
            Root.style.display = DisplayStyle.Flex;
        }

        public void Hide() {
            Root.Clear();
            Root.style.display = DisplayStyle.None;
        }
    }
} 

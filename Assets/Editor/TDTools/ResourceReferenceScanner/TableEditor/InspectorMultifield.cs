using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public class InspectorMultifield<T> : BaseInspectorField where T : BaseInspectorField {
        char _seperator;
        Func<string, T> _makeItem;
        List<T> _list;
        string _label;

        public override string GetValue() {
            if (_list.Count == 0)
                return "";

            string result = _list[0].GetValue();
            for (int i = 1; i < _list.Count; i++) {
                result = $"{result}{_seperator}{_list[i].GetValue()}";
            }
            return result;
        }

        public InspectorMultifield(char seperator, string label, string value, Func<string, T> makeItem = null) :base(value) {
            _seperator = seperator;
            _makeItem = makeItem;
            _list = new List<T>();
            Root = new VisualElement();
            _label = label;

            if (value != "")
                RebuildContent(value.Split(seperator));
            else
                RebuildContent(null);
        }

        void RebuildContent(string[] values = null) {
            Root.Clear();
            if (values != null) {
                for (int i = 0; i < values.Length; i++) {
                    BuildContent(values[i]);
                }
            } else { 
                var t = _list;
                _list = new List<T>();
                for (int i = 0; i < t.Count; i++) {
                    BuildContent(t[i].GetValue());
                }
            }

            VisualElement ve = new VisualElement();
            ve.style.flexDirection = FlexDirection.Row;
            ve.style.alignItems = Align.Center;

            Button buttonAdd = new Button();
            buttonAdd.text = "+";
            buttonAdd.style.flexGrow = 0;
            buttonAdd.style.marginTop = 4;
            buttonAdd.style.marginBottom = 4;
            buttonAdd.clicked += () => {
                BuildContent("");
                RebuildContent();
            };
            ve.Add(buttonAdd);

            ve.Add(new Label(_label));

            Root.Add(ve);
        }

        void BuildContent(string value) {
            int index = _list.Count;
            VisualElement rowVE = new VisualElement();
            rowVE.style.flexDirection = FlexDirection.Row;
            rowVE.style.borderTopWidth = 1;
            rowVE.style.borderTopColor = Color.gray;
            rowVE.style.borderBottomWidth = 1;
            rowVE.style.borderBottomColor = Color.gray;

            Button buttonRemove = new Button();
            buttonRemove.text = "-";
            buttonRemove.style.flexGrow = 0;
            buttonRemove.style.marginTop = 4;
            buttonRemove.style.marginBottom = 4;
            rowVE.Add(buttonRemove);
            rowVE.style.justifyContent = Justify.SpaceBetween;

            buttonRemove.clicked += () => {
                _list.RemoveAt(index);
                RebuildContent();
            };

            //rowVE.style.alignItems = Align.Stretch;
            if (_makeItem != null) {
                _list.Add(_makeItem(value));
                _list[index].Root.style.flexGrow = 1;
                _list[index].Root.style.flexShrink = 0;
                rowVE.Add(_list[index].Root);
            } else {
                //list.Add(new T(s[0]));
            }
            Root.Add(rowVE);
        }
    }

}
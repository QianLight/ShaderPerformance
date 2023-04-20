using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public abstract class BaseInspectorField {
        public VisualElement Root;

        public abstract string GetValue();

        public BaseInspectorField(string value) { }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.SkillTreeSimulator {
    public class SkillTreeSimulatorPasteWindow : EditorWindow {
        public string Line;

        public static void Show(string s) {
            var wnd = GetWindow<SkillTreeSimulatorPasteWindow>();
            wnd.titleContent = new GUIContent("Êä³ö¸´ÖÆ");
            wnd.Line = s;
            wnd.CreateGUI();
        }

        void CreateGUI() {
            rootVisualElement.Clear();
            TextField field = new TextField();
            field.value = Line;
            field.isReadOnly = true;
            field.style.flexGrow = 100;
            rootVisualElement.Add(field);
        }


    }
}
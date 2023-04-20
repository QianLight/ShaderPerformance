using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UIAnalyer;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UIAnalyer
{

    public interface IControl
    {
        void MoveLeft(ref Rect postion, Rect rect);

        void MoveRight(ref Rect postion, Rect rect);
        void MoveNextLine(ref Rect position, Rect rect, int line = 1);
    }
    public class UISystemSelection : IControl
    {
        private UISystemAnalyer m_control;
        private float m_line_height = 20;
        public UISystemAnalyer control { get { return m_control; } }

        private Vector2 m_scroll = Vector2.zero;

        private UISystemInfo m_systemInfo;

        public UISystemInfo Selection { get { return m_systemInfo; } }
        // private SerializedObject serializedObject;
        public UISystemSelection(UISystemAnalyer control)
        {
            m_control = control;
        }
        public void SetSelection(UISystemInfo info)
        {
            m_systemInfo = info;
            // serializedObject = new SerializedObject(m_systemInfo.data);        
        }

        public void CreateUISystem()
        {
            UISystemModel.CurrentLastIndex++;
            SystemDefineData defineData = new SystemDefineData();
            defineData.CreateEmptyData();
            defineData.id = UISystemModel.CurrentLastIndex;
            FieldValue field = defineData.GetPublicField(1);
            field.SetValue("New_" + UISystemModel.CurrentLastIndex);
            defineData.displayName = field.StrValue();
            UISystemFolderInfo folder = m_control.sourceModel.folder.AddFolder<UISystemFolderInfo>(defineData);
            SetSelection(folder);
            m_control.systemTree.SetSelection(new List<int>(){
                defineData.id
            });
            m_control.systemTree.Reload();

        }
        public void OnGUI(Rect rect)
        {
            if (m_systemInfo == null)
            {
                EditorGUI.LabelField(rect, "未选择");
            }
            else
            {
                RowGUI(m_systemInfo, rect);
            }
        }



        private void RowGUI(UISystemInfo system, Rect full)
        {
            if (system.data.TabStrs == null) return;

            Rect rect = new Rect(full.x + 20, full.y, full.width - 40, 0);
            Rect position = new Rect(rect.x, rect.y, rect.width, 0);

            for (int i = 0; i < system.data.values.Length; i++)
            {
                MoveNextLine(ref position, rect);
                MoveLeft(ref position, rect);
                EditorGUI.LabelField(position, system.data.values[i].fieldName);
                MoveRight(ref position, rect);
                system.data.values[i].RowGUI(position,this);
            }
        }

        private int m_lastLine = 1;

        public void MoveLeft(ref Rect postion, Rect rect)
        {
            postion.x = rect.x;
            postion.width = rect.width * 0.3f;
        }

        public void MoveRight(ref Rect postion, Rect rect)
        {
            postion.x = rect.x + rect.width * 0.3f;
            postion.width = rect.width * 0.7f;
        }
        public void MoveNextLine(ref Rect position, Rect rect, int line = 1)
        {
            position.x = rect.x;
            position.y += m_line_height * m_lastLine;
            position.height = m_line_height * line;
            position.width = rect.width;
            m_lastLine = line;
        }
    }
}
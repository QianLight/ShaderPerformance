using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BlueprintAreaComment : BluePrintWidget
    {
        public List<BluePrintWidget> commentWidgets = new List<BluePrintWidget>();
        public string commentText;

        bool bShowSettingWindow = false;
        public Vector4 offset = new Vector4(5, 50, 10, 20);

        private BlueprinButton settingButton;
        private BlueprinButton settingCloseButton;
        private BlueprinButton deleteButton;
        private BlueprintImageToggle settingEditButton;

        public virtual void Init(BluePrintGraph root)
        {
            Root = root;

            settingButton = new BlueprinButton(this);
            settingButton.ImageName = "BluePrint/Setting";
            settingButton.RegisterClickEvent(ButtonClickCb);

            settingCloseButton = new BlueprinButton(this);
            settingCloseButton.ImageName = "BluePrint/Setting";
            settingCloseButton.RegisterClickEvent(CloseSettingWindow);

            deleteButton = new BlueprinButton(this);
            deleteButton.ImageName = "BluePrint/ExstringActive";
            deleteButton.RegisterClickEvent(OnDeleteClick);

            //settingEditButton = new BlueprintImageToggle(this);
            //settingEditButton.OnImageName = "BluePrint/PinArrowRight";
            //settingEditButton.OffImageName = "BluePrint/PinArrowRight1";
            //settingEditButton.RegisterClickEvent(OnEditToggleClick);

            AddChild(settingButton);
            AddChild(settingCloseButton);
            //AddChild(settingEditButton);
            AddChild(deleteButton);
        }

        public void AddCommentNode(BluePrintWidget widget)
        {
            if (!commentWidgets.Contains(widget)) commentWidgets.Add(widget);
        }
        public void RemoveCommentNode(BluePrintWidget widget)
        {
            commentWidgets.Remove(widget);
        }

        public bool ContainWidget(BluePrintWidget w)
        {
            return commentWidgets.Contains(w);
        }

        public void Resize()
        {
            Rect area = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

            for(int i = 0; i < commentWidgets.Count; ++i)
            {
                if (commentWidgets[i].Bounds.x < area.x) area.x = commentWidgets[i].Bounds.x;
                if (commentWidgets[i].Bounds.y < area.y) area.y = commentWidgets[i].Bounds.y;
                if (commentWidgets[i].Bounds.x + commentWidgets[i].Bounds.width > area.width) area.width = commentWidgets[i].Bounds.x + commentWidgets[i].Bounds.width;
                if (commentWidgets[i].Bounds.y + commentWidgets[i].Bounds.height > area.height) area.height = commentWidgets[i].Bounds.y + commentWidgets[i].Bounds.height;
            }

            Bounds = new Rect(area.x - offset.x, area.y - offset.y, area.width - area.x + offset.x + offset.z, area.height - area.y + offset.w + offset.y);
        }
        
        public override void Draw()
        {
            Resize();

            adjustedBounds = Bounds.Scale(Scale);
            DrawBackground(adjustedBounds);

            settingButton.Bounds = new Rect(adjustedBounds.x + adjustedBounds.width - 20, adjustedBounds.y + 10, 16, 16);
            settingButton.Draw();

            DrawNoteText();
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (bShowSettingWindow)
                DrawSettingWindow();
        }

        private void DrawBackground(Rect targetBounds)
        {
            DrawTool.DrawStretchBox(targetBounds, BlueprintStyles.BoxHighlighter4, 12f);
        }

        private void DrawNoteText()
        {
            Rect textRect = new Rect(adjustedBounds.x + 10, adjustedBounds.y + 10, adjustedBounds.width - 40, adjustedBounds.height - 20);
            commentText = EditorGUI.TextArea(textRect, commentText, BlueprintStyles.AreaCommentStyle);
        }

        protected void ButtonClickCb(object o)
        {
            bShowSettingWindow = true;
            Root.SetCommentEditorWindow(this);
            //Root.SetCommentMode(true);
        }

        public void CloseSettingWindow(object o)
        {
            //Root.SetCommentMode(false);
            bShowSettingWindow = false;
        }

        public void OnDeleteClick(object o )
        {
            Root.RemoveComment(this);
        }

        private void DrawSettingWindow()
        {
            Rect bgRect = new Rect(adjustedBounds.x + adjustedBounds.width, adjustedBounds.y, 200, 120);
            DrawTool.DrawStretchBox(bgRect, BlueprintStyles.NodeBackground, 0f);

            settingCloseButton.Bounds = new Rect(bgRect.x + 200 - 30, bgRect.y + 10, 16, 16);
            settingCloseButton.Draw();

            EditorGUI.LabelField(new Rect(bgRect.x + 10, bgRect.y + 40, 30, 16), "left");
            offset.x = EditorGUI.FloatField(new Rect(bgRect.x + 40, bgRect.y + 40, 30, 16),  offset.x);

            EditorGUI.LabelField(new Rect(bgRect.x + 50, bgRect.y + 10, 30, 16), "up");
            offset.y = EditorGUI.FloatField(new Rect(bgRect.x + 80, bgRect.y + 10, 30, 16),  offset.y);

            EditorGUI.LabelField(new Rect(bgRect.x + 90, bgRect.y + 40, 30, 16), "right");
            offset.z = EditorGUI.FloatField(new Rect(bgRect.x + 120, bgRect.y + 40, 30, 16),  offset.z);

            EditorGUI.LabelField(new Rect(bgRect.x + 50, bgRect.y + 70, 30, 16), "down");
            offset.w = EditorGUI.FloatField(new Rect(bgRect.x + 80, bgRect.y + 70, 30, 16), offset.w);
            Resize();

            //settingEditButton.Bounds = new Rect(bgRect.x + 20, bgRect.y + 90, 40, 20);
            //settingEditButton.Draw();

            deleteButton.Bounds = new Rect(bgRect.x + 20, bgRect.y + 90, 40, 20);
            deleteButton.Draw();
        }

        private void OnEditToggleClick(bool state, object o)
        {
            //Root.SetCommentMode(state);
        }
    }
}

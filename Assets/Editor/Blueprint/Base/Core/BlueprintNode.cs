using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class BluePrintNode : BluePrintWidget
    {
        public bool hasError { get; set; }

        public NodeConfigData nodeEditorData;
        public List<BluePrintPin> pinList = new List<BluePrintPin>();

        protected string _header_image = BluePrintHelper.strDefaultNodeHeader;
        protected virtual string HeaderImage
        {
            get { return _header_image; }
            set { _header_image = value; }
        }

        public float nodeWidth = 120f;
        protected float titleHeight = 40f;
        public bool DrawHead = true;

        public bool MaskAsDelete  { get; set; }

        public bool CanbeFold = false;

        BlueprinButton expandButton;

        public string NodeName
        {
            get
            {
                return !string.IsNullOrEmpty(nodeEditorData.TitleName) ? nodeEditorData.TitleName :
                         !string.IsNullOrEmpty(nodeEditorData.Tag) ? nodeEditorData.Tag : nodeEditorData.BackgroundText;
            }
        }
        public virtual void Init(BluePrintGraph root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            Root = root;
            Bounds = new Rect(pos.x, pos.y, 50, 50);
            Enabled = true;
            if (nodeEditorData == null)
            {
                nodeEditorData = new NodeConfigData();
                nodeEditorData.Position = pos;
            }

            if(AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "", PinType.Main, PinStream.In, 20);
                BluePrintPin pinOut = new BluePrintPin(this, -2, "", PinType.Main, PinStream.Out, 20);
                AddPin(pinIn);
                AddPin(pinOut);
            }

            expandButton = new BlueprinButton(this);
            expandButton.RegisterClickEvent(ExpandButtonClickCb);
            AddChild(expandButton);
        }

        public virtual void PostInit(){}
        

        public virtual void SetInternalParam(int interParam, string customType) { }

        public virtual void UnInit()
        {
            pinList.Clear();
        }

        public void AddPin(BluePrintPin pin)
        {
            pinList.Add(pin);
            AddChild(pin);
        }

        public void RemovePin(BluePrintPin pin)
        {
            pinList.Remove(pin);
            pin.OnDeleted();
        }

        public BluePrintPin GetPin(int pinID)
        {
            for(int i = 0; i < pinList.Count; ++i)
            {
                if (pinList[i].pinID == pinID)
                    return pinList[i];
            }

            return null;
        }

        public void SetTitle(string title)
        {
            nodeEditorData.TitleName = title;
        }

        public BlueprintConnection ConnectPin(BluePrintPin start, BluePrintPin end)
        {
            BlueprintConnection conn = new BlueprintConnection(start, end);
            start.AddConnection(conn);

            BlueprintReverseConnection reverseConnection = new BlueprintReverseConnection(end, start);
            end.AddReversceConnection(reverseConnection);

            return conn;
        }

        public override void Draw()
        {
            adjustedBounds = Bounds.Scale(Scale);

            if (!DrawHead) titleHeight = 10;
            Vector2 titleSize = DrawTool.CalculateTextSize(nodeEditorData.TitleName, BlueprintStyles.HeaderStyle);
            adjustedBounds.width = Math.Max(nodeWidth * Scale, titleSize.x + 40 * Scale);
            adjustedBounds.height = (GetPinRectHeight() + titleHeight + 10) * Scale;
            Bounds.width = adjustedBounds.width / Scale;
            Bounds.height = adjustedBounds.height / Scale;

            var boxRect = adjustedBounds;
            //var headRect = new Rect(boxRect.x, boxRect.y, boxRect.width, titleHeight * Scale);
            float headTitleHeight = titleSize.y / Scale * 1.2f; // title在字体没有缩放时的高度 放大20%
            var headRect = new Rect(Bounds.x, Bounds.y, Bounds.width, Math.Max(headTitleHeight, titleHeight));
            //var titleRect = new Rect(headRect.x + 20, headRect.y, headRect.width - 40, titleHeight - (BlueprintStyles.HeaderStyle.fontSize) - 1);
            var titleRect = new Rect(headRect.x + 20, headRect.y + (headRect.height - headTitleHeight) * 0.2f/*文字放置偏上一些*/, headRect.width - 40, headTitleHeight);
            //var pinRect = new Rect(boxRect.x, boxRect.y + titleHeight * Scale, boxRect.width, boxRect.height - titleHeight * Scale);

            var pinRect = new Rect(Bounds.x, Bounds.y + titleHeight, Bounds.width, Bounds.height - titleHeight);

            DrawTag(adjustedBounds);

            DrawBeforeBackground(boxRect);

            DrawBackground(boxRect);

            if(DrawHead) DrawHeader(headRect);

            DrawTitle(titleRect, nodeEditorData.TitleName);

            DrawPin(pinRect);

            Rect r = Rect.zero;
            if(DrawNodeExtra(boxRect, ref r))
            {
                adjustedBounds = r;
            }

            DrawTipBox(adjustedBounds);

            DrawNote(adjustedBounds);

            nodeEditorData.Position = Bounds.position;
        }

        public virtual bool DrawNodeExtra(Rect boxRect, ref Rect RectWithExtra)
        {
            return false;
        }

        public virtual void DrawTipBox(Rect boxRect)
        {
            Root.NodeBehaviour.ShowTipFrame(this, boxRect);
        }

        public void DrawTag(Rect adjustedBounds)
        {
            var width = DrawTool.CalculateTextSize(nodeEditorData.Tag, BlueprintStyles.Tag1).x;
            var labelRect = new Rect((adjustedBounds.x + (adjustedBounds.width / 2)) - (width / 2), adjustedBounds.y - 16f * Scale, width, 16f * Scale);
            DrawTool.DrawLabel(labelRect, nodeEditorData.Tag, BlueprintStyles.Tag1, TextAnchor.MiddleCenter);
        }

        public virtual void DrawTitle(Rect titleBounds, string title)
        {
            if (IsEditing)
            {
                DrawTool.DrawTextbox(titleBounds.Scale(Scale), title, BlueprintStyles.HeaderStyle, TextAnchor.LowerCenter, (v, finished) =>
                 {
                     nodeEditorData.TitleName = v;
                     if (finished) IsEditing = false;
                 });
            }
            else
            {
                DrawTool.DrawLabel(titleBounds.Scale(Scale), title, BlueprintStyles.HeaderStyle, TextAnchor.LowerCenter);
            }
        }

        public virtual void DrawHeader(Rect headRect)
        {
            DrawTool.DrawNodeHeader(headRect.Scale(Scale), BlueprintStyles.Header(), BlueprintStyles.GetSkinTexture(Enabled ? HeaderImage : "BluePrint/Header0"));

            if(CanbeFold)
            {
                expandButton.ImageName = nodeEditorData.Expand ? "BluePrint/expand_down_arrow" : "BluePrint/expand_left_arrow";
                expandButton.Bounds = new Rect(headRect.x + headRect.width - 25, headRect.y + 8, 16, 16);
                expandButton.Draw();
            }
        }

        protected void ExpandButtonClickCb(object o)
        {
            nodeEditorData.Expand = !nodeEditorData.Expand;
        }

        public void DrawBackground(Rect targetBounds)
        {
            DrawTool.DrawStretchBox(targetBounds, BlueprintStyles.NodeBackground, 12f);
        }

        public void DrawBeforeBackground(Rect boxRect)
        {

        }

        protected int GetPinRectHeight()
        {
            int inHeight = 0;
            int outHeight = 0;

            for (int i = 0; i < pinList.Count; i++)
            {
                if(!nodeEditorData.Expand && !pinList[i].HasConnectionOrDefaultValue()) continue;

                if (pinList[i].pinStream == PinStream.In)
                {
                    inHeight += pinList[i].pinHeight;
                }
                else if (pinList[i].pinStream == PinStream.Out)
                {
                    outHeight += pinList[i].pinHeight;
                }
            }

            int maxHeight = Mathf.Max(inHeight, outHeight);

            return maxHeight;
        }

        public virtual void DrawPin(Rect pinRect)
        {
            int inCount = 0;
            int outCount = 0;

            //Vector2 size = new Vector2(16, 16) * Scale;
            float iconSize = 16;

            for (int i = 0; i < pinList.Count; i++)
            {
                if(!nodeEditorData.Expand && !pinList[i].HasConnectionOrDefaultValue()) continue;

                if(pinList[i].pinStream == PinStream.In)
                {
                    pinList[i].Bounds = new Rect(pinRect.x + 8, pinRect.y + inCount * 22, iconSize, iconSize);
                    inCount++;
                }
                else if(pinList[i].pinStream == PinStream.Out)
                {
                    pinList[i].Bounds = new Rect(pinRect.x + pinRect.width - 24, pinRect.y + outCount * 22, iconSize, iconSize);
                    outCount++;
                }

                pinList[i].Draw();
            }
        }

        public virtual void DrawNote(Rect adjustedBounds)
        {
            if(!string.IsNullOrEmpty(nodeEditorData.CustomNote) && nodeEditorData.ShowNote)
            {
                Vector2 size = DrawTool.CalculateTextSize(nodeEditorData.CustomNote, BlueprintStyles.Tag1);
                float textWidth = size.x;
                float textHeight = size.y;
                //float width = Mathf.Min(textWidth, 120 * Scale);
                //float height = ((int)((textWidth - 1) / width) + 1) * 30 * Scale;
                float width = textWidth;
                string[] strList = nodeEditorData.CustomNote.Split('\n');
                float height = strList.Length * 18 + 6;
                Rect rect = new Rect(adjustedBounds.x + adjustedBounds.width - 40, adjustedBounds.y - height + 30, width, height);
                DrawTool.DrawStretchBox(rect, BlueprintStyles.NoteBackground, 12f);

                Rect textRect = new Rect(rect.x + 10, rect.y + 6, rect.width - 20, rect.y);
                string printStr = nodeEditorData.CustomNote;
                //int insertIndex = 15;

                //while (insertIndex < nodeEditorData.CustomNote.Length)
                //{
                //    printStr = printStr.Insert(insertIndex, "\\n");
                //    insertIndex += 15;
                //}

                DrawTool.DrawLabel(textRect, printStr, BlueprintStyles.NoteTextStyle, TextAnchor.UpperLeft);

                //GUILayout.BeginArea(textRect);
                //GUIContent gc = new GUIContent(nodeEditorData.CustomNote);
                //GUILayout.Label();
                //GUILayout.EndArea();
            }
        }

        protected override bool OnMouseDownOutSide(Event e)
        {
            //if (IsSelected) root.selectNode = null;
            //IsSelected = false;
            IsEditing = false;
            UnityEngine.GUI.FocusControl("");

            return false;
        }

        protected override bool OnMouseLeftDown(Event e)
        {
            Root.NodeBehaviour.OnMouseLeftDown(this);

            return true;
            
        }

        protected override bool OnMouseRightDown(Event e)
        {
            var genericMenu = new GenericMenu();
            //genericMenu.AddItem(new GUIContent("Rename"), false, OnRenameClicked);
            genericMenu.AddItem(new GUIContent("Enable"), Enabled, OnEnableClicked);
            genericMenu.AddItem(new GUIContent("Delete"), false, OnDeleteClicked);
            genericMenu.ShowAsContext();

            return true;
        }

        protected override bool OnMouseUp(Event e)
        {
            base.OnMouseUp(e);

            if (Root.editorWindow.OpenSnap)
            {
                int snap = Root.editorWindow.SnapSize;
                Rect rect = Bounds;
                Bounds.x = (int)(rect.x / snap) * snap;
                Bounds.y = (int)(rect.y / snap) * snap;
            }

            Root.ReSizeAllComment();

            return true;
        }

        public virtual void OnAdded() { }

        public virtual void OnDeleted()
        {
            for (int i = 0; i < pinList.Count; ++i)
            {
                pinList[i].OnDeleted();
            }
        }

        protected void OnEnableClicked()
        {
            Enabled = !Enabled;
        }

        public virtual void OnDeleteClicked()
        {
            Root.DeleteNodeConsiderSubGraph(this);
        }

        public virtual void DrawDataInspector()
        {
            GUILayout.BeginHorizontal();
            //GUILayout.Label("GraphID:" + graphConfigData.graphID);
            EditorGUILayout.LabelField("NodeID:", new GUILayoutOption[] { GUILayout.Width(150f) });
            EditorGUILayout.LabelField(nodeEditorData.NodeID.ToString(), new GUILayoutOption[] { GUILayout.Width(120f) });
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Note:", new GUILayoutOption[] { GUILayout.Width(150f) });
            nodeEditorData.ShowNote = EditorGUILayout.Toggle(nodeEditorData.ShowNote, new GUILayoutOption[] { GUILayout.Width(120f) });
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            nodeEditorData.CustomNote = EditorGUILayout.TextArea(nodeEditorData.CustomNote, new GUILayoutOption[] { GUILayout.Width(270f), GUILayout.Height(50f) });
            EditorGUILayout.EndHorizontal();
        }

        public virtual void OnConnectionBreak(BluePrintPin start, BluePrintPin end) {}

        public virtual void OnConnectionSucc(BluePrintPin start, BluePrintPin end, BlueprintConnection connection) {}
        
        public virtual bool CanConnect(BluePrintPin start, BluePrintPin end) { return true; }

        //public virtual void OnBeforeNodeDelete() { }

        public virtual void OnSelected() { }

        public virtual void OnUnselected() { }

        public virtual bool IsExecuted() { return false; }

        public virtual void DrawGizmo() { }

        public virtual VariantType GetDataType(int pinID)
        {
            var pin = pinList.Find(p => p.pinID == pinID);
            if (pin.pinStream == PinStream.In)
                return GetInputDataType(pin as BluePrintValuedPin);
            else
                return GetOutputDataType(pin as BluePrintValuedPin);
        }

        /// <summary>
        /// ����������pin����Ϊcustom�򷵻�����pin���������� 
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public virtual VariantType GetInputDataType(BluePrintValuedPin pin)
        {
            if (pin.dataType == VariantType.Var_Custom)
            {
                var inPin = (pin.reverseConnections[0].reverseConnectEnd as BluePrintValuedPin);
                return inPin.GetRealType();
            }
            else
                return pin.dataType;
        }
        /// <summary>
        /// �ɸ�������дʵ������������Ͷ�̬
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public virtual VariantType GetOutputDataType(BluePrintValuedPin pin)
        {
            return pin.dataType;
        }

        public BlueprintNodeErrorInfo nodeErrorInfo = new BlueprintNodeErrorInfo();
        public virtual void CheckError()
        {
            nodeErrorInfo.Reset();
            nodeErrorInfo.nodeID = nodeEditorData.NodeID;
            nodeErrorInfo.graphName = Root.GraphName;
        }

        public bool HasCompileError() { return nodeErrorInfo.HasError; }
    }
}
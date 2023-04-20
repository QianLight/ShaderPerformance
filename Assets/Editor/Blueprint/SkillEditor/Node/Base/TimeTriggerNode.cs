using EcsData;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public abstract class TimeTriggerNode<N> : BaseSkillNode where N : XBaseData, new()
    {
        public N HosterData = new N();

        public bool LockX = true;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                BluePrintPin pinOut = new BluePrintPin(this, -2, "Out", PinType.Main, PinStream.Out);
                AddPin(pinIn);
                AddPin(pinOut);
            }

            string tag = typeof(N).ToString();
            tag = tag.Remove(0, tag.LastIndexOf('.') + 2).Replace("Data", "");
            nodeEditorData.Tag = tag;
            if (HosterData.Index != -1) nodeEditorData.Tag += "_" + HosterData.Index.ToString();
            if (string.IsNullOrEmpty(nodeEditorData.TitleName)) nodeEditorData.TitleName = tag;
        }

        public override void InitData<T>(T data, NodeConfigData configData)
        {
            HosterData = data as N;
            nodeEditorData = configData;
        }

        public override T GetHosterData<T>()
        {
            return HosterData as T;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            bool oldTimeBased = HosterData.TimeBased;
            HosterData.TimeBased = EditorGUITool.Toggle("TimeBased", HosterData.TimeBased);

            if (HosterData.TimeBased)
            {
                if (!oldTimeBased)
                {
                    HosterData.At = GetRoot.PosXToTime(Bounds.x - OffsetX);
                    LockX = true;
                }

                LockX = EditorGUITool.Toggle("Lock X", LockX);
                HosterData.At = GetRoot.FrameToTime(EditorGUITool.Slider("Play At: ", GetRoot.TimeToFrame(HosterData.At), 0, GetRoot.TimeToFrame(GetRoot.Length)));
                Bounds = Bounds.SetPos(GetRoot.TimeToPosX(HosterData.At) + OffsetX, Bounds.y);
            }
            else
            {
                HosterData.At = 0;
                LockX = false;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (IsSelected && HosterData.TimeBased)
            {
                GetRoot.DrawLine(GetRoot.TimeToPosX(HosterData.At) * Scale, Color.green);
            }
        }

        protected override bool OnMouseDrag(Event e)
        {
            float oldX = Bounds.x;
            base.OnMouseDrag(e);

            if (HosterData.TimeBased)
            {
                if (Bounds.x > GetRoot.TimeToPosX(GetRoot.Length))
                {
                    Bounds = Bounds.SetPos(GetRoot.TimeToPosX(GetRoot.Length), Bounds.y);
                }

                if (LockX) Bounds = Bounds.SetPos(oldX, Bounds.y);

                HosterData.At = GetRoot.PosXToTime(Bounds.x - OffsetX);
            }

            return true;
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            if(HosterData.TimeBased)SetTriggerTime(HosterData.At, ref TriggerTime);
            DFSTriggerTime(this);
        }
    }
}
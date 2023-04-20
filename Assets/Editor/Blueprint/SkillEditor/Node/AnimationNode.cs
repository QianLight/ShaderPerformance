using BluePrint;
using CFEngine;
using EcsData;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public class AnimationNode : TimeTriggerNode<XAnimationData>
    {
        public override bool OneConnectPinOut { get { return pinList.Count > 2 || HosterData.AnimationTimeLine; } }

        private AnimationClip motion;

        private AnimationClip forwardMotion;
        private AnimationClip rightForwardMotion;
        private AnimationClip rightMotion;
        private AnimationClip rightBackMotion;
        private AnimationClip backMotion;
        private AnimationClip leftBackMotion;
        private AnimationClip leftMotion;
        private AnimationClip leftForwardMotion;
        private bool showRatio = false;
        private bool showTrans = false;
        public override float OffsetX
        {
            get { return HosterData.AnimationTimeLine ? -50 : 0; }
        }

        public override void DrawPin(Rect pinRect)
        {
            int inCount = 0;
            int outCount = 0;

            //Vector2 size = new Vector2(16, 16) * Scale;
            float iconSize = 16;

            for (int i = 0; i < pinList.Count; i++)
            {
                if (!nodeEditorData.Expand && !pinList[i].HasConnectionOrDefaultValue()) continue;

                if (pinList[i].pinStream == PinStream.In)
                {
                    pinList[i].Bounds = new Rect(pinRect.x + 8, pinRect.y + inCount * 22, iconSize, iconSize);
                    inCount++;
                }
                else if (pinList[i].pinStream == PinStream.Out)
                {
                    if (HosterData.AnimationTimeLine)
                    {
                        pinList[i].Bounds = new Rect(Bounds.x + 40 + GetRoot.GetDelta() * (i > 1 ? GetRoot.TimeToFrame(HosterData.TransTime[i - 2]) : 0), Bounds.y + titleHeight + 10, 10, iconSize);
                        pinList[i].Desc = "";
                        pinList[i].connectDeltaX = 0;
                        pinList[i].connectDeltaY = 0;
                        pinList[i].pinHeight = 0;
                    }
                    else
                    {
                        pinList[i].Bounds = new Rect(pinRect.x + pinRect.width - 24, pinRect.y + outCount * 22, iconSize, iconSize);
                        pinList[i].Desc = "Out";
                        pinList[i].connectDeltaX = 15;
                        pinList[i].connectDeltaY = 70;
                        pinList[i].pinHeight = 22;
                    }
                    outCount++;
                    if (i > 1) DrawPinEx(pinList[i], HosterData.TransTime[i - 2]);
                }

                pinList[i].Draw();
            }

            if (HosterData.AnimationTimeLine)
            {
                if (motion != null) DrawTimeLine((int)GetRoot.TimeToFrame(GetAnimationLength()));
                else nodeWidth = 0;
            }
            else
            {
                nodeWidth = 0;
            }
        }

        private void DrawPinEx(BluePrintPin pin, float time)
        {
            if (pin.connections.Count == 0)
            {
                var node = CreateTransNode(pin.Bounds.position + new Vector2(50, 50));

                BluePrintPin startPin = pin;
                BluePrintPin endPin = node.pinList[0];
                ConnectPin(startPin, endPin);
            }

            TimerNode n = pin.connections[0].connectEnd.GetNode<TimerNode>();
            n.Bounds.position = new Vector2(pin.Bounds.position.x + 40, n.Bounds.position.y);
            n.HosterData.Time = time;
            n.nodeEditorData.TitleName = string.Empty;
            n.nodeEditorData.Tag = n.nodeEditorData.Tag.Replace("Timer", "Trans");
            n.NoRightDownEvent = true;
            pin.NoRightDownEvent = true;
            n.ShowInspector = false;
            n.DrawHead = false;
            n.nodeWidth = 90;
            for (int i = 0; i < n.pinList.Count; ++i)
            {
                n.pinList[i].Desc = i == 0 ? ((int)(GetRoot.TimeToFrame(time) * 100) / 100f).ToString() : string.Empty;
            }
        }

        public void DrawTimeLine(int frame)
        {
            nodeWidth = 100 + GetRoot.GetDelta() * frame;
            Vector2 pos = new Vector2(Bounds.x + 50, Bounds.y + titleHeight * 0.7f) * Scale;
            float delta = GetRoot.GetDelta() * Scale;
            Vector2 start;
            Vector2 end;
            Color oldHandleColor = Handles.color;
            Handles.color = Color.white;
            for (int i = 0; i <= frame; ++i)
            {

                start = new Vector2(pos.x + i * delta, pos.y + 0);
                if (i % 10 == 0)
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 20 * Scale);
                }
                else if (i % 5 == 0)
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 10 * Scale);
                }
                else
                {
                    end = new Vector2(pos.x + i * delta, pos.y + 5 * Scale);
                }

                Handles.DrawLine(start, end);
                if (i % 10 == 0)
                {
                    GUI.Label(new Rect(start.x + 1, start.y, 100*Scale, 25 * Scale), new GUIContent(i.ToString()), BlueprintStyles.PinTextStyle);
                }
            }

            DrawRatioLine(0, HosterData.RatioList.Count > 0 ? HosterData.RatioList[0].RatioTime : GetAnimationLength(), 1);
            for (int i = 1; i < HosterData.RatioList.Count; ++i)
            {
                DrawRatioLine(HosterData.RatioList[i - 1].RatioTime, HosterData.RatioList[i].RatioTime, HosterData.RatioList[i - 1].RatioValue);
            }
            if (HosterData.RatioList.Count != 0)
                DrawRatioLine(HosterData.RatioList[HosterData.RatioList.Count - 1].RatioTime, GetAnimationLength(), HosterData.RatioList[HosterData.RatioList.Count - 1].RatioValue);

            Handles.color = oldHandleColor;
        }

        private void DrawRatioLine(float at,float end,float value)
        {
            Vector2 pos = new Vector2(Bounds.x + 50, Bounds.y + titleHeight * 0.7f) * Scale;
            float delta = GetRoot.GetDelta() * Scale;
            Vector2 v1 = new Vector2(pos.x + GetRoot.TimeToFrame(at) * delta, pos.y);
            Vector2 v2 = new Vector2(pos.x + GetRoot.TimeToFrame(end) * delta, pos.y);
            if (value == 1) Handles.color = new Color(1, 1, 1, 0.3f);
            else if (value > 1) Handles.color = new Color(0, 1, 0, 0.3f);
            else if (value < 1) Handles.color = new Color(1, 0, 0, 0.3f);
            float len = 20 * Scale;
            for (int i = 0; i < len; ++i)
            {
                Handles.DrawLine(v1, v2);
                ++v1.y;
                ++v2.y;
            }
        }

        private float GetMaxLength()
        {
            if (motion != null)
            {
                float left = motion.length;
                float cur = 0;
                float ratio = 1;
                for (int i = 0; i < HosterData.RatioList.Count; ++i)
                {
                    float delta = Mathf.Max(HosterData.RatioList[i].RatioTime - cur, 0);
                    if (left < delta * ratio)
                        delta = left / ratio;
                    left -= delta * ratio;
                    cur += delta;
                    if (i != HosterData.RatioList.Count - 1) ratio = HosterData.RatioList[i].RatioValue;
                }
                return cur + left / ratio;
            }
            else return 0;
        }

        private float GetAnimationLength()
        {
            if (motion != null)
            {
                float left = motion.length;
                float cur = 0;
                float ratio = 1;
                for (int i = 0; i < HosterData.RatioList.Count; ++i)
                {
                    float delta = Mathf.Max(HosterData.RatioList[i].RatioTime - cur, 0);
                    if (left < delta * ratio)
                        delta = left / ratio;
                    left -= delta * ratio;
                    cur += delta;
                    ratio = HosterData.RatioList[i].RatioValue;
                }
                return cur + left / ratio;
            }
            else return 0;
        }

        public override void BuildDataByPin()
        {
            GetHosterData<XBaseData>().TransferData.Clear();

            for (int i = 1; i < pinList.Count; ++i)
            {
                if (pinList[i].connections.Count != 0)
                {
                    for (int j = 0; j < pinList[i].connections.Count; ++j)
                        AddPinData<XBaseData>(pinList[i].connections[j].connectEnd.GetNode<BaseSkillNode>().GetHosterData<XBaseData>());
                }
                else
                {
                    AddPinData<XBaseData>(new XBaseData() { Index = -1 });
                }
            }
        }

        public override void BuildPinByData(Dictionary<int, BaseSkillNode> IndexToNodeDic)
        {
            for (int i = 2; i < pinList.Count; ++i)
            {
                pinList[i].OnDeleted();
            }
            if (pinList.Count > 2)
                pinList.RemoveRange(2, pinList.Count - 2);

            for (int i = 0; i < GetHosterData<XBaseData>().TransferData.Count; ++i)
            {
                if (i > 0 && HosterData.TransTime.Count > 0)
                {
                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, "", PinType.Main, PinStream.Out, 0, 0, 0);
                    AddPin(pinOut);
                }
                if (GetHosterData<XBaseData>().TransferData[i].Index != -1)
                {
                    BluePrintPin startPin = pinList[pinList.Count - 1];
                    BluePrintPin endPin = IndexToNodeDic[GetHosterData<XBaseData>().TransferData[i].Index].pinList[0];
                    ConnectPin(startPin, endPin);
                }
            }
        }

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, false);
            HeaderImage = "BluePrint/Header2";

            if (GetRoot.NeedInitRes)
            {
                motion = LoadMotion(HosterData.ClipPath);

                forwardMotion = LoadMotion(HosterData.Forward);
                rightForwardMotion = LoadMotion(HosterData.RightForward);
                rightMotion = LoadMotion(HosterData.Right);
                rightBackMotion = LoadMotion(HosterData.RightBack);
                backMotion = LoadMotion(HosterData.Back);
                leftBackMotion = LoadMotion(HosterData.LeftBack);
                leftMotion = LoadMotion(HosterData.Left);
                leftForwardMotion = LoadMotion(HosterData.LeftForward);
            }

            BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
            AddPin(pinIn);
            BluePrintPin pinOut = new BluePrintPin(this, -2, "", PinType.Main, PinStream.Out);
            AddPin(pinOut);

            if (HosterData != null)
            {
                for (int i = 0; i < HosterData.TransTime.Count; ++i)
                {
                    pinOut = new BluePrintPin(this, pinList.Count, "", PinType.Main, PinStream.Out, 0, 0, 0);
                    AddPin(pinOut);
                }
            }
        }

        public override void OnDeleteClicked()
        {
            for (int i = pinList.Count - 1; i > 1; --i)
            {
                pinList[i].connections[0].connectEnd.GetNode<BaseSkillNode>().OnDeleteClicked();
                RemovePin(pinList[i]);
            }

            base.OnDeleteClicked();
        }

        private AnimationClip LoadMotion (string path)
        {
            return AssetDatabase.LoadAssetAtPath (ResourecePath + path + ".anim", typeof (AnimationClip)) as AnimationClip;
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            ObjectFieldByPath<AnimationClip>("Motion Clip", ref motion, ref HosterData.ClipPath);
            if (motion != null) TimeFrameField("结束时间(s): ", HosterData.At + GetAnimationLength());

            if (HosterData.RatioList.Count != 0)
            {
                HosterData.RatioParamIndex = -1;
            }
            else
            {
                ParamTemplate("Ratio", HosterData,
                    () => { HosterData.Ratio = EditorGUITool.FloatField("Ratio", HosterData.Ratio); });
                HosterData.Ratio = HosterData.Ratio < 0.01f ? 0.01f : HosterData.Ratio;
            }

            if (GetRoot is SkillGraph)
            {
                HosterData.BlendIn = EditorGUITool.Popup("BlendIn", HosterData.BlendIn, EditorGUITool.Translate<XBlendModeType>());
                HosterData.BlendNext = EditorGUITool.Popup("BlendNext", HosterData.BlendNext, EditorGUITool.Translate<XBlendModeType>());
                EditorGUILayout.Space();
                HosterData.MultipleDirection = EditorGUITool.Toggle("MultipleDirection", HosterData.MultipleDirection);
                if (HosterData.MultipleDirection)
                {
                    ObjectFieldByPath<AnimationClip>("Forward", ref forwardMotion, ref HosterData.Forward);
                    ObjectFieldByPath<AnimationClip>("RightForward", ref rightForwardMotion, ref HosterData.RightForward);
                    ObjectFieldByPath<AnimationClip>("Right", ref rightMotion, ref HosterData.Right);
                    ObjectFieldByPath<AnimationClip>("RightBack", ref rightBackMotion, ref HosterData.RightBack);
                    ObjectFieldByPath<AnimationClip>("Back", ref backMotion, ref HosterData.Back);
                    ObjectFieldByPath<AnimationClip>("LeftBack", ref leftBackMotion, ref HosterData.LeftBack);
                    ObjectFieldByPath<AnimationClip>("Left", ref leftMotion, ref HosterData.Left);
                    ObjectFieldByPath<AnimationClip>("LeftForward", ref leftForwardMotion, ref HosterData.LeftForward);
                }
            }
            bool alawaysAnimation = HosterData.HasFlag(EcsData.XAnimationData.Flag_AlawaysAnimator);
            EditorGUI.BeginChangeCheck();
            alawaysAnimation = EditorGUITool.Toggle("AlwaysAnimation", alawaysAnimation);
            if (EditorGUI.EndChangeCheck())
            {
                HosterData.SetFlag(EcsData.XAnimationData.Flag_AlawaysAnimator, alawaysAnimation);
            }
            bool updateWhenOffscreen = HosterData.HasFlag(EcsData.XAnimationData.Flag_UpdateWhenOffscreen);
            EditorGUI.BeginChangeCheck();
            updateWhenOffscreen = EditorGUITool.Toggle("updateWhenOffscreen", updateWhenOffscreen);
            if (EditorGUI.EndChangeCheck())
            {
                HosterData.SetFlag(EcsData.XAnimationData.Flag_UpdateWhenOffscreen, alawaysAnimation);
            }
            if(updateWhenOffscreen)
            {
                PartConfig.instance.OnPartGUI(Root.graphConfigData.tag, ref HosterData.PartMask);
            }

            if (HosterData.RatioList.Count == 0 && HosterData.TransTime.Count == 0)
            {
                HosterData.AnimationTimeLine = EditorGUITool.Toggle("AnimationTimeLine", HosterData.AnimationTimeLine);
                if (HosterData.AnimationTimeLine && pinList[1].connections.Count > 1)
                {
                    LogError("使用AnimationTimeLine，Out连出节点个数必须小于2！！！");
                    HosterData.AnimationTimeLine = false;
                }
            }
            else HosterData.AnimationTimeLine = true;
            if (HosterData.RatioParamIndex != -1) HosterData.AnimationTimeLine = false;

            if (HosterData.AnimationTimeLine)
            {
                CacheTrans();
                DrawRatio();
                ResumeTrans();
                DrawTrans();
            }
        }

        private void CacheTrans()
        {
            for(int i=0;i<HosterData.TransTime.Count;++i)
            {
                HosterData.TransTime[i] = ScriptTimeToAnimTime(HosterData.TransTime[i]);
            }
        }

        private void ResumeTrans()
        {
            for (int i = 0; i < HosterData.TransTime.Count; ++i)
            {
                HosterData.TransTime[i] = AnimTimeToScriptTime(HosterData.TransTime[i]);
            }
        }

        private void DrawTrans()
        {
            DrawLine();
            EditorGUILayout.BeginHorizontal();
            //EditorGUITool.LabelField("Trans");
            showTrans = EditorGUILayout.Foldout(showTrans, "Trans");
            if (HosterData.TransTime.Count < 8)
            {
                if (GUILayout.Button("+"))
                {
                    showTrans = true;
                    HosterData.TransTime.Add(HosterData.TransTime.Count == 0 ? 0 : HosterData.TransTime[HosterData.TransTime.Count - 1]);

                    BluePrintPin pinOut = new BluePrintPin(this, pinList.Count, "", PinType.Main, PinStream.Out, 0, 0, 0);
                    AddPin(pinOut);
                }
            }
            EditorGUILayout.EndHorizontal();
            if(showTrans)
            {
                for (int i = 0; i < HosterData.TransTime.Count; ++i)
                {
                    DrawLine();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Trans_" + i.ToString());
                    if (GUILayout.Button("Delete"))
                    {
                        HosterData.TransTime.RemoveAt(i);
                        pinList[i + 2].connections[0].connectEnd.GetNode<BaseSkillNode>().OnDeleteClicked();
                        RemovePin(pinList[i + 2]);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    float minValue = 0;
                    float maxValue = GetAnimationLength();
                    HosterData.TransTime[i] = Mathf.Max(minValue, Mathf.Min(maxValue, TimeFrameField("At", HosterData.TransTime[i])));
                }
            }
        }

        private TimerNode CreateTransNode(Vector3 pos)
        {
            var data = (GetRoot is SkillGraph) ? (GetRoot.GetConfigData<EcsData.XSkillData>().TimerData) : (GetRoot.GetConfigData<EcsData.XHitData>().TimerData);
            var node = GetRoot.AddNodeInGraphByScript<XTimerData, EditorNode.TimerNode>(pos, ref data, true);
            return node;
        }

        private void DrawRatio()
        {
            EditorGUILayout.BeginHorizontal();
            //EditorGUITool.LabelField("Ratio");
            showRatio = EditorGUILayout.Foldout(showRatio, "Ratio");
            if (HosterData.RatioList.Count < 8)
            {
                if (GUILayout.Button("+"))
                {
                    showRatio = true;
                    XAnimationRatioData data = new XAnimationRatioData();
                    data.RatioTime = HosterData.RatioList.Count == 0 ? 0 : HosterData.RatioList[HosterData.RatioList.Count - 1].RatioTime;
                    data.RatioValue = 1;
                    HosterData.RatioList.Add(data);

                    data = new XAnimationRatioData();
                    data.RatioTime = GetAnimationLength();
                    data.RatioValue = 1;
                    HosterData.RatioList.Add(data);
                }
            }
            EditorGUILayout.EndHorizontal();

            if(showRatio)
            {
                for (int i = 0; i < HosterData.RatioList.Count; i += 2)
                {
                    DrawLine();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUITool.LabelField("Ratio_" + i.ToString());
                    if (GUILayout.Button("Delete"))
                    {
                        HosterData.RatioList.RemoveAt(i + 1);
                        HosterData.RatioList.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                    float minValue = GetMinTime(i);
                    float maxValue = GetMaxTime(i);
                    float preValue = HosterData.RatioList[i].RatioTime;
                    HosterData.RatioList[i].RatioTime = Mathf.Max(minValue,
                        Mathf.Min(maxValue, TimeFrameField("At", HosterData.RatioList[i].RatioTime)));
                    if (preValue != HosterData.RatioList[i].RatioTime)
                    {
                        float delta = (HosterData.RatioList[i].RatioTime - preValue) - (HosterData.RatioList[i].RatioTime - preValue) * HosterData.RatioList[i].RatioValue;
                        MoveRatio(delta, i + 1);
                    }

                    minValue = GetMinTime(i + 1);
                    maxValue = GetMaxTime(i + 1);
                    preValue = HosterData.RatioList[i + 1].RatioTime;
                    HosterData.RatioList[i + 1].RatioTime = Mathf.Max(minValue, Mathf.Min(maxValue, TimeFrameField("End", HosterData.RatioList[i + 1].RatioTime)));
                    if (preValue != HosterData.RatioList[i + 1].RatioTime)
                    {
                        float delta = (HosterData.RatioList[i + 1].RatioTime - preValue) - (HosterData.RatioList[i + 1].RatioTime - preValue) * HosterData.RatioList[i].RatioValue;
                        MoveRatio(delta, i + 2);
                    }

                    preValue = HosterData.RatioList[i].RatioValue;
                    HosterData.RatioList[i].RatioValue = Mathf.Min(3f, Mathf.Max(0.05f, EditorGUITool.FloatField(  "Value", HosterData.RatioList[i].RatioValue)));
                    if (preValue != HosterData.RatioList[i].RatioValue)
                    {
                        float delta = (HosterData.RatioList[i + 1].RatioTime - HosterData.RatioList[i].RatioTime) * preValue / HosterData.RatioList[i].RatioValue - (HosterData.RatioList[i + 1].RatioTime - HosterData.RatioList[i].RatioTime);
                        MoveRatio(delta, i + 1);
                    }
                }
            }
        }

        private float GetMinTime(int index)
        {
            return index != 0 ? HosterData.RatioList[index - 1].RatioTime : 0;
        }

        private float GetMaxTime(int index)
        {
            float nextTime = ((index + 1 == HosterData.RatioList.Count) ? GetAnimationLength() : HosterData.RatioList[index + 1].RatioTime) - HosterData.RatioList[index].RatioTime;
            float curValue = (index == 0 ? 1 : HosterData.RatioList[index - 1].RatioValue);
            float nextValue = (index == HosterData.RatioList.Count) ? 1 : HosterData.RatioList[index].RatioValue;
            return Mathf.Min(GetMaxLength(), HosterData.RatioList[index].RatioTime + nextTime * nextValue / curValue);
        }

        private float ScriptTimeToAnimTime(float t)
        {
            float ratio = 1;
            float animTime = 0;
            float scriptTime = t;
            float curTime = 0;
            int index = 0;
            while (scriptTime > 0)
            {
                if (index == HosterData.RatioList.Count)
                {
                    animTime += scriptTime * ratio;
                    scriptTime = 0;
                    break;
                }

                float tmp = HosterData.RatioList[index].RatioTime - curTime;
                if (tmp >= scriptTime)
                {
                    animTime += scriptTime * ratio;
                    scriptTime = 0;
                    break;
                }
                else
                {
                    animTime += tmp * ratio;
                    scriptTime -= tmp;
                    curTime += tmp;
                    ratio = HosterData.RatioList[index].RatioValue;
                    ++index;
                    continue;
                }
            }

            return animTime;
        }

        private float AnimTimeToScriptTime(float t)
        {
            float ratio = 1;
            float animTime = t;
            float scriptTime = 0;
            int index = 0;
            while (animTime > 0)
            {
                if (index == HosterData.RatioList.Count)
                {
                    scriptTime += animTime / ratio;
                    animTime = 0;
                    break;
                }

                float tmp = (HosterData.RatioList[index].RatioTime - scriptTime) * ratio;
                if (tmp >= animTime)
                {
                    scriptTime += animTime / ratio;
                    animTime = 0;
                    break;
                }
                else
                {
                    scriptTime += tmp / ratio;
                    animTime -= tmp;
                    ratio = HosterData.RatioList[index].RatioValue;
                    ++index;
                    continue;
                }
            }
            return scriptTime;
        }

        private void MoveRatio(float deltaTime,int beginIndex)
        {
            for (int i = beginIndex; i < HosterData.RatioList.Count; ++i)
            {
                HosterData.RatioList[i].RatioTime += deltaTime;
            }
        }

        public override T CopyData<T>(T data)
        {
            XAnimationData copy = base.CopyData(data) as XAnimationData;

            List<XAnimationRatioData> tmp = new List<XAnimationRatioData>();
            for (int i = 0; i < copy.RatioList.Count; ++i)
                tmp.Add(copy.RatioList[i].Clone() as XAnimationRatioData);
            copy.RatioList = tmp;

            List<float> transTime = new List<float>();
            for (int i = 0; i < copy.TransTime.Count; ++i)
                transTime.Add(copy.TransTime[i]);
            copy.TransTime = transTime;

            return copy as T;
        }

        public override void CopyDataFromTemplate (int templateID, int presentID)
        {
            CFUtilPoolLib.XEntityPresentation.RowData templateData = XEntityPresentationReader.GetData ((uint) templateID);
            CFUtilPoolLib.XEntityPresentation.RowData presentData = XEntityPresentationReader.GetData ((uint) presentID);

            string templateStr = templateData.AnimLocation.Remove(templateData.AnimLocation.Length - 1, 1);
            string presentStr = string.IsNullOrEmpty(GetRoot.GetConfigData<XConfigData>().AnimTemplate) ?
                presentData.AnimLocation.Remove(presentData.AnimLocation.Length - 1, 1) : GetRoot.GetConfigData<XConfigData>().AnimTemplate;
            string presentAnimLocation = presentData.AnimLocation.Remove(presentData.AnimLocation.Length - 1, 1);

            //HosterData.ClipPath = HosterData.ClipPath.Replace(templateStr, presentStr);
            var splitStr = HosterData.ClipPath.Split('/');
            HosterData.ClipPath = splitStr[0] + "/" + splitStr[1].Replace(templateStr, presentAnimLocation) 
                                         + "/" + splitStr[2].Replace(templateStr, presentStr);
        }

        private void AnimScanPolicy(OrderResList result, ResItem item,string path)
        {
            if (!string.IsNullOrEmpty(path))
                result.Add(item, Path.GetFileName(path) + ".anim", ResourecePath + path + ".anim");
        }

        public override void ScanPolicy(OrderResList result, ResItem item)
        {
            AnimScanPolicy(result, item, HosterData.ClipPath);
            if (HosterData.MultipleDirection)
            {
                AnimScanPolicy(result, item, HosterData.Forward);
                AnimScanPolicy(result, item, HosterData.RightForward);
                AnimScanPolicy(result, item, HosterData.Right);
                AnimScanPolicy(result, item, HosterData.RightBack);
                AnimScanPolicy(result, item, HosterData.Back);
                AnimScanPolicy(result, item, HosterData.LeftBack);
                AnimScanPolicy(result, item, HosterData.Left);
                AnimScanPolicy(result, item, HosterData.LeftForward);
            }
        }
    }
}
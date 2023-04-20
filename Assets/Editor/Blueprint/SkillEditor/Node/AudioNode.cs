using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;

namespace EditorNode
{
    public class AudioNode : TimeTriggerNode<XAudioData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header8";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.AudioName = EditorGUITool.TextField("AudioName: ", HosterData.AudioName);
            HosterData.ChannelID = (int)(CFUtilPoolLib.AudioChannel)EditorGUILayout.EnumPopup("ChannelID: ", (CFUtilPoolLib.AudioChannel)HosterData.ChannelID);
            HosterData.StopAtSkillEnd = EditorGUITool.Toggle("StopAtSkillEnd", HosterData.StopAtSkillEnd);
            HosterData.Follow = EditorGUITool.Toggle("Follow", HosterData.Follow);
        }

        public override void CopyDataFromTemplate(int templateID, int presentID)
        {
            CFUtilPoolLib.XEntityPresentation.RowData templateData = XEntityPresentationReader.GetData((uint)templateID);
            CFUtilPoolLib.XEntityPresentation.RowData presentData = XEntityPresentationReader.GetData((uint)presentID);

            string templateStr = templateData.AnimLocation.Remove(templateData.AnimLocation.Length - 1, 1);
            string presentStr = string.IsNullOrEmpty(GetRoot.GetConfigData<XConfigData>().AudioTemplate) ?
                presentData.AnimLocation.Remove(presentData.AnimLocation.Length - 1, 1) : GetRoot.GetConfigData<XConfigData>().AudioTemplate;

            HosterData.AudioName = HosterData.AudioName.Replace(templateStr, presentStr);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using CFUtilPoolLib;
using VirtualSkill;

namespace TDTools
{
    public interface IRecorderTaskItem
    {
        bool IsRuning { get; }
        bool IsFinish { get; }
        void DoFunc(object o);
        void FinishFunc(object o);
    }

    public class RecordingTaskItemET : IRecorderTaskItem
    {
        private bool isRuning;
        public bool IsRuning { get { return isRuning; } }
        private bool isFinish;
        public bool IsFinish { get { return isFinish; } }


        public bool NeedStart;
        public bool NeedFinish;
        public string SkillName;
        public int CameraDirection;
        public string PupetSkillName;
        public ESpecialMask SpecialMask;
        public RecorderController RecorderController;

        public void DoFunc(object o)
        {
            if (NeedStart)
            {
                RecorderSettings.OutputName = $"{Path.GetFileNameWithoutExtension(SkillName)}";
                RecorderController.PrepareRecording();
                RecorderController.StartRecording();
                SkillHoster.GetHoster.CheckRemoveEntity();
            }
            AutoRecorderMgr.GetMgr.SetCameraParam(CameraDirection);
            if (OverdrawMonitor.isOn)
            {
                OverdrawMonitor.Instance.StartObserveProfile(1, $"{AutoRecorderMgr.GetMgr.AssetsSkillLocation}{SkillName}");
            }
            XTimerMgr.singleton.SetTimer(0.5f, FireSkill, null);
            isRuning = true;
        }

        private void FireSkill(object o)
        {
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            (skillEditor.CurrentGraph as SkillGraph).OpenData($"{AutoRecorderMgr.GetMgr.SkillLocation}{SkillName}");
            if (SpecialMask.HasFlag(ESpecialMask.NeedHit))
            {
                var playerObj = AutoRecorderMgr.GetMgr.ERD.PlayerObj;
                var pupetPos = AutoRecorderMgr.GetMgr.ERD.PupetPos;
                SkillHoster.GetHoster.CreatePuppet(10017, 180, playerObj.transform.position.x + pupetPos.x,
                    playerObj.transform.position.y + pupetPos.y, playerObj.transform.position.z + pupetPos.z);
            }
            SkillHoster.aiMode = SpecialMask.HasFlag(ESpecialMask.AIMode);
            SkillHoster.GetHoster.FireSkillForRecord($"{AutoRecorderMgr.GetMgr.AssetsSkillLocation}{SkillName}", SkillHoster.PlayerIndex);
        }

        public void FinishFunc(object o)
        {
            if (NeedFinish)
            {
                RecorderController.StopRecording();
            }
            XTimerMgr.singleton.SetTimer(0.2f, obj => { isFinish = true; }, null);
        }
    }

    public class RecordingTaskItemRT : IRecorderTaskItem
    {
        private bool isRuning;
        public bool IsRuning { get { return isRuning; } }
        private bool isFinish;
        public bool IsFinish { get { return isFinish; } }
        public bool NeedStart;
        public bool NeedFinish;
        public string SkillName;
        public int CameraDirection;
        public RecorderController RecorderController;

        public void DoFunc(object o)
        {
            if (NeedStart)
            {

            }
        }

        public void FinishFunc(object o)
        {
            if (NeedFinish)
            {

            }
        }
    }
}

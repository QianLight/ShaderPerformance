using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using CFClient;
using CFEngine;
using CFUtilPoolLib;
using VirtualSkill;

namespace TDTools
{
    public enum AutoRecorderType
    {
        Editor,
        Runtime,
    }

    [Flags]
    public enum ESpecialMask
    {
        NeedHit = 1,
        AIMode = 2,
    }
    public class AutoRecorderMgr
    {
        private AutoRecorderType type;
        private SkillEditor skillEditor;
        private static AutoRecorderMgr mgr = new AutoRecorderMgr();

        public RecorderController recorderController;
        public RecorderControllerSettings recorderControllerSettings;

        public string CurPreName;
        public string OutputPath;
        public string SkillLocation;
        public string AssetsSkillLocation;

        //private bool skillPlaying = false;
        private bool autoCast = false;

        public AutoRecorderConfig Config;
        public EditorRecorderData ERD;
        public AutoRecorderCameraData ARCD = new AutoRecorderCameraData();

        public List<ISelectFile> FileList = new List<ISelectFile>();
        private Queue<IRecorderTaskItem> taskQueue = new Queue<IRecorderTaskItem>();
        private IRecorderTaskItem curTask;
        public static AutoRecorderMgr GetMgr
        {
            get { return mgr; }
        }

        public bool IsEditor { get { return type == AutoRecorderType.Editor; } }
        public bool IsBehit { get; set; }
        public bool IsRecording { get { return curTask != null; } }

        public bool PreOpen()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("当前不在运行状态下");
                return false;
            }
            if (XClientNetwork.singleton.XLoginStep == XLoginStep.Playing)
                type = AutoRecorderType.Runtime;
            else
            {
                skillEditor = EditorWindow.GetWindow<SkillEditor>();
                if (skillEditor != null && SkillHoster.GetHoster != null)
                    type = AutoRecorderType.Editor;
                else
                {
                    Debug.Log("当前不在游戏进行中且非技能编辑器脚本运行状态");
                    return false;
                }
            }
            return true;
        }

        public void InitEnv()
        {
            InitRecorderSetting();
            InitConfig();
            if (IsEditor)
                InitEditorRecorder();
            else
                InitRuntimeRecorder();
            InitFileList();
        }

        private void InitRecorderSetting()
        {
            if (recorderController == null)
            {
                recorderControllerSettings = RecorderControllerSettings.LoadOrCreate($"{Application.dataPath}/Editor/TDTools/AutoRecorder/recorder.pref");
                foreach (var setting in recorderControllerSettings.RecorderSettings)
                {
                    (setting as MovieRecorderSettings).videoBitRateMode = VideoBitrateMode.High;
                }
                recorderController = new RecorderController(recorderControllerSettings);
            }
        }

        private void InitBasicData(int pid, bool isBehit = false)
        {
            var presentData = XEntityPresentationReader.GetData((uint)pid);
            if (!isBehit)
            {
                SkillLocation = $"{Application.dataPath}/BundleRes/SkillPackage/{presentData.SkillLocation}";
                AssetsSkillLocation = $"/BundleRes/SkillPackage/{presentData.SkillLocation}/";
            }
            else
            {
                SkillLocation = $"{Application.dataPath}/BundleRes/SkillPackage/Role_hit";
                AssetsSkillLocation = $"/BundleRes/SkillPackage/Role_hit/";
            }
            CurPreName = presentData.Prefab;
        }

        private void InitEditorRecorder()
        {
            if (!OverdrawMonitor.isOn)
            {
                SFXMgr.singleton.createMonitor = OverdrawMonitor.Instance.StartObserveProfile;
                SFXMgr.singleton.destoryMonitor = OverdrawMonitor.Instance.EndObserveProfile;
            }
            var curGraph = skillEditor.CurrentGraph as SkillGraph;
            InitBasicData(curGraph.configData.PresentID);
            InitERD();
        }

        private void InitERD()
        {
            ERD = new EditorRecorderData
            {
                PlayerObj = GameObject.Find("Player"),
                PupetPos = new Vector3(0, 0, 3),
                PlayerIndex = SkillHoster.PlayerIndex,
                Target = SkillHoster.GetHoster.Target,
                CameraComponent = SkillHoster.GetHoster.cameraComponent,
                EntityDic = SkillHoster.GetHoster.EntityDic,
            };
        }

        private void InitFileList()
        {
            if (CurPreName != null)
            {
                FileList.Clear();
                var list = Directory.GetFiles(SkillLocation, "*.bytes", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < list.Length; ++i)
                {
                    if (IsEditor && !IsBehit)
                    {
                        var data = new FileData()
                        {
                            Name = Path.GetFileName(list[i]),
                            SpecialInt = 0,
                            PupetSkillName = string.Empty,
                        };
                        if (CurPreName == Config.RecorderName && i < Config.FileSelect.Count)
                        {
                            data.SetSelect(Config.FileSelect[i]);
                            data.SpecialInt = Config.FileSpecialList[i];
                            data.PupetSkillName = Config.PupetStringList[i];
                        }
                        FileList.Add(data);
                    }
                    else if (IsEditor && IsBehit)
                    {
                        var data = new HitFileData()
                        {
                            Name = Path.GetFileName(list[i]),
                        };
                    }
                }
            }
        }

        private void InitRuntimeRecorder()
        {
            //InitRRD();
            //EnterScene
            //HideRole
            //AddCamera

        }

        private void InitConfig()
        {
            string configPath = $"{Application.dataPath}/Editor/TDTools/AutoRecorder/UserConfig.ini";
            string configInfo = "";
            if (File.Exists(configPath))
                configInfo = File.ReadAllText(configPath);
            Config = new AutoRecorderConfig(configInfo);
            OutputPath = Config.OutPath;
        }

        private void WriteConfig()
        {
            string configPath = $"{Application.dataPath}/Editor/TDTools/AutoRecorder/UserConfig.ini";
            //Config.UpdateConfig();
        }

        public void SetSelectAll(bool isSelect)
        {
            FileList.ForEach(item => item.SetSelect(isSelect));
        }

        public void PasteData(out List<string> error)
        {
            var copyBuffer = GUIUtility.systemCopyBuffer.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            for (int i = 0; i < copyBuffer.Count; i++)
            {
                if (string.IsNullOrEmpty(copyBuffer[i]))
                    copyBuffer.RemoveAt(i);
                else if (!copyBuffer[i].EndsWith(".bytes"))
                    copyBuffer[i] = copyBuffer[i] + ".bytes";
            }
            error = new List<string>();
            for (int i = 0; i < copyBuffer.Count; ++i)
            {
                bool hasFind = false;
                foreach (var file in FileList)
                {
                    if (file.Name == copyBuffer[i])
                    {
                        file.SetSelect(true);
                        hasFind = true;
                    }
                }
                if (!hasFind)
                    error.Add(copyBuffer[i]);
            }
        }

        private void PreStart(int[] dirArray)
        {
            foreach (var file in FileList)
            {
                if (!file.Select)
                    continue;

                if (IsEditor && !IsBehit)
                {
                    for (int i = 0; i < dirArray.Length; ++i)
                    {
                        var fileData = file as FileData;
                        var item = new RecordingTaskItemET();
                        item.SkillName = fileData.Name;
                        item.NeedStart = i == 0;
                        item.NeedFinish = i == dirArray.Length - 1;
                        item.CameraDirection = dirArray[i];
                        item.PupetSkillName = fileData.PupetSkillName;
                        item.RecorderController = recorderController;
                        taskQueue.Enqueue(item);
                    }
                }
            }
        }

        public void StartRecord()
        {
            PreStart(new int[] { 0, 1, 2, 3 });
            curTask = taskQueue.Dequeue();
            RecorderSettings.OutputDir = OutputPath;
            SkillHoster.GetHoster.showGUI = false;
            AddAutoCaster();
            AddSkillEndHandler();
        }

        public void SetCameraParam(int direction = -1)
        {
            GameObject camera = GameObject.Find("FreeLook_skillEditor(Clone)");
            ARCD.SetCameraParam(camera, direction);
        }

        public void ResetCameraParam()
        {
            GameObject camera = GameObject.Find("FreeLook_skillEditor(Clone)");
            ARCD.ResetCameraParam(camera);
        }

        public void AutoSkillCaster()
        {
            if (!Application.isPlaying)
            {
                RemoveAutoCaster();
                return;
            }

            XTimerMgr.singleton.Update(Time.deltaTime);

            if (curTask.IsFinish)
            {
                if (taskQueue.Count > 0)
                    curTask = taskQueue.Dequeue();
                else
                {
                    curTask = null;
                    RecorderSettings.OutputDir = "";
                    RecorderSettings.OutputName = "";
                    RemoveAutoCaster();
                    RemoveSkillEndHandler();
                    Debug.Log("录制完成");
                }
            }

            if (curTask != null && !curTask.IsRuning)
            {
                curTask.DoFunc(null);
                //Debug.Log($"{(curTask as RecordingTaskItemET).SkillName}/{(curTask as RecordingTaskItemET).NeedFinish}");
            }
        }

        private void AddAutoCaster()
        {
            if (IsEditor)
            {
                SkillHoster.GetHoster.AutoFire += AutoSkillCaster;
            }
        }


        private void RemoveAutoCaster()
        {
            if (IsEditor)
            {
                SkillHoster.GetHoster.AutoFire -= AutoSkillCaster;
            }
        }

        private void SkillEnd()
        {
            XTimerMgr.singleton.SetTimer(0.2f, curTask.FinishFunc, null);
        }
        private void AddSkillEndHandler()
        {
            if (IsEditor)
            {
                OverdrawMonitor.Instance.OnSkillEnd += SkillEnd;
            }
        }

        private void RemoveSkillEndHandler()
        {
            if (IsEditor)
            {
                OverdrawMonitor.Instance.OnSkillEnd -= SkillEnd;
            }
        }

        public bool ChcekSelectFileList(out string info)
        {
            StringBuilder sb = new StringBuilder();
            bool result = false;
            foreach (var item in FileList)
            {
                if (item.Select)
                {
                    bool res = CheckSelectFile(item.Name, out string itemInfo);
                    if (res)
                    {
                        result = true;
                        sb.AppendLine(itemInfo);
                    }
                }
            }
            info = sb.ToString();
            return result;
        }

        public bool CheckSelectFile(string skillName, out string info)
        {
            StringBuilder sb = new StringBuilder();
            SkillGraph sg = new SkillGraph();
            sg.NeedInitRes = false;
            sg.OpenData($"{SkillLocation}{skillName}");
            var transfer = sg.GetTransferNode();
            foreach (var item in sg.configData.RandomData)
            {
                if (!item.TimeBased && !transfer.Contains(item.Index))
                    continue;
                sb.AppendLine($"{skillName} 有 Random 节点 Index = {item.Index};");
            }
            info = sb.ToString();
            return info.Length > 0;
        }
    }

    public struct EditorRecorderData
    {
        public GameObject PlayerObj;
        public Vector3 PupetPos;
        public ulong PlayerIndex;
        public ulong Target;
        public SkillCamera CameraComponent;
        public Dictionary<ulong, Entity> EntityDic;
    }

    public class NewConfig
    {
        public string OutPath;
        public string RecorderName;
        public int CameraType;
        public int CameraDirection;
        public bool needDeSmoke;
        public float t_height;
        public float t_radius;
        public float t_offset;
        public List<bool> FileSelect;
        public List<int> fileSpecialList;
        public List<string> pupetStringFieldList;

        public void WriteConfig(string path,NewConfig config)
        {
            DataIO.SerializeData<NewConfig>(path, config);
        }
        public NewConfig ReadConfig(string path)
        {
            return DataIO.DeserializeData<NewConfig>(path);
        }
    }

    [Serializable]
    public class AutoRecorderConfig
    {
        const int VER = 1;
        [SerializeField]
        public int Version;
        [SerializeField]
        public string OutPath;
        [SerializeField]
        public string RecorderName;
        [SerializeField]
        public List<bool> FileSelect;
        [SerializeField]
        public List<int> FileSpecialList;
        [SerializeField]
        public List<string> PupetStringList;

        public AutoRecorderConfig(string configInfo)
        {
            FileSelect = new List<bool>();
            FileSpecialList = new List<int>();
            PupetStringList = new List<string>();
            if (string.IsNullOrEmpty(configInfo))
            {
                OutPath = RecorderName = "";
                return;
            }
            var versions = configInfo.Trim().Split(new string[] { "===version\r\n" }, StringSplitOptions.None);
            if (!int.TryParse(versions[0], out int version) || version != VER)
            {
                OutPath = RecorderName = "";
                return;
            }
            var info = versions[1].Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
            OutPath = info[0];
            RecorderName = info[1];
            for (int i = 2; i < info.Length; ++i)
            {
                var content = info[i].Trim().Split('=');
                FileSelect.Add(content[0] == "1");
                FileSpecialList.Add(int.Parse(content[1]));
                PupetStringList.Add(content[2]);
            }
        }

        public void UpdateConfig(string outpath, string recordName, List<ISelectFile> selects)
        {
            OutPath = outpath;
            RecorderName = recordName;
            FileSelect.Clear();
            FileSpecialList.Clear();
            PupetStringList.Clear();
        }

        public void WriteConfig(string configFile)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{VER}===version");
            sb.AppendLine(OutPath);
            sb.AppendLine(RecorderName);
            for (int i = 0; i < FileSelect.Count; ++i)
            {
                sb.AppendLine($"{(FileSelect[i] ? 1 : 0)}={FileSpecialList[i]}={PupetStringList[i]}");
            }
            using (var file = File.Open(configFile, FileMode.Create))
            {
                var info = Encoding.UTF8.GetBytes(sb.ToString());
                file.Write(info, 0, info.Length);
            }
        }
    }

    public interface ISelectFile
    {
        void SetSelect(bool isSelect);
        bool Select { get; set; }

        string Name { get; set; }
    }

    public class FileData : ISelectFile
    {
        public string Name { get; set; }
        public bool Select { get; set; }
        public int SpecialInt;
        public ESpecialMask Mask => (ESpecialMask)SpecialInt;
        public string PupetSkillName;

        public void SetSelect(bool isSelect)
        {
            Select = isSelect;
        }
    }

    public class HitFileData : ISelectFile
    {
        public string Path;
        public string Name { get; set; }
        public bool Select { get; set; }
        public void SetSelect(bool isSelect)
        {
            Select = isSelect;
        }
    }
}
